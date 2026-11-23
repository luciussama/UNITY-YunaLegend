# Cleanup Audit — Agente 3 (Guardião de Limpeza e Otimização)

**Data:** 22/11/2026  
**Sessão:** `20261122-antigravity-agente3-limpeza` (Job `143` no `agent_registry.py`)  
**Política aplicada:** Zona Intocável preservada + Regra da Dúvida (Quarentena Documentada) + Validação pós-limpeza no Unity.

---

## 1. Remoções Seguras Executadas (Ciclo 1 — 562,86 MB recuperados)

Todos os itens abaixo eram evidências temporárias de auditoria ou estudos/folhas explicitamente **reprovados/superados** por marcos já aprovados (`UnityStudyF000-042ActingV6` e `UnityStudyF000-058V2`). Nenhuma cena, prefab, script C# ou asset ativo em `study-manifest.json` foi tocado.

| Categoria | Itens removidos | Espaço recuperado | Motivo |
| --- | --- | --- | --- |
| Evidências de auditoria BearChase | `Evidence/bear-chase-audit/` (17 PNGs/MP4) | `20,36 MB` | Capturas/vídeo temporários de auditoria de fase já concluída; diretório é recriado sob demanda por `BearChaseVerificationCapture.cs`. |
| Capturas Unity reprovadas/superadas (`CIN01`) | 11 diretórios (`UnityStudyV3`, `UnityStudyV3Pass2`, `UnityStudyF000-042`, `RegistrationV2`, `ModelV3`, `UniformV1`, `ActingV2`, `ActingV3`, `ActingV4`, `ActingV5`, `UnityStudyF000-058V1`) | `442,42 MB` | Estudos intermediários reprovados ou superados pelos marcos aprovados `UnityStudyF000-042ActingV6` e `UnityStudyF000-058V2`. |
| Folhas brutas/fatiadas descartadas e diagnósticos (`CIN01`) | 71 itens (`yuna-f000-024-*`, `yuna-f025-033-*`, `yuna-f034-042-*`, `yuna-*-detail-*`, `yuna-*-acting-v2/v3/v4*`, `yuna-f043-051-v1*`, `yuna-f052-058-v1*`, `v3-on-room-*.png`, `diagnostic-f034-bottom.png`, JSONs v1/v3/v4) | `100,08 MB` | Desenhos substituídos pelas versões ativas em `study-manifest.json`. |
| **Total recuperado** | **83 itens** | **562,86 MB** | **59/59 quadros ativos em `study-manifest.json` verificados íntegros.** |

### Zona Intocável preservada 100%
- `Evidence/Cinematics25/UnityStudyF000-042ActingV6/` e `Evidence/Cinematics25/UnityStudyF000-058V2/` (marcos com aceite humano registrado nas entradas `1459` e `1465` do diário).
- Todas as folhas ativas referenciadas por `study-manifest.json` (`acting-v5`, `acting-v6`, `f043-058 v2`), `Original/`, `Validation/`, `CIN01..04-direcao.md`, `README.md`, `prepare_sheet.py`, `test_prepare_sheet.py` e JSONs de proveniência ativos.
- Todos os scripts em desenvolvimento ativo de `CIN01` (`ToyRoomIntroTimeline.cs`, `HandDrawnExposurePlayer.cs`, `Cinematics25StudyCapture.cs`, etc.).

---

## 2. Quarentena Documentada (Regra da Dúvida — NÃO excluídos)

Os arquivos abaixo não possuem referência direta por GUID em `.unity`/`.prefab`/`.mat`/`.asset` nem citação literal em scripts `.cs`, mas podem ser carregados por modelos `.fbx`/`.glb`, importadores externos ou convenções dinâmicas. **Não foram removidos** e aguardam confirmação humana antes de qualquer exclusão:

### 2.1 Imagens sem referência direta detectada em `Assets/Art/` (19 arquivos — 14,30 MB)

| Tamanho | Caminho | Motivo da suspeita / quarentena |
| --- | --- | --- |
| `2,33 MB` | `Assets/Art/ToyRoom/Models/Props/bunny_Albedo.png` | Sem GUID em cenas/materiais; pode estar embutida/vinculada a modelo 3D `.fbx`/`.glb` de ToyRoom. |
| `2,18 MB` | `Assets/Art/BearChase/Textures/train-atlas-v1.png` | Sem referência direta em YAML ou C#; confirmar se já foi substituída por v2/material procedural. |
| `2,08 MB` | `Assets/Art/ToyRoom/Models/Props/teddy_Albedo.png` | Sem GUID em cenas/materiais; pode estar vinculada a modelo 3D do urso em ToyRoom. |
| `2,01 MB` | `Assets/Art/BearChase/Textures/blocks-atlas-v1.png` | Sem referência direta em YAML ou C#; confirmar se é textura legada de BearChase. |
| `1,65 MB` | `Assets/Art/Arena/Environment/chest-toys-open-v2.png` | Sem referência direta em cenas/materiais/C#; confirmar se é asset sobressalente da Arena. |
| `1,06 MB` | `Assets/Art/Arena/Girl/arena-actions.png` | Sem referência direta em cenas/materiais/C#; confirmar se a Arena ainda usa spritesheet 2D legado. |
| `0,99 MB` | `Assets/Art/Arena/Girl/arena-base.png` | Mesmo caso acima (spritesheet 2D da Arena). |
| `0,54 MB` | `Assets/Art/Arena/Girl/arena-damage.png` | Mesmo caso acima. |
| `0,47 MB` | `Assets/Art/Arena/Girl/arena-defense.png` | Mesmo caso acima. |
| `0,33 MB` | `Assets/Art/Arena/Girl/arena-crouch.png` | Mesmo caso acima. |
| `0,18 MB` | `Assets/Art/Arena/Environment/bookshelf-v1.png` | Sem referência direta em YAML ou C#; possível arte 2D substituída na Arena. |
| `0,12 MB` | `Assets/Art/Arena/Environment/oak-cabinet-v1.png` | Sem referência direta em YAML ou C#. |
| `0,07 MB` | `Assets/Art/ToyRoom/Textures/Legacy/central-rug-v1.png` | Pasta `Legacy/`; sem referência direta em YAML ou C#. |
| `0,07 MB` | `Assets/Art/Arena/Environment/rocking-horse-v1.png` | Sem referência direta em YAML ou C#. |
| `0,07 MB` | `Assets/Art/Arena/Models/Textures/T_Bear_Face.png` | Sem GUID em YAML; pode ser referenciada por `.fbx` ou script escultor da Arena. |
| `0,05 MB` | `Assets/Art/Arena/Models/Textures/T_Dino_Body.png` | Mesmo caso acima. |
| `0,05 MB` | `Assets/Art/Arena/Models/Textures/T_Dino_Head.png` | Mesmo caso acima. |
| `0,01 MB` | `Assets/Art/Arena/Models/T_Arena_SlashArc.png` | Sem referência direta em YAML ou C#. |
| `<0,01 MB` | `Assets/Art/Arena/Environment/contact-shadow.png` | Sem referência direta em YAML ou C#. |

### 2.2 Builds locais não versionadas e higiene opcional de logs em Runtime
- **`Build/ArenaWebGL/` (`147 MB`, não rastreada pelo Git):** build WebGL local antiga da Arena; mantida em disco até confirmação humana se pode ser apagada.
- **Logs informativos em Runtime (`Assets/Scripts/Runtime/UI/TitlePhaseSelector.cs`, `DarkRoomOpeningSequence.cs`, `DarkRoomBlockoutBuilder.cs`, `DarkRoomProgressionManager.cs`):** chamadas `Debug.Log(...)` mantidas por prudência (nenhuma altera lógica; podem ser condicionadas a `#if UNITY_EDITOR` em ciclo futuro). Os `Debug.Log` de `Preview*Verification.cs` são marcadores usados por testes automatizados e **não** devem ser removidos.

---

## 3. Validação Pós-Limpeza (Build de Verificação — Agente 2)

- **Compilação Unity (`6000.3.28f1 -batchmode`):** **0 erros de compilação** (`error CS`: 0).
- **Suíte EditMode (`-runTests -testPlatform EditMode`):** **164/173 passaram** (`0` falhas relacionadas à limpeza; todas as suítes ativas de `ToyRoom`, `Arena`, `BearChase`, `Illustrated25D` e `Cinematics25` passaram integralmente). As 9 falhas observadas são pré-existentes de testes legados que buscam a cena antiga `Assets/Scenes/DarkRoom_Gameplay.unity` (substituída historicamente por `Fase-Jogavel-Completa.unity`).
- **Integridade do manifesto ativo (`Evidence/Cinematics25/study-manifest.json`):** **59/59 quadros presentes** em disco e testes unitários de `prepare_sheet.py` validados.
- **Resultado:** limpeza aprovada sem necessidade de reversão via Git.
