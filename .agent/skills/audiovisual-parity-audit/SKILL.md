---
name: audiovisual-parity-audit
description: >-
  Realiza auditoria audiovisual completa de paridade entre o jogo canônico legado
  (https://luciussama.github.io/legendGirl/) e a implementação 2.5D na Unity
  (UNITY-YunaLegend). Use esta skill sempre que precisar comparar, auditar ou
  planejar ajustes de cenografia de fundo, texturas, iluminação, proporções,
  camadas de profundidade (paralaxe -> Z-depth) e áudio/SFX em relação à obra original.
---

# Skill de Auditoria Audiovisual de Paridade (Canônico Web vs. Unity 2.5D)

Esta skill define a metodologia profissional de **Game Art Direction & Audio QA Parity Audit** para avaliar e alinhar a cena Unity (`DarkRoom_Gameplay.unity`) com a referência canônica jogável em `https://luciussama.github.io/legendGirl/`.

> **Regra Inviolável do Projeto (`AGENTS.md`)**:
> A URL `https://luciussama.github.io/legendGirl/` deve ser usada **exclusivamente para observação, medição e comparação visual/sonora**. Nunca copie, migre ou importe código legado nem clone o repositório antigo. Toda implementação na Unity permanece 100% nativa em C# / URP.

---

## 1. As 6 Competências Essenciais da Auditoria em Ambiente de Jogo

Baseado nas melhores práticas da indústria para remasters e traduções **2D para 2.5D**, toda auditoria deve avaliar sistematicamente estes seis pilares:

### 1.1. Hierarquia de Valores e Iluminação (Value Hierarchy & Squint Test)
- **Separação de Planos (Foreground / Midground / Background)**: Em escala de cinza (*Squint Test*), a personagem (Yuna), a guia (Nanda) e a superfície de pouso das plataformas devem destacar-se imediatamente contra o fundo noturno.
- **Luz Localizada vs. Luz Global**: O quarto escuro canônico utiliza máscara de penumbra (`#101522` normal / `#080b14` dramático) com *pools* radiais suaves centrados na fada, leve halo na menina, compensação sutil nas plataformas próximas e feixes translúcidos nas janelas (opacidade máx. `0.019` que decai a zero antes do piso, sem estourar o chão com manchas duras).
- **Sinergia de Pós-Processamento**: Bloom e Vignette devem reforçar a atmosfera etérea sem lavar as cores base nem criar artefatos estourados.

### 1.2. Leitura de Silhueta, Escala e Proporção (Silhouette & Scale Accuracy)
- **Unidade de Referência Humana**: Usar sempre a altura da Yuna (`44px` no canônico = `1.40m` na Unity, fator `~0.0318 m/px`) como régua antropométrica para validar móveis, janelas, quadros e brinquedos.
- **Proibição de Oclusores Monolíticos**: Elementos de fundo nunca devem virar paredões ou cubos maciços gigantes que bloqueiem o papel de parede ou compitam com as plataformas de gameplay.
- **Silhueta Contínua**: Objetos decorativos não podem gerar blocos retangulares sem leitura de forma (ex.: cubos de cor sólida gigantes sem recorte ou moldura).

### 1.3. Tradução de Camadas de Profundidade (2D Parallax $\rightarrow$ 2.5D Z-Depth)
No canônico 2D, a sensação de profundidade vem de coeficientes de paralaxe distintos em relação a `camX`. Na Unity 2.5D (câmera perspectiva com `FOV = 45°` em `Z = -16m`), esses planos devem ser traduzidos em distâncias `Z` coerentes (ou compensação de paralaxe) e escalados para compensar a perspectiva:
- **Parede de fundo e papel de parede** (`0.15x` no canônico) $\rightarrow$ plano mais profundo (`Z = 7.5m`).
- **Guirlanda de bandeirolas no alto** (`0.20x` no canônico) $\rightarrow$ plano superior de fundo (`Z = 6.8m`).
- **Janelas noturnas em arco** (`0.30x` no canônico, `wx = [780, 1950]`) $\rightarrow$ plano médio-fundo (`Z = 6.2m`).
- **Prateleiras flutuantes altas** (`0.40x` no canônico, `wx = [580, 1400, 2100]`) $\rightarrow$ plano decorativo de parede (`Z = 5.6m`).
- **Ganchos de parede e desenhos em giz de cera** (`0.45x` no canônico) $\rightarrow$ plano de detalhes da parede (`Z = 5.2m`).
- **Piso, rodapé e brinquedos/tapetes espalhados no chão** (`1.00x` no canônico) $\rightarrow$ plano do palco (`Z = 0m` a `Z = 2.5m`, fora da linha de colisão da corrida em `Z = 0`).

### 1.4. LookDev, Paleta Cromática (Color Script) e Higiene de UV / Texel Density
- **Fidelidade de Paleta**: Verificar os tons exatos do berçário vitoriano noturno:
  - Parede base `#0d0a14` com listras verticais `#17131d`, véu de papel de parede (`alpha 0.12`) e estrelinhas douradas discretas (`#facc15` com `alpha 0.065`).
  - Rodapé de madeira escura arroxeada (`#2b2138` com friso `#3e3152`).
  - Janelas com moldura `#1a1325`/`#4d3c68`, céu `#090b14`, **lua crescente prateada** (`#dce7f5`), **divisória simples em cruz (2×2)** (`#322549`) e **cortinas translúcidas lilases** (`rgba(168, 85, 247, 0.20)`).
- **Orientação de UVs em Primitivas 3D**: O `PrimitiveType.Cube` da Unity possui orientação UV específica nas faces `+Z` e `-Z`. Sempre inspecionar a captura renderizada para garantir que arcos, letras (`ABC`) e texturas direcionais não fiquem invertidos verticalmente (de cabeça para baixo) nem estirados.
- **Densidade de Texels Uniforme**: Ajustar `mainTextureScale` proporcionalmente às dimensões em metros do objeto para evitar borrões ou repetição excessiva.

### 1.5. Composição Ambiental e Narrativa Visual (Environmental Storytelling)
- Verificar a presença dos elementos que dão vida de "quarto infantil habitado" à parede e ao chão:
  - **Guirlanda de bandeirolas coloridas** no topo da parede (`#f43f5e`, `#facc15`, `#06b6d4`, `#a855f7`, `#10b981`, `#fb923c`).
  - **Ganchos de parede (`wallPegs`)** com chapéu de bruxinho roxo com estrela, capinha vermelha de herói e cachecol listrado azul/amarelo.
  - **Desenhos infantis emoldurados (`wallDrawings`)**: sol sorridente, castelo roxo/rosa, arco-íris e gatinho laranja.
  - **Prateleiras altas (`wallShelves`)** com mãos-francesas, livrinhos inclinados, globo de neve de vidro e barquinho à vela de madeira.
  - **Cenografia de chão (`roomScenery`)**: tapetes (`striped_rug`, `fluffy_rug`), livros abertos com fita marcadora, bolinhas de gude, robô de lata, soldadinho, cavalinho de madeira, giz de cera espalhado, carrinho, meias listradas, aviãozinho de papel, berço de balanço e carretel.

### 1.6. Paridade de Áudio e Sincronia Audiovisual (Audio-Visual Sync & Mix)
- **Foley por Material**: Passos em madeira, pouso macio em pelúcia/almofada e batida percussiva no tamborito.
- **Progressão Dinâmica**: Escala de pitch/intensidade acompanhando a aceleração nas Etapas 2 e 3.
- **Equilíbrio de Mixagem**: Trilha sonora (BGM) contínua e envolvente, brilho sonoro sutil na fada Nanda e som de derrota/Game Over calibrado de forma suave (20% do volume base, sem assustar o jogador).

---

## 2. Fluxo Operacional Passo a Passo da Auditoria

Sempre que executar uma auditoria com esta skill:

1. **Registrar o Plano no Diário (`tools\ai_diary.py`)**:
   - Antes de qualquer comando operacional, consulte `python tools\ai_diary.py list --limit 50` e abra um registro `planned`.
2. **Capturar / Observar os Dois Lados nos Mesmos Enquadramentos**:
   - **Unity**: Executar `DarkRoomStartCapture.Capture` (ou estendê-lo para capturar Início da Etapa 1, Topo do Castelo / Início da Etapa 2, Porta Falsa e Portal dos Sonhos na Etapa 3) gerando PNGs em `Evidence/`.
   - **Canônico Web**: Observar e capturar os mesmos pontos em `https://luciussama.github.io/legendGirl/`.
3. **Executar a Verificação Cruzada (Checklist dos 6 Pilares)**:
   - Inspecionar visualmente as imagens geradas (`read_file` nos PNGs) e confrontar com as métricas de cor, posição, escala e UV.
4. **Documentar o Diagnóstico e Plano de Ação nas 3 Categorias Obrigatórias**:
   - **Categoria 1 — O que deu certo e não precisa mais alterar**: Listar sistemas, materiais, luzes, áudios, UI e mecânicas já validados e fiéis ao canônico, protegendo-os contra regressões.
   - **Categoria 2 — O que ainda precisa trabalhar**: Detalhar cada divergência encontrada com causa-raiz técnica (arquivo, linha, coordenadas, material/UV) e especificação exata de como deve ficar segundo o canônico.
   - **Categoria 3 — Prioridade recomendada para os itens do quesito 2**: Ordenar em ondas lógicas de execução (P0 Crítico / Defeitos Visuais Graves $\rightarrow$ P1 Estrutura Arquitetônica do Fundo $\rightarrow$ P2 Props Narrativos de Parede $\rightarrow$ P3 Props de Chão e Polimento Final) para orientar a implementação incremental segura.
5. **Salvar no Diário (`docs/ia/diario.sqlite3`)**:
   - Registrar a conclusão (`completed`) vinculada ao `--plan-id` e persistir o plano estruturado no diário para que qualquer IA subsequente saiba exatamente onde e como trabalhar.
