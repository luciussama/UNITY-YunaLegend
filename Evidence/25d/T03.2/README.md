# T03.2 v1 — corrida pintada em revisão

12/11/2026, sessão `2026-11-12-codex-run-t032-f92c`, Codex (GPT-6).
Estado: **em_revisao**, não aceita como animação final. Repouso T03.1 aprovado
na conversa; corrida, salto e tropeço exigem suas próprias revisões.

## Material atual

- [Corrida ampliada, velocidade normal](review-v1/run-detail.gif), 640×640, 4 s.
- [Mesma captura em câmera lenta](review-v1/run-detail-slow.gif), 0,5×, 8 s.
- [Escala de gameplay](review-v1/run-gameplay.gif), 1280×720, 4 s.
- `review-v1/pose-0..7.png`: oito poses realmente renderizadas pela Unity.
- [Verificação 30/60/120 Hz](review-v1/verification.json), dados e hashes dos
  240 PNGs brutos da captura principal. GIFs não têm áudio.

As pernas alternam apoio e voo, dobram joelhos e movem sapatos. Braços, saia e
rabo de cavalo acompanham oito poses pintadas; o corpo inteiro não é uma imagem
neutra inclinada. A candidata mantém rosto/cabelo/roupa da referência aprovada.
A continuidade da identidade e a cadência precisam da avaliação humana em
[reviews/T03.2-v1.md](reviews/T03.2-v1.md).

## Autoria/importação

Imagegen nativo criou um único atlas com oito poses (4×2), tendo a pintura
`Evidence/25d/T01/yuna-profile-v1.png` como referência de identidade e acabamento.
Prompt integral em `prompt-run-atlas-v1.txt`; saída bruta preservada como
`yuna-run-atlas-generated-v1.png`, copiada sem alteração para `Assets/.../Run/`.
Não é pintura manual nem arte importada do legado. A imagem original aprovada
permanece intacta. Não houve edição artística por Python.

A ferramenta retornou **1448×1086**, apesar da solicitação de 2048×1536. A grade
real é exata: 362×543 por célula. Texture2D, NPOT None, alpha/sRGB, bilinear/clamp,
sem mipmaps/ReadWrite, uncompressed. Oito Sprite assets FullRect usam a mesma
textura. PPU único para todas as poses: 1049,2857666 × 362/1024 = 370,9397.
Não se normalizou cada pose pela caixa opaca: o corpo pode agachar e levantar.

Pivô X comum de 536/1024 da célula. Pivôs Y autorados pela linha real da sola nas
poses de apoio: FloorY [493,496,495,495,495,496,496,495], origem superior esquerda.
Nas poses de voo 3 e 7, a linha de piso continua em y495 e os pés estão acima.
`YunaRunIllustration.FootX/BottomY` registram os pontos da sola, identificados nos
pixels opacos; marcos usados na análise não aparecem desenhados no jogo.

## Método de apoio e limites de apresentação

O controlador físico original conduz o deslocamento, gravidade e GaitPhase.
O mesmo GaitPhase já conduz os passos sonoros; não foi criado outro relógio de
movimento ou alterada a velocidade de 5,2 m/s. Autorun e a desativação da morte
pela rota existem apenas na fixture de chão; não alteram Gameplay.

A primeira versão, trocando só sprites, arrastava o marco da sola aproximadamente
0,24 m durante a fase de apoio. Três tentativas de corrigir apenas a perna com
malha falharam artisticamente: botas deformadas, apesar da âncora numericamente
estável. Foram rejeitadas. **A candidata final não usa essa deformação.**

O SpriteRenderer conserva cada pintura inteira (um quad, sem esticar membros).
Durante as poses 0–2 ou 4–6, o visual filho recebe uma translação horizontal para
manter seu marco de apoio no mundo; libera a âncora nas poses de voo. O alvo nasce
0,20 m à frente para distribuir o movimento visual em torno do root, com limite
de 0,35 m (raio do CharacterController preservado). A translação observada máxima
foi 0,22082 m. É autoria de root motion **visual**, não deslocamento do controlador.

Verticalmente, somente o visual compensa o afastamento medido do chão real
(0,08 m nesta fixture), dentro do limite de skin+0,02 m da T02. A correção horizontal
é distinta desse ajuste vertical. Posição/velocidade física, forma/centro/skin do
colisor, gravidade e salto permanecem intactos. A pintura recebe translação e
virada, sem warp, interpolação inventada de membros ou normalização de silhueta.

**Revisar o ritmo:** o corpo acompanha poses discretas e sua compensação horizontal
pode parecer marcada ou pulsante. A medição de apoio não aprova fluidez. Avaliar
o clipe normal e o lento; não esconder essa característica mudando a física ou
o enquadramento aprovado. Ajustes desta percepção voltam a T03.2.

## Evidência executada

Unity 6000.3.28f1, cópia `Build/Publish/Project`, cena isolada
`Assets/Art/Illustrated25D/Yuna/Run/YunaRunReview.unity`. Chão técnico plano de 70 m,
marcas a cada metro para expor movimento/deslizamento; não é arte de plataforma.
Projeção A 45°, distância 9,0533 m e escala vertical aprovadas; seguimento rígido
do root nesta fixture, sem alegar revisão da câmera completa da fase. A cena não
está na lista de publicação. A cena salva entregue é da última execução, a 120 Hz,
de onde vêm as GIFs; a amostragem das imagens é 30 fps em todas as execuções.

| Medida | 30 Hz | 60 Hz | 120 Hz |
| --- | --- | --- | --- |
| Capturas / poses | 120 / 8 | 120 / 8 | 120 / 8 |
| Percurso entre primeira/última captura | 20,62667 m | 20,62667 m | 20,62667 m |
| Deslizamento máximo do marco na mesma fase de apoio | 0,00000095 m | 0 m | 0,00000095 m |
| Erro vertical desse marco | 0 m | 0 m | 0 m |
| Maior translação horizontal do visual | 0,20000 m | 0,17101 m | 0,22082 m |

Controlador e Gameplay com SHA idêntico antes/depois; colisor/escala do root
inalterados; trajetória física linear de 5,2 m/s com erro inferior a 0,001 m.
O marco da sola passa as metas de 2 px horizontal/1 px vertical a 720p na fixture.
Não é verificação de todo o contorno opaco do sapato nem de apoio inclinado real.
Oito poses foram efetivamente vistas em cada execução. Há três clips de passos
existentes configurados e AudioSource desmutado na cena; GaitPhase compartilhado
preserva o relógio original. GIFs são mudas e não certificam a percepção dos sons.

Atlas contabilizado em 12,44494 MiB no editor. É medição desse atlas, não de todo o
conjunto Yuna/GPU. Carregar ingenuamente junto das duas texturas do repouso
(24,0096 MiB) daria cerca de 36,45 MiB, acima da meta conjunta de 32 MiB da T02:
otimização/residência de atlas deve ser resolvida antes de declarar o orçamento
integrado cumprido. Não houve medição nova de CPU/GPU/aparelho nesta candidata.

Não exercitados: transições idle/run/jump, retorno/iluminação no sentido inverso,
apoios reais do piloto, tropeço/pausa/retry, dispositivos, retrato ou publicação.
A âncora usa um Collider de chão técnico explícito; a integração deve consultar
as superfícies reais de PlatformSupportProfile e nunca prolongar apoio em lacunas.

## Reprodução e histórico de ensaios

Abrir a cena Run e entrar em Play Mode para ver corrida e sons no chão. Captura:

```text
Unity.exe -batchmode -projectPath <copia> -executeMethod YunaLegend.YunaIllustration.Editor.YunaRunReviewCapture.Run -runHz 120 -runOutput <evidencias> -runFrames <frames> -logFile <log>
```

Sem `-quit`; o harness sai após o Play Mode. Aceita `-runHz 30`, `60` e `120`.
Logs finais: `Build/25d-t032-final-30.log`, `-60.log`, `-120.log`, marcador
`YUNA_RUN_REVIEW_DONE success=True`. Verificação e GIFs:
`python tools/make_run_review.py` (Pillow local em `Build/25d/PythonDeps`).
O sucesso da captura é distinto das asserções de apoio no script de auditoria.

Pastas `locked/`, `refined/`, `protected/` e `rejected-warp-runtime.cs.txt` são
ensaios rejeitados, não candidatos aprovados. `attempt01*` registra a troca simples
com deslizamento; `whole/` é o ensaio a 60 Hz antes de centrar o root motion;
`fps30/60/120` registra saturação identificada na primeira regressão. `final30/60/120`
e `review-v1/` são os resultados atuais. Nenhum registro anterior foi apagado.
Exceção conhecida do indexador Editor.Search nos logs não impediu as capturas.

Manifesto atual: `manifest-v1.json`. Nenhum commit, push, build ou publicação nova.
T03.2 aguarda revisão individual antes de T03.3 (salto completo).

## v2 — fechamento do clipe (12/11/2026, sessão `2026-11-12-deepseek-run-tail-4b7e`)

Correção do final do clipe pedida pelo usuário após a investigação v1. O clipe v1 tinha
120 quadros (10,23 passadas) e reiniciava 1,5 pose atrás, com a grade 0,63 m fora de fase.
A v2 usa **128 quadros (11,0053 passadas)**, começa na troca de pose e re-espaça a régua
para 1,00848 m: fase no loop 0,0053 ciclo, grade 0,0 m, salto de tela 5,0 px (igual a um
quadro normal). Apoio preservado (deriva 0,0 m); pintura, física, velocidade, colisor e
Gameplay intactos.

O passo do corpo entre poses (até 0,307 m) permanece: a passada pintada é 0,6317 m por
ciclo contra 2,016 m do controlador (3,19×). A [variante sem compensação](../../25d/T03.2/final-v2-smooth-60/run-detail.gif)
mostra a outra ponta do compromisso (corpo liso, apoio derrapando 0,242 m). Arte mais
longa ou velocidade/cadência compatíveis são decisão de direção, não de código.
Material: [review-v2](../review-v2/README.md); revisão [T03.2-v2](reviews/T03.2-v2.md).
A v1 é preservada como histórico e suas evidências não foram reescritas.
