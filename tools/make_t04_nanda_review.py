"""Build T04.1 Nanda flight review GIFs, pose strip and key-frame sheets."""
from pathlib import Path
import json
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image, ImageDraw, ImageFont

FRAMES = ROOT / "Build/25d/T04Nanda-120"
PROOF = ROOT / "Evidence/25d/T04.1/final-120/flight-proof.json"
OUT = ROOT / "Evidence/25d/T04.1/review"
KEY_FRAMES = [
    (0.13, "Direita - pose 0"),
    (0.40, "Direita - subida"),
    (0.67, "Direita - neutro"),
    (0.93, "Direita - descida"),
    (1.33, "Direita - ciclo 2"),
    (2.13, "Direita - ultimo"),
    (2.40, "Virada / flipX"),
    (2.67, "Cruzamento"),
    (3.07, "Esquerda - pose 0"),
    (3.60, "Esquerda - subida"),
    (4.13, "Esquerda - neutro"),
    (4.67, "Esquerda - ultimo"),
]


def font(size):
    try:
        return ImageFont.truetype("C:/Windows/Fonts/arial.ttf", size)
    except OSError:
        return ImageFont.load_default()


def load(group, index):
    return Image.open(FRAMES / group / f"{index:03d}.png").convert("RGBA")


def make_gif(group, name, fps, width):
    count = len(list((FRAMES / group).glob("*.png")))
    images = []
    for index in range(count):
        image = load(group, index).convert("RGB")
        size = (width, round(image.height * width / image.width))
        images.append(image.resize(size, Image.LANCZOS).quantize(colors=128, method=Image.MEDIANCUT))
    output = OUT / name
    images[0].save(output, save_all=True, append_images=images[1:], duration=1000 // fps,
                   loop=0, optimize=True, disposal=2)
    print(f"{output.relative_to(ROOT)}: {len(images)} frames, {images[0].width}x{images[0].height}, "
          f"{fps} fps, {output.stat().st_size / (1024 * 1024):.2f} MB")


def make_sheet(group, name, cell_width, columns=4):
    rows = (len(KEY_FRAMES) + columns - 1) // columns
    sample = load(group, 0)
    cell_height = round(sample.height * cell_width / sample.width)
    label_height = 28
    margin = 8
    title_height = 44
    width = columns * cell_width + (columns + 1) * margin
    height = title_height + rows * (cell_height + label_height) + (rows + 1) * margin
    sheet = Image.new("RGB", (width, height), (28, 30, 36))
    draw = ImageDraw.Draw(sheet)
    draw.text((margin, 10), f"T04.1 Nanda em voo - {group} - 120 Hz simulacao / 15 fps captura",
              fill=(255, 255, 255), font=font(19))
    for cell, (seconds, label) in enumerate(KEY_FRAMES):
        index = round(seconds * 15)
        image = load(group, index).convert("RGB")
        image = image.resize((cell_width, cell_height), Image.LANCZOS)
        column = cell % columns
        row = cell // columns
        x = margin + column * (cell_width + margin)
        y = title_height + margin + row * (cell_height + label_height + margin)
        sheet.paste(image, (x, y))
        draw.text((x, y + cell_height + 4), f"t={seconds:04.2f}s  {label}", fill=(240, 240, 240), font=font(15))
    output = OUT / name
    sheet.save(output, optimize=True)
    print(f"{output.relative_to(ROOT)}: {sheet.width}x{sheet.height}, {output.stat().st_size / (1024 * 1024):.2f} MB")


def make_pose_strip():
    proof = json.loads(PROOF.read_text(encoding="utf-8"))
    first = {}
    for sample in proof["samples"]:
        first.setdefault(sample["pose"], sample)
    cell = 260
    margin = 8
    title = 46
    sheet = Image.new("RGB", (4 * cell + 5 * margin, title + 2 * (cell + 34) + 3 * margin), (24, 26, 32))
    draw = ImageDraw.Draw(sheet)
    draw.text((margin, 10), "T04.1 - oito poses renderizadas no caminho padrao de sprite (execucao 120 Hz)",
              fill=(255, 255, 255), font=font(19))
    for pose in range(8):
        sample = first[pose]
        image = load("detail", sample["frame"]).convert("RGB").resize((cell, cell), Image.LANCZOS)
        column = pose % 4
        row = pose // 4
        x = margin + column * (cell + margin)
        y = title + margin + row * (cell + 34 + margin)
        sheet.paste(image, (x, y))
        draw.text((x, y + cell + 4), f"pose {pose}  t={sample['t']:.2f}s  flipX={str(sample['flipX']).lower()}",
                  fill=(235, 235, 235), font=font(15))
    output = OUT / "pose-strip.png"
    sheet.save(output, optimize=True)
    print(f"{output.relative_to(ROOT)}: {sheet.width}x{sheet.height}")


def make_flip_compare():
    proof = json.loads(PROOF.read_text(encoding="utf-8"))
    right = Image.open(ROOT / "Evidence/25d/T04.1/final-120/flip-probe-right.png").convert("RGB")
    left = Image.open(ROOT / "Evidence/25d/T04.1/final-120/flip-probe-left.png").convert("RGB")
    margin = 8
    title = 58
    sheet = Image.new("RGB", (2 * right.width + 3 * margin, title + right.height + 3 * margin + 26), (24, 26, 32))
    draw = ImageDraw.Draw(sheet)
    draw.text((margin, 8), "T04.1 - sonda renderizada de espelhamento (material padrao do SpriteRenderer)",
              fill=(255, 255, 255), font=font(18))
    draw.text((margin, 34), proof["flipDiagnostic"], fill=(210, 220, 240), font=font(15))
    sheet.paste(right, (margin, title + margin))
    sheet.paste(left, (2 * margin + right.width, title + margin))
    draw.text((margin, title + margin + right.height + 2), "flipX=false", fill=(235, 235, 235), font=font(15))
    draw.text((2 * margin + right.width, title + margin + right.height + 2), "flipX=true",
              fill=(235, 235, 235), font=font(15))
    output = OUT / "flip-compare.png"
    sheet.save(output, optimize=True)
    print(f"{output.relative_to(ROOT)}: {sheet.width}x{sheet.height}")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    make_gif("game", "game-normal.gif", 15, 960)
    make_gif("game", "game-slow.gif", 10, 960)
    make_gif("detail", "detail-normal.gif", 15, 640)
    make_gif("detail", "detail-slow.gif", 10, 640)
    make_sheet("game", "game-review-sheet.png", 426)
    make_sheet("detail", "detail-review-sheet.png", 320)
    make_pose_strip()
    make_flip_compare()


if __name__ == "__main__":
    main()
