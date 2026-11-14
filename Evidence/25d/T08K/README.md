# T08K — Extensão do Quarto + Travessia conjunta 08 (Caixa de Música) + 09 (Castelinho)

14/11/2026. Sessão `2026-11-14-antigravity-08-09-cena`, registro 22 (`t08k/musica08-castelo09-quarto-cena-v1`).
Estado: **candidata entregue para revisão humana** (sem aceite global `T06`/`T07`).

## Arquivos e build
- **Extensão do quarto:** `Assets/Art/Illustrated25D/Environment/Room/room-wall-extension-v1.png`, `RoomBackgroundT054.cs` (`anchorPanelIndex`), `PenumbraT054.shader` / `PenumbraLightingT054.cs` (`_HasMoonWindow`).
- **Plataformas:** `Assets/Art/Illustrated25D/Environment/MusicBox/MusicBox08Platform.cs` (`music-box-08-v2.png`) e `Assets/Art/Illustrated25D/Environment/Castle/Castle09Platform.cs` (`block-castle-09-v1.png` + `block-castle-09-front-towers.png` em `sortingOrder = 25`).
- **Cena Unity própria:** `Assets/Art/Illustrated25D/Environment/Castle/Preview-Continuacao-09.unity`
- **Build Windows separada:** `Build/Windows/YunaLegend-Continuacao-09/YunaLegend.exe` (builds anteriores preservadas).

## Validação no executável (`30 / 60 / 120 Hz`)
Todas as três taxas passaram (`success: true`) com **2 travessias completas**, **10 saltos por travessia**, **1 estralo no móvel**, **reset limpo (`R`)**, **3 painéis contínuos de fundo (`roomPanels = 3`)**, **0 erros** e **0 renderers legados**:
- **Ponto de tensão (`08 → 09`):** `castleTensionGap = 2,7495 m` (`08 TopY = -3,55 m` → `09 TopY = -3,95 m`).
- **Contato:** `idleGapToSupport = 0.0`, `landingGapToSupport = 0.0`, `maxMusicBoxAnchorGap = 0.0`, `maxCastleAnchorGap < 0.000001`.
- `30 Hz`: `Evidence/25d/T08K/final-30/verification.json`
- `60 Hz`: `Evidence/25d/T08K/final-60/verification.json`
- `120 Hz`: `Evidence/25d/T08K/final-120/verification.json`
- **Capturas (`60 Hz`):** [pouso na caixa de música 08](./final-60/attempt-0-musicbox-land.png), [salto de tensão 2,75 m](./final-60/attempt-0-tension-jump.png), [pouso atrás da torre no castelinho 09](./final-60/attempt-0-castle-land.png) e [repouso final no castelinho 09](./final-60/attempt-0-castle-idle.png).
