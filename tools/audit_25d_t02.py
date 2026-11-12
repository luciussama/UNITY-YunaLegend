"""Audit recorded T02 evidence against current sources; does not run Unity."""
import hashlib
import json
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]


def audit():
    def read(path):
        return json.loads((ROOT / path).read_text(encoding="utf-8"))

    def sha(path):
        return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()

    proof = read("Evidence/25d/T02.3/animation-proof.json")
    camera = read("Evidence/25d/T02.2b/stable-camera.json")
    checks = {}
    for path, key in [
        ("Assets/Scenes/DarkRoom_Gameplay.unity", "gameplayAfter"),
        ("Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs", "controllerAfter"),
        ("Assets/Art/Illustrated25D/Yuna/Yuna_Profile_v1.png", "paintingHash"),
    ]:
        checks[path] = sha(path) == proof[key]
    for key in ["sourceGameplayUnchanged", "gameplayHadNoSkin", "reachedBear",
                "reachedBooks", "visualAndColliderUnchanged", "soleNeverCrossedFloor"]:
        checks[key] = proof[key] is True
    checks["camera_height_spread_below_0.01px"] = camera["maxHeightPixels"] - camera["minHeightPixels"] < .01
    checks["camera_constant_depth"] = camera["maxDepth"] == camera["minDepth"]
    tests = ET.parse(ROOT / "Evidence/25d/T02.3/playmode-tests.xml").getroot()
    checks["recorded_tests_32_passed"] = tests.attrib["passed"] == "32" and tests.attrib["failed"] == "0"
    sources = [
        "Assets/Scripts/Runtime/Illustrated25D/PaintedMeshDeformer.cs",
        "Assets/Scripts/Runtime/Illustrated25D/IllustratedPaintedAnimation.cs",
        "Assets/Scripts/Editor/IllustratedAnimationProof.cs",
        "Assets/Scripts/Editor/IllustratedDeformationDetail.cs",
        "Assets/Scripts/Runtime/Camera/SideScroll25DCamera.cs",
        "Assets/Scenes/Validation/IllustratedAnimationProof.unity",
        "Assets/Art/Illustrated25D/Yuna/Yuna_Profile_v1.png.meta",
    ]
    for path in sources:
        recorded = subprocess.check_output(["git", "show", "5d2e6db:" + path], cwd=ROOT)
        # Git stores this camera source as LF and checks it out as CRLF by policy.
        current = (ROOT / path).read_bytes()
        if path == "Assets/Scripts/Runtime/Camera/SideScroll25DCamera.cs":
            checks["checkpoint_lf_normalized:" + path] = recorded.replace(b"\r\n", b"\n") == current.replace(b"\r\n", b"\n")
        else:
            checks["checkpoint:" + path] = hashlib.sha256(recorded).hexdigest() == sha(path)
    reviews = [
        "Evidence/25d/T02.1/reviews/T02.1-v1-aprovacao.md",
        "Evidence/25d/T02.2/reviews/T02.2a-v1-aprovacao.md",
        "Evidence/25d/T02.2b/review-v1.md",
        "Evidence/25d/T02.3/reviews/T02.3-v2-aprovacao.md",
    ]
    for path in reviews:
        checks["review_exists:" + path] = (ROOT / path).is_file()
    support = [s["soleToSupport"] for s in camera["samples"] if s["grounded"] and s["supportMeasured"]]
    files = sources + reviews + ["Evidence/25d/T02.3/animation-proof.json",
        "Evidence/25d/T02.3/playmode-tests.xml", "Evidence/25d/T02.2b/stable-camera.json"]
    return {
        "method": "Read-only evidence/source audit; recorded Unity results, no new Unity test or device measurement.",
        "checks": checks, "success": all(checks.values()),
        "baseline": {"skin_cpu_mean_ms": proof["skinAverageMilliseconds"],
                     "skin_cpu_max_ms": proof["skinMaxMilliseconds"],
                     "texture_editor_mib": proof["textureMegabytes"],
                     "camera_height_px_at_720": [camera["minHeightPixels"], camera["maxHeightPixels"]],
                     "grounded_root_to_support_m": [min(support), max(support)]},
        "recorded_test_names": [t.attrib["name"] for t in tests.iter("test-case")],
        "sha256": {path: sha(path) for path in files},
    }


if __name__ == "__main__":
    result = audit()
    destination = ROOT / "Evidence/25d/T02/technical-audit.json"
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"success": result["success"], "checks": len(result["checks"]), "report": str(destination)}))
    raise SystemExit(0 if result["success"] else 1)
