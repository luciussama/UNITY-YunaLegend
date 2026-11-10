# 02 — Capítulo 1 (Parte 2): A Sala de Brinquedos — Exploração e Mistério (Unity 3D)

> **Instrução de Implementação (White Room — Unity 3D):**
> Esta fase ocorre imediatamente após Yuna e Nanda atravessarem o Verdadeiro Portal dos Sonhos no Quarto Escuro.
> Diferente do Quarto Escuro noturno e vertical, **A Sala de Brinquedos** é um ambiente 3D claro, amplo, aconchegante e banhado pela luz dourada da manhã.
> **Câmera e Movimentação:** Na exploração da sala, utilize uma **câmera 3D em perspectiva elevada / diorama** (ângulo de ~30° a 40° inclinada para baixo, estilo *Luigi's Mansion 3* ou *Link's Awakening*), onde Yuna caminha livremente pelo chão do quarto nos eixos horizontais (**X e Z** — largura e profundidade), desviando dos móveis 3D, recolhendo os brinquedos espalhados e levando-os até o Baú de Brinquedos.

---

## 1. Direção Visual 3D e Atmosfera da Sala de Brinquedos

- **Ambiente 3D (`1672 x 1200` unidades de área útil do quarto):**
  - Um amplo salão de brinquedos infantil iluminado pelo sol da manhã.
  - **Paredes e Janelas (Fundo / Norte):** Paredes em tom creme-rosado suave com lambris de madeira clara. Duas grandes janelas ensolaradas deixam entrar feixes de luz matinal dourada sobre o piso.
  - **Piso:** Assoalho de madeira clara envernizada com um grande tapete central macio (tons pastel de creme, rosa-antigo e sálvia) onde Yuna pisa.
  - **Atmosfera:** Acolhedora, mágica e inicialmente tranquila — mas que gradualmente ganha um ar de mistério conforme os brinquedos guardados começam a reaparecer sozinhos.

---

## 2. Cinemática de Introdução da Sala (`ToyRoomIntroduction` — 23.3 segundos)

Logo após o *Portal Wipe* dourado que encerra o Quarto Escuro, inicia-se a cena de chegada à Sala de Brinquedos, com planos na altura dos olhos da Yuna e da Nanda sobre o tapete da sala:

### Cronograma Cena a Cena (`TR_002` a `TR_008`):
1. **`TR_002` (`0.8s`) — Saída do Portal:**
   - Som de travessia mágica (`portalExitWhoosh` + `softMagicBurst`). O clarão dourado se dissipa revelando a Sala de Brinquedos cheia de brinquedos espalhados por todo o chão.
2. **`TR_003` (`2.5s`) — A Chegada no Tapete:**
   - Yuna dá alguns passos para dentro da sala até parar sobre o tapete central, olhando ao redor admirada e um pouco confusa com a quantidade de brinquedos espalhados. Nanda flutua ao lado dela.
3. **`TR_004` (`4.0s`) — A Constatação da Bagunça:**
   - A câmera aproxima suavemente no rosto da menina (`zoom 1.42 -> 1.48`).
   - **Caixa de Diálogo (`MENINA`, expressão `normal`):**
     > *"Aqui também..."*
4. **`TR_005` (`4.0s`) — O Desejo de Brincar:**
   - Nanda voa observando a sala ao redor e retorna à altura do olhar da Yuna.
   - **Caixa de Diálogo (`MENINA`, expressão `normal`):**
     > *"Eu queria espaço pra brincar."*
5. **`TR_006` (`7.0s`) — Duas Narrações Poéticas Manuscritas:**
   - **De `0.0s a 3.1s` — Quadro de Narração Poética 1 (`POETIC_LINES.toyRoomPlay`):**
     > *"A sala estava cheia.*
     > *Nenhum lugar esperava por ela."*
   - **De `3.1s a 3.9s`:** Breve respiro visual silencioso (`0.8s`).
   - **De `3.9s a 7.0s` — Quadro de Narração Poética 2 (`POETIC_LINES.toyRoomPlace`):**
     > *"A caixa tinha um fundo.*
     > *O resto está vazio."*
6. **`TR_007` (`2.0s`) — A Ideia da Nanda:**
   - Nanda bate as asas mais rápido, animada, apontando para o grande Baú de Brinquedos encostado na parede do fundo.
   - **Caixa de Diálogo (`NANDA`, expressão `happy`):**
     > *"Vamos. Um de cada vez!"*
7. **`TR_008` (`3.0s`) — Abertura do Plano e Música:**
   - A música alegre e curiosa da Sala de Brinquedos entra em *fade-in* gradual de 3 segundos enquanto a câmera abre para o enquadramento de exploração da sala.

---

## 3. Catálogo Completo de Móveis 3D da Sala (7 Móveis com Colisão)

Os 7 móveis 3D abaixo compõem a arquitetura navegável da sala (coordenadas referenciais em um espaço de `1600 x 1200`, onde `Y` menor é mais próximo da parede do fundo/norte e `Y` maior é mais próximo da frente/sul da sala):

| ID | Tipo / Nome do Móvel 3D | Posição `(X, Z/Y)` e Dimensões `(L x P)` | Função e Descrição Visual 3D para Unity |
| :--- | :--- | :--- | :--- |
| `toy-chest` | **Baú de Brinquedos (Alvo Principal)** | `X: 1040, Y: 190` (`190 x 115`) | Grande baú de madeira arqueado com ferragens douradas encostado próximo à parede norte. Possui **tampa articulada (`lidOpen`)** que abre suavemente quando Yuna se aproxima carregando um brinquedo e emite um **brilho mágico dourado (`glowAlpha`)** no interior. |
| `bookshelf` | **Estante de Livros** | `X: 380, Y: 170` (`220 x 90`) | Estante baixa de madeira na parede norte, repleta de livros infantis coloridos e pequenos enfeites. |
| `wardrobe` | **Guarda-Roupa** | `X: 740, Y: 170` (`140 x 90`) | Armário clássico de duas portas em madeira clara com entalhes delicados, posicionado entre a estante e o baú. |
| `rocking-horse` | **Cavalinho de Pau** | `X: 480, Y: 420` (`100 x 80`) | Cavalinho de balanço esculpido em madeira branca e sela vermelha sobre arcos curvos no meio-esquerdo da sala. |
| `table` | **Mesinha Infantil** | `X: 620, Y: 780` (`160 x 110`) | Mesa redonda/oval de madeira para chá e desenho no centro-sul da sala. |
| `armchair` | **Poltrona Aconchegante** | `X: 180, Y: 840` (`130 x 120`) | Poltrona estofada macia no canto inferior esquerdo, com almofada bordada. |
| `block-fortress` | **Fortaleza de Blocos** | `X: 1240, Y: 800` (`220 x 170`) | Uma grande construção de blocos de madeira coloridos e torres no canto inferior direito da sala. |

---

## 4. Catálogo dos 8 Brinquedos Interativos e Mecânica de Peso

Espalhados pelo chão da sala estão **8 brinquedos 3D interativos**. Cada brinquedo possui um fator de peso (`weight`, que multiplica a velocidade base da Yuna ao carregá-lo: quanto menor o número, mais pesado o brinquedo e mais lento o passo da menina):

| ID | Nome Oficial (pt-BR) | Posição Inicial `(X, Y)` | Fator de Peso (`weight`) | Descrição do Modelo 3D na Unity |
| :--- | :--- | :--- | :--- | :--- |
| `teddy` | **Ursinho Pipoca** | `X: 340, Y: 480` | `0.94` (Leve) | Ursinho de pelúcia caramelo fofo sentado, com gravatinha borboleta vermelha. |
| `train` | **Trenzinho Real a Vapor** | `X: 760, Y: 520` | `0.90` (Médio-Pesado) | Locomotiva de brinquedo em madeira e metal pintado de vermelho, preto e dourado com chaminé. |
| `robot` | **Robô Estelar Faísca** | `X: 1140, Y: 540` | `0.92` (Médio) | Robozinho retrô de lata azul-metálico e amarelo com anteninha no topo e visor luminoso. |
| `bunny` | **Coelhinho de Algodão** | `X: 460, Y: 980` | `0.96` (Muito Leve) | Coelhinho branco felpudo de orelhas compridas caídas e narizinho rosa. |
| `duck` | **Patinho de Banho Imperial** | `X: 940, Y: 840` | `0.95` (Leve) | Patinho amarelo rechonchudo usando uma pequena coroa dourada. |
| `blocks` | **Torre de Blocos Coloridos** | `X: 1180, Y: 720` | `0.85` (O Mais Pesado) | Conjunto empilhado de blocos de madeira maciça pintados em vermelho, azul, amarelo e verde. |
| `drum` | **Tamborzinho Encantado** | `X: 640, Y: 960` | `0.93` (Médio-Leve) | Tambor infantil cilíndrico vermelho e marfim com cordas douradas laterais. |
| `jack` | **Caixinha de Surpresa da Fada** | `X: 880, Y: 360` | `0.91` (Médio) | Caixa de música/surpresa cúbica colorida com manivela dourada e estrela estampada. |

### Mecânica de Coleta e Guarda no Baú:
1. **Pegar Brinquedo:** Quando Yuna se aproxima de um brinquedo no chão e não está segurando nada, ela pode pegá-lo (ou pega ao tocar/acionar o botão de ação). Ela segura o brinquedo 3D com as duas mãozinhas à frente do corpo (`isCarried = true`), e o brinquedo oscila levemente (`bounceOffset`) conforme os passos dela.
2. **Orientação da Nanda:** Assim que Yuna segura um brinquedo, Nanda voa diretamente para cima do **Baú de Brinquedos (`toy-chest`)**, brilhando como um farol para indicar onde guardá-lo.
3. **Guardar no Baú:** Ao encostar na frente do Baú de Brinquedos, a tampa do baú se abre com um brilho dourado, o brinquedo faz um pequeno arco entrando no baú (`isOrganized = true`), toca um efeito sonoro cristalino de entrega e a tampa se fecha.

---

## 5. O Mistério da Recorrência ("Os Brinquedos Estão Voltando!")

A grande surpresa narrativa desta fase é que **a tarefa de arrumar o quarto nunca termina normalmente**, porque uma força mágica da imaginação está trazendo os brinquedos de volta:

### Regras Exatas da Recorrência (`ToyRoomStory.js`):
1. **Gatilho de Ativação:**
   - Quando restam **2 ou menos brinquedos** espalhados na sala (`toys.filter(!isOrganized).length <= 2`), o sistema ativa silenciosamente a recorrência (`recurrenceActive = true`).
2. **Como os Brinquedos Retornam:**
   - A partir desse momento, cada vez que Yuna guarda um brinquedo no baú, ele entra numa fila de retorno com atraso de **`90 frames` (`1.5 segundo`)**.
   - Quando o timer zera, o jogo escolhe um ponto livre do piso que esteja **fora do campo de visão atual da câmera (`outsideViewport`)** e longe da Yuna, e recoloca o brinquedo lá no chão silenciosamente!
3. **Primeiro Estranhamento (Após 2 Brinquedos Retornados Guardados Novamente):**
   - Quando a contagem de brinquedos que já tinham voltado e foram guardados de novo atinge **`returnedToyCount === 2`**, dispara um diálogo automático durante a exploração (`RETURN_DIALOGUE_BEATS`, duração de 6 segundos / 360 frames):
   - **De `0.0s a 1.0s` — Caixa de Diálogo (`NANDA`, expressão `normal`):**
     > *"Estranho..."*
   - **De `1.5s a 6.0s` — Caixa de Diálogo (`MENINA`, expressão `normal`):**
     > *"Acho que esse brinquedo já tinha sido guardado."*

---

## 6. Cutscene de Investigação e Revelação da Espada de Brinquedo

Quando Yuna guarda o **4º brinquedo retornado** (`returnedToyCount >= 4`) e não está segurando nada nas mãos, o controle de movimento é pausado e inicia-se a **Sequência de Investigação (`INVESTIGATION_BEATS`)**:

### Cronograma Exato dos 10 Beats da Investigação:
1. **Beat 1 (`3.0s` / 180 frames — Foco nos brinquedos):**
   - Nanda voa em órbita observando os brinquedos que reapareceram no chão.
   - **Caixa de Diálogo (`NANDA`):**
     > *"Tem alguma coisa estranha..."*
2. **Beat 2 (`1.0s` / 60 frames — Pausa visual):**
   - Silêncio breve enquanto Nanda inspeciona outro brinquedo.
3. **Beat 3 (`4.0s` / 240 frames — Foco nos brinquedos):**
   - **Caixa de Diálogo (`MENINA`):**
     > *"Estamos vendo os mesmos brinquedos de novo."*
4. **Beat 4 (`6.0s` / 360 frames — Narração Poética Manuscrita):**
   - **Quadro de Narração Poética (Sem locutor):**
     > *"Os brinquedos continuavam voltando.*
     > *Como se o quarto esquecesse que alguém tinha acabado de guardá-los."*
5. **Beat 5 (`2.0s` / 120 frames — Transição de Foco):**
   - Nanda sente uma presença diferente e voa em direção ao piso **à direita do Baú de Brinquedos** (logo abaixo da janela direita, em `X: chest.x + chest.w + 130`, `Y: chest.y + chest.h + 25`).
6. **Beat 6 (`5.0s` / 300 frames — Revelação da Espada!):**
   - **A câmera move seu alvo para acompanhar a fadinha Nanda (`cameraTarget = fairy`)**.
   - No chão ensolarado à direita do baú, **surge brilhando a Espada de Brinquedo (`sword.visible = true`)**: uma linda espadinha infantil de madeira dourada/clara (`#e8c990`), guarda-mão azul-turquesa arredondado (`#83c5c8`) e cabo coral/terracota (`#c77768`).
   - **Quadro de Narração Poética (Sem locutor):**
     > *"Foi então que uma coisa nova apareceu.*
     > *Ou talvez estivesse ali desde sempre."*
7. **Beat 7 (`1.5s` / 90 frames):**
   - **Caixa de Diálogo (`MENINA`):**
     > *"Hm?"*
8. **Beat 8 (`3.0s` / 180 frames):**
   - Nanda paira diretamente acima da espada (`80 unidades` acima dela), apontando-a.
   - **Caixa de Diálogo (`NANDA`):**
     > *"Que brinquedo é esse?"*
9. **Beat 9 (`2.5s` / 150 frames):**
   - **Caixa de Diálogo (`MENINA`):**
     > *"Eu não lembro dele."*
10. **Beat 10 (`2.5s` / 150 frames):**
    - **Caixa de Diálogo (`NANDA`):**
      > *"E eu lembraria."*

---

## 7. Coleta da Espada de Brinquedo e Transição para a Arena

1. **Busca da Espada (`SEEKING_SWORD`):**
   - Ao terminar o Beat 10, a câmera volta para a Yuna e o jogador reassume o controle.
   - Os brinquedos comuns não podem mais ser pegos (`canCollectToys = false`). Nanda permanece pairando sobre a Espada de Brinquedo à direita do baú, chamando Yuna.
2. **A Coleta e Pose Heroica (`PICKUP` — 5.0 segundos / 300 frames):**
   - Assim que Yuna caminha até a Espada de Brinquedo e encosta nela:
   - Toca o efeito sonoro especial de coleta e **a trilha musical muda para o Tema da Espada (`useSwordTrack`)**.
   - Yuna ergue a Espada de Brinquedo com orgulho. Durante os 5 segundos desta cena, surgem **dois quadros de Narração Poética Manuscrita**:
     - **De `0.0s a 2.0s` (`0 a 120 frames`):**
       > *"Ela não estava procurando por aquilo."*
     - **De `2.0s a 5.0s` (`120 a 300 frames`):**
       > *"Mas algumas coisas parecem encontrar a gente primeiro."*
3. **Gatilho para a Transformação da Arena (`EQUIPPED`):**
   - Assim que a coleta termina (`swordEquipped = true`), o quarto inteiro começa a responder à imaginação da Yuna, iniciando imediatamente o **Capítulo 1 — Parte 3: A Arena Imaginária**.
