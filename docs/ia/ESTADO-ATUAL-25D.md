# Estado atual — sala escura ilustrada

Atualização em 12/11/2026, sessões `2026-11-12-dsh-resume-25d-7c41` e `2026-11-12-codex-run-tail-e42b`. Este é um índice curto; reservas e resultados verificáveis permanecem no estado estruturado e no diário.

## Feito e aprovado

- Inventário T00 e base técnica T00B aceitos com limites registrados. Menu, entrada na fase, dois saltos, morte e retry automatizados pelo Input System; correção de foco do menu; build WebGL local da base anterior.
- Direção T01 aprovada: Yuna, Nanda azul, urso arredondado, livros inclinados, móvel e composição noturna. [Bíblia aprovada](../../Evidence/25d/T01/DIRECAO-VISUAL-APROVADA.md).
- Yuna estática na Unity aprovada em T02.1: pintura integral, transparência, pivô/escala, ordenação e oclusão em cena isolada.
- Projeção A, perspectiva suave 45°, escolhida em T02.2a. [Aceite literal](../../Evidence/25d/T02.2/reviews/T02.2a-v1-aprovacao.md).
- Câmera A estável aprovada em T02.2b: Yuna em 134,4 px constantes, acompanhamento suave e antecipação do pouso. [Registro da resposta humana](../../Evidence/25d/T02.2b/review-v1.md).

## Prova técnica concluída e entrega atual

[T02.3 v2 — prova do método de animação](../../Evidence/25d/T02.3/README.md) aprovada pelo usuário nesta conversa em 12/11/2026. [Resposta literal e hashes](../../Evidence/25d/T02.3/reviews/T02.3-v2-aprovacao.md). A inclinação foi corrigida na v2; camada medida em 0,164 ms por quadro no editor, root/colisor/pivô preservados e 32/32 PlayMode registrados na entrega anterior. **As pernas não articulam** e o ciclo tem cerca de 2 px de oscilação: aprovação da prova não significa corrida final aprovada. Poses desenhadas continuam sendo a direção de T03; deformação fica como recurso secundário.

**T02 aceita tecnicamente:** [contratos](../../Evidence/25d/T02/CONTRATOS-TECNICOS.md) e [auditoria](../../Evidence/25d/T02/technical-audit.json) com 23/23 verificações. Metas de contato e orçamento definidas; arte/física preservadas. O conjunto completo de amostras grounded apresenta root/apoio de 0,0800–0,1475 m, ainda a calibrar visualmente na integração. Atlas e cobertura de aparelhos continuam pendentes.

**T03.1 v1 aprovada:** [repouso/respiração/piscada](../../Evidence/25d/T03.1/README.md), com [resposta humana registrada](../../Evidence/25d/T03.1/reviews/T03.1-v1-aprovacao.md). Essa aprovação não se refere à corrida.

**T03.2 v2 aprovada:** clipe de 128 quadros (11,0053 passadas), início na troca de pose e régua do chão re-espaçada (1,00848 m): fase no loop **0,0053 ciclo**, grade **0,0 m**, salto de tela no loop 5,0 px (mediana entre quadros 6,3 px); apoio preservado (deriva 0,0 m), pintura/física/velocidade/colisor/Gameplay intactos. O usuário aprovou após ver os GIFs, aceitando como estado atual o **passo do corpo entre poses** (até 0,307 m; 21/127 intervalos parados). [Aprovação literal](../../Evidence/25d/T03.2/reviews/T03.2-v2-aprovacao.md), [revisão](../../Evidence/25d/T03.2/reviews/T03.2-v2.md), [entrega](../../Evidence/25d/T03.2/review-v2/README.md). Permanece aberta a decisão de arte (passada ~2,4× mais longa) ou velocidade/cadência (~1,63 m/s) para eliminar esse passo; a passada pintada é 0,6317 m por ciclo contra 2,016 m do controlador (3,19×). T03.1 aprovada; T02 aceita tecnicamente.

Detalhe da v1 (histórico): [corrida com oito poses pintadas](../../Evidence/25d/T03.2/README.md), pernas/joelhos/sapatos alternando, em chão técnico com controlador real a 5,2 m/s. Verificada a 30/60/120 Hz na v1; âncora de sola com deslizamento máximo <0,000001 m; colisor/physics/Gameplay preservados. **Tranco da v1 confirmado:** na última passada completa (3,833→3,867 s), a compensação do corpo passava de −0,22082 m para zero de uma vez (~21 px a 720p). A v1 não foi aprovada nem rejeitada; fica em **ajustar** e é preservada como histórico.

## Próximos passos pequenos

**Ajuste adiado pelo usuário:** ponto futuro registrado no diário na entrada **595**. Manter o diagnóstico disponível; nenhuma correção da corrida foi executada neste pedido.

1. T03.3: salto com subida, queda e pouso — um comportamento por revisão. As poses pintadas exigem imagegen, que esta sessão não tem; o passo técnico (cena/fixture/captura) pode ser preparado, mas a arte precisa de uma sessão com essa ferramenta.
2. Depois: T03.4 tropeço e recuperação; transições e orçamento conjunto ainda pendentes.
3. T04/T05: Nanda reconhecível com luz azul e ambiente ilustrado, um objeto por revisão, mantendo apoios reais e móvel cedendo/retry.
4. Integrar somente o piloto urso → livros → móvel; aprovação em movimento antes de expandir a fase.

Pendências de integração: calibração visual dos pés sobre superfícies reais (amostras de root/apoio vão de 0,0800 a 0,1475 m, próximo da skin do CharacterController), composição retrato, narrativa completa e dispositivos reais. Não mudar root/física ou achatar objetos para esconder diferenças. Não houve publicação da nova apresentação.

## Histórico separado

[Arquivo da direção anterior](../../Archive/pre-25d-2026-11-12/README.md). Documentos 3D, estados antigos, auditorias, capturas, sondas e referências foram agrupados com manifesto de origem/destino e SHA. Diário e evidências aprovadas 2,5D não foram reescritos. Caminhos antigos neles podem ser resolvidos por `python tools/archive_history.py resolve CAMINHO`.

O código, as cenas base, materiais e modelos ainda consumidos pela Unity permanecem em Assets. Arquivar uma decisão artística não autoriza retirar uma dependência de execução. O roteiro narrativo, as licenças e as instruções de build/publicação continuam ativos.
