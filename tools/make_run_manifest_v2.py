#!/usr/bin/env python3
"""Write the T03.2 v2 manifest: the exact sources, scene, proofs, GIFs and approvals.

The v1 manifest predates the loop fix and does not describe this checkout. This one
covers the files that produced the delivered capture and the files delivered, so a
reviewer can check the bytes instead of trusting a narrative.

Example:
    python tools/make_run_manifest_v2.py
"""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

SOURCES = [
    "Assets/Art/Illustrated25D/Yuna/Run/YunaRunIllustration.cs",
    "Assets/Art/Illustrated25D/Yuna/Run/Editor/YunaRunReviewCapture.cs",
    "Assets/Art/Illustrated25D/Yuna/Run/YunaRunReview.unity",
    "Assets/Art/Illustrated25D/Yuna/Run/Yuna_Run_Atlas_v1.png",
    "Assets/Art/Illustrated25D/Yuna/Run/Run_Painted.mat",
    "Assets/Art/Illustrated25D/Yuna/Run/TechnicalMetreMarks.mat",
    *[f"Assets/Art/Illustrated25D/Yuna/Run/Frames/Run{index}.asset" for index in range(8)],
    "tools/make_run_review.py",
    "tools/make_run_review_v2.py",
]
EVIDENCE = [
    "Evidence/25d/T03.2/README.md",
    "Evidence/25d/T03.2/review-v2/README.md",
    "Evidence/25d/T03.2/review-v2/run-proof.json",
    "Evidence/25d/T03.2/review-v2/verification.json",
    "Evidence/25d/T03.2/review-v2/run-gameplay.gif",
    "Evidence/25d/T03.2/review-v2/run-detail.gif",
    "Evidence/25d/T03.2/review-v2/run-detail-slow.gif",
    "Evidence/25d/T03.2/reviews/T03.2-v2.md",
    "Evidence/25d/T03.2/reviews/T03.2-v2-aprovacao.md",
    *[
        f"Evidence/25d/T03.2/{folder}/{name}"
        for folder in ("validation-v2-60", "validation-v2-120", "final-v2-30", "final-v2-60",
                       "final-v2-smooth-60")
        for name in ("run-proof.json", "verification.json")
    ],
]


def entry(relative):
    path = ROOT / relative
    if not path.exists():
        raise SystemExit("missing manifest input: " + relative)
    data = path.read_bytes()
    return {"sha256": hashlib.sha256(data).hexdigest(), "bytes": len(data)}


def main():
    manifest = {
        "scope": "T03.2 v2: the delivered clip capture, the sources and scene that produced it, the "
                 "per-rate and diagnostic proofs, the review GIFs and the human approval record. "
                 "Raw frames stay in Build/ (outside Git); verification.json carries their 256 hashes.",
        "clip": {
            "samples": 128,
            "simulation_hz": 120,
            "cycles_per_clip": 11.005291938781738,
            "loop_phase_residual_poses": 0.04233551025390625,
            "mark_spacing_m": 1.0084848403930664,
        },
        "sources": {path: entry(path) for path in SOURCES},
        "evidence": {path: entry(path) for path in EVIDENCE},
    }
    manifest["counts"] = {"sources": len(manifest["sources"]), "evidence": len(manifest["evidence"])}
    output = ROOT / "Evidence/25d/T03.2/review-v2/manifest-v2.json"
    output.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"{output}: {manifest['counts']['sources']} sources, {manifest['counts']['evidence']} evidence files")


if __name__ == "__main__":
    main()
