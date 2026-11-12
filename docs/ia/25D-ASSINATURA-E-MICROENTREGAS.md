# Assinatura preservada e entregas pequenas

Diretriz do usuário em 12/11/2026: “Vamos começar. Você é o engenheiro e orquestrador do projeto. Defina o trabalho, mas em passos pequenos, para avaliação humana meticulosa. Já que demos um passo atrás, a assinatura visual deve ser a mesma”. Sessão: `2026-11-12-codex-start-25d-4f6a`.

**Atualização vinculante, 12/11/2026, sessão `2026-11-12-codex-nanda-25d-e913`:** o usuário determinou que Nanda é uma menina que prefere azul; fazer sua aparência azulada e a luz que emana da mesma cor. Esta direção substitui o âmbar/dourado referido abaixo para Nanda, sem mudar a assinatura pintada ou a paleta de Yuna. Vestido, asas e emissão terão azuis suaves; pele/cabelo mantêm leitura natural com reflexos frios. A luz azul no conceito não comprova implementação da luz na Unity.

## Compromisso artístico

A migração muda a forma de construir a apresentação na Unity; preserva a assinatura ilustrada reconhecida no legado e no menu. A referência visual local principal é `Assets/Art/UI/Title_Background.png`, já existente no projeto. Não redesenhar menu, inventar outra protagonista ou trocar a linguagem por pixel art, vetor plano, anime genérico, recorte rígido ou render plástico.

Preservar, em conjunto:

- Pintura de livro infantil com volumes suaves e bordas trabalhadas, detalhes de tecido/madeira e luz integrada à ilustração.
- Yuna delicada, proporções infantis, rosto arredondado e olhos castanhos expressivos, cabelo castanho com franja e rabo de cavalo alto, vestido creme com carmim, meias claras e sapatos de tira. Comparar diretamente rosto e silhueta com o menu; não aprovar apenas porque roupa/cor coincidem.
- Rubi/carmim, creme, castanhos e luz quente como vínculos da identidade de Yuna. No quarto noturno, índigo/violeta e luar frio coexistem com a luz azul suave de Nanda; não tingir tudo de sépia nem clarear o quarto inteiro.
- Móveis e brinquedos com aparência tátil e doméstica, formas próprias, escala de aventura infantil e detalhes subordinados à leitura do caminho.
- Nanda como companheira reconhecível e expressiva, conforme a prancha azul aprovada em T01.2. A esfera Unity não serve de referência de aparência final.

A identidade atual foi fixada na pintura `Evidence/25d/T01/yuna-profile-v1.png` e aprovada novamente na Unity em T02.1. A ilustração do menu continua sendo a âncora de acabamento; não combinar livremente ornamentos de outras propostas. A prancha 3D anterior, complementar e histórica, está em `Archive/pre-25d-2026-11-12/Evidence/yuna-concept-2026-11-11/yuna-3d-concept-v1.png`; não é direção de produção 3D final.

O legado permanece referência de observação pela URL autorizada; não extrair nem importar suas imagens/código. Verificar proveniência antes de reutilizar arte local; registrar geração/autoria de novas imagens. Não é necessário refazer uma arte local utilizável apenas para chamá-la de nova.

## Unidade de trabalho e de aprovação

Uma entrega visual responde a uma pergunta principal. Apresentar referência, resultado, versão/hashes e duas ou três questões concretas de avaliação. Mostrar ampliação e tamanho real de gameplay quando pertinente. Aprovar desenho não aprova animação; aprovar animação isolada não aprova integração, contato ou câmera.

Produzir uma candidata bem fundamentada por vez. Alternativas apenas quando houver uma decisão real a comparar. Não gerar em lote o restante dos personagens/objetos enquanto a peça anterior estiver indefinida. Ajustes decorrentes da avaliação voltam à mesma microentrega com versão nova.

Registrar cada revisão em `Evidence/25d/<TAREFA>/reviews/<ID>.md`: versão revisada, arquivos/hashes, pergunta, resposta humana real e referência à mensagem, decisão e pendências. Usar `proposta`, `em_revisao`, `ajustar` ou `aprovada`; ausência de resposta não é aprovação. Aprovação só vale para o escopo explicitamente visto. O coordenador não deve registrar aceite agregado antes de conferir todas as revisões exigidas. O CLI sozinho não impõe esse detalhamento.

## Sequência operacional pequena

| Microentrega | Entrega concreta | Avaliação e limite |
| --- | --- | --- |
| T00.1 — agora | Inventário/hashes da base, mapa do que preservar e este contrato | Engenharia: conferir fontes e pendências. Não muda gameplay. Não fecha toda T00. |
| T00.2 | Reproduzir menu → fase e registrar percurso inicial da base, logs e captura em movimento | Distinguir falha real de entrada remota; corrigir apenas defeito confirmado em passo separado. Documentar build e cobertura. Migrou para T00B: obrigatório antes de T02; não bloqueia conceito estático T01. |
| T01.1 | **Uma pose lateral neutra de Yuna**, corpo inteiro, acabamento ilustrado coerente com o menu | Humana: “é a mesma menina?”, proporções, rosto/cabelo/vestido. Sem produzir ciclos ainda. |
| T01.2 | Uma pose de Nanda, em tamanho legível e na relação de escala com Yuna aprovada | Humana: identidade, expressão, asas e relação de companhia. Sem substituir a avaliação por brilho/partículas. |
| T01.3 | Um estudo do urso com a linha de apoio explicada em imagem técnica separada | Humana: assinatura, material e silhueta; engenharia: apoio real. Arte do jogo não recebe linha artificial de piso. |
| T01.4 | Livros inclinados, uma composição | Humana: mesma família visual e leitura do topo; engenharia: inclinação/apoios. |
| T01.5 | Móvel íntegro e detalhe da ruptura localizada | Humana: madeira/idade/fratura; engenharia: regiões e cedimento. Não finalizar toda a animação aqui. |
| T01.6 | Um quadro do trecho com as peças conceituais aprovadas e fundo noturno | Humana: unidade, atmosfera e leitura em tamanho real. Fecha bíblia/T01 somente com revisões anteriores. É composição de direção, não build final. |
| T02.1 | Uma personagem provisória em cena técnica isolada, sobre o root físico atual | Engenharia: pivô/escala/sorting/oclusão. Preservar main scene e física. |
| T02.2 | Mesmo trecho com duas opções de câmera, mudando somente a câmera | Humana: leitura/contexto/conforto. Fixar opção; sem misturar nova física ou nova animação na comparação. |
| T02.3 | Prova curta do método de animação e orçamento medido | Humana: deformação/pintura coerente; engenharia: custo/importação/contratos. Fecha T02. |
| T03.1 | Yuna em repouso/respiração, poucos segundos em loop | Humana: vida/expressão sem deformação excessiva. |
| T03.2 | Somente corrida, loop e deslocamento sobre chão de teste | Humana: fluidez/personalidade; engenharia: pés/velocidade/sons. |
| T03.3 | Um salto completo com subida, queda e pouso | Humana: arco/corpo/impacto; engenharia: contato e controle previsível. |
| T03.4 | Tropeço e retomada | Humana: legibilidade e delicadeza; engenharia: pausa local/retry. Fecha T03 após revisão do conjunto. |
| T04.1 | Nanda em voo, asas e orientação | Humana: companhia e vida; engenharia: ritmo de atualização. |
| T04.2 | Nanda acompanhando Yuna com luz e trilha discretas | Humana: foco/antecipação; engenharia: guia existente. Fecha T04. |
| T05.1–3 | Integrar um objeto de cada vez: urso, livros, móvel | Revisão visual e teste de contato após cada objeto; nunca integração em lote sem inspeção. |
| T05.4 | Camadas de fundo e iluminação do trecho | Humana: acolhimento noturno, profundidade e unidade. Fecha T05. |
| T06.1 | Uma travessia urso → livros | Humana: jogo em movimento, câmera e leitura; engenharia: salto/apoio. |
| T06.2 | Travessia do móvel com cedimento/tropeço e saída | Humana: ritmo e continuidade; engenharia: regiões/reset. |
| T06.3 | Falhar e tentar novamente no trecho | Humana: acolhimento/ritmo de retorno; engenharia: estado/UI/input. |
| T07.1 | Piloto contínuo em build, teclado e toque, com limitações identificadas | Aprovação humana agregada antes da expansão. Sem ocultar diferenças entre teste desktop e aparelho real. |

T08–T10 permanecem no plano principal; antes de expandir, subdividir cada nova plataforma/ação narrativa com o mesmo princípio de uma mudança avaliável por vez. Nenhuma microentrega futura está pronta ou aprovada na criação deste documento.

**Detalhamento de T02.2 em 12/11/2026, sessão `2026-11-12-codex-camera-25d-b91f`:** T02.1 foi aprovada pelo usuário. A comparação revelou forte variação de escala herdada (64→162 px) e diferenças na leitura de apoios. Dividir a revisão de câmera em T02.2a (escolha humana da projeção, A perspectiva versus B ortográfica, com escala equivalente no plano de corrida) e T02.2b (estabilizar zoom/antecipação na projeção escolhida e revisar conforto). Mesma física e mesma pose nas duas variantes. A escolha de projeção não aceita toda a câmera; T02.3 aguarda essa revisão. Evidências e limitações: `Evidence/25d/T02.2/README.md`. A escolha posterior de A está registrada em sua revisão de aprovação; T02.2b permanece em revisão.

## Orquestração

**Estado após a revisão:** usuário escolheu A (perspectiva suave 45°) em T02.2a; T02.2b de escala estável foi implementada e aguarda avaliação humana. A animação corporal ainda não existe. Use [ESTADO-ATUAL-25D.md](ESTADO-ATUAL-25D.md) e o estado estruturado para a reserva atual; as sessões iniciais mencionadas acima são histórico. O arquivamento da direção anterior não altera a ordem das microentregas.

O coordenador e a reserva atuais constam em `PLANO-25D-ESTADO.json`; não assumir como ativa uma sessão antiga citada neste documento. A execução segue sequencial enquanto decisões do piloto estiverem indefinidas. Futuramente pode haver agentes por escopo T03/T04/T05 conforme autorização e o plano, sem mudar arquivos compartilhados e sem ultrapassar revisões pendentes.

Ao encerrar cada passo: diário com resultado, checkpoint com próxima microentrega, arquivos e comandos de retomada, evidências e processos. Ao entregar para avaliação humana, deixar o trabalho concreto revisável e manter os próximos passos dependentes pendentes; não repetir pedido de autorização para trabalho técnico já autorizado.
