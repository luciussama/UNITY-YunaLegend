# Plataforma 00 — cabeça do urso (arte conceitual)

11/11/2026 · sessão `2026-11-11-dsh-urso-conceito-8d2c` · DeepSeek Harness (deepseek-flash).

**Esta entrega substitui o modelo anterior**, que havia divergido da arte conceitual aprovada. A referência obrigatória é a arte fornecida pelo usuário — urso marrom de pelúcia, olhando de frente, com base cilíndrica de madeira. Nenhum código legado foi consultado ou importado.

## O que a arte conceitual exigia

Pelo de pelúcia marrom quente; olhos de botão escuros com costura em X e brilho; focinho creme grande com borda tracejada; nariz escuro com brilho; sorriso costurado; remendo e emendas costuradas; orelhas redondas com interno creme e tracejado; laço vermelho grande; e uma peça cilíndrica de madeira (baia) sob a cabeça, visível nas laterais.

## O que foi implementado

- **Modelo refeito** (`DarkRoomPlatformArt`): cabeça em superelipsoide largo (2,48 × 0,78 × 1,65 m) apoiada numa **pe cilíndrica de madeira** (0,32 m de altura, elíptica para acompanhar a profundidade da cabeça). Total 1,10 m; a superfície de pouso continua no topo da cabeça e o colisor não mudou de formato.
- **Texturas procedurais novas**: `T_Bear_Fur` (pelo com trama e degradê), `T_Bear_Muzzle` (feltro creme), `T_Bear_Wood` (12 tábuas verticais), `T_Bear_Velvet` (veludo vermelho com dobras) e `T_Bear_Button` (aro, costura em X e brilho de verniz). Materiais `M_Bear_*` correspondentes.
- **Costuras em geometria**: emendas central e laterais, borda da coroa, contorno do focinho, interno das orelhas, sorriso e remendo. Todas acumuladas numa única malha (`Costuras`), sem centenas de GameObjects.
- **Olhos de botão** em disco com UV planar, mostrando o aro, o X e o brilho desenhados na textura.
- **Laço** construído com duas asas e nó em veludo, apoiado sobre o plinto.

### Correção da luz tênue das plataformas

As demais plataformas brilham por **emissão de contorno** (`T_Platform_ContourEmission`): o mapa é claro nas bordas da UV e quase apagado no interior. No modelo anterior a UV era esférica, então o contorno luminoso caía nas costas e nos polos do volume — ou seja, a luz ficava **dentro/atrás da peça** e não aparecia. Agora a UV da cabeça é **projetada da frente**, e o mapa de contorno volta a desenhar o brilho na silhueta visível. Pelo, focinho e madeira usam esse mesmo esquema; as peças pequenas (botão, linha, nariz, laço) usam emissão uniforme baixa.

## Evidências

- `comparison-concept.png`: arte conceitual sobre o render frontal do modelo.
- `02-bear-front.png` / `03-bear-three-quarter.png` / `04-bear-side.png`: renders de forma com **luz neutra temporária** (não é a iluminação final).
- `05-gameplay-approach.png`: câmera lateral a 16 m, FOV 45°, apenas luzes da cena — mostra o brilho tênue saindo do objeto como nas demais plataformas.
- `editmode.xml`, `playmode.xml`: suítes.

## Limitações conhecidas

- **Topo achatado por exigência de gameplay**; a arte conceitual é mais arredondada no alto. O colisor acompanha o topo do mesh.
- As **orelhas sobem ~0,10 m acima do plano de pouso** (como na arte). Nos cantos extremos, um pouso pode encostar visualmente na orelha.
- A arte é frontal; o modelo tem 1,65 m de profundidade — de três quartos ele lê como um volume mais comprido que o desenho.
- Costuras são quads finos (sem espessura); vistas de perfil quase desaparecem.
- Texturas são procedurais; não reproduzem a pincelada manual da arte.
- O restante da Etapa 1 (01–09) segue em blockout com as dimensões canônicas.

## Assinatura canônica (revisão final)

Ajustes pedidos pelo usuário sobre a versão anterior:

- **Banqueta de madeira mais larga que o urso**: 3,0 m de diâmetro contra 2,48 m da cabeça, com corpo e aro chanfrado. É uma peça cilíndrica de assento, com tábuas verticais.
- **Urso redondo**: excentricidade do superelipsoide de 0,30 → **0,50**, silhueta arredondada em vez do bloco anterior.
- **Superfície de pouso plana e circular**: o mesh agora tem `topFlatten = 0,12` — o topo é cortado num disco plano com o vértice máximo exatamente em `height/2`, casando com o colisor para os pés da criança.
- **Orelhas maiores e mais extraídas**: raio 0,40 m, avançando ~0,18 m acima do plano de pouso e além da silhueta.
- **Pelo levemente mais escuro**: paleta do albedo rebaixada (castanho mais profundo).

## Texturas — limite do procedural e o que foi feito

A primeira versão das texturas era uma única camada de ruído com gradiente: sem fio, sem relevo e sem sombreamento pintado. O upgrade atual:

- **Pelo 1024 px** com fBm de 4 oitavas + **11 000 fios desenhados** como linhas curtas e curvas, com afinamento e ponta clara, mais o gradiente pintado de luz (topo claro, base escura).
- **Normal maps** derivados do mesmo campo de altura por Sobel: pelo (força 3,2), madeira (4,0) e focinho (1,6). O `TextureImporter` marca essas PNGs como `NormalMap` e o material usa `_BumpMap`/`_BumpScale`.
- **Madeira 1024** com 12 tábuas, veio, bisel e escurecimento nas extremidades; **focinho 512** com grão de feltro e vinheta.

O que ainda dá para evoluir, em ordem de custo/benefício:

| Caminho | O que entrega | Custo |
| --- | --- | --- |
| Mais camadas procedurais (clumps de pelo, umidade, poeira, costura com sombra) | Aproxima da pincelada sem asset externo | Baixo |
| Roughness/altura por peça + vertex color de oclusão | Volume pintado (sombra sob o laço, orelhas, base) | Médio |
| Textura **pintada à mão** (Krita/Photoshop/Substance) por plataforma | reproduz de fato a arte conceitual | Requer autoria externa; a pipeline já aceita o PNG |
| Atlas por plataforma / UV única desenhada | acabamento de produção | Alto |

O limite honesto: geração procedural nunca reproduz pincelada manual. Para igualar a arte, o caminho é receber a textura pintada — o material e o importador já estão prontos para isso.

## Categorias da skill

1. **Preservar:** câmera, luz calibrada, pós-processamento, áudio, VFX, rota e físicas de salto; o esquema de emissão de contorno das plataformas.
2. **Ainda trabalhar:** 01–09; acabamento de UV/texel por peça; Etapas 2 e 3; portal e personagens.
3. **Prioridade:** livros 01 e cômoda 02 (família rígida), tambor 05 (topo circular), castelinho 09, então expansão.

## Validação

- **EditMode 23/23** e **PlayMode 21/21** sobre a cena gerada com o modelo novo.
- Refazer o modelo mudou a altura da plataforma 00 (1,27 → 1,10 m) e, com ela, o topo (−7,83 → −8,00 m); os testes recalculam topos a partir da cena e seguem aprovados.
- A cena foi gerada no projeto isolado `Build/CameraFramingValidation` porque o editor principal estava aberto; cena, materiais e texturas foram copiados de volta com `.meta`, e as 84 referências de GUID da cena foram conferidas (2 são scripts de pacote URP).
