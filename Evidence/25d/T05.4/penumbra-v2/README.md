# T05.4 — penumbra v2, candidata para revisão individual

13/11/2026, sessão `2026-11-13-codex-penumbra-e92d`, Codex GPT-6.
Estado: **aprovada individualmente em 13/11/2026**. Resposta literal: “Aprovado.” Registro em [revisão individual](reviews/penumbra-v2-aprovacao.md).

A limpeza anterior foi considerada resolvida pelo usuário, mantendo os caches. Não há passo operacional anterior interrompido. A reserva T05 foi transferida formalmente para esta sessão pelo coordenador, por autorização do usuário para continuar o próximo ajuste.

## Apresentação

Panorama janela-v4, pinturas aprovadas e superfícies físicas preservados. Somente luar da janela, luz azul localizada de Nanda e fosforescência muito tênue nas plataformas. Sem luz quente adicional. O luar acompanha a janela no parallax; a luz azul acompanha a posição de Nanda. Cor das pinturas de origem inalterada, incluindo a identidade creme/carmim de Yuna.

Shader nativo URP por fragmento, em coordenadas do mundo 3D, aplicado somente na candidata. Camada projetada de iluminação 2,5D sobre pintura: não usa Physics2D, não troca pipeline, não desloca apoio e não gera novas imagens. Refletância baixa do luar se apaga ao desligar essa fonte. A fosforescência é emissão própria discreta, sem halo grande nem nova lâmpada. Nanda usa a pose aprovada da T04.1; Yuna usa o Sprite oficial aprovado, em escala original. As duas são provas estáticas nesta captura, não integração T06.

`PenumbraLightingT054` atualiza os materiais locais em LateUpdate. Mesma ordenação/transparência dos materiais anteriores; materiais originais restaurados ao destruir o componente. Materiais e iluminação somente instalados quando se usa `-t054Penumbra`; Gameplay não foi salvo.

## Evidências reais

- Unity6000.3.28f1, 4 PNGs1280×720 da câmera de inspeção aprovada, sem retoque de imagem.
- `room-audit.json`: erro de piso0, delta do apoio dos livros0,9 renderers iluminados.
- `proof/`: câmera/apoios fixos, Nanda off, Nanda deslocada4m, luar off, somente fosforescência, todas fontes off.
- `lighting-checks.json`:7checagens passaram. Tampo46,21 de luminância média0..255 completo,24,75 semNanda,25,41 comNanda deslocada; região exclui sprite da fada. Yuna38,22/13,45 completa/semNanda. Janela74,78/0 com/sem luar. Plataforma distante3,27 só fosforescência e0 com tudo off: emissão verificável e muito abaixo da região iluminada.
- `regression-window-v4/`: quatro capturas **sem** flag de penumbra, idênticas pixel a pixel às imagens aprovadas da janela-v4.
- `editmode.xml`:52/52 aprovados, zero falhas/skips. PlayMode32/32 e percurso menu/urso/livros/morte/retry já haviam passado na sessão anterior; não foram reexecutados nem atribuídos a esta nova candidata.
- `hashes.json`: fontes, artes usadas e evidências com SHA-256. Nenhuma pintura foi editada ou gerada nesta etapa.

A primeira tentativa, `../penumbra-v1/`, tinha legibilidade insuficiente e listras por ZWrite indevido nos meshes transparentes do urso/livros. Foi preservada como tentativa técnica superada; não é a candidata atual.

## Reprodução

Com nenhum Editor concorrente, executar Unity com:

```
-batchmode -projectPath <repo> -executeMethod YunaLegend.Illustrated25D.Environment.Editor.RoomT054Capture.Run -t054Penumbra -t054Output <saida> -logFile <log>
```

Sem `-quit`; o harness encerra o Editor. Para a regressão anterior, substituir `-t054Penumbra` por `-t054HighWindow`. Logs locais em `Build/25d/t054-penumbra-*.log`.

## Limites e revisão

Iluminação ilustrada projetada; **sem sombras dinâmicas de oclusão**. Sombras de contato anteriores preservadas. Capturas estáticas não validam iluminação em movimento, física do móvel, runtime da travessia, WebGL ou dispositivo. Plataformas fora do piloto e visuais/fontes legados são ocultados só na fixture desta captura. Cena principal e controladores não foram alterados.

Revisão individual recebida conforme AGENTS.md/25D-ASSINATURA-E-MICROENTREGAS.md: o usuário aprovou a candidata apresentada. O aceite estético é limitado à penumbra estática, luar, luz azul e brilho tênue das plataformas. T05.4/T05 agregadas e T06 continuam pendentes; movimento e oclusão dinâmica não foram aprovados.
