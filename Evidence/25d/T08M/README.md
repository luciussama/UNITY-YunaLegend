# T08M — Fase Jogável 2.5D Ilustrada (`00 → 10`) com Paridade ao Jogo Legado

## Escopo Entregue
- **Cena Unity:** `Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-00-10.unity`
- **Build Windows64:** `Build/Windows/YunaLegend-Jogavel-00-10/YunaLegend.exe`
- **Scripts novos:**
  - `Assets/Art/Illustrated25D/Environment/Stage2Runtime/TrainTrestle10Platform.cs` (Plataforma `10 — Pista Elevada do Trem`, usando `Assets/Art/Illustrated25D/Environment/Stage2/10-train-trestle-v1.png`, `LandingPixelY = 522`, `FeetPixelY = 807`, `TopY = -3.65 m`, `Stage2OpeningGap = 2.115 m` após o Castelo `09`)
  - `Assets/Art/Illustrated25D/Environment/Stage2Runtime/LegacyPlayableStageFlow.cs` (Fluxo completo alinhado ao legado)
  - `Assets/Art/Illustrated25D/Environment/Stage2Runtime/PreviewPlayable0010Verification.cs` (Verificação automatizada 30/60/120 Hz)
  - `Assets/Art/Illustrated25D/Environment/Stage2Runtime/Editor/Playable0010BatchRunner.cs`
- **Integração:** `Assets/Art/Illustrated25D/Preview/IllustratedPreview.cs` e `Assets/Art/Illustrated25D/Preview/Editor/IllustratedPreviewBuild.cs` (`Route.PlayableStage2P10`).

## Paridade com o Jogo Legado (`https://luciussama.github.io/legendGirl/`)
1. **Abertura (4 passos):**
   - Fala 1 (`NANDA`): *"Levanta, dorminhoca. Temos que brincar. Você dormiu demais."*
   - Narração poética (caixa creme superior com borda dupla rosa): *"Cada coisa guardava seu nome. Entre elas, nada dizia por onde ir."*
   - Fala 2 (`NANDA`, aproximando-se de Yuna): *"SENÃO A GENTE NÃO SAI DAQUI NUNQUINHA!"*
   - Cue pré-pulo: `[ESPAÇO] PRESSIONE ESPAÇO` + seta amarela `↓` sobre o Urso (`01`), iniciando a corrida automática ao pressionar Espaço/clique.
2. **Gameplay Fase 1 (`00 → 09`):**
   - Nanda azul (`AGENTS.md` § 3) guiando à frente; quebra do móvel (`03`) com pausa local e tropeço/recuperação preservados; botões superiores direitos (`R`, `||`, `SOM`).
3. **Cutscene do Castelo `09` → Fase 2 (`A Fuga`) → Plataforma `10`:**
   - Ao pousar no Castelinho (`09`), Yuna para, a câmera abre visão da rota revelando a Pista Elevada do Trem (`10`), exibe a narração poética + falas de Nanda e Yuna (`DarkRoomDialogueScript`), e transiciona para **FASE 2 — A FUGA** (`4.0 → 5.4 m/s`, `jumpHeight = 2.55 m`) até pousar e concluir sobre a Plataforma `10`.
   - Tela de derrota alinhada ao legado (`✦ QUARTO ESCURO ✦` / *"Você não conseguiu sair do quarto."* com *Tentar de novo* no checkpoint e *Voltar ao início*).

## Evidências de Verificação (30 / 60 / 120 Hz)
- `Evidence/25d/T08M/verify-30hz/playable-00-10-30hz.json`: `success: true`, `errors: 0`, `maxLegacy: 0`, `openingStepsOk: true`, `castleCutsceneOk: true`, `stage2Started: true`, `trainTrestle10Landed: true`, `stage2Completed: true`.
- `Evidence/25d/T08M/verify-60hz/playable-00-10-60hz.json`: `success: true`, `errors: 0`, `maxLegacy: 0`, `openingStepsOk: true`, `castleCutsceneOk: true`, `stage2Started: true`, `trainTrestle10Landed: true`, `stage2Completed: true`.
- `Evidence/25d/T08M/verify-120hz/playable-00-10-120hz.json`: `success: true`, `errors: 0`, `maxLegacy: 0`, `openingStepsOk: true`, `castleCutsceneOk: true`, `stage2Started: true`, `trainTrestle10Landed: true`, `stage2Completed: true`.
