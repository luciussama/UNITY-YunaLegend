# Tentativa 03 — Idle final de perfil

Status: **reprovada na revisão humana**, preservada em 12/11/2026.

## Sintoma

Em `t=13,50 s`, a folha de revisão mostrava somente o chão. Os quadros 377–419
também estavam sem a personagem, embora a prova registrasse
`visibleSource=Idle`, `idleVisible=true` e uma única fonte visível.

## Causa

O controlador gira o root para `Y=+90°` ao correr à direita e `Y=-90°` ao correr
à esquerda. Run e Stumble compensam essa rotação no próprio transform; o Idle
não compensava. Depois da corrida à esquerda, o plano do Idle ficou de perfil
para a câmera.

O diagnóstico runtime registrou:

```text
idleActiveInHierarchy=True
rendererEnabled=True
frustum=True
root=(-1.56, 0.08, 0.00)
boundsSize=(0.00, 1.46, 0.98)
mainTexture=Yuna_Profile_Runtime_v1
```

A largura zero no bounds revela o plano visto de lado. A verificação antiga
aceitava `renderer.enabled`, sem provar que o Idle estava projetado na câmera.

## Conteúdo preservado

- `final-30-joint-proof.json`
- `final-60-joint-proof.json`
- `final-120-joint-proof.json`
- `baseline-30-joint-proof.json`
- `game-review-sheet.png`
- `detail-review-sheet.png`

Diagnóstico completo: `Build/25d/T03Diag-30/joint-proof.json` e
`Build/25d/t03-diag-idle-30.log`.

## Correção

`YunaIdleIllustration` passou a compensar a rotação Y do root e a espelhar o
perfil conforme `RunDirection`. O harness agora exige
`sawIdleFacingCamera=true`, com bounds projetados e posição dentro do viewport.
As provas finais corrigidas substituem os arquivos `final-*` e `baseline-30`;
esta tentativa permanece como histórico.
