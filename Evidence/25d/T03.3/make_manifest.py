"""Freeze T03.3 v1 delivered files and check executed clone/source equivalence.
Run from repository root after preparing the review. Does not hash mutable diary/state.
"""
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).parent


def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()


def main():
    clone = ROOT / 'Build/Publish/Project'
    jump = ROOT / 'Assets/Art/Illustrated25D/Yuna/Jump'
    sources = [p for p in jump.rglob('*') if p.is_file()] + [Path(str(jump) + '.meta')]
    dependencies = [ROOT / p for p in [
        'Assets/Scenes/DarkRoom_Gameplay.unity',
        'Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs',
        'Assets/Art/Illustrated25D/Yuna/Yuna_Profile_v1.png',
        'Assets/Art/Illustrated25D/Yuna/Run/YunaRunIllustration.cs',
        'Assets/Art/Illustrated25D/Yuna/Run/YunaRunReview.unity',
        'Assets/Art/Illustrated25D/Yuna/Run/Yuna_Run_Atlas_v1.png',
        'Assets/Art/Illustrated25D/Yuna/Run/Run_Painted.mat',
        'ProjectSettings/ProjectVersion.txt', 'Packages/manifest.json', 'Packages/packages-lock.json']]
    evidence = [p for p in HERE.rglob('*') if p.is_file() and p.name != 'manifest-v1.json' and '__pycache__' not in p.parts and p.suffix != '.pyc']
    def inventory(paths):
        return {p.relative_to(ROOT).as_posix(): {'sha256': sha(p), 'bytes': p.stat().st_size} for p in sorted(paths)}
    cloned = {p.relative_to(ROOT).as_posix(): (clone / p.relative_to(ROOT)).is_file() and p.read_bytes() == (clone / p.relative_to(ROOT)).read_bytes() for p in sources + dependencies}
    assert all(cloned.values()), [p for p, ok in cloned.items() if not ok]
    guids = {}
    for p in (ROOT / 'Assets').rglob('*.meta'):
        m = re.search(r'^guid: (\w+)', p.read_text(encoding='utf-8', errors='replace'), re.M)
        if m:
            guids.setdefault(m[1], []).append(p.relative_to(ROOT).as_posix())
    duplicates = {g: v for g, v in guids.items() if len(v) > 1}
    assert not duplicates, duplicates
    build = (ROOT / 'ProjectSettings/EditorBuildSettings.asset').read_text(encoding='utf-8')
    assert 'YunaJumpReview' not in build
    primary = HERE / 'review-v1-verified/jump-proof.json'
    assert primary.read_bytes() == (HERE / 'validated-120/jump-proof.json').read_bytes()
    proof = json.loads(primary.read_text(encoding='utf-8'))
    verification = json.loads((HERE / 'review-v1-verified/verification.json').read_text(encoding='utf-8'))
    assert proof['simulationHz'] == verification['simulation_hz'] == 120
    assert sha(primary) == verification['proof_sha256']
    raw = ROOT / 'Build/25d/JumpValidated120'
    assert all(sha(raw / name) == value for name, value in verification['source_frame_sha256'].items())
    run = json.loads((ROOT / 'Evidence/25d/T03.2/review-v2/manifest-v2.json').read_text(encoding='utf-8'))
    run_files = {**run['sources'], **run['evidence']}
    assert all(sha(ROOT / name) == value['sha256'] for name, value in run_files.items())
    result = {'version': 'T03.3-v1', 'state': 'em_revisao', 'session': '2026-11-12-codex-jump-t033-c91e', 'agent': 'Codex (GPT-6)',
        'base_commit': subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip(),
        'unity': '6000.3.28f1', 'primary_simulation_hz': 120, 'capture_fps': 30,
        'source': inventory(sources), 'dependencies': inventory(dependencies), 'evidence': inventory(evidence),
        'executed_clone_equal_current': cloned, 'approved_run_manifest_unchanged': len(run_files),
        'duplicate_asset_guids': duplicates, 'outside_build_settings': True,
        'raw_frame_count_matched': len(verification['source_frame_sha256']),
        'limits': 'Flat fixture candidate awaiting human review. No device/audio/pilot certification. Historical attempt01 has no complete previous-source snapshot. Diary/state remain append-only/mutable and outside this manifest.'}
    (HERE / 'manifest-v1.json').write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'sources': len(sources), 'dependencies': len(dependencies), 'evidence': len(evidence), 'clone_equal': len(cloned), 'run_preserved': len(run_files), 'raw_frames': len(verification['source_frame_sha256'])}))


if __name__ == '__main__':
    main()
