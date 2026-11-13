"""Create runtime copies of approved profile/blink textures with HQ import settings.

The PNG bytes are copied verbatim so T02 provenance is preserved. Only the new
.meta files differ, leaving the approved original import settings untouched.
"""
from pathlib import Path
import hashlib
import re

ROOT = Path(__file__).resolve().parents[1]
YUNA = ROOT / "Assets/Art/Illustrated25D/Yuna"
OUT = YUNA / "Joint/Runtime"
ITEMS = [
    (YUNA / "Yuna_Profile_v1.png", OUT / "Yuna_Profile_Runtime_v1.png"),
    (YUNA / "Idle/Yuna_Blink_v1.png", OUT / "Yuna_Blink_Runtime_v1.png"),
]


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def stable_guid(asset_path: Path) -> str:
    relative = asset_path.relative_to(ROOT).as_posix()
    return hashlib.sha1(("yuna-t03-runtime:" + relative).encode("utf-8")).hexdigest()[:32]


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    for source, output in ITEMS:
        source_meta = source.with_suffix(source.suffix + ".meta")
        output_meta = output.with_suffix(output.suffix + ".meta")
        source_bytes = source.read_bytes()
        output.write_bytes(source_bytes)
        if sha(source) != sha(output):
            raise ValueError(f"Runtime copy differs from approved source: {output}")
        text = source_meta.read_text(encoding="utf-8")
        text, count = re.subn(r"(?m)^guid: [0-9a-f]{32}$", "guid: " + stable_guid(output), text)
        if count != 1:
            raise ValueError(f"Expected exactly one guid in {source_meta}")
        text, count = re.subn(r"(?m)^(\s*textureCompression:) \d+$", r"\1 2", text)
        if count == 0:
            raise ValueError(f"No textureCompression entry in {source_meta}")
        output_meta.write_text(text, encoding="utf-8", newline="\n")
        print(
            f"{source.relative_to(ROOT)} -> {output.relative_to(ROOT)} | "
            f"sha256={sha(output)} | meta={output_meta.relative_to(ROOT)} | "
            f"guid={stable_guid(output)} | hq_overrides={count}"
        )


if __name__ == "__main__":
    main()
