# T05.4 — janela alta v4, candidata para revisão

13/11/2026, sessão `2026-11-13-gpt-t054-contact-91d2`, Codex GPT.

Feedback humano da escala-v3: “A janela está muito baixa. Ela deveria ser mais
alta, e a minha sugestão é que ela fique bem acima do móvel fraturado, pois ela
será a única iluminação, quando a sala estiver efetivamente na penumbra, além
da fadinha.” V3 requer ajuste; não aprovada.

A janela foi movida para cima e para a direita, sobre o móvel no enquadramento04.
Tamanho da janela, rodapé, plataformas, câmera e contato-v2 preservados.
Luar pintado acompanha a nova posição. Primeiro ensaio cortava o topo da janela;
foi preservado em `attempt1/`. A edição final desceu janela/quadros em4% da altura
da pintura para dar margem à câmera, mantendo separação ampla acima do móvel.

## Fonte e reprodução

Built-in imagegen, duas edições do panorama-v3, sem processamento raster posterior.
Fontes em `../source/window-v4/`; asset irmão `room-panorama-v4.png` em
`Assets/Art/Illustrated25D/Environment/Room/`. Prompts em `PROMPTS.md`.
Panorama2172×724, largura32m, PPU67,875; mesmas âncoras e parallax0,35 da v3.

Unity6000.3.28f1, capturas1280×720 reais, sem salvar Gameplay.
Reprodução batch: `YunaLegend.Illustrated25D.Environment.Editor.RoomT054Capture.Run`
com `-t054HighWindow -t054Output Evidence/25d/T05.4/window-v4`, sem `-quit`.

## Direção de iluminação confirmada

Na apresentação final em penumbra, as únicas fontes de luz devem ser o luar
desta janela alta e a emissão azul de Nanda. Não introduzir luz quente ambiente
ou luminárias extras. Luz refletida pode dar legibilidade sem parecer outra fonte.
Esta candidata ainda usa objetos unlit e iluminação de inspeção: não comprova
penumbra final, influência real da janela/Nanda sobre superfícies ou oclusão da luz.
Essa implementação é a próxima microentrega após a revisão de posição.

Revisão individual aprovada em13/11/2026. Resposta literal do usuário nesta
conversa, após a captura04 final: “legal, aprovado.” Aceite limitado à altura
e posição da janela acima do móvel e à folga no enquadramento apresentado.
Sem aceite agregado da T05.4, sem validação runtime da quebra ou travessia.
O bloco preto à direita continua sendo plataforma posterior fora do piloto.
