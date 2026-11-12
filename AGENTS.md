# Protocolo de trabalho das IAs

Este projeto utiliza Unity (motor de jogos; scripts normalmente em C#). O diário de desenvolvimento é obrigatório para qualquer IA que trabalhe neste repositório.

## DIREÇÃO VIGENTE OBRIGATÓRIA — 2,5D ilustrado (12/11/2026)

O usuário escolheu **personagens 2D ilustrados em cenário com profundidade**, implementados nativamente na Unity. Leia integralmente [o plano obrigatório](docs/ia/PLANO-25D-OBRIGATORIO.md) e execute `python tools\plan_25d.py status` antes de atuar nesta migração. O estado está em [PLANO-25D-ESTADO.json](docs/ia/PLANO-25D-ESTADO.json). Esta direção substitui as recomendações antigas de refinar Yuna/Nanda como modelos 3D finais. Preserve os estudos e as evidências anteriores como histórico e referência.

Comece pelo [estado atual resumido](docs/ia/ESTADO-ATUAL-25D.md). A documentação e as referências anteriores estão em [Archive](Archive/pre-25d-2026-11-12/README.md), fora do contexto ativo por pedido do usuário. Consulte-as somente para uma investigação específica, não como tarefas a retomar. Caminhos antigos no diário ou em manifestes aprovados podem ser localizados com `python tools/archive_history.py resolve CAMINHO`; não reescreva esses registros históricos.

- Não converter toda a fase antes de aprovar o piloto **urso → livros → móvel**, com Yuna e Nanda ilustradas em movimento.
- Escolher 2,5D não aprova automaticamente novos desenhos, animações ou o piloto. Produza a entrega revisável antes de solicitar avaliação nos pontos definidos pelo plano.
- Reserve a tarefa pelo coordenador antes de alterar arquivos; respeite dependências e recursos exclusivos. Não assuma reservas de outro agente por tempo decorrido.
- Checkpoints não substituem o diário obrigatório. Resumos de chat e instruções antigas não substituem o estado canônico verificado em disco.
- A sessão que estabeleceu este plano autorizou documentação e planejamento; a fila não representa implementação já executada.
- Em 12/11/2026, o usuário autorizou iniciar e exigiu **a mesma assinatura visual e passos pequenos para avaliação humana meticulosa**. Consulte [assinatura e microentregas](docs/ia/25D-ASSINATURA-E-MICROENTREGAS.md). Cada entrega visual deve ser revisada individualmente antes da próxima que dependa dela; o aceite agregado T01/T07 não dispensa essas revisões. Início registrado na T00; não presumir baseline ou arte já concluídos.
- **Nanda azul (diretriz posterior de 12/11/2026):** a menina prefere azul; sua aparência e a luz que emana devem ser azuladas. Esta instrução substitui o âmbar/dourado antigo de Nanda. Preservar a assinatura ilustrada e a identidade; não recolorir Yuna. Arte conceitual azul aprovada em T01; personagem animada e luz real ainda precisam de implementação/validação.
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
- Implementação vigente: `PlatformSupportProfile` descreve segmentos e regiões expostas no plano de corrida, extraídos de caixas inclinadas e triângulos das malhas reais. `DarkRoomPlatformArt.GetSupportShapes` compartilha as formas entre visual, física e autoria do circuito; os marcadores consultam os colliders reais, com cache invalidado por movimento/inclinação. Movimento, bordas, câmera e testes consomem essa API. `PlatformSpec.contact` permanece como descrição de autoria de um tampo simples (como a capa dos livros), não como limite do contrato de apoio.
- Ao modelar: defina **primeiro** onde os pés da criança pousam, modele a malha para o topo visível coincidir com essa superfície e valide por teste que o topo do colisor é igual ao topo visível.

### Base física preservada e apresentação atual

A plataforma 00 mantém cinco MeshColliders reais da cabeça, orelhas e banqueta; a 01 mantém a capa inclinada, com regiões múltiplas/curvas e sem pontes entre lacunas. A plataforma 02 tem 8,4 m e duas regiões de tampo compartilhadas entre visual e física por `YieldingWoodFurniture`, com pausa local 80 ms/tropeço/retry. As evidências históricas estão no [arquivo](Archive/pre-25d-2026-11-12/README.md); os contratos atuais de arte e apoio estão em `Evidence/25d/T01`.

Yuna ilustrada estática e câmera A perspectiva 45° foram aprovadas; a câmera de escala estável T02.2b está em revisão. Não há animação corporal 2D final. Modelos/scripts antigos ainda usados pela cena base permanecem em Assets como dependências técnicas, não como direção artística. Não removê-los por parecerem antigos sem conferir cenas/prefabs/GUIDs.

## Referência jogável e comparação

A IA tem autonomia para acessar e jogar o legado **exclusivamente pela URL https://luciussama.github.io/legendGirl/** e comparar seu comportamento e apresentação com a fase Unity em desenvolvimento. Essa autorização é para observação e comparação: **não copiar, migrar ou importar código legado, não clonar o repositório legado nem utilizar uma cópia local dele neste projeto**. A implementação permanece nativa Unity/C#. A build Web da Unity é uma exportação deste projeto, não uma retomada da implementação HTML antiga.

Instruções orientam agentes compatíveis; não interceptam automaticamente ferramentas de IAs externas. Veja o [guia do diário](docs/ia/README.md) para comandos e esquema.
