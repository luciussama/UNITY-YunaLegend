# Protocolo de Trabalho das IAs — Projeto Unity (C#)

O diário de desenvolvimento (`docs/ia/diario.sqlite3`) é obrigatório para qualquer IA neste repositório.

## 0. Registro obrigatório por assunto — concorrência e idempotência

- **Paralelismo obrigatório quando útil:** IAs podem e devem trabalhar simultaneamente em assuntos distintos. Não existe reserva global do projeto. É proibido atuar simultaneamente **no mesmo assunto**; arquivos ou recursos compartilhados exigem coordenação específica, sem bloquear assuntos independentes.
- **Arquivo único:** `docs/ia/diario.sqlite3` mantém o diário de passos e o registro de atuação (`agent_jobs`, `agent_resources`, `agent_events`). É um log de eventos sequencial e imutável, semelhante ao Visualizador de Eventos do Windows. Não utilizar o Registro do Windows nem criar registros paralelos em Markdown/JSON.
- **Todo pedido novo:** antes de editar ou executar o trabalho, consultar `python tools/agent_registry.py status`, o diário recente/planos sem resultado e o coordenador. Comparar o significado do pedido com os assuntos existentes, inclusive finalizados (`status --all`); não inventar outra chave para contornar uma reserva.
- **Aquisição atômica:** usar `claim --subject CHAVE-CANONICA --session SESSAO-UNICA --agent MODELO --resource RECURSO --details ESCOPO`. Repetir `--resource` para cada recurso estritamente necessário. Usar a mesma chave para o mesmo assunto, identificando tarefa/microentrega quando existir. A consulta isolada não autoriza iniciar: somente `claim` bem-sucedido confirma a reserva. Em conflito/erro, parar esse assunto e trabalhar apenas em outro independente.
- **Sequência obrigatória:** `iniciado` (claim) → `em_execucao` (run) → `finalizado` (finish com resultado/evidências). Só o dono da sessão pode avançar. Antes de modificar arquivos, executar `run`. Manter também os pares `planned`/resultado do diário. Falha, cancelamento ou conclusão encerram com resultado explícito; interrupção, bloqueio temporário e espera de revisão conservam a reserva aberta, com checkpoint no diário.
- **Idempotência:** repetir um comando com os mesmos parâmetros devolve o registro/evento existente, sem duplicá-lo. Um assunto tem um único registro permanente. Pedido realmente novo após encerramento usa chave de revisão explícita e referência ao anterior; nunca abrir outra revisão enquanto o assunto anterior estiver aberto. Se o pedido já foi atendido, apresentar o resultado existente. Não finalizar nem assumir o registro de outra IA por tempo decorrido; recuperar com o dono ou por decisão humana registrada.
- **Coordenador existente:** tarefas 2,5D continuam exigindo reserva no `plan_25d.py`, na mesma sessão. Declarar `plan:t06` (exemplo) e os recursos dessa tarefa no registro; assuntos de infraestrutura usam recursos específicos. Reservas antigas são respeitadas, sem atribuir retrospectivamente ações à nova IA. Reservar ambos antes de editar; se a segunda aquisição falhar, registrar o bloqueio e liberar apenas a reserva recém-adquirida da própria sessão, com cancelamento explícito.
- **Escopo da garantia:** todos os agentes devem usar o mesmo arquivo físico do banco no checkout central. Cópias em clones/worktrees não coordenam entre si. Recursos são chaves canônicas; arquivos usam caminhos relativos e diretórios cobrem descendentes. Escolher recursos pequenos e pertinentes. Não reservar `integration` ou a raiz para assuntos independentes. Não apagar o banco, remover triggers ou escrever diretamente nas tabelas para contornar o CLI.

Procedimento e exemplos: [registro de agentes](docs/ia/REGISTRO-AGENTES.md).
Skill de engenharia: [agent-coordination](skills/agent-coordination/SKILL.md).

## 1. Economia de Contexto e Roteamento de Ferramentas
- **Leitura sob demanda:** Leia apenas os arquivos estritamente necess?rios ? tarefa atual. Nunca leia o reposit?rio inteiro ou arquivos de hist?rico (`Archive/`, `REGRAS-LEGADAS-3D.md`) sem necessidade investigativa espec?fica.
- **Execu??o direta:** Utilize livremente qualquer ferramenta dispon?vel no ambiente atual (gera??o de imagem, `computer-use`, `browser`, `chrome`, leitura de `.png` ou navega??o na URL legada) sempre que a tarefa exigir. Nunca pe?a ao usu?rio para trocar de modelo preventivamente.
- **Otimiza??o program?tica:** Para fatiar, alinhar spritesheets ou checar dimens?es/alpha (`getbbox`), prefira scripts Python (`Pillow`) ou Unity Editor Scripts em modo texto (`-batchmode`) para economizar tokens.

## 2. Skills e Estado Canônico (Carregamento Seletivo)
1. Consulte primeiro [ESTADO-ATUAL-25D.md](docs/ia/ESTADO-ATUAL-25D.md) e rode `python tools\plan_25d.py status`.
2. Consulte [SKILLS.md](SKILLS.md) e [yuna-project](skills/yuna-project/SKILL.md) carregando **apenas** a skill pertinente ao pedido (arte, validação visual ou WebGL). IAs sem suporte nativo a skills devem ler os arquivos como Markdown.
3. Leia [PLANO-25D-OBRIGATORIO.md](docs/ia/PLANO-25D-OBRIGATORIO.md) ou [PLANO-25D-ESTADO.json](docs/ia/PLANO-25D-ESTADO.json) apenas nas seções relevantes à tarefa reservada.
4. Caminhos antigos devem ser resolvidos com `python tools/archive_history.py resolve CAMINHO` (não reescrever registros históricos).

## 3. Direção Vigente Obrigatória — 2,5D Ilustrado (12/11/2026)
- **Escopo:** Personagens 2D ilustrados em cenário com profundidade, nativos na Unity (substitui modelos 3D finais de Yuna/Nanda; preserve modelos/scripts antigos usados na cena base sem removê-los sem checar cenas/prefabs/GUIDs).
- **Piloto obrigatório:** Não converter toda a fase antes da aprovação humana do piloto **urso → livros → móvel** com Yuna e Nanda ilustradas em movimento.
- **Microentregas:** Siga [25D-ASSINATURA-E-MICROENTREGAS.md](docs/ia/25D-ASSINATURA-E-MICROENTREGAS.md). Produza cada entrega visual individualmente e aguarde revisão humana antes da etapa dependente (aceite agregado T01/T07 não dispensa revisões individuais; fila planejada não comprova execução ou aprovação).
- **Nanda azul:** Aparência e luz emanada devem ser azuladas (substituindo o âmbar/dourado). Preserve assinatura ilustrada e identidade; não recolorir Yuna.
- **Móvel e desequilíbrio (12/11/2026):** Manter quebra/cedimento, estralo, pausa local 80ms, tropeço2D/recuperação de Yuna e reset no retry ([25D-MOVEL-QUEBRA-E-DESEQUILIBRIO.md](docs/ia/25D-MOVEL-QUEBRA-E-DESEQUILIBRIO.md)). Validar sequência real em execução antes de concluir.
- **Coordenação:** Reserve a tarefa pelo coordenador antes de editar arquivos; respeite dependências e nunca assuma reservas de outro agente por tempo decorrido.

## 4. Regra Obrigatória — Formato das Plataformas (Entrada 305)
- **Sem caixas uniformes:** Cada objeto tem silhueta e apoio próprios (urso de pelúcia, pilha diagonal de livros, tambor cilíndrico, castelinho). Proibido achatar/redimensionar arte para caber em colisor padrão.
- **Apoio real:** A superfície de contato pertence ao objeto (pode ser deslocada do centro, mais estreita que o volume, inclinada, escalonada ou múltipla). Colisor, cálculo de vão/pulo, enquadramento de câmera e testes devem se adaptar à superfície real.
- **Física e ordem de criação:** Preserve o controlador físico 3D e `PlatformSupportProfile` (sem migrar para Physics2D). Defina **primeiro** onde os pés pousam antes de criar a arte e valide na câmera final a correspondência superfície/apoio, inclusive inclinação e cedimento.

## 5. Protocolo do Diário (`docs/ia/diario.sqlite3`)
- **Início:** Leia o histórico recente com `python tools\ai_diary.py list --limit 15` (aumente o limite apenas se precisar rastrear IDs antigos) e verifique planos sem resultado. Não atribua a si ações de outra IA.
- **Ciclo:** Use identificador de sessão único por tarefa identificando o modelo/agente. Registre `planned` antes de cada passo operacional e `completed`, `failed` ou `blocked` com `--plan-id` logo após (incluindo o que foi feito, arquivos afetados, comandos, resultados e validações reais). Registre decisões/limitações como `info`.
- **Sequência e recursão:** Nunca execute o próximo passo sem registrar o resultado do anterior (em caso de interrupção, mantenha o plano pendente e documente a recuperação na próxima sessão). Não registre chamadas feitas exclusivamente para ler/escrever o próprio diário; agrupe chamadas apenas quando compuserem um mesmo passo verificável.
- **Imutabilidade e versionamento:** Nunca apague ou reescreva entradas anteriores; corrija criando nova entrada `info` referenciando o ID anterior. Não registre credenciais, dados pessoais ou raciocínio interno privado. Versione `diario.sqlite3` sem descartar históricos em merges. (Sem acesso a Python, use cliente SQLite conforme [docs/ia/README.md](docs/ia/README.md); se não puder gravar, informe o impedimento antes de modificar o projeto).

## 6. Referência Jogável (Legado)
- Autorizada consulta/jogo exclusivamente via `https://luciussama.github.io/legendGirl/` (somente quando o modelo ativo suportar navegação/visão) para comparação visual e comportamental com a fase Unity. **Proibido** copiar, migrar, clonar ou utilizar cópia local do código HTML legado; a implementação permanece 100% nativa Unity/C#. A build Web da Unity é uma exportação deste projeto, não uma retomada da implementação HTML antiga.
