# T05.2 - livros: contrato de apoio medido

Data: 13/11/2026. Sessao `2026-11-13-codex-t052-books-b7c4`, Codex (GPT-6.1).
Status: contrato aplicado na cena persistida; **T05.2 aguarda revisao humana** e
a T05 agregada continua pendente.

## Fonte e escala

- Fonte: `Evidence/25d/T01/books-v1.png`, prancha aprovada na T01.4.
- SHA-256: `cdbf8f5b1fa6f8cc3dd92372d142d26a72325005f4e84a97d49bd14fd83b0bec`.
- Textura: `1620 x 971`, RGBA8, limiar de alpha `32`.
- Alpha bbox inclusivo: `(83, 51)..(1444, 926)`.
- Escala fisica adotada: `1,11 m / 875 px = 0,001268571 m/px`.
- Largura visivel: `1,726526 m`; altura visivel: `1,110000 m`.

A copia em `Assets/Art/Illustrated25D/Environment/Books/Books_Platform_v1.png`
e byte-identica a fonte aprovada. Nenhuma nova pintura foi gerada.

## Capa superior medida

O apoio nao e uma caixa envolvendo a pilha. E a capa rigida do livro de cima,
com os cantos medidos na pintura:

- frente esquerda: `(274, 208)`;
- fundo esquerdo: `(532, 52)`;
- fundo direito: `(1428, 192)`;
- frente direita: `(1245, 355)`;
- pontos medios em profundidade: `(403, 130)` e `(1336.5, 273.5)`.

O topo mais alto do alpha pertence a lombada traseira, nao a superficie de
pouso. Por isso o AABB visual nao e usado como plano de apoio.

## Parametros fisicos

- Largura do apoio: `1,184211 m`.
- Inclinacao: `-8,739244 graus` (desce para a direita).
- Offset horizontal do contato em relacao ao centro do quadro: `+0,134786 m`.
- Altura do apoio acima do centro do quadro: `0,363763 m`.
- Profundidade fisica: `1,150000 m`.
- Espessura da laje de contato: `0,120000 m`.
- Plano visual e plano de contato: `z = 0`.

No mundo, o marcador 1 fica em `(5,935214, -7,913763, 0)` e o ponto de pouso
central medido fica em `(6,070000, -7,550000, 0)`.

## Representacao fisica

O apoio e um unico `MeshCollider` nao convexo, gerado da laje inclinada medida.
O perfil superior produz uma regiao continua com dois segmentos e nenhum apoio
fora da profundidade fisica. O visual ilustrado usa uma malha de quatro
vertices no mesmo plano `z=0`; o antigo `Visual_3D` permanece apenas como
`Visual_3D_Referencia_Desativada`.

## Rota

O contato autorado dos livros foi ajustado para `Stage1ContactX[1] = 4,070 m`.
Com o perfil real, o resultado medido e:

- vao urso -> livros: `2,451482 m`; alcance validado: `3,759794 m`;
- vao livros -> proxima plataforma: `1,250000 m`; alcance validado: `3,649846 m`;
- erro maximo entre perfil e collider: `0,000000 m`;
- amostras de raycast fora da profundidade fisica: `0`.

## Visual de revisao da Yuna

As capturas usam a pintura ilustrada oficial, nao o modelo 3D descartado:

- sprite: `Assets/Art/Illustrated25D/Yuna/Joint/Runtime/Yuna_Profile_Runtime_v1.png`;
- SHA-256: `0b9315b4dd525f23f2ef14db04682846b43b3ca7793f47670129afc88dfb9044`,
  identico a `Evidence/25d/T01/yuna-profile-v1.png`;
- material: `Idle_Painted_Runtime.mat`, shader `PaintedIdle`;
- orientacao: virada para a direita (`RunDirection=1`, `flipX=false`);
- `Body_Visual` 3D: inativo durante toda a captura;
- alinhamento: base do sprite em `y=-7,550000 m`, erro `0,000000 m`.

O `SpriteRenderer` de revisao e criado em memoria pelo harness e destruido ao
fim. A cena nao e salva. A cena persistida ainda contem o corpo 3D, que sera
substituido pela integracao permanente da T06.

## Limites

Este contrato prova o apoio estatico, a geometria persistida e o enquadramento
da camera final. Nao aprova movimento, pouso, sensacao de salto, entrada em
aparelhos, WebGL, o movel, a T05 agregada ou o piloto em movimento.
O sprite ilustrado e instalado somente para a revisao; ele nao comprova a
integracao permanente T03/T06 nem substitui a cena persistida.
