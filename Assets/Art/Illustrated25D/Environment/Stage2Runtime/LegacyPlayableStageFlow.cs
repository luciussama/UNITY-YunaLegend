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
            Stage2Completed
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
        private Vector3 bearTopWorld;

        private float stage2BannerTimer;
        private float stage2RunTimer;
        private bool muted;
        private bool paused;

        public FlowPhase Phase => (FlowPhase)(int)stateMachine.Phase;
        public bool IsPaused => paused;
        public bool IsMuted => muted;
        public bool Stage2CheckpointReached => stateMachine.Stage2CheckpointReached;
        public bool ShowIdlePose => stateMachine.ShowIdlePose;

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
            hud.GetFailure = () => preview != null ? preview.Failure : null;
            hud.GetStage2BannerTimer = () => stage2BannerTimer;
            hud.GetPlayerWorldPosition = () => player != null ? player.transform.position : Vector3.zero;
            hud.GetBearTopWorldPosition = () => bearTopWorld;
            hud.OnAdvanceRequested += AdvanceStepForAutomation;
            hud.OnTogglePauseRequested += TogglePause;
            hud.OnToggleMuteRequested += ToggleMute;
            hud.OnRetryFromBeginningRequested += () => preview.Retry();
            hud.OnRetryFromCheckpointRequested += RetryFromCheckpoint;

            if (player != null)
            {
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
            Time.timeScale = 1f;
            AudioListener.volume = 1f;
        }

        public void AdvanceStepForAutomation()
        {
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
                    || currentPhase == StageFlowStateMachine.FlowPhase.Stage2Gameplay)
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

            stateMachine.Tick(Time.deltaTime, CheckConfirmInput(), CheckJumpOrStartInput());

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
        }

        private void HandlePhaseChanged(StageFlowStateMachine.FlowPhase next)
        {
            switch (next)
            {
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake1:
                case StageFlowStateMachine.FlowPhase.OpeningPoeticIntro:
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake2:
                case StageFlowStateMachine.FlowPhase.PreJumpCue:
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
                    player.AutoRunActive = false;
                    if (cam != null) cam.AutoScrollActive = false;
                    break;
            }
        }

        private void RetryFromCheckpoint()
        {
            if (!stateMachine.Stage2CheckpointReached)
            {
                preview.Retry();
                return;
            }

            Vector3 castleSpawn = new Vector3(Castle09Platform.TargetX - 0.5f, Castle09Platform.TopY + 0.08f, 0f);
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
            return Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
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
