# Estado atual — sala escura ilustrada

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
