using System;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicHudController : MonoBehaviour
    {
        public event Action RetryRequested;
        public event Action MenuRequested;
        public event Action PauseRequested;
        public float TouchAxis => (right != null && right.Held ? 1f : 0f) - (left != null && left.Held ? 1f : 0f);
        private PicnicHoldButton left;
        private PicnicHoldButton right;
        private bool jump;
        private GameObject root;
        private GameObject controls;
        private GameObject modal;
        private Text status;
        private readonly PicnicHudSymbol[] hearts = new PicnicHudSymbol[3];
        private PicnicHudSymbol infinity;
        private Text hint;
        private Text modalTitle;
        private Text retryLabel;
        public bool ConsumeJump()
        {
            bool value = jump;
            jump = false;
            return value;
        }
        public void Build()
        {
            root = PicnicUi.Canvas("Picnic HUD", 20).gameObject;
            var panel = PicnicUi.Image("Status parchment", root.transform, new Vector2(0.02f, 0.87f), new Vector2(0.45f, 0.98f), new Color(0.98f, 0.96f, 0.84f, 0.94f));
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i] = PicnicHudSymbol.Create("Heart " + (i + 1), panel.transform,
                    new Vector2(0.04f + i * 0.105f, 0.52f), new Vector2(0.135f + i * 0.105f, 0.95f),
                    PicnicSymbol.Heart, new Color(0.65f, 0.18f, 0.22f));
            }
            infinity = PicnicHudSymbol.Create("Unlimited lives", panel.transform, new Vector2(0.15f, 0.52f),
                new Vector2(0.31f, 0.95f), PicnicSymbol.Infinity, new Color(0.15f, 0.24f, 0.16f));
            status = PicnicUi.Text("Progress and region", panel.transform, new Vector2(0.04f, 0f), new Vector2(0.96f, 0.52f), "", 23, new Color(0.15f, 0.24f, 0.16f));
            hint = PicnicUi.Text("Nanda reassurance", root.transform, new Vector2(0.1f, 0.71f), new Vector2(0.88f, 0.85f), "", 23, new Color(0.08f, 0.17f, 0.13f));
            PicnicUi.Button("Pause", root.transform, new Vector2(0.9f, 0.9f), new Vector2(0.98f, 0.98f), "Pausa").onClick.AddListener(() => PauseRequested?.Invoke());
            controls = PicnicUi.Rect("Touch controls", root.transform, Vector2.zero, Vector2.one).gameObject;
            left = PicnicUi.Button("Left", controls.transform, new Vector2(0.025f, 0.035f), new Vector2(0.105f, 0.15f), "ESQ").gameObject.AddComponent<PicnicHoldButton>();
            right = PicnicUi.Button("Right", controls.transform, new Vector2(0.12f, 0.035f), new Vector2(0.2f, 0.15f), "DIR").gameObject.AddComponent<PicnicHoldButton>();
            PicnicUi.Button("Jump", controls.transform, new Vector2(0.84f, 0.035f), new Vector2(0.975f, 0.15f), "PULAR").onClick.AddListener(() => jump = true);
            modal = PicnicUi.Image("Pause or result", root.transform, new Vector2(0.25f, 0.28f), new Vector2(0.75f, 0.72f), new Color(0.97f, 0.95f, 0.85f, 0.98f)).gameObject;
            modalTitle = PicnicUi.Text("Title", modal.transform, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.9f), "", 30, new Color(0.14f, 0.24f, 0.17f));
            var retry = PicnicUi.Button("Continue or retry", modal.transform, new Vector2(0.12f, 0.34f), new Vector2(0.88f, 0.54f), "");
            retryLabel = retry.GetComponentInChildren<Text>();
            retry.onClick.AddListener(() => RetryRequested?.Invoke());
            PicnicUi.Button("Return to menu", modal.transform, new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.28f), "Voltar ao início").onClick.AddListener(() => MenuRequested?.Invoke());
        }
        public void Apply(PicnicSimulation sim, bool paused)
        {
            bool gameplay = sim.Phase != PicnicPhase.Opening && sim.Phase != PicnicPhase.Ending;
            root.SetActive(gameplay);
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].gameObject.SetActive(!sim.EasyMode || i == 0);
                hearts[i].color = i < sim.Hearts || sim.EasyMode
                    ? new Color(0.65f, 0.18f, 0.22f) : new Color(0.65f, 0.18f, 0.22f, 0.2f);
            }
            infinity.gameObject.SetActive(sim.EasyMode);
            status.text = Mathf.FloorToInt(sim.X / PicnicStageSpec.Goal * 100f) + "% - " + sim.Stage.Sections[sim.Section].Name;
            controls.SetActive(sim.Phase == PicnicPhase.Playing && !paused);
            hint.text = sim.Phase == PicnicPhase.Rescuing ? "ESTOU AQUI\nNanda leva você de volta ao chão seguro."
                : sim.Panic ? "NANDA\nCalma, Yuna, é só um besouro. Ele não vai te fazer mal.\nYuna corre sozinha: controle apenas o pulo."
                : sim.X < 7f ? "ATÉ O PIQUENIQUE\nA/D ou ESQ/DIR para andar - Espaço ou PULAR para pular" : "";
            bool result = sim.Phase == PicnicPhase.Defeat || sim.Phase == PicnicPhase.Complete;
            modal.SetActive(paused || result);
            modalTitle.text = paused ? "Uma pausa no jardim" : sim.Phase == PicnicPhase.Defeat ? "Vamos tentar mais uma vez?" : "O PIQUENIQUE\nUm lugar para descansar";
            retryLabel.text = paused ? "Continuar" : result && sim.Phase == PicnicPhase.Complete ? "Passear de novo" : "Tentar novamente";
        }
        private void OnDestroy()
        {
            if (root != null) Destroy(root);
        }
    }
}
