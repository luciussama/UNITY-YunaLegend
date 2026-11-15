using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.NandaIllustration;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Controla a apresentação ilustrada da fada Nanda (sprite animado, luz azul, trilha e acompanhamento).
    /// </summary>
    public sealed class NandaGuideController : MonoBehaviour
    {
        public NandaFlightIllustration Flight { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
        private NandaAccompaniment accompaniment;

        public void Install(
            NandaFairyGuide guide,
            Yuna25DPlayerController player,
            Sprite[] nandaFrames,
            Material spriteMaterial)
        {
            foreach (var trail in guide.GetComponentsInChildren<TrailRenderer>(true))
            {
                trail.emitting = false;
                trail.Clear();
            }

            var visual = new GameObject("Nanda_Ilustrada");
            visual.SetActive(false);
            visual.transform.SetParent(guide.transform, false);

            Renderer = visual.AddComponent<SpriteRenderer>();
            Renderer.sprite = nandaFrames[0];
            Renderer.sortingOrder = 110;

            Flight = guide.gameObject.AddComponent<NandaFlightIllustration>();
            Flight.painting = Renderer;
            Flight.visualRoot = visual.transform;
            Flight.frames = nandaFrames;
            visual.SetActive(true);

            var lampGo = new GameObject("Nanda_Luz_Azul");
            lampGo.transform.SetParent(guide.transform, false);
            var lampBlue = lampGo.AddComponent<Light>();
            lampBlue.type = LightType.Point;

            var trailGo = new GameObject("Nanda_Trilha_Azul");
            trailGo.transform.SetParent(guide.transform, false);
            var ribbon = trailGo.AddComponent<TrailRenderer>();
            ribbon.sharedMaterial = spriteMaterial;

            accompaniment = guide.gameObject.AddComponent<NandaAccompaniment>();
            accompaniment.guide = guide;
            accompaniment.player = player;
            accompaniment.flight = Flight;
            accompaniment.blueLight = lampBlue;
            accompaniment.blueTrail = ribbon;

            // Awake precedes assignment on an active guide; re-enable to initialize trail settings.
            accompaniment.enabled = false;
            ConfigureTrail(ribbon);
            accompaniment.enabled = true;
            accompaniment.ResetPresentation();
        }

        public void PositionAtStart(NandaFairyGuide guide, Transform playerTransform)
        {
            guide.SetYuna(playerTransform);
            guide.ReturnToYuna();
            guide.transform.position = playerTransform.position + new Vector3(1.65f, 1.4f, -0.15f);
            accompaniment?.ResetPresentation();
        }

        private static void ConfigureTrail(TrailRenderer ribbon)
        {
            ribbon.time = 0.32f;
            ribbon.minVertexDistance = 0.04f;
            ribbon.widthCurve = AnimationCurve.Linear(0f, 0.028f, 1f, 0f);
            ribbon.widthMultiplier = 1f;
            ribbon.sortingOrder = 109;
            ribbon.numCapVertices = 3;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.34f, 0.63f, 1f), 0f),
                    new GradientColorKey(new Color(0.25f, 0.48f, 0.92f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.24f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            ribbon.colorGradient = gradient;
        }
    }
}
