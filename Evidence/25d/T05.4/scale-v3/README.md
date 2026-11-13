# T05.4 — escala v3, candidata em revisão

13/11/2026, Codex GPT, sessão `2026-11-13-gpt-t054-contact-91d2`.

Nova pintura panorâmica contínua, gerada com o built-in imagegen, em vez de três
painéis separados. A geração inicial e a edição da janela estão preservadas em
`../source/scale-v3/`; asset irmão em
`Assets/Art/Illustrated25D/Environment/Room/room-panorama-v3.png`.
Não substitui arquivos aprovados anteriores. Prompts e proveniência em `PROMPTS.md`.

## Implementação e medidas

- Dimensões reais 2172×724; largura no mundo 32 m, altura 10,666667 m,
  PPU 67,875. NPOT None, máximo 4096, sem compressão, mipmaps ligados.
- Linha de piso adotada na linha 642, medida por transições horizontais
  na pintura; topo da madeira na linha 565. Altura da madeira: 77/67,875 =
  1,134 m, contra cerca de 2,77 m da candidata anterior.
- Janela agora baixa e larga; estimativa visual cerca de 2,4–2,7 × 1,4–1,5 m.
  Não é uma medição segmentada com tolerância de pixels certificada.
- Parallax 0,35. Fundo único sem emendas; camadas mid e near antigas omitidas
  para não reintroduzir objetos de outra escala ou a poltrona sobreposta.
- Base adicional dos livros e sombras suaves da contact-v2 preservadas.
  `floorErrorMetres=0`, `bookSupportDelta=0` no audit desta execução.

Quatro PNGs 1280×720 capturados na Unity 6000.3.28f1, sentinela
`T054_ROOM_CAPTURE_OK` em `../scale-v3.log`; compilação sem erros C#.
Cena Gameplay não salva. Sem Play Mode ou travessia nesta entrega.

Reprodução: batchmode com
`-executeMethod YunaLegend.Illustrated25D.Environment.Editor.RoomT054Capture.Run`
`-t054Panorama -t054Output Evidence/25d/T05.4/scale-v3`, sem `-quit`.

O campo `method` do audit mantém o rótulo do harness contact-v2; as dimensões,
parallax e os PNGs correspondem à execução com panorama v3.

## Revisão humana pendente

Conferir a proporção da janela/rodapé em `02-quarto-livros.png` e
`04-quarto-movel.png`. Esta é revisão da escala, não aceite da T05.4 agregada.
Acabamento de iluminação continua pendente: o luar é pintado, e não projeta
luz sobre os objetos unlit. Nanda ainda aparece como esfera técnica no harness.
O bloco preto à direita é uma plataforma posterior preservada, fora do piloto.
A estreita faixa escura inferior da câmera de inspeção continua presente.
Sequência do móvel e integração do piloto pertencem à T06.
