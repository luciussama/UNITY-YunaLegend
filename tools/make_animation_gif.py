#!/usr/bin/env python3
"""Build review GIFs from captured Unity PNG frames (T02.3 and later micro-deliveries).

The frames are raw Play Mode renders; this script only resizes/crops and assembles the 8-bit GIF.
No retouching of geometry, colour or timing beyond the requested frame step.

Example:
    python tools/make_animation_gif.py --frames Build/25d/AnimationFrames \
        --out Evidence/25d/T02.3/animation-run.gif --from 18 --to 120 --fps 15 --max-width 720
"""
import argparse
import re
from pathlib import Path

from PIL import Image


def frame_number(path):
    match = re.search(r"(\d+)", path.stem)
    return int(match.group(1)) if match else -1


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--frames", required=True, help="directory with frame-XXXXX.png renders at 60 fps")
    parser.add_argument("--out", required=True)
    parser.add_argument("--from", dest="first", type=int, default=0)
    parser.add_argument("--to", dest="last", type=int, default=10 ** 9)
    parser.add_argument("--fps", type=int, default=15)
    parser.add_argument("--source-fps", type=int, default=30, help="rate of the captured frame files (T02.3 capture saves every 2nd 60 Hz frame)")
    parser.add_argument("--colors", type=int, default=128)
    parser.add_argument("--max-width", type=int, default=720)
    parser.add_argument("--crop", default=None, help="x,y,w,h in source pixels, applied before resizing")
    parser.add_argument("--zoom", type=float, default=0.0, help="scale after crop; 0 keeps --max-width")
    arguments = parser.parse_args()

    directory = Path(arguments.frames)
    paths = sorted((p for p in directory.glob("frame-*.png") if arguments.first <= frame_number(p) <= arguments.last),
                   key=frame_number)
    if not paths:
        raise SystemExit("no frames in range: " + str(directory))
    step = max(1, round(arguments.source_fps / arguments.fps))
    selected = paths[::step]
    crop = tuple(int(value) for value in arguments.crop.split(",")) if arguments.crop else None

    images = []
    for path in selected:
        image = Image.open(path).convert("RGB")
        if crop:
            left, top, width, height = crop
            image = image.crop((left, top, left + width, top + height))
        if arguments.zoom > 0:
            size = (round(image.width * arguments.zoom), round(image.height * arguments.zoom))
        else:
            size = (arguments.max_width, round(image.height * arguments.max_width / image.width))
        images.append(image.resize(size, Image.LANCZOS))

    output = Path(arguments.out)
    output.parent.mkdir(parents=True, exist_ok=True)
    palette = [frame.quantize(colors=arguments.colors, method=Image.MEDIANCUT) for frame in images]
    palette[0].save(output, save_all=True, append_images=palette[1:], duration=1000 // arguments.fps,
                    loop=0, optimize=True, disposal=2)
    print(f"{output}: {len(palette)} frames, {images[0].width}x{images[0].height}, "
          f"{arguments.fps} fps, {output.stat().st_size / (1024 * 1024):.2f} MB")


if __name__ == "__main__":
    main()
