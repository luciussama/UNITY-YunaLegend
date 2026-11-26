# Correções mobile — 26/11/2026 (`20261126-mobile-camera-dialogues`)

Pacote integrado de correções exclusivas para mobile:

- **Cartas (Quarto Escuro):** penumbra fixa e câmera mantendo referência da menina durante o desabamento.
- **Arena:** enquadramento mobile mais próximo, afastamento gradual e margem retrato para a locomotiva inteira.
- **Diálogos:** caixas acima da cabeça, `14px` lógicos, `lineSpacing 1`, `padding 6px` e espaço `2px`, altura pelo conteúdo; textos e ações preservados.
- **Jardim (Picnic):** afastamento moderado em paisagem (`Z = -16.2`, +20% sobre `-13.5`) e abertura retrato adaptativa (`world_width >= 4.8`, limite `Z = -22.5`) para enquadrar Yuna e o próximo apoio à frente (`PicnicMobileCameraFraming` + `PicnicSceneryController`), mantendo escala visual `1.15` e suavização XYZ (`5/s`) com âncora vertical no último pouso.

Desktop segue os caminhos anteriores. `TitleScreen` e arquivos de seleção/ordem de cenas não foram alterados; `git diff` dos 4 arquivos protegidos está vazio e os hashes conferem com `protected-files.json`. Adaptador de diálogo também passou snapshot `TITLE_DESKTOP_NO_MUTATION_OK`.

## Validações reais

- **Unity `6000.3.28f1` em clone isolado (`Build/PicnicVisualAuditProject`):** todos os 14 arquivos-fonte em `source-hashes.json` e 4 arquivos protegidos em `protected-files.json` conferem SHA256 100% entre checkout central e clone.
- **Arena (`37/37` EditMode):** 6 poses `2/5/9m` em `390×844` e `844×390` com todos os bounds horizontais de Yuna/inimigo dentro da tela (`../../MobileArenaCamera`).
- **Cartas (`2/2` PlayMode):** controlador 3D real em `30/60/120 Hz`, influência do colapso na câmera `0m`, diferença física `0m`, 20 materiais na penumbra e restauração desktop verificadas (`../../MobileCards/final`).
- **Diálogos:** 15 renders estáticos de cinco fases + 4 PlayMode do quarto escuro/introdução dos brinquedos, `TITLE_DESKTOP_NO_MUTATION_OK`.
- **Jardim (`25/25` EditMode):** 10 novos testes unitários em `PicnicMobileCameraFramingTests` + 6 capturas Editor (`390×844` e `844×390` em `porch-edge`, `suspended` e `river`) com marcador `PICNIC_MOBILE_CAMERA_PLANNING_OK` (`../../MobilePicnicCamera`).
- **Build WebGL integrada (`WebCiBuild.Build`):** concluída com `exit 0` e marcador `YUNA_WEB_PAGES_OK` em `WebGL/`, manifesto atualizado em `WebGL/release.json` (`releaseId: 20261126-mobile-camera-dialogues`).

## Limitações honestas e pendência

- Não houve percurso integral de todas as fases, combate completo ou teste em aparelho físico mobile nesta execução.
- Commit na branch `fix/mobile-camera-dialogues`, merge em `main` e push para publicação no GitHub Pages aguardam confirmação humana explícita.
