# Tentativa 04 — tropeço esquerdo sem espelhamento

Esta pasta preserva a segunda candidata da T03, reprovada antes de qualquer
aceite. Nela o tropeço esquerdo era renderizado com o desenho original voltado
para a direita.

## Causa

`Stumble_Painted.mat` usa o shader customizado
`YunaLegend/IllustratedStumbleCandidate`. O shader transformava os vértices sem
ler o flip do `SpriteRenderer`, então `painting.flipX=true` não espelhava a
imagem. Run e Jump usam o shader de sprite padrão e não tinham o defeito.

O harness da época verificava apenas `stumble.Pose` e visibilidade. Por isso as
provas terminavam com `success=true` apesar da orientação errada.

## Evidência preservada

- [Quadro do tropeço esquerdo em t=11,23 s](left-stumble-t11.23.png), capturado
  antes da correção: a personagem está claramente voltada para a direita.
- `final-30-joint-proof.json`, `final-60-joint-proof.json`,
  `final-120-joint-proof.json` e `baseline-30-joint-proof.json`: provas antigas,
  sem os campos `stumbleFlipHonored`, `sawStumbleFacingRight` e
  `sawStumbleFacingLeft`.
- [Folha de gameplay](game-review-sheet.png) e
  [folha de detalhe](detail-review-sheet.png): a folha antiga rotulava o
  tropeço direito em `6,50 s`, depois do evento terminar em `6,37 s`; o quadro
  mostrava corrida. Para o tropeço direito, o instante correto é `6,20 s`.

## Estado

Não é candidata. A correção está na terceira candidata, em
[../README.md](../README.md). A T03 agregada continua não aceita.
