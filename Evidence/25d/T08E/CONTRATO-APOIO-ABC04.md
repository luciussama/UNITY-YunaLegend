# ABC04 — contrato de apoio antes da integração

14/11/2026. Sessão `2026-11-13-dsh-abc04-cena`, registro 16, tarefa T08E.
Este contrato foi escrito **depois** da arte aprovada (T08D) e **antes** de instalar o
objeto na cena. A pintura não foi alterada; os números abaixo vêm dela.

## Medição da pintura aprovada

`Assets/Art/Illustrated25D/Environment/ABC/abc-stack-v1.png`, 1278×1230 RGBA,
SHA256 `75af893ca38895c3d3a0984685783064eca5e747ef68bce36b829a42390aa968`.

| Referência | Pixel | Uso |
| --- | --- | --- |
| Linha de pouso (9 cm acima da borda do tampo) | y = 125 | pé acima do friso |
| Base dos blocos inferiores | y = 1177 | assenta no piso |
| Face frontal do bloco A | x = 417..825 | largura do apoio |
| Centro do conteúdo | x = 636,5 | pivô horizontal da pintura |

- A largura do apoio é a **face frontal** do bloco A (408 px, 34,2% do conteúdo).
  Não inclui a face lateral desenhada nem a perspectiva do tampo.
- O catálogo antigo (84×50×40 px) e o contato default de largura inteira **não**
  foram usados.

## Escala e apoio físico

- Escala uniforme **10% menor** que a base da cômoda: 269,6690 px/m (base 245,1537).
- Piso −9,10 m → linha de pouso −5,199 m ⇒ **3,901 m** de altura.
- Largura do objeto 4,4276 m; apoio próprio **1,5130 m × 1,59 m de profundidade**,
  0,0575 m à esquerda do centro do conteúdo (a face lateral desenhada fica à direita).
- Um único `BoxCollider` no topo do bloco A (`Topo_Real_Bloco_A`), com o mesmo
  `PlatformSupportProfile` e a física 3D existentes. Nada de caixa uniforme.
- O objeto assenta no piso e ocupa o marcador 4 já existente na cena (x = 23,1271 m).

## Contato: sola desenhada presa à superfície

Foi medido, não estimado: a sola **desenhada** da Idle ficava 2,8 cm abaixo do topo
físico e a da corrida 4,9 cm, porque a arte tem o pé abaixo do pivô. Isso fazia a
perna entrar na letra mesmo com o apoio correto.

Correção: a instância do ABC usa `pinVisibleSole`, que fixa o limite inferior do
renderer da pose ativa no topo do colisor. Medição nas três taxas:

| Pose | `visual min` − topo | resultado |
| --- | --- | --- |
| Idle (repouso final) | −0,0000 m | pé na superfície |
| Pouso (Jump ativo) | −0,0000 m | pé na superfície |

O root, a cápsula, a física e as animações não foram alterados; só a posição visual.

## Ajustes pedidos na revisão

1. "Parando dentro dos blocos": a vinheta de Idle (MeshRenderer, ordem 0) desenhava
   atrás da pintura (ordem 20). A pintura passou para a **ordem −1**.
2. "Perna dentro da letra": o pouso estava no topo do quadro frontal (y=185). Voltou
   para a **borda do tampo plano** (y=150) e a peça foi reduzida.
3. "Ainda precisa ser menor": escala uniforme reduzida para **10% abaixo da base**
   (a peça ficou 4,43 m de largura e 3,90 m de altura).
4. "Pé dentro do tampo": a linha de pouso subiu de y=150 para **y=125**, deixando
   a sola ~9 cm acima do lip e todo o friso abaixo da linha do pé. Aprovado em
   preview 120 Hz e depois validado nas três taxas.

## Rota e dificuldade

- Vão de apoio até a cômoda: **2,1510 m**.
- Degrau vertical: **0,851 m** (a cômoda está em −6,05 m; o apoio em −5,199 m).
- Folga visual entre as duas pinturas: 0,556 m. As bordas dos sprites incluem
  transparência; uma leitura por bounds de sprite pode parecer sobreposição.
- A altura de salto configurada é 2,4 m; o controlador mira 12% antes do centro do
  apoio. O degrau caiu de 1,18 m para 0,76 m por causa das reduções pedidas; o topo
  estreito (1,51 m) continua sendo a dificuldade desta peça.

## Limite declarado

A pilha tem 3,90 m e a câmera aprovada cobre 7,5 m de altura. Quando Yuna está no
alto, a base no piso fica fora do quadro. O apoio e o piso estão corretos; o
enquadramento aprovado é que não mostra a base nesse momento.

Se a peça precisar ficar ainda menor, o degrau cai na mesma proporção (cada −5% custa
~0,15 m de degrau). Nada aqui certifica aceite global T06/T07, nem os demais defeitos
de sola.
