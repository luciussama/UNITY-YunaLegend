# T08B — acompanhar a subida sem aproximar

13/11/2026, registro 10, sessão `2026-11-13-codex-camera-subida`.
Pedido confirmado pelo usuário: **“Acompanhar a subida, mantendo a distância.”**
**Candidata de câmera entregue para revisão visual.**

O modo ilustrado já mantinha distância, mas sua altura seguia principalmente os
apoios e começava a subir após o pouso. Agora também acompanha a altura de Yuna
durante a subida, mantendo o enquadramento do percurso como limite inferior.
O amortecimento de 0,45 s foi preservado. Não foi acrescentado zoom.
Mudança comportamental somente em `ApplyIllustratedFrame`; modo legado preservado.

## Abrir

- [Vídeo válido, com UI](native-proof-60/camera-ascent.mp4), sem áudio.
- [Frame durante subida](native-proof-60/ascent.png) e
  [frame sobre o móvel](native-proof-60/landing.png).
- Executável candidato: `Build/Windows/YunaLegend-Camera/YunaLegend.exe`.
  Build de acompanhamento anterior e T06.3 aprovada preservadas.

Vídeo nativo **604×720**, 30 fps, **1.120 quadros / 37,33 s**, simulação 60 Hz.
É uma janela desktop estreita, não teste em telefone/retrato/safe area.
Sem esticar, remontar ou retocar quadros. Decodificação integral em
[video-inspection.txt](native-proof-60/video-inspection.txt).

## Verificação

| Grandeza | Resultado durante travessias, saltos e reinícios |
| --- | --- |
| Z da câmera | −9,053300858 m constante |
| Campo de visão | 45° constante |
| Projeção de 1 m no plano da menina | 95,999924–96,000092 px |
| Câmera subindo enquanto Yuna sobe | 216 frames |
| Acompanhamento antes do primeiro pouso | variação vertical de 0,6975 m |

[Trace completo](native-proof-60/camera-ascent.json): `fixedDistanceAndScale=true`.
A medida de um metro é independente da pose: mudanças da silhueta durante salto
e corrida não são confundidas com zoom. A distância fixa é a profundidade em
relação ao plano de jogo; o deslocamento vertical amortecido varia naturalmente.

[Teste funcional](native-retry-60/verification.json): cinco tentativas, quatro
recargas, reinício na pausa e na recuperação, falha e nova travessia, P/Esc;
`success=true`, zero erros e um evento por travessia. Nova verificação a 60 Hz;
não atribuir a esta candidata a cobertura 30/120 Hz de versões anteriores.

Estante/redesenho e os afastamentos de sola já diagnosticados permanecem
registrados, sem correção nesta microentrega. A cômoda T08A não foi integrada.
Não há aceite agregado T06/T07 nem expansão jogável completa com esta prova.

## Reprodução e procedência

Unity 6000.3.28f1, exportador oficial na cópia técnica; build `Succeeded`, zero
erros, 189.862.889 bytes no resumo Unity. Arquivos do candidato copiados para a
pasta separada de câmera. O checkout central contém o mesmo fonte de câmera.

Argumentos de prova:

```text
-previewRetryVerify <pasta-retry> -cameraAscentProof <pasta-trace>
-cameraAscentVideoEncoder <ffmpeg.exe> -previewHz 60 -windowed -logFile <log>
```

O gravador opt-in usa dimensões reais da primeira captura e rejeita alteração
da janela durante a sessão. Sem flags, não cria gravador nem instrumentação.
Os dois primeiros ensaios (`retry-60` e `final-60`) tinham vídeo inválido porque
o gravador antigo assumia 1280×720, enquanto a janela estava limitada a 604×720.
**Não usar esses MP4 como evidência visual.** Foram preservados como histórico.
O vídeo válido é exclusivamente `native-proof-60/camera-ascent.mp4`.

Manifesto de fonte, build e evidências: [manifest.json](manifest.json).
Sem publicação, commit/push, teste em aparelho, WebGL, áudio ou desempenho real.
Pergunta visual: o acompanhamento vertical mantém a leitura do caminho e a
sensação de subida com distância confortável? Resposta humana: **pendente**.
