# T03.3 v1 — salto completo ilustrado

**Estado: em_revisao.** 12/11/2026, Codex (GPT-6), sessão
`2026-11-12-codex-jump-t033-c91e`. Base `cdfca7d`, Unity 6000.3.28f1/URP existente.
T03 transferida pelo coordenador após escolha explícita do usuário. A tarefa
continua ativa; T03.1/T03.2 aprovadas foram preservadas. Não iniciada T03.4.

## O que avaliar

Um salto completo com oito poses desenhadas: saída, subida inicial/final,
ápice recolhido, queda, preparação, impacto e recuperação. A entrada é um Space
do Input System; o controlador 3D existente produz a trajetória. A apresentação
lê velocidade vertical e contato e só dispara o pouso após `IsGrounded`.

- [Salto em tamanho de gameplay](review-v1-verified/jump-gameplay.gif), 1280×720.
- [Ampliação em velocidade normal](review-v1-verified/jump-detail.gif), 640×640.
- [Ampliação em câmera lenta](review-v1-verified/jump-detail-slow.gif), metade da velocidade.
- [Perguntas da revisão](reviews/T03.3-v1.md).

Os três GIFs principais usam **exatamente os quadros da execução 120 Hz**,
capturados a 30 fps: 66 quadros, 2,2 s de simulação. Foram acrescentadas pausas
de apresentação de 250 ms no primeiro e 500 ms no último quadro: GIF normal
2,95 s e lento 5,90 s. A repetição é um replay da ação, **não um loop contínuo**:
o reinício volta à posição inicial do cenário. Não houve retoque do render.

A câmera gameplay mantém perspectiva 45°, distância 9,0533 m e escala da prova,
acompanha somente X e fica em Y=2,05 para enquadrar o arco. A câmera ampliada
acompanha X/Y para examinar poses; não representa aprovação da câmera do piloto.
Gravações são mudas e não certificam percepção de áudio.

## Implementação e arte

`Assets/Art/Illustrated25D/Yuna/Jump/` contém o driver, harness Editor, atlas,
oito Sprite assets, metas e cena `YunaJumpReview.unity`. Cena derivada da fixture
Run, fora dos BuildSettings. O driver de corrida aprovado permanece intacto;
sua pintura fica oculta durante o salto e retorna após o impacto.

Imagegen **embutido**, usando `Yuna_Profile_v1.png` aprovada como referência;
[prompt integral](prompt-jump-v1.txt), [saída bruta](yuna-jump-generated-v1.png),
[contrato de apoio/importação](CONTRATO.md). Alpha real RGBA, 1448×1086,
4×2 células 362×543; um PPU comum, sem redimensionar poses por caixa opaca.
Não foi importado código/arte do legado nem utilizada API por CLI.

Velocidade 5,2 m/s, altura serializada 2,6 m, gravidade serializada 22 m/s²,
CharacterController e root preservados. Os defaults do script são diferentes,
não foram confundidos com os valores executados da cena. Gameplay principal
não foi alterada. Nenhum Physics2D, colisor extra ou ajuste de salto para a arte.

O impacto/recuperação conserva uma âncora visual da sola durante cerca de
67 ms, com compensação limitada a 0,35 m. Depois o offset visual se aproxima do
offset nativo da corrida a no máximo 2 m/s, sem mexer na velocidade física.
A corrida fora dessa transição conserva suas próprias trocas discretas.

## Validação nova executada

Três Unity PlayMode batch **seriais**, na cópia existente `Build/Publish/Project`,
sem abrir outro Editor no checkout principal. Antes do ensaio, 77 fontes de
código/shader/asmdef existentes conferiram com a cópia. A checagem final detectou que a cena Run na cópia diferia da aprovada;
a cena de origem foi sincronizada com o checkout e as três taxas foram
reexecutadas em `validated-*`. Após sincronizar a nova pasta Jump, os 26
arquivos internos executados conferiram byte a byte com a entrega. O manifesto relaciona fontes, dependências e evidências.

| Medida | 30 Hz | 60 Hz | 120 Hz |
| --- | --- | --- | --- |
| Checks da captura e auditoria | 14/14 | 14/14 | 14/14 |
| Decolagens / pousos | 1 / 1 | 1 / 1 | 1 / 1 |
| Poses avaliadas / quadros por câmera | 8 / 66 | 8 / 66 | 8 / 66 |
| Altura medida acima do root grounded | 2,597896 m | 2,599910 m | 2,599910 m |
| Voo entre primeira amostra aérea e contato | 0,966667 s | 0,966667 s | 0,966667 s |
| Deriva da âncora no impacto/recuperação | 0 m | 0,000000477 m | 0,000000477 m |
| Separação vertical da âncora | 0 m | 0 m | 0 m |
| Maior passo visual durante RunBlend, a 30 fps | 0,240001 m | 0,240001 m | 0,240001 m |

`jump-proof.json` de cada taxa contém ticks na frequência de simulação e amostras
de captura a 30 fps. O sucesso do harness não foi usado sozinho: auditoria Python
verifica estados, oito poses, arco, fontes, contato e quantidades de quadros.
O envelope opaco inferior (alpha ≥128) das poses 6/7 coincide com o baseline do
suporte. Essa medida não certifica toda a extensão das duas solas, regiões
inclinadas ou qualquer plataforma real. Quadros de ápice e pouso e a sequência
de estágios foram inspecionados estaticamente, sem aprovação estética automática.

Provas/verificações separadas em `validated-30/`, `validated-60/`, `validated-120/`.
Prova principal `review-v1-verified/jump-proof.json` é cópia exata de `validated-120/`.
`review-v1-verified/verification.json` identifica os 132 hashes dos quadros brutos dessa
execução. Os quadros brutos ficam em `Build/25d/JumpValidated120`, fora do Git.
Manifesto de fontes/evidências: [manifest-v1.json](manifest-v1.json).

## Limites para revisão

- O impacto é curto porque a corrida continua a 5,2 m/s. Avaliar se parece suave
  ou apressado; teste numérico não decide o peso/personalidade do salto.
- Na passagem impacto→recuperação, o pivô recua cerca de 0,03235 m enquanto o
  ponto da sola permanece fixo, devido à troca do desenho. Deve ser julgado no
  vídeo; não é pausa/recuo físico do controlador. O blend de retorno avança,
  com passos entre 0,1515 e 0,2400 m na execução principal a 30 fps.
- Corrida nativa antes/depois mantém o passo discreto. Neste clipe não alinhado
  à fase de corrida, maior passo do pivô no clipe inteiro: 0,4982/0,4115/0,2867 m
  em 30/60/120 Hz. Esses números não substituem a versão de corrida aprovada e
  não certificam fluidez integral em todas as taxas. Fontes Run e seu manifesto
  de 35 hashes continuam intactos.
- Atlas Jump contabilizado isoladamente em aproximadamente 12,44 MiB no Editor.
  Não cumpre orçamento residente conjunto de 32 MiB por si só; atlas/estados,
  GPU e dispositivos reais ainda precisam ser tratados antes da integração.
- Não exercitados: plataformas reais do piloto, superfícies inclinadas,
  virada/iluminação do outro sentido, toque/menu, retry visual, tropeço, retrato,
  build WebGL, aparelho ou áudio ouvido. Gameplay não recebeu a nova apresentação.
- Os logs têm a exceção conhecida `UnityEditor.Search.SearchDatabase` na indexação
  inicial; capturas e marcador `YUNA_JUMP_REVIEW_DONE success=True` completaram.

## Reprodução e histórico

Em cópia isolada com os mesmos fontes e GUIDs, executar sem `-quit`:

```text
Unity.exe -batchmode -projectPath <copia> -executeMethod YunaLegend.YunaIllustration.Editor.YunaJumpReviewCapture.Run -jumpHz 120 -jumpOutput <evidencias> -jumpFrames <quadros> -logFile <log>
```

Aceita `-jumpHz 30`, `60` ou `120`. O harness entra em Play Mode e sai após 66
amostras por câmera. Recria somente a cena Jump nessa cópia; não executar sobre
o Editor principal aberto. Em uso manual, abrir `YunaJumpReview.unity`, entrar
em Play Mode e pressionar Space após estar no chão.

```powershell
$env:PYTHONPATH='Build/25d/PythonDeps'
python Evidence/25d/T03.3/make_review.py --proof Evidence/25d/T03.3/review-v1-verified/jump-proof.json --frames Build/25d/JumpValidated120 --out Evidence/25d/T03.3/review-v1
```

`--no-gifs` recalcula métricas e hashes sem regravar GIFs. Saídas devem ir para
pasta nova em uma revisão posterior; não sobrescrever candidata aprovada.
Logs finais: `Build/25d-t033-validated-30.log`, `-60.log`, `-120.log`.

`attempt01-60/` é ensaio histórico anterior ao ajuste de transição: captura
concluiu, mas o retorno empilhava offset e troca de pose, com passo de 0,4886 m.
Não é a candidata atual e seus fontes anteriores não têm snapshot completo.
As pastas `final-30/60/120` e `review-v1/` são ensaios anteriores à
reconciliação da cena de origem, preservados como histórico sem alegação de
equivalência completa. Os principais de `review-v1-verified/` vêm da nova
execução `validated-120/`, com dependências conferidas. Não misturar essas proveniências.

Nenhum commit, push ou publicação executado nesta tarefa. Próximo passo é
registrar a resposta humana da T03.3 v1; ajustar esta candidata se necessário,
sem avançar para T03.4 antes da revisão.
