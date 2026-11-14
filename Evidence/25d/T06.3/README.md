# T06.3 — reinício durante o evento do móvel

Entrega candidata de 13/11/2026, sessão `2026-11-13-codex-t063`, registro de agentes **5**.
**Validada tecnicamente; aguarda revisão humana da sequência. T06 permanece aberta.**

## Abrir e assistir

- Build oficial: `Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe` (manter a pasta inteira).
- Vídeo contínuo: [v3-visible-60/continuous.mp4](v3-visible-60/continuous.mp4).
- Inventário SHA-256: [manifest.json](manifest.json).
- Pacote portátil: `Build/Exports/YunaLegend-T063-20261113.zip`, com executável/dados, vídeo e relatórios.

Build Windows x64, Unity **6000.3.28f1**, exportador oficial
`YunaLegend.Illustrated25D.Preview.Editor.IllustratedPreviewBuild.Run`.
Resultado final: `Succeeded`, zero erros, **189.844.169 bytes** no resumo Unity.
O rebuild inicial pós-remoção passou smoke60 em `baseline-60/verification.json`;
a build final inclui também o verificador opt-in e foi novamente testada nas três taxas.

Visual, arte, controlador físico 3D e `PlatformSupportProfile` preservados.
Não houve correção de gameplay: o retry existente recarrega a cena e passou a nova prova.
A cena de acompanhamento foi regenerada pelo exportador a partir da Gameplay limpa.
A composição duplicada permanece removida; há somente um `RoomBackgroundT054` por cena.

## O que foi provado

Cada taxa executa cinco tentativas na mesma sessão do player, com quatro recargas reais:

1. Urso → livros → móvel; R ainda durante a pausa local do evento.
2. Nova travessia; R durante a recuperação do tropeço.
3. Nova travessia completa; P pausa e Esc retoma antes do primeiro salto; R após concluir.
4. Falha proposital, omitindo o primeiro salto; tela de falha; R.
5. Nova travessia completa depois da falha.

| Simulação | Reset na pausa | Reset na recuperação | Reset após falha | Eventos por tentativa | Erros/exceções |
| --- | --- | --- | --- | --- | --- |
| 30 Hz | passou | passou | passou | 1/1/1/0/1 | 0 |
| 60 Hz | passou | passou | passou | 1/1/1/0/1 | 0 |
| 120 Hz | passou | passou | passou | 1/1/1/0/1 | 0 |

Relatórios finais: [30 Hz](v3-30/verification.json),
[60 Hz com vídeo](v3-visible-60/verification.json), [120 Hz](v3-120/verification.json).
Os contadores são observados durante toda a tentativa, não somente após o reset.
Na tentativa que falha antes do urso não há evento de móvel; em cada travessia há exatamente um.

O reset confere: início da rota, morte/tropeço/pausa/pose desativados, tampo sem cedimento,
fratura oculta, contador zero, áudio do móvel parado, posições locais dos tampos físicos e
pintados iguais ao estado inicial, fase dos passos restaurada, relógio de apresentação da
companhia reiniciado e Nanda próxima. Em movimento, cada tampo pintado acompanha posição e
rotação do apoio físico. Zero renderers legados visíveis e uma fonte de Yuna visível.
O máximo medido de cedimento é **0,075 m** e de pausa local **0,08 s**.
A recuperação continua configurada em **0,28 s** no código preservado; não foi inferida uma
medição mais precisa de sua duração a partir do vídeo a 30 fps.

A instrumentação está em `PreviewRetryVerification.cs`; só atua com `-previewRetryVerify`.
A build normal mantém entrada e relógios normais. Para gravar UI, foi habilitado o módulo
builtin `com.unity.modules.screencapture` 1.0.0 e sua dependência builtin imageconversion.
Nenhuma versão de biblioteca externa do jogo foi atualizada.

## Vídeo e inspeção visual

Vídeo H.264 1280×720, **37,30 s / 1.119 quadros a 30 fps**.
Simulação fixada em **60 Hz**; captura a cada dois quadros, diretamente no executável,
em fim de frame, incluindo OnGUI. Uma única sessão, sem cortes, montagem ou retoque.
A escala temporal do arquivo segue a simulação; não representa desempenho/FPS real nem
preserva a duração de espera de carregamento no relógio de parede.

| Instante aproximado | Conteúdo |
| --- | --- |
| 0–7,2 s | Primeira travessia e R durante pausa local |
| 7,2–14,5 s | Segunda travessia e R na recuperação |
| 14,5–23,5 s | Terceira travessia, P/Esc e conclusão |
| 23,5–28,5 s | Falha proposital, UI de falha e retry |
| 28,5–37,3 s | Nova travessia e conclusão |

[Contato de quadros para inspeção](v3-visible-60/contact-sheet.jpg) e
[metadados/decodificação completa](v3-visible-60/video-inspection.txt).
Os quadros de reinício, móvel, falha e fim foram inspecionados visualmente.
Computer-use no mesmo executável, sem flags de teste, confirmou por clique:
**Começar → corrida → falha → Recomeçar → tela inicial**. O player ficou aberto no início.
O vídeo automático usa R; o clique foi validado separadamente, sem alegar que está no vídeo.

**Vídeo sem áudio.** O teste confere contador do evento e fonte de áudio parada após reset;
não houve gravação/escuta nem certificação de estralo/mixagem por audição.
Teclado sintético do Input System nos ensaios automatizados; botões por mouse na inspeção.
Sem teste de toque, aparelho móvel, WebGL ou benchmark de desempenho.

## Reprodução

Com o Editor fechado, executar o exportador batch acima ou usar o menu
**YunaLegend → Gerar build Windows de acompanhamento**.
No executável acrescentar:

```text
-previewRetryVerify <pasta-de-saida> -previewHz 30|60|120 -screen-width 1280 -screen-height 720 -windowed -logFile <log>
```

Para vídeo acrescentar `-previewVideoEncoder <caminho-ffmpeg.exe>` e usar janela **visível**.
Nesta máquina o encoder local está em
`Build/tools/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v9.0.2.exe`.
O encoder não é necessário para jogar nem para os testes sem captura.

Histórico preservado: `attempt-01-60` passou antes de habilitar o módulo;
`final-30/120` passaram na build anterior; `final-60` produziu vídeo preto por leitura
inválida do framebuffer. `v3-60` falhou na captura em janela oculta. Esses vídeos são
inválidos e não compõem a entrega. A prova final de 60 Hz é **v3-visible-60**.
Logs locais de build/player permanecem nesta pasta, ignorados pelo Git conforme regra existente.

## Revisão humana pendente

Conferir no vídeo a fluidez do retorno ao início e a leitura do novo evento na travessia
seguinte. A estética já aprovada é a base; não se pede nova revisão estética.
Sem resposta humana não há aceite T06/T07 nem autorização de expansão.
Reserva do registro e do coordenador mantida durante a revisão.
