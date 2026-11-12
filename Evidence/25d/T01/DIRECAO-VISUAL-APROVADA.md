# Direção visual aprovada — piloto Dark Room

Marco T01, aprovado em etapas pelo usuário nesta conversa em 12/11/2026. Este documento consolida a bíblia visual curta e o conjunto revisável. Não equivale a entrega jogável.

| Referência | Arquivo | Revisão humana |
| --- | --- | --- |
| Yuna, identidade e acabamento | `yuna-profile-v1.png` | `reviews/T01.1-v1-aprovacao.md` |
| Nanda azul e brilho azul representado | `nanda-blue-v1.png` | `reviews/T01.2-v1-aprovacao.md` |
| Urso arredondado, pelúcia/banqueta | `bear-v1.png` | `reviews/T01.3-v1-aprovacao.md` |
| Livros antigos inclinados | `books-v1.png` | `reviews/T01.4-v1-aprovacao.md` |
| Móvel, madeira envelhecida/fratura | `furniture-v1.png` | `reviews/T01.5-v1-aprovacao.md` |
| Unidade/atmosfera noturna | `composition-v2.png` | `reviews/T01.6-v2-aprovacao.md` |

## Assinatura

Ilustração de livro infantil, pincelada tátil e bordas trabalhadas, volumes suaves e materiais reconhecíveis. Preservar a identidade do menu e da Yuna aprovada: cabelo castanho, pele natural, roupa creme/carmim, meias claras e sapatos de tira. Nanda tem cabelo mel, roupa/asas azuis e emissão azul; não voltar ao halo âmbar antigo. Urso castanho/creme com laço carmim, livros com páginas creme e encadernações antigas, móvel em madeira quente com latão discreto.

Quarto noturno com índigo/violeta, luar frio e luz azul localizada, mantendo calor nos materiais pintados e leitura de rosto/apoios. Acolhimento vem também de formas, expressões e detalhes domésticos; não converter o conjunto em render plástico, pixel art ou filtro sépia. O quadro composto é referência de atmosfera; não precisa ser convertido num único fundo estático e não é planta da rota.

Apoios seguem a forma real: coroa curva/orelhas/regiões expostas do urso, capa superior inclinada dos livros, regiões reais do tampo do móvel. Os contratos específicos acompanham as imagens. Nunca achatar/redimensionar silhuetas para caber num colisor padrão. A quebra e o tropeço/retry permanecem requisitos conforme `docs/ia/25D-MOVEL-QUEBRA-E-DESEQUILIBRIO.md`.

## Passagem à engenharia

T00B continua pendente: entrada menu→fase e registro confiável do percurso base. Depois, T02.1 prepara uma cena técnica isolada com somente Yuna ilustrada sobre o root físico atual, tratando alpha/bordas, pivô na sola, escala, materiais/sorting e contato. Em seguida, comparar câmera e método de animação em passos separados, sem mudar física junto da apresentação.

A primeira entrega técnica revisável deve mostrar Yuna real na Unity, parada sobre um apoio simples, com captura fiel e comparação da referência. Movimento/artefatos provisórios de prova devem ser identificados. Não chamar uma imagem estática movida pelo controlador de animação final. Sem integrar a fase inteira, redesenhar referências ou alegar que a composição já roda no motor.

## Rastreabilidade e limites

Imagens individuais, prompts e manifests preservam autoria/origem/hashes. Revisões humanas acima definem o escopo aprovado. Escala/câmera final, vãos, contato, recortes/alpha, camada de fratura, animações, luz azul real, desempenho e cobertura de dispositivos permanecem por validar. T01 aprovada não satisfaz T00B/T02/T07.
