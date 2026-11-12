# Yuna — primeira malha articulável

Sessão `2026-11-11-codex-yuna-modelo-92e3`, plano 373. Commit de partida solicitado pelo usuário: `4ceb98f`, com diário, evidências anteriores e conceito aprovado. Commits posteriores de tela de entrada/Pages são de outro trabalho e foram preservados.

## Entrega deste estudo

Malha 3D real, com rig e skin, gerada por autoria paramétrica no Unity Editor. Não é uma imagem aplicada numa placa. O alvo continua sendo a prancha aprovada em `../yuna-concept-2026-11-11/yuna-3d-concept-v1.png`; este estudo técnico ainda não equivale ao acabamento daquela imagem.

- `Assets/Characters/Yuna/Yuna_Study.prefab`: visual com um SkinnedMeshRenderer, 26 ossos, mãos com cinco dedos geométricos, pés, sapatos, meias, vestido, gola, cabelo, rabo de cavalo e laço. Os dedos ainda acompanham o osso da mão; não possuem articulação individual. Não há rig facial nem Avatar Humanoid de retargeting.
- `Yuna_StudyMesh.asset`: 45.789 vértices, 83.024 triângulos e altura de repouso de 1,40 m, com sola em Y local=0. Dezoito slots de materiais; ainda requer consolidação e medição de desempenho. Esses números são observações do piloto, não um orçamento aprovado para mobile.
- `YunaCharacterModelBuilder`: fonte reproduzível da malha, esqueleto, skin, materiais e prefab. O menu `YunaLegend/Yuna/Gerar modelo articulavel de estudo` regenera mantendo os GUIDs dos assets existentes. O menu de aplicar à cena troca apenas o visual.
- `YunaCharacterView`: primeiras poses procedurais de repouso, corrida, subida, queda, pouso e tropeço, além de movimento secundário simples de cabelo/laço/saia. Não são os clips finais de animação. A locomoção continua no controlador existente, sem root motion.
- A fase dos pés usa o relógio existente de passos; a pausa local de 80 ms congela a pose e depois permite o tropeço articulado. A verificação de contato cobre a altura vertical da sola. Deslizamento horizontal e ajuste fino da passada ainda precisam de avaliação e calibração visual.
- A luz de preenchimento existente acompanha o lado da câmera nos dois sentidos. Os materiais têm compensação tênue de leitura, abaixo do limiar do Bloom. Não se acrescentou outra fonte de luz à cena.

## Evidências

`01`–`05`: vistas neutras frente, três quartos, perfil, costas e rosto. `06`–`08`: poses de corrida, salto e tropeço. São renders da geometria real com luzes temporárias de inspeção. O capturador amostra a pose corrente com `BakeMesh` para evitar que uma captura síncrona de EditMode reutilize o cache GPU da pose anterior; o prefab continua sendo skinned.

`09`–`10`: teste visual estático na câmera atual do jogo, com luzes da cena. Nanda foi posicionada adiante da personagem para evitar a sobreposição causada por reposicionar apenas a menina em EditMode. A cena não é salva pelo capturador.

`runtime/`: captura controlada em Play Mode da entrada, travessia, pausa, tropeço e saída do móvel02. A ferramenta versionada é `DarkRoomFurnitureCapture`; a abertura/progressão/UI são desativadas e o buffer de pulo é acionado nas bordas para tornar o percurso reproduzível. O personagem, câmera, fada, móvel e física executam seus updates reais. A sequência não substitui uma sessão manual completa.

`model-report.json`, `integration.json`, XMLs de testes e `manifest.json` registram métricas, guardas e hashes. A integração aceita somente a substituição do filho visual da menina: o transform do pai troca uma referência de filho; o antigo visual de cápsula é removido e entra o prefab. Colisor, rota, câmera, luzes serializadas e demais sons são preservados.

**Estado atual:** assets copiados para o projeto principal e visual instalado em `Assets/Scenes/DarkRoom_Gameplay.unity`. O prefab pode ser aberto pelo Project em `Assets/Characters/Yuna/Yuna_Study.prefab`; uma cena já aberta pode precisar ser recarregada. Testes:45/45EditMode e27/27PlayMode aprovados. A modelagem artística permanece em curso. O checkpoint detalhado, escrito antes da integração e complementado depois dela, está em `docs/ia/YUNA-ESTADO-MODELAGEM-2026-11-11.md`.

Reprodução: Unity 6000.3.28f1, projeto isolado em `Build/CameraFramingValidation`. Execute `YunaLegend.EditorScripts.YunaCharacterModelCapture.BuildAndRun` com `-batchmode -quit -projectPath PROJETO -yunaModelOutput DIRETORIO`; execute `YunaCharacterModelBuilder.ApplyToScene` para instalar o visual numa cópia da cena. Rode EditMode e PlayMode por `-runTests -testPlatform ... -testResults XML`, sem `-quit`.

## Diagnóstico e continuidade

**Preservado:** prancha aprovada, escala de referência, controlador físico, circuito, câmera com afastamento de 5%, pausa do móvel e Game Over sem o efeito eletrônico. Não foi feita nova consulta ao legado nesta sessão; a consulta que orientou o conceito está nas entradas360–366 do diário.

**Ainda precisa trabalhar:** semelhança do rosto com a prancha, olhos/pálpebras e bochechas mais naturais, cabelo com mechas e cachos menos geométricos, tecido e pele com acabamento pintado, transições da gola/babados e refinamento das mãos. As poses ainda precisam de animação autoral, transições e passada com melhor contato horizontal. Dormir, despertar, levantar, expressões, comemoração e outras ações do roteiro não foram produzidas nesta entrega.

**Ordem recomendada:** primeiro aproximar rosto, silhueta do cabelo e proporções da referência aprovada; depois consolidar UVs/materiais e polir pele/tecido/sapatos; depois substituir as poses de estudo por clips e transições, incluindo o tropeço e os passos; medir desempenho antes de multiplicar assets. Nanda continua na fila, sem modelagem iniciada nesta etapa.

Recuperações registradas: normais inicialmente degeneradas pelo epsilon da normalização; buffers antigos do Mesh não persistidos na regeneração; orientação de tubos; gola que se sobrepunha; sola elevada em 4 mm por alvo fora do alcance da perna; captura de pose com cache GPU anterior. Os renders rejeitados foram sobrescritos durante a iteração; os logs e o histórico do diário preservam os problemas e correções. Uma pequena correção no getter de `TitlePhaseSelector` permite ler a fase padrão antes de `Start`, condição descoberta nos testes novos da outra tarefa; o menu não foi redesenhado.

Nenhum código/asset legado foi importado. Nenhum download de ferramenta ou modelo de terceiros foi necessário. Não houve benchmark nem build WebGL nesta sessão.
