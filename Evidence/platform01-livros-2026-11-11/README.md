# Plataforma 01 — pilha diagonal de livros

11/11/2026 · sessão `2026-11-11-dsh-livros-plataforma01` · DeepSeek Harness (deepseek-flash).

Segunda plataforma modelada sob a **regra obrigatória** (diário, entrada **305**; `AGENTS.md`): a superfície de contato pertence ao objeto; colisor, salto e câmera se adaptam a ela.

## Superfície de contato própria

- `PlatformSpec.contact` → `PlatformContactSurface` (`offsetX`, `offsetY`, `tiltDegrees`, `width`, `depth`).
- `DarkRoomPlatformMarker.GetTopCenterWorld()` / `GetContactRotation()` leem o tampo real.
- Colisor: caixa cheia quando o contato é o padrão; **laje fina rotacionada** quando há inclinação/deslocamento.
- Teste compara o **centro da face superior** da capa do livro de cima com o topo do colisor (não o AABB — que cresce com a inclinação).

Valores: `offsetX = 0,24 m`, `tilt = −4°`, `1,85 × 1,15 m`.

## Modelo e arte

Cinco volumes empilhados com deslocamento (−0,20 → +0,245 m) e inclinação (−0,8° → −4°) progressivos.

- **Capas pastéis** de couro (taupe, oliva suave, rosa empoeirado, castanho, ocre).
- **Lombadas pintadas** por atlas (`T_Book_Spines`): título gravado em ouro, filetes duplos, ornamentos de canto e costura pontilhada — com tipografia de bitmap própria (NURSERY TALES, MYTHS, FAIRY TALES, EASTCERT & DOERS, SORTMENT).
- **Miolo de páginas** com textura e normal map próprios (`T_Book_Pages`), visível nas laterais.
- Fita vermelha na lateral direita e borla dourada na base.
- Texturas `T_Book_Leather` e `T_Book_Spines`; materiais `M_Book_Leather_*` e `M_Book_Spines`.

## Evidências

- `06-books-front.png`, `07-books-three-quarter.png`, `08-books-top.png` — renders de inspeção com luz neutra temporária.
- `editmode.xml` (26/26) e `playmode.xml` (21/21).

## Limitações

- A tipografia é bitmap 5×7 ampliada: legível, porém com aspecto de letreiro, não de tipografia serifada pintada.
- A inclinação de 4° lê a diagonal sem comprometer o pouso; a arte conceitual é mais inclinada.
- Sem ornamentos nas capas (só na lombada) e a borla é discreta no enquadramento de jogo.

## Validação

- **EditMode 26/26** e **PlayMode 21/21** sobre a cena final, confirmando que o percurso é jogável com a superfície inclinada e deslocada.
