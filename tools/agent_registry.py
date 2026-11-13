"""Registro transacional de agentes no diario SQLite (somente stdlib)."""
import argparse
from contextlib import closing
import json
import re
import sqlite3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DATABASE = ROOT / 'docs/ia/diario.sqlite3'
SCHEMA = """
CREATE TABLE IF NOT EXISTS agent_jobs (
 id INTEGER PRIMARY KEY AUTOINCREMENT,
 subject TEXT NOT NULL UNIQUE,
 session TEXT NOT NULL,
 agent TEXT NOT NULL,
 created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
);
CREATE TABLE IF NOT EXISTS agent_resources (
 job_id INTEGER NOT NULL REFERENCES agent_jobs(id),
 resource TEXT NOT NULL,
 PRIMARY KEY(job_id, resource)
);
CREATE TABLE IF NOT EXISTS agent_events (
 id INTEGER PRIMARY KEY AUTOINCREMENT,
 job_id INTEGER NOT NULL REFERENCES agent_jobs(id),
 state TEXT NOT NULL CHECK(state IN ('iniciado','em_execucao','finalizado')),
 outcome TEXT NOT NULL DEFAULT '',
 details TEXT NOT NULL,
 created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
);
CREATE TRIGGER IF NOT EXISTS agent_event_sequence BEFORE INSERT ON agent_events
BEGIN
 SELECT CASE WHEN
  (NEW.state='iniciado' AND EXISTS(SELECT 1 FROM agent_events WHERE job_id=NEW.job_id)) OR
  (NEW.state='em_execucao' AND COALESCE((SELECT state FROM agent_events WHERE job_id=NEW.job_id ORDER BY id DESC LIMIT 1),'')!='iniciado') OR
  (NEW.state='finalizado' AND COALESCE((SELECT state FROM agent_events WHERE job_id=NEW.job_id ORDER BY id DESC LIMIT 1),'')!='em_execucao')
 THEN RAISE(ABORT,'Transicao de estado invalida') END;
 SELECT CASE WHEN (NEW.state='finalizado' AND NEW.outcome NOT IN ('concluido','falhou','cancelado')) OR
  (NEW.state!='finalizado' AND NEW.outcome!='') THEN RAISE(ABORT,'Resultado invalido') END;
END;
"""


def key(value):
    value = value.strip().replace('\\', '/').rstrip('/').casefold()
    if not re.fullmatch(r'[a-z0-9][a-z0-9_./:-]*', value) or any(p in ('', '.', '..') for p in value.split('/')):
        raise ValueError('Use chave canonica ASCII, sem espacos nem caminho relativo ..')
    return value


def overlaps(a, b):
    return a == b or a.startswith(b + '/') or b.startswith(a + '/')


def initialize(db):
    db.executescript(SCHEMA)
    for table in ('agent_jobs', 'agent_resources', 'agent_events'):
        for operation in ('UPDATE', 'DELETE'):
            db.execute(f"CREATE TRIGGER IF NOT EXISTS {table}_no_{operation.lower()} "
                       f"BEFORE {operation} ON {table} BEGIN SELECT RAISE(ABORT,'Registro imutavel'); END")
    db.commit()


def jobs(db):
    result = []
    for row in db.execute('SELECT * FROM agent_jobs ORDER BY id'):
        job = dict(row)
        event = db.execute('SELECT * FROM agent_events WHERE job_id=? ORDER BY id DESC LIMIT 1',
                           (job['id'],)).fetchone()
        job.update(state=event['state'], outcome=event['outcome'])
        job['resources'] = [r[0] for r in db.execute(
            'SELECT resource FROM agent_resources WHERE job_id=? ORDER BY resource', (job['id'],))]
        result.append(job)
    return result


def claim(db, subject, session, agent, resources, details):
    subject = key(subject)
    resources = sorted(set(key(r) for r in resources))
    db.execute('BEGIN IMMEDIATE')
    existing = jobs(db)
    for job in existing:
        if job['subject'] == subject:
            if job['session'] != session:
                raise ValueError(f"Assunto ja registrado: #{job['id']} dono={job['session']} estado={job['state']}")
            if job['resources'] != resources or job['agent'] != agent:
                raise ValueError('Repeticao com parametros diferentes; preserve a reserva original')
            return dict(job, reused=True)
    for job in existing:
        if job['state'] != 'finalizado' and any(overlaps(a, b) for a in resources for b in job['resources']):
            raise ValueError(f"Recurso ocupado por #{job['id']} dono={job['session']}")
    # Compatibilidade com reservas anteriores ao registro. Leitura sob o lock do coordenador.
    from plan_25d import STATE, locked
    with locked(STATE):
        state = json.loads(STATE.read_text(encoding='utf-8'))
        for task in state['tasks']:
            owner = task.get('owner') or {}
            if task['status'] in ('active', 'review') and owner.get('session') != session:
                reserved = ['plan:' + task['id'].casefold()] + task['resources']
                if any(overlaps(a, key(b)) for a in resources for b in reserved):
                    raise ValueError(f"Coordenador ocupado: {task['id']} dono={owner.get('session')}")
        cursor = db.execute('INSERT INTO agent_jobs(subject,session,agent) VALUES (?,?,?)',
                            (subject, session, agent))
        identifier = cursor.lastrowid
        db.executemany('INSERT INTO agent_resources VALUES (?,?)', [(identifier, r) for r in resources])
        db.execute("INSERT INTO agent_events(job_id,state,details) VALUES (?,'iniciado',?)", (identifier, details))
        db.commit()
    return dict(next(j for j in jobs(db) if j['id'] == identifier), reused=False)


def transition(db, identifier, session, state, details, outcome=''):
    db.execute('BEGIN IMMEDIATE')
    job = next((j for j in jobs(db) if j['id'] == identifier), None)
    if not job or job['session'] != session:
        raise ValueError('Registro inexistente ou pertence a outra sessao')
    if job['state'] == state:
        last = db.execute('SELECT details,outcome FROM agent_events WHERE job_id=? ORDER BY id DESC LIMIT 1',
                          (identifier,)).fetchone()
        if (last['details'], last['outcome']) != (details, outcome):
            raise ValueError('Repeticao divergente; nao reescreva um evento')
        return dict(job, reused=True)
    db.execute('INSERT INTO agent_events(job_id,state,outcome,details) VALUES (?,?,?,?)',
               (identifier, state, outcome, details))
    db.commit()
    return dict(id=identifier, state=state, outcome=outcome, reused=False)


def nonempty(value):
    if not value.strip():
        raise argparse.ArgumentTypeError('Valor vazio')
    return value


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--db', type=Path, default=DATABASE)
    commands = parser.add_subparsers(dest='command', required=True)
    commands.add_parser('init')
    status = commands.add_parser('status')
    status.add_argument('--all', action='store_true')
    history = commands.add_parser('history')
    history.add_argument('--id', type=int, required=True)
    acquire = commands.add_parser('claim')
    acquire.add_argument('--subject', required=True)
    acquire.add_argument('--agent', required=True, type=nonempty)
    acquire.add_argument('--resource', action='append', required=True)
    for name, command in [('claim', acquire), ('run', commands.add_parser('run')),
                          ('finish', commands.add_parser('finish'))]:
        command.add_argument('--session', required=True, type=nonempty)
        command.add_argument('--details', required=True, type=nonempty)
        if name != 'claim':
            command.add_argument('--id', type=int, required=True)
        if name == 'finish':
            command.add_argument('--outcome', required=True, choices=['concluido', 'falhou', 'cancelado'])
    args = parser.parse_args(argv)
    try:
        with closing(sqlite3.connect(args.db.resolve().as_uri() + '?mode=rw', uri=True, timeout=30)) as db:
            db.row_factory = sqlite3.Row
            db.execute('PRAGMA foreign_keys=ON')
            if args.command == 'init':
                initialize(db)
                result = {'initialized': True}
            elif args.command == 'status':
                result = [j for j in jobs(db) if args.all or j['state'] != 'finalizado']
            elif args.command == 'history':
                result = [dict(r) for r in db.execute('SELECT * FROM agent_events WHERE job_id=? ORDER BY id', (args.id,))]
            elif args.command == 'claim':
                result = claim(db, args.subject, args.session, args.agent, args.resource, args.details)
            else:
                result = transition(db, args.id, args.session,
                                    'em_execucao' if args.command == 'run' else 'finalizado',
                                    args.details, getattr(args, 'outcome', ''))
            print(json.dumps(result, ensure_ascii=False, indent=2))
    except (sqlite3.Error, OSError, ValueError, KeyError) as error:
        parser.exit(1, f'Registro indisponivel/conflito: {error}\n')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
