"""Build the T04.1 Nanda hover loop from the approved T01 painting.

The approved PNG is never modified. A feathered mask around each pair of wings
drives a vertical displacement warp of the whole painted image in that region.
Wing, glow and edge move together as one continuous surface, so there is no
layer seam. The body, face and hair stay at zero displacement.
"""
import hashlib
import json
import math
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageMath

SOURCE = ROOT / "Evidence/25d/T01/nanda-blue-v1.png"
SOURCE_SHA = "f16ba3862f7e1fe263916922fa742eef1426b98d6e4d81f794e1083e74725678"
FRAMES = ROOT / "Evidence/25d/T04.1/frames"
INSPECTION = ROOT / "Evidence/25d/T04.1/inspection"
ATLAS = ROOT / "Assets/Art/Illustrated25D/Nanda/Flight/Nanda_Flight_Atlas_v1.png"
FRAME_COUNT = 8
FRAME_SIZE = (512, 592)
ATLAS_SIZE = (2048, 1184)
PIVOT = (0.5, 0.42061)
PPU = 880.0
WING_AMPLITUDE = 80.0

# Polygons are in the approved 1168x1346 source, top-left origin.
RIGHT_WINGS = [
    [(640, 555), (650, 470), (700, 380), (780, 295), (870, 230), (975, 195), (1045, 205), (1040, 255), (990, 315), (915, 385), (840, 450), (770, 505), (705, 550)],
    [(655, 550), (710, 520), (790, 515), (880, 540), (960, 590), (965, 620), (910, 650), (830, 655), (750, 635), (690, 600)],
]
LEFT_WINGS = [
    [(335, 555), (310, 500), (260, 450), (200, 395), (140, 355), (100, 345), (95, 390), (120, 450), (170, 505), (230, 545), (290, 565)],
    [(330, 575), (300, 560), (255, 565), (230, 590), (245, 615), (290, 625), (330, 620)],
]
BODY = [
    # left arm and hand, which cross the left wing
    [(320, 540), (360, 560), (340, 610), (300, 650), (250, 680), (190, 700), (160, 680), (175, 640), (220, 610), (270, 580)],
    # right arm and hand, below the right wing
    [(700, 600), (780, 650), (880, 700), (900, 730), (860, 750), (800, 720), (730, 670), (690, 630)],
    # left hair curl in front of the upper wing
    [(280, 350), (360, 360), (380, 430), (350, 500), (300, 480), (270, 420)],
    # right shoulder and dress root
    [(620, 470), (685, 480), (705, 540), (685, 590), (640, 600), (610, 530)],
    # left shoulder and dress root
    [(305, 480), (365, 490), (385, 545), (365, 595), (320, 610), (285, 540)],
]


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def polygon_mask(size, polygon, expand=0):
    image = Image.new("L", size, 0)
    draw = ImageDraw.Draw(image)
    draw.polygon(polygon, fill=255)
    if expand:
        draw.line(polygon + [polygon[0]], fill=255, width=expand * 2, joint="curve")
    return image


def union(masks, size):
    result = Image.new("L", size, 0)
    for mask in masks:
        result = ImageChops.lighter(result, mask)
    return result


def radial_ramp(size, anchor, root_radius, max_distance):
    """Zero at the wing root, one at the tip, with a smooth shoulder."""
    image = Image.new("L", size, 0)
    pixels = image.load()
    ax, ay = anchor
    for y in range(size[1]):
        for x in range(size[0]):
            distance = math.hypot(x - ax, y - ay)
            value = max(0.0, min(1.0, (distance - root_radius) / float(max_distance - root_radius)))
            pixels[x, y] = round(255 * value * value * (3 - 2 * value))
    return image


def premultiplied(source):
    r, g, b, a = source.split()
    return Image.merge("RGBA", (
        ImageMath.unsafe_eval("convert(r*a/255,'L')", r=r, a=a),
        ImageMath.unsafe_eval("convert(g*a/255,'L')", g=g, a=a),
        ImageMath.unsafe_eval("convert(b*a/255,'L')", b=b, a=a),
        a,
    ))


def warped(source, packed, weight, amplitude):
    """Shift each pixel vertically by amplitude * feathered weight."""
    if abs(amplitude) < 1e-6:
        return source.copy()
    output = source.copy()
    box = weight.getbbox()
    if not box:
        return output
    x0, y0, x1, y1 = box
    x0, y0 = max(0, x0 - 2), max(0, y0 - 2)
    x1, y1 = min(source.width, x1 + 2), min(source.height, y1 + 2)
    packed_pixels = packed.load()
    weight_pixels = weight.load()
    output_pixels = output.load()
    height = source.height
    for y in range(y0, y1):
        for x in range(x0, x1):
            feather = weight_pixels[x, y]
            if feather == 0:
                continue
            sy = y + amplitude * (feather / 255.0)
            low = int(math.floor(sy))
            fraction = sy - low
            low = max(0, min(height - 1, low))
            high = max(0, min(height - 1, low + 1))
            p0 = packed_pixels[x, low]
            p1 = packed_pixels[x, high]
            a = p0[3] + (p1[3] - p0[3]) * fraction
            if a <= 0:
                output_pixels[x, y] = (0, 0, 0, 0)
                continue
            r = (p0[0] + (p1[0] - p0[0]) * fraction) * 255.0 / a
            g = (p0[1] + (p1[1] - p0[1]) * fraction) * 255.0 / a
            b = (p0[2] + (p1[2] - p0[2]) * fraction) * 255.0 / a
            output_pixels[x, y] = (
                max(0, min(255, round(r))),
                max(0, min(255, round(g))),
                max(0, min(255, round(b))),
                max(0, min(255, round(a))),
            )
    return output


def main() -> None:
    if sha(SOURCE) != SOURCE_SHA:
        raise SystemExit("Approved Nanda source hash changed")
    FRAMES.mkdir(parents=True, exist_ok=True)
    INSPECTION.mkdir(parents=True, exist_ok=True)
    ATLAS.parent.mkdir(parents=True, exist_ok=True)

    with Image.open(SOURCE) as opened:
        source = opened.convert("RGBA")
    if source.size != (1168, 1346):
        raise SystemExit(f"Unexpected source size: {source.size}")

    # The body is the real painted silhouette minus the wing cores. Protection
    # polygons add the arm, shoulder and hair pixels that overlap the wings.
    character = source.getchannel("A").point(lambda value: 255 if value > 40 else 0)
    wing_core = union(
        [polygon_mask(source.size, polygon, expand=50) for polygon in RIGHT_WINGS + LEFT_WINGS],
        source.size,
    )
    body_protection = union([polygon_mask(source.size, polygon) for polygon in BODY], source.size)
    body_union = ImageChops.lighter(ImageChops.subtract(character, wing_core), body_protection)
    # A rigid vertical lift of the complete wing fan (edge and halo included).
    # The mask is cut at the body, which hides the root seam behind the dress;
    # the outer cut falls in transparent background, far from the painted glow.
    right_margin = ImageChops.subtract(
        union([polygon_mask(source.size, polygon, expand=100) for polygon in RIGHT_WINGS], source.size),
        body_union,
    )
    left_margin = ImageChops.subtract(
        union([polygon_mask(source.size, polygon, expand=100) for polygon in LEFT_WINGS], source.size),
        body_union,
    )
    right_weight = ImageChops.multiply(
        ImageChops.multiply(right_margin, character),
        radial_ramp(source.size, (672, 525), 40, 500),
    ).filter(ImageFilter.GaussianBlur(10))
    left_weight = ImageChops.multiply(
        ImageChops.multiply(left_margin, character),
        radial_ramp(source.size, (322, 545), 40, 310),
    ).filter(ImageFilter.GaussianBlur(10))
    weight = ImageChops.lighter(right_weight, left_weight)
    packed = premultiplied(source)

    debug = Image.new("RGBA", source.size, (235, 235, 235, 255))
    debug.alpha_composite(source)
    body_tint = Image.new("RGBA", source.size, (220, 40, 40, 255))
    body_tint.putalpha(body_union.point(lambda value: value * 45 // 255))
    wing_tint = Image.new("RGBA", source.size, (40, 110, 255, 255))
    wing_tint.putalpha(weight.point(lambda value: value * 45 // 255))
    debug.alpha_composite(body_tint)
    debug.alpha_composite(wing_tint)
    debug.convert("RGB").save(INSPECTION / "masks.png")

    neutral = warped(source, packed, weight, 0.0)
    neutral_diff = 0
    src = source.load()
    out = neutral.load()
    for y in range(source.height):
        for x in range(source.width):
            if src[x, y][3] != out[x, y][3] or (src[x, y][3] and src[x, y] != out[x, y]):
                neutral_diff += 1
    if neutral_diff:
        raise SystemExit(f"Neutral warp changed {neutral_diff} visible pixels")

    sheet = Image.new("RGBA", (FRAME_SIZE[0] * 4, FRAME_SIZE[1] * 2), (0, 0, 0, 0))
    hashes = []
    frame_boxes = []
    signals = []
    for index in range(FRAME_COUNT):
        theta = 2 * math.pi * (index + 0.5) / FRAME_COUNT
        # The second harmonic makes the up/down flap asymmetric and keeps all
        # eight sampled poses distinct.
        signal = math.sin(theta) + 0.3 * math.sin(2 * theta)
        signals.append(signal)
        frame = warped(source, packed, right_weight, WING_AMPLITUDE * signal)
        frame = warped(frame, packed, left_weight, WING_AMPLITUDE * signal)
        frame.save(FRAMES / f"nanda-hover-{index:02d}.png", "PNG", optimize=False)
        hashes.append(sha(FRAMES / f"nanda-hover-{index:02d}.png"))
        frame_boxes.append(frame.getbbox())
        scaled = frame.resize((512, 590), Image.Resampling.LANCZOS)
        cell = Image.new("RGBA", FRAME_SIZE, (0, 0, 0, 0))
        cell.paste(scaled, (0, 1))
        sheet.alpha_composite(cell, ((index % 4) * FRAME_SIZE[0], (index // 4) * FRAME_SIZE[1]))

    if sheet.size != ATLAS_SIZE:
        raise SystemExit(f"Unexpected atlas size: {sheet.size}")
    sheet.save(ATLAS, "PNG", optimize=False)
    with Image.open(ATLAS) as checked:
        if checked.size != ATLAS_SIZE or checked.mode != "RGBA":
            raise SystemExit("Atlas verification failed")

    distances = []
    for index in range(FRAME_COUNT):
        a = Image.open(FRAMES / f"nanda-hover-{index:02d}.png").convert("RGBA")
        b = Image.open(FRAMES / f"nanda-hover-{(index + 1) % FRAME_COUNT:02d}.png").convert("RGBA")
        pa, pb = a.load(), b.load()
        changed = 0
        for y in range(0, a.height, 3):
            for x in range(0, a.width, 3):
                if pa[x, y] != pb[x, y]:
                    changed += 1
        distances.append(changed)
        if changed < 200:
            raise SystemExit(f"Frames {index} and {index + 1} are too similar: {changed}")

    manifest = {
        "schema_version": 1,
        "microdelivery": "T04.1",
        "tool": "deterministic feathered vertical warp over the approved T01 painting",
        "source": {"path": "Evidence/25d/T01/nanda-blue-v1.png", "sha256": SOURCE_SHA, "size": [1168, 1346]},
        "neutral_warp_diff_pixels": neutral_diff,
        "frame_count": FRAME_COUNT,
        "evidence_frame_size": list(source.size),
        "atlas_cell_size": list(FRAME_SIZE),
        "atlas": {"path": "Assets/Art/Illustrated25D/Nanda/Flight/Nanda_Flight_Atlas_v1.png", "size": list(ATLAS_SIZE), "sha256": sha(ATLAS)},
        "sprite": {"pivot": list(PIVOT), "pixels_per_unit": PPU, "body_height_metres": 0.6202, "canvas_height_metres": round(FRAME_SIZE[1] / PPU, 4)},
        "warp": {"amplitude_pixels": WING_AMPLITUDE, "core_expand_pixels": 50, "margin_expand_pixels": 100, "right_root": [672, 525], "left_root": [322, 545], "right_polygons": RIGHT_WINGS, "left_polygons": LEFT_WINGS, "body_protection": BODY},
        "signals": signals,
        "frames": [{"index": index, "path": f"Evidence/25d/T04.1/frames/nanda-hover-{index:02d}.png", "sha256": hashes[index], "bbox": frame_boxes[index], "sampled_changed_pixels_to_next": distances[index]} for index in range(FRAME_COUNT)],
    }
    (ROOT / "Evidence/25d/T04.1/frames-manifest.json").write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    contact = sheet.copy()
    background = Image.new("RGBA", contact.size, (22, 24, 32, 255))
    background.alpha_composite(contact)
    background.convert("RGB").save(INSPECTION / "atlas-contact.png")
    print(json.dumps({"atlas_sha256": manifest["atlas"]["sha256"], "neutral_diff": neutral_diff, "frame_changes": distances}, ensure_ascii=False))


if __name__ == "__main__":
    main()
