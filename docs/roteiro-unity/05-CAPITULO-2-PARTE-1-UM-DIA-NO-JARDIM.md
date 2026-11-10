# 05 — Capítulo 2 (Parte 1): Um Dia no Jardim (Unity 3D Side-Scroller)

> **Instrução de Implementação (White Room — Unity 3D):**
> O Capítulo 2 leva Yuna e Nanda para fora de casa em uma manhã ensolarada de piquenique.
> Construa esta fase como um **Plataforma 3D Lateral (2.5D) ao ar livre**, com ~5 minutos de travessia da esquerda para a direita (`X = 0` até `X = 53.000+` unidades), com vegetação rica em camadas 3D, riachos de água cristalina com refração e peixinhos/plantas aquáticas, rodas d'água de madeira giratórias, nuvens fofas no alto e uma mecânica central de **sensibilidade sensorial aos terrenos** (inspirada na percepção intensa de uma criança neurodivergente/sensível diante de texturas, sons e insetos do jardim).

---

## 1. Cinemática de Abertura: O Portão de Casa e a Revelação do Nome (`GARDEN_OPENING` — 50 segundos)

- **Cenário 3D da Abertura (`house-without-actors`):**
  - A porta da varanda de madeira de casa acaba de se abrir para o jardim iluminado pelo sol da manhã (`GARDEN_HOME` em `X = 30 a 530`).
  - Há uma pequena escada de madeira de 4 degraus (`X: 338, Y: 540` $\rightarrow$ `X: 432, Y: 546` $\rightarrow$ `X: 476, Y: 574` $\rightarrow$ `X: 530, Y: 590`) que desce da varanda até o chão do jardim.
  - Yuna está parada no topo do degrau banhado de sol, olhando para a imensidão viva do jardim, enquanto Nanda voa animada à sua frente.

### Cronograma Exato dos 9 Quadros de Abertura (`gardenNarrative.js` — Fonte Manuscrita em Quadro Pastel):

1. **Quadro 1 (`5.0 segundos` — Pose 0: Yuna parada na porta olhando o sol):**
   - **Cabeçalho:** `UM DIA NO JARDIM`
   - **Texto:**
     > *"A porta se abriu para uma manhã cheia de coisas.*
     > *O sol aquecia o último degrau."*
2. **Quadro 2 (`5.0 segundos` — Pose 0, leve aproximação):**
   - **Cabeçalho:** `NANDA`
   - **Texto (Primeira vez em toda a campanha que o nome de Yuna é revelado!):**
     > *"Vamos, Yuna, vamos no piquenique, vai ser divertido!"*
3. **Quadro 3 (`5.0 segundos` — Pose 0):**
   - **Cabeçalho:** `UM PRIMEIRO PASSO`
   - **Texto:**
     > *"Yuna.*
     > *Seu nome atravessou a porta antes dos seus pés."*
4. **Quadro 4 (`6.0 segundos` — Pose 0):**
   - **Cabeçalho:** `UM PRIMEIRO PASSO`
   - **Texto:**
     > *"Ela queria o piquenique, os amigos, o pão quentinho.*
     > *Mas o jardim chegava todo de uma vez."*
5. **Quadro 5 (`8.0 segundos` — Pose 1: Yuna encolhe levemente os ombros, sentindo o excesso de estímulos):**
   - **Cabeçalho:** `UM PRIMEIRO PASSO`
   - **Texto:**
     > *"A grama roçava suas pernas. As pedrinhas faziam cada passo parecer diferente.*
     > *Um inseto levantou voo, e seus ombros subiram junto."*
6. **Quadro 6 (`5.0 segundos` — Pose 1):**
   - **Cabeçalho:** `UM PRIMEIRO PASSO`
   - **Texto:**
     > *"Dentro dela, a alegria dava pulinhos.*
     > *O medo também."*
7. **Quadro 7 (`5.0 segundos` — Pose 1):**
   - **Cabeçalho:** `YUNA`
   - **Texto:**
     > *"Eu quero ir… mas o jardim faz tanta coisa ao mesmo tempo."*
8. **Quadro 8 (`5.0 segundos` — Pose 1: Nanda se aproxima com carinho):**
   - **Cabeçalho:** `NANDA`
   - **Texto:**
     > *"Então vamos um pedacinho de cada vez. Eu vou com você."*
9. **Quadro 9 (`6.0 segundos` — Pose 2: Yuna respira fundo e dá o passo na escada):**
   - **Cabeçalho:** `UM PRIMEIRO PASSO`
   - **Texto:**
     > *"A vontade de ir e a vontade de ficar couberam no mesmo passo.*
     > *E ela desceu."*

- **Transição para o Gameplay:** Após o Quadro 9 (ou ao clicar em `Pular cena` caso já tenha sido vista), um breve *fade* claro (`0.45s`) corta para o plano lateral 2.5D com Yuna nos degraus da varanda, exibindo nos primeiros metros (`X < 500`) a dica:
  - **Cabeçalho:** `ATÉ O PIQUENIQUE`
  - **Texto:** `A/D ou ← → para andar • Espaço para pular` (ou `Use ◀ ▶ para andar e PULAR para saltar.` no mobile).

---

## 2. Mecânica Sensorial dos Terrenos (`GARDEN_TERRAINS`)

No Jardim, **o material onde Yuna pisa altera diretamente sua velocidade, sua postura corporal 3D e seu equilíbrio**, traduzindo em mecânica de jogo a sensibilidade sensorial da personagem:

| Tipo de Terreno (`type`) | Velocidade Horizontal (`speed`) | Impulso de Salto (`jump`) | Comportamento Físico e Atuação 3D da Yuna |
| :--- | :--- | :--- | :--- |
| **Madeira (`wood`), Terra (`soil`), Pedra de Rio (`stone`), Nuvem (`cloud`)** | `220 unid./s` (Padrão) | `720` | Caminhada e corrida naturais, passo firme e confortável. |
| **Grama Alta (`grass`)** | **`100 unid./s` (Lenta — 45% da velocidade)** | `720` | As folhas de grama alta (renderizadas também em camada frontal 3D à frente das canelas da Yuna) roçam nas pernas dela. Yuna acumula `discomfort`, anda devagarinho com os bracinhos encolhidos e expressão de incômodo sensorial (`tall_grass_walk`). |
| **Pedrinhas / Cascalho (`gravel`)** | **`410 unid./s` (Quase o dobro da velocidade!)** | `720` | As pedrinhas soltas fazem cada passo parecer instável: Yuna dispara numa corrida rápida e agitada tentando manter o equilíbrio, balançando o tronco (`rotate(sin(t * 11) * 0.045)`), com **parada imediata ao soltar o controle** e poses de equilíbrio na beira (`balancePose`). |

### Elementos Especiais de Plataforma do Jardim:
1. **Riachos com Pedras de Travessia (`river`):**
   - Correntes de água clara (`waterY = 650`) com peixinhos e plantas submersas em 3D. Sobre a água há sequências de `3 a 5` pedras arredondadas (`stone`, largura `160`, espaçamento `110 a 130`). Cair na água é falha imediata.
2. **Rodas de Madeira Giratórias (`GARDEN_WHEELS` — 4 Rodas no total):**
   - Grandes rodas de moinho/jardim em madeira com **4 plataformas horizontais acopladas** que giram continuamente em órbita elíptica (`rx = 150, ry = 115`, velocidade angular `~0.38 rad/s`).
   - Há 1 roda na seção inicial (`X = 5050`) e **3 rodas em sequência na seção aérea** ("Por cima do jardim", sendo a do meio em sentido anti-horário `-0.36 rad/s`).
3. **Nuvens Temporárias (`cloudStatus`):**
   - Nuvens brancas fofas suspensas no ar.
   - Assim que Yuna pousa numa nuvem pela primeira vez, a nuvem "arma" um relógio e começa a **piscar cada vez mais rápido**, desaparecendo completamente após **`2.8 segundos` no Modo Normal (`3.8 segundos` no Modo Fácil)**!
   - A nuvem permanece invisível/sem colisão por `4.0 segundos` e depois reaparece intacta.

---

## 3. Criaturas do Jardim e os 3 Encontros de Pânico com o Besouro

Diferente da Arena Imaginária, no Jardim Yuna não usa espada. Ela lida com os insetos pulando sobre eles (estilo *Mario / Rayman*) ou desviando:

### 3.1. Os 4 Tipos de Inimigos Comuns:
1. **Formiga (`ant` — 1 HP):**
   - Formiga ruiva/castanha 3D que patrulha trechos de terra, grama e pedrinhas.
   - **Variante Formiga Saltadora do Rio (`formiga-do-rio`):** Nos rios a partir da 3ª região, há uma formiga que salta em arcos periódicos de pedra em pedra sobre a água!
   - **Derrota:** Basta **1 pisão (`stomp`)** por cima para derrotá-la (Yuna quica no ar ao pisar).
2. **Lagarta (`caterpillar` — 2 HP):**
   - Começa como uma lagarta verde tranquila (`2 HP`).
   - **Reação ao 1º Pisão:** Ao receber o primeiro pisão (`hp cai para 1`), ela **muda de cor para Vermelha (`lagartaVermelha`)**, fica zangada e **acelera a patrulha/investida** (`animação passa de 7 Hz para 12 Hz`)!
   - **Derrota:** Exige um **2º pisão** para ser derrotada definitivamente.
3. **Abelha em Rasante (`bee` — 1 HP, 5 abelhas ao longo da fase):**
   - Paira no ar a `180 unidades` acima do chão em um ponto `A` (`homeX`).
   - Quando Yuna chega a menos de `320 unidades` de distância, a abelha faz **`0.5s` de preparação (`prepare`)** apontando o corpo e então executa um **voo rasante parabólico (`dive` de `1.25s`)** que desce quase até raspar o chão no meio do caminho e sobe até estacionar no ponto `B` (`otherX`, a `360 unidades` de distância), alternando entre `A` e `B` após `1.8s` de recarga.
   - **Derrota:** Pode ser derrotada com **1 pisão** por cima durante o voo baixo, ou evitada passando no tempo certo.
4. **Minhoca das Pedrinhas (`worm` — Obstáculo Vivo Indestrutível, 4 minhocas na fase):**
   - Escondida sob os trechos de cascalho (`gravel`).
   - Quando Yuna se aproxima (`distância < 500`), pequenas pedrinhas começam a tremer no chão como aviso; quando chega a `< 280 unidades`, a minhoca **emerge rapidamente do solo (`0.22s`)** até `72 unidades` de altura, funcionando como um espinho vivo (não pode ser pisada; Yuna deve saltar por cima dela!). Quando Yuna se afasta (`> 360`), ela se recolhe para baixo da terra.

### 3.2. Os 3 Encontros de Pânico com o Besouro (`GARDEN_BEETLES`)
Um momento único de gameplay e empatia narrativa: em **3 lugares específicos da fase**, surge voando atrás da Yuna um **Besouro Rinoceronte/Jardim (`beetle`)**.
- **A Verdade sobre o Besouro:** O besouro **é totalmente inofensivo e não causa dano**, mas Yuna tem medo do barulho das asas dele!
- **Mecânica do Pânico (`panic`):**
  - Assim que Yuna pisa no gatilho do encontro, o besouro entra voando pela esquerda e paira atrás dela (`135 unidades` atrás e `70` acima), enquanto gotinhas de suor de susto saltam da cabeça da Yuna.
  - **Corrida Automática de Susto:** Assustada, **Yuna passa a correr sozinha para a frente (`vx` automático para a direita)** com uma animação exclusiva de pânico (`FEAR_FRAMES`, olhando assustada para trás enquanto corre)!
  - **Controle da Jogadora:** Enquanto Yuna corre sozinha, **o jogador controla apenas o PULO** para fazê-la saltar sobre as pedras do riacho ou sobre as nuvens sem cair!
  - **Mensagem de Acolhimento da Nanda no Topo da Tela durante o Pânico:**
    - **Cabeçalho:** `NANDA`
    - **Texto:**
      > *"calma, Yuna, é só um besouro. Ele não vai te fazer mal"*
  - (Na primeira vez que o jogador vê ou falha num trecho de besouro, aparece também a dica com cabeçalho `UM SUSTO NO CAMINHO`: *"Quando Yuna se assusta, ela corre sozinha. Controle apenas o pulo."*).
- **Onde ocorrem os 3 Encontros com o Besouro e como terminam:**
  1. **`besouro-riacho-1`:** Na margem antes do riacho do **Trecho 0 (Canteiros de margaridas)**. Termina assim que Yuna salta as 3 pedras do rio e pousa em segurança na margem firme do outro lado (`trecho-0-clareira`).
  2. **`besouro-riacho-2`:** Na margem antes do riacho de 5 pedras do **Trecho 4 (Riachos de água clara)**. Termina ao alcançar a margem segura (`trecho-4-clareira`).
  3. **`besouro-nuvens-finais`:** Pouco antes da sequência final de **4 nuvens (`nuvem-final-0` a `3`)** que leva à Grande Árvore. Dura **`6.0 segundos`** enquanto Yuna salta pelas nuvens finais até a clareira do piquenique; ao terminar, o besouro vai embora voando para o alto à esquerda.

---

## 4. Mapa Completo das 9 Seções do Jardim e Sistema de Vidas/Resgate

### 4.1. As 9 Seções em Ordem de Travessia (`GARDEN_SECTIONS`)
O HUD no canto superior esquerdo mostra os corações (`♥ ♥ ♥` no Normal, `♥ ∞` no Fácil), a porcentagem (`0% a 100% do caminho`) e o nome poético da seção atual:

1. **`O portão de casa` (`X: 0 a 7350`):**
   - Degraus da varanda (`wood`), 2 plataformas suspensas de madeira, primeira faixa de grama alta (`primeira-grama`), primeiro riacho de 3 pedras, grama da margem, trecho de pedrinhas (`pedrinhas-da-roda`), a **1ª Roda Giratória (`X: 5050`)**, 3 nuvens temporárias iniciais e a clareira dos primeiros bichinhos (1 lagarta e 1 formiga).
2. **`Canteiros de margaridas` (`Trecho 0`):**
   - Caminho de terra em relevo, longo gramado com manchas de grama alta e colina suave, margem com o **1º Besouro (`besouro-riacho-1`)** sobre riacho de 3 pedras, e clareira com abelha em rasante.
3. **`O caminho das hortênsias` (`Trecho 2`):**
   - Caminho de terra, gramado ondulado de `2400 unid.`, trecho de pedrinhas com **Minhoca emergente** e clareira elevada com abelha.
4. **`Riachos de água clara` (`Trecho 4`):**
   - Caminho, gramado, margem com o **2º Besouro (`besouro-riacho-2`)** sobre um grande rio de **5 pedras** (com formiga saltadora de rio!), clareira e grande colina de terra.
5. **`Por cima do jardim` (`Seção Aérea` — `4360 unidades` de extensão):**
   - Uma travessia espetacular pelo alto das copas e do céu do jardim!
   - Começa com **3 Rodas de Madeira Giratórias em sequência (`aerea-1`, `aerea-2`, `aerea-3`)**, seguidas por uma travessia aérea alternando **6 Nuvens Temporárias (`aereo-nuvem-0` a `5`)** e **2 Ilhas Suspensas de Pedrinhas (`aereo-ilha-0` e `1`)**, até descer na saída aérea.
6. **`Entre salgueiros` (`Trecho 6`):**
   - Caminho, longo relevo de duas ondas com faixas de grama alta, pedrinhas com **Minhoca** e ampla clareira com abelha.
7. **`O jardim das borboletas` (`Trecho 8`):**
   - Caminho, pedrinhas com **Minhoca**, gramado extenso, rio de **4 pedras** com formiga saltadora e clareira com abelha.
8. **`A alameda da grande árvore` (`Trecho 10`):**
   - Caminho, gramado de duas ondas, rio de **5 pedras** com formiga saltadora e clareira final.
9. **`Reta Final das Nuvens e a Grande Árvore` (`GARDEN_FINAL_START` até `GARDEN_GOAL`):**
   - Patamar de descanso (`descanso-final`), o **3º Besouro (`besouro-nuvens-finais`)** conduzindo o salto pelas **4 Nuvens Finais** até a imensa clareira sob a **Grande Árvore (`arvore`, `1450 unidades`)**, onde a toalha de piquenique está estendida!

### 4.2. Diferença de Vidas e Resgate entre Normal e Fácil
- **No Modo NORMAL (`3 Corações`):**
  - Tocar num inseto tira `1 coração` (`2.0s` de imunidade). Se zerar os 3 corações ou cair na água/abismo, ocorre `Game Over`, retomando do último **Checkpoint de seção** (conservando todos os insetos já derrotados!).
- **No Modo FÁCIL (`♥ ∞ — Vida Infinita com Resgate da Nanda`):**
  - Yuna nunca sofre Game Over no Fácil!
  - Se cair na água ou no vazio (`RESCUING`), Nanda voa até ela e a carrega suavemente pelo ar de volta ao último chão firme seguro, exibindo o quadro:
    - **Cabeçalho:** `ESTOU AQUI` / **Texto:** *"Nanda leva você de volta ao chão seguro."*
  - Se tomar vários esbarrões seguidos de insetos (`DISORIENTED`), Nanda a recua um pouquinho até um patamar seguro anterior para respirar, exibindo o quadro:
    - **Cabeçalho:** `COM CALMA` / **Texto:** *"Um instante para se encontrar de novo…"*

---

## 5. Cinemática de Encerramento: O Piquenique e o Adormecer (`GARDEN_ENDING` — 87 segundos)

Quando Yuna chega à **Grande Árvore (`X = GARDEN_GOAL`)**, a fase é concluída (`stage = COMPLETE`) e corta para a belíssima cena do piquenique à sombra das raízes e da copa frondosa:
- **Cenário 3D do Piquenique (`picnic-without-yuna` + `GARDEN_PICNIC`):**
  - Uma toalha de piquenique xadrez estendida sobre a grama macia aos pés da Grande Árvore.
  - Sobre a toalha já estão esperando os amigos brinquedos (o Ursinho, o Coelhinho, o Patinho e o Robozinho) junto a uma cesta de vime, pratinhos com **pão na chapa quentinho** e copos de **suco de laranja** (incluindo um pãozinho minúsculo e um copinho miudinho do tamanho da mão da fadinha Nanda!).

### Cronograma Exato dos 14 Quadros Finais (`gardenNarrative.js`):

1. **Quadro 1 (`6.0s` — Panorâmica apresentando os brinquedos na toalha até chegar em Yuna):**
   - **Cabeçalho:** `UM LUGAR PARA DESCANSAR`
   - **Texto:**
     > *"Quando a toalha apareceu, Yuna soltou um suspiro comprido.*
     > *Tinha trazido o jardim inteiro nas pernas."*
2. **Quadro 2 (`6.0s` — Yuna senta-se na toalha):**
   - **Cabeçalho:** `UM LUGAR PARA DESCANSAR`
   - **Texto:**
     > *"Sentou-se devagar entre os amigos.*
     > *Por um instante, só ficou ali, sentindo o chão quietinho debaixo dela."*
3. **Quadro 3 (`5.0s` — Close suave):**
   - **Cabeçalho:** `YUNA`
   - **Texto:**
     > *"Cheguei… mas minhas pernas ainda estão correndo."*
4. **Quadro 4 (`5.0s`):**
   - **Cabeçalho:** `NANDA`
   - **Texto:**
     > *"Então deixa que elas descansem. Hoje eu sento aqui."*
5. **Quadro 5 (`8.0s` — Nanda pousa na toalha e senta dobrando as perninhas):**
   - **Cabeçalho:** `UM LUGAR PARA DESCANSAR`
   - **Texto:**
     > *"Nanda pousou ao seu lado e dobrou as perninhas.*
     > *Para ela havia um pão do tamanho de sua mãozinha e um copinho de suco miudinho."*
6. **Quadro 6 (`5.0s` — Yuna segura seu pão na chapa):**
   - **Cabeçalho:** `UM LUGAR PARA DESCANSAR`
   - **Texto:**
     > *"Um piquenique inteiro cabia naquele pedacinho de toalha.*
     > *Yuna segurou seu pão."*
7. **Quadro 7 (`4.0s`):**
   - **Cabeçalho:** `YUNA`
   - **Texto:**
     > *"O jardim foi bonito. E foi muito."*
8. **Quadro 8 (`3.0s`):**
   - **Cabeçalho:** `NANDA`
   - **Texto:**
     > *"Eu sei."*
9. **Quadro 9 (`6.0s` — Animação de Yuna dando mordidinhas no pão):**
   - **Cabeçalho:** `UM LUGAR PARA DESCANSAR`
   - **Texto:**
     > *"As folhas continuaram conversando lá no alto.*
     > *O pão estava morno. O suco, fresquinho."*
10. **Quadro 10 (`8.0s`):**
    - **Cabeçalho:** `UM LUGAR PARA DESCANSAR`
    - **Texto:**
      > *"Por enquanto, Yuna não precisava dar mais nenhum passo.*
      > *Ficou ali, comendo devagar, com a fada sentada ao seu lado."*
11. **Quadro 11 (`7.0s`):**
    - **Cabeçalho:** `MAIS UM PEDACINHO`
    - **Texto:**
      > *"Veio outro pão na chapa. E mais um gole de laranja.*
      > *Na toalha, a tarde foi ficando macia."*
12. **Quadro 12 (`7.0s` — Transição para a pose dormindo `sleepShot`):**
    - **Cabeçalho:** `A TARDE FECHOU OS OLHOS`
    - **Texto:**
      > *"A barriga ficou quentinha. As pálpebras, pesadas.*
      > *Yuna tentou guardar um último sorriso."*
13. **Quadro 13 (`7.0s` — Yuna e Nanda deitadas dormindo na toalha):**
    - **Cabeçalho:** `A TARDE FECHOU OS OLHOS`
    - **Texto:**
      > *"O sono chegou sem fazer barulho.*
      > *Nanda dobrou as asas e se aconchegou ao lado dela."*
14. **Quadro 14 (`7.0s` — Gancho Poético para o Jardim dos Sonhos):**
    - **Cabeçalho:** `UM JARDIM POR DENTRO`
    - **Texto:**
      > *"Lá fora, o jardim continuou do mesmo tamanho.*
      > *Dentro do sonho, uma folha já podia ser floresta."*

- Ao encerrar o Quadro 14, a campanha avança diretamente para o **Capítulo 2 — Parte 2: O Jardim dos Sonhos (Dream Forest)**.
