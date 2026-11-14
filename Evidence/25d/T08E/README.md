# T08E — travessia até os blocos ABC (candidata individual)

14/11/2026. Sessão `2026-11-13-dsh-abc04-cena`, registro 16, tarefa T08E.
**Candidata individual para revisão humana; não é aceite da fase inteira.**
Depende da cômoda T08C, já aceita.

A microentrega acrescenta **um único objeto**: a pilha ABC04 depois da cômoda, com
apoio próprio no topo estreito do bloco A. Urso, livros, móvel, cômoda e a câmera
aprovada foram preservados. A cena e a build anteriores continuam intactas
(hashes conferidos em [manifest.json](manifest.json)).

## Abrir

- Build nova: `Build/Windows/YunaLegend-Continuacao-04/YunaLegend.exe` (manter a pasta).
- [Vídeo contínuo](final-video-60/camera-ascent.mp4): duas travessias e um reinício.
- [Pouso](final-60/attempt-0-abc-land.png) e [repouso final](final-60/attempt-1-abc-idle.png).
- Cena Unity própria: `Assets/Art/Illustrated25D/Environment/ABC/Preview-Continuacao-04.unity`.
- A build aprovada até a cômoda continua em `Build/Windows/YunaLegend-Continuacao/`.

O vídeo é nativo 604×720, **22,03 s a 30 fps**, simulação a 60 Hz, com UI, sem
áudio nem cortes. A janela desktop não equivale a aparelho/retrato/safe area.

## Apoio definido pela pintura

O contrato completo está em [CONTRATO-APOIO-ABC04.md](CONTRATO-APOIO-ABC04.md) e os
números medidos em [asset-measurements.json](asset-measurements.json). Resumo:

- Escala uniforme **10% menor** que a base (269,6690 px/m); piso −9,10 m → borda
  visível do tampo −5,199 m ⇒ **3,901 m** de altura.
- Apoio **1,5130 m** de largura (face frontal do bloco A, 34,2% do conteúdo) por
  1,59 m de profundidade. É o único colisor do objeto.
- Vão até a cômoda **2,1510 m**; degrau vertical **0,851 m**. A pilha assenta no piso.
- Folga visual entre as pinturas **0,556 m**; bounds de sprite incluem transparência.

A cômoda continua com a compensação de sola já aprovada; os demais apoios anteriores
não foram alterados. No ABC a sola **desenhada** é presa à superfície pelo renderer
da pose ativa (`pinVisibleSole`), porque a arte tem o pé abaixo do pivô.

## Correções pedidas na revisão

1. **"Parando dentro dos blocos"**: o repouso final usa a Idle, um `MeshRenderer` com
   ordem 0, enquanto a pintura estava em ordem 20; o tampo cobria as pernas. A pintura
   passou para a **ordem −1** (atrás da Idle, à frente do piso e das sombras).
2. **"Perna dentro da letra"**: o pouso estava no topo do quadro frontal (y=185); a
   linha voltou para a **borda do tampo plano** (y=150).
3. **"Ainda precisa ser menor"**: escala uniforme reduzida para **10% abaixo da base**.
   A peça ficou com 4,43 m de largura, 3,90 m de altura e degrau de 0,85 m.
4. **"Pé dentro do tampo"**: o pouso subiu de y=150 para **y=125**; a sola fica ~9 cm
   acima do lip e todo o friso abaixo da linha do pé. Aprovado em preview 120 Hz.

Medição do contato (não estimativa): a sola desenhada ficava **2,8 cm (Idle) e 4,9 cm
(corrida) abaixo** do topo físico. Com o pin, o gap medido ficou **0,0000 m** na Idle
e no pouso, nas três taxas. O root, a cápsula, a física e as animações não mudaram.

## Validação no executável

| Simulação | Duas travessias | Saltos | Estralos | Reset | Erros / legado | Pouso ABC |
| --- | --- | --- | --- | --- | --- | --- |
| 30 Hz | passou | 5 / 5 | 1 / 1 | limpo | 0 / 0 | x = 22,937 |
| 60 Hz | passou | 5 / 5 | 1 / 1 | limpo | 0 / 0 | x = 22,864 |
| 120 Hz | passou | 5 / 5 | 1 / 1 | limpo | 0 / 0 | x = 22,855 |

Relatórios: [30](final-30/verification.json), [60](final-60/verification.json),
[120](final-120/verification.json). Duas tentativas por taxa, com recarga real por R.
O apoio ABC fica em x 22,3131..23,8261; todas as taxas pousam dentro dele.

O erro da âncora visual foi **0,0 m** nas duas superfícies novas (cômoda e ABC) nas
três taxas. Não foi aplicada correção à física, às poses ou à cápsula.

O [trace da câmera](final-video-60/camera-ascent.json) confirma Z −9,0533 m, FOV 45°
e 96 px/m constantes, com 192 quadros de subida. A câmera aprovada não foi alterada.

## Limites

- A pilha tem 3,90 m; com a câmera aprovada (7,5 m de altura) a base no piso sai do
  quadro quando Yuna está no alto. O apoio está correto; a composição é que não
  mostra a base nesse momento.
- Cada −5% adicional de tamanho custa ~0,15 m de degrau; abaixo de ~3,4 m o salto
  vira quase um passo.
- O contato é o da cômoda/T08C (compensação visual de 8 cm); a curva do urso e os
  demais defeitos de sola continuam para T06.
- Não há validação de aparelho, toque real, retrato, safe area, WebGL, áudio ou
  desempenho. Sem aceite global T06/T07; estante segue adiada.
- Nenhum commit/push nesta entrega.

## Reprodução

```text
Unity 6000.3.28f1, batchmode:
-executeMethod YunaLegend.Illustrated25D.Preview.Editor.IllustratedPreviewBuild.RunContinuationBlocks

Verificação: -previewAbcVerify <pasta> -previewHz 30|60|120
Vídeo: -previewAbcVerify <pasta> -cameraAscentProof <pasta-video>
       -cameraAscentVideoEncoder <ffmpeg.exe> -previewHz 60
```

Fontes, build e evidências com SHA256: [manifest.json](manifest.json).
Perguntas da revisão: [revisao-proposta.md](revisao-proposta.md).

