# Estado atual — sala escura ilustrada

## T08F / T08G: travessia até o Tamborito de Marcha (05) — ACEITA (14/11/2026)

Sessões `2026-11-14-antigravity-tambor05` (registro 17, arte `T08F`) e
`2026-11-14-antigravity-tambor05-cena` (registro 18, integração `T08G`).
**Aceite individual** do usuário ([registro](../../Evidence/25d/T08G/aprovacao.md)).
[Build, cena, contrato de apoio e provas](../../Evidence/25d/T08G/README.md).

O Tamborito de Marcha (05) foi integrado após os blocos ABC (`T08E`) com superfície de
corrida/pouso sobre a pele plana superior (`y = 265` em `drum-05-v1.png`, ~95 px acima do
aro frontal), escala uniforme `146,1538 px/m` (`TopY = -4,55 m`, altura `4,55 m` no piso
`-9,10 m`), largura útil `4,1053 m`, vão `2,4020 m` e degrau `+0,6489 m`.
30/60/120 Hz passaram com 2 travessias, **6 saltos**, 1 estralo, reset limpo, zero erros,
zero legado e `pinVisibleSole` com gap `0,0000 m`. Cena nova em
`Drum/Preview-Continuacao-05.unity` e build separada `Build/Windows/YunaLegend-Continuacao-05`.

Próximo passo solicitado pelo usuário: sincronizar no GitHub e avançar preparando e
integrando as **duas próximas plataformas** (`06 Almofadão de Veludo` e `07 Gavetas da Cômoda`)
em conjunto. Estante segue adiada; T06/T07 globais, aparelho/WebGL/áudio/desempenho
continuam pendentes.

## T08E: travessia até os blocos ABC — ACEITA (14/11/2026)

Sessão `2026-11-13-dsh-abc04-cena`, registro 16, tarefa T08E. **Aceite individual**
do usuário: “Aceito, pode continuar.” ([registro](../../Evidence/25d/T08E/aprovacao.md)).
[Build, vídeo, cena e contrato de apoio](../../Evidence/25d/T08E/README.md).

A pilha ABC04 foi integrada depois da cômoda com apoio medido na pintura aprovada:
linha de pouso em y=125 (9 cm acima do lip), base em y=1177, escala uniforme
269,6690 px/m (10% menor), topo em −5,199 m, apoio próprio de 1,5130 m (34,2% do
conteúdo) e profundidade 1,59 m. A pilha assenta no piso (−9,10 m); vão até a cômoda
2,1510 m e degrau 0,851 m. Constantes em
[CONTRATO-APOIO-ABC04.md](../../Evidence/25d/T08E/CONTRATO-APOIO-ABC04.md).

30/60/120 Hz: duas travessias, **5 saltos**, 1 estralo, reset limpo, zero erros e
zero legado; pousos 22,937/22,864/22,855 dentro do apoio. Câmera aprovada preservada
(Z/FOV/escala constantes). Cena nova em `ABC/Preview-Continuacao-04.unity` e build
separada `Build/Windows/YunaLegend-Continuacao-04`; T08C e sua build têm hashes
inalterados.

Correções aprovadas: (1) vinheta de Idle (ordem 0) estava atrás da pintura (ordem 20);
a pintura passou para ordem −1. (2) A perna entrava na letra A; a linha voltou para a
borda do tampo. (3) A sola desenhada ficava 2,8 cm (Idle) e 4,9 cm (corrida) abaixo do
topo; `pinVisibleSole` prende a sola ao topo, com gap 0,0000 m nas três taxas. (4) O pé
ainda ficava no tampo; a linha subiu para y=125 e todo o friso ficou abaixo da linha do
pé. Root, cápsula, física e animações não mudaram.

Próximo passo: aguardar a definição humana da próxima microentrega (estante segue
adiada). A base não aparece no enquadramento aprovado quando Yuna está no alto; sola do
urso e demais defeitos continuam em T06; aparelho/WebGL/áudio/desempenho e aceite
global T06/T07 não foram validados. Publicado em `46b49a5` no branch
`codex/dark-room-2d` (push verificado; main não alterada).

## ABC04: arte conceitual aprovada; construção da cena adiada

O usuário aprovou `abc-stack-v1.png` e definiu expressamente: **“o entregável vai ser só a arte conceitual”**.
[Aprovação e escopo literal](../../Evidence/25d/T08D/aprovacao-conceitual.md).
A pintura está salva em `Assets/Art/Illustrated25D/Environment/ABC/abc-stack-v1.png`. T08D concluída no escopo conceitual.

**Construção da cena ABC04: POR FAZER — ADIADA PELO USUÁRIO.**
Não iniciar agora posicionamento, integração, apoio físico, salto ou nova build. Retomar apenas mediante novo pedido do usuário. A arte aprovada não valida contato ou dificuldade.
Build aprovada até a cômoda T08C preservada. Estante permanece adiada, sem aceite global T06/T07. As seções abaixo registram estados anteriores e não autorizam iniciar a construção adiada.

## Travessia até a cômoda03 aprovada — 13/11/2026

Usuário: **“aprovado me diga qual é o próximo passo”** após build/vídeoT08C.
[Aceite individual](../../Evidence/25d/T08C/aprovacao.md). T08C aceita e registro11
encerrado; provas anteriores preservadas, nenhum teste novo neste registro.
Próxima microentrega: **blocos ABC de madeira (04)** após a cômoda, definindo
primeiro apoios reais, depois arte e integração na câmera aprovada; validar salto,
pouso e retry. Ainda não iniciada; este pedido solicita informar o próximo passo.
Sem aceite agregadoT06/T07 ou conclusão da fase. Checkpoints abaixo são históricos.

## Continuação jogável até a cômoda03 — candidata em revisão

Usuário autorizou prosseguir após câmera aprovada. PreparaçãoT08A encerrada no
escopo de uso da candidata; registro9 liberado. Registro11/T08C implementa somente
a próxima travessia: **urso → livros → móvel → cômoda**.
[Build, vídeo, cena própria e provas](../../Evidence/25d/T08C/README.md).
30/60/120Hz: duas travessias por taxa,4saltos,1estralo e reset limpo,zero erros.
Sola opaca no novo apoio,60Hz:±1,35mm; compensação visual restrita à cômoda03.
Câmera aprovada preservada,semzoom. Móvel anterior/arte/física não redesenhados.
Cena nova: `Assets/Art/Illustrated25D/Environment/Dresser/Preview-Continuacao.unity`.
Build separada: `Build/Windows/YunaLegend-Continuacao/YunaLegend.exe`.
Próximo: revisão individual desta nova travessia, antes dos blocos ABC04.
Não inferir aceite agregadoT06/T07 ou conclusão da fase. Pedido externo de
retomada da estante (diário1016/1017) não foi assumido nesta microentrega.

## Câmera T08B aprovada — 13/11/2026

Usuário aprovou a câmera de subida sem zoom: **“aprovafo. gostei.”**
[Aceite literal](../../Evidence/25d/T08B/aprovacao.md). T08B aceita e registro 10
encerrado. Provas anteriores preservadas, sem novos testes ou commit/push.
Cômoda T08A continua em revisão individual (registro 9); estante/redesenho adiados.
A aprovação da fase permanece parcial, sem aceite agregado T06/T07.
As seções seguintes são checkpoints anteriores à aprovação da câmera.

## Câmera acompanha a subida com distância fixa — candidata entregue

Pedido explícito: **“Acompanhar a subida, mantendo a distância.”**
[T08B: vídeo, prova e reprodução](../../Evidence/25d/T08B/README.md), registro 10.
Agora o modo ilustrado acompanha também a altura de Yuna antes do pouso;
Z=−9,0533 m e FOV45° constantes, escala96px/m. Retry60 passou sem erros.
Vídeo nativo604×720,37,33s,semáudio; janela desktop,sem validação de aparelho.
Executável separado em `Build/Windows/YunaLegend-Camera/YunaLegend.exe`.
Revisão visual pendente; T08B e a arte da cômoda T08A têm reservas independentes.
Aceite parcial e adiamento da estante preservados. Nenhum aceite agregado T06/T07
ou integração da cômoda foi inferido. Sem commit/push.

## Continuidade: cômoda candidata e câmera sem zoom (13/11/2026)

Aceite parcial e estante adiada mantidos. Cômoda 03 preparada como imagem
individual em [T08A](../../Evidence/25d/T08A/README.md), sem integração alegada.
Registro 9 mantém arte/docs para revisão. Usuário priorizou o acompanhamento
vertical e confirmou **“Acompanhar a subida, mantendo a distância”**.
Microentrega T08B/registro 10 altera somente a câmera ilustrada, em recursos
independentes; sem zoom, redesenho da estante ou mudança de física/animação.
T08A/T08B são preparações individuais autorizadas; T08 principal mantém sua
dependência de T07, que não foi aceita globalmente.

## Aceite parcial; estante adiada e próxima peça autorizada (13/11/2026)

Usuário concedeu **aprovação parcial** e adiou o ajuste/redesenho da estante.
Pediu continuar a evolução para ter maior visibilidade da fase.
[Decisão literal e escopo](../../Evidence/25d/T06-contact/v4/aceite-parcial.md).
Não retomar a estante nesta etapa. Achados dos pés continuam registrados, sem
declarar correção ou aceite agregado T06/T07. Próxima microentrega: cômoda pequena
de carvalho, índice 3, com apoio definido antes da pintura e revisão individual.
Preparação visual isolada; não é conclusão ou aprovação global da expansão T08.

## Auditoria da sola — afastamento confirmado nas superfícies (13/11/2026)

Usuário relatou pé afastado do móvel e pediu análise das demais superfícies.
[Diagnóstico, cobertura e capturas](../../Evidence/25d/T06-contact/foot-audit/README.md).
Repouso no móvel: 7,91 cm; chão: ~8 cm; pouso nos livros: 6,86–7,78 cm.
Urso também apresenta afastamento, com dependência da curva e da posição da sola.
Causa: skinWidth0,08m e compensação visual ausente no Idle e dependente de
reviewSupport não atribuído em Run/Jump na integração. Tropeço já compensa na
maior parte; não aplicar deslocamento uniforme a todos os estados.
Instrumentação opt-in somente na build diagnóstica técnica; nenhuma correção
de animação/física aplicada. Build normal v4 e T06.3 preservadas.
Próximo: corrigir contato visual do pé de apoio por estado/superfície, com prova
de continuidade. Registro 8 permanece aberto; T06/T07 não encerradas.

## Contato v4 — referência corrigida para baixo do friso (13/11/2026)

Usuário pediu ajuste da v3: objetos acima do friso do chão. O inset anterior
82/724 apontava a linha 642 do panorama, dentro do acabamento da parede.
Agora a ancoragem usa 64/724, linha 660, abaixo da borda inferior do friso (~654).
[Candidata v4, capturas, provas e limites](../../Evidence/25d/T06-contact/v4/README.md).
Registro 8 e sessão `2026-11-13-codex-contact-penumbra` mantidos para revisão.
Física e apoios preservados. Exportação na cópia técnica com fontes conferidos,
pois o Editor central está em Play pausado; reiniciar Play nele para carregar
o alinhamento atualizado. Build candidata central atualizada, T06.3 aprovada preservada.
Próximo: revisão visual da v4 abaixo do friso; T06/T07 continuam abertas.
As seções seguintes descrevem candidatas e checkpoints anteriores.

## Contato na penumbra — candidata v3 em revisão (13/11/2026)

Registro 8, sessão `2026-11-13-codex-contact-penumbra`. Corrigidos o piso calculado
por colisor em vez da base pintada, a extensão visual dos livros e as sombras XZ
vistas de perfil pela câmera horizontal. Reflexo frio localizado permite ler a
sombra de contato. Física, apoios de pouso, câmera e pinturas preservados.
[Candidata, comparação e reprodução](../../Evidence/25d/T06-contact/README.md).
Build aprovada T06.3 preservada no backup; 170 arquivos idênticos ao manifesto.
Próximo: revisão humana individual do contato nesta candidata. T06/T07 continuam
abertas; oclusão dinâmica e os demais limites abaixo não foram validados.
Os checkpoints seguintes são históricos anteriores à correção candidata.

## T06 reaberta por regressão de contato — T07 bloqueada (13/11/2026)

Sessão `2026-11-13-dsh-consolidacao-t06t07` (DeepSeek), registro 6
(`t06/consolidacao-criterios-v1`) e `plan:t06`. O pedido foi consolidar os
critérios restantes de T06/T07 antes de expandir, com limites explícitos de áudio,
teclado/toque, aparelhos e WebGL.

A consolidação avançou, mas o usuário relatou **urso e livros flutuando sem sombra**
(diário 972). A build final foi capturada e o defeito **confirmado**: as bases tocam
o piso, porém a sombra escura do `ContactShadowT054` não lê sobre o piso quase preto
da penumbra. O aceite técnico de T06 foi **invalidado no mesmo dia** (`reopen`);
T06 está reaberta e T07 segue **bloqueada**. [Diagnóstico e capturas](../../Evidence/25d/T06/README.md#regressão-de-contato-reportada-e-confirmada).

- Matriz de critérios com evidência e limite por item, parâmetros de câmera/física/
  evento e reprodução em [Evidence/25d/T06/README.md](../../Evidence/25d/T06/README.md)
  e [parametros.json](../../Evidence/25d/T06/parametros.json).
- **Áudio capturado** do executável (mix 48 kHz, 70,19 s) e analisado por
  correlação com o clip aprovado: quatro eventos de estralo, nenhum na falha.
  Descoberta registrada: a música `High-Frequency Violin` toca em loop no piloto.
  [Análise](../../Evidence/25d/T06/audio-60/audio-analysis.json). **Não houve
  escuta humana nem certificação de mixagem.**
- Build final revalidada em **30/60/120 Hz** (`success=true`, zero erros), com uma
  fonte de Yuna, zero renderers legados e um estralo por travessia. PlayMode de
  entrada (teclado, mouse, W e toque) aprovado; **EditMode 52/52**.
- Limite declarado: item T06-4 (A/B da assistência horizontal) não foi executado;
  não foi maquiado como comparação concluída.
- **Próximo passo:** nova microentrega de contato/ancoragem na penumbra, na câmera
  final, comparada com contact-v2 e penumbra-v2 e revisada pelo usuário. Só depois
  a T06 pode ser reaceita e a T07 submetida.
- Limites: escuta/mixagem, aparelho físico/retrato/safe area, WebGL, sombras
  dinâmicas de oclusão, A/B da assistência e desempenho real. A verificação de
  120 Hz exige a janela do player em primeiro plano; em segundo plano a renderização
  a ~28 fps altera o passo da física sob `captureDeltaTime`.
- Nenhum commit/push nesta consolidação. A build aprovada da T06.3 foi preservada
  em `Build/Windows/YunaLegend-Acompanhamento-T063-aprovada/`.

## T06.3 aprovada — 13/11/2026

Usuário aprovou a entrega apresentada com **“aprovadíssimo”**, após destacar a
diferença do fundo com o movimento. [Aceite individual](../../Evidence/25d/T06.3/reviews/T06.3-v1-aprovacao.md).
Provas técnicas e build anteriores preservadas; nenhum novo teste ou rebuild.
Registro 5 concluído e reserva T06 liberada após o checkpoint deste aceite.
Próximo: consolidar os critérios restantes de T06/T07 antes de expandir o piloto.
Áudio por escuta, toque/aparelhos e WebGL permanecem pendentes; sem commit/push.
Os checkpoints abaixo são históricos anteriores à aprovação.

## T06.3 entregue para revisão — 13/11/2026

Registro **5**, assunto `t06/retry-movel-v1`, sessão `2026-11-13-codex-t063`.
Build oficial **regenerada após a remoção da composição duplicada** e validada.
T06.3 passou no executável em **30/60/120 Hz**, com R na pausa local, R na recuperação,
nova travessia completa, P/Esc, falha e nova travessia: zero exceções, um evento por
travessia e reset validado. Física e visual aprovado preservados; nenhuma correção
de gameplay foi necessária. Módulo builtin ScreenCapture habilitado para a captura.

[Entrega, reprodução, limites e hashes](../../Evidence/25d/T06.3/README.md).
Vídeo contínuo com UI: **37,30 s**, simulação60 Hz/captura30 fps, **sem áudio**.
Começar/Recomeçar confirmados também por clique na build normal, deixada na tela inicial.
**Próximo: revisão humana da sequência T06.3.** T06/T07 não encerradas, sem expansão.
Registro5 e reserva T06 mantidos abertos aguardando revisão; não assumir por tempo decorrido.
Os checkpoints abaixo são histórico anterior a esta entrega.


## Checkpoint para Git e próxima entrega — 13/11/2026

Build de acompanhamento aprovada e menu atual simplificado; registros, fontes,
cena e evidências consolidados para push em `codex/dark-room-2d`.
**Próximo entregável: T06.3 — nova build + vídeo contínuo de falha/reinício durante
o evento do móvel e segunda travessia, sem estado residual ou evento duplicado.**
[Escopo, critérios e retomada](PROXIMO-ENTREGAVEL.md). Preparado, não iniciado.
A apresentação aprovada permanece como base; não repetir sua revisão estética.
T06 e T07 não estão encerradas, e a expansão permanece para depois do piloto.
Build binária em `Build/Windows/YunaLegend-Acompanhamento`, fora do Git.
O inventário `publish-checkpoint.json` e smoke60 distinguem a regeneração local
posterior da v6 originalmente aprovada. Diário registra commit/push.

## Menu do Editor simplificado — 13/11/2026

Por solicitação do usuário, menu **YunaLegend** reduzido aos três atalhos atuais:
abrir cena de acompanhamento, gerar build Windows de acompanhamento e abrir pasta
da build.19atalhos anteriores removidos do menu; métodos/fontes preservados.
Compilação exit0 e menu conferido na janela Unity. Build interativa mantém Editor
aberto e restaura cenas. [Registro](../../Evidence/25d/menu-cleanup/README.md).
A build de acompanhamento v6 foi aprovada pelo usuário: “perfeito. Aprovado.”.

## Build de acompanhamento ilustrado — 13/11/2026 (atualização mais recente)

Sessão `2026-11-13-codex-follow-build`. Pedido explícito de reunir as entregas
atuais sem fallback visual 3D atendido em **Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe**.
Cena própria `DarkRoom_IllustratedPreview`, Yuna T03, Nanda T04 próxima/azul,
urso/livros/móvel e composição T05.4 com janela-v4/penumbra-v2. Física 3D
preservada; trecho encerra sobre o móvel, sem exibir plataformas posteriores.
Build v6 exportada e testada no player em30/60/120Hz: success=true, zero exceções,
um evento de móvel, pausa local0,08s, cedimento0,075m, tropeço e reset; zero
renderers legados visíveis. Corrigida referência nula de apoio do tropeço.
Fonte do clone técnico e checkout:872arquivos idênticos por SHA; Editor do usuário
preservado. Nova build aberta na tela inicial para o usuário. [Entrega, provas e
limites](../../Evidence/25d/T06-acompanhamento/README.md).

**Build de acompanhamento aprovada pelo usuário:** “perfeito. Aprovado.”
[Registro do aceite](../../Evidence/25d/T06-acompanhamento/reviews/acompanhamento-v6-aprovacao.md).
O aceite refere-se à entrega apresentada; critérios restantes de T06/T07,
aparelhos e expansão não são encerrados automaticamente. Menu já limpo;
próxima entrega definida em PROXIMO-ENTREGAVEL.md. Sombras dinâmicas, áudio por escuta,
aparelhos e WebGL não foram validados. A build anterior `YunaLegend-Pilot`
continua como histórico incompleto; usar **YunaLegend-Acompanhamento**.

## T05 consolidada — 13/11/2026

Sessão `2026-11-13-dsh-pilot-7b3e` (DeepSeek). **T05 aceita** após conferir as
seis revisões individuais: T05.1 urso (`aprovado.`), T05.2 livros (`aprovado.`),
T05.3 móvel (`aprovado`), T05.4 contato (`agora sim! :) Sabia que você ia
conseguir.`), janela (`legal, aprovado.`) e penumbra (`Aprovado.`). Consolidação
em [Evidence/25d/T05/README.md](../../Evidence/25d/T05/README.md). O aceite cobre
ambiente estático e contratos de apoio; travessia, salto, pouso, quebra do móvel
em runtime (pausa de 80 ms, estralo, tropeço, retry), sombras dinâmicas,
aparelhos e WebGL continuam pendentes. Próximo: **T06.1 — travessia urso → livros
em movimento**, com câmera final e física preservada.

## T06.1 candidata — travessia urso → livros (13/11/2026)

Sessão `2026-11-13-dsh-pilot-7b3e` (DeepSeek). T05 aceita liberou a T06, que está
reservada por esta sessão. Primeira microentrega em revisão:
[Evidence/25d/T06.1](../../Evidence/25d/T06.1/README.md), com
[proposta de revisão](../../Evidence/25d/T06.1/reviews/T06.1-proposta.md).
O harness `YunaT061TraversalCapture` instala Idle/Run/Jump/Stumble aprovados e o
resolvedor sobre o controlador real na `DarkRoom_Gameplay`, oculta o corpo 3D e
dirige chão → urso → livros por `InputSystem` na câmera final, sem salvar a cena.
60 Hz e 120 Hz concluem (2 saltos, 1 fonte visível, 0 conflitos, SHA da cena e do
controlador intactos); **30 Hz falha** porque o salto parte em `x=2,96` e passa
por cima da pilha. A composição da T05.4 e o voo da T04.1 ainda não entram nesta
captura, e T06.2/T06.3 dependem da revisão humana desta candidata.

**Build de validação (13/11/2026):** o visual ilustrado foi integrado na cena salva
pelo `YunaIllustrationInstaller` (runtime, esconde apenas os renderers do corpo 3D)
e a build Windows x64 foi gerada em `Build/Windows/YunaLegend-Pilot/YunaLegend.exe`
(183 MB, 0 erros). Instruções e limites em
[BUILD-VALIDACAO.md](../../Evidence/25d/T06.1/BUILD-VALIDACAO.md). O harness T06.1
passa a reaproveitar o instalador persistido; `capture-60-integrated` repete a
travessia com 2 saltos, 1 fonte visível e 0 conflitos.

**Migração para o sistema oficial (13/11/2026):** decisão humana — o
**acompanhamento criado pelo Codex** (`IllustratedPreview`, cena
`DarkRoom_IllustratedPreview`, build em `Build/Windows/YunaLegend-Acompanhamento`)
é o caminho oficial, por já estar ajustado. Retirados do projeto: build própria
(`WindowsPilotBuild`), harness `YunaT061TraversalCapture`, `make_t061_review.py` e,
na sequência, o `RoomCompositionRuntimeInstaller`/setup e o objeto
`T054_Composicao_Runtime` nas duas cenas (script `RetireLegacyComposition`).
`YunaIllustrationInstaller` permanece: o acompanhamento depende do resolvedor que
ele cria. As capturas T06.1 v1–v3 ficam como histórico, não como caminho vigente.
**Encerramento (13/11/2026):** o usuário aprovou o encerramento deste trabalho
(“pode aprovar o que vc fez e marcar no diário como concluído”). A **T06 foi
liberada** no coordenador (sem dono). Continua pendente regravar a build oficial do
acompanhamento — exige Unity fechado — e a revisão visual da T06.1 v3, que
condiciona T06.2/T06.3 e a T07.

**T06.1 v3 (13/11/2026, sessão `2026-11-13-dsh-pilot-t061v2-8a1f`):**
`RoomCompositionRuntimeInstaller` instala em runtime a composição aprovada completa
— panorama v4, sombras de contato, **móvel ilustrado** (T05.3), **Nanda ilustrada**
(T04.1) e **penumbra** (luar, luz azul, fosforescência) — no trecho do piloto, com
272 renderers legados ocultos. Captura em
[Evidence/25d/T06.1/capture-v3-60](../../Evidence/25d/T06.1/capture-v3-60), revisão
[T06.1-v3](../../Evidence/25d/T06.1/reviews/T06.1-v3.md) e build regravada
(193 MB, 0 erros). A v2 fica marcada como superada: ela desligou a penumbra por
engano — o tom azulado da Yuna no aprovado é intencional e sem a penumbra o quarto
fica claro e o móvel aparece como silhueta preta.

**Handoff (13/11/2026):** T06 foi **liberada** no coordenador. A próxima IA começa
por [Evidence/25d/T06.1/HANDOFF.md](../../Evidence/25d/T06.1/HANDOFF.md), que traz
estado, pendências em ordem, comandos de retomada e as armadilhas encontradas
(rótulo de integridade da pasta `Build/`, churn do URP na compilação e a relação
harness × instalador). T06.1 continua em revisão; nada foi aprovado.

## Retomada — penumbra T05.4, 13/11/2026

Sessão `2026-11-13-codex-penumbra-e92d`, reserva T05 transferida formalmente.
Limpeza de caches encerrada por decisão humana, mantendo os arquivos; referências
documentais corrigidas e regressão anterior salva nos commits301e0a3/34100ad.
Nenhum passo operacional anterior sem resultado. Nova candidata:
[penumbra-v2](../../Evidence/25d/T05.4/penumbra-v2/README.md), aprovada individualmente
pelo usuário em13/11/2026: “Aprovado.” [Escopo do aceite](../../Evidence/25d/T05.4/penumbra-v2/reviews/penumbra-v2-aprovacao.md).
Luar da janela alta, luz azul localizada de Nanda e fosforescência muito tênue nas
plataformas. Shader projetado2,5D em coordenadas3D, sem mudar pintura/apoio/pipeline.
Quatro capturasUnity e cinco provas A/B;7checagens de fontes passaram;52/52EditMode;
regressão sem flag idêntica pixel a pixel às quatro capturas aprovadas da janela-v4.
Erro de piso e delta do apoio dos livros0. Tentativa-v1 superada preservada.
**Sem sombras dinâmicas de oclusão, sem validação em movimento nesta candidata e
sem salvar Gameplay.** Penumbra estática aprovada; T05.4/T05 agregadas e
integração/piloto T06/T07 continuam pendentes. A direção adicional humana de brilho
fosforescente discreto das plataformas está incorporada, sem aceitar a etapa por ele.


## Atualização de retomada — contato T05.4, 13/11/2026

Sessão `2026-11-13-gpt-t054-contact-91d2` (Codex GPT), reserva T05 ativa.
O índice abaixo desta atualização contém checkpoints anteriores: T05.3 foi
aprovada posteriormente (diário 803); T05.4 continua sem aceite.
Após diagnóstico e autorização humana, candidata isolada em
`Evidence/25d/T05.4/contact-v2/README.md`: livros com base adicional reutilizando
pintura sem esticar, topo/collider preservados, camada da poltrona omitida,
sombras orientadas no piso e suavizadas. Quatro capturas reais Unity; base/piso
−8,855561 m, erro 0, deslocamento do topo 0. Cena não salva.
Contato e sobreposição da candidata aprovados pelo usuário em 13/11/2026:
“agora sim! :) Sabia que você ia conseguir.” Registro em contact-v2/README.md.
Escala v3 produzida em `Evidence/25d/T05.4/scale-v3/README.md`: novo panorama
imagegen 2172×724, 32 m, rodapé aproximadamente 1,134 m, janela baixa/larga,
parallax 0,35, sem camadas mid/near antigas. Quatro capturas Unity reais;
contato e topo dos livros preservados. V3 recebeu pedido de ajuste: janela baixa.
Candidata `Evidence/25d/T05.4/window-v4/README.md` sobe a janela e a posiciona
acima do móvel; altura/posição aprovadas pelo usuário em13/11/2026 com resposta
literal “legal, aprovado.” Registro em window-v4/README.md. Direção confirmada pelo
usuário: sala em penumbra iluminada apenas pela janela e Nanda azul.
Depois: implementar acabamento de iluminação conforme essa direção.
T05.4 agregada sem aceite;
nenhuma validação runtime nestas candidatas.

Atualização em 13/11/2026; última sessão `2026-11-13-codex-t052-books-b7c4`, preservando as entregas das sessões anteriores. Este é um índice curto; reservas e resultados verificáveis permanecem no estado estruturado e no diário.

## Feito e aprovado

- Inventário T00 e base técnica T00B aceitos com limites registrados. Menu, entrada na fase, dois saltos, morte e retry automatizados pelo Input System; correção de foco do menu; build WebGL local da base anterior.
- Direção T01 aprovada: Yuna, Nanda azul, urso arredondado, livros inclinados, móvel e composição noturna. [Bíblia aprovada](../../Evidence/25d/T01/DIRECAO-VISUAL-APROVADA.md).
- Yuna estática na Unity aprovada em T02.1: pintura integral, transparência, pivô/escala, ordenação e oclusão em cena isolada.
- Projeção A, perspectiva suave 45°, escolhida em T02.2a. [Aceite literal](../../Evidence/25d/T02.2/reviews/T02.2a-v1-aprovacao.md).
- Câmera A estável aprovada em T02.2b: Yuna em 134,4 px constantes, acompanhamento suave e antecipação do pouso. [Registro da resposta humana](../../Evidence/25d/T02.2b/review-v1.md).

## Prova técnica concluída e entrega atual

[T02.3 v2 — prova do método de animação](../../Evidence/25d/T02.3/README.md) aprovada pelo usuário nesta conversa em 12/11/2026. [Resposta literal e hashes](../../Evidence/25d/T02.3/reviews/T02.3-v2-aprovacao.md). A inclinação foi corrigida na v2; camada medida em 0,164 ms por quadro no editor, root/colisor/pivô preservados e 32/32 PlayMode registrados na entrega anterior. **As pernas não articulam** e o ciclo tem cerca de 2 px de oscilação: aprovação da prova não significa corrida final aprovada. Poses desenhadas continuam sendo a direção de T03; deformação fica como recurso secundário.

**T02 aceita tecnicamente:** [contratos](../../Evidence/25d/T02/CONTRATOS-TECNICOS.md) e [auditoria](../../Evidence/25d/T02/technical-audit.json) com 23/23 verificações. Metas de contato e orçamento definidas; arte/física preservadas. O conjunto completo de amostras grounded apresenta root/apoio de 0,0800–0,1475 m, ainda a calibrar visualmente na integração. Atlas e cobertura de aparelhos continuam pendentes.

**T03.1 v1 aprovada:** [repouso/respiração/piscada](../../Evidence/25d/T03.1/README.md), com [resposta humana registrada](../../Evidence/25d/T03.1/reviews/T03.1-v1-aprovacao.md). Essa aprovação não se refere à corrida.

**T03.2 v2 aprovada:** clipe de 128 quadros (11,0053 passadas), início na troca de pose e régua do chão re-espaçada (1,00848 m): fase no loop **0,0053 ciclo**, grade **0,0 m**, salto de tela no loop 5,0 px (mediana entre quadros 6,3 px); apoio preservado (deriva 0,0 m), pintura/física/velocidade/colisor/Gameplay intactos. O usuário aprovou após ver os GIFs, aceitando como estado atual o **passo do corpo entre poses** (até 0,307 m; 21/127 intervalos parados). [Aprovação literal](../../Evidence/25d/T03.2/reviews/T03.2-v2-aprovacao.md), [revisão](../../Evidence/25d/T03.2/reviews/T03.2-v2.md), [entrega](../../Evidence/25d/T03.2/review-v2/README.md). A decisão de arte (passada ~2,4× mais longa) ou velocidade/cadência (~1,63 m/s) permanece registrada como limite aceito, sem bloquear T04/T05; a passada pintada é 0,6317 m por ciclo contra 2,016 m do controlador (3,19×). T03.1 aprovada; T02 aceita tecnicamente.

Detalhe da v1 (histórico): [corrida com oito poses pintadas](../../Evidence/25d/T03.2/README.md), pernas/joelhos/sapatos alternando, em chão técnico com controlador real a 5,2 m/s. Verificada a 30/60/120 Hz na v1; âncora de sola com deslizamento máximo <0,000001 m; colisor/physics/Gameplay preservados. **Tranco da v1 confirmado:** na última passada completa (3,833→3,867 s), a compensação do corpo passava de −0,22082 m para zero de uma vez (~21 px a 720p). A v1 não foi aprovada nem rejeitada; fica em **ajustar** e é preservada como histórico.

**T03.3 v1 aprovada:** [salto completo ilustrado](../../Evidence/25d/T03.3/README.md), oito poses pintadas com imagegen, subida/ápice/queda/pouso e retorno à corrida na fixture. Três capturas novas a 30/60/120 Hz, 66 quadros por câmera a 30 fps; uma decolagem/um pouso, altura 2,598–2,600 m, deriva do ponto de apoio <0,000001 m e separação vertical zero. Física/cena Gameplay/controlador/corrida preservados. GIFs principais e prova são da mesma execução de **120 Hz**. Impacto curto (~67 ms), articulação e transição aprovados pelo usuário; não integrada ao piloto. Aprovação em Evidence/25d/T03.3/reviews/T03.3-v1-aprovacao.md. [Revisão individual](../../Evidence/25d/T03.3/reviews/T03.3-v1.md).

**T03.4 v2 aprovada:** [tropeço e retomada](../../Evidence/25d/T03.4/README.md), oito poses originais via imagegen, pausa local80ms/recuperação280ms preservadas. Provas isoladas30/60/120Hz,reset durante evento e repetição,physics/corrida/salto intactos. [Aprovação humana registrada](../../Evidence/25d/T03.4/reviews/T03.4-v2-aprovacao.md). Não integrada ao móvel/piloto; orçamento e revisão conjunta foram fechados no aceite agregado da T03.

**T03 conjunta — aceita em 13/11/2026:** [revisão conjunta](../../Evidence/25d/T03/README.md), [contrato](../../Evidence/25d/T03/CONTRATO.md), [manifesto](../../Evidence/25d/T03/manifest.json) e [aceite literal](../../Evidence/25d/T03/reviews/T03-conjunto.md). Sequência de14s em30/60/120Hz,420 quadros por câmera, dois saltos/pousos e dois tropeços. A primeira revisão humana encontrou o Idle final vazio em t=13,5s: o root gira para Y=±90° durante a corrida, Run/Stumble compensam a rotação, mas o Idle não compensava e ficava de perfil para a câmera (`boundsSize.x=0`). `YunaIdleIllustration` foi corrigido para compensar a rotação do root e espelhar o perfil por `RunDirection`; o harness exige `sawIdleFacingCamera=true` após a parada. A segunda revisão encontrou o tropeço esquerdo desenhado para a direita: `StumblePainted.shader` não consumia o flip do `SpriteRenderer`. O shader agora aplica `UnityFlipSprite` com `unity_SpriteProps.xy`; o harness exige `stumbleFlipHonored=true` na prova renderizada espelhada e `sawStumbleFacingRight/Left=true` nos dois eventos. Diagnóstico: `pose=2 asymmetry=8052802 mirrorError=25 tolerance=1006600`; as oito amostras de cada tropeço confirmam `direction=1/flipX=false` e `direction=-1/flipX=true`. As três provas passam com420 quadros,0 conflitos, fonte máxima1 e Idle final visível. O resolvedor (ordem240, Stumble>Jump>Run>Idle) mantém179/358/716 sobreposições pré-resolvedor e0 conflitos finais. Orçamento: **61,0407 MiB sem compressão → 16,6389 MiB CompressedHQ** (limite32), todas emRGBA_DXT5; perfil/blink são cópias runtime byte-idênticas e atlas têm padding transparente. Comparação de840 renders: PSNR mínimo58,3305dB gameplay/43,5257dB detalhe; alpha inalterado. [GIFs e folhas](../../Evidence/25d/T03/review). Rótulos corrigidos para tropeço direito em6,20s e esquerdo em11,23s. T03.1–T03.4 preservadas; **T03 agregada aceita**; tentativas preservadas em `Evidence/25d/T03/attempt-03-idle-edge-on` e `attempt-04-stumble-unflipped`.

**T04.1 — aprovada em 13/11/2026:** [Nanda ilustrada em voo](../../Evidence/25d/T04.1/README.md), [contrato](../../Evidence/25d/T04.1/CONTRATO.md) e [manifesto](../../Evidence/25d/T04.1/manifest.json). Oito poses de asas (`512×592`, atlas `2048×1184`, PPU 880, pivô `0,42061`) derivadas da prancha aprovada da T01 por warp vertical mascarado; a neutra altera 0 pixels visíveis. `NandaFlightIllustration` anima só o visual (ciclo 0,8 s, hover 3,5 cm/2,4 s, `flipX` por direção, material padrão de sprite); navegação, luz e trilha continuam com o guia. Provas 30/60/120 Hz com 72 quadros por câmera, oito poses, 48 mudanças idênticas, `mirrorError=0` e 0 violações de orientação; a Yuna da fixture é apenas escala estática. Atlas residente 4,8134 MiB CompressedHQ (limite 16). Gameplay, `NandaFairyGuide.cs` e PNG aprovado inalterados. **T04.1 aceita pelo usuário em 13/11/2026** (resposta literal "aprovado.", registrada em `Evidence/25d/T04.1/reviews/T04.1-revisao.md`); T04.2, T05.3 e T06 continuam pendentes.

## Bloco de aprovações — 13/11/2026 (sessão `2026-11-13-copilot-visual-parallel-b29a5ec7`)

- **T04.2 aprovada e T04 agregada aceita.** Resposta literal `voo da nada aprovado. Ficou muito bom.` em [revisão da T04.2](../../Evidence/25d/T04.2/reviews/T04.2-revisao.md); agregado em [T04](../../Evidence/25d/T04/README.md). Provas 30/60/120 Hz, 150 quadros/câmera, separação 1,416/1,408/1,397 m, `mirrorError=0`. Limite aberto: luz azul sobre superfícies não demonstrada (sprite unlit) e integração na fase pertence à T06.
- **T05.3 móvel: captura estática concluída, revisão humana pendente.** Seis PNGs 1280×720 (intacto, quebrado, detalhe, reset, dois overlays de apoio) e folha comparativa em [T05.3](../../Evidence/25d/T05.3/README.md); sentinela `T053_EDITMODE_INSPECTION_PASS`, `sag` 0,075 m. **Pausa local de 80 ms, estralo, tropeço e retry não foram validados em runtime.**
- **Planos antigos encerrados:** 72, 92, 97, 152, 164, 168, 178, 185, 534, 628, 689 e 787 receberam resultado na sessão de origem; nenhum plano `planned` sem resultado permanece.
- **`AGENTS.md` reparado:** o arquivo em disco estava truncado no meio da seção 5 e havia perdido a seção 6; reconstruído a partir do HEAD preservando apenas a edição da seção 1. Nenhuma regra de diário/legado foi descartada.

## Retomada T05 — 13/11/2026

Atualização da sessão `2026-11-13-codex-t052-books-b7c4` (Codex GPT-6.1), que assumiu o hand-off persistido pelo Copilot na entrada 728. A transferência formal da reserva T05 está registrada na entrada 730; o histórico das tentativas 717–727 permanece preservado no diário.

- **T05.1 — urso aprovada:** resposta literal `"aprovado."` em [revisão individual](../../Evidence/25d/T05.1/reviews/T05.1-urso-proposta.md). O aceite é exclusivo do urso e não inclui movimento, pouso ou T05 agregada.
- **T05.2 — livros aprovada:** resposta literal `"aprovado."` em 13/11/2026, registrada na [proposta de revisão](../../Evidence/25d/T05.2/reviews/T05.2-livros-proposta.md). [README](../../Evidence/25d/T05.2/README.md), [contrato medido](../../Evidence/25d/T05.2/CONTRATO.md), [câmera final](../../Evidence/25d/T05.2/01-gameplay-books-contact.png), [detalhe](../../Evidence/25d/T05.2/02-books-detail.png), [close de contato](../../Evidence/25d/T05.2/03-books-contact-close.png) e [manifesto](../../Evidence/25d/T05.2/manifest.json). A prancha aprovada da T01 foi usada sem nova geração; a capa superior rígida foi medida em `1,184211 m`, `-8,739244°`, offset `+0,134786 m`, profundidade `1,15 m`. O marcador 1 usa `MeshCollider` não convexo contínuo, visual ilustrado e mantém o antigo `Visual_3D` inativo. As capturas usam a pintura ilustrada oficial da Yuna (SHA-256 `0b9315b4…`), virada para a direita; o `Body_Visual` 3D fica inativo e o visual é instalado só no harness, sem salvar a cena. A integração permanente T03 continua na T06. O aceite cobre somente a T05.2; T05.3 móvel e a T05 agregada continuam pendentes.
- **Validação real desta sessão:** setup e harness exit `0`; erro máximo perfil/collider `0`; nenhum apoio fora da profundidade física; vãos urso→livros `2,451482 m` e livros→próxima `1,250000 m`, ambos abaixo do alcance validado; sprite oficial ativo com hash T01, `flipX=false`, erro de pés `0`; EditMode `52/52`. As capturas e logs estão no diretório T05.2.
- **Coordenação atual:** T05 pertence a `2026-11-13-codex-t052-books-b7c4`; T05.3 móvel, T05 agregada e T06 continuam pendentes.
- **Próximo passo exato:** aguardar o comando do usuário para iniciar T05.3 (móvel). Não aceitar a T05 agregada antes do aceite do móvel. T04.2 e T06 continuam pendentes.

## Próximos passos pequenos

**Histórico do ajuste 595:** fechamento do clipe tratado na T03.2 v2; aprovação e reconciliação posterior registradas em 607/625. O passo corporal continua como limite aceito, sem nova correção da corrida nesta T03.3.

1. T03.3 aprovada. T03.4 v2 aprovada individualmente pelo usuário; pausa local de 80 ms e recuperação de 280 ms verificadas na fixture.
2. **T03, T04 e T05 aceitas.** Próximo: T06.1 — travessia urso → livros em movimento, com câmera final, física preservada e prova em 30/60/120 Hz; depois T06.2 (travessia do móvel com cedimento/tropeço) e T06.3 (falhar e tentar de novo). Podem ocorrer em paralelo apenas em escopos disjuntos; cada microentrega visual exige revisão humana antes da dependente.
3. T06 só após T03+T04+T05: integrar exclusivamente o piloto urso → livros → móvel, com câmera final, run/jump/land/stumble/retry e UI, preservando física.
4. T07 exige build/vídeo e aprovação explícita do piloto em movimento antes de expandir a fase.

Pendências de integração: calibração visual dos pés sobre superfícies reais (amostras de root/apoio vão de 0,0800 a 0,1475 m, próximo da skin do CharacterController), composição retrato, narrativa completa e dispositivos reais. Não mudar root/física ou achatar objetos para esconder diferenças. Não houve publicação da nova apresentação.

## Histórico separado

[Arquivo da direção anterior](../../Archive/pre-25d-2026-11-12/README.md). Documentos 3D, estados antigos, auditorias, capturas, sondas e referências foram agrupados com manifesto de origem/destino e SHA. Diário e evidências aprovadas 2,5D não foram reescritos. Caminhos antigos neles podem ser resolvidos por `python tools/archive_history.py resolve CAMINHO`.

O código, as cenas base, materiais e modelos ainda consumidos pela Unity permanecem em Assets. Arquivar uma decisão artística não autoriza retirar uma dependência de execução. O roteiro narrativo, as licenças e as instruções de build/publicação continuam ativos.
