"""Read-only guard for replacing only the Yuna visual. Run from the repo root."""
import hashlib
import json
import re
from pathlib import Path


def blocks(path):
    return {m[1]: m[0] for m in re.finditer(r"^--- !u!\d+ &(-?\d+).*?(?=^--- !u!|\Z)",
            path.read_text(encoding="utf-8-sig"), re.M | re.S)}


snapshot = Path("Build/yuna-scene-current-before.unity")
main = Path("Assets/Scenes/DarkRoom_Gameplay.unity")
validated = Path("Build/CameraFramingValidation/Assets/Scenes/DarkRoom_Gameplay.unity")
a, b = blocks(snapshot), blocks(validated)
body = next(k for k, v in a.items() if v.splitlines()[1] == "GameObject:"
            and re.search(r"^  m_Name: Body_Visual$", v, re.M))
transforms = {k: (re.search(r"m_GameObject: \{fileID: (\d+)\}", v)[1],
                 re.search(r"m_Father: \{fileID: (\d+)\}", v)[1])
              for k, v in a.items() if v.splitlines()[1] == "Transform:"}
body_transform = next(k for k, (go, _) in transforms.items() if go == body)
parent = transforms[body_transform][1]
chosen = {body_transform}
while True:
    expanded = chosen | {k for k, (_, p) in transforms.items() if p in chosen}
    if expanded == chosen:
        break
    chosen = expanded
owners = {transforms[k][0] for k in chosen}
allowed = owners | {k for k, v in a.items()
                    if re.search(r"m_GameObject: \{fileID: (\d+)\}", v)
                    and re.search(r"m_GameObject: \{fileID: (\d+)\}", v)[1] in owners}
changed = {k for k in a.keys() & b.keys() if a[k] != b[k]}
removed, added = a.keys() - b.keys(), b.keys() - a.keys()
guid = re.search(r"guid: (\w+)", Path("Build/CameraFramingValidation/Assets/Characters/Yuna/Yuna_Study.prefab.meta").read_text())[1]
unexpected = (changed - {parent}) | (removed - allowed)
unexpected |= {k for k in added if guid not in b[k]}
report = {"mainStillMatchesSnapshot": main.read_bytes() == snapshot.read_bytes(),
          "changedExisting": sorted(changed), "removed": sorted(removed), "added": sorted(added),
          "unexpected": sorted(unexpected), "unchangedExisting": len(a.keys() & b.keys()) - len(changed),
          "baselineSha256": hashlib.sha256(snapshot.read_bytes()).hexdigest(),
          "validatedSha256": hashlib.sha256(validated.read_bytes()).hexdigest(),
          "otherAudioSha256": {str(p): hashlib.sha256(p.read_bytes()).hexdigest()
                               for p in Path("Assets/Audio/SFX").glob("*.wav")}}
print(json.dumps(report, indent=2))
assert report["mainStillMatchesSnapshot"], "Scene changed again; validate the current scene before integration."
assert not unexpected, "Unexpected changes outside the character visual."
