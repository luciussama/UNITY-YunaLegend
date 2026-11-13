"""Package only real T04.2 capture frames; never synthesize Unity evidence."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture', default='Evidence/25d/T04.2/capture-60')
    parser.add_argument('--manifest-only', action='store_true')
    args = parser.parse_args()
    evidence = ROOT / 'Evidence' / '25d' / 'T04.2'
    capture = ROOT / args.capture
    if not args.manifest_only:
        from PIL import Image, ImageDraw
        proof = json.loads((capture / 'accompaniment-proof.json').read_text(encoding='utf-8'))
        if not proof['success']:
            raise SystemExit('Capture proof failed; do not package as review candidate.')
        review = evidence / 'review'
        review.mkdir(exist_ok=True)
        for group, size in [('game', (960, 540)), ('detail', (800, 600))]:
            paths = sorted((capture / group).glob('*.png'))
            if len(paths) != 150:
                raise SystemExit(f'{group}: expected 150 real rendered frames, found {len(paths)}')
            images = []
            for path in paths:
                with Image.open(path) as image:
                    images.append(image.convert('RGB').resize(size, Image.Resampling.LANCZOS))
            for speed, duration in [('normal', [67, 67, 66] * 50), ('slow', 100)]:
                images[0].save(review / f'{group}-{speed}.gif', save_all=True,
                               append_images=images[1:], duration=duration, loop=0, optimize=False)
            sheet = Image.new('RGB', (size[0], (size[1] // 3 + 24) * 4), '#182135')
            draw = ImageDraw.Draw(sheet)
            for cell, index in enumerate([0, 15, 40, 52, 70, 76, 85, 98, 112, 128, 138, 149]):
                x = cell % 3 * (size[0] // 3)
                y = cell // 3 * (size[1] // 3 + 24)
                sheet.paste(images[index].resize((size[0] // 3, size[1] // 3)), (x, y))
                draw.text((x + 4, y + size[1] // 3 + 4), f"t={proof['samples'][index]['t']:.2f}s", fill='white')
            sheet.save(review / f'{group}-sheet.png')
    files = []
    for directory in [ROOT / 'Assets' / 'Art' / 'Illustrated25D' / 'Nanda', evidence]:
        files.extend(p for p in directory.rglob('*') if p.is_file() and p.name != 'manifest.json')
    files.append(Path(__file__).resolve())
    manifest = {
        'session': '2026-11-13-copilot-t042-b29a5ec7',
        'status': 'candidate; no human acceptance; inspect capture proofs separately',
        'files': [{'path': str(p.relative_to(ROOT)).replace('\\', '/'),
                   'sha256': hashlib.sha256(p.read_bytes()).hexdigest(), 'bytes': p.stat().st_size}
                  for p in sorted(set(files))]
    }
    (evidence / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
    print(f'Manifest: {len(manifest["files"])} files; real capture packaging={not args.manifest_only}')


if __name__ == '__main__':
    main()
