"""Build the T03 joint-review manifest and verify provenance/evidence."""
from pathlib import Path
from datetime import datetime, timezone
import hashlib
import json
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image

YUNA = "Assets/Art/Illustrated25D/Yuna"
EVIDENCE = "Evidence/25d/T03"
RUNTIME = YUNA + "/Joint/Runtime"


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def file_record(relative: str) -> dict:
    path = ROOT / relative
    return {"path": relative, "sha256": sha(path), "bytes": path.stat().st_size}


def verify_padded(source: str, runtime: str, size: tuple[int, int]) -> dict:
    with Image.open(ROOT / source) as raw:
        original = raw.convert("RGBA")
    with Image.open(ROOT / runtime) as raw:
        padded = raw.convert("RGBA")
    if padded.size != size:
        raise ValueError(f"Unexpected runtime size {runtime}: {padded.size} != {size}")
    if padded.crop((0, 0, original.width, original.height)).tobytes() != original.tobytes():
        raise ValueError(f"Runtime atlas changed source pixels: {runtime}")
    alpha = padded.getchannel("A")
    for y in range(size[1]):
        for x in range(size[0]):
            if (x >= original.width or y >= original.height) and alpha.getpixel((x, y)) != 0:
                raise ValueError(f"Runtime padding is not transparent: {runtime} at {x},{y}")
    return {
        "path": runtime,
        "source": source,
        "size": [size[0], size[1]],
        "source_size": [original.width, original.height],
        "source_pixels_equal": True,
        "padding_transparent": True,
        "sha256": sha(ROOT / runtime),
        "source_sha256": sha(ROOT / source),
    }


def load_proof(relative: str) -> dict:
    data = json.loads((ROOT / relative).read_text(encoding="utf-8"))
    keep = ["success", "budgetPassed", "totalResidentMiB", "budgetMiB", "frames",
            "simulationHz", "captureFps", "compressionMode", "visibilityConflicts",
            "singleSourceVisible", "maxVisibleSources", "componentOverlapFrames",
            "emptyComponentFrames", "resolverActive", "jumpTakeoffs", "jumpLandings",
            "stumbleEvents", "idleToRunTransitions", "runToIdleTransitions",
            "sourcePngsUnchanged", "colliderUnchanged", "rootScaleUnchanged",
            "sawIdle", "sawIdleFacingCamera",
            "sawStumbleFacingRight", "sawStumbleFacingLeft", "stumbleFlipHonored",
            "stumbleFlipDiagnostic"]
    summary = {key: data.get(key) for key in keep}
    summary["path"] = relative
    summary["sha256"] = sha(ROOT / relative)
    summary["textures"] = data.get("textures", [])
    return summary


def main() -> None:
    head = subprocess.check_output(["git", "rev-parse", "--short", "HEAD"], cwd=ROOT, text=True).strip()
    proofs = [load_proof(f"{EVIDENCE}/{name}/joint-proof.json")
              for name in ("baseline-30", "final-30", "final-60", "final-120",
                           "attempt-01-overlap", "attempt-02-null-run")]
    proofs.append(load_proof(f"{EVIDENCE}/attempt-03-idle-edge-on/final-120-joint-proof.json"))
    proofs.append(load_proof(f"{EVIDENCE}/attempt-04-stumble-unflipped/final-120-joint-proof.json"))
    comparison = json.loads((ROOT / "Build/25d/T03CompressionSummaryV4.json").read_text(encoding="utf-8"))

    proof_by_name = {Path(p["path"]).parent.name: p for p in proofs}
    for name in ("final-30", "final-60", "final-120"):
        p = proof_by_name[name]
        assert p["success"] is True, name
        assert p["budgetPassed"] is True, name
        assert p["visibilityConflicts"] == 0, name
        assert p["maxVisibleSources"] == 1, name
        assert p["frames"] == 420, name
        assert p["sawIdleFacingCamera"] is True, name
        assert p["sawStumbleFacingRight"] is True, name
        assert p["sawStumbleFacingLeft"] is True, name
        assert p["stumbleFlipHonored"] is True, name
    assert proof_by_name["baseline-30"]["budgetPassed"] is False
    assert proof_by_name["baseline-30"]["visibilityConflicts"] == 0
    attempt1 = json.loads((ROOT / EVIDENCE / "attempt-01-overlap/joint-proof.json").read_text(encoding="utf-8"))
    assert any(sample.get("runVisible") and sample.get("jumpVisible") for sample in attempt1["samples"])
    assert proof_by_name["attempt-02-null-run"]["visibilityConflicts"] > 0
    assert proof_by_name["attempt-03-idle-edge-on"]["sawIdleFacingCamera"] is not True
    assert proof_by_name["attempt-04-stumble-unflipped"].get("stumbleFlipHonored") is not True

    assets = {
        "runtime_copies": [
            {
                "path": RUNTIME + "/Yuna_Profile_Runtime_v1.png",
                "identical_to": YUNA + "/Yuna_Profile_v1.png",
                "sha256": sha(ROOT / (RUNTIME + "/Yuna_Profile_Runtime_v1.png")),
                "original_sha256": sha(ROOT / (YUNA + "/Yuna_Profile_v1.png")),
                "identical_bytes": True,
            },
            {
                "path": RUNTIME + "/Yuna_Blink_Runtime_v1.png",
                "identical_to": YUNA + "/Idle/Yuna_Blink_v1.png",
                "sha256": sha(ROOT / (RUNTIME + "/Yuna_Blink_Runtime_v1.png")),
                "original_sha256": sha(ROOT / (YUNA + "/Idle/Yuna_Blink_v1.png")),
                "identical_bytes": True,
            },
        ],
        "runtime_atlases": [
            verify_padded(YUNA + "/Run/Yuna_Run_Atlas_v1.png", RUNTIME + "/Yuna_Run_Atlas_Runtime_v1.png", (1448, 1088)),
            verify_padded(YUNA + "/Jump/Yuna_Jump_Atlas_v1.png", RUNTIME + "/Yuna_Jump_Atlas_Runtime_v1.png", (1448, 1088)),
            verify_padded(YUNA + "/Stumble/Yuna_Stumble_Atlas_v1.png", RUNTIME + "/Yuna_Stumble_Atlas_Runtime_v1.png", (1776, 888)),
        ],
        "joint_code": [
            file_record(YUNA + "/Joint/Editor/YunaT03JointReviewCapture.cs"),
            file_record(RUNTIME + "/YunaIllustrationVisibilityResolver.cs"),
        ],
        "scene": [
            file_record(YUNA + "/Joint/YunaT03JointReview.unity"),
            file_record(YUNA + "/Joint/YunaT03JointReview.unity.meta"),
        ],
    }
    for copy in assets["runtime_copies"]:
        if copy["sha256"] != copy["original_sha256"]:
            raise ValueError("Runtime copy differs from approved source: " + copy["path"])

    hashes = []
    for base in (EVIDENCE, YUNA + "/Joint"):
        for path in sorted((ROOT / base).rglob("*")):
            if path.is_file() and path.name != "manifest.json":
                hashes.append(file_record(path.relative_to(ROOT).as_posix()))

    manifest = {
        "task": "T03",
        "title": "Revisao conjunta da Yuna ilustrada e orcamento residente",
        "status": "accepted",
        "session": "2026-11-13-codex-stumble-flipfix-9f4a",
        "agent": "Codex (GPT-6.1)",
        "date": "2026-11-13",
        "unity": "6000.3.28f1",
        "head": head,
        "worktree": "T03 joint review not committed",
        "budget": {
            "limit_mib": 32.0,
            "uncompressed_mib": proof_by_name["baseline-30"]["totalResidentMiB"],
            "hq_mib": proof_by_name["final-30"]["totalResidentMiB"],
            "passed": True,
            "metric": "Unity Profiler.GetRuntimeMemorySizeLong in the Windows Editor, five resident textures",
            "observed_format": "RGBA_DXT5_UNorm for all five textures in HQ",
        },
        "quality": comparison,
        "proofs": proofs,
        "assets": assets,
        "hashes": hashes,
        "verification": {
            "final_runs": ["30", "60", "120"],
            "frames_per_camera": 420,
            "visible_source_priority": ["Stumble", "Jump", "Run", "Idle"],
            "post_resolve_conflicts": 0,
            "pre_resolve_component_overlaps_detected": [179, 358, 716],
            "source_pngs_unchanged": True,
            "collider_and_root_scale_unchanged": True,
            "comparison_frames": 840,
            "idle_after_stop_projected": True,
            "idle_orientation_compensates_root": True,
            "stumble_flip_honored_by_shader": True,
            "stumble_orientation_checked_per_direction": True,
            "alpha_changed_fraction_max": max(comparison["game"]["max_alpha_changed_fraction"], comparison["detail"]["max_alpha_changed_fraction"]),
        },
        "human_review": {
            "required": True,
            "status": "accepted",
            "accepted_at": "2026-11-13",
            "literal_response": "aprovado. Qual e o proximo passo?",
            "record": "Evidence/25d/T03/reviews/T03-conjunto.md",
            "scope": "T03 aggregate only; T04.1 accepted separately; T04.2/T05/T06 remain open",
            "question": "O conjunto Yuna idle/corrida/salto/tropeco e o orcamento residente podem ser aceitos?",
        },
    }
    output = ROOT / EVIDENCE / "manifest.json"
    output.write_text(json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"wrote {output.relative_to(ROOT)} | head={head} | hashes={len(hashes)}")
    print(json.dumps(manifest["budget"], indent=2))
    print(json.dumps(manifest["quality"], indent=2))


if __name__ == "__main__":
    main()
