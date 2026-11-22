"""Slice drawings and register planted soles; never scale poses independently."""
import argparse
import hashlib
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Build/Cinematics25WebControl/pythondeps'))
from PIL import Image


def foreground(image):
    """Find the figure, excluding disconnected matte specks from measurement.

    This mask is used only for landmarks. It never replaces the artwork alpha.
    """
    width, height = image.size
    remaining = bytearray(1 if alpha >= 192 else 0 for alpha in image.getchannel('A').tobytes())
    largest = []
    for seed in range(len(remaining)):
        if not remaining[seed]:
            continue
        stack = [seed]
        remaining[seed] = 0
        component = []
        while stack:
            point = stack.pop()
            component.append(point)
            x, y = point % width, point // width
            neighbours = []
            if x > 0:
                neighbours.append(point - 1)
            if x + 1 < width:
                neighbours.append(point + 1)
            if y > 0:
                neighbours.append(point - width)
            if y + 1 < height:
                neighbours.append(point + width)
            for neighbour in neighbours:
                if remaining[neighbour]:
                    remaining[neighbour] = 0
                    stack.append(neighbour)
        if len(component) > len(largest):
            largest = component
    if not largest:
        raise ValueError('No connected opaque figure found')
    xs = [point % width for point in largest]
    ys = [point // width for point in largest]
    return (min(xs), min(ys), max(xs) + 1, max(ys) + 1), largest


def opaque_box(image):
    return foreground(image)[0]


def sole(image):
    points = []
    box, component = foreground(image)
    lower_quarter = box[1] + (box[3] - box[1]) * .75
    for point in component:
        x, y = point % image.width, point // image.width
        if y < lower_quarter:
            continue
        red, green, blue, alpha = image.getpixel((x, y))
        if red > green * 1.25 and red > blue * 1.1 and green < 140:
            points.append((x, y))
    if not points:
        raise ValueError('No planted shoe landmark found')
    return (min(x for x, _ in points) + max(x for x, _ in points)) / 2, max(y for _, y in points) + 1


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('raw', type=Path)
    parser.add_argument('previous', type=Path)
    parser.add_argument('output', type=Path)
    parser.add_argument('--start', type=int, required=True)
    parser.add_argument('--count', type=int, default=9)
    parser.add_argument('--columns', type=int, default=3)
    parser.add_argument('--rows', type=int, default=3)
    args = parser.parse_args()
    image = Image.open(args.raw).convert('RGBA')
    if image.getchannel('A').histogram()[0] == 0:
        raise ValueError('Sheet has no fully transparent background')
    previous = Image.open(args.previous).convert('RGBA')
    previous_box = opaque_box(previous)
    cells = []
    for index in range(args.count):
        row, column = divmod(index, args.columns)
        cells.append(image.crop((round(column * image.width / args.columns),
                                 round(row * image.height / args.rows),
                                 round((column + 1) * image.width / args.columns),
                                 round((row + 1) * image.height / args.rows))))
    first_box = opaque_box(cells[0])
    # One scale for the entire page, from its continuity pose. Later poses keep
    # their own anatomical changes, unlike fitting each opaque bounding box.
    scale = ((previous_box[3] - previous_box[1]) * 512 / previous.width
             / (first_box[3] - first_box[1]))
    args.output.mkdir(parents=True, exist_ok=True)
    registrations = []
    for index, cell in enumerate(cells):
        scaled = cell.resize((round(cell.width * scale), round(cell.height * scale)), Image.Resampling.LANCZOS)
        center, bottom = sole(scaled)
        offset = (round(256 - center), 480 - bottom)
        box = opaque_box(scaled)
        if box[0] + offset[0] < 0 or box[2] + offset[0] > 512 or box[1] + offset[1] < 0:
            raise ValueError(f'Opaque drawing would be clipped: {args.start + index}')
        canvas = Image.new('RGBA', (512, 512))
        canvas.alpha_composite(scaled, offset)
        canvas.save(args.output / f'f{args.start + index:03}.png')
        registrations.append({'frame': args.start + index, 'translation': offset, 'soleY': 480})
    metadata = {'raw': str(args.raw), 'sha256': hashlib.sha256(args.raw.read_bytes()).hexdigest(),
                'pageScale': scale, 'canvas': [512, 512], 'frames': registrations}
    (args.output / 'registration.json').write_text(json.dumps(metadata, indent=2), encoding='utf-8')
    print(json.dumps({'frames': args.count, 'pageScale': scale, 'output': str(args.output)}))


if __name__ == '__main__':
    main()
