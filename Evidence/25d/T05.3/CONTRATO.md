# T05.3 candidata v1 — apoio definido antes das camadas

Sessão `2026-11-13-copilot-t053-b29a5ec7`. Reserva 752/753. Proposta, revisão humana pendente.

## Geometria primeiro

Fixture exclusiva de largura8,4 × profundidade2,4 × altura2,3 m. O apoio é somente o tampo horizontal, y=1,15, espessura0,16, z=±1,2. Duas malhas não convexas têm bordas complementares seguindo uma polilinha de fadiga irregular em XZ; não uma caixa única. Em repouso as bordas encostam (sem vão aberto e sem ponte). Depois do estralo, cada metade segue exatamente a transformação do contato legado `Contacto_Fisico/Tampo_*`; abertura adicional total5cm e queda7,5cm de referência legadas. A borda varia com Z; fibras são decoração abaixo do apoio, nunca colliders.

A candidata usa a fonte aprovada T01.5 (`furniture-v1.png`, SHA2562926cc4b2d9aa0abf1d8330f36cb265d541b7902f0a6f253bd49840899a70797). O gabinete principal ocupa a faixa superior da prancha; o detalhe inferior é referência, não outro móvel colocado em cena. A estrutura frontal é recortada em escala uniforme; o tampo é uma superfície 3D com UVs do topo pintado (mapeamento separado para adaptar a perspectiva da prancha). Não esticar a pintura íntegra inteira para representar ruptura.

## Integração isolada

Componente novo instala visual e colliders apenas no marcador2 da fixture carregada em memória; conserva `YieldingWoodFurniture` e os nomes de contato consumidos por ela. Não altera o controlador, `PlatformSupportProfile`, scripts legados ou Gameplay. O perfil vem de `FromColliders` existente. Apresentação sincroniza em LateUpdate com as transformações físicas aplicadas pelo evento legado. Não implementa outro relógio de pausa, evento ou retry.

## Limites obrigatórios

Preparação inicial não executou Unity. Após liberação serial do integrador foram executadas compilação e inspeção EditMode; PlayMode/build não foram executados. Preview estático não valida carga, estralo, pausa LOCAL80ms, tropeço280ms ou retry em gameplay. Harness batch EditMode captura poses com `Preview` e reset sem alegar travessia. T06.2/3 e provas30/60/120Hz reais permanecem pendentes. Após captura serial do integrador, comparar topo visível/apoio na câmera final e testar revisão humana: madeira/identidade, fratura localizada e leitura de queda. Sem aceite e sem T05.4.
