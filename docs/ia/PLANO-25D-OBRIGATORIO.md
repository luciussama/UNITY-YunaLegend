# Plano obrigatório — Dark Room 2,5D ilustrado

Versão 1, decisão do usuário em 12/11/2026. Sessão de planejamento: `2026-11-12-codex-plan-25d-c92a`, Codex (GPT-6). Esta sessão documenta o trabalho; não inicia a produção artística.

**Situação atual em 12/11/2026:** T00/T00B/T01 aceitas com os limites das evidências; T02.1 estática aprovada; projeção A escolhida em T02.2a; câmera estável T02.2b implementada e em revisão. Os parágrafos de criação abaixo explicam a origem das dependências, não pendências já resolvidas. Veja o [estado atual](ESTADO-ATUAL-25D.md), o estado estruturado e o diário. Material anterior foi [arquivado](../../Archive/pre-25d-2026-11-12/README.md); não carregá-lo como fila ativa.

**Atualização de execução, 12/11/2026:** o usuário autorizou iniciar, com a mesma assinatura visual e passos pequenos para avaliação humana meticulosa. O [contrato de assinatura e microentregas](25D-ASSINATURA-E-MICROENTREGAS.md) complementa este plano e torna obrigatórias as revisões individuais de cada entrega visual. A T00 foi reservada; o estado e seu checkpoint indicam o avanço real.

**Ajuste de dependências, sessão `2026-11-12-codex-execute-25d-d28a`:** separar inventário T00 de validação jogável T00B. A entrada no navegador continua inconclusiva e não impede ilustração conceitual independente. T00B conserva reprodução de menu, build e gravação pendentes e bloqueia T02 e toda integração posterior. Essa separação não declara nenhum critério jogável cumprido.

## 1. Decisão vinculante e fontes de verdade

Construir **Yuna e Nanda como personagens 2D ilustradas animadas, em cenário com profundidade**, na Unity/C#. Recuperar suavidade, acolhimento e unidade de livro infantil. Não continuar a produção de personagens 3D finais como direção padrão. Modelos atuais podem orientar escala/poses e servir a testes, sem serem confundidos com arte aprovada.

**Nanda azul — instrução posterior do usuário em 12/11/2026:** aparência azulada e emissão de luz da mesma cor, porque a menina prefere azul. Substitui o âmbar/dourado antigo de Nanda em documentos históricos. Preservar assinatura pintada e identidade; luz conceitual precisa de implementação real e validação na integração, sem recolorir Yuna.

**Mecânica confirmada posteriormente:** manter móvel quebrando/cedendo e Yuna se desequilibrando, com estralo, pausa local80ms e retry. O [contrato específico](25D-MOVEL-QUEBRA-E-DESEQUILIBRIO.md) rege a separação de camadas, contato real e revisões em movimento; não confundir o aceite visual do móvel com implementação concluída.

Ordem de autoridade: instrução atual do usuário; AGENTS.md e este plano para o escopo vigente; estado estruturado para tarefa/reserva/checkpoint; diário para histórico e evidência. Havendo divergência, verificar disco/Git e registrar correção antes de editar. Evidência real prevalece sobre uma alegação de conclusão. Não apagar histórico nem reclassificar ações de outra IA como próprias.

Arquivos canônicos:

- `AGENTS.md`: entrada obrigatória para qualquer agente.
- `docs/ia/PLANO-25D-ESTADO.json`: fila, dependências, reservas, checkpoints e histórico de transições.
- `tools/plan_25d.py`: coordenação com exclusão mútua e gravação atômica.
- `docs/ia/diario.sqlite3`: registro obrigatório de cada passo operacional.
- `docs/ia/ESTADO-ATUAL-25D.md`: índice curto da execução atual e próxima microentrega.
- `Evidence/25d/<ID-da-tarefa>/`: evidências da nova direção, com aprovações e limitações explícitas. O diagnóstico inicial está no arquivo histórico, como contexto da decisão.

Regras escritas vinculam agentes que as seguem. A ferramenta bloqueia transições inválidas efetuadas por ela; não intercepta edições diretas no sistema de arquivos. Aprovação humana não é autenticada pelo CLI. Proteção efetiva contra bypass exige permissões e integração protegida no serviço Git; não alegar que elas foram configuradas. Nunca tratar aprovação técnica como aprovação estética.

## 2. Reaproveitamento e limites de escopo

Preservar inicialmente progressão, diálogos, checkpoints, retry, áudio, entrada desktop/toque, controlador físico 3D e contrato `PlatformSupportProfile`. Usar plano de corrida XY com profundidade visual em Z. **Não converter para Physics2D por conveniência**: isso exigiria proposta técnica separada e regressão de todo o circuito.

Reconstruir a apresentação: sprites/partes ilustradas, animações, materiais, camadas de cenário, ordenação e composição. Avaliar individualmente os modelos de urso/livros/móvel como base; seu reaproveitamento depende da aparência final. Não há percentual de reaproveitamento garantido.

Conservar a regra das plataformas: silhueta e apoio próprios, regiões desconexas, inclinação e deformação reais. Antes da arte de uma plataforma, definir e registrar onde os pés pousam. Validar contorno visível contra apoio físico na câmera final; um sprite não autoriza caixas uniformes ou pontes invisíveis. No móvel, arte e contatos devem receber a mesma pose de cedimento; manter pausa local de 80 ms e retry até eventual mudança explicitamente justificada.

Legado: somente observação pela URL `https://luciussama.github.io/legendGirl/`. Não clonar, ler/importar código nem extrair assets. Criar arte original; registrar autoria, ferramenta, prompt/referência quando aplicável e licença de terceiros. Assets do próprio projeto podem ser reutilizados após verificar sua proveniência. Referência aprovada anterior orienta identidade, não obriga reproduzir o meio 3D.

## 3. Contratos que o piloto deve fechar

- **Personagens:** pivô de Yuna na sola, escala consistente com root físico, perfil legível nos dois sentidos, rosto/cabelo/vestido coerentes com o menu. Nanda precisa ser personagem reconhecível, não esfera final. Fixar direção da luz pintada antes de espelhar sprites.
- **Animação:** escolha entre quadros desenhados, rig 2D ou combinação somente após prova em movimento. Medir continuidade, antecipação, pouso, contato e deslizamento. Não considerar deformar uma imagem estática suficiente para aprovação.
- **Render:** estudar SpriteRenderer/meshes planos e materiais compatíveis com URP atual. Não trocar renderer/pipeline global sem inventário e proposta. Definir sorting layers, prioridade por Z, oclusão por móveis, alpha, bordas dos atlas e importação. Parallax apenas decorativo; plano jogável não deve deslocar em relação aos contatos.
- **Câmera:** comparar ortográfica e perspectiva de baixa distorção; selecionar uma após evidência. Não impor ortográfica antecipadamente. Primeiro isolar câmera estável com física preservada; depois avaliar movimento/assistência em experimentos separados.
- **Dispositivos:** piloto horizontal desktop e mobile horizontal/retrato; UI, safe areas, legibilidade e destino do próximo salto devem caber. Em retrato, propor composição própria quando necessário, sem alterar silhuetas para caber.
- **Entrega:** manifest de assets/pivôs/escalas/importação; parâmetros de câmera; configuração de física; nomes de estados/eventos; comandos de reprodução; build e evidências vinculadas ao commit ou hashes dos arquivos testados.

## 4. Sequência e critérios de saída

Todas as tarefas começam `pending`. `accepted` significa critério cumprido e revisão registrada; não é sinônimo de arquivo criado. A ferramenta exige dependências aceitas antes de reservar a próxima tarefa. T01 e T07 são marcos humanos agregados; a instrução posterior do usuário exige também as revisões visuais individuais do contrato de microentregas. O CLI verifica o estado agregado, não autentica nem substitui essas revisões.

| ID | Dependências | Entrega e critério de saída |
| --- | --- | --- |
| T00 | — | Inventário verificável: branch/HEAD/status, cenas/GUIDs, sistemas/arte, hashes, logs/testes históricos diferenciados de novos, correspondência dos fontes com o clone compilado e limitações. Não certifica entrada ou gameplay. |
| T00B | T00 | Validação jogável: confirmar build/fontes, reproduzir menu → fase e registrar abertura, percurso inicial e falha/retry com captura/gravação. Distinguir automação de defeito confirmado. Pendente até evidência real; bloqueia T02. |
| T01 | T00 | Bíblia visual curta e prancha original com Yuna, Nanda, urso/livros/móvel e quadro na escala de gameplay; paleta, acabamento, luz, expressão e relação menu/fase. **Aprovação explícita do usuário da direção visual**, com referência à mensagem e arquivos exatos. Ajustar até aprovar; não produzir fase inteira. |
| T02 | T01,T00B | Prova técnica descartável em cena isolada: render/ordenação, pivô/escala, sprite sobre root físico, um ciclo e um salto provisórios, oclusão/parallax e câmera em formatos equivalentes. Comparar soluções de animação e câmera, registrar decisão e custo. Sem alterar física para disfarçar arte. Contratos de integração definidos e revisados tecnicamente. |
| T03 | T02 | Yuna ilustrada do piloto: repouso/respiração, corrida, subida, queda, pouso e tropeço; reação e transição necessárias ao trecho; despertar completo fica para T08. Mesma identidade da prancha, pés estáveis e virada sem iluminação incoerente. Assets, fontes editáveis quando disponíveis, atlas/pivôs e prova em movimento. |
| T04 | T02 | Nanda ilustrada: voo, asas, repouso no ar, orientação, expressão e acompanhamento; luz/trilha subordinadas à personagem. Eventos continuam no guia existente. Entregar teste de leitura em tamanho real de gameplay e dois sentidos. |
| T05 | T02 | Piloto ambiental: urso, livros inclinados e móvel, fundo/camadas/luz coerentes. Contorno visível e superfícies físicas coincidem; múltiplas regiões não viram apoio contínuo. Móvel conserva cedimento/retry. Ruptura irregular localizada e envelhecimento integram o polimento, evitando corte reto perfeito. |
| T06 | T03,T04,T05 | Integração exclusiva do piloto na cena de avaliação, sem expandir a fase. Run/jump/land/stumble/retry com câmera final e UI. Primeiro preservar física; comparar variantes de câmera, assistência horizontal e animação separadamente. Registrar parâmetros, resultados e limitações; corrigir regressões de entrada do menu. |
| T07 | T06 | Build jogável e vídeo do piloto em câmera final. Revisão humana de acolhimento, identidade, companhia, fluidez, previsibilidade e leitura dos apoios. **Aprovação explícita do piloto em movimento** antes de expansão; imagem bonita ou testes verdes não substituem. Se rejeitado, reabrir tarefa pertinente e invalidar aceitação dependente antes de prosseguir. |
| T08 | T07 | Expandir somente a linguagem aprovada para o restante do Dark Room, incluindo abertura/despertar, retorno/etapas finais, demais plataformas, UI e transições. Inventariar cada objeto com estado e evidência; nenhum placeholder final ocultado pelo relatório. Repetir contratos de apoio/câmera e preservar narrativa. |
| T09 | T08 | Regressão completa EditMode/PlayMode apropriada; percurso manual completo, falha/retry/checkpoints, teclado/toque, áudio/pausa, orientação e desempenho medido em aparelhos identificados. Registrar versões, dimensões, fps/frame time/memória e limites; emulador não equivale a aparelho. Fixar orçamento na T02 e verificar aqui. Sem aparelho, marcar cobertura pendente, sem declarar concluído. |
| T10 | T09 | Build final reproduzível, commit/tag ou SHA, manifest/hash, publicação GitHub conforme autorização vigente e workflow verificado, URL realmente carregada e jogada em desktop/mobile. Confirmar menu → gameplay e assets HTTP; build bem-sucedida sozinha não comprova publicação. Documentar rollback para versão conhecida. |

T03/T04/T05 podem ocorrer em paralelo apenas em escopos disjuntos. Outros passos são sequenciais. Não iniciar produção paralela enquanto os contratos da T02 não estiverem aceitos. O cronograma deriva dessas dependências; não inventar prazos sem medir a produção do piloto.

## 5. Coordenação entre agentes

1. Um checkout de integração e um coordenador ativo para cena principal, ProjectSettings/Packages, AGENTS, plano/estado e integração do diário. Identificar coordenador/checkout no checkpoint T00. Nunca dois Unity Editors no mesmo projeto para compilar.
2. Cada agente usa sessão única e consulta diário, planos sem resultado, `git status` e `plan_25d.py status`. Faz reserva com seu ID de tarefa e sessão. A ferramenta rejeita dependências pendentes, tarefa reservada e recurso compartilhado ocupado.
3. Escopos paralelos: Yuna em `Assets/Art/Illustrated25D/Yuna/`; Nanda em `.../Nanda/`; ambiente em `.../Environment/`. Diretórios são destinos planejados, não assets já existentes. Harness de cada agente deve ficar no seu escopo. Qualquer arquivo compartilhado exige entrega ao integrador, sem sobrescrita concorrente.
4. Cena principal, geradores, física, câmera, pipeline, Packages/ProjectSettings e banco têm integração serial. AGENTS não autoriza spawn automático por si só: delegar apenas quando houver autorização aplicável. Reservas organizam agentes já participantes.
5. Se houver worktrees, todos os agentes apontam `--state` para o **mesmo arquivo absoluto central** e `ai_diary.py --db` para o mesmo banco central acessível. Não trabalhar com cópias divergentes do estado/banco. Somente o integrador commita esses arquivos. Sem armazenamento central compartilhado, trabalhar sequencialmente; interromper paralelismo, não inventar merge binário.
6. Não confiar em tempo de lease: reserva não expira automaticamente. `handover` exige verificar dono/processos/Git/arquivos e registrar justificativa. Não matar processo de outro agente/usuário para desbloquear trabalho.
7. Entrega ao integrador: arquivos e GUIDs, SHA base/branch/commit, diferenças esperadas, comandos/resultados, evidências, limitações e próximo passo. Não regenerar a cena inteira para incorporar um asset. Verificar diff e GUIDs antes/depois.

## 6. Passo operacional, interrupção e retomada

Antes de cada passo, registrar `planned` no diário. Após executá-lo, registrar resultado real ligado por `--plan-id`; não executar o passo seguinte sem resultado. Operações de diário são isentas de recursão. O coordenador mantém a tarefa maior reservada entre esses passos.

Antes de operação longa ou de encerrar contexto, atualizar checkpoint com: feito; próximo comando/ação exata; arquivos afetados; validações e resultados reais; pendências/riscos; processos em execução (PID, projeto, log e saída, sem credenciais); plano do diário ainda pendente se houver. Manter passos pequenos o bastante para deixar evidência antes de esgotar contexto. Não depender de memória do chat, do modelo ou de tokens restantes estimados.

Na retomada:

1. Ler AGENTS, plano completo, estado, último checkpoint e diário recente/planos sem resultado. Confirmar checkout, SHA e alterações locais; não fazer reset/clean por conveniência.
2. Verificar processos/logs/artefatos do passo interrompido antes de repetir build, gerar arte ou regenerar assets. Um PID sozinho não prova identidade; verificar projeto e log sem expor tokens.
3. Se o plano antigo ficou pendente, registrar `info` na nova sessão com ID antigo. Resultados `--plan-id` exigem a sessão original no banco: acrescentar recuperação nessa sessão identificando o agente recuperador e a sessão nova nos detalhes; não atribuir autoria original a si. Novas operações usam novos planos na sessão nova.
4. Conferir evidência contra arquivos atuais. Recuperar resultados incompletos, registrar falha/bloqueio quando real, transferir reserva explicitamente se necessário e continuar do primeiro critério não cumprido.
5. Não marcar concluído para encerrar uma conversa. Tarefa interrompida permanece reservada com checkpoint; bloqueada permanece pendente de resolução. Não reaproveitar evidência antiga para versão diferente sem demonstrar equivalência.

## 7. Uso do coordenador

Executar a partir do checkout de integração. Em worktrees, `--state CAMINHO_ABSOLUTO_CENTRAL` deve vir antes do subcomando.

```powershell
python tools\plan_25d.py status
python tools\plan_25d.py check
# Antes destes comandos de mutacao, abrir plano no diario; depois registrar resultado.
python tools\plan_25d.py claim T00 --session SESSAO --agent MODELO
python tools\plan_25d.py checkpoint T00 --session SESSAO --done "Inventario registrado" --next "Capturar percurso inicial" --validation "Comando e resultado reais" --pending "Ainda sem build" --files "CAMINHO" --processes "Nenhum"
python tools\plan_25d.py submit T00 --session SESSAO --evidence "Evidence/25d/T00/README.md" --validation "Comandos, resultados e limites"
python tools\plan_25d.py accept T00 --session SESSAO_REVISOR --reference "Revisao tecnica: evidencia e criterios conferidos"
# T01/T07: adicionar --human-approval e citar aprovacao humana real em --reference.
# Esse sinalizador nao autentica consentimento; agente nao pode aprovar em nome do usuario.
python tools\plan_25d.py handover T00 --session NOVA_SESSAO --agent MODELO --from-session SESSAO --reason "Dono/processos/arquivos verificados; motivo da transferencia"
python tools\plan_25d.py reopen T00 --session SESSAO --reason "Criterio deixou de valer; corrigir e revisar"
```

`submit` exige checkpoint e evidência existente, calcula SHA256 e passa a `review`. `accept` verifica hashes e passa a `accepted`; registro de referência não autentica consentimento. `reopen` invalida aceitação da tarefa e seus dependentes, mas recusa se algum deles está reservado/em revisão: combinar liberação primeiro. `release` exige motivo e libera para `pending`, preservando checkpoint/histórico. Todas as mutações exigem sessão; proprietário precisa coincidir para checkpoint/submit/release. Acesso aos arquivos e existência de evidência não certificam qualidade artística nem veracidade de um teste declarado.

## 8. Evidências e revisão

Cada tarefa entregue inclui README com objetivo, estado/limites, ambiente/versão, SHA ou hashes, reprodução, validações, arquivos e autoria. Anexar imagens sem retoque do jogo para verificar geometria e gravação para movimento. Distinguir render conceitual, inspeção estática, Play Mode automatizado, sessão manual e dispositivo real. Captura deve identificar configurações relevantes. Registrar devolutivas humanas com data, referência à mensagem e versão exata revisada; não guardar dados pessoais desnecessários.

T02 define tolerâncias documentadas de coincidência apoio/arte, contato dos pés e orçamento de render/memória apropriado aos dispositivos-alvo, após medir baseline. Não inventar aprovação porque valores não foram fixados ainda. Testar apoios inclinados/desconexos, cedimento e retry em 30/60/120 Hz quando pertinente; comparar quadros de contato e vídeo, além dos testes automatizados. Não duplicar testes que apenas espelham implementação.

Revisão humana T07: comparar trecho representativo com a referência observada, nas mesmas dimensões, pedir momentos concretos de desconforto e avaliar identidade, acolhimento, presença de Nanda, fluidez, previsibilidade e legibilidade. Não inferir aprovação de silêncio ou tempo decorrido. Rejeição retorna à tarefa responsável; ampliar produção continua bloqueado.

## 9. Riscos e saída segura

Riscos prioritários: sprites inconsistentes entre quadros; rig com aparência de recorte rígido; arte iluminada conflitante com luz dinâmica; aliasing/alpha/sorting; pés sem contato; camadas deslocando apoios; fratura do móvel divergente da arte; textura/atlas excedendo mobile; menu sem entrada; ferramenta indisponível; merge de cena ou banco concorrente.

Mitigar no piloto e registrar decisões. Preservar baseline e cena atual para rollback; usar cena de avaliação separada antes da integração. Não apagar estudos 3D nem publicar piloto como fase final. Builds antigas em `WebGL/` não ganham status novo por existirem. Publicação anterior não foi confirmada e deve ser auditada em T00/T10.

Próxima ação após este planejamento: iniciar **T00** quando a execução for solicitada, reservar a tarefa, estabelecer baseline reproduzível e preparar a entrega visual de T01. Nenhuma aprovação artística foi concedida nesta sessão além da escolha da segunda proposta.
