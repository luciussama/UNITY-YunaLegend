# Tela de entrada — O Quarto dos Brinquedos

11/11/2026 · sessão `2026-11-11-dsh-tela-entrada-7f31` · DeepSeek Harness (deepseek-flash).

Boas-vindas construída a partir do **material canônico fornecido** (Yuna lendo na poltrona vermelha). O fundo foi recortado sem os textos embutidos; o menu é UI nativa.

## O que a tela tem

- **Fundo**: arte canônica recortada em 16:9 (`Title_Background.png`), sem título/menu assados.
- **Sombra lateral** (`Title_LeftShade.png`) para legibilidade do texto.
- **Título** "O Quarto dos Brinquedos" em creme com sombra.
- **Menu canônico**: `Novo jogo`, `Carregar jogo` (desabilitado sem save), `Modo fácil` com etiqueta `Ativado`/`Desativado`.
- **Rodapé**: "Nenhuma aventura salva neste navegador." (ou "Aventura salva encontrada…").
- **Engrenagem** no canto superior direito (`Title_Gear.png`) abrindo um painel de configurações com **volume geral**.
- **Navegação**: mouse, teclado e gamepad via `InputSystemUIInputModule` (o projeto usa apenas o novo Input System). Marcador `›` acompanha o item focado.

## Comportamento

- `Novo jogo` → carrega `DarkRoom_Gameplay`.
- `Carregar jogo` → só habilita com `HasSave` (flag em `PlayerPrefs`); hoje o jogo ainda não grava save, então permanece desabilitado.
- `Modo fácil` → alterna e persiste (`PlayerPrefs`), exposto por `TitleScreenController.EasyMode` para o gameplay consumir.
- Volume → `AudioListener.volume`, persistido.
- `TitleScreen` é a **primeira cena** do Build Settings e do build Web (`WebPreviewBuild` agora inclui entrada + gameplay).

## Arquivos

- `Assets/Scenes/TitleScreen.unity`
- `Assets/Scripts/Runtime/UI/TitleScreenController.cs`
- `Assets/Scripts/Editor/TitleScreenBuilder.cs` (menu `YunaLegend/Construir Tela de Entrada`, `Capturar`, `Validar`)
- `Assets/Art/UI/Title_Background.png`, `Title_LeftShade.png`, `Title_Gear.png`
- `ProjectSettings/EditorBuildSettings.asset` (entrada índice 0, gameplay índice 1)
- `Assets/Scripts/Tests/TitleScreenTests.cs`

## Perna balançando (pedido do usuário)

Na arte canônica a menina balança a perna enquanto lê. Para animar sem repintar, a perna direita (meia + sapato) foi **recortada da própria arte** (`Title_Leg.png`, 240×260) e virou uma camada sobreposta sobre o fundo **intacto**:

- O recorte é posicionado com âncoras normalizadas idênticas ao retângulo da arte, com **pivô no joelho** — em repouso coincide pixel a pixel e é invisível.
- `TitleLegSwing` gira a perna em **±2°** com período de 3,4 s (pêndulo suave).
- O recorte mantém uma margem da poltrona em volta da perna; é essa margem que cobre o desenho original quando ela gira (por isso não foi necessário "tapar buraco" no fundo).
- `02-title-leg-swing.png` mostra o extremo do balanço; a diferença entre os dois quadros fica confinada a x 1310–1502, y 819–1074.

Arquivos: `Assets/Art/UI/Title_Leg.png`, `Assets/Scripts/Runtime/UI/TitleLegSwing.cs`. O builder instancia a camada, configura o pêndulo e a validação falha se a perna não estiver presente.

## Validação

- Cena gerada e validada por `TitleScreenBuilder.Validate()`: `TITLE_SCREEN_VALIDATED buttons=5 texts=10`, cena de destino `DarkRoom_Gameplay`, Build Settings começando pela entrada.
- Captura `01-title.png` renderizada pela câmera da própria tela (Canvas em ScreenSpaceCamera), sem overlay de editor.
- O teste NUnit `TitleScreenTests` foi adicionado à suíte, mas **ainda não executado pelo Test Runner**: o editor do projeto principal estava ocupado por trabalho paralelo durante esta entrega. Rode a suíte assim que possível.

## Limitações

- **Tipografia**: o projeto não tem fonte serifada licenciada; o título usa a fonte embutida (Arial-like). Trocar é uma linha quando houver um asset serifado (ex.: Cormorant/OFL).
- **Save real inexistente**: `Carregar jogo` depende de um flag que ninguém grava ainda.
- **Modo fácil** é persistido e exposto, mas o gameplay ainda não lê o valor.
- Painel de configurações cobre apenas volume.
- Fundo é 16:9; em ultrawide haverá corte/estiramento (CanvasScaler com match 0.5).
