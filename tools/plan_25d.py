"""Coordinator for the illustrated 2.5D plan. Python stdlib; not a security boundary."""
import argparse
from contextlib import closing, contextmanager
from datetime import datetime, timezone
import hashlib
import json
import os
import sqlite3
from pathlib import Path
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
STATE = ROOT / 'docs/ia/PLANO-25D-ESTADO.json'
STATUSES = {'pending', 'active', 'review', 'accepted'}


@contextmanager
def locked(path):
    # OS-owned lock is released even if the process crashes. Never delete the file.
    lock_path = path.parent / '.plano25d.lock'
    with lock_path.open('a+b') as handle:
        handle.seek(0, 2)
        if handle.tell() == 0:
            handle.write(b'0')
            handle.flush()
        deadline = time.monotonic() + 5
        acquired = False
        try:
            while not acquired:
                handle.seek(0)
                try:
                    if os.name == 'nt':
                        import msvcrt
                        msvcrt.locking(handle.fileno(), msvcrt.LK_NBLCK, 1)
                    else:
                        import fcntl
                        fcntl.flock(handle.fileno(), fcntl.LOCK_EX | fcntl.LOCK_NB)
                    acquired = True
                except OSError:
                    if time.monotonic() >= deadline:
                        raise ValueError('Estado ocupado; tente novamente sem apagar o lock.')
                    time.sleep(.05)
            yield
        finally:
            if acquired:
                handle.seek(0)
                if os.name == 'nt':
                    import msvcrt
                    msvcrt.locking(handle.fileno(), msvcrt.LK_UNLCK, 1)
                else:
                    import fcntl
                    fcntl.flock(handle.fileno(), fcntl.LOCK_UN)


def validate(state):
    tasks = state['tasks']
    if state['schema_version'] != 1:
        raise ValueError('Versao de esquema desconhecida.')
    identifiers = [t['id'] for t in tasks]
    if len(set(identifiers)) != len(identifiers):
        raise ValueError('IDs duplicados.')
    by_id = {t['id']: t for t in tasks}
    occupied = set()
    for task in tasks:
        if task['status'] not in STATUSES:
            raise ValueError('Status invalido.')
        for dependency in task['depends_on']:
            if dependency not in by_id or dependency == task['id']:
                raise ValueError('Dependencia invalida.')
        if task['status'] in {'active', 'review', 'accepted'}:
            if any(by_id[d]['status'] != 'accepted' for d in task['depends_on']):
                raise ValueError('Dependencia nao aceita: ' + task['id'])
        if task['status'] in {'active', 'review'}:
            if not task.get('owner'):
                raise ValueError('Reserva sem dono.')
            resources = set(task['resources'])
            if occupied & resources:
                raise ValueError('Recursos com reservas conflitantes.')
            occupied |= resources
        if task['status'] in {'review', 'accepted'} and not task.get('evidence'):
            raise ValueError('Entrega sem evidencia.')
        if task['status'] == 'accepted' and not task.get('acceptance'):
            raise ValueError('Aceite sem referencia.')
    def visit(identifier, chain):
        if identifier in chain:
            raise ValueError('Ciclo de dependencias.')
        for dependency in by_id[identifier]['depends_on']:
            visit(dependency, chain | {identifier})
    for identifier in identifiers:
        visit(identifier, set())


def save(path, state):
    validate(state)
    temporary = None
    try:
        with tempfile.NamedTemporaryFile(mode='w', encoding='utf-8', dir=path.parent,
                                         prefix='.plano25d-', suffix='.tmp', delete=False) as out:
            temporary = Path(out.name)
            json.dump(state, out, indent=2, ensure_ascii=False)
            out.write('\n')
            out.flush()
            os.fsync(out.fileno())
        os.replace(temporary, path)
    finally:
        if temporary and temporary.exists():
            temporary.unlink()


def nonempty(value):
    if not value.strip():
        raise argparse.ArgumentTypeError('Texto obrigatorio.')
    return value


def check_agent_registry(root, task, session):
    """Called under coordinator lock; no writes or acquisition of a SQLite write lock."""
    database = root / 'docs/ia/diario.sqlite3'
    if not database.exists():  # Isolated fixtures / pre-registry checkouts.
        return
    from agent_registry import jobs, key, overlaps
    try:
        with closing(sqlite3.connect(database.resolve().as_uri() + '?mode=ro', uri=True, timeout=5)) as db:
            db.row_factory = sqlite3.Row
            if not db.execute("SELECT 1 FROM sqlite_master WHERE name='agent_jobs'").fetchone():
                return
            requested = [key('plan:' + task['id'])] + [key(r) for r in task['resources']]
            for job in jobs(db):
                if job['state'] != 'finalizado' and job['session'] != session:
                    if any(overlaps(a, b) for a in requested for b in job['resources']):
                        raise ValueError(f"Registro de agentes ocupado: #{job['id']} dono={job['session']}")
    except sqlite3.Error as error:
        raise ValueError('Nao foi possivel validar registro de agentes: ' + str(error)) from error


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--state', type=Path, default=STATE)
    sub = parser.add_subparsers(dest='command', required=True)
    for command in ['status', 'check', 'claim', 'checkpoint', 'submit', 'accept', 'release', 'handover', 'reopen']:
        p = sub.add_parser(command)
        if command in {'status', 'check'}:
            continue
        p.add_argument('task')
        p.add_argument('--session', required=True, type=nonempty)
        if command in {'claim', 'handover'}:
            p.add_argument('--agent', required=True, type=nonempty)
        if command == 'checkpoint':
            for field in ['done', 'next', 'validation', 'pending', 'files', 'processes']:
                p.add_argument('--' + field, required=True, type=nonempty)
        if command == 'submit':
            p.add_argument('--evidence', action='append', required=True, type=nonempty)
            p.add_argument('--validation', required=True, type=nonempty)
        if command == 'accept':
            p.add_argument('--reference', required=True, type=nonempty)
            p.add_argument('--human-approval', action='store_true', help='Declara referencia a aprovacao humana real; nao autentica consentimento.')
        if command in {'release', 'handover', 'reopen'}:
            p.add_argument('--reason', required=True, type=nonempty)
        if command == 'handover':
            p.add_argument('--from-session', required=True, type=nonempty)
    args = parser.parse_args(argv)
    path = args.state.resolve()
    # State path is canonical; evidence paths resolve against that checkout, not cwd.
    root = path.parents[2]
    try:
        with locked(path):
            state = json.loads(path.read_text(encoding='utf-8'))
            validate(state)
            by_id = {t['id']: t for t in state['tasks']}
            if args.command == 'check':
                for item_task in state['tasks']:
                    if item_task['status'] in {'review', 'accepted'}:
                        for item in item_task['evidence']:
                            artifact = (root / item['path']).resolve()
                            if not artifact.is_relative_to(root) or hashlib.sha256(artifact.read_bytes()).hexdigest() != item['sha256']:
                                raise ValueError('Evidencia alterada: ' + item_task['id'])
                print('OK: esquema, dependencias, reservas e referencias estruturais.')
                return 0
            if args.command == 'status':
                print(json.dumps(state, ensure_ascii=False, indent=2))
                return 0
            if args.task not in by_id:
                raise ValueError('Tarefa desconhecida.')
            task = by_id[args.task]
            now = datetime.now(timezone.utc).isoformat()
            if args.command == 'claim':
                check_agent_registry(root, task, args.session)
                if task['status'] != 'pending':
                    raise ValueError('Tarefa ja reservada ou aceita; nao sobrescrever dono.')
                if any(by_id[d]['status'] != 'accepted' for d in task['depends_on']):
                    raise ValueError('Dependencias pendentes.')
                resources = set(task['resources'])
                if any(resources & set(t['resources']) for t in state['tasks'] if t['status'] in {'active', 'review'}):
                    raise ValueError('Recurso reservado por outro trabalho.')
                task.update(status='active', owner={'session': args.session, 'agent': args.agent})
                state['execution_started'] = True
                if args.task == 'T00':
                    state['integration_checkout'] = str(root)
                    state['integration_coordinator'] = {'session': args.session, 'agent': args.agent}
            elif args.command in {'checkpoint', 'submit', 'release'}:
                if task['status'] not in {'active', 'review'} or task['owner']['session'] != args.session:
                    raise ValueError('Exige reserva da propria sessao.')
                if args.command == 'checkpoint':
                    if task['status'] != 'active':
                        raise ValueError('Reabra a entrega antes de modificar checkpoint.')
                    task['checkpoint'] = {k: getattr(args, k) for k in ['done', 'next', 'validation', 'pending', 'files', 'processes']}
                    task['checkpoint']['updated_at'] = now
                elif args.command == 'release':
                    task.update(status='pending', owner=None)
                else:
                    if task['status'] != 'active' or not task.get('checkpoint'):
                        raise ValueError('Entrega exige tarefa ativa e checkpoint.')
                    evidence = []
                    for name in args.evidence:
                        artifact = (root / name).resolve()
                        if not artifact.is_relative_to(root) or not artifact.is_file():
                            raise ValueError('Evidencia deve ser arquivo existente dentro do checkout central.')
                        evidence.append({'path': artifact.relative_to(root).as_posix(), 'sha256': hashlib.sha256(artifact.read_bytes()).hexdigest()})
                    task.update(status='review', evidence=evidence, validation=args.validation)
            elif args.command == 'accept':
                if task['status'] != 'review':
                    raise ValueError('Somente entrega em review pode ser aceita.')
                if task['human_gate'] and not args.human_approval:
                    raise ValueError('Marco humano exige --human-approval e referencia a aprovacao real do usuario.')
                for item in task['evidence']:
                    artifact = (root / item['path']).resolve()
                    if not artifact.is_relative_to(root) or hashlib.sha256(artifact.read_bytes()).hexdigest() != item['sha256']:
                        raise ValueError('Evidencia alterada; refazer entrega.')
                task.update(status='accepted', owner=None, acceptance={'session': args.session, 'reference': args.reference, 'at': now, 'human_required': task['human_gate']})
            elif args.command == 'handover':
                check_agent_registry(root, task, args.session)
                if task['status'] not in {'active', 'review'} or task['owner']['session'] != args.from_session:
                    raise ValueError('Dono esperado nao coincide; releia estado.')
                task['owner'] = {'session': args.session, 'agent': args.agent}
            elif args.command == 'reopen':
                affected = {args.task}
                while True:
                    enlarged = affected | {t['id'] for t in state['tasks'] if set(t['depends_on']) & affected}
                    if enlarged == affected:
                        break
                    affected = enlarged
                if any(by_id[i]['status'] in {'active', 'review'} for i in affected):
                    raise ValueError('Libere reservas afetadas antes de reabrir e invalidar dependentes.')
                for identifier in affected:
                    by_id[identifier].update(status='pending', owner=None)
                    by_id[identifier].pop('acceptance', None)
            event = {k: v for k, v in vars(args).items() if k != 'state'}
            event['at'] = now
            state['history'].append(event)
            state['updated_at'] = now
            save(path, state)
            print(json.dumps({'task': args.task, 'status': task['status'], 'session': args.session}))
        return 0
    except (OSError, ValueError, KeyError, TypeError) as error:
        print('Erro: ' + str(error))
        return 1


if __name__ == '__main__':
    raise SystemExit(main())
