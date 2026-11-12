# Diagnóstico artístico — Dark Room, 12/11/2026

Sessão: `2026-11-12-codex-art-diagnosis-a17c`. Agente: Codex (GPT-6).

> Decisão posterior do usuário em 12/11/2026: seguir a segunda proposta, personagens 2D ilustrados em cenário com profundidade. O [plano obrigatório](PLANO-25D-OBRIGATORIO.md) rege a execução; a alternativa 3D discutida abaixo permanece apenas como parte do diagnóstico anterior.

## Alcance e evidência

O legado foi acessado exclusivamente em https://luciussama.github.io/legendGirl/. Foram observados despertar, diálogos, apresentação do capítulo, tutorial, salto inicial, chegada ao urso, falhas e retry. Não foi completada a fase. Nenhum código ou asset legado foi lido, copiado ou importado. A interação remota deixa intervalos entre entradas: não permite concluir sobre latência fina, taxa de quadros ou superioridade física do legado.

A build Unity anterior terminou com `Build Finished, Result: Success`, `YUNA_WEB_PAGES_OK` e exit 0. Foi servida localmente; o menu renderizou, mas não avançou com os cliques/Enter enviados pela automação. Não se determinou se a causa está na build ou na entrada automatizada. A tentativa de captura do editor retornou outra janela e foi abandonada sem interação. Portanto, não houve sessão manual comparativa completa de Unity nesta avaliação.

A análise da fase Unity usa as capturas reais versionadas `Evidence/yuna-model-study-2026-11-11/09-gameplay-bear.png`, `10-gameplay-stumble.png`, `05-face.png`, a documentação e os registros da captura controlada em Play Mode em `runtime/`. As imagens 09/10 são composições estáticas na câmera do jogo; o material runtime executa física e apresentação reais com entradas de salto injetadas. Foram lidos `YunaCharacterView`, `Yuna25DPlayerController` e `SideScroll25DCamera`. Nenhum asset ou comportamento foi modificado.

## Diagnóstico principal

Minha interpretação do feedback é uma perda de unidade artística e de continuidade corporal, somada à comparação de uma apresentação ilustrada mais acabada com um estudo 3D ainda provisório. A evidência não sustenta concluir que os revisores rejeitam qualquer possibilidade de 3D. Também não autoriza descartar a preferência explícita deles por 2D.

O legado vende uma fantasia de livro infantil: personagem ilustrada, objetos pintados, volumes arredondados, detalhes domésticos e iluminação focal da fada. O quarto é bastante escuro; logo, o acolhimento não depende simplesmente de clarear a imagem. Ele aparece na combinação entre rosto, tecido, madeira, proporções, luz quente e presença de uma companheira reconhecível.

A Unity preserva elementos do cenário, mas ainda não entrega esse mesmo conjunto. Yuna tem rosto sem expressão animada, olhos muito circulares, cabelo organizado em volumes rígidos, tecido com pouca riqueza pictórica e uma silhueta de perfil menos próxima da ilustração. Nanda aparece como uma esfera luminosa nas evidências. A documentação identifica esses elementos como provisórios. Esse estado técnico é legítimo, mas não constitui uma entrega artística equivalente ao alvo aprovado.

## O que provavelmente provoca o desconforto

| Aspecto | Evidência observada | Interpretação artística |
| --- | --- | --- |
| Continuidade da protagonista | Menu ilustrado com rosto delicado; fase com modelo de estudo e outra leitura de rosto/cabelo | A troca pode ser percebida como perda de identidade e de qualidade ao entrar no jogo |
| Companhia de Nanda | Fada ilustrada reconhecível no legado; esfera nas capturas Unity | Perde-se uma relação afetiva visível; sobra principalmente a função de marcador |
| Unidade dos objetos | Urso, livros e móvel Unity apresentam diferentes níveis de acabamento; blockout ainda aparece | O conjunto parece montado de peças em estágios distintos, mesmo quando cada peça funciona |
| Luz e materiais | Superfícies Unity têm áreas de brilho e grandes massas escuras, com menos detalhe doméstico legível nos quadros comparados | Madeira, pele e tecido se distinguem menos pela linguagem pintada; a sala pode parecer mais vazia e rígida |
| Enquadramento | Legado observado oferece mais contexto da sequência; câmera Unity aproxima personagem e objetos e altera distância conforme o próximo apoio | O close expõe o acabamento provisório e pode aumentar a percepção de velocidade/escala variável |
| Movimento corporal | Poses procedurais de estudo; transições sem mistura temporal explícita; contato horizontal dos pés ainda não calibrado | Mudanças de pose e possível deslizamento podem fazer o corpo parecer transportado pelo controlador |

“Baixo orçamento” é, aqui, uma leitura estética plausível de acabamento desigual, materiais genéricos e animação provisória. Contagem alta de polígonos não resolve essa percepção. Mais bloom, mais objetos ou texturas mais detalhadas isoladamente também não asseguram unidade.

## Movimento: fatos e hipóteses

`YunaCharacterView.Evaluate` restaura transformações e aplica uma pose conforme o estado atual. Há movimento secundário e IK de pernas, mas não há mistura temporal explícita entre as poses de corrida, salto, queda e pouso. A documentação reconhece que a passada horizontal e o deslizamento dos pés ainda precisam de calibração. A sola estar na altura correta não prova que o pé fique plantado no mundo.

O controlador pode alterar a velocidade horizontal ao decolar para favorecer o próximo apoio e voltar à velocidade de corrida no chão. Essa assistência pode ser útil. A hipótese a testar é se a mudança se torna perceptível como arrancada/frenagem involuntária. Não foi medido que ela cause o feedback dos revisores.

A câmera ajusta largura e altura do quadro conforme os apoios, com suavização. Essa adaptação resolve enquadramento de formas reais, mas precisa ser julgada também pelo ritmo percebido: distância de câmera e velocidade na tela podem mudar juntas. Não recomendo remover o contrato de apoio nem uniformizar plataformas; qualquer revisão deve continuar respeitando suas superfícies e silhuetas reais.

No legado, a personagem pequena e a apresentação em um plano lateral simplificam a leitura de pose, trajetória e destino. Isso pode contribuir para a sensação de suavidade. Não se mediram as curvas de movimento do legado; não atribuir valores físicos ou mecanismos internos a ele.

## Decisão recomendada

Suspender a expansão visual da fase até aprovar um pequeno trecho representativo: urso, livros e móvel, com alguns saltos, corrida, pouso e retry. O critério de aprovação deve ser a experiência vista em movimento, na câmera final, e não apenas um render de personagem.

Comparar duas propostas com os mesmos acontecimentos e condições de apresentação:

1. Uma direção 2D/2,5D de livro ilustrado, implementada nativamente na Unity com arte nova e movimento autoral. Unity não exige apresentação 3D.
2. Uma direção 3D pictórica demonstrada por um trecho realmente acabado: Yuna e Nanda reconhecíveis, materiais coerentes, luz suave e animações com contato convincente.

O legado não deve ser a fonte de código/assets dessas propostas. Ele é referência autorizada de observação.

Minha preferência de produção, diante do feedback recebido, é priorizar a proposta 2D/2,5D ilustrada para a próxima avaliação. Ela tem maior proximidade com o valor já reconhecido pelos revisores. Se houver desejo de manter personagens 3D, isso precisa ser validado nesse trecho antes de investir no restante.

Para diagnosticar movimento sem misturar causas, comparar primeiro uma câmera estável com o controlador atual; depois variar assistência horizontal e animação separadamente. Avaliar leitura do destino, previsibilidade de salto, contato dos pés, transições e sensação de companhia. Pedir aos revisores exemplos concretos de momentos agradáveis/desagradáveis, além da preferência geral, sem induzir a escolha de dimensão.

Não foi feito teste perceptivo com os revisores nesta sessão. As interpretações acima são hipóteses de direção de arte fundamentadas nas observações descritas, não conclusões psicológicas sobre pessoas específicas.
