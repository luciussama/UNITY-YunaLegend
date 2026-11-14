# Contato v4 — piso abaixo do friso

13/11/2026. Registro 8, sessão `2026-11-13-codex-contact-penumbra`.
**Candidata em revisão. Substitui a v3, que não foi aprovada.**

O usuário apontou na [imagem marcada](user-reference.png): “Você está posicionando
os objetos acima do friso do chão, por isso está suspenso.” A auditoria da v3
comparava as bases com uma cota calculada e não verificava o friso pintado.

O panorama original mede 2172×724. O antigo inset `82/724` escolhia a linha 642,
no acabamento inferior da parede. O friso termina aproximadamente na linha 654;
a ancoragem passa à linha **660**, na primeira faixa de tábuas: `64/724`.
Ver [recorte original do friso e piso](../floor-reference.png), sem retoque.

A correção reposiciona verticalmente o panorama em relação às bases existentes.
Os objetos não são achatados, nem seus apoios/colisores movidos. A altura e a
posição física de Yuna são preservadas. Câmera, parallax horizontal, pintura e
fontes de iluminação também são preservados. Mantida a sombra de contato da v3.

## Conferir

- [Antes/depois no início](ready-before-after.png): v3 à esquerda, v4 à direita.
- [Urso e livros](trim-before-after.png): v3 à esquerda, v4 à direita.
- Capturas completas: [início](smoke-60/01-ready-stable.png),
  [urso](smoke-60/02-bear.png), [livros](smoke-60/03-books.png).
- [Vídeo contínuo com UI](retry-60/continuous.mp4), sem áudio;
  simulação 60 Hz, captura 30 fps. Não é benchmark de desempenho real.
- Build atualizada: `Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe`.

Pergunta desta revisão: as bases agora parecem sobre o piso, **abaixo do friso**,
na posição indicada pelo usuário? Resposta humana: **pendente**.
T06/T07 continuam abertas; nenhum aceite é inferido dos testes.

## Prova e limites

Unity 6000.3.28f1, exportador oficial. Build `Succeeded`, zero erros.
Como o Editor central está em Play pausado, ele foi preservado: exportação na
cópia técnica `UNITY-YunaLegend-preview-20261113`. Antes de exportar, 872 arquivos
de Assets/Packages/ProjectSettings idênticos ao checkout central por SHA-256;
quatro scripts legados ausentes do central foram removidos apenas da cópia.
O exportador regenera a cena somente nessa cópia; não sobrescrevemos a cena
aberta no Editor central. [Auditoria](../v4-clone-check.json).
[169 arquivos da build copiados e conferidos](build-copy-check.json).
O `LEIA-ME.txt` adicional do destino é documentação local preservada.

[Smoke 60 Hz](smoke-60/verification.json): travessia completa, quebra, recuperação
e retry passaram, zero erros, um evento, bases com erro zero. A nova auditoria
usa também um marco independente na textura: borda inferior do friso na linha
654. As duas bases ficam **0,088398 m abaixo** desse marco; a v3 falharia porque
suas bases ficavam acima dele. Inspecionadas as bordas pintadas nas capturas.

[Retry 60 Hz](retry-60/verification.json): cinco tentativas com reinício na pausa,
na recuperação, após concluir e após falha. As provas 30/120 Hz da v3 são
históricas; não foram repetidas para esta alteração de alinhamento do fundo.
Sem alegar validação nova de áudio, aparelho/retrato/safe area, WebGL, oclusão
dinâmica, A/B da assistência horizontal ou desempenho real.

O Editor central continua pausado na execução antiga: reiniciar o Play é
necessário para reinstalar o panorama atualizado nele. A build nova já contém
a correção. Build T06.3 aprovada e evidências anteriores preservadas; sem commit/push.
Fontes, build e provas da v4 estão em [manifest.json](manifest.json).
