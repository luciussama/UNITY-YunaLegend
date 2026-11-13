# T04.1 — contrato do voo ilustrado da Nanda

Estado: candidata em revisão humana; **T04.1 não aceita**. Codex (GPT-6.1),
13/11/2026, sessão `2026-11-13-codex-t04-nanda-25c1`.

## Escopo

Entregar a apresentação ilustrada da Nanda **em voo**: oito poses de asas
derivadas da prancha aprovada da T01, ciclo determinístico, hover local e
orientação por direção, com prova nas três taxas de simulação. Esta entrega é
uma prova isolada de apresentação.

Não integra o `NandaFairyGuide`, não altera a Gameplay, a física, o controlador,
a luz, a trilha, a câmera de produção ou os PNGs aprovados. Não inicia T04.2,
T05, T06 ou T07.

## Critérios de aceite

1. Oito poses distintas e legíveis; a neutra não altera nenhum pixel visível da
   prancha aprovada.
2. Ciclo de **0,8 s** com as oito poses; 30/60/120 Hz produzem a mesma lista de
   48 mudanças de pose e `poseMismatches=0`.
3. Direção `+1` com `flipX=false` e direção `-1` com `flipX=true` em todas as
   amostras; nenhuma violação.
4. A sonda renderizada no material padrão do `SpriteRenderer` deve produzir o
   espelho da imagem sem flip, com erro dentro da tolerância.
5. O componente anima apenas o visual; o root, a navegação, a luz e a trilha
   continuam com o guia.
6. Atlas `2048×1184` (oito células `512×592`), PPU 880, pivô
   `(0,5; 0,42061)`, `CompressedHQ`, sem mipmap, sem leitura na CPU,
   clamp/bilinear, max 2048; residente ≤ 16 MiB.
7. Fontes aprovadas, Gameplay e `NandaFairyGuide.cs` byte a byte; a fixture T03
   é aberta somente para a câmera de escala.
8. Revisão humana dos GIFs, folhas e sonda antes de qualquer dependente.

## Contrato técnico

`NandaFlightIllustration` expõe `Pose`, `Cycle`, `CyclePhase`, `HoverOffset`,
`Direction`, `FlipX`, `FrameCount` e `Evaluate(seconds, facing)`. O `LateUpdate`
usa `Time.deltaTime` quando `previewClock=true`; o harness desliga o relógio e
chama `Evaluate` com o tempo absoluto, garantindo cadência independente da taxa.
O hover é aplicado em um filho (`visualRoot`), nunca no root. Se o componente
estiver no mesmo objeto do renderer, o hover não move o root.

O `flipX` é aplicado no `SpriteRenderer` e o desenho usa o **material padrão**
de sprite. Não há shader customizado nesta entrega; o caminho de flip é o mesmo
de Run/Jump da Yuna, e a sonda prova o resultado renderizado.

O harness abre a fixture T03, desliga controlador/corrida/salto/tropeço,
congela a Yuna em `Evaluate(0)` e usa as câmeras de produção (7,5 m verticais a
1280×720; detalhe a 1,2 m). A Nanda é criada em `Nanda_FlightReview` e a cena é
salva em `NandaFlightReview.unity`; a fixture original não é salva.

## Evidência mínima

- `final-30`, `final-60`, `final-120`: provas com `success=true`, 72 quadros por
  câmera, oito poses, duas direções e sonda de espelhamento.
- `review/`: GIFs normal/lento, folhas, faixa das oito poses e comparação de
  espelhamento.
- `frames/` e `frames-manifest.json`: quadros em resolução cheia e proveniência.
- `manifest.json`: hashes de fonte, atlas, sprites, código, cena e revisão.
- `reviews/T04.1-revisao.md`: registro da resposta humana.

## Fora de escopo

Luz pulsante, trilha, acompanhamento, falas, expressões faciais novas, integração
no guia, câmera de produção dinâmica, móvel, plataformas, UI, áudio, WebGL,
aparelho e aceite da T04 agregada. T04.2/T05/T06 dependem desta revisão.
