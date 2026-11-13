# T05.4 — contato v2, candidata para revisão

Sessão `2026-11-13-gpt-t054-contact-91d2`, Codex GPT, 13/11/2026.

Capturas estáticas reais da Unity 6000.3.28f1, 1280×720. Cena aberta em memória,
sem salvar Gameplay. A candidata anterior permanece em `../capture/`.

- Pilha apoiada: uma cópia da pintura aprovada, sem deformação, fica atrás e
  abaixo da pilha original; os livros inferiores expostos completam 0,386798 m.
  A capa de pouso, o perfil e o collider originais não foram deslocados.
- Camada de primeiro plano omitida nesta candidata: a poltrona semitransparente
  deixa de atravessar o móvel. PNG original preservado.
- Sombras procedurais orientadas no plano XZ do piso, com opacidade reduzida.
  São aproximações de contato, não iluminação dinâmica ou sombras projetadas.

Medição em `room-audit.json`: base antes −8,468763 m, depois −8,855561 m,
piso −8,855561 m; erro de base 0 m e alteração de topo 0 m.
Sentinela `T054_ROOM_CAPTURE_OK` em `../contact-v2.log`.

Reprodução: executar `YunaLegend.Illustrated25D.Environment.Editor.RoomT054Capture.Run`
em batchmode, sem `-quit`; saída padrão neste diretório.

## Revisão individual aprovada — 13/11/2026

Conferir `02-quarto-livros.png` e `04-quarto-movel.png`: a nova pilha parece
sustentada e natural? O móvel ficou livre da mancha que cobria sua frente?
Resposta literal do usuário nesta conversa, após a apresentação da captura 02:
“agora sim! :) Sabia que você ia conseguir.”
Aceite limitado à correção de contato dos livros e sobreposição do primeiro plano
apresentada. Não aprova escala, iluminação final, T05.4 agregada ou runtime.

Escala de janela/rodapé e acabamento de iluminação continuam pendentes.
O bloco preto à direita do móvel é uma plataforma posterior preservada pelo
harness, fora do piloto urso→livros→móvel. Nenhuma travessia, quebra, pausa,
tropeço ou retry foi validado nesta captura estática.
