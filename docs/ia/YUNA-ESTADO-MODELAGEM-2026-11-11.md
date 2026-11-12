# Yuna: estado detalhado da construção em 11/11/2026

> **Atualização de direção em 12/11/2026:** este documento é histórico do estudo 3D. O usuário escolheu personagens 2D ilustrados em cenário com profundidade. Recomendações de continuidade 3D abaixo foram substituídas pelo [plano obrigatório 2,5D](PLANO-25D-OBRIGATORIO.md); não retomar a modelagem 3D final a partir deste checkpoint antigo. Preserve os assets e as evidências.

**EM CURSO.** Sessão `2026-11-11-codex-yuna-modelo-92e3`, plano373. Este documento atende ao pedido do usuário de registrar o estado antes de continuar. A distinção principal é: **o modelo foi produzido e testado no projeto Unity isolado; ainda não foi instalado no projeto principal**. Por isso a cena principal ainda mostra a cápsula.

## 1. Referência e compromisso visual

- Referência aprovada explicitamente: `Evidence/yuna-concept-2026-11-11/yuna-3d-concept-v1.png`. Aceite em `APROVADO.md` e diário368. Prompt original preservado em `prompt.txt`.
- A consulta anterior ao canônico foi feita somente pela URL autorizada, incluindo despertar, menina em pé e primeiro salto. Nenhum código, sprite ou modelo legado foi importado.
- Aparência a preservar: menina delicada de proporções infantis, rosto arredondado e olhos castanhos, cabelo castanho com franja e rabo de cavalo alto, laço rubi, vestido creme com detalhes carmim, meias brancas e sapatos escuros.
- O render conceitual é o alvo artístico. **A malha atual é um estudo inicial, ainda distante do acabamento da prancha.** A aprovação da imagem não significa aprovação automática desta malha, nem que ela esteja pronta.
- Escala de referência: 1,40m na Unity. O controlador físico existente continua com altura1,4, raio0,35 e centroY0,7. O trabalho visual não recalibrou os saltos.

## 2. O que foi construído

### 2.1 Autoria da malha e assets

Fonte principal: `Assets/Scripts/Editor/YunaCharacterModelBuilder.cs`, disponível no projeto principal. A implementação é paramétrica e nativa Unity/C#: cria vértices, triângulos, normais, UVs, pesos, bind poses, materiais e prefab persistentes.

Assets efetivamente gerados **até este checkpoint somente dentro de `Build/CameraFramingValidation/Assets/Characters/Yuna/`**:

- `Yuna_Study.prefab`: visual articulável.
- `Yuna_StudyMesh.asset`: malha skinned, com45.789vértices e83.024triângulos.
- `Materials/`:18slots de materiais na malha, com pele, olhos, cabelo, tecido, laço, meias, couro e metal. Há materiais ainda provisórios e slots a consolidar. Não foi feito benchmark; estes números não são um orçamento aprovado para mobile.
- Altura de repouso medida:1,40m; ponto inferior da sola emYlocal0.
- Um `SkinnedMeshRenderer`, sem colliders próprios e sem novas luzes dentro do modelo.

Partes geométricas já existentes:

| Parte | Construído | Limitação atual |
| --- | --- | --- |
| Cabeça/rosto | Volume próprio com queixo/bochechas, nariz integrado, boca e orelhas | Semelhança e naturalidade ainda precisam de polimento |
| Olhos | Áreas curvas sobre a face, íris/pupila/brilhos e contorno de pálpebra | Leitura ainda muito simples; sem rig facial ou piscar |
| Cabelo | Calota, franja em mechas, laterais e rabo de cavalo volumétrico | Formas ainda geométricas; cachos e transições não reproduzem o acabamento aprovado |
| Laço | Nó, duas abas e pontas | Forma/material provisórios; movimento secundário simples |
| Corpo/vestido | Corpete, saia com pregas, mangas, gola arredondada, babado e vivos carmim | Acabamento de tecido, costuras e continuidade precisam melhorar |
| Braços/mãos | Braços afunilados, palma, quatro dedos e polegar por mão | Dedos seguem o osso da mão; sem articulação individual |
| Pernas/meias | Pernas, joelhos e meias com relevo | Anatomia e dobra do tecido ainda simplificadas |
| Sapatos | Solado, cabedal arredondado, tira e ferragem | Modelagem/material ainda de estudo; cabedal foi corrigido para encontrar o solado |

### 2.2 Esqueleto e skin

26ossos: quadril, coluna, peito, pescoço, cabeça, rabo de cavalo e ponta, laço; braços/antebraços/mãos dos dois lados; coxas/canelas/pés/pontas dos pés dos dois lados; quatro controles de saia.

- Pesos de skin normalizados, com mistura nos braços/pernas e na saia.
- A saia mistura regiões vizinhas, em vez de trocar abruptamente de um osso para outro.
- Proporções do primeiro estudo foram ajustadas: cabeça menos alongada e ombros infantis mais estreitos.
- Não há ainda AvatarHumanoid, retargeting, esqueleto dos dedos, ossos faciais, blendshapes ou sistema de expressões.

### 2.3 Movimento visual inicial

`Assets/Scripts/Runtime/DarkRoom/YunaCharacterView.cs` já está escrito no projeto principal. Produz poses **procedurais de estudo**, não clips finais:

- Repouso com respiração discreta.
- Corrida com braços, pernas e ajuste de altura do quadril.
- Pose de subida do salto, queda e amortecimento de pouso.
- Tropeço com inclinação do tronco e abertura dos braços.
- Movimento secundário simples de cabelo, laço e saia.
- IK de duas articulações para manter a sola na altura do piso durante a fase de apoio da corrida e no pouso.

O relógio visual usa a fase do som de passos já existente. A pausa local de80ms do móvel congela a pose e é seguida pelo tropeço. **A calibração horizontal da passada e a eliminação de deslizamento ainda não estão concluídas.** O teste de sola comprova altura vertical, não ausência de deslizamento em coordenadas de mundo.

O controlador continua responsável por mover o root e o colisor. A view não usa root motion, não altera a trajetória do salto e não substitui a física.

### 2.4 Integração preparada em código

- `YunaCharacterModelBuilder.Install`: troca o filho `Body_Visual` pelo prefab, mantendo o root físico.
- `ApplyToScene`: menu de instalação incremental, usado até agora apenas no clone.
- `DarkRoomScenePopulator`: preparado para preservar/reinstalar o visual autorado quando o prefab estiver disponível, evitando retornar à cápsula numa regeneração.
- `Yuna25DPlayerController`: expõe fase de passos e intensidade do tropeço; restaura a pose/relógio no retry e usa o antigo movimento da cápsula somente quando não há view articulada.
- A view posiciona a luz de preenchimento já existente do lado da câmera para os dois sentidos de corrida; os materiais usam compensação tênue de leitura, abaixo do limiar do Bloom. O ajuste não acrescenta outra luz.
- Harness e teste do móvel foram adaptados para observar o tronco articulado, mantendo compatibilidade com a cápsula.

## 3. O que está sendo construído/validado agora

1. **Encerramento da primeira entrega técnica do modelo:** confirmar as capturas finais e reunir evidências de poses e da travessia real do móvel.
2. **Captura real em PlayMode:** iniciada pelo processoUnity18424, log `Build/yuna-model-runtime-capture.log`, saída prevista em `Evidence/yuna-model-study-2026-11-11/runtime/`. Neste checkpoint ainda não foi lido o resultado final; não afirmar que a captura terminou até verificá-lo.
3. **Integração principal ainda pendente:** copiar a pasta de assets gerados e metadados do clone para o projeto principal; confirmar GUIDs e instalar a cena validada sem sobrescrever outras alterações.

A guarda de cena já foi executada em modo leitura e encontrou apenas a troca esperada:1bloco existente alterado (lista de filhos do rootYuna),8blocos do antigo visual removidos e2blocos do prefab adicionados. A cena principal ainda coincidia com o snapshot `Build/yuna-scene-current-before.unity`. **A guarda passou, mas a cópia/integracão ainda não ocorreu.**

## 4. O que já pode ser visto na Unity

### Projeto principal: `C:/Users/sergi/repos/UNITY-YunaLegend`

**Neste checkpoint, a menina da cena `Assets/Scenes/DarkRoom_Gameplay.unity` ainda é a cápsula.** Os scripts novos existem, mas a pasta `Assets/Characters/Yuna` ainda não foi transferida do clone. Ter scripts no projeto não significa que o modelo já esteja na cena. Abrir ou darPlay no projeto principal ainda não mostra a nova menina.

Também não é possível abrir o prefab novo pelo Project do principal enquanto essa cópia não for feita. O próximo passo de integração deve corrigir exatamente isso e registrar a confirmação visual.

### Projeto isolado: `C:/Users/sergi/repos/UNITY-YunaLegend/Build/CameraFramingValidation`

O prefab está em `Assets/Characters/Yuna/Yuna_Study.prefab` e já foi instalado na cópia da cena de gameplay. Pode ser inspecionado no PrefabMode; emPlay, a view acompanha o controlador e o evento do móvel. **Não abrir esse projeto enquanto o processo de validação/captura estiver usando-o.** O diretórioBuild é ignorado peloGit e serve como ambiente de trabalho, não como destino final dos assets.

### Imagens já acessíveis no projeto principal

Em `Evidence/yuna-model-study-2026-11-11/`:

- `01-front.png`, `02-three-quarter.png`, `03-profile.png`, `04-back.png`, `05-face.png`.
- `06-run.png`, `07-jump.png`, `08-stumble.png`.
- `09-gameplay-bear.png`, `10-gameplay-stumble.png`.
- `model-report.json`, testesXML e instruções de reprodução em `README.md`.

São renders da geometria real, não novas imagens deImageGen. As poses estáticas são amostradas comBakeMesh para evitar cache de pose antiga numa captura síncrona deEditMode. As vistas de inspeção usam luzes temporárias; as imagens de gameplay usam as luzes da cena. A fada foi colocada adiante nas capturas estáticas para não permanecer sobreposta à menina reposicionada.

## 5. Validações efetivamente executadas

- EditMode:45/45aprovados, última repetição em `Build/yuna-model-editmode-verified.log` e `Evidence/yuna-model-study-2026-11-11/editmode.xml`.
- PlayMode:27/27aprovados, repetição sobre o estudo final em `Build/yuna-model-playmode-final.log` e `Evidence/yuna-model-study-2026-11-11/playmode.xml`.
- Cinco testes novos de modelo: escala/skin/rig/normais, deformação/restauro sem mover o root, sola da perna de apoio ao longo do ciclo, duas solas no pouso e luz existente no mesmo lado da câmera em ambos os sentidos.
- Testes do móvel continuam cobrindo80ms, evento único, retry, saída normal e30/60/120fps. Agora verificam a pose do tronco articulado quando há modelo.
- A sincronização do clone precisou incluir os asmdefs e oBuildSettings atuais, pois outro agente acrescentou a tela de entrada. Uma correção pequena em `TitlePhaseSelector.CurrentPhaseScene` fornece o primeiro item serializado antes deStart; isso resolveu a falha detectada por aquele teste, sem redesenhar o menu.
- Não foi feito benchmark, buildWebGL ou validação completa emmobile. Não há comprovação de fidelidade visual final à prancha aprovada.

## 6. Problemas encontrados e correções já feitas

1. Normais inicialmente todas para cima: o produto de tangentes pequenas ficava abaixo doepsilon da normalização. Corrigido normalizando as tangentes antes doCross.
2. Regeneração não persistia os buffers atualizados doMesh existente: passou a copiar explicitamente os buffers e marcar oasset como alterado, preservandoGUID.
3. Tubos de cabelo/braços com orientação e bases que mudavam abruptamente: corrigidas orientação e base de referência.
4. Olhos/nariz pareciam peças salientes: olhos passaram a acompanhar a superfície facial e o nariz foi integrado ao volume.
5. Boca estava dentro da cabeça: reposicionada na superfície facial.
6. Gola sobreposta/degenerada: reconstruída como duas áreas arredondadas com contorno.
7. Sola4mm acima do piso na extensão máxima: alvo ultrapassava alcance da perna; ajustada a altura do quadril durante a corrida.
8. Cabedal elipsoidal parecia se separar do solado no pé levantado: substituído por uma forma que encontra a borda do solado.
9. Poses nas capturas pareciam não mudar: cacheGPU da pose anterior; capturador usaBakeMesh da pose corrente.
10. Captura estática parecia ter luz/objetos dentro do corpo: Nanda permanecia na posição de autoria quando só a menina era reposicionada; corrigido no capturador.

Os renders rejeitados foram sobrescritos durante a iteração. Logs e registros do diário preservam os diagnósticos; não alegar que todas as versões rejeitadas estão guardadas comoPNG.

## 7. O que falta construir, por ordem

| Prioridade | Entrega faltante | Critério concreto |
| --- | --- | --- |
| Agora | Transferir assets e instalar no principal | Prefab visível noProject e menina articulada presente na cena principal, com demais sistemas preservados |
| Agora | Concluir captura dinâmica e documentação | Conferir resultado real, salvar sequência e metadados, fechar o marco técnico sem declarar acabamento final |
| P0 artístico | Rosto mais próximo da prancha | Refinar bochechas/queixo, olhos/pálpebras, nariz/boca e expressão; comparar frente/perfil/3quartos |
| P0 artístico | Silhueta/cachos do cabelo e laço | Menos aparência de mechas geométricas; volumes e movimento coerentes com o conceito |
| P1 | Topologia,UVs e materiais | Consolidar materiais, acabamento pintado de pele/tecido/couro, transições e costuras; medir custo |
| P1 | Mãos/pés e deformações | Melhor anatomia, dedos articulados se necessários, eliminar interpenetrações e refinar sapatos |
| P1 | Animação final da locomoção | Clips autorais e transições de repouso/corrida/salto/queda/pouso/tropeço, passada e sons sincronizados sem deslizar |
| P1 | Expressões e olhar | Piscar, olhar paraNanda/obstáculo, reação de susto e tropeço; rig oublendshapes faciais |
| P2 narrativo | Abertura e reações | Dormir, despertar, levantar, sentar/reagir após queda e comemorar |
| Depois | Ações dos capítulos seguintes | Pegar/carregar objetos, espada, furtividade, lama, folha e escorregadores; não implementadas agora |

Nanda permanece na fila de modelagem. O refinamento da fratura envelhecida do móvel também permanece no backlog, sem substituírem o escopo atual da menina.

## 8. Git e continuidade

O commit pedido antes de começar foi criado: `4ceb98f`. O desenvolvimento posterior ainda está no workingtree e/ou em commits intermediários de outro trabalho no mesmocheckout; não atribuir a esta sessão a autoria da tela de entrada ou dos workflowsPages. Não houvepush nesta sessão.

**Próximo passo após este registro:** verificar a captura em execução, transferir os assets com metadados estáveis, executar a guarda novamente e integrar a versão de estudo no projeto principal. Depois confirmar em evidência o que o usuário passa a enxergar. Atualizar este documento com um adendo factual quando a integração ocorrer; preservar este checkpoint como registro de que ela ainda não estava feita.

## 9. Adendo posterior: primeira versão integrada ao projeto principal

O checkpoint acima foi escrito antes de continuar, como solicitado. **Depois dele, a integração foi executada:**

- Copiados41arquivos de assets/metadados para `Assets/Characters/Yuna`, com metadados das pastas e GUIDs preservados. O prefab e a malha agora existem no projeto principal.
- A cena principal havia sido regenerada novamente pelo editor/outro trabalho. A primeira guarda recusou substituí-la. Foi usada a versão atual como novo baseline, e a instalação incremental foi repetida no clone.
- A guarda final aceitou somente1bloco existente alterado (lista de filhos do rootYuna),8blocos antigos da cápsula removidos e2blocos de referência ao prefab adicionados; demais blocos existentes ficaram idênticos. O snapshot anterior está em `Evidence/yuna-model-study-2026-11-11/scene-before-yuna-integration.unity.gz`.
- A cena validada foi copiada para `Assets/Scenes/DarkRoom_Gameplay.unity` no principal. O hash do arquivo integrado corresponde ao validado. Todos os arquivos de áudio foram preservados porhash durante a integração. Não se copiou perfil de pós-processamento nem se regeneraram sons.
- A captura emPlayMode concluiu79quadros, um estralo, saída no apoio03, inclinação máxima de tronco25,35° e `Time.timeScale=1`. Sequência em `Evidence/yuna-model-study-2026-11-11/runtime/traversal.gif`,640×360/20fps; os quadros originais960×540 são reproduzíveis pelo harness e ficam emBuild. A captura estática do tropeço está em `runtime/04-stumble.png`.

**O que pode ser visto agora:** no projeto principal, abrir `Assets/Characters/Yuna/Yuna_Study.prefab` noPrefabMode, ou reabrir `Assets/Scenes/DarkRoom_Gameplay.unity` para ver/jogar com o primeiro modelo articulável. Se a cena já estava aberta, a janela pode continuar com a versão anterior em memória até recarregá-la; o arquivo em disco já foi atualizado. Não foi tirado um screenshot da janela do editor principal do usuário: as evidências são renders Unity do projeto isolado, e a integração foi conferida por assets/GUIDs/hash da cena e guarda de blocos.

A imagem real do estudo foi enviada inline ao usuário após ele pedir umprint, com aviso explícito de que rosto/cabelo/materiais ainda estão simplificados. A aprovação artística continua pertencendo à prancha original; esta entrega não declara o modelo final aprovado ou concluído. O item7 permanece como fila de construção artística e de animação.

A repetição final dePlayMode sobre a cena atual instalada no clone passou27/27, somada aos45/45EditMode. Após a integração, houve mais uma regeneração externa da cena principal: seusIDs ehashmudaram, mas a referência ao mesmo prefabYuna permaneceu e os arquivos do prefab/mesh principal continuam idênticos aos validados. Essa regeneração posterior foi preservada, sem tentar restaurar o snapshot antigo. `integration.json` registra separadamente a integração e essa observação posterior.

**Marco técnico373 concluído:** primeiro estudo articulável, assets persistentes, testes, captura e instalação principal. **Modelagem artística continua EM CURSO**, com a próxima etapa concentrada no rosto, cabelo e acabamento, conforme a lista do item7.
