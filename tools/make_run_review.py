"""Audit T03.2 captures and assemble unretouched Unity preview GIFs."""
import hashlib
import json
from pathlib import Path
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image


def stance_groups(samples):
    groups, group = [], []
    for sample in samples:
        if sample["contact"]:
            if group and sample["pose"] // 4 != group[-1]["pose"] // 4:
                groups.append(group)
                group = []
            group.append(sample)
        elif group:
            groups.append(group)
            group = []
    if group:
        groups.append(group)
    return groups


def main():
    evidence = ROOT / "Evidence/25d/T03.2"
    output = evidence / "review-v1"
    output.mkdir(exist_ok=True)
    verification = {"scope": "Isolated flat-ground authoring fixture; sole marker is a measured point in the painted sprite, not a full platform/contact or device certification.", "fps": {}, "gifs": {}}
    for hz in [30, 60, 120]:
        source = evidence / f"final{hz}/run-proof.json"
        report = json.loads(source.read_text(encoding="utf-8"))
        groups = stance_groups(report["samples"])
        drift = max(max(s["footX"] for s in group) - min(s["footX"] for s in group) for group in groups)
        vertical = max(abs(report["minContactY"]), abs(report["maxContactY"]))
        samples = report["samples"]
        trajectory_error = max(abs(s["x"] - samples[0]["x"] - report["runSpeed"] * i / 30) for i, s in enumerate(samples))
        assert report["success"] and report["frames"] == 120 and report["poseCount"] == 8
        assert drift <= 2 / 96 and vertical <= 1 / 96
        assert report["maxBodyShift"] <= .35 and trajectory_error < .001
        assert report["gameplayBefore"] == report["gameplayAfter"] and report["controllerBefore"] == report["controllerAfter"]
        assert report["colliderUnchanged"] and report["rootScaleUnchanged"]
        verification["fps"][hz] = {"contact_drift_m": drift, "contact_drift_px_at_720": drift * 96,
            "vertical_error_m": vertical, "physical_linear_trajectory_error_m": trajectory_error,
            "max_visual_horizontal_offset_m": report["maxBodyShift"], "travel_m": report["travel"],
            "atlas_editor_mib": report["atlasMiB"], "poses": report["poseCount"], "passed": True}
        shutil.copy2(source, output / f"run-proof-{hz}.json")
    # The serialized scene delivered with the asset is from the last (120 Hz) execution.
    for path in (evidence / "final120").glob("*.png"):
        shutil.copy2(path, output / path.name)
    for group, name in [("game", "run-gameplay.gif"), ("detail", "run-detail.gif")]:
        files = sorted((ROOT / f"Build/25d/RunFinal120/{group}").glob("*.png"))
        assert len(files) == 120
        images = [Image.open(path).convert("RGB") for path in files]
        palette = images[0].quantize(colors=256, method=Image.Quantize.MEDIANCUT)
        frames = [im.quantize(palette=palette, dither=Image.Dither.NONE) for im in images]
        durations = [(round((i + 1) * 100 / 30) - round(i * 100 / 30)) * 10 for i in range(120)]
        frames[0].save(output / name, save_all=True, append_images=frames[1:], duration=durations, loop=0, optimize=False, disposal=2)
        if group == "detail":
            frames[0].save(output / "run-detail-slow.gif", save_all=True, append_images=frames[1:], duration=[d * 2 for d in durations], loop=0, optimize=False, disposal=2)
        with Image.open(output / name) as gif:
            total = 0
            for index in range(gif.n_frames):
                gif.seek(index)
                total += gif.info["duration"]
            assert total == 4000
            verification["gifs"][name] = {"frames": gif.n_frames, "duration_ms": total, "size": list(gif.size),
                "method": "raw renders, no crop/resize/retouch/interpolation; fixed GIF palette only"}
    verification["source_frame_sha256"] = {
        f"{group}/{p.name}": hashlib.sha256(p.read_bytes()).hexdigest()
        for group in ["game", "detail"] for p in sorted((ROOT / f"Build/25d/RunFinal120/{group}").glob("*.png"))}
    (output / "verification.json").write_text(json.dumps(verification, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"fps": verification["fps"], "gifs": verification["gifs"]}))


if __name__ == "__main__":
    main()
