using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Adaptador MonoBehaviour (Humble Object) que anima o Castelo de Cartas Bamboleante (Plataforma 30):
    /// - Enquanto Yuna pisa nas cartas: a estrutura bamboleia sutilmente.
    /// - Logo apos Yuna pular da plataforma 30: desmonta as 15 cartas de baralho inteiras e individuais
    ///   (sem recortes retangulares brutos nem fundo branco preso), fazendo-as deslizar, girar e pousar no chao.
    /// - Reseta de forma limpa no retry.
    /// </summary>
    public sealed class CardCastleCollapseView : MonoBehaviour
    {
        private IllustratedPlatformView platformView;
        private Yuna25DPlayerController player;
        private SpriteRenderer mainPainting;
        private BoxCollider topCollider;

        private GameObject tiersRoot;
        private readonly Transform[] tierTransforms = new Transform[CardCastleCollapseTimeline.TierCount];
        private readonly SpriteRenderer[] tierRenderers = new SpriteRenderer[CardCastleCollapseTimeline.TierCount];
        private readonly Vector3[] tierBaseLocalPositions = new Vector3[CardCastleCollapseTimeline.TierCount];

        private readonly Transform[] flutterTransforms = new Transform[CardCastleCollapseTimeline.IndividualCardCount];
        private readonly SpriteRenderer[] flutterRenderers = new SpriteRenderer[CardCastleCollapseTimeline.IndividualCardCount];
        private readonly Sprite[] generatedCardSprites = new Sprite[CardCastleCollapseTimeline.IndividualCardCount];
        private readonly Texture2D[] generatedCardTextures = new Texture2D[CardCastleCollapseTimeline.IndividualCardCount];

        private AudioSource collapseAudioSource;
        private AudioClip collapseCascadeClip;

        private bool playerHasSteppedOnCastle;
        private bool isCollapsing;
        private bool collapseCompleted;
        private float steppedElapsed;
        private float collapseElapsed;

        public bool? MobilePresentationOverride { get; set; }
        private bool IsMobilePresentation => MobilePresentationOverride ?? Application.isMobilePlatform;
        private Material mobilePenumbraMaterial;
        private readonly Dictionary<SpriteRenderer, Material> mobileOriginalMaterials = new Dictionary<SpriteRenderer, Material>();

        private void LateUpdate()
        {
            ApplyMobilePenumbra();
        }

        public void ApplyMobilePenumbra()
        {
            if (!IsMobilePresentation)
            {
                RestoreMobileMaterials();
                return;
            }
            if (mobilePenumbraMaterial == null)
            {
                var shader = Shader.Find("YunaLegend/RoomPenumbraT054");
                if (shader == null)
                {
                    return;
                }
                mobilePenumbraMaterial = new Material(shader) { name = "Mobile_CardScenery_Penumbra" };
                mobilePenumbraMaterial.SetFloat("_Sprite", 1f);
                mobilePenumbraMaterial.SetFloat("_SceneryPenumbra", 1f);
                mobilePenumbraMaterial.SetFloat("_ZWrite", 0f);
            }
            UseMobileMaterial(mainPainting);
            foreach (var renderer in tierRenderers)
            {
                UseMobileMaterial(renderer);
            }
            foreach (var renderer in flutterRenderers)
            {
                UseMobileMaterial(renderer);
            }
        }

        private void UseMobileMaterial(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sharedMaterial == mobilePenumbraMaterial)
            {
                return;
            }
            // Lighting installation happens after platform installation; retain its latest material for desktop.
            mobileOriginalMaterials[renderer] = renderer.sharedMaterial;
            renderer.sharedMaterial = mobilePenumbraMaterial;
        }

        private void RestoreMobileMaterials()
        {
            foreach (var pair in mobileOriginalMaterials)
            {
                if (pair.Key != null && pair.Key.sharedMaterial == mobilePenumbraMaterial)
                {
                    pair.Key.sharedMaterial = pair.Value;
                }
            }
            mobileOriginalMaterials.Clear();
        }

        public bool PlayerHasSteppedOnCastle => playerHasSteppedOnCastle;
        public bool IsCollapsing => isCollapsing;
        public bool CollapseCompleted => collapseCompleted;
        public float CollapseElapsed => collapseElapsed;

        public void Install(
            IllustratedPlatformView view,
            Yuna25DPlayerController yunaPlayer,
            Texture2D castleTexture,
            IllustratedPlatformSpec spec)
        {
            platformView = view;
            player = yunaPlayer;
            mainPainting = view != null ? view.Painting : null;
            topCollider = view != null ? view.Top : null;

            BuildShatterTiersAndIndividualCards(castleTexture, spec);
            BuildCollapseAudio();
            ResetCollapseState();
        }

        private void OnDestroy()
        {
            RestoreMobileMaterials();
            if (mobilePenumbraMaterial != null)
            {
                if (Application.isPlaying) Destroy(mobilePenumbraMaterial);
                else DestroyImmediate(mobilePenumbraMaterial);
            }
            for (int i = 0; i < CardCastleCollapseTimeline.IndividualCardCount; i++)
            {
                if (generatedCardSprites[i] != null) Destroy(generatedCardSprites[i]);
                if (generatedCardTextures[i] != null) Destroy(generatedCardTextures[i]);
            }
        }

        public void ResetCollapseState()
        {
            playerHasSteppedOnCastle = false;
            isCollapsing = false;
            collapseCompleted = false;
            steppedElapsed = 0f;
            collapseElapsed = 0f;

            if (mainPainting != null)
            {
                mainPainting.enabled = true;
            }

            if (topCollider != null)
            {
                topCollider.enabled = true;
            }

            if (tiersRoot != null)
            {
                tiersRoot.SetActive(false);
            }

            for (int i = 0; i < CardCastleCollapseTimeline.TierCount; i++)
            {
                if (tierTransforms[i] != null)
                {
                    tierTransforms[i].localPosition = tierBaseLocalPositions[i];
                    tierTransforms[i].localRotation = Quaternion.identity;
                }

                if (tierRenderers[i] != null)
                {
                    tierRenderers[i].enabled = true;
                    tierRenderers[i].color = Color.white;
                }
            }

            for (int i = 0; i < CardCastleCollapseTimeline.IndividualCardCount; i++)
            {
                if (flutterRenderers[i] != null)
                {
                    flutterRenderers[i].enabled = false;
                }
            }
        }

        public void TriggerCollapseNow()
        {
            if (isCollapsing || collapseCompleted)
            {
                return;
            }

            playerHasSteppedOnCastle = true;
            isCollapsing = true;
            collapseElapsed = 0f;

            if (mainPainting != null)
            {
                mainPainting.enabled = false;
            }

            if (topCollider != null)
            {
                topCollider.enabled = false;
            }

            if (tiersRoot != null)
            {
                tiersRoot.SetActive(true);
            }

            // Oculta as 4 fatias compostas e exibe exclusivamente as 15 cartas inteiras individuais
            for (int i = 0; i < CardCastleCollapseTimeline.TierCount; i++)
            {
                if (tierRenderers[i] != null)
                {
                    tierRenderers[i].enabled = false;
                }
            }

            if (collapseAudioSource != null && collapseCascadeClip != null)
            {
                collapseAudioSource.PlayOneShot(collapseCascadeClip, 0.10f);
            }

            EvaluateAtCollapseTime(0f);
        }

        public void EvaluateAtCollapseTime(float timeSeconds)
        {
            if (tiersRoot != null && !tiersRoot.activeSelf)
            {
                tiersRoot.SetActive(true);
            }

            if (mainPainting != null)
            {
                mainPainting.enabled = false;
            }

            for (int i = 0; i < CardCastleCollapseTimeline.TierCount; i++)
            {
                if (tierRenderers[i] != null)
                {
                    tierRenderers[i].enabled = false;
                }
            }

            collapseElapsed = Mathf.Max(0f, timeSeconds);

            for (int i = 0; i < CardCastleCollapseTimeline.IndividualCardCount; i++)
            {
                if (flutterTransforms[i] == null || flutterRenderers[i] == null)
                {
                    continue;
                }

                CardCastleFlutterCardSample card = CardCastleCollapseTimeline.EvaluateFlutterCard(i, collapseElapsed);
                bool show = card.Alpha > 0.01f;
                flutterRenderers[i].enabled = show;
                if (show)
                {
                    Color baseTint = mainPainting != null ? mainPainting.color : Color.white;
                    flutterTransforms[i].localPosition = new Vector3(card.LocalXMetres, card.LocalYMetres, 0.08f + (0.01f * (i % 5)));
                    flutterTransforms[i].localRotation = Quaternion.Euler(0f, 0f, card.RotationDegrees);
                    flutterTransforms[i].localScale = new Vector3(card.ScaleX, 1f, 1f);
                    flutterRenderers[i].color = new Color(
                        baseTint.r * 0.26f,
                        baseTint.g * 0.26f,
                        baseTint.b * 0.32f,
                        baseTint.a * card.Alpha * 0.36f);
                }
            }
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            if ((playerHasSteppedOnCastle || isCollapsing || collapseCompleted)
                && (player.CurrentSupportIndex < 0 || (player.CurrentSupportIndex >= 22 && player.CurrentSupportIndex < CardCastleCollapseTimeline.PlatformIndex)))
            {
                ResetCollapseState();
                return;
            }

            if (isCollapsing)
            {
                collapseElapsed += Time.deltaTime;
                EvaluateAtCollapseTime(collapseElapsed);
                if (collapseElapsed >= CardCastleCollapseTimeline.CollapseDurationSeconds)
                {
                    isCollapsing = false;
                    collapseCompleted = true;
                    if (tiersRoot != null)
                    {
                        tiersRoot.SetActive(false);
                    }
                }
                return;
            }

            if (collapseCompleted)
            {
                return;
            }

            bool onCardCastle = player.CurrentSupportIndex == CardCastleCollapseTimeline.PlatformIndex && !player.IsDead;
            if (onCardCastle && player.IsGrounded)
            {
                if (!playerHasSteppedOnCastle)
                {
                    playerHasSteppedOnCastle = true;
                    steppedElapsed = 0f;
                    if (mainPainting != null)
                    {
                        mainPainting.enabled = false;
                    }

                    if (tiersRoot != null)
                    {
                        tiersRoot.SetActive(true);
                    }
                }

                steppedElapsed += Time.deltaTime;
                ApplySteppedWobble(steppedElapsed);
                return;
            }

            if (playerHasSteppedOnCastle
                && (!player.IsGrounded || player.Velocity.y > 0.2f || player.CurrentSupportIndex > CardCastleCollapseTimeline.PlatformIndex))
            {
                TriggerCollapseNow();
            }
        }

        private void ApplySteppedWobble(float timeSeconds)
        {
            for (int i = 0; i < CardCastleCollapseTimeline.TierCount; i++)
            {
                if (tierTransforms[i] == null)
                {
                    continue;
                }

                if (tierRenderers[i] != null)
                {
                    tierRenderers[i].enabled = true;
                }

                CardCastleTierSample sample = CardCastleCollapseTimeline.EvaluateSteppedWobble(i, timeSeconds);
                tierTransforms[i].localPosition = tierBaseLocalPositions[i] + new Vector3(sample.OffsetXMetres, sample.OffsetYMetres, 0f);
                tierTransforms[i].localRotation = Quaternion.Euler(0f, 0f, sample.RotationDegrees);
            }
        }

        private void BuildShatterTiersAndIndividualCards(Texture2D texture, IllustratedPlatformSpec spec)
        {
            if (texture == null)
            {
                return;
            }

            tiersRoot = new GameObject("Castelo_Cartas_30_Camadas_Desmoronamento");
            tiersRoot.transform.SetParent(transform, false);
            tiersRoot.transform.position = new Vector3(spec.targetX, spec.floorY, 0f);

            float ppm = spec.PixelsPerMetre;
            var mat = mainPainting != null && mainPainting.sharedMaterial != null
                ? mainPainting.sharedMaterial
                : new Material(Shader.Find("Sprites/Default"));

            // 1) 4 andares completos usados apenas enquanto a menina pisa no castelo (bamboleio sutil sem separar)
            for (int i = 0; i < CardCastleCollapseTimeline.TierCount; i++)
            {
                int topPx = CardCastleCollapseTimeline.TierTopPixelY[i];
                int bottomPx = CardCastleCollapseTimeline.TierBottomPixelY[i];
                int sliceHeightPx = bottomPx - topPx;
                int unityBottomY = texture.height - bottomPx;

                float centerPixelY = (topPx + bottomPx) * 0.5f;
                float worldOffsetY = (spec.feetPixelY - centerPixelY) / ppm;

                var tierGo = new GameObject($"CardTier_{i}");
                tierGo.transform.SetParent(tiersRoot.transform, false);
                tierBaseLocalPositions[i] = new Vector3(0f, worldOffsetY, 0f);
                tierGo.transform.localPosition = tierBaseLocalPositions[i];

                var sprite = Sprite.Create(
                    texture,
                    new Rect(0f, unityBottomY, texture.width, sliceHeightPx),
                    new Vector2(spec.centrePixelX / texture.width, 0.5f),
                    ppm,
                    0,
                    SpriteMeshType.FullRect);

                var sr = tierGo.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sharedMaterial = mat;
                sr.sortingOrder = i;

                tierTransforms[i] = tierGo.transform;
                tierRenderers[i] = sr;
            }

            // 2) 15 cartas de baralho inteiras e individuais (silhueta arredondada limpa, zero fundo branco externo)
            for (int c = 0; c < CardCastleCollapseTimeline.IndividualCardCount; c++)
            {
                CardCastleCollapseTimeline.InitialCardLayoutSpec cardSpec = CardCastleCollapseTimeline.Cards[c];
                var cardGo = new GameObject($"IndividualPlayingCard_{c:00}_{cardSpec.RankText}");
                cardGo.transform.SetParent(tiersRoot.transform, false);

                Texture2D cardTex = CreateIllustratedPlayingCardTexture(cardSpec, c);
                generatedCardTextures[c] = cardTex;

                // Escala fiel as cartas do castelo (~0.92m x 1.38m para cartas verticais; ~1.85m x 0.42m para pontes em perspectiva)
                float cardPpu = cardSpec.IsHorizontalBridge ? 145f : 185f;
                Sprite cardSprite = Sprite.Create(
                    cardTex,
                    new Rect(0f, 0f, cardTex.width, cardTex.height),
                    new Vector2(0.5f, 0.5f),
                    cardPpu,
                    0,
                    SpriteMeshType.FullRect);
                generatedCardSprites[c] = cardSprite;

                var sr = cardGo.AddComponent<SpriteRenderer>();
                sr.sprite = cardSprite;
                sr.sharedMaterial = mat;
                sr.sortingOrder = -6;
                sr.enabled = false;

                flutterTransforms[c] = cardGo.transform;
                flutterRenderers[c] = sr;
            }

            tiersRoot.SetActive(false);
        }

        private static Texture2D CreateIllustratedPlayingCardTexture(
            CardCastleCollapseTimeline.InitialCardLayoutSpec spec,
            int seedIndex)
        {
            int w = spec.IsHorizontalBridge ? 280 : 180;
            int h = spec.IsHorizontalBridge ? 76 : 264;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[w * h];
            float halfW = w * 0.5f;
            float halfH = h * 0.5f;
            float cornerRadius = spec.IsHorizontalBridge ? 11f : 14f;
            float boxW = halfW - cornerRadius - 2f;
            float boxH = halfH - cornerRadius - 2f;

            // Paleta profundamente escurecida para passar despercebida na penumbra do Quarto Escuro
            Color parchmentLight = new Color(0.20f, 0.19f, 0.23f, 1f);
            Color parchmentWarm = new Color(0.14f, 0.13f, 0.17f, 1f);
            Color edgeStroke = new Color(0.06f, 0.06f, 0.09f, 1f);
            Color suitColor = spec.IsRedSuit
                ? new Color(0.22f, 0.08f, 0.09f, 1f)
                : new Color(0.07f, 0.09f, 0.14f, 1f);
            Color goldAccent = new Color(0.22f, 0.18f, 0.10f, 1f);

            for (int y = 0; y < h; y++)
            {
                float ny = (y + 0.5f - halfH) / halfH;
                float py = Mathf.Abs((y + 0.5f) - halfH);
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + 0.5f - halfW) / halfW;
                    float px = Mathf.Abs((x + 0.5f) - halfW);

                    float qx = Mathf.Max(px - boxW, 0f);
                    float qy = Mathf.Max(py - boxH, 0f);
                    float dist = Mathf.Sqrt((qx * qx) + (qy * qy)) + Mathf.Min(Mathf.Max(px - boxW, py - boxH), 0f) - cornerRadius;

                    float alpha = Mathf.Clamp01(0.6f - dist);
                    if (alpha <= 0f)
                    {
                        pixels[(y * w) + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    // Textura de papel aquarelado com sombreamento suave nas bordas
                    float grain = Mathf.Sin((x + seedIndex * 17) * 0.18f) * Mathf.Cos((y + seedIndex * 11) * 0.16f) * 0.025f;
                    float vignette = Mathf.Clamp01((nx * nx * 0.45f) + (ny * ny * 0.45f));
                    Color col = Color.Lerp(parchmentLight, parchmentWarm, vignette + grain);

                    // Borda externa desenhada a bico de pena (2.5px)
                    float strokeBlend = Mathf.Clamp01((-dist - 2.6f) + 0.5f);
                    col = Color.Lerp(edgeStroke, col, strokeBlend);

                    if (!spec.IsHorizontalBridge)
                    {
                        // Moldura interna fina
                        bool inInnerBorder = (Mathf.Abs(nx) > 0.78f && Mathf.Abs(nx) < 0.81f && Mathf.Abs(ny) < 0.84f)
                            || (Mathf.Abs(ny) > 0.82f && Mathf.Abs(ny) < 0.85f && Mathf.Abs(nx) < 0.80f);
                        if (inInnerBorder)
                        {
                            col = Color.Lerp(col, edgeStroke, 0.35f);
                        }

                        // Indices de canto (superior esquerdo e inferior direito)
                        bool topLeftPip = nx < -0.58f && nx > -0.78f && ny > 0.56f && ny < 0.80f;
                        bool bottomRightPip = nx > 0.58f && nx < 0.78f && ny < -0.56f && ny > -0.80f;
                        if (topLeftPip || bottomRightPip)
                        {
                            float cx = topLeftPip ? -0.68f : 0.68f;
                            float cy = topLeftPip ? 0.68f : -0.68f;
                            if (EvaluateSuitShape(nx - cx, ny - cy, 0.08f, spec.IsRedSuit))
                            {
                                col = suitColor;
                            }
                        }

                        if (spec.IsCourtCard)
                        {
                            // Retrato real ilustrado (Rei K♠ / Dama Q♥ com coroa dourada, rosto ilustrado e manto carmim/marinho, fiel a 30-card-castle-v1.png)
                            if (Mathf.Abs(nx) < 0.56f && Mathf.Abs(ny) < 0.64f)
                            {
                                float ay = Mathf.Abs(ny);
                                float ax = Mathf.Abs(nx);
                                Color courtCol = new Color(0.16f, 0.21f, 0.31f, 1f); // Fundo marinho da moldura real

                                // Moldura dourada interna do retrato
                                if (ax > 0.50f || ay > 0.58f)
                                {
                                    courtCol = goldAccent;
                                }
                                // Coroa dourada com pontas (ay entre 0.42 e 0.54, ax < 0.32)
                                else if (ay > 0.42f && ay <= 0.54f && ax < 0.32f)
                                {
                                    float crownSpikes = 0.48f + 0.06f * Mathf.Cos(nx * 24f);
                                    courtCol = ay <= crownSpikes ? goldAccent : courtCol;
                                }
                                // Rosto oval em pergaminho claro com cabelos/barba desenhados (ay entre 0.20 e 0.42)
                                else if (ay > 0.20f && ay <= 0.42f && ax < 0.36f)
                                {
                                    float faceDist = Mathf.Sqrt((nx * nx * 8.5f) + ((ay - 0.31f) * (ay - 0.31f) * 14f));
                                    if (faceDist < 0.68f)
                                    {
                                        courtCol = parchmentLight;
                                        // Olhos e traços delicados a bico de pena
                                        if (Mathf.Abs(ay - 0.34f) < 0.025f && (Mathf.Abs(ax - 0.09f) < 0.035f))
                                        {
                                            courtCol = edgeStroke;
                                        }
                                    }
                                    else if (faceDist < 0.98f)
                                    {
                                        courtCol = spec.IsRedSuit ? new Color(0.24f, 0.22f, 0.26f, 1f) : new Color(0.76f, 0.72f, 0.64f, 1f);
                                    }
                                }
                                // Manto real carmim com estola dourada e bordados verticais (ay <= 0.22)
                                else if (ay <= 0.22f && ax < 0.46f)
                                {
                                    bool goldSash = Mathf.Abs(ax - 0.14f) < 0.065f || Mathf.Abs(nx + ny * 0.6f) < 0.08f;
                                    courtCol = goldSash
                                        ? goldAccent
                                        : new Color(0.76f, 0.21f, 0.19f, 1f);
                                }

                                col = Color.Lerp(col, courtCol, 0.92f);
                            }
                        }
                        else
                        {
                            // Naipes centrais desenhados (Coracao ou Espada)
                            if (EvaluateSuitShape(nx, ny, 0.24f, spec.IsRedSuit))
                            {
                                col = suitColor;
                            }
                            if (spec.RankText != "A")
                            {
                                if (EvaluateSuitShape(nx - 0.34f, ny - 0.42f, 0.13f, spec.IsRedSuit)
                                    || EvaluateSuitShape(nx + 0.34f, ny + 0.42f, 0.13f, spec.IsRedSuit)
                                    || EvaluateSuitShape(nx - 0.34f, ny + 0.42f, 0.13f, spec.IsRedSuit)
                                    || EvaluateSuitShape(nx + 0.34f, ny - 0.42f, 0.13f, spec.IsRedSuit))
                                {
                                    col = suitColor;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Carta horizontal vista em perspectiva rasa (com naipe nos cantos)
                        if (EvaluateSuitShape(nx + 0.64f, ny * 0.45f, 0.14f, spec.IsRedSuit)
                            || EvaluateSuitShape(nx - 0.64f, ny * 0.45f, 0.14f, spec.IsRedSuit))
                        {
                            col = suitColor;
                        }
                    }

                    col.a = alpha;
                    pixels[(y * w) + x] = (Color32)col;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        private static bool EvaluateSuitShape(float dx, float dy, float radius, bool isHeart)
        {
            float x = dx / Mathf.Max(0.001f, radius);
            float y = dy / Mathf.Max(0.001f, radius);
            if (!isHeart)
            {
                // Espada (coracao invertido + haste curta)
                y = -y;
                if (y < -0.55f && y > -1.05f && Mathf.Abs(x) < 0.22f)
                {
                    return true;
                }
            }

            // Formula implicita do coracao: (x^2 + y^2 - 1)^3 - x^2 * y^3 <= 0
            float sx = x * 1.15f;
            float sy = (y * 1.15f) + 0.15f;
            float a = (sx * sx) + (sy * sy) - 1f;
            return (a * a * a) - (sx * sx * sy * sy * sy) <= 0f;
        }

        private void BuildCollapseAudio()
        {
            collapseAudioSource = gameObject.GetComponent<AudioSource>();
            if (collapseAudioSource == null)
            {
                collapseAudioSource = gameObject.AddComponent<AudioSource>();
            }

            collapseAudioSource.playOnAwake = false;
            collapseAudioSource.spatialBlend = 0f;

            const int sr = 22050;
            const float duration = 0.85f;
            int totalSamples = Mathf.RoundToInt(sr * duration);
            float[] data = new float[totalSamples];

            int cardSnapCount = CardCastleCollapseTimeline.IndividualCardCount;
            for (int c = 0; c < cardSnapCount; c++)
            {
                float startSec = c * 0.042f;
                int startIdx = Mathf.RoundToInt(startSec * sr);
                var stepSpec = DarkRoomSurfaceSoundCatalog.EvaluateStepSpec(
                    CardCastleCollapseTimeline.PlatformIndex,
                    c,
                    isLanding: c == 0);
                float[] snapSamples = DarkRoomSurfaceSoundCatalog.SynthesizeSamples(stepSpec, sr);
                float gain = Mathf.Lerp(0.85f, 0.26f, c / (float)cardSnapCount);

                for (int i = 0; i < snapSamples.Length && (startIdx + i) < totalSamples; i++)
                {
                    data[startIdx + i] = Mathf.Clamp(data[startIdx + i] + (snapSamples[i] * gain), -0.95f, 0.95f);
                }
            }

            collapseCascadeClip = AudioClip.Create("SFX_CardCastleCollapse", totalSamples, 1, sr, false);
            collapseCascadeClip.SetData(data, 0);
        }
    }
}
