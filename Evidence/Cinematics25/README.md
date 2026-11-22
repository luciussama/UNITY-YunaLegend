# Continuidade — remake de todas as cinemáticas a 25 FPS

Atualizado em **22/11/2026**, a pedido do usuário. Este documento é o índice de evidências e o estado de retomada; o registro sequencial obrigatório permanece exclusivamente em `docs/ia/diario.sqlite3`.

**Estado real: INTERROMPIDO POR ORDEM DO USUÁRIO NO LIMIAR DE 5% DA QUOTA DISPONÍVEL. Nenhuma cinemática refeita recebeu [APROVADO].** A abertura original continua ativa no jogo. Há implementação de timing e apresentação de desenhos, mas somente um estudo parcial de Yuna, com 43 desenhos, foi produzido. A última revisão completa reprovou sua continuidade. Uma revisão uniforme posterior foi renderizada e ainda não teve parecer concluído. Não gerar novos lotes antes de resolver o veto.

## Ponto de retomada após interrupção

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
