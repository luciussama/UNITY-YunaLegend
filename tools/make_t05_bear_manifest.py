#!/usr/bin/env python3
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EVIDENCE = ROOT / "Evidence/25d/T05.1"
OUTPUT = EVIDENCE / "manifest.json"

SOURCES = [
    "Evidence/25d/T01/bear-v1.png",
    "Assets/Art/Illustrated25D/Environment/Bear/Bear_Platform_v1.png",
    "Assets/Art/Illustrated25D/Environment/Bear/Resources/Bear_Platform_Profile.json",
    "Assets/Art/Illustrated25D/Environment/Bear/Bear_Platform_Contact.asset",
    "Assets/Art/Illustrated25D/Environment/Bear/Bear_Platform_Visual.asset",
    "Assets/Art/Illustrated25D/Environment/Bear/Bear_Platform_Painted.mat",
    "Assets/Art/Illustrated25D/Environment/Bear/Editor/BearPlatformSetup.cs",
    "Assets/Art/Illustrated25D/Environment/Bear/Editor/BearPlatformReviewCapture.cs",
    "Assets/Scripts/Runtime/Illustrated25D/Environment/Bear/IllustratedBearPlatform.cs",
    "Assets/Scripts/Runtime/Illustrated25D/Environment/Bear/IllustratedBearPlatformProfile.cs",
    "Assets/Scripts/Runtime/DarkRoom/DarkRoomPlatformArt.cs",
    "Assets/Scripts/Runtime/DarkRoom/DarkRoomPlatformGeometry.cs",
    "Assets/Scripts/Runtime/DarkRoom/DarkRoomThreeStageCircuitGenerator.cs",
    "Assets/Scripts/Tests/DarkRoomPlatformGeometryTests.cs",
    "Assets/Scenes/DarkRoom_Gameplay.unity",
]


def entry(path):
    full = ROOT / path
    data = full.read_bytes()
    return {
        "path": path,
        "bytes": len(data),
        "sha256": hashlib.sha256(data).hexdigest(),
    }


evidence = []
for file in sorted(EVIDENCE.rglob("*")):
    if not file.is_file() or file == OUTPUT:
        continue
    evidence.append(entry(file.relative_to(ROOT).as_posix()))

manifest = {
    "schema": 1,
    "task": "T05.1",
    "scope": "urso: perfil de apoio, integracao ilustrada, camera final e revisao",
    "status": "T05.1 aprovada pelo usuario em 2026-11-13; T05 agregada pendente",
    "approval": {
        "scope": "T05.1 urso",
        "status": "accepted",
        "literal_response": "aprovado.",
        "date": "2026-11-13",
        "reference": "Evidence/25d/T05.1/reviews/T05.1-urso-proposta.md",
    },
    "review_questions": [
        "A coroa e as orelhas da prancha aprovada leem como o apoio real do urso no tamanho de gameplay?",
        "Na camera final, os pes da Yuna parecem em contato com a coroa, sem flutuacao ou oclusao enganosa?",
        "O detalhe preserva pintura, silhueta e proporcao sem introduzir borda reta ou tampo invisivel?",
    ],
    "contract": {
        "crown_world_y": -8.0,
        "crown_height_above_floor_m": 1.1,
        "visible_width_m": 2.017602,
        "physical_depth_m": 1.2,
        "collider": "one non-convex extruded MeshCollider",
        "visual_plane_local_z": 0.0,
        "support_regions": 7,
        "support_segments": 656,
        "support_error_m": 0.000001,
        "outside_depth_hits": 0,
        "bear_to_books_gap_m": 2.490840,
        "validated_jump_reach_m": 2.491969,
    },
    "validations": {
        "unity_setup": "exit 0; T05_BEAR_SETUP_SAVED width=2.017602 depth=1.2 crownY=-8",
        "editmode": "51 passed, 0 failed (validation/editmode-51.xml)",
        "review_harness": "exit 0; crown raycast error 0; support max error 0.000001 m; no support at z=±0.62",
    },
    "limitations": [
        "Revisao estatica e da camera final; nao aprova movimento, pouso, aparelhos ou o conjunto T05.",
        "Yuna no quadro e o visual ainda presente na cena principal; a integracao T03 pertence a T06.",
        "Livros e movel nao foram integrados nem revisados nesta microentrega.",
    ],
    "evidence": evidence,
    "sources": [entry(path) for path in SOURCES],
}

OUTPUT.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
print("WROTE {} ({} evidence files, {} sources)".format(
    OUTPUT.relative_to(ROOT), len(evidence), len(SOURCES)))
