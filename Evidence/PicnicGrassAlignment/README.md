# Grama assentada nas superfícies — candidata para revisão

Pedido de 25/11/2026: corrigir a grama deslocada do chão, conforme captura humana.

As faixas anteriores eram sprites retos apoiados somente na altura central.
Agora cada faixa tem uma malha subdividida a cada 13 pixels, com a base acompanhando
`PicnicLevelData.SurfaceY` em toda a largura. Os UVs preservam a textura existente.
A margem transparente inferior permanece embutida no chão; a fileira frontal foi
recuada para dentro do topo. Física, curvas do terreno e distribuição dos canteiros
permanecem iguais. Nenhuma arte raster foi gerada ou repintada nesta correção.

## Evidências

- [Comparação de subida e descida, antes à esquerda](comparison-slopes.png).
- `before/` e `after/`: 12 enquadramentos gerais e 9 posições em três colinas.
- `unity-after.log`: compilação e captura geral Unity com saída 0.
- `unity-hills-after.log`: `GRASS_REVIEW_OK captures=9`, saída 0.
- `validation-manifest.json`: hashes dos fontes e versão do Editor.

Capturas reais estáticas da câmera Unity, em cópia isolada
`Build/PicnicVisualAuditProject`, versão 6000.3.28f1. A comparação contém somente
recorte, redimensionamento e rótulos; os PNGs individuais não foram retocados.
Não foi validado gameplay contínuo, WebGL ou desempenho nesta microentrega.
A aparência aguarda revisão humana; não implica aprovação da fase.

## Reprodução

O runtime reconstrói a cobertura ao iniciar Play. Para atualizar uma prévia antiga
na aba Scene, fora do Play Mode, usar **YunaLegend → Fase 5 - Um Dia no Jardim →
Ver Cena Toda no Editor**. Não é necessário reconstruir ou salvar a cena.

O harness `GrassSurfaceReview.cs` fica fora de `Assets`. Para repetir a inspeção,
copiá-lo para `Assets/Editor` somente na cópia isolada e executar Unity em batch
com `-executeMethod GrassSurfaceReview.Capture -grassReview after`.

Branch da correção: `fix/picnic-editor-playmode`. Sem commit ou push nesta entrega.
O Editor do usuário foi preservado. Nenhum processo batch desta correção ficou aberto.
As texturas `meadow-lawn-top/wall` preparadas pelo Antigravity foram preservadas;
esta correção usa a arte que já estava aplicada, sem presumir aprovação dessas alternativas.
