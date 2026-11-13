---
name: yuna-project
description: Trabalhar no projeto Unity/C# Yuna Legend, incluindo retomada, diario, coordenacao e implementacao nativa 2,5D. Consulte antes de alterar este repositorio; carregue as skills de arte, validacao visual ou WebGL somente quando a tarefa exigir.
---

# Yuna Legend — trabalho no projeto

Localize a raiz do checkout com `AGENTS.md` e `Assets/`. Todos os caminhos abaixo
são relativos a essa raiz, mesmo se esta skill estiver instalada fora do projeto.

## Entrada e fonte de verdade

Antes de executar qualquer pedido, seguir a seção 0 de `AGENTS.md`: consultar
`python tools/agent_registry.py status`, adquirir o assunto e registrar execução.
IAs atuam em paralelo em assuntos distintos; nunca duplicar assunto aberto.
Para mudanças no registro, consultar `skills/agent-coordination/SKILL.md`.

Leia `AGENTS.md`, `SKILLS.md` e `docs/ia/ESTADO-ATUAL-25D.md`. Para a migração,
leia integralmente `docs/ia/PLANO-25D-OBRIGATORIO.md`, consulte
`python tools/plan_25d.py status`, `python tools/ai_diary.py list --limit 50`
e os planos sem resultado conforme `docs/ia/README.md`.

Instrução atual do usuário governa o escopo; o plano/AGENTS governam os contratos,
o estado estruturado registra reservas/checkpoints e o diário registra ações e
evidências. Havendo divergência, confira disco/Git e registre a correção. Leia
histórico arquivado apenas para uma investigação específica, não como fila ativa.

Confirme branch/HEAD, alterações locais e processos antes de repetir comandos.
Preserve trabalho de outras sessões. Reserve tarefa e recursos pelo coordenador
antes de editar; reserva não vence por tempo. Para documentação sem tarefa própria
no CLI, registre no diário a reserva de escopo, após verificar o recurso de
integração e as sessões existentes; não assuma uma tarefa de arte para editar guias.

## Diário e continuidade

Use sessão única por tarefa e identifique o agente/modelo conhecido. Registre
`planned` antes de cada passo verificável e seu resultado com `--plan-id` antes
do próximo. Operações exclusivamente do diário não exigem registrar a si mesmas.
Nunca reescreva entradas nem reivindique ações de outra IA. Use `info` para
decisões/correções; diferencie pendência intencional de execução interrompida.

Mantenha checkpoint com feito, validação real, próximo passo exato, arquivos,
limites e processos/logs. Respeite adiamentos explícitos do usuário; ler uma fila
ou um ponto futuro não inicia sua execução. Registre o banco junto às mudanças
na integração de Git, coordenando o SQLite em trabalho paralelo.

## Implementação Unity

- Siga a direção ativa: personagens 2D ilustradas em cenário com profundidade,
  Unity/C#. Preserve controlador físico 3D e `PlatformSupportProfile`.
- Leia `docs/ia/25D-ASSINATURA-E-MICROENTREGAS.md` para arte/animação e
  `docs/ia/25D-MOVEL-QUEBRA-E-DESEQUILIBRIO.md` quando o móvel for relevante.
- Personagens 3D antigos podem continuar como dependências técnicas. Verifique
  cenas, prefabs e GUIDs antes de retirar qualquer asset. Preserve `.meta`.
- Preserve silhueta e apoio próprios de cada plataforma. Adapte colisão, câmera
  e teste à superfície real. Não ajuste o objeto a uma caixa de contato padrão.
- Faça mudanças pequenas e isoladas. Não regenere a cena inteira para incorporar
  um sprite, nem altere física para esconder defeito visual.
- Verifique a versão em `ProjectSettings/ProjectVersion.txt` e as dependências em
  `Packages/`. Não presuma versões ou caminhos pessoais do Editor.

Não abra dois Editors no mesmo projeto. Antes de batch, confira projeto/processo,
lock e log; não mate Editor do usuário nem remova lock apenas por parecer antigo.
Use cópia isolada quando apropriado, conferindo os fontes e GUIDs copiados. Nunca
apresente resultado de clone divergente como teste da versão atual.

## Escolher o procedimento necessário

| Trabalho | Ler/usar |
| --- | --- |
| Pintura original, sprite ou nova pose | Skill `imagegen` e assinatura visual; fontes aprovadas e proveniência do asset |
| Animação/câmera/contato/apoios | Skill `yuna-visual-validation`; contratos/evidências da tarefa atual |
| Build, navegador ou publicação WebGL | Skill `yuna-webgl` e guias Web ativos |
| Script de coordenação/diário | Guia `docs/ia/README.md` e testes pertinentes em `tests/` |

## Validação e entrega

Execute verificações proporcionais à mudança. Mudanças de física/entrada exigem
casos de gameplay pertinentes; mudanças de apresentação exigem evidência visual,
não apenas testes verdes. Documentação simples pede links/formato/integridade,
não uma build de Unity sem motivo. Registre comandos, saídas, versão e limites.

Entregue algo revisável antes de solicitar a revisão humana definida no plano.
Não avance microentrega visual dependente sem a revisão da anterior, nem marque
T03/T07 ou a fase concluída só para encerrar o chat. Um teste técnico não aprova
estética; um desenho aprovado não aprova movimento ou integração.
