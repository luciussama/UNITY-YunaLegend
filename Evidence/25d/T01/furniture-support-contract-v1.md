# Móvel — contrato de apoio e ruptura antes da arte

T01.5, sessão `2026-11-12-codex-furniture-25d-f502`, 12/11/2026. Proposta conceitual, não validação física de arte nova.

## Apoio e forma

Yuna atravessa o tampo superior comprido do móvel, de esquerda para direita. Apoio pertence à madeira visível; não às gavetas, prateleiras, molduras frontais nem a uma caixa envolvendo tudo. Preservar móvel baixo e largo: dois grupos laterais com três gavetas cada, puxadores de argola de latão envelhecido, prateleiras abertas centrais, rodapé/pés baixos e tampo com bordas suavemente trabalhadas. Sem espelho ou adereços extras que impeçam a travessia.

A implementação atual tem duas regiões de tampo, que recebem a mesma pose no visual e nos contatos físicos. A fenda atual de4cm e as duas caixas simples são autoria provisória, não justificativa para desenhar uma costura reta perfeita. A arte nova deve apresentar madeira envelhecida, com desgaste discreto, veios e uma área enfraquecida perto do centro. Em repouso o móvel permanece íntegro e utilizável, com sinais sutis de fadiga; no detalhe após esforço, mostrar ruptura irregular localizada, fibras e pequenas lascas seguindo os veios.

Evitar fenda reta uniforme de uma extremidade a outra, aparência de duas peças apenas encostadas, corte de serra ou destruição exagerada. O detalhe de ruptura deve corresponder à área enfraquecida do mesmo tampo, não a outro tipo de móvel. Nenhuma luz/linha artificial marca apoio.

## Mecânica e integração futuras

Dados da base: móvel8,4×2,3×2,4m, dois tampos; um estralo, pausa local80ms, tropeço280ms e cedimento até7,5cm; retry restaura estado. Fontes consultadas: `YieldingWoodFurniture`, `DarkRoomYieldingFurnitureArt` e `DarkRoomPlatformArt.GetSupportShapes`; diário305 e349.

Preservar o comportamento até eventual alteração justificada. Futuramente, a geometria da fratura pode exigir perfis/malhas de contato mais precisos do que duas caixas; visual e contatos devem continuar na mesma transformação, sem ponte física em espaço aberto e sem superfície sustentando área apenas desenhada como buraco. Não deformar a arte aprovada para manter o colisor provisório. Validar pés, regiões, pose, fenda, câmera e retry antes de alegar integração concluída.

O estudo desta microentrega mostra aparência íntegra e um detalhe conceitual após fadiga. Não é animação nem demonstração da pausa/cedimento em execução. Medidas exatas, pivôs, cortes em camadas e estados finais precisam da prova técnica posterior.
