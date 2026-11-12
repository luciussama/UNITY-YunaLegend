"""Descriptive display-image measurements; subject AABBs include background."""
from pathlib import Path
import json
import math
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Build/VisualAuditTools'))
from PIL import Image, ImageDraw, ImageFont
import numpy as np

OUT = Path(__file__).resolve().parent
font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 20)
metrics = {}
panels = []
for variant in ('before', 'after'):
    meta = json.loads((OUT / variant / 'unity-metadata.json').read_text())
    metrics[variant] = {}
    for shot in meta['shots']:
        image = Image.open(OUT / variant / ('calibrated-' + shot['name'] + '.png')).convert('RGB')
        assert image.size == (1280, 720)
        pixels = np.asarray(image, dtype=float)
        regions = {'frame': pixels}
        for subject in shot['subjects']:
            b = subject['boundsPixels']
            x0, y0 = max(0, math.floor(b['x'])), max(0, math.floor(b['y']))
            x1, y1 = min(1280, math.ceil(b['x'] + b['width'])), min(720, math.ceil(b['y'] + b['height']))
            if x1 > x0 and y1 > y0:
                regions[subject['name']] = pixels[y0:y1, x0:x1]
        values = {}
        for name, roi in regions.items():
            luma = roi @ np.array([.2126, .7152, .0722])
            values[name] = dict(meanLuma=float(luma.mean()), p95Luma=float(np.percentile(luma, 95)),
                                white250Pct=float((roi.min(axis=2) >= 250).mean() * 100),
                                channel250Pct=float((roi.max(axis=2) >= 250).mean() * 100))
        metrics[variant][shot['name']] = values

for name in ('01-start', '02-castle', '03-false-door', '04-portal', '05-fall-ground', '06-castle-overview'):
    row = Image.new('RGB', (1280, 400), (22, 25, 32))
    draw = ImageDraw.Draw(row)
    for i, (variant, label) in enumerate((('before', 'Antes'), ('after', 'Depois'))):
        source = Image.open(OUT / variant / ('calibrated-' + name + '.png'))
        row.paste(source.resize((640, 360), Image.Resampling.LANCZOS), (i * 640, 40))
        draw.text((i * 640 + 12, 8), label + ' | ' + name, font=font, fill='white')
    panels.append(row)
panels[1].save(OUT / 'comparison-close.png')
board = Image.new('RGB', (1280, 2400))
for i, row in enumerate(panels):
    board.paste(row, (0, i * 400))
board.save(OUT / 'comparison-all.png')
(OUT / 'metrics.json').write_text(json.dumps(metrics, indent=2))
print(json.dumps({v: metrics[v]['02-castle'] for v in metrics}, indent=2))
