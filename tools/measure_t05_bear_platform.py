#!/usr/bin/env python3
# Measure bear-v1.png for the T05.1 physical support contract.
import hashlib, json, struct, zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'Evidence/25d/T01/bear-v1.png'
OUTPUT = ROOT / 'Assets/Art/Illustrated25D/Environment/Bear/Resources/Bear_Platform_Profile.json'
ALPHA = 32
STEP = 4
CROWN_HEIGHT = 1.10


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


def measure(path):
    w, h, rgba = read_png(path)
    top, stride = [h] * w, w * 4
    min_x, min_y, max_x, max_y = w, h, -1, -1
    for y in range(h):
        for x in range(w):
            if rgba[y*stride+x*4+3] < ALPHA:
                continue
            min_x, max_x = min(min_x, x), max(max_x, x)
            min_y, max_y = min(min_y, y), max(max_y, y)
            top[x] = min(top[x], y)
    left = round(min_x + (max_x-min_x)*.4)
    right = round(min_x + (max_x-min_x)*.6)
    crown_y = min(top[left:right+1])
    crown_xs = [x for x in range(left, right+1) if top[x] <= crown_y+1]
    crown_x = sum(crown_xs) / len(crown_xs)
    scale = CROWN_HEIGHT / (max_y-crown_y)
    profile = [{'textureX': x, 'textureY': top[x],
                'localX': round((x-crown_x)*scale, 6),
                'localY': round((max_y-top[x])*scale-CROWN_HEIGHT*.5, 6)}
               for x in range(min_x, max_x+1, STEP)]
    return {'schema': 1, 'source': str(path.relative_to(ROOT)).replace('\\', '/'),
            'sourceSha256': hashlib.sha256(path.read_bytes()).hexdigest(),
            'sourceBytes': path.stat().st_size,
            'texture': {'width': w, 'height': h, 'format': 'RGBA8', 'alphaThreshold': ALPHA},
            'alpha': {'bboxInclusive': [min_x, min_y, max_x, max_y]},
            'mapping': {'crownHeightAboveFloor': CROWN_HEIGHT, 'bottomTextureY': max_y,
                        'crownTextureY': crown_y, 'centralSearchX': [left, right],
                        'crownTextureX': round(crown_x, 6), 'metresPerTexturePixel': round(scale, 9),
                        'contactOffsetX': round((crown_x-(min_x+max_x)*.5)*scale, 6),
                        'visibleWidth': round((max_x-min_x)*scale, 6),
                        'visibleHeight': round((max_y-min_y)*scale, 6),
                        'earTopAboveFloor': round((max_y-min_y)*scale, 6),
                        'crownAboveEarTop': round((crown_y-min_y)*scale, 6),
                        'quadBoundsLocal': {'left': round((min_x-crown_x)*scale, 6),
                                            'right': round((max_x-crown_x)*scale, 6),
                                            'bottom': round(-CROWN_HEIGHT*.5, 6),
                                            'top': round((max_y-min_y)*scale-CROWN_HEIGHT*.5, 6)},
                        'uvBounds': {'left': round(min_x/w, 9), 'right': round((max_x+1)/w, 9),
                                     'bottom': round(1-(max_y+1)/h, 9), 'top': round(1-min_y/h, 9)}},
            'topProfile': profile}


result = measure(SOURCE)
OUTPUT.parent.mkdir(parents=True, exist_ok=True)
OUTPUT.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
m = result['mapping']
print('BEAR_PROFILE texture={}x{} bbox={} crown=({},{}) scale={:.9f} width={:.6f} offsetX={:.6f} samples={}'.format(
    result['texture']['width'], result['texture']['height'], result['alpha']['bboxInclusive'],
    m['crownTextureX'], m['crownTextureY'], m['metresPerTexturePixel'], m['visibleWidth'],
    m['contactOffsetX'], len(result['topProfile'])))
print('WROTE ' + str(OUTPUT))
