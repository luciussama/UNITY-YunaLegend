# T03 — revisão conjunta (aceita — terceira candidata)

Estado: **aceita pelo usuário em 13/11/2026**. A primeira revisão encontrou o
Idle final vazio em `t=13,5 s`; a segunda encontrou o tropeço esquerdo desenhado
para a direita. As duas causas foram corrigidas e as provas refeitas. T03.1–T03.4
continuam aprovadas individualmente. T03 aceita; **T04/T05 liberadas** e T06
continua dependente das duas.

## Links para avaliar

- [Gameplay normal](../review/game-normal.gif)
- [Gameplay lento](../review/game-slow.gif)
- [Detalhe normal](../review/detail-normal.gif)
- [Detalhe lento](../review/detail-slow.gif)
- [Folha de gameplay](../review/game-review-sheet.png)
- [Folha de detalhe](../review/detail-review-sheet.png)

## Checklist

- [x] Em t=5,0 s e t=10,0 s aparece **uma única** Yuna no pouso.
- [x] A passagem corrida → salto → pouso → corrida não tem pop ou salto de
      posição perceptível.
- [x] A passagem corrida → tropeço → recuperação → corrida mantém contato e
      ritmo; o tropeço não parece apenas uma pose colada.
- [x] Em `t=6,20 s` o tropeço está voltado para a direita e em `t=11,23 s` está
      voltado para a esquerda, coerente com a corrida de cada trecho.
- [x] A virada em 7,4 s mantém a leitura do corpo e do cabelo.
- [x] O idle final em 13,5 s aparece voltado para a esquerda, sem desaparecer e
      sem herdar pose ou deslocamento da corrida.
- [x] A compressão HQ mantém detalhe suficiente no gameplay e na ampliação.
- [x] O conjunto pode ser aceito como apresentação da T03, ou há ajustes
      específicos a pedir.

## Resultado automático

| Execução | Quadros | Conflitos pós-resolvedor | Fonte máxima | Orçamento |
| --- | ---: | ---: | ---: | --- |
| 30 Hz | 420 | 0 | 1 | 16,6389 MiB |
| 60 Hz | 420 | 0 | 1 | 16,6389 MiB |
| 120 Hz | 420 | 0 | 1 | 16,6389 MiB |
| Baseline 30 Hz | 420 | 0 | 1 | 61,0407 MiB |

PSNR mínimo: 58,3305 dB (gameplay) e 43,5257 dB (detalhe); alpha alterado 0,0000.

## Correção aplicada após a primeira revisão

- O root gira para `Y=±90°` durante a corrida. O Idle não compensava essa
  rotação e ficava de perfil para a câmera; o bounds final tinha largura `0,00`.
- `YunaIdleIllustration` agora compensa a rotação do root e espelha o perfil
  conforme `RunDirection`.
- O harness exige `sawIdleFacingCamera=true` após a parada. As três execuções
  finais passam e a folha final mostra a Yuna no idle.
- A candidata defeituosa está preservada em
  [attempt-03-idle-edge-on](../attempt-03-idle-edge-on/README.md).

## Resposta literal — primeira revisão

> "Me explica por que o 'Idle final' está sem a menina?"

O Idle estava ativo, mas sem compensar a rotação Y de `±90°` do root; ficava de
perfil para a câmera. A correção e a nova prova estão descritas acima.

## Resposta literal — segunda revisão

> "O 'tropeço esquerda' é um evento da sequência de teste da T03, não do gameplay
> integrado. [...] O shader não implementa o caminho de flip usado pelo
> `SpriteRenderer`, então `flipX=true` não espelha a imagem. [...] não é
> intencional e a T03 agregada não deve ser aceita assim."

Resumo da resposta: o shader customizado do Stumble ignorava o flip, o harness
só verificava pose/visibilidade e o rótulo do tropeço direito usava um instante
depois do fim do evento. As três correções estão abaixo.

## Correção aplicada após a segunda revisão

- `StumblePainted.shader` passou a incluir `Core2D.hlsl` e aplicar
  `UnityFlipSprite(..., unity_SpriteProps.xy)` no vértice, consumindo o flip do
  `SpriteRenderer`.
- O harness exige `stumbleFlipHonored=true` por prova renderizada espelhada e
  `sawStumbleFacingRight/Left=true` nos dois eventos. Diagnóstico:
  `pose=2 asymmetry=8052802 mirrorError=25 tolerance=1006600`.
- O rótulo do tropeço direito passou de `6,50 s` para `6,20 s`; o esquerdo usa
  `11,23 s`.
- A candidata defeituosa e o quadro antigo do tropeço esquerdo estão em
  [attempt-04-stumble-unflipped](../attempt-04-stumble-unflipped/README.md).

## Resposta literal — terceira revisão (aceite)

> "aprovado. Qual é o próximo passo?"

Aceite humano registrado em 13/11/2026 sobre a terceira candidata. A T03
agregada está aceita; T04 e T05 podem começar e T06 continua bloqueada até as
duas serem aceitas.
