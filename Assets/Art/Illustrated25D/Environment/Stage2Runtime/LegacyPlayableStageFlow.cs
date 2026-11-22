using UnityEngine;
using UnityEngine.InputSystem;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Preview;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Adaptador Unity do fluxo jogável (00..10): conecta entrada, física de Yuna,
    /// câmera e guia Nanda à máquina de estados pura (StageFlowStateMachine) e ao
    /// presenter de interface (StageHudController).
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
            Stage2Completed,
            FalseDoorFalling,
            FalseDoorCutsceneStep0,
            FalseDoorCutsceneStep1,
            Stage3Gameplay,
            TruePortalVictory
        }

        private const float Stage2InitialSpeed = 4.0f;
        private const float Stage2TargetSpeed = 5.4f;
        private const float Stage2JumpHeight = 2.55f;
        private const float Stage2AccelTime = 1.6f;

        private readonly StageFlowStateMachine stateMachine = new StageFlowStateMachine();

        private IllustratedPreview preview;
        private Yuna25DPlayerController player;
        private NandaFairyGuide guide;
        private SideScroll25DCamera cam;
        private StageHudController hud;
        private OpeningCinematicController openingCinematic;
        private FalseDoorCinematicController falseDoorCinematic;
        private Vector3 bearTopWorld;

        private const float FalseDoorFallenDuration = 1.05f;
        private const float FalseDoorLookLeftEnd = 1.60f;
        private const float FalseDoorLookRightEnd = 2.15f;
        private const float FalseDoorDialogueDelay = 2.35f;

        private float stage2BannerTimer;
        private float stage2RunTimer;
        private bool muted;
        private bool paused;
        private Component falseDoorPlatformView;
        private Vector3 falseDoorOriginalLocalPos;
        private Quaternion falseDoorOriginalLocalRot;
        private bool falseDoorPoseSaved;
        private bool falseDoorCollapsing;
        private float falseDoorCollapseVelocityY;
        private float falseDoorCutsceneElapsed;
        private int falseDoorBeatStage;
        private float portalVictoryElapsed;
        private bool transitionedToToyRoom;

        public FlowPhase Phase => (FlowPhase)(int)stateMachine.Phase;
        public bool IsPaused => paused;
        public bool IsMuted => muted;
        public bool Stage2CheckpointReached => stateMachine.Stage2CheckpointReached;
        public bool ShowIdlePose => stateMachine.ShowIdlePose;
        public bool ShowFalseDoorDialogue =>
            stateMachine.Phase == StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0
            && falseDoorCutsceneElapsed >= FalseDoorDialogueDelay;

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

            paused = false;
            muted = false;
            AudioListener.volume = 1f;

            stateMachine.OnPhaseChanged += HandlePhaseChanged;
            stateMachine.Reset();

            hud = gameObject.AddComponent<StageHudController>();
            hud.GetPhase = () => stateMachine.Phase;
            hud.IsPaused = () => paused;
            hud.IsMuted = () => muted;
            hud.IsPlayerDead = () => player != null && player.IsDead;
            hud.IsFalseDoorDialogueReady = () => ShowFalseDoorDialogue;
            hud.GetFailure = () => preview != null ? preview.Failure : null;
            hud.GetStage2BannerTimer = () => stage2BannerTimer;
            hud.GetOpeningChapterBannerAlpha = () => openingCinematic != null ? openingCinematic.ChapterBannerAlpha : 0f;
            hud.GetOpeningCurtainAlpha = () => openingCinematic != null ? openingCinematic.CurtainFadeAlpha : 0f;
            hud.GetOpeningDialogueBeat = () => openingCinematic != null ? openingCinematic.DialogueBeat : 0;
            hud.IsOpeningYunaInBed = () => openingCinematic != null && openingCinematic.IsYunaInBed;
            hud.GetPlayerWorldPosition = () => player != null ? player.transform.position : Vector3.zero;
            hud.GetBearTopWorldPosition = () => bearTopWorld;
            hud.OnAdvanceRequested += AdvanceStepForAutomation;
            hud.OnTogglePauseRequested += TogglePause;
            hud.OnToggleMuteRequested += ToggleMute;
            hud.OnRetryFromBeginningRequested += () => preview.Retry();
            hud.OnRetryFromCheckpointRequested += RetryFromCheckpoint;

            openingCinematic = gameObject.AddComponent<OpeningCinematicController>();
            openingCinematic.Initialize(
                player,
                guide,
                cam,
                preview != null ? preview.Lighting : null);

            falseDoorCinematic = gameObject.AddComponent<FalseDoorCinematicController>();
            falseDoorCinematic.Initialize(
                player,
                cam,
                preview != null ? preview.Lighting : null);

            if (player != null)
            {
                player.OnCinematicFallLanded += HandleCinematicFallLanded;
                player.PrepareForOpening(DarkRoomStartLayout.Position);
                player.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
                player.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
                player.AutoRunActive = false;
            }
            PositionNandaForPhase(stateMachine.Phase);
        }

        private void OnDestroy()
        {
            stateMachine.OnPhaseChanged -= HandlePhaseChanged;
            if (player != null)
            {
                player.OnCinematicFallLanded -= HandleCinematicFallLanded;
            }
            Time.timeScale = 1f;
            AudioListener.volume = 1f;
        }

        private void HandleCinematicFallLanded()
        {
            stateMachine.NotifyCinematicFallLanded();
        }

        public void AdvanceStepForAutomation()
        {
            if (stateMachine.Phase == StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0
                && falseDoorCutsceneElapsed < FalseDoorDialogueDelay)
            {
                falseDoorCutsceneElapsed = FalseDoorDialogueDelay;
                UpdateFalseDoorCutsceneBeats();
            }
            stateMachine.AdvanceStep();
        }

        private void Update()
        {
            if (preview == null || !preview.Ready || player == null)
            {
                return;
            }

            if (CheckKey(Key.M)) ToggleMute();
            if (CheckKey(Key.R)) { preview.Retry(); return; }

            var currentPhase = stateMachine.Phase;
            if ((CheckKey(Key.P) || CheckKey(Key.Escape))
                && (currentPhase == StageFlowStateMachine.FlowPhase.Stage1Gameplay
                    || currentPhase == StageFlowStateMachine.FlowPhase.Stage2Gameplay
                    || currentPhase == StageFlowStateMachine.FlowPhase.Stage3Gameplay)
                && !player.IsDead)
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

            if (falseDoorCollapsing && falseDoorPlatformView != null)
            {
                falseDoorCollapseVelocityY -= 18f * Time.deltaTime;
                falseDoorPlatformView.transform.localPosition += Vector3.up * (falseDoorCollapseVelocityY * Time.deltaTime);
                falseDoorPlatformView.transform.Rotate(0f, 0f, -55f * Time.deltaTime);
                if (falseDoorPlatformView.transform.position.y < -11f)
                {
                    falseDoorCollapsing = false;
                    falseDoorPlatformView.gameObject.SetActive(false);
                }
            }

            if (currentPhase == StageFlowStateMachine.FlowPhase.TruePortalVictory)
            {
                if (!transitionedToToyRoom
                    && UnityEngine.Application.CanStreamedLevelBeLoaded("ToyRoom_Gameplay"))
                {
                    transitionedToToyRoom = true;
                    UnityEngine.SceneManagement.SceneManager.LoadScene("ToyRoom_Gameplay");
                }
                return;
            }

            if (currentPhase == StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0)
            {
                falseDoorCutsceneElapsed += Time.deltaTime;
            }

            bool allowConfirm = currentPhase != StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0
                || falseDoorCutsceneElapsed >= FalseDoorDialogueDelay;
            stateMachine.Tick(Time.deltaTime, allowConfirm && CheckConfirmInput(), CheckJumpOrStartInput());

            if (stateMachine.Phase == StageFlowStateMachine.FlowPhase.Stage1Gameplay)
            {
                if (player.CurrentSupportIndex == 9
                    && player.IsGrounded
                    && preview.Furniture != null
                    && preview.Furniture.HasCracked
                    && !player.IsStumbling
                    && player.transform.position.x >= Castle09Platform.TargetX - 0.8f)
                {
                    stateMachine.TryTriggerCastleCutscene();
                }
            }
            else if (stateMachine.Phase == StageFlowStateMachine.FlowPhase.Stage2Gameplay)
            {
                if (stage2BannerTimer > 0f) stage2BannerTimer -= Time.deltaTime;
                stage2RunTimer += Time.deltaTime;

                if (preview.LastPlatformIndex <= 10)
                {
                    float t = Mathf.Clamp01(stage2RunTimer / Stage2AccelTime);
                    player.SetRunSpeed(Mathf.Lerp(Stage2InitialSpeed, Stage2TargetSpeed, t));

                    if (player.CurrentSupportIndex == 10
                        && player.IsGrounded
                        && !player.IsStumbling
                        && player.transform.position.x >= TrainTrestle10Platform.TargetX + TrainTrestle10Platform.SupportWidth * 0.35f)
                    {
                        stateMachine.TryCompleteStage2();
                    }
                }
                else
                {
                    int support = Mathf.Clamp(player.CurrentSupportIndex, 10, 21);
                    float progress = (support - 10) / 11f;
                    player.SetRunSpeed(Mathf.Lerp(
                        Preview.Controllers.DarkRoomFullStageCatalog.Stage2InitialSpeed,
                        Preview.Controllers.DarkRoomFullStageCatalog.Stage2MaxSpeed,
                        progress));
                    player.SetJumpHeight(Mathf.Lerp(
                        Preview.Controllers.DarkRoomFullStageCatalog.Stage2InitialJumpHeight,
                        Preview.Controllers.DarkRoomFullStageCatalog.Stage2MaxJumpHeight,
                        progress));

                    if (support >= 21 && player.IsGrounded && !player.IsStumbling)
                    {
                        stateMachine.TryTriggerFalseDoorFall();
                    }
                }
            }
            else if (stateMachine.Phase == StageFlowStateMachine.FlowPhase.Stage3Gameplay)
            {
                if (stage2BannerTimer > 0f) stage2BannerTimer -= Time.deltaTime;

                int support = Mathf.Clamp(player.CurrentSupportIndex, 22, 37);
                float progress = (support - 22) / 15f;
                player.SetRunSpeed(Mathf.Lerp(
                    Preview.Controllers.DarkRoomFullStageCatalog.Stage3InitialSpeed,
                    Preview.Controllers.DarkRoomFullStageCatalog.Stage3MaxSpeed,
                    progress));
                player.SetJumpHeight(Mathf.Lerp(
                    Preview.Controllers.DarkRoomFullStageCatalog.Stage3InitialJumpHeight,
                    Preview.Controllers.DarkRoomFullStageCatalog.Stage3MaxJumpHeight,
                    progress));

                if (support >= 37 && player.CurrentSupportIndex == 37 && player.IsGrounded && !player.IsStumbling)
                {
                    stateMachine.TryCompleteTruePortal();
                }
            }
        }

        private void HandlePhaseChanged(StageFlowStateMachine.FlowPhase next)
        {
            openingCinematic?.OnPhaseChanged(next);
            falseDoorCinematic?.OnPhaseChanged(next);
            switch (next)
            {
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake1:
                case StageFlowStateMachine.FlowPhase.OpeningPoeticIntro:
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake2:
                case StageFlowStateMachine.FlowPhase.PreJumpCue:
                    cam?.SetIllustratedSpanScale(1f);
                    PositionNandaForPhase(next);
                    break;

                case StageFlowStateMachine.FlowPhase.Stage1Gameplay:
                    player.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
                    player.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
                    player.SetFloorDeathActive(true);
                    player.AutoRunActive = true;
                    player.AllowFloorLaunch(player.transform.position.x);
                    guide?.ReturnToYuna();
                    break;

                case StageFlowStateMachine.FlowPhase.CastleCutsceneStep0:
                    player.AutoRunActive = false;
                    if (cam != null)
                    {
                        cam.AutoScrollActive = false;
                        cam.BeginCastleRouteOverview();
                    }
                    guide?.SetTargetPlatform(10);
                    break;

                case StageFlowStateMachine.FlowPhase.Stage2Gameplay:
                    stage2BannerTimer = 2.6f;
                    stage2RunTimer = 0f;
                    cam?.EndRouteOverview();
                    player.SetRunSpeed(Stage2InitialSpeed);
                    player.SetJumpHeight(Stage2JumpHeight);
                    player.SetRunDirection(1f);
                    player.SetFloorDeathActive(true);
                    player.AutoRunActive = true;
                    guide?.ReturnToYuna();
                    break;

                case StageFlowStateMachine.FlowPhase.Stage2Completed:
                case StageFlowStateMachine.FlowPhase.TruePortalVictory:
                    portalVictoryElapsed = 0f;
                    transitionedToToyRoom = false;
                    player.AutoRunActive = false;
                    if (cam != null) cam.AutoScrollActive = false;
                    break;

                case StageFlowStateMachine.FlowPhase.FalseDoorFalling:
                    if (cam != null) cam.SetScrollState(false);
                    EnsureFloorCoversPosition(ResolveStage3ReturnStartX());
                    BeginFalseDoorPlatformCollapse();
                    player.StartCinematicFall(ResolveStage3ReturnStartX());
                    break;

                case StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0:
                    preview?.PlatformController?.SetStage3PlatformsActive(true);
                    player.ResetLastPlatformIndex();
                    falseDoorCutsceneElapsed = 0f;
                    falseDoorBeatStage = 0;
                    player.SetRunDirection(1f);
                    player.BeginStumble(0f, FalseDoorFallenDuration + 0.5f);
                    guide?.ReturnToYuna();
                    break;

                case StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep1:
                    player.EndStumble();
                    player.SetRunDirection(-1f);
                    guide?.ReturnToYuna();
                    guide?.ApplyMood(YunaLegend.Dialogue.NandaMood.Annoyed);
                    break;

                case StageFlowStateMachine.FlowPhase.Stage3Gameplay:
                    player.EndStumble();
                    preview?.PlatformController?.SetStage3PlatformsActive(true);
                    stage2BannerTimer = 2.6f;
                    player.SetRunDirection(-1f);
                    player.SetRunSpeed(Preview.Controllers.DarkRoomFullStageCatalog.Stage3InitialSpeed);
                    player.SetJumpHeight(Preview.Controllers.DarkRoomFullStageCatalog.Stage3InitialJumpHeight);
                    player.ResetLastPlatformIndex();
                    player.AllowFloorLaunch(player.transform.position.x);
                    player.SetFloorDeathActive(true);
                    player.AutoRunActive = true;
                    cam?.SetIllustratedSpanScale(1.15f);
                    cam?.SnapToTargetX(player.transform.position.x, DarkRoomReturnLayout.CameraOffset);
                    guide?.ReturnToYuna();
                    break;
            }
        }

        private void UpdateFalseDoorCutsceneBeats()
        {
            if (player == null)
            {
                return;
            }

            if (falseDoorBeatStage < 1 && falseDoorCutsceneElapsed >= FalseDoorFallenDuration)
            {
                falseDoorBeatStage = 1;
                player.EndStumble();
                player.SetRunDirection(-1f);
            }

            if (falseDoorBeatStage < 2 && falseDoorCutsceneElapsed >= FalseDoorLookLeftEnd)
            {
                falseDoorBeatStage = 2;
                player.SetRunDirection(1f);
            }

            if (falseDoorBeatStage < 3 && falseDoorCutsceneElapsed >= FalseDoorLookRightEnd)
            {
                falseDoorBeatStage = 3;
                player.SetRunDirection(-1f);
            }
        }

        private void BeginFalseDoorPlatformCollapse()
        {
            falseDoorPlatformView = preview != null && preview.PlatformController != null
                ? preview.PlatformController.GetGenericView(21)
                : null;
            if (falseDoorPlatformView == null)
            {
                return;
            }

            if (!falseDoorPoseSaved)
            {
                falseDoorOriginalLocalPos = falseDoorPlatformView.transform.localPosition;
                falseDoorOriginalLocalRot = falseDoorPlatformView.transform.localRotation;
                falseDoorPoseSaved = true;
            }

            foreach (var col in falseDoorPlatformView.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
            }

            falseDoorCollapseVelocityY = 0f;
            falseDoorCollapsing = true;
        }

        private void ResetFalseDoorPlatformView()
        {
            falseDoorCollapsing = false;
            falseDoorCollapseVelocityY = 0f;
            if (falseDoorPlatformView == null)
            {
                return;
            }

            falseDoorPlatformView.gameObject.SetActive(true);
            if (falseDoorPoseSaved)
            {
                falseDoorPlatformView.transform.localPosition = falseDoorOriginalLocalPos;
                falseDoorPlatformView.transform.localRotation = falseDoorOriginalLocalRot;
            }

            foreach (var col in falseDoorPlatformView.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = true;
            }
        }

        private void EnsureFloorCoversPosition(float targetX)
        {
            var floor = GameObject.Find("Chao_Escuro_Sala");
            if (floor == null)
            {
                return;
            }

            Vector3 scale = floor.transform.localScale;
            Vector3 pos = floor.transform.position;
            float maxX = pos.x + scale.x * 0.5f;
            if (maxX < targetX + 20f)
            {
                float newMaxX = targetX + 30f;
                float minX = pos.x - scale.x * 0.5f;
                scale.x = newMaxX - minX;
                pos.x = (minX + newMaxX) * 0.5f;
                floor.transform.localScale = scale;
                floor.transform.position = pos;
            }
        }

        private float ResolveStage3ReturnStartX()
        {
            var portal21 = preview != null && preview.PlatformController != null
                ? preview.PlatformController.GetGenericView(21)
                : null;
            return portal21 != null ? portal21.transform.position.x - 1.2f : player.transform.position.x - 1.2f;
        }

        private void RetryFromCheckpoint()
        {
            if (stateMachine.Stage3CheckpointReached)
            {
                Vector3 stage3Spawn = new Vector3(ResolveStage3ReturnStartX(), DarkRoomReturnLayout.StartY, 0f);
                player.ResetPlayer(stage3Spawn, -1f);
                player.ResetLastPlatformIndex();
                player.AllowFloorLaunch(stage3Spawn.x);
                stateMachine.RespawnAtStage3Checkpoint();
                cam?.SnapToTargetX(stage3Spawn.x, DarkRoomReturnLayout.CameraOffset);
                return;
            }

            if (!stateMachine.Stage2CheckpointReached)
            {
                Vector3 stage1Spawn = DarkRoomStartLayout.Position;
                preview?.PlatformController?.Furniture?.ResetForRetry();
                player.ResetPlayer(stage1Spawn, 1f);
                player.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
                player.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
                player.ResetLastPlatformIndex();
                player.AllowFloorLaunch(stage1Spawn.x);
                openingCinematic?.PrepareRespawnAtPreJump();
                stateMachine.RespawnAtStage1Checkpoint();
                cam?.SnapToTargetX(stage1Spawn.x);
                return;
            }

            Vector3 castleSpawn = new Vector3(Castle09Platform.TargetX - 0.5f, Castle09Platform.TopY + 0.08f, 0f);
            ResetFalseDoorPlatformView();
            preview?.PlatformController?.SetStage3PlatformsActive(false);
            player.ResetPlayer(castleSpawn, 1f);
            player.RestoreRouteSupport(9);
            stateMachine.RespawnAtStage2Checkpoint();
            cam?.SnapToTargetX(castleSpawn.x);
        }

        private void PositionNandaForPhase(StageFlowStateMachine.FlowPhase targetPhase)
        {
            if (guide == null || player == null) return;
            switch (targetPhase)
            {
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake1:
                case StageFlowStateMachine.FlowPhase.OpeningPoeticIntro:
                    guide.transform.position = player.transform.position + new Vector3(0.95f, 1.45f, -0.15f);
                    break;
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake2:
                    guide.transform.position = player.transform.position + new Vector3(0.42f, 1.05f, -0.15f);
                    break;
                case StageFlowStateMachine.FlowPhase.PreJumpCue:
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
            if (Keyboard.current != null
                && (Keyboard.current.spaceKey.wasPressedThisFrame
                    || Keyboard.current.enterKey.wasPressedThisFrame
                    || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                return true;
            }

            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                return true;
            }

            // No modo mobile, toque em qualquer area da tela (exceto botoes do topo direito) passa as falas rapido
            if ((Application.isMobilePlatform || Touchscreen.current != null)
                && Touchscreen.current != null
                && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
                bool inTopRightHud = touchPos.x > Screen.width - 180f && touchPos.y > Screen.height - 96f;
                if (!inTopRightHud)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CheckJumpOrStartInput()
        {
            if (CheckConfirmInput()) return true;
            if (Keyboard.current != null
                && (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame))
            {
                return true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        }
    }
}
