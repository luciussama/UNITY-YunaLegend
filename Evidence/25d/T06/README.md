# T06 — integração e movimento do piloto: consolidação

Data: 13/11/2026. Sessão `2026-11-13-dsh-consolidacao-t06t07`
(DeepSeek/deepseek-flash). Registro de agentes **6**
(`t06/consolidacao-criterios-v1`) e reserva `plan:t06` ativos durante o trabalho.

**Estado: T06 reaberta em 13/11/2026** depois de uma regressão de contato
reportada pelo usuário e confirmada na build final: urso e livros tocam o piso,
mas não têm sombra de contato visível na penumbra. O aceite técnico registrado
mais abaixo foi invalidado pelo coordenador (`reopen`) e T07 permanece **bloqueada**
até a correção ser apresentada e revisada. Esta entrega não inicia T08, não publica
e não declara teste em aparelho móvel ou WebGL.

O pedido que originou a sessão foi consolidar os critérios restantes de T06/T07
antes de expandir o piloto, com cobertura e limites explícitos de áudio,
teclado/toque, aparelhos e WebGL.

## O que esta consolidação acrescenta

1. Matriz de critérios de T06 e T07/T07.1 com evidência e limite por item.
2. **Captura real de áudio** do executável (mix 48 kHz) com análise de amplitude e
   correlação com o clip aprovado do estralo — o piloto tem música de fundo tocando,
   o que não estava registrado antes.
3. Reexecução final de **30/60/120 Hz** na build instrumentada, com cinco tentativas
   e quatro recargas reais por taxa.
4. Cobertura de entrada por **PlayMode** (teclado, mouse, W e toque) e regressão
   **EditMode 52/52**.
5. Parâmetros de câmera, física e evento registrados em
   [`parametros.json`](parametros.json), com SHA-256 das fontes e da build.
6. Pacote de revisão de T07 em [`../T07/README.md`](../T07/README.md), bloqueado
   pela regressão descrita abaixo.

## Regressão de contato reportada e confirmada

Em 13/11/2026 o usuário relatou **urso e livros flutuando sem sombra** (registro
[diário 972](../../../docs/ia/diario.sqlite3); o Codex respeitou a reserva desta
sessão e não alterou arquivos). A build final foi capturada com `-previewVerify`
a 60 Hz e as imagens foram inspecionadas com ampliação:
[urso](contact-regression/02-bear.png) e
[livros](contact-regression/03-books.png).

- **Confirmado:** as bases tocam o piso, mas não há sombra/ancoragem visível sob
  o urso nem sob os livros na câmera final.
- **Causa provável:** `ContactShadowT054` instala elipses **escuras** (alfa `0,24`
  e `0,36`) e a penumbra deixou o piso quase preto; uma sombra escura sobre piso
  escuro não lê. O contato aprovado no `contact-v2` claro não se sustenta no
  runtime escuro. Não é geometria de apoio: os colisores e o pouso medido
  continuam corretos.
- **Correção:** não aplicada nesta sessão. A ancoragem na penumbra exige uma
  decisão visual (por exemplo, luz fria de contato/"moon sheen" em vez de sombra
  escura, ou piso iluminado sob as bases) e uma nova microentrega revisável.
- **Efeito no estado:** T06 reaberta; T07 bloqueada; a expansão continua proibida.
  O novo candidato deve ser capturado na câmera final, comparado com
  [contact-v2](../T05.4/contact-v2/README.md) e
  [penumbra-v2](../T05.4/penumbra-v2/README.md) e submetido à revisão humana.

## Matriz T06

| # | Critério (fonte) | Situação | Evidência |
| --- | --- | --- | --- |
| T06-1 | Integração exclusiva do piloto na cena de avaliação, sem expandir a fase ([plano](../../../docs/ia/PLANO-25D-OBRIGATORIO.md), linha T06) | **Cumprido** | `DarkRoom_IllustratedPreview` com apenas os marcadores 0–2 ativos; plataformas seguintes desativadas em [`IllustratedPreview.cs`](../../../Assets/Art/Illustrated25D/Preview/IllustratedPreview.cs); build sem fallback 3D |
| T06-2 | Run/jump/land/stumble/retry com câmera final e UI | **Cumprido** | [T06.3](../T06.3/README.md): 5 tentativas por taxa, R na pausa e na recuperação, P/Esc, falha e nova travessia; [vídeo contínuo](../T06.3/v3-visible-60/continuous.mp4) |
| T06-3 | Física 3D e `PlatformSupportProfile` preservados | **Cumprido** | Nenhuma alteração em `Yuna25DPlayerController`/`YieldingWoodFurniture`/física; pousos medidos por tentativa em [`parametros.json`](parametros.json) e nos `verification.json`; EditMode 52/52 e PlayMode de entrada aprovados |
| T06-4 | Comparar variantes de câmera, assistência horizontal e animação separadamente | **Parcial — limite declarado** | Câmera A × B e animação pintada × deformação foram decididas em [T02.2](../T02.2/README.md)/[T02.3](../T02.3/README.md) e não reabertas. A **assistência horizontal de pouso não teve A/B isolado**: o comportamento aprovado foi preservado e medido, sem variante alternativa. Registrado como limite, não como comparação executada |
| T06-5 | Registrar parâmetros, resultados e limitações | **Cumprido** | [`parametros.json`](parametros.json) + esta página + `verification.json` por taxa |
| T06-6 | Corrigir regressões de entrada do menu | **Cumprido** | Menu reduzido a 3 atalhos e build aberta por clique em Começar/Recomeçar; [registro](../menu-cleanup/README.md) e [T06.3](../T06.3/README.md) |
| T06-7 | Registrar áudio capturado/escutado e seus limites, sem presumir validação | **Parcial — capturado, não escutado** | [`audio-60/gameplay-audio.wav`](audio-60/gameplay-audio.wav) e [`audio-analysis.json`](audio-60/audio-analysis.json); análise abaixo. Nenhuma escuta humana ou certificação de mixagem |
| T06-8 | Captura do executável com UI, taxa de simulação e de captura identificadas | **Cumprido** | Vídeo T06.3: 1280×720, simulação 60 Hz, captura 30 fps, 37,30 s, sem áudio. A build final só difere na instrumentação opt-in; visual e gameplay não mudaram |
| T06-9 | Contato e sombras legíveis sob urso e livros na penumbra (herdado de [T05.4](../T05.4/README.md)) | **Regressão — reaberta** | [Capturas](contact-regression/02-bear.png) e [diagnóstico](#regressão-de-contato-reportada-e-confirmada): bases tocam o piso, mas a sombra escura não lê sobre piso escuro. Correção visual pendente de nova microentrega |

Aceite técnico da T06 se apoia nos itens acima, nas sete evidências já submetidas em
[T06.3](../T06.3/README.md) e na revalidação desta sessão. O item T06-4 permanece
como limite explícito; não foi maquiado como comparação concluída. O item T06-9
**não está cumprido** e foi o motivo da reabertura.

## Matriz T07 / T07.1

| # | Critério (fonte) | Situação | Evidência / observação |
| --- | --- | --- | --- |
| T07-1 | Build jogável e vídeo do piloto em câmera final ([plano](../../../docs/ia/PLANO-25D-OBRIGATORIO.md), linha T07) | **Cumprido** | Build de acompanhamento + [vídeo 37,30 s](../T06.3/v3-visible-60/continuous.mp4); build final verificada nesta sessão |
| T07-2 | Revisão humana de acolhimento, identidade, companhia, fluidez, previsibilidade e leitura dos apoios | **Pendente** | Perguntas e evidências no [pacote T07](../T07/README.md); nenhuma resposta humana nesta sessão |
| T07-3 | **Aprovação explícita do piloto em movimento** antes da expansão | **Pendente** | Não inferida de “perfeito. Aprovado.” (v6) nem de “aprovadíssimo” (T06.3); ambos aprovam entregas específicas |
| T07.1 | Piloto contínuo em build, teclado e toque, com limitações identificadas ([microentregas](../../../docs/ia/25D-ASSINATURA-E-MICROENTREGAS.md)) | **Parcial — teclado/toque sintéticos** | Teclado, mouse, W e toque cobertos por PlayMode em 30/60/120 Hz; sem aparelho real. Limitações abaixo |

## Áudio capturado (não escutado)

Captura opt-in `-previewAudioCapture`, em fim de quadro, do **mix final** do
executável: 48 kHz, estéreo, PCM16, **70,187 s** (cinco tentativas e quatro
recargas). Arquivos: [WAV](audio-60/gameplay-audio.wav),
[marcas](audio-60/gameplay-audio.json) e
[análise](audio-60/audio-analysis.json). O WAV pode ser aberto por uma pessoa;
nenhuma escuta foi feita por IA e nenhuma mixagem foi certificada.

- **Música de fundo presente:** `High-Frequency Violin`, em loop, volume `0,50`,
  tocando também após o reset. Isso não estava registrado nas entregas anteriores
  e muda a leitura do “silêncio” após reinício: o estralo para, a música continua.
- **Estralo:** a análise por correlação normalizada encontra o clip aprovado
  `wood_furniture_crack.wav` nos quatro eventos de móvel (NCC `0,268`–`0,589`;
  deslocamento `0`–`0,13 s` em relação à marca). Há exatamente **um** evento por
  travessia concluída e **nenhum** na tentativa que falha antes do móvel.
- **Pico no evento:** `0,146`–`0,218` contra pico de `0,043` no início das
  tentativas (música); a medida é compatível com o estralo audível sobre a música.
- **Limite:** o vídeo T06.3 é mudo e de outra execução; não há sincronização
  áudio–vídeo nesta evidência. Escuta e julgamento de mixagem continuam humanos.

## Teclado, mouse e toque

- O controlador lê `Keyboard` (Espaço/↑/W), `Mouse` (botão esquerdo) e
  `Touchscreen` (toque primário) em [`Yuna25DPlayerController.cs`](../../../Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs).
  A UI do acompanhamento é IMGUI; botões recebem toque como mouse.
- **PlayMode aprovado:** `TapAndHoldHaveIdenticalBallisticArcOnEveryDevice`
  passou em 30/60/120 Hz nos quatro dispositivos (teclado, mouse, toque e W),
  com toque curto e longo, ápice `2,399 m` e diferença máxima `0,005 m`.
  [XML](tests/playmode-touch-ballistic.xml).
- **EditMode 52/52** aprovado: [XML](tests/editmode.xml).
- **Limite:** dispositivos sintéticos do Input System; nenhum aparelho físico,
  orientação retrato, safe area ou toque real foram testados nesta sessão.

## Revalidação da build final em 30/60/120 Hz

Build `Succeeded`, zero erros, `189.849.481` bytes no resumo Unity. Executável em
`Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe` (SHA-256 no manifesto).
Cada taxa executa as cinco tentativas da T06.3 no **mesmo binário final**.

| Simulação | `success` | Erros | Quadros | Estralos por tentativa | Cedimento máx. | Pausa local | Fonte visível | Legado visível |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 30 Hz | `true` | 0 | 1132 | 1/1/1/0/1 | 0,075 m | 0,08 s | 1 | 0 |
| 60 Hz | `true` | 0 | 2239 | 1/1/1/0/1 | 0,075 m | 0,08 s | 1 | 0 |
| 120 Hz | `true` | 0 | 4456 | 1/1/1/0/1 | 0,075 m | 0,08 s | 1 | 0 |

Relatórios: [30 Hz](rate-30/verification.json), [60 Hz com áudio](audio-60/verification.json)
e [120 Hz](rate-120/verification.json).

**Cuidado de reprodução:** com `Time.captureDeltaTime`, a janela precisa estar
visível/em primeiro plano. Em segundo plano o player ficou limitado a ~28 fps e a
física do salto divergiu na borda do móvel (parou em `x≈7,2`); com a janela em
primeiro plano a mesma build passou (4456 quadros). O limite de tempo do harness
foi trocado de 25 s de relógio para 30 s de simulação por tentativa, justamente
para não confundir lentidão de renderização com defeito de percurso — mudança
somente no verificador opt-in, sem efeito no jogo normal.

## Parâmetros resumidos

Detalhe completo, com fontes e SHA-256, em [`parametros.json`](parametros.json).

- **Câmera:** perspectiva (opção A), FOV 45°, vão vertical 7,5 m, profundidade
  calculada `9,0534 m`, acompanhamento `0,45 s`, avanço `0,18` da largura,
  âncora `1,8 m` acima do apoio.
- **Física:** `CharacterController` 3D, gravidade `22 m/s²`, corrida `3,6 m/s`,
  salto `2,4 m` (ápice `0,467 s`), coyote `0,15 s`, buffer `0,15 s`.
- **Assistência de pouso:** velocidade horizontal calibrada para o ponto de pouso,
  `lerp 0,75` entre alcance base e alvo, multiplicador `0,80–1,22×`; preservada do
  controlador aprovado. Pousos medidos: livros `Δx≈0,008 m`, móvel `Δx≈0,054 m`
  (o valor do primeiro apoio, `0,400 m`, é distância além do ponto de entrada, não
  erro de pouso).
- **Móvel:** atraso local `0,08 s`, recuperação `0,28 s`, cedimento `0,075 m`,
  clip `wood_furniture_crack.wav`.

## O que esta consolidação não valida

- Escuta, estralo percebido, equilíbrio de volumes ou mixagem.
- Toque em aparelho real, retrato, safe area, aparelhos móveis e desempenho/FPS.
- WebGL: o módulo existe no Editor, mas não houve build nem execução no navegador.
- Sombras dinâmicas de oclusão; a penumbra é projetada sem sombras dinâmicas.
- Comparação A/B isolada da assistência horizontal (item T06-4).
- Aceite agregado de T07 e autorização de expansão.

## Reprodução

1. Build: com o Editor fechado,
   `Unity.exe -batchmode -quit -projectPath <projeto> -executeMethod YunaLegend.Illustrated25D.Preview.Editor.IllustratedPreviewBuild.Run -logFile <log>`.
2. Verificação: `YunaLegend.exe -previewRetryVerify <pasta> -previewHz 30|60|120 -screen-width 1280 -screen-height 720 -windowed -logFile <log>`,
   com a janela em primeiro plano. Para áudio, acrescentar `-previewAudioCapture`.
3. Análise de áudio: `python tools/analyze_t06_audio.py <pasta>` (a correlação usa
   numpy quando disponível; sem numpy o script mantém as medidas de amplitude).
4. Testes: `Unity.exe -batchmode -nographics -runTests -testPlatform EditMode|PlayMode ...`.
5. Manifesto: `python tools/make_t06_consolidation.py`.

## Arquivos e integridade

- Build: `Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe`
  (SHA-256 `fbd073ca59c4918edbc6704f5393cf59021173af11714c00db4212c50117492b`).
  A build aprovada da T06.3 foi preservada em
  `Build/Windows/YunaLegend-Acompanhamento-T063-aprovada/` antes do rebuild.
- Fontes instrumentadas: `PreviewRetryVerification.cs` (opt-in) e
  `PreviewAudioCapture.cs` (novo, só cria listener com a flag); nenhuma mudança em
  gameplay, arte, física ou câmera.
- Cena `DarkRoom_IllustratedPreview` regenerada pelo exportador (IDs locais
  renovados); conteúdo equivalente ao aprovado, sem edição manual de física/arte.
- O `git diff --check` foi limpo após normalizar espaços à direita nas linhas
  `m_Name:` vazias geradas pelo Unity (somente espaços; nenhum valor YAML mudou).

## Aceite

O aceite técnico registrado nesta página em 13/11/2026 foi **invalidado no mesmo
dia** pelo coordenador, após a confirmação da regressão de contato (item T06-9).
**T06 está reaberta** e depende de uma nova microentrega de ancoragem na penumbra,
revisada pelo usuário. As aprovações de T06.1–T06.3 e das microentregas de T05
continuam como base histórica; esta consolidação não as reescreve. T07 só se
encerra com resposta humana explícita ao [pacote de revisão](../T07/README.md),
que permanece **bloqueado** até a correção.
