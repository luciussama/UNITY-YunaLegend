# T05.4 — especificação determinística e prompts das camadas de fundo

Sessão `2026-11-13-copilot-t053-b29a5ec7`. Candidata T05.3 aprovada em 13/11/2026; esta é a terceira entrega da rodada em ondas. **Nada aqui foi gerado ou aprovado**; são especificações para geração externa pelo usuário e integração por mim.

## 1. Enquadramento real medido no projeto

| Grandeza | Valor | Origem |
| --- | --- | --- |
| Resolução de avaliação | 1280 × 720 | contrato T02/T03 |
| Escala no plano da Yuna (z0) | **96 px/m** | `Evidence/25d/T02/CONTRATOS-TECNICOS.md` |
| Área visível | **13,3333 × 7,5 m** | 1280/96 e 720/96 |
| Câmera | perspectiva 45° (projeção A) | T02.2a/T02.2b |
| Travessia do piloto | ≈ **15,9 m** | urso 2,0176 + vão 2,4908 + livros 1,7265 + vão 1,25 + móvel 8,4 |
| PPU dos sprites de plataforma | 100 | `.meta` de urso/livros/móvel |
| Cor do luar direcional | `#6175B3`, intensidade 0,1, 6570 K | cena `DarkRoom_Gameplay` |
| Luz da janela | `#A8D1FF`, intensidade 3,6 | cena `DarkRoom_Gameplay` |
| Azul da Nanda (não pintar) | `#57A1FF` → `#407AF0` | `NandaAccompaniment.cs` |

## 2. Canvas e registro (contrato rígido)

Cada camada é **um PNG separado**, não uma tira de sprites.

- Tamanho final **exato: 2048 × 1024 px** (sRGB, 8 bits, RGBA).
- Escala de autoria: **96 px por metro** → o canvas cobre **21,3333 × 10,6667 m**.
- Mapa de mundo (usar sempre este):
  - coluna `x_px = (x_m + 10,6667) × 96`
  - linha `y_px = (9,6667 − y_m) × 96` (contando do topo)
- **Linha de piso obrigatória: 96 px acima da borda inferior**, ou seja, linha `y_px = 928`. Toda base de móvel de fundo toca essa linha; nada flutua.
- Centro da travessia = `x_m = 0`. Urso começa em ≈ `x_m = −8,0`; móvel termina em ≈ `x_m = +7,9`.
- Se a ferramenta não aceitar 2048×1024, gere no canvas 2:1 mais largo disponível e **reduza/amplie com Lanczos para 2048×1024**; a composição deve seguir as proporções abaixo, não o tamanho bruto.

### Parallax declarado (implementação minha, não desenhar)

| Camada | Fator | Cobertura exigida | Resultado |
| --- | --- | --- | --- |
| L0 fundo | 0,35 | 13,33 + 15,9×0,35 = **18,90 m** | 21,33 m ✓ sobra 2,4 m |
| L1 meio | 0,45 | 13,33 + 15,9×0,45 = **20,49 m** | 21,33 m ✓ sobra 0,8 m |
| L2 frente | fixa | elementos só nas pontas | não precisa cobrir o corredor |

## 3. Composição por camada (alvos em % da largura/altura)

### L0 — `layer0-far.png` (opaca, sem alpha)

- Parede do quarto em índigo/violeta, papel de parede discreto, rodapé de madeira sobre a linha de piso.
- **Janela**: abertura entre **18% e 39% da largura** (≈ `x_m −6,8 … −2,4`), topo em **12%** da altura, peitoril em **68%** (acima da linha de piso, sem virar degrau). Moldura de madeira + cortina.
- **Lua/ céu noturno** visível pela janela: azul profundo com halo frio; o luar entra em **cone suave para a direita e para baixo**, esmaecendo antes de `x = 0`.
- **Poça de luar no piso** entre `x_m −6,0 … −1,0`, largura máxima 5 m, opacidade baixa; **não** formar superfície horizontal clara com borda nítida (não pode parecer plataforma).
- Piso de tábuas quentes e escurecidas, com tapete listrado só na faixa `x_m +2 … +7` (o móvel cobre parte).
- **Proibido nesta camada:** personagens, urso, livros, móvel, qualquer objeto de apoio, texto, moldura de imagem.

### L1 — `layer1-mid.png` (RGBA, fundo transparente)

- Silhuetas de mobília **distante**, atrás das plataformas, todas com base **na linha de piso (928)**:
  - estante baixa com livros entre **x_m −10,2 … −8,6**;
  - cadeira/banquinho entre **x_m +0,2 … +1,4** (topo ≤ 1,0 m);
  - caixa de brinquedos entre **x_m +9,0 … +10,4**.
- **Corredor livre:** entre `x_m −8,6 … +0,2` e `+1,4 … +9,0` nada pintado acima da linha de piso, para não competir com urso/livros/móvel.
- Topo de qualquer silhueta **não pode ter superfície plana maior que 0,6 m** de largura (evita leitura de plataforma falsa).
- Tratamento atmosférico: 35–45% mais escuro que L0, contraste reduzido, borda superior com pincelada; rim light frio **no lado esquerdo** (vem da janela).
- **Proibido:** objetos de apoio do piloto, personagens, texto.

### L2 — `layer2-near.png` (RGBA, fundo transparente)

- Apenas enquadramento de primeiro plano, **nas pontas**, nunca no corredor central:
  - cortina/pano desfocado ocupando **0–12%** da largura, do topo até a linha de piso;
  - encosto de poltrona/pufe desfocado em **88–100%** da largura, altura até 55%.
- Opacidade máxima **0,85**; nada opaco no meio; silhueta escura (quase preta com azul), sem detalhe fino.
- Serve para profundidade; **não** pode cobrir a Yuna nem sugerir apoio.

Única camada que não pana: seus elementos ficam ancorados nas pontas do mundo.

## 4. Paleta (usar exatamente estes hex)

| Papel | Hex |
| --- | --- |
| Índigo profundo (sombra) | `#1E2547` |
| Parede principal | `#2A3160` |
| Violeta de sombra | `#3B2E52` |
| Luar frio | `#A8D1FF` |
| Núcleo do luar | `#D6E9FF` |
| Madeira quente (rodapé/móveis) | `#8A5A3B` |
| Madeira clara | `#C9A06A` |
| Creme papel | `#F2E3C6` |
| Carmim de acento | `#8E2B3A` |
| Latão discreto | `#B08A4E` |

Não tingir tudo de sépia; não clarear o quarto inteiro; não usar preto puro nas sombras.

## 5. Estilo (bloco comum aos três prompts)

Assinatura obrigatória: **ilustração de livro infantil pintada**, guache/acrílica com pincelada visível, bordas trabalhadas, volumes suaves, materiais táteis (madeira riscada, tecido com trama, papel envelhecido). Luz integrada à pintura. Referência de atmosfera: `Evidence/25d/T01/composition-v2.png` e a identidade do menu `Assets/Art/UI/Title_Background.png`.

Proibido: render 3D/plástico, pixel art, vetor chapado, anime genérico, fotorrealismo, contorno preto uniforme, filtro sépia, HDR estourado, lente olho-de-peixe.

## 6. Prompts prontos

> Gere cada um separadamente. Se a ferramenta aceitar imagem de referência, use `composition-v2.png` (atmosfera) e, para L1/L2, use o **L0 já gerado** como referência de registro.

### Prompt L0 — fundo

```
Children's picture-book gouache painting, night bedroom interior for a 2.5D platformer background, wide 2:1 canvas.
Composition: back wall of a cozy child's bedroom in deep indigo (#1E2547) and violet (#3B2E52), subtle damask wallpaper pattern with soft painted brush texture, warm wooden baseboard (#8A5A3B) running exactly along a horizontal floor line at 9.4% of the image height measured from the bottom.
A tall window occupies the horizontal band from 18% to 39% of the width: wooden frame (#8A5A3B, #C9A06A), thin brass latch (#B08A4E), parted warm cream curtains (#F2E3C6) with visible fabric weave and soft painted folds. Outside the window: deep night sky, a soft moon with a cool halo (#D6E9FF core, #A8D1FF glow), distant treeline silhouette.
Moonlight cone: cold pale blue (#A8D1FF) volumetric light entering from the upper-left window, travelling down and to the right, soft edges, fading before the horizontal center of the image. A very soft, low-contrast pool of moonlight on the floorboards between 22% and 52% of the width, with no hard rim, never reading as a flat ledge.
Floor: warm old wooden floorboards (#8A5A3B, #C9A06A) receding gently, a faded cream-and-carmine (#F2E3C6, #8E2B3A) striped rug visible only on the right half. Dust motes only faintly suggested.
Style: tactile children's-book illustration, visible brush strokes, soft rounded volumes, gentle edges, warm materials against cool night light. No characters, no toys, no furniture, no bear, no books, no cabinet, no platforms, no ledges, no text, no watermark, no border, no 3D render, no plastic sheen, no pixel art, no sepia filter.
Output: single flat painting, edge-to-edge, 2:1 aspect ratio.
```

### Prompt L1 — silhuetas de meio

```
Children's picture-book gouache painting, MID-GROUND layer for a 2.5D platformer, transparent background, wide 2:1 canvas.
Draw ONLY three distant background furniture silhouettes, all standing exactly on one horizontal line placed at 9.4% of the image height measured from the BOTTOM edge. Nothing above that line except the objects described; everything else fully transparent.
1) A low bookshelf with a few books between 3% and 20% of the width, top at about 62% of the image height.
2) A small wooden stool / chair between 51% and 56% of the width, seat top no higher than 68% of image height.
3) A rounded toy chest between 92% and 100% of the width, top no higher than 55% of image height.
No flat top surface wider than 5% of image width (must never look like a platform). Atmospheric depth: 40% darker and lower contrast than a lit night room, indigo (#1E2547) and violet (#3B2E52) masses, warm wood accents (#8A5A3B) only where light grazes, cool rim light (#A8D1FF) on LEFT edges only, soft painted brush edges, slightly blurred silhouette contour.
Style: tactile painted children's-book illustration, gouache, soft volumes. Fully transparent empty space elsewhere, including the entire lower band below the floor line and the central corridor.
No characters, no bear, no pile of books, no cabinet, no text, no watermark, no border, no 3D render, no plastic, no pixel art.
Output: RGBA PNG with real alpha, 2:1 aspect ratio.
```

### Prompt L2 — primeiro plano

```
Children's picture-book gouache painting, FOREGROUND framing layer for a 2.5D platformer, transparent background, wide 2:1 canvas.
Draw ONLY two dark out-of-focus framing elements, hard-silhouette, almost black-blue (#141A33) with a faint cool #A8D1FF edge:
1) A heavy curtain / drape occupying the left edge from 0% to 12% of the width, from the top edge down to 9.4% of the height measured from the BOTTOM.
2) The rounded back of an armchair / pouf occupying the right edge from 88% to 100% of the width, rising to about 55% of the image height.
Everything between 14% and 86% of the width is fully transparent. Maximum opacity of painted pixels 85%, soft blurred painted edges, no fine detail, no highlights brighter than mid-tone.
No characters, no platforms, no furniture with visible top surfaces, no text, no watermark, no border, no 3D render, no plastic, no pixel art.
Output: RGBA PNG with real alpha, 2:1 aspect ratio.
```

### Negative prompt (aplicar aos três, se a ferramenta aceitar)

```
text, letters, logo, watermark, signature, frame, border, character, girl, fairy, bear, teddy, books, cabinet, dresser, platform, ledge, step, hard geometric edge, straight cut, 3D render, plastic, glossy, octane, unreal engine, pixel art, vector flat, anime, sepia, HDR bloom, lens flare, fisheye, oversaturated
```

## 7. Entrega e verificação

Coloque os três arquivos em `Evidence/25d/T05.4/source/` com estes nomes exatos:

```
layer0-far.png
layer1-mid.png
layer2-near.png
```

Eu verifico e registro, antes de importar para `Assets/Art/Illustrated25D/Environment/Room/`:

1. dimensões exatas 2048×1024 e modo RGBA;
2. linha de piso detectada em `y_px = 928 ± 6`;
3. bbox de alpha da L1/L2 (nada no corredor central `x_px 686–1626`);
4. ausência de plataformas/personagens (inspeção visual + relatório);
5. SHA-256 dos três arquivos no manifesto.

Se alguma medida falhar, devolvo o ajuste exato antes de qualquer importação — nada é aceito por aproximação.

## 8. O que ainda falta depois deste passo

Rig de parallax e iluminação na Unity, captura em escala real de gameplay, prova 30/60/120 Hz e revisão humana da T05.4. A T05 agregada e a T06 só liberam depois disso.
