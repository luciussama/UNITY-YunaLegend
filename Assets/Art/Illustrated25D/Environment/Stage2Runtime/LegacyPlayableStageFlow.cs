using UnityEngine;
using UnityEngine.InputSystem;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.Dialogue;
using YunaLegend.Illustrated25D.Preview;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Fluxo jogavel completo (00..10) alinhado as cenas, falas, passos e jogabilidade
    /// do jogo legado: abertura com falas de Nanda + narracao poetica, cue [ESPACO],
    /// corrida 00..09 com quebra do movel, cutscene do Castelo 09 e transicao para
    /// Fase 2 (A Fuga) ate a Pista Elevada do Trem (10).
    /// </summary>
    public sealed class LegacyPlayableStageFlow : MonoBehaviour
    {
        public enum FlowPhase
        {
            OpeningNandaWake1,
            OpeningPoeticIntro,
            OpeningNandaWake2,
            PreJumpCue,
            Stage1Gameplay,
            CastleCutsceneStep0,
            CastleCutsceneStep1,
            Stage2Gameplay,
            Stage2Completed
        }

        private const float Stage2InitialSpeed = 4.0f;
        private const float Stage2TargetSpeed = 5.4f;
        private const float Stage2JumpHeight = 2.55f;
        private const float Stage2AccelTime = 1.6f;

        private IllustratedPreview preview;
        private Yuna25DPlayerController player;
        private NandaFairyGuide guide;
        private SideScroll25DCamera cam;
        private Vector3 bearTopWorld;

        private FlowPhase phase = FlowPhase.OpeningNandaWake1;
        private float stepTimer;
        private float stage2BannerTimer;
        private float stage2RunTimer;
        private bool muted;
        private bool paused;
        private bool castleCutsceneTriggered;
        private bool stage2CheckpointReached;

        private Texture2D solidTex;

        public FlowPhase Phase => phase;
        public bool IsPaused => paused;
        public bool IsMuted => muted;
        public bool Stage2CheckpointReached => stage2CheckpointReached;
        public bool ShowIdlePose => phase != FlowPhase.Stage1Gameplay && phase != FlowPhase.Stage2Gameplay;

        public void Initialize(
            IllustratedPreview owner,
            Yuna25DPlayerController yunaPlayer,
            NandaFairyGuide nandaGuide,
            SideScroll25DCamera cameraDriver,
            Vector3 bearLandingWorld)
        {
            preview = owner;
            player = yunaPlayer;
            guide = nandaGuide;
            cam = cameraDriver;
            bearTopWorld = bearLandingWorld;

            solidTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            solidTex.SetPixel(0, 0, Color.white);
            solidTex.Apply();

            phase = FlowPhase.OpeningNandaWake1;
            stepTimer = 0f;
            paused = false;
            muted = false;
            AudioListener.volume = 1f;
            castleCutsceneTriggered = false;
            stage2CheckpointReached = false;

            if (player != null)
            {
                player.PrepareForOpening(DarkRoomStartLayout.Position);
                player.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
                player.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
                player.AutoRunActive = false;
            }
            PositionNandaForPhase();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            AudioListener.volume = 1f;
            if (solidTex != null) Destroy(solidTex);
        }

        public void AdvanceStepForAutomation()
        {
            AdvanceOpeningOrCutscene();
        }

        private void Update()
        {
            if (preview == null || !preview.Ready || player == null) return;

            if (CheckKey(Key.M)) ToggleMute();
            if (CheckKey(Key.R)) { preview.Retry(); return; }
            if ((CheckKey(Key.P) || CheckKey(Key.Escape)) &&
                (phase == FlowPhase.Stage1Gameplay || phase == FlowPhase.Stage2Gameplay) &&
                !player.IsDead)
            {
                TogglePause();
                return;
            }

            if (paused) return;

            if (player.IsDead)
            {
                if (CheckConfirmInput()) RetryFromCheckpoint();
                return;
            }

            stepTimer += Time.deltaTime;

            switch (phase)
            {
                case FlowPhase.OpeningNandaWake1:
                    if (stepTimer >= 4.4f || CheckConfirmInput())
                    {
                        phase = FlowPhase.OpeningPoeticIntro;
                        stepTimer = 0f;
                        PositionNandaForPhase();
                    }
                    break;

                case FlowPhase.OpeningPoeticIntro:
                    if (stepTimer >= 4.4f || CheckConfirmInput())
                    {
                        phase = FlowPhase.OpeningNandaWake2;
                        stepTimer = 0f;
                        PositionNandaForPhase();
                    }
                    break;

                case FlowPhase.OpeningNandaWake2:
                    if (stepTimer >= 3.4f || CheckConfirmInput())
                    {
                        phase = FlowPhase.PreJumpCue;
                        stepTimer = 0f;
                        PositionNandaForPhase();
                    }
                    break;

                case FlowPhase.PreJumpCue:
                    if (CheckJumpOrStartInput())
                    {
                        BeginStage1Run();
                    }
                    break;

                case FlowPhase.Stage1Gameplay:
                    if (!castleCutsceneTriggered &&
                        player.CurrentSupportIndex == 9 &&
                        player.IsGrounded &&
                        preview.Furniture != null &&
                        preview.Furniture.HasCracked &&
                        !player.IsStumbling &&
                        player.transform.position.x >= Castle09Platform.TargetX - 0.8f)
                    {
                        TriggerCastleCutscene();
                    }
                    break;

                case FlowPhase.CastleCutsceneStep0:
                    if (CheckConfirmInput())
                    {
                        phase = FlowPhase.CastleCutsceneStep1;
                        stepTimer = 0f;
                    }
                    break;

                case FlowPhase.CastleCutsceneStep1:
                    if (CheckConfirmInput())
                    {
                        StartStage2Escape();
                    }
                    break;

                case FlowPhase.Stage2Gameplay:
                    if (stage2BannerTimer > 0f) stage2BannerTimer -= Time.deltaTime;
                    stage2RunTimer += Time.deltaTime;
                    float t = Mathf.Clamp01(stage2RunTimer / Stage2AccelTime);
                    player.SetRunSpeed(Mathf.Lerp(Stage2InitialSpeed, Stage2TargetSpeed, t));

                    if (player.CurrentSupportIndex == 10 &&
                        player.IsGrounded &&
                        !player.IsStumbling &&
                        player.transform.position.x >= TrainTrestle10Platform.TargetX + TrainTrestle10Platform.SupportWidth * 0.35f)
                    {
                        phase = FlowPhase.Stage2Completed;
                        player.AutoRunActive = false;
                        if (cam != null) cam.AutoScrollActive = false;
                    }
                    break;
            }
        }

        private void AdvanceOpeningOrCutscene()
        {
            switch (phase)
            {
                case FlowPhase.OpeningNandaWake1:
                    phase = FlowPhase.OpeningPoeticIntro;
                    stepTimer = 0f;
                    PositionNandaForPhase();
                    break;
                case FlowPhase.OpeningPoeticIntro:
                    phase = FlowPhase.OpeningNandaWake2;
                    stepTimer = 0f;
                    PositionNandaForPhase();
                    break;
                case FlowPhase.OpeningNandaWake2:
                    phase = FlowPhase.PreJumpCue;
                    stepTimer = 0f;
                    PositionNandaForPhase();
                    break;
                case FlowPhase.PreJumpCue:
                    BeginStage1Run();
                    break;
                case FlowPhase.CastleCutsceneStep0:
                    phase = FlowPhase.CastleCutsceneStep1;
                    stepTimer = 0f;
                    break;
                case FlowPhase.CastleCutsceneStep1:
                    StartStage2Escape();
                    break;
            }
        }

        private void BeginStage1Run()
        {
            phase = FlowPhase.Stage1Gameplay;
            stepTimer = 0f;
            player.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
            player.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
            player.SetFloorDeathActive(true);
            player.AutoRunActive = true;
            player.AllowFloorLaunch(player.transform.position.x);
            if (guide != null) guide.ReturnToYuna();
        }

        private void TriggerCastleCutscene()
        {
            castleCutsceneTriggered = true;
            phase = FlowPhase.CastleCutsceneStep0;
            stepTimer = 0f;
            player.AutoRunActive = false;
            if (cam != null)
            {
                cam.AutoScrollActive = false;
                cam.BeginCastleRouteOverview();
            }
            if (guide != null) guide.SetTargetPlatform(10);
        }

        private void StartStage2Escape()
        {
            phase = FlowPhase.Stage2Gameplay;
            stage2CheckpointReached = true;
            stage2BannerTimer = 2.6f;
            stage2RunTimer = 0f;

            if (cam != null) cam.EndRouteOverview();
            player.SetRunSpeed(Stage2InitialSpeed);
            player.SetJumpHeight(Stage2JumpHeight);
            player.SetRunDirection(1f);
            player.SetFloorDeathActive(true);
            player.AutoRunActive = true;
            if (guide != null) guide.ReturnToYuna();
        }

        private void RetryFromCheckpoint()
        {
            if (!stage2CheckpointReached)
            {
                preview.Retry();
                return;
            }

            Vector3 castleSpawn = new Vector3(Castle09Platform.TargetX - 0.5f, Castle09Platform.TopY + 0.08f, 0f);
            player.ResetPlayer(castleSpawn, 1f);
            player.RestoreRouteSupport(9);
            player.SetRunSpeed(Stage2InitialSpeed);
            player.SetJumpHeight(Stage2JumpHeight);
            player.SetFloorDeathActive(true);
            player.AutoRunActive = true;
            stage2RunTimer = 0f;
            stage2BannerTimer = 1.8f;
            phase = FlowPhase.Stage2Gameplay;
            if (cam != null)
            {
                cam.EndRouteOverview();
                cam.SnapToTargetX(castleSpawn.x);
            }
            if (guide != null) guide.ReturnToYuna();
        }

        private void PositionNandaForPhase()
        {
            if (guide == null || player == null) return;
            switch (phase)
            {
                case FlowPhase.OpeningNandaWake1:
                case FlowPhase.OpeningPoeticIntro:
                    guide.transform.position = player.transform.position + new Vector3(0.95f, 1.45f, -0.15f);
                    break;
                case FlowPhase.OpeningNandaWake2:
                    guide.transform.position = player.transform.position + new Vector3(0.42f, 1.05f, -0.15f);
                    break;
                case FlowPhase.PreJumpCue:
                    guide.transform.position = player.transform.position + new Vector3(1.65f, 1.4f, -0.15f);
                    break;
            }
        }

        private void TogglePause()
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        private void ToggleMute()
        {
            muted = !muted;
            AudioListener.volume = muted ? 0f : 1f;
        }

        private static bool CheckKey(Key key)
        {
            return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
        }

        private static bool CheckConfirmInput()
        {
            if (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame ||
                 Keyboard.current.enterKey.wasPressedThisFrame ||
                 Keyboard.current.numpadEnterKey.wasPressedThisFrame))
                return true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                return true;
            return false;
        }

        private static bool CheckJumpOrStartInput()
        {
            if (CheckConfirmInput()) return true;
            if (Keyboard.current != null &&
                (Keyboard.current.upArrowKey.wasPressedThisFrame ||
                 Keyboard.current.wKey.wasPressedThisFrame))
                return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return true;
            return false;
        }

        private void OnGUI()
        {
            if (preview == null) return;

            DrawTopRightButtons();

            if (!string.IsNullOrEmpty(preview.Failure))
            {
                DrawCenteredCard("✦ ERRO NA CENA ✦", preview.Failure, null, null, null);
                return;
            }

            if (paused)
            {
                DrawCenteredCard(
                    "✦ PAUSADO ✦",
                    "Jogo pausado",
                    "Pressione P ou clique em Continuar para retomar.",
                    "Continuar",
                    TogglePause,
                    "Voltar ao início",
                    () => preview.Retry());
                return;
            }

            if (player != null && player.IsDead)
            {
                DrawCenteredCard(
                    "✦ QUARTO ESCURO ✦",
                    "Você não conseguiu sair do quarto.",
                    "A penumbra engoliu o caminho... Mas a luz da fadinha ainda chama por você.",
                    "Tentar de novo",
                    RetryFromCheckpoint,
                    "Voltar ao início",
                    () => preview.Retry());
                return;
            }

            switch (phase)
            {
                case FlowPhase.OpeningNandaWake1:
                    DrawLegacyBottomDialogue(
                        "NANDA",
                        "Levanta, dorminhoca.\nTemos que brincar.\nVocê dormiu demais.",
                        "Avança sozinho · Clique ou Enter adianta");
                    break;

                case FlowPhase.OpeningPoeticIntro:
                    DrawLegacyPoeticCard(
                        "Cada coisa guardava seu nome.\nEntre elas, nada dizia por onde ir.",
                        "Avança sozinho · Clique ou Enter adianta");
                    break;

                case FlowPhase.OpeningNandaWake2:
                    DrawLegacyBottomDialogue(
                        "NANDA",
                        "SENÃO A GENTE NÃO SAI DAQUI NUNQUINHA!",
                        "Avança sozinho · Clique ou Enter adianta");
                    break;

                case FlowPhase.PreJumpCue:
                    DrawPreJumpCue();
                    break;

                case FlowPhase.CastleCutsceneStep0:
                    DrawLegacyPoeticCard(
                        DarkRoomDialogueScript.PoeticCastle,
                        "Clique ou pressione Espaço / Enter para continuar");
                    DrawLegacyBottomDialogue(
                        "NANDA",
                        DarkRoomDialogueScript.CastleNanda.text,
                        "Clique ou pressione Espaço / Enter para continuar");
                    break;

                case FlowPhase.CastleCutsceneStep1:
                    DrawLegacyBottomDialogue(
                        "YUNA",
                        DarkRoomDialogueScript.CastleYuna.text,
                        "Clique ou pressione Espaço / Enter para iniciar A Fuga");
                    break;

                case FlowPhase.Stage2Gameplay:
                    if (stage2BannerTimer > 0f)
                    {
                        DrawStageBanner("FASE 2 — A FUGA");
                    }
                    break;

                case FlowPhase.Stage2Completed:
                    DrawCenteredCard(
                        "✦ A FUGA — PLATAFORMA 10 ✦",
                        "Você alcançou a Pista Elevada do Trem!",
                        "Fim do trecho com plataformas desenhadas em sequência (00 → 10). As plataformas 11–13 ainda estão em produção.",
                        "Jogar novamente",
                        () => preview.Retry());
                    break;
            }
        }

        private void DrawTopRightButtons()
        {
            float size = 38f;
            float gap = 10f;
            float top = 16f;
            float right = Screen.width - 18f;

            if (DrawRoundButton(new Rect(right - size, top, size, size), muted ? "OFF" : "SOM", false))
                ToggleMute();
            if (DrawRoundButton(new Rect(right - size * 2f - gap, top, size, size), paused ? ">" : "||", paused))
                TogglePause();
            if (DrawRoundButton(new Rect(right - size * 3f - gap * 2f, top, size, size), "R", false))
                preview.Retry();
        }

        private bool DrawRoundButton(Rect rect, string label, bool active)
        {
            Color prev = GUI.color;
            GUI.color = active ? new Color(0.95f, 0.76f, 0.25f, 0.95f) : new Color(0.08f, 0.08f, 0.14f, 0.85f);
            GUI.DrawTexture(rect, solidTex);
            GUI.color = new Color(0.85f, 0.76f, 0.52f, 0.75f);
            DrawBorder(rect, 1.5f);
            GUI.color = prev;

            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = label.Length > 2 ? 11 : 15,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = active ? Color.black : new Color(0.96f, 0.92f, 0.84f);
            return GUI.Button(rect, label, style);
        }

        private void DrawLegacyBottomDialogue(string speaker, string body, string footer)
        {
            float w = Mathf.Min(680f, Screen.width - 48f);
            float h = 158f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - h - 28f;

            Color prev = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.10f, 0.90f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidTex);
            GUI.color = new Color(0.28f, 0.30f, 0.42f, 0.55f);
            DrawBorder(new Rect(x, y, w, h), 1.5f);
            GUI.color = prev;

            var speakerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            speakerStyle.normal.textColor = new Color(0.75f, 0.78f, 0.86f);
            GUI.Label(new Rect(x + 20f, y + 14f, w - 40f, 20f), speaker, speakerStyle);

            var bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 21,
                wordWrap = true,
                fontStyle = phase == FlowPhase.OpeningNandaWake2 ? FontStyle.Bold : FontStyle.Normal
            };
            bodyStyle.normal.textColor = new Color(0.98f, 0.97f, 0.94f);
            GUI.Label(new Rect(x + 28f, y + 36f, w - 56f, 84f), body, bodyStyle);

            var footStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 11
            };
            footStyle.normal.textColor = new Color(0.68f, 0.70f, 0.78f);
            GUI.Label(new Rect(x + 20f, y + h - 28f, w - 40f, 20f), footer, footStyle);

            if (GUI.Button(new Rect(x, y, w, h), GUIContent.none, GUIStyle.none))
            {
                AdvanceOpeningOrCutscene();
            }
        }

        private void DrawLegacyPoeticCard(string text, string footer)
        {
            float w = Mathf.Min(640f, Screen.width - 64f);
            float h = 118f;
            float x = (Screen.width - w) * 0.5f;
            float y = 26f;

            Color prev = GUI.color;
            GUI.color = new Color(0.98f, 0.96f, 0.91f, 0.98f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidTex);
            GUI.color = new Color(0.88f, 0.72f, 0.75f, 0.95f);
            DrawBorder(new Rect(x, y, w, h), 2f);
            DrawBorder(new Rect(x + 4f, y + 4f, w - 8f, h - 8f), 1f);
            GUI.color = prev;

            var textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Italic,
                wordWrap = true
            };
            textStyle.normal.textColor = new Color(0.22f, 0.16f, 0.18f);
            GUI.Label(new Rect(x + 24f, y + 14f, w - 48f, 68f), text, textStyle);

            var footStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 11
            };
            footStyle.normal.textColor = new Color(0.45f, 0.36f, 0.38f);
            GUI.Label(new Rect(x + 20f, y + h - 26f, w - 40f, 18f), footer, footStyle);

            if (GUI.Button(new Rect(x, y, w, h), GUIContent.none, GUIStyle.none))
            {
                AdvanceOpeningOrCutscene();
            }
        }

        private void DrawPreJumpCue()
        {
            if (Camera.main == null || player == null) return;

            Vector3 yunaScreen = Camera.main.WorldToScreenPoint(player.transform.position + Vector3.up * 2.15f);
            if (yunaScreen.z > 0f)
            {
                float sx = Mathf.Clamp(yunaScreen.x - 125f, 20f, Screen.width - 270f);
                float sy = Mathf.Clamp(Screen.height - yunaScreen.y - 44f, 60f, Screen.height - 100f);

                Color prev = GUI.color;
                GUI.color = new Color(0.06f, 0.06f, 0.10f, 0.88f);
                GUI.DrawTexture(new Rect(sx, sy, 250f, 40f), solidTex);
                GUI.color = new Color(0.95f, 0.82f, 0.55f, 0.75f);
                DrawBorder(new Rect(sx, sy, 250f, 40f), 1.5f);
                GUI.color = prev;

                var cueStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
                cueStyle.normal.textColor = new Color(0.98f, 0.94f, 0.82f);
                GUI.Label(new Rect(sx, sy, 250f, 40f), "[ESPAÇO]  PRESSIONE ESPAÇO", cueStyle);
            }

            Vector3 bearScreen = Camera.main.WorldToScreenPoint(bearTopWorld + Vector3.up * 0.9f);
            if (bearScreen.z > 0f)
            {
                float bx = bearScreen.x;
                float by = Screen.height - bearScreen.y - 26f + Mathf.Sin(Time.time * 4.5f) * 5f;
                Color prev = GUI.color;
                GUI.color = new Color(0.98f, 0.88f, 0.45f, 0.98f);
                GUI.DrawTexture(new Rect(bx - 2f, by - 14f, 4f, 18f), solidTex);
                GUI.DrawTexture(new Rect(bx - 9f, by + 2f, 18f, 4f), solidTex);
                GUI.DrawTexture(new Rect(bx - 5f, by + 6f, 10f, 4f), solidTex);
                GUI.DrawTexture(new Rect(bx - 2f, by + 10f, 4f, 4f), solidTex);
                GUI.color = prev;
            }
        }

        private void DrawStageBanner(string title)
        {
            float w = 320f;
            float h = 44f;
            float x = (Screen.width - w) * 0.5f;
            float y = 24f;

            Color prev = GUI.color;
            GUI.color = new Color(0.06f, 0.05f, 0.12f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidTex);
            GUI.color = new Color(0.94f, 0.78f, 0.34f, 0.9f);
            DrawBorder(new Rect(x, y, w, h), 1.5f);
            GUI.color = prev;

            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = new Color(0.98f, 0.92f, 0.70f);
            GUI.Label(new Rect(x, y, w, h), title, style);
        }

        private void DrawCenteredCard(
            string header,
            string title,
            string subtitle,
            string primaryLabel,
            System.Action primaryAction,
            string secondaryLabel = null,
            System.Action secondaryAction = null)
        {
            float w = Mathf.Min(540f, Screen.width - 48f);
            float h = 228f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            Color prev = GUI.color;
            GUI.color = new Color(0.05f, 0.04f, 0.10f, 0.95f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidTex);
            GUI.color = new Color(0.76f, 0.48f, 0.24f, 0.85f);
            DrawBorder(new Rect(x, y, w, h), 1.5f);
            GUI.color = prev;

            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            headerStyle.normal.textColor = new Color(0.88f, 0.58f, 0.58f);
            GUI.Label(new Rect(x + 20f, y + 18f, w - 40f, 22f), header, headerStyle);

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            titleStyle.normal.textColor = new Color(0.98f, 0.94f, 0.68f);
            GUI.Label(new Rect(x + 24f, y + 44f, w - 48f, 62f), title, titleStyle);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Italic,
                    wordWrap = true
                };
                subStyle.normal.textColor = new Color(0.86f, 0.84f, 0.90f);
                GUI.Label(new Rect(x + 28f, y + 108f, w - 56f, 44f), subtitle, subStyle);
            }

            if (!string.IsNullOrEmpty(primaryLabel) && primaryAction != null)
            {
                bool hasTwo = !string.IsNullOrEmpty(secondaryLabel) && secondaryAction != null;
                float bw = hasTwo ? 180f : 220f;
                float bh = 42f;
                float by = y + h - 58f;
                float bx1 = hasTwo ? x + w * 0.5f - bw - 10f : x + (w - bw) * 0.5f;

                GUI.color = new Color(0.96f, 0.76f, 0.24f, 1f);
                GUI.DrawTexture(new Rect(bx1, by, bw, bh), solidTex);
                GUI.color = prev;
                var btn1Style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 15,
                    fontStyle = FontStyle.Bold
                };
                btn1Style.normal.textColor = new Color(0.10f, 0.08f, 0.05f);
                if (GUI.Button(new Rect(bx1, by, bw, bh), primaryLabel, btn1Style))
                    primaryAction.Invoke();

                if (hasTwo)
                {
                    float bx2 = x + w * 0.5f + 10f;
                    GUI.color = new Color(0.12f, 0.12f, 0.18f, 1f);
                    GUI.DrawTexture(new Rect(bx2, by, bw, bh), solidTex);
                    GUI.color = new Color(0.55f, 0.55f, 0.65f, 0.8f);
                    DrawBorder(new Rect(bx2, by, bw, bh), 1f);
                    GUI.color = prev;
                    var btn2Style = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 15,
                        fontStyle = FontStyle.Bold
                    };
                    btn2Style.normal.textColor = new Color(0.94f, 0.94f, 0.96f);
                    if (GUI.Button(new Rect(bx2, by, bw, bh), secondaryLabel, btn2Style))
                        secondaryAction.Invoke();
                }
            }
        }

        private void DrawBorder(Rect r, float t)
        {
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, t), solidTex);
            GUI.DrawTexture(new Rect(r.x, r.yMax - t, r.width, t), solidTex);
            GUI.DrawTexture(new Rect(r.x, r.y, t, r.height), solidTex);
            GUI.DrawTexture(new Rect(r.xMax - t, r.y, t, r.height), solidTex);
        }
    }
}
