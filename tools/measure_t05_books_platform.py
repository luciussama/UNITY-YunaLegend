#!/usr/bin/env python3
# Measure the approved books-v1.png top cover for the T05.2 support contract.
import hashlib
import json
import math
import struct
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'Evidence/25d/T01/books-v1.png'
OUTPUT = ROOT / 'Assets/Art/Illustrated25D/Environment/Books/Resources/Books_Platform_Profile.json'
ALPHA = 32
STACK_HEIGHT = 1.11
PHYSICAL_DEPTH = 1.15

# Measured on the approved painting: the rigid top cover of the upper volume.
# Image coordinates are inclusive pixels with y pointing down.
FRONT_LEFT = (274, 208)
BACK_LEFT = (532, 52)
BACK_RIGHT = (1428, 192)
FRONT_RIGHT = (1245, 355)


def paeth(a, b, c):
    p = a + b - c
    pa, pb, pc = abs(p-a), abs(p-b), abs(p-c)
    return a if pa <= pb and pa <= pc else (b if pb <= pc else c)


def read_png(path):
    data = path.read_bytes()
    assert data[:8] == b'\x89PNG\r\n\x1a\n'
    pos, chunks = 8, bytearray()
    while pos < len(data):
        size = struct.unpack('>I', data[pos:pos+4])[0]
        kind, body = data[pos+4:pos+8], data[pos+8:pos+8+size]
        pos += size + 12
        if kind == b'IHDR':
            w, h, depth, color, _, _, interlace = struct.unpack('>IIBBBBB', body)
        elif kind == b'IDAT':
            chunks.extend(body)
        elif kind == b'IEND':
            break
    assert depth == 8 and color == 6 and interlace == 0
    raw, stride, rows = zlib.decompress(bytes(chunks)), w * 4, bytearray(h * w * 4)
    prev = bytearray(stride)
    cursor = 0
    for y in range(h):
        filt = raw[cursor]
        cursor += 1
        row = bytearray(raw[cursor:cursor+stride])
        cursor += stride
        for x in range(stride):
            left = row[x-4] if x >= 4 else 0
            up = prev[x]
            upper = prev[x-4] if x >= 4 else 0
            if filt == 1: row[x] = (row[x] + left) & 255
            elif filt == 2: row[x] = (row[x] + up) & 255
            elif filt == 3: row[x] = (row[x] + ((left + up) >> 1)) & 255
            elif filt == 4: row[x] = (row[x] + paeth(left, up, upper)) & 255
            else: assert filt == 0
        rows[y*stride:(y+1)*stride] = row
        prev = row
    return w, h, bytes(rows)


def alpha_at(rgba, stride, x, y):
    return rgba[y*stride+x*4+3]


def measure(path):
    w, h, rgba = read_png(path)
    stride = w * 4
    min_x, min_y, max_x, max_y = w, h, -1, -1
    for y in range(h):
        for x in range(w):
            if alpha_at(rgba, stride, x, y) < ALPHA:
                continue
            min_x, max_x = min(min_x, x), max(max_x, x)
            min_y, max_y = min(min_y, y), max(max_y, y)

    corners = (FRONT_LEFT, BACK_LEFT, BACK_RIGHT, FRONT_RIGHT)
    for x, y in corners:
        assert min_x <= x <= max_x and min_y <= y <= max_y, 'corner outside alpha'
        assert max(alpha_at(rgba, stride, xx, yy)
                   for yy in range(max(min_y, y-2), min(max_y, y+2)+1)
                   for xx in range(max(min_x, x-2), min(max_x, x+2)+1)) >= ALPHA, \
            'corner has no visible pixel'

    mid_left = ((FRONT_LEFT[0] + BACK_LEFT[0]) * .5,
                (FRONT_LEFT[1] + BACK_LEFT[1]) * .5)
    mid_right = ((FRONT_RIGHT[0] + BACK_RIGHT[0]) * .5,
                 (FRONT_RIGHT[1] + BACK_RIGHT[1]) * .5)
    support_center_x = (mid_left[0] + mid_right[0]) * .5
    support_center_y = (mid_left[1] + mid_right[1]) * .5
    bbox_center_x = (min_x + max_x) * .5
    bbox_center_y = (min_y + max_y) * .5

    scale = STACK_HEIGHT / (max_y - min_y)
    support_width = (mid_right[0] - mid_left[0]) * scale
    tilt = -math.degrees(math.atan2(mid_right[1] - mid_left[1],
                                    mid_right[0] - mid_left[0]))

    return {
        'schema': 1,
        'source': str(path.relative_to(ROOT)).replace('\\', '/'),
        'sourceSha256': hashlib.sha256(path.read_bytes()).hexdigest(),
        'sourceBytes': path.stat().st_size,
        'texture': {'width': w, 'height': h, 'format': 'RGBA8', 'alphaThreshold': ALPHA},
        'alpha': {'bboxInclusive': [min_x, min_y, max_x, max_y]},
        'topCover': {
            'frontLeft': list(FRONT_LEFT),
            'backLeft': list(BACK_LEFT),
            'backRight': list(BACK_RIGHT),
            'frontRight': list(FRONT_RIGHT),
            'midDepthLeft': [round(mid_left[0], 3), round(mid_left[1], 3)],
            'midDepthRight': [round(mid_right[0], 3), round(mid_right[1], 3)],
        },
        'mapping': {
            'metresPerTexturePixel': round(scale, 9),
            'stackHeight': STACK_HEIGHT,
            'supportWidth': round(support_width, 6),
            'supportTiltDegrees': round(tilt, 6),
            'contactOffsetX': round((support_center_x - bbox_center_x) * scale, 6),
            'supportHeightAboveCentre': round((bbox_center_y - support_center_y) * scale, 6),
            'physicalDepth': PHYSICAL_DEPTH,
            'visibleWidth': round((max_x - min_x) * scale, 6),
            'visibleHeight': round((max_y - min_y) * scale, 6),
            'quadBoundsLocal': {
                'left': round((min_x - bbox_center_x) * scale, 6),
                'right': round((max_x - bbox_center_x) * scale, 6),
                'bottom': round((bbox_center_y - max_y) * scale, 6),
                'top': round((bbox_center_y - min_y) * scale, 6),
            },
            'uvBounds': {
                'left': round(min_x / w, 9),
                'right': round((max_x + 1) / w, 9),
                'bottom': round(1 - (max_y + 1) / h, 9),
                'top': round(1 - min_y / h, 9),
            },
        },
    }


result = measure(SOURCE)
OUTPUT.parent.mkdir(parents=True, exist_ok=True)
OUTPUT.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
m = result['mapping']
print('BOOKS_PROFILE texture={}x{} bbox={} supportWidth={:.6f} tilt={:.6f} offsetX={:.6f} supportY={:.6f} depth={:.6f}'.format(
    result['texture']['width'], result['texture']['height'], result['alpha']['bboxInclusive'],
    m['supportWidth'], m['supportTiltDegrees'], m['contactOffsetX'],
    m['supportHeightAboveCentre'], m['physicalDepth']))
print('WROTE ' + str(OUTPUT))
