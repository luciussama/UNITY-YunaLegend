# Plano de ação do Piquenique para agentes programadores

O piloto QA do Piquenique foi calibrado e a coleta visual pendente foi concluída. As próximas correções prioritárias são o apoio dos personagens na abertura e no encerramento, a correspondência entre poses e narração e a legibilidade do HUD. Este plano organiza o trabalho dos programadores por domínio, com arquivos, dependências e critérios de aceite. Os achados estão em [AUDITORIA.md](AUDITORIA.md).

Foram medidos **135 apoios**. A revisão do piloto com reutilização de previsões passou **14 testes EditMode e três percursos PlayMode**: zero quedas e três corações a 30/60/120 Hz. Na retomada de 24/11, o IAB do root permitiu registrar nove setores, três rodas aéreas, o trecho final e os 14 quadros finais, com revisão independente das imagens. Abertura e encerramento continuam com problemas visuais; a fase não está artisticamente aprovada. [Parecer independente](IabAudit/Review/RELATORIO-INDEPENDENTE.md).

## Coordenação dos agentes

O registro central é `docs/ia/diario.sqlite3`; seguir AGENTS.md, `agent_registry.py` e `plan_25d.py`. Não criar reservas concorrentes para o mesmo assunto nem usar outro checkout como coordenador.

- Auditoria e plano: registro **161**, sessão `codex-20261123-picnic-audit-plan`, encerrado com a entrega desta revisão. Uma auditoria posterior às correções exige revisão explícita vinculada a esse registro.
- Implementação existente: registro **155**, assunto `piquenique/implementacao-v1`, e tarefa **PIC01**. O pedido humano de calibração transferiu ambos para `codex-20261123-picnic-jump-config`.
- O programador atual possui a implementação; os demais responsáveis abaixo são atribuições propostas para a execução posterior. Antes de dividir arquivos entre agentes, combinar recursos mínimos com o dono e registrar os handovers necessários.
- `PicnicSimulation.cs`, `PicnicReferenceLayout.cs`, `PicnicStageBootstrap.cs` e o arquivo de testes atual são recursos compartilhados. Um único editor por arquivo em cada etapa; os demais agentes fornecem contratos e fazem integração sequencial.
- Cada passo exige planned e resultado no diário. Microentregas visuais devem ser apresentadas individualmente para revisão humana antes de avançar à dependente.

Papéis: Engine Programmer para regras e colisões; AI Systems Designer para piloto; Technical Artist e Graphics Programmer para apoios e apresentação; UI/UX para HUD; Narrative Designer para encenação; QA independente para reteste. Producer coordena a ordem.

## Ordem de execução

| Ordem | Entrega | Prioridade | Responsável principal | Depende de |
| --- | --- | --- | --- | --- |
| 1 | A01 Medidas e piloto de salto | P0 | Programador QA e Core | Medidas e código entregues; WebGL chega ao fim; contato fino segue A05 |
| 2 | A02 QA por trechos e telemetria | P0 | Programador QA | Contrato de A01 |
| 3 | A03 Introdução e encerramento apoiados | P1 | Programador de narrativa e Technical Artist | Primeira revisão de apoio; independe do piloto |
| 3 | A04 HUD legível e acessível | P1 | Programador UI | Independente de A01 e A03 |
| 4 | A05 Correspondência física e visual | P1 | Programador de física | A01 para reproduções automáticas |
| 5 | A06 Pânicos e margens de chegada | P1 | Programador Core | A01 e contrato de A05 |
| 5 | A07 Água, resgate e retry | P1 | Programador Core | Contrato de A05; serializar com A06 |
| 6 | A08 Criaturas e formigas de rio | P1 | Programador Core | A05; serializar alterações de Core |
| 6 | A09 Atuação dos terrenos | P1 | Programador de personagem | A05 e eventos definidos pelo Core |
| 6 | A10 Cenografia e câmera | P1 | Programador gráfico | A02 para rever toda a fase |
| 7 | A11 Saída da fase e campanha | P1 | Programador de fluxo | A03 e catálogo real da campanha |
| 7 | A12 Som e sinais sensoriais | P2 | Programador de áudio | Eventos de A06 a A09 |
| 8 | A13 Regressão integral independente | Gate de entrega | QA | Integração das correções |

A03 e A04 podem avançar em paralelo em arquivos diferentes. A06, A07 e A08 compartilham o Core e devem ser integradas em sequência. A13 deve retestar cada microentrega e depois a fase completa.

## A01 Medir apoios e configurar saltos

**Estado demonstrado:** 135 apoios medidos; 30/60/120 Hz chegaram a Complete com zero quedas, três corações e três pânicos, em cerca de 207 s de travessia simulada. Quantidade de saltos: 131/136/131. O caso extra de frame variável 24/40/57/120 Hz também completou sem quedas, com dois corações. Isso não significa ausência de dano em toda condição de execução.

**Dados entregues:** [135 apoios](../Picnic/JumpCalibration/support-measurements.csv), [pontos a 60 Hz](../Picnic/JumpCalibration/jump-points-60.csv), [resumo das quatro execuções](../Picnic/JumpCalibration/calibration-summary.json). Há também arquivos de pontos a 30, 120 e frame variável. Os pontos são amostrados por quadro; as arestas vêm das funções dos apoios. `adjacent-edge-gaps-at-t0.csv` é um inventário de vizinhos espaciais e sobreposições, não uma sequência obrigatória de saltos.

**Limite medido:** 7/11/14 pousos a 30/60/120 Hz e oito no caso variável usam a tolerância de borda de 0,08 m. A menor folga é −0,07345581 m. Não converter zero quedas em promessa de que todas as solas ficam integralmente dentro da arte. As variantes de folga positiva testadas em `CandidateSurface/` falharam com frame variável e não foram aplicadas. A05 deve conciliar contato físico e sola real sem adulterar os números.

**Revisão de desempenho aplicada:** reutilização de 80 snapshots, com quatro sequências .NET idênticas à revisão anterior e redução superior a 99,95% nas alocações medidas. [Equivalência](../Picnic/JumpCalibration/PerformanceCandidate/central-equivalence.json), [EditMode 14/14](PooledValidation/editmode.xml) e [PlayMode 3/3](PooledValidation/playmode.xml). As duas últimas validações pertencem à revisão com reutilização, não somente ao protótipo anterior de 13 testes.

**Problema:** piloto original prevê voo usando velocidade do terreno de origem, diferente da velocidade no ar. O long run para em 4%, e há outras paradas nas ilhas aéreas e em patamares posteriores.

**Arquivos:** `Assets/Scripts/Runtime/Picnic/Core/PicnicQaPilot.cs`, `PicnicSimulation.cs`, novos tipos puros de planejamento, `Assets/Scripts/Tests/EditMode/Picnic/`. Dados de saída em `Evidence/Picnic/JumpCalibration/`.

**Implementação:**

1. Enumerar cada `PicnicSupportSpec`, incluindo os degraus da casa e as 16 plataformas das quatro rodas. Exportar ID, terreno, limites X, largura, altura real nas bordas, inclinação, curva e função temporal.
2. Calcular o vão de **aresta de saída até aresta de entrada**, separado da distância entre centros. Distinguir sobreposição, degrau contínuo e salto. Não ordenar plataformas móveis somente por centro em t=0.
3. Para cada travessia escolhida, registrar apoio de origem/destino, instante/fase, ponto de decolagem, intervalo de pouso seguro, tempo de voo, diferença de altura e margem às bordas. Nas rodas, os pontos devem acompanhar a posição no instante previsto de chegada.
4. Prever comandos com as mesmas regras puras da simulação, incluindo velocidade no ar, troca de terreno, pânico, nuvens, inimigos e movimento das rodas. Se houver cópia para previsão, ela deve ser profunda, não herdar assinantes de eventos nem modificar o estado vivo.
5. Recalcular quando mudar apoio, perigo ou fase relevante. Limitar custo e reaproveitar planos; não fazer buscas extensas a cada frame do navegador.
6. Preservar física, altura de salto, velocidade, obstáculos e arte do jogador. Dificuldade não deve mudar para fazer o piloto passar. Uma travessia sem solução deve ser relatada com medidas e parâmetros.

**Aceite:** tabela completa de apoios e travessias sem valores estimados por aparência; regressões dos suportes 7, 45, 63, 66 e 70; long run desde o início a 30/60/120 Hz, só com comandos normais, até Ending e Complete, três encontros de pânico e nenhuma queda. A promessa de 100% só vale para as condições efetivamente testadas. Exigir também PlayMode e nova WebGL para validar corpo e apresentação reais.

## A02 Cobrir trechos sem esconder a partida contínua

**Arquivos:** piloto Core, `YunaLegend.Qa.Core` e `WebQaRuntime`, adaptador Picnic e testes de opções QA. Serializar qualquer alteração em `PicnicStageBootstrap.cs` com A11.

Adicionar, se necessário, cenários determinísticos opt-in para cada seção e cada fase das rodas, com opção de cenário estritamente dependente de `qa=1`. Registrar início, apoio, decolagem, pouso, queda, dano, resgate, checkpoint e conclusão nos eventos QA, com parâmetros suficientes para reproduzir.

O QA da primeira WebGL calibrada observou progresso de 5% a 7% em 47 s reais e aproximadamente 23% durante a coleta inicial. A reutilização de snapshots já foi aplicada e validada em Unity; medir a versão `PooledWebGL` no navegador. Se continuar lenta, instrumentar separadamente tempo do piloto, GC, frame e renderização no modo QA, mantendo a avaliação visual independente. Não atribuir o custo a uma causa única sem essas medidas.

Na rodada otimizada e com Chrome visível, o progresso de 2% a 25% ocorreu em 46,687 s. Guardar a visibilidade da janela como condição de reprodução: navegador oculto pode limitar atualizações. Ainda falta instrumentação de tempo/FPS para quantificar desempenho. O arquivo `PooledDesktop/09-current-after-interruption.jpg` confirma o estado final; não cobre os eventos perdidos entre 56% e 100%.

**Aceite:** URL sem QA conserva comportamento do jogador; parâmetro de cenário sem `qa=1` não ativa piloto; cenários não alteram dificuldade. Relatórios distinguem partida contínua de início em checkpoint. Um cenário local verde não substitui o long run. Medir tempo de planejamento no WebGL e demonstrar que não congela a entrada ou a renderização.

## A03 Apoiar e encenar as cinemáticas

**Arquivos:** `PicnicNarrativeController.cs`, descritores/timeline puros e testes pertinentes; referências de arte sob coordenação da implementação.

Vincular a sola da Yuna a um ponto do degrau na mesma transformação da pintura. Preservar proporção do fundo e dos atores no redimensionamento. Retirar a oscilação global da posição de apoio; movimento corporal deve conservar a sola quando a pose exige contato.

Mapear as poses 0, 1 e 2 aos nove quadros de abertura, incluindo aproximação de Nanda e passo/descida final. Implementar o fade claro de 0,45 s e a passagem para o personagem físico na escada, com coerência de posição, escala e orientação.

No final, definir anchors próprios para toalha, Yuna sentada/deitada e Nanda pousada/dormindo. Encenar chegada, sentar, pão/suco, descanso e sono conforme os 14 quadros; não reutilizar posições da varanda sem validar o novo cenário.

**Reprodução confirmada na coleta IAB:** [ending-02](IabAudit/ending-02.jpg) mostra Yuna suspensa quando o texto descreve sentar/sentir o chão; [ending-05](IabAudit/ending-05.jpg) mostra Nanda sem apoio apesar do pouso narrado; [ending-13](IabAudit/ending-13.jpg) mantém Yuna deitada no ar e Nanda com asas abertas, afastadas da toalha. Rever primeiro os pontos de apoio na pintura e as poses de contato, depois movimento e duração. Não corrigir a narrativa para esconder ações ausentes: representar pão e copo nos quadros 6/9/11, usando assets/poses adequados e revisão humana de cada microentrega.

**Aceite:** vídeos sem retoque da abertura e do encerramento, comparação com arte de referência, pés apoiados e ausência de mudança brusca de tamanho. Testar 1280×720, 1920×1080 e uma janela estreita; preservar texto e áreas clicáveis. Revisões humanas separadas para apoio inicial, movimento/saída e final. Manter falas literais e os nove/14 beats. Conciliar cabeçalho de 87 s com soma de 84 s antes de modificar durações.

## A04 Mostrar vidas, direções e mensagens corretas

**Arquivos:** `PicnicHudController.cs`, `PicnicUi.cs`, `PicnicHoldButton.cs` e, mediante coordenação com Core, dados do motivo de resgate.

Verificar cobertura real dos glifos da fonte. Usar ícones existentes ou fonte com os caracteres necessários para corações, infinito e setas. Mostrar o estado de vida de forma inequívoca no Normal e Fácil. Corrigir `A grande ?rvore` no descriptor de seção, sob integração do dono do Core.

Distinguir ESTOU AQUI de COM CALMA segundo queda/desorientação. Adaptar a dica aos controles disponíveis e manter contraste e foco de teclado. Validar pressionar/soltar, saída do ponteiro, pausa e retorno sem direção presa.

**Aceite:** capturas Normal com 3/2/1 vidas e Fácil com vida infinita legíveis, setas visíveis, todos os acentos corretos, toque simultâneo direção+pulo e navegação por teclado. Mensagens corretas por motivo de resgate, sem sobrepor objetivos ou botões.

## A05 Conciliar apoio analítico, corpo e pintura

**Arquivos:** `PicnicPlatformController.cs`, `PicnicCharacterController.cs`, contratos `PlatformSupportProfile` pertinentes e testes PlayMode.

Medir diferenças entre posição Core, `CharacterController`, sola pintada e superfície de apoio. O código original usa tolerância de posição até cerca de 15,8 cm para ajuste descrito como skin de 1,5 cm e reposiciona quando o erro passa de 3 m. Resolver a autoridade de movimento sem esconder divergência por teleporte.

Integrar ou adaptar explicitamente `PlatformSupportProfile` para a mesma geometria usada nas previsões e colisores 3D. Conferir solo curvo, degraus, bordas, pedras, plataformas de roda e underside. Ajustar a física à superfície real; não achatar arte nem criar apoio onde há vão.

**Aceite:** PlayMode 30/60/120 Hz com curvas de erro e capturas na câmera final, incluindo pouso na borda, subida por baixo e transporte pelas rodas. Fixar tolerâncias a partir do contrato/skin antes do teste e relatar erros acima delas. Nenhum deslocamento de reconciliação pode substituir uma travessia ou colisão real.

Começar a inspeção das bordas por estes pontos medidos: salto 121 a 30 Hz, pedra 0 → 1 do rio do trecho 10 (folga −0,059814453 m); salto 82 a 60 Hz, aereo-nuvem-3 → aereo-ilha-1 (−0,07345581 m); salto 56 a 120 Hz, trecho-4-gramado → relevo-1 (−0,06826782 m). Consultar tempos e arestas nos CSVs para distinguir contato da sola, centro do corpo e tolerância de colisão.

## A06 Encerrar pânico na margem segura

**Arquivos:** `PicnicSimulation.cs`, `PicnicStageSpec.cs`, descritores de encontros e testes Core.

Trocar limites soltos de X por referência ao apoio seguro de saída de cada rio. O primeiro pânico termina hoje na última pedra; o segundo limite também antecede a margem. Manter o terceiro encontro com duração de seis segundos, sem dano do besouro e com controle de pulo do jogador.

**Aceite:** primeiro e segundo eventos de fim só ocorrem apoiados na margem correta; nenhum evento é perdido/duplicado ao cair, resgatar ou tentar de novo. Terceiro dura seis segundos dentro da tolerância de atualização. QA registra os três encontros numa partida contínua.

## A07 Água, segurança do resgate e retry

**Arquivos:** `PicnicSimulation.cs`, descritores de zonas/retomada, adaptadores Nanda/HUD e testes.

Separar contato com água e limite de queda no vazio; a regra original usa apenas `Y < -2.8`, abaixo da água desenhada. No Normal, entrada em água causa derrota; no Fácil, resgate. Definir ponto seguro anterior com folga da borda e do inseto antes de atualizar o snapshot.

Distinguir queda de desorientação para escolher trajetória e mensagem. Conciliar retry inicial em X=1 com início nas escadas em X=3.38; manter persistência dos insetos derrotados e limpar apenas estados transitórios previstos.

**Aceite:** nove checkpoints apoiados com margem segura; retry inicial coerente; nenhuma sequência de resgate para o mesmo perigo; testes de água, abismo, impacto e repetição após imunidade. Resgate deve terminar numa posição livre de inseto e sustentada pelo apoio real. Confirmar visualmente Nanda carregando e pousando Yuna.

## A08 Completar criaturas e colisões de combate

**Arquivos:** dados de criatura, estado puro em `PicnicSimulation.cs`, `PicnicCreatureController.cs` e testes.

Adicionar variante declarativa de formiga de rio, com arcos de pedra a pedra e parâmetros de patrulha próprios. Não criar subclasses por posição nem cadeias de flags no bootstrap. Conferir formiga normal, dois pisões da lagarta com mudança de cor/velocidade, abelha com preparo/rasante/recarga e minhoca indestrutível com aviso e emergência.

**Minhoca com base suspensa:** conferir [replay-0087](IabAudit/replay-0087.jpg) e [0093](IabAudit/replay-0093.jpg), em que torrão/base aparece cerca de 100–112 px acima da trilha. `PicnicCreatureController.Apply` usa `creature.Y + 0.06` como posição genérica, enquanto a minhoca varia Y durante a emergência no Core. Verificar se Y representa cabeça, base ou deslocamento de emergência e mapear a âncora visual ao apoio real. Testar repouso, emergência e recolhimento com desenho e zona de dano coerentes, sem simplesmente baixar a sprite até ocultar uma divergência de colisão.

**Aceite:** arco pousa em pedras reais; não patrulha no vazio; colisões por cima e lado são distintas; imunidade e bounce funcionam nas três frequências. Abelha alterna A/B sem saltar de posição e minhoca não pode ser derrotada com pisão. Evidência de cada tipo em Unity e ao menos uma ocorrência real no percurso WebGL.

## A09 Atuação sensorial e contato dos pés

**Arquivos:** `PicnicCharacterController.cs`, seleção de animação pura/descritores e assets existentes aprovados.

Criar apresentação distinta para grama alta, cascalho, equilíbrio na borda, salto/pouso e pânico olhando para trás. A implementação original reutiliza corrida e inclina a imagem inteira. Definir âncora comum por estado/pose sem normalizar cada sprite pelo bbox nem deformar a personagem.

**Aceite:** transições solo → grama → cascalho → salto → pouso preservam escala, sola e orientação. Velocidades continuam 1/4,1/2,2 m/s conforme terreno e regra de voo validada. Vídeos em velocidade normal e revisão individual de cada atuação; grama frontal encobre apenas a região prevista das pernas.

## A10 Revisar água, rodas e câmera aérea

**Arquivos:** `PicnicSceneryController.cs`, `PicnicWheelView.cs`, configuração de materiais/arte sob reserva própria.

Substituir a aparência de solo tingido na água por apresentação coerente com água clara, com peixes/plantas submersos, limites e sorting verificáveis. Construir rodas de madeira que preservem a leitura ilustrada e o vínculo visual com as quatro plataformas. Acompanhar a mesma órbita usada pelo apoio.

Rever o Y fixo da câmera nos trechos aéreos: manter Yuna, destino de pouso e perigos legíveis sem chicote, zoom involuntário ou revelar bordas do fundo. Aprovar cada ajuste visual separadamente.

**Aceite:** vídeo dos rios, das quatro rodas em mais de uma fase angular e da sequência aérea completa. Plataforma não se separa do braço da roda; água não parece uma faixa de solo; saltos não saem do enquadramento. Comparação lado a lado com as referências ilustradas pertinentes, sem métricas de fidelidade inventadas.

## A11 Encaminhar a conclusão para a campanha

**Arquivos:** eventos de conclusão do Core, `PicnicStageBootstrap.cs`, catálogo/serviço de campanha e HUD de resultado em integração com A04.

O roteiro pede avanço para Jardim dos Sonhos após o último quadro; o código auditado oferece modal e repetir a fase. Conectar a conclusão por evento a um destino do catálogo efetivamente existente. Se o próximo capítulo ainda não estiver implementado, registrar essa dependência e apresentar o estado previsto para fim de conteúdo; não declarar a campanha completa nem carregar cena inexistente.

**Aceite:** conclusão emitida uma vez, tela e destino corretos, sem reload duplicado. Repetir a fase limpa estados; retorno ao menu mantém seleção correta. Nova cena, se futuramente autorizada/criada, exige cadastro e pré-seleção conforme AGENTS.md.

## A12 Auditar e integrar áudio sensorial

**Arquivos:** `PicnicAudioController.cs`, eventos de domínio e assets de som pertinentes.

Ouvir a build e registrar o que existe. O controller atual só carrega música e pausa/retoma; projetar sinais de terreno, água, asas, susto e resgate conforme direção sonora e sem excesso de estímulos. Não alegar silêncio com base apenas na análise estática.

**Aceite:** reprodução após interação no navegador, master volume e pausa corretos, música sem duplicação ao reiniciar e sinais audíveis em mixagem confortável. Teste real de áudio, além de verificação de referências de clips.

## A13 Matriz de regressão e liberação

Cada programador entrega diff, testes, tabela de casos, evidência visual pertinente e limitações. QA usa build identificada por hash, diferente da linha de base original quando houver código novo.

| Camada | Casos obrigatórios |
| --- | --- |
| EditMode Core | Catálogo, medidas, janelas de pouso, grama/ar, cascalho/ar, nuvens, fases das rodas, dano, pânico, resgate, retry e conclusão |
| Long run | Início real sem reflexão/teleporte, comandos normais, 30/60/120 Hz, três pânicos, Ending/Complete e registro de quedas/danos |
| PlayMode | Corpo 3D, perfil de apoio, sola, colisões, transporte pelas rodas e diferenças Core/corpo nas três taxas |
| WebGL QA | `?qa=1&qa_autoplay=1`, nove seções, rios, quatro rodas, nuvens, criaturas, três pânicos, chegada e final |
| Jogador | URL sem QA, teclado, toque, toque curto de pulo, direção+pulo simultâneos, pausa/foco, Normal/Fácil, derrota/retry e resgate |
| Visual | Abertura, todas as famílias de apoio, estados de personagem, câmera aérea, água, HUD e final; revisão humana por microentrega |
| Aparelho | Ao menos um aparelho real antes de declarar validação móvel; desktop redimensionado não conta como celular |

Preservar `QaPilot_CompletesContinuousCourseWithRealCommands` como gate, corrigindo a implementação quando ele falhar. Não relaxar assert, remover inimigo ou trocar meta por um setup local. O XML antigo `Evidence/Picnic/core-tests.xml` não serve como teste da revisão nova.

## Entregáveis e fechamento

- Medidas e pontos configurados: `Evidence/Picnic/JumpCalibration/`, com versão dos descritores e resultados da calibração.
- Relatório original e evidências: esta pasta, `RetryDesktop/`, `ContinuationDesktop/` e `TechnicalAudit/`.
- Nova validação Unity/WebGL: pasta e hash próprios, com cobertura e limites explícitos.
- Plano atualizado com estado real de A01 e bloqueios remanescentes.
- Coleta IAB e revisão independente: `IabAudit/`, com 55 frames da primeira passagem, 148 replays, 14 quadros finais e registros do método. [Parecer](IabAudit/Review/RELATORIO-INDEPENDENTE.md).

As demais correções estão preparadas para execução pelos agentes programadores. A autorização adicional desta conversa cobre medir e configurar os saltos; não equivale a concluir ou aprovar automaticamente as outras microentregas.

A coleta pendente de três rodas aéreas, 56–100% e 14 quadros finais foi encerrada em 24/11 pelo IAB disponível no root. O bloqueio anterior do Chrome permanece como ocorrência histórica; não foi contornado. O root operou somente UI pública e um avaliador independente revisou as imagens sem fontes ou logs. Os lotes de replay foram pausados entre capturas e os quadros finais avançados por Continuar, portanto o material não comprova FPS, áudio ou animação contínua.

O próximo passo é executar as correções por domínio sob coordenação de PIC01/155 e reapresentar cada microentrega visual para revisão humana. O encerramento da auditoria não fecha a implementação nem aprova artisticamente a fase.
