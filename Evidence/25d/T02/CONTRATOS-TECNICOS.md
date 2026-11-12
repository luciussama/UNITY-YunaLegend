# T02 — contratos técnicos do piloto

12/11/2026, sessão `2026-11-12-codex-close-t02-c73a`, Codex (GPT-6).
Revisão técnica dos fontes e das provas existentes; sem nova execução Unity.
As metas abaixo são critérios de produção e integração, não resultados de aparelho.

## Decisão

Manter Unity/C#, controlador 3D e pipeline URP existentes. Câmera A perspectiva
45°, escala estável. Quadros pintados para ações articuladas; deformação aprovada
como recurso secundário. Não usar a malha de uma única pintura para simular passada.
T02.1, T02.2a, T02.2b e T02.3 v2 possuem revisões humanas em suas pastas.

## Baseline e rastreabilidade

`technical-audit.json`, reproduzido por `python tools/audit_25d_t02.py`, verifica
hashes da pintura, Gameplay e controlador contra a prova, e dos componentes/cena
contra o checkpoint `5d2e6db`. Os 32 testes PlayMode registrados são anteriores;
incluem apoios desconexos/inclinados, ambos sentidos, 30/60/120 Hz, cedimento com
pausa local de 80 ms e retry. Isso valida a base física; não demonstra que a nova
arte do móvel já acompanha essa base.

CPU da deformação: média 0,1643 ms, máximo 0,2337 ms no editor. Textura 1024×1536:
12,0009 MiB contabilizados pelo editor; pixels RGBA crus: 6 MiB. Não somar nem
confundir essas duas medidas. Doze quadros crus: 72 MiB estimados, sem atlas real.
Altura neutra da personagem a 720p: 134,39993–134,40007 px. A malha tem 481 vértices,
864 triângulos; o custo de desenho completo/GPU não foi medido nesta prova.

## Contato visual e plataformas

Definir sempre os apoios reais antes de desenhar a plataforma. `PlatformSupportProfile`
continua fonte de regiões/segmentos: urso com cinco MeshColliders, livros com capa
inclinada e regiões desconexas, móvel de 8,4 m com duas regiões móveis. Nenhuma
ponte de colisão ou padronização de silhueta para facilitar o desenho.

Metas para T03/T05/T06, avaliadas na câmera final a 1280×720 e em escala equivalente:

- Contorno de apoio visível versus superfície física: erro máximo de 1 px em repouso,
  inclusive inclinações e extremos das regiões; 2 px durante cedimento, sem mudar
  a conectividade. Em z0, 96 px/m: 1 px corresponde a aproximadamente 0,01042 m.
- Pé plantado: separação vertical máxima de 1 px; tolerância transitória de 2 px
  por até dois quadros a 60 Hz no pouso. Medir pixels opacos da sola, não vértice
  de malha transparente. Pé no ar não é pé de apoio.
- Deslizamento do pé plantado em relação ao apoio: até 2 px no intervalo de contato.
  Em móvel cedendo, avaliar no referencial do tampo móvel, não no mundo parado.
- Calibrar somente o visual filho a partir da superfície real sob o pé. Limitar
  eventual compensação à skin medida + 0,02 m, sem alterar root, colisor, velocidade,
  gravidade, salto ou posição das plataformas. Sem superfície válida, não prender
  pé a um apoio inventado. Nunca prolongar contato sobre lacuna.
- Retry remove compensação/transientes, restaura tábuas e permite novo estralo único.

Nos pontos de repouso citados da prova o root está cerca de 0,08 m acima do apoio:
aproximadamente 7,7 px a 720p. O conjunto completo de amostras grounded com apoio
medido vai de 0,0800 a 0,1475 m (até 14,2 px); não reduzir essa faixa ao valor da skin.
**A prova ainda não passa a meta final de contato visual.** É uma lacuna conhecida
de implementação T03/T06; não é motivo para mudar física nem para reprovar a decisão
do método. O mínimo de vértices da malha +0,0133 m não mede a sola opaca contra o tampo.

## Importação, pivôs e luz

Pintura fonte original permanece intacta. Perfil aprovado: 1024×1536, PPU 1049,2858,
pivô em (536,29) px desde o canto inferior esquerdo; altura opaca neutra 1,4 m.
Novos quadros usam canvas e âncora de sola comuns, nunca normalização de cada pose
pela sua caixa opaca (isso faria o tamanho variar ao saltar/agachar).

Alpha transparente real, sRGB, clamp, bilinear, sem mipmaps, FullRect; manter o
importador da arte aprovada. Novos atlas: no mínimo 4 px de padding com extrusão
de borda, sem colisão entre células; fonte RGBA preservada e tamanho de runtime
reduzido somente após verificar rosto, olhos, tecido e contorno na câmera real.
Desabilitar Read/Write em novos sprites de runtime que não precisem de leitura CPU.
Compressão é variante a validar, não mudança global automática.

Shader URP Sprite-Unlit-Default branco conserva luz pintada. Mesma sorting layer
e ordem 0 para Yuna; fundos abaixo, efeitos secundários atrás quando apropriado.
Profundidade Z e oclusores opacos devem continuar escondendo Yuna como T02.1.
Parallax somente decorativo, sem deslocar plano de apoio. Não alterar renderer/pipeline.
Virada compensa yaw físico apenas no visual. Espelhamento é provisório: revisar
ambos sentidos para luz pintada coerente. Nanda deve ser azul; não recolorir Yuna.

## Estados e eventos

Ler o controlador; não introduzir um segundo relógio de movimento físico.
Prioridade: Stumble → Jump/Fall → Land → Run → Idle. A prova usa `GaitPhase`,
`IsGrounded`, `Velocity`, `IsStumbling`, `StumblePoseAmount`, `RunDirection`,
`AutoRunActive` e `IsDead`. T03 implementará poses/transições, com fase de corrida
relacionada ao deslocamento e contatos identificados. Retry reinicia estado e fase.
Estralo e pausa local continuam em `YieldingWoodFurniture`; animação reage aos
estados existentes e não os dispara por estimativa de tempo. Não usar Time.timeScale
para produzir os 80 ms locais. Despertar completo fica em T08.

## Orçamento de produção

Metas iniciais após a baseline; verificar no piloto e T09, sem alegar cobertura atual:

| Item | Limite/meta | Como validar |
| --- | --- | --- |
| Deformação secundária de Yuna | CPU média ≤0,5 ms; máximo observado ≤1 ms em captura longa | Mesma instrumentação da prova, depois aparelhos identificados |
| Yuna visível | 1 desenho por material; até 2 durante transição, ≤481 vértices se malha | Profiler/Frame Debugger na build do piloto |
| Arte residente de Yuna | ≤32 MiB, incluindo atlas carregados e cópias de textura contabilizadas | Profiler na build, carregamento por grupo de ações se preciso |
| Arte 2D residente do piloto | ≤128 MiB para Yuna, Nanda e ambiente | Medir texturas residentes, sem confundir PNG com GPU |
| Jogo completo | desktop 60 fps (16,67 ms); mobile mínimo 30 fps (33,33 ms) | Tempo total de quadro, CPU/GPU, aparelho/modelo/build identificados em T09 |

Exemplo de dimensionamento, **estimativa**: 12 quadros a 512×768 RGBA ocupam 18 MiB
de pixels antes de padding/cópias; isso exige validar redução e residência. Não
importar doze quadros 1024×1536 não comprimidos como solução final presumida.
Nenhuma meta acima transforma a captura batch em medição de GPU ou aparelho.

## Câmera, formatos e validação seguinte

45°, distância Z 9,0533 m, altura visível 7,5 m, SmoothDamp X 0,45 s; antecipação
pelo movimento existente e altura por apoios, sem perseguir o arco. Opt-in só em
cenas de validação; Gameplay principal preservada. 16:9 e 19,5:9 têm evidência.
Retrato exige composição própria antes de aprovação de piloto nesse formato;
nenhum achatamento dos objetos ou ajuste de salto para fazê-los caber.

T03 revisa repouso, corrida, salto/pouso e tropeço separadamente. T05 testa cada
contorno real. T06 mede pés/apoios e móvel em movimento e retry, repetindo casos
relevantes a 30/60/120 Hz. T07 exige build e aprovação do piloto contínuo. T09 cobre
dispositivos reais, desempenho e retrato: sem aparelho, cobertura permanece pendente.
T02 aceita a prova/método e estes contratos; não declara cumpridas essas entregas.
