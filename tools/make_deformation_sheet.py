#!/usr/bin/env python3
"""Compose the T02.3 deformation comparison sheet from the detail renders.

Layout: a grid of labelled panels plus a displacement map between the bind pose and the run pose.
The displacement map is `25% of the bind image + 6x the absolute difference`, so brighter pixels moved more.
Everything comes from the Unity renders; this script does not deform anything itself.

Example:
    python tools/make_deformation_sheet.py --dir Evidence/25d/T02.3/deformation \
        --out Evidence/25d/T02.3/deformation-comparison.png
"""
import argparse
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

TITLES = {
    "01-bind": ("SEM POSE (referencia)", "pintura aprovada, malha em repouso"),
    "02-run-contact": ("CORRIDA - CONTATO", "inclinacao 8,2 graus, compressao 5,5%"),
    "03-run-passing": ("CORRIDA - PASSAGEM", "inclinacao 5,8 graus, compressao 1,5%"),
    "04-land": ("POUSO", "achatamento 7,5%, alargamento 5%"),
    "05-limit": ("LIMITE (nao e candidata)", "inclinacao 25 graus, compressao 12%"),
    "diff": ("MAPA DE DESLOCAMENTO", "corrida menos repouso, 6x ampliado"),
}
ORDER = ["01-bind", "02-run-contact", "03-run-passing", "04-land", "05-limit", "diff"]
CELL = (620, 388)
PAD = 12
LABEL = 54


def font(size, bold=False):
    for name in ("segoeuib.ttf" if bold else "segoeui.ttf",):
        try:
            return ImageFont.truetype("C:/Windows/Fonts/" + name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def displacement(bind, run):
    a = np.asarray(bind.convert("RGB"), dtype=np.float32)
    b = np.asarray(run.convert("RGB"), dtype=np.float32)
    delta = np.abs(a - b).max(axis=2)
    base = a.mean(axis=2) * 0.25
    bright = np.clip(base + delta * 6.0, 0, 255)
    image = Image.fromarray(np.stack([bright] * 3, axis=2).astype(np.uint8), "RGB")
    return image, delta


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--dir", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--bind", default="01-bind")
    parser.add_argument("--run", default="02-run-contact")
    arguments = parser.parse_args()

    directory = Path(arguments.dir)
    images = {name: Image.open(directory / (name + ".png")).convert("RGB") for name in ORDER[:-1]}
    diff, delta = displacement(images[arguments.bind], images[arguments.run])
    images["diff"] = diff

    columns, rows = 3, 2
    width = columns * CELL[0] + (columns + 1) * PAD
    height = rows * (CELL[1] + LABEL) + (rows + 1) * PAD
    sheet = Image.new("RGB", (width, height), (16, 18, 24))
    draw = ImageDraw.Draw(sheet)
    title_font, note_font = font(20, True), font(15)

    for index, name in enumerate(ORDER):
        column, row = index % columns, index // columns
        left = PAD + column * (CELL[0] + PAD)
        top = PAD + row * (CELL[1] + LABEL + PAD)
        panel = images[name].resize(CELL, Image.LANCZOS)
        sheet.paste(panel, (left, top))
        title, note = TITLES[name]
        draw.text((left, top + CELL[1] + 6), title, font=title_font, fill=(238, 232, 220))
        draw.text((left, top + CELL[1] + 30), note, font=note_font, fill=(150, 160, 175))

    output = Path(arguments.out)
    output.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output)
    changed = int((delta > 8).sum())
    print(f"{output}: {width}x{height}, {len(ORDER)} paineis")
    print(f"deslocamento maximo {delta.max():.0f}/255; pixels alterados (>8) {changed} de {delta.size} "
          f"({100.0 * changed / delta.size:.2f}% da area do painel)")


if __name__ == "__main__":
    main()
