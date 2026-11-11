"""Generate post-processing comparison boards and descriptive display-pixel metrics.

Dependencies: Pillow and NumPy in Build/VisualAuditTools (same versions as the
previous audit). Originals are engine HDR renders, converted to display sRGB.
"""
from pathlib import Path
import sys
import json
import math

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Build/VisualAuditTools'))
from PIL import Image, ImageDraw, ImageFont
import numpy as np

OUT = Path(__file__).resolve().parent
FONT = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 22)
SMALL = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 16)
meta = json.loads((OUT / 'unity-metadata.json').read_text())
variants = ['off', 'legacy-enabled', 'calibrated']
labels = ['Desligado', 'Perfil anterior ligado', 'Calibrado (Neutral + Bloom suave)']


def panel(image, title, size=(640, 360)):
    result = Image.new('RGB', (size[0], size[1] + 42), (22, 25, 32))
    ImageDraw.Draw(result).text((12, 8), title, fill=(240, 241, 245), font=FONT if size[0] == 640 else SMALL)
    result.paste(image.resize(size, Image.Resampling.LANCZOS), (0, 42))
    return result


rows = []
for shot in meta['shots']:
    arrays = {}
    for variant in variants:
        with Image.open(OUT / f"{variant}-{shot['name']}.png") as image:
            image.verify()
        arrays[variant] = np.asarray(Image.open(OUT / f"{variant}-{shot['name']}.png").convert('RGB'), dtype=float)
        assert arrays[variant].shape == (720, 1280, 3)
    difference = float(np.mean(np.abs(arrays['calibrated'] - arrays['off'])))
    assert difference > .1, (shot['name'], 'Post-processing produced no observable change')
    for variant, array in arrays.items():
        luma = array @ np.array([.2126, .7152, .0722])
        subjects = []
        for subject in shot['subjects']:
            r = subject['boundsPixels']
            x0, y0 = max(0, math.floor(r['x'])), max(0, math.floor(r['y']))
            x1, y1 = min(1280, math.ceil(r['x'] + r['width'])), min(720, math.ceil(r['y'] + r['height']))
            if x0 >= x1 or y0 >= y1:
                continue
            roi = array[y0:y1, x0:x1]
            subjects.append(dict(name=subject['name'], bbox=[x0,y0,x1,y1],
                                 mean_luma8=round(float(np.mean(luma[y0:y1,x0:x1])),3),
                                 white_pixels_percent=round(float(np.mean(np.min(roi,axis=2)>=250)*100),3),
                                 any_channel_250_percent=round(float(np.mean(np.max(roi,axis=2)>=250)*100),3)))
        rows.append(dict(frame=shot['name'], variant=variant,
                         mean_luma8=round(float(np.mean(luma)),3),
                         median_luma8=round(float(np.median(luma)),3),
                         dark_below8_percent=round(float(np.mean(luma<8)*100),3),
                         subjects=subjects,
                         mean_absolute_display_difference_from_off=round(float(np.mean(np.abs(array-arrays['off']))),3)))

(OUT/'metrics.json').write_text(json.dumps(dict(method="Descriptive sRGB display-pixel luma Y'=0.2126R+0.7152G+0.0722B. Projected AABBs include background; not segmented actors. 8/250 are diagnostic bins, not accessibility thresholds. All variants share actor/camera poses; HDR before tone mapping. No exposure adjustment in these comparison boards.",rows=rows),indent=2),encoding='utf-8')

close = Image.new('RGB',(1280,402))
for i,variant in enumerate(['off','calibrated']):
    image=Image.open(OUT/f'{variant}-02-castle.png')
    close.paste(panel(image,['Antes: pos-processamento desligado','Depois: perfil calibrado'][i]),(i*640,0))
close.save(OUT/'comparison-close.png')

contexts=['02-castle','05-fall-ground','06-castle-overview']
board=Image.new('RGB',(1280,1206))
options=Image.new('RGB',(1440,936))
for row,name in enumerate(contexts):
    for col,variant in enumerate(['off','calibrated']):
        image=Image.open(OUT/f'{variant}-{name}.png')
        board.paste(panel(image,f'{name} / '+['Desligado','Calibrado'][col]),(col*640,row*402))
    for col,variant in enumerate(variants):
        image=Image.open(OUT/f'{variant}-{name}.png')
        options.paste(panel(image,f'{name} / {labels[col]}',(480,270)),(col*480,row*312))
board.save(OUT/'comparison-contexts.png')
options.save(OUT/'comparison-profiles.png')
print(json.dumps([dict(frame=r['frame'],variant=r['variant'],mean=r['mean_luma8'],dark=r['dark_below8_percent'],nanda=[s for s in r['subjects'] if s['name']=='Nanda']) for r in rows if r['frame']=='02-castle'],indent=2))
