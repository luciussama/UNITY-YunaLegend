# 01 — Capítulo 1 (Parte 1): O Quarto Escuro (Unity 3D Side-Scroller)

> **Instrução de Implementação (White Room — Unity 3D):**
> Construa esta fase inteiramente como um **Plataforma 3D Lateral (2.5D)** no estilo de *Ori*, *Little Nightmares* (porém mágico e acolhedor, sem terror) e *Rayman Legends*.
> Yuna (vista da escala pequenina de um brinquedo/criança num quarto imenso) move-se no plano lateral (eixos X e Y), escalando móveis, pilhas de livros e brinquedos gigantes modelados em 3D, com várias camadas de profundidade (eixo Z) no cenário e iluminação dinâmica noturna.

---

## 1. Direção Visual 3D, Iluminação e Atmosfera do Quarto Escuro

- **Ambiente 3D:** Um quarto infantil vitoriano/clássico visto em escala monumental durante a noite. O piso é de tábuas de madeira escura corrida com rodapé alto esculpido. A parede de fundo traz papel de parede noturno em tom índigo/violeta-escuro com listras sutis e estrelas apagadas.
- **Elementos de Fundo 3D (Background com Paralaxe Natural):**
  - Grandes janelas altas com molduras de madeira por onde entra um feixe diagonal de luar azul-prateado volumétrico (*Volumetric Light Shafts* com partículas de poeira suspensas).
  - Cortinas pesadas de veludo azul-meia-noite amarradas nas laterais, quadros emoldurados na parede, estantes distantes desfocadas (*Depth of Field*) e silhuetas de móveis no plano de fundo.
- **Iluminação Dinâmica (Dark Atmosphere + Lights):**
  - O quarto está imerso numa penumbra azul-violeta profunda, mas acolhedora.
  - **Luzes Quentes Pontuais:**
    1. **Nanda (A Fadinha Guia):** Carrega uma luz pontual dourado-âmbar quente que ilumina dinamicamente a Yuna e a superfície da próxima plataforma onde ela pousa.
    2. **Halo Suave da Yuna:** Uma luz de preenchimento (*fill light*) muito sutil ao redor da menina para que sua silhueta e seu vestido creme com laço vermelho nunca sumam na escuridão.
    3. **Lâmpadas e Portais:** O Abajur Cogumelo (Plataforma 12), o Lustre de Cristais (Plataforma 17), a Porta Falsa e o Verdadeiro Portal dos Sonhos emitem brilhos próprios no espaço 3D.
- **Regra Visual Estrita:** Nenhuma plataforma 3D pode ter linhas brilhantes, faixas pintadas ou contornos de hitbox no topo. O jogador enxerga onde pisar pela própria superfície 3D dos livros, gavetas, tampos de madeira e brinquedos iluminados pela luz da fadinha Nanda.

---

## 2. Cinemática de Abertura ("O Despertar no Quarto Escuro")

- **Duração Total:** `42.5 segundos` (divididos em etapas sincronizadas).
- **Posição Inicial:** Yuna está deitada encolhida dormindo no chão de madeira (`X = 80`), no canto esquerdo do quarto escuro. A tela começa totalmente preta.

### Cronograma Cena a Cena da Abertura (`OpeningSequence.js` + `poeticNarrator.js`):
1. **`0.0s a 4.0s` — Fade-in na Penumbra e Chegada da Luz:**
   - A tela preta abre suavemente revelando o canto escuro do quarto. Yuna dorme profundamente encolhida no chão de madeira (`X = 80`), respirando devagar. Ainda não há música, apenas o som ambiente noturno do quarto.
   - Nanda (a fadinha dourada) surge voando na escuridão, deixando um rastro de pó brilhante, e pousa/paira perto da menina adormecida, iluminando o rosto da Yuna com sua luz quente.
2. **`4.0s a 9.0s` — Primeira Chamada da Fada:**
   - Nanda voa em pequenos círculos tentando acordar Yuna.
   - **Caixa de Diálogo (`NANDA`, expressão `normal`):**
     > *"Levanta, dorminhoca.*
     > *Temos que brincar.*
     > *Você dormiu demais."*
3. **`10.0s a 13.0s` — Narração Poética 1 (`openingYesterday`):**
   - Enquanto Yuna começa a despertar e olhar ao redor estranhando a imensidão sombria do quarto:
   - **Quadro de Narração Poética Manuscrita:**
     > *"O quarto era o mesmo.*
     > *Só não a reconhecia."*
4. **`14.0s a 20.0s` — Narração Poética 2 (`openingPath`):**
   - A câmera mostra a penumbra dos móveis gigantes ao redor.
   - **Quadro de Narração Poética Manuscrita:**
     > *"Cada coisa guardava seu nome.*
     > *Entre elas, nada dizia por onde ir."*
5. **`21.0s a 24.0s` — Nanda Perde a Paciência (Fala 2):**
   - Nanda bate as asinhas com força, dando um puxãozinho de energia luminosa perto da menina.
   - **Caixa de Diálogo (`NANDA`, expressão `annoyed` / `determined`):**
     > *"LEVANTA, SUA PREGUIÇOSA!"*
6. **`25.0s a 29.0s` — O Chamado Urgente (Fala 3):**
   - **Caixa de Diálogo (`NANDA`, expressão `determined`):**
     > *"SENÃO A GENTE NÃO SAI DAQUI NUNQUINHA!"*
7. **`29.0s a 36.0s` — Yuna se Levanta e a Música Começa:**
   - Yuna finalmente se põe de pé, pronta para seguir a fada.
   - **Aos `33.5s`:** Entra a música tema do Quarto Escuro.
8. **`36.0s a 41.0s` — Cartão de Capítulo:**
   - Surge na tela o cartão ilustrado de abertura de capítulo:
     > **CAPÍTULO 1**
     > **Quarto de brinquedos**
9. **`41.0s a 42.5s` — Revelação e Transição para o Jogo:**
   - O cartão se dissipa suavemente, a câmera assume o enquadramento lateral fixo de jogabilidade e Nanda pousa sobre a **Plataforma 0 (Cabeça do Urso de Pelúcia)**, aguardando o primeiro salto.

---

## 3. Mecânica Fundamental de Plataforma do Quarto Escuro

- **Salto Guiado / Arco de Salto de Plataforma em Plataforma:**
  - No Quarto Escuro, a progressão é focada em **saltos rítmicos e precisos de uma plataforma para a próxima**, guiados pela fadinha Nanda.
  - A cada plataforma alcançada por Yuna, Nanda voa imediatamente para o centro da **próxima plataforma da sequência**, iluminando o alvo com seu halo dourado e partículas mágicas.
  - Na **Fase 1 (Subida ao Castelo)**, o jogador aprende o ritmo sem pressão de tempo: a câmera acompanha lateralmente sem forçar a morte por rolagem.
  - Se Yuna errar um salto e cair no chão na Fase 1, ou quando ela retoma um ponto de espera (*Standby*), Nanda voa até ela e exibe o diálogo de incentivo:
    - **Caixa de Diálogo (`NANDA`, expressão `normal`):**
      > *"Você está bem? Vamos tentar novamente!"*
- **Tutorial do Primeiro Salto (`FIRST_JUMP_TUTORIAL`):**
  - Logo ao terminar a abertura, enquanto Yuna ainda está no chão antes da Plataforma 0, exibe um indicador elegante apontando para a Cabeça do Urso de Pelúcia onde Nanda está pousada, instruindo o jogador a acionar o salto (`Espaço / Seta para Cima` no teclado, `Botão A / X` no controle, ou `Toque` na tela). Assim que Yuna realiza o primeiro salto e pousa na Plataforma 0, o tutorial desaparece.

---

## 4. Fase 1: A Subida ao Castelo (Plataformas 0 a 9 — Esquerda para Direita)

Yuna sobe da esquerda para a direita por **10 plataformas 3D ilustradas**, todas apoiadas visualmente na arquitetura do quarto ou flutuando com magia sutil:

| Índice | Nome do Objeto 3D | Posição Referencial `(X, Y)` e Largura | Descrição Visual 3D para Modelagem na Unity |
| :--- | :--- | :--- | :--- |
| **0** | **Cabeça do Urso de Pelúcia** | `X: 165, Y: 440` (Largura: `78`) | Um grande urso de pelúcia bege macio sentado no chão; Yuna pousa no topo arredondado e felpudo da cabeça dele, entre as orelhinhas costuradas. |
| **1** | **Pilha de Livros Ilustrados** | `X: 280, Y: 398` (Largura: `88`) | Três grossos livros de capa dura coloridos (bordô, verde-musgo e azul-marinho) com detalhes dourados na lombada, empilhados um sobre o outro. |
| **2** | **Penteadeira Encantada** | `X: 410, Y: 350` (Largura: `100`) | Uma penteadeira clássica de madeira clara com espelho oval ao fundo, frascos de perfume de vidro colorido e tampo liso onde Yuna pisa. |
| **3** | **Cômoda Pequena de Madeira** | `X: 548, Y: 302` (Largura: `92`) | Criado-mudo/cômoda de carvalho com puxadores de latão antigo e pés torneados. |
| **4** | **Pilha de Blocos ABC** | `X: 678, Y: 258` (Largura: `84`) | Cubos infantis de madeira pintada com letras entalhadas (`A`, `B`, `C`) formando um patamar elevado. |
| **5** | **Tamborito de Marcha** | `X: 800, Y: 310` (Largura: `85`) | Um tambor de brinquedo cilíndrico vermelho e branco com cordões dourados em ziguezague; Yuna pousa na pele esticada do topo do tambor. |
| **6** | **Almofadão de Veludo** | `X: 922, Y: 262` (Largura: `92`) | Uma grande almofada real de veludo roxo/vinho com borlas douradas nos quatro cantos, apoiada sobre um banquinho alto. |
| **7** | **Gavetas da Cômoda** | `X: 1052, Y: 215` (Largura: `90`) | Uma gaveta de madeira puxada para fora de um armário alto, servindo como degrau suspenso com tecidos dobrados dentro. |
| **8** | **Caixa de Música da Bailarina** | `X: 1180, Y: 175` (Largura: `95`) | Uma delicada caixa de música aberta em madeira rosada e ouro, com uma pequena bailarina de porcelana ao fundo e engrenagens douradas. |
| **9** | **Castelinho de Blocos** | `X: 1320, Y: 225` (Largura: `145`) | Uma larga fortaleza de brinquedo construída com blocos de madeira, torres azuis pontiagudas e bandeirinhas vermelhas. É o grande patamar de descanso onde ocorre a Cutscene do Castelo. |

---

## 5. Cutscene do Castelo (No Topo da Plataforma 9)

Ao pousar na **Plataforma 9 (Castelinho de Blocos)**:
- O controle de plataforma pausa temporariamente. Yuna dá pulinhos de comemoração (`celebrate`) enquanto Nanda brilha intensamente acima das torres do castelinho.
- **Passo 1 da Cutscene:**
  - Nanda voa um pouco à frente, olhando para a direita (onde o quarto continua ficando mais alto e misterioso).
  - **Quadro de Narração Poética Manuscrita (`POETIC_LINES.castle`, exibido nos primeiros 3.5s):**
    > *"De longe, a porta ainda cumpria sua promessa."*
  - **Caixa de Diálogo (`NANDA`, expressão `normal`):**
    > *"A porta é por ali. Anda, vem comigo!"*
- **Passo 2 da Cutscene:**
  - Yuna olha para a distância à frente e para a altura em que estão.
  - **Caixa de Diálogo (`MENINA`, expressão `normal`):**
    > *"Daqui parece perto. Lá embaixo, não."*
- Assim que o jogador confirma o avanço do Passo 2, inicia-se imediatamente a **Fase 2: A Fuga**.

---

## 6. Fase 2: A Fuga (Plataformas 10 a 21 + Porta Falsa — Rolagem Acelerada para a Direita)

### 6.1. Mecânica do Modo Fuga (`ESCAPE MODE`)
- **Rolagem Lateral Forçada e Progressiva:** A câmera passa a avançar automaticamente para a direita. Se Yuna demorar demais e ficar para trás na borda esquerda da tela (ou cair no abismo escuro abaixo dos móveis), a penumbra a alcança e ocorre **Game Over**:
  - **Tela de Fim de Jogo:**
    - **Emblema:** `✦ QUARTO ESCURO ✦`
    - **Título:** `Você não conseguiu sair do quarto.`
    - **Subtítulo:** `A penumbra engoliu o caminho... Mas a luz da fadinha ainda chama por você.`
    - **Botões:** `Tentar de novo` (retoma do início da seção atual, ou seja, do Castelinho de Blocos na Fase 2) e `Voltar ao início`.
- **Evolução Progressiva do Pulo e da Velocidade (12 Níveis):**
  - A cada nova plataforma alcançada da **10 até a 21**, Yuna sobe um nível de impulso (`Nível 1/12` até `Nível 12/12`).
  - **HUD da Fuga:** No canto superior direito, aparece uma insígnia elegante com barra de progresso: `⚡ FUGA: PULO NÍVEL X/12`, acompanhada de linhas sutis de vento no ar e um gradiente vermelho de alerta na borda esquerda caso Yuna chegue a menos de `170px` da borda da câmera.
  - **Banner inicial ao começar a Fuga:** `"A SOMBRA ESTÁ AVANÇANDO! SALTE RÁPIDO!"` (ou aviso de evolução de nível a cada salto).

### 6.2. Sequência das 12 Plataformas da Fase 2 (10 a 21)

| Índice | Nível Fuga | Nome do Objeto 3D | Posição Referencial `(X, Y)` e Largura | Velocidade da Câmera (Normal) | Descrição Visual 3D para Modelagem na Unity |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **10** | `1/12` | **Pista Elevada do Trem** | `X: 1520, Y: 195` (Larg: `105`) | `1.35` | Um viaduto curvo de trilhos de madeira suspenso por pilares de brinquedo. |
| **11** | `2/12` | **Prateleira de Brinquedos** | `X: 1685, Y: 160` (Larg: `110`) | `1.50` | Prateleira de madeira presa à parede com mãos-francesas ornamentadas e pequenos bibelôs no fundo. |
| **12** | `3/12` | **Abajur Cogumelo** | `X: 1855, Y: 220` (Larg: `95`) | `1.65` | Um grande abajur em forma de cogumelo vermelho com pintinhas brancas que emite luz quente por baixo da cúpula. |
| **13** | `4/12` | **Telhado da Casa de Bonecas** | `X: 2015, Y: 170` (Larg: `115`) | `1.82` | O telhado inclinado de telhas cor-de-rosa de uma alta casa de bonecas vitoriana com janelinhas iluminadas. |
| **14** | `5/12` | **Globo Terrestre Ilustrado** | `X: 2195, Y: 230` (Larg: `92`) | `2.00` | Um globo geográfico antigo em tons sépia e azul-celeste preso num arco de latão; Yuna pisa no polo superior do globo. |
| **15** | `6/12` | **Pipa Encantada de Bambu** | `X: 2355, Y: 178` (Larg: `100`) | `2.18` | Uma pipa losangular de papel de seda colorido e varetas de bambu com rabiola de fitas flutuando no ar noturno. |
| **16** | `7/12` | **Livro de Gravuras Flutuante** | `X: 2525, Y: 140` (Larg: `108`) | `2.36` | Um livro mágico aberto no ar, com páginas arqueadas formando uma ponte sólida e partículas douradas subindo. |
| **17** | `8/12` | **Lustre de Cristais** | `X: 2705, Y: 198` (Larg: `112`) | `2.55` | Um candelabro suspenso de bronze com pingentes de cristal facetado que refratam a luz em pequenos arco-íris. |
| **18** | `9/12` | **Varão de Cortina Estrelada** | `X: 2890, Y: 148` (Larg: `118`) | `2.75` | O longo varão cilíndrico de madeira dourada no alto da janela, com argolas segurando a cortina estrelada. |
| **19** | `10/12` | **Relógio Cuco Vintage** | `X: 3080, Y: 205` (Larg: `102`) | `2.95` | Relógio cuco entalhado em madeira de floresta negra com pêndulo dourado e **base/suporte de madeira sólida incorporada à parede** onde os pés pousam. |
| **20** | `11/12` | **Beiral do Grande Guarda-Roupa** | `X: 3255, Y: 155` (Larg: `125`) | `3.15` | A cornija superior esculpida de um guarda-roupa monumental de mogno. |
| **21** | `12/12` | **O Portal dos Sonhos** | `X: 3455, Y: 195` (Larg: `185`) | `3.35` | Um amplo patamar cerimonial onde está posicionada a **Porta de Saída (`exitDoor` em `X: 3535, Y: 110`)**, uma porta arqueada brilhante que parece ser a saída do quarto. |

---

## 7. O Plot Twist da Porta Falsa e o Tutorial de Retorno

Ao pousar na **Plataforma 21 (`O Portal dos Sonhos`)**, a rolagem da Fuga para e inicia-se uma das cenas mais marcantes e divertidas do Capítulo 1:

### Cronograma da Reviravolta (`Plot Twist` — 6 Etapas):
1. **Etapa 1 — Comemoração Antes da Hora:**
   - Yuna comemora feliz diante da porta iluminada achando que venceu a fase.
2. **Etapa 2 — A Porta é de Papelão/Madeira Falsa!:**
   - Com um estalo cômico e dramático, a **Porta de Saída (`exitDoor`) descola da parede**, inclina-se e **escorrega/despenca lá para baixo** no vazio escuro!
3. **Etapa 3 — Queda no Chão e Inspeção da Fada:**
   - O suporte cede e Yuna cai lá de cima até aterrissar sentada no chão de madeira (`FLOOR_Y = 482`, na altura de `X ~ 3480`), sem sofrer dano letal, mas chocada (com estrelinhas/gotinha de espanto).
   - Nanda voa rapidamente até o buraco onde a porta estava, olha para baixo incrédula e depois desce até a Yuna com as mãozinhas na cintura, furiosa com a pegadinha do quarto.
4. **Etapa 4 — Diálogo de Espanto da Menina (Moldura Vermelha de Plot Twist):**
   - **Caixa de Diálogo (`MENINA`, expressão `shocked`, borda vermelha):**
     > *"Mas era ali. Eu tinha certeza."*
5. **Etapa 5 — Narração Poética e Diálogo de Frustração da Nanda:**
   - **Quadro de Narração Poética Manuscrita (`POETIC_LINES.falseDoor`, nos primeiros 3s da etapa):**
     > *"A madeira caiu.*
     > *A promessa ficou de pé."*
   - **Caixa de Diálogo (`NANDA`, expressão `annoyed`, primeira linha em amarelo vivo):**
     > *"Eu também achei. Droga... Vamos olhar por ali."*
6. **Etapa 6 — Demonstração do Salto Reverso (Da Direita para a Esquerda):**
   - Nanda voa para a esquerda e realiza um arco parabólico luminoso demonstrando que agora **o quarto se bagunçou inteiro** e elas precisam voltar **da direita para a esquerda**, saltando por uma nova trilha caótica de objetos flutuantes e revirados!
   - Uma seta dourada estilizada aponta para a esquerda (`←`) sobre a primeira plataforma da volta.

---

## 8. Fase 3: A Subida Caótica (15 Plataformas + Portal Verdadeiro — Direita para Esquerda)

### 8.1. Mecânica da Fase 3 (`SUBIDA CAÓTICA`)
- **Sentido Inverso (Direita $\rightarrow$ Esquerda):** Agora Yuna salta da direita (`X = 3310`) de volta até a extrema esquerda do quarto (`X = 45`).
- **Checkpoint Justo:** Se Yuna cair ou for alcançada pela sombra na Fase 3 e clicar em `Tentar de novo`, ela **não volta para a Fase 1 ou 2**; ela recomeça exatamente no início da **Fase 3** (`X = 3480`), já pronta para subir a trilha caótica.
- **HUD da Fase 3:** O emblema no topo direito muda para roxo/magenta e dourado: `🌪️ SUBIDA CAÓTICA: NÍVEL X/15`, as linhas de velocidade correm no sentido inverso, e o alerta vermelho de perigo fica na **borda direita** da tela.

### 8.2. Sequência das 15 Plataformas da Fase 3 + Plataforma Final (Da Direita para a Esquerda)

| Nível | Nome do Objeto 3D | Posição Referencial `(X, Y)` e Largura | Velocidade da Câmera (Normal) | Descrição Visual 3D para Modelagem na Unity |
| :--- | :--- | :--- | :--- | :--- |
| **1/15** | **Pilha de Blocos Tombada** | `X: 3310, Y: 410` (Larg: `95`) | `1.55` | Blocos de madeira revirados no chão formando o primeiro degrau de subida para a esquerda. |
| **2/15** | **Boneca de Pano Desconjuntada** | `X: 3110, Y: 350` (Larg: `95`) | `1.72` | Uma grande boneca de pano com cabelos de lã vermelha e vestido xadrez sentada de jeito torto sobre um caixote. |
| **3/15** | **Caixa de Giz de Cera Aberta** | `X: 2905, Y: 290` (Larg: `100`) | `1.90` | Uma caixa amarela e verde de giz de cera gigante aberta, com vários gizes coloridos apontando para fora. |
| **4/15** | **Pilha Torta de Contos de Fada** | `X: 2695, Y: 355` (Larg: `95`) | `2.08` | Livros antigos empilhados em ângulo louco, quase escorregando, mas firmes sob os pés. |
| **5/15** | **Tamborzinho Amassado** | `X: 2485, Y: 280` (Larg: `92`) | `2.26` | Um tambor de lata inclinado com baquetas cruzadas embaixo. |
| **6/15** | **Urso de Pelúcia Desmoronado** | `X: 2275, Y: 215` (Larg: `98`) | `2.45` | Um urso de pelúcia deitado de bruços sobre uma prateleira alta; Yuna pisa nas costas fofas dele. |
| **7/15** | **Xilofone Colorido Inclinado** | `X: 2060, Y: 275` (Larg: `105`) | `2.65` | Um xilofone infantil com teclas metálicas nas cores do arco-íris que emitem um brilho suave ao toque. |
| **8/15** | **Locomotiva Descarrilada** | `X: 1845, Y: 200` (Larg: `102`) | `2.85` | Um vagão/locomotiva de madeira vermelha e preta parado de lado sobre livros. |
| **9/15** | **Castelo de Cartas Bamboleante** | `X: 1630, Y: 145` (Larg: `96`) | `3.05` | Uma estrutura mágica de cartas de baralho gigantes ilustradas com reis e estrelas douradas. |
| **10/15** | **Caixa de Música Desregulada** | `X: 1415, Y: 210` (Larg: `100`) | `3.26` | Caixa de música inclinada com molas douradas aparentes e brilho mágico. |
| **11/15** | **Robô de Lata Desparafusado** | `X: 1200, Y: 150` (Larg: `95`) | `3.48` | Um robô retrô de lata azul sentado com antenas tortas; Yuna apoia nos ombros quadrados dele. |
| **12/15** | **Pião de Madeira Rodopiante** | `X: 985, Y: 215` (Larg: `92`) | `3.70` | Um grande pião de madeira listrada flutuando estabilizado no ar, servindo de plataforma circular no topo. |
| **13/15** | **Carretel com Fita Flutuante** | `X: 770, Y: 150` (Larg: `95`) | `3.92` | Um carretel gigante de madeira com fita de cetim rosa desenrolando pelo ar em ondas 3D. |
| **14/15** | **Móbile Desequilibrado** | `X: 555, Y: 195` (Larg: `98`) | `4.15` | Estrutura suspensa de luas, nuvens e estrelas de feltro dourado penduradas no teto. |
| **15/15** | **Livro de Feitiços no Vácuo** | `X: 335, Y: 140` (Larg: `102`) | `4.38` | Um grimório infantil iluminado flutuando perto da parede esquerda, último salto antes do portal real. |
| **Final** | **O Verdadeiro Portal dos Sonhos** | `X: 45, Y: 180` (Larg: `195`) | `4.60` | Grande balcão real iluminado onde repousa a **Porta Verdadeira (`trueExitDoor` em `X: 85, Y: 88`, largura `68`, altura `92`)**, emanando intensa luz dourada e partículas de sonhos. |

---

## 9. A Travessia do Verdadeiro Portal (Transição para a Sala de Brinquedos)

- Quando Yuna pousa na plataforma final **O Verdadeiro Portal dos Sonhos (`X: 45, Y: 180`)**:
  1. A rolagem caótica cessa imediatamente e a música de tensão se resolve num acorde mágico luminoso.
  2. **Narração Poética Manuscrita:**
     > *"E lá estava ela: a porta que só se abre para quem não desiste de procurar."*
  3. Yuna caminha até a **Porta Verdadeira (`trueExitDoor`)** ao lado de Nanda.
  4. Um **efeito de Íris / Envelope de Luz Dourada (`Portal Wipe`)** se expande a partir do centro da porta, banhando toda a tela em luz quente marfim/dourada e fazendo a transição direta para o **Capítulo 1 — Parte 2: A Sala de Brinquedos**.
