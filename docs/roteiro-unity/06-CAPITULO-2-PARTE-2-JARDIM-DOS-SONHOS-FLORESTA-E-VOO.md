# Documento 06 — Capítulo 2, Parte 2: O Jardim dos Sonhos (Floresta Onírica e Voo na Folha)

> **Propósito deste documento (White-Room Prompt para Unity 3D)**:
> Este documento especifica de forma completa e autossuficiente a **Parte 2 do Capítulo 2: O Jardim dos Sonhos — Floresta Onírica e Voo na Folha** de *O Quarto dos Brinquedos*. Deve ser usado como especificação direta para construir toda a cena, a abertura silenciosa com Nanda dormindo em tamanho humano, a travessia aérea vertical/lateral por hastes de grama gigantes, o voo sobre besouros de transporte, as abelhas amigas com cotonete, as teias de aranha verticais e o voo planado em folha (*Leaf Glider*) entre galhos espinhosos em **3D com jogabilidade lateral (2.5D)** na Unity, **sem depender de código ou assets legados**.
> **Restrição absoluta**: Zero linhas de código neste documento. Todas as regras, medidas proporcionais, tempos, diálogos e mecânicas estão descritos em linguagem natural estruturada.

---

## 1. Visão Geral e Papel Dramático da Fase

Após Yuna e Nanda adormecerem juntas sobre a toalha de piquenique ao final do passeio ensolarado pelo jardim, a história mergulha no **Jardim dos Sonhos**. Dentro do sono de Yuna, a proporção do mundo se inverte de forma ainda mais surreal e melancólica: o jardim calmo da tarde transforma-se em uma **floresta noturna colossal**, banhada por uma lua azulada gigante, névoa fria, esporos luminosos flutuantes e silhuetas de vegetação tão altas que uma única folha pode esconder a lua.

Mais importante: neste sonho, **Nanda tem exatamente o mesmo tamanho humano de Yuna**, mas está caída em sono profundo sobre o chão úmido da floresta e não acorda de jeito nenhum. Pela primeira vez na campanha, Yuna fica sem a voz protetora e sem a orientação da irmã fadinha, precisando avançar sozinha pelo desconhecido.

A jornada divide-se em dois grandes atos contínuos dentro deste documento (que depois desembocam diretamente no Pesadelo Furtivo do Documento 07):
1. **A Travessia das Copas e Gramas Gigantes (6 Seções Aéreas)**: Escalada e salto entre talos de grama flexíveis que balançam ao vento, travessia sobre o dorso de besouros voadores, apoio em abelhas felpudas inofensivas (que possuem um cotonete no lugar do ferrão) e escalada por fios verticais de teias de aranha patrulhadas por aranhazinhas.
2. **O Voo Planado na Folha (*Leaf Glider*)**: Ao alcançar o galho mais alto onde o vento sopra forte, Yuna segura uma grande folha verde acima da cabeça como uma asa-delta/paraquedas e plana por um longo corredor aéreo sinuoso repleto de pares de galhos espinhosos superiores e inferiores, até o vento cessar e ela cair nas profundezas escuras do sonho.

---

## 2. Direção Visual 3D, Atmosfera e Áudio

### 2.1. Cenário 3D (2.5D Side-Scrolling Aéreo em Camadas Profundas)
- **Paleta Noturna Onírica**: Tons profundos de azul-meia-noite, índigo, ciano enevoado, verde-musgo escuro iluminado por luar prateado e partículas bioluminescentes suaves.
- **Fundo Distante (*Skybox* e *Far Background* em 3D)**:
  - Uma **Lua Cheia imensa e pálida** cercada por um halo volumétrico frio e nuvens noturnas translúcidas que deslizam lentamente.
  - Silhuetas monumentais de talos de grama, dentes-de-leão gigantes e troncos distantes com efeito de profundidade de campo (*Depth of Field*) e névoa atmosférica (*Height Fog*) cobrindo o abismo inferior.
- **Plano Jugável ($Z = 0$)**:
  - **Hastes de Grama Gigantes**: Colunas vegetais cilíndricas/achatadas em 3D que brotam do abismo abaixo da câmera e sobem até o topo do quadro. Possuem superfície fibrosa verde-azulada, nós vegetais que servem de referência visual para as mãos de Yuna e animação física contínua de curvatura senoidal (balanço de vento).
  - **Folhas largas de descanso (Checkpoints)**: Grandes folhas horizontais curvadas que brotam de caules laterais, funcionando como plataformas sólidas e seguras entre uma seção aérea e outra. Quando ativadas como checkpoint, exibem uma suave pulsação luminosa azul-clara de orvalho.
- **Primeiro Plano (*Foreground* em $Z < 0$)**:
  - Folhas desfocadas em silhueta escura passando perto da lente da câmera e partículas de pólen noturno/orvalho flutuando pelo ar.

### 2.2. Modelos 3D dos Personagens e Criaturas da Floresta
- **Yuna**: Mesmo modelo 3D de vestido claro e cabelos escuros, agora recebendo iluminação de borda (*Rim Light*) azul-prateada da lua. Possui animações dedicadas para:
  - Sacudir Nanda no chão e olhar ao redor assustada na abertura;
  - Agarrar e escalar verticalmente os talos de grama e os fios de teia (mãos e pés alternando apoios enquanto o corpo acompanha a inclinação do talo);
  - Equilibrar-se sobre o casco curvo dos besouros e o dorso felpudo das abelhas;
  - Segurar o pecíolo e as bordas de uma grande folha verde acima da cabeça durante o voo planado, inclinando o corpo para cima ou para baixo conforme a corrente de ar.
- **Nanda em Tamanho Humano (Apenas na Clareira Inicial)**:
  - Pela única vez no jogo, o modelo 3D de Nanda é renderizado na **mesma escala de Yuna** (tamanho de criança humana), mantendo suas asas translúcidas de fada dobradas e adormecidas junto às costas, deitada serenamente no chão da clareira fria.
- **Besouros de Transporte (*Carapaças à Deriva*)**:
  - Grandes besouros arredondados em 3D com carapaça quitinosa azul-esverdeada iridescente. Seu dorso superior funciona como uma plataforma móvel onde Yuna pode pisar. Quando Yuna sobe neles, suas élitros se entreabrem levemente e as asas membranosas batem com um zumbido grave e calmo.
- **Abelhas Amigas com Cotonete (*Asas sobre o Abismo*)**:
  - Abelhas rechonchudas e peludas em 3D, com listras amarelas e marrons suaves sob a luz noturna.
  - **Detalhe Visual Crítico**: Como este mundo nasce da mente infantil de Yuna, **no lugar do ferrão traseiro cada abelha possui a haste e a ponta branca macia de um cotonete!** Elas são totalmente pacíficas: Yuna pode pousar sobre suas costas peludas ou agarrar suas laterais felpudas para subir até o topo.
- **Fios de Teia e Aranhazinhas (*Fios entre as Nuvens*)**:
  - Fios verticais de seda branca reluzente esticados entre a copa superior invisível e o abismo inferior.
  - Em cada fio há uma **Aranhazinha 3D** escura de olhos brilhantes que sobe e desce ciclicamente pelo próprio fio. Diferente dos besouros e das abelhas, tocar na aranha assusta/derruba Yuna do fio.

### 2.3. Direção de Áudio
- **Abertura Silenciosa**: Durante toda a cena de abertura com Nanda adormecida (38 segundos), **não há música de fundo** — apenas uma ambiência muito sutil de vento frio noturno (*cold silence hum*), reforçando a solidão e o estranhamento do sonho.
- **Trilha Musical da Floresta e do Voo**: Assim que Yuna decide seguir em frente sozinha (Beat 7 da abertura), inicia-se a faixa instrumental melancólica e etérea **"Misty Blues"** (blues/jazz instrumental lento e sonhador), que acompanha tanto as 6 seções de escalada quanto todo o voo planado na folha.
- **Efeitos Sonoros**: Farfalhar de folhas ao agarrar gramas, zumbido grave de asas de besouro, zumbido suave das abelhas, som de seda esticada nas teias e sopro contínuo de vento durante o voo na folha.

---

## 3. Cena de Abertura: O Despertar Solitário no Sonho (7 Beats)

A fase começa na clareira inicial do sonho (`X = 0` a `400`). Yuna está de pé no chão escuro e enevoado; logo atrás dela, Nanda está deitada dormindo, com o mesmo tamanho de Yuna. O controle do jogador permanece bloqueado durante os **7 beats cinematográficos** (duração total de **38 segundos**, com avanço manual permitido após o tempo mínimo de leitura de cada beat):

### Beat 1 (`shot: 'lost'`, Duração: `3,0s`, Mínimo: `3,0s`, Sem diálogo)
- **Ação Visual**: Plano aberto da clareira fria sob a lua gigante. Yuna olha para os lados, encolhendo levemente os ombros por causa do frio. Nenhuma caixa de diálogo é exibida.

### Beat 2 (`shot: 'discover'`, Duração: `6,0s`, Mínimo: `2,0s`)
- **Ação Visual**: Yuna olha para trás e vê Nanda deitada no chão, do seu mesmo tamanho, dormindo profundamente. Yuna se aproxima dela.
- **Diálogo (`Narrador`)**:
  > "Ao olhar para trás, Yuna encontrou Nanda.
  > Estranhamente, a fadinha tinha o mesmo tamanho que ela."

### Beat 3 (`shot: 'shake'`, Duração: `7,0s`, Mínimo: `3,0s`)
- **Ação Visual**: Yuna se ajoelha ao lado de Nanda e coloca as mãos em seu ombro, sacudindo-a delicadamente para tentar acordá-la.
- **Diálogo (`Yuna`)**:
  > "Nanda, onde estamos? Está frio aqui."

### Beat 4 (`shot: 'wait'`, Duração: `3,0s`, Mínimo: `3,0s`, Sem diálogo)
- **Ação Visual**: Yuna para de sacudir e observa em silêncio, esperando uma resposta. Nanda continua imóvel, respirando devagar em seu sono profundo, com as asas dobradas.

### Beat 5 (`shot: 'plead'`, Duração: `7,0s`, Mínimo: `3,0s`)
- **Ação Visual**: Yuna insiste mais uma vez, com expressão mais assustada, olhando para a imensidão escura da floresta ao redor.
- **Diálogo (`Yuna`)**:
  > "Nanda, estou com medo! Me fale como sair daqui."

### Beat 6 (`shot: 'alone'`, Duração: `8,0s`, Mínimo: `3,0s`)
- **Ação Visual**: Yuna se levanta devagar ao lado de Nanda adormecida, compreendendo que a irmã não vai despertar ali.
- **Diálogo (`Narrador`)**:
  > "Sem a orientação da irmã fadinha, Yuna se sentiu perdida.
  > Sem escolha, decidiu seguir em frente e deixar Nanda dormindo."

### Beat 7 (`shot: 'leave'`, Duração: `4,0s`, Mínimo: `4,0s`, Sem diálogo)
- **Ação Visual**: Yuna vira-se para a direita, dá os primeiros passos em direção às primeiras hastes gigantes de grama na borda da clareira, e a trilha musical **"Misty Blues"** começa a tocar suavemente. Ao término deste beat, o controle é liberado para o jogador.

---

## 4. Regras de Mecânica da Travessia Aérea (Seções 1 a 6)

### 4.1. Movimentação no Chão e nas Folhas de Descanso
- Enquanto está sobre o solo inicial ou sobre uma das grandes folhas horizontais de descanso (`rest-0` a `rest-5`), Yuna caminha (`Esquerda/Direita`, velocidade base `230`) e salta (`Espaço / Pular`, impulso vertical `680`, gravidade `1450`).

### 4.2. Agarrar e Escalar Gramas, Abelhas e Teias (`Climbers`)
- **Captura Automática ao Contato**: Sempre que Yuna toca a área de captura de um talo de grama, da lateral peluda de uma abelha ou de um fio vertical de teia de aranha (desde que não esteja no breve intervalo de recuo pós-salto de `0,22s`), ela **se agarra automaticamente** ao suporte.
- **Movimento Vertical de Escalada**:
  - Pressionar `W / Seta para Cima` faz Yuna escalar para cima (velocidade `220 u/s`).
  - Pressionar `S / Seta para Baixo` faz Yuna descer pelo suporte (velocidade `220 u/s`).
  - Cada suporte possui limites superior (`top`) e inferior (`bottom`).
- **Acompanhamento Físico do Balanço (Sway)**:
  - Enquanto está agarrada a um talo de grama ou fio de teia, a posição horizontal ($X$) e a inclinação visual do corpo de Yuna acompanham em tempo real a curvatura senoidal daquele ponto exato da haste (quanto mais perto do topo do talo de grama, maior é o deslocamento lateral do balanço).
- **Salto a partir da Escalada**:
  - Pressionar `Espaço / Pular` enquanto está agarrada lança Yuna no ar para o próximo apoio.
  - Se o jogador estiver segurando `Esquerda` ou `Direita`, Yuna salta naquela direção com velocidade lateral de impulso (`350 u/s`, somada a `35%` da velocidade horizontal atual do balanço do talo!) e impulso vertical (`650 u/s`). Se não estiver segurando direção alguma, ela salta para a direita por padrão.

### 4.3. Cartões de Tutorial Contextuais (`DREAM_INTRO`)
À medida que Yuna avança pelas primeiras seções, o jogo exibe discretamente cartões poéticos/instrutivos no topo da tela quando ela se aproxima de cada nova mecânica:
1. **Logo ao deixar a clareira inicial**:
   > "O jardim cresceu por dentro do sono.
   > Uma folha podia esconder a lua."
2. **Ao se aproximar do primeiro talo de grama**:
   > "Ao tocar as gramas, Yuna se agarra.
   > W/S ou ▲/▼ escala; Espaço ou PULAR salta."
3. **Ao chegar à seção dos besouros voadores (Seção 3)**:
   > "Suba no lombo e espere o voo.
   > Salte quando o próximo apoio estiver perto."

### 4.4. Abismo Inferior, Checkpoints e Diferenças de Dificuldade (`NORMAL` vs `FÁCIL`)
- Abaixo das hastes de grama e plataformas aéreas existe o abismo enevoado (`fallLimit = 1090`).
- **Folhas de Descanso (`rest-0` a `rest-5`)**: Ao final de cada seção há uma grande folha horizontal segura que registra automaticamente o **Checkpoint** daquela seção.
- **Penalidade de Queda ou Toque em Aranha**:
  - **Modo NORMAL**: Cair no abismo ou ser atingida por uma aranhazinha nas teias exibe a tela de **GAME OVER** (`"Tentar de novo"` / `"Menu Principal"`), reposicionando Yuna no último checkpoint alcançado ao tentar novamente.
  - **Modo FÁCIL (`EASY`)**:
    - O alcance horizontal de captura das gramas, teias e abelhas é **25% mais largo** (`easyMargin = 14px` extras para cada lado).
    - As plataformas de folhas, besouros e costas de abelhas têm superfície de pouso mais generosa (`+36` de largura).
    - Cair no abismo ou tocar em uma aranha **não dá Game Over**: uma corrente de vento suave resgata Yuna imediatamente e a deposita de volta na última folha de descanso (checkpoint), exibindo a mensagem reconfortante: `"O vento do sonho amparou Yuna de volta à folha segura."`

---

## 5. Layout Detalhado das 6 Seções Aéreas (`DREAM_SECTIONS`)

O percurso aéreo se estende de `X = 0` até `X = 28228`, dividido em **6 seções nomeadas**:

### Seção 1: *"Onde a grama toca a lua"* (`X = 430` a `6910`, `kind: 'grass'`)
- **Composição**: **24 hastes de grama gigantes** (`grass-0-0` a `grass-0-23`), espaçadas regularmente a cada `270` unidades horizontais.
- **Alturas e Balanço**:
  - As pontas superiores (`top`) variam ritmicamente entre `Y = 180` e `Y = 270`, enquanto as bases (`bottom`) descem até `Y = 980`.
  - Cada haste oscila suavemente de um lado para o outro com **amplitude de `44` unidades** e **período de `8,5 segundos`** (defasadas progressivamente em fase para criar o efeito visual de uma onda de vento passando pelo capinzal).
- **Checkpoint 0 (`rest-0`)**: Ao saltar da 24ª haste, Yuna pousa na primeira grande folha de descanso (`X = 6820`, largura `380`, `Y = 580`).

### Seção 2: *"O sopro entre as folhas"* (`X = 7250` a `13754`, `kind: 'wind'`)
- **Composição**: **25 hastes de grama gigantes** (`grass-1-0` a `grass-1-24`), espaçadas a cada `266` unidades.
- **Vento Forte**:
  - Nesta região o vento noturno sopra com muito mais força: a amplitude do balanço de cada haste sobe para **`75` unidades** (com período mais rápido de **`6,2 segundos`**).
  - Como o salto de Yuna herda parte do impulso lateral da haste, o jogador aprende a saltar no momento em que o talo está se inclinando para a direita para ganhar alcance extra, além de subir e descer pelos talos para alinhar a altura com a haste seguinte (`top` variando entre `Y = 150` e `Y = 270`).
- **Checkpoint 1 (`rest-1`)**: Grande folha de descanso em `X = 13664` (largura `380`, `Y = 560`).

### Seção 3: *"Carapaças à deriva"* (`X = 14094` a `23114`, `kind: 'beetle'`)
- **Composição**: Uma travessia aérea de **`9020` unidades** sobre o abismo aberto, realizada no lombo de **4 grandes Besouros Voadores de Transporte** (`beetle-0` a `beetle-3`).
- **Funcionamento de cada Besouro**:
  - Cada besouro começa pousado/pairando em repouso em sua posição inicial (`startX, startY`). Seu casco superior forma uma plataforma de `196 x 84` unidades.
  - Assim que Yuna pisa sobre o casco de um besouro, ele aguarda **`0,65 segundo`** (`takeoffDelay`, preparando as asas) e inicia uma jornada de voo curvo para a direita até seu ponto de destino (`endX, endY`), levando Yuna junto sobre o seu dorso!
  - Durante o voo, o besouro descreve um arco vertical principal (`arc`) combinado com uma ondulação senoidal suave (`wave`):
    1. **Besouro 1 (`beetle-0`)**: Parte de `(14094, 590)` até `(16214, 450)` ao longo de **`12 segundos`** (arco de `-170`, ondulação `+65`).
    2. **Besouro 2 (`beetle-1`)**: Parte de `(16324, 470)` até `(18564, 580)` ao longo de **`14 segundos`** (arco de `+140`, ondulação `-70`).
    3. **Besouro 3 (`beetle-2`)**: Parte de `(18674, 590)` até `(20894, 430)` ao longo de **`13 segundos`** (arco de `-190`, ondulação `+75`).
    4. **Besouro 4 (`beetle-3`)**: Parte de `(21004, 450)` até `(23034, 520)` ao longo de **`14 segundos`** (arco de `+100`, ondulação `-65`).
  - Quando um besouro chega ao fim do seu trajeto, Yuna salta do casco dele diretamente para o dorso do próximo besouro que está esperando logo à frente (se Yuna saltar ou cair de um besouro, ele retorna suavemente à sua posição inicial para permitir nova tentativa).
- **Checkpoint 2 (`rest-2`)**: Ao saltar do 4º besouro, Yuna pousa na folha de descanso em `X = 23064` (largura `380`, `Y = 530`).

### Seção 4: *"Asas sobre o abismo"* (`X = 23494` a `26050`, `kind: 'bee'`)
- **Composição**: Uma sequência mista de **22 apoios aéreos** espaçados a cada `250` unidades, intercalando **hastes de grama** e **Abelhas Amigas com Cotonete** (a cada 3 posições surge uma abelha: índices `0, 3, 6, 9, 12, 15, 18, 21`).
- **Comportamento das Abelhas Amigas**:
  - Cada abelha (`184 x 96`) paira no ar batendo as asas e oscila verticalmente para cima e para baixo com **amplitude de `100` unidades** e **período de `6,5 segundos`**.
  - Como têm um cotonete macio no lugar do ferrão, são 100% amigas:
    - Se Yuna cair por cima delas, pousa em segurança sobre o **dorso felpudo** (que funciona como plataforma móvel vertical).
    - Se Yuna tocar na **lateral peluda** da abelha durante um salto mais baixo, ela **se agarra ao pelo da abelha** como um suporte escalável e pode subir (`W / ▲`) até ficar de pé sobre as costas da abelha!
- **Checkpoint 3 (`rest-3`)**: Folha de descanso em `X = 25960` (largura `380`, `Y = 510`).

### Seção 5: *"Fios entre as nuvens"* (`X = 26390` a `28228`, `kind: 'web'`)
- **Composição**: **6 fios verticais de teia de aranha** (`web-0` a `web-5`), espaçados a cada **`310` unidades**, descendo das nuvens (`top = -420`) até o fundo (`bottom = 980`), com balanço mínimo (`amplitude: 10`, período `9,0s`).
- **Ciclo de Patrulha das Aranhazinhas**:
  - Cada um dos 6 fios é patrulhado por uma **Aranhazinha** (`140 x 84`) que repete um ciclo exato de **`9,0 segundos`** (defasado em `-1,8s` de um fio para o outro):
    1. **Descida (`DESCENDING`, `0,0s` a `1,8s`)**: A aranha desce rapidamente pelo fio desde o topo oculto (`Y = -180`) até a parte baixa (`Y = 700`).
    2. **Espera embaixo (`LOW`, `1,8s` a `5,2s`)**: A aranha fica parada lá embaixo em `Y = 700` por `3,4 segundos` — **esta é a janela ideal para Yuna escalar pela metade superior do fio (`Y = 200` a `450`) e saltar para o próximo fio!**
    3. **Subida (`ASCENDING`, `5,2s` a `7,0s`)**: A aranha sobe de volta de `Y = 700` até `Y = -180`.
    4. **Espera no alto (`HIGH`, `7,0s` a `9,0s`)**: A aranha descansa oculta no topo por `2,0 segundos` — permitindo que Yuna passe pela parte média/baixa do fio se preferir.
- **Checkpoint 4 (`rest-4`)**: Após o 6º fio de teia, Yuna pousa na folha de descanso em `X = 27850` (largura `380`, `Y = 510`).

### Seção 6: *"Os galhos que o vento embala"* (`X = 28228`, `kind: 'branch'`)
- Logo à frente da folha `rest-4` estende-se o **Grande Galho de Decolagem (`rest-5`)** em `X = 28228` (largura `460`, `Y = 510`).
- Ao pisar neste galho, o Checkpoint 5 é registrado e inicia-se automaticamente a sequência de decolagem do **Voo na Folha (*Leaf Glider*)**.

---

## 6. O Voo Planado na Folha (*Leaf Glider*, `X = 28228` a `39428`)

### 6.1. Decolagem Automática no Galho (`GLIDE_APPROACH` e `GLIDE_GUST`)
1. **Aproximação (`GLIDE_APPROACH`)**: Assim que Yuna pisa no galho `rest-5`, ela caminha automaticamente até a ponta direita do galho (`X = 28478`) e apanha uma grande folha verde arqueada.
2. **Sopro de Decolagem (`GLIDE_GUST`, `1,1s`)**: Uma lufada ascendente de vento noturno envolve o galho. Yuna ergue a folha acima da cabeça com as duas mãos e o vento a levanta suavemente no ar (`Y` sobe de `510` para `430` enquanto avança `150` unidades para a direita).
3. **Mensagem de Instrução**: Na tela surge brevemente a dica:
   > "O vento levou Yuna na folha! Use W/S ou ▲/▼ para desviar dos espinhos."

### 6.2. Física e Controle do Voo Planado (`GLIDING`)
- **Percurso Total**: Da posição `X = 28228` até `X = 39428` (**`11.200` unidades de extensão**, cerca de **51 segundos de voo contínuo**).
- **Velocidade Horizontal Constante**: O vento leva Yuna para a direita a uma velocidade fixa de **`220 unidades por segundo`**.
- **Controle Vertical com Inércia Suave**:
  - O jogador controla apenas a altitude (`W / Seta para Cima` para ganhar altura, `S / Seta para Baixo` para mergulhar).
  - Velocidade vertical máxima: **`400 u/s`** (`maxVertical`).
  - Aceleração vertical suave (inércia de ar): **`400 u/s²`**, fazendo com que a folha responda como um planador real no vento, sem mudanças bruscas de direção.
  - **Queda Natural (Planeio Neutro)**: Quando o jogador não pressiona cima nem baixo, a folha desce suavemente a **`35 u/s`** (`sink`).
  - **Inclinação Visual (*Pitch*)**: O corpo de Yuna e a folha inclinam-se proporcionalmente à velocidade vertical (entre `-0,18 rad` ao subir e `+0,18 rad` ao mergulhar).
- **Acompanhamento de Câmera no Voo**:
  - Como o corredor de vento sobe às alturas das copas e depois mergulha fundo entre os galhos (variando a altitude central da rota entre `Y = -700` e `Y = +790`), durante o modo `GLIDING` a câmera acompanha suavemente tanto o avanço lateral quanto a altitude de Yuna, mantendo-a enquadrada com ampla visão antecipada dos galhos espinhosos à frente.

### 6.3. Os 21 Pares de Galhos Espinhosos (`DREAM_GLIDE_THORNS`)
Ao longo do voo, Yuna precisa atravessar a abertura (*gap*) entre **21 pares de galhos retorcidos cobertos de espinhos** (um galho vindo de cima e outro subindo de baixo), espaçados a cada **`480` unidades horizontais** (começando em `X = 29228` até `X = 38828`):
- **Abertura Vertical (*Gap*)**:
  - Nos primeiros 8 pares (`thorn-0` a `thorn-7`), a passagem livre entre os espinhos superiores e inferiores tem **`300` unidades** de altura (no modo `FÁCIL`, todas as passagens recebem `+70` unidades extras de abertura!).
  - Do 9º ao 21º par (`thorn-8` a `thorn-20`), onde a rota faz subidas e mergulhos mais acentuados, a passagem livre tem **`340` unidades** de altura.
- **Traçado de Montanha-Russa Aérea (Altitudes Centrais da Passagem)**:
  1. **Início Calmo (`thorn-0` a `thorn-2`)**: Passagem centralizada em torno de `Y = 420 → 350`.
  2. **Primeira Grande Subida (`thorn-3` a `thorn-6`)**: A corrente de ar exige que Yuna suba continuamente de `Y = 180` até as copas altas em **`Y = -650`**.
  3. **Primeiro Grande Mergulho (`thorn-7` a `thorn-11`)**: O corredor despenca das alturas (`Y = -420`) até o fundo do vale entre raízes aéreas em **`Y = +790`**.
  4. **Segunda Grande Subida (`thorn-12` a `thorn-16`)**: Yuna torna a ganhar altitude rapidamente de `Y = +520` até o ponto mais alto do voo em **`Y = -700`**.
  5. **Mergulho Final (`thorn-17` a `thorn-20`)**: O corredor desce de `Y = -350` até **`Y = +670`** e estabiliza em `Y = 470` na saída dos espinhos.
- **Colisão com Espinhos (`GLIDE_FALLING`)**:
  - Se Yuna encostar nos espinhos superiores ou inferiores (ou sair muito para fora do corredor de vento), a folha escapa e ela entra na animação de queda (`GLIDE_FALLING`, `1,35s`), exibindo a mensagem `"Os espinhos rasgaram o voo da folha..."` e **reiniciando imediatamente no galho `rest-5` (`X = 28228`)** para tentar o voo novamente sem precisar refazer as 5 seções anteriores.

### 6.4. Fim do Vento e Queda para o Pesadelo (`GLIDE_LANDING` $\rightarrow$ `NIGHTMARE_FALL`)
- Ao ultrapassar o 21º par de espinhos e atingir `X = 39428`, a corrente de vento cessa.
- Durante **`3,6 segundos`** (`GLIDE_LANDING`), a música *"Misty Blues"* vai silenciando e Yuna perde sustentação, descendo com a folha até pousar na borda final (`dream-finish`, `X = 39558`).
- No instante em que pisa nessa borda escura, o chão de folhas cede ao vazio do sono profundo (`nightmare-start`), iniciando imediatamente a queda vertical para a fase seguinte (**Documento 07 — O Pesadelo Furtivo e o Despertar**).

---

## 7. Critérios de Aceite para Construção na Unity (Checklist White-Room)

1. **Abertura de 7 Beats com Nanda em Tamanho Humano**: Nanda deve estar deitada dormindo na clareira inicial com a mesma altura de Yuna, em silêncio absoluto (sem música) até o Beat 7, exibindo os diálogos exatos em português.
2. **Escalada Fluida em 3D com Balanço Senoidal**: As 49 hastes de grama das Seções 1 e 2 devem curvar-se fisicamente no vento; Yuna deve agarrar-se a elas ao contato, subir/descer e herdar o impulso do balanço ao saltar.
3. **4 Besouros Voadores e Abelhas Amigas com Cotonete**: Os 4 besouros da Seção 3 devem aguardar `0,65s` após o pouso de Yuna e voar em curva até o próximo ponto; as abelhas da Seção 4 devem exibir claramente um cotonete no lugar do ferrão e permitir tanto pisar no dorso quanto agarrar nas laterais.
4. **6 Fios de Teia com Ciclo de Patrulha de 9s**: As aranhazinhas devem descer, aguardar embaixo (`3,4s`), subir e aguardar no topo (`2,0s`), exigindo leitura de tempo para atravessar os 6 fios.
5. **Voo Planado na Folha (*Leaf Glider*)**: Decolagem automática no galho `rest-5`, 51 segundos de voo com inércia vertical suave através dos 21 pares de galhos espinhosos em trajeto de montanha-russa aérea, culminando na perda de vento e transição direta para o Pesadelo.
