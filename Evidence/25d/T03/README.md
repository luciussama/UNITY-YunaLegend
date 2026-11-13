# T03 — revisão conjunta da Yuna ilustrada e orçamento residente

Estado: **aceita pelo usuário em 13/11/2026**. Codex (GPT-6.1), sessão
`2026-11-13-codex-stumble-flipfix-9f4a`. Base `7988a0a`, Unity 6000.3.28f1.
T03.1 (idle), T03.2 (corrida), T03.3 (salto) e T03.4 v2 (tropeço) estão
aprovadas individualmente e foram preservadas. **A T03 agregada está aceita**;
T04/T05 estão liberadas e T06 continua dependente das duas.

A primeira candidata agregada foi reprovada na revisão humana porque o **Idle
final desaparecia em t=13,5 s**. Causa e evidência estão em
[tentativa 03](attempt-03-idle-edge-on/README.md). A segunda revisão apontou que
o **tropeço esquerdo continuava voltado para a direita**: o material customizado
do Stumble não consumia o flip do `SpriteRenderer`. Causa e evidência estão em
[tentativa 04](attempt-04-stumble-unflipped/README.md). Esta revisão usa as
provas corrigidas, mede o espelhamento renderizado e verifica a orientação dos
dois tropeços.

A terceira candidata foi aceita com a resposta literal do usuário:
"aprovado. Qual é o próximo passo?".

## O que revisar

- [Gameplay normal](review/game-normal.gif) — 14 s, 960×540, 15 fps de reprodução.
- [Gameplay lento](review/game-slow.gif) — o mesmo trecho, 10 fps.
- [Detalhe normal](review/detail-normal.gif) — 640×640, 15 fps.
- [Detalhe lento](review/detail-slow.gif) — 640×640, 10 fps.
- [Folha de gameplay](review/game-review-sheet.png) e
  [folha de detalhe](review/detail-review-sheet.png) — nove momentos-chave.
- [Registro de revisão](reviews/T03-conjunto.md).

Os GIFs usam os renders da execução **120 Hz** (simulação), capturados a 30 fps.
Não há retoque, interpolação ou recorte; somente paleta GIF e quantização de
duração. A sequência é determinística: idle; corrida à direita; salto em 4,0 s;
tropeço em 6,0 s; virada em 7,4 s; salto em 9,0 s; tropeço em 11,0 s; parada em
12,6 s; idle até 14,0 s. Os rótulos usam `t=6,20 s` para o tropeço direito
(dentro da recuperação) e `t=11,23 s` para o tropeço esquerdo.

## Correção crítica desta etapa

A primeira prova conjunta tinha duas Yunas sobrepostas em **t=5,0 s** e
**t=10,0 s**: a corrida era reativada pelo `YunaStumbleIllustration` (ordem 230)
depois de o `YunaJumpIllustration` (ordem 220) desligá-la. Os samples 149–151 e
299–301 da [tentativa com sobreposição](attempt-01-overlap/joint-proof.json)
registram `runVisible=true` e `jumpVisible=true` ao mesmo tempo.

Foi adicionado o `YunaIllustrationVisibilityResolver` (ordem 240), autoridade
única de visibilidade do conjunto, com prioridade **Stumble > Jump > Run > Idle**.
Ele roda depois dos três componentes aprovados, sem alterá-los. O harness agora
falha se não houver exatamente uma fonte visível. Resultado:

| Execução | Sobreposições antes do resolvedor | Conflitos depois | Máximo de fontes |
| --- | ---: | ---: | ---: |
| 30 Hz | 179 quadros | 0 | 1 |
| 60 Hz | 358 quadros | 0 | 1 |
| 120 Hz | 716 quadros | 0 | 1 |

Samples 149–151 e 299–301 das três execuções finais mostram `Jump` com uma fonte
e, no quadro seguinte, `Run` com uma fonte. Inspeção visual dos quadros 149,
299 e das folhas confirma uma única Yuna nos dois pousos.

### Correção do Idle final

O controlador gira o root para `Y=+90°`/`Y=-90°` conforme a direção. Run e
Stumble compensam essa rotação no próprio transform; o Idle não compensava.
Depois da corrida à esquerda, o plano do Idle ficava de perfil para a câmera:

```text
idleActiveInHierarchy=True
rendererEnabled=True
root=(-1.56, 0.08, 0.00)
boundsSize=(0.00, 1.46, 0.98)
```

`YunaIdleIllustration` agora compensa a rotação Y do root e espelha o perfil
segundo `RunDirection`. O harness exige `sawIdleFacingCamera=true` depois da
parada, com bounds projetados e centro dentro do viewport. A prova de 30 Hz
mostrou `sawIdleFacingCamera=true`; a folha final mostra Yuna voltada para a
esquerda em `t=13,5 s`.

### Correção do espelhamento do tropeço

O tropeço usa `Stumble_Painted.mat`, cujo shader
`YunaLegend/IllustratedStumbleCandidate` transformava os vértices sem ler o
flip do `SpriteRenderer`. Run e Jump usam o shader de sprite padrão; por isso o
`flipX` funcionava neles e não no Stumble. O resultado era o desenho original
voltado para a direita mesmo com `RunDirection=-1` e `painting.flipX=true`.

O shader agora inclui `Core2D.hlsl` e aplica
`UnityFlipSprite(positionOS, unity_SpriteProps.xy)` no vértice, o mesmo caminho
do shader de sprite da URP. O harness ganhou duas verificações independentes:

- `stumbleFlipHonored`: renderiza o quadro mais assimétrico do Stumble sem e com
  `flipX`, fora da câmera de jogo, e exige que a segunda imagem seja o espelho
  da primeira. Diagnóstico desta execução: `pose=2 asymmetry=8052802
  mirrorError=25 tolerance=1006600`.
- `sawStumbleFacingRight` / `sawStumbleFacingLeft`: exigem `flipX=false` durante
  o tropeço à direita e `flipX=true` durante o tropeço à esquerda em quadros com
  pose ativa e fonte visível `Stumble`.

As oito amostras de cada tropeço confirmam `direction=1/flipX=false` e
`direction=-1/flipX=true`. A checagem anterior só media `stumble.Pose` e
visibilidade; por isso a candidata defeituosa passava. A candidata anterior e o
quadro do tropeço esquerdo estão preservados em
[tentativa 04](attempt-04-stumble-unflipped/README.md).

## Orçamento residente

O limite é **32 MiB** para as cinco texturas residentes da Yuna. A medição usa
`Profiler.GetRuntimeMemorySizeLong` no Editor Windows, a mesma métrica para todas
as variantes.

| Variante | Texturas | Total | Resultado |
| --- | --- | ---: | --- |
| Sem compressão | perfil + blink + três atlas | **61,0407 MiB** | falha |
| CompressedHQ | perfil + blink + três atlas runtime | **16,6389 MiB** | passa |

Texturas HQ observadas: `idle profile` 3,00 MiB; `idle blink` 3,00 MiB;
`run atlas` 3,69 MiB; `jump atlas` 3,69 MiB; `stumble atlas` 3,25 MiB. Todas em
`RGBA_DXT5_UNorm` no Editor Windows. [Prova final 30 Hz](final-30/joint-proof.json),
[60 Hz](final-60/joint-proof.json), [120 Hz](final-120/joint-proof.json) e
[baseline](baseline-30/joint-proof.json).

A compressão só funciona porque os três atlas foram **acondicionados com padding
transparente** para dimensões múltiplas de 4 e múltiplas de 8 quando necessário:
Run/Jump `1448×1088`, Stumble `1776×888`. Os pixels originais foram copiados byte
a byte; só a área nova é alpha 0. Os atlas aprovados originais (`1448×1086` e
`1774×887`) não são múltiplos de 4 e o Unity os mantinha sem compressão.

Perfil e blink HQ são **cópias runtime byte-idênticas** às fontes aprovadas, com
metadados próprios em `Joint/Runtime/`. Os PNGs e `.meta` originais da T02 não
foram alterados. [Manifesto](manifest.json) registra hashes, cópias e a
verificação de que o padding é transparente e o miolo é idêntico à fonte.

## Qualidade da compressão

Foram comparados **840 renders** (420 gameplay + 420 detalhe) entre o baseline
sem compressão e a candidata HQ, todos no mesmo estado de simulação:

| Grupo | PSNR mínimo | PSNR médio | Maior diferença de canal | Alpha alterado |
| --- | ---: | ---: | ---: | ---: |
| Gameplay | **58,3305 dB** | 60,2858 dB | 62/255 | 0,0000 |
| Detalhe | **43,5257 dB** | 45,3487 dB | 69/255 | 0,0000 |

O alpha renderizado não mudou em nenhum quadro. A decisão sobre suavidade e
detalhe perceptível é humana; a folha de detalhe existe para isso.

## Implementação e proveniência

- [Harness conjunto](../../../Assets/Art/Illustrated25D/Yuna/Joint/Editor/YunaT03JointReviewCapture.cs)
  monta a cena isolada a partir da fixture Run, adiciona Idle/Jump/Stumble e o
  resolvedor, executa a sequência, mede as cinco texturas e valida o
  espelhamento renderizado do Stumble.
- [Resolvedor](../../../Assets/Art/Illustrated25D/Yuna/Joint/Runtime/YunaIllustrationVisibilityResolver.cs)
  é reutilizável na T06; `showIdle` deve ser ligado ao estado real de idle.
- [Cena da fixture](../../../Assets/Art/Illustrated25D/Yuna/Joint/YunaT03JointReview.unity)
  e Sprites em `Joint/Runtime/Frames/` estão no repositório principal.
- [Idle](../../../Assets/Art/Illustrated25D/Yuna/Idle/YunaIdleIllustration.cs)
  compensa a rotação do root e espelha o perfil; a fixture isolada não muda.
- O harness usa a cópia `Joint/Runtime/Idle_Painted_Runtime.mat`; o material
  aprovado `Idle/Idle_Painted.mat` permanece apontando para as texturas originais.
- Os componentes aprovados T03.1–T03.4, a física, o controlador, a Gameplay e os
  PNGs aprovados não foram alterados, exceto a correção de orientação do Idle
  necessária para o conjunto.

## Limitações

- Prova no **Editor Windows/D3D12**, chão técnico plano, câmera da fixture. Não é
  medição de GPU, aparelho, build WebGL, mobile ou câmera de produção.
- `Profiler.GetRuntimeMemorySizeLong` no Editor reporta cerca de 2× o tamanho
  teórico bruto; a comparação é válida entre as variantes, mas o número absoluto
  não deve ser tratado como orçamento de dispositivo.
- Sem móvel cedendo, estralo, plataformas inclinadas, piloto integrado, retry da
  Gameplay, áudio ou UI. T05.3/T06.2/T06.3 continuam responsáveis por isso.
- O resolvedor cobre visibilidade, não interpolação de pose. A transição
  corrida↔salto↔tropeço continua sendo a soma dos componentes aprovados; avaliar
  pop e ritmo visualmente.
- A exceção conhecida `UnityEditor.Search...ArgumentOutOfRangeException` aparece
  nos logs de abertura e não interrompeu as capturas. As três execuções finais
  terminaram com `YUNA_T03_JOINT_DONE success=True`.

## Reprodução

Na cópia isolada com fontes/GUIDs equivalentes, sem `-quit`:

```text
Unity.exe -batchmode -projectPath <copia> -executeMethod YunaLegend.YunaIllustration.Editor.YunaT03JointReviewCapture.Run -t03Hz <30|60|120> -t03Compression <hq|uncompressed> -t03Output <evidencias> -t03Frames <quadros> -logFile <log>
```

Recompor manifesto e comparação:

```text
python tools/make_t03_joint_manifest.py
python tools/compare_t03_compression.py
python tools/make_t03_joint_review.py
```

Tentativas preservadas: [sobreposição](attempt-01-overlap/joint-proof.json),
[resolvedor com referência nula](attempt-02-null-run/joint-proof.json),
[Idle final de perfil](attempt-03-idle-edge-on/README.md) e
[tropeço sem espelhamento](attempt-04-stumble-unflipped/README.md). Nenhuma
delas é candidata. A terceira candidata é a T03 aceita; as tentativas ficam
preservadas apenas como histórico.
