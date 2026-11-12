# Prontidão para modelagem — Quarto Escuro

11/11/2026 · Codex (GPT-6) · sessão `2026-11-11-codex-prontidao-assets-f813`.

**Decisão: já podemos iniciar a modelagem por um conjunto piloto integrado ao jogo.** A base de câmera, percurso, luz e pós-processamento permite avaliar formas e materiais reais. A produção em lote deve seguir a aprovação desse piloto, porque ainda falta validar linguagem de formas, personagens e integração de malhas próprias.

Esta avaliação usa a skill `.agent/skills/audiovisual-parity-audit/SKILL.md`, as capturas HDR recentes, os XMLs de testes e a inspeção dos geradores. Não é uma nova auditoria de paridade: não foram feitas novas capturas nem jogada Web nesta tarefa. A comparação anterior com o legado tinha cobertura direta de início/castelo; não certifica porta falsa/portal. Nenhum código legado consultado ou importado.

## Releitura das prioridades anteriores

A observação do usuário corrige a interpretação de produção: cápsula/esfera, apoios genéricos e portal provisório representam sobretudo **assets ainda não produzidos**. O rótulo P0 do portal nas entradas anteriores descrevia a gravidade visual da imagem, mas não exige polir seu cubo provisório antes de começar a modelagem. Esta decisão atualiza essa prioridade sem apagar o diagnóstico anterior.

No portal, a causa observada no código é concreta: `DarkRoomBlockoutBuilder.AddPlatformDecorativeDetails` aplica o mesmo `dreamPortalMaterial` às duas colunas cúbicas e ao visual da plataforma. As colunas ainda herdam a escala não uniforme do bloco. Malha própria, separação entre estrutura e superfície mágica, UVs e materiais específicos devem resolver isso na produção do asset. Modelar a forma, por si só, não garante UVs corretos ou transparência sem cantos pretos: esses passam a ser critérios de entrega do portal.

Já existem texturas, materiais e peças de cenografia compostas proceduralmente. O que falta é o conjunto de malhas e personagens próprios da fase. A busca de arquivos encontrou um `Assets/ThirdParty/Models/character-starter.glb`, com DefaultImporter; sua presença não comprova personagem importado, rigado ou integrado. Não foram encontrados prefabs, FBX, Blend, animações ou controllers nessa busca em Assets. Nenhum asset externo foi aberto ou aprovado como solução visual.

## Avaliação pelos seis pilares

| Pilar | Evidência atual | Consequência para modelagem |
| --- | --- | --- |
| Valores e iluminação | HDR/Neutral e luz localizada calibrados; seis estados antes/depois | Base suficiente; recalibrar com materiais finais, sobretudo pele, tecido, asas e emissivos |
| Silhueta e escala | Câmera aprovada; Yuna com CharacterController de 1,4 m; proxies dominam a leitura | Principal trabalho agora: formas reconhecíveis no tamanho real de tela |
| Profundidade | Percurso em Z=0 e cenografia em planos de fundo | Preservar separação e evitar ornamentos tapando aterrissagens |
| Materiais e UVs | Texturas existentes aplicadas a primitivas; portal compartilha material inadequadamente | Definir UVs, densidade de textura e materiais por função no piloto |
| Composição narrativa | Roteiro/catálogo identificam urso, livros, tambor, castelo e destinos | Há conteúdo suficiente para começar; os close-ups tornam a identidade dos objetos decisiva |
| Áudio | Sons/eventos já fazem parte da implementação; não reavaliados auditivamente nesta tarefa | Vincular animação/pouso à superfície prevista e preservar eventos |

Os XMLs existentes foram lidos: 17 EditMode e 21 PlayMode aprovados na tarefa anterior. Isso dá confiança na base, não certifica os modelos futuros nem substitui aprovação visual ou teste de desempenho. Nenhum teste foi reexecutado nesta avaliação documental.

## 1. Preservar como contrato de integração

- Enquadramento próximo ao salto, FOV 45°, panorama tutorial do castelo e retorno ao close aprovados pelo usuário.
- Dimensões e topos das superfícies jogáveis, posições e índices da rota, triggers e fluxo das três etapas.
- CharacterController e locomoção separados da futura malha animada; movimento do root continua controlado pelo jogo. Evitar deslocamento duplicado por root motion.
- Luz localizada, fundo noturno e volume HDR como referência de trabalho; validação final acontece com os materiais novos.
- Afastamento de Nanda e seus alvos. Asas, cabelo e vestido exigirão rever o envelope visual de segurança, hoje dimensionado para os proxies.

O guia antigo `GUIA-PREPARACAO-QUARTO-ESCURO.md` ainda contém câmera fixa apenas em X, anterior à revisão aprovada. Essa regra não deve guiar a nova produção. Também há conversões históricas de coordenadas: utilizar as dimensões em unidades Unity geradas pelo circuito atual e medidas na cena, sem reaplicar o fator do catálogo de design. A altura de Yuna é a régua visual de referência.

## 2. Preparar junto ao primeiro asset

**Integração:** hoje `BuildPlatformList` cria `Visual_3D` como Cube escalado para width/height/depth, mantém BoxCollider nesse mesmo objeto e adiciona detalhes como filhos. `BuildBlockout` apaga e recria os objetos. Uma substituição manual pode ser perdida e uma malha importada sob esse scale pode deformar.

Antes de integrar o piloto, criar ponto de substituição por prefab/identificador do objeto, mantendo um root de gameplay e um filho visual com escala controlada. O colisor deve preservar a superfície testada e acompanhar uma superfície visível coerente; não deve virar um tampo invisível distante da malha. O gerador deve reinstanciar o asset cadastrado e manter fallback para o bloco provisório.

**Ficha técnica mínima por asset:** nome/índice, medidas da superfície de pouso, envelope visual, pivô, orientação, escala de importação, UVs, materiais e partes móveis. Não estipular orçamento arbitrário de polígonos/texturas: medir o piloto na plataforma de destino e só então definir limites para o conjunto. Não foi feito profiling nesta tarefa.

**Personagens:** definir silhueta e proporções de Yuna e Nanda antes do acabamento. Incluir previsão de rig e poses necessárias: repouso/despertar/levantar, corrida, salto/pouso/queda e comemoração para Yuna; voo, indicação e humores para Nanda. Podem começar com malhas de estudo, mantendo o controlador atual.

## 3. Ordem de produção recomendada

| Etapa | Entrega concreta | Critério para avançar |
| --- | --- | --- |
| Preparação curta | Fichas de escala e silhueta de Yuna/Nanda; encaixe de visuais no gerador | Importação preserva medidas e sobrevive à regeneração |
| Piloto de arte | Urso 00, livros 01 e tambor 05, com malhas de estudo dos personagens | Orgânico, madeira/papel e cilindro legíveis sob a luz real; pouso coerente |
| Primeira cena representativa | Piloto no trecho inicial, materiais e animações básicas dos personagens | Reconhecimento no close, ausência de sobreposição, saltos e desempenho aceitáveis |
| Marcos narrativos | Castelo 09, porta falsa 21 e portal 37 | Silhueta e narrativa claras; UVs sem repetição acidental/deformação; portal sem cantos pretos |
| Expansão | Demais plataformas por famílias reutilizáveis: livros, madeira, tecido, metal e brinquedos | Variações preservam identidade, superfície e limites medidos |
| Acabamento | Cenografia secundária, animações complementares e ajuste final de luz | Comparação visual e sonora renovada nos estados relevantes |

O piloto deve ser visto na câmera efetiva do jogo, em movimento e também em cinza. Um render de apresentação em fundo claro não basta. No portal, manter estrutura, área de passagem e efeito mágico separados; não reutilizar o desenho integral do portal nas colunas. Não é necessário terminar todos os personagens para iniciar objetos estáticos, mas sua escala/silhueta deve orientar o conjunto desde o começo.

**Próximo passo recomendado:** preparar esse contrato de assets e produzir o urso 00 como primeira substituição completa, validando-o com um estudo de Yuna na escala do jogo. Livros e tambor completam o piloto para testar famílias diferentes antes da expansão.

## Caminhos de evidência

- `Evidence/value-hierarchy-2026-11-11/comparison-all.png`: seis estados revisados visualmente nesta avaliação.
- `Evidence/value-hierarchy-2026-11-11/README.md`, `editmode.xml`, `playmode.xml`: método e validações anteriores.
- `Assets/Scripts/Runtime/DarkRoom/DarkRoomBlockoutBuilder.cs`: construção, escala, colisor e portal provisórios.
- `Assets/Scripts/Runtime/DarkRoom/DarkRoomThreeStageCircuitGenerator.cs`: dimensões atuais do percurso.
- `Assets/Scripts/Runtime/DarkRoom/DarkRoomPlatformCatalog.cs`: identidade narrativa dos objetos.
- `Assets/Scripts/Editor/DarkRoomScenePopulator.cs`: proxies e controlador de Yuna/Nanda.
- `docs/roteiro-unity/01-CAPITULO-1-PARTE-1-QUARTO-ESCURO.md`: direção narrativa e descrição dos apoios.

Somente documentação e diário alterados nesta tarefa; nenhum asset modelado ou código de jogo modificado.
