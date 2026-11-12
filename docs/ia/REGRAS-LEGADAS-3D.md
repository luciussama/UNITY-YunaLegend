# Regras legadas — implementação 3D e retratos de progresso

Extraído do [AGENTS.md](../../AGENTS.md) em 12/11/2026, a pedido do usuário, para reduzir o contexto obrigatório. **Consulta histórica opcional, não fila de trabalho nem direção artística vigente.** Consultar somente em investigação específica da base antiga. Para regras atuais, usar o protocolo da raiz e o [plano 2,5D](PLANO-25D-OBRIGATORIO.md).

## Formulações da implementação baseada em malhas

Os trechos abaixo foram preservados literalmente. Descrevem a base técnica anterior; não exigem plataformas visíveis modeladas em 3D na apresentação ilustrada. A física 3D e o contrato de apoio continuam vigentes, conforme o plano atual. Verificar o código e as evidências atuais antes de assumir que os detalhes permanecem iguais.

> Implementação vigente: `PlatformSupportProfile` descreve segmentos e regiões expostas no plano de corrida, extraídos de caixas inclinadas e triângulos das malhas reais. `DarkRoomPlatformArt.GetSupportShapes` compartilha as formas entre visual, física e autoria do circuito; os marcadores consultam os colliders reais, com cache invalidado por movimento/inclinação. Movimento, bordas, câmera e testes consomem essa API. `PlatformSpec.contact` permanece como descrição de autoria de um tampo simples (como a capa dos livros), não como limite do contrato de apoio.

> Ao modelar: defina **primeiro** onde os pés da criança pousam, modele a malha para o topo visível coincidir com essa superfície e valide por teste que o topo do colisor é igual ao topo visível.

## Base física descrita no protocolo anterior

> A plataforma 00 mantém cinco MeshColliders reais da cabeça, orelhas e banqueta; a 01 mantém a capa inclinada, com regiões múltiplas/curvas e sem pontes entre lacunas. A plataforma 02 tem 8,4 m e duas regiões de tampo compartilhadas entre visual e física por `YieldingWoodFurniture`, com pausa local 80 ms/tropeço/retry. As evidências históricas estão no [arquivo](../../Archive/pre-25d-2026-11-12/README.md); os contratos atuais de arte e apoio estão em `Evidence/25d/T01`.

Arquivar essa descrição não autoriza alterar dimensões, retirar colisores ou substituir o controlador por Physics2D.

## Retratos de progresso retirados da entrada obrigatória

Estes trechos não são regras 3D: são registros temporais, preservados aqui para não sobrecarregar nem desatualizar o protocolo. Não representam o estado atual. Consultar o [índice de estado](ESTADO-ATUAL-25D.md), o [estado estruturado](PLANO-25D-ESTADO.json) e o diário, confrontando divergências com evidências em disco.

> A sessão que estabeleceu este plano autorizou documentação e planejamento; a fila não representa implementação já executada.

> Início registrado na T00; não presumir baseline ou arte já concluídos.

> Arte conceitual azul aprovada em T01; personagem animada e luz real ainda precisam de implementação/validação.

> Yuna ilustrada estática e câmera A perspectiva 45° foram aprovadas; a câmera de escala estável T02.2b está em revisão. Não há animação corporal 2D final.

A proteção das dependências antigas permanece vigente no protocolo principal: não removê-las sem conferir cenas, prefabs e GUIDs. Os estudos e as evidências anteriores continuam no [arquivo pré-2,5D](../../Archive/pre-25d-2026-11-12/README.md).
