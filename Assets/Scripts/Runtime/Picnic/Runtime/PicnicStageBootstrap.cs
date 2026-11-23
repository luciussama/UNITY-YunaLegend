using UnityEngine;
using UnityEngine.SceneManagement;
using YunaLegend.Picnic.Core;
using YunaLegend.Qa.Runtime;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>Composition root: wires input, pure state and specialist views.</summary>
    public sealed class PicnicStageBootstrap : MonoBehaviour
    {
        [SerializeField] private Camera stageCamera;
        [SerializeField] private PicnicPlatformController platforms;
        [SerializeField] private PicnicSceneryController scenery;
        [SerializeField] private PicnicCharacterController player;
        [SerializeField] private PicnicNandaController nanda;
        [SerializeField] private PicnicCreatureController creatures;
        [SerializeField] private PicnicAudioController audioController;
        [SerializeField] private PicnicInputAdapter input;
        [SerializeField] private PicnicHudController hud;
        [SerializeField] private PicnicNarrativeController narrative;
        private bool paused;
        public PicnicSimulation Simulation { get; private set; }
        public void Configure(Camera camera, PicnicPlatformController platformView, PicnicSceneryController sceneryView,
            PicnicCharacterController playerView, PicnicNandaController guideView, PicnicCreatureController creatureView,
            PicnicAudioController sound, PicnicInputAdapter inputView, PicnicHudController hudView, PicnicNarrativeController storyView)
        {
            stageCamera = camera;
            platforms = platformView;
            scenery = sceneryView;
            player = playerView;
            nanda = guideView;
            creatures = creatureView;
            audioController = sound;
            input = inputView;
            hud = hudView;
            narrative = storyView;
        }
        private void Start()
        {
            Simulation = new PicnicSimulation(PlayerPrefs.GetInt("YunaLegend.EasyMode", 0) == 1);
            Simulation.Event += HandleDomainEvent;
            platforms.Build(Simulation.Stage);
            scenery.Build(stageCamera, Simulation.Stage);
            player.Build();
            nanda.Build();
            creatures.Build(Simulation);
            audioController.Build();
            hud.Build();
            narrative.Build();
            hud.RetryRequested += HandleRetry;
            hud.MenuRequested += HandleMenu;
            hud.PauseRequested += TogglePause;
            narrative.AdvanceRequested += HandleAdvance;
            narrative.SkipRequested += HandleSkip;
            ApplyViews(0f);
            WebQaRuntime.LogEvent("picnic_ready", "supports=" + Simulation.Stage.Supports.Count);
        }
        private void Update()
        {
            if (Simulation == null) return;
            input.Poll(hud.TouchAxis, hud.ConsumeJump());
            if (input.Pause) TogglePause();
            if (paused) return;
            if (input.Advance && (Simulation.Phase == PicnicPhase.Opening || Simulation.Phase == PicnicPhase.Ending)) HandleAdvance();
            var command = WebQaRuntime.Current.AutoPlay ? PicnicQaPilot.Read(Simulation) : new PicnicInput(input.Axis, input.Jump);
            float dt = Mathf.Min(0.1f, UnityEngine.Time.deltaTime);
            Simulation.Step(dt, command.Axis, command.Jump);
            ApplyViews(dt);
        }
        private void ApplyViews(float dt)
        {
            platforms.Apply(Simulation);
            player.Apply(Simulation, dt);
            nanda.Apply(Simulation);
            creatures.Apply(Simulation);
            scenery.Apply(Simulation);
            hud.Apply(Simulation, paused);
            narrative.Apply(Simulation);
        }
        private void TogglePause()
        {
            paused = !paused;
            audioController.SetPaused(paused);
            hud.Apply(Simulation, paused);
        }
        private void HandleRetry()
        {
            if (paused) { TogglePause(); return; }
            if (Simulation.Phase == PicnicPhase.Complete)
            {
                SceneManager.LoadScene("Picnic_Gameplay");
                return;
            }
            Simulation.Retry();
            ApplyViews(0f);
        }
        private void HandleMenu() { SceneManager.LoadScene("TitleScreen"); }
        private void HandleAdvance() { Simulation.AdvanceNarrative(); ApplyViews(0f); }
        private void HandleSkip() { Simulation.SkipNarrative(); ApplyViews(0f); }
        private void HandleDomainEvent(string name)
        {
            WebQaRuntime.LogEvent("picnic_" + name, $"x={Simulation.X:F2} y={Simulation.Y:F2} hearts={Simulation.Hearts}");
        }
        private void OnDestroy()
        {
            if (Simulation != null) Simulation.Event -= HandleDomainEvent;
            if (hud != null)
            {
                hud.RetryRequested -= HandleRetry;
                hud.MenuRequested -= HandleMenu;
                hud.PauseRequested -= TogglePause;
            }
            if (narrative != null)
            {
                narrative.AdvanceRequested -= HandleAdvance;
                narrative.SkipRequested -= HandleSkip;
            }
        }
    }
}
