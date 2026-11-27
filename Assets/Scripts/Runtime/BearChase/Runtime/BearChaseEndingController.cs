using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using YunaLegend.BearChase.Core;
using YunaLegend.Qa.Runtime;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Adaptador uGUI da cena 2D ilustrada de finalizacao do Capitulo 1 ("Um Quarto Inteiro"),
    /// fiel as 5 batidas do legado com adaptacoes cosmeticas (Nanda azul-ciano e Yuna canonica).
    /// </summary>
    public sealed class BearChaseEndingController : MonoBehaviour
    {
        private const string EndingResourceRoot = "BearChaseEnding/";
        private const string EntranceFallbackRoot = "BearChaseEntrance/";

        private Canvas canvas;
        private RectTransform zoomContainer;
        private Image roomBackgroundImage;
        private RectTransform yunaRect;
        private Image yunaImage;
        private RectTransform yunaShadowRect;
        private RectTransform nandaGlowRect;
        private RectTransform nandaRect;
        private Image nandaImage;

        private GameObject dialogueBoxRoot;
        private Text speakerHeaderText;
        private Text dialogueBodyText;
        private Image fadeOverlay;

        private Sprite roomBgSprite;
        private readonly Sprite[] yunaPoseSprites = new Sprite[3];
        private readonly Sprite[] nandaPoseSprites = new Sprite[2];
        private Sprite softCircleSprite;
        private Texture2D softCircleTexture;

        private bool built;
        private bool isActive;
        private float elapsed;
        private BearChaseEndingBeat lastLoggedBeat = BearChaseEndingBeat.Completed;

        public bool IsActive => isActive;
        public event Action OnEndingCompleted;

        public void BeginEnding()
        {
            EnsureBuilt();
            isActive = true;
            elapsed = 0f;
            lastLoggedBeat = BearChaseEndingBeat.Completed;

            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }

            WebQaRuntime.LogEvent("bearchase_ending_started", "scene=UmQuartoInteiro");
            ApplySample(BearChaseEndingTimeline.Evaluate(0f));
        }

        public void StopEnding()
        {
            isActive = false;
            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
        }

        public void TryAdvanceBeat()
        {
            if (!isActive)
            {
                return;
            }

            elapsed = BearChaseEndingTimeline.GetNextAdvanceTime(elapsed);
            ApplySample(BearChaseEndingTimeline.Evaluate(elapsed));
        }

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            if (WasAdvanceInputPressed())
            {
                TryAdvanceBeat();
                return;
            }

            elapsed += Mathf.Min(0.1f, Time.deltaTime);
            BearChaseEndingSample sample = BearChaseEndingTimeline.Evaluate(elapsed);
            ApplySample(sample);

            if (sample.IsCompleted)
            {
                isActive = false;
                WebQaRuntime.LogEvent("bearchase_ending_completed", $"duration={elapsed:F2}");
                OnEndingCompleted?.Invoke();
            }
        }

        private void OnDestroy()
        {
            DestroySafe(roomBgSprite);
            for (int i = 0; i < yunaPoseSprites.Length; i++)
            {
                DestroySafe(yunaPoseSprites[i]);
            }

            for (int i = 0; i < nandaPoseSprites.Length; i++)
            {
                DestroySafe(nandaPoseSprites[i]);
            }

            DestroySafe(softCircleSprite);
            DestroySafe(softCircleTexture);
        }

        private void ApplySample(BearChaseEndingSample sample)
        {
            if (sample.Beat != lastLoggedBeat)
            {
                lastLoggedBeat = sample.Beat;
                WebQaRuntime.LogEvent(
                    "bearchase_ending_beat",
                    $"beat={sample.Beat} yuna={sample.YunaPose} nanda={sample.NandaPose} zoom={sample.CameraZoom:F2}");
            }

            if (zoomContainer != null)
            {
                float z = sample.CameraZoom;
                zoomContainer.localScale = new Vector3(z, z, 1f);
            }

            if (yunaImage != null)
            {
                int yIdx = Mathf.Clamp((int)sample.YunaPose, 0, yunaPoseSprites.Length - 1);
                if (yunaPoseSprites[yIdx] != null)
                {
                    yunaImage.sprite = yunaPoseSprites[yIdx];
                }
            }

            if (nandaImage != null && nandaRect != null)
            {
                int nIdx = Mathf.Clamp((int)sample.NandaPose, 0, nandaPoseSprites.Length - 1);
                if (nandaPoseSprites[nIdx] != null)
                {
                    nandaImage.sprite = nandaPoseSprites[nIdx];
                }

                var nAnchor = new Vector2(0.585f, 0.435f + sample.NandaBobY);
                nandaRect.anchorMin = nAnchor;
                nandaRect.anchorMax = nAnchor;
                if (nandaGlowRect != null)
                {
                    nandaGlowRect.anchorMin = nAnchor;
                    nandaGlowRect.anchorMax = nAnchor;
                }
            }

            if (dialogueBoxRoot != null)
            {
                dialogueBoxRoot.SetActive(sample.ShowDialogue);
                if (sample.ShowDialogue)
                {
                    if (speakerHeaderText != null)
                    {
                        speakerHeaderText.text = sample.SpeakerHeader;
                    }

                    if (dialogueBodyText != null)
                    {
                        dialogueBodyText.text = sample.DialogueText;
                    }
                }
            }

            if (fadeOverlay != null)
            {
                fadeOverlay.color = new Color(0.03f, 0.02f, 0.02f, Mathf.Clamp01(sample.FadeAlpha));
            }
        }

        private bool WasAdvanceInputPressed()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
            {
                return true;
            }

            Pointer pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        private void EnsureBuilt()
        {
            if (built)
            {
                return;
            }

            built = true;
            softCircleSprite = CreateRadialSoftSprite(128, out softCircleTexture);
            LoadAssets();
            BuildCanvas();
        }

        private void LoadAssets()
        {
            Texture2D bgTex = Resources.Load<Texture2D>(EndingResourceRoot + "ending-room-bg-v1");
            if (bgTex != null)
            {
                roomBgSprite = Sprite.Create(bgTex, new Rect(0f, 0f, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f);
            }

            Texture2D yunaTex = Resources.Load<Texture2D>(EndingResourceRoot + "ending-yuna-poses-v1");
            if (yunaTex != null)
            {
                float cellW = yunaTex.width / 3f;
                float cellH = yunaTex.height;
                for (int i = 0; i < 3; i++)
                {
                    yunaPoseSprites[i] = Sprite.Create(
                        yunaTex,
                        new Rect(i * cellW, 0f, cellW, cellH),
                        new Vector2(0.5f, 0f),
                        100f);
                }
            }
            else
            {
                Texture2D fallbackYuna = Resources.Load<Texture2D>(EntranceFallbackRoot + "yuna-approach-walk-atlas-v2");
                if (fallbackYuna != null)
                {
                    float cellW = fallbackYuna.width / 4f;
                    float cellH = fallbackYuna.height / 2f;
                    Sprite sp = Sprite.Create(fallbackYuna, new Rect(3f * cellW, 0f, cellW, cellH), new Vector2(0.5f, 0f), 100f);
                    for (int i = 0; i < 3; i++)
                    {
                        yunaPoseSprites[i] = sp;
                    }
                }
            }

            Texture2D nandaTex = Resources.Load<Texture2D>(EndingResourceRoot + "ending-nanda-blue-poses-v1");
            if (nandaTex != null)
            {
                float cellW = nandaTex.width / 2f;
                float cellH = nandaTex.height;
                for (int i = 0; i < 2; i++)
                {
                    nandaPoseSprites[i] = Sprite.Create(
                        nandaTex,
                        new Rect(i * cellW, 0f, cellW, cellH),
                        new Vector2(0.5f, 0.5f),
                        100f);
                }
            }
            else
            {
                Texture2D fallbackNanda = Resources.Load<Texture2D>(EntranceFallbackRoot + "illustrated-fairy-v1");
                if (fallbackNanda != null)
                {
                    Sprite sp = Sprite.Create(fallbackNanda, new Rect(0f, 0f, fallbackNanda.width, fallbackNanda.height), new Vector2(0.5f, 0.5f), 100f);
                    nandaPoseSprites[0] = sp;
                    nandaPoseSprites[1] = sp;
                }
            }
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("BearChase_EndingCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var rootRt = canvasGo.GetComponent<RectTransform>();
            var bgBack = rootRt.gameObject.AddComponent<Image>();
            bgBack.color = new Color(0.04f, 0.03f, 0.03f, 1f);
            bgBack.raycastTarget = false;

            // Container de zoom com pivo deslocado para enquadrar Yuna a esquerda e Nanda azul no centro-direita no facepalm
            zoomContainer = CreateRect(
                "EndingZoomContainer",
                rootRt,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.555f, 0.375f),
                Vector2.zero,
                Vector2.zero);

            RectTransform roomRt = CreateRect("EndingRoomBackground", zoomContainer, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            roomBackgroundImage = roomRt.gameObject.AddComponent<Image>();
            roomBackgroundImage.sprite = roomBgSprite;
            roomBackgroundImage.preserveAspect = false;
            roomBackgroundImage.color = roomBgSprite != null ? Color.white : new Color(0.24f, 0.16f, 0.10f, 1f);
            roomBackgroundImage.raycastTarget = false;

            yunaShadowRect = CreateRect("EndingYunaShadow", zoomContainer, new Vector2(0.455f, 0.165f), new Vector2(0.455f, 0.165f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(225f, 44f));
            var yShadowImg = yunaShadowRect.gameObject.AddComponent<Image>();
            yShadowImg.sprite = softCircleSprite;
            yShadowImg.color = new Color(0.06f, 0.03f, 0.02f, 0.52f);
            yShadowImg.raycastTarget = false;

            yunaRect = CreateRect("EndingYuna", zoomContainer, new Vector2(0.455f, 0.155f), new Vector2(0.455f, 0.155f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(390f, 390f));
            yunaImage = yunaRect.gameObject.AddComponent<Image>();
            yunaImage.preserveAspect = true;
            yunaImage.raycastTarget = false;

            nandaGlowRect = CreateRect("EndingNandaCyanAura", zoomContainer, new Vector2(0.585f, 0.435f), new Vector2(0.585f, 0.435f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250f, 250f));
            var glowImg = nandaGlowRect.gameObject.AddComponent<Image>();
            glowImg.sprite = softCircleSprite;
            glowImg.color = new Color(0.22f, 0.85f, 1.0f, 0.42f);
            glowImg.raycastTarget = false;

            nandaRect = CreateRect("EndingNandaBlue", zoomContainer, new Vector2(0.585f, 0.435f), new Vector2(0.585f, 0.435f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 180f));
            nandaImage = nandaRect.gameObject.AddComponent<Image>();
            nandaImage.preserveAspect = true;
            nandaImage.raycastTarget = false;

            BuildDialogueCard(rootRt);

            RectTransform fadeRt = CreateRect("EndingFadeOverlay", rootRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            fadeOverlay = fadeRt.gameObject.AddComponent<Image>();
            fadeOverlay.color = new Color(0.03f, 0.02f, 0.02f, 0f);
            fadeOverlay.raycastTarget = false;

            canvas.gameObject.SetActive(false);
        }

        private void BuildDialogueCard(RectTransform parent)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform outerRt = CreateRect(
                "EndingDialogueCard",
                parent,
                new Vector2(0.5f, 0.96f),
                new Vector2(0.5f, 0.96f),
                new Vector2(0.5f, 1f),
                Vector2.zero,
                new Vector2(860f, 156f));
            dialogueBoxRoot = outerRt.gameObject;

            Image outerBg = outerRt.gameObject.AddComponent<Image>();
            outerBg.color = new Color(0.84f, 0.76f, 0.80f, 0.96f);
            outerBg.raycastTarget = false;

            RectTransform innerRt = CreateRect(
                "InnerCreamCard",
                outerRt,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(-14f, -14f));
            Image innerBg = innerRt.gameObject.AddComponent<Image>();
            innerBg.color = new Color(0.99f, 0.97f, 0.93f, 0.98f);
            innerBg.raycastTarget = false;

            RectTransform headerRt = CreateRect("SpeakerHeader", innerRt, new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.94f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            speakerHeaderText = headerRt.gameObject.AddComponent<Text>();
            speakerHeaderText.font = font;
            speakerHeaderText.fontSize = 26;
            speakerHeaderText.fontStyle = FontStyle.Italic;
            speakerHeaderText.alignment = TextAnchor.MiddleCenter;
            speakerHeaderText.color = new Color(0.45f, 0.37f, 0.36f, 1f);
            speakerHeaderText.raycastTarget = false;

            RectTransform bodyRt = CreateRect("DialogueBody", innerRt, new Vector2(0.05f, 0.08f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            dialogueBodyText = bodyRt.gameObject.AddComponent<Text>();
            dialogueBodyText.font = font;
            dialogueBodyText.fontSize = 30;
            dialogueBodyText.fontStyle = FontStyle.Italic;
            dialogueBodyText.alignment = TextAnchor.MiddleCenter;
            dialogueBodyText.color = new Color(0.24f, 0.18f, 0.16f, 1f);
            dialogueBodyText.raycastTarget = false;

            YunaLegend.Presentation.HudTheme.EnsureCornerTapCue(outerRt, 22f, 12f, 10f, new Color(0.30f, 0.24f, 0.39f, 0.76f));
        }

        private static RectTransform CreateRect(
            string name,
            RectTransform parent,
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
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d);
                    a *= a;
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void DestroySafe(UnityEngine.Object obj)
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
    }
}
