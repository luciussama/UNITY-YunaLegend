# Móvel 02: travessia com cedimento

Implementação nativa Unity/C#, sessão `2026-11-11-codex-movel02-cedimento-935a`, plano de diário 346. A referência anexada orientou cor e estilo; não foi usada como sprite nem como planta obrigatória do móvel.

## Comportamento

- Penteadeira 02, imediatamente depois dos livros: 8,4 × 2,3 × 2,4 m. Base no piso em Y=-9,1; topo em Y=-6,8.
- Entrada pelos livros com vão real de 1,25 m; saída para 03 com 1,35 m. O gerador propaga a ampliação ao restante da rota, mantendo os desníveis e os vãos seguintes. O retry do castelo usa agora o apoio real, em vez de X=39 fixo.
- Dois tampos visíveis com dois colliders correspondentes; abertura inicial de 4 cm, sem superfície invisível ligando as regiões. Visual e física recebem a mesma transformação de cedimento. Cedimento após o estralo: até 7,5 cm.
- Ao passar por 48% da largura apoiada, um único estralo dispara uma pausa local de 80 ms no avanço e no relógio de passos. Gravidade e tempo global continuam. Seguem-se 280 ms de pose procedural de tropeço; ao sair, o movimento normal é retomado. Retry restaura as peças e permite um novo evento único.
- A câmera acompanha a travessia em close e a fadinha voa adiante sobre o tampo. Mantido o afastamento de 5% aprovado e o Game Over sem o efeito eletrônico anterior.

## Evidências e reprodução

`DarkRoomFurnitureSetup.ApplyAndSave` monta o móvel e atualiza a rota incrementalmente, sem executar `DarkRoomScenePopulator` nem regenerar os outros áudios. `DarkRoomFurnitureCapture.Run`, no repositório, gera as imagens e o JSON com `-yunaFurnitureOutput CAMINHO` e os quadros com `-yunaFurnitureFrames CAMINHO`. Use Unity 6000.3.28f1, `-batchmode -projectPath PROJETO -executeMethod YunaLegend.EditorScripts.DarkRoomFurnitureCapture.Run` (sem `-quit`, pois a ferramenta encerra após o Play Mode).

- `00`–`05`: captura real de Play Mode, luzes da cena, câmera ativa, delta de captura de 1/60 s. Apenas abertura/progressão/UI foram desativadas; o buffer de pulo foi acionado nas bordas para reproduzir a entrada e a saída. Não equivale a uma sessão manual completa.
- `07`–`09`: inspeção estática com duas luzes temporárias; `09` força a pose quebrada. Essas luzes não são salvas na cena.
- `live-traversal.json`: amostras de jogador/câmera/apoio/cedimento/pausa/pose/tempo global. Captura de referência: 79 quadros, um estralo, inclinação máxima observada de 13,65°, saída no apoio 03.
- `traversal.gif`: sequência dos quadros reais, 20 fps; imagens sem retoque. Quadros intermediários ficam em `Build/FurnitureCaptureFrames`, ignorado pelo Git, mas são reproduzíveis pela ferramenta versionada.
- `editmode.xml` e `playmode.xml`: **37/37 EditMode e 27/27 PlayMode aprovados**, incluindo repetição sobre a cena principal regenerada. Os testes do móvel cobrem a coincidência tampo/collider, abertura real, dimensão/vãos, evento único, pausa exata de 80 ms, pose, saída, retry e 30/60/120 fps.
- `integration.json`: guarda da integração da cena e hashes. `base-source/` preserva os cinco fontes e a cena anteriores ao móvel; o relatório da base compartilhada está em `../supports-camera-2026-11-11/README.md`.

## Arte, som e limites

Madeira: `Assets/Materials/T_Furniture_Wood_Painted.png`, imagem original gerada com a ferramenta ImageGen. Arquivo de origem local: `C:/Users/sergi/.codex/generated_images/01a1cafb-e557-76c1-bc8a-84603922e127/exec-47dc368a-6cac-4a30-916a-114e5c05a2c7.png`. Direção registrada: textura difusa repetível de madeira castanha e mel, pintada à mão, veios horizontais e nós discretos, luz uniforme, sem ferragens, gavetas, perspectiva, texto ou rachaduras. Este é um resumo da direção; o texto literal completo do prompt não foi preservado no checkpoint.

O novo `wood_furniture_crack.wav` é um efeito original sintético de 0,24 s, mono, 44,1 kHz/16 bits, com ruído impulsivo filtrado e ressonâncias amortecidas. Não foi importada biblioteca externa nem substituído o banco atual de passos. Kenney Impact Sounds e OpenGameArt foram apenas pesquisados como possíveis fontes livres; a escolha de passos gravados fica pendente.

O móvel usa sete renderizadores, quatro grupos estáticos de materiais, dois tampos e farpas. Não houve benchmark de desempenho nem nova comparação jogável com o legado nesta etapa. Ainda há objetos de blockout na rota.

A emenda reta central é provisória. Pedido posterior do usuário, registrado no diário 349: substituir por fratura irregular e localizada, com sinais de fadiga e envelhecimento, veios ramificados, fibras e lascas; evitar um corte perfeito de ponta a ponta ou aparência de montagem ruim. Esse refinamento está anotado para fazer.

**Próxima execução prioritária:** modelar Yuna e Nanda, incluindo pés/rig e animações de corrida, salto, pouso e tropeço, além das asas e voo da fadinha. O tropeço atual inclina o corpo provisório; não é uma animação final de personagem.
