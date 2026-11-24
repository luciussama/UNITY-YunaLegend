using System;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicNarrativeController : MonoBehaviour
    {
        public event Action AdvanceRequested;
        public event Action SkipRequested;
        private GameObject root;
        private Image background;
        private Image yuna;
        private Image nanda;
        private Text title;
        private Text body;
        public void Build()
        {
            root = PicnicUi.Canvas("Picnic story", 30).gameObject;
            background = PicnicUi.Image("Original actor-free painting", root.transform, Vector2.zero, Vector2.one, Color.white, "opening-bg");
            yuna = PicnicUi.Image("Current Yuna", root.transform, new Vector2(0.35f, 0.33f), new Vector2(0.55f, 0.88f), Color.white, "yuna-pose1");
            yuna.preserveAspect = true;
            nanda = PicnicUi.Image("Current blue Nanda", root.transform, new Vector2(0.56f, 0.53f), new Vector2(0.69f, 0.81f), Color.white, "nanda0");
            nanda.preserveAspect = true;
            var box = PicnicUi.Image("Narrative frame", root.transform, new Vector2(0.14f, 0.04f), new Vector2(0.86f, 0.29f), new Color(0.99f, 0.96f, 0.86f, 0.97f));
            title = PicnicUi.Text("Speaker", box.transform, new Vector2(0.03f, 0.73f), new Vector2(0.97f, 0.97f), "", 22, new Color(0.22f, 0.32f, 0.2f));
            body = PicnicUi.Text("Dialogue", box.transform, new Vector2(0.03f, 0.25f), new Vector2(0.97f, 0.73f), "", 25, new Color(0.2f, 0.18f, 0.13f));
            PicnicUi.Button("Next beat", box.transform, new Vector2(0.68f, 0.03f), new Vector2(0.97f, 0.23f), "Continuar ›").onClick.AddListener(() => AdvanceRequested?.Invoke());
            PicnicUi.Button("Skip story", root.transform, new Vector2(0.84f, 0.91f), new Vector2(0.98f, 0.98f), "Pular cena").onClick.AddListener(() => SkipRequested?.Invoke());
        }
        public void Apply(PicnicSimulation sim)
        {
            bool opening = sim.Phase == PicnicPhase.Opening;
            bool active = opening || sim.Phase == PicnicPhase.Ending;
            root.SetActive(active);
            if (!active) return;
            var narrative = opening ? PicnicNarrative.Opening : PicnicNarrative.Ending;
            int index = narrative.IndexAt(sim.NarrativeTime);
            title.text = narrative.Beats[index].Title;
            body.text = narrative.Beats[index].Text;
            background.sprite = PicnicArt.Load(opening ? "opening-bg" : "ending-bg");
            yuna.sprite = PicnicArt.Load(opening ? (index >= 4 && index < 8 ? "yuna-pose2" : "yuna-pose1") : index >= 11 ? "yuna-sleeping" : "yuna-sitting");
            nanda.sprite = PicnicArt.Load(!opening && index >= 4 ? "nanda-rest" : "nanda" + (Mathf.FloorToInt(sim.NarrativeTime * 12f) % 8));
            if (opening)
            {
                ApplyOpeningComposition(sim.NarrativeTime);
                return;
            }
            RestoreEndingComposition();
            yuna.rectTransform.anchoredPosition = new Vector2(0f, Mathf.Sin(sim.NarrativeTime * 1.5f) * 1.4f);
            nanda.rectTransform.anchoredPosition = new Vector2(0f, index >= 4 ? -75f : Mathf.Sin(sim.NarrativeTime * 3f) * 7f);
        }

        private void ApplyOpeningComposition(float time)
        {
            Rect viewport = ((RectTransform)root.transform).rect;
            if (viewport.width <= 0f || viewport.height <= 0f)
            {
                return;
            }
            var frame = PicnicOpeningComposition.Resolve(viewport.width, viewport.height, time);
            PlaceInPainting(background.rectTransform, frame.Painting);
            PlaceInPainting(yuna.rectTransform, frame.Yuna);
            PlaceInPainting(nanda.rectTransform, frame.Nanda);
        }

        private static void PlaceInPainting(RectTransform target, PicnicIllustrationRect rect)
        {
            target.anchorMin = new Vector2(0f, 1f);
            target.anchorMax = new Vector2(0f, 1f);
            target.pivot = new Vector2(0f, 1f);
            target.anchoredPosition = new Vector2(rect.Left, -rect.Top);
            target.sizeDelta = new Vector2(rect.Width, rect.Height);
        }

        private void RestoreEndingComposition()
        {
            RestoreRect(background.rectTransform, Vector2.zero, Vector2.one);
            RestoreRect(yuna.rectTransform, new Vector2(0.35f, 0.33f), new Vector2(0.55f, 0.88f));
            RestoreRect(nanda.rectTransform, new Vector2(0.56f, 0.53f), new Vector2(0.69f, 0.81f));
        }

        private static void RestoreRect(RectTransform target, Vector2 min, Vector2 max)
        {
            target.anchorMin = min;
            target.anchorMax = max;
            target.pivot = new Vector2(0.5f, 0.5f);
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }
        private void OnDestroy()
        {
            if (root != null) Destroy(root);
        }
    }
}
