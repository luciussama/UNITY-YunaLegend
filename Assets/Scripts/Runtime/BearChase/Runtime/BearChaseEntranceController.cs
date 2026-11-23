using UnityEngine;
using UnityEngine.UI;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Adaptador uGUI nativo da cinematica ilustrada 2D de entrada:
    /// replica composicao, sprites migrados e andamento do legado, preservando o urso dourado-mel
    /// com laco vermelho e a fadinha Nanda azul-ciano com halo azulado.
    /// </summary>
    public sealed class BearChaseEntranceController : MonoBehaviour
    {
        private const string ResourceRoot = "BearChaseEntrance/";
        private const float FairyLeftWingSplitRatio = 0.24f;
        private const float FairyRightWingSplitRatio = 0.77f;
        // Centers of the grounded shoe soles, measured on the 1774 px approach sheet.
        private static readonly float[] ApproachSoleX = { 312f, 674f, 1202f, 1535f };

        private Canvas canvas;
        private RectTransform stageRect;
        private RectTransform yunaRect;
        private Image yunaImage;
        private RectTransform yunaShadowRect;
        private Image yunaShadowImage;
        private RectTransform yunaContactShadowRect;
        private Image yunaContactShadowImage;
        private RectTransform bearRect;
        private Image bearImage;
        private RectTransform bearShadowRect;
        private Image bearShadowImage;
        private RectTransform bearContactShadowRect;
        private Image bearContactShadowImage;
        private RectTransform fairyRect;
        private RectTransform fairyBodyRect;
        private RectTransform fairyLeftWingRect;
        private RectTransform fairyRightWingRect;
        private RectTransform fairyGlowRect;
        private Image fairyBodyImage;
        private Image fairyLeftWingImage;
        private Image fairyRightWingImage;
        private Image fadeOverlay;

        private readonly struct AlignedSpriteFrame
        {
            public readonly Sprite Sprite;
            public readonly float WidthPx;
            public readonly float HeightPx;
            public readonly float AnchorNormX;
            public readonly float UiScale;

            public AlignedSpriteFrame(Sprite sprite, float widthPx, float heightPx, float anchorNormX, float uiScale)
            {
                Sprite = sprite;
                WidthPx = widthPx;
                HeightPx = heightPx;
                AnchorNormX = anchorNormX;
                UiScale = uiScale;
            }
        }

        private readonly AlignedSpriteFrame[] approachGirlFrames = new AlignedSpriteFrame[4];
        private readonly AlignedSpriteFrame[] approachBearFrames = new AlignedSpriteFrame[4];
        private readonly AlignedSpriteFrame[] reachGirlFrames = new AlignedSpriteFrame[4];
        private Sprite fairyBodySprite;
        private Sprite fairyLeftWingSprite;
        private Sprite fairyRightWingSprite;
        private Sprite softCircleSprite;
        private Texture2D softCircleTexture;
        private Sprite verticalFadeSprite;
        private Texture2D verticalFadeTexture;

        private bool built;

        public void EnsureBuilt()
        {
            if (built)
            {
                return;
            }

            built = true;
            softCircleSprite = CreateRadialSoftSprite(128, out softCircleTexture);
            verticalFadeSprite = CreateVerticalFadeSprite(64, out verticalFadeTexture);
            LoadSlicedSprites();
            BuildCanvasHierarchy();
        }

        private void OnDestroy()
        {
            DestroyFrameArray(approachGirlFrames);
            DestroyFrameArray(approachBearFrames);
            DestroyFrameArray(reachGirlFrames);

            DestroyObjectSafe(fairyBodySprite);
            fairyBodySprite = null;

            DestroyObjectSafe(fairyLeftWingSprite);
            fairyLeftWingSprite = null;

            DestroyObjectSafe(fairyRightWingSprite);
            fairyRightWingSprite = null;

            DestroyObjectSafe(softCircleSprite);
            softCircleSprite = null;

            DestroyObjectSafe(softCircleTexture);
            softCircleTexture = null;

            DestroyObjectSafe(verticalFadeSprite);
            verticalFadeSprite = null;

            DestroyObjectSafe(verticalFadeTexture);
            verticalFadeTexture = null;
        }

        private static void DestroyFrameArray(AlignedSpriteFrame[] frames)
        {
            if (frames == null)
            {
                return;
            }

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i].Sprite != null)
                {
                    DestroyObjectSafe(frames[i].Sprite);
                    frames[i] = default;
                }
            }
        }

        private static void DestroyObjectSafe(Object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

        public void SetVisible(bool visible)
        {
            EnsureBuilt();
            if (canvas != null && canvas.gameObject.activeSelf != visible)
            {
                canvas.gameObject.SetActive(visible);
            }
        }

        public void SetCaptureCamera(Camera cam)
        {
            EnsureBuilt();
            if (canvas == null)
            {
                return;
            }

            if (cam != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 1.0f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
            }

            Canvas.ForceUpdateCanvases();
        }

        public void ApplySample(BearChaseEntranceSample sample, bool isEntranceActive)
        {
            SetVisible(isEntranceActive);
            if (!isEntranceActive || stageRect == null)
            {
                return;
            }

            const float floorNormY = 0.245f;

            // Yuna (caminhada suave com pivo de cabeca/tronco continuo ou alcance ancorado nos pes)
            if (yunaRect != null && yunaImage != null)
            {
                AlignedSpriteFrame[] activeSet = sample.UseReachAtlas ? reachGirlFrames : approachGirlFrames;
                int idx = Mathf.Clamp(sample.GirlFrameIndex, 0, activeSet.Length - 1);
                AlignedSpriteFrame frame = activeSet[idx];
                if (frame.Sprite != null)
                {
                    yunaImage.sprite = frame.Sprite;
                    yunaImage.preserveAspect = false;
                }

                float girlW = frame.WidthPx > 0f ? frame.WidthPx * frame.UiScale : (sample.UseReachAtlas ? 350f : 420f);
                float girlH = frame.HeightPx > 0f ? frame.HeightPx * frame.UiScale : 395f;
                float pivotX = frame.WidthPx > 0f ? frame.AnchorNormX : 0.5f;

                float supportX = sample.NormalizedYunaX;
                bool isGroundedStep = sample.Beat == BearChaseEntranceBeat.Approach
                    && sample.StepCueNumber > 0
                    && supportX < BearChaseEntranceTimeline.YunaReachX
                    && frame.Sprite != null;
                if (isGroundedStep)
                {
                    Rect spriteRect = frame.Sprite.rect;
                    float soleX = ApproachSoleX[idx] * frame.Sprite.texture.width / 1774f;
                    pivotX = Mathf.Clamp01((soleX - spriteRect.x) / spriteRect.width);
                    supportX = BearChaseEntranceTimeline.ApproachSupportX(sample.StepCueNumber);
                }

                var yAnchor = new Vector2(supportX, floorNormY + sample.YunaWalkBobY);
                yunaRect.anchorMin = yAnchor;
                yunaRect.anchorMax = yAnchor;
                yunaRect.pivot = new Vector2(pivotX, 0f);
                yunaRect.anchoredPosition = Vector2.zero;
                yunaRect.sizeDelta = new Vector2(girlW, girlH);
                yunaRect.localScale = new Vector3(sample.YunaScaleX, sample.YunaScaleY, 1f);
                yunaRect.localRotation = Quaternion.Euler(0f, 0f, sample.YunaTiltDeg);

                float yunaAirFactor = Mathf.Clamp01(sample.YunaWalkBobY / 0.036f);
                bool isWideStride = !sample.UseReachAtlas && (idx == 0 || idx == 2);
                float baseShadowW = sample.UseReachAtlas ? 235f : (isWideStride ? 228f : 178f);

                if (yunaShadowRect != null)
                {
                    float shrink = 1f - yunaAirFactor * 0.24f;
                    var sAnchor = new Vector2(sample.NormalizedYunaX, floorNormY - 0.002f);
                    yunaShadowRect.anchorMin = sAnchor;
                    yunaShadowRect.anchorMax = sAnchor;
                    yunaShadowRect.anchoredPosition = Vector2.zero;
                    yunaShadowRect.sizeDelta = new Vector2(baseShadowW * shrink, 42f * shrink);
                    if (yunaShadowImage != null)
                    {
                        yunaShadowImage.color = new Color(0.05f, 0.025f, 0.015f, Mathf.Lerp(0.56f, 0.34f, yunaAirFactor));
                    }
                }

                if (yunaContactShadowRect != null)
                {
                    float coreShrink = 1f - yunaAirFactor * 0.38f;
                    var cAnchor = new Vector2(supportX, floorNormY - 0.001f);
                    yunaContactShadowRect.anchorMin = cAnchor;
                    yunaContactShadowRect.anchorMax = cAnchor;
                    yunaContactShadowRect.anchoredPosition = Vector2.zero;
                    yunaContactShadowRect.sizeDelta = new Vector2(baseShadowW * 0.72f * coreShrink, 20f * coreShrink);
                    if (yunaContactShadowImage != null)
                    {
                        yunaContactShadowImage.color = new Color(0.03f, 0.015f, 0.01f, Mathf.Lerp(0.72f, 0.14f, yunaAirFactor));
                    }
                }
            }

            // Urso dourado-mel caido/dormindo (espelhado com cabeca a esquerda) e despertando em sobressalto
            if (bearRect != null && bearImage != null)
            {
                int bIdx = Mathf.Clamp(sample.BearFrameIndex, 0, approachBearFrames.Length - 1);
                AlignedSpriteFrame bearFrame = approachBearFrames[bIdx];
                if (bearFrame.Sprite != null)
                {
                    bearImage.sprite = bearFrame.Sprite;
                    bearImage.preserveAspect = false;
                }

                float bearW = bearFrame.WidthPx > 0f ? bearFrame.WidthPx * bearFrame.UiScale : 450f;
                float bearH = bearFrame.HeightPx > 0f ? bearFrame.HeightPx * bearFrame.UiScale : 340f;
                float bearPivotX = bearFrame.WidthPx > 0f ? bearFrame.AnchorNormX : 0.5f;

                var bAnchor = new Vector2(sample.NormalizedBearX, floorNormY - 0.008f + sample.BearStartleOffsetY);
                bearRect.anchorMin = bAnchor;
                bearRect.anchorMax = bAnchor;
                bearRect.pivot = new Vector2(bearPivotX, 0f);
                bearRect.anchoredPosition = Vector2.zero;
                bearRect.sizeDelta = new Vector2(bearW, bearH);
                float bearSignX = sample.BearFlipX ? -1f : 1f;
                bearRect.localScale = new Vector3(bearSignX * sample.BearScaleX, sample.BearScaleY, 1f);
                bearRect.localRotation = Quaternion.Euler(0f, 0f, sample.BearTiltDeg);

                float bearAirFactor = Mathf.Clamp01(sample.BearStartleOffsetY / 0.028f);
                float bearShadowW = bIdx == 0 ? 430f : (bIdx == 1 ? 395f : (bIdx == 2 ? 355f : 390f));
                float bearShadowH = bIdx == 3 ? 66f : (bIdx == 2 ? 56f : 52f);
                float bearShadowY = bIdx == 3 ? (floorNormY + 0.008f) : (bIdx == 2 ? (floorNormY + 0.002f) : (floorNormY - 0.006f));

                if (bearShadowRect != null)
                {
                    float shadowShrink = 1f - bearAirFactor * 0.18f;
                    var bsAnchor = new Vector2(sample.NormalizedBearX, bearShadowY);
                    bearShadowRect.anchorMin = bsAnchor;
                    bearShadowRect.anchorMax = bsAnchor;
                    bearShadowRect.anchoredPosition = Vector2.zero;
                    bearShadowRect.sizeDelta = new Vector2(bearShadowW * shadowShrink, bearShadowH * shadowShrink);
                    if (bearShadowImage != null)
                    {
                        float baseAlpha = bIdx >= 2 ? 0.64f : 0.58f;
                        bearShadowImage.color = new Color(0.04f, 0.02f, 0.01f, Mathf.Lerp(baseAlpha, 0.38f, bearAirFactor));
                    }
                }

                if (bearContactShadowRect != null)
                {
                    float coreShrink = 1f - bearAirFactor * 0.34f;
                    var bcAnchor = new Vector2(sample.NormalizedBearX, floorNormY - 0.006f);
                    bearContactShadowRect.anchorMin = bcAnchor;
                    bearContactShadowRect.anchorMax = bcAnchor;
                    bearContactShadowRect.anchoredPosition = Vector2.zero;
                    bearContactShadowRect.sizeDelta = new Vector2(bearShadowW * 0.78f * coreShrink, 26f * coreShrink);
                    if (bearContactShadowImage != null)
                    {
                        bearContactShadowImage.color = new Color(0.025f, 0.012f, 0.008f, Mathf.Lerp(0.74f, 0.18f, bearAirFactor));
                    }
                }
            }

            // Nanda azul-ciano acompanhando continuamente com asas articuladas sem deformar rosto/corpo
            if (fairyRect != null)
            {
                float fy = sample.NormalizedFairyY + sample.FairyBobOffset;
                var fAnchor = new Vector2(sample.NormalizedFairyX, fy);
                const float fairyW = 118f;
                const float fairyH = 128f;
                float leftWingW = fairyW * FairyLeftWingSplitRatio;
                float bodyW = fairyW * (FairyRightWingSplitRatio - FairyLeftWingSplitRatio);
                float rightWingW = fairyW * (1f - FairyRightWingSplitRatio);
                float bodyHalfW = bodyW * 0.5f;
                float beat = Mathf.Clamp(sample.FairyWingScaleX, 0.35f, 1.05f);

                fairyRect.anchorMin = fAnchor;
                fairyRect.anchorMax = fAnchor;
                fairyRect.anchoredPosition = Vector2.zero;
                fairyRect.sizeDelta = new Vector2(fairyW, fairyH);
                fairyRect.localScale = Vector3.one;
                fairyRect.localRotation = Quaternion.Euler(0f, 0f, sample.FairyBobOffset * -240f);

                if (fairyBodyRect != null)
                {
                    fairyBodyRect.anchoredPosition = Vector2.zero;
                    fairyBodyRect.sizeDelta = new Vector2(bodyW, fairyH);
                }

                if (fairyLeftWingRect != null)
                {
                    fairyLeftWingRect.anchoredPosition = new Vector2(-bodyHalfW + 1f, 0f);
                    fairyLeftWingRect.sizeDelta = new Vector2(leftWingW * beat, fairyH);
                }

                if (fairyRightWingRect != null)
                {
                    fairyRightWingRect.anchoredPosition = new Vector2(bodyHalfW - 1f, 0f);
                    fairyRightWingRect.sizeDelta = new Vector2(rightWingW * beat, fairyH);
                }

                if (fairyGlowRect != null)
                {
                    float pulse = 1f + Mathf.Abs(sample.FairyBobOffset) * 8f;
                    fairyGlowRect.anchorMin = fAnchor;
                    fairyGlowRect.anchorMax = fAnchor;
                    fairyGlowRect.anchoredPosition = Vector2.zero;
                    fairyGlowRect.sizeDelta = new Vector2(265f * pulse, 265f * pulse);
                }
            }

            if (fadeOverlay != null)
            {
                fadeOverlay.color = new Color(0.03f, 0.02f, 0.02f, Mathf.Clamp01(sample.FadeAlpha));
            }
        }

        private void LoadSlicedSprites()
        {
            Texture2D approachTex = Resources.Load<Texture2D>(ResourceRoot + "approach-atlas-adapted-v1")
                ?? Resources.Load<Texture2D>(ResourceRoot + "approach-atlas-v1");
            if (approachTex != null)
            {
                float sx = approachTex.width / 1774f;
                float sy = approachTex.height / 887f;
                int[] girlCols = { Mathf.RoundToInt(90 * sx), Mathf.RoundToInt(445 * sx), Mathf.RoundToInt(882 * sx), Mathf.RoundToInt(1332 * sx), Mathf.RoundToInt(1690 * sx) };
                int[] bearCols = { Mathf.RoundToInt(35 * sx), Mathf.RoundToInt(485 * sx), Mathf.RoundToInt(922 * sx), Mathf.RoundToInt(1360 * sx), Mathf.RoundToInt(1740 * sx) };
                int girlStartY = Mathf.RoundToInt(398 * sy);
                int girlHeight = approachTex.height - girlStartY;
                int bearHeight = Mathf.RoundToInt(374 * sy);

                // Pivo estritamente em cabeca/tronco na caminhada elimina oscilacao lateral de f3 (+-1.7 px max)
                BuildAlignedRow(
                    approachTex,
                    girlCols,
                    girlStartY,
                    girlHeight,
                    primaryBandMin: 0.55f,
                    primaryBandMax: 0.92f,
                    secondaryBandMin: 0.00f,
                    secondaryBandMax: 0.12f,
                    secondaryWeight: 0f,
                    targetTallHeightUi: 395f,
                    approachGirlFrames);

                BuildAlignedRow(
                    approachTex,
                    bearCols,
                    0,
                    bearHeight,
                    primaryBandMin: 0.00f,
                    primaryBandMax: 0.24f,
                    secondaryBandMin: 0.00f,
                    secondaryBandMax: 0.24f,
                    secondaryWeight: 0f,
                    targetTallHeightUi: 350f,
                    approachBearFrames);
            }

            Texture2D reachTex = Resources.Load<Texture2D>(ResourceRoot + "reach-atlas-v1");
            if (reachTex != null)
            {
                float rxScale = reachTex.width / 2172f;
                int[] reachCols = { Mathf.RoundToInt(70 * rxScale), Mathf.RoundToInt(545 * rxScale), Mathf.RoundToInt(1099 * rxScale), Mathf.RoundToInt(1693 * rxScale), Mathf.RoundToInt(2120 * rxScale) };

                // Ancora firmemente nos pes plantados no tapete enquanto tronco e braco avancam ate o urso e recuam no susto
                BuildAlignedRow(
                    reachTex,
                    reachCols,
                    0,
                    reachTex.height,
                    primaryBandMin: 0.00f,
                    primaryBandMax: 0.12f,
                    secondaryBandMin: 0.00f,
                    secondaryBandMax: 0.12f,
                    secondaryWeight: 0f,
                    targetTallHeightUi: 395f,
                    reachGirlFrames);
            }

            Texture2D fairyTex = Resources.Load<Texture2D>(ResourceRoot + "illustrated-fairy-v1");
            if (fairyTex != null)
            {
                float w = fairyTex.width;
                float h = fairyTex.height;
                float left = Mathf.Round(w * FairyLeftWingSplitRatio);
                float right = Mathf.Round(w * FairyRightWingSplitRatio);

                fairyLeftWingSprite = Sprite.Create(
                    fairyTex,
                    new Rect(0f, 0f, left, h),
                    new Vector2(1f, 0.5f),
                    100f);
                fairyBodySprite = Sprite.Create(
                    fairyTex,
                    new Rect(left, 0f, right - left, h),
                    new Vector2(0.5f, 0.5f),
                    100f);
                fairyRightWingSprite = Sprite.Create(
                    fairyTex,
                    new Rect(right, 0f, w - right, h),
                    new Vector2(0f, 0.5f),
                    100f);
            }
        }

        private static void BuildAlignedRow(
            Texture2D tex,
            int[] cols,
            int bandStartY,
            int bandHeight,
            float primaryBandMin,
            float primaryBandMax,
            float secondaryBandMin,
            float secondaryBandMax,
            float secondaryWeight,
            float targetTallHeightUi,
            AlignedSpriteFrame[] dest)
        {
            int count = Mathf.Min(dest.Length, cols.Length - 1);
            var tightRects = new Rect[count];
            var anchorsX = new float[count];
            float maxTightH = 1f;

            Color32[] pixels = tex.isReadable ? tex.GetPixels32() : null;
            int texW = tex.width;

            for (int col = 0; col < count; col++)
            {
                int x0 = Mathf.Clamp(cols[col], 0, texW - 1);
                int x1 = Mathf.Clamp(cols[col + 1], x0 + 1, texW);
                int y0 = Mathf.Clamp(bandStartY, 0, tex.height - 1);
                int y1 = Mathf.Clamp(bandStartY + bandHeight, y0 + 1, tex.height);

                if (pixels == null)
                {
                    int fw = x1 - x0;
                    int fh = y1 - y0;
                    tightRects[col] = new Rect(x0, y0, fw, fh);
                    anchorsX[col] = 0.5f;
                    maxTightH = Mathf.Max(maxTightH, fh);
                    continue;
                }

                int minX = x1;
                int maxX = x0;
                int minY = y1;
                int maxY = y0;

                for (int y = y0; y < y1; y++)
                {
                    int rowBase = y * texW;
                    for (int x = x0; x < x1; x++)
                    {
                        if (pixels[rowBase + x].a > 24)
                        {
                            minX = Mathf.Min(minX, x);
                            maxX = Mathf.Max(maxX, x);
                            minY = Mathf.Min(minY, y);
                            maxY = Mathf.Max(maxY, y);
                        }
                    }
                }

                if (maxX <= minX || maxY <= minY)
                {
                    tightRects[col] = new Rect(x0, y0, x1 - x0, y1 - y0);
                    anchorsX[col] = 0.5f;
                    maxTightH = Mathf.Max(maxTightH, y1 - y0);
                    continue;
                }

                int tw = maxX - minX + 1;
                int th = maxY - minY + 1;

                float primaryCenterX = ComputeBandCenterX(pixels, texW, minX, maxX, minY, th, primaryBandMin, primaryBandMax);
                float centerX = primaryCenterX;
                if (secondaryWeight > 0.001f)
                {
                    float secondaryCenterX = ComputeBandCenterX(pixels, texW, minX, maxX, minY, th, secondaryBandMin, secondaryBandMax);
                    centerX = Mathf.Lerp(primaryCenterX, secondaryCenterX, Mathf.Clamp01(secondaryWeight));
                }

                anchorsX[col] = Mathf.Clamp((centerX - minX) / tw, 0.25f, 0.75f);
                tightRects[col] = new Rect(minX, minY, tw, th);
                maxTightH = Mathf.Max(maxTightH, th);
            }

            float uniformScale = targetTallHeightUi / maxTightH;
            for (int col = 0; col < count; col++)
            {
                Rect r = tightRects[col];
                Sprite sp = Sprite.Create(tex, r, new Vector2(anchorsX[col], 0f), 100f);
                dest[col] = new AlignedSpriteFrame(sp, r.width, r.height, anchorsX[col], uniformScale);
            }
        }

        private static float ComputeBandCenterX(
            Color32[] pixels,
            int texW,
            int minX,
            int maxX,
            int minY,
            int tightH,
            float bandMin,
            float bandMax)
        {
            int ayStart = minY + Mathf.Clamp(Mathf.FloorToInt(tightH * bandMin), 0, tightH - 1);
            int ayEnd = minY + Mathf.Clamp(Mathf.CeilToInt(tightH * bandMax), 1, tightH);

            double sumX = 0.0;
            long weightSum = 0;
            for (int y = ayStart; y < ayEnd; y++)
            {
                int rowBase = y * texW;
                for (int x = minX; x <= maxX; x++)
                {
                    byte alpha = pixels[rowBase + x].a;
                    if (alpha > 24)
                    {
                        sumX += (double)x * alpha;
                        weightSum += alpha;
                    }
                }
            }

            return weightSum > 0
                ? (float)(sumX / weightSum)
                : (minX + maxX) * 0.5f;
        }

        private void BuildCanvasHierarchy()
        {
            var canvasGo = new GameObject("BearChase_EntranceCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var rootRt = canvasGo.GetComponent<RectTransform>();
            var backdrop = rootRt.gameObject.AddComponent<Image>();
            backdrop.color = new Color(0.05f, 0.04f, 0.04f, 1f);
            backdrop.raycastTarget = false;

            stageRect = CreateRect("EntranceStage", rootRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            BuildRoomBackdrop(stageRect);
            BuildWindowAndSunbeam(stageRect);
            BuildFloorAndRug(stageRect);
            BuildActors(stageRect);
            BuildFadeOverlay(stageRect);
        }

        private void BuildRoomBackdrop(RectTransform parent)
        {
            Texture2D wallTex = Resources.Load<Texture2D>(ResourceRoot + "wallpaper-v1");
            var wallRt = CreateRect("Wallpaper", parent, new Vector2(0f, 0.34f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var wallRaw = wallRt.gameObject.AddComponent<RawImage>();
            wallRaw.texture = wallTex;
            wallRaw.uvRect = new Rect(0f, 0f, 4.6f, 2.2f);
            wallRaw.color = new Color(0.92f, 0.87f, 0.78f, 1f);
            wallRaw.raycastTarget = false;

            Texture2D trimTex = Resources.Load<Texture2D>(ResourceRoot + "oak-trim-v1");
            var trimRt = CreateRect("OakTrim", parent, new Vector2(0f, 0.30f), new Vector2(1f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var trimRaw = trimRt.gameObject.AddComponent<RawImage>();
            trimRaw.texture = trimTex;
            trimRaw.uvRect = new Rect(0f, 0f, 3.8f, 1f);
            trimRaw.color = new Color(0.94f, 0.88f, 0.80f, 1f);
            trimRaw.raycastTarget = false;
        }

        private void BuildWindowAndSunbeam(RectTransform parent)
        {
            Texture2D winTex = Resources.Load<Texture2D>(ResourceRoot + "window-curtains-v1");
            if (winTex != null)
            {
                var winRt = CreateRect("Window", parent, new Vector2(0.675f, 0.68f), new Vector2(0.675f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(415f, 440f));
                var winImg = winRt.gameObject.AddComponent<Image>();
                winImg.sprite = Sprite.Create(winTex, new Rect(0, 0, winTex.width, winTex.height), new Vector2(0.5f, 0.5f));
                winImg.preserveAspect = true;
                winImg.raycastTarget = false;
            }

            var beamRt = CreateRect("WindowSunGlow", parent, new Vector2(0.675f, 0.46f), new Vector2(0.675f, 0.46f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 540f));
            var beamImg = beamRt.gameObject.AddComponent<Image>();
            beamImg.sprite = softCircleSprite;
            beamImg.color = new Color(1.0f, 0.90f, 0.62f, 0.24f);
            beamImg.raycastTarget = false;

            // Feixe diagonal suave de luz matinal da janela incidindo sobre o tapete e o urso
            var shaftRt = CreateRect("WindowSunbeamShaft", parent, new Vector2(0.625f, 0.44f), new Vector2(0.625f, 0.44f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 760f));
            shaftRt.localRotation = Quaternion.Euler(0f, 0f, -24f);
            var shaftImg = shaftRt.gameObject.AddComponent<Image>();
            shaftImg.sprite = softCircleSprite;
            shaftImg.color = new Color(1.0f, 0.94f, 0.74f, 0.17f);
            shaftImg.raycastTarget = false;
        }

        private void BuildFloorAndRug(RectTransform parent)
        {
            Texture2D floorTex = Resources.Load<Texture2D>(ResourceRoot + "entrance-floor-v1");
            var floorRt = CreateRect("WoodFloor", parent, new Vector2(0f, 0f), new Vector2(1f, 0.345f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var floorRaw = floorRt.gameObject.AddComponent<RawImage>();
            floorRaw.texture = floorTex;
            floorRaw.uvRect = new Rect(0f, 0f, 2.8f, 1.0f);
            floorRaw.color = new Color(0.95f, 0.89f, 0.80f, 1f);
            floorRaw.raycastTarget = false;

            // Sombra suave em gradiente vertical sob o rodape (sem corte retangular duro sobre o assoalho)
            var wallShadeRt = CreateRect("BaseboardFloorShadow", parent, new Vector2(0f, 0.285f), new Vector2(1f, 0.348f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var wallShade = wallShadeRt.gameObject.AddComponent<Image>();
            wallShade.sprite = verticalFadeSprite;
            wallShade.color = new Color(0.11f, 0.05f, 0.02f, 0.42f);
            wallShade.raycastTarget = false;

            Texture2D rugTex = Resources.Load<Texture2D>(ResourceRoot + "rectangular-rug-v1");
            if (rugTex != null)
            {
                var rugRt = CreateRect("VintageRug", parent, new Vector2(0.545f, 0.215f), new Vector2(0.545f, 0.215f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1140f, 255f));
                var rugImg = rugRt.gameObject.AddComponent<Image>();
                rugImg.sprite = Sprite.Create(rugTex, new Rect(0, 0, rugTex.width, rugTex.height), new Vector2(0.5f, 0.5f));
                rugImg.raycastTarget = false;
            }

            // Poca quente de sol da janela sobre o tapete e o urso dormindo para destacar o foco da cena
            var sunPoolRt = CreateRect("WindowFloorSunPool", parent, new Vector2(0.655f, 0.240f), new Vector2(0.655f, 0.240f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 185f));
            var sunPoolImg = sunPoolRt.gameObject.AddComponent<Image>();
            sunPoolImg.sprite = softCircleSprite;
            sunPoolImg.color = new Color(1.0f, 0.93f, 0.70f, 0.22f);
            sunPoolImg.raycastTarget = false;
        }

        private void BuildActors(RectTransform parent)
        {
            yunaShadowRect = CreateRect("YunaShadow", parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(215f, 42f));
            yunaShadowImage = yunaShadowRect.gameObject.AddComponent<Image>();
            yunaShadowImage.sprite = softCircleSprite;
            yunaShadowImage.color = new Color(0.05f, 0.025f, 0.015f, 0.56f);
            yunaShadowImage.raycastTarget = false;

            yunaContactShadowRect = CreateRect("YunaContactShadow", parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(155f, 20f));
            yunaContactShadowImage = yunaContactShadowRect.gameObject.AddComponent<Image>();
            yunaContactShadowImage.sprite = softCircleSprite;
            yunaContactShadowImage.color = new Color(0.03f, 0.015f, 0.01f, 0.72f);
            yunaContactShadowImage.raycastTarget = false;

            bearShadowRect = CreateRect("BearShadow", parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(410f, 54f));
            bearShadowImage = bearShadowRect.gameObject.AddComponent<Image>();
            bearShadowImage.sprite = softCircleSprite;
            bearShadowImage.color = new Color(0.04f, 0.02f, 0.01f, 0.60f);
            bearShadowImage.raycastTarget = false;

            bearContactShadowRect = CreateRect("BearContactShadow", parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 26f));
            bearContactShadowImage = bearContactShadowRect.gameObject.AddComponent<Image>();
            bearContactShadowImage.sprite = softCircleSprite;
            bearContactShadowImage.color = new Color(0.025f, 0.012f, 0.008f, 0.74f);
            bearContactShadowImage.raycastTarget = false;

            bearRect = CreateRect("BearActor", parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(450f, 350f));
            bearImage = bearRect.gameObject.AddComponent<Image>();
            bearImage.preserveAspect = false;
            bearImage.raycastTarget = false;

            yunaRect = CreateRect("YunaActor", parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(280f, 395f));
            yunaImage = yunaRect.gameObject.AddComponent<Image>();
            yunaImage.preserveAspect = false;
            yunaImage.raycastTarget = false;

            fairyGlowRect = CreateRect("FairyBlueAura", parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(265f, 265f));
            var glowImg = fairyGlowRect.gameObject.AddComponent<Image>();
            glowImg.sprite = softCircleSprite;
            glowImg.color = new Color(0.24f, 0.84f, 1.0f, 0.44f);
            glowImg.raycastTarget = false;

            fairyRect = CreateRect("FairyActor", parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(118f, 128f));

            fairyLeftWingRect = CreateRect("FairyLeftWing", fairyRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(28f, 128f));
            fairyLeftWingImage = fairyLeftWingRect.gameObject.AddComponent<Image>();
            fairyLeftWingImage.sprite = fairyLeftWingSprite;
            fairyLeftWingImage.preserveAspect = false;
            fairyLeftWingImage.raycastTarget = false;

            fairyRightWingRect = CreateRect("FairyRightWing", fairyRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(28f, 128f));
            fairyRightWingImage = fairyRightWingRect.gameObject.AddComponent<Image>();
            fairyRightWingImage.sprite = fairyRightWingSprite;
            fairyRightWingImage.preserveAspect = false;
            fairyRightWingImage.raycastTarget = false;

            fairyBodyRect = CreateRect("FairyBody", fairyRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(62f, 128f));
            fairyBodyImage = fairyBodyRect.gameObject.AddComponent<Image>();
            fairyBodyImage.sprite = fairyBodySprite;
            fairyBodyImage.preserveAspect = false;
            fairyBodyImage.raycastTarget = false;
        }

        private void BuildFadeOverlay(RectTransform parent)
        {
            var fadeRt = CreateRect("EntranceFadeOverlay", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            fadeOverlay = fadeRt.gameObject.AddComponent<Image>();
            fadeOverlay.color = new Color(0.03f, 0.02f, 0.02f, 0f);
            fadeOverlay.raycastTarget = false;
        }

        private static Sprite CreateVerticalFadeSprite(int height, out Texture2D tex)
        {
            tex = new Texture2D(2, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[2 * height];

            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                float a = t * t;
                byte alpha = (byte)Mathf.RoundToInt(a * 255f);
                var c = new Color32(255, 255, 255, alpha);
                pixels[y * 2] = c;
                pixels[y * 2 + 1] = c;
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateRadialSoftSprite(int size, out Texture2D tex)
        {
            tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - r);
                    a = a * a * (3f - 2f * a);
                    byte alpha = (byte)Mathf.RoundToInt(a * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPos,
            Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return rt;
        }
    }
}
