# Documento 07 — Capítulo 2, Parte 3: O Pesadelo Furtivo e o Despertar

> **Propósito deste documento (White-Room Prompt para Unity 3D)**:
> Este documento especifica de forma completa e autossuficiente a **Parte 3 do Capítulo 2: O Pesadelo Furtivo e o Despertar** de *O Quarto dos Brinquedos*. Deve ser usado como especificação direta para construir toda a cena do pântano escuro do pesadelo, a revelação da Barata Monumental com seus feixes volumétricos de luz nos olhos, a travessia furtiva entre rochas, troncos e morros de terra, a mecânica de mergulho e submersão nos 4 lagos de lama para se esconder da luz, o bote final inevitável e o despertar sob o sol no piquenique com Nanda em **3D com jogabilidade lateral (2.5D)** na Unity, **sem depender de código ou assets legados**.
> **Restrição absoluta**: Zero linhas de código neste documento. Todas as regras, medidas proporcionais, tempos, diálogos e mecânicas estão descritos em linguagem natural estruturada.

---

## 1. Visão Geral e Papel Dramático da Fase

Quando o vento do sonho cessa ao final do voo na folha (Documento 06), Yuna despenca para a camada mais profunda e sombria do seu sono: **O Pesadelo**.

O cenário não é mais azulado nem poético; é um pântano sufocante, escuro, úmido e opressor, onde o ar carrega *"cheiro de comida que ninguém quer, cheiro de luz apagada, cheiro de vontade de ir pra casa, cheiro de tristeza"*. Das árvores retorcidas ao fundo emerge o maior pavor infantil daquele jardim: uma **Barata Monumental** (de proporções titânicas em relação à pequena Yuna), que patrulha o fundo do cenário projetando **dois feixes de luz volumétrica amarelada diretamente de seus olhos** como holofotes de busca.

Esta fase muda completamente o ritmo do jogo para **Stealth Lateral 2.5D (Furtividade e Sombra)** no estilo de *Inside* e *Little Nightmares*:
1. **Metade 1 — Terra Firme, Rochas, Troncos e Morros (`X = 220` a `4400`)**: Yuna caminha na ponta dos pés, escondendo-se atrás da sombra física projetada por **6 rochas** e **6 troncos cortados** no primeiro plano enquanto escala **3 morros íngremes de terra** nos intervalos em que os holofotes oculares da Barata Monumental varrem para o outro lado.
2. **Metade 2 — Os 4 Lagos de Lama Profunda (`X = 4400` a `8620`)**: O chão dá lugar a quatro extensos lagos de lama espessa. Yuna precisa saltar na lama, nadar com a cabeça para fora e, sempre que os feixes de luz da Barata se aproximam, **mergulhar completamente sob a superfície da lama** para não ser vista, escalando os barrancos escorregadios entre um lago e outro.
3. **O Encurralamento Final e o Grito de Despertar (`X = 8632` a `8980`)**: Ao sair do quarto lago de lama, não há mais onde se esconder. A Barata Monumental cresce sobre Yuna na escuridão e dá o bote final — que corta instantaneamente, no exato momento do grito, para a **toalha de piquenique banhada de sol**, onde Nanda acorda assustada e abraça/acalma a irmã.

---

## 2. Direção Visual 3D, Iluminação Volumétrica e Áudio

### 2.1. Cenário 3D e Sistema de Sombras Reais (2.5D Stealth Lighting)
- **Paleta do Pesadelo**: Preto-carvão, marrom-lama escuro, verde-pântano doentio, cinza-fumaça e o amarelo-esverdeado pálido e agressivo dos holofotes oculares da Barata Monumental.
- **Iluminação Volumétrica e Oclusão Real em 3D**:
  - Na Unity, os dois olhos da Barata Monumental (posicionada no plano de fundo próximo, em $Z > 0$, olhando para a trilha jugável em $Z = 0$) devem emitir **dois cones reais de luz volumétrica (*Spotlights* com *Volumetric Fog/Dust* e *Shadow Casting* ativo)**.
  - Os objetos de cobertura (**6 rochas** e **6 troncos**) ficam posicionados ligeiramente entre a Barata e a faixa onde Yuna caminha (ou no mesmo plano com volume 3D espesso), projetando **sombras reais e nítidas no chão e sobre o corpo de Yuna**. Assim, o jogador enxerga com total clareza visual exatamente onde a luz bate e onde a sombra protege Yuna.
- **Superfície dos 4 Lagos de Lama**:
  - A lama é escura, densa e viscosa, com brilho úmido na superfície, bolhas ocasionais e deformação/ondulação quando Yuna salta, nada ou submerge.
  - Quando Yuna submerge totalmente na lama, a superfície opaca do lodo bloqueia os feixes de luz da Barata, tornando-a invisível para o monstro.

### 2.2. Modelos 3D e Animações Específicas
- **Yuna no Pesadelo**:
  - **Caminhada Furtiva (*Tiptoe*)**: Quando está em terra firme no pesadelo, Yuna caminha encolhida, passos curtos e silenciosos na ponta dos pés, olhando apreensiva para o fundo.
  - **Animação de Salto na Lama (`mudJump`, 4 estágios)**: Impulso na borda do barranco $\rightarrow$ arco no ar encolhendo as pernas $\rightarrow$ impacto espirrando lama pesada $\rightarrow$ afundamento até a altura do pescoço.
  - **Nado na Lama e Submersão**:
    - *Cabeça fora da lama*: Apenas a cabeça e os ombros sujos de lama aparecem acima da superfície enquanto ela rema com os braços.
    - *Submersa*: Yuna puxa o ar e afunda a cabeça inteira sob o lodo; apenas pequenas bolhas e uma leve ondulação na superfície indicam onde ela está nadando por baixo da lama.
  - **Animação de Escalada de Barranco (`bankClimb`, 6 estágios detalhados)**:
    - Ao chegar à margem oposta de um lago de lama, Yuna apoia as duas palmas das mãos no barranco alto (`estágio 1`), faz força com os braços para erguer o tronco (`estágio 2`), **sofre um breve escorregão dramático na lama úmida entre `28%` e `44%` da subida** (`estágio 3`), firma o joelho na borda (`estágio 4`), apoia o pé (`estágio 5`) e finalmente se ergue de pé em terra firme (`estágio 6`).
- **A Barata Monumental (`Colossal Cockroach`)**:
  - Modelo 3D colossal (largura equivalente a **`1900` unidades** do cenário — maior que uma casa em relação a Yuna).
  - Carapaça escura segmentada e reluzente, longas antenas trêmulas que sondam o ar, patas espinhosas que fazem o chão vibrar a cada passo e dois grandes olhos compostos brilhantes que disparam os feixes gêmeos de luz de busca.

### 2.3. Direção de Áudio
- **Queda e Introdução**: Ao cair no pântano, a música do sonho para completamente. Ouve-se o baque úmido do pouso e um zumbido grave e claustrofóbico de silêncio opressor.
- **Revelação e Furtividade**: Quando a Barata Monumental surge das sombras, entra a faixa tensa e percussiva **"Bug Attack"** (música de suspense/horror rítmico), acompanhada pelos estrondos pesados das patas da barata (`roach-step`) a cada passo de patrulha.
- **Sons de Lama**: Impacto pesado ao cair no lodo (`mud-impact`), som viscoso de nado e submersão, respiração contida e escorregão no barranco.
- **Clímax e Despertar**: Estrondo do bote final (`roach-stomp`), cortado abruptamente pelo **grito agudo de susto de Yuna (`nightmare-scream`)**, que ecoa já no silêncio ensolarado do jardim real, seguido pelo canto calmo de pássaros da tarde.

---

## 3. Queda, Introdução e Revelação da Barata Monumental

### 3.1. Queda e Pouso no Pântano (`NIGHTMARE_FALL`, `1,8s`)
- Yuna cai verticalmente da escuridão superior (`Y = -80` até o solo do pântano em `Y = 850`, na posição inicial `X = 220`) e se levanta devagar no chão escuro e úmido.

### 3.2. Cena de Introdução: O Cheiro de Tristeza (`NIGHTMARE_INTRO`, 2 Beats)
Com Yuna parada na entrada do pântano escuro, ocorrem **2 beats narrativos** (com avanço manual permitido após o tempo mínimo):

- **Beat 1 (Duração: `6,0s`, Mínimo: `2,5s`)**:
  - **Ação Visual**: Yuna olha ao redor na penumbra vazia do pântano, estranhando a ausência dos besouros e abelhas da floresta anterior.
  - **Diálogo (`Yuna`)**:
    > "Será que os bichos foram embora?"

- **Beat 2 (Duração: `14,0s`, Mínimo: `4,0s`)**:
  - **Ação Visual**: Uma névoa densa e escura começa a se arrastar pelo chão enquanto a câmera enquadra a floresta morta ao fundo.
  - **Diálogo (`Narrador`)**:
    > "Um cheiro repugnante aparece no ar. Cheiro de comida que ninguém quer, cheiro de luz apagada, cheiro de vontade de ir pra casa, cheiro de tristeza."

### 3.3. A Revelação do Monstro (`NIGHTMARE_REVEAL`, `8,0s`)
- Assim que o Beat 2 termina, inicia-se a música **"Bug Attack"**.
- Durante **`8,0 segundos`**, ao fundo da floresta pântanosa (alinhada à frente de Yuna em `X = 900`), a silhueta gigantesca da **Barata Monumental** emerge das árvores escuras, crescendo de escala (`0.32` até `1.0`) a cada passo pesado (`roach-step` sincronizado com tremores sutis de câmera).
- Nos segundos finais da revelação, seus dois olhos se acendem com intensidade máxima, projetando os **dois feixes cônicos de luz volumétrica** sobre o chão do pântano.
- Uma dica curta aparece na tela ao liberar o controle:
  > "Esconda-se atrás das pedras e troncos para fugir do olhar da barata!"

---

## 4. Regras de Patrulha da Barata e Detecção pelos Feixes de Luz

### 4.1. Movimentação e Varredura dos Feixes Oculares (`nightmareLights`)
- Durante toda a fase furtiva (`X = 220` a `8750`), a Barata Monumental caminha pelo plano de fundo acompanhando a região geral onde Yuna está, oscilando lateralmente e inclinando a cabeça para varrer o cenário com seus dois holofotes oculares.
- **Geometria dos Feixes**:
  - A cabeça da Barata fica elevada acima do cenário e seus dois olhos (esquerdo e direito) apontam os cones de luz para baixo, varrendo o chão e os lagos de lama.
  - **Abertura de cada cone de luz (`halfAngle`)**:
    - Modo `NORMAL`: **`0,21 radianos`** (~12° para cada lado do eixo central do feixe).
    - Modo `FÁCIL (`EASY`)`: **`0,17 radianos`** (~9,7°, feixes mais estreitos e mais fáceis de evitar).
  - Os feixes varrem de um lado para o outro em ciclos rítmicos previsíveis, permitindo que o jogador observe a luz passar por cima da pedra/tronco onde Yuna está escondida e corra para a próxima cobertura assim que o feixe se afastar.

### 4.2. Oclusão por Sombras e Lama (`pointBlockedByCover` e `mudOccludesPoint`)
Para verificar se Yuna está visível ou escondida, são testados 3 pontos do corpo dela (pés, tronco e cabeça):
1. **Oclusão por Cobertura em Terra Firme**: Se a linha reta entre o olho da Barata e o ponto do corpo de Yuna atravessar o volume 3D de uma **Rocha (`rock`)** ou de um **Tronco (`trunk`)**, a luz é bloqueada e aquele ponto está **na sombra (seguro)**.
2. **Oclusão por Submersão na Lama**: Quando Yuna está dentro de um dos 4 lagos de lama e **mergulha totalmente (`submerged`, profundidade `>= 0.92`)**, a superfície opaca da lama bloqueia 100% da visão da Barata. (Enquanto ela nada com a cabeça fora da lama, a cabeça continua exposta aos feixes de luz!).

### 4.3. Tempo de Exposição e Bote de Captura (`NIGHTMARE_STRIKE`)
- Se qualquer ponto não-ocluído do corpo de Yuna ficar dentro do cone de luz de um dos olhos da Barata por mais de:
  - **`0,12 segundo`** contínuo no modo `NORMAL` (ou **`0,24 segundo`** no modo `FÁCIL`),
  - A Barata solta um guincho, trava os dois feixes diretamente sobre Yuna e avança em um **Bote Fulminante (`NIGHTMARE_STRIKE`, duração `1,45s`, impacto `roach-stomp` em `0,72s`)**.
- O bote exibe a tela de **GAME OVER** (`"A barata gigante encontrou Yuna na luz..."`), permitindo reiniciar imediatamente do último **Checkpoint Furtivo** alcançado.

---

## 5. Layout Completo da Travessia Furtiva (`X = 220` a `8750`)

A velocidade de caminhada furtiva de Yuna em terra firme é reduzida para **`125 u/s`** (com salto de impulso `680` e gravidade `1500`), transmitindo o peso e a cautela do pesadelo. Existem **4 Checkpoints Furtivos** ao longo do percurso:
- **Checkpoint 0**: Início em terra firme (`X = 220`).
- **Checkpoint 1**: Antes da subida dos 3 morros de terra (`X = 2380`, atrás da cobertura `stealth-cover-6`).
- **Checkpoint 2**: Dentro do **1º Lago de Lama** (`X = 4560`, já submersa em segurança na lama).
- **Checkpoint 3**: Dentro do **3º Lago de Lama** (`X = 6560`, já submersa em segurança na lama).

### 5.1. Parte 1: Rochas, Troncos e os 3 Morros de Terra (`X = 220` a `4400`)
- **As 12 Coberturas de Primeiro Plano (`STEALTH_COVERS`)**:
  - Distribuídas a cada **`360` unidades horizontais** desde `X = 220` até `X = 4180`:
  - **Coberturas 0 a 5 (`X = 220, 580, 940, 1300, 1660, 2020`)**: **6 Rochas Musgosas (`rock`)** de alturas variadas (`128` a `144` unidades) e larguras de `112` a `134` unidades sobre o solo plano (`Y = 850`).
  - **Coberturas 6 a 11 (`X = 2380, 2740, 3100, 3460, 3820, 4180`)**: **6 Troncos Ocos/Cortados (`trunk`)** mais altos (`146` a `164` unidades de altura) posicionados estrategicamente antes, sobre e entre os morros de terra.
- **Os 3 Morros Íngremes de Terra (`STEALTH_PLATFORMS`)**:
  - Entre `X = 2520` e `X = 4280`, o terreno se eleva em **3 colinas/morros contínuos de terra escura** que exigem que Yuna salte/escale a encosta enquanto cronometra a passagem dos feixes de luz:
    1. **Morro 1 (`stealth-hill-0`)**: De `X = 2520` a `2980`, elevando o solo de `Y = 850` para o topo em **`Y = 775`** (com o tronco `stealth-cover-7` no topo em `X = 2740`).
    2. **Morro 2 (`stealth-hill-1`)**: De `X = 3240` a `3700`, subindo até **`Y = 750`** (com o tronco `stealth-cover-9` no topo em `X = 3460`).
    3. **Morro 3 (`stealth-hill-2`)**: De `X = 3960` a `4280`, subindo até **`Y = 785`** (com o tronco `stealth-cover-11` no topo em `X = 4180`).

### 5.2. Parte 2: Os 4 Lagos de Lama Profunda (`X = 4400` a `8620`)
Ao descer do 3º morro de terra em `X = 4400`, começa a sequência dos **4 Lagos de Lama (`DREAM_MUD_LAKES`)**, separados por estreitos barrancos de terra firme:

1. **Lago de Lama 1 (`mud-lake-0`)**:
   - Extensão: de `X = 4400` a `X = 5250` (**`850` unidades de largura**, profundidade `330`).
   - Contém o **Checkpoint 2** em `X = 4560`.
   - Na margem direita (`X = 5250` a `5310`, apenas `60` unidades de largura!), há um barranco estreito com o tronco de cobertura **`mud-bank-cover-0`** (`X = 5280`).
2. **Lago de Lama 2 (`mud-lake-1`)**:
   - Extensão: de `X = 5310` a `X = 6330` (**`1020` unidades de largura**, profundidade `410`).
   - Na margem direita (`X = 6330` a `6430`, `100` unidades de largura), há um barranco com o tronco de cobertura **`mud-bank-cover-1`** (`X = 6380`).
3. **Lago de Lama 3 (`mud-lake-2`)**:
   - Extensão: de `X = 6430` a `X = 7130` (**`700` unidades de largura**, profundidade `300`).
   - Contém o **Checkpoint 3** em `X = 6560`.
   - Na margem direita (`X = 7130` a `7210`, `80` unidades de largura), há um barranco com o tronco de cobertura **`mud-bank-cover-2`** (`X = 7170`).
4. **Lago de Lama 4 (`mud-lake-3`, O Lago Longo)**:
   - Extensão: de `X = 7210` a `X = 8620` (**`1410` unidades de largura**, profundidade `430`).
   - Exige múltiplos ciclos de nadar e submergir sob os feixes de luz da Barata até alcançar o barranco final em `X = 8620`.

### 5.3. Regras Detalhadas da Mecânica de Lama (`stepDreamMud`)
- **Entrada no Lago (`enter`, `1,1s`)**:
  - Ao chegar à beira de um lago de lama e avançar/saltar para dentro dele, Yuna executa um salto automático em arco (`1,1s`), caindo na lama a `95` unidades da borda com um espirro pesado de lodo em `0,68s` (`mud-impact`).
- **Nado à Superfície vs. Submersão Protetora (`swim`)**:
  - **Nadando com a cabeça para fora**: Quando o jogador pressiona `Esquerda` ou `Direita` sem segurar para baixo, Yuna nada a **`105 u/s`** com a cabeça fora da lama (`mudDepth = 0.68`). Nesse estado ela avança mais rápido, mas **sua cabeça pode ser detectada pelos feixes de luz da Barata**.
  - **Mergulhando sob a lama**: Se o jogador **parar de mover lateralmente** ou **segurar `S / Seta para Baixo`**, Yuna afunda imediatamente a cabeça sob a superfície da lama (`mudDepth = 1.0`, totalmente invisível aos holofotes da Barata!). Enquanto segura para baixo, ela também pode nadar submersa mais devagar a **`78 u/s`**.
  - Na primeira vez que entra no Lago 1, surge a dica:
    > "Na lama, pare ou segure S/▼ para submergir e se esconder da luz!"
- **Escalada do Barranco (`climb`, `1,9s`)**:
  - Ao encostar na parede direita (ou esquerda) do lago de lama e pressionar na direção do barranco ou `Espaço / Pular`, Yuna inicia a subida de **`1,9 segundo`** pelos 6 estágios da animação `bankClimb` (incluindo o pequeno escorregão dramático no lodo antes de conseguir firmar o joelho e subir para o barranco).
  - **Atenção Tática**: Durante os `1,9s` em que está escalando o barranco para sair da lama, o corpo de Yuna fica exposto fora do lodo; por isso, o jogador deve iniciar a subida assim que o feixe de luz da Barata acabar de passar pelo barranco!

---

## 6. Clímax: O Bote Inevitável e o Despertar no Piquenique

### 6.1. O Trecho Final sem Cobertura (`NIGHTMARE_FINAL`, `6,0s`)
- Quando Yuna termina de escalar o barranco direito do 4º Lago de Lama (`X >= 8632`), ela chega a uma faixa nua e escura do pântano onde **não há mais pedras, nem troncos, nem lama**.
- O controle do jogador é bloqueado e inicia-se a sequência roteirizada final do pesadelo (`NIGHTMARE_FINAL`, duração exata de **`6,0 segundos`**):
  1. **`0,0s` a `2,7s`**: Yuna caminha/corre para a direita por `350` unidades na escuridão tentando achar uma saída, enquanto a Barata Monumental se aproxima rapidamente pelo fundo.
  2. **`2,7s` a `5,65s`**: Sem saída à frente, Yuna para, vira-se para trás (`facing = -1`) e olha para cima, paralisada de pavor. A Barata Monumental ergue-se diretamente atrás/acima dela, crescendo até **`1,35x` do seu tamanho normal**, bloqueando todo o horizonte escuro com seus dois holofotes oculares cravados em Yuna.
  3. **`5,65s` a `6,0s`**: A Barata Monumental despenca sobre Yuna com um estrondo ensurdecedor (`roach-stomp`).

### 6.2. Corte Seco e o Despertar sob o Sol (`NIGHTMARE_WAKE`, 3 Beats)
- No exato milissegundo em que o bote da Barata atinge a tela (`6,0s`), ocorre um **corte seco instantâneo** da escuridão total do pântano para a **claridade dourada e vibrante da toalha de piquenique no Jardim real**!
- Ouve-se o grito de susto de Yuna (`nightmare-scream`). Yuna salta sentada sobre a toalha xadrez, ofegante.
- Ao lado dela (novamente em seu tamanho normal de pequenina fada luminosa de `0.38x`), **Nanda acorda assustada aos `0,55s`** e voa imediatamente para perto do rosto da irmã.
- Seguem-se os **3 beats finais de despertar** (duração total de **17 segundos**):

#### Beat 1 (Duração: `5,0s`, Mínimo: `2,2s`)
- **Ação Visual**: Yuna sentada na toalha de piquenique sob o sol da tarde, com as mãos no peito, enquanto Nanda paira preocupada diante dela.
- **Diálogo (`Narrador`)**:
  > "O susto saiu num grito.
  > Nanda abriu os olhos."

#### Beat 2 (Duração: `5,0s`, Mínimo: `2,2s`)
- **Ação Visual**: Nanda aproxima-se carinhosamente de Yuna, brilhando sob a luz quente do jardim.
- **Diálogo (`Nanda`)**:
  > "Yuna… estou aqui."

#### Beat 3 (Duração: `7,0s`, Mínimo: `2,5s`)
- **Ação Visual**: Yuna respira fundo, olhando para a grama verde ensolarada, as flores do jardim e a irmã fadinha ao seu lado.
- **Diálogo (`Narrador`)**:
  > "Era a toalha. Era o sol.
  > O pesadelo tinha ficado do outro lado do sono."

Ao concluir o Beat 3, a cena emenda diretamente na abertura da última fase do jogo (**Documento 08 — O Brinquedão Final**), na mesma toalha de piquenique.

---

## 7. Critérios de Aceite para Construção na Unity (Checklist White-Room)

1. **Introdução e Revelação da Barata Monumental**: Queda no pântano escuro, 2 beats de diálogo inicial sobre o *"cheiro de tristeza"* e revelação de `8,0s` da Barata Monumental emergindo das árvores com som de passos pesados e feixes oculares volumétricos.
2. **Sombra Física Real nas 12 Coberturas e 3 Morros**: Os 2 cones de luz dos olhos da Barata devem ser bloqueados pelas 6 rochas e 6 troncos em 3D, exigindo que Yuna avance de sombra em sombra e escale os 3 morros de terra nos intervalos da varredura.
3. **Mecânica Completa nos 4 Lagos de Lama**: Salto em arco com espirro de lodo (`1,1s`), nado com cabeça fora (`105 u/s`, detectável pela luz), submersão total ao parar ou segurar para baixo (`78 u/s`, 100% invisível aos feixes de luz) e escalada de barranco em 6 estágios com breve escorregão (`1,9s`).
4. **Clímax Roteirizado e Corte Seco para o Sol**: Após sair do 4º lago de lama (`X >= 8632`), sequência automática de `6,0s` sem cobertura onde a Barata cresce a `1,35x` e dá o bote, cortando no exato instante do grito para o despertar ensolarado na toalha de piquenique com os 3 beats finais de Nanda e do Narrador.
