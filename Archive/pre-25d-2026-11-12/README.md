# Arquivo da direção anterior

Arquivado a pedido do usuário em 12/11/2026, após a microentrega de câmera estável. Sessão `2026-11-12-codex-archive-previous-25d-9e5b`, Codex (GPT-6).

**Este material é histórico, não instrução vigente nem fila de trabalho.** Para continuar, usar o [estado atual](../../docs/ia/ESTADO-ATUAL-25D.md), [AGENTS.md](../../AGENTS.md), plano/estado 2,5D e diário. Não retomar modelagem 3D final a partir dos handoffs aqui preservados.

Foram movidos, sem alteração de conteúdo, **366 arquivos, 123,23 MiB**, em 29 grupos: oito documentos/estados antigos, 17 grupos de evidências anteriores ao diretório `Evidence/25d` e quatro logs rastreados da raiz. Documentos e evidências espelham os caminhos originais:

- [docs/ia](docs/ia): guias de produção 3D, handoffs, investigação, avaliação de modelagem, diagnóstico inicial e dois estados antigos EM-CURSO.
- [Evidence](Evidence): auditorias, estudos do modelo 3D, conceito anterior de Yuna, capturas de plataformas/móvel, iluminação, câmera, contatos e título, com sondas/XMLs/manifestes originais.
- [root-logs](root-logs): quatro logs que já eram rastreados no Git, preservados no arquivo versionado.
- [manifest.json](manifest.json): cada caminho original, destino, tamanho, SHA256 e indicação de rastreamento anterior no Git.

O diário SQLite e os manifestes/revisões aprovados da nova direção conservam seus bytes e seus caminhos históricos. Não foram reescritos para fingir que o arquivo sempre esteve aqui. Para localizar uma referência antiga:

```
python tools/archive_history.py resolve Evidence/yuna-concept-2026-11-11/yuna-3d-concept-v1.png
python tools/archive_history.py verify
```

Links relativos e caminhos absolutos dentro de relatórios antigos representam a estrutura da época. O resolvedor trata caminhos de arquivos do manifesto; para um diretório antigo, acrescentar seu caminho abaixo desta pasta espelhada. Não executar sondas históricas sobre a cena principal sem revisar seu escopo e dependências. Usar cópia isolada quando for preciso reproduzir uma auditoria anterior.

`Assets` não foi movido: modelos, materiais, código e física antigos ainda sustentam a base de comparação e testes. Também permanecem ativos o roteiro narrativo, as licenças, o protocolo do diário e o pipeline Web. A arte e os aceites atuais continuam em `Evidence/25d`.

Cinco logs soltos da raiz que não eram rastreados e a exportação WebGL antiga, também não rastreada, foram guardados separadamente em `Build/Archive` (artefatos locais ignorados). Os quatro logs já rastreados estão em root-logs acima. A pasta `WebGL/` poderá ser gerada novamente numa futura publicação autorizada; o workflow não foi alterado e nenhuma build antiga foi publicada nesta organização.

As buscas comuns com ripgrep ignoram `Archive/` por `.ignore`, reduzindo o contexto da direção anterior. Para pesquisar deliberadamente aqui, usar `rg --no-ignore termo Archive/pre-25d-2026-11-12`.
