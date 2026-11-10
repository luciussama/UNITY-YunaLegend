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

## Referência jogável e comparação

A IA tem autonomia para acessar e jogar o legado **exclusivamente pela URL https://luciussama.github.io/legendGirl/** e comparar seu comportamento e apresentação com a fase Unity em desenvolvimento. Essa autorização é para observação e comparação: **não copiar, migrar ou importar código legado, não clonar o repositório legado nem utilizar uma cópia local dele neste projeto**. A implementação permanece nativa Unity/C#. A build Web da Unity é uma exportação deste projeto, não uma retomada da implementação HTML antiga.

Instruções orientam agentes compatíveis; não interceptam automaticamente ferramentas de IAs externas. Veja o [guia do diário](docs/ia/README.md) para comandos e esquema.
