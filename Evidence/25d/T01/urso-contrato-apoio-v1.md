# Urso — contrato de apoio antes da arte

T01.3, sessão `2026-11-12-codex-bear-25d-71ac`, 12/11/2026. **Proposta de autoria, não validação física de uma imagem nova.**

## Onde Yuna pousa

O alvo principal é a coroa arredondada da cabeça, na região central entre as orelhas. A altura dos pés acompanha a curva local; não cortar a cabeça num tampo plano. Não desenhar plataforma retangular, prancha, laje ou linha de piso sobre a pelúcia.

As orelhas têm volumes e alturas próprios. Suas faces superiores podem constituir regiões de apoio separadas; não ligar seus topos por uma linha horizontal invisível. Bordas da banqueta só contam como apoio onde a superfície física estiver realmente exposta no plano de corrida. O focinho, olhos, remendo, laço frontal e costuras são detalhes visuais, não justificam contatos artificiais na projeção da câmera.

## Forma visual a preservar

Cabeça frontal de urso de pelúcia castanha, larga e arredondada, com orelhas redondas, olhos de botão escuros costurados em X, focinho creme, nariz escuro, sorriso costurado, emendas/remendo e laço carmim. Assenta numa banqueta baixa cilíndrica de madeira mais larga que a cabeça. Não transformar em urso sentado com braços/pernas nem em caixa uniforme.

## Evidência anterior versus nova autoria

- Lidos AGENTS e entrada 305 do diário. Regras de forma real e apoio prevalecem sobre recomendações antigas de achatamento no README de plataforma00.
- Fonte atual `DarkRoomPlatformArt.GetSupportShapes`: cinco formas Mesh, cabeça, duas orelhas, base e aro. `PlatformSupportProfile` extrai faces expostas da geometria real, sem ponte por largura de volume.
- Evidência histórica `Evidence/supports-camera-2026-11-11/support-geometry.json`: índice0 com5regiões/86segmentos, minX≈0,47,maxX≈3,53, minY≈−8,795,maxY≈−7,842,âncora principal(2,−8,0). São dados históricos da cena3D, não coordenadas certificadas da futura arte2D.
- Renders locais do urso e comparação conceitual anterior foram inspecionados. O estudo novo adotará contorno arredondado, independentemente do topo achatado visto em materiais antigos.

Após aprovação artística, integrar em cena técnica com pivô/escala definidos, confrontar contorno aparente e superfície física em câmera final e ajustar a geometria real caso necessário. Só afirmar coincidência após medição/teste. Não deformar a arte para caber nos colliders anteriores.

O diagrama separado `urso-apoios-esquema-v1.svg` explica regiões e intenções sem ser uma sobreposição medida da imagem. Não é extração da malha nem substitui raycasts/testes da integração. A arte exibida ao jogador não deve conter essas marcações.
