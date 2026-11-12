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
4. **Isolamento por Recorte Temporal de Plataformas (Regra de Encenação):**
   - As plataformas pertencem a recortes temporais distintos da escalada pelo quarto. Em nenhum momento todos os 38 obstáculos coexistem no espaço visual.
   - Enquanto a jogadora está na **Etapa 1**, somente as plataformas `00 a 09` estão ativas; as demais permanecem totalmente invisíveis e desativadas.
   - Ao iniciar a **Etapa 2**, as plataformas da subida anterior desaparecem (mantendo apenas o patamar do castelo enquanto a jogadora salta para a plataforma 10) e surgem as plataformas `10 a 21`.
   - Durante a **queda do poster**, nenhuma plataforma da rota fica visível; apenas o chão da sala recebe a personagem.
   - Ao despertar para a **Etapa 3** (corrida reversa para a esquerda), todas as plataformas anteriores da subida para a direita são ocultadas e ativam-se exclusivamente as plataformas `22 a 37`.

---

## 3. Estrutura Canônica do Percurso de Plataformas (38 Plataformas Mapeadas)

O circuito completo em 3 etapas está formalizado em `DarkRoomPlatformCatalog.cs` e gerado com calibração física em `DarkRoomThreeStageCircuitGenerator.cs`:

- **Etapa 1: Subida ao Castelo (Índices 00 a 09, Esquerda para Direita — 10 Plataformas):**
  - Autorunner tranquilo e acolhedor (velocidade 3.6 m/s, pulo 2.4m, vão uniforme de 1.4m).
  - Começa na Cabeça do Urso de Pelúcia (`00`) e sobe suavemente por livros, penteadeira, cômoda, blocos ABC, tambor, almofada de veludo, gavetas e caixa de música até o **Castelinho de Blocos (`09`)**.
  - **Cutscene do Castelo:** O autorunner pausa; ocorrem diálogos interativos avançados por input do jogador (Espaço/Enter/Clique/Botão A):
    1. Nanda: *"A porta é por ali. Anda, vem comigo!"* + Narração Poética: *"De longe, a porta ainda cumpria sua promessa."*
    2. Menina: *"Daqui parece perto. Lá embaixo, não."*
  - Ao confirmar, inicia imediatamente a Etapa 2.
- **Etapa 2: A Fuga Acelerada (Índices 10 a 21, Esquerda para Direita — 12 Plataformas):**
  - Aceleração gradual e progressão de pulo: velocidade escala de 4.0 m/s a 6.0 m/s; altura de salto de 2.5m a 3.4m; gaps de 1.6m a 2.69m.
  - Câmera avança com enquadramento de seção fixo (`fixedY = -1.5f`) e esmagamento lateral de tela ativo caso o jogador fique para trás.
  - Percorre trilhos do trem, prateleira, abajur cogumelo, telhado de bonecas, globo, pipa, livro no ar, lustre de cristais, varão de cortina, relógio cuco, beiral do guarda-roupa até atingir o **Poster da Porta Falsa na Parede (`21`)**.
- **O Plot Twist da Porta Falsa e Queda Cinemática Real:**
  - O suporte cede e Yuna despenca em **queda física real sob gravidade** do CharacterController (não letal) até aterrissar no chão de madeira da sala (`Y ~ -9.1m`).
  - Nanda voa até Yuna sentada no chão e expressa frustração cômica.
  - Diálogos interativos avançados por input:
    1. Menina (estilo especial de plot twist): *"Mas era ali. Eu tinha certeza."*
    2. Nanda: *"Eu também achei. Droga... Vamos olhar por ali."* + Narração Poética: *"A madeira caiu. A promessa ficou de pé."*
  - Nanda demonstra a direção reversa e o circuito inverte para a esquerda.
- **Etapa 3: A Subida Caótica Reversa (Índices 22 a 37, Direita para Esquerda — 16 Plataformas):**
  - Yuna corre virada para a esquerda (`runDirection = -1f`), com velocidade escalando de 4.2 m/s a 6.6 m/s e pulos longos de 2.6m a 3.9m.
  - Plataformas progressivamente menores (2.2m até 1.27m) e gaps maiores (1.8m até 4.6m), calibrados para o arco completo por toque.
  - Inicia nos blocos tombados (`22`) perto do chão e sobe por boneca revirada, giz de cera, livros tortos, tambor amassado, urso, xilofone, locomotiva, cartas, molas, robô, pião, carretel, móbile e grimório até o **Verdadeiro Portal dos Sonhos (`37`)**, patamar largo de 4.5m que encerra o quarto escuro com a narração: *"E lá estava ela: a porta que só se abre para quem não desiste de procurar."*

---

## 4. Configuração Técnica URP (Unity 6.3 LTS)

- **Render Pipeline:** Universal Render Pipeline (URP 17.0.3).
- **Modo de Renderização:** *Forward+* recomendado para suportar até 32 luzes pontuais no Android e 256 no PC (feixes de janelas, Nanda, abajur, lustre e portais).
- **Input System:** Novo pacote `com.unity.inputsystem` (1.13.1) ativo por padrão em `ProjectSettings.asset`, unificando Teclado, Gamepad e Toque em tela (Android).
- **Detecção de Colisão em CharacterController:** A aterrissagem em plataformas é capturada via `OnControllerColliderHit` diretamente em `Yuna25DPlayerController`, notificando `DarkRoomProgressionManager` com precisão milimétrica.
- **Checkpoints e Retry:** Cada etapa define seu ponto de retorno (`Phase1Ascent` no piso antes da plat 0, `Phase2Escape` no castelo plat 9, `Phase3ChaoticAscent` no chão de queda da porta falsa). Quedas no chão escuro ou ser atropelado pela margem da tela acionam a tela de Game Over imediatamente desde o primeiro salto da Etapa 1, exibindo o botão "Tentar de Novo" (acionável também por Espaço / Clique / Botão A).

---

## 5. Como Gerar o Blockout 3D na Unity

1. Abra o projeto no **Unity Hub** selecionando a versão `6000.3.28f1`.
2. Abra a cena `Assets/Scenes/DarkRoom_Gameplay.unity`.
3. No menu principal da Unity, selecione **YunaLegend ➔ Construir Cena Quarto Escuro** (executa `DarkRoomScenePopulator.PopulateDarkRoomScene`).
4. Todas as 38 plataformas em 3 etapas canônicas, piso corrido, Yuna, Nanda, câmera 2.5D travada em X e gerenciador de progressão são automaticamente gerados e configurados.


## 6. Rota obrigatória e validação física (10/11/2026)

- Após a abertura, Yuna corre no piso em (-10, -9, 0); Nanda aponta para o urso (plataforma 0), com tutorial junto à guia. Retry da primeira etapa repete essa largada, sem conceder plataforma visitada.
- As 38 plataformas devem ser conquistadas na ordem. Ultrapassar o alvo, aterrissar fora da sequência, cair abaixo da rota ou bater de frente numa plataforma durante a descida causa Game Over. Não existe caminho alternativo pelo piso.
- O castelo continua sólido no início da fuga até o pouso em 10. Retry da fuga restaura apoio 9; retorno reinicia no piso sob a parede real do pôster, com 22 ainda obrigatória.
- A única queda não letal é a narrativa do pôster: as plataformas desaparecem durante a queda; só depois do diálogo surge a etapa reversa. Etapas futuras não ficam visíveis nem colidíveis antes da transição.
- Validação automatizada: suite PlayMode DarkRoomMandatoryRouteTests usa CharacterController real, incluindo largada, bypass, salto válido, falha, checkpoints 2/3, queda narrativa e todos os pares de saltos. Suite EditMode cobre a cena salva e visibilidade. Executar em cópia isolada de Assets, Packages e ProjectSettings se o editor do usuário estiver aberto; não abrir batch no mesmo projeto.
- Referência HTML pública foi observada parcialmente; não houve conclusão manual da fase HTML. Testes automatizados não substituem revisão visual final de enquadramento/tutorial pelo usuário.

## 7. Largada acessivel validada em Unity (10/11/2026)

- `DarkRoomStartLayout` unifica spawn/abertura/retry, velocidade3.6, salto2.4 e offset da camera. Inicio x=-10; primeira borda x=0.5: 2.92s geometricos de aproximacao (antes0.83s); ha mais de2s antes da janela de salto.
- Primeiro topo=-8.30 (0.80m acima do piso=-9.10); segundo=-7.55 (+0.75m); terceiro=-6.80 e quarto=-6.05. Plataforma4 em diante, castelo e demais etapas permanecem nas alturas anteriores. Nao ha autopulo nem controle lateral.
- `DarkRoomStartInputTests` carrega a cena salva em PlayMode e envia QueueStateEvent ao InputSystem, sem reflection no salto nem TickMovement manual. Matriz30/60/120Hz, Espaço/W/clique/toque por apenas1frame: quatro delays2.40/2.50/2.60/2.70s (48 pousos). Teste adicional compara24 arcos de toque e hold700ms: altura2.4m com tolerancia6cm e diferenca entre dispositivos<=5mm. Soltar nao pula o proximo vao; no-input e erro continuam GameOver; retry e checkpoints preservados.
- Baseline original reproduzido em copia isolada: x=-2.5, toque teclado em0.60s por100ms, derrota aos0.90s, x0.10/y-7.37. Harness corrigido para foco de dispositivos no editor batch; medições anteriores sem essa configuração não eram validas.
- Evidencias locais em `Build\StartValidation`: `baseline-real.xml`, `play-final.xml`, `edit-final.xml`, logs e `Evidence\start-camera.png`. Captura1280x720 da camera real por RenderTexture; nao inclui OnGUI e nao comprova visual do tutorial, nem houve controle manual GUI. `DarkRoomStartCapture.Capture` reproduz a captura via executeMethod sem alterar cena.
- Tutorial agora distingue "Prepare" de "Pule agora" conforme proximidade, para nao sugerir um salto2s cedo. Texto acompanha Nanda na tela; revisao visual final do overlay permanece recomendada.