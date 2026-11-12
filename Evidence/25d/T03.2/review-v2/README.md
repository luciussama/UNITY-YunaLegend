# T03.2 v2 — fechamento do clipe e comparação apoio × continuidade

12/11/2026, sessão `2026-11-12-deepseek-run-tail-4b7e`, DeepSeek (deepseek-flash).
Pedido do usuário: corrigir o frame incorreto no final da corrida, identificado pelo
Codex em [diário 595](../../../../docs/ia/diario.sqlite3) e na
[investigação v1](../investigation-tail-v1/README.md). Estado: **em_revisao**.

## O que foi corrigido

O clipe da v1 tinha 120 quadros = **10,23 passadas**; ao repetir, voltava **1,5 pose
atrás** (0,086 de ciclo) e o chão recomeçava **0,63 m fora de fase**. Era esse o
"tranco no final": não uma pose errada, mas o clipe terminando no meio de uma passada
e com a grade deslocada.

A v2 fecha o clipe:

- **128 quadros de 1/30 s** = **11,0053 passadas**: a fase no loop passa de 0,086 para
  **0,0053 de ciclo (0,042 de pose)**.
- A amostragem começa **na troca de pose** (fase 0), não em fase arbitrária.
- A régua do chão é **re-espaçada para 1,00848 m** (22 marcas cabem exatamente nos
  22,1867 m do clipe): a fase da grade no loop cai de 0,63 m para **0,0 m**.
- Medido nos quadros renderizados: o salto de tela entre o último e o primeiro quadro é
  **5,0 px**, contra mediana de **6,3 px** entre quadros normais — o loop fecha como um
  quadro comum (`closes_like_a_normal_frame: true`).

Verificação: [`verification.json`](verification.json) e [`run-proof.json`](run-proof.json)
(execução a 120 Hz; as de 30 e 60 Hz ficam em `../final-v2-30/` e `../final-v2-60/`).

| Medida | v1 | v2 |
| --- | --- | --- |
| Quadros / passadas | 120 / 10,23 | **128 / 11,0053** |
| Fase no loop | 0,086 ciclo (1,5 pose) | **0,0053 ciclo (0,042 pose)** |
| Grade no loop | 0,63 m | **0,0 m** |
| Salto de tela no loop | ~5,5 px com fase errada | **5,0 px com fase certa** |
| Deriva do apoio na fase de apoio | 0,000001 m | **0,0 m** |
| Erro vertical do marco | 0 m | **0 m** |
| Erro da trajetória física | <0,001 m | **0,0000016 m** |
| Colisor / escala / Gameplay / controlador | preservados | **preservados (SHA iguais)** |

## O que não foi corrigido — e por quê

O **passo do corpo entre poses** continua: 21 de 127 intervalos têm corpo parado e o
maior passo é **0,307 m** (a 120 Hz; 0,373 m a 30 Hz). Isso não é o fechamento do clipe:
é a **passada pintada ser 3,19× menor que a passada física**. Contas com os dados do
próprio clipe:

- chão percorrido durante um apoio (poses 0–2 de 4, velocidade 5,2 m/s, intervalo de
  passo 0,193846 s): **0,756 m**;
- recuo da sola pintada no mesmo apoio (pixels de `FootX` ÷ 370,94 de PPU): **0,3158 m**;
- diferença a absorver por apoio: **0,4402 m**;
- por ciclo: pintura **0,6317 m** contra chão **2,016 m** → **3,19×**;
- velocidade natural desta pintura na cadência atual: **1,63 m/s**.

Enquanto o pé fica plantado, esses 0,44 m por apoio só podem aparecer como **passo do
corpo** a cada troca de pose; se o corpo seguir o root, eles aparecem como **deriva do
pé**. Não há terceira opção sem mudar arte ou velocidade. Para tornar isso explícito,
capturei também a variante sem compensação (diagnóstico, **não** candidata):

| Variante (60 Hz) | Corpo | Apoio |
| --- | --- | --- |
| **v2 compensada** (candidata) | passos de 0 a 0,307 m, 21/127 quadros parados | deriva **0,0 m** |
| **sem compensação** (diagnóstico) | passo constante **0,17333 m** (desvio 0,0000007), 0 quadros parados | deriva **0,2423 m = 23,3 px a 720p** |

Ou seja: corpo liso custa 23 px de skate por apoio; pé plantado custa 0,3 m de passo a
cada troca de pose. A meta de 2 px de apoio não coexiste com continuidade corporal
enquanto a passada pintada for 3,19× menor que a física.

Arquivos: [`../final-v2-smooth-60/`](../final-v2-smooth-60/) (prova, verificação e GIFs
do diagnóstico).

## O que revisar

1. [Corrida em escala de gameplay](run-gameplay.gif) (128 quadros, 4,27 s) e
   [ampliação](run-detail.gif) / [câmera lenta](run-detail-slow.gif): o loop fecha sem
   salto visível?
2. O passo do corpo entre poses permanece. Aceitar como **passo discreto da pintura** ou
   decidir entre **nova arte com passada ~2,4× mais longa** e **reduzir a velocidade/cadência
   para a arte (~1,6 m/s)**? Comparar com o
   [diagnóstico sem compensação](../final-v2-smooth-60/run-detail.gif).
3. Qualquer que seja a escolha, nada foi alterado em pintura, física, velocidade ou
   colisor; o clipe salvo em `Assets/.../Run/YunaRunReview.unity` é o compensado.

Perguntas registradas em [../reviews/T03.2-v2.md](../reviews/T03.2-v2.md).

## Reprodução

```
Unity.exe -batchmode -projectPath <copia> ^
  -executeMethod YunaLegend.YunaIllustration.Editor.YunaRunReviewCapture.Run ^
  -runHz 120 -runSamples 128 -runFootPlant 1 ^
  -runOutput <evidencias> -runFrames <quadros> -logFile <log>
```

Sem `-quit`; o harness sai após o Play Mode. Marcador `YUNA_RUN_REVIEW_DONE success=True`.
Variante diagnóstica: `-runFootPlant 0`. GIFs e auditoria:
`python tools/make_run_review_v2.py --frames <quadros> --proof <run-proof.json> --out <pasta>`
(`--no-gifs` só audita). Logs: `Build/25d-t032-v2-{30,60,120}.log`, `Build/25d-t032-v2s-60.log`.

## Limites

- Fixture de chão técnico plano; não é apoio real de plataforma, aparelho, retrato ou
  publicação. A câmera acompanha o root de forma rígida nesta prova.
- A fase do loop fecha exatamente no clipe entregue (120 Hz). A 30 Hz o salto de tela no
  loop mede 15,8 px porque a compensação não é função pura da fase — se a direção de arte
  escolher continuar com esta técnica, a formulação precisa ser derivada da fase para ser
  independente da taxa.
- O GIF não tem áudio; a percepção de cadência dos passos não é certificada aqui.
- Este trabalho não iniciou salto T03.3 nem alterou repouso T03.1.
