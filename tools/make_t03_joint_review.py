"""Build T03 joint review GIFs and key-frame sheets from captured PNGs."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image, ImageDraw, ImageFont

FRAMES = ROOT / "Build/25d/T03JointFinalV4-120"
OUT = ROOT / "Evidence/25d/T03/review"
KEY_FRAMES = [
    (1.2, "Idle"),
    (3.2, "Run direita"),
    (4.35, "Salto"),
    (4.85, "Apex/queda"),
    (5.0, "Pouso"),
    (6.2, "Tropeco direita"),
    (7.7, "Run esquerda"),
    (11.23, "Tropeco esquerda"),
    (13.5, "Idle final"),
]


def load(group, index):
    return Image.open(FRAMES / group / f"{index:03d}.png").convert("RGBA")


def font(size):
    try:
        return ImageFont.truetype("C:/Windows/Fonts/arial.ttf", size)
    except OSError:
        return ImageFont.load_default()


def make_gif(group, name, fps, width):
    count = len(list((FRAMES / group).glob("*.png")))
    step = max(1, round(30 / fps))
    images = []
    for index in range(0, count, step):
        image = load(group, index).convert("RGB")
        size = (width, round(image.height * width / image.width))
        images.append(image.resize(size, Image.LANCZOS).quantize(colors=128, method=Image.MEDIANCUT))
    output = OUT / name
    images[0].save(output, save_all=True, append_images=images[1:], duration=1000 // fps,
                   loop=0, optimize=True, disposal=2)
    print(f"{output.relative_to(ROOT)}: {len(images)} frames, {images[0].width}x{images[0].height}, "
          f"{fps} fps, {output.stat().st_size / (1024 * 1024):.2f} MB")


def make_sheet(group, name, cell_width, columns=3):
    rows = (len(KEY_FRAMES) + columns - 1) // columns
    sample = load(group, 0)
    cell_height = round(sample.height * cell_width / sample.width)
    label_height = 30
    margin = 8
    title_height = 44
    width = columns * cell_width + (columns + 1) * margin
    height = title_height + rows * (cell_height + label_height) + (rows + 1) * margin
    sheet = Image.new("RGB", (width, height), (28, 30, 36))
    draw = ImageDraw.Draw(sheet)
    draw.text((margin, 10), f"T03 conjunto — {group} — 120 Hz simulação / 30 fps captura", fill=(255, 255, 255), font=font(20))
    for cell, (seconds, label) in enumerate(KEY_FRAMES):
        index = round(seconds * 30)
        image = load(group, index).convert("RGB")
        image = image.resize((cell_width, cell_height), Image.LANCZOS)
        column = cell % columns
        row = cell // columns
        x = margin + column * (cell_width + margin)
        y = title_height + margin + row * (cell_height + label_height + margin)
        sheet.paste(image, (x, y))
        draw.text((x, y + cell_height + 5), f"t={seconds:04.2f}s  {label}", fill=(240, 240, 240), font=font(17))
    output = OUT / name
    sheet.save(output, optimize=True)
    print(f"{output.relative_to(ROOT)}: {sheet.width}x{sheet.height}, {output.stat().st_size / (1024 * 1024):.2f} MB")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    make_gif("game", "game-normal.gif", 15, 960)
    make_gif("game", "game-slow.gif", 10, 960)
    make_gif("detail", "detail-normal.gif", 15, 640)
    make_gif("detail", "detail-slow.gif", 10, 640)
    make_sheet("game", "game-review-sheet.png", 426)
    make_sheet("detail", "detail-review-sheet.png", 320)


if __name__ == "__main__":
    main()
