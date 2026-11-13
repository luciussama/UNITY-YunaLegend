"""Build the T04.1 manifest and verify provenance of every proof and asset."""
from pathlib import Path
import hashlib
import json
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image

EVIDENCE = "Evidence/25d/T04.1"
NANDA = "Assets/Art/Illustrated25D/Nanda/Flight"
SOURCE = "Evidence/25d/T01/nanda-blue-v1.png"
SOURCE_SHA = "f16ba3862f7e1fe263916922fa742eef1426b98d6e4d81f794e1083e74725678"
ATLAS = NANDA + "/Nanda_Flight_Atlas_v1.png"


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def record(relative):
    path = ROOT / relative
    return {"path": relative, "sha256": sha(path), "bytes": path.stat().st_size}


def load_json(relative):
    return json.loads((ROOT / relative).read_text(encoding="utf-8"))


def summary(proof, path):
    keep = ["success", "simulationHz", "captureFps", "framesRun", "duration", "cycleSeconds",
            "allPosesSeen", "sawRight", "sawLeft", "flipHonored", "flipViolations",
            "poseCadenceExact", "poseMismatches", "poseChangesRecorded", "cyclesObserved",
            "residentMiB", "budgetMiB", "budgetPassed", "sourcePngsUnchanged",
            "sceneUntouched", "guideUntouched", "compressionMode", "flipDiagnostic", "texture"]
    out = {key: proof.get(key) for key in keep}
    out["path"] = path
    out["sha256"] = sha(ROOT / path)
    out["posesSeen"] = proof.get("posesSeen")
    return out


def main():
    head = subprocess.check_output(["git", "rev-parse", "--short", "HEAD"], cwd=ROOT, text=True).strip()
    frames_manifest = load_json(f"{EVIDENCE}/frames-manifest.json")
    proofs = {hz: load_json(f"{EVIDENCE}/final-{hz}/flight-proof.json") for hz in (30, 60, 120)}

    assert sha(ROOT / SOURCE) == SOURCE_SHA, "approved T01 source changed"
    assert sha(ROOT / ATLAS) == frames_manifest["atlas"]["sha256"], "atlas differs from frames manifest"
    assert frames_manifest["evidence_frame_size"] == [1168, 1346]
    assert frames_manifest["atlas_cell_size"] == [512, 592]

    with Image.open(ROOT / ATLAS) as atlas:
        assert atlas.size == (2048, 1184) and atlas.mode == "RGBA"
    for frame in frames_manifest["frames"]:
        path = ROOT / frame["path"]
        assert sha(path) == frame["sha256"], "evidence frame changed: " + frame["path"]
        with Image.open(path) as image:
            assert image.size == (1168, 1346), "evidence frame size: " + frame["path"]

    cadence = None
    for hz, proof in proofs.items():
        assert proof["success"] is True, hz
        assert proof["allPosesSeen"] is True and proof["posesSeen"] == list(range(8)), hz
        assert proof["sawRight"] is True and proof["sawLeft"] is True, hz
        assert proof["flipHonored"] is True and proof["flipViolations"] == 0, hz
        assert proof["poseCadenceExact"] is True and proof["poseMismatches"] == 0, hz
        assert proof["poseChangesRecorded"] == 48 and proof["framesRun"] == 72, hz
        assert proof["budgetPassed"] is True, hz
        assert proof["sourcePngsUnchanged"] is True and proof["sceneUntouched"] is True
        assert proof["guideUntouched"] is True
        assert proof["texture"]["width"] == 2048 and proof["texture"]["height"] == 1184
        assert proof["texture"]["graphicsFormat"] == "RGBA_DXT5_UNorm", hz
        assert "CompressedHQ" in proof["texture"]["importSettings"], hz
        sequence = [(round(change["t"], 6), change["pose"], change["direction"], change["flipX"])
                    for change in proof["poseChanges"]]
        if cadence is None:
            cadence = sequence
        else:
            assert sequence == cadence, "cadence differs at " + str(hz) + " Hz"
    assert cadence is not None and len(cadence) == 48

    sprite_assets = []
    for index in range(8):
        path = f"{NANDA}/Runtime/Frames/NandaFlight{index}.asset"
        assert (ROOT / path).exists(), path
        sprite_assets.append(record(path))

    code = [
        record(NANDA + "/NandaFlightIllustration.cs"),
        record(NANDA + "/Editor/NandaFlightReviewCapture.cs"),
        record(NANDA + "/NandaFlightReview.unity"),
        record(NANDA + "/NandaFlightReview.unity.meta"),
        record("tools/make_t04_nanda_frames.py"),
        record("tools/make_t04_nanda_review.py"),
    ]

    documents = [
        record(EVIDENCE + "/README.md"),
        record(EVIDENCE + "/CONTRATO.md"),
        record(EVIDENCE + "/reviews/T04.1-revisao.md"),
    ]

    review = []
    for name in ("game-normal.gif", "game-slow.gif", "detail-normal.gif", "detail-slow.gif",
                 "game-review-sheet.png", "detail-review-sheet.png", "pose-strip.png", "flip-compare.png"):
        review.append(record(f"{EVIDENCE}/review/{name}"))

    inspection = []
    for path in sorted((ROOT / EVIDENCE / "inspection").glob("*")):
        if path.is_file():
            inspection.append(record(path.relative_to(ROOT).as_posix()))

    # 7.5 m vertical span at 1280x720 is the production illustrated framing.
    pixels_per_metre = 720 / 7.5
    scale = {
        "game_camera_vertical_span_metres": 7.5,
        "game_capture": [1280, 720],
        "pixels_per_metre": pixels_per_metre,
        "nanda_canvas_height_metres": round(592 / 880.0, 4),
        "nanda_on_screen_pixels": round(592 / 880.0 * pixels_per_metre, 1),
        "yuna_idle_height_metres": 1.46,
        "yuna_on_screen_pixels": round(1.46 * pixels_per_metre, 1),
        "detail_camera_distance_metres": 1.2,
    }

    manifest = {
        "task": "T04.1",
        "title": "Nanda ilustrada em voo: asas, ciclo e orientacao",
        "status": "accepted",
        "session": "2026-11-13-codex-t04-nanda-25c1",
        "agent": "Codex (GPT-6.1)",
        "date": "2026-11-13",
        "unity": "6000.3.28f1",
        "head": head,
        "worktree": "T04.1 not committed",
        "source": {
            "path": SOURCE,
            "sha256": SOURCE_SHA,
            "size": frames_manifest["evidence_frame_size"],
            "unchanged": True,
        },
        "atlas": {
            "path": ATLAS,
            "sha256": frames_manifest["atlas"]["sha256"],
            "size": frames_manifest["atlas"]["size"],
            "cell_size": frames_manifest["atlas_cell_size"],
            "frames": 8,
            "pivot": frames_manifest["sprite"]["pivot"],
            "pixels_per_unit": frames_manifest["sprite"]["pixels_per_unit"],
        },
        "frames": [record(frame["path"]) for frame in frames_manifest["frames"]],
        "sprite_assets": sprite_assets,
        "code": code,
        "proofs": [summary(proofs[hz], f"{EVIDENCE}/final-{hz}/flight-proof.json") for hz in (30, 60, 120)],
        "cadence": {
            "pose_changes_per_run": 48,
            "identical_at_30_60_120": True,
            "first": cadence[0],
            "last": cadence[-1],
            "cycle_seconds": round(proofs[30]["cycleSeconds"], 3),
            "frames_per_cycle": 8,
        },
        "orientation": {
            "method": "rendered mirror probe on the default SpriteRenderer material plus per-sample flipX checks",
            "right_flipX": False,
            "left_flipX": True,
            "mirror": proofs[30]["flipDiagnostic"],
            "flip_violations": 0,
        },
        "budget": {
            "limit_mib": 16.0,
            "observed_mib": round(proofs[30]["residentMiB"], 4),
            "passed": True,
            "metric": "Profiler.GetRuntimeMemorySizeLong in the Windows Editor, one resident atlas",
            "observed_format": proofs[30]["texture"]["graphicsFormat"],
            "import": proofs[30]["texture"]["importSettings"],
        },
        "scale": scale,
        "documents": documents,
        "review": review,
        "inspection": inspection,
        "human_review": {
            "required": True,
            "status": "accepted",
            "accepted_at": "2026-11-13",
            "literal_response": "aprovado.",
            "record": EVIDENCE + "/reviews/T04.1-revisao.md",
            "scope": "T04.1 only; T04.2 and aggregate T04 remain open",
            "question": "A Nanda le como companheira viva e o ritmo das asas/hover convence? A orientacao direita/esquerda esta correta?",
        },
        "verification": {
            "final_runs": [30, 60, 120],
            "frames_per_camera": 72,
            "all_eight_poses_rendered": True,
            "both_directions_observed": True,
            "flip_honored_by_standard_sprite_path": True,
            "pose_cadence_identical_across_rates": True,
            "source_pngs_unchanged": True,
            "gameplay_scene_unchanged": True,
            "guide_script_unchanged": True,
            "static_yuna_is_scale_only": True,
            "component_owns_visual_only": True,
        },
        "next": "T04.1 aceita; iniciar T04.2 somente por pedido do usuario. T04 agregada nao aceita.",
    }
    output = ROOT / EVIDENCE / "manifest.json"
    output.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"wrote {output.relative_to(ROOT)} | head={head}")
    print(json.dumps({"budget": manifest["budget"], "scale": manifest["scale"],
                      "orientation": manifest["orientation"], "cadence": manifest["cadence"]}, indent=2))


if __name__ == "__main__":
    main()
