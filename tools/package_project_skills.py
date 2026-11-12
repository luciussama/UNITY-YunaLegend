"""Package the shareable skill instructions; never include the game's assets or diary."""
import argparse
import hashlib
from pathlib import Path
import zipfile


def main():
    root = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--out", type=Path, default=root / "Build/Exports/yuna-legend-skills.zip")
    args = parser.parse_args()
    skill_root = (root / "skills").resolve()
    if args.out.suffix.lower() != ".zip" or args.out.is_symlink():
        parser.error("Output must be a regular .zip path")
    if args.out.resolve().is_relative_to(skill_root):
        parser.error("Place the export outside skills/ to avoid including an earlier export")
    files = [root / "SKILLS.md"]
    files += [p for p in (root / "skills").rglob("*") if p.is_file() and "__pycache__" not in p.parts and p.suffix != ".pyc"]
    for required in ["yuna-project", "yuna-visual-validation", "yuna-webgl", "imagegen"]:
        if not (root / "skills" / required / "SKILL.md").is_file():
            raise SystemExit("Missing skill: " + required)
    if not (root / "skills/imagegen/LICENSE.txt").is_file():
        raise SystemExit("Missing upstream license")
    manifest = []
    args.out.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(args.out, "w", compression=zipfile.ZIP_DEFLATED) as archive:
        for path in sorted(files):
            if path.is_symlink():
                raise SystemExit("Refusing symlink in share package: " + str(path))
            if path != root / "SKILLS.md" and not path.resolve().is_relative_to(skill_root):
                raise SystemExit("Skill file resolves outside the source directory: " + str(path))
            name = path.relative_to(root).as_posix()
            data = path.read_bytes()
            archive.writestr(name, data)
            manifest.append(hashlib.sha256(data).hexdigest() + "  " + name)
        archive.writestr("MANIFEST.sha256", "\n".join(manifest) + "\n")
    with zipfile.ZipFile(args.out) as archive:
        assert archive.testzip() is None
        for row in manifest:
            digest, name = row.split("  ", 1)
            assert hashlib.sha256(archive.read(name)).hexdigest() == digest
    print(f"{args.out.resolve()}: {len(files)} source files, {args.out.stat().st_size} bytes, SHA256 verified")


if __name__ == "__main__":
    main()
