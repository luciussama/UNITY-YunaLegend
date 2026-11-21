using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Composition Root da Fase 4 (Perseguicao do Urso Fujao):
    /// conecta simulacao pura, entrada do jogador (A/D, setas, toque), camera, Yuna, Urso, Nanda, audio e HUD.
    /// </summary>
    public sealed class BearChaseStageBootstrap : MonoBehaviour
    {
        [SerializeField] private BearChaseCameraController cameraController;
        [SerializeField] private BearChasePlayerController playerController;
        [SerializeField] private BearChaseBearAnimator bearAnimator;
        [SerializeField] private BearChaseNandaController nandaController;
        [SerializeField] private BearChaseHudController hudController;
        [SerializeField] private BearChaseAudioController audioController;

        [SerializeField] private GameObject entranceRoomRoot;
        [SerializeField] private GameObject chaseCorridorRoot;

        [SerializeField] private Vector3 entranceYunaStart = new Vector3(-2.65f, 0f, 0f);
        [SerializeField] private Vector3 entranceYunaNudge = new Vector3(0.62f, 0f, 0f);
        [SerializeField] private Vector3 entranceBearPos = new Vector3(2.15f, 0f, 0.05f);
        [SerializeField] private Vector3 chaseOrigin = new Vector3(0f, 0f, 40f);

        private BearChaseSimulation simulation;
        private bool isPaused;

        public GameObject EntranceRoomRoot => entranceRoomRoot;
        public GameObject ChaseCorridorRoot => chaseCorridorRoot;

        public void Configure(
            BearChaseCameraController cam,
            BearChasePlayerController player,
            BearChaseBearAnimator bear,
            BearChaseNandaController nanda,
            BearChaseHudController hud,
            BearChaseAudioController audio,
            GameObject entranceRoot,
            GameObject chaseRoot)
        {
            cameraController = cam;
            playerController = player;
            bearAnimator = bear;
            nandaController = nanda;
            hudController = hud;
            audioController = audio;
            entranceRoomRoot = entranceRoot;
            chaseCorridorRoot = chaseRoot;
        }

        private void Awake()
        {
            simulation = new BearChaseSimulation();
            simulation.OnPhaseChanged += HandlePhaseChanged;
            simulation.OnHeartsChanged += HandleHeartsChanged;
            simulation.OnObstacleHit += HandleObstacleHit;
            simulation.OnEasyModeOffered += HandleEasyModeOffered;
            simulation.OnVictory += HandleVictory;
        }

        private void Start()
        {
            if (hudController != null)
            {
                hudController.OnPauseToggled += HandlePauseToggled;
                hudController.OnMuteToggled += HandleMuteToggled;
                hudController.OnRetryRequested += HandleRetryRequested;
                hudController.OnReturnToTitleRequested += HandleReturnToTitleRequested;
                hudController.SetHearts(simulation.Hearts, simulation.MaxHearts);
            }

            ApplyStageRootVisibility(simulation.Phase);
            UpdateControllers(0f);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (simulation != null)
            {
                simulation.OnPhaseChanged -= HandlePhaseChanged;
                simulation.OnHeartsChanged -= HandleHeartsChanged;
                simulation.OnObstacleHit -= HandleObstacleHit;
                simulation.OnEasyModeOffered -= HandleEasyModeOffered;
                simulation.OnVictory -= HandleVictory;
            }

            if (hudController != null)
            {
                hudController.OnPauseToggled -= HandlePauseToggled;
                hudController.OnMuteToggled -= HandleMuteToggled;
                hudController.OnRetryRequested -= HandleRetryRequested;
                hudController.OnReturnToTitleRequested -= HandleReturnToTitleRequested;
            }
        }

        private void Update()
        {
            HandleUtilityHotkeys();
            if (isPaused || simulation == null)
            {
                return;
            }

            simulation.SetLateralInput(ReadLateralInput());
            simulation.Step(Time.deltaTime);
            UpdateControllers(Time.deltaTime);
        }

        private void UpdateControllers(float dt)
        {
            playerController?.UpdatePlayer(simulation, entranceYunaStart, entranceYunaNudge, chaseOrigin, dt);
            bearAnimator?.UpdateBear(simulation, entranceBearPos, chaseOrigin, dt);
            nandaController?.UpdateNanda(simulation.Phase, dt);

            Vector3 yunaChasePos = chaseOrigin + new Vector3(
                BearChaseConfig.TrackCenterX(simulation.YunaZ) + simulation.YunaLaneOffsetX,
                0f,
                simulation.YunaZ);
            cameraController?.UpdateCamera(simulation, yunaChasePos, dt);
            hudController?.UpdateVisualState(simulation);
        }

        private void HandlePhaseChanged(BearChasePhase phase)
        {
            ApplyStageRootVisibility(phase);
            if (phase == BearChasePhase.BearWakeUp)
            {
                audioController?.PlayBearWake();
            }
        }

        private void ApplyStageRootVisibility(BearChasePhase phase)
        {
            bool showEntrance = phase == BearChasePhase.EntranceWalk
                || phase == BearChasePhase.EntranceNudge
                || phase == BearChasePhase.BearWakeUp
                || phase == BearChasePhase.TransitionToChase;

            if (entranceRoomRoot != null)
            {
                entranceRoomRoot.SetActive(showEntrance);
            }

            if (chaseCorridorRoot != null)
            {
                chaseCorridorRoot.SetActive(!showEntrance);
            }
        }

        private void HandleHeartsChanged(int current, int max)
        {
            hudController?.SetHearts(current, max);
        }

        private void HandleObstacleHit(BearChaseObstacleSpec spec)
        {
            audioController?.PlayHit();
            cameraController?.AddShake(0.24f, 0.20f);
        }

        private void HandleEasyModeOffered()
        {
            hudController?.ShowEasyModeOffer();
        }

        private void HandleVictory()
        {
            audioController?.PlayVictory();
        }

        private void HandlePauseToggled()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            audioController?.SetPaused(isPaused);
        }

        private void HandleMuteToggled()
        {
            if (audioController != null)
            {
                bool muted = audioController.ToggleMute();
                hudController?.SetMuted(muted);
            }
        }

        private void HandleRetryRequested(bool activateEasyMode)
        {
            isPaused = false;
            Time.timeScale = 1f;
            simulation.RetryChase(activateEasyMode);
            ApplyStageRootVisibility(simulation.Phase);
            UpdateControllers(0f);
        }

        private void HandleReturnToTitleRequested()
        {
            isPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("TitleScreen");
        }

        private static void HandleUtilityHotkeys()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }

            if (kb.pKey.wasPressedThisFrame)
            {
                var inst = FindFirstObjectByType<BearChaseStageBootstrap>();
                inst?.HandlePauseToggled();
            }

            if (kb.mKey.wasPressedThisFrame)
            {
                var inst = FindFirstObjectByType<BearChaseStageBootstrap>();
                inst?.HandleMuteToggled();
            }
        }

        private static float ReadLateralInput()
        {
            float val = 0f;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
                {
                    val -= 1f;
                }

                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
                {
                    val += 1f;
                }
            }

            var pointer = Pointer.current;
            if (pointer != null && pointer.press.isPressed && Screen.width > 0)
            {
                float x = pointer.position.ReadValue().x;
                val += x < Screen.width * 0.5f ? -1f : 1f;
            }

            return Mathf.Clamp(val, -1f, 1f);
        }
    }
}
