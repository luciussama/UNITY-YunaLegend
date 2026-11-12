"""Reproduce the visual-audit figures and descriptive pixel measurements.

Run from repository root after installing the pinned dependencies in
Build/VisualAuditTools. This never modifies source captures or game assets.
"""
import csv
import hashlib
import json
import math
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / 'Build/VisualAuditTools'))
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter

OUT = Path(__file__).resolve().parents[1]
FONT = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 22)
SMALL = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 16)
META = json.loads((OUT / 'unity-metadata.json').read_text(encoding='utf-8'))


def box(rect):
    return [math.floor(rect['x']), math.floor(rect['y']),
            math.ceil(rect['x'] + rect['width']), math.ceil(rect['y'] + rect['height'])]


def luma(array):
    return array[..., :3] @ np.array([.2126, .7152, .0722])


def stats(image, bbox=None):
    array = np.asarray(image.convert('RGB'), dtype=np.float64)
    if bbox is not None:
        x0, y0, x1, y1 = bbox
        x0, y0, x1, y1 = max(0,x0), max(0,y0), min(image.width,x1), min(image.height,y1)
        array = array[y0:y1, x0:x1]
    values = luma(array)
    return dict(mean_luma8=round(float(np.mean(values)),3),
                median_luma8=round(float(np.median(values)),3),
                p90_luma8=round(float(np.percentile(values,90)),3),
                p99_luma8=round(float(np.percentile(values,99)),3),
                percent_luma_below_8=round(float(np.mean(values<8)*100),3),
                percent_any_channel_at_least_250=round(float(np.mean(np.max(array,axis=2)>=250)*100),3),
                percent_all_channels_at_least_250=round(float(np.mean(np.min(array,axis=2)>=250)*100),3))


def gray(image):
    return Image.fromarray(np.clip(luma(np.asarray(image.convert('RGB'),dtype=float)),0,255).astype('uint8')).convert('RGB')


def labeled(image, title, note='', size=(640,360)):
    panel = Image.new('RGB',(size[0],size[1]+62),(22,25,32))
    panel.paste(image.resize(size,Image.Resampling.LANCZOS),(0,62))
    draw=ImageDraw.Draw(panel)
    draw.text((12,6),title,font=FONT,fill=(240,241,245))
    draw.text((12,34),note,font=SMALL,fill=(169,184,204))
    return panel


rows=[]
annotated=[]
for shot in META['shots']:
    path=OUT/f"unity-{shot['name']}.png"
    image=Image.open(path).convert('RGB')
    gray(image).save(OUT/f"unity-{shot['name']}-gray.png")
    gray(image).resize((320,180),Image.Resampling.LANCZOS).filter(ImageFilter.GaussianBlur(1.2)).save(OUT/f"unity-{shot['name']}-squint.png")
    art=image.copy();draw=ImageDraw.Draw(art)
    rows.append(dict(frame=shot['name'],subject='FRAME',**stats(image)))
    for i,subject in enumerate(shot['subjects']):
        bbox=box(subject['boundsPixels'])
        color=[(110,204,255),(255,216,98),(135,238,176),(216,170,255)][i]
        draw.rectangle(bbox,outline=color,width=2)
        label=f"{subject['name']} ({bbox[2]-bbox[0]}x{bbox[3]-bbox[1]}px)"
        draw.text((bbox[0],max(0,bbox[1]-23)),label,font=SMALL,fill=color)
        x0,y0,x1,y1=bbox
        rim=12
        outer=(max(0,x0-rim),max(0,y0-rim),min(image.width,x1+rim),min(image.height,y1+rim))
        pixels=np.asarray(image,dtype=float)
        mask=np.zeros((image.height,image.width),bool)
        mask[outer[1]:outer[3],outer[0]:outer[2]]=True
        mask[max(0,y0):min(image.height,y1),max(0,x0):min(image.width,x1)]=False
        near=float(np.median(luma(pixels)[mask]))
        measurement=stats(image,bbox)
        rows.append(dict(frame=shot['name'],subject=subject['name'],bbox=bbox,
                         nearby_border_median_luma8=round(near,3),
                         p90_minus_nearby_border=round(measurement['p90_luma8']-near,3),**measurement))
    art.save(OUT/f"unity-{shot['name']}-roi.png")
    annotated.append(art)

with (OUT/'luma-measurements.csv').open('w',newline='',encoding='utf-8-sig') as stream:
    fields=list(dict.fromkeys(key for row in rows for key in row))
    writer=csv.DictWriter(stream,fieldnames=fields);writer.writeheader();writer.writerows(rows)

all_gray=Image.new('RGB',(1280,844),(22,25,32))
all_color=Image.new('RGB',(1280,844),(22,25,32))
for i,shot in enumerate(META['shots']):
    image=Image.open(OUT/f"unity-{shot['name']}.png").convert('RGB')
    all_color.paste(labeled(image,shot['name'],'Unity / Play Mode controlado / sem OnGUI'),((i%2)*640,(i//2)*422))
    all_gray.paste(labeled(gray(image),shot['name'],'Cinza Y\' Rec.709 / sem ajuste de exposicao'),((i%2)*640,(i//2)*422))
all_color.save(OUT/'unity-four-points-color.png')
all_gray.save(OUT/'unity-four-points-gray.png')

# Web screenshots retain the full browser viewport. Crop only the measured game
# container, then resize for the comparison board. Original JPEGs stay untouched.
pairs=[('01-start','web-01-start.jpg'),('02-castle','web-02-escape-start.jpg')]
comparison=Image.new('RGB',(1280,844),(22,25,32))
for i,(name,webname) in enumerate(pairs):
    unity=Image.open(OUT/f'unity-{name}.png').convert('RGB')
    web=Image.open(OUT/webname).convert('RGB').crop((120,68,1160,652))
    comparison.paste(labeled(web,f'Web / {name}','UI observada; cameras e enquadramentos diferentes'),(0,i*422))
    comparison.paste(labeled(unity,f'Unity / {name}','Snapshot controlado; sem overlays OnGUI'),(640,i*422))
    gray(web).save(OUT/f'web-{name}-game-gray.png')
comparison.save(OUT/'comparison-start-castle.png')

# Manual Web ROIs are approximate visible rectangles; they do not establish exact
# sprite masks or calibrated cross-renderer luminance equivalence.
web_roi={
    'web-01-start.jpg': {'Yuna':(193,582,221,636),'Nanda':(239,517,273,555),'Bear':(310,564,532,634)},
    'web-02-escape-start.jpg': {'Yuna':(311,333,339,383),'Nanda':(591,334,635,374),'Train':(552,335,682,431)}
}
web_rows=[]
for filename,subjects in web_roi.items():
    image=Image.open(OUT/filename).convert('RGB')
    for name,bbox in subjects.items():
        web_rows.append(dict(file=filename,subject=name,bbox=list(bbox),**stats(image,bbox)))

measurements=dict(method="Descriptive display-encoded luma Y'=0.2126R+0.7152G+0.0722B, range0..255. Bounds are projected renderer AABBs, not segmentation masks. Border median uses12px surrounding ring. Below8/above250 are diagnostic bins, not game accessibility pass/fail thresholds. No exposure adjustment. Cross-renderer measurements are not calibrated or pixel-registered.",
                  unity=rows,web_approximate_rois=web_rows,
                  initial_fairy_player_distance=round(math.dist(list(META['shots'][0]['player'].values()),list(META['shots'][0]['fairy'].values())),3),
                  web_advanced_comparison="Missing: false door and dream portal not reached via UI; do not infer canonical appearance from Unity captures.")
(OUT/'measurements.json').write_text(json.dumps(measurements,indent=2,ensure_ascii=False),encoding='utf-8')
print(json.dumps(dict(unity_subjects=[r for r in rows if r['subject']!='FRAME'],web=web_rows,initial_distance=measurements['initial_fairy_player_distance']),indent=2))
