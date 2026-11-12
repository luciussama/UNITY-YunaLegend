"""Assemble unretouched T03.1 Unity renders with one GIF palette and exact timing."""
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image


def main():
    output = ROOT / "Evidence/25d/T03.1"
    report = {"method": "Raw Unity renders, no crop, scaling or retouch; GIF palette quantization only. Fixed palette per clip, 30/40 ms cadence for exactly four seconds."}
    for group, name in [("game", "idle-gameplay.gif"), ("detail", "idle-detail.gif")]:
        files = sorted((ROOT / "Build/25d/IdleFrames" / group).glob("*.png"))
        if len(files) != 120:
            raise ValueError("Expected 120 frames: " + group)
        images = [Image.open(p).convert("RGB") for p in files]
        palette = images[0].quantize(colors=256, method=Image.Quantize.MEDIANCUT)
        frames = [im.quantize(palette=palette, dither=Image.Dither.NONE) for im in images]
        duration = [(round((i + 1) * 100 / 30) - round(i * 100 / 30)) * 10 for i in range(120)]
        frames[0].save(output / name, save_all=True, append_images=frames[1:],
                       duration=duration, loop=0, optimize=False, disposal=2)
        with Image.open(output / name) as gif:
            total = 0
            for i in range(gif.n_frames):
                gif.seek(i)
                total += gif.info["duration"]
            assert total == 4000
            report[group] = {"source_frames": 120, "gif_frames": gif.n_frames,
                             "duration_ms": total, "size": list(gif.size)}
        if group == "detail":
            # Both shoes/soles and their support under the stationary fixture.
            region = [(x, y) for y in range(540, 600) for x in range(260, 385)]
            base = images[0].load()
            maximum = 0
            for im in images[1:]:
                pixels = im.load()
                maximum = max(maximum, sum(pixels[x, y] != base[x, y] for x, y in region))
            report["stationary_foot_region"] = {"rectangle_xywh": [260, 540, 125, 60],
                "max_changed_pixels_from_first_frame": maximum,
                "scope": "Pixel stability in stationary fixture; not contact correctness on real platforms."}
            assert maximum == 0
    (output / "preview-verification.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report))


if __name__ == "__main__":
    main()
