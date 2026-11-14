# T08C — continuação jogável até a cômoda 03

13/11/2026. Registro 11, sessão `2026-11-13-codex-comoda03-runtime`.
**Candidata jogável individual para revisão; não é conclusão da fase inteira.**

O usuário pediu continuar a evolução da fase e autorizou “pode prosseguirmos”.
A candidata acrescenta uma travessia: **urso → livros → móvel → cômoda**.
Usa a câmera de subida sem zoom aprovada em T08B. Marcadores 4 em diante seguem
desativados. O móvel anterior e seus visuais não foram redesenhados.

## Abrir

- Build: `Build/Windows/YunaLegend-Continuacao/YunaLegend.exe` (manter a pasta inteira).
- [Vídeo contínuo](final-video-60/camera-ascent.mp4): duas travessias e um reinício.
- [Cômoda em jogo](dresser-in-game.png),
  [pouso](final-60/attempt-0-dresser-land.png),
  [repouso final](final-60/attempt-0-dresser-idle.png).
- Cena Unity própria: `Assets/Art/Illustrated25D/Environment/Dresser/Preview-Continuacao.unity`.
  Abrir essa cena para avaliar a continuação no Editor. A cena de acompanhamento
  anterior e suas builds foram preservadas; o menu anterior continua disponível.

Vídeo nativo604×720, **592 quadros / 19,73 s a 30 fps**, simulação60 Hz, com UI,
sem áudio, cortes, retoque ou deformação. A janela desktop estreita não equivale
a teste em aparelho/retrato/safe area. [Decodificação integral](final-video-60/video-inspection.txt).

## Apoio definido pela pintura

A escala uniforme agora usa a **borda frontal de pouso**, não o ponto mais alto
da perspectiva desenhada. No PNG1254×1254: linha frontal y=130,5px, base dos pés
y=1183,5px, centrox=621,5px. Para ligar piso−9,10m ao apoio−6,05m, são
345,2459px/m. A largura visível resulta2,9052m, sem achatar ou esticar a arte.
A borda de trás desenhada fica mais alta por perspectiva; não é a linha dos pés.

O apoio horizontal próprio mede **2,50m × 1,749m**, dentro das bordas do tampo.
Somente a cômoda03 recebe esse novo contato. Sua posiçãox=18,9121m é calculada
pelo apoio real anterior, preservando **vão1,35m em repouso**. A deformação do
móvel anterior continua ativa durante o salto. `PlatformSupportProfile` e a
física3D continuam governando pouso, vão e trajetória.

Na cômoda, a folga da cápsula é compensada apenas no visual de Yuna, mantendo
raízes físicas, colisores e animações originais. A compensação só atua com apoio3
e é removida antes da avaliação seguinte; poses de voo conservam sua elevação.
Não foi aplicada ao chão, urso, livros ou móvel anterior.

Um receptor local usa a própria textura do piso para tornar a sombra de contato
legível junto à base. É uma aproximação ilustrada de reflexão/contato, não oclusão
dinâmica. A âncora do panorama foi mantida nos três objetos anteriores para não
reposicionar a janela aprovada ao acrescentar a cômoda.

## Validação no executável

| Simulação | Duas travessias completas | Reinício limpo | Saltos por tentativa | Estralos por tentativa | Erros / legado visível |
| --- | --- | --- | --- | --- | --- |
| 30 Hz | passou | sim | 4 / 4 | 1 / 1 | 0 / 0 |
| 60 Hz | passou | sim | 4 / 4 | 1 / 1 | 0 / 0 |
| 120 Hz | passou | sim | 4 / 4 | 1 / 1 | 0 / 0 |

Relatórios: [30](final-30/verification.json), [60](final-60/verification.json),
[120](final-120/verification.json). Player colocado em primeiro plano nas taxas.
No teste120Hz, manter a janela ativa devido ao limite já conhecido do background.

Auditoria independente da sola opaca em60Hz: **133 amostras de contato no apoio3**,
separação−1,348 a+1,348mm, aproximadamente±0,13px. Usa pixels vermelhos com alfa≥128
e a malha deformada para Idle, não apenas a cota do root. Poses Run de voo foram
excluídas desse resumo. [Resumo](feet-summary.json),
[amostras](final-feet-60/foot-contact.json).
Nas três taxas, o erro da âncora visual no novo apoio foi zero.

O [trace da câmera](final-video-60/camera-ascent.json) confirma Z−9,0533m,
FOV45° e projeção96px/m constantes. Não foi alterada a câmera aprovada.
Limites anteriores de áudio, aparelhos, WebGL, desempenho e demais defeitos de
sola permanecem registrados; não inferir correção global por estes testes.

## Reprodução e versão

Unity6000.3.28f1, buildv3 `Succeeded`, zero erros,196.167.681bytes no resumo Unity.
Entrada batch do exportador oficial:
`YunaLegend.Illustrated25D.Preview.Editor.IllustratedPreviewBuild.RunContinuation`.
O caminho atual gera a cena/candidata separadas. Exportador e menu anteriores
mantêm seu fluxo de acompanhamento sem a opção de continuação.

Prova: `-previewDresserVerify <pasta> -previewHz 30|60|120`.
Auditoria: `-previewFootAudit <pasta>`.
Vídeo: `-cameraAscentProof <pasta> -cameraAscentVideoEncoder <ffmpeg.exe>`.
Sem flags, nenhuma dessas instrumentações controla o jogo.

O primeiro ensaio em `route-60` chegou corretamente à cômoda, mas o verificador
encerrava antes de esperar o reload. Sua `success=false` foi preservada e o
guard de espera corrigido; a evidência final é exclusivamente `final-*`.

Fontes, cena, importação, build e provas: [manifest.json](manifest.json).
O menu Unity permaneceu com três atalhos. Nenhum commit/push ou publicação.
PreparaçãoT08A foi encerrada após autorização de prosseguir para esta avaliação;
não significa aceite final desta travessia nem aceite agregadoT06/T07.

Perguntas da revisão: a cômoda parece pertencer ao quarto? O salto, o contato dos
pés e a continuidade da subida ficaram claros? Resposta humana: **pendente**.
Próxima peça do percurso após esta avaliação: blocos ABC de madeira (04).
