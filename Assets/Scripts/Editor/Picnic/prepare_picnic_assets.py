"""Prepare authorised raster assets only; never imports or copies legacy game code."""
from pathlib import Path
import hashlib
import json
import shutil
from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
LEGACY = ROOT.parent / 'legendGirl'
OUT = ROOT / 'Assets/Art/Picnic/Resources/Picnic'
OUT.mkdir(parents=True, exist_ok=True)
manifest = []

def emit(source, name, box=None):
    src = Path(source)
    image = Image.open(src).convert('RGBA')
    if box:
        x, y, w, h = box
        image = image.crop((x, y, x+w, y+h))
    image.save(OUT / (name + '.png'))
    manifest.append(dict(source=str(src.relative_to(src.parents[4])) if False else str(src),
                         output=name+'.png', crop=box,
                         source_sha256=hashlib.sha256(src.read_bytes()).hexdigest(),
                         output_sha256=hashlib.sha256((OUT/(name+'.png')).read_bytes()).hexdigest()))

garden = LEGACY / 'assets/art/garden'
sets = json.loads((garden/'frames.json').read_text())['sets']
for key, names in [('objetos',['grass','wood','stone','cloud','ant0','ant1','caterpillar0','caterpillar1','tree']),
                   ('ambiente',['house','tall-grass','gravel','unused-picnic']),
                   ('lagartaVermelha',['red-caterpillar0','red-caterpillar1'])]:
    data = sets[key]
    for frame, name in zip(data['frames'], names):
        if name == 'unused-picnic': continue
        emit(garden/data['file'], name, tuple(frame[k] for k in ('x','y','w','h')))
emit(garden/'scenery-v1.png','scenery')
emit(garden/'scenery-v1.png','scenery-tile',(480,0,1056,1024))
emit(garden/'source/wall-plant-v1.png','wall')
emit(garden/'source/soil-continuous-v1.png','soil')
emit(garden/'cinematic/house-without-actors-v1.png','opening-bg')
emit(garden/'cinematic/picnic-without-yuna-v1.png','ending-bg')
emit(garden/'runtime/picnic-lateral-v3.png','picnic',(4,4,1753,534))
for i, rect in enumerate([(4,4,458,400),(684,4,473,241),(1364,4,481,346),(4,516,563,344),(684,516,332,359)]):
    emit(garden/'runtime/insects-v1.png', ['bee0','bee1','beetle0','beetle1','worm'][i], rect)
for i, rect in enumerate([(4,4,312,416),(516,4,273,124),(1028,4,279,127),(4,516,283,123),(516,516,235,393),(1028,516,270,268)]):
    emit(garden/'runtime/life-aquatic-v1.png','aquatic'+str(i),rect)

# Preserve current approved characters, with a common canvas and scale per animation.
run = ROOT/'Assets/Art/Illustrated25D/Yuna/Run/Yuna_Run_Atlas_v1.png'
for i in range(8):
    emit(run, 'yuna-run'+str(i), ((i%4)*362,(i//4)*543,362,543))
emit(ROOT/'Assets/Art/Illustrated25D/Yuna/Yuna_Profile_v1.png','yuna-idle')
for i in range(3):
    emit(ROOT/'Assets/Art/BearChase/Resources/BearChaseEnding/ending-yuna-poses-v1.png','yuna-pose'+str(i),(i*512,0,512,512))
for kind, name in [('Sitting','yuna-sitting'),('Sleeping','yuna-sleeping')]:
    emit(ROOT/f'Assets/Art/Illustrated25D/Environment/Opening/Resources/Opening/Yuna_{kind}_Pose_v1.png',name)
for i in range(8):
    emit(ROOT/'Assets/Art/Illustrated25D/Nanda/Flight/Nanda_Flight_Atlas_v1.png','nanda'+str(i),((i%4)*512,(i//4)*592,512,592))
emit(ROOT/'Assets/Art/BearChase/Resources/BearChaseEnding/ending-nanda-blue-poses-v1.png','nanda-rest',(0,0,512,512))
music = LEGACY/'assets/audio/Blues Shuffle Loop.mp3'
shutil.copyfile(music,OUT/'garden-music.mp3')
manifest.append(dict(source=str(music),output='garden-music.mp3',source_sha256=hashlib.sha256(music.read_bytes()).hexdigest()))
(ROOT/'Evidence/Picnic/asset-provenance.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(f'{len(manifest)} authorised assets prepared in {OUT}')
