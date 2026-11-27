"""Assemble unretouched legacy/Unity evidence; time-dependent frames are not synchronized."""
import sys
sys.path.insert(0, 'C:/Users/sergi/AppData/Local/Temp/dreamforest-entrance-python')
from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
import csv
import json
import hashlib
import subprocess

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf',24)
for beat,name in [(1,'discover'),(2,'shake'),(4,'plead'),(5,'alone')]:
    source = OUT / f'Legacy/{beat}-{name}.jpg'
    target = OUT / f'Unity/1043x840/{beat}-{name}.png'
    if not source.exists() or not target.exists():
        continue
    a,b=Image.open(source).convert('RGB'),Image.open(target).convert('RGB')
    canvas=Image.new('RGB',(a.width+b.width,a.height+48),(15,20,29))
    draw=ImageDraw.Draw(canvas)
    draw.text((20,10),'LEGADO - captura jogavel',font=font,fill='white')
    draw.text((a.width+20,10),'UNITY - camera nativa (tempo nao sincronizado)',font=font,fill='white')
    canvas.paste(a,(0,48))
    canvas.paste(b,(a.width,48))
    canvas.save(OUT/f'compare-{name}.png')

previews=[]
for file in sorted((OUT/'Legacy').glob('*.jpg')):
    im=Image.open(file).convert('RGB')
    im.thumbnail((260,210))
    tile=Image.new('RGB',(280,250),(15,20,29))
    tile.paste(im,((280-im.width)//2,30))
    ImageDraw.Draw(tile).text((5,5),file.name,font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',14),fill='white')
    previews.append(tile)
sheet=Image.new('RGB',(280*4,250*((len(previews)+3)//4)),(15,20,29))
for i,im in enumerate(previews): sheet.paste(im,((i%4)*280,(i//4)*250))
sheet.save(OUT/'legacy-index.png')

timeline=OUT/'PlayMode/timeline.csv'
if timeline.exists() and (OUT/'PlayMode/result.txt').exists():
    rows=list(csv.DictReader(timeline.open()))
    if rows:
        # Retain real observed durations in the review video; the capture rate is not the simulation rate.
        concat=OUT/'PlayMode/concat.txt'
        with concat.open('w',encoding='utf-8') as stream:
            for i,row in enumerate(rows):
                stream.write(f"file 'frame-{int(row['frame']):04d}.png'\n")
                duration=(float(rows[i+1]['elapsed'])-float(row['elapsed'])) if i+1<len(rows) else 1/12
                stream.write(f'duration {duration:.6f}\n')
            stream.write(f"file 'frame-{int(rows[-1]['frame']):04d}.png'\n")
        import imageio_ffmpeg
        command=[imageio_ffmpeg.get_ffmpeg_exe(),'-y','-loglevel','error','-f','concat','-safe','0','-i',str(concat),
            '-vf','fps=24','-c:v','libx264','-crf','19','-pix_fmt','yuv420p','-movflags','+faststart',str(OUT/'entrance-playmode.mp4')]
        subprocess.run(command,check=True)
        print(f'Video assembled from {len(rows)} real PlayMode samples; silent review file, audio not recorded.')

paths=list((ROOT/'Assets/Scripts/Runtime/DreamForestEntrance').rglob('*'))
paths+=list((ROOT/'Assets/Scripts/Editor/DreamForestEntrance').rglob('*'))
paths+=list((ROOT/'Assets/Scripts/Tests/EditMode/DreamForestEntrance').rglob('*'))
paths+=list((ROOT/'Assets/Art/DreamForestEntrance').rglob('*.shader'))
paths += [ROOT/'Assets/Scripts/Runtime/DreamForest/Runtime/DreamForestStageBootstrap.cs',
          ROOT/'Assets/Scripts/Runtime/DreamForest/Runtime/YunaLegend.DreamForest.Runtime.asmdef']
records={str(p.relative_to(ROOT)).replace('\\','/'):hashlib.sha256(p.read_bytes()).hexdigest()
    for p in paths if p.is_file() and p.suffix in ['.cs','.asmdef','.shader']}
(OUT/'sources.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
print('Review images and source hashes prepared.')
