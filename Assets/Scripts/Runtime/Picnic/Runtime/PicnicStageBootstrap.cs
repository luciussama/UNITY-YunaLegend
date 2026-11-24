using UnityEngine;
using UnityEngine.SceneManagement;
using YunaLegend.Picnic.Core;
using YunaLegend.Qa.Runtime;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// Composition root for Picnic_Gameplay (1B faithful port): wires input, pure PicnicSimulation,
    /// opt-in PicnicQaPilot, and specialist SRP views.
    /// </summary>
    public sealed class PicnicStageBootstrap : MonoBehaviour
    {
        [SerializeField] private Camera stageCamera;
        [SerializeField] private PicnicPlatformController platforms;
        [SerializeField] private PicnicSceneryController scenery;
        [SerializeField] private PicnicActorController actors;
        [SerializeField] private PicnicNarrativeAndHudController ui;
        [SerializeField] private PicnicAudioController audioController;

        private bool paused;
        public PicnicSimulation Simulation { get; private set; }

        public void Configure(Camera cam, PicnicPlatformController platformCtrl,
            PicnicSceneryController sceneryCtrl, PicnicActorController actorCtrl,
            PicnicNarrativeAndHudController uiCtrl, PicnicAudioController audioCtrl = null)
        {
            stageCamera = cam;
            platforms = platformCtrl;
            scenery = sceneryCtrl;
            actors = actorCtrl;
            ui = uiCtrl;
            audioController = audioCtrl;
        }

        private void Start()
        {
            bool easy = PlayerPrefs.GetInt("YunaLegend.EasyMode", 0) == 1;
            Simulation = new PicnicSimulation(easy);
            Simulation.Event += HandleDomainEvent;

            platforms.Build(Simulation.Level);
            scenery.Build(stageCamera, Simulation.Level);
            actors.Build(Simulation);
            ui.Build();
            if (audioController == null)
            {
                audioController = gameObject.AddComponent<PicnicAudioController>();
            }
            audioController.Build();

            ui.AdvanceRequested += HandleAdvance;
            ui.SkipRequested += HandleSkip;
            ui.RetryRequested += HandleRetry;
            ui.MenuRequested += HandleMenu;
            ui.PauseRequested += TogglePause;

            ApplyViews();
            WebQaRuntime.LogEvent("picnic_1b_ready", "platforms=" + Simulation.Level.StaticPlatforms.Count);
        }

        private void Update()
        {
            if (Simulation == null) return;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) TogglePause();
            if (paused) return;

            PicnicInput input;
            if (WebQaRuntime.Current.AutoPlay)
            {
                input = PicnicQaPilot.Read(Simulation);
            }
            else
            {
                float kbAxis = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                             - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
                float axis = Mathf.Abs(kbAxis) > 0.01f ? kbAxis : ui.TouchAxis;
                bool jump = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || ui.ConsumeTouchJump();
                bool advance = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
                input = new PicnicInput(axis, jump, advance, false);
            }

            float dt = Mathf.Min(0.1f, UnityEngine.Time.deltaTime);
            Simulation.Step(dt, input);
            ApplyViews();
        }

        private void ApplyViews()
        {
            platforms.Apply(Simulation);
            actors.Apply(Simulation);
            scenery.Apply(Simulation);
            ui.Apply(Simulation, paused);
            if (audioController != null) audioController.Apply(Simulation);
        }

        private void TogglePause()
        {
            paused = !paused;
            if (audioController != null) audioController.SetPaused(paused);
            ui.Apply(Simulation, paused);
        }

        private void HandleAdvance()
        {
            Simulation.AdvanceStory();
            ApplyViews();
        }

        private void HandleSkip()
        {
            Simulation.SkipStory();
            ApplyViews();
        }

        private void HandleRetry()
        {
            if (paused) { TogglePause(); return; }
            if (Simulation.Stage == PicnicStage.GameOver)
            {
                Simulation.Retry();
                ApplyViews();
            }
        }

        private void HandleMenu()
        {
            SceneManager.LoadScene("TitleScreen");
        }

        private void HandleDomainEvent(string evt)
        {
            if (audioController != null) audioController.OnDomainEvent(evt);
            WebQaRuntime.LogEvent("picnic_" + evt, $"x={Simulation.PlayerX:F1} y={Simulation.PlayerY:F1} hp={Simulation.Hp}");
        }

        private void OnDestroy()
        {
            if (Simulation != null) Simulation.Event -= HandleDomainEvent;
            if (ui != null)
            {
                ui.AdvanceRequested -= HandleAdvance;
                ui.SkipRequested -= HandleSkip;
                ui.RetryRequested -= HandleRetry;
                ui.MenuRequested -= HandleMenu;
                ui.PauseRequested -= TogglePause;
            }
        }
    }
}
