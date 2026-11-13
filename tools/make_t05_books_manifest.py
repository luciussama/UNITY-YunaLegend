#!/usr/bin/env python3
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EVIDENCE = ROOT / "Evidence/25d/T05.2"
OUTPUT = EVIDENCE / "manifest.json"

SOURCES = [
    "Evidence/25d/T01/books-v1.png",
    "Evidence/25d/T01/yuna-profile-v1.png",
    "Assets/Art/Illustrated25D/Environment/Books/Books_Platform_v1.png",
    "Assets/Art/Illustrated25D/Environment/Books/Resources/Books_Platform_Profile.json",
    "Assets/Art/Illustrated25D/Environment/Books/Books_Platform_Contact.asset",
    "Assets/Art/Illustrated25D/Environment/Books/Books_Platform_Visual.asset",
    "Assets/Art/Illustrated25D/Environment/Books/Books_Platform_Painted.mat",
    "Assets/Art/Illustrated25D/Environment/Books/Editor/BooksPlatformSetup.cs",
    "Assets/Art/Illustrated25D/Environment/Books/Editor/BooksPlatformReviewCapture.cs",
    "Assets/Art/Illustrated25D/Yuna/Joint/Runtime/Yuna_Profile_Runtime_v1.png",
    "Assets/Art/Illustrated25D/Yuna/Joint/Runtime/Idle_Painted_Runtime.mat",
    "Assets/Art/Illustrated25D/Yuna/Idle/PaintedIdle.shader",
    "Assets/Scripts/Runtime/Illustrated25D/Environment/Books/IllustratedBookStackPlatform.cs",
    "Assets/Scripts/Runtime/Illustrated25D/Environment/Books/IllustratedBookStackPlatformProfile.cs",
    "Assets/Scripts/Runtime/DarkRoom/DarkRoomPlatformArt.cs",
    "Assets/Scripts/Runtime/DarkRoom/DarkRoomPlatformGeometry.cs",
    "Assets/Scripts/Runtime/DarkRoom/DarkRoomThreeStageCircuitGenerator.cs",
    "Assets/Scripts/Tests/DarkRoomPlatformGeometryTests.cs",
    "Assets/Scenes/DarkRoom_Gameplay.unity",
    "tools/measure_t05_books_platform.py",
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
    "task": "T05.2",
    "scope": "livros: perfil de apoio medido, integracao ilustrada, camera final e revisao",
    "status": "T05.2 candidata aguardando revisao humana; T05 agregada pendente",
    "approval": {
        "scope": "T05.2 livros",
        "status": "pending",
        "literal_response": None,
        "date": None,
        "reference": "Evidence/25d/T05.2/reviews/T05.2-livros-proposta.md",
    },
    "review_questions": [
        "A Yuna no quadro e o sprite ilustrado oficial (nao o modelo 3D descartado)?",
        "A capa superior le como o apoio real da pilha no enquadramento final?",
        "Na camera final e no close, os pes da Yuna parecem em contato com a capa, sem flutuacao ou afundamento?",
        "O detalhe preserva a pintura e os cinco volumes sem introduzir tampo invisivel, borda reta ou deformacao para caber no colisor?",
    ],
    "contract": {
        "source_sha256": "cdbf8f5b1fa6f8cc3dd92372d142d26a72325005f4e84a97d49bd14fd83b0bec",
        "texture_width": 1620,
        "texture_height": 971,
        "metres_per_texture_pixel": 0.001268571,
        "visible_width_m": 1.726526,
        "visible_height_m": 1.11,
        "support_width_m": 1.184211,
        "support_tilt_degrees": -8.739244,
        "contact_offset_x_m": 0.134786,
        "support_height_above_centre_m": 0.363763,
        "physical_depth_m": 1.15,
        "collider": "one non-convex MeshCollider",
        "visual_plane_local_z": 0.0,
        "support_regions": 1,
        "support_segments": 2,
        "support_error_m": 0.000000,
        "outside_depth_hits": 0,
        "bear_to_books_gap_m": 2.451482,
        "bear_to_books_reach_m": 3.759794,
        "books_to_next_gap_m": 1.25,
        "books_to_next_reach_m": 3.649846,
        "review_yuna_visual": "approved illustrated idle painting, review-only; scene not saved",
        "review_yuna_sprite": "Yuna_Profile_Runtime_v1.png",
        "review_yuna_sprite_sha256": "0b9315b4dd525f23f2ef14db04682846b43b3ca7793f47670129afc88dfb9044",
        "review_yuna_material": "Idle_Painted_Runtime.mat",
        "review_yuna_legacy_3d_active": False,
        "review_yuna_official_active": True,
        "review_yuna_facing_right": True,
        "review_yuna_feet_error_m": 0.0,
    },
    "validations": {
        "unity_setup": "exit 0; T05_BOOKS_SETUP_SAVED width=1.726526 height=1.11 support=1.184211 tilt=-8.739244 offsetX=0.134786 depth=1.15 collider=MeshCollider",
        "editmode": "52 passed, 0 failed (validation/editmode-52.xml)",
        "review_harness": "exit 0; top raycast error 0; support max error 0; no support outside physical depth; both route gaps within jump reach; official illustrated Yuna active, legacy 3D body inactive, sprite hash matches T01, facing right, feet error 0",
    },
    "limitations": [
        "Revisao estatica e da camera final; nao aprova movimento, pouso, aparelhos ou o conjunto T05.",
        "Yuna nas capturas usa a pintura ilustrada oficial apenas no harness de revisao; a cena persistida mantem o corpo 3D e nao e salva. A integracao permanente T03 pertence a T06.",
        "O movel nao foi integrado nem revisado nesta microentrega.",
    ],
    "evidence": evidence,
    "sources": [entry(path) for path in SOURCES],
}

OUTPUT.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
print("WROTE {} ({} evidence files, {} sources)".format(
    OUTPUT.relative_to(ROOT), len(evidence), len(SOURCES)))
