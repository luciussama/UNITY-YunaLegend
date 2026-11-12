"""Diagnose the delivered T03.2 v1 clip without changing animation or images."""
import hashlib
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image, ImageChops, ImageStat


def main():
    evidence = ROOT / "Evidence/25d/T03.2"
    source = evidence / "review-v1/run-proof-120.json"
    report = json.loads(source.read_text(encoding="utf-8"))
    samples = report["samples"]
    transitions = []
    for before, after in zip(samples, samples[1:]):
        physical = after["x"] - before["x"]
        offset = after["bodyShift"] - before["bodyShift"]
        transitions.append({"from": before["sample"], "to": after["sample"],
            "time_to_s": after["sample"] / 30, "pose_from": before["pose"], "pose_to": after["pose"],
            "offset_from_m": before["bodyShift"], "offset_to_m": after["bodyShift"],
            "offset_step_m": offset, "offset_step_px_at_720": offset * 96,
            "physical_step_m": physical, "visual_pivot_step_m": physical + offset,
            "visual_pivot_speed_m_s": (physical + offset) * 30})
    biggest = max(abs(item["offset_step_m"]) for item in transitions)
    releases = [item for item in transitions if item["pose_from"] % 4 == 2 and item["pose_to"] % 4 == 3]
    last_release = releases[-1]
    assert biggest > .2 and last_release["from"] == 115 and last_release["to"] == 116
    with Image.open(evidence / "review-v1/run-detail.gif") as gif:
        durations = []
        for index in range(gif.n_frames):
            gif.seek(index)
            durations.append(gif.info["duration"])
    phase_distance = samples[-1]["x"] + report["runSpeed"] / 30 - samples[0]["x"]
    source_code = ROOT / "Assets/Art/Illustrated25D/Yuna/Run/YunaRunIllustration.cs"
    reset_line = next(i for i, line in enumerate(source_code.read_text().splitlines(), 1) if "bodyShift = correction;" in line)
    result = {
        "version": "T03.2-v1-investigation", "status": "defect_confirmed_not_fixed",
        "method": "Read recorded controller/visual samples, delivered GIF timing and original raw camera renders. No artwork, runtime code or physics mutation.",
        "cause": "correction starts at zero; flight clears the anchor and bodyShift is assigned correction without a release transition. Foot anchoring checks did not check body-position continuity.",
        "source": {"path": source_code.relative_to(ROOT).as_posix(), "reset_line": reset_line,
            "sha256": hashlib.sha256(source_code.read_bytes()).hexdigest()},
        "last_complete_step_release": last_release,
        "release_events": releases,
        "preceding_visual_pivot_step_m": transitions[last_release["from"] - 1]["visual_pivot_step_m"],
        "largest_visual_offset_step_m": biggest,
        "physics_speed_m_s": report["runSpeed"],
        "gif": {"frames": len(durations), "duration_ms": sum(durations), "tail_durations_ms": durations[-8:],
            "pose_transition_at_wrap": [samples[-1]["pose"], samples[0]["pose"]],
            "offset_transition_at_wrap_m": samples[0]["bodyShift"] - samples[-1]["bodyShift"],
            "ground_marker_period_m": 1, "wrap_phase_error_modulo_m": phase_distance % 1,
            "shortest_equivalent_marker_phase_m": min(phase_distance % 1, 1 - phase_distance % 1)},
        "tail_samples": samples[-12:], "start_samples": samples[:4],
    }
    for group in ["game", "detail"]:
        files = sorted((ROOT / f"Build/25d/RunFinal120/{group}").glob("*.png"))
        if len(files) != 120:
            result[group + "_raw_images"] = "not available; numeric report and delivered GIF still available"
            continue
        images = [Image.open(path).convert("RGB") for path in files]
        delta = lambda a, b: sum(ImageStat.Stat(ImageChops.difference(a, b)).mean) / 3
        internal = [delta(a, b) for a, b in zip(images, images[1:])]
        result[group + "_raw_images"] = {"wrap_mean_rgb_difference": delta(images[-1], images[0]),
            "ordinary_step_min_max_mean_rgb_difference": [min(internal), max(internal)],
            "interpretation": "Whole-image averages do not measure perceived smoothness, but do not indicate an encoder-only large last-frame spike."}
    output = evidence / "investigation-tail-v1"
    output.mkdir(exist_ok=True)
    (output / "diagnosis.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"last_release": last_release, "gif_wrap": result["gif"], "source_line": reset_line}))


if __name__ == "__main__":
    main()
