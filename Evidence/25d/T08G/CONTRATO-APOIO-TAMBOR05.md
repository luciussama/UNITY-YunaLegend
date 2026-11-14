# Tamborito de Marcha (05) — contrato de apoio integrado

14/11/2026. Sessão `2026-11-14-antigravity-tambor05-cena`, registro 18 (`t08g/tambor05-cena-v1`).

## Medição na pintura aprovada (`drum-05-v1.png`, 1024×1024 RGBA)
- **Linha de corrida/pouso (`LandingPixelY`):** `y = 265` (faixa plana central-inferior da pele de pergaminho, ~95 px acima do aro frontal em `y ≈ 360` e ~115 px abaixo do aro traseiro em `y ≈ 150`).
- **Base (`FeetPixelY`):** `y = 930` (borda inferior do aro de baixo, assentada no piso `-9,10 m`).
- **Largura útil da pele (`LipLeftPixelX..LipRightPixelX`):** `x = 215..815` (`600 px`, estritamente dentro da borda interna dos aros em `195..830`).
- **Centro do conteúdo (`CentrePixelX`):** `x = 510,5` (`ContentPixelWidth = 750 px`).

## Escala, posição e apoio físico (`Drum05Platform.cs`)
- **Escala uniforme:** `146,1538 px/m` (`TargetHeight = 4,55 m`, `FloorY = -9,10 m`, `TopY = -4,55 m`).
- **Posição:** `x = 28,25 m`; largura visual `5,1316 m`; apoio próprio `4,1053 m × 2,00 m` (`offsetX = +0,0308 m`).
- **Vão e degrau após `T08E` (blocos ABC em `x = 23,1271 m`, `TopY = -5,199 m`):**
  - Vão entre apoios: `2,4020 m`
  - Degrau vertical: `+0,6489 m`
- **Contato:** `pinVisibleSole = true` em `DresserFootAlignment` (`supportIndex = 5`), pintura em `sortingOrder = -1` (atrás de Yuna em ordem `0`).
