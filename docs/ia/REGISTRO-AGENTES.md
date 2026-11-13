# Registro de atuação por assunto

IAs trabalham em paralelo em assuntos diferentes. A exclusão é por assunto e
pelos recursos compartilhados declarados; não há bloqueio global da atuação.
O banco fixo é `docs/ia/diario.sqlite3`, junto do diário já existente. O modelo é
um log de eventos como o Visualizador de Eventos do Windows: ID crescente,
horário UTC, identificação do agente/sessão, assunto, estado e evidência.

`agent_jobs` fixa a identidade do assunto; `agent_resources` fixa o escopo;
`agent_events` registra as transições sem UPDATE/DELETE. Estados:
`iniciado → em_execucao → finalizado`. Resultado final: `concluido`, `falhou` ou
`cancelado`. Bloqueio temporário/espera de revisão não encerra a reserva.

## Procedimento

Antes de cada pedido, consultar o registro, o diário recente e seus planos sem
resultado, além do coordenador. A decisão semântica é obrigatória: textos
diferentes podem descrever o mesmo assunto. Preferir IDs existentes como
`t06/retry-movel-v1`; não usar o nome do modelo como parte da chave do assunto.

```powershell
python tools/agent_registry.py status
python tools/agent_registry.py status --all
python tools/ai_diary.py list --limit 15
python tools/plan_25d.py status

# Exemplo independente; substituir assunto, sessao e escopo pelo pedido real.
python tools/agent_registry.py claim --subject infra/exemplo-v1 --session sessao-unica --agent MODELO --resource tools/exemplo.py --details 'Objetivo e limites'
# Usar o ID retornado; 123 e apenas exemplo.
python tools/agent_registry.py run --id 123 --session sessao-unica --details 'Execucao iniciada'
# Registrar planned/resultado no diario para cada passo e realizar validacoes.
python tools/agent_registry.py finish --id 123 --session sessao-unica --outcome concluido --details 'Arquivos, testes executados, resultado e pendencias'
python tools/agent_registry.py history --id 123
```

Se o pedido pertence ao plano 2,5D, declarar `--resource plan:t06` e repetir
`--resource` com os recursos da tarefa (`integration`, `art-yuna` etc.). Depois
reservar a tarefa pelo coordenador na mesma sessão. Esses recursos amplos só
se aplicam ao escopo da tarefa Unity; documentação/infraestrutura independente
usa caminhos específicos. O coordenador e o registro verificam conflitos entre
si. Uma reserva parcial não permite editar: em falha da segunda aquisição,
registrar o bloqueio e cancelar/liberar somente a aquisição da própria sessão.
Um cancelamento antes do trabalho ainda registra `run` com a descrição de que
apenas a liberação está sendo processada e `finish --outcome cancelado`.

Idempotência: a mesma chave/sessão/agente/recursos retorna o mesmo ID. Repetições
de transição com mesmos detalhes/resultado não geram outro evento. Outra sessão
não assume essa chave, nem mesmo após o encerramento. Uma demanda nova após o
encerramento usa revisão explícita (`...-v2`) referenciando o registro anterior;
essa relação é revisada pelo agente, não inferida automaticamente pelo programa.

## Concorrência e recuperação

SQLite `BEGIN IMMEDIATE` serializa apenas a curta aquisição/alteração do registro;
o trabalho das IAs segue simultâneo. Recursos iguais ou relação pasta/descendente
conflitam enquanto abertos; assuntos distintos com recursos distintos prosseguem.
Normalização ignora caixa, troca barras Windows e rejeita segmentos ambíguos.
Uma queda durante a transação faz rollback; depois do commit a reserva permanece,
sem prazo de expiração. O dono retoma com a mesma sessão e ID.

Se o dono estiver indisponível, a IA registra o impedimento no diário e aguarda
decisão humana para recuperação. O CLI v1 não oferece transferência de dono:
não editar as tabelas nem inventar outra chave para assumir trabalho abandonado.
Essa limitação preserva a evidência até um procedimento de transferência auditável.

Todos devem acessar o mesmo banco físico central, com `--db CAMINHO` antes do
subcomando se necessário. `claim` consulta o coordenador deste checkout central;
executar o CLI central, mesmo trabalhando em uma cópia de fontes. SQLite em cópias
separadas ou merges Git não implementam uma trava distribuída. Usar disco local
compartilhado por processos; para máquinas diferentes seria necessário um serviço
central de reservas. Não sobrescrever bancos divergentes em merge.

O programa garante unicidade e transações para clientes que usam esse protocolo;
não identifica automaticamente sinônimos de assuntos, recursos omitidos ou agentes
que ignorem o CLI. Triggers protegem imutabilidade e sequência contra acidentes,
não contra quem tenha acesso irrestrito ao arquivo.

## Instalação e verificação

O banco do projeto já está inicializado. `python tools/agent_registry.py init`
é uma migração aditiva e repetível para uma cópia existente; preserva `entries`
e sua versão de esquema. Não cria silenciosamente outro banco caso o caminho erre.

```powershell
python -m unittest discover -s tests -p 'test_agent_registry.py' -v
python -m unittest discover -s tests -p 'test_plan_25d.py' -v
python tools/ai_diary.py check
```

As reservas antigas não são convertidas retroativamente em atividades novas.
O registro consulta as tarefas ativas/em revisão do coordenador; planos antigos
sem resultado devem ser investigados sem assumir autoria ou encerrá-los por idade.
