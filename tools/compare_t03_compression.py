"""Compare T03 baseline and compressed captures frame by frame."""
from pathlib import Path
import json
import math
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image, ImageChops


def channel_mse(hist, pixels):
    return sum(i * i * count for i, count in enumerate(hist)) / pixels


def compare(baseline, candidate, group, frame, output):
    left = Image.open(baseline / group / frame).convert("RGBA")
    right = Image.open(candidate / group / frame).convert("RGBA")
    if left.size != right.size:
        raise ValueError(f"Size mismatch {group}/{frame}: {left.size} != {right.size}")
    width, height = left.size
    pixels = width * height
    diff = ImageChops.difference(left, right)
    red, green, blue, alpha = diff.split()
    mse = (channel_mse(red.histogram(), pixels)
           + channel_mse(green.histogram(), pixels)
           + channel_mse(blue.histogram(), pixels)) / 3.0
    psnr = 99.0 if mse == 0 else 10.0 * math.log10((255.0 * 255.0) / mse)
    mask = ImageChops.lighter(ImageChops.lighter(red, green), blue)
    mask_hist = mask.histogram()
    changed = pixels - mask_hist[0]
    alpha_hist = alpha.histogram()
    alpha_changed = pixels - alpha_hist[0]
    max_diff = 0
    for channel in (red, green, blue):
        hist = channel.histogram()
        for value in range(255, -1, -1):
            if hist[value]:
                max_diff = max(max_diff, value)
                break
    output.append({
        "group": group,
        "frame": frame,
        "psnr": psnr,
        "max_channel_diff": max_diff,
        "changed_fraction": changed / pixels,
        "alpha_changed_fraction": alpha_changed / pixels,
    })


def main():
    baseline = ROOT / "Build/25d/T03JointBaselineV4-30"
    candidate = ROOT / "Build/25d/T03JointFinalV4-30"
    records = []
    for group, count in (("game", 420), ("detail", 420)):
        for index in range(count):
            compare(baseline, candidate, group, f"{index:03d}.png", records)
    out = ROOT / "Build/25d/T03CompressionCompareV4.json"
    out.write_text(json.dumps(records, indent=2), encoding="utf-8")
    summary = {}
    for group in ("game", "detail"):
        rows = [r for r in records if r["group"] == group]
        summary[group] = {
            "frames": len(rows),
            "psnr_min": min(r["psnr"] for r in rows),
            "psnr_mean": sum(r["psnr"] for r in rows) / len(rows),
            "max_channel_diff": max(r["max_channel_diff"] for r in rows),
            "max_changed_fraction": max(r["changed_fraction"] for r in rows),
            "max_alpha_changed_fraction": max(r["alpha_changed_fraction"] for r in rows),
        }
    summary_path = ROOT / "Build/25d/T03CompressionSummaryV4.json"
    summary_path.write_text(json.dumps(summary, indent=2), encoding="utf-8")
    print(json.dumps(summary, indent=2))
    print(f"wrote {out.relative_to(ROOT)} and {summary_path.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
