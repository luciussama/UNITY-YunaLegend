# Publicar a build Web no GitHub Pages (e avaliar no celular)

**Resposta curta:** sim. O Unity tem WebGL Build Support instalado nesta máquina, a build já pode sair preparada para o GitHub Pages e um workflow publica a página. A versão mobile roda, mas com limites reais (desempenho e navegador), explicados no fim.

## Dois caminhos

| Caminho | Precisa de licença no CI? | Como funciona |
| --- | --- | --- |
| **A. Build local + deploy** (recomendado) | Não | Você compila no Unity, commita `WebGL/`; o workflow `pages.yml` publica. |
| **B. Build no Actions** (opcional) | Sim | `webgl-build.yml` usa `game-ci/unity-builder` para compilar e publicar. |

O caminho A evita lidar com licença Unity em runner e funciona hoje. O caminho B só é necessário se você quiser que o site seja atualizado sem a sua máquina.

## O que a build de Pages já faz

`YunaLegend > Build Web para GitHub Pages` (`WebCiBuild.Build`) ajusta e depois restaura as configurações:

- **Compressão gzip + `decompressionFallback`**: o GitHub Pages não permite cabeçalhos customizados (`Content-Encoding`), então a descompressão acontece no JavaScript.
- **Threads desligadas**: sem `SharedArrayBuffer`, não é preciso enviar COOP/COEP (o Pages não envia).
- **Memória** inicial 256 MB e máxima 1024 MB (calibrado para celular). Sobrescreva com `YUNA_WEB_INITIAL_MB` / `YUNA_WEB_MAX_MB`.
- **Cenas**: `TitleScreen` (entrada) e `DarkRoom_Gameplay`, nessa ordem.
- **HTML mobile**: viewport com `viewport-fit=cover`, tela cheia, fundo escuro, sem zoom/seleção, `touch-action: none` no canvas e um aviso “gire o aparelho” em retrato.
- **Sem build de desenvolvimento**: gera menos arquivos e não expõe o profiler.

## Caminho A — passo a passo

```powershell
# 1. no Unity, com o projeto fechado em outros editores:
#    YunaLegend > Build Web para GitHub Pages
#    ou via CLI:
& "C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe" -batchmode -quit `
  -projectPath "C:\Users\sergi\repos\UNITY-YunaLegend" `
  -executeMethod YunaLegend.EditorScripts.WebCiBuild.Build `
  -logFile Build\web-pages.log

# 2. teste local antes de publicar
node tools\serve_web_preview.cjs WebGL 8080
```

Publique com `git add WebGL && git commit`. O workflow `pages.yml` roda em push para `main`, `master` ou `feature/dark-room` quando `WebGL/**` muda, e também por `workflow_dispatch`.

**Configuração única no GitHub:** *Settings → Pages → Source: **GitHub Actions***. A URL fica `https://<usuario>.github.io/UNITY-YunaLegend/`.

> A pasta `WebGL/` é artefato de build e pesa dezenas de MB. Se o repositório ficar grande demais, publique em uma branch `gh-pages` (ou use o caminho B) e mantenha `WebGL/` fora do branch principal.

## Caminho B — build no Actions

1. Gere o arquivo `.ulf` da licença Unity (fluxo `unity-request-activation-file` do game-ci).
2. Em *Settings → Secrets and variables → Actions*, crie `UNITY_LICENSE` (conteúdo do `.ulf`), `UNITY_EMAIL` e `UNITY_PASSWORD`.
3. Rode **Build WebGL com Unity e publicar no Pages** manualmente.

Limitação conhecida: o `game-ci/unity-builder` escolhe a imagem Docker pela versão do Unity. Se `unityci/editor:6000.3.28f1-webgl-3` não existir no Docker Hub, o job falha ao baixar a imagem — nesse caso use um runner self-hosted com o Unity instalado (troque `runs-on`) ou o caminho A.

## Mobile: o que funciona e o que limita

- **Toque já é suportado**: `Yuna25DPlayerController` lê `Touchscreen.primaryTouch` (além de teclado, mouse e gamepad). A tela de entrada usa uGUI, que aceita toque pelo `InputSystemUIInputModule`.
- **Áudio**: navegadores móveis só liberam áudio após o primeiro toque. O primeiro toque no menu já serve.
- **Orientação**: o jogo é 2.5D horizontal. Em retrato o HTML mostra o aviso para girar; a build usa rotação automática.
- **Desempenho**: URP com sombras e pós-processamento é pesado em celular intermediário. Se travar, reduza sombras/resolução ou ofereça uma variante de qualidade baixa (decisão de arte, não automática).
- **iOS Safari**: exige WebGL2 (iOS 15+). Abas com pouca memória podem derrubar o contexto; a memória inicial de 256 MB ajuda a reduzir picos.
- **GitHub Pages** limita o site publicado (ordem de 1 GB) e **não** permite cabeçalhos customizados — por isso gzip+fallback e threads desligadas.

## Problemas comuns

| Sintoma | Causa provável | Correção |
| --- | --- | --- |
| Materiais magenta | pipeline URP não atribuído | `YunaLegend > Configurar Renderizacao URP` |
| Tela preta ao abrir | memória inicial baixa | aumente `YUNA_WEB_INITIAL_MB` |
| Erro de `SharedArrayBuffer` | build com threads | mantenha `threadsSupport = false` |
| 404 em arquivos `.gz` | servidor sem cabeçalho | use a build com `decompressionFallback` |
| Página branca | `WebGL/index.html` ausente no commit | gere e commite a pasta |
| Entrada não abre o jogo | ordem das cenas | `TitleScreen` deve ser a primeira |

## Segurança

A página é pública. A build não contém segredos, credenciais nem o banco do diário (que fica fora de `Assets/`). Não commite `Library/`, `Build/WebPreview` nem logs.
