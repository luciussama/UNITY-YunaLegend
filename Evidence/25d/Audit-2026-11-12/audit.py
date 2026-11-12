"""Read-only checks of delivered sources/evidence; no Unity or visual approval.
Run from repository root: python Evidence/25d/Audit-2026-11-12/audit.py
Writes only results.json in this directory. Historical mismatches stay visible.
"""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import sqlite3
import subprocess

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).parent


def sha(path):
    p = ROOT / path
    return hashlib.sha256(p.read_bytes()).hexdigest() if p.is_file() else None


def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT, text=True).strip()


def verify(mapping):
    bad = [{'path': p, 'expected': h, 'actual': sha(p)} for p, h in mapping.items() if sha(p) != h]
    return {'checked': len(mapping), 'matched': len(mapping) - len(bad), 'mismatches': bad}


def main():
    state = json.loads((ROOT / 'docs/ia/PLANO-25D-ESTADO.json').read_text(encoding='utf-8'))
    result = {'head': git('rev-parse', 'HEAD'), 'branch': git('branch', '--show-current'),
              'base': '8e094f06c528cb43899b0822e67f6a6813574c6d',
              'method': 'New filesystem/hash/SQL/source checks and recalculation of recorded samples; no new Unity, device, audio or visual test.'}
    changed = git('diff', '--name-only', '8e094f0..HEAD').splitlines()
    result['integrated_paths'] = changed
    result['integrated_path_count'] = len(changed)
    result['current_status'] = git('status', '--short')
    result['protected_runtime_changes'] = git('diff', '--name-only', '8e094f0..HEAD', '--', 'Assets/Scripts', 'Assets/Scenes', 'Packages', 'ProjectSettings').splitlines()
    result['tasks'] = [{k: t.get(k) for k in ['id', 'status', 'owner', 'resources']} for t in state['tasks']]
    result['state_evidence'] = verify({e['path']: e['sha256'] for t in state['tasks'] for e in t.get('evidence', [])})
    c = sqlite3.connect((ROOT / 'docs/ia/diario.sqlite3').as_uri() + '?mode=ro', uri=True)
    c.row_factory = sqlite3.Row
    result['diary_integrity'] = [r[0] for r in c.execute('PRAGMA integrity_check')]
    result['diary_foreign_keys'] = [list(r) for r in c.execute('PRAGMA foreign_key_check')]
    result['pending_plans'] = [dict(r) for r in c.execute("SELECT p.id,p.session,p.action FROM entries p WHERE p.phase='planned' AND NOT EXISTS(SELECT 1 FROM entries r WHERE r.plan_id=p.id) ORDER BY p.id")]
    result['diary_latest'] = c.execute('SELECT max(id) FROM entries').fetchone()[0]
    c.close()
    result['manifests'] = {}
    for name in ['T03.1', 'T03.2']:
        p = f'Evidence/25d/{name}/manifest-v1.json'
        m = json.loads((ROOT / p).read_text(encoding='utf-8'))
        result['manifests'][p] = verify(m['sha256'])
    sources = json.loads((ROOT / 'skills/SOURCES.json').read_text(encoding='utf-8'))
    result['skills_snapshot'] = verify(sources['upstream']['sha256'])
    spec = importlib.util.spec_from_file_location('audit_t02', ROOT / 'tools/audit_25d_t02.py')
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    t02 = module.audit()
    result['t02_recalculated'] = {'success': t02['success'], 'checks': t02['checks']}
    result['recorded_samples'] = {}
    for folder in ['review-v2', 'final-v2-30', 'final-v2-60', 'final-v2-120', 'final-v2-smooth-60']:
        p = f'Evidence/25d/T03.2/{folder}/run-proof.json'
        d = json.loads((ROOT / p).read_text(encoding='utf-8'))
        groups, group = [], []
        for s in d['samples']:
            if s['contact']:
                if group and s['pose'] // 4 != group[-1]['pose'] // 4:
                    groups.append(group)
                    group = []
                group.append(s)
            elif group:
                groups.append(group)
                group = []
        if group:
            groups.append(group)
        drift = max(max(s['footX'] for s in g) - min(s['footX'] for s in g) for g in groups)
        steps = [(b['x'] + b['bodyShift']) - (a['x'] + a['bodyShift']) for a, b in zip(d['samples'], d['samples'][1:])]
        result['recorded_samples'][p] = {'proof_sha256': sha(p), 'success_recorded': d['success'],
            'frames': d['frames'], 'simulationHz': d['simulationHz'],
            'contact_drift_recalculated_m': drift, 'max_body_step_recalculated_m': max(steps),
            'frozen_intervals_recalculated': sum(abs(s) < .01 for s in steps),
            'gameplay_matches_current': d['gameplayBefore'] == d['gameplayAfter'] == sha('Assets/Scenes/DarkRoom_Gameplay.unity'),
            'controller_matches_current': d['controllerBefore'] == d['controllerAfter'] == sha('Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs'),
            'atlas_matches_current': d['atlasHash'] == sha('Assets/Art/Illustrated25D/Yuna/Run/Yuna_Run_Atlas_v1.png')}
    links = []
    for p in [ROOT / 'SKILLS.md', ROOT / 'docs/ia/ESTADO-ATUAL-25D.md', *ROOT.glob('skills/yuna-*/SKILL.md'),
              *ROOT.glob('Evidence/25d/T03.*/**/*.md')]:
        for target in re.findall(r'\]\(([^)]+)\)', p.read_text(encoding='utf-8')):
            if '://' in target or target.startswith('#'):
                continue
            destination = (p.parent / target.split('#')[0]).resolve()
            if not destination.exists():
                links.append({'file': p.relative_to(ROOT).as_posix(), 'target': target})
    result['broken_local_links'] = links
    guids = {}
    for p in (ROOT / 'Assets').rglob('*.meta'):
        m = re.search(r'^guid: (\w+)', p.read_text(encoding='utf-8', errors='replace'), re.M)
        if m:
            guids.setdefault(m[1], []).append(p.relative_to(ROOT).as_posix())
    result['duplicate_asset_guids'] = {g: paths for g, paths in guids.items() if len(paths) > 1}
    result['capture_source_hashes_now'] = {p.as_posix(): sha(p) for p in [Path('Assets/Art/Illustrated25D/Yuna/Run/Editor/YunaRunReviewCapture.cs'), Path('Assets/Art/Illustrated25D/Yuna/Run/YunaRunIllustration.cs'), Path('Assets/Art/Illustrated25D/Yuna/Run/YunaRunReview.unity'), Path('tools/make_run_review_v2.py')]}
    result['v2_frame_provenance'] = {}
    for folder, raw in [('review-v2', 'RunV2_120'), ('final-v2-30', 'RunV2_30'), ('final-v2-60', 'RunV2_60'), ('final-v2-smooth-60', 'RunV2S_60')]:
        v = json.loads((ROOT / f'Evidence/25d/T03.2/{folder}/verification.json').read_text(encoding='utf-8'))
        mapping = {f'Build/25d/{raw}/{p}': h for p, h in v['source_frame_sha256'].items()}
        result['v2_frame_provenance'][folder] = {'raw_folder': raw, 'hashes': verify(mapping), 'verification_clip': v['clip'], 'verification_loop': v.get('loop'), 'verification_continuity': v['continuity']}
    # Snapshot, not a retroactive binding of these sources to prior Unity runs.
    result['integrated_file_hashes_now'] = {p: sha(p) for p in changed}
    (OUT / 'results.json').write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: result[k] for k in ['head', 'integrated_path_count', 'protected_runtime_changes', 'diary_integrity', 'diary_foreign_keys', 'state_evidence', 'manifests', 'skills_snapshot', 'broken_local_links', 'duplicate_asset_guids']}, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
