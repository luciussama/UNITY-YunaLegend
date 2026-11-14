# T08H — Contrato de apoio antes da arte (06 Almofadão + 07 Gavetas)

14/11/2026. Sessão `2026-11-14-antigravity-06-07-visual`, registro 19.
Este contrato define **onde os pés pousam** antes da criação das artes das plataformas `06` e `07`.

## 06 — Almofadão de Veludo Roxo (`06_Almofadao_Veludo_Roxo`)
- **Referência:** Marcador 6 (`x = 31,1271 m`, `TopY = -4,23 m`, `width = 2,93 m`, `height = 0,95 m`) e observação do jogo legado (`step_202_shot_3.png`: almofada roxa capitonê com botões dourados e pingentes nos cantos sobre suporte alto fino de madeira).
- **Onde os pés pousam:** exclusivamente sobre o **plano superior acolchoado do almofadão roxo**, excluindo os pingentes e bordas caídas laterais.
- **Apoio físico:** `BoxCollider` único ajustado à largura útil do tampo acolchoado, preservando `PlatformSupportProfile`, física 3D e `pinVisibleSole`.

## 07 — Gavetas Abertas do Armário (`07_Gavetas_Abertas_Armario`)
- **Referência:** Marcador 7 (`x = 35,1271 m`, `TopY = -3,91 m`, `width = 2,86 m`, `height = 0,80 m`) e observação do jogo legado (`step_202_shot_3.png`: armário/cômoda alta de madeira escura com gavetas abertas em escadinha e tampo plano superior).
- **Onde os pés pousam:** exclusivamente sobre o **tampo superior plano do armário**, acima da moldura frontal e excluindo as gavetas abertas inferiores.
- **Apoio físico:** `BoxCollider` único ajustado à largura útil do tampo superior, preservando `PlatformSupportProfile`, física 3D e `pinVisibleSole`.

## Escopo desta entrega
- Apenas **contratos de apoio + artes conceituais 2,5D** (`cushion-06-v1.png` e `drawers-07-v1.png`) para aprovação humana antes da integração conjunta em cena e build.
