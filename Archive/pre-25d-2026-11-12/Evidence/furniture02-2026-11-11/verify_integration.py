"""Read-only scene guard. Run from repo root before copying the validated scene."""
import hashlib
import json
import re
from pathlib import Path


def blocks(path):
    text = Path(path).read_text(encoding="utf-8-sig")
    return {m[1]: m[0] for m in re.finditer(
        r"^--- !u!\d+ &(-?\d+).*?(?=^--- !u!|\Z)", text, re.M | re.S)}


def editable_subtrees(data):
    transforms, names, components = {}, {}, {}
    for key, value in data.items():
        name = re.search(r"^  m_Name: (.*)$", value, re.M)
        if name:
            names[key] = name[1]
        owner = re.search(r"m_GameObject: \{fileID: (\d+)\}", value)
        if owner:
            components.setdefault(owner[1], set()).add(key)
        if value.splitlines()[1] == "Transform:":
            parent = re.search(r"m_Father: \{fileID: (\d+)\}", value)[1]
            transforms[key] = (owner[1], parent)
    marker = next(v for v in data.values() if "DarkRoomPlatformMarker" in v
                  and re.search(r"^    index: 2$", v, re.M))
    furniture_go = re.search(r"m_GameObject: \{fileID: (\d+)\}", marker)[1]
    furniture_transform = next(k for k, (go, _) in transforms.items() if go == furniture_go)
    chosen = {k for k, (go, parent) in transforms.items()
              if names.get(go) == "Contacto_Fisico"
              or (names.get(go) == "Visual_3D" and parent == furniture_transform)}
    while True:
        expanded = chosen | {k for k, (_, parent) in transforms.items() if parent in chosen}
        if expanded == chosen:
            break
        chosen = expanded
    allowed = set()
    for key in chosen:
        go = transforms[key][0]
        allowed |= {go} | components.get(go, set())
    for key in list(allowed):
        for ref in re.findall(r"m_(?:Mesh|SharedMesh): \{fileID: (-?\d+)\}", data[key]):
            if ref in data and data[ref].splitlines()[1] == "Mesh:":
                allowed.add(ref)
    # Parent's child list and camera's initial pose are expected updates.
    return allowed | {furniture_transform, "101", "302"}


baseline = Path("Build/furniture-integration-current.unity")
main = Path("Assets/Scenes/DarkRoom_Gameplay.unity")
validated = Path("Build/CameraFramingValidation/Assets/Scenes/DarkRoom_Gameplay.unity")
a, b = blocks(baseline), blocks(validated)
changed = {k for k in a.keys() & b.keys() if a[k] != b[k]}
removed, added = a.keys() - b.keys(), b.keys() - a.keys()
unexpected = ((changed | removed) - editable_subtrees(a)) | (added - editable_subtrees(b))
same_main = main.read_bytes() == baseline.read_bytes()
report = {"mainStillMatchesSnapshot": same_main, "changedExisting": sorted(changed),
          "removed": sorted(removed), "added": sorted(added), "unexpected": sorted(unexpected),
          "unchangedExisting": len(a.keys() & b.keys()) - len(changed),
          "baselineSha256": hashlib.sha256(baseline.read_bytes()).hexdigest(),
          "validatedSha256": hashlib.sha256(validated.read_bytes()).hexdigest(),
          "otherAudioSha256": {str(p): hashlib.sha256(p.read_bytes()).hexdigest()
                               for p in Path("Assets/Audio/SFX").glob("*.wav")
                               if p.name != "wood_furniture_crack.wav"}}
print(json.dumps(report, indent=2))
assert same_main, "Main scene changed again; take a fresh snapshot and validate it."
assert not unexpected, "Unexpected scene changes; inspect before integrating."
