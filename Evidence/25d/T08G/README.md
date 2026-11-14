# T08G — Travessia até o Tamborito de Marcha (05)

14/11/2026. Sessão `2026-11-14-antigravity-tambor05-cena`, registro 18 (`t08g/tambor05-cena-v1`).
Estado: **candidata entregue para revisão humana** (sem aceite global `T06`/`T07`).

## Arquivos e build
- **Script da plataforma:** `Assets/Art/Illustrated25D/Environment/Drum/Drum05Platform.cs`
- **Cena Unity própria:** `Assets/Art/Illustrated25D/Environment/Drum/Preview-Continuacao-05.unity`
- **Build Windows separada:** `Build/Windows/YunaLegend-Continuacao-05/YunaLegend.exe` (cenas/builds `T08C` e `T08E` preservadas).
- **Contrato de apoio:** [CONTRATO-APOIO-TAMBOR05.md](./CONTRATO-APOIO-TAMBOR05.md)

## Validação no executável (`30 / 60 / 120 Hz`)
Todas as três taxas passaram com **2 travessias completas**, **6 saltos por travessia**, **1 estralo no móvel**, **reset limpo (`R`)**, **0 erros** e **0 renderers legados**:
- `30 Hz`: `Evidence/25d/T08G/final-30/verification.json` (`idleGapToSupport = 0.0`, `landingGapToSupport = 0.0`)
- `60 Hz`: `Evidence/25d/T08G/final-60/verification.json` (`idleGapToSupport = 0.0`, `landingGapToSupport = 0.0`, `drumLandingX = 27.3303`)
- `120 Hz`: `Evidence/25d/T08G/final-120/verification.json` (`idleGapToSupport = 0.0`, `landingGapToSupport = 0.0`)
- **Capturas (60 Hz):** [pouso no tambor](./final-60/attempt-0-drum-land.png) e [repouso final no tambor](./final-60/attempt-0-drum-idle.png).
