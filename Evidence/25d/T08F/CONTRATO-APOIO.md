# T08F — Contrato de apoio antes da arte (Tamborito de Marcha 05)

14/11/2026. Sessão `2026-11-14-antigravity-tambor05`, registro 17.
Este contrato define **onde os pés pousam** antes da criação da arte do Tamborito de Marcha (`05_Tamborito_Marcha`).

## Referência na cena base e no catálogo
- **Cena:** `Assets/Scenes/DarkRoom_Gameplay.unity` (`Etapa1_Castelo_05_05_Tamborito_Marcha`, marcador `index: 5`).
- **Catálogo:** `DarkRoomPlatformCatalog.cs` — *"Tambor cilíndrico vermelho e branco com cordões"* (Fase 1: Subida Calma).
- **Posição do marcador base:** `x = 27,1271 m`, `y = -5,345 m`, `width = 2,7 m`, `depth = 2,7 m`, `height = 1,59 m` (topo base em `-4,55 m`; distância centro-a-centro de `4,00 m` após os blocos ABC em `x = 23,1271 m`).

## Superfície de apoio real (definida antes da pintura)
- **Silhueta:** tambor cilíndrico vertical com aros de madeira superior e inferior, cordões laterais em cruz e pele plana no topo.
- **Onde os pés pousam:** exclusivamente sobre a **pele plana superior** do tambor, excluindo os aros externos salientes e os cordões laterais.
- **Linha de pouso:** acima do rebordo/aro frontal, de modo que todo o aro frontal fique abaixo da sola desenhada (evitando que os pés entrem na borda do tambor, como validado em `T08E` com `pinVisibleSole`).
- **Apoio físico:** `BoxCollider` único ajustado à largura útil da pele plana (~75%–82% da largura visual do cilindro, a ser medida na imagem após aprovação), preservando `PlatformSupportProfile` e o controlador físico 3D.
- **Rota prevista a partir de `T08E`:** blocos ABC (`topo = -5,199 m`, `x ≈ 23,07 m`) → Tamborito de Marcha (`x ≈ 27,13 m`, topo alvo em torno de `-4,55 m` a `-4,35 m`, degrau `+0,65 m` a `+0,85 m`, vão `~2,0 m` dentro do alcance do salto de `2,4 m`).

## Escopo desta entrega
- Apenas **contrato de apoio + arte conceitual individual** para revisão humana.
- Sem integração em cena ou build antes da aprovação visual da peça.
