# Auditoria do Piquenique

Em 23/11/2026, a build do Piquenique permitiu abrir o menu e jogar, mas o piloto QA parou em 4%. A introdução deixa Yuna sem apoio na escada e o HUD não mostra corações nem setas discerníveis. A análise do Core identificou problemas também em trechos posteriores. O plano de correção está em [PLANO-DE-ACAO.md](PLANO-DE-ACAO.md).

**Retomada em 24/11/2026:** a coleta pendente foi concluída pelo IAB no contexto root, com revisão independente das imagens. Foram registrados os nove setores, as três rodas aéreas individualmente, o trecho final, os 14 quadros do encerramento e o retorno ao menu. Há defeitos visuais importantes a corrigir; a fase não está artisticamente aprovada. [Parecer independente](IabAudit/Review/RELATORIO-INDEPENDENTE.md).

## Versão e método

- Projeto Unity `6000.3.28f1`, branch `piquenique`, HEAD da auditoria original `aa3cedc479a1233e71059e48ab902374a6dfed12`.
- Build original: `Evidence/PicnicVisualAudit/WebGL`, servida em loopback na porta 8088.
- SHA256 do WASM original: `5CED249AA5A25A16B4EAF71E71EC19F65FDD93404097F26CCBDDC2399894C971`.
- QA visual independente: Chrome desktop, janela horizontal; `?qa=1&qa_autoplay=1` e outra sessão `?qa=1` para comandos manuais. Sem leitura de fontes, logs ou APIs internas pelo avaliador visual.
- Análise técnica: leitura dos C# atuais e execução do Core original como console .NET 8. O harness não executa `CharacterController`, renderização, áudio ou entrada Unity.
- O roteiro local [Um Dia no Jardim](../../docs/roteiro-unity/05-CAPITULO-2-PARTE-1-UM-DIA-NO-JARDIM.md) orienta comportamento e narrativa. Sua descrição antiga de arte 3D deve ser interpretada sob a direção ilustrada vigente de AGENTS.md.

Os resultados originais em `TechnicalAudit/results.json` foram obtidos antes da calibração de saltos solicitada depois pelo usuário. Não representam um teste da implementação calibrada.

## Cobertura da build original

| Parte | Cobertura | Resultado |
| --- | --- | --- |
| Menu e entrada | Visual WebGL | Novo jogo, seleção do modo fácil e abertura funcionaram |
| Introdução | Visual WebGL | Continuar e Pular cena funcionam; apoio de Yuna incorreto |
| Percurso automático inicial | Visual WebGL | 1% até 4%; piloto para antes do primeiro rio |
| Percurso manual inicial | Visual WebGL | Até 1%; duas tentativas de vencer o primeiro vão falharam; salto isolado confirmado |
| Pausa e menu | Visual WebGL | Pausa, Continuar e Voltar ao início funcionam |
| Derrota e retry | Visual WebGL | Modal e botão funcionam; reinício muda a posição de Yuna |
| Fácil e resgate por queda | Visual WebGL | Nanda se aproxima, mensagem aparece e Yuna retorna à borda da ilha |
| Nove checkpoints | Core sintético | Os nove retries começam apoiados |
| Rios posteriores e ilhas aéreas | Core sintético | Encontrados outros bloqueios do piloto; saltos locais com comandos normais conseguem atravessar alguns deles |
| Nuvens | Core sintético | Desaparecimento normal/fácil e retorno após quatro segundos verificados |
| Pânicos e formigas de rio | Core sintético e leitura | Fim antecipado do primeiro pânico; formigas sem arco de salto |
| Rodas em execução Unity | Sem cobertura visual posterior | Exigir teste por fase angular e comparação apoio/corpo |
| Encerramento | Core sintético e leitura | Gatilho Ending alcançável em setup final; cinemática e campanha não validadas visualmente |
| Áudio e aparelho móvel | Sem avaliação | Não declarar qualidade, audibilidade ou desempenho |

Os setups sintéticos posicionam cópias da simulação por reflexão exclusivamente no harness de auditoria. Eles não contam como partida contínua nem como aprovação visual. As tentativas manuais por automação não demonstram que o primeiro vão seja impossível para uma pessoa.

## Achados observados no navegador

### V01 Yuna flutua na introdução

Os sapatos ficam à direita do patamar superior, sem degrau sob as solas. A posição persiste após Continuar. No gameplay inicial, os pés aparecem apoiados na escada: são duas apresentações distintas.

Reprodução: Novo jogo → observar primeiro quadro → Continuar. Esperado: Yuna apoiada no degrau iluminado, com escala e contato preservados durante a atuação.

Evidências: [abertura ampla](RetryDesktop/06-opening-wide.jpg), [detalhe dos pés](RetryDesktop/07-opening-feet.jpg), [após Continuar](RetryDesktop/08-opening-continued.jpg), [escada no gameplay](ContinuationDesktop/01-start-manual.jpg).

Causa confirmada por leitura: `PicnicNarrativeController.cs:22` usa retângulo normalizado independente da pintura da escada; linha 45 desloca verticalmente a imagem inteira por seno. O cenário de fundo ocupa outro retângulo. Não há vínculo de apoio entre sola e degrau.

### V02 Piloto automático para em 4%

Yuna permanece na última ilha gramada antes das pedras do primeiro rio. Duas observações separadas por cerca de 45 segundos e outra após Pausa → Continuar mantiveram o progresso.

Evidências: [progresso parado](RetryDesktop/02-progress.jpg), [após retomar](RetryDesktop/04-blocked-after-continue.jpg).

A reprodução numérica T01 abaixo separa o defeito do piloto da capacidade de salto do jogador.

### V03 HUD sem vidas e direções legíveis

O HUD mostra percentual, nome da seção e Pausa, mas não mostra corações discerníveis. Os dois controles inferiores esquerdos são quadrados escuros sem setas legíveis. A dica inicial também perde símbolos direcionais.

Evidências: [HUD](RetryDesktop/02-progress.jpg), [início manual](ContinuationDesktop/01-start-manual.jpg), [modo fácil](ContinuationDesktop/05-easy-mode.jpg).

`PicnicHudController.cs:38–39,52` usa glifos Unicode para setas, corações e infinito; `PicnicUi.cs:42` seleciona `LegacyRuntime.ttf`. A falta de glifos é uma hipótese de causa a verificar na fonte e na build, não um diagnóstico confirmado por inspeção de fonte.

### V04 Retry muda o ponto inicial

Após derrota no primeiro vão, Tentar novamente retorna a 0%, no chão à esquerda da casa. Novo jogo havia colocado Yuna nos degraus. O botão responde, mas a continuidade espacial muda.

Evidências: [derrota](ContinuationDesktop/02-fall-first-gap.jpg), [retry](ContinuationDesktop/03-retry-reset.jpg), [segunda tentativa](ContinuationDesktop/04-jump-button-direction.jpg).

O Core confirma a diferença: spawn `X=3.38`, retry da seção inicial `X=1`. Os nove pontos de retry são apoiados na simulação; a questão aqui é o contrato do ponto de retomada e a correspondência com a escada.

### V05 Resgate fácil funciona no trecho observado

A queda aciona ESTOU AQUI, Nanda se aproxima e Yuna retorna ao chão com transparência temporária. A posição observada é perto da borda de onde caiu; validar margem de segurança contra repetição da queda.

Evidências: [resgate](ContinuationDesktop/07-easy-outcome.jpg), [retorno](ContinuationDesktop/08-rescue-landed.jpg).

## Achados reproduzidos no Core original

Fonte dos números: [results.json](TechnicalAudit/results.json). Reprodução:

```powershell
dotnet run --project Evidence/PicnicVisualAudit/TechnicalAudit/TechnicalAudit.csproj --configuration Release
```

Esse comando recompila o Core presente no checkout; depois de alterações ele produz uma execução nova. Preserve o JSON original para comparação.

### T01 Predição de salto do piloto diverge do voo real

| Frequência | Posição após 900 segundos nominais | Apoio | Comando final |
| --- | --- | --- | --- |
| 30 Hz | 22.763360 | primeira-grama-relevo-4 | Axis 0, Jump false |
| 60 Hz | 22.763360 | primeira-grama-relevo-4 | Axis 0, Jump false |
| 120 Hz | 22.753359 | primeira-grama-relevo-4 | Axis 0, Jump false |

Cada execução conserva três corações, cinco saltos e zero quedas. O piloto prevê o salto usando velocidade da grama, 1 m/s; a simulação passa a 2,2 m/s no ar. A partir de `X=22.8`, um salto normal chega à primeira pedra em 0,84–0,87 s nas três frequências.

Outras paradas locais aparecem nos suportes 45, 63, 66 e 70. Saltos normais a partir de posições próximas às bordas alcançam respectivamente 47, 64, 67 e 72. Mortes em outros setups do harness são falhas dessas execuções do piloto, sem demonstrar impossibilidade geométrica do trecho.

### T02 Primeiro pânico acaba antes da margem

`panic_ended_0` ocorre em `X=116.51098`, ainda na última pedra, suporte 32. A margem `trecho-0-clareira` começa em `117.7`. A regra exige apenas ultrapassar `PanicEnds` e estar apoiada, sem identificar a margem segura.

O segundo limite, `210.5`, também antecede a margem `trecho-4-clareira`, que começa em `213.78`. A travessia sintética desse segundo rio terminou em queda antes da margem; corrigir o piloto antes de atribuir a falha exclusivamente à geometria.

### T03 Formigas dos rios não fazem arcos

Nas posições de origem 203.46, 376.8 e 431.58, as formigas oscilam em X, mas mantêm Y exatamente em 0.74, 0.82 e 0.86 por dez segundos. Falta a variante pedra → pedra prevista no roteiro.

### T04 Contratos básicos positivos

- Nove retries sintéticos começam apoiados.
- Nuvem remota: deixa de apoiar aos 2,8 s no Normal e 3,8 s no Fácil; reaparece aproximadamente aos 6,8167/7,8167 s, incluindo quantização de atualização.
- Partindo sinteticamente de `descanso-final`, o pânico final dura seis segundos e `Ending` começa em `X=474.8835`, sem queda nessa execução.

## Lacunas identificadas por leitura

Estas constatações descrevem o código, sem afirmar que todos os sintomas foram vistos no navegador.

| Área | Constatação | Consequência para a correção |
| --- | --- | --- |
| Corpo e Core | `body.Move` não realimenta a posição Core; erro maior que 3 m provoca reposicionamento; tolerância quadrada 0.025 equivale a cerca de 15,8 cm, superior à skin de 1,5 cm | Medir diferença entre corpo real, sola e apoio em PlayMode; Core verde sozinho não basta |
| Superfícies | Plataforma usa geometria analítica e caixas próprias; o módulo não consome `PlatformSupportProfile` | Conciliar com contrato de apoio do projeto e inspecionar cada silhueta |
| Água | Falha usa `Y < -2.8`; água é desenhada a -0.8, sem domínio de contato com água | Separar entrada em água de queda no vazio e testar Normal/Fácil |
| Resgate por impacto | Ponto seguro pode ser atualizado no local do impacto antes de resolver o inseto | Recuperar para um apoio anterior com folga, fora do perigo |
| Feedback fácil | HUD só diferencia resgate genérico; COM CALMA não é selecionado para desorientação | Transportar o motivo de resgate em estado/evento puro e apresentar texto correto |
| Atuação sensorial | Personagem usa corrida, pose parada, inclinação e flip; não há seleção específica de caminhada na grama, equilíbrio na borda ou corrida olhando para trás | Implementar estados de apresentação dirigidos por dados sem deformar a arte |
| Cinemática | Abertura não usa pose0; quadro final volta a pose1; apresentação troca poucas imagens e texto, sem descida, aproximação ou fade de 0,45 s | Completar ações por quadro e contato com cenário; revisão visual individual |
| Encerramento | Mesmos retângulos de atores da abertura; troca sentado/dormindo de Yuna, Nanda usa um único asset de repouso; não há trajetória de pouso nem seleção distinta para sono da fada | Validar escala e apoio na toalha e encenar ações previstas |
| Campanha | Complete exibe modal e opção de repetir; não há encaminhamento para Jardim dos Sonhos nessa composição | Conectar ao fluxo de campanha conforme catálogo real, sem inventar fase disponível |
| Cenografia | Água usa sprite de solo tingido/tiled; rodas usam quatro faixas de madeira; câmera mantém Y constante inclusive na região aérea | Revisar água, construção das rodas, leitura de altura e enquadramento após liberar percurso |
| Som | Controller carrega uma música e pausa/retoma; não há assinaturas de eventos de terreno/insetos/resgate nele | Auditar som real e projetar sinais sensoriais com mixagem confortável |
| Texto | Última seção contém literalmente `A grande ?rvore` em `PicnicReferenceLayout.cs:155` | Corrigir o texto de origem e cobrir caracteres acentuados na build |

O cabeçalho do roteiro diz 87 s para o final, mas os 14 tempos individuais somam 84 s, como no Core e no teste existente. Conciliar essa divergência editorial; não adicionar três segundos arbitrariamente.

## Próxima validação

O pedido posterior de medir cada componente e configurar pontos de salto produziu uma calibração específica do piloto QA. Foram medidos [135 apoios](../Picnic/JumpCalibration/support-measurements.csv) e exportados os pontos reais de comando de decolagem e pouso observado por quadro. As quatro execuções novas estão em [calibration-summary.json](../Picnic/JumpCalibration/calibration-summary.json): 30/60/120 Hz completaram sem quedas e com três corações; o caso de frame variável completou sem quedas e com dois corações. Esses resultados não substituem a linha de base acima. A liberação final exige partida contínua em Unity e WebGL, além dos testes numéricos.

## Medidas das bordas e limite da garantia de pouso

Os CSVs de pontos incluem arestas nos instantes observados, desnível e folgas à esquerda/direita do destino, calculados em C# a partir dos descritores. Há pousos cujo centro fica ligeiramente além da aresta, dentro da tolerância de 0,08 m já existente no Core:

| Execução | Pousos com alguma folga negativa | Menor folga |
| --- | --- | --- |
| 30 Hz | 7 | −0,059814453 m |
| 60 Hz | 11 | −0,07345581 m |
| 120 Hz | 14 | −0,06826782 m |
| Frame variável | 8 | −0,068603516 m |

Nenhuma linha ultrapassa a tolerância do Core. Zero quedas demonstra sucesso das travessias testadas, sem demonstrar que a sola inteira permanece dentro da pintura. O tamanho e o ponto de contato da sola precisam da validação visual de A05.

Duas variantes isoladas tentaram exigir folga positiva. Com 0,08 m, as três frequências fixas completaram com folgas positivas, mas o caso variável caiu na primeira roda. Com 0,04 m, o caso variável também caiu e houve uma folga negativa residual a 30 Hz. Ambas foram rejeitadas; a geometria e a tolerância do jogador permanecem as da versão auditada. Evidências em [CandidateSurface](../Picnic/JumpCalibration/CandidateSurface/).

## Continuação visual da primeira build calibrada

O QA independente abriu a build da porta 8089 com Fácil desativado, atravessou o bloqueio de 4%, o primeiro rio, a primeira roda, as nuvens iniciais e chegou ao primeiro pânico, em cerca de 23% do percurso. Foram vistos corações/setas ainda ausentes e água em faixas retangulares escuras com divisões visíveis. Em uma captura de pedra, as solas parecem sobrepor a face frontal; conferir a sequência e o perfil de apoio antes de concluir a causa.

Após a interrupção por quota, a mesma aba foi encontrada em 100%, com modal O PIQUENIQUE e opções Passear de novo / Voltar ao início. [Estado final observado](CalibratedDesktop/18-resumed-complete.png). Não houve reinício pelo avaliador. O HUD mostra literalmente A grande ?rvore, confirmando em runtime o defeito de texto do descriptor.

A observação de 25% a 100% e dos beats finais foi perdida durante a interrupção. O estado de conclusão não substitui essa cobertura visual. Na medição de ritmo, capturas de 5% e 7% ficaram separadas por 47 s reais; isso motivou a investigação de custo do piloto, sem medir FPS por imagens estáticas.

## Otimização das previsões e revisão atual

O piloto passou a reutilizar 80 snapshots em dois buffers, com arrays independentes de criaturas/nuvens e eventos isolados. A medição .NET indicou redução superior a 99,95% nas alocações por percurso. As quatro sequências de comandos e os resultados permaneceram exatamente iguais aos anteriores: [equivalência central](../Picnic/JumpCalibration/PerformanceCandidate/central-equivalence.json).

A revisão com reutilização passou [14 de 14 testes EditMode](PooledValidation/editmode.xml) e [três de três PlayMode](PooledValidation/playmode.xml). A 30/60/120 Hz foram 131/136/131 saltos, zero quedas, três corações e três pânicos. A diferença amostrada entre corpo e Core foi 0 m nos três percursos; esse teste não mede a sola pintada nem revela toda correção interna do adaptador.

A exportação `PooledWebGL` foi compilada com sucesso. SHA256 do WASM: `6CB599478FF0BEF06024F4E8BFC04AC6A58EF796EE1015F1053E78076D4CC132`. [Log da build](PooledValidation/build.log).

Com Fácil desativado e Chrome visível em primeiro plano, o QA registrou progresso de 2% às 03:27:10.776 UTC para 25% às 03:27:57.463 UTC, em 46,687 s reais, incluindo espera e ferramentas. A condição de visibilidade mudou em relação à coleta lenta anterior; o resultado não isola o efeito da otimização nem mede FPS. [Início da medição](PooledDesktop/02-timed-start.jpg) e [fim](PooledDesktop/03-timed-end.jpg).

Foram capturados os trechos de 37% a 56%: Riachos de água clara, pedras do rio, subida, roda e sequência de nuvens de Por cima do jardim. [Roda aérea](PooledDesktop/07-progress.jpg) e [rota aérea](PooledDesktop/08-air-route.jpg). Não foi possível individualizar as três rodas aéreas nas capturas. Depois da interrupção da amostragem, a mesma partida foi encontrada em [100% com modal de conclusão](PooledDesktop/09-current-after-interruption.jpg), sem reinício pelo avaliador.

## Bloqueio anterior de controle do navegador

A revisão de 56% a 100% e dos 14 quadros finais ficou pendente na sessão anterior. Uma repetição focada não produziu capturas e foi encerrada após reavaliação. Outra tentativa foi interrompida pelo Computer Use por não determinar a URL do Chrome com confiança suficiente. Não houve tentativa de contornar essa proteção naquela sessão.

Após novo pedido humano em 24/11, o IAB disponível no root permitiu a coleta sem usar input nativo do Windows. O IAB não estava disponível nos subagentes; por isso o root operou a UI e um avaliador independente revisou exclusivamente as imagens, sem fontes, configuração ou logs do jogo.

## Coleta IAB concluída e revisão independente

Mesma build `PooledWebGL`, hash informado acima; nenhuma recompilação ou correção de gameplay nesta retomada. Viewport padrão de 696 × 730, modo Normal, URL `?qa=1&qa_autoplay=1`. Foram salvos 55 frames da primeira passagem, 148 da segunda, 14 quadros finais e evidências de conclusão/menu. [Método](IabAudit/capture-method.json), [tempos da segunda passagem](IabAudit/replay-captures.json).

A primeira passagem teve lacunas entre lotes. Na segunda, a UI foi pausada com P entre lotes, impedindo avanço enquanto as imagens eram analisadas. Os 14 quadros finais foram avançados manualmente por Continuar, com a timeline pausada. São capturas amostradas de runtime real, não vídeo contínuo nem comprovação de FPS ou das durações originais.

| Cobertura que faltava | Evidência |
| --- | --- |
| Três rodas aéreas individualizadas | [Primeira](IabAudit/replay-0060.jpg), [segunda](IabAudit/replay-0062.jpg), [terceira](IabAudit/replay-0065.jpg) |
| Salgueiros e borboletas | replay-0080–0103, descritos no parecer independente |
| Alameda e rio final | [Últimas pedras](IabAudit/replay-0117.jpg) e replay-0104–0128 |
| Pânico e nuvens finais | replay-0131–0139; besouro aparece nesses frames e não aparece em 0140 |
| Chegada e 14 quadros finais | [Quadro 2](IabAudit/ending-02.jpg), [quadro 5](IabAudit/ending-05.jpg), [quadro 13](IabAudit/ending-13.jpg), além dos demais ending-01–14 |
| Conclusão, repetir, pular encerramento e menu | [Conclusão](IabAudit/complete.jpg), [segunda conclusão](IabAudit/second-complete.jpg), [menu](IabAudit/returned-menu.jpg) |

O parecer independente identificou os nove setores nas imagens. Nas três rodas, Yuna aparece sobre tábuas diferentes e os pés ficam próximos do topo nos stills destacados, sem penetração grosseira evidente. Isso não substitui a análise fina de toda a sola e dos colisores.

### Achados confirmados ou reforçados nesta retomada

- **Final sem apoio:** Yuna sentada permanece suspensa acima do gramado, afastada da toalha; nos quadros 12–14 a pose deitada também fica no ar. Em ending-02 o texto descreve sentir o chão sob ela, sem suporte desenhado que explique a posição.
- **Nanda e ações narradas:** ending-05/10 descrevem pousar e sentar, mas Nanda permanece no ar com asas abertas; ending-13 descreve dobrar asas e se aconchegar, sem pose correspondente. Em ending-06/09/11 não se veem pão/copo nas mãos nas ações narradas. Esses stills comprovam divergência de composição/pose, sem julgar toda interpolação.
- **Minhoca e torrão suspensos:** [replay-0087](IabAudit/replay-0087.jpg) e [0093](IabAudit/replay-0093.jpg) mostram a base marrom aproximadamente 100–112 pixels acima da trilha. Pela imagem, a criatura parece flutuar; não se conclui erro de colisão ou sua causa somente por isso. A leitura técnica posterior encontrou posicionamento genérico `creature.Y + 0.06` no renderer e variação de Y da minhoca como altura de emergência no Core: conferir a semântica e a âncora em A08.
- **HUD:** persistem controles direcionais sem identificação e texto literal A grande ?rvore em 95–100%.
- **Água:** persistem faixas retangulares escuras translúcidas, com leitura fraca de superfície/volume, apesar dos peixes visíveis.

O [parecer completo](IabAudit/Review/RELATORIO-INDEPENDENTE.md) contém prioridades visuais, coordenadas e imagens de reprodução. Abertura, HUD, cenografia, atuação e demais correções continuam no plano. Áudio, FPS, interpolação contínua e aprovação humana de arte continuam fora do resultado desta coleta.
