"""Create block-compressible T03 runtime atlases without touching approved sources."""
from pathlib import Path
import hashlib
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Build/25d/PythonDeps"))
from PIL import Image

OUT = ROOT / "Assets/Art/Illustrated25D/Yuna/Joint/Runtime"
ITEMS = [
    ("Assets/Art/Illustrated25D/Yuna/Run/Yuna_Run_Atlas_v1.png", "Yuna_Run_Atlas_Runtime_v1.png", (1448, 1088)),
    ("Assets/Art/Illustrated25D/Yuna/Jump/Yuna_Jump_Atlas_v1.png", "Yuna_Jump_Atlas_Runtime_v1.png", (1448, 1088)),
    ("Assets/Art/Illustrated25D/Yuna/Stumble/Yuna_Stumble_Atlas_v1.png", "Yuna_Stumble_Atlas_Runtime_v1.png", (1776, 888)),
]


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    for source_name, output_name, size in ITEMS:
        source = ROOT / source_name
        output = OUT / output_name
        with Image.open(source) as image:
            rgba = image.convert("RGBA")
        if size[0] < rgba.width or size[1] < rgba.height:
            raise ValueError(f"Padding target is smaller than source: {source}")
        padded = Image.new("RGBA", size, (0, 0, 0, 0))
        padded.paste(rgba, (0, 0))
        if padded.crop((0, 0, rgba.width, rgba.height)).tobytes() != rgba.tobytes():
            raise ValueError(f"Source pixels changed while padding: {source}")
        alpha = padded.getchannel("A")
        changed = []
        for y in range(size[1]):
            for x in range(size[0]):
                if (x >= rgba.width or y >= rgba.height) and alpha.getpixel((x, y)) != 0:
                    changed.append((x, y))
        if changed:
            raise ValueError(f"Padding is not transparent: {source}")
        padded.save(output, "PNG", optimize=False)
        with Image.open(output) as written:
            if written.size != size or written.mode != "RGBA":
                raise ValueError(f"Invalid runtime PNG: {output}")
            if written.crop((0, 0, rgba.width, rgba.height)).tobytes() != rgba.tobytes():
                raise ValueError(f"Saved runtime PNG changed source pixels: {output}")
        print(f"{source_name} -> {output.relative_to(ROOT)} | {size[0]}x{size[1]} | source={sha(source)} | runtime={sha(output)}")


if __name__ == "__main__":
    main()
