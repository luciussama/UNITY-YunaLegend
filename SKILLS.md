# Skills compartilháveis — Yuna Legend

Este é o ponto de entrada para qualquer IA que trabalhe neste projeto. As
instruções ficam em `skills/`, junto de suas referências, recursos e licenças,
prontas para acompanhar o projeto no Git. Não dependem do caminho pessoal de
quem preparou o pacote.

## Como consultar

1. Leia [AGENTS.md](AGENTS.md) e [yuna-project](skills/yuna-project/SKILL.md).
2. Consulte o estado e o diário no checkout; escolha somente a skill necessária
   ao pedido atual na tabela abaixo. Uma skill descreve como trabalhar, não aprova
   tarefas, imagens ou mudanças por conta própria.
3. Se a IA não reconhecer o formato `SKILL.md`, abra-o como Markdown e siga as
   instruções. Os campos YAML identificam a skill; não exigem um programa especial.

| Skill incluída | Quando consultar | Requisitos de execução |
| --- | --- | --- |
| [agent-coordination](skills/agent-coordination/SKILL.md) | Engenharia do registro de agentes, reservas idempotentes e concorrência por assunto | Python 3; banco central do diário; sem dependências externas |
| [yuna-project](skills/yuna-project/SKILL.md) | Entrada no projeto, C#/Unity, coordenação, diário, implementação e retomada | Checkout do projeto; Python 3 ou cliente SQLite; Unity quando a tarefa exigir |
| [imagegen](skills/imagegen/SKILL.md) | Criar/editar pinturas, personagens, poses, sprites e conceitos raster | Ferramenta de geração/edição de imagens disponível no ambiente; fallback de API somente conforme a própria skill |
| [yuna-visual-validation](skills/yuna-visual-validation/SKILL.md) | Rever animação, câmera, contato, apoios, cenas de prova e evidência em movimento | Unity e ferramentas de captura/análise adequadas; controle de UI quando necessário |
| [yuna-webgl](skills/yuna-webgl/SKILL.md) | Exportar, servir, testar ou publicar a build Web da Unity | Unity com suporte WebGL; Node para o servidor local; acesso ao destino somente para publicação autorizada |

As três skills `yuna-*` são instruções próprias deste projeto. `imagegen` é uma
cópia integral da skill de autoria fornecida pelo ambiente, com licença e recursos
preservados. [SOURCES.json](skills/SOURCES.json) registra origem, data e hashes.

## Usar em outra IA

Forneça este arquivo como entrada, junto da pasta `skills/`. Uma mensagem de
encaminhamento suficiente é:

> Neste checkout de UNITY-YunaLegend, leia AGENTS.md, SKILLS.md e a skill
> yuna-project. Consulte o estado e o diário antes de atuar. Para a tarefa atual,
> carregue somente as skills pertinentes e respeite as revisões humanas do plano.

Os caminhos de projeto mencionados nas skills são relativos à raiz do checkout,
identificada por `AGENTS.md`, `Assets/`, `Packages/` e `ProjectSettings/`. Se as
skills forem instaladas em outro diretório, mantenha essa resolução pela raiz
do jogo, não pelo diretório global de skills da IA.

As instruções podem ser compartilhadas; ferramentas, contas e permissões precisam
existir no ambiente receptor. Copiar `SKILL.md` não instala Unity, um gerador de
imagens, controle de Windows ou um backend do Codex. Use as ferramentas equivalentes
do provedor e registre o que foi realmente possível validar. Quando houver skills
de controle de computador no ambiente, leia também suas instruções antes de usar UI.

## Compartilhar e atualizar

No checkout, gere um arquivo ZIP com as instruções e todos os recursos incluídos:

```powershell
python tools/package_project_skills.py
```

Saída padrão: `Build/Exports/yuna-legend-skills.zip`. O ZIP contém este guia,
`skills/` e um manifesto SHA256. A pasta `Build/` é saída local; o conteúdo fonte
versionável permanece em `SKILLS.md`, `skills/` e no exportador.

O pacote é de instruções de autoria. Para executar tarefas do jogo, quem o recebe
precisa também do checkout com os documentos, ferramentas, evidências e diário
canônicos citados. O ZIP não leva banco do diário, assets do jogo, credenciais,
builds ou caches. Arquivos internos de exemplo da skill não são uma licença para
publicar ou consumir APIs fora da autorização recebida.

Ao atualizar uma skill própria, preserve o escopo e valide seus links/frontmatter.
Ao atualizar o snapshot `imagegen`, confira a licença e os recursos referenciados,
registre a nova origem/hashes em `skills/SOURCES.json` e gere novamente o ZIP.
Não grave progresso corrente nas skills: ele pertence ao estado e ao diário.
