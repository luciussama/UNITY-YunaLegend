# Enquadramento do Quarto Escuro — 11/11/2026

Sessão `2026-11-11-codex-camera-desorientacao-c61a`, Codex (GPT-6).

Direção do usuário: acompanhar Yuna em recorte fechado, revelar principalmente o próximo salto e ocultar o solo durante a subida, preservando desorientação. Na queda, acompanhar a menina até o chão. Exceção narrativa: na pausa do castelo, afastar para mostrar o restante do caminho até a porta falsa, com Nanda indicando a saída; ao iniciar a Fuga, retornar ao close.

Implementação: `SideScroll25DCamera` calcula largura de 10,5 a 20 m pelo apoio e pelo alvo de Nanda; acompanha a altura do percurso com suavização, sem perseguir cada arco de salto. FOV 45° e rotação lateral estáveis. O panorama temporário enquadra o castelo e a porta, mostrando a Fuga durante a pausa. O retorno ao close suaviza altura/distância e realinha X para a menina antes de retomar a rolagem e o ScreenCrush.

Validação final: **16/16 EditMode e 20/20 PlayMode**, zero falhas. Três testes novos verificam alvo/solo/arco, retorno/queda e panorama/retomada. Os primeiros testes revelaram carregamento incorreto de cena e interferência dos fixtures, corrigidos com o carregador de Editor e descarregamento da cena; resultados intermediários estão registrados no diário.

As seis imagens são estados controlados de Play Mode, renderizados pela câmera, sem overlays OnGUI. O harness reposiciona atores para documentar o enquadramento; não é uma gravação do percurso jogado. A queda é representada pelo seu estado final no chão. A transição e o comportamento em execução são cobertos pelos testes, não inferidos das imagens estáticas. `capture-harness.cs.txt` contém a ferramenta executada somente na cópia isolada `Build/CameraFramingValidation`.

| Estado | Largura do quadro em Z=0 | Solo: Y projetado na tela (0–1 visível) |
|---|---:|---:|
| Largada | 20,00 m | 0,322 |
| Close após castelo / alvo 10 | 10,76 m | -0,839 |
| Aproximação da porta | 14,63 m | -0,947 |
| Aproximação do portal | 15,42 m | -0,454 |
| Chão após queda | 14,63 m | 0,257 |
| Panorama do castelo | aproximadamente 72,61 m | exceção narrativa |

`camera-behavior.png` reúne close, panorama, subida e chão após queda. `unity-metadata.json` e `framing-checks.json` guardam medidas e posições completas. `editmode.xml`, `playmode.xml` e `capture.log` registram a validação. O manifest lista caminhos e SHA-256.

Foram preservados os demais blocos da cena fonte; a integração alterou somente o Transform da câmera (ID 101), pois os parâmetros serializados de SideScroll já coincidiam entre fonte e cópia validada. Nenhuma regeneração geral de arte/áudio/plataformas foi necessária. O roteiro e a skill audiovisual receberam a nova regra e a exceção do castelo.

As diferenças de arte, iluminação e portal apontadas pela auditoria visual anterior não foram corrigidas nesta tarefa de câmera. As imagens da auditoria anterior permanecem como evidência histórica do enquadramento anterior.
