using UnityEngine;
using UnityEngine.SceneManagement;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Composition Root fino da Fase 4 (Perseguicao do Urso Fujao):
    /// conecta a simulacao pura aos adaptadores especialistas via eventos de dominio.
    /// </summary>
    public sealed class BearChaseStageBootstrap : MonoBehaviour
    {
        private const string EasyModePrefKey = "YunaLegend.EasyMode";

        [SerializeField] private BearChaseInputAdapter inputAdapter;
        [SerializeField] private BearChaseEntranceController entranceController;
        [SerializeField] private BearChaseSceneryController sceneryController;
        [SerializeField] private BearChaseLightingController lightingController;
        [SerializeField] private BearChaseObstacleController obstacleController;
        [SerializeField] private BearChaseCameraController cameraController;
        [SerializeField] private BearChasePlayerController playerController;
        [SerializeField] private BearChaseBearAnimator bearAnimator;
        [SerializeField] private BearChaseNandaController nandaController;
        [SerializeField] private BearChaseHudController hudController;
        [SerializeField] private BearChaseAudioController audioController;
        [SerializeField] private Vector3 chaseOrigin = new Vector3(0f, 0f, 40f);

        private BearChaseSimulation simulation;
        private bool isPaused;

        public Vector3 ChaseOrigin => chaseOrigin;

        public void Configure(
            BearChaseInputAdapter input,
            BearChaseEntranceController entrance,
            BearChaseSceneryController scenery,
            BearChaseLightingController lighting,
            BearChaseObstacleController obstacles,
            BearChaseCameraController cam,
            BearChasePlayerController player,
            BearChaseBearAnimator bear,
            BearChaseNandaController nanda,
            BearChaseHudController hud,
            BearChaseAudioController audio)
        {
            inputAdapter = input;
            entranceController = entrance;
            sceneryController = scenery;
            lightingController = lighting;
            obstacleController = obstacles;
            cameraController = cam;
            playerController = player;
            bearAnimator = bear;
            nandaController = nanda;
            hudController = hud;
            audioController = audio;
        }

        private void Awake()
        {
            simulation = new BearChaseSimulation();
            bool useEasyMode = PlayerPrefs.GetInt(EasyModePrefKey, 0) == 1;
            if (useEasyMode)
            {
                simulation.ResetFullStage(skipEntrance: false, useEasyMode: true);
            }

            BindSimulationEvents(bind: true);
        }

        private void Start()
        {
            BindExternalEvents(bind: true);
            lightingController?.ApplyAtmosphere();
            audioController?.ResetForStage(skipEntrance: !simulation.IsEntrancePhase);
            sceneryController?.ApplyPhaseVisibility(simulation.Phase);
            hudController?.SetHearts(simulation.Hearts, simulation.MaxHearts);
            hudController?.HandlePhaseChanged(simulation.Phase, simulation.ShouldOfferEasyMode);
            UpdateControllers(0f);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            BindSimulationEvents(bind: false);
            BindExternalEvents(bind: false);
        }

        private void Update()
        {
            inputAdapter?.PollInput();
            if (isPaused || simulation == null)
            {
                return;
            }

            float dt = Mathf.Min(0.1f, Time.deltaTime);
            float lateral = inputAdapter != null ? inputAdapter.LateralAxis : 0f;
            simulation.SetLateralInput(lateral);
            simulation.Step(dt);
            UpdateControllers(dt);
        }

        private void UpdateControllers(float dt)
        {
            entranceController?.ApplySample(simulation.EntranceSample, simulation.IsEntrancePhase);
            playerController?.UpdatePlayer(simulation, chaseOrigin, dt);
            bearAnimator?.UpdateBear(simulation, chaseOrigin, dt);
            nandaController?.UpdateNanda(simulation, chaseOrigin, dt);
            obstacleController?.SyncToys(simulation.ActiveThrownToys, chaseOrigin, simulation.TrackSpec, !simulation.IsEntrancePhase);
            lightingController?.UpdateWindowLights(simulation);
            cameraController?.UpdateCamera(simulation, chaseOrigin, dt);
            audioController?.UpdateFootsteps(simulation, dt);
            hudController?.UpdatePerFrameOverlays(simulation);
        }

        private void BindSimulationEvents(bool bind)
        {
            if (simulation == null)
            {
                return;
            }

            if (bind)
            {
                simulation.OnPhaseChanged += HandlePhaseChanged;
                simulation.OnHeartsChanged += HandleHeartsChanged;
                simulation.OnBearThrowTelegraphed += HandleBearThrowTelegraphed;
                simulation.OnBearToyThrown += HandleBearToyThrown;
                simulation.OnThrownToyLanded += HandleThrownToyLanded;
                simulation.OnObstacleHit += HandleObstacleHit;
                simulation.OnDefeat += HandleDefeat;
                simulation.OnVictory += HandleVictory;
            }
            else
            {
                simulation.OnPhaseChanged -= HandlePhaseChanged;
                simulation.OnHeartsChanged -= HandleHeartsChanged;
                simulation.OnBearThrowTelegraphed -= HandleBearThrowTelegraphed;
                simulation.OnBearToyThrown -= HandleBearToyThrown;
                simulation.OnThrownToyLanded -= HandleThrownToyLanded;
                simulation.OnObstacleHit -= HandleObstacleHit;
                simulation.OnDefeat -= HandleDefeat;
                simulation.OnVictory -= HandleVictory;
            }
        }

        private void BindExternalEvents(bool bind)
        {
            if (bind)
            {
                if (inputAdapter != null)
                {
                    inputAdapter.OnPausePressed += HandlePauseToggled;
                    inputAdapter.OnMutePressed += HandleMuteToggled;
                }

                if (hudController != null)
                {
                    hudController.OnPauseToggled += HandlePauseToggled;
                    hudController.OnMuteToggled += HandleMuteToggled;
                    hudController.OnRetryRequested += HandleRetryRequested;
                    hudController.OnReturnToTitleRequested += HandleReturnToTitleRequested;
                }
            }
            else
            {
                if (inputAdapter != null)
                {
                    inputAdapter.OnPausePressed -= HandlePauseToggled;
                    inputAdapter.OnMutePressed -= HandleMuteToggled;
                }

                if (hudController != null)
                {
                    hudController.OnPauseToggled -= HandlePauseToggled;
                    hudController.OnMuteToggled -= HandleMuteToggled;
                    hudController.OnRetryRequested -= HandleRetryRequested;
                    hudController.OnReturnToTitleRequested -= HandleReturnToTitleRequested;
                }
            }
        }

        private void HandlePhaseChanged(BearChasePhase phase)
        {
            sceneryController?.ApplyPhaseVisibility(phase);
            hudController?.HandlePhaseChanged(phase, simulation.ShouldOfferEasyMode);

            if (phase == BearChasePhase.BearWakeUp)
            {
                audioController?.PlayBearWake();
            }
            else if (phase == BearChasePhase.Chasing)
            {
                audioController?.EnsureChaseMusicPlaying();
            }
        }

        private void HandleHeartsChanged(int current, int max)
        {
            hudController?.SetHearts(current, max);
        }

        private void HandleBearThrowTelegraphed(BearChaseObstacleKind kind)
        {
            nandaController?.TriggerAlertPulse();
        }

        private void HandleBearToyThrown(BearChaseThrownToyState toy)
        {
            audioController?.PlayThrow();
        }

        private void HandleThrownToyLanded(BearChaseThrownToyState toy)
        {
            audioController?.PlayToyLand();
            obstacleController?.PlayLandEffect(toy, chaseOrigin, simulation.TrackSpec);
        }

        private void HandleObstacleHit(BearChaseThrownToyState toy)
        {
            audioController?.PlayHit();
            nandaController?.TriggerAlertPulse();
            cameraController?.AddShake(0.26f, 0.22f);
        }

        private void HandleDefeat()
        {
            audioController?.PlayDefeat();
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
            hudController?.SetPaused(isPaused);
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
            hudController?.SetPaused(false);
            obstacleController?.ResetAll();
            cameraController?.ResetCameraPose();
            simulation.RetryChase(activateEasyMode);
            audioController?.ResetForStage(skipEntrance: true);
            UpdateControllers(0f);
        }

        private void HandleReturnToTitleRequested()
        {
            isPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("TitleScreen");
        }
    }
}
