# T08J — Contrato de apoio antes da arte (08 Caixa de Música + 09 Castelinho)

14/11/2026. Sessão `2026-11-14-antigravity-08-09-visual`, registro 21.
Este contrato define **onde os pés pousam** e a **cadência de salto (ponto de tensão)** antes da pintura/integração de `08` e `09`.

## 08 — Caixa de Música da Bailarina (`08_Caixa_Musica_Bailarina`)
- **Referência:** Catálogo (`index = 8`, `Caixa de Musica da Bailarina`: *"Caixa aberta em madeira rosada e engrenagens douradas"*) e jogo legado (`music_box` com `support: 'stand'`, pernas torneadas de madeira nobre até o piso `FloorY = -9,10 m`).
- **Altura e posição planejadas:**
  - `TopY = -3,55 m` (`Height = 5,55 m`, degrau de subida `+0,36 m` em relação às Gavetas `07` em `-3,91 m`).
  - Vão `07 → 08`: **~1,95 m** (salto normal de subida para manter fluidez após as gavetas).
- **Onde os pés pousam:** exclusivamente sobre o **tampo frontal amplo da mesa de mogno**, com a caixa de música aberta posicionada na metade traseira da mesa (atrás de Yuna em profundidade 2,5D).
- **Apoio físico:** `BoxCollider` único ajustado à largura útil do tampo frontal (`~3,10 m`), preservando `PlatformSupportProfile`, física 3D e `pinVisibleSole`.

## 09 — Castelinho de Blocos (`09_Castelinho_Blocos`) + Ponto de Tensão
- **Referência:** Catálogo (`index = 9`, `Castelinho de Blocos`: *"Fortaleza com blocos de madeira e torres azuis pontiagudas"*) e jogo legado (`block_castle` apoiado no piso `FloorY = -9,10 m`, com deck central entre torres azuis).
- **Altura e posição planejadas (Ponto de Tensão):**
  - `TopY = -3,95 m` (`Height = 5,15 m`, descida de `-0,40 m` em relação à Caixa de Música `08`).
  - Vão `08 → 09`: **~2,75 m** entre a borda direita de apoio de `08` e a borda esquerda de apoio de `09` (alcance máximo em salto plano/descida com `RunSpeed = 3,6 m/s`, `JumpHeight = 2,4 m`, `g = 28 m/s²` é `~3,10 m` no ar + raio/coyote).
  - **Objetivo de design:** quebrar o ritmo robótico (`1,62 m` / `1,95 m`), exigindo que Yuna corra quase até a extremidade direita da Caixa de Música antes de saltar — um pulo mais arriscado, porém 100% confiável em `30 / 60 / 120 Hz`.
- **Onde os pés pousam:** exclusivamente sobre o **deck/ponte plana de madeira** do castelinho (`y ≈ 645 px`), com as **duas torres frontais** separadas em camada à frente (`block-castle-09-front-towers.png`, `sortingOrder` acima de Yuna) para que Yuna passe brevemente por trás da torre esquerda ao entrar no patamar.
- **Apoio físico:** `BoxCollider` único ajustado à largura útil do patamar central (`~3,40 m`), preservando `PlatformSupportProfile`, física 3D e `pinVisibleSole`.

## Escopo desta entrega
- Diagnóstico do fundo do quarto (`ANALISE-FUNDO-QUARTO.md`), contrato de apoio (`CONTRATO-APOIO-08-09.md`) e candidatas visuais 2,5D RGBA (`music-box-08-v1.png` e `block-castle-09-v1.png`) para aprovação humana antes da integração em cena e build.
