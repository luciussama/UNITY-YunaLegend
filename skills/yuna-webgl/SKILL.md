---
name: yuna-webgl
description: Compilar, servir, avaliar e, quando autorizado, publicar uma exportacao WebGL nativa do projeto Unity Yuna Legend. Use para build ou comportamento no navegador; nao para reimplementar o jogo em HTML ou gerar um site novo.
---

# Yuna — build e avaliação WebGL

Use junto de `yuna-project`. Leia no checkout `docs/ia/WEB-PREVIEW.md` e, para
publicação, `docs/ia/WEB-DEPLOY.md`. Confira os fontes dos geradores e workflows
atuais; instruções históricas de versões/branches não substituem o disco.

## Preparar a exportação

Verifique `ProjectSettings/ProjectVersion.txt`, suporte WebGL instalado, cenas
salvas e lista de build. Não inclua cenas de avaliação como produção por acidente.
Confirme o projeto do Editor e os processos. Com Editor do usuário aberto, use
cópia isolada e confira fontes/GUIDs; não abra outro Editor no mesmo projeto.

Os pontos de entrada existentes são `YunaLegend.EditorScripts.WebPreviewBuild.Build`
para avaliação local e `YunaLegend.EditorScripts.WebCiBuild.Build` para Pages.
Confira a implementação antes de executar; use saída/log próprios, registre
processo e resultado e confirme que configurações temporárias foram restauradas.
Diagnostique o log real, não apenas a presença de uma pasta antiga de build.

## Avaliação local

Use o servidor do projeto na interface loopback:

```powershell
node tools/serve_web_preview.cjs Build/WebPreview 8080
```

A saída de uma build pode ser outra: confira e passe o diretório correto. Teste
menu→fase, foco/entrada, salto por toque curto, morte/retry, diálogos e áudio
pertinentes ao escopo. Recompile depois de mudar fontes; a página não reflete
automaticamente alterações do Editor. Registre navegador, orientação e dispositivo.

Se a captura Web parecer corrompida, verifique a técnica de captura/buffer WebGL
e compare com observação real antes de alterar arte/shaders. O guia de preview
registra o caso `preserveDrawingBuffer`; não trate todo artefato de leitura como
defeito confirmado da renderização do jogo.

Para comparação com o legado, use exclusivamente
`https://luciussama.github.io/legendGirl/`, conforme AGENTS. Observe/jogue sem
clonar, importar código ou extrair assets. A exportação permanece Unity/C# nativa.

## Publicação e evidências

Consulte a autorização vigente do usuário. Um pedido de teste local não publica;
quando publicar já estiver autorizado, prepare e valide o resultado concreto
sem criar uma nova confirmação por rotina. Não exponha credenciais em comandos,
logs, diário ou pacote. Confira destino, workflow, branch e arquivos efetivos.

Para Pages, verifique a configuração atual de compressão/fallback, threads,
memória e ordem de cenas no gerador; não aplique uma lista antiga sem inspeção.
Registre commit/hash e resultado da publicação. Depois, carregue a URL real,
confira assets/HTTP e jogue o trecho relevante. Build ou workflow verde não
comprovam entrada, áudio, toque ou desempenho em aparelho.

Entregue URL realmente validada, versão, evidências e limites. A avaliação Web
complementa os destinos nativos do jogo; não declara cobertura de dispositivos
que não foram testados. Publicação do piloto continua sujeita aos marcos humanos
do plano e ao escopo autorizado.
