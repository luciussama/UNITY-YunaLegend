#!/usr/bin/env python3
"""Audit a T03.2 run capture and assemble unretouched GIFs, checking the clip loop closure.

Reads the raw Unity frames and the run-proof JSON written by
YunaRunReviewCapture, verifies the support/continuity numbers, measures whether the
looping GIF restarts on the same pose phase and ground pattern, and writes the GIFs.

Example:
    python tools/make_run_review_v2.py --frames Build/25d/RunV2_60 \
        --proof Evidence/25d/T03.2/review-v2/run-proof.json \
        --out Evidence/25d/T03.2/review-v2
"""
import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
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


def character_screen_x(paths):
    positions = []
    for path in paths:
        pixels = np.asarray(Image.open(path).convert("RGB"), dtype=np.int16)
        red, green, blue = pixels[:, :, 0], pixels[:, :, 1], pixels[:, :, 2]
        mask = (red > 90) & (red > blue + 30) & (green < red + 8)
        columns = np.nonzero(mask)[1]
        positions.append(float(columns.mean()) if len(columns) >= 200 else float("nan"))
    return np.array(positions)


def gif_duration(path):
    with Image.open(path) as gif:
        total = 0
        for index in range(gif.n_frames):
            gif.seek(index)
            total += gif.info["duration"]
        return {"frames": gif.n_frames, "duration_ms": total, "size": list(gif.size)}


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--frames", required=True)
    parser.add_argument("--proof", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--no-gifs", action="store_true", help="audit numbers only; do not write GIFs")
    arguments = parser.parse_args()

    frames = Path(arguments.frames)
    report = json.loads(Path(arguments.proof).read_text(encoding="utf-8"))
    output = Path(arguments.out)
    output.mkdir(parents=True, exist_ok=True)
    samples = report["samples"]

    groups = stance_groups(samples)
    drift = max(max(s["footX"] for s in group) - min(s["footX"] for s in group) for group in groups)
    vertical = max(abs(report["minContactY"]), abs(report["maxContactY"]))
    trajectory_error = max(abs(s["x"] - samples[0]["x"] - report["runSpeed"] * i / 30) for i, s in enumerate(samples))
    steps = [(b["x"] + b["bodyShift"]) - (a["x"] + a["bodyShift"]) for a, b in zip(samples, samples[1:])]
    frozen = sum(1 for step in steps if abs(step) < .01)

    verification = {
        "scope": "Isolated flat-ground authoring fixture; sole marker is a measured point in the painted sprite, "
                 "not a full platform/contact or device certification.",
        "clip": {
            "samples": report["frames"],
            "simulation_hz": report["simulationHz"],
            "travel_m": report["travel"],
            "cycles_per_clip": report["cyclesPerClip"],
            "loop_phase_residual_cycles": report["loopPhaseResidual"],
            "loop_phase_residual_poses": report["loopPhaseResidual"] * report["poseCount"],
            "mark_spacing_m": report["markSpacing"],
            "grid_residual_m": report["clipTravel"] % report["markSpacing"],
        },
        "support": {
            "contact_drift_m": drift,
            "contact_drift_px_at_720": drift * 96,
            "vertical_error_m": vertical,
            "physical_linear_trajectory_error_m": trajectory_error,
            "max_visual_horizontal_offset_m": report["maxBodyShift"],
        },
        "continuity": {
            "body_step_mean_m": float(np.mean(steps)),
            "body_step_min_m": float(np.min(steps)),
            "body_step_max_m": float(np.max(steps)),
            "body_step_sd_m": float(np.std(steps)),
            "frames_with_body_frozen": frozen,
            "frames": len(steps),
        },
        "gifs": {},
    }

    for group, name in (("game", "run-gameplay.gif"), ("detail", "run-detail.gif")):
        files = sorted((frames / group).glob("*.png"))
        if len(files) != report["frames"]:
            raise SystemExit(f"expected {report['frames']} frames in {group}, found {len(files)}")
        images = [Image.open(path).convert("RGB") for path in files]
        palette = images[0].quantize(colors=256, method=Image.Quantize.MEDIANCUT)
        paletted = [image.quantize(palette=palette, dither=Image.Dither.NONE) for image in images]
        durations = [(round((i + 1) * 100 / 30) - round(i * 100 / 30)) * 10 for i in range(len(images))]
        if arguments.no_gifs:
            paletted = images  # keep the loop measurement below, skip writing
        else:
            paletted[0].save(output / name, save_all=True, append_images=paletted[1:], duration=durations,
                             loop=0, optimize=False, disposal=2)
            verification["gifs"][name] = gif_duration(output / name)
        if group == "detail" and not arguments.no_gifs:
            paletted[0].save(output / "run-detail-slow.gif", save_all=True, append_images=paletted[1:],
                             duration=[duration * 2 for duration in durations], loop=0, optimize=False, disposal=2)
            verification["gifs"]["run-detail-slow.gif"] = gif_duration(output / "run-detail-slow.gif")
        if group == "game":
            screen = character_screen_x(files)
            consecutive = np.abs(np.diff(screen))
            raw = [np.asarray(image, dtype=np.int16) for image in images]
            pixel_steps = [float(np.abs(b - a).mean()) for a, b in zip(raw, raw[1:])]
            verification["loop"] = {
                "screen_x_loop_jump_px": float(abs(screen[0] - screen[-1])),
                "screen_x_step_median_px": float(np.median(consecutive)),
                "pixel_diff_loop": float(np.abs(raw[0] - raw[-1]).mean()),
                "pixel_diff_median": float(np.median(pixel_steps)),
                "closes_like_a_normal_frame": bool(abs(screen[0] - screen[-1]) <= max(2.0, np.median(consecutive) * 1.5)),
            }

    verification["source_frame_sha256"] = {
        f"{group}/{path.name}": hashlib.sha256(path.read_bytes()).hexdigest()
        for group in ("game", "detail") for path in sorted((frames / group).glob("*.png"))
    }
    (output / "verification.json").write_text(json.dumps(verification, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({key: verification[key] for key in ("clip", "support", "continuity", "loop", "gifs")}, indent=2))


if __name__ == "__main__":
    main()
