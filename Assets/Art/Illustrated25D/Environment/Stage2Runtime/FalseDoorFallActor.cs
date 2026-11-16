using UnityEngine;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Ator visual 2.5D da Porta Falsa: renderiza Yuna caindo sentada, pousando sentada no chão
    /// e se levantando/arrumando o vestido com movimentos suaves a 30 fps na penumbra.
    /// </summary>
    public sealed class FalseDoorFallActor : MonoBehaviour
    {
        private const float SeatedPixelsPerUnit = 560f;
        private const float GettingUpPixelsPerUnit = 590f;
        private const float DressPixelsPerUnit = 680f;
        private const int ActorSortingOrder = 22;

        private SpriteRenderer seatedRenderer;
        private SpriteRenderer gettingUpRenderer;
        private SpriteRenderer dressRenderer;
        private bool built;

        public void Initialize(PenumbraLightingT054 lighting)
        {
            if (built)
            {
                return;
            }

            Texture2D seatedTex = Resources.Load<Texture2D>("Opening/Yuna_Fall_Seated_v1");
            Texture2D gettingUpTex = Resources.Load<Texture2D>("Opening/Yuna_Getting_Up_v1");
            Texture2D dressTex = Resources.Load<Texture2D>("Opening/Yuna_Adjust_Dress_v1");
            if (seatedTex == null || gettingUpTex == null || dressTex == null)
            {
                Debug.LogWarning("[FalseDoorFallActor] Sprites da queda sentada não encontrados em Resources/Opening/.");
                return;
            }

            seatedRenderer = CreateLayer("YunaFallSeated", seatedTex, SeatedPixelsPerUnit, new Vector2(0.5f, 0.08f));
            gettingUpRenderer = CreateLayer("YunaGettingUp", gettingUpTex, GettingUpPixelsPerUnit, new Vector2(0.5f, 0.05f));
            dressRenderer = CreateLayer("YunaAdjustDress", dressTex, DressPixelsPerUnit, new Vector2(0.5f, 0.05f));

            if (lighting != null)
            {
                BindToPenumbra(lighting, seatedRenderer);
                BindToPenumbra(lighting, gettingUpRenderer);
                BindToPenumbra(lighting, dressRenderer);
            }

            built = true;
            SetVisible(false);
        }

        public void ApplySample(Vector3 playerWorldPos, FalseDoorCinematicTimeline.Sample sample)
        {
            if (!built)
            {
                return;
            }

            if (!sample.UseFallSeatedActor)
            {
                SetVisible(false);
                return;
            }

            transform.position = playerWorldPos + new Vector3(0f, sample.ActorOffsetY, -0.02f);
            transform.localRotation = Quaternion.Euler(0f, 0f, sample.ActorTiltDeg);
            transform.localScale = new Vector3(sample.YunaSquashX, sample.YunaSquashY, 1f);

            if (seatedRenderer != null)
            {
                bool showSeated = sample.SeatedPoseAlpha > 0.01f;
                seatedRenderer.enabled = showSeated;
                if (showSeated)
                {
                    SetAlpha(seatedRenderer, sample.SeatedPoseAlpha);
                }
            }

            if (gettingUpRenderer != null)
            {
                bool showGetUp = sample.GettingUpPoseAlpha > 0.01f;
                gettingUpRenderer.enabled = showGetUp;
                if (showGetUp)
                {
                    SetAlpha(gettingUpRenderer, sample.GettingUpPoseAlpha);
                }
            }

            if (dressRenderer != null)
            {
                bool showDress = sample.AdjustDressAlpha > 0.01f;
                dressRenderer.enabled = showDress;
                if (showDress)
                {
                    SetAlpha(dressRenderer, sample.AdjustDressAlpha);
                }
            }
        }

        public void SetVisible(bool visible)
        {
            if (!built)
            {
                return;
            }

            if (!visible)
            {
                if (seatedRenderer != null) seatedRenderer.enabled = false;
                if (gettingUpRenderer != null) gettingUpRenderer.enabled = false;
                if (dressRenderer != null) dressRenderer.enabled = false;
            }
        }

        private SpriteRenderer CreateLayer(string layerName, Texture2D texture, float ppu, Vector2 pivot)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);
            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                pivot,
                ppu,
                0,
                SpriteMeshType.FullRect);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = ActorSortingOrder;
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
                // Mantém sprite padrão se shader/room não estiver pronto.
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
