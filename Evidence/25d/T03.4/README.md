# T03.4 v2 — tropeço e retomada ilustrados

Estado: **aprovada**, 12/11/2026. Codex (GPT-6), sessão
`2026-11-12-codex-stumble-t034-a71c`. Base `fb68a26`, Unity 6000.3.28f1.
Transferência solicitada pelo usuário atual e registrada no coordenador/diário.
T03.1/T03.2/T03.3 aprovadas preservadas; T03 inteira ainda não concluída.

Aprovação humana: “Gostei. pode aprovar.” [Registro e hashes](reviews/T03.4-v2-aprovacao.md). Aceite individual; conjunto T03 ainda pendente.

## Entrega para ver

- [Gameplay](review-v2/stumble-game.gif), 1280×720.
- [Ampliação normal](review-v2/stumble-detail.gif), 640×640.
- [Ampliação lenta](review-v2/stumble-detail-slow.gif), metade da velocidade.
- [Revisão individual](reviews/T03.4-v2.md).

Os três clipes usam os mesmos 90 renders da execução **120 Hz**, capturados
em 30 fps: 3 segundos normais / 6 segundos lentos. São replays, não loops
contínuos: ao reiniciar, a cena volta ao início. Sem retoque, interpolação ou
recorte; somente paleta GIF e quantização de duração em centésimos de segundo.
Sequência: corrida; evento completo em 0,5 s; segundo evento em 1,5 s,
interrompido por reset em 1,6 s; repetição completa em 2,0 s. Os tempos indicam
o tick da fixture; a apresentação amostra em 30 fps.

## Arte e implementação

Oito poses originais articulam joelhos, braços, cabeça, saia e rabo de cavalo.
Imagegen **embutido**, referência aprovada `Evidence/25d/T01/yuna-profile-v1.png`.
[Prompt integral](prompt-stumble-v1.txt), [fonte bruta](yuna-stumble-generated-v1.png)
e cópia byte a byte em `Stumble/Yuna_Stumble_Atlas_v1.png`. RGBA 1774×887,
alpha 0–255, grade 4×2 arredondada em células 443/444 px. Mesma escala em todas
as poses, **330 PPU**, sem normalizar caixas opacas. Baselines e pontos de sola
medidos no alpha ≥128, em ilhas opacas reais; não usar a média entre dois pés.

Driver/harness/cena/Sprites/metas estão exclusivamente em
`Assets/Art/Illustrated25D/Yuna/Stumble/`, fora dos BuildSettings. O driver lê
`IsStumbling` e `StumbleDelayRemaining`. Durante a pausa conserva a pintura e
a posição visual da corrida; depois percorre as oito poses nos 280 ms e retorna
à corrida com correção limitada a 2 m/s. Cada pose tem ponto de sola ancorado
no chão real enquanto é exibida; a troca de pose representa um novo apoio.
Nenhuma deformação da pintura inteira substitui as poses.

A QA identificou névoa no canvas bruto (alpha baixo, principalmente 1–16/255).
O shader/material exclusivo Stumble aplica `alpha *= smoothstep(.075,.18,alpha)`:
remove fundo quase transparente e preserva alpha ≥0,18. Fonte RGBA original
intacta; Run/Jump e pipeline global não foram alterados. Novas capturas v2 foram
feitas depois dessa correção; v1 é ensaio histórico anterior ao acabamento.

O harness chama o **BeginStumble(.08f,.28f) existente**. Não existe simulador
alternativo de movimento. Velocidade 5,2 m/s, CharacterController, root, física,
Gameplay e apresentações Run/Jump permanecem iguais. O relógio global avança,
Time.timeScale=1, enquanto avanço horizontal e relógio de passos ficam parados
no trecho de pausa. O contador de recuperação do controlador é observado por
reflection somente no harness Editor, sem ampliar sua API de produção.

`ResetPresentation()` limpa pose, compensação, âncora e transição. Na fixture,
é chamado imediatamente depois do ResetPlayer real; passos/timer ficam em zero,
cuja fórmula pública corresponde à fase **0,5**, e o evento seguinte funciona.
Esse hook deverá ser conectado ao retry do piloto na T06: esta entrega isolada
não afirma que o retry da Gameplay já consome o driver novo. O driver também
limpa o estado ao observar o cancelamento do evento.

## Verificação executada

Três execuções PlayMode batch seriais na cópia `Build/Publish/Project`, com
fontes, Packages e ProjectSettings sincronizados antes da prova. Ao finalizar,
639 arquivos preexistentes conferiram com a cópia e com os hashes de entrada;
novos arquivos Stumble também conferiram byte a byte após sincronização de saída.
Fontes aprovados não foram alterados. [Manifesto](manifest-v2.json) identifica
fontes/dependências e evidências; [hashes de entrada](clone-input-hashes.json).

| Medida | 30 Hz | 60 Hz | 120 Hz |
| --- | --- | --- | --- |
| Eventos / poses / renders por câmera | 3 / 8 / 90 | 3 / 8 / 90 | 3 / 8 / 90 |
| Pausa consumida em cada evento | 80 ms | 80 ms | 80 ms |
| Recuperações completas (duas) | 280 ms | 280 ms | 280 ms |
| Deriva no ponto da sola durante a mesma pose | 0 m | <0,000001 m | <0,000001 m |
| Separação vertical do ponto da sola | 0 m | 0 m | 0 m |
| Maior erro de avanço físico esperado | <0,000001 m | <0,000001 m | <0,000001 m |
| Maior passo do pivô visual junto ao tropeço, 30 fps | 0,195 m | 0,260 m | 0,217 m |
| Menor passo do pivô visual junto ao tropeço, 30 fps | 0,107 m | 0,024 m | 0,060 m |
| Reset lógico/visual e repetição | passou | passou | passou |

As durações acima são **consumo real dos contadores**, não arredondamento de
quadros do GIF. A duração observável do estado é quantizada por tick.
`make_review.py` verifica também avanço previsto por velocidade × (tempo−80 ms),
oito poses, contador único por chamada, escala/colisor, hashes e continuidade.
A recuperação interrompida do segundo evento não é contada como 280 ms completos.
A medição está em `verification.json/delay_consumed_seconds`, calculada dos ticks reais.

Fontes da prova principal: `final-v2-120/stumble-proof.json`, cópia exata em
`review-v2/`; 180 hashes dos renders em `review-v2/verification.json`.
Raw em `Build/25d/StumbleFinalV2-120/{game,detail}` (fora do Git).
Poses 0/2/3/7 e quadros antes/depois foram inspecionados estaticamente; timeline,
solas e pivô foram auditados. Não houve sessão manual nem escuta de áudio.

## Limites

- A âncora valida um ponto opaco por pose, não toda a extensão da sola nem
  continuidade do mesmo pé entre desenhos. Há mudanças discretas de apoio;
  avaliar ritmo e tamanho/posição do corpo na entrada e saída. Testes não
  aprovam identidade, delicadeza ou fluidez automaticamente.
- A corrida antes/depois conserva os passos corporais já aceitos na T03.2.
  A pintura nova e a transição isolada receberam aprovação humana; a revisão do conjunto permanece pendente.
- Atlas observado em aproximadamente 12,14 MiB no Editor. O orçamento
  residente conjunto de 32 MiB ainda precisa de otimização; não fechar T03
  nem integrar o piloto só com esta prova. Sem medição GPU/aparelho.
- Sem móvel cedendo, estralo percebido, plataformas inclinadas/desconexas,
  virada, toque, menu, retrato, WebGL ou gameplay integrada. T05.3/T06.2/T06.3
  continuam responsáveis pela sequência real e retry completo.
- Logs contêm a exceção conhecida do indexador UnityEditor.Search na abertura;
  os três processos terminaram e `YUNA_STUMBLE_REVIEW_DONE success=True` foi emitido.

## Reprodução

Em cópia isolada com fontes/GUIDs equivalentes, sem `-quit`:

```text
Unity.exe -batchmode -projectPath <copia> -executeMethod YunaLegend.YunaIllustration.Editor.YunaStumbleReviewCapture.Run -stumbleHz 120 -stumbleOutput <evidencias> -stumbleFrames <quadros> -logFile <log>
```

Aceita 30/60/120 Hz; gera apenas cena/Sprites Stumble. Para recompor auditoria/GIFs:
`python Evidence/25d/T03.4/make_review.py`. Logs em
`Build/25d-t034-final-v2-{30,60,120}.log`.

Ensaios `attempt01-60` (expectativa incorreta do reset e PPU305) e `validated-30`
(reset corrigido, antes de corrigir os pontos opacos) são históricos, sem alegação
de equivalência com a candidata. `final-v1-*`/`review-v1` são ensaios antes da correção do alpha. Nenhum commit, push ou publicação nesta sessão.
