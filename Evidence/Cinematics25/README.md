# Continuidade — remake de todas as cinemáticas a 25 FPS

## Checkpoint — 23/11/2026 (`pausado, por enquanto`)

**Status:** **`pausado, por enquanto`** (a pedido do usuário; branch `cinematics` mesclada em `main` e encerrada).

Sessão ativa a assumir quando o trabalho for retomado via `handover --from-session 20261122-antigravity-cin-orquestrador`: **`20261122-antigravity-cin-orquestrador`** (registros **`129`, `130`, `133`, `134`** no `agent_registry.py` e tarefa **`CIN01`** no `plan_25d.py`). Estado estruturado também salvo em **`checkpoint_state.json`** na raiz do repositório.

- **Aprovado e intocável (não alterar):** estudos parciais `UnityStudyF000-042ActingV6` e `UnityStudyF000-058V2` + todos os 59 frames `F000–F058` listados em `Evidence/Cinematics25/study-manifest.json` (aceite humano na entrada `1465` do diário). Nenhuma cena completa tem `LOCK_APPROVED` ainda.
- **Avaliação do Agente 1 sobre `F059–F074 v1` (`yuna-f059-074-v1/`):** **`[REPROVADO: SEG_01]`** (diário `1489`):
  1. `F058→F059`: pop de escala (~7px menor na saia, sapatos deslocados), traço suave e paleta lavada; redesenhar `F059–F061` na escala/linha/saturação de `F058` antes de entrelaçar dedos.
  2. `F064→F065`: olhos e cabeça giram juntos; olhos devem iniciar busca em `F063` e cabeça acompanhar em `F065` (repetir atraso de 2 frames na volta `F069→F071`).
  3. `F059–F074`: coque, elástico rosa e franja travados; desenhar arrasto de 2–5 frames após giro da cabeça (assenta em `F067–F070`).
  4. `F065–F074`: laço lateral e barra da saia sem ondulação leve (2–4 frames) ao recolher braços.
  5. `F071→F072`: figura salta ~5–7px para a esquerda e coque alarga; fixar pés no mesmo ponto.
  6. `F066–F074`: pausa sem respiração; desenhar ciclo leve de ombros/peito (~12–16 frames) sem mudar escala.
- **O que não foi gerado ainda:** `yuna-f059-074-v2/` não existe (geração bloqueada por `429 RESOURCE_EXHAUSTED` no gerador de imagens e cota individual do subagente, diário `1491` e `1493`). Manifesto auxiliar `study-manifest-f000-074.json` e `.venv-cin/` (Pillow + numpy + imageio-ffmpeg, ignorado em `.git/info/exclude`) ficaram prontos.
- **Próximo passo exato:** assumir `129/130/133/134` e `CIN01` via handover; produzir **`F059–F074 v2`** em `Evidence/Cinematics25/yuna-f059-074-v2/` ancorado em `yuna-f052-058-v2/f058.png` (plano sugerido: 4 folhas 2×2 — `F059–062`, `F063–066`, `F067–070`, `F071–074` — com remoção de fundo/vão entre pernas + `prepare_sheet.py --columns 2 --rows 2 --count 4`); reenviar ao Diretor Criativo (Agente 1); com aprovação do trecho, capturar `UnityStudyF000-074V1` e seguir `SEG_02–SEG_06`, Nanda azul, áudio, integração, WebGL, vídeos `FULL`/`SEGMENTS` e demais cenas (`CIN02–CIN04`).
- **Ambiente limpo:** timer agendado (`task-83`) cancelado; todos os subagentes encerrados; nenhum Unity Editor ou servidor ativo. Alterações locais preservadas no checkout central (sem commit/push nesta sessão).

---

## Estado vigente anterior — retomada Antigravity, 22/11/2026

Sessão **`20261122-antigravity-resume-cin01`**, após handover humano explícito da sessão `20261122-codex-resume-cinematics` nos registros **129/130/133/134** e na tarefa **CIN01**, e após aceite humano individual do estudo `ActingV6 F000–F042` registrado na entrada **1459** do diário.

**Estudo F000–F058 (`UnityStudyF000-058V2`) LIBERADO pelo Diretor Criativo e APROVADO pelo usuário (entrada `1465` do diário).** Retomada de `F059–F074` bloqueada por esgotamento temporário de cota do gerador de imagens (`429 RESOURCE_EXHAUSTED` em `gemini-3.1-flash-image`, reset previsto em ~4h30m).
- A V1 (`UnityStudyF000-058V1`) foi reprovada pelo Diretor em caixa-preta por pop das mãos em `F046 -> F047`, flicker abre/fecha em `F047–F051` e ombros estáticos.
- A V2 preservou integralmente os 43 desenhos aprovados de `ActingV6` (`F000–F042`) e produziu `F043–F058` em 3 folhas dedicadas (`yuna-f043-047-v2`, ponte 2×2 `yuna-f048-051-v2` e assentamento `yuna-f052-058-v2`), com limpeza de fundo/vão entre pernas (`white_to_alpha.py`), escala uniforme por folha e registro pelas solas (`prepare_sheet.py`).
- [Vídeo do estudo — 59 quadros, 25 FPS, 2,36 s, sem áudio](UnityStudyF000-058V2/study-25fps.mp4) — **aceite humano individual registrado** (restrito ao estudo parcial `F000–F058`, não equivale à aprovação da cinemática completa).
- [Parecer independente e limites](UnityStudyF000-058V2/creative-review.md): **[LIBERADO ESTUDO F0-58]**, baseado em PNGs brutos Unity, sem reprodução de vídeo ou áudio. Não equivale à aprovação da cinemática completa.
- [Recibo Unity](UnityStudyF000-058V2/receipt.json) e [hashes/fontes](UnityStudyF000-058V2/source-integrity.json): **59/59 capturas, zero exposições ausentes**; intervalos reais **29,9397–50,0819 ms**, média **40,1424 ms**.
- Proveniência: `f043-058-v2-native-generation.json` (e `f043-058-v1-native-generation.json` preservada). `study-manifest.json` permanece íntegro apontando para `UnityStudyF000-058V2`.

**Próximo passo:** após reset de cota do gerador de imagens (ou via handover explícito ao Codex), produzir **`F059–F074`** (final do Plano 1 *Espaço ocupado*: *dedos entrelaçam; olhar procura lugar livre*, com precedência de olhos em 2 frames antes da cabeça) ancorado em `yuna-f052-058-v2/f058.png`, capturar `UnityStudyF000-074V1` (75/75) e submeter ao Diretor Criativo.

---

## Checkpoint histórico anterior — retomada Codex, 22/11/2026

Sessão **`20261122-codex-resume-cinematics`**, após handover humano explícito dos registros **129/130/133/134** e de **CIN01**. Branch `cinematics`; base recebida `0b385d7`. O checkpoint histórico do Antigravity abaixo está preservado, mas esta seção governa a retomada.

**Estudo F000–F042 liberado pelo Diretor Criativo na ActingV6; revisão humana individual pendente.** Concluídas as folhas F027–F042 que faltavam, seguidas das revisões ActingV4, V5 e V6. A V6 preserva 36 desenhos da V5 e substitui apenas F018 e F031–F036. Os brutos, prompts e registros por folha permanecem nas versões irmãs. A geração usou `image_gen.imagegen` nativa; nenhum erro de quota dessa ferramenta ocorreu.

- [Vídeo do estudo — 43 quadros, 25 FPS, 1,72 s, sem áudio](UnityStudyF000-042ActingV6/study-25fps.mp4).
- [Parecer independente e limites](UnityStudyF000-042ActingV6/creative-review.md): **[LIBERADO ESTUDO F0-42]**, baseado em PNGs, sem reprodução do vídeo pelo Diretor. Não equivale à aprovação da cinemática completa.
- [Referência de identidade à esquerda e captura Unity à direita](UnityStudyF000-042ActingV6/reference-vs-unity-f042.png).
- [Recibo Unity](UnityStudyF000-042ActingV6/receipt.json) e [hashes/fontes](UnityStudyF000-042ActingV6/source-integrity.json): **43/43 capturas, zero exposições ausentes**; intervalos reais **30,0323–49,7916 ms**, média **40,167219 ms**. O vídeo CFR25 não comprova pacing perfeito do runtime.
- Proveniência: `acting-v3-native-generation.json` até `acting-v6-native-generation.json`. Registro com uma escala uniforme por folha e ancoragem das solas; sem deformação anatômica ou composição de rosto sobre corpo fixo.

**Próximo passo:** revisão humana individual deste estudo. Após aceite, continuar **F043–F058**, desenvolvendo progressivamente o cedimento dos ombros e a aproximação das mãos ao peito conforme roteiro, preservando o alvo ocular e o movimento secundário contido. Não inferir aceite por silêncio. CIN01 tem 625 frames previstos; faltam F043–F624 de Yuna, Nanda, áudio, integração e validação WebGL, além da produção visual das demais cinemáticas. O fluxo original do jogo continua ativo.

Não houve alteração de C# nesta retomada; os **61/61 EditMode** são a evidência anterior do Antigravity, não uma nova execução. Os Editors iniciados para as quatro capturas foram encerrados pelo harness; nenhum servidor foi iniciado. Nenhum commit/push nesta retomada. Reservas principais ficam abertas para revisão. `plan_25d.py check` continua falhando pela ausência histórica de `Evidence/25d/T00/README.md`; o resolvedor de arquivos antigos também encontra o manifesto de Archive ausente. Esses itens não foram reconstruídos artificialmente.

---

## Checkpoint histórico do Antigravity

Atualizado em **22/11/2026** (checkpoint de transição **Antigravity → Codex**, sessão ativa `2026-11-22-antigravity-cinematics25`). Este documento é o índice de evidências e o estado de retomada; o registro sequencial obrigatório permanece exclusivamente em `docs/ia/diario.sqlite3`.

**Estado real: PRONTO PARA HANDOVER AO CODEX. Nenhuma cinemática refeita recebeu [APROVADO] na build WebGL final.** A abertura original continua ativa no jogo. Todos os **4 roteiros de direção principais (CIN01–CIN04)** e todas as **4 timelines Core C# puras a 25 FPS** estão implementados e validados (`61/61` testes EditMode passando). Em `CIN01`, o estudo parcial `F000–F042` avançou duas iterações (`ActingV2` capturado/auditado e `ActingV3` concluído de `F000–F026` com recorte/halo limpo); apenas as Folhas 4 (`F027–F035`) e 5 (`F036–F042`) de `ActingV3` ficaram pendentes por esgotamento de cota do gerador de imagens (`429 RESOURCE_EXHAUSTED`).

## Ponto de retomada para o Codex (22/11/2026 — pós-Antigravity)

- **Sessão ativa a assumir via `handover` explícito:** **`2026-11-22-antigravity-cinematics25`** (jobs abertos `129`, `130`, `133`, `134` no `agent_registry.py` e tarefa `CIN01` no `plan_25d.py`).
- **O que foi concluído nesta sessão (Antigravity):**
  1. **Auditoria de `UnityStudyF000-042UniformV1`:** Diretor Criativo aprovou proporções infantis, acabamento aquarelado e apoio dos pés, mas **REPROVOU** por corpo congelado abaixo do pescoço, falta de respiração (`F0–18`), falta de varredura de olhos com precedência de 2 frames (`F19–39`) e falta de cedimento de ombros/mãos (`F40–42`).
  2. **Produção e auditoria de `UnityStudyF000-042ActingV2`:** 5 novas folhas 3×3 (`yuna-f000-008-acting-v2` a `yuna-f036-042-acting-v2`), capturadas no Unity `-batchmode` (`43/43`, `0` perdas). O Diretor **aprovou** a respiração em `F000–F017` (incluindo piscada em `F013`), a precedência de olhos em 2 frames (`F018–F021`), o modelo/aquarela e a pose emocional em `F041–F042`, exigindo **4 correções pontuais**: (a) remover halo branco e fundo branco preso entre as pernas em `F0–42`; (b) neutralizar sorriso em `F17–24`; (c) ampliar varredura ocular + *overlapping* em `F27–35`; (d) suavizar elevação dos braços/mãos em arco contínuo `F38–42` (sem *pop* em `F39→40`) com pés 100% travados em `F40–42`.
  3. **Início de `ActingV3` (`F000–F026` prontos):** concluídos `yuna-f000-008-acting-v3/`, `yuna-f009-017-acting-v3/` (com boca `f017.png` neutralizada) e `yuna-f018-026-acting-v3/` (zero sorriso, precedência de olhos preservada), todos com limpeza total de halo branco e do vão entre as pernas/meias (`pageScale 1.4717–1.4843`, `0` clipping). **Bloqueio 429** impediu apenas a geração das Folhas 4 (`F027–F035`) e 5 (`F036–F042`) e a captura `UnityStudyF000-042ActingV3`. (`study-manifest.json` continua íntegro apontando para `UnityStudyF000-042ActingV2`.)
  4. **Passo 1 concluído para CIN02, CIN03 e CIN04:** capturas brutas da build WebGL de referência arquivadas em `Original/DarkRoom/` (3 PNGs), `Original/Arena/` (5 PNGs) e `Original/BearChase/` (6 PNGs). O Diretor Criativo em caixa-preta redigiu os 3 novos roteiros completos a 25 FPS:
     - **[CIN02-direcao.md](CIN02-direcao.md)** (*O Quarto Escuro* — Despertar com Nanda, `300 frames` / 12s, 4 planos, 3 falas literais de Nanda)
     - **[CIN03-direcao.md](CIN03-direcao.md)** (*Arena Imaginária* — Vinheta/Cut-in do Golpe Especial Arco-Íris, `75 frames` / 3s, 3 planos, sem legendas)
     - **[CIN04-direcao.md](CIN04-direcao.md)** (*Perseguição ao Urso* — Aproximação, Toque, Despertar e Fuga, `350 frames` / 14s, 4 planos, pantomima sem legendas)
  5. **Core C# puro a 25 FPS + Testes EditMode (`61/61 Passed`):** implementados `DarkRoomHandDrawnTiming.cs`, `ArenaSpecialHandDrawnTiming.cs` e `BearChaseHandDrawnTiming.cs` (`noEngineReferences: true`, sem alterar fluxos legados) e suas suítes NUnit cobrindo 30/60/120 Hz, retenção de 40 ms, fronteiras de planos e legendas. Relatório em **`Validation/timelines-cin01-04-editmode.xml`** (`total=61, passed=61, failed=0, skipped=0`).
- **Próxima ação exata para o Codex:**
  1. Executar `handover` dos jobs `129`, `130`, `133`, `134` e de `CIN01` a partir da sessão `2026-11-22-antigravity-cinematics25`.
  2. Gerar as **Folhas 4 (`F027–F035`)** e **5 (`F036–F042`)** de `acting-v3` (aplicando a mesma limpeza de halo branco e do vão entre as pernas usada em `F000–F026`, ancorando em `yuna-f018-026-acting-v3/f026.png` + modelo `yuna-f000-008-detail-v1/f000.png`), atualizar `study-manifest.json` para `UnityStudyF000-042ActingV3`, capturar no Unity `-batchmode` e submeter os renders brutos ao Diretor Criativo.
  3. Assim que o Diretor liberar `F000–F042`, prosseguir com `F043–F624` de Yuna, atuação de Nanda azul, áudio, integração na cena, build WebGL e auditoria final `[APROVADO]` de `CIN01` antes de executar `CIN02`, `CIN03` e `CIN04`.
- **Ambiente limpo:** nenhum Unity Editor, servidor WebGL ou gerador de imagens está ativo; `UNITY-YunaLegend.slnx` restaurado; branch `cinematics` sincronizada com `origin/cinematics`.

---

## Histórico do checkpoint anterior (interrupção inicial do Codex por quota)

- O contador nativo de uso confirmou **95% consumidos / 5% disponíveis** na janela de 300 minutos. O usuário autorizou expressamente parar os agentes, atualizar o diário e enviar as alterações ao GitHub nesse limiar. A implementação foi suspensa; o Diretor foi interrompido durante a revisão, sem aprovação inferida.
- Última candidata de **estudo**: `UnityStudyF000-042UniformV1/`, 43 desenhos capturados e zero exposições ausentes. Ela uniformiza definição de F0–24 e modelo de F25–42. **Revisão criativa pendente** das fronteiras F8→9, F17→18, F24→25 e F33→34.
- O manifesto atual `study-manifest.json` aponta para esses 43 arquivos, distribuídos em `yuna-f000-008-detail-v1/`, `yuna-f009-017-detail-v1/`, `yuna-f018-024-detail-v1/`, `yuna-f025-033-uniform-v1/` e `yuna-f034-042-uniform-v1/`.
- O parecer anterior reconheceu as proporções corrigidas em F25–33, mas exigiu melhorar definição de F0–24 e redesenhar F34–42 no mesmo modelo. Essas correções estão na prova uniforme acima; **não considerá-las aprovadas sem nova auditoria**.
- Próxima ação: retomar a mesma sessão/reservas e pedir ao Diretor que conclua a revisão dos renders brutos da prova uniforme. Se reprovado, corrigir o defeito específico; se liberar a continuação do estudo, produzir F43–49 conforme roteiro. Ainda faltam 582 desenhos de Yuna, toda a atuação de Nanda, áudio, integração e todas as demais cinemáticas.
- Todos os Editors e gerações de imagem estão encerrados. O servidor próprio 8087 foi encerrado; o comando de reinício está abaixo. O viewport temporário foi restaurado. A aba de inspeção já não existia na limpeza.
- **16/16 EditMode e 3/3 testes de regressão dos landmarks passaram.** Resultados versionáveis em `Validation/timing-editmode.xml` e `Validation/landmark-tests.txt`.
- Registros 129/130/133/134 e CIN01 permanecem abertos com checkpoint. O registro de publicação 136 refere-se somente ao envio do checkpoint, não à conclusão das cinemáticas. O diário contém a autorização e o motivo da interrupção.
- Nenhum novo lote F43+ foi produzido. Nenhuma cena ou build de produção foi substituída/publicada; o push contém trabalho em andamento e evidências.

Este README contém detalhes de engenharia e destina-se ao Executor. O Diretor deve receber somente roteiro, instruções de reprodução e renders brutos; não encaminhar fontes, hierarquias, logs ou este documento técnico a ele.

## Objetivo e contrato que continuam válidos

- Refazer **todas** as cinemáticas do projeto Unity: desenho tradicional 2D, traço orgânico, aquarela/pintura artesanal e atuação expressiva em full animation a 25 FPS.
- Exatamente dois papéis: Diretor Criativo em caixa-preta e Engenheiro Executor. O Executor não muda decisões criativas sem o Diretor. Não abrir outros agentes.
- O Diretor observa a execução/render, escreve o roteiro e audita cada candidata. Nenhuma cena pode ser encerrada sem seu parecer formal [APROVADO]; a tarefa geral só termina com todas aprovadas.
- Não substituir atuação por translação/deformação de recortes ou troca de rosto sobre corpo imóvel. Preservar Yuna e Nanda azul.
- Implementação nativa Unity/C#. Legado, se necessário, somente por observação da URL autorizada; **não foi consultado nesta execução** e nenhum código legado foi copiado.

## Base e coordenação

Checkout central: `C:/Users/sergi/repos/UNITY-YunaLegend`. Unity `6000.3.28f1`.

Foi confirmado com `git fetch origin main` que a branch `cinematics` e `origin/main` estavam no mesmo commit `390bf53c947b644fdfc28ae1ab93212224f63e57`, com divergência `0 / 0`. Não houve commit, push, merge ou mudança de branch nesta tarefa. O usuário pediu depois para deixar essa verificação resolvida e concentrar a implementação.

Sessão única a retomar: **`20261122-codex-executor-cinematics`**. Não assumir outra sessão nem criar revisão para contornar reservas.

| Registro | Assunto | Estado de continuidade |
| --- | --- | --- |
| 129 | `cinematica/remake-2d-25fps-v1` | Aberto; escopo geral e evidências |
| 130 | `cinematica/webgl-inspecao-v1` | Aberto; exportação local de referência |
| 133 | `cinematica/toyroom-intro25-v1` | Aberto; recursos de CIN01 e implementação |
| 134 | `cinematica/toyroom-prova-atuacao-v1` | Aberto; adaptador e harness de estudo |
| 131 / 132 | Configurações temporárias / cadastro no plano | Encerrados com resultado explícito |

O coordenador `plan_25d.py` tem **CIN01 ativa nesta mesma sessão**. Ela foi adicionada com autorização do pedido atual e dependência da direção visual T01; **não aceita nem modifica T06/T07/T08 anteriores**. O plano antigo 1338, Piquenique, permanece independente e não foi executado.

`plan_25d.py check` global encontrou uma evidência histórica ausente (`Evidence/25d/T00/README.md`). Não resolver inventando evidência ou aceites; cadastro/claim de CIN01 validaram a estrutura e foram bem-sucedidos.

## Inventário inicial a completar

As cinco cenas habilitadas da exportação são o menu `TitleScreen` e quatro fases. A classificação abaixo ainda requer inspeção narrativa individual; **somente a abertura de ToyRoom foi reproduzida para direção nesta execução**.

| Cena canônica | Sequências identificadas para inspeção/remake |
| --- | --- |
| `Fase-Jogavel-Completa` — Quarto Escuro | Despertar/abertura com Nanda; evento do castelo; Porta Falsa/queda/recuperação; portal verdadeiro/desfecho |
| `ToyRoom_Gameplay` — Sala de Brinquedos | Abertura (**CIN01**); brinquedos reaparecidos/diálogo estranho; investigação/revelação da espada; recolher espada/transição |
| `Arena_Gameplay` | Vinheta do golpe especial; conferir visualmente se os desfechos de combate são cinemáticas ou somente UI |
| `BearChase_Gameplay` | Aproximação/toque/despertar/fuga; captura/desfecho; classificar também tropeço/queda narrativa |

Não declarar o inventário exaustivo antes de jogar/observar os demais gatilhos.

## O que foi efetivamente feito

### Referência WebGL e inspeção original

- Compilada exportação real das cinco cenas atuais, com `WebCiBuild`, em `Build/Cinematics25Web`; log `Logs/cinematics25-webgl-build.log`, término com código 0 e marcador `YUNA_WEB_PAGES_OK`.
- Arquivos reais da build: data 156.993.014 bytes, wasm 11.903.765 bytes, além de framework/loader. Os arquivos WebGL versionados em outra pasta eram ponteiros LFS; a inspeção usou a nova exportação real.
- Servidor em loopback: **`http://127.0.0.1:8087/`**. Respondeu HTTP 200.
- A abertura de ToyRoom foi reproduzida duas vezes até o gameplay `Brinquedos Arrumados: 0/8`; capturas brutas em `Original/`.
- O navegador funcionou no root, mas não estava disponível no subagente Diretor. O root operou a reprodução e forneceu **somente renders** ao Diretor. Ele não leu fontes ou hierarquias.
- O Diretor leu as falas e os cartões nas capturas 10–16. Não ouviu áudio nem afirmou ter observado movimento contínuo.

### Direção definitiva da primeira abertura

O roteiro autorizado está em **[CIN01-direcao.md](CIN01-direcao.md)**. Ele é a especificação criativa; não substituí-lo por sugestões do Executor.

- 625 frames, contados de 0 a 624: **25 segundos a 25 FPS**.
- Seis planos: 0–74, 75–174, 175–274, 275–399, 400–524, 525–624.
- Preservar o fundo aquarelado atual; redesenhar atuação de Yuna e Nanda.
- Manter literalmente `Aqui também...`, `Eu queria espaço pra brincar.` e `Vamos. Um de cada vez!` nos intervalos especificados no roteiro.
- Por decisão explícita do Diretor, retirar os dois cartões narrativos e comunicar lotação/isolamento pela atuação; usar nome Yuna em vez de Menina e legendas inferiores sem instruções de avanço.
- Olhos precedem cabeça em 2 frames; cabelo, laço e roupa seguem 2–5 frames. Asas desenhadas mudam perspectiva e curvatura.
- Som novo está especificado como proposta no roteiro, pois o original não foi ouvido. Ainda não foi implementado.

### Código implementado, ainda sem ativar o remake

| Arquivo | Implementação efetiva |
| --- | --- |
| `Assets/Scripts/Runtime/ToyRoom/Core/ToyRoomIntroTimeline.cs` | Novo `ToyRoomHandDrawnTiming`: 25 FPS, 625 frames, seis planos e intervalos literais de legendas. C# puro, asmdef sem engine. A timeline original foi preservada. |
| `Assets/Scripts/Runtime/ToyRoom/Runtime/HandDrawnExposurePlayer.cs` | Adaptador fino que apresenta sprites discretos, emite eventos e informa exposições perdidas. Não deforma desenhos. Ainda não está conectado à abertura de produção. |
| `Assets/Editor/Cinematics25StudyCapture.cs` | Harness de estudo em Play Mode que cria cena somente em memória, aquece render, lê manifesto de desenhos, captura em memória e salva depois. Não cria/salva cena do projeto nem muda o menu. |
| `Assets/Scripts/Tests/EditMode/ToyRoom/ToyRoomIntroTimelineTests.cs` | Testes do novo clock/legendas e preservação dos testes da abertura original. |
| `prepare_sheet.py` | Fatiamento de células, uma escala uniforme por página a partir da pose de continuidade e registro pelas solas; não ajusta a altura de cada pose individualmente. **As passagens ainda falharam visualmente; esse procedimento precisa ser auditado, não presumido correto.** |
| `study-manifest.json` | Dados de entrada da prova atual: caminhos dos 43 desenhos F0–42. Não é diário ou registro de atuação. |

### Arte e proveniência

Foi usada a ferramenta built-in `image_gen`, com referências locais, sem API/CLI de geração de imagens. Não houve composição vetorial como substituto de pintura.

| Material | Estado |
| --- | --- |
| `yuna-f000-024-raw-v1.png` | Rejeitado por atuação e alpha sem transparência total; preservar como estudo descartado |
| `yuna-f000-024-raw-v2.png` | Corrigiu mãos/boca; final do olhar rejeitado |
| `yuna-f000-024-raw-v3.png` e `yuna-f000-024-v3/` | 25 desenhos que puderam orientar continuação; ainda sem aprovação de fluidez/definição final |
| `yuna-f025-033-raw-v1.png` | Maior resolução por desenho, mas trajetória de abatimento rejeitada |
| `yuna-f025-033-raw-v2.png` e `yuna-f025-033-v2/` | Busca lateral corrigida; folhas permitiram continuar, mas render posterior reprovou transição F24→25 |
| `yuna-f034-042-raw-v1.png` e `yuna-f034-042-v1/` | Pausa e início do gesto; render posterior reprovou transição F33→34 |

São **43 desenhos de estudo de Yuna**, não 43 frames finais aprovados. F0–24 foram fatiados em canvas 256×256; F25–42 em 512×512. Os desenhos brutos e os registros de paginação estão preservados. Fontes de identidade: `girl-acting-v1.png`; fundo: `room-v1.png`, ambos em `Assets/Art/ToyRoom/Introduction/Resources/ToyRoomIntro/`.

As primeiras folhas eram 5×5; a resolução por desenho ficou suave. As posteriores passaram a 3×3 para melhorar definição. Essa mudança de lote revelou incompatibilidades, descritas no veto abaixo. A remoção de fundo do primeiro estudo foi cancelada quando a atuação dele foi rejeitada; não há ativo final associado àquela chamada.

Os prompts literais completos permanecem nas chamadas desta conversa. Para retomada sem contexto, o roteiro e os pareceres deste documento são a fonte de reconstrução; não confundir um resumo de intenção com prompt literal. Não promover os estudos a assets finais sem guardar a proveniência final correspondente.

## Validações reais e limites

- **16/16 testes EditMode passaram**, 0 falhas, em `Build/Cinematics25WebControl/timing-editmode.xml`. Incluem cobertura de todos os 625 índices em simulações 30/60/120 Hz, exposições lógicas de 40 ms, legendas e compatibilidade da timeline original.
- `UnityStudyV3/`: primeira prova de 25 desenhos, com custo de aquecimento/codificação alongando o início. Todos os desenhos estavam presentes; uma entrada do diário inicialmente interpretou isso como perda e foi corrigida por entrada append-only.
- `UnityStudyV3Pass2/`: 25 desenhos, zero exposições ausentes; intervalos reais 29,869–50,004 ms, média 40,195 ms. Vídeo nominal a 25 FPS, 1 segundo, sem áudio.
- `UnityStudyF000-042/`: 43 desenhos, zero exposições ausentes; intervalos reais 29,646–50,037 ms, média 39,896 ms. Vídeo nominal a 25 FPS, 1,72 segundo, sem áudio.
- **CFR 25 FPS do vídeo não prova hardware apresentando cada desenho exatamente por 40 ms.** A variação real está nos receipts. Não declarar o requisito rigoroso de reprodução concluído.
- O Diretor examinou frames brutos; declarou que **não assistiu aos MP4s**. Não atribuir escuta, percepção contínua ou aprovação a ele.
- As provas são estudos de um ator sobre fundo, não a cinemática completa, gameplay, áudio ou validação de dispositivos.

## Parecer que motivou a suspensão dos novos lotes

**[REPROVADO — continuidade deste estudo parcial]. Suspender novos lotes até corrigir as duas passagens.**

O Diretor examinou renders 22–27, 31–36 e 39–42:

1. **F24→25:** silhueta fica estreita; cabeça e rosto proporcionalmente menores; pernas parecem mais longas; acabamento muda abruptamente de suave para definido. Parece troca de desenho-base.
2. **F33→34:** Yuna encolhe e as solas sobem no tapete. F35 volta ao chão, mantendo tamanho menor que F33.
3. F39–42 não mostrou novo problema gestual evidente nas capturas, mas isso não compensa as rupturas.

Instrução do Diretor: conservar as **proporções infantis de F0–24**, com cabeça grande e corpo curto, trazendo a definição nova para esse mesmo modelo. Manter posição e tamanho dos pés; a ação não justifica deslocá-los. Corrigir e reapresentar **F22–27 e F31–36** antes de continuar. As autorizações anteriores das folhas eram somente para produzir/avaliar estudos; não eram aprovação da continuidade.

## Próximo passo exato e trabalho restante

1. Retomar as reservas existentes e ler este estado, o roteiro e os renders rejeitados. **Não gerar F43+ ainda.**
2. Diagnosticar tecnicamente escala, alpha/landmarks, registro das solas e diferenças reais de proporção nas passagens F24→25 e F33→34. O normalizador atual não deve ser considerado validado apenas porque os números de sola coincidem.
3. Corrigir os desenhos e/ou a paginação respeitando o modelo infantil do lote inicial. Melhorar definição de F0–24 para não haver salto de acabamento. Não corrigir proporção com achatamento/alongamento de recortes. Se a correção mudar atuação/enquadramento, obter decisão do Diretor.
4. Refazer prova nativa com os mesmos enquadramentos e enviar somente renders F22–27/F31–36 ao Diretor. Preservar os ensaios rejeitados e usar outro diretório de saída. Continuar somente após o veto ser resolvido.
5. Completar os **625 desenhos de Yuna** e a atuação de **Nanda**, que ainda não tem novo lote produzido. Cumprir todos os planos, lábios/expressões, overlapping e contato de pé do roteiro.
6. Implementar sons/vozes ou obter do Diretor decisão explícita sobre alternativa fiel. Não há áudio novo nesta candidata. A busca opcional de app temporal pelo Pinokio falhou porque o control plane recusou conexão; nenhum app foi instalado/iniciado. Não usar isso como justificativa para substituir desenho por deformação mecânica.
7. Integrar a sequência completa à abertura real, preservando APIs/eventos/gameplay. O novo clock e o player atualmente **não substituem** o fluxo original. Completar legendas inferiores, câmera, Nanda, sombras, áudio e retorno ao gameplay.
8. Validar EditMode/PlayMode e reprodução real a 25 FPS; não confundir frequência de simulação, captura, vídeo e apresentação. Conferir pausa, salto/avanço permitido, conclusão e retorno ao jogo.
9. Gerar candidata WebGL separada da referência original e solicitar revisão integral ao Diretor, sem código/explicações técnicas. Repetir ajustes até [APROVADO] real.
10. Só depois seguir o ciclo individual para todas as demais cinemáticas do inventário. **Nenhuma foi considerada concluída.**

### Diagnóstico técnico iniciado após este checkpoint

O detector anterior incluía resíduos opacos isolados abaixo dos sapatos de F34. Isso contaminava a altura usada para a escala da página e a âncora da sola. `prepare_sheet.py` agora mede somente o maior componente opaco conectado, sem modificar os pixels/alpha da pintura. Continua usando uma única escala por página, sem ajustar individualmente a altura de cada pose.

Foi repetida a prova com **os mesmos 43 desenhos**, sem nova geração: `yuna-f000-024-v4/`, `yuna-f025-033-v3/` e `yuna-f034-042-v2/`, todos com canvas 512. A escala da última página mudou de 1,077830 para 1,177835 após remover o resíduo da medição. O manifesto atual aponta para `UnityStudyF000-042RegistrationV2/`. A prova capturou 43 desenhos sem ausências; intervalos reais 29,7455–50,0781 ms, média 39,9249 ms. Plano **1397**, resultado **1398**.

**Parecer mais recente do Diretor:** a correção de apoio em **F33→34 funcionou nas capturas**, sem o encolhimento grosseiro anterior. Há pequena mudança na barra a conferir em movimento. **O veto permanece em F24→25**, por modelo mais estreito, cabeça/rosto menores, pernas longas e salto de definição. Nenhuma aprovação da abertura e nenhum MP4 assistido pelo Diretor.

O plano **1399** foi concluído com o redesenho infantil F25–33. Depois foram uniformizados definição F0–24 e modelo F34–42, produzindo o estudo `UnityStudyF000-042UniformV1/`. O plano **1409** foi interrompido por quota durante a revisão do Diretor; o ponto de retomada no início deste README prevalece sobre os estados históricos desta seção.

## Reprodução e arquivos úteis

Na raiz do checkout, se o servidor já estiver ativo, reutilizá-lo:

```powershell
node tools/serve_web_preview.cjs Build/Cinematics25Web 8087
```

A URL é a **referência atual sem o remake integrado**. No menu, a bandeira seleciona fases; escolher Quarto dos Brinquedos inicia a abertura. A aba IAB usada pelo root tinha browser canônico `2`, tab `1`; IDs podem ficar obsoletos, consultar o contexto atual antes de usar. O subagente Diretor não tinha esse backend.

Para repetir o estudo atual, sem salvar cenas:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.3.28f1/Editor/Unity.exe' -batchmode -force-d3d11 -projectPath 'C:/Users/sergi/repos/UNITY-YunaLegend' -executeMethod YunaLegend.EditorScripts.Cinematics25StudyCapture.Capture -logFile 'C:/Users/sergi/repos/UNITY-YunaLegend/Logs/cinematics25-study-f000-042.log'
```

**Antes da repetição**, mudar `output` no manifesto para um diretório novo, preservando a prova rejeitada. Não abrir outro Editor no mesmo projeto. O harness também aceita o caminho de manifesto pela variável `YUNA_CIN_STUDY_MANIFEST`.

Para fatiar uma página de estudo:

```powershell
python Evidence/Cinematics25/prepare_sheet.py ARQUIVO_BRUTO QUADRO_ANTERIOR PASTA_SAIDA --start PRIMEIRO_FRAME
```

Esse script ainda precisa da auditoria mencionada; não usá-lo cegamente para corrigir os saltos.

Dependências de medição/codificação foram instaladas somente em `Build/Cinematics25WebControl/pythondeps`: Pillow 13.0.0 e imageio-ffmpeg 0.8.1. Sem instalação global. A saída de build e as dependências são artefatos locais, não substituem fontes/evidências versionáveis.

`git status` normal encontrou falha no filtro `git-lfs` do ambiente. Para leitura de estado foi usado:

```powershell
git -c filter.lfs.process= -c filter.lfs.required=false status --short
```

Não usar reset/clean para resolver diferenças.

## Cuidados operacionais para não perder trabalho

- Nenhum Unity está em execução no checkpoint. O servidor de referência, anteriormente PID **18268** e sessão **2881**, foi encerrado na parada por quota. Não reutilizar esse PID; reiniciar pelo comando documentado se necessário.
- Nenhuma geração de imagem ou app Pinokio está pendente. O Diretor terminou o parecer e está ocioso; reutilizar `/root/diretor_criativo`, sem abrir terceiro agente.
- **Não abrir o Editor interativo inadvertidamente:** um gerador preexistente de ToyRoom reconstrói assets ao iniciar. Isso ocorreu no começo; efeitos próprios foram identificados e restaurados seletivamente. O modo batch ignora esse gerador.
- Configurações alteradas automaticamente pela build foram restauradas, inclusive backup byte a byte de `ProjectSettings.asset`. Não há publicação nova.
- Unity reordena automaticamente `UNITY-YunaLegend.slnx`; comparar antes de restaurar somente essa alteração própria. Nunca descartar código, evidências ou o banco.
- Manter diário SQLite com append-only e pares planned/resultado. Não atribuir a si ações de outros agentes. Não finalizar 129/130/133/134 ou CIN01 por falta de contexto ou porque o estudo foi reprovado.
