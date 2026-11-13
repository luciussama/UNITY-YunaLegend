# Protocolo de Trabalho das IAs — Projeto Unity (C#)

O diário de desenvolvimento (`docs/ia/diario.sqlite3`) é obrigatório para qualquer IA neste repositório.

## 1. Economia de Contexto e Roteamento Agnóstico de Modelos (CRÍTICO)
- **Leitura sob demanda:** Leia apenas os arquivos estritamente necessários à tarefa atual. Nunca leia o repositório inteiro ou arquivos de histórico (`Archive/`, `REGRAS-LEGADAS-3D.md`) sem necessidade investigativa específica.
- **Execução Visual / Arte (Quando Autorizado ou Suportado):** Se o usuário incluir `[MODO VISUAL]` no prompt ou solicitar explicitamente geração de arte/inspeção visual (no Unity Editor, build WebGL ou referência legada), utilize os recursos nativos disponíveis no ambiente atual (geração de imagem, `computer-use`, `browser`, `chrome` ou leitura de `.png`).
- **Fallback Seguro para Modelos Somente-Texto (ex: DeepSeek):**
  - Na ausência de pedido visual explícito, priorize geração e validação programática via scripts Python (`Pillow`, `getbbox`) ou Unity Editor Scripts em modo texto (`-batchmode`), evitando ler binários/imagens em base64 para economizar tokens.
  - Se uma chamada visual ou de geração de imagem falhar por limitação do modelo ativo (ex: erro HTTP 400 / modelo sem suporte multimodal), **não repita a chamada**. Registre o passo como `blocked` no diário informando o asset/cena pendente e peça ao usuário: *"O modelo atual não suporta visão/geração de imagem. Alterne para um modelo multimodal no cliente atual (ex: GPT ou Antigravity) e envie `[MODO VISUAL]` para continuar."*

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