using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using YunaLegend.DreamForest.Core;
using YunaLegend.DreamForestEntrance.Core;
using YunaLegend.DreamForestEntrance.Runtime;
using YunaLegend.Qa.Runtime;

namespace YunaLegend.DreamForest.Runtime
{
    /// <summary>
    /// Thin Composition Root for DreamForest_Gameplay (Jardim dos Sonhos).
    /// Wires pure Core simulation, SRP controllers, keyboard/touch inputs, and opt-in QA pilot.
    /// </summary>
    public sealed class DreamForestStageBootstrap : MonoBehaviour
    {
        private DreamForestSimulation simulation;
        private DreamForestSceneryController scenery;
        private DreamForestActorController actor;
        private DreamForestAudioController audioCtrl;
        private DreamForestHudController hud;
        private DreamEntranceView entrance;
        private DreamEntranceAmbience entranceAmbience;

        private bool paused;

        private void Awake()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camGo.tag = "MainCamera";
                cam = camGo.GetComponent<Camera>();
            }

            bool easy = PlayerPrefs.GetInt("YunaLegend.EasyMode", 0) == 1;
            simulation = new DreamForestSimulation(easy);

            scenery = new GameObject("DreamForestScenery").AddComponent<DreamForestSceneryController>();
            scenery.transform.SetParent(transform, false);
            scenery.Build(cam, simulation.Level);

            actor = new GameObject("DreamForestActor").AddComponent<DreamForestActorController>();
            actor.transform.SetParent(transform, false);
            actor.Build();

            audioCtrl = new GameObject("DreamForestAudio").AddComponent<DreamForestAudioController>();
            audioCtrl.transform.SetParent(transform, false);
            audioCtrl.Build();

            hud = new GameObject("DreamForestHud").AddComponent<DreamForestHudController>();
            hud.transform.SetParent(transform, false);
            hud.Build();

            hud.AdvanceRequested += OnAdvanceRequested;
            hud.PauseRequested += OnPauseRequested;
            hud.MuteRequested += OnMuteRequested;
            hud.RetryRequested += OnRetryRequested;
            hud.MenuRequested += OnMenuRequested;

            entrance = new GameObject("DreamForestEntrance").AddComponent<DreamEntranceView>();
            entrance.transform.SetParent(transform, false);
            entrance.Build();
            entrance.AdvanceRequested += OnAdvanceRequested;
            entrance.PauseRequested += OnPauseRequested;
            entrance.MuteRequested += OnMuteRequested;
            entrance.StageRequested += OnStageRequested;
            entranceAmbience = new GameObject("DreamForestEntranceAmbience").AddComponent<DreamEntranceAmbience>();
            entranceAmbience.transform.SetParent(transform, false);
            entranceAmbience.Build(audioCtrl.GetComponent<AudioSource>());

            ApplyVisuals();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            bool inEntrance = simulation.Stage == DreamForestStage.Opening;
            bool closedPhaseMenu = inEntrance && kb != null && kb.escapeKey.wasPressedThisFrame && entrance.ClosePhaseMenu();
            if (kb != null && ((kb.escapeKey.wasPressedThisFrame && !closedPhaseMenu) || (inEntrance && kb.pKey.wasPressedThisFrame)))
            {
                OnPauseRequested();
            }

            if (inEntrance && kb != null && kb.mKey.wasPressedThisFrame)
            {
                OnMuteRequested();
            }

            if (paused)
            {
                ApplyVisuals();
                return;
            }

            DreamForestInput input = WebQaRuntime.Current.AutoPlay
                ? (inEntrance ? DreamEntranceQaPilot.ComputeInput() : DreamForestQaPilot.ComputeInput(simulation))
                : SamplePlayerInput(kb);

            var events = simulation.Step(input, Time.deltaTime);
            audioCtrl.HandleEvents(events);
            ApplyVisuals();
        }

        private DreamForestInput SamplePlayerInput(Keyboard kb)
        {
            float x = hud != null ? hud.TouchAxisX : 0f;
            float y = hud != null ? hud.TouchAxisY : 0f;
            bool jump = hud != null && hud.TouchJumpHeld;
            bool advance = false;

            if (kb != null)
            {
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) x = -1f;
                else if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) x = 1f;

                if (kb.upArrowKey.isPressed || kb.wKey.isPressed) y = -1f;
                else if (kb.downArrowKey.isPressed || kb.sKey.isPressed) y = 1f;

                if (kb.spaceKey.isPressed || kb.zKey.isPressed) jump = true;
                if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                {
                    advance = simulation.Stage != DreamForestStage.Opening || !EntranceAdvanceSurface.ControlHasKeyboardFocus;
                }
            }

            return new DreamForestInput(x, y, jump, advance);
        }

        private void ApplyVisuals()
        {
            bool opening = simulation.Stage == DreamForestStage.Opening;
            scenery.gameObject.SetActive(!opening);
            actor.gameObject.SetActive(!opening);
            hud.gameObject.SetActive(!opening);
            entrance.Apply(simulation, paused, audioCtrl.IsMuted);
            if (!opening)
            {
                scenery.Apply(simulation);
                actor.Apply(simulation);
                hud.Apply(simulation, paused, audioCtrl.IsMuted);
            }
            audioCtrl.Apply(simulation, paused);
            entranceAmbience.Apply(simulation, paused, audioCtrl.IsMuted);
        }

        private void OnAdvanceRequested()
        {
            if (!paused)
            {
                simulation.AdvanceDialogue();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (simulation != null && simulation.Stage == DreamForestStage.Opening)
            {
                paused = !hasFocus;
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (simulation != null && simulation.Stage == DreamForestStage.Opening)
            {
                paused = isPaused;
            }
        }
        private void OnPauseRequested() => paused = !paused;
        private void OnMuteRequested() => audioCtrl.SetMuted(!audioCtrl.IsMuted);

        private void OnRetryRequested()
        {
            if (paused)
            {
                paused = false;
                return;
            }
            if (simulation.Stage == DreamForestStage.Complete)
            {
                simulation.ResetToBeginning();
                return;
            }
            simulation.Retry();
        }

        private static void OnMenuRequested()
        {
            SceneManager.LoadScene("TitleScreen");
        }

        private static void OnStageRequested(string scene)
        {
            SceneManager.LoadScene(scene);
        }
    }
}
