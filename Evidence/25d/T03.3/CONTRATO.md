# T03.3 — contrato da candidata de salto

Codex (GPT-6), sessão `2026-11-12-codex-jump-t033-c91e`, 12/11/2026.
Transferência de T03 autorizada explicitamente pelo usuário; plano 628 permanece
de autoria DeepSeek. T03.1/T03.2 aprovadas não autorizam aprovação automática do salto.

## Apoio e apresentação antes da prova

Fixture: colisor plano técnico real de T03.2, topo Y=0; não é plataforma do piloto.
Pés pousam nesse topo. Nenhum contato físico novo, ponte ou Physics2D.
O controlador 3D continua com os valores serializados da cena: velocidade
5,2 m/s, altura nominal 2,6 m e gravidade 22 m/s² (diferem dos defaults 3/28
do script, sem alterar nenhum dos dois). Um Space do Input System inicia o arco, sem definir posição/velocidade
por um segundo simulador. Pouso visual só começa quando `IsGrounded` retorna.

Arte original produzida pelo imagegen embutido com a pintura aprovada como
referência de identidade; prompt integral em `prompt-jump-v1.txt`. Fonte bruta
`yuna-jump-generated-v1.png`, cópia byte a byte em `Jump/Yuna_Jump_Atlas_v1.png`.
O tamanho real retornado é 1448×1086, oito células 362×543 em 4×2. Não é o
2048×1536 solicitado; importação/slicing usa dimensões reais.

PPU único 1049,2857666 × 362/1024, mesma escala de canvas da corrida.
Não ajustar tamanho por caixa opaca. O pivô X é 536/1024 da largura da célula.
Baseline de canvas 511 px, exceto as solas de contato medidas em 508/510 px
nas poses 5/6; não alinhar pé recolhido de voo ao chão. Subida/apice/queda
mantêm o referencial do canvas e são deslocadas pelo root físico.

Oito desenhos: saída, subida inicial, subida final, ápice recolhido, queda,
preparação de contato, impacto e recuperação. O relógio de cada ação visual
não altera o arco. O impacto/recuperação é curto porque o root segue correndo;
avaliar a transição para corrida e o contato juntos. Render de corrida aprovado
é reutilizado sem mudar seus fontes. Aterrissagem não pode congelar o root.

## Verificações e entrega pretendida

- Provar uma saída, subida, ápice, queda e aterrissagem a 30/60/120 Hz;
  fontes idênticos e hashes por captura, sem misturar provas de taxas distintas.
- Medir altura/tempos reais, raiz/colisor/escala, deriva/separação vertical do
  ponto de sola em contato e continuidade do deslocamento visual.
- Inspecionar também a sola opaca no render; um ponto matemático não certifica
  todo o sapato. Metas T02: 1 px vertical, transitório 2 px por até dois quadros
  de 60 Hz no pouso, 2 px horizontal durante contato plantado.
- GIF normal em 1280×720 com contexto do arco e ampliação/lento para articulação.
  Gameplay: câmera fixa em Y=2,05, 45°, Z=-9,0533, sem perseguir o arco ou zoom.
  Ampliação acompanha a personagem para inspecionar desenho, não conforto de câmera.
- Não chamar fixture de piloto integrado, nem medir aparelho/GPU por batch.
  Orçamento residente conjunto e T03.4 continuam pendentes.

Estado: candidata em produção, sem aprovação humana e sem promessa de que as
metas já foram cumpridas. Ensaios falhos ficam preservados e identificados.
