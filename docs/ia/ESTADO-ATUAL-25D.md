# Estado atual — sala escura ilustrada

Atualização em 12/11/2026, sessões `2026-11-12-dsh-resume-25d-7c41`. Este é um índice curto; reservas e resultados verificáveis permanecem no estado estruturado e no diário.

## Feito e aprovado

- Inventário T00 e base técnica T00B aceitos com limites registrados. Menu, entrada na fase, dois saltos, morte e retry automatizados pelo Input System; correção de foco do menu; build WebGL local da base anterior.
- Direção T01 aprovada: Yuna, Nanda azul, urso arredondado, livros inclinados, móvel e composição noturna. [Bíblia aprovada](../../Evidence/25d/T01/DIRECAO-VISUAL-APROVADA.md).
- Yuna estática na Unity aprovada em T02.1: pintura integral, transparência, pivô/escala, ordenação e oclusão em cena isolada.
- Projeção A, perspectiva suave 45°, escolhida em T02.2a. [Aceite literal](../../Evidence/25d/T02.2/reviews/T02.2a-v1-aprovacao.md).
- Câmera A estável aprovada em T02.2b: Yuna em 134,4 px constantes, acompanhamento suave e antecipação do pouso. [Registro da resposta humana](../../Evidence/25d/T02.2b/review-v1.md).

## Entrega atual para revisão

[T02.3 — prova do método de animação e orçamento](../../Evidence/25d/T02.3/README.md): a pintura aprovada deformada por uma malha proporcional (481 vértices), conduzida pelo estado real do controlador, em cena isolada com a câmera aprovada. Medido: camada de 0,167 ms por quadro, sola nunca abaixo do apoio, root/colisor/pivô intactos, Gameplay e controlador com SHA inalterado. **As pernas não articulam** e a camada resultou quase imperceptível (≈2 px de oscilação no ciclo, inclinação de sinal invertido): a prova mostra o limite de deformar uma pintura única e sustenta a decisão de quadros desenhados em T03.

## Próximos passos pequenos

1. Registrar a avaliação da prova T02.3 e ajustar se necessário; só então fechar T02.
2. T03: repouso, corrida, salto/pouso e tropeço de Yuna com poses desenhadas, um comportamento por revisão.
3. T04/T05: Nanda reconhecível com luz azul e ambiente ilustrado, um objeto por revisão, mantendo apoios reais e móvel cedendo/retry.
4. Integrar somente o piloto urso → livros → móvel; aprovação em movimento antes de expandir a fase.

Pendências de integração: calibração visual dos pés sobre superfícies reais (root está cerca de 0,08 m acima delas, próximo da skin do CharacterController), composição retrato, narrativa completa e dispositivos reais. Não mudar root/física ou achatar objetos para esconder diferenças. Não houve publicação da nova apresentação.

## Histórico separado

[Arquivo da direção anterior](../../Archive/pre-25d-2026-11-12/README.md). Documentos 3D, estados antigos, auditorias, capturas, sondas e referências foram agrupados com manifesto de origem/destino e SHA. Diário e evidências aprovadas 2,5D não foram reescritos. Caminhos antigos neles podem ser resolvidos por `python tools/archive_history.py resolve CAMINHO`.

O código, as cenas base, materiais e modelos ainda consumidos pela Unity permanecem em Assets. Arquivar uma decisão artística não autoriza retirar uma dependência de execução. O roteiro narrativo, as licenças e as instruções de build/publicação continuam ativos.
