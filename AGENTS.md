# Protocolo de trabalho das IAs

Este projeto utiliza Unity (motor de jogos; scripts normalmente em C#). O diário de desenvolvimento é obrigatório para qualquer IA que trabalhe neste repositório.

## Skills do projeto — consulta obrigatória

Leia [SKILLS.md](SKILLS.md) e a skill [yuna-project](skills/yuna-project/SKILL.md) antes de atuar. O guia aponta as skills de arte, validação visual e WebGL, com instruções e recursos compartilháveis em `skills/`. Carregue somente as pertinentes ao pedido atual. IAs sem suporte nativo a skills devem ler os arquivos como Markdown. Skills orientam o procedimento; autorização, reservas, revisões humanas e estado continuam regidos por este protocolo e pelos arquivos canônicos.

## DIREÇÃO VIGENTE OBRIGATÓRIA — 2,5D ilustrado (12/11/2026)

O usuário escolheu **personagens 2D ilustrados em cenário com profundidade**, implementados nativamente na Unity. Leia integralmente [o plano obrigatório](docs/ia/PLANO-25D-OBRIGATORIO.md) e execute `python tools\plan_25d.py status` antes de atuar nesta migração. O estado está em [PLANO-25D-ESTADO.json](docs/ia/PLANO-25D-ESTADO.json). Esta direção substitui as recomendações antigas de refinar Yuna/Nanda como modelos 3D finais. Preserve os estudos e as evidências anteriores como histórico e referência.

Comece pelo [estado atual resumido](docs/ia/ESTADO-ATUAL-25D.md). A documentação e as referências anteriores estão em [Archive](Archive/pre-25d-2026-11-12/README.md), fora do contexto ativo por pedido do usuário. Consulte-as somente para uma investigação específica, não como tarefas a retomar. Caminhos antigos no diário ou em manifestes aprovados podem ser localizados com `python tools/archive_history.py resolve CAMINHO`; não reescreva esses registros históricos.

- Não converter toda a fase antes de aprovar o piloto **urso → livros → móvel**, com Yuna e Nanda ilustradas em movimento.
- Escolher 2,5D não aprova automaticamente novos desenhos, animações ou o piloto. Produza a entrega revisável antes de solicitar avaliação nos pontos definidos pelo plano.
- Reserve a tarefa pelo coordenador antes de alterar arquivos; respeite dependências e recursos exclusivos. Não assuma reservas de outro agente por tempo decorrido.
- Checkpoints não substituem o diário obrigatório. Resumos de chat e instruções antigas não substituem o estado canônico verificado em disco.
- Preserve **a mesma assinatura visual e passos pequenos para avaliação humana meticulosa**. Consulte [assinatura e microentregas](docs/ia/25D-ASSINATURA-E-MICROENTREGAS.md). Cada entrega visual deve ser revisada individualmente antes da próxima que dependa dela; o aceite agregado T01/T07 não dispensa essas revisões. Fila planejada não comprova execução ou aprovação.
- **Nanda azul:** sua aparência e a luz que emana devem ser azuladas, substituindo o âmbar/dourado antigo. Preservar a assinatura ilustrada e a identidade; não recolorir Yuna.
- **Móvel e desequilíbrio confirmados em 12/11/2026:** manter quebra/cedimento, estralo, pausa local80ms, tropeço2D/recuperação de Yuna e reset no retry. Leia [o compromisso e critérios de validação](docs/ia/25D-MOVEL-QUEBRA-E-DESEQUILIBRIO.md). Aprovação da aparência não significa animação/física já implementadas; validar a sequência real antes de declarar concluída.

- Antes de trabalhar, leia o histórico recente com `python tools\ai_diary.py list --limit 50` e consulte os planos ainda sem resultado. Não atribua a si ações de outra IA.
- Use um identificador de sessão único por tarefa e identifique o agente/modelo quando conhecido.
- Antes de cada passo operacional, registre `planned`; após executá-lo, acrescente `completed`, `failed` ou `blocked` com `--plan-id` apontando para o plano. Inclua o que foi feito, arquivos afetados, comandos, resultados e validações reais. Registre também decisões e limitações como `info`.
- Não execute o próximo passo sem registrar o resultado do anterior. Se houver interrupção, mantenha o plano pendente e documente a recuperação na próxima sessão. Um plano pode receber novos resultados de recuperação.
- Não registre cada chamada feita exclusivamente para ler/escrever o próprio diário: isso causaria recursão infinita. Agrupe chamadas somente quando compuserem um mesmo passo verificável.
- Nunca apague nem reescreva entradas anteriores. Corrija informações com uma nova entrada `info` referenciando o ID anterior. Não registre credenciais, dados pessoais desnecessários ou raciocínio interno privado; registre ações, decisões técnicas objetivas e evidências.
- O banco `docs/ia/diario.sqlite3` faz parte do projeto e deve acompanhar as alterações no controle de versão. Evite merges binários: em trabalho paralelo, coordene a integração dos históricos e nunca descarte um banco silenciosamente.
- Caso não tenha acesso a Python, use qualquer cliente SQLite seguindo o esquema e o protocolo do [guia](docs/ia/README.md). Se não puder gravar no banco, informe o impedimento antes de modificar o projeto.

## REGRA OBRIGATÓRIA — formato das plataformas

**Leia antes de modelar qualquer plataforma.** Registrada no diário na entrada **305** (11/11/2026, a pedido do usuário).

- As plataformas **não são caixas uniformes**. Cada objeto tem silhueta, tamanho e apoio próprios (urso de pelúcia, pilha diagonal de livros, tambor cilíndrico, castelinho…). É proibido achatá-las ou redimensioná-las para caber num colisor padrão.
- A **superfície de contato pertence ao objeto**: pode ser deslocada do centro, mais estreita que o volume, inclinada (pilha diagonal), escalonada ou múltipla.
- Colisor, cálculo de vão/pulo, enquadramento de câmera e testes **devem se adaptar ao formato real da superfície**; nunca adaptar o objeto ao colisor.
- Preserve o controlador físico 3D e o contrato `PlatformSupportProfile`; a apresentação ilustrada não autoriza migração para Physics2D nem apoio artificial sobre lacunas.
- Antes de criar a arte, defina **primeiro** onde os pés pousam. Valide por testes e evidência na câmera final a correspondência entre superfície visível e apoio físico, inclusive inclinação e cedimento. A arte não precisa ser uma malha 3D visível.

Modelos/scripts antigos ainda usados pela cena base são dependências técnicas, não direção artística. Não removê-los sem conferir cenas/prefabs/GUIDs. Detalhes históricos de malhas/colisores e retratos antigos de progresso estão nas [regras legadas](docs/ia/REGRAS-LEGADAS-3D.md), de consulta opcional; não carregá-los como instruções vigentes. Aprovações e pendências devem ser consultadas no estado canônico, não presumidas deste protocolo.

## Referência jogável e comparação

A IA tem autonomia para acessar e jogar o legado **exclusivamente pela URL https://luciussama.github.io/legendGirl/** e comparar seu comportamento e apresentação com a fase Unity em desenvolvimento. Essa autorização é para observação e comparação: **não copiar, migrar ou importar código legado, não clonar o repositório legado nem utilizar uma cópia local dele neste projeto**. A implementação permanece nativa Unity/C#. A build Web da Unity é uma exportação deste projeto, não uma retomada da implementação HTML antiga.

Instruções orientam agentes compatíveis; não interceptam automaticamente ferramentas de IAs externas. Veja o [guia do diário](docs/ia/README.md) para comandos e esquema.
