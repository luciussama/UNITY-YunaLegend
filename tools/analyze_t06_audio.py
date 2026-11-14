#!/usr/bin/env python3
"""Analyze the opt-in audio capture of the illustrated pilot (T06 consolidation).

Reads gameplay-audio.wav, the marks sidecar written by PreviewAudioCapture and
verification.json, then reports whether the wood-crack clip is present once per
completed traversal and whether the reset returns to near silence. The tool does
not listen to the audio; it measures amplitude and, when numpy is available,
normalized cross-correlation against the approved crack clip. A human must still
listen to the WAV before any claim of mixing quality.
"""
from __future__ import annotations

import argparse
import array
import json
import math
import sys
import wave
from pathlib import Path

CRACK_WINDOW = 0.35
SILENCE_PEAK = 0.01
CRACK_PEAK = 0.05
CORRELATION_FLOOR = 0.2
CORRELATION_OFFSET = 0.30


def load_mono(path: Path) -> tuple[list[float], int]:
    with wave.open(str(path), "rb") as handle:
        channels = handle.getnchannels()
        width = handle.getsampwidth()
        rate = handle.getframerate()
        frames = handle.getnframes()
        raw = handle.readframes(frames)
    if width != 2:
        raise SystemExit(f"expected PCM16, got {width * 8} bits")
    samples = array.array("h")
    samples.frombytes(raw)
    if channels == 1:
        mono = [value / 32768.0 for value in samples]
    else:
        mono = [sum(samples[index:index + channels]) / (channels * 32768.0)
                for index in range(0, len(samples), channels)]
    return mono, rate


def rms(values: list[float]) -> float:
    if not values:
        return 0.0
    return math.sqrt(sum(value * value for value in values) / len(values))


def peak(values: list[float]) -> float:
    return max((abs(value) for value in values), default=0.0)


def window(mono: list[float], rate: int, start: float, end: float) -> list[float]:
    first = max(0, int(start * rate))
    last = min(len(mono), int(end * rate))
    return mono[first:last]


def correlate(mix_path: Path, reference_path: Path, marks: list[dict]) -> dict:
    try:
        import numpy as np
    except ImportError:
        return {"available": False, "reason": "numpy not installed"}
    if not reference_path.exists():
        return {"available": False, "reason": f"missing {reference_path}"}

    def read(path: Path):
        with wave.open(str(path), "rb") as handle:
            channels = handle.getnchannels()
            rate = handle.getframerate()
            raw = handle.readframes(handle.getnframes())
        values = np.frombuffer(raw, dtype="<i2").astype(np.float64) / 32768.0
        if channels > 1:
            values = values.reshape(-1, channels).mean(axis=1)
        return values, rate

    ref, ref_rate = read(reference_path)
    mix, mix_rate = read(mix_path)
    timeline = np.arange(len(ref)) / ref_rate
    resampled_timeline = np.arange(max(1, int(len(ref) * mix_rate / ref_rate))) / mix_rate
    ref = np.interp(resampled_timeline, timeline, ref)
    ref = ref - ref.mean()
    ref_norm = float(np.linalg.norm(ref))
    if ref_norm <= 1e-9:
        return {"available": False, "reason": "silent reference clip"}
    mix = mix - mix.mean()
    size = 1
    while size < len(mix) + len(ref):
        size *= 2
    correlation = np.fft.irfft(
        np.fft.rfft(mix, size) * np.fft.rfft(ref[::-1], size), size)[len(ref) - 1:len(mix)]
    energy = np.concatenate([[0.0], np.cumsum(mix * mix)])
    window_energy = energy[len(ref):] - energy[:-len(ref)]
    ncc = correlation / (np.sqrt(np.maximum(window_energy, 1e-12)) * ref_norm)
    at_marks = []
    for mark in marks:
        if not mark["label"].startswith("crack-"):
            continue
        centre = int(mark["seconds"] * mix_rate)
        first = max(0, centre - int(CORRELATION_OFFSET * mix_rate))
        last = min(len(ncc), centre + int(CORRELATION_OFFSET * mix_rate))
        if last <= first:
            continue
        offset = int(np.argmax(ncc[first:last]))
        best = first + offset
        at_marks.append({
            "label": mark["label"],
            "mark_seconds": mark["seconds"],
            "best_seconds": round(best / mix_rate, 3),
            "offset_seconds": round((best - centre) / mix_rate, 3),
            "ncc": round(float(ncc[best]), 4),
            "match": bool(ncc[best] >= CORRELATION_FLOOR and abs(best - centre) / mix_rate <= CORRELATION_OFFSET),
        })
    top = np.argsort(ncc)[-5:][::-1]
    return {
        "available": True,
        "reference": str(reference_path.as_posix()),
        "reference_seconds": round(len(ref) / mix_rate, 3),
        "max_ncc": round(float(ncc.max()), 4),
        "mean_ncc": round(float(ncc.mean()), 5),
        "top": [{"seconds": round(int(index) / mix_rate, 3), "ncc": round(float(ncc[index]), 4)} for index in top],
        "at_marks": at_marks,
        "matched": all(item["match"] for item in at_marks) and len(at_marks) == 4,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("directory", type=Path)
    parser.add_argument("--reference", type=Path,
                        default=Path("Assets/Audio/SFX/wood_furniture_crack.wav"))
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()

    audio_path = args.directory / "gameplay-audio.wav"
    marks_path = args.directory / "gameplay-audio.json"
    verification_path = args.directory / "verification.json"
    for required in (audio_path, marks_path, verification_path):
        if not required.exists():
            raise SystemExit(f"missing {required}")

    mono, rate = load_mono(audio_path)
    marks_doc = json.loads(marks_path.read_text(encoding="utf-8"))
    verification = json.loads(verification_path.read_text(encoding="utf-8"))
    marks = marks_doc.get("marks", [])
    duration = len(mono) / rate

    played = [entry for entry in verification.get("audioSources", []) if "playing=True" in entry]
    bgm_playing = any("GameManager" in entry or "bgm" in entry.lower() for entry in played)

    cracks = [mark for mark in marks if mark["label"].startswith("crack-")]
    starts = [mark for mark in marks if mark["label"].endswith("-start")]
    results = []
    for mark in cracks:
        start = mark["seconds"]
        after = window(mono, rate, start, start + CRACK_WINDOW)
        before = window(mono, rate, max(0.0, start - CRACK_WINDOW), start)
        after_rms = rms(after)
        before_rms = rms(before)
        results.append({
            "label": mark["label"],
            "seconds": start,
            "rms_before": round(before_rms, 6),
            "rms_after": round(after_rms, 6),
            "peak_after": round(peak(after), 6),
            "ratio": round(after_rms / before_rms if before_rms > 1e-9 else float("inf"), 3),
            "peak_at_event": peak(after) >= CRACK_PEAK,
        })

    start_results = []
    for mark in starts:
        start = mark["seconds"]
        sample = window(mono, rate, start, start + 0.8)
        start_results.append({
            "label": mark["label"],
            "seconds": start,
            "rms": round(rms(sample), 6),
            "peak": round(peak(sample), 6),
            "silent": peak(sample) <= SILENCE_PEAK,
        })

    correlation = correlate(audio_path, args.reference, marks)
    attempts_with_crack = {int(item["label"].split("-")[1]) for item in cracks}
    checks = {
        "audio_captured": len(mono) > 0 and peak(mono) > SILENCE_PEAK,
        "four_crack_events": len(cracks) == 4 and attempts_with_crack == {0, 1, 2, 4},
        "crack_peak_at_event": all(item["peak_at_event"] for item in results),
        "crack_clip_matches_mix": correlation.get("matched") if correlation.get("available") else None,
        "no_crack_on_failed_attempt": 3 not in attempts_with_crack,
        "reset_quiet": all(item["silent"] for item in start_results) if not bgm_playing else None,
    }
    passed = all(value is True for value in checks.values() if value is not None)
    report = {
        "source": str(audio_path.as_posix()),
        "sample_rate": rate,
        "duration_seconds": round(duration, 3),
        "overall_peak": round(peak(mono), 6),
        "overall_rms": round(rms(mono), 6),
        "bgm_playing": bgm_playing,
        "audio_sources": verification.get("audioSources", []),
        "checks": checks,
        "passed": passed,
        "cracks": results,
        "attempt_starts": start_results,
        "correlation": correlation,
        "note": "Amplitude and correlation measurements only; no human listening was performed.",
    }
    destination = args.output or (args.directory / "audio-analysis.json")
    destination.write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2, ensure_ascii=False))
    return 0 if passed else 1


if __name__ == "__main__":
    sys.exit(main())
