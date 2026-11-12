# T00 — base antes da apresentação ilustrada

Sessão `2026-11-12-codex-start-25d-4f6a`, Codex (GPT-6), 12/11/2026. **T00.1 executada; T00 ainda ativa.**

## Estado observado

- Branch `codex/dark-room-2d`, baseline `29c91233daab178421d7f5ba631e968c0daa5477`.
- Unity 6000.3.28f1; URP 17.0.3; Input System 1.13.1, conforme arquivos locais. Sem atualização de pacotes nesta etapa.
- Cenas no build: `TitleScreen` primeiro, `DarkRoom_Gameplay` depois. GUIDs e hashes no manifest.
- No início desta sessão havia somente diário modificado e `WebGL/` antiga não rastreada. Nenhum asset de jogo foi alterado neste passo.
- `Assets/Art/UI/Title_Background.png` e a prancha aprovada de Yuna foram inspecionados visualmente. O menu é a âncora do acabamento ilustrado; a prancha complementa identidade e tem diferenças que devem ser explicitadas na revisão T01.1.

## Sistemas a preservar inicialmente

| Sistema | Fonte atual | Tratamento |
| --- | --- | --- |
| Root físico/entrada/salto | `Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs` | Manter Physics3D e separar apresentação; avaliar assistência depois, com comparação isolada |
| Apoios reais | `PlatformSupportProfile.cs`, `DarkRoomPlatformArt.cs`, marcadores/catálogo do mesmo diretório | Preservar formatos, inclinações e regiões desconexas; adaptar representação visual ao apoio coerente |
| Móvel que cede | `YieldingWoodFurniture.cs`, `DarkRoomYieldingFurnitureArt.cs` | Manter transformação compartilhada, pausa e retry; trocar acabamento e ruptura sob revisão |
| Progressão/abertura | `DarkRoomProgressionManager.cs`, `DarkRoomOpeningSequence.cs` | Preservar narrativa/estados; trocar encenação gradualmente |
| Guia de Nanda | `NandaFairyGuide.cs` | Preservar eventos/guia; esfera não é aparência final |
| Apresentação de Yuna | `YunaCharacterView.cs`, prefab/mesh do estudo | Preservar referência técnica; substituir visual final por ilustração após aprovação |
| Câmera | `Assets/Scripts/Runtime/Camera/SideScroll25DCamera.cs` | Baseline preservada; comparar opções em cena técnica, sem impor resultado |
| Menu/build | `Assets/Scripts/Runtime/UI/TitleScreenController.cs`, `TitlePhaseSelector.cs`, `Assets/Scripts/Editor/WebCiBuild.cs` | Preservar arte/fluxo; entrada exige reprodução confiável antes de diagnóstico |

Os caminhos abreviados da tabela são relativos a `Assets/Scripts/Runtime/DarkRoom/`, exceto onde há caminho explícito. O inventário de arquivos/hashes no manifest é a referência exata.

## Evidência histórica disponível, não reexecutada aqui

- Build anterior: `Build/Publish/webgl-isolated.log` contém `Build Finished, Result: Success`, `YUNA_WEB_PAGES_OK` e exit 0. Fonte foi compilada em cópia isolada; não é uma build 2D nem publicação confirmada.
- Observação da sessão artística anterior: menu renderizou no IAB, mas entrada enviada pela automação não avançou. Causa não confirmada. Não declarar defeito do jogo por essa observação isolada.
- Testes históricos de câmera: 47 EditMode na evidência de 11/11; estudo de Yuna: 45 EditMode/27 PlayMode no respectivo README. Não são resultados de uma nova execução desta sessão.
- Capturas do estudo e `Evidence/yuna-model-study-2026-11-11/runtime/traversal.gif` são referências históricas de apresentação/movimento. A captura runtime usa entradas controladas e não equivale a uma sessão manual.

## Próximo passo obrigatório: T00.2

1. Verificar existência/identidade de editor ou servidor antes de iniciar outro; não encerrar processo do usuário. Se necessário, usar a cópia isolada de build já existente.
2. Reproduzir menu → fase com entrada confiável e documentar ambiente. Observar o resultado real antes de afirmar bug; não modificar fluxo para contornar uma limitação de automação sem prova.
3. Registrar abertura, salto inicial, urso/livros e falha/retry da versão base, com configuração e vídeo/capturas. Se houver impedimento, registrar exatamente a cobertura faltante e continuar T00 ativa.
4. Confirmar se a build existente corresponde aos fontes a avaliar; caso contrário, recompilar e vincular evidências ao novo hash.
5. Entregar T00 com evidências completas e revisão técnica. Só então reservar T01 e produzir **uma pose lateral neutra de Yuna** conforme o contrato de microentregas.

Não foi jogado novamente o legado nesta microentrega; vale a observação documentada no diagnóstico. Não houve geração de arte, conversão de cena, mudança de física/câmera, build nova ou deploy.

## Revisão técnica de escopo nesta retomada

T00 passa a certificar somente inventário/baseline, não a validação jogável. Os critérios de entrada, captura e gravação acima permanecem pendentes na nova T00B, que bloqueia T02. T01 conceitual pode avançar independentemente. Comparados 33 arquivos Assets/Packages do inventário contra Build/Publish/Project: todos iguais após normalização CRLF/LF. O menu foi observado novamente em IAB nesta sessão: renderiza/anima, Enter focado e clique não avançam, sem warn/error no console. Nenhuma causa ou correção foi confirmada. Não houve gravação nova do percurso. A pendência não foi apagada nem aprovada.
