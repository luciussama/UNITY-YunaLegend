# 03 — Capítulo 1 (Parte 3): A Arena Imaginária — Combate de Chefes (Unity 3D / 2.5D)

> **Instrução de Implementação (White Room — Unity 3D):**
> Assim que Yuna equipa a Espada de Brinquedo na Sala de Brinquedos, a imaginação dela transforma o quarto na **Arena Imaginária ("O Quarto Lendário")**.
> Esta fase deve ser implementada como um **Combate de Ação Lateral 3D (2.5D)** no estilo dos duelos de chefes de *Hollow Knight / Ori / Cuphead*, mas inteiramente com modelos 3D estilizados, iluminação dramática de palco/sonho e câmera lateral enquadrando toda a arena (`X = 160` a `X = 1440`, piso em `Y = 690`).

---

## 1. A Transformação Épica do Quarto em Arena (`180 frames` / `3.0 segundos`)

Quando a fase começa (`TOY_ROOM_BATTLE`):
1. **Transição Visual 3D (`3.0s`):**
   - A câmera desliza suavemente da visão de exploração superior para um **plano lateral 2.5D heroico** ao nível do chão.
   - A luz matinal comum da sala se converte em uma **atmosfera épica de imaginação**: as paredes do quarto recuam na penumbra violeta-dourada, feixes dramáticos de luz iluminam o tapete central como uma arena lendária, e o **Baú de Brinquedos (`toy-chest`)** fica posicionado à direita da arena (`X = 1240`), com a tampa aberta e emanando névoa e brilho mágico dourado.
   - Yuna assume sua postura de combate à esquerda (`X = 430`), segurando a Espada de Brinquedo (que na imaginação dela brilha com poder real), enquanto Nanda paira no plano de fundo acompanhando a luta.
2. **Transição de Entrada e Saída dos Chefes (`120 frames` / `2.0 segundos` por troca):**
   - Cada um dos 4 adversários **emerge fisicamente de dentro do Baú de Brinquedos** à direita: primeiro o brinquedo se eleva de dentro do baú crescendo de escala (`0.16x` para `1.0x`), descreve um arco imponente no ar e aterrissa com peso no chão da arena (`X = 1120`), ganhando vida como um colosso de brinquedo!
   - Quando um dos 3 primeiros chefes é derrotado, ele encolhe de volta ao tamanho de brinquedo inerte e voa em arco de volta para dentro do Baú de Brinquedos (`outgoing`), enquanto o próximo chefe salta do baú para a arena (`incoming`).

---

## 2. Regras Fundamentais do Combate e o Sistema "Cabo de Guerra de Corações"

### 2.1. Movimentação e Ações da Yuna
- **Movimento Lateral (`Esquerda / Direita`):** Velocidade ágil no plano da arena, respeitando os limites laterais (`left = 160`, `right = 1440`) e a colisão física com o corpo do chefe (Yuna não atravessa o corpo do chefe pelo chão, apenas saltando por cima quando aplicável).
- **Salto (`Cima / Botão de Pulo`):** Permite saltar sobre investidas rasteiras, nuvens de vapor baixas e o *Rocket Punch* do Robô.
- **Agachar (`Baixo`):** Yuna se abaixa rapidamente ficando com metade da altura normal (`crouching = true`), essencial para passar ilesa por baixo do feixe alto do *Laser Beam* do Robô. Ela também pode desferir estocadas rápidas de espada enquanto está agachada!
- **Ataque de Espada (`Botão X / Espaço`):**
  - Duração total do golpe: `18 frames` (`0.3s`), com `6 frames` de preparação (*windup*), `6 frames` de lâmina ativa e `6 frames` de recolhimento, e tempo de recarga (*cooldown*) de `26 a 36 frames`.
- **Defesa Automática ao Recuar (`Guarding`):**
  - Se o inimigo estiver atacando e a jogadora segurar a direção **para trás** (afastando-se do chefe) fora de um golpe próprio, Yuna ergue a espada firme à frente do corpo (`DEFESA · ESPADA À FRENTE`), bloqueando o dano e mantendo os pés firmes no chão sem ser atravessada pela investida!

### 2.2. O Sistema de Transferência de Corações (`transferHeart`)
- **Não existe perda simples de HP — cada golpe válido transfere 1 coração entre Yuna e o Chefe!**
  - Yuna começa a sequência com **3 Corações (`hp: 3, maxHp: 3`)**.
  - **Quando Yuna acerta um golpe de espada num chefe vulnerável:** O chefe perde `1 HP` e **Yuna recupera `1 HP`** (até seu máximo de 3).
  - **Quando o chefe acerta um ataque não defendido em Yuna:** Yuna perde `1 HP` (entrando em `48 frames` / `0.8s` de invencibilidade piscante e `28 frames` de recuo) e **o chefe recupera `1 HP`** (até o `maxHp` daquele chefe)!
- **Defesa e Contra-Ataque dos Chefes (`arenaEnemyGuarding`):**
  - Os chefes (exceto o Trenzinho) possuem janelas de proteção durante seus ataques ou logo após tomarem um golpe (`PREPARANDO RESPOSTA · AGUARDE` / `RESPOSTA PROTEGIDA`). Se Yuna bater na guarda deles, saem faíscas metálicas/bloqueio sem transferir coração, exigindo que o jogador aguarde a **janela de abertura pós-golpe (`ABERTURA · CONTRA-ATAQUE`)** para punir o chefe.

---

## 3. O Ataque Especial: "Espada Arco-Íris" (Com Close-up de Anime)

Yuna possui **1 único uso por batalha inteira** (`specialUsed = false -> true`) do seu poder supremo da imaginação: a **Espada Arco-Íris (`ARENA_SPECIAL`)**, acionado pelo botão `R` (teclado) ou `Y` (gamepad) ou botão `ESPECIAL` na tela.

### Cronograma Cinematográfico do Especial (`156 frames` / `2.6 segundos` — física e IA congeladas):
1. **`Frames 0 a 78` (`0.0s a 1.3s`) — Carregamento e Corte de Anime nos Olhos:**
   - Yuna se vira para o inimigo e ergue a espada envolta por uma aura de **7 faixas das cores do arco-íris** (`#ff637d`, `#ffa34e`, `#ffe978`, `#80edb1`, `#74dbff`, `#9292ff`, `#e49aff`) e partículas orbitais.
   - **Dos frames `12 a 88`:** A câmera dá um zoom dramático no rosto da Yuna e surge na tela um **Painel Horizontal Cinematográfico Estilo Anime (`eyes-power-v1`)** atravessando o centro da tela com bordas diagonais iluminadas e raias de energia coloridas, mostrando em close-up os dois olhos da Yuna brilhando com determinação mágica!
2. **`Frames 78 a 94` (`1.3s a 1.56s`) — Puxada e Disparo:**
   - O painel de close-up se fecha rapidamente devolvendo a visão panorâmica da arena; Yuna puxa a lâmina (`frame 88`) e desfere o corte supremo (`frame 94`).
3. **`Frames 94 a 118` (`1.56s a 1.96s`) — O Raio Arco-Íris:**
   - Um feixe colossal de 7 faixas de luz arco-íris com núcleo branco incandescente dispara da ponta da espada atravessando a arena até atingir o chefe no `frame 118`, gerando ondas de choque concêntricas multicoloridas!
4. **Regra de Dano do Especial:**
   - **Contra o Trenzinho, o Robô ou o Dinossauro:** O Especial **derrota o chefe instantaneamente (`HP vai a 0`)**, não importa quanta vida ele tinha!
   - **Regra Especial contra o Urso (Chefe Final):** O Urso **nunca morre pelo Especial**! Se o Especial for usado no Urso, a vida dele cai imediatamente para **exatamente `1 coração` (`hp = 1`)** e ele entra imediatamente em estado de **Fúria (`enrage = true`)**, obrigando a jogadora a conquistar o último golpe na espada!

---

## 4. Os 4 Chefes da Arena Imaginária (Em Ordem de Batalha)

### Chefe 1/4: Trenzinho Real a Vapor (`train` — Nome no HUD: `Locomotiva de Cerco`)
- **Vida Inicial:** `3 Corações` (ou `2` na versão base sem apresentação estendida).
- **Visual 3D:** O trenzinho de madeira e ferro cresceu para o tamanho de uma locomotiva a vapor viva, com chaminé fumegante, farol frontal aceso e **bielas mecânicas nas rodas que giram em círculo perfeito** conforme ele avança e recua pelos trilhos imaginários da arena.
- **Comportamento e Ataques:**
  - Não possui esquiva adaptativa nem guarda complexa; é o chefe introdutório perfeito.
  1. **`Steam Blast` (Jato de Vapor):** A locomotiva apita, acumula pressão na caldeira (`54 frames` de aviso) e dispara uma densa rajada horizontal de nuvens de vapor fervente rente ao chão (`18 frames` ativos). *Como evitar:* Pular por cima do vapor ou recuar defendendo.
  2. **`Front Charge` (Investida Frontal):** As rodas giram em falso soltando faíscas e a locomotiva avança com tudo dando um encontrão com o limpa-trilhos frontal. *Como evitar:* Pular sobre a locomotiva ou defender recuando e contra-atacar durante os `36 frames` em que ela freia.

### Chefe 2/4: Robô Estelar Faísca (`robot` — Nome no HUD: `Sentinela de Lata`)
- **Vida Inicial:** `4 Corações`.
- **Visual 3D:** Um imponente robô retrô de lata azul-metálico, rebites cromados, visor de LED vermelho/amarelo brilhante, antena giratória no topo da cabeça e braços acoplados a grandes molas metálicas espirais com luvas de boxe vermelhas nas pontas.
- **IA Adaptativa e Esquiva:** A partir do Robô, a IA observa os hábitos da jogadora (`BEHAVIOR_MEMORY`: se ela pula muito, ataca demais ou fica longe) e pode executar um **Recuo Rápido (`EVADE`)** quando vê Yuna preparando um golpe de espada, respondendo imediatamente com um contra-ataque!
- **Ataques:**
  1. **`Laser Beam` (Raio Laser Alto):** O visor do robô carrega energia vermelha e dispara um feixe laser horizontal contínuo na altura da cabeça/peito da Yuna (`altura = 85 a 145` acima do chão).
     - *Como evitar:* **Abaixar (`crouch`)** — o feixe passa zunindo por cima do laço da Yuna sem acertá-la! (Pular faz Yuna ser atingida pelo feixe).
  2. **`Rocket Punch` (Soco-Foguete com Mola):** O robô recolhe o torso e dispara a luva de boxe presa à mola metálica em linha reta baixa/média até a distância onde Yuna está (`frames 44 a 90`), recolhendo a mola em seguida.
     - *Como evitar:* **Pular** por cima do braço esticado ou defender recuando (se Yuna ficar agachada parada, o punho baixo pode acertá-la).

### Chefe 3/4: Dinossauro (`dinosaur` — Nome no HUD: `Dragão de Brinquedo`)
- **Vida Inicial:** `4 Corações`.
- **Visual 3D:** Um feroz e carismático T-Rex / Dragão articulado de brinquedo verde-esmeralda com escamas dorsais laranja, olhos amarelos brilhantes e mandíbula articulada.
- **IA Adaptativa:** Alterna agressivamente entre curta e média distância, esquivando de cortes previsíveis e punindo pulos precipitados.
- **Ataques:**
  1. **`Bite` (Mordida Voraz):** Golpe de curto alcance em que o dinossauro abre a bocarra e dá uma abocanhada rápida à frente.
  2. **`Rush` (Investida Selvagem em 6 Poses):** Ataque de pressão e antiaéreo (`antiJump: 0.75`). O dinossauro abaixa a cabeça com o rabo esticado (2 poses de preparação), dispara numa corrida feroz rasgando a arena (2 poses ativas) e derrapa ao parar (2 poses de recuperação). *Como evitar:* Defender com firmeza ou saltar no tempo exato e punir a derrapagem.

### Chefe 4/4 (Chefe Final da Arena): O Urso (`bear` — Nome no HUD: `Guardião de Pelúcia`)
- **Vida Inicial:** `8 Corações` (o dobro de resistência dos anteriores!).
- **Visual 3D:** O Ursinho Pipoca transformado pela imaginação num gigantesco **Guardião de Pelúcia** de 2,5 metros de altura relativo à Yuna, com pelagem densa em 3D, olhos de botão reluzentes e presença pesada que faz o chão vibrar a cada passo.
- **Estado de Fúria (`ENRAGE — O GUARDIÃO DESPERTOU`):**
  - Quando a vida do Urso cai para **metade ou menos (`<= 4 HP`)** — ou quando ele é atingido pelo Especial da Espada Arco-Íris — ele ruge, ganha uma aura ardente de fúria (`enrage = true`), sua velocidade de esquiva aumenta em `10%`, o tempo de recarga das esquivas diminui em `20%` e o subtítulo da arena anuncia: **`O GUARDIÃO DESPERTOU`**.
- **Ataques:**
  1. **`Bear Hug` (Abraço de Urso):** Ataque pesado de curta distância em que o Urso abre os braços enormes e fecha um agarrão/impacto esmagador à sua frente.
  2. **`Belly Charge` (Barrigada Implacável):** O Urso estufa o peito/barriga e avança cobrindo grande distância da arena com enorme pressão física.
  3. **`Toy Leap` (Salto Sísmico de Pelúcia):** O ataque mais perigoso e antiaéreo (`antiJump: 1.0`, janela total estendida até o `frame 96`). O Urso salta alto em arco parabólico sobre a arena para esmagar Yuna de cima para baixo, gerando impacto no pouso! *Como evitar:* Passar por baixo do arco do salto ou recuar em defesa antes do impacto e contra-atacar no pouso.

---

## 5. HUD da Arena Imaginária e Tela de Derrota

### 5.1. Layout do HUD Superior
- **Barra de Vida da Jogadora (Esquerda, cor Dourada `#f4ca75`):**
  - Rótulo acima da barra: `MENINA · A ESPADA DA IMAGINAÇÃO` (ou `MENINA` em telas estreitas).
  - Indicador abaixo da barra: `ESPECIAL · R / Y · 1 USO` (em azul-claro `#bfeaff`, mudando para `ESPECIAL · USADO` em cinza `#b7aebf` após o uso).
- **Barra de Vida do Chefe (Direita, preenchimento reverso da direita para a esquerda, cor Rosa-Carmim `#cb748b`):**
  - Rótulo acima da barra:
    - Chefe 1: `Locomotiva de Cerco`
    - Chefe 2: `Sentinela de Lata`
    - Chefe 3: `Dragão de Brinquedo`
    - Chefe 4: `Guardião de Pelúcia`
- **Subtítulo Dinâmico Central (entre as barras, informando o estado tático em tempo real):**
  - Padrão: `O QUARTO LENDÁRIO · 1 / 4` (atualizando para `2 / 4`, `3 / 4`, `4 / 4`).
  - Na entrada do Urso: `O GUARDIÃO DE PELÚCIA`.
  - Durante troca de chefes: `OS BRINQUEDOS GANHAM VIDA`.
  - Durante o Especial: `ESPADA ARCO-ÍRIS`.
  - Quando Yuna defende: `DEFESA · ESPADA À FRENTE`.
  - Quando o chefe prepara contra-ataque: `PREPARANDO RESPOSTA · AGUARDE` ou `RESPOSTA PROTEGIDA · ABAIXE OU DEFENDA` (para Laser/Rocket Punch) / `RESPOSTA PROTEGIDA · PULE OU DEFENDA`.
  - Quando o chefe termina o golpe e fica vulnerável: `ABERTURA · CONTRA-ATAQUE`.
  - Quando o Urso entra em fúria: `O GUARDIÃO DESPERTOU`.

### 5.2. Derrota na Arena (`DEFEAT`)
- Se Yuna perder seus 3 corações (`player.hp === 0`), ela cai sentada/deitada no tapete e surge uma faixa escura translúcida no centro da tela:
  - **Título:** `Sua aventura ainda não acabou.`
  - **Subtítulo:** `Pressione ESPADA para tentar novamente.`
  - (E se for a 3ª derrota no modo Normal, exibe o diálogo global `deseja tentar no fácil?`).

---

## 6. Vitória sobre o Urso e Transição para a Perseguição 3D

- Quando Yuna acerta o golpe final que zera o último coração do **Guardião de Pelúcia (`bear.hp === 0`)**:
  - Diferente dos 3 chefes anteriores (que voavam direto de volta para o baú), **o Urso não volta sozinho para o baú**!
  - Ele cai sentado/deitado no chão da sala (`mode = 'VICTORY'`), voltando à aparência de ursinho de pelúcia no quarto real, e a cena corta diretamente para a abertura do **Capítulo 1 — Parte 4: A Perseguição do Urso (Bear Chase)**.
