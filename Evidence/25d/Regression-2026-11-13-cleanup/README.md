# Regressão do estado atual — 13/11/2026

Sessão `2026-11-13-codex-commit-clean-regression-c61a`, Codex GPT-6.
Checkpoint anterior à tentativa de limpeza: commit `301e0a3`.
Unity 6000.3.28f1, Windows, projeto principal; nenhum Editor concorrente.

## Resultados

- EditMode: 52/52 aprovados, zero falhas ou testes ignorados (`editmode.xml`).
- PlayMode: 32/32 aprovados, zero falhas ou testes ignorados (`playmode.xml`). Inclui câmera, input, apoios, retorno, Nanda, animação e móvel nas fixtures existentes.
- Percurso pelo InputSystem: Enter no menu, Space até urso e livros, queda e Enter para retry. Cinco resultados verdadeiros, 74 amostras (`keyboard-entry/input-and-traversal.json`). Não chama callbacks de início/retry diretamente.
- Quatro renders reais em `window-v4/`, 1280×720, idênticos pixel a pixel às quatro imagens aprovadas em `../T05.4/window-v4/` (Pillow ImageChops/ImageStat). Auditoria JSON idêntica; erro do piso e deslocamento do apoio dos livros iguais a zero. Imagem 04 inspecionada visualmente.
- `source-hashes.json` registra os arquivos Assets/Packages/ProjectSettings efetivamente usados; `summary.json` resume os resultados.

## Limpeza e limite

A exclusão de `Library`, `Build/Publish/Project/Library`, `Build/VisualAuditTools` e `Build/SkillValidationDeps` foi rejeitada pela política automática do ambiente antes da execução (`blocked by policy`). Nenhum desses caches foi apagado e não houve tentativa de contornar o bloqueio. Portanto, esta execução verifica o estado atual, sem comprovar uma reimportação após exclusão dos caches.

Correção documental aplicada em `docs/ia/WEB-PREVIEW.md`: XML e template HTML históricos apontam aos destinos preservados em Build/Archive. O caminho `Build/web-pages.log` em WEB-DEPLOY é saída de um comando de build, não evidência histórica faltante, e foi mantido.

Correção ao relatório anterior (diário 845): VisualAuditTools contém numpy/Pillow (1.575 arquivos, 67,76 MiB), SkillValidationDeps contém PyYAML (43 arquivos, 0,47 MiB); não estavam vazios neste token. Library principal/clone tinham 3.438,63/3.091,61 MiB.

## Reprodução

Com o Editor fechado, usar o executável Unity instalado, `-batchmode -projectPath <repo> -logFile <log>` e uma das opções abaixo, em sequência:

```
-runTests -testPlatform EditMode -testResults <editmode.xml>
-runTests -testPlatform PlayMode -testResults <playmode.xml>
-executeMethod YunaLegend.EditorScripts.IllustratedBaselineCapture.Run -baselineOutput <output>/keyboard-entry -baselineFrames <repo>/Build/25d/Regression-2026-11-13-cleanup/frames
-executeMethod YunaLegend.Illustrated25D.Environment.Editor.RoomT054Capture.Run -t054HighWindow -t054Output <output>/window-v4
```

Os harnesses/test runner encerram o Editor; não usar `-quit` para interromper as provas assíncronas. Logs locais `.log` não versionados acompanham esta execução. Quadros brutos do percurso ficam em Build/25d/Regression-2026-11-13-cleanup/frames.

Não foram salvas cenas nem implementada a penumbra. T05 segue com sua reserva anterior. O percurso executa a apresentação base salva; a captura ilustrada é estática. Isso não conclui T06, não aprova o piloto ilustrado em movimento e não é cobertura WebGL/manual/dispositivo real. A direção registrada para a próxima etapa é luar, Nanda azul e fosforescência muito tênue das plataformas.
