# Livros — contrato de apoio antes da arte

T01.4, sessão `2026-11-12-codex-books-25d-c804`, 12/11/2026. **Proposta de autoria, não validação física da arte nova.**

Yuna pousa na capa rígida visível do livro superior, inclinada suavemente para baixo à direita e deslocada para a direita em relação ao centro da pilha. Os pés devem seguir essa capa, não uma caixa envolvendo todos os livros ou uma faixa horizontal invisível.

A pilha mantém cinco volumes de espessuras próximas, com pequenas diferenças de largura, deslocamento e inclinação. Nada de blocos idênticos fundidos ou livros flutuantes. Capas, lombadas, páginas e bordas visíveis devem pertencer aos volumes reais. Pequenos degraus de capas inferiores só serão contatos se estiverem fisicamente expostos no plano de corrida e forem tratados pelo contrato de apoio; não ampliar automaticamente o apoio principal à largura total da pilha.

Base de referência atual consultada: `DarkRoomPlatformGeometry.GetContact(1)` declara offsetX0,24m, tilt−4°, largura1,85m e profundidade1,15m. `DarkRoomPlatformArt.BuildBookStack` usa cinco volumes com deslocamentos graduais para a direita e inclinações de−0,8° a−4°. Esses valores documentam a autoria3D vigente; não são coordenadas certificadas da nova ilustração.

Paleta/material de referência: encadernação antiga em couro/tecido castanho, carmim, verde escuro e ocre discreto; páginas creme envelhecidas e ornamentos dourados pintados, sem brilho mágico. Textura e desgaste suaves de livro infantil acolhedor, coerentes com o urso aprovado. Um marcador de fita pode cair pela lateral, sem constituir apoio.

Após aprovação da arte, definir pivô/escala/câmera no protótipo e medir onde a capa cruza o plano de corrida. Ajustar contato/geometria ao formato aprovado com testes, sem nivelar ou deformar o objeto para caber no colisor anterior. Nenhuma coincidência arte/collider está comprovada nesta microentrega.
