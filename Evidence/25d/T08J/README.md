# T08J — Análise do Fundo do Quarto + Candidatas Visuais 2,5D (08 e 09)

14/11/2026. Sessão `2026-11-14-antigravity-08-09-visual`, registro 21 (`t08j/musica08-castelo09-visual-v1`).
Estado: **candidatas entregues para revisão humana** (antes da correção do fundo e integração conjunta na cena Unity).

## 1. Análise crítica do fundo do quarto após o Tambor (05)
- Ver relatório completo em [ANALISE-FUNDO-QUARTO.md](./ANALISE-FUNDO-QUARTO.md).
- Resumo: `IllustratedPreview.cs` instala **apenas 1 painel** (`room-panorama-v4.png`, `32,0 m` de largura, `travelFraction = 0,35`), ancorado nas 3 primeiras plataformas. A borda direita do quarto termina exatamente na altura do **Tambor (05)** (`x ≈ 28 m`), deixando o fundo vazio em `06` (`33,80 m`) e `07` (`38,80 m`). Além disso, `PenumbraT054.shader` fixa a janela da lua por UV (`0.564, 0.558`), então repetir o mesmo painel clonaria a janela.

## 2. Contrato de apoio e ponto de tensão (08 e 09)
- Ver contrato completo em [CONTRATO-APOIO-08-09.md](./CONTRATO-APOIO-08-09.md).
- **08 — Caixa de Música da Bailarina:** `Assets/Art/Illustrated25D/Environment/MusicBox/music-box-08-v1.png` (apoio no tampo frontal plano, com suporte torneado até o piso `FloorY = -9,10 m`).
- **09 — Castelinho de Blocos:** `Assets/Art/Illustrated25D/Environment/Castle/block-castle-09-v1.png` (apoio no patamar plano central entre as torres azuis).
- **Ponto de tensão (`08 → 09`):** vão ampliado para `~2,75 m` com descida de `-0,40 m` (`-3,55 m → -3,95 m`), quebrando a cadência robótica e exigindo salto próximo à borda direita da Caixa de Música.
