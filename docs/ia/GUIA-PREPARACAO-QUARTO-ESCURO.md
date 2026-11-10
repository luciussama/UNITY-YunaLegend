# Guia de Preparação e Direção Artística 3D: O Quarto Escuro (Unity 6.3 URP)

Este documento sintetiza os parâmetros de engenharia, direção de arte e fluxo de produção para a primeira parte do jogo (**Capítulo 1 — Parte 1: O Quarto Escuro**), consolidando os requisitos dos documentos `00-VISAO-GERAL-E-SISTEMAS-GLOBAIS.md` e `01-CAPITULO-1-PARTE-1-QUARTO-ESCURO.md`.

---

## 1. Visão Artística 3D e Atmosfera Noturna

- **Estética Geral:** Plataforma 3D Lateral (2.5D) no estilo conto de fadas estilizado (*Ori*, *Little Nightmares* sem elementos de terror grotesco, *Rayman Legends*).
- **Escala:** Yuna é pequenina (escala de brinquedo) num quarto monumental com móveis de carvalho, livros gigantes e brinquedos clássicos.
- **Paleta de Cores do Quarto:**
  - *Fundo e penumbra:* Azul-índigo profundo, violeta-escuro e toques de azul-meia-noite.
  - *Luzes pontuais mágicas:* Dourado-âmbar quente da fadinha Nanda (`#FDE047` / `#D97706`), halo sutil pêssego da Yuna e feixes volumétricos azul-prateados através de janelas vitorianas.
- **Tipografia Narrativa:** Fonte manuscrita **Caveat** (`Assets/Fonts/Caveat.ttf`, sob licença SIL OFL 1.1) aplicada na narração poética sobre a cena.

---

## 2. Regras Invioláveis de Implementação (White Room Unity)

1. **Zero Linhas ou Hitboxes Artificiais de Plataforma (Regra 3.1):**
   - É expressamente proibido colocar linhas neon, traços brilhantes ou faixas no topo dos móveis para indicar superfícies pisáveis.
   - O jogador deve entender o caminho através da geometria física tridimensional dos objetos, reforçada pela iluminação quente de Nanda que antecipa a próxima plataforma.
2. **Câmera Lateral Estritamente Estável (Regra 3.2):**
   - Durante a jogabilidade ativa, a câmera faz rolagem apenas no eixo **X**.
   - É proibida qualquer variação de altura (Y), FOV ou inclinação reativa a pulos ou aterrissagens. A altura e distância são fixas por seção para permitir leitura precisa de parábolas de salto. O script `SideScroll25DCamera.cs` implementa essa trava física.
3. **Sem Vazamento de Input de Diálogo:**
   - O comando de confirmação de diálogos (Espaço / Botão A / Toque) nunca deve disparar o pulo de Yuna no frame seguinte; a liberação do botão é mandatória antes de aceitar ação física.

---

## 3. Estrutura do Percurso de Plataformas (28 Plataformas Mapeadas)

O catálogo canônico está formalizado em `DarkRoomPlatformCatalog.cs`:

- **Fase 1: Subida ao Castelo (Índices 0 a 9, Esquerda para Direita):**
  - Começa na Cabeça do Urso de Pelúcia (`X = 165`), sobe por livros, penteadeira, cômoda, blocos ABC, tambor, almofada de veludo, gavetas, caixa de música e culmina no **Castelinho de Blocos (`X = 1320`)**, onde ocorre a cutscene com fala de Nanda e Yuna.
- **Fase 2: A Fuga (Índices 10 a 21, Rolagem Acelerada para Direita):**
  - Câmera em avanço forçado com 12 níveis de impulso de pulo (`1/12` a `12/12`).
  - Passa por trilhos suspensos, abajur cogumelo, telhado de bonecas, globo ilustrado, pipa, lustre de cristais, relógio cuco e atinge o **Portal dos Sonhos (`X = 3455`)**.
- **Plot Twist da Porta Falsa:**
  - A porta de madeira tomba e cai no abismo; Yuna aterrissa no piso (`Y = -482`), e Nanda mostra o caminho reverso com indignação cômica.
- **Fase 3: A Subida Caótica (Índices 22 a 37, Direita para Esquerda):**
  - Retorno acelerado de `X = 3310` até `X = 45`.
  - 15 níveis reversos através de blocos tombados, boneca torta, locomotiva descarrilada, xilofone, cartas e grimório, finalizando no **Verdadeiro Portal dos Sonhos (`X = 45`)**, que abre a transição luminosa para a Sala de Brinquedos (Capítulo 1, Parte 2).

---

## 4. Configuração Técnica URP (Unity 6.3 LTS)

- **Render Pipeline:** Universal Render Pipeline (URP 17.0.3).
- **Modo de Renderização:** *Forward+* recomendado para suportar até 32 luzes pontuais no Android e 256 no PC (feixes de janelas, Nanda, abajur, lustre e portais).
- **Input System:** Novo pacote `com.unity.inputsystem` (1.13.1) ativo por padrão em `ProjectSettings.asset`, unificando Teclado, Gamepad e Toque em tela (Android).

---

## 5. Como Gerar o Blockout 3D na Unity

1. Abra o projeto no **Unity Hub** selecionando a versão `6000.3.28f1`.
2. Abra ou crie uma cena vazia em `Assets/Scenes/DarkRoom_Blockout.unity`.
3. Crie um GameObject vazio na cena e adicione o componente `DarkRoomBlockoutBuilder`.
4. No Inspector ou via clique direito, acione o menu de contexto: **"Construir Blockout Completo"**.
5. As 28 plataformas serão instanciadas com caixas 3D proporcionais, nomes descritivos, colisores invisíveis e marcadores informativos.
