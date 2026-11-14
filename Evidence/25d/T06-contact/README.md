# T06 — contato na penumbra, candidata v3

13/11/2026, sessão `2026-11-13-codex-contact-penumbra`, registro 8.
**Candidata para revisão individual; não encerra T06/T07.**

Regressão relatada: urso e livros parecem flutuar, sem sombra junto ao chão.
O diagnóstico inicial de contraste (diário 979) era incompleto:

- A câmera do acompanhamento tem rotação identidade. As elipses de sombra XZ
  eram vistas de perfil; o harness estático aprovado tinha câmera inclinada.
- O piso era escolhido pelo limite inferior dos colisores de contato, que
  descrevem os apoios de pouso e não a base pintada. A base ilustrada do urso
  está em −9,10 m. A extensão visual dos livros terminava acima dessa base.
- Mesmo uma sombra corretamente orientada desaparece sobre o piso quase preto.

## Correção

O piso agora segue a base mapeada da pintura do urso. Uma cópia da pintura dos
livros, na escala original e atrás da pilha, completa a base até essa mesma cota,
usando a técnica já aprovada em contact-v2. A capa superior e seus colisores não
foram deslocados. Pinturas, controlador, física e câmera permanecem preservados.

As sombras de contato do acompanhamento são elipses projetadas para a câmera
horizontal, presas às bases. Um reflexo frio localizado ilumina a textura do piso
sob os objetos para que o núcleo escuro seja legível. Ele acompanha coordenadas
do mundo, responde à intensidade do luar e não clareia a parede ou o quarto todo.
É uma aproximação ilustrada de contato, **não sombra dinâmica de oclusão**.
O novo modo é opt-in; as capturas estáticas antigas não foram alteradas.

## Conferir

- [Comparação](before-after.png): esquerda anterior, direita candidata; urso
  acima e livros abaixo. Apenas recorte/composição por ffmpeg; sem correção de
  exposição ou retoque. Originais anteriores em `../T06/contact-regression/`.
- Capturas completas da candidata: [urso](v3-60/02-bear.png) e
  [livros](v3-60/03-books.png), renderizadas pelo executável, 1280×720.
- [Vídeo contínuo da candidata](final-60/continuous.mp4), com UI e sem áudio;
  simulação 60 Hz, captura 30 fps, cinco tentativas na mesma sessão.
- Build candidata: `Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe`.
  Backup aprovado: `Build/Windows/YunaLegend-Acompanhamento-T063-aprovada/`;
  [170 arquivos conferidos](approved-backup-check.json), nenhum divergente.

Perguntas da revisão: o urso e a pilha agora parecem apoiados no chão? A sombra
de contato e a iluminação discreta do piso ficam naturais durante a travessia?
Resposta humana desta candidata: **pendente**.

## Prova e reprodução

Unity 6000.3.28f1; exportador oficial
`YunaLegend.Illustrated25D.Preview.Editor.IllustratedPreviewBuild.Run`.
Build v3: `Succeeded`, zero erros, 189.851.593 bytes no resumo Unity.

`-previewVerify <saída> -previewHz 60` produz as capturas e mede, no início e após
retry, distância da base pintada ao piso e orientação da sombra. Resultado:
`bearFloorGap=0`, `booksFloorGap=0`, `shadowFacing=1`, `paintedContact=true`;
travessia, móvel e retry passaram, zero erros e um evento.
Isso mede a geometria mapeada da pintura; não substitui inspeção da borda alfa.

`-previewRetryVerify <saída> -previewHz 30|60|120` executa reinício na pausa local,
na recuperação, após completar e após falha; acrescentar
`-previewVideoEncoder <ffmpeg.exe>` para vídeo. Janela **em primeiro plano**,
1280×720, `-windowed`. Os relatórios finais estão em `final-*/verification.json`.
O vídeo segue tempo de simulação; não é benchmark de desempenho real.

Resultados da mesma build v3:

| Simulação | Retry durante pausa/recuperação e após falha | Erros | Eventos nas cinco tentativas |
| --- | --- | --- | --- |
| 30 Hz | passou | 0 | 1/1/1/0/1 |
| 60 Hz | passou | 0 | 1/1/1/0/1 |
| 120 Hz | passou | 0 | 1/1/1/0/1 |

Vídeo decodificado integralmente: 37,30 s, 1.119 quadros, 1280×720 a 30 fps;
quadros de urso/livros também inspecionados na captura com UI. A iluminação do
piso continua legível durante o deslocamento e após as recargas.
Verificação contra as fontes aprovadas: [105 arquivos de runtime/arte física](physical-art-check.json),
sem divergências. Os scripts de apresentação corrigidos estão no manifesto atual.

V1/v2 são ensaios anteriores, preservados; a candidata é **v3**.
Manifesto de fontes, build e provas: [manifest.json](manifest.json).
Limites anteriores permanecem: escuta/mixagem, aparelho/retrato/safe area,
WebGL, oclusão dinâmica, A/B da assistência horizontal e desempenho real.
Sem commit/push. T07 continua bloqueada até revisão da correção e consolidação T06.
