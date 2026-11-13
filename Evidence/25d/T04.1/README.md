# T04.1 — Nanda ilustrada em voo: asas, ciclo e orientação

Estado: **T04.1 aceita pelo usuário em 13/11/2026**. Codex (GPT-6.1), sessão
`2026-11-13-codex-t04-nanda-25c1`. Base `7988a0a`, Unity 6000.3.28f1.
Esta microentrega cobre **somente** o voo ilustrado, a cadência das asas e a
orientação. T04.2 (luz/trilha), T05, T06 e a integração no guia não foram
iniciados.

## O que revisar

- [Gameplay normal](review/game-normal.gif) — 4,8 s, 960×540, 15 fps.
- [Gameplay lento](review/game-slow.gif) — o mesmo trecho, 10 fps.
- [Detalhe normal](review/detail-normal.gif) — 640×640, 15 fps.
- [Detalhe lento](review/detail-slow.gif) — 640×640, 10 fps.
- [Folha de gameplay](review/game-review-sheet.png) e
  [folha de detalhe](review/detail-review-sheet.png) — doze momentos-chave.
- [Oito poses renderizadas](review/pose-strip.png) — prova de que as oito
  células aparecem no caminho padrão de sprite.
- [Comparação de espelhamento](review/flip-compare.png) — render sem e com
  `flipX`.

Pergunta da revisão: **a Nanda lê como companheira viva e o ritmo das asas/hover
convence? A orientação direita/esquerda está correta?** Resposta literal:
**"aprovado."** (13/11/2026), registrada em
[reviews/T04.1-revisao.md](reviews/T04.1-revisao.md). O aceite é exclusivo da
T04.1: não aceita T04.2 nem a T04 agregada.

## O que foi entregue

- [Componente](../../../Assets/Art/Illustrated25D/Nanda/Flight/NandaFlightIllustration.cs)
  anima apenas o desenho: oito poses, ciclo de **0,8 s**, hover local de
  **3,5 cm / 2,4 s**, `flipX` por direção e `Evaluate(seconds)` determinístico.
  O root continua pertencendo ao `NandaFairyGuide`; o componente não move a
  navegação, a luz nem a trilha.
- [Harness isolado](../../../Assets/Art/Illustrated25D/Nanda/Flight/Editor/NandaFlightReviewCapture.cs)
  monta a Nanda sobre a fixture aceita da T03, congela a Yuna como **escala
  estática** e simula 30/60/120 Hz. A câmera de gameplay usa o enquadramento de
  produção (7,5 m verticais, 1280×720, 96 px/m): Nanda tem **64,6 px** de altura
  e Yuna, **140,2 px**. A câmera de detalhe fica a 1,2 m.
- [Atlas](../../../Assets/Art/Illustrated25D/Nanda/Flight/Nanda_Flight_Atlas_v1.png)
  `2048×1184`, oito células `512×592`, PPU **880**, pivô `(0,5; 0,42061)`.
  Importação `CompressedHQ`, sem mipmap, sem leitura na CPU, clamp/bilinear,
  max 2048. Textura residente: **4,8134 MiB** (limite 16).
- [Gerador determinístico](../../../tools/make_t04_nanda_frames.py): warp
  vertical com máscara radial ancorada na raiz das asas sobre a prancha aprovada
  da T01. O quadro neutro altera **0 pixels visíveis**; a fonte aprovada continua
  byte a byte (`f16ba386…`).
- [Cena de revisão](../../../Assets/Art/Illustrated25D/Nanda/Flight/NandaFlightReview.unity)
  e oito `Sprite` assets em `Runtime/Frames/`. A fixture T03 é apenas aberta e
  nunca salva; a Gameplay e o `NandaFairyGuide.cs` permanecem intactos.

## Resultado das provas

| Execução | Quadros | Poses | Direções | Mudanças de pose | Espelhamento | Orçamento |
| --- | ---: | ---: | --- | ---: | --- | --- |
| 30 Hz | 72 | 0–7 | direita + esquerda | 48 | `mirrorError=0` | 4,8134 MiB |
| 60 Hz | 72 | 0–7 | direita + esquerda | 48 | `mirrorError=0` | 4,8134 MiB |
| 120 Hz | 72 | 0–7 | direita + esquerda | 48 | `mirrorError=0` | 4,8134 MiB |

As três execuções produzem a **mesma lista de 48 mudanças de pose** em
`t=0,10…4,80 s`, com `poseMismatches=0` em todas. O harness falha se uma pose
faltar, se `flipX` divergir da direção, se a sonda renderizada não for o espelho,
se a fonte/atlas/cena/guia mudarem ou se o orçamento estourar.

A sonda usa o **material padrão do `SpriteRenderer`** (sem shader customizado):
renders de `flipX=false` e `flipX=true` comparados pixel a pixel com
`asymmetry=6912790`, `mirrorError=0`, tolerância `864098`. Em todas as amostras,
direção `+1` tem `flipX=false` e direção `-1` tem `flipX=true`.

## Decisões e limites

- O loop usa deslocamento vertical contínuo da pintura inteira na região das
  asas, não camadas giradas separadas. Isso evita costura, franja escura e borda
  solta; face, vestido e braços ficam protegidos.
- A troca de lado da Nanda **no GIF** é coreografia do harness (0,45 s, com leve
  arco) para a revisão. O componente não navega; em produção o guia continua
  dono do deslocamento.
- A Yuna da fixture é **apenas régua de escala**. Não há acompanhamento, luz,
  trilha, falas ou gameplay da Nanda nesta entrega.
- Prova no **Editor Windows/D3D12**, câmera técnica. Não é medição de GPU,
  aparelho, build WebGL, mobile ou câmera de produção.
- O atlas é reduzido de 1168×1346 para 512×592 por célula para caber em 2048;
  a folha de detalhe serve para julgar a perda. O arquivo de quadros em
  `frames/` permanece em resolução cheia **para inspeção**, não é o runtime.

## Reprodução

Sem `-quit`, com o projeto sem outra instância da Unity:

```text
Unity.exe -batchmode -projectPath <projeto> -executeMethod YunaLegend.NandaIllustration.Editor.NandaFlightReviewCapture.Run -t04Hz <30|60|120> -t04Compression hq -t04Output Evidence/25d/T04.1/final-<hz> -t04Frames Build/25d/T04Nanda-<hz> -logFile <log>
```

Recompor revisão e manifesto:

```text
python tools/make_t04_nanda_review.py
python tools/make_t04_nanda_manifest.py
```

Provas: [30 Hz](final-30/flight-proof.json),
[60 Hz](final-60/flight-proof.json), [120 Hz](final-120/flight-proof.json).
Manifesto: [manifest.json](manifest.json). Contrato:
[CONTRATO.md](CONTRATO.md). Registro de revisão:
[reviews/T04.1-revisao.md](reviews/T04.1-revisao.md).
