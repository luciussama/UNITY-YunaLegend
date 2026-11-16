using UnityEngine;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Ator visual 2.5D da abertura: monta a cama, o cobertor e Yuna (deitada -> sentada)
    /// em camadas independentes animadas a 30 fps e integradas à penumbra noturna (PenumbraLightingT054).
    /// </summary>
    public sealed class OpeningBedActor : MonoBehaviour
    {
        private const float BedPixelsPerUnit = 310f;
        private const int BedSortingOrder = 17;
        private const int SleepingSortingOrder = 19;
        private const int BlanketOverSortingOrder = 20;
        private const int BlanketUnderSortingOrder = 18;
        private const int SittingSortingOrder = 21;

        private SpriteRenderer bedRenderer;
        private SpriteRenderer blanketRenderer;
        private SpriteRenderer sleepingRenderer;
        private SpriteRenderer sittingRenderer;

        private Vector3 anchorWorld;
        private bool built;

        public bool IsBuilt => built;

        public void Initialize(Vector3 yunaFloorWorld, PenumbraLightingT054 lighting)
        {
            anchorWorld = yunaFloorWorld + new Vector3(0.22f, -0.04f, 0.06f);
            transform.position = anchorWorld;

            if (built)
            {
                return;
            }

            Texture2D bedTex = Resources.Load<Texture2D>("Opening/Yuna_Bed_Frame_v1");
            Texture2D blanketTex = Resources.Load<Texture2D>("Opening/Yuna_Bed_Blanket_v1");
            Texture2D sleepTex = Resources.Load<Texture2D>("Opening/Yuna_Sleeping_Pose_v1");
            Texture2D sitTex = Resources.Load<Texture2D>("Opening/Yuna_Sitting_Pose_v1");

            if (bedTex == null || blanketTex == null || sleepTex == null || sitTex == null)
            {
                Debug.LogWarning("[OpeningBedActor] Texturas da cama/abertura não encontradas em Resources/Opening/.");
                return;
            }

            bedRenderer = CreateLayer("BedFrame", bedTex, new Vector2(0.5f, 0.08f), BedSortingOrder, Vector3.zero, 1f);
            sleepingRenderer = CreateLayer("YunaSleeping", sleepTex, new Vector2(0.5f, 0.45f), SleepingSortingOrder, new Vector3(-0.30f, 1.08f, -0.02f), 0.52f);
            sleepingRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -6f);
            blanketRenderer = CreateLayer("BedBlanket", blanketTex, new Vector2(0.5f, 0.25f), BlanketOverSortingOrder, new Vector3(0.42f, 0.40f, -0.03f), 0.60f);
            sittingRenderer = CreateLayer("YunaSitting", sitTex, new Vector2(0.5f, 0.10f), SittingSortingOrder, new Vector3(0.05f, 0.42f, -0.04f), 0.46f);

            if (lighting != null)
            {
                BindToPenumbra(lighting, bedRenderer);
                BindToPenumbra(lighting, sleepingRenderer);
                BindToPenumbra(lighting, blanketRenderer);
                BindToPenumbra(lighting, sittingRenderer);
            }

            built = true;
            SetVisible(false);
        }

        public void ApplySample(OpeningCinematicTimeline.Sample sample)
        {
            if (!built)
            {
                return;
            }

            if (!sample.YunaInBed)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            float t = sample.QuantizedTime;
            float sit = Mathf.Clamp01(sample.BedSitProgress);
            float exit = Mathf.Clamp01(sample.BedExitProgress);

            // Respiração suave na cama (30 fps).
            float breath = Mathf.Sin(t * 2.1f);
            float sleepFade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((sit - 0.15f) / 0.40f));
            float sitFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((sit - 0.15f) / 0.45f));

            // Yuna dormindo -> esboçando levantar (cabeça sempre descoberta no travesseiro).
            if (sleepingRenderer != null)
            {
                sleepingRenderer.enabled = sleepFade > 0.01f;
                sleepingRenderer.transform.localPosition = new Vector3(
                    -0.30f + sit * 0.12f,
                    1.08f + breath * 0.012f + sit * 0.16f,
                    -0.02f);
                sleepingRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -6f + breath * 1.1f - sit * 16f);
                sleepingRenderer.transform.localScale = new Vector3(0.52f, 0.52f * (1f + breath * 0.018f), 1f);
                SetAlpha(sleepingRenderer, sleepFade);
            }

            // Cobertor cobre apenas do peito para baixo e dobra/desliza para a direita ao sentar.
            if (blanketRenderer != null)
            {
                float slide = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(sit * 1.15f));
                blanketRenderer.sortingOrder = sit > 0.35f ? BlanketUnderSortingOrder : BlanketOverSortingOrder;
                blanketRenderer.transform.localPosition = new Vector3(
                    0.42f + slide * 0.35f,
                    0.40f - slide * 0.05f + breath * 0.008f * (1f - slide),
                    -0.03f);
                blanketRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -slide * 6f);
                blanketRenderer.transform.localScale = new Vector3(
                    0.60f * (1f - slide * 0.24f),
                    0.60f * (1f - slide * 0.12f),
                    1f);
            }

            // Yuna sentada na beira da cama -> descendo para o chão.
            if (sittingRenderer != null)
            {
                sittingRenderer.enabled = sitFade > 0.01f;
                float hop = Mathf.Sin(exit * Mathf.PI) * 0.09f;
                sittingRenderer.transform.localPosition = new Vector3(
                    Mathf.Lerp(0.05f, -0.18f, exit),
                    Mathf.Lerp(0.42f, 0.02f, exit) + hop,
                    -0.03f);
                float leanTilt = (1f - sit) * 10f - Mathf.Sin(exit * Mathf.PI) * 5f;
                sittingRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, leanTilt);
                SetAlpha(sittingRenderer, sitFade);
            }
        }

        public void SetVisible(bool visible)
        {
            if (!built)
            {
                return;
            }

            if (bedRenderer != null) bedRenderer.enabled = visible;
            if (blanketRenderer != null) blanketRenderer.enabled = visible;
            if (sleepingRenderer != null && !visible) sleepingRenderer.enabled = false;
            if (sittingRenderer != null && !visible) sittingRenderer.enabled = false;
        }

        private SpriteRenderer CreateLayer(
            string layerName,
            Texture2D texture,
            Vector2 pivot,
            int sortingOrder,
            Vector3 localPos,
            float uniformScale)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                pivot,
                BedPixelsPerUnit,
                0,
                SpriteMeshType.FullRect);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        private static void BindToPenumbra(PenumbraLightingT054 lighting, SpriteRenderer sr)
        {
            if (sr == null)
            {
                return;
            }

            try
            {
                lighting.Bind(sr, role: 2, includeDisabled: true, suppressHaze: true);
            }
            catch
            {
                // Se shader/room não estiver pronto, mantém sprite padrão sem abortar a cena.
            }
        }

        private static void SetAlpha(SpriteRenderer sr, float alpha)
        {
            Color c = sr.color;
            c.a = Mathf.Clamp01(alpha);
            sr.color = c;
        }
    }
}
