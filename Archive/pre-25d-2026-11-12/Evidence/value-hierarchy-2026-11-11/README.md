# Hierarquia luminosa e afastamento de Nanda — 11/11/2026

Implementação nativa Unity/C#, sessão `2026-11-11-codex-luz-nanda-7bc2`, Codex (GPT-6). Continuação do diagnóstico audiovisual anterior e da aprovação do pós-processamento. Não foi realizada nova sessão de comparação Web; a única referência legada autorizada permanece a URL jogável. Nenhum código legado foi consultado ou importado.

## Resultado implementado

- **Nanda:** cálculo em LateUpdate, após a locomoção. O alvo respeita o sentido atual de Yuna, com antecedência mínima de 1,4 m. Oscilação lateral/vertical perto da plataforma e órbita curta X/Y/Z ao acompanhar a menina. Uma elipse em XY envolve a cápsula de Yuna, a fada e uma margem visual: raios 0,95 m e 1,2 m no tamanho atual. Quando próxima (até 2,5 m horizontalmente), Nanda também reserva o lado dianteiro da silhueta. A separação é aplicada tanto ao alvo quanto ao resultado da suavização; a velocidade é zerada quando há correção. Nenhum colisor físico foi adicionado.
- **Yuna:** a luz de preenchimento antes estava dentro do corpo. Movida para posição local (-0,6; 1,25; -1,6), intensidade 1,6, alcance 3,2, sem sombras. Vestido/pele recebem emissão mínima constante (0,055; 0,045; 0,04), permitindo leitura sem clarear o quarto inteiro.
- **Nanda/luz:** intensidade base 6 → 2,2; alcance 11,5 → 7. Multiplicadores de humor preservados. Referência guideLight e valores também persistidos na cena.
- **Nanda/material:** emissão (3,8; 3,268; 1,444) → (1,6; 1,2; 0,6), base dourada suave, mantendo variação da textura em vez de disco branco uniforme.
- **Apoios:** smoothness 0,12 nos materiais com mapa de contorno, iluminação local menos agressiva. No shader Simple Lit deste projeto, `_SpecularHighlights=1` significa **sem especular**; preservado esse estado com keywords corretas. A redução principal dos picos vem da iluminação de Nanda. Não confundir esse enum com o parâmetro equivalente do shader Lit.

Configurador idempotente: `Assets/Scripts/Editor/DarkRoomValueHierarchySetup.cs`, menu **YunaLegend/Calibrar hierarquia luminosa**. Integrado ao gerador de materiais e ao ScenePopulator para manter as escolhas em regenerações futuras. O trabalho executou apenas o configurador localizado, sem chamar Populate geral nem regenerar áudio/arte.

## Evidências e método

- `before/` e `after/`: seis PNGs HDR por versão, 1280×720, e metadata de atores, câmera, luzes, materiais e caixas projetadas.
- `comparison-close.png`: antes/depois no castelo. `comparison-all.png`: largada, castelo, porta falsa, portal, solo após queda e panorama tutorial.
- `analyze.py` e `metrics.json`: luminância SDR descritiva, percentil95 e porcentagem de pixels com qualquer canal >=250. Caixas AABB incluem fundo; não são máscaras exatas de personagem. Não há normalização de exposição entre imagens.
- `pose-check.json`: Yuna/FOV iguais nos seis estados; câmera exatamente igual em cinco. Panorama tem diferença de 0,004 m por término da suavização em instantes de render distintos. Nanda e partículas animadas diferem entre execuções; imagens comparam o resultado conjunto de luz e voo, não isolam uma única variável.
- Renderização com `DarkRoomStartCapture.RenderDisplayImage`: intermediário ARGBHalf linear, conversão SDR sRGB antes do PNG. Pós-processamento aprovado mantido.
- Capturas em Play Mode controlado: locomotor/manager desabilitados e poses preparadas; Nanda e partículas ativos. Não são vídeo de travessia nem prova de desempenho. Camera.Render não inclui OnGUI.
- `capture-harness.cs.txt`: cópia do capturador executado. O nome herdado PostProcessingCapture e a frase herdada “all three variants” no log são rótulos antigos: nesta tarefa ele renderizou **somente calibrated**, antes e depois. Não existem três variantes novas por estado.
- `first-trial/`: primeiro ensaio visual e XML com a falha recuperada, preservados. O primeiro material deixou Nanda âmbar demais; o primeiro teste revelou atraso que a deixava atrás de Yuna.

### Medidas no close do castelo

| Região/medida | Antes | Depois |
| --- | ---: | ---: |
| Yuna: luminância média na caixa | 32,46 | 59,65 |
| Nanda: luminância P95 | 254,21 | 226,34 |
| Nanda: qualquer canal >=250 | 47,01% | 0,05% |
| Próximo apoio: luminância P95 | 174,56 | 80,97 |
| Próximo apoio: qualquer canal >=250 | 1,73% | 0% |
| Quadro: luminância média | 10,52 | 9,73 |

Na largada, qualquer canal >=250 na caixa do apoio caiu de 5,57% a 0%. O portal ainda contém 0,21% desses pixels na sua caixa; geometria/UV/emissão próprios do portal permanecem um P0 separado. Não há alegação de eliminação absoluta de saturação em todos os canais/cenas.

## Validação real

- **EditMode: 17/17 aprovados**, `editmode.xml`.
- **PlayMode: 21/21 aprovados**, `playmode.xml`, incluindo câmera, percurso, retorno e o novo NandaSeparationTests.
- Novo teste: 150 amostras de aproximação nos dois sentidos, trajetória vertical sintética, alvo de plataforma, retorno à menina e reposicionamento em sobreposição exata. Executa o LateUpdate e verifica a elipse em cada amostra; confirma ausência de colisor e posição adiante ao final. É teste controlado do componente, não uma travessia completa com input humano.
- Compilação final sem erros de C#. Houve CS0234 transitório ao tentar usar uma classe URP.Editor sem referência de assembly; corrigido com as propriedades/keywords equivalentes, sem novas dependências.
- Capturador completou os seis estados. Log contém exceção já observada do indexador UnityEditor.Search.SearchDatabase na inicialização; não impediu os renders nem os testes. Trechos em `validation-excerpts.txt`, logs completos locais em `Build/value-*.log`.
- `git diff --check -- Assets/Scripts`: aprovado. Diário SQLite com integridade e chaves estrangeiras válidas.

## Integração e preservação

`integration.json` registra 24 materiais e somente quatro blocos da cena alterados em relação ao snapshot anterior a esta integração: Transform/Light de preenchimento e componente/Light de Nanda. Os outros 1649 blocos foram comparados e preservados, incluindo câmera e Volume. O diff Git global contém trabalho anterior e alterações de cena já presentes no snapshot; ele não representa somente esta tarefa. Não foi feito reset desse conteúdo.

A validação utilizou projeto isolado em Build/CameraFramingValidation porque o editor do projeto principal estava aberto. Três materiais VFX tiveram fila automaticamente reserializada no projeto isolado; essas alterações não foram copiadas de volta. Os SFX e demais modificações locais anteriores foram preservados. Nenhum commit ou push novo nesta tarefa.

## Categorias da skill audiovisual

1. **Preservar:** close e panorama tutorial aprovados; perfil HDR/Neutral/Bloom; enquadramento, geometria, físicas de salto, áudio e cenografia. Quarto continua escuro e personagem ganha luz localizada.
2. **Ainda trabalhar:** portal repetido/deformado e cantos pretos; proxies cápsula/esfera e identidade dos apoios. A textura visível na fada ainda é a do proxy, não detalhe anatômico de uma personagem final.
3. **Prioridades seguintes:** P0 portal; P1 modelos de Yuna/Nanda e identidade das plataformas; depois polimento visual e nova comparação jogada. Esta entrega cobre a hierarquia luminosa e a prevenção de contato da guia no tamanho atual.

Os seis pilares foram considerados no escopo: valores/iluminação ajustados; silhueta protegida pela zona de afastamento; profundidade e composição preservadas; materiais corrigidos sem nova arte/UV; áudio preservado sem nova auditoria sonora. A comparação com o legado continua baseada no diagnóstico anterior, sem alegar nova paridade completa.
