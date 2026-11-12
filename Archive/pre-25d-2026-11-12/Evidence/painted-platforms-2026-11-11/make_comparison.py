"""Assemble unretouched evidence crops. Requires Pillow and Windows Arial."""
from pathlib import Path
import sys

OUT = Path(__file__).resolve().parent
sys.path.insert(0, str(OUT.parents[1] / 'Build/VisualAuditTools'))
from PIL import Image, ImageDraw, ImageFont

font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 22)
board = Image.new('RGB', (1280, 860), (22, 25, 32))
draw = ImageDraw.Draw(board)
rows = [
    ('03-bear-three-quarter.png', (330, 305, 990, 720), 'Urso: pintura e laterais arredondadas'),
    ('07-books-three-quarter.png', (325, 265, 960, 715), 'Livros: couro pintado e detalhes dourados'),
]
for row, (name, crop, title) in enumerate(rows):
    for col, (variant, label) in enumerate([('before', 'Antes'), ('after', 'Depois')]):
        image = Image.open(OUT / variant / name).convert('RGB').crop(crop)
        image.thumbnail((640, 390), Image.Resampling.LANCZOS)
        x, y = col * 640, row * 430
        draw.text((x + 12, y + 7), label + ' | ' + title, font=font, fill='white')
        board.paste(image, (x + (640-image.width)//2, y + 40 + (390-image.height)//2))
board.save(OUT / 'comparison-detail.png')
