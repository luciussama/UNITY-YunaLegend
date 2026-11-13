# T05 — ambiente e plataformas do piloto: consolidação agregada

Data: 13/11/2026. Sessão da consolidação: `2026-11-13-dsh-pilot-7b3e`
(DeepSeek). As quatro microentregas foram produzidas por sessões Codex/Copilot
anteriores; esta sessão apenas conferiu as revisões e registrou o aceite.

Estado: **T05 aceita em 13/11/2026**, com as quatro microentregas aprovadas
individualmente pelo usuário. Este aceite cobre somente o ambiente estático e os
contratos de apoio. Não cobre integração em movimento (T06), piloto em movimento
(T07), aparelhos ou WebGL.

## Microentregas e aprovações

| Microentrega | Escopo aprovado | Resposta humana literal | Registro |
| --- | --- | --- | --- |
| T05.1 | Urso: coroa curva como apoio, contrato físico e câmera final | `aprovado.` | [README](../T05.1/README.md) · [proposta](../T05.1/reviews/T05.1-urso-proposta.md) |
| T05.2 | Livros: capa superior inclinada, contrato físico e câmera final | `aprovado.` | [README](../T05.2/README.md) · [proposta](../T05.2/reviews/T05.2-livros-proposta.md) |
| T05.3 | Móvel: silhueta, ruptura localizada, apoio real, cedimento estático e reset | `aprovado` | [README](../T05.3/README.md) · [revisão](../T05.3/reviews/T05.3-revisao.md) |
| T05.4 | Contato da pilha de livros | `agora sim! :) Sabia que você ia conseguir.` | [contact-v2](../T05.4/contact-v2/README.md) |
| T05.4 | Altura e posição da janela | `legal, aprovado.` | [window-v4](../T05.4/window-v4/README.md) |
| T05.4 | Penumbra estática: luar, luz azul de Nanda e fosforescência tênue | `Aprovado.` | [penumbra-v2](../T05.4/penumbra-v2/README.md) · [aprovação](../T05.4/penumbra-v2/reviews/penumbra-v2-aprovacao.md) |

O aceite agregado da T05 é técnico e se apoia nessas revisões individuais, como
exige [25D-ASSINATURA-E-MICROENTREGAS.md](../../../docs/ia/25D-ASSINATURA-E-MICROENTREGAS.md).
A instrução para consolidar veio do usuário nesta conversa (13/11/2026), depois
das aprovações acima.

## Contratos de apoio consolidados

| Objeto | Apoio real | Profundidade | Física | Vão seguinte |
| --- | --- | --- | --- | --- |
| Urso (`index0`) | coroa entre orelhas, 360 amostras, 7 regiões | `1,20 m` | 1 `MeshCollider` não convexo, 2872 vértices | `2,490840 m` (alcance validado `2,491969 m`) |
| Livros (`index1`) | capa superior `1,184211 m`, inclinação `-8,739244°`, offset `+0,134786 m` | `1,15 m` | 1 `MeshCollider` não convexo, 8 vértices | `1,250000 m` |
| Móvel (`index2`) | dois tampos com fronteira irregular complementar; `sag` `0,075 m` | cena real `2,4 m` | `MeshCollider` lê os novos tampos; caixas legadas desabilitadas, não removidas | — |

- Erro máximo entre perfil e `MeshCollider` no urso: `0,000001 m`; raycast central `0`.
- Erro de apoio dos livros no harness: `0`; zero apoios fora da profundidade.
- Erro de piso na penumbra: `0`; delta do apoio dos livros `0`.
- Nenhuma arte aprovada de T01 foi alterada ou regerada; cada README traz SHA-256
  das fontes e `manifest.json`/`hashes.json` com as evidências.

## Integração na cena

- Marcadores `index0`, `index1` e `index2` em `Assets/Scenes/DarkRoom_Gameplay.unity`.
- Componentes `IllustratedBearPlatform`, `IllustratedBookStackPlatform` e
  `FurnitureIllustrationCandidate`; `DarkRoomPlatformGeometry` e
  `DarkRoomPlatformArt` usam o perfil ilustrado quando disponível, com fallback legado.
- `Visual_3D` legado preservado (renomeado e inativo), conforme AGENTS.md.
- Yuna e Nanda nas capturas são provas estáticas da arte oficial; a cena
  persistida **ainda contém o corpo 3D** de Yuna. A integração permanente do
  visual ilustrado (idle/run/jump/stumble) é a T06.

## Validação executada (somente estática)

| Prova | Resultado |
| --- | --- |
| EditMode T05.1 | `51/51` |
| EditMode T05.2 e T05.4 (penumbra) | `52/52` |
| Harness de câmera final (urso, livros) | exit `0`, apoio `0`/`0,000001 m`, sem apoio fora da profundidade |
| Fixture EditMode do móvel | `T053_EDITMODE_INSPECTION_PASS`, 1 região íntegra e 2 abertas, reset restaurado |
| Penumbra | 7 checagens de luz aprovadas; regressão sem flag idêntica pixel a pixel à janela-v4 |
| PlayMode | `32/32` registrados em sessão anterior; não reexecutados na penumbra |

## Limites deste aceite

- Não valida travessia, salto, pouso, sensação de controle, câmera final
  acompanhando, morte/retry nem entrada em aparelho real.
- A quebra do móvel **não** foi validada em runtime: pausa local de `80 ms`,
  estralo, tropeço e reset real seguem para T06.2/T06.3.
- Iluminação ilustrada projetada, **sem sombras dinâmicas de oclusão**.
- Plataformas fora do piloto foram ocultadas apenas nas fixtures de captura.
- T06 continua dependente de aceite humano do piloto em movimento (T07).

## Reprodução

Cada microentrega documenta seus comandos exatos e seu harness:
[T05.1](../T05.1/README.md), [T05.2](../T05.2/README.md),
[T05.3](../T05.3/README.md) e [T05.4](../T05.4/README.md). Os setups salvam a
cena; os harnesses de revisão não salvam.
