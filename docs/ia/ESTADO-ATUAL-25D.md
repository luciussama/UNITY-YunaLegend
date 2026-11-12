# Estado atual — sala escura ilustrada

Atualização em 12/11/2026, sessões `2026-11-12-codex-camera-stable-74ac` e `2026-11-12-codex-archive-previous-25d-9e5b`. Este é um índice curto; reservas e resultados verificáveis permanecem no estado estruturado e no diário.

## Feito e aprovado

- Inventário T00 e base técnica T00B aceitos com limites registrados. Menu, entrada na fase, dois saltos, morte e retry automatizados pelo Input System; correção de foco do menu; build WebGL local da base anterior.
- Direção T01 aprovada: Yuna, Nanda azul, urso arredondado, livros inclinados, móvel e composição noturna. [Bíblia aprovada](../../Evidence/25d/T01/DIRECAO-VISUAL-APROVADA.md).
- Yuna estática na Unity aprovada em T02.1: pintura integral, transparência, pivô/escala, ordenação e oclusão em cena isolada.
- Projeção A, perspectiva suave 45°, escolhida em T02.2a. [Aceite literal](../../Evidence/25d/T02.2/reviews/T02.2a-v1-aprovacao.md).

## Entrega atual para revisão

[T02.2b — câmera A estável](../../Evidence/25d/T02.2b/README.md): tamanho de Yuna praticamente constante em 134,4 px a 720 px de altura; acompanhamento suave e antecipação do próximo pouso. Captura real com entradas de teclado e 4/4 testes PlayMode de câmera aprovados. Modo opt-in apenas em `Assets/Scenes/Validation/IllustratedStableCamera.unity`; Gameplay e controlador físico preservados.

**Aguardando revisão humana de conforto desta câmera.** T02 continua ativa. A Yuna ainda usa uma única pose estática durante o deslocamento dos saltos; não há animação corporal 2D final. Ambiente e esfera de Nanda nas provas ainda são a base técnica antiga.

## Próximos passos pequenos

1. Registrar a avaliação da câmera T02.2b e ajustar se necessário.
2. T02.3: prova do método de animação, ciclo/salto provisórios e custo medido; revisão antes de fechar T02.
3. T03: repouso, corrida, salto/pouso e tropeço de Yuna, um comportamento por revisão.
4. T04/T05: Nanda reconhecível com luz azul e ambiente ilustrado, um objeto por revisão, mantendo apoios reais e móvel cedendo/retry.
5. Integrar somente o piloto urso → livros → móvel; aprovação em movimento antes de expandir a fase.

Pendências de integração: calibração visual dos pés sobre superfícies reais (root está cerca de 0,08 m acima delas, próximo da skin do CharacterController), composição retrato, narrativa completa e dispositivos reais. Não mudar root/física ou achatar objetos para esconder diferenças. Não houve publicação da nova apresentação.

## Histórico separado

[Arquivo da direção anterior](../../Archive/pre-25d-2026-11-12/README.md). Documentos 3D, estados antigos, auditorias, capturas, sondas e referências foram agrupados com manifesto de origem/destino e SHA. Diário e evidências aprovadas 2,5D não foram reescritos. Caminhos antigos neles podem ser resolvidos por `python tools/archive_history.py resolve CAMINHO`.

O código, as cenas base, materiais e modelos ainda consumidos pela Unity permanecem em Assets. Arquivar uma decisão artística não autoriza retirar uma dependência de execução. O roteiro narrativo, as licenças e as instruções de build/publicação continuam ativos.
