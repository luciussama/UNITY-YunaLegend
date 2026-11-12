# Avaliação interativa da Unity no navegador

> Para publicar a build como página no **GitHub Pages** (incluindo uso no celular), veja [WEB-DEPLOY.md](WEB-DEPLOY.md): build com gzip + fallback, threads desligadas, memória de celular, dois workflows e limites reais do mobile.

A build Web é uma exportação deste projeto Unity/C#, não a versão HTML legada. Windows e Android continuam sendo destinos principais. Artefatos e cópias de validação ficam em `Build/` (ignorados pelo Git).

## Referência autorizada

A IA pode acessar e jogar **https://luciussama.github.io/legendGirl/** para observar e comparar comportamento, ritmo, interface e percurso. Usar exclusivamente a URL como referência. **Não clonar, copiar, importar nem migrar código legado para este projeto.** Essa regra também está no [AGENTS.md](../../AGENTS.md) e no diário SQLite.

## Pipeline de renderização

Os materiais do quarto usam URP Simple Lit. O projeto precisa do asset `Assets/Settings/DarkRoomPipeline.asset` em Graphics e nos níveis de Quality; apenas instalar o pacote URP não ativa o pipeline. Materiais URP no Built-in aparecem magenta. O menu **YunaLegend > Configurar Renderizacao URP** restaura essa atribuição sem reconstruir a cena. A construção da cena também executa essa configuração. Após atualizar o projeto com o Editor aberto, confira Graphics/Quality antes de recompilar a Web.

A correção de pipeline não constitui aprovação artística: a comparação visual da nova build Web com a referência ainda precisa ser concluída.

## Compilar

Requer Unity `6000.3.28f1` com **WebGL Build Support**, adicionado pelo Unity Hub. No Editor, use **YunaLegend > Build Web de avaliacao**. A cena salva `Assets/Scenes/DarkRoom_Gameplay.unity` é compilada; alterações não salvas não fazem parte da build.

A ferramenta `WebPreviewBuild` usa build de desenvolvimento sem compressão, para simplificar o servidor local, e restaura as configurações temporárias que modifica. Saída padrão: `Build/WebPreview`. A variável de ambiente `YUNA_WEB_OUTPUT` permite mudar a saída.

Para executar via CLI, use `-batchmode -quit -buildTarget WebGL -executeMethod YunaLegend.EditorScripts.WebPreviewBuild.Build -projectPath CAMINHO -logFile LOG`. Não abra simultaneamente dois Editors no mesmo projeto. Para validar com o Editor do usuário aberto, compile uma cópia isolada de `Assets`, `Packages` e `ProjectSettings`, sem copiar `Library`.

## Servir localmente

Na raiz do projeto, com Node.js disponível:

```powershell
node tools\serve_web_preview.cjs Build\WebPreview 8080
```

Abra `http://127.0.0.1:8080`. O servidor aceita apenas GET/HEAD, limita acesso ao diretório da build e escuta somente loopback. Não publica arquivos na internet. Pare com Ctrl+C ao terminar. Não exponha o servidor em interfaces públicas.

## Avaliar

Dê foco ao canvas e teste Espaço/W, clique e diálogos. Pode ser necessária interação para liberar áudio no navegador. Compare a build Unity com a URL legada em abas separadas, sem transferir implementação. Após alterar código ou cena, recompile: o navegador não reflete automaticamente mudanças feitas no Editor.

A avaliação Web complementa, mas não substitui, testes de Windows e Android. Desempenho, áudio, foco, toque e recursos gráficos podem diferir. Registre no diário os testes realmente executados e suas limitações.

## Comparação comportamental de 10/11/2026

A observação do coordenador na URL legada mostrou corrida automática, Nanda adiante e um salto inteiro com um toque curto de Espaço; não comprovou completar a fase. A build Unity anterior cortava a subida ao soltar, tornando esse mesmo gesto insuficiente. A implementação Unity agora usa gravidade constante e arco completo disparado pela borda do comando, com deslocamento balístico corrigido por meio passo para30/60/120Hz. Segurar não aumenta a altura nem dispara novos pulos; há buffer curto e coyote time, não autopulo.

Diferenças deliberadas: Unity mantém corrida e câmera letais desde a largada, em vez de congelar esperando o tutorial; o jogador continua precisando saltar todas as plataformas. Preserva largada x=-10, primeiro topo0.8m acima do piso, castelo com diálogo, queda narrativa não letal do pôster, retorno reverso e três segundos de preparação após o diálogo. O alvo inicial reverso usa lead3.2m; gaps finais reversos crescem até4.6m para acomodar o arco completo sem ultrapassar os apoios pequenos. Não houve leitura, cópia ou migração de fonte legado.

Validação automatizada: `Build\ReturnStartValidation\ballistic-third.xml` registra15/15 PlayMode. Inclui48 largadas e48 retornos por toque de1frame com Espaço/W/clique/toque em30/60/120Hz;24 comparações tap/hold; checkpoints, ausência de input, esmagamento, proibição de bypass e queda narrativa. Os36 pares elevados são simulados com CharacterController real nos três fps, varrendo pontos de decolagem e exigindo janela contínua mínima100ms; isso comprova pares e checkpoints, **não uma partida humana completa nem uma execução contínua integral**. Logs correspondentes ficam no mesmo diretório. A cena gerada está persistida no projeto.

## Diagnóstico visual da captura Web (10/11/2026)

A imagem capturada anteriormente apresentou corrupção com grandes áreas pretas, faixas e blocos distorcidos na área do canvas, apesar de o usuário confirmar que a exibição na tela dele estava correta e idêntica à do Editor.

A causa foi identificada na técnica de leitura do navegador automatizado: o contexto WebGL2 da Unity é criado por padrão com `preserveDrawingBuffer = false`. Ao capturar a página com ferramentas que leem o buffer entre apresentações ou ao solicitar PNG após a troca de buffers, o navegador lia um conteúdo descartado ou não sincronizado, produzindo os artefatos visuais.

Para a avaliação automatizada no navegador local, o template HTML em `Build/WebPreview/index.html` e a ferramenta `WebPreviewBuild` passam a interceptar a criação do contexto e solicitar `preserveDrawingBuffer = true` sem alterar os shaders, a cena ou as configurações dos projetos Windows e Android. Uma captura direta do canvas após essa alteração (`Build/canvas-direct.png`) confirmou a imagem nítida, com o cenário 3D noturno, o chão de madeira, Yuna, Nanda e os blocos sem distorções.

Os avisos `WebGL INVALID_ENUM: getInternalformatParameter: invalid internalformat` continuam ocorrendo durante a inicialização do WebGL2 pelo navegador; eles representam consultas de capacidade não suportadas pelo ambiente, mas **não causaram o defeito visual**.

