# Documento 08 — Capítulo 2, Parte 4: O Brinquedão (Grande Final da Campanha)

> **Propósito deste documento (White-Room Prompt para Unity 3D)**:
> Este documento especifica de forma completa e autossuficiente a **Parte 4 do Capítulo 2 (Fase Final do Jogo): O Brinquedão** de *O Quarto dos Brinquedos*. Deve ser usado como especificação direta para construir o convite de Nanda após o pesadelo, o gigantesco circuito externo de parque de diversões/playground ao ar livre (passarelas acolchoadas, escorregadores, pontes de piscina de bolinhas com peixes saltadores, camas elásticas, castelos, pontes que cedem com rotas inferiores de recuperação, parede elástica com rampa reversa de lançamento ao céu e calhas elásticas em "U") e o circuito interno de **4 Tobogãs Tubulares de Alta Velocidade** (com corrida do triângulo, torre de rebotes estilo pinball e ponte de 15 blocos que colapsam em onda), encerrando com a cena final de vitória e descanso no gramado em **3D com jogabilidade lateral (2.5D)** na Unity, **sem depender de código ou assets legados**.
> **Restrição absoluta**: Zero linhas de código neste documento. Todas as regras, medidas proporcionais, tempos, diálogos e mecânicas estão descritos em linguagem natural estruturada.

---

## 1. Visão Geral e Papel Dramático da Fase

Após despertar assustada do pesadelo com a Barata Monumental, Yuna ainda sente um resquício de medo. Para devolver a alegria e a coragem à irmãzinha, Nanda faz um convite irresistível: ir brincar no **Brinquedão** — uma estrutura colossal, colorida e ensolarada de playground infantil (*kid play / circuito acrobático de parque*), cheia de pontes suspensas, piscinas de bolinhas, escorregadores gigantes, camas elásticas, castelos infláveis/acolchoados e tobogãs tubulares translúcidos.

Esta última fase é a **celebração catártica da jornada de Yuna**. O tom sombrio do pesadelo fica totalmente para trás, dando lugar a um **Platformer 2.5D Acrobático de Alta Velocidade e Fluxo Contínuo** (inspirado nas fases musicais/acrobáticas de *Rayman Legends* e nos circuitos de velocidade de *Super Mario 3D World* / *Sonic*), dividido em duas grandes etapas:
1. **Etapa 1 — O Grande Circuito Externo ao Sol (`X = 0` a `35600`, 10 Seções)**: Uma corrida vibrante ao ar livre onde quase não há morte por queda nos trechos acrobáticos — se Yuna cair de uma ponte que cede entre os castelos, ela aterrissa em **rotas inferiores de recuperação** ou em **calhas elásticas em formato de "U"** que a lançam de volta às alturas! Nanda voa ao lado de Yuna durante todo o percurso, vibrando e incentivando.
2. **Etapa 2 — Os 4 Tobogãs Tubulares Internos (`Interior Tubes`)**: No ponto mais alto do Brinquedão, Yuna mergulha na boca de quatro imensos tubos espirais translúcidos de alta velocidade, intercalados por três desafios curtos de reflexo puro: a **Arrancada do Triângulo**, a **Torre dos Rebotes (Pinball Vertical)** e a **Corrida dos 15 Blocos que Cedem**, até ser ejetada pelo **Tobogã da Chegada** diretamente no gramado festivo final.

---

## 2. Direção Visual 3D, Estética do Brinquedão e Áudio

### 2.1. Cenário 3D (2.5D Playground Monumental ao Ar Livre)
- **Paleta Alegre e Ensolarada**: Vermelho-cereja, amarelo-sol, azul-céu vibrante, verde-limão, laranja e roxo-brinquedo, com iluminação solar quente de fim de tarde, céu azul límpido com nuvens brancas fofas e bandeirolas coloridas de festa (*bunting flags*) tremulando ao vento.
- **Materiais 3D do Brinquedão**:
  - **Passarelas e Torres (`passeio`, `castelo`)**: Estruturas tubulares revestidas de espuma colorida macia (vinil acolchoado brilhante com costuras visíveis), telas de proteção translúcidas ao fundo e torres com telhadinhos cônicos de castelo de brinquedo.
  - **Escorregadores Abertos (`slide`)**: Calhas plásticas largas e polidas (amarelas, laranjas e azuis) com brilho especular intenso onde Yuna desliza sentada em alta velocidade.
  - **Pontes de Piscina de Bolinhas e Peixes de Brinquedo (`fish-bridge`)**: Passarelas suspensas sobre tanques cheios de milhares de bolinhas plásticas coloridas em 3D. De dentro das bolinhas saltam **Peixes de Brinquedo Infláveis/Plásticos** coloridos em arcos parabólicos.
  - **Camas Elásticas (`trampoline`) e Lonas de Retorno**: Superfícies circulares/retangulares de lona preta e borda acolchoada vermelha/azul com molas metálicas que se comprimem e distendem com força quando Yuna pisa nelas.
  - **Pontes Quebradiças (`crumble-bridge`)**: Sequências de 4 blocos acolchoados suspensos por tiras elásticas; quando Yuna pisa num bloco, ele treme e desce/cede brevemente, deixando-a passar para a rota de baixo se demorar.
  - **Parede Elástica Inclinada e Calhas em "U" (`elastic-wall`, `halfpipe`)**: Grandes colchões verticais/inclinados de alta impulsão com setas coloridas pintadas no vinil.
  - **Tobogãs Tubulares Internos (`tubes`)**: Tubos cilíndricos gigantes de acrílico colorido semitransparente (permitindo ver Yuna deslizando em altíssima velocidade por dentro do tubo com anéis luminosos externos passando rapidamente).

### 2.2. Direção de Áudio
- **Abertura no Piquenique**: Som ambiente calmo de pássaros e brisa de jardim.
- **Trilha Musical do Brinquedão**: Assim que a fase começa, entra uma trilha instrumental **alegre, acelerada, saltitante e festiva** (ritmo de aventura infantil empolgante que acompanha o embalo dos escorregadores e trampolins).
- **Efeitos Sonoros**: Som de vinil/espuma nos passos, silvo rápido de escorregador, *"boing!"* elástico grave e agudo nas camas elásticas e paredes de rebote, espirro de bolinhas plásticas quando os peixes saltam, som de tubo oco veloz nos tobogãs e fanfarra suave de chegada no gramado final.

---

## 3. Cena de Abertura: O Convite para o Brinquedão (7 Beats)

A cena começa exatamente onde o Documento 07 terminou: Yuna sentada sobre a toalha de piquenique no jardim ensolarado, ainda se recuperando do susto do pesadelo, com Nanda pairando ao seu lado. Ocorrem **7 beats narrativos** (duração total de **35 segundos**, com avanço manual permitido após o tempo mínimo):

### Beat 1 (`pose: 0`, Duração: `5,0s`)
- **Ação Visual**: Yuna sentada na toalha, cabisbaixa, abraçando os joelhos enquanto Nanda a observa com ternura.
- **Diálogo (`Narrador`)**:
  > "O grito tinha passado.
  > O medo, ainda não."

### Beat 2 (`pose: 0`, Duração: `5,0s`)
- **Ação Visual**: Yuna olha para Nanda com voz baixinha.
- **Diálogo (`Yuna`)**:
  > "Eu não gostei daquele sonho…"

### Beat 3 (`pose: 0`, Duração: `5,0s`)
- **Ação Visual**: Nanda voa bem pertinho do rosto de Yuna, fazendo um carinho luminoso.
- **Diálogo (`Nanda`)**:
  > "Já passou, Yuna. Estou aqui com você."

### Beat 4 (`pose: 1`, Duração: `7,0s`)
- **Ação Visual**: Nanda dá uma pirueta animada no ar e aponta para o horizonte ensolarado do parque, onde brilha a estrutura colorida do Brinquedão. Yuna ergue a cabeça, curiosa.
- **Diálogo (`Nanda`)**:
  > "Vamos ao Brinquedão? A gente pode correr pelas pontes e voar nos tobogãs!"

### Beat 5 (`pose: 1`, Duração: `4,0s`)
- **Ação Visual**: Yuna fica de pé sobre a toalha, já mais animada, mas querendo ter certeza de que não ficará sozinha como no sonho.
- **Diálogo (`Yuna`)**:
  > "Você vai comigo?"

### Beat 6 (`pose: 2`, Duração: `4,0s`)
- **Ação Visual**: Nanda abre um grande sorriso brilhante no ar.
- **Diálogo (`Nanda`)**:
  > "Até o último escorregador!"

### Beat 7 (`pose: 2`, Duração: `5,0s`)
- **Ação Visual**: Yuna sorri de volta e se prepara para correr em direção à entrada do Brinquedão.
- **Diálogo (`Narrador`)**:
  > "Naquele convite cabia
  > um pouquinho de coragem."

---

## 4. Etapa 1: O Grande Circuito Externo (`X = 0` a `35600`, 10 Seções)

### 4.1. Física Acrobática e Controles do Circuito Externo
- No Brinquedão, Yuna corre com muito mais energia e velocidade que nas fases anteriores:
  - **Velocidade máxima de corrida (`maxSpeed`)**: **`700 u/s`** (aceleração `1350 u/s²`).
  - **Impulso de salto (`jump`)**: **`730 u/s`** (gravidade `1500 u/s²`).
  - **Velocidade nos Escorregadores (`slideSpeed`)**: Ao pisar em uma rampa de escorregador, Yuna senta e desliza automaticamente acelerando até **`980 u/s`**! Saltar durante o escorregador conserva o embalo horizontal.
  - **Velocidade de Lançamento Elástico (`elasticSpeed`)**: Até **`2400 u/s`** quando impulsionada pela parede elástica e pela rampa reversa.
- **Acompanhamento de Nanda**: Durante todo o circuito externo, Nanda voa alegremente perto do ombro de Yuna, deixando um rastro de partículas douradas.

### 4.2. Layout Detalhado das 10 Seções Externas (`BRINQUEDAO_SECTIONS`)

#### Seção 1: *"Passarelas ao sol"* (`X = 0` a `3900`, Checkpoint 0 em `X = 140`)
1. **Passarela Inicial (`passeio`)**: `X = 0` a `900` (`Y = 740`).
2. **Primeira Subida Acolchoada (`subida`)**: Rampa de `X = 900` a `1500` (subindo de `Y = 740` para `540`) e patamar `topo-1` (`1500–1750`, `Y = 540`).
3. **Primeiro Escorregador (`escorrega-sol`)**: De `X = 1750` a `2550`, descendo de `Y = 540` para `760`, onde Yuna ganha velocidade máxima de escorregamento (`980 u/s`).
4. **Primeira Ponte de Piscina de Bolinhas (`bolinhas-sol`)**: De `X = 2550` a `3350` (`Y = 760`), seguida pelo patamar `saida-bolinhas` (`3350–3900`).
   - **Mecânica dos Peixes Saltadores (`BRINQUEDAO_FISH`)**: Sob as 3 pontes de bolinhas (`bolinhas-sol`, `bolinhas-vento` e `bolinhas-grande`), há **2 peixes de brinquedo** por ponte (6 ao todo). Quando Yuna se aproxima a menos de `150` unidades da posição de um peixe, ele salta verticalmente das bolinhas (`air: 0,64s`, subindo `155` unidades acima da ponte). Se tocar em Yuna, **não mata**: apenas dá um empurrãozinho cômico para trás (`knockback`), incentivando-a a saltar por cima do peixe sem perder o embalo!

#### Seção 2: *"Pontes e gargalhadas"* (`X = 3900` a `7800`, Checkpoint 1 em `X = 3520`)
1. **Degrau de Impulso (`degrau-1`)**: `X = 3700` a `4100` (`Y = 690`).
2. **Ponte do Jardim (`ponte-jardim`) e Atalho Superior (`atalho-sol`)**:
   - Rota principal pela ponte suspensa `ponte-jardim` (`X = 4100` a `4950`, subindo de `Y = 620` a `580`).
   - **Rota de Atalho Superior**: Saltando alto a partir do degrau, Yuna alcança a plataforma elevada **`atalho-sol`** (`X = 4250` a `4850`, `Y = 440`).
3. **Torre e Escorregador do Vento**: Subida `torre-vento` (`4950–5650`, subindo de `Y = 580` a `450`), topo (`5650–5850`, `Y = 450`) e o longo **`escorrega-vento`** (`5850–6650`, descendo de `Y = 450` a `760`).
4. **Segunda Ponte de Bolinhas (`bolinhas-vento`)**: `X = 6650` a `7500` (`Y = 760`, com mais 2 peixes saltadores) e patamar `pausa-vento` (`7500–7800`).

#### Seção 3: *"Lá do alto"* (`X = 7800` a `11900`, Checkpoint 2 em `X = 7620`)
1. **Degrau (`degrau-2`)**: `X = 7900` a `8300` (`Y = 680`).
2. **Ponte do Arco (`ponte-arco`) e Atalho do Arco (`atalho-arco`)**:
   - Rota principal `ponte-arco` (`8300–9150`, subindo de `Y = 600` a `540`).
   - **Atalho Superior**: Plataforma elevada **`atalho-arco`** (`8450–9050`, `Y = 360`).
3. **Subida Alta e Escorregador Grande**: Rampa `subida-alta` (`9150–9900`, subindo até `Y = 380`), `topo-alto` (`9900–10150`, `Y = 380`) e o veloz **`escorrega-grande`** (`10150–11100`, despencando de `Y = 380` para `760`).
4. **Terceira Ponte de Bolinhas (`bolinhas-grande`)**: `X = 11100` a `11900` (`Y = 760`, com mais 2 peixes saltadores).

#### Seção 4: *"Mais um escorregador"* (`X = 11900` a `14800`, Checkpoint 3 em `X = 12050`)
- Passarela `reta-final` (`11900–12300`, `Y = 760`), `degrau-3` (`12300–12650`, `Y = 690`), `degrau-4` (`12650–13000`, `Y = 620`), `ponte-final` (`13000–13700`, `Y = 550`) e o grande terraço panorâmico `chegada` (`13700–14800`, `Y = 550`, onde fica o Checkpoint 4 em `X = 14050`).

#### Seção 5: *"O grande mergulho"* (`X = 14800` a `17000`)
- Na borda do terraço começa o colossal **Escorregador Gigante (`escorrega-gigante`)**: uma descida ininterrupta de **`2200` unidades de comprimento** (`X = 14800` a `17000`), mergulhando de `Y = 550` até a base profunda em **`Y = 1350`**!

#### Seção 6: *"Um mar de camas elásticas"* (`X = 17000` a `23350`, Checkpoints 5 e 6)
Ao final do Escorregador Gigante, Yuna entra em uma sequência acrobática tripla composta por **3 Conjuntos de [Cama Elástica $\rightarrow$ Torre de Castelo $\rightarrow$ Ponte Quebradiça de 4 Blocos]**:
- **Conjunto 1 (`X = 17000` a `19200`)**:
  - **Cama Elástica 1 (`cama-1`)**: `X = 17000` a `17320` (`Y = 1350`). Impulsiona Yuna automaticamente para o alto com força vertical **`1350 u/s`**!
  - **Torre do Castelo 1 (`castelo-1`)**: `X = 17420` a `17820` (`Y = 900`).
  - **Ponte Quebradiça 1 (`ponte-cai-1-0` a `1-3`)**: 4 segmentos de `195` unidades cada (`X = 17820` a `18600`, `Y = 900`). Cada bloco cede **`0,28s`** após o toque dos pés de Yuna (`0,45s` no modo `FÁCIL`).
- **Conjunto 2 (`X = 19150` a `21350`, Checkpoint 5 em `X = 19220`)**:
  - **Cama Elástica 2 (`cama-2`)**: `19150–19470` (`Y = 1300`, impulso **`1380 u/s`**) $\rightarrow$ **Castelo 2 (`castelo-2`)**: `19570–19970` (`Y = 820`) $\rightarrow$ **Ponte Quebradiça 2 (`ponte-cai-2-0..3`)**: `19970–20750` (`Y = 820`).
- **Conjunto 3 (`X = 21300` a `23350`, Checkpoint 6 em `X = 21360`)**:
  - **Cama Elástica 3 (`cama-3`)**: `21300–21620` (`Y = 1250`, impulso **`1400 u/s`**) $\rightarrow$ **Castelo 3 (`castelo-3`)**: `21720–22120` (`Y = 740`) $\rightarrow$ **Ponte Quebradiça 3 (`ponte-cai-3-0..3`)**: `22120–22900` (`Y = 740`).
- **Rotas Inferiores de Recuperação sem Morte (`brinquedaoRecovery.js`)**:
  - Se Yuna cair de qualquer uma das 3 pontes quebradiças ou errar o pulo de um castelo, **ela não morre**: lá embaixo (`Y = 1770` a `1900`) existem **3 passarelas acolchoadas inferiores completas** (`apoio-inferior-1..3`, `retorno-inferior-1..3` e rampas `ligacao-inferior-1..3`) que amortecem a queda e permitem que ela corra por baixo e suba de volta diretamente para a próxima Cama Elástica!

#### Seção 7: *"Paredes que devolvem o impulso"* (`X = 23350` a `25100`, Checkpoint 7 em `X = 23500`)
- Do 3º castelo, Yuna chega ao **`castelo-queda`** (`23350–23750`, `Y = 980`) e desliza pelo vertiginoso **`escorrega-queda`** (`23750–25100`), descendo de `Y = 980` até **`Y = 1700`**.
- No pé desse escorregador (`X = 25100, Y = 1700`), está posicionada a grande **Parede Elástica Inclinada (`BRINQUEDAO_ELASTIC`)**.

#### Seção 8: *"De volta ao céu!"* (`X = 25100` a `27700`)
- Ao bater na **Parede Elástica Inclinada** em `X = 25100`, o impacto rebate Yuna com impulso colossal (`2350 u/s`), disparando-a de costas/frente diretamente para cima ao longo da gigantesca **Rampa Reversa de Decolagem (`rampa-reversa`, `X = 25100` a `26700`, subindo de `Y = 1700` até `Y = -150`)**!
- Ao atingir a ponta da Rampa Reversa em `X = 26700` (`Y = -150`), Yuna é **lançada voando para o alto do céu** (`launchBrinquedaoSky`, velocidade vertical **`-1550 u/s`**), subindo acima das nuvens até a região dos **Castelos nas Alturas**!

#### Seção 9: *"Castelos nas alturas"* (`X = 27700` a `34080`, Checkpoints 8 e 9)
Lá no alto do céu (`Y = -650` a `-1250`), estende-se uma segunda sequência acrobática de **3 Conjuntos de [Cama Elástica Alta $\rightarrow$ Castelo Alto $\rightarrow$ Ponte Quebradiça Alta]**:
1. **Conjunto Alto 1**: `cama-alta-1` (`27700–28020`, `Y = -650`, impulso `1360`) $\rightarrow$ `castelo-alto-1` (`28120–28520`, `Y = -1080`) $\rightarrow$ `ponte-alta-1-0..3` (`28520–29280`, `Y = -1080`, cede em `0,24s`).
2. **Conjunto Alto 2 (Checkpoint 8 em `X = 29790`)**: `cama-alta-2` (`29720–30040`, `Y = -720`, impulso `1380`) $\rightarrow$ `castelo-alto-2` (`30140–30540`, `Y = -1160`) $\rightarrow$ `ponte-alta-2-0..3` (`30540–31300`, `Y = -1160`).
3. **Conjunto Alto 3 (Checkpoint 9 em `X = 31810`)**: `cama-alta-3` (`31740–32060`, `Y = -790`, impulso `1400`) $\rightarrow$ `castelo-alto-3` (`32160–32560`, `Y = -1240`) $\rightarrow$ `ponte-alta-3-0..3` (`32560–33320`, `Y = -1240`).
- **As 3 Calhas Elásticas em "U" (*Halfpipes*) e Lonas de Retorno ao Céu**:
  - Se Yuna cair de qualquer uma das pontes altas lá no céu, ela **não cai no abismo**: logo abaixo de cada conjunto (`Y = +350, +200, +150`) há uma gigantesca **Calha Elástica em "U" (`calha-elastica-1..3`)** ladeada por **paredes elásticas laterais de 3 andares (`lateral-i-left/right-0..2`)**!
  - Ao cair na calha, Yuna quica de uma parede lateral para a outra; **após 2 rebotes laterais (`hits >= 2`)**, a calha ejeta Yuna em um arco balístico perfeito para cima da **Lona de Retorno (`lona-retorno-1..3`, impulso vertical `2200–2300 u/s`)**, que a dispara como um foguete de volta para os Castelos nas Alturas!

#### Seção 10: *"Entrada dos túneis"* (`X = 34080` a `35600`, Checkpoint 10 em `X = 34320`)
- Ao atravessar a 3ª ponte alta, Yuna pousa no grande **`terraço-final`** (`X = 34080` a `35600`, `Y = -1250`).
- Em `X = 35380` abre-se o portal circular colorido do primeiro **Tobogã Tubular Interno**. Ao cruzar essa entrada, inicia-se automaticamente a **Etapa 2 (Circuito Interno de Tubos)**.

---

## 5. Etapa 2: O Circuito Interno de 4 Tobogãs Tubulares (`brinquedaoInterior.js`)

O interior do Brinquedão consiste em **4 Tobogãs Tubulares Translúcidos de Alta Velocidade** intercalados por **3 Desafios Acrobáticos**. Cada tubo funciona como um **Checkpoint automático** (se Yuna errar um desafio entre dois tubos, ela reinicia diretamente no tubo imediatamente anterior):

### 5.1. Tubo 1: *"Arrancada do Triângulo"* (Velocidade no tubo: `2300 u/s`)
- **Deslizamento pelo Tubo 1**: Yuna desliza por dentro do primeiro tubo espiral translúcido (`X = 240` a `2320` da cena interna) a **`2300 u/s`**, acompanhando as curvas sinuosas do tubo.
- **Desafio 1 — A Corrida do Triângulo Azul (`run` mode)**:
  - A boca de saída do Tubo 1 ejeta Yuna correndo automaticamente em alta velocidade (**`760 u/s`**) sobre uma pista acolchoada reta de **`2200` unidades** de extensão (`Y = 790`).
  - No meio da pista (em `X = 1120` a partir da saída do tubo) há um grande **Obstáculo Triangular Azul de Espuma (`triangle`, largura `170`, altura `94`)**.
  - O jogador deve pressionar **`Espaço / Pular`** no tempo certo para saltar por cima do triângulo em plena velocidade (impulso de salto `760`, gravidade `1750`).
  - Se bater no triângulo, Yuna dá um tranco acolchoado para trás (`0,8s`) e repete a descida do Tubo 1. Se saltar com sucesso, ela corre até o fim da pista e mergulha na boca do **Tubo 2**!

### 5.2. Tubo 2: *"Torre dos Rebotes"* (Velocidade no tubo: `2950 u/s`)
- **Deslizamento pelo Tubo 2**: Ainda mais veloz (`2950 u/s`), o Tubo 2 faz um grande looping descendente e ejeta Yuna no topo de um poço vertical acolchoado: a **Torre dos Rebotes**.
- **Desafio 2 — A Torre dos Rebotes Estilo Pinball (`ricochet` mode)**:
  - Ao sair do Tubo 2, Yuna entra em uma sequência acrobática espetacular de **6 Rebotes Diagonais Automáticos (`BRINQUEDAO_REBOUNDS`, `0,65s` por arco de voo)**, quicando em zigue-zague entre colchões elásticos inclinados nas paredes esquerda e direita da torre enquanto desce de `Y = 850` até `Y = 1960`:
    1. Rebote 1 (Parede Direita: `X = 2830, Y = 850`) $\rightarrow$
    2. Rebote 2 (Parede Esquerda: `X = 2160, Y = 1090`) $\rightarrow$
    3. Rebote 3 (Parede Direita: `X = 2850, Y = 1320`) $\rightarrow$
    4. Rebote 4 (Parede Esquerda: `X = 2160, Y = 1540`) $\rightarrow$
    5. Rebote 5 (Parede Direita: `X = 2850, Y = 1760`) $\rightarrow$
    6. Rebote 6 (Parede Esquerda: `X = 2240, Y = 1960`) $\rightarrow$ Ejeta Yuna diretamente dentro da boca do **Tubo 3** (`X = 2860, Y = 2110`)!

### 5.3. Tubo 3: *"Corrida dos Blocos"* (Velocidade no tubo: `3150 u/s`)
- **Deslizamento pelo Tubo 3**: Yuna desliza a **`3150 u/s`** pelas curvas do terceiro tubo e é ejetada na plataforma inicial da **Ponte dos 15 Blocos Quebradiços (`gauntlet` mode)**.
- **Desafio 3 — A Ponte dos 15 Blocos que Cedem em Onda (`brinquedaoGauntlet.js`)**:
  - Entre a saída do Tubo 3 (`X = 0` a `300`) e a plataforma de entrada do Tubo 4 (`X = 2100` a `2520`) estende-se uma ponte formada por **15 blocos acolchoados suspensos** (`count: 15`, de `X = 300` a `2100`, cada bloco com `120` unidades de largura).
  - Assim que Yuna pisa na ponte, inicia-se uma **onda contínua de colapso de trás para a frente**: após um atraso inicial de **`0,55s`**, os blocos começam a despencar um após o outro a cada **`0,15 segundo`** (`1,3x` mais lento no modo `FÁCIL`, dando mais folga).
  - O jogador controla Yuna (`velocidade máxima 520 u/s`, `salto 720 u/s`) e deve correr e saltar sem hesitar até alcançar a plataforma segura em `X = 2100` e entrar na boca do **Tubo 4**! (Se cair no vão dos blocos que caíram, a tela de Game Over permite tentar novamente direto do Tubo 3).

### 5.4. Tubo 4: *"Tobogã da Chegada"* (Velocidade no tubo: `3450 u/s`)
- **O Grande Mergulho Final**: O último tobogã é o mais rápido de todo o jogo (**`3450 u/s`**), descrevendo duas grandes ondas de montanha-russa antes de disparar Yuna para fora da estrutura do Brinquedão, pousando suavemente sobre o **Gramado Ensolarado da Chegada (`finish` mode)**, decorado com bandeirolas coloridas e uma grande faixa aérea escrita **"CHEGADA"**.

---

## 6. Cena de Encerramento da Campanha: O Descanso no Gramado (`BRINQUEDAO_ENDING`, 4 Beats)

Ao sair do último tobogã, Yuna caminha desacelerando pelo gramado macio sob o sol dourado do fim de tarde e deita-se confortavelmente na grama, com um sorriso sereno no rosto. Nanda voa até ela, pousa ao seu lado e dobra as asinhas para descansar junto com a irmã.

Seguem-se os **4 beats finais da campanha** (duração total de **25 segundos**):

### Beat 1 (`END_0`, `pose: 2`, Duração: `5,5s`, Mínimo: `2,2s`)
- **Ação Visual**: Yuna desacelera no gramado após a faixa de chegada e senta/deita na grama iluminada pelo sol dourado, respirando aliviada e feliz.
- **Diálogo (`Narrador`)**:
  > "O último tobogã tinha ficado para trás.
  > As pernas pesavam, mas o coração estava leve."

### Beat 2 (`END_1`, `pose: 1`, Duração: `6,0s`, Mínimo: `2,2s`)
- **Ação Visual**: Nanda paira suavemente sobre Yuna com um brilho dourado acolhedor.
- **Diálogo (`Nanda`)**:
  > "Você foi muito corajosa hoje, Yuna.
  > Agora pode descansar."

### Beat 3 (`END_2`, `pose: 2`, Duração: `7,0s`, Mínimo: `2,5s`)
- **Ação Visual**: Yuna fecha os olhos em paz no gramado; Nanda pousa pertinho dela e dobra delicadamente suas asas translúcidas para dormir junto.
- **Diálogo (`Narrador`)**:
  > "O sono veio como um abraço bom.
  > E a fadinha dobrou as asas para sonhar junto."

### Beat 4 (`END_3`, `pose: 3`, Duração: `6,5s`, Mínimo: `2,5s`, Cartão Final e Fade Out)
- **Ação Visual**: A luz dourada da tarde envolve as duas irmãs descansando no jardim enquanto a tela faz um *fade out* suave e exibe o título de encerramento:
- **Cabeçalho (`BRINQUEDÃO CONCLUÍDO`)**:
  > "Entre uma ponte e um escorregador,
  > o dia voltou a caber num sorriso."

Ao concluir o último beat, o progresso salva a campanha completa como concluída e retorna suavemente à **Tela de Título (Menu Principal)**.

---

## 7. Critérios de Aceite para Construção na Unity (Checklist White-Room)

1. **Cena de Convite no Piquenique (7 Beats)**: Exibir os 7 beats de diálogo entre Yuna, Nanda e o Narrador logo após o despertar do pesadelo.
2. **Circuito Externo Completo (10 Seções, `X = 0` a `35600`)**: Passarelas acolchoadas, escorregadores de alta velocidade (`980 u/s`), 3 pontes de bolinhas com 6 peixes saltadores que empurram sem matar, 2 atalhos superiores, escorregador gigante, 3 conjuntos de camas elásticas/castelos/pontes quebradiças com rotas inferiores de recuperação, parede elástica + rampa reversa de lançamento ao céu e 3 conjuntos nas alturas com calhas elásticas em "U" de 2 rebotes laterais.
3. **Circuito Interno de 4 Tobogãs Tubulares e 3 Desafios**:
   - Tubo 1 (`2300 u/s`) $\rightarrow$ Corrida automática com salto sobre o Triângulo Azul;
   - Tubo 2 (`2950 u/s`) $\rightarrow$ Torre dos Rebotes com 6 quiques diagonais automáticos estilo pinball;
   - Tubo 3 (`3150 u/s`) $\rightarrow$ Ponte de 15 blocos que colapsam em onda contínua (`0,15s` por bloco);
   - Tubo 4 (`3450 u/s`) $\rightarrow$ Ejeção para o Gramado da Chegada.
4. **Encerramento Emocional da Campanha (4 Beats)**: Cena final de Yuna e Nanda descansando juntas no gramado ensolarado com os 4 beats de texto exatos e retorno ao Menu Principal.
