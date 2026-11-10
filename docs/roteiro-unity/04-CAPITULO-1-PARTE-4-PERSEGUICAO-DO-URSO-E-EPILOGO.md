# 04 — Capítulo 1 (Parte 4): A Perseguição do Urso 3D e o Epílogo do Quarto Arrumado (Unity 3D)

> **Instrução de Implementação (White Room — Unity 3D):**
> Após derrotar o Guardião de Pelúcia na Arena Imaginária, o último brinquedo que falta guardar no baú é justamente o **Urso**.
> Esta fase divide-se em três momentos:
> 1. **Abertura Dramática e Cômica (`6.05s`):** Yuna se aproxima devagar para pegar o urso caído no chão, mas quando seus dedos quase encostam na pelúcia, o urso dá um pulo de susto e sai correndo pelo quarto!
> 2. **Corrida em Corredor 3D (`Bear Chase` — 720 metros):** A câmera muda para **3ª pessoa de costas (*Over-the-Shoulder 3D Runner*)**, acompanhando Yuna enquanto ela corre atrás do urso por um longo corredor sinuoso de 720 metros com curvas e rampas dentro do quarto agigantado, desviando para a esquerda e para a direita dos brinquedos que o urso arremessa para trás.
> 3. **Epílogo do Quarto Arrumado (`ToyRoomEpilogue`):** Com todos os brinquedos finalmente dentro do baú, o quarto está limpo... e Yuna faz a pergunta inocente que leva Nanda ao famoso *facepalm* silencioso antes de encerrar o Capítulo 1.

---

## 1. Cinemática de Abertura da Perseguição (`BEAR_DOWN` $\rightarrow$ `CHASE_START`)

A cena começa num plano lateral 3D próximo ao chão da Sala de Brinquedos (tapete, poltrona ao fundo esquerdo, janela com cortinas à direita, assoalho de carvalho):

### Cronograma das 4 Etapas de Largada (`chaseConfig.js` + `chaseHUD.js`):
1. **`BEAR_DOWN` (`1.20 segundo`):**
   - O Urso de Pelúcia está caído sentado/deitado no chão à direita (`74%` da tela). Yuna está à esquerda (`19%`), e Nanda flutua suavemente atrás do ombro dela.
   - **Banner Superior do HUD:** `O urso caiu…`
2. **`GIRL_APPROACH` (`3.60 segundos`):**
   - **Banner Superior do HUD:** `Ela se aproxima devagar.`
   - De `0% a 73%` deste tempo (`0.0s a 2.63s`): Yuna caminha pé ante pé, devagarinho, em direção ao urso caído (ouvem-se os passinhos suaves da menina a cada `0.65s`).
   - De `73% a 100%` (`2.63s a 3.60s`): Yuna para bem pertinho do urso e **estende delicadamente a mãozinha para frente (`reaching`)**, com a ponta dos dedos chegando a poucos centímetros da pelúcia!
3. **`BEAR_RISE` (`1.25 segundo`):**
   - **Banner Superior do HUD:** `Ele está se levantando!`
   - No instante em que a mão de Yuna quase o toca (`0.0s a 0.15s`), o Urso abre os olhos num susto, levanta-se num salto (`0.15s a 0.67s`) enquanto Yuna recolhe a mão surpresa, vira de costas (`0.82s`) e **dispara em fuga para a direita**!
   - Nos últimos `0.25s` desta etapa, um *fade* rápido faz o corte para a câmera 3D de perseguição posicionada atrás das costas da Yuna.
4. **`CHASE_START` (`1.10 segundo`):**
   - A câmera 3D em 3ª pessoa se revela atrás da Yuna. O Urso abre vantagem inicial correndo à frente (`gap` sobe de `3m` para `29m`), e a corrida começa!

---

## 2. Mecânica e Geometria da Corrida 3D (`RUNNING` — 720 Metros)

### 2.1. O Corredor Sinuoso 3D (`chaseRoute.js`)
- **Comprimento Total da Pista:** `720 metros` (`Z = 0` até `Z = -720`), com largura transitável de `7.2 metros` (`halfWidth = 3.6m`, de `X = -3.6` a `X = +3.6`), dividida logicamente em **3 faixas (`lanes`)**:
  - Faixa Esquerda: `X = -2.4m`
  - Faixa Central: `X = 0.0m`
  - Faixa Direita: `X = +2.4m`
- **Curvas e Rampas Contínuas (Interpolação Cúbica *Hermite*):**
  - O corredor do quarto agigantado não é reto; ele serpenteia suavemente passando por **nós de curva e elevação (`ROUTE_NODES`)**:
    - `0m a 40m`: Reta plana inicial (`X = 0, Y = 0`).
    - `100m`: Curva à direita (`X = +22m`) e subida (`Y = +2.8m`).
    - `165m`: Passagem central (`X = -6m, Y = +0.5m`).
    - `225m`: Curva fechada à esquerda (`X = -28m`) e descida (`Y = -2.2m`).
    - `290m`: Subida acentuada (`X = +8m, Y = +3.8m`).
    - `355m`: Curva ampla à direita (`X = +30m, Y = +1.0m`).
    - `420m`: Descida à esquerda (`X = -12m, Y = -2.8m`).
    - `485m`: Curva à esquerda em subida (`X = -30m, Y = +1.4m`).
    - `555m`: Ponto mais alto da pista (`X = +16m, Y = +4.2m`).
    - `620m`: Curva à direita em descida (`X = +28m, Y = -1.5m`).
    - `680m a 720m`: Reta final plana (`X = 0, Y = 0`) onde o Baú de Brinquedos aguarda.
- **Cenário 3D ao Longo do Corredor:**
  - Paredes altas do quarto com rodapés de carvalho, papel de parede ilustrado, janelas iluminadas, tapetes passadeira sob os pés e grandes móveis 3D passando nas laterais do corredor (estantes, poltronas, blocos gigantes), dando intensa sensação de velocidade.
- **Nanda em Voo 3D:**
  - Durante toda a corrida, a fadinha Nanda voa em 3D atrás e acima da Yuna (`Y ~ 2.65m`), oscilando suavemente de um lado para o outro com rastro de luz.

### 2.2. Física da Corrida, Curvatura e Aproximação (`chasePacing.js`)
- **Velocidade Longitudinal (`speed`):** Começa em `8.0 m/s` e acelera gradualmente conforme a intensidade da perseguição até `12.0 m/s` (`8 * 1.5`), sendo também influenciada pelas subidas e descidas (`slope`) da pista.
- **Velocidade Lateral da Jogadora (`lateralSpeed`):** `6.0 m/s` ao mover para a esquerda (`A / Seta Esquerda`) ou direita (`D / Seta Direita`).
- **Deriva Centrífuga nas Curvas (`drift`):** Nas curvas da pista, a força centrífuga puxa Yuna levemente para o lado externo da curva (`fator 0.55` no Normal, `0.25` no Fácil), exigindo que o jogador compense na direção!
- **Distância para o Urso (`gap`):**
  - O Urso começa a `29 metros` de distância à frente da Yuna.
  - Conforme Yuna corre até os `94%` da pista, a distância diminui gradualmente (`de 29m para 19m no Normal / 22m no Fácil`).
  - Nos **últimos 6% da pista (`94% a 100%`, de `676m a 720m`)**, Yuna dá o *sprint* final e fecha a distância rapidamente até `1.2 metro` para alcançar o Urso!

### 2.3. Brinquedos Arremessados pelo Urso e Emboscadas (`chasePatterns.js`)
- Enquanto foge lá na frente, o Urso arremessa grupos de brinquedos 3D para trás em arcos parabólicos (`tempo de voo = 0.65s -> 0.38s`) que caem na pista bloqueando faixas:
  - **Os 3 Tipos de Obstáculos 3D Arremessados (`kind`):**
    1. `blocks` — Pilhas de Blocos Coloridos
    2. `train` — Vagões/Trenzinho de Madeira
    3. `bunny` — Coelhinhos de Pelúcia
  - **9 Pontos de Emboscada na Entrada de Curvas (`ROUTE_AMBUSHES`):** Nas marcas de `82m, 148m, 208m, 274m, 338m, 404m, 468m, 538m e 603m`, o Urso lança padrões posicionados estrategicamente na entrada da curva, deixando sempre pelo menos **um corredor livre (`safeLane`)** alcançável.
  - **Pressão Final (Últimos 15% da pista no Modo Normal — `612m a 720m`):** O Urso entra em desespero e arremessa pares de brinquedos largos **a cada `1.0 segundo`**, alternando a faixa livre entre saídas vizinhas para um clímax emocionante!

### 2.4. Vidas (Corações), Tropeços e Game Over (`chaseHealth.js`)
- **Corações no Topo Central do HUD:**
  - Modo **NORMAL**: **3 Corações** (tolera 2 tropeços; no 3º impacto cai).
  - Modo **FÁCIL (`EASY`)**: **6 Corações** (o dobro de tolerância!).
- **Reação ao Tropeçar num Brinquedo (`obstacleHit`):**
  - Se ainda restarem corações: Yuna tropeça, sua velocidade cai para `58%` durante `0.65s` (`stumble`), o Urso abre `+1m` de vantagem temporária, e ela recebe `1.3s` de invencibilidade (`2.0s` no Fácil).
  - **Queda e Derrota (`FALLING` $\rightarrow$ `GAME_OVER`):** Se perder o último coração, Yuna cai no tapete (`1.1s` de animação de queda) e surge a mensagem:
    - **Banner Superior:** `A menina não alcançou o urso. Tentar novamente?`
    - **Rodapé:** `A menina caiu. Tente novamente pelos botões da tela.`

---

## 3. Captura do Urso e Retorno ao Baú (`CAUGHT` $\rightarrow$ `CHEST_RETURN` $\rightarrow$ `COMPLETE`)

Quando Yuna atinge os **`720 metros`** sem perder todos os corações:
1. **`CAUGHT` (`1.40 segundo`):**
   - **Banner do HUD:** `Você alcançou o urso!`
   - Yuna finalmente alcança o Urso de Pelúcia e o abraça/segura com as duas mãos!
2. **`CHEST_RETURN` (`2.80 segundos`):**
   - **Banner do HUD:** `De volta ao baú.`
   - Yuna caminha até o grande Baú de Brinquedos no fim do corredor, a tampa se abre e ela guarda o Urso lá dentro junto com todos os outros brinquedos.
3. **`COMPLETE`:**
   - **Banner do HUD:** `Perseguição concluída!`
   - Dispara o evento `chestReturned` e inicia-se imediatamente o **Epílogo do Quarto Arrumado**.

---

## 4. Epílogo do Capítulo 1: O Quarto Arrumado e o Facepalm da Nanda (`ToyRoomEpilogue.js`)

A câmera corta para um plano amplo e belíssimo da **Sala de Brinquedos totalmente arrumada (`chapter-clean-room`)**: o chão e o tapete estão completamente livres, todos os brinquedos estão guardados dentro do baú fechado, e a luz suave ilumina Yuna e Nanda no centro do quarto.

### Cronograma Exato dos 6 Quadros do Epílogo (`37.5 segundos` no total):

1. **Quadro 1 (`7.0 segundos` — Pose 0: Yuna sorrindo aliviada):**
   - **Cabeçalho do Quadro Poético:** `NANDA`
   - **Texto (Fonte Manuscrita em Quadro Pastel):**
     > *"Ufa, conseguimos. Guardamos os brinquedos a tempo. Agora podemos brincar."*
2. **Quadro 2 (`6.0 segundos` — Pose 1: Yuna olhando ao redor do quarto vazio):**
   - **Cabeçalho do Quadro Poético:** `UM QUARTO INTEIRO`
   - **Texto:**
     > *"A menina sorriu.*
     > *De canto a canto, passeou os olhos pelo quarto."*
3. **Quadro 3 (`7.0 segundos` — Transição suave da Pose 1 para a Pose 2: O sorriso diminui, pensativa):**
   - **Cabeçalho do Quadro Poético:** `UM QUARTO INTEIRO`
   - **Texto:**
     > *"Onde antes havia brinquedos, agora havia espaço.*
     > *O sorriso ficou pequeno. Uma pergunta cresceu no lugar."*
4. **Quadro 4 (`8.0 segundos` — Pose 2: Yuna olha inocentemente para Nanda):**
   - **Cabeçalho do Quadro Poético:** `MENINA`
   - **Texto:**
     > *"Mas… agora todos os brinquedos estão guardados.*
     > *Como vamos brincar? Vamos pegar os brinquedos de novo?"*
5. **Quadro 5 (`4.5 segundos` — O Facepalm Silencioso da Nanda — `facepalm: true`):**
   - **Regra Especial:** Este quadro **não possui texto** e **não pode ser pulado pelo botão de avançar diálogo**!
   - A câmera realiza um **zoom lento e cômico em direção à fadinha Nanda** (`zoom até 2.5x+`), que fecha os olhos e **leva a mãozinha à testa num clássico *facepalm* silencioso** diante da lógica irrefutável da menina, depois de todo o trabalho que tiveram para guardar tudo!
6. **Quadro 6 (`5.0 segundos` — Cartão de Transição para o Capítulo 2):**
   - Surge em tela cheia o cartão ilustrado anunciando o novo capítulo da campanha:
     > **CAPÍTULO 2**
     > **DIA DE PIQUENIQUE NO JARDIM**
   - Ao concluir este cartão, o jogo inicia o **Capítulo 2 — Parte 1: Um Dia no Jardim**.
