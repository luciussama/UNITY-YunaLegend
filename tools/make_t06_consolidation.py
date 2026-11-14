#!/usr/bin/env python3
"""Build the T06/T07 consolidation manifest from the delivered artifacts.

Hashes the accompaniment build and the sources that define the pilot, reads the
per-rate verification files and writes Evidence/25d/T06/parametros.json. Values
that live in C# are cited with their source file; this tool does not parse
claims as proof.
"""
from __future__ import annotations

import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Evidence" / "25d" / "T06" / "parametros.json"
BUILD = ROOT / "Build" / "Windows" / "YunaLegend-Acompanhamento" / "YunaLegend.exe"

SOURCES = [
    "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs",
    "Assets/Scripts/Runtime/DarkRoom/YieldingWoodFurniture.cs",
    "Assets/Scripts/Runtime/Camera/SideScroll25DCamera.cs",
    "Assets/Art/Illustrated25D/Preview/IllustratedPreview.cs",
    "Assets/Art/Illustrated25D/Preview/PreviewRetryVerification.cs",
    "Assets/Art/Illustrated25D/Preview/PreviewAudioCapture.cs",
    "Assets/Art/Illustrated25D/Preview/Editor/IllustratedPreviewBuild.cs",
    "Assets/Scenes/DarkRoom_IllustratedPreview.unity",
    "Assets/Scenes/DarkRoom_Gameplay.unity",
    "Packages/manifest.json",
    "Packages/packages-lock.json",
]

RATES = {
    "30": ROOT / "Evidence" / "25d" / "T06" / "rate-30" / "verification.json",
    "60": ROOT / "Evidence" / "25d" / "T06" / "audio-60" / "verification.json",
    "120": ROOT / "Evidence" / "25d" / "T06" / "rate-120" / "verification.json",
}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def rate_summary(path: Path) -> dict | None:
    if not path.exists():
        return None
    data = json.loads(path.read_text(encoding="utf-8"))
    attempts = data.get("attempts", [])
    return {
        "verification": str(path.relative_to(ROOT).as_posix()),
        "sha256": sha256(path),
        "hz": data.get("hz"),
        "success": data.get("success"),
        "errors": data.get("errors"),
        "frames": data.get("frames"),
        "audio_capture": data.get("audioCapture", False),
        "attempts": len(attempts),
        "cracks_per_attempt": [attempt.get("cracks") for attempt in attempts],
        "max_sag_m": max((attempt.get("sag", 0) for attempt in attempts), default=None),
        "max_pause_s": max((attempt.get("pause", 0) for attempt in attempts), default=None),
        "max_visible_yuna": data.get("maxVisible"),
        "legacy_visible": data.get("maxLegacy"),
    }


def main() -> int:
    if not BUILD.exists():
        print(f"missing build: {BUILD}", file=sys.stderr)
        return 1
    manifest = {
        "task": "T06",
        "consolidation": "T06/T07 criteria before expansion",
        "session": "2026-11-13-dsh-consolidacao-t06t07",
        "agent": "DeepSeek (deepseek-flash)",
        "unity": "6000.3.28f1",
        "build": {
            "path": str(BUILD.relative_to(ROOT).as_posix()),
            "bytes": BUILD.stat().st_size,
            "sha256": sha256(BUILD),
            "result": "Succeeded",
            "errors": 0,
        },
        "sources": [
            {"path": source, "sha256": sha256(ROOT / source)}
            for source in SOURCES if (ROOT / source).exists()
        ],
        "camera": {
            "projection": "perspective (T02.2a option A)",
            "field_of_view_deg": 45,
            "illustrated_vertical_span_m": 7.5,
            "camera_depth_m": round(7.5 / (2 * 0.41421356237309503), 4),
            "follow_time_s": 0.45,
            "x_lead_fraction_of_view": 0.18,
            "y_anchor_offset_m": 1.8,
            "next_support_weight": 0.25,
            "next_support_clamp_m": 0.5,
            "source": "Assets/Scripts/Runtime/Camera/SideScroll25DCamera.cs",
        },
        "physics": {
            "engine": "CharacterController 3D + PlatformSupportProfile",
            "gravity_m_s2": 22,
            "run_speed_m_s": 3.6,
            "jump_height_m": 2.4,
            "jump_apex_time_s": round((2 * 2.4 / 22) ** 0.5, 4),
            "coyote_time_s": 0.15,
            "jump_buffer_s": 0.15,
            "landing_assist": {
                "type": "horizontal speed calibrated to the next support landing point",
                "lerp_actual_to_base": 0.75,
                "multiplier_min": 0.80,
                "multiplier_max": 1.22,
                "comparison_ab_run": False,
                "note": "Preserved controller behavior; no isolated A/B variant was run in T06. Declared limit.",
            },
            "source": "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs + scene serialized values (gravity 22, coyote 0.15, buffer 0.15)",
        },
        "furniture_event": {
            "step_delay_s": 0.08,
            "recovery_s": 0.28,
            "max_sag_m": 0.075,
            "crack_clip": "Assets/Audio/SFX/wood_furniture_crack.wav",
            "crack_clip_sha256": sha256(ROOT / "Assets/Audio/SFX/wood_furniture_crack.wav"),
            "one_event_per_traversal": True,
            "source": "Assets/Scripts/Runtime/DarkRoom/YieldingWoodFurniture.cs",
        },
        "rates": {hz: rate_summary(path) for hz, path in RATES.items()},
        "audio": {
            "capture": "end-of-frame player mix, 48 kHz stereo PCM16, opt-in -previewAudioCapture",
            "background_music_in_mix": "High-Frequency Violin (looping, volume 0.50)",
            "listened_by_human": False,
            "evidence": "Evidence/25d/T06/audio-60/",
        },
        "input": {
            "keyboard": "Space/Up/W jump, P/Esc pause, R retry, Enter/Space start",
            "mouse": "left click jump and UI buttons",
            "touch": "primary touch jump; IMGUI buttons receive touch as mouse",
            "covered_by_automated_tests": "PlayMode DarkRoomStartInputTests (synthetic Keyboard/Mouse/Touchscreen)",
            "real_touchscreen_device": False,
        },
        "limits": [
            "No audio listening/mixing certification; only capture and clip correlation.",
            "No real touchscreen device, mobile orientation or safe-area validation.",
            "No WebGL build or browser run.",
            "No dynamic occlusion shadows.",
            "Horizontal landing assistance preserved without an isolated A/B comparison.",
            "T07 explicit human approval still pending; this manifest does not grant it.",
        ],
    }
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"wrote {OUT.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
