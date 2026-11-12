# T02.3 — prova do método de animação e orçamento medido

Sessão `2026-11-12-dsh-resume-25d-7c41`, DeepSeek Harness (deepseek-flash), 12/11/2026. Estado: **implementada e em revisão humana**; não fecha T02 sozinha.

## Pergunta desta microentrega

Uma única pintura aprovada consegue virar animação coerente, e quanto custa essa camada? Este passo não produz a animação final: ele mede a técnica, expõe os limites e prepara a decisão de T03 (quadros desenhados × rig × combinação).

## O que foi feito

Cena isolada `Assets/Scenes/Validation/IllustratedAnimationProof.unity`, criada a partir de uma cópia da cena Gameplay (mesmo controlador, mesmas plataformas, mesma câmera A estável aprovada em T02.2b). O corpo 3D de Yuna foi substituído por uma **malha proporcional** que deforma a pintura aprovada:

- malha 12 × 36 (481 vértices, 864 triângulos), UVs exatas do PNG aprovado, pivô na sola preservado;
- camada de deformação com inclinação de tronco no quadril, contra-inclinação da cabeça, faixa dos braços, respiração do peito, compressão de pernas e *squash/stretch* de pouso, **ancorados na linha da sola y = 0**;
- conduzida pelo estado real do controlador (`GaitPhase`, apoio, subida/queda/pouso) — a mesma escolha de pose que o estudo 3D antigo usava, agora aplicada à pintura;
- travessia real por Input System: corrida, dois saltos, urso e livros alcançados, 4,7 s de tempo simulado, 143 quadros a 30 fps.

A cena Gameplay, o controlador e a pintura **não foram alterados**: `d507921b…` (Gameplay) e `d47aea95…` (controlador) iguais antes/depois; pintura `0b9315b4…`, o mesmo hash aprovado em T01.

## Medições

| Medida | Valor |
| --- | --- |
| Custo da camada (editor, CPU) | **0,164 ms** média, 0,234 ms máxima por quadro |
| Malha | 481 vértices, 864 triângulos, 1 draw |
| Pintura importada | 1024×1536, **12,0 MB** em memória (RGBA não comprimido) |
| Quadro de pintura cru | 6,0 MB por quadro |
| Atlas estimado para 12 quadros de corrida | **≈72 MB crus** (estimativa, não medida) |
| Sola | mínima **+0,0133 m**, nunca cruzou a linha de apoio |
| Root / colisor / pivô | inalterados (rotação, escala, centro, altura, raio, offsets) |
| Deslocamento medido | 17,46 m de percurso |

A estimativa de troca de quadros é explicitamente **conta, não render**: parte da importação atual (RGBA não comprimido). Um atlas comprimido mudaria o número e exige decisão de importação própria.

## O que a camada faz, em escala

[deformation-comparison.png](deformation-comparison.png) mostra a mesma pintura em repouso, nas duas poses reais da corrida, no pouso e no limite da técnica, a 2,2× a altura do personagem; o último painel é um mapa de deslocamento (corrida menos repouso, 6× ampliado). Os renders vêm de `deformation/` (1280×800 cada) com câmera dedicada; nenhum retoque.

No tamanho de jogo, o resultado é pequeno: tronco e cabeça deslocam 4,7–6,6 px sobre os pés (personagem de 92 px na captura), e a variação dentro do ciclo é de ~2 px. A folha mostra que a camada **inclina o tronco e comprime o corpo**, mas **não constrói passada** — no limite (25°) a leitura é de reverência, com as pernas retas e os sapatos entortando.

## Leitura honesta e limites

- **As pernas não articulam.** A corrida lê como inclinação + compressão + faixa de braços; não é um ciclo final. É justamente a evidência de que deformar uma pintura única não entrega a passada exigida pelo piloto. T03 continua precisando de poses-chave desenhadas (ou de partes pintadas separadas).
- **Correção após revisão (12/11/2026):** a frase acima estava otimista. Medido nos quadros, o tronco/cabeça desloca **≈4,7 px a 6,6 px** em relação aos pés (personagem de 92 px na captura; ≈7 a 10 px a 720p) e a **oscilação dentro do ciclo é de ≈2 px**; a compressão vertical fica em ≈3–5 px e a faixa lateral em ≈3 px. Ou seja: no tamanho de jogo a camada é **praticamente imperceptível** e não se lê como corrida.
- **O sinal da inclinação foi corrigido:** a v1 inclinava para trás (`lean` positivo girava o topo para −X, e ela corre para +X). Agora `lean` positivo inclina para +X, o sentido da corrida. Captura, GIFs, JSON e a folha de comparação foram refeitos depois da correção.
- **Nomes corrigidos para não mentir:** `armSwing` virou `sideBandSwing` (a faixa lateral é a borda frontal/traseira da silhueta, não os braços pintados) e `legCompression` virou `bodyCompression` (a compressão é vertical do corpo inteiro; não há joelho nem perna articulada).
- Ambiente e Nanda continuam sendo a base técnica antiga. Não é o que esta prova avalia.
- A medição é de editor em batch, não de aparelho; não há cobertura de navegador, celular ou fps real.
- Tropeço/pausa local (T03.4/T06.2) não foi exercitado: depende do móvel que cede.
- O tempo de skinning em editor superestima código gerenciado trivial; serve como ordem de grandeza, não como orçamento de plataforma.

## Decisão proposta (sujeita à revisão humana)

**Combinação:** quadros desenhados para a passada e o salto + esta camada para vida secundária (respiro, inclinação, *squash* de pouso). Motivo: a camada preserva a pintura, custa 0,17 ms e nunca fura o apoio; mas não articula membros.

## O que revisar

1. [Folha de comparação da deformação](deformation-comparison.png) — repouso × corrida × pouso × limite, a 2,2×, com mapa de deslocamento. Renderizações soltas em `deformation/` (1280×800).
2. [Corrida em tamanho de jogo](animation-run.gif) — 30 fps, 2,2 s.
3. [Salto e pouso](animation-jump.gif) — 30 fps, 1,6 s.
4. [Ampliação do salto](animation-closeup-jump.gif) — 1,6× sobre Yuna: a pintura continua coerente ou parece borracha?
5. Quadros-chave: `01-entry`, `02-run-contact`, `03-run-passing`, `04-takeoff`, `05-rise`, `06-fall`, `07-land`, `08-recovered`, `09-books` (1280×720).

Perguntas em [reviews/T02.3-v1.md](reviews/T02.3-v1.md). Dados brutos em `animation-proof.json`; testes em `playmode-tests.xml` (**32/32 PlayMode**, incluindo 4 contratos novos desta prova).

## Reprodução

```
Unity.exe -batchmode -projectPath <copia do projeto> ^
  -executeMethod YunaLegend.EditorScripts.IllustratedAnimationProof.Run ^
  -animationOutput <evidencias> -animationFrames <pasta de quadros> ^
  -logFile <log>
```

Sem `-quit`: o harness sai depois do Play Mode. Marcador: `ILLUSTRATED_ANIMATION_PROOF_DONE success=True`. GIFs montados por `python tools/make_animation_gif.py` a partir dos quadros brutos (sem retoque).

Testes: `Unity.exe -batchmode -projectPath <copia> -runTests -testPlatform PlayMode -testResults playmode-tests.xml`.

Comparação controlada (edit mode, cerca de 1 minuto; não salva cena nem entra em Play Mode):

```
Unity.exe -batchmode -projectPath <copia> -quit ^
  -executeMethod YunaLegend.EditorScripts.IllustratedDeformationDetail.Run ^
  -detailOutput <evidencias>/deformation -logFile <log>
```

Marcador: `ILLUSTRATED_DEFORMATION_DETAIL_DONE panels=5`. Folha montada por `python tools/make_deformation_sheet.py --dir <deformation> --out deformation-comparison.png`.

## Arquivos

- `Assets/Scenes/Validation/IllustratedAnimationProof.unity` — cena isolada da prova.
- `Assets/Scripts/Runtime/Illustrated25D/PaintedMeshDeformer.cs` — malha proporcional e deformação.
- `Assets/Scripts/Runtime/Illustrated25D/IllustratedPaintedAnimation.cs` — condução pela pose do controlador.
- `Assets/Scripts/Editor/IllustratedAnimationProof.cs` — construção da cena, captura, medição e contrato.
- `Assets/Scripts/Editor/IllustratedDeformationDetail.cs` — comparação controlada em escala (edit mode).
- `Assets/Scripts/Tests/PlayMode/IllustratedAnimationProofTests.cs` — 4 contratos PlayMode.
- `Assets/Art/Illustrated25D/Yuna/Painting_Mesh_Unlit.mat` — material da malha (mesma pintura).
- `tools/make_animation_gif.py` — montagem dos GIFs a partir dos quadros.
- `tools/make_deformation_sheet.py` — folha de comparação e mapa de deslocamento.

O componente de prova fica **apenas** na cena de validação; um teste falha se ele aparecer na cena Gameplay.
