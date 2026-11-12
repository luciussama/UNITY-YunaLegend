# 00 — Visão Geral, Direção 3D (2.5D) e Sistemas Globais (Unity)

> **Propósito deste documento (Estratégia White Room):**
> Este documento é o **Prompt Mestre de Arquitetura, Direção de Arte 3D e Sistemas Globais** do jogo **"O Quarto dos Brinquedos"**. Ele deve ser fornecido ao gerador/agente na Unity antes ou junto com os capítulos individuais para estabelecer as regras universais do projeto.
> **Regra de Ouro:** Construa tudo nativamente na Unity em **3D**, sem depender de nenhum sprite 2D legado. Todo o jogo utiliza modelos 3D, iluminação dinâmica, materiais físicamente baseados (PBR/Stylized) e física/controladores nativos da Unity.

---

## 1. Conceito Geral e Mudança de Dimensionalidade (3D Plataforma Lateral / 2.5D)

**"O Quarto dos Brinquedos"** é uma aventura narrativa de plataforma e imaginação infantil dividida em **2 Capítulos (8 fases/partes jogáveis)**.
Na versão original o jogo alternava entre 2D e 3D; **nesta nova versão para Unity, o jogo inteiro é 100% 3D**, adotando a linguagem de **Plataforma 3D Lateral (2.5D)** consagrada por jogos como *Ori and the Will of the Wisps*, *Rayman Legends* e os jogos modernos da série *Super Mario Bros. Wonder / New Super Mario Bros.*:

1. **Jogabilidade em Plano Lateral com Mundo 3D Profundo (2.5D):**
   - Nas fases de plataforma e combate (Quarto Escuro, Arena Imaginária, Um Dia no Jardim, Jardim dos Sonhos, Pesadelo Furtivo e Brinquedão), a ação da jogadora acontece sobre um plano principal (eixo X para esquerda/direita e eixo Y para salto/altura), mas todo o cenário possui **profundidade 3D real no eixo Z**:
     - **Primeiro Plano (Foreground):** Objetos desfocados próximos à lente (folhagens, poeira mágica flutuante, silhuetas de móveis, partículas de luz) que passam com paralaxe natural da câmera perspectiva 3D.
     - **Plano Jogável (Midground / Gameplay Plane):** Onde Yuna, Nanda, inimigos e superfícies pisáveis (móveis 3D, pontes, pedras, hastes de grama, tobogãs) têm colisão física real.
     - **Planos de Fundo (Background Layers):** Paredes do quarto, janelas com luar volumétrico, jardins profundos, árvores gigantes, horizonte atmosférico e criaturas colossais (como a Barata Monumental que patrulha no plano de fundo projetando feixes de luz 3D sobre o plano jogável).
2. **Exceções de Movimentação 3D Livre / Corredor:**
   - **Capítulo 1 — Parte 2 (Sala de Brinquedos - Exploração):** A câmera adota um enquadramento 3D panorâmico elevado (estilo diorama / *Luigi's Mansion* / *Link's Awakening Remake*), permitindo que Yuna caminhe pelo piso do quarto em **X e Z** (profundidade e largura) para pegar brinquedos espalhados e levá-los até o baú.
   - **Capítulo 1 — Parte 4 (Perseguição do Urso):** Uma corrida em corredor 3D (*3D Runner* estilo *Crash Bandicoot* / *Temple Run* de costas para a personagem), com câmera *over-the-shoulder* acompanhando Yuna enquanto ela corre ao longo do eixo Z por curvas e rampas de 720 metros dentro do quarto agigantado, desviando lateralmente (eixo X) de brinquedos arremessados.

---

## 2. Protagonistas e Elenco Principal (Especificação de Modelos e Rigs 3D)

### 2.1. Yuna (A Menina Protagonista)
- **Identidade:** Uma garotinha curiosa, sensível e imaginativa. No Capítulo 1, nas falas iniciais de caixa de diálogo, seu rótulo aparece como `MENINA` (pois seu nome ainda é segredo para o jogador); na abertura do Capítulo 2 (Um Dia no Jardim), Nanda revela seu nome: **Yuna**, e a partir daí todos os diálogos usam `YUNA`.
- **Visual 3D:**
  - Proporções estilizadas de animação 3D (*stylized storybook / Pixar / Ghibli*): cabeça expressiva com olhos grandes castanho-escuros e pontos de brilho especular, bochechas levemente coradas, pele clara tom pêssego.
  - Cabelo castanho-quente com franja macia sobre a testa e um **rabo de cavalo alto** preso no topo da cabeça por um **laço de fita vermelho-rubi** (com física secundária de mola/*spring bone* na mecha do rabo de cavalo e nas pontas do laço durante corridas e saltos).
  - Vestido infantil vintage cor creme/marfim com gola e barra adornadas por babados e um vivo vermelho-carmim; meias brancas e sapatinhos escuros arredondados.
- **Conjunto de Animações 3D Obrigatórias no Rig da Yuna:**
  - *Quarto Escuro / Plataforma Geral:* Dormindo encolhida no chão (`sleep`), despertando esfregando os olhos (`wake`), parada respirando (`idle`), caminhada/corrida (`walk`/`run`), antecipação de salto, subida aérea, pico do salto, queda e amortecimento de pouso (`jump_start`, `jump_air`, `fall`, `land`), comemoração saltitante (`celebrate`), susto/choque (`shocked`).
  - *Sala de Brinquedos:* Pegar brinquedo no chão com as duas mãos (`pickup`), caminhar carregando brinquedo à frente do peito com inclinação proporcional ao peso (`carry_light`, `carry_medium`, `carry_heavy`), depositar brinquedo no baú (`drop_in_chest`), segurar e erguer a Espada de Brinquedo acima da cabeça (`sword_raise`).
  - *Arena Imaginária (Combate):* Postura de guarda/idle com espada (`sword_idle`), caminhada com espada (`sword_walk`), agachamento defensivo/esquiva baixa (`crouch`), bloqueio firme com a espada erguida à frente (`guard`), golpe horizontal/diagonal de espada em 3 fases — preparação, corte ativo e recuperação (`sword_attack`), pose de carregamento e disparo do Especial Arco-Íris (`special_attack`), recuo de dano (`damage`).
  - *Jardim (Reações Sensoriais):* Caminhada cautelosa com careta de incômodo sensorial na grama alta (`tall_grass_walk`), corrida acelerada tentando equilibrar os braços nas pedrinhas (`pebbles_run`), corrida em pânico olhando para trás ao fugir do besouro (`beetle_panic_run`), tropeço/queda ao tocar num inseto (`hurt_fall`), sentada na toalha de piquenique comendo e adormecendo (`picnic_sit`, `picnic_sleep`).
  - *Jardim dos Sonhos e Pesadelo:* Segurar nas duas bordas de uma grande folha verde funcionando como paraquedas/asa-delta (`leaf_glider`), caminhar na ponta dos pés em furtividade (`tiptoe_stealth`), agachar escondida atrás de pedras e troncos (`stealth_hide`), saltar na lama (`mud_jump`), nadar na lama com a cabeça fora d'água (`mud_swim`), submergir completamente na lama deixando só bolhas (`mud_dive`), escalar a margem de lama em 6 estágios apoiando as palmas das mãos e joelhos (`mud_climb_out`).
  - *Brinquedão:* Escorregar sentada em rampas e tobogãs com os braços abertos (`slide`), quicar em camas elásticas e lonas (`trampoline_bounce`).

### 2.2. Nanda (A Fadinha Guia)
- **Identidade:** Melhor amiga, protetora e guia da Yuna. Bem-humorada, às vezes impaciente quando algo dá errado, mas profundamente acolhedora.
- **Visual 3D:**
  - No jogo normal, é uma pequenina fada luminosa (cerca de 15 a 20 cm em relação à Yuna), com corpo delicado dourado-claro, vestido/túnica cintilante, rostinho expressivo e **quatro asas diáfanas translúcidas** (tons de ciano, rosa-choque e ouro) que batem rapidamente.
  - Emite uma **luz pontual quente (Point Light)** ao seu redor e deixa um rastro suave de partículas de poeira estelar dourada/rosada.
  - **Exceção no Capítulo 2 — Parte 2 (Abertura do Jardim dos Sonhos):** Apenas na abertura onírica da floresta dos sonhos, Nanda aparece **do mesmo tamanho humano que Yuna**, deitada dormindo profundamente sobre a grama fria, com suas asas translúcidas dobradas junto às costas e sem emitir seu brilho guia habitual.
- **Comportamento 3D da Nanda:**
  - Segue Yuna com movimento amortecido por mola (*smooth damp*) pairando levemente atrás e acima do ombro da menina, ou voa à frente para apontar o próximo objetivo/plataforma.
  - Possui estados emocionais claros que alteram sua cor/intensidade de brilho e postura: `normal` (dourado suave), `happy` (cintilação viva com piruetas no ar), `annoyed` (tom âmbar/avermelhado quando a porta falsa cai), `facepalm` (leva a mãozinha à testa em silêncio no epílogo do quarto arrumado).

---

## 3. Regras Invioláveis de Câmera e Direção Visual

Estas regras definem a identidade física e artística do jogo e **devem ser seguidas rigorosamente na implementação Unity**:

### 3.1. Direção Visual das Superfícies e Plataformas
1. **Zero Linhas ou Realces Artificiais de Hitbox:** É **estritamente proibido** desenhar traços, barras luminosas, contornos neon, réguas de contato ou decalques artificiais sobre o topo dos móveis, livros, brinquedos, pedras ou pontes para indicar onde pisar.
2. **Geometria Natural 3D:** Prateleiras, tampos de cômodas, cabeças de pelúcia, livros e pontes devem parecer objetos 3D naturais do mundo. A jogadora entende onde pisar pela silhueta 3D, pela iluminação natural da cena e pela posição da fadinha guia.
3. **Colisores Invisíveis Precisos:** Os `BoxCollider` / `MeshCollider` de piso devem coincidir exatamente com o tampo físico visível do modelo 3D onde os pés da Yuna pousam. Sobreposições técnicas de colisão só podem existir em modo de depuração (*Gizmos* do Editor), jamais na câmera de jogo.
4. **Exceção do Relógio Cuco (Plataforma 19 do Quarto Escuro):** A estrutura do relógio cuco preso à parede incorpora um suporte/base de madeira modelado fisicamente onde os pés da menina apoiam, sem linhas artificiais.

### 3.2. Comportamento da Câmera Lateral (2.5D)
1. **Recorte fechado acompanhando Yuna (direção revisada pelo usuário em 11/11/2026):** No Quarto Escuro, a câmera acompanha a menina e revela principalmente o próximo obstáculo. O enquadramento deve transmitir desorientação, sem fornecer uma vista geral de onde ela está no quarto. Esta orientação substitui a regra anterior de altura fixa por seção.
2. **Altura do percurso e estabilidade do salto:** A altura da câmera acompanha suavemente a progressão dos apoios, sem perseguir o arco de cada salto. A distância é dimensionada pelo apoio atual e pelo próximo alvo, com transição suave quando o alvo muda. FOV e inclinação permanecem estáveis. O solo deve sair do quadro durante a subida e aparecer na aproximação inicial e na queda ao chão.
3. **Queda e momentos narrativos:** Durante a queda, a câmera acompanha a menina verticalmente até o solo. Pausas e diálogos mantêm o foco nela; ao retomar a corrida, o enquadramento se orienta ao próximo salto, inclusive na inversão para a esquerda.
4. **Demonstração do castelo:** Na breve pausa de Yuna sobre o castelo, a câmera se afasta para mostrar o restante do caminho até a porta falsa enquanto Nanda indica a saída, como um tutorial visual. Ao iniciar a Fuga, volta suavemente ao close. Esse panorama é uma exceção narrativa ao recorte restrito da jogabilidade.

---

## 4. Sistema de Diálogos e Narração Poética

O jogo possui **duas linguagens de texto narrativo** que funcionam em conjunto:

### 4.1. Caixa de Diálogo Vitoriana (Falas de Personagens)
Usada para conversas diretas entre `NANDA` e `MENINA` / `YUNA`:
- **Estética:** Uma caixa horizontal elegante posicionada na área segura da tela (com faixas cinematográficas *letterbox* pretas de 42px no topo e na base durante cutscenes). O fundo da caixa tem gradiente de veludo noturno (`#1a1426` para `#0d0914`), borda dupla dourada (`#d97706` e `#fde047`, mudando para vermelho `#ef4444` durante o *Plot Twist* da porta falsa) e pequenas joias douradas nos 4 cantos.
- **Retrato e Nome:** À esquerda da caixa, um medalhão circular ornamentado exibe o retrato expressivo de quem fala (`NANDA` com halo dourado/rosa/ciano, ou `MENINA`/`YUNA` com halo pêssego/rosa/violeta) acompanhado do crachá com o nome abaixo do círculo.
- **Tipografia e Destaques:** Fonte serifada clássica em itálico (estilo *Palatino / Georgia*) na cor creme claro (`#fef9c3`). A expressão `*risos*` dentro de qualquer fala é sempre destacada em **negrito itálico rosa (`#f472b6`)**.
- **Controles de Avanço:** No canto inferior direito da caixa, exibe o aviso sensível ao dispositivo: `Espaço para continuar ➔` (teclado), `(X) para continuar ➔` (gamepad) ou `Toque na tela para continuar ➔` (mobile), além de botão de `Pular cena` quando a cena já foi vista anteriormente. Um clique/aperto que avança um diálogo **nunca deve vazar** como comando de pulo para a personagem no frame seguinte (exigir soltar o botão antes de aceitar pulo).

### 4.2. Narração Poética Manuscrita (Narrador)
Usada para as reflexões poéticas e contos de fada que acompanham as cenas:
- **Estética:** Sem moldura pesada. O texto surge diretamente sobre a cena (em área segura que nunca cubra o rosto da Yuna ou da Nanda) usando uma **fonte manuscrita caligráfica** (como *Caveat* ou similar), na cor marfim suave com sombra difusa escura para leitura perfeita sobre qualquer fundo.
- **Ritmo:** As linhas poéticas aparecem com transição suave (*fade-in* / *fade-out*), permitindo que a cena 3D respire como um livro ilustrado vivo.

---

## 5. Modos de Dificuldade e Sistema de Ajuda ("Deseja tentar no fácil?")

O jogo possui dois modos de dificuldade selecionáveis na Tela Inicial: **NORMAL** e **FÁCIL (`EASY`)**.

### 5.1. O que muda no Modo Fácil (`EASY`)
1. **Quarto Escuro (Fuga e Subida Caótica):** A velocidade da rolagem forçada da câmera é reduzida (fator `0.78x`), dando mais tempo para saltar entre os móveis.
2. **Arena Imaginária (Combate):** Os chefes causam menos pressão encadeada e possuem janelas de recuperação mais generosas.
3. **Perseguição do Urso (Corredor 3D):** A menina possui mais corações de vida/tolerância a tropeços, a deriva centrífuga nas curvas é reduzida (`0.25` vs `0.55` no Normal), a invulnerabilidade pós-tropeço dura `2.0s` (vs `1.3s`), e a pressão final de arremessos contínuos nos últimos 15% da pista é desativada.
4. **Um Dia no Jardim:** As nuvens temporárias da seção aérea demoram mais para dissipar (`1.75s` no Fácil contra `1.25s` no Normal).
5. **Jardim dos Sonhos e Pesadelo:** A travessia no voo de folha tem folga maior nos galhos espinhosos, e a janela de detecção dos feixes de luz da Barata Monumental é mais tolerante.
6. **Brinquedão:** As pontes frágeis dos castelos demoram `1.6x` mais para cair após o toque, e a onda de colapso dos 15 blocos coloridos no 3º tobogã corre `1.3x` mais devagar.

### 5.2. Oferta Automática do Modo Fácil após 3 Derrotas
- Se a jogadora estiver jogando no modo **NORMAL** e acumular **3 derrotas (Game Over)** na mesma etapa da aventura, o jogo exibe uma janela modal acolhedora antes de reiniciar:
  - **Título:** `deseja tentar no fácil?`
  - **Texto:** `Você continua nesta parte da aventura, no nível fácil.`
  - **Botões:** `Não` (mantém Normal e tenta de novo) e `Sim` (muda a campanha para Fácil imediatamente, mantendo o progresso atual).

---

## 6. Tela Inicial (Menu Principal 3D Ilustrado)

- **Cenário 3D de Fundo da Tela Inicial:**
  - Um canto aconchegante e iluminado do quarto de leitura. À direita do enquadramento, sobre um tapete circular macio, há uma grande poltrona clássica estofada onde **Yuna está sentada lendo um livro ilustrado**.
  - **Animação Viva:** Enquanto o menu está aberto, Yuna balança suavemente as duas perninhas (canelas, meias brancas e sapatinhos) para frente e para trás em um ritmo tranquilo e contínuo (`seno(tempo * 2.6)`), piscando os olhos ocasionalmente enquanto partículas de luz flutuam pelo ar.
- **Interface do Menu Principal (à esquerda):**
  - **Título Principal:**
    `O Quarto`
    `dos Brinquedos`
  - **Botões de Ação:**
    1. `Novo jogo` (Se já houver uma campanha salva, abre confirmação: **Título:** `Recomeçar a aventura?` / **Texto:** `Toda a sua aventura será reiniciada. Você voltará ao começo da história e perderá o progresso salvo. Deseja continuar?` / **Botões:** `Cancelar` e `Recomeçar`).
    2. `Carregar jogo` (Habilitado se houver save; exibe dica `Carregar retoma sua aventura e a dificuldade salva.` ou, se desabilitado, `Nenhuma aventura salva neste dispositivo.`).
    3. `Modo fácil: Desativado / Ativado` (Botão *toggle* que alterna a dificuldade para uma nova partida).
  - **Barra Superior de Controles Globais (presente durante todo o jogo):**
    - Botão de **Pausa (`P`)**: congela simulação, timers e músicas.
    - Botão de **Som / Mudo (`M`)**: alterna o áudio global.
    - Seletor de **Revisão de Fase (Debug/Capítulo)**: permite testar diretamente qualquer uma das fases (`Quarto escuro`, `Sala de brinquedos`, `Arena de batalha`, `Perseguição 3D`, `Um dia no jardim`, `Jardim dos sonhos`, `Brinquedão`) a partir de sua cena inicial.

---

## 7. Índice dos Documentos de Roteiro por Capítulo

Para alimentar o Codex na Unity de forma modular e organizada, o roteiro completo está dividido nos seguintes documentos dentro desta pasta (`docs/roteiro-unity/`):

1. **`00-VISAO-GERAL-E-SISTEMAS-GLOBAIS.md`** *(Este documento)* — Direção 3D/2.5D, personagens, câmera, UI, diálogos e menu.
2. **`01-CAPITULO-1-PARTE-1-QUARTO-ESCURO.md`** — Abertura, Tutorial, Fase 1 (Subida ao Castelo), Fase 2 (A Fuga), Plot Twist da Porta Falsa e Fase 3 (Subida Caótica até o Portal Verdadeiro).
3. **`02-CAPITULO-1-PARTE-2-SALA-DE-BRINQUEDOS-EXPLORACAO.md`** — Introdução da Sala Iluminada, os 7 móveis, os 8 brinquedos com pesos, o mistério dos brinquedos que voltam sozinhos, a investigação do baú e a descoberta da Espada de Brinquedo.
4. **`03-CAPITULO-1-PARTE-3-ARENA-IMAGINARIA.md`** — A transformação épica do quarto em Arena 3D, mecânica de combate lateral 2.5D, roubo de corações, Especial da Espada Arco-Íris com close de anime e os 4 Chefes (Trenzinho, Robô, Dinossauro e Urso).
5. **`04-CAPITULO-1-PARTE-4-PERSEGUICAO-DO-URSO-E-EPILOGO.md`** — Abertura com o Urso caído, a corrida em corredor 3D de 720m desviando de brinquedos, captura e retorno ao baú, Epílogo do Quarto Arrumado, *facepalm* da Nanda e cartão do Capítulo 2.
6. **`05-CAPITULO-2-PARTE-1-UM-DIA-NO-JARDIM.md`** — Abertura na escada da varanda (revelação do nome de Yuna), mecânicas sensoriais de terreno (grama alta, pedrinhas, madeira, nuvens, rodas d'água), inimigos do jardim, os 3 encontros de pânico com o Besouro e o Piquenique sob a Grande Árvore.
7. **`06-CAPITULO-2-PARTE-2-JARDIM-DOS-SONHOS-FLORESTA-E-VOO.md`** — Abertura silenciosa e fria com Nanda em tamanho humano adormecida, escalada aérea em hastes de grama gigantes que dobram ao vento, viagens nas costas de besouros voadores, abelhinhas do bem com cotonete, teias de aranha e o Voo de Folha por 21 pares de galhos espinhosos.
8. **`07-CAPITULO-2-PARTE-3-PESADELO-FURTIVO-E-DESPERTAR.md`** — Chegada ao pântano sombrio, aparição da Barata Monumental no fundo, furtividade entre sombras de pedras e troncos contra os feixes de luz dos olhos da barata, travessia a nado e mergulho em 4 lagos de lama, captura cinematográfica e o despertar no piquenique.
9. **`08-CAPITULO-2-PARTE-4-O-BRINQUEDAO-FINAL.md`** — Convite ensolarado pós-pesadelo, o gigantesco circuito externo do Brinquedão (escorregadores, pontes com peixes saltadores, camas elásticas, castelos nas alturas, parede elástica de lançamento e calhas de recuperação em U) e os 4 Tobogãs Internos até o encerramento emocionante da campanha.
