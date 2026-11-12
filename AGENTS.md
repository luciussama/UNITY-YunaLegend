# Protocolo de trabalho das IAs

Este projeto utiliza Unity (motor de jogos; scripts normalmente em C#). O diário de desenvolvimento é obrigatório para qualquer IA que trabalhe neste repositório.

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

### Atualização de implementação: urso arredondado

A plataforma 00 usa cinco MeshColliders estáticos derivados das formas visíveis da cabeça, orelhas e banqueta, sem o antigo apoio retangular invisível. A plataforma 01 mantém a laje inclinada sobre a capa superior. O contrato de apoio inclui regiões múltiplas e curvas discretizadas a partir da malha; não cria pontes entre áreas desconexas nem usa a largura do volume como se fosse uma superfície contínua. Ver `Evidence/supports-camera-2026-11-11/README.md` para validações e limites. A câmera fechada recebeu afastamento de 5%, mantendo FOV 45°; o panorama tutorial continua separado.

A plataforma 02 é um móvel de 8,4 m com duas regiões de tampo que cedem durante a travessia. `YieldingWoodFurniture` aplica a mesma pose aos tampos visíveis e aos contatos físicos; a pausa local de 80 ms e o tropeço são restaurados no retry. Ver `Evidence/furniture02-2026-11-11/README.md`. A fratura reta é provisória: o usuário pediu envelhecimento e ruptura irregular localizada para polimento futuro.

Yuna possui um primeiro estudo articulável em `Assets/Characters/Yuna/Yuna_Study.prefab`, instalado como `Body_Visual`; malha/rig são reproduzidos por `YunaCharacterModelBuilder`. A física continua no root e a apresentação em `YunaCharacterView`, com poses procedurais provisórias. A referência artística aprovada permanece `Evidence/yuna-concept-2026-11-11/yuna-3d-concept-v1.png`; o estudo ainda precisa de refinamento de rosto/cabelo/materiais e animações finais. Estado detalhado em `docs/ia/YUNA-ESTADO-MODELAGEM-2026-11-11.md`. Não considerar a primeira malha equivalente ao render aprovado. Nanda ainda aguarda modelagem.

## Referência jogável e comparação

A IA tem autonomia para acessar e jogar o legado **exclusivamente pela URL https://luciussama.github.io/legendGirl/** e comparar seu comportamento e apresentação com a fase Unity em desenvolvimento. Essa autorização é para observação e comparação: **não copiar, migrar ou importar código legado, não clonar o repositório legado nem utilizar uma cópia local dele neste projeto**. A implementação permanece nativa Unity/C#. A build Web da Unity é uma exportação deste projeto, não uma retomada da implementação HTML antiga.

Instruções orientam agentes compatíveis; não interceptam automaticamente ferramentas de IAs externas. Veja o [guia do diário](docs/ia/README.md) para comandos e esquema.
