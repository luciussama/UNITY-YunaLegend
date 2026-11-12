# Diagnóstico visual — Quarto Escuro

Data: 11/11/2026. Sessão: `2026-11-11-codex-auditoria-visual-4p-8d31`. Agente: Codex (GPT-6).

**Conclusão:** o próximo trabalho visual deve corrigir a hierarquia de luz e a apresentação do portal; depois, substituir as silhuetas provisórias dos personagens e dar identidade às plataformas. A cenografia infantil recente deve ser preservada como base. Não foram feitas correções artísticas nesta auditoria.

**Cobertura:** quatro pontos examinados na Unity; comparação Web direta em dois deles. A porta falsa e o portal da referência Web não foram alcançados. Este documento não certifica paridade integral.

**Versão auditada:** durante a coleta, outro agente concluiu a revisão de gameplay da Etapa 3 (diário #240). A primeira coleta foi preservada em `primeira-coleta/`; a cópia isolada foi sincronizada novamente e as quatro capturas/medições finais foram refeitas com a cena atualizada. A correção de gameplay pertence ao outro agente. `source-snapshot.json` identifica os arquivos efetivamente capturados por SHA-256; este diagnóstico não atribui os resultados daquela revisão à presente sessão.

## Método e limites

- Skill aplicada: `.agent/skills/audiovisual-parity-audit/SKILL.md`, seis pilares e três categorias de diagnóstico.
- Referência acessada exclusivamente em `https://luciussama.github.io/legendGirl/`, por controles visíveis. Nenhum código legado foi consultado, copiado ou importado.
- Navegador em 1280×720; canvas lógico observado de 960×540, apresentado em aproximadamente 1040×585 no retângulo (120,67.5). As capturas JPEG originais preservam o viewport inteiro. O viewport temporário foi restaurado ao final.
- Três tentativas em modo fácil, com pausas para observação. A primeira terminou antes da penteadeira; a segunda, após os blocos; a terceira chegou ao castelo e à fuga, incluindo locomotiva, relógio, abajur e casa de bonecas, e terminou em queda na aproximação ao globo. As derrotas não são evidência de defeito de gameplay.
- Unity 6000.3.28f1, cópia isolada em `Build/VisualAudit_8d31`. A cena fonte não foi regenerada nem salva. O harness entrou em Play Mode e produziu quatro estados controlados com a geometria/material/luzes persistidos. Yuna foi posicionada nos pontos, a visibilidade de etapa foi selecionada e Nanda recebeu a plataforma-alvo. Locomoção, gestor e movimento da câmera ficaram desativados; Nanda e partículas tiveram atualização por aproximadamente 1,8 s em cada estado. Isso permite avaliar a renderização, mas não equivale a jogar o percurso ou validar a física.
- `Camera.Render` exclui overlays `OnGUI`; o diagnóstico não valida diálogo, HUD, pausa ou Game Over da Unity. As imagens Web contêm UI, que não foi considerada equivalente a uma captura sem HUD.
- As vistas têm a mesma proporção 16:9 e correspondência de momento narrativo, mas não registro pixel a pixel. Câmeras, perspectiva, posições aparentes e escala de apresentação diferem.
- Cinza calculado sem aumentar exposição: luma de exibição `Y' = 0,2126R + 0,7152G + 0,0722B`, em 0–255. A prancha reduzida usa 320×180 e desfoque de 1,2 px para inspeção de massas de valor. Isso não é medição fotométrica calibrada.
- ROIs Unity são caixas projetadas de renderers, contendo fundo e possíveis partículas; ROIs Web são retângulos manuais aproximados. As porcentagens não são máscaras exatas dos personagens. Cortes abaixo de 8 e acima de 250 são faixas descritivas deste diagnóstico, não critérios normativos de acessibilidade.
- A importação da cópia isolada registrou `ArgumentOutOfRangeException` no indexador `UnityEditor.Search.SearchDatabase`. As quatro capturas foram concluídas; não houve erro de compilação no harness. A captura inicial também permitiu uma derrota antes de congelar o estado; o harness foi corrigido para inicializar antes e chamar `ResetPlayer`, e as quatro imagens finais foram refeitas. O log final não registra Game Over.

## Cobertura por ponto

| Ponto | Unity | Referência Web | O que se pode concluir |
|---|---|---|---|
| Início | `unity-01-start.png` | `web-01-start.jpg`, abertura e salto | Comparar distribuição de luz, silhuetas e primeiro alvo; posições exatas diferem |
| Castelo / início da fuga | `unity-02-castle.png` | `web-02-castle.jpg`, `web-02-escape-start.jpg` | Comparar personagens, castelo e primeiro alvo da fuga |
| Aproximação da porta falsa | `unity-03-false-door.png` | Não alcançada | Diagnóstico local de forma, material e enquadramento; paridade pendente |
| Aproximação do portal | `unity-04-portal.png` | Não alcançado | Defeito local de apresentação comprovado; desenho canônico final pendente |

Na Unity foram usados apoio/alvo `-1/0`, `9/10`, `20/21`, `36/37`. Posições de câmera: `(-6.5,-4.5,-16)`, `(42.5,-1.5,-16)`, `(101.34272,-1.5,-16)`, `(15.72667,-3.5,-16)`; FOV 45°. Posições de atores, caixas projetadas, meshes, materiais, emissões e cinco luzes constam integralmente em `unity-metadata.json`.

## Categoria 1 — O que preservar

1. **Direção de penumbra e foco narrativo em Nanda.** O Web observado também mantém grandes áreas escuras e concentra luz na fada, na menina e no apoio próximo. O objetivo não é tornar o quarto uniformemente claro.
2. **Vocabulário do fundo infantil:** listras noturnas, bandeirinhas, rodapé, janelas com lua crescente, quadros e adereços reconhecíveis. As vistas comparadas confirmam a pertinência desses motivos. Isso não comprova igualdade exata de posição, escala ou cor.
3. **Cenografia de parede e piso recém-implementada:** é uma base útil para polimento; não é o principal bloqueio de leitura constatado. O cenário não deve competir com a rota.
4. **Separação entre decoração e colisão.** Mudanças futuras de silhueta devem atuar em filhos visuais, conservando o circuito físico até uma validação própria de gameplay. Esta auditoria não modifica nem certifica a precisão dos saltos.
5. **Implementação nativa Unity/C#/URP.** As diferenças devem orientar arte e configuração próprias. Não há necessidade de trazer a implementação Web.

Áudio e aprovação de gameplay não foram revalidados aqui. Os 33 testes aprovados da sessão anterior são históricos e não cobrem qualidade artística.

## Categoria 2 — O que precisa trabalhar

### V01 — Pós-processamento não está ativo na câmera (confirmado)

`Assets/Scenes/DarkRoom_Gameplay.unity:231` serializa `m_RenderPostProcessing: 0`; o metadata confirma `postProcessing: false` nas quatro vistas. `DarkRoomScenePopulator.cs:40` obtém/adiciona `UniversalAdditionalCameraData`, mas não habilita essa propriedade. `DarkRoomArtAssetBuilder.cs:1607` configura Bloom, Vignette e Color Adjustments no Volume; sua mera existência não comprova que os efeitos chegaram à imagem.

**Encaminhamento:** validar a ligação câmera → renderer → Volume e produzir comparação A/B mantendo os mesmos quatro estados. Recalibrar os valores somente depois de verificar a imagem resultante; ativar os valores atuais sem revisão pode alterar fortemente o aspecto aprovado. O diagnóstico não afirma que ligar Bloom, isoladamente, resolva a iluminação.

### V02 — Hierarquia de valores desigual: menina escura, fada branca e apoios com picos (confirmado nas capturas)

| Região Unity | Mediana Y' | P90 Y' | Pixels com algum canal ≥250 |
|---|---:|---:|---:|
| Yuna / início | 18,43 | 25,76 | 0% |
| Nanda / início | 15,29 | 255,00 | 44,74% |
| Plataforma 0 / início | 15,85 | 245,83 | 20,50% |
| Yuna / castelo | 19,72 | 51,34 | 0% |
| Nanda / castelo | 255,00 | 255,00 | 61,81% |
| Plataforma 10 / castelo | 10,50 | 132,59 | 5,11% |
| Yuna / porta falsa | 17,71 | 31,43 | 0% |
| Plataforma 20 / porta falsa | 6,71 | 22,92 | 0% |
| Yuna / portal | 19,72 | 26,59 | 0% |
| Portal / plataforma 37 | 7,72 | 227,36 | 11,37% |

As caixas incluem fundo; por isso mediana baixa e P90 alto de Nanda não são contraditórios. No início, aproximadamente 44,52% da caixa de Nanda tem **todos** os canais ≥250; no castelo, 61,81%. Isso comprova pixels brancos sem detalhe na captura, não um limiar universal de reprovação.

Na referência, as ROIs aproximadas da menina apresentam P90 de 102,77 no início e 94,28 no castelo; Nanda, 182,23 e 189,84. As diferenças apoiam a observação de que a fada Web retém forma e cor enquanto a Unity vira um ponto branco; não são proporções calibradas de luminância entre engines.

**Causas e condições verificadas:** `NandaFairyGuide.cs:17` define intensidade 6 e alcance 11,5 m; `:109` desloca a fada até o alvo +1,2 m. No início, a distância entre as origens da menina e fada é 12,137 m, acima desse alcance. A luz local de Yuna permanece em 0,55/range 2,6 (`DarkRoomScenePopulator.cs:203`). A emissão de Nanda é HDR 3,8× (`DarkRoomArtAssetBuilder.cs:113`). O mapa chamado de contorno (`:221`) depende de bordas UV; isso não garante um contorno de silhueta no espaço da câmera, especialmente na cápsula.

**Encaminhamento:** equilibrar a luz que guia o olhar, o preenchimento da menina e a superfície do próximo pouso; preservar gradação e cor em vez de um topo amarelo/branco achatado. Verificar iluminação/UV/material por caso, sem elevar a luz global do quarto. Definir um contorno visual apropriado à malha final da menina. Fazer a validação com os mesmos estados, incluindo a aproximação inicial distante da fada.

### V03 — Portal com textura repetida, deformação e cantos pretos (confirmado; P0 local)

`unity-04-portal.png` mostra o mesmo vórtice achatado no piso e repetido nas colunas, com retângulos escuros ao redor. A silhueta resultante não forma uma passagem única e legível.

**Causa:** `DarkRoomBlockoutBuilder.cs:580` cria toda plataforma como cubo; `:656` resolve o material da plataforma 37 como `dreamPortalMaterial`. Em `:718–719`, as duas colunas também usam esse mesmo material, sem geometria de arco superior. `GenerateDreamPortalTexture` (`DarkRoomArtAssetBuilder.cs:1514`) cria uma imagem circular com exterior `Color.clear`; `M_DreamPortal_Gold` (`:110`) é criado sem alpha clip, e o material é aplicado a faces de proporções diferentes. O exterior transparente da imagem não produz automaticamente um recorte no material opaco.

**Encaminhamento:** separar estrutura do arco, superfície interior do portal e apoio físico; mapear o vórtice somente à superfície destinada a ele; usar recorte/transparência deliberada e UV coerente. Preservar a posição/colisor da plataforma nesta tarefa visual. Validar o desenho final contra a referência quando o ponto Web puder ser observado; esta coleta comprova o defeito local, não o design canônico do portal.

### V04 — Personagens ainda têm silhuetas de protótipo (confirmado)

O metadata registra `Body_Visual = Capsule`, laço `Sphere`, e `Sparkle_Body = Sphere`. `DarkRoomScenePopulator.cs:176–194` chama o corpo de visual provisório; `:250–255` cria a esfera da fada. Nas capturas não há rosto, cabelo, membros ou asas reconhecíveis nos atores jogáveis. Pequenas figuras no piso são props; não devem ser confundidas com a menina jogável.

Na referência observada, menina e fada têm silhuetas ilustradas e expressões identificáveis. Em miniatura, a cápsula escura perde a leitura humana; aumentar apenas sua luz não fornece essa identidade.

**Encaminhamento:** criar visual final de Yuna e Nanda coerente com a direção pastel, com pose e leitura lateral, mantendo os componentes de controle separados dos filhos visuais. Começar com vistas de repouso, corrida, salto e pouso; validar o tamanho aparente e o contraste nos quatro pontos.

### V05 — Plataformas reconhecidas principalmente como caixas (confirmado nas vistas e no construtor)

`BuildPlatformList` cria o corpo visual como `PrimitiveType.Cube` (`DarkRoomBlockoutBuilder.cs:580`). O método `ResolveMaterialForSpec` troca materiais; `AddPlatformDecorativeDetails` acrescenta detalhes a parte dos índices, mas não resolve a identidade de muitos apoios da fuga e do retorno. A locomotiva observada no Web, por exemplo, tem forma identificável; a plataforma 10 capturada na Unity é um bloco com textura de trilho.

**Encaminhamento:** priorizar as silhuetas do urso, castelo e primeiros apoios da fuga, depois a etapa de retorno. Modelar detalhes visuais sem introduzir saliências físicas na faixa Z=0. Medir se a superfície útil de pouso permanece clara e coerente com o colisor; não alterar os saltos como parte de uma correção artística.

### V06 — Porta falsa tem leitura horizontal e discreta (confirmado localmente; paridade pendente)

Na vista `unity-03-false-door.png`, a caixa total projetada do alvo mede aproximadamente 237×93 px. A representação da porta fica no lado de uma plataforma baixa, com baixo detalhe e contraste. `DarkRoomThreeStageCircuitGenerator.cs` define a plataforma final da fuga com largura 3,8 e altura 1,4; o construtor aplica a imagem quadrada da porta no cubo e acrescenta `Moldura_Poster` (`DarkRoomBlockoutBuilder.cs:712`).

**Encaminhamento:** revisar orientação, proporção e apresentação do pôster como elemento visual independente da superfície física. A forma desejada exata deve aguardar a captura Web desse ponto. Não há base nesta sessão para afirmar uma altura canônica específica.

### V07 — Escala aparente, enquadramento e profundidade precisam de uma segunda passagem (medido parcialmente)

As caixas de Yuna na Unity têm altura de 78–85 px em 720p. Sua dificuldade de leitura não decorre apenas de ser pequena. Na aproximação da porta e do portal, os atores e apoios se concentram na parte superior, com grande área inferior de parede/piso vazio.

O tamanho de mundo também não garante proporção aparente: a janela de 4,15 m em Z=6,2 e a menina de aproximadamente 1,4 m em Z=0, com câmera Z=-16, produzem uma razão projetiva aproximada de `4,15/1,4 × 16/22,2 ≈ 2,14`. Isso difere da razão de mundo ≈2,96. A escala precisa ser julgada junto da perspectiva e da malha final, sem repetir os aumentos exagerados de janela registrados no histórico.

**Encaminhamento:** validar tamanho aparente usando a menina final como régua. Revisar presets fixos por seção somente dentro das restrições do projeto; não adicionar acompanhamento vertical reativo a saltos. A captura da Unity é controlada e não comprova a composição ao longo de toda a corrida.

### V08 — Sombras suaves de luzes locais não estão comprovadas (configuração confirmada)

`DarkRoomScenePopulator.cs` solicita `LightShadows.Soft` nos spots lunares, mas `Assets/Settings/DarkRoomPipeline.asset:49` registra `m_AdditionalLightShadowsSupported: 0` e `:66`, `m_SoftShadowsSupported: 0`. `DarkRoomRenderPipelineSetup.Configure` não ativa essas opções nos assets existentes.

**Encaminhamento:** verificar a configuração efetiva e custo das sombras antes de afirmar que o luar projeta sombras suaves reais. O feixe translúcido e a iluminação do piso, por si, não são prova de sombra física. Esse ajuste deve ser avaliado junto da hierarquia de luz, preservando a suavidade desejada.

## Categoria 3 — Prioridade de execução

| Ordem | Prioridade | Trabalho | Evidência de aceitação |
|---|---|---|---|
| 1 | P0 | Validar caminho de pós-processamento e equilibrar Nanda/Yuna/apoio (V01–V02) | A/B nos mesmos quatro estados, cores e cinza; menina reconhecível, fada com gradação e apoio sem área chapada dominante; penumbra preservada |
| 2 | P0 | Corrigir apresentação do portal (V03) | Um único portal legível; textura sem repetição nas colunas, sem vórtice achatado no apoio e sem cantos pretos indevidos |
| 3 | P1 | Visual final de Yuna e Nanda (V04) | Silhuetas laterais reconhecíveis em 320×180, poses de corrida/salto/pouso e contraste consistente |
| 4 | P1 | Identidade dos apoios e leitura da porta falsa (V05–V06) | Forma do objeto reconhecível, topo legível e correspondência visual com a colisão; completar referência Web dos pontos finais |
| 5 | P2 | Escala aparente, composição por seção e sombras (V07–V08) | Medidas em tela com malha final; presets permitidos pelo roteiro; sombras verificadas no render efetivo |
| 6 | P3 | Polimento de UV, densidade de textura e props de fundo | Só após estabilizar personagem/rota; preservar elementos de parede e piso já aprovados |

Não se recomenda adicionar uma nova camada de mobiliário antes de resolver os achados prioritários. Não foi realizado ajuste corretivo nesta sessão.

## Checklist dos seis pilares

| Pilar da skill | Resultado desta coleta |
|---|---|
| Hierarquia de valores / iluminação | Examinada nos quatro snapshots; diferenças diretas de Web em início/castelo; V01–V02 |
| Silhueta / escala | Cápsula/esfera e apoios retangulares comprovados; escala aparente parcialmente medida; V04–V07 |
| Profundidade / paralaxe | Z e perspectiva considerados; deslocamento contínuo da câmera não medido quantitativamente; sem certificação de equivalência de paralaxe |
| Paleta / UV / materiais | Motivos e paleta-base pertinentes; portal e porta com problemas locais; igualdade cromática não calibrada |
| Composição / narrativa | Elementos recentes preserváveis; contraste e identidade da rota precisam prioridade |
| Áudio / sincronismo | Não auditado: pedido atual visual; nenhuma mixagem ou sincronização certificada |

## Evidências e reprodução

- `comparison-start-castle.png`: Web à esquerda, Unity à direita; duas correspondências narrativas, sem registro pixel a pixel.
- `unity-four-points-color.png` e `unity-four-points-gray.png`: quatro estados em cor e cinza.
- `unity-*-roi.png`: caixas usadas nas medições.
- `unity-*-gray.png` e `unity-*-squint.png`: inspeção de valores em resolução integral e reduzida.
- `web-*.jpg`: capturas brutas de UI, tentativas e percurso observado. Somente início/castelo constituem correspondências dos quatro pontos solicitados.
- `unity-metadata.json`: câmeras, posições, alvos, renderers, materiais, emissões e luzes.
- `measurements.json` e `luma-measurements.csv`: medições completas e limitações do cálculo.
- `source-snapshot.json`: hashes dos arquivos da cópia auditada e comparação com o workspace na consolidação.
- `capture.log`: log da execução final e conclusão das quatro capturas. Os avisos do indexador das execuções anteriores estão preservados em `primeira-coleta/capture.log`.
- `tools/VisualAuditCapture.cs.txt`: harness executado na cópia isolada; não faz parte da implementação do jogo.
- `tools/analyze_captures.py` e `tools/requirements.txt`: reprodução das pranchas e medições.
- `manifest.json`: inventário de arquivos, tamanhos e SHA-256 (exclui o próprio manifest para evitar autorreferência).

Reprodução da análise, na raiz: `python -m pip install --target Build/VisualAuditTools -r Evidence/audit-2026-11-11/tools/requirements.txt`, seguido de `python Evidence/audit-2026-11-11/tools/analyze_captures.py`.

As imagens são a evidência visual primária. Metadados e medidas sustentam o diagnóstico, mas não substituem a avaliação em movimento. Permanecem pendentes a comparação Web da porta falsa/portal e a validação de movimento, HUD e áudio.
