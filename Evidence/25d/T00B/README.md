# T00B — referência executável antes da ilustração

Sessão `2026-11-12-codex-unity-proof-25d-a73e`, Codex (GPT-6), 12/11/2026.

## Evidência real

`IllustratedBaselineCapture.Run` abriu a cena TitleScreen salva, entrou em Play Mode e enfileirou eventos `KeyboardState` no Input System. Não chamou o callback de Novo Jogo, LoadScene ou o callback de retry. Enter iniciou a fase; duas entradas Space chegaram ao urso e aos livros. A queda posterior foi deliberada; Enter reiniciou pelo fluxo normal. Os cinco resultados estão em `keyboard-entry/input-and-traversal.json`.

O primeiro ensaio encontrou foco inicial na bandeira de fases. `TitlePhaseSelector.Start` chamava Close e substituía a seleção de Novo Jogo. Agora a inicialização apenas oculta o dropdown; fechar uma interação continua devolvendo foco à bandeira. A regressão e os outros três testes do título passaram: `title-tests-final.xml`. Os dois XML anteriores preservam erros de preparação da fixture EditMode, corrigidos; não são testes verdes.

`baseline-camera.gif` contém 60 frames reais, a 10 fps, cobrindo seis segundos de tempo simulado. `keyboard-entry/00…07-*-camera.png` são renders da câmera Unity de 1280×720. Os frames brutos estão no diretório ignorado `Build/25d/BaselineFramesKeyboard`. A arte da fase ainda é a apresentação 3D anterior; Nanda âmbar nessa referência não substitui a decisão azul aprovada para a nova versão.

## Limites

- Automação no Editor Unity 6000.3.28f1; não é cobertura manual de navegador ou aparelho.
- Preservado `skipOpeningForQuickTest: 1` da cena salva. A entrada rápida está registrada; a abertura narrativa completa de 42 segundos não foi validada neste ensaio.
- O render da câmera não inclui o HUD em IMGUI. Morte e retry são comprovados por estados/log e sequência, não pela presença da tela de Game Over no PNG. As solicitações ScreenCapture em batch não produziram capturas nativas.
- O log contém exceção do indexador Editor.Search durante inicialização; não impediu compilação ou o percurso. Não foi classificada como defeito de gameplay.
- A compilação WebGL local verifica a produção do artefato a partir dos fontes relacionados no manifesto; não comprova execução dessa build em navegador. Não houve publicação. O diretório antigo `WebGL/` é anterior à correção de foco e não deve ser usado como entrega atual.

## Reprodução

Executar em cópia isolada do projeto, sem uma segunda Unity aberta sobre a mesma cópia:

```
Unity.exe -batchmode -projectPath <copia> -executeMethod YunaLegend.EditorScripts.IllustratedBaselineCapture.Run -baselineOutput <evidencias> -baselineFrames <frames> -logFile <log>
Unity.exe -batchmode -projectPath <copia> -runTests -testPlatform EditMode -testFilter YunaLegend.Tests.TitleScreenTests -testResults <xml> -logFile <log>
```

O harness encerra o Editor ao terminar. Para WebGL, definir `YUNA_WEB_OUTPUT` e executar `YunaLegend.EditorScripts.WebCiBuild.Build` com `-batchmode -quit`. Os hashes e a correspondência com a cópia compilada acompanham `source-build-manifest.json`.

Aceite técnico T00B abrange a base rápida observada e seus limites explícitos. Abertura narrativa, dispositivos e publicação permanecem nas etapas posteriores do plano; nenhuma imagem conceitual passa a ser gameplay por este registro.

Correspondência: 234/236 arquivos Assets/Packages são idênticos byte a byte ao clone. `DarkRoomPipeline.asset` difere somente nas quebras de linha; `DefaultVolumeProfile.asset` foi reserializado pela Unity com cinco flags active habilitadas em efeitos de valor zero (intensity 0 ou modo None). O manifesto conserva os dois hashes e discrimina essas exceções. Scripts, cenas e arte usados na base coincidem. Não alegar igualdade binária integral.
