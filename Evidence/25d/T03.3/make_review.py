"""Audit a T03.3 capture and assemble unretouched GIFs (requires Pillow).
Run from the repository root, using --proof, --frames and --out.
No Unity execution; reads existing captured frames. GIF repetition is a replay,
not a seamless animation loop. The first and last frame have a presentation hold.
"""
import argparse
import hashlib
import json
from pathlib import Path
import statistics
from PIL import Image


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def audit(proof, frames):
    d = json.loads(Path(proof).read_text(encoding='utf-8'))
    samples = d['samples']
    ticks = d.get('ticks', samples)
    contact = [s for s in ticks if s['contact']]
    ground = [s['y'] for s in ticks if s['grounded']]
    airborne = [s for s in ticks if not s['grounded'] and s['pose'] >= 0]
    height = max(s['y'] for s in ticks) - statistics.median(ground)
    landed = next(s for s in ticks if s['contact'])
    launch = airborne[0]
    apex = max(airborne, key=lambda s: s['y'])
    steps = [(b['x'] + b['shift']) - (a['x'] + a['shift']) for a, b in zip(samples, samples[1:])]
    transition_steps = [(b['x'] + b['shift']) - (a['x'] + a['shift']) for a, b in zip(samples, samples[1:]) if a['state'] == 'RunBlend' or b['state'] == 'RunBlend']
    atlas = Image.open('Assets/Art/Illustrated25D/Yuna/Jump/Yuna_Jump_Atlas_v1.png').convert('RGBA')
    baselines = [511, 511, 511, 511, 511, 508, 510, 511]
    ppu = 1049.2857666 * 362 / 1024
    source_soles = {}
    for pose in [6, 7]:
        a = atlas.crop((pose % 4 * 362, pose // 4 * 543, (pose % 4 + 1) * 362, (pose // 4 + 1) * 543)).getchannel('A')
        bbox = a.point(lambda x: 255 if x >= 128 else 0).getbbox()
        source_soles[str(pose)] = {'opaque_bbox': bbox, 'baseline': baselines[pose],
            'opaque_bottom_vs_support_px_at_720': abs(bbox[3] - baselines[pose]) / ppu * 96}
    checks = {
        'capture_success': d['success'],
        'one_takeoff_and_landing': d['takeoffs'] == d['landings'] == 1,
        'collider_and_scale_preserved': d['colliderUnchanged'] and d['rootScaleUnchanged'],
        'gameplay_matches_current': d['gameplayBefore'] == d['gameplayAfter'] == sha('Assets/Scenes/DarkRoom_Gameplay.unity'),
        'controller_matches_current': d['controllerBefore'] == d['controllerAfter'] == sha('Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs'),
        'atlas_matches_current': d['atlasHash'] == sha('Assets/Art/Illustrated25D/Yuna/Jump/Yuna_Jump_Atlas_v1.png'),
        'states_present': {'Takeoff', 'Rise', 'Apex', 'Fall', 'Land', 'Recover'} <= {s['state'] for s in ticks},
        'all_eight_drawings_evaluated': set(range(8)) <= {s['pose'] for s in ticks},
        'height_within_0_03m_of_serialized_height': abs(height - d['jumpHeight']) < .03,
        'landing_anchor_drift_below_2px': (max(s['footX'] for s in contact) - min(s['footX'] for s in contact)) * 96 <= 2,
        'landing_anchor_height_below_1px': max(abs(s['footY']) for s in contact) * 96 <= 1,
        'opaque_landing_soles_below_1px': max(v['opaque_bottom_vs_support_px_at_720'] for v in source_soles.values()) <= 1,
    }
    raw = {}
    for group in ['game', 'detail']:
        paths = sorted((Path(frames) / group).glob('*.png'))
        checks[group + '_66_frames'] = len(paths) == d['frames'] == 66
        for p in paths:
            raw[group + '/' + p.name] = sha(p)
    return {'method': 'Recalculation from recorded PlayMode ticks and raw PNG hashes; source alpha contour audit on flat fixture, not a device/pilot certification.',
        'simulation_hz': d['simulationHz'], 'capture_fps': 30, 'proof_sha256': sha(proof),
        'checks': checks, 'success': all(checks.values()),
        'arc': {'height_m': height, 'serialized_height_m': d['jumpHeight'], 'launch_first_airborne_s': launch['t'],
                'apex_s': apex['t'], 'landing_s': landed['t'], 'flight_observed_s': landed['t'] - launch['t']},
        'contact': {'ticks': len(contact), 'horizontal_drift_m': max(s['footX'] for s in contact) - min(s['footX'] for s in contact),
                    'vertical_error_m': max(abs(s['footY']) for s in contact), 'source_opaque_soles': source_soles},
        'continuity': {'all_clip_body_step_max_m': max(steps), 'all_clip_body_step_min_m': min(steps),
            'run_blend_body_step_max_m': max(transition_steps), 'run_blend_body_step_min_m': min(transition_steps),
            'limitation': 'Run outside the jump retains its approved per-pose body stepping. Contact pose changes include authored body motion; pivot metrics alone do not certify aesthetics.'},
        'atlas_editor_mib': d['atlasMiB'], 'source_frame_sha256': raw}


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--proof', required=True); p.add_argument('--frames', required=True); p.add_argument('--out', required=True)
    p.add_argument('--no-gifs', action='store_true')
    a = p.parse_args(); output = Path(a.out); output.mkdir(parents=True, exist_ok=True)
    result = audit(a.proof, a.frames)
    if not a.no_gifs:
        gifs = {}
        for group, name in [('game', 'jump-gameplay.gif'), ('detail', 'jump-detail.gif')]:
            paths = sorted((Path(a.frames) / group).glob('*.png'))
            frames = [Image.open(f).convert('RGB') for f in paths]
            palette = frames[0].quantize(colors=256, method=Image.Quantize.MEDIANCUT)
            paletted = [im.quantize(palette=palette, dither=Image.Dither.NONE) for im in frames]
            durations = [(round((i + 1) * 100 / 30) - round(i * 100 / 30)) * 10 for i in range(len(frames))]
            durations[0] += 250; durations[-1] += 500
            paletted[0].save(output / name, save_all=True, append_images=paletted[1:], duration=durations, loop=0, optimize=False, disposal=2)
            gifs[name] = {'sha256': sha(output / name), 'frames': len(frames), 'duration_ms': sum(durations)}
            if group == 'detail':
                name = 'jump-detail-slow.gif'
                paletted[0].save(output / name, save_all=True, append_images=paletted[1:], duration=[n * 2 for n in durations], loop=0, optimize=False, disposal=2)
                gifs[name] = {'sha256': sha(output / name), 'frames': len(frames), 'duration_ms': sum(durations) * 2}
        result['gifs'] = gifs
    (output / 'verification.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: result[k] for k in ['success', 'simulation_hz', 'checks', 'arc', 'contact', 'continuity']}, indent=2))
    raise SystemExit(0 if result['success'] else 1)


if __name__ == '__main__':
    main()
