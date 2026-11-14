# T08E — proposta de revisão individual

14/11/2026. Sessão `2026-11-13-dsh-abc04-cena`. Registro 16 em execução
(aguardando revisão). Não é aceite T06/T07.

## Análise crítica do contato

O pé **não** estava encostado de fato. Medido no runtime, a sola **desenhada** ficava
2,8 cm abaixo do topo físico na Idle e 4,9 cm na corrida — a arte tem o pé abaixo do
pivô, então o apoio correto não bastava e a perna entrava na letra.

Correção: a sola desenhada é presa ao topo do colisor pelo renderer da pose ativa
(`pinVisibleSole`). Medição nas três taxas: **gap 0,0000 m** na Idle e no pouso. Root,
cápsula, física e animações continuam intactos.

## O que olhar

1. **Contato**: o pé toca a superfície e a perna fica fora da letra? As imagens de
   [pouso](final-60/attempt-0-abc-land.png) e [repouso](final-60/attempt-1-abc-idle.png)
   e o [vídeo](final-video-60/camera-ascent.mp4) mostram o resultado.
2. **Tamanho**: a peça agora está 10% menor que a escala base (4,43 m de largura,
   3,81 m de altura). Ainda precisa menor? Cada −5% custa ~0,15 m de degrau.
3. **Dificuldade**: o degrau caiu para 0,76 m e o topo tem 1,51 m. Ainda parece o
   salto mais difícil do trecho?
4. **Pertencimento**: a pilha parece um brinquedo do quarto e continua a leitura da
   cômoda?

## O que mudou

- Apenas o objeto 04 (pilha ABC) e sua integração; cena/build novas e separadas.
- Apoio medido na pintura aprovada antes de instalar; nada de caixa padrão.
- Ordem de desenho da Idle (−1) e sola desenhada presa à superfície.
- Pouso na borda do tampo plano (y=150) e escala uniforme 10% menor.
- A cômoda e os três objetos anteriores não foram redesenhados.

## O que ainda não está provado

- A base da pilha no piso não aparece no enquadramento aprovado quando Yuna está no alto.
- Contato do urso e demais solas continuam com as pendências de T06.
- Aparelho, WebGL, áudio, desempenho e aceite global T06/T07 seguem em aberto.

Resposta esperada: aprovar só esta microentrega, pedir outro tamanho/ajuste de apoio
ou rejeitar. Sem aceite agregado por esta proposta.
