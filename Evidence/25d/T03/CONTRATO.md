# T03 — contrato da revisão conjunta

Estado: terceira candidata em revisão humana; T03 agregada **não aceita**. Codex
(GPT-6.1), 13/11/2026, sessão `2026-11-13-codex-stumble-flipfix-9f4a`. A primeira
candidata foi reprovada porque o Idle final ficava de perfil para a câmera; a
segunda, porque o tropeço esquerdo continuava desenhado para a direita. As
tentativas preservadas estão em `attempt-03-idle-edge-on/` e
`attempt-04-stumble-unflipped/`.

## Escopo

Revisar o conjunto das apresentações já aprovadas individualmente — idle,
corrida, salto e tropeço — em uma sequência única, nos dois sentidos, e provar
que o conjunto cabe no orçamento residente de **32 MiB** sem alterar física,
controlador, Gameplay, câmera de produção ou PNGs aprovados.

Esta entrega é uma prova isolada de apresentação. Não integra o piloto, não
implementa o móvel cedendo, não altera o retry da Gameplay e não fecha a T03.

## Critérios de aceite

1. Exatamente uma fonte visual de Yuna por quadro, com prioridade
   **Stumble > Jump > Run > Idle**.
2. Nenhum quadro com duas fontes visíveis ou nenhuma fonte; o harness deve falhar
   automaticamente se isso ocorrer.
3. Depois da parada, o Idle deve estar ativo, projetado para a câmera e dentro do
   viewport; o harness falha se `sawIdleFacingCamera` não for verdadeiro.
4. Os dois tropeços devem renderizar na direção do evento: `flipX=false` no
   tropeço direito e `flipX=true` no esquerdo, com o material do Stumble
   comprovadamente espelhando a imagem renderizada.
5. Sequência determinística de 14 s com idle, corrida direita, salto, tropeço,
   virada, corrida esquerda, salto, tropeço, parada e idle final.
6. 420 quadros capturados por câmera em 30/60/120 Hz de simulação, com dois
   saltos, dois pousos e dois tropeços.
7. Total residente das cinco texturas da Yuna ≤ 32 MiB na medição declarada.
8. Fontes aprovadas byte a byte, colisor e escala do root inalterados.
9. Revisão visual humana das transições, pousos, virada, tropeço e qualidade da
   compressão antes de qualquer aceite agregado.

## Contrato técnico

O `YunaIllustrationVisibilityResolver` roda em `LateUpdate` com
`DefaultExecutionOrder(240)`, depois de Run (210), Jump (220) e Stumble (230).
Ele lê as poses ativas e aplica a prioridade, sem mover o root nem alterar a
física. `showIdle` é a entrada de integração: `true` usa Idle como fallback;
`false` usa Run. Na T06 esse campo deve ser ligado ao estado real de idle.

O harness chama `Resolve()` depois de atualizar os três componentes e mede
`visibleSources` em todo tick simulado. A prova falha se o valor não for 1. As
sobreposições *antes* do resolvedor são esperadas e registradas como diagnóstico
(179/358/716 quadros em 30/60/120 Hz), pois os componentes aprovados ainda
reivindicam a corrida; o que importa é o estado final.

O `YunaIdleIllustration` compensa a rotação Y do root e ajusta `flipX` por
`RunDirection`, como Run/Stumble já fazem. Após a parada, o harness também mede
o bounds projetado e a posição do centro no viewport. A flag booleana de
renderer habilitado não basta para aceitar o retorno ao Idle.

O `StumblePainted.shader` aplica `UnityFlipSprite` com `unity_SpriteProps.xy`,
o mesmo flip por desenho usado pelos sprites da URP. O harness valida o shader
renderizando o quadro mais assimétrico sem e com `flipX` e comparando as duas
imagens espelhadas; além disso, exige `flipX=false` no tropeço direito e
`flipX=true` no esquerdo em quadros com pose ativa. Medir apenas a pose ou a
visibilidade não detectava o desenho fixo voltado para a direita.

O harness monta o Idle com uma cópia runtime
`Joint/Runtime/Idle_Painted_Runtime.mat`. O material aprovado
`Idle/Idle_Painted.mat` não é alterado pelas capturas.

## Orçamento e importação

A candidata usa:

- cópias runtime de perfil e blink, byte-idênticas às fontes aprovadas, com
  metadados próprios em `Joint/Runtime/`;
- atlas Run/Jump `1448×1088` e Stumble `1776×888`, formados por padding
  transparente sobre os pixels originais;
- importação `CompressedHQ`, sem mipmap, sem leitura na CPU, clamp/bilinear,
  max 2048.

Os `.meta` originais de perfil/blink e os PNGs aprovados não são alterados.
A métrica é `Profiler.GetRuntimeMemorySizeLong` no Editor Windows; o valor
absoluto inclui a contabilidade do Editor e não substitui medição de aparelho.
O baseline sem compressão é 61,0407 MiB; a candidata HQ é 16,6389 MiB.

## Evidência mínima

- `final-30`, `final-60`, `final-120`: provas com `success=true`, orçamento e
  fonte única.
- `baseline-30`: mesma sequência sem compressão, para comparação.
- `review/`: GIFs normal/lento e folhas de revisão.
- `manifest.json`: hashes, proveniência, verificação do padding e resultados.
- `attempt-01-overlap`, `attempt-02-null-run`,
  `attempt-03-idle-edge-on` e `attempt-04-stumble-unflipped`: tentativas
  preservadas, não candidatas.

## Fora de escopo

Móvel quebrando/cedendo, pausa de 80 ms integrada ao piloto, tropeço no retry
real, plataformas ilustradas, câmera de produção, UI, áudio, WebGL, mobile,
aparelho e aceite da T03. T04/T05 permanecem bloqueadas até a revisão humana.
