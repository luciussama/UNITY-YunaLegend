# T05.4 v2 — painéis largos em escala doméstica (prompts)

Decisão do usuário em 13/11/2026: a versão a 0,70 melhorou, mas o quarto ainda é grande demais. A correção é arte mais larga **na mesma escala**, não reescalar a arte atual.

## Por que três painéis

A câmera mostra **13,333 m × 7,5 m** (1280×720 a 96 px/m). Um único enquadramento de 16:9 pintado em escala doméstica não cobre esse campo com folga para parallax. Três painéis contínuos dão **40 m de parede**: cabe o campo visível de 13,333 m mais o deslocamento de parallax.

## Contrato de cada painel (idêntico nos três)

| Grandeza | Valor | Como desenhar |
| --- | --- | --- |
| Canvas | **1920 × 1080 px** | 16:9, sRGB |
| Escala de autoria | **144 px/m** | 1920/144 = 13,333 m de largura; 1080/144 = 7,5 m de altura |
| Linha de piso | **144 px acima da borda inferior** (linha `y = 936` contando do topo) | tábua de piso toca essa linha; nada abaixo dela além do próprio piso |
| Rodapé de madeira | **1,10 m = 158 px** acima da linha de piso | topo do rodapé em `y = 778` |
| Parede acima do rodapé | restante até o topo | tom índigo/violeta com padrão floral discreto |
| Rodapé do chão (skirting) | 0,20 m = 29 px | faixa de madeira encostada no piso |
| Luz | luar entrando pela janela do painel central | cone frio descendo para a direita; painéis laterais 20–30% mais escuros |

Objetos opcionais (usar tamanhos em metros, não proporções “artísticas”):

- **Janela** (somente painel central): **2,40 m de largura (346 px) × 1,40 m de altura (202 px)**; peitoril **1,05 m acima do piso (151 px)**; moldura de madeira 0,09 m.
- **Estante baixa**: 1,20 m de largura × 0,90 m de altura; encostada na parede, base na linha de piso.
- **Banquinho**: 0,42 m de largura × 0,48 m de altura.
- **Baú**: 0,90 m de largura × 0,55 m de altura.
- **Quadros na parede**: no máximo 0,35 × 0,45 m.
- **Bandeirinhas**: no máximo 0,18 m de altura cada.

Regra de ouro: **nada de proporção “de desenho”** — se o enunciado der metros, respeite-os. Um objeto de 0,5 m deve ocupar ~72 px de altura no canvas.

## Continuidade entre painéis (crítico)

Os três formam uma parede contínua de 40 m: `room-left` | `room-center` | `room-right`.

- O padrão do papel de parede deve atravessar a emenda sem salto: mesmo motivo, mesma escala, mesma fase.
- A linha de piso, o rodapé, o topo do rodapé e a faixa de skirting devem estar **na mesma altura** nos três e tocar as bordas esquerda/direita sem margem.
- As tábuas do piso continuam na mesma direção e escala.
- Não desenhar moldura, vinheta ou borda no perímetro; a imagem vai até a borda.

## Prompts

> Fluxo: gere primeiro o **central** (tem a janela e define a luz). Depois use-o como imagem de referência para gerar **esquerda** e **direita**, pedindo explicitamente “continue this exact wall to the left/right, same scale and pattern phase”.

### Central (`room-center.png`)

```
Children's picture-book gouache painting, one panel of a continuous bedroom wall, exact canvas 1920x1080 (16:9), authored at exactly 144 pixels per metre.

SCALE — obey in pixels:
- Floor line: a horizontal line exactly 144 px above the BOTTOM edge (at y=936 counting from the top). Wooden floorboards fill only the 144 px below it.
- Baseboard / wainscot: warm wood (#8A5A3B, #C9A06A) exactly 158 px tall, sitting ON the floor line, so its top edge is at y=778. Above it, wall.
- Skirting strip touching the floor: 29 px.
- Wall above the baseboard: deep indigo (#1E2547) with violet shadow (#3B2E52) and a discreet painted floral damask, motif no larger than 40 px.
- Window: exactly 346 px wide and 202 px tall, sill 151 px above the floor line, centred in the panel. Arched top, wooden frame 13 px, brass latch (#B08A4E), cream curtains (#F2E3C6) parted to both sides, real fabric weave. Outside: deep night sky, moon with cool halo (#D6E9FF core, #A8D1FF glow), distant tree silhouettes.
- Moonlight: cold (#A8D1FF) cone entering through the window, travelling down and to the RIGHT, soft edges, plus a very soft low-contrast pool on the floor with no hard rim.
- Wall paintings: at most two small frames, 50x65 px and 35x45 px, hung above the baseboard.
- Bunting: a short garland of 5 triangular flags, each 26 px tall, hanging in the upper left.

STYLE: tactile children's-book illustration, visible gouache brush strokes, soft rounded volumes, worked edges, integrated painted light. No 3D render, no plastic sheen, no pixel art, no vector flat, no sepia, no text, no watermark, no border, no vignette.
COMPOSITION: no characters, no furniture, no teddy bear, no pile of books, no platforms, no ledges.
Output: single flat painting, edge to edge, 1920x1080.
```

### Esquerda (`room-left.png`)

```
Continue the exact same bedroom wall as the reference image, to the LEFT. Same gouache style, same canvas 1920x1080, same 144 pixels per metre, same wall colour, same baseboard height (158 px above a floor line exactly 144 px above the bottom edge), same floorboards, same skirting, and the damask pattern must continue seamlessly to the left edge so the two panels butt without a visible seam.

Content: plain wall with the wallpaper pattern continuing, a low bookshelf exactly 173 px wide and 130 px tall standing with its base on the floor line, and a small stool exactly 60 px wide and 69 px tall beside it. Two tiny framed pictures, no larger than 50x65 px. Light: this end of the room is 25% darker than the reference; no moon cone here, only a faint cool falloff from the right.

No window, no characters, no teddy bear, no pile of books, no platforms or ledges, no text, no watermark, no frame, no vignette. Output: single flat painting, edge to edge, 1920x1080.
```

### Direita (`room-right.png`)

```
Continue the exact same bedroom wall as the reference image, to the RIGHT. Same gouache style, same canvas 1920x1080, same 144 pixels per metre, same wall colour, same baseboard height (158 px above a floor line exactly 144 px above the bottom edge), same floorboards, same skirting, and the damask pattern must continue seamlessly to the right edge so the two panels butt without a visible seam.

Content: a wooden toy chest exactly 130 px wide and 79 px tall with its base on the floor line, and a soft striped rug start at the far right. Wall pattern continues; one tiny framed picture, at most 50x65 px. Light: 20% darker than the reference, with the faint tail of the moon cone entering from the left.

No window, no characters, no teddy bear, no pile of books, no platforms or ledges, no text, no watermark, no frame, no vignette. Output: single flat painting, edge to edge, 1920x1080.
```

### Negative prompt (aplicar aos três)

```
text, letters, logo, watermark, signature, picture frame, border, vignette, character, girl, fairy, bear, teddy, stack of books, cabinet, dresser, platform, ledge, step, hard geometric edge, straight cut, 3D render, plastic, glossy, octane, unreal engine, pixel art, vector flat, anime, sepia, HDR bloom, lens flare, fisheye
```

## Entrega

Salvar em `Evidence/25d/T05.4/source/`:

```
room-left.png
room-center.png
room-right.png
```

Verificação que eu farei antes de importar: 1920×1080 RGBA/RGB; linha de piso em `y = 936 ± 6` nos três; topo do rodapé em `y = 778 ± 8`; janela em 346×202 px no central; emendas sem salto (comparo a coluna de borda esquerda/direita em RGB); ausência de personagens/plataformas. Divergência volta com o ajuste exato.

## O que muda no rig

- Três painéis lado a lado = **40 m** de fundo; parallax volta a **0,35** (cabe com folga: preciso de 13,33 + 15,29×0,65 = 23,3 m).
- Importação a **144 px/m na arte**; na tela o fundo aparece a 96 px/m (redução de 0,67 → mais nítido que hoje).
- As camadas de meio e primeiro plano atuais ficam como estão até a nova arte ser aprovada.
