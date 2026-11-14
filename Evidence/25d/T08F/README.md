# T08F — Candidata visual do Tamborito de Marcha (05)

14/11/2026. Sessão `2026-11-14-antigravity-tambor05`, registro 17 (`t08f/tambor05-visual-v1`).
Estado: **em revisão visual pelo usuário**, sem integração Unity ou alteração de build.

## Arte gerada
- **Arquivo:** `Assets/Art/Illustrated25D/Environment/Drum/drum-05-v1.png`
- **Formato:** `1024×1024` RGBA com fundo transparente (`480.739` pixels transparentes, `bbox = (135, 80, 886, 937)` — objeto inteiro sem cortes).
- **SHA256:** `e8784c325f36e58ac1c14254890c25df7e0c152d7b904c6ea2efd2d34e904778`
- **Estilo:** ilustração de livro infantil com pincelada tátil, coerente com `abc-stack-v1.png` e `dresser-v1.png` (madeira envelhecida mel, corpo vermelho profundo com pátina, pele creme plana no topo, cordões creme em cruz com passadores de couro e ganchos de latão).

## Contrato de apoio (definido antes da pintura)
- Detalhes em [CONTRATO-APOIO.md](./CONTRATO-APOIO.md).
- Os pés pousam exclusivamente na **pele plana superior**, acima do rebordo/aro frontal, sem caixa uniforme e preservando `PlatformSupportProfile` e a física 3D.

## Próximo passo após aprovação
1. Medir os pixels exatos da pele plana e do aro frontal em `drum-05-v1.png` (largura útil e linha de pouso).
2. Integrar o Tamborito de Marcha após os blocos ABC (`T08E`) em cena/build separada, validando salto, contato (`pinVisibleSole`), câmera aprovada e reset em 30/60/120 Hz.
