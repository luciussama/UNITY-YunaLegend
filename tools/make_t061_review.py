"""Monta o pacote de revisao da T06.1 (GIFs e resumo) a partir das capturas reais."""

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image  # noqa: E402

BASE = ROOT / "Evidence/25d/T06.1"
REVIEW = BASE / "review"


def load_report(hz):
    path = BASE / f"capture-{hz}" / "traversal-report.json"
    if not path.exists():
        return None
    return json.loads(path.read_text(encoding="utf-8"))


def build_gif(hz, report):
    frames = sorted((BASE / f"capture-{hz}" / "frames").glob("*.png"))
    if not frames:
        return None
    step = 3 if len(frames) > 120 else 2 if len(frames) > 60 else 1
    images = []
    for path in frames[::step]:
        with Image.open(path) as image:
            images.append(image.convert("RGB").resize((480, 270)).copy())
    REVIEW.mkdir(parents=True, exist_ok=True)
    out = REVIEW / f"traversal-{hz}.gif"
    images[0].save(out, save_all=True, append_images=images[1:], duration=80, loop=0, optimize=True)
    return out, len(images)


def main():
    REVIEW.mkdir(parents=True, exist_ok=True)
    summary = []
    for hz in (30, 60, 120):
        report = load_report(hz)
        if report is None:
            print(f"{hz} Hz: sem relatorio")
            continue
        built = build_gif(hz, report)
        summary.append(
            {
                "hz": hz,
                "success": report["success"],
                "bear": report["reachedBear"],
                "books": report["reachedBooks"],
                "jumps": f"{report['jumpTakeoffs']}/{report['jumpLandings']}",
                "visible": report["maxVisibleSources"],
                "conflicts": report["visibilityConflicts"],
                "died": report["died"],
                "bearGap": f"{report['minBearGap']:.3f}..{report['maxBearGap']:.3f}",
                "booksGap": f"{report['minBooksGap']:.3f}..{report['maxBooksGap']:.3f}",
                "frames": report["frames"],
                "gameplay": report["gameplayUnchanged"],
                "controller": report["controllerUnchanged"],
                "collider": report["colliderUnchanged"],
                "gif": str(built[0].relative_to(BASE)) if built else "-",
                "gifFrames": built[1] if built else 0,
            }
        )
    (REVIEW / "summary.json").write_text(json.dumps(summary, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps(summary, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()

