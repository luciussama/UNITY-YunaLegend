# Compromisso de continuidade — móvel, quebra e desequilíbrio

Direção confirmada pelo usuário em 12/11/2026, após aprovação visual do móvel: manter o móvel se quebrando e Yuna se desequilibrando na versão2D/2,5D. Registro detalhado em `Evidence/25d/T01/reviews/T01.5-v1-aprovacao.md`, sessão `2026-11-12-codex-composition-25d-90ba`.

## Sequência a preservar e provar

1. Yuna atravessa o tampo de madeira visível. Fratura por fadiga localizada, irregular e seguindo os veios; nada de corte reto uniforme ou apoio invisível ligando lacunas.
2. Ao evento de carga/estralo existente, o tampo cede. Separar estrutura estática, regiões móveis, abertura/fibras/lascas e estados visuais suficientes para que a ruptura apareça coerentemente. A técnica exata de camadas/rig/quadros será escolhida e provada no protótipo; não depender de uma única pintura intacta esticada.
3. Visual e contatos físicos recebem a mesma pose. Qualquer recorte aberto deve coincidir com a falta de apoio real, conforme `PlatformSupportProfile`; ajustar geometria ao formato aprovado.
4. Preservar o estralo e a pausa **local de80ms** na lógica existente. Não congelar o jogo inteiro nem alterar tempo global para simular o evento.
5. Yuna apresenta tropeço2D e recuperação: reação do corpo, braços buscando equilíbrio, ajuste dos pés e movimento secundário do vestido/cabelo. Não fingir animação final inclinando apenas a imagem inteira. Ritmo e poses requerem avaliação humana isolada.
6. O retry restaura tampo, fratura, efeitos/áudio, estado do evento, pose de Yuna e relógio dos passos; permitir novo evento único na tentativa seguinte.

A base já possui `YieldingWoodFurniture`, integração com o controlador e evento de tropeço. A migração deve reaproveitar essa estrutura onde adequada, adaptando apresentação e contato. Preservar a referência atual de recuperação280ms/cedimento7,5cm até mudanças justificadas por comparação; esses valores não certificam uma animação2D que ainda não existe.

## Critérios de revisão

- T03.4: provar tropeço/retomada de Yuna isoladamente em movimento.
- T05.3: integrar o móvel por camadas/estados; conferir arte/contato e abertura antes/depois da ruptura.
- T06.2: apresentar travessia real completa, estralo, pausa local, cedimento, tropeço e saída em câmera final.
- T06.3: falhar e repetir, demonstrando reset correto e repetição sem eventos duplicados.
- Registrar testes pertinentes para30/60/120Hz, evento único, duração local, pose/contato e reset, além de vídeo real e avaliação humana. Testes verdes não substituem julgamento da fluidez.

**Estado desta confirmação:** compromisso registrado; nenhuma arte animada, camada de runtime ou física nova foi implementada por este documento. A aprovação do estudo estático não aprova antecipadamente animação, contato ou sequência integrada.
