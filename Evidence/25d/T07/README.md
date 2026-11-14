# T07 — aprovação humana do piloto em movimento

Data: 13/11/2026. Sessão `2026-11-13-dsh-consolidacao-t06t07`.
**Estado: bloqueada. Nada foi aprovado nesta página e a revisão ainda não deve ser pedida.**

> **Bloqueio de 13/11/2026:** o usuário relatou urso e livros flutuando sem sombra
> (diário 972). A regressão foi confirmada na build final e a T06 foi reaberta.
> A revisão de T07 só deve acontecer depois de uma nova microentrega de contato/
> ancoragem na penumbra, aprovada pelo usuário. Ver
> [regressão na consolidação T06](../T06/README.md#regressão-de-contato-reportada-e-confirmada).

Esta é a entrega agregada que precede a expansão da fase. A T06 foi consolidada
tecnicamente em [Evidence/25d/T06](../T06/README.md); a T07 exige, além dos testes
verdes, a **aprovação explícita do piloto em movimento**
([plano](../../../docs/ia/PLANO-25D-OBRIGATORIO.md), linha T07).

## Abrir e assistir

- **Build:** `Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe`
  (manter a pasta inteira). SHA-256 do executável no [manifesto da T06](../T06/parametros.json).
  A build aprovada anterior está preservada em
  `Build/Windows/YunaLegend-Acompanhamento-T063-aprovada/`.
- **Vídeo curto da sequência aprovada:** [T06.3, 37,30 s](../T06.3/v3-visible-60/continuous.mp4)
  — 1280×720, simulação 60 Hz, captura 30 fps, **sem áudio**.
- **Áudio real do piloto (separado do vídeo):**
  [gameplay-audio.wav](../T06/audio-60/gameplay-audio.wav) — mix 48 kHz, 70,19 s,
  com a música `High-Frequency Violin` e o estralo do móvel. Não está sincronizado
  com o vídeo; serve para escuta, não como prova de mixagem.
- **Critérios e limites:** [T06 consolidada](../T06/README.md).

A build final só difere da build mostrada no vídeo pela instrumentação opt-in de
verificação (captura de áudio, marcas e timeout). Nenhuma mudança de gameplay, arte,
física, câmera ou UI foi feita depois da aprovação da T06.3.

## Perguntas concretas para a revisão

Responder olhando o vídeo e/ou jogando o trecho. Não é necessário responder tudo;
o que importa é o que incomodar.

1. **Acolhimento:** o quarto noturno, a penumbra e a música convidam a entrar e
   acompanhar, ou algum elemento puxa a atenção para fora da cena?
2. **Identidade:** Yuna continua a mesma menina da prancha/menu (rosto, cabelo,
   vestido, proporção) em repouso, corrida, salto e tropeço?
3. **Companhia:** Nanda está legível, próxima e expressiva, com luz/trilha azul
   subordinadas à personagem — sem virar efeito ou partícula?
4. **Fluidez:** a corrida, o salto, o pouso, o tropeço e o retorno ao início têm
   ritmo contínuo? Algum momento parece travado, acelerado ou “colado”?
5. **Previsibilidade:** dá para ler quando pular e onde vai cair? O salto parece
   responder ao comando, sem surpresa de último instante?
6. **Leitura dos apoios:** urso, livros e móvel têm superfícies de contato críveis
   e os pés pousam onde a arte sugere? O cedimento do móvel é legível?
7. **Falha e retorno:** após a falha ou o reinício, o estado volta limpo e o novo
   evento é reconhecível? O vídeo da T06.3 cobre pausa, recuperação, falha e nova
   travessia.

Se algo for reprovado, indicar o instante aproximado do vídeo (ou a ação) e o
critério afetado; a tarefa correspondente (T03, T04, T05, T06) deve ser reaberta
antes de qualquer expansão.

## O que este pacote cobre

- Piloto contínuo em build, com teclado, mouse e toque **cobertos por testes
  sintéticos** em 30/60/120 Hz; nenhum aparelho físico foi usado.
- Travessia urso → livros → móvel com câmera final, UI, salto/pouso, tropeço,
  cedimento de 7,5 cm, pausa local de 80 ms, recuperação de 280 ms e retry.
- Física 3D e `PlatformSupportProfile` preservados; zero renderers legados
  visíveis e uma fonte de Yuna por vez.
- Áudio capturado e analisado por correlação com o clip aprovado.

## O que este pacote não prova

- Escuta humana, mixagem, equilíbrio de volumes ou o estralo “soando bem”.
- Toque em aparelho real, retrato, safe area, aparelhos móveis ou WebGL.
- Sombras dinâmicas de oclusão.
- Comparação A/B isolada da assistência horizontal de pouso.
- Desempenho/FPS em aparelho; o vídeo segue a simulação, não o relógio de parede.

## Como registrar a resposta

A resposta humana deve ser registrada em
[`reviews/T07.1-proposta.md`](reviews/T07.1-proposta.md), com a versão revisada
(build + vídeo), a mensagem literal e a decisão (aprovada / ajustar / rejeitada).
O aceite de T07 no coordenador exige `--human-approval` e referência à resposta
real; um agente não pode conceder esse aceite em nome do usuário.

## Aprovações anteriores que **não** equivalem a T07

- “perfeito. Aprovado.” — build de acompanhamento v6
  ([registro](../T06-acompanhamento/reviews/acompanhamento-v6-aprovacao.md)).
- “aprovadíssimo” — sequência de falha/retry T06.3
  ([registro](../T06.3/reviews/T06.3-v1-aprovacao.md)).

Ambas aprovam entregas específicas e são base do piloto; nenhuma delas é a
aprovação agregada do piloto em movimento exigida por T07.
