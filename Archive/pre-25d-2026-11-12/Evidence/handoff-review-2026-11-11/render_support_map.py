"""Draw the measured mesh/box contact grid, without modifying source images."""
from pathlib import Path
import json
import sys

OUT = Path(__file__).resolve().parent
sys.path.insert(0, str(OUT.parents[1] / 'Build/VisualAuditTools'))
from PIL import Image, ImageDraw, ImageFont

data = json.loads((OUT / 'probe.json').read_text())
image = Image.new('RGB', (1120, 660), '#161b24')
draw = ImageDraw.Draw(image)
font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 21)
small = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 17)
colors = ['#e85d68', '#45be9d', '#e6ad4c', '#7f9bea']
for column, p in enumerate(data['platforms']):
    x0, y0, cell = 65 + column * 555, 95, 21
    draw.text((x0, 20), ['Urso 00', 'Livros 01'][column], fill='white', font=font)
    draw.text((x0, 50), 'Grade sobre o topo do colisor (vista de cima)', fill='#cbd2df', font=small)
    for x in range(21):
        for z in range(21):
            xx, yy = x0 + x * cell, y0 + (20-z) * cell
            draw.rectangle((xx, yy, xx+cell-2, yy+cell-2), fill=colors[p['support'][x*21+z]])
    draw.line((x0-10, y0+10*cell+9, x0+21*cell+5, y0+10*cell+9), fill='white', width=2)
    draw.text((x0, 545), 'Linha branca: corredor de corrida Z=0', fill='white', font=small)
for i, label in enumerate(['Sem malha sob o ponto', 'Coincide (ate 2 cm)', 'Malha abaixo do apoio', 'Malha acima do apoio']):
    x = 25 + (i % 2) * 555
    y = 590 + (i // 2) * 28
    draw.rectangle((x,y+3,x+15,y+18), fill=colors[i])
    draw.text((x+24,y), label, fill='white', font=small)
image.save(OUT / 'contact-support-map.png')
