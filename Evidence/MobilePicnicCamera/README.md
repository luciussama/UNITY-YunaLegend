# Jardim (Picnic): enquadramento mobile para planejamento de saltos

Implementação exclusiva para mobile (`Application.isMobilePlatform` ou override explícito para QA). O desktop mantém o enquadramento original (`Z = -13.5`, escala visual `1.15`, física, `TitleScreen` e ordem das cenas inalterados).

## Regras implementadas (`PicnicMobileCameraFraming` + `PicnicSceneryController`)

- **Paisagem (`844x390`):** afastamento mínimo moderado para `Z = -16.2` (+20% sobre `-13.5`), ampliando o campo à frente para planejar o próximo salto.
- **Retrato (`390x844`):** largura útil mínima de `4.8` unidades e limite de afastamento até `Z = -22.5` apenas o necessário para enquadrar Yuna e o próximo apoio relevante à frente.
- **Seleção de próximo apoio:** considera apoios à frente até `310px` (descarta apoios atrás, acima de `182px` ou abaixo de `240px`), com suavização XYZ (`5/s`) e âncora vertical no último pouso para evitar balanço excessivo durante o salto.

## Validação real

- **Unity 6000.3.28f1** em clone isolado (`Build/PicnicVisualAuditProject`): **25/25 testes EditMode do Picnic passaram** (`editmode-picnic-final.xml`, `failed=0`, `duration=0.588s`), incluindo os 10 novos testes unitários em `PicnicMobileCameraFramingTests`.
- **Integridade de fontes:** todos os 4 arquivos (`PicnicSceneryController.cs`, `PicnicMobileCameraFraming.cs`, `PicnicMobileCameraFramingTests.cs`, `PicnicMobileCameraReview.cs`) conferem SHA256 idêntico entre o checkout central e o clone (`sources.json`).
- **Capturas Editor (`PicnicMobileCameraReview.Capture`, retorno 0):** 6 vistas encenadas em `390x844` e `844x390` (`porch-edge`, `suspended`, `river`) com marcador `PICNIC_MOBILE_CAMERA_PLANNING_OK six staged views; player and next landing visible` (`capture-markers.txt`) e métricas em `picnic-mobile-camera-planning.csv`.
  - Retrato (`390x844`): `camera_z = -21.6339`, `world_width = 4.8000`, `player_x = 0.3625`, próximo apoio entre `0.6740..0.8888` da largura da tela.
  - Paisagem (`844x390`): `camera_z = -16.2000`, `world_width = 16.8336`, `player_x = 0.3625`, próximo apoio entre `0.4510..0.5125` da largura da tela.

## Limitações honestas

- As imagens são fixtures Editor em posições encenadas; não substituem percurso completo nem teste em aparelho físico mobile.
