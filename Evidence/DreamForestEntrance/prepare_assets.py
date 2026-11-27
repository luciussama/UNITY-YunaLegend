"""Reproduce the legacy's authored crops without repainting source pixels."""
import hashlib
import json
from pathlib import Path
import shutil
import sys

sys.path.insert(0, 'C:/Users/sergi/AppData/Local/Temp/dreamforest-entrance-python')
from PIL import Image, ImageDraw, ImageFont
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont

ROOT = Path(__file__).resolve().parents[2]
LEGACY = ROOT.parent / 'legendGirl'
OUT = ROOT / 'Assets/Art/DreamForestEntrance/Resources/DreamForestEntrance'
OUT.mkdir(parents=True, exist_ok=True)

def read_catalog(relative):
    text = (LEGACY / relative).read_text(encoding='utf-8')
    return json.loads(text[text.index('{'):text.rfind('}') + 1])

art = read_catalog('src/js/dream-forest/dreamNightmareArt.js')
chapters = read_catalog('src/js/chapters/chapterArt.js')
records = []

def record(source, target, **extra):
    records.append(dict(source=str(source.relative_to(LEGACY)).replace('\\', '/'),
        destination=str(target.relative_to(ROOT)).replace('\\', '/'),
        sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),
        outputSha256=hashlib.sha256(target.read_bytes()).hexdigest(), **extra))

for key, prefix in [('openingGirl', 'Yuna'), ('openingLook', 'Look')]:
    spec = art[key]
    source = LEGACY / 'assets/art' / spec['file']
    original = Image.open(source).convert('RGBA')
    for index, frame in enumerate(spec['frames']):
        crop = original.crop((frame['x'], frame['y'], frame['x'] + frame['w'], frame['y'] + frame['h']))
        if 'rowMask' in frame:
            # Same polygon as CanvasRenderingContext2D.clip in dreamOpeningRenderer.
            mask = Image.new('L', crop.size)
            rows = frame['rowMask']
            polygon = [(r[0], y) for y, r in enumerate(rows)]
            polygon += [(rows[y][1] + 1, y + 1) for y in range(len(rows) - 1, -1, -1)]
            ImageDraw.Draw(mask).polygon(polygon, fill=255)
            alpha = crop.getchannel('A')
            from PIL import ImageChops
            crop.putalpha(ImageChops.multiply(alpha, mask))
        target = OUT / f'{prefix}{index}.png'
        crop.save(target)
        record(source, target, crop={k:frame[k] for k in ['x','y','w','h','anchorX']},
            hand=frame.get('hand'), referenceHeight=spec['referenceHeight'], rowMask='rowMask' in frame)

source = LEGACY / 'assets/art/chapters/nanda-gestures-v1.png'
f = chapters['fairy']['frames'][1]
target = OUT / 'NandaSleeping.png'
Image.open(source).crop((f['x'], f['y'], f['x']+f['w'], f['y']+f['h'])).save(target)
record(source, target, crop=f)

source = LEGACY / 'assets/art' / art['cinematicForest']['file']
target = OUT / 'Forest.png'
shutil.copyfile(source, target)
record(source, target, unmodified=True)

source = LEGACY / 'assets/fonts/Caveat-Variable.ttf'
target = OUT / 'Caveat600.ttf'
font = instantiateVariableFont(TTFont(source), {'wght':600}, inplace=False)
font.save(target)
record(source, target, weight=600)

# Supersampled radial alpha for the original blurred contact ellipse.
shadow = Image.new('RGBA', (256, 64))
import math
for y in range(64):
    for x in range(256):
        radius = ((x-127.5)/112)**2 + ((y-31.5)/16)**2
        a = round(115 / (1 + math.exp(min(50, (radius-1)*7))))
        shadow.putpixel((x,y), (0,0,0,a))
shadow.save(OUT / 'ContactShadow.png')

# The web controls use the operating system's emoji glyphs; rasterize those same glyphs.
emoji = ImageFont.truetype('C:/Windows/Fonts/seguiemj.ttf', 72)
for name, glyph in [('PauseIcon','⏸'),('PlayIcon','▶'),('SoundIcon','🔊'),('MuteIcon','🔇')]:
    icon = Image.new('RGBA',(180,160))
    draw = ImageDraw.Draw(icon)
    box = draw.textbbox((0,0),glyph,font=emoji,embedded_color=True)
    draw.text(((180-box[2]+box[0])/2-box[0],(160-box[3]+box[1])/2-box[1]),glyph,font=emoji,embedded_color=True)
    icon = icon.crop(icon.getbbox())
    icon.thumbnail((72,72),Image.Resampling.LANCZOS)
    canvas = Image.new('RGBA',(80,80))
    canvas.alpha_composite(icon,((80-icon.width)//2,(80-icon.height)//2))
    canvas.save(OUT/f'{name}.png')

for name, border in [('Control',(250,204,21,89)),('FlagControl',(185,177,196,61))]:
    circle = Image.new('RGBA',(176,176))
    ImageDraw.Draw(circle).ellipse((4,4,172,172),fill=(18,16,23,191),outline=border,width=4)
    circle.resize((88,88),Image.Resampling.LANCZOS).save(OUT/f'{name}.png')

circle=Image.new('RGBA',(176,176))
ImageDraw.Draw(circle).ellipse((4,4,172,172),fill=(245,158,11,217),outline=(254,240,138,255),width=4)
circle.resize((88,88),Image.Resampling.LANCZOS).save(OUT/'PauseControl.png')
for name, radius, border in [('MenuPanel',12,(185,177,196,61)),('SelectBox',10,(185,177,196,102))]:
    panel=Image.new('RGBA',(384,384))
    ImageDraw.Draw(panel).rounded_rectangle((2,2,382,382),radius=radius*4,fill=(25,21,31,255),outline=border,width=4)
    panel.resize((96,96),Image.Resampling.LANCZOS).save(OUT/f'{name}.png')
(ROOT / 'Evidence/DreamForestEntrance/assets-provenance.json').write_text(
    json.dumps(records, ensure_ascii=False, indent=2), encoding='utf-8')
print(f'Imported {len(records)} source-derived assets; original artwork preserved.')
