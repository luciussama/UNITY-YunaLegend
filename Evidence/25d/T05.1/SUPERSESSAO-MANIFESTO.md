# T05.1 — nota de supersessao do manifesto historico

Data: 13/11/2026. Sessao `2026-11-13-codex-t052-books-b7c4`, Codex (GPT-6.1).

Este arquivo explica o [`manifest.json`](manifest.json) historico da T05.1. O
manifesto foi gerado por `tools/make_t05_bear_manifest.py` no momento do aceite
da T05.1 e **nao foi reescrito**. Ele continua provando o contrato e as fontes
especificas do urso, mas cinco fontes compartilhadas do Dark Room foram
alteradas depois pela T05.2.

## Supersessao

Para as fontes compartilhadas abaixo, o estado atual e o registrado em
`Evidence/25d/T05.2/manifest.json`:

- `superseded_by`: `Evidence/25d/T05.2/manifest.json`
- `superseded_at`: 13/11/2026
- `scope`: fontes compartilhadas do Dark Room alteradas pela integracao dos
  livros

| Fonte | SHA-256 na T05.1 | SHA-256 atual | Bytes T05.1 -> atual |
| --- | --- | --- | --- |
| `Assets/Scripts/Runtime/DarkRoom/DarkRoomPlatformArt.cs` | `628019a20e4c7fa589e8c14ac24ff03d725bd7ae341931c722c772b6efe4e2e5` | `d01dda3fd44d72687ae9eec223564c26ec473795b3be86eef1e24fcf1816c08c` | 42.929 -> 43.090 |
| `Assets/Scripts/Runtime/DarkRoom/DarkRoomPlatformGeometry.cs` | `db2f59bf93b0c421a3b3a925fd82afea87acd27334c1aeb2c13ae5a8874435b9` | `b3b17c79cadb69b507af9569907d0edd6c390aaa381bb6f086152554ad91c39d` | 9.944 -> 10.438 |
| `Assets/Scripts/Runtime/DarkRoom/DarkRoomThreeStageCircuitGenerator.cs` | `e0bf15e1950f52b75d593440871a2acd6831622b490282f8174f727ea6f3f2c8` | `d81b1c535bea46fc8aa4071e27e965778753ff9d0cdb0fbdcf1e37e871401a6f` | 9.927 -> 9.928 |
| `Assets/Scripts/Tests/DarkRoomPlatformGeometryTests.cs` | `6a27cceb44e613c5fd756da9aea6dfed07349a4b327770a01c12a17fac1e6366` | `db0ad84b2466d1fe59d88268453d6fcaaf756373ae79bbb2d0fccf83165ad194` | 15.077 -> 17.890 |
| `Assets/Scenes/DarkRoom_Gameplay.unity` | `f24464d8010af2a02a67d564c3420c38f340e9bcc1bf4a8671752a4395a0e25a` | `a824af77826f8adfa647af81c65c617068548f69041649914e1d9926f4dcb553` | 4.649.409 -> 4.653.741 |

As dez fontes especificas do urso e as oito evidencias da T05.1 continuam
byte-identicas. O aceite da T05.1 permanece valido; a divergencia acima e
evolucao posterior da T05.2, nao corrupcao nem adulteracao.

## Limite de reprodutibilidade

T05.1 e T05.2 foram commitadas juntas em `bc40170`. Por isso, os bytes
pre-T05.2 das cinco fontes compartilhadas nao estao preservados no historico
Git; o manifesto guarda seus hashes antigos, mas nao permite reconstruir
aquelas versoes a partir do `HEAD`. Para auditoria corrente, use o manifesto da
T05.2.

## Regra

Esta nota e adicional e nao altera `manifest.json`, `README.md` nem as
evidencias aceitas. Nao regenerar o manifesto da T05.1 para "corrigir" os
hashes: isso apagaria o snapshot historico do aceite.
