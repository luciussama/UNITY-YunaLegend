using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Interface da Arena Imaginaria: coracoes da Yuna e do inimigo, dica de abertura/defesa,
    /// corte dramatico do Especial Arco-Iris (eyes-power-v1.png), controles touch mobile e paineis.
    /// </summary>
    public sealed class ArenaHudController : MonoBehaviour
    {
        [SerializeField] private Font uiFont;
        [SerializeField] private Sprite eyesCutinSprite;

        private Canvas hudCanvas;
        private Text playerHeartsText;
        private Text enemyTitleText;
        private Text enemyHeartsText;
        private Text statusHintText;
        private Text specialReadyText;
        private Button specialMobileButton;

        private GameObject cutinRoot;
        private Image cutinImage;
        private Image rainbowBeamImage;

        private GameObject pausePanel;
        private GameObject outcomePanel;
        private Text outcomeTitleText;
        private Text outcomeBodyText;

        private GameObject mobileControlsRoot;
        private ArenaVirtualDPad virtualDPad;
        private Image fadeOverlay;
        private float fadeAlpha = 1f;
        private bool touchAttackQueued;
        private bool touchSpecialQueued;

        public bool IsPaused { get; private set; }
        public Vector2 TouchMove => virtualDPad != null ? virtualDPad.Direction : Vector2.zero;
        public bool TouchCrouchHeld => virtualDPad != null && virtualDPad.CrouchHeld;

        public event Action<bool> OnPauseToggled;
        public event Action OnMuteToggled;
        public event Action OnRestartRequested;
        public event Action OnReturnToTitleRequested;
        public event Action OnNextStageRequested;

        public void Configure(Font font, Sprite eyesSprite)
        {
            uiFont = font;
            eyesCutinSprite = eyesSprite;
        }

        private void Awake()
        {
            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            BuildHud();
        }

        public bool ConsumeTouchJump()
        {
            return virtualDPad != null && virtualDPad.ConsumeJumpPressed();
        }

        public bool ConsumeTouchAttack()
        {
            bool v = touchAttackQueued;
            touchAttackQueued = false;
            return v;
        }

        public bool ConsumeTouchSpecial()
        {
            bool v = touchSpecialQueued;
            touchSpecialQueued = false;
            return v;
        }

        public void Refresh(ArenaCombatSimulation sim)
        {
            if (sim == null)
            {
                return;
            }

            if (playerHeartsText != null)
            {
                playerHeartsText.text = $"YUNA  {FormatHearts(sim.PlayerHp, sim.PlayerMaxHp)}";
            }

            if (enemyTitleText != null)
            {
                string rage = (sim.BearEnraged && sim.CurrentEnemySpec.Id == ArenaEnemyId.Bear) ? " (FÚRIA!)" : string.Empty;
                enemyTitleText.text = $"{sim.CurrentEnemySpec.Title}{rage}  [{sim.WaveIndex + 1}/{sim.TotalWaves}]";
            }

            if (enemyHeartsText != null)
            {
                enemyHeartsText.text = FormatHearts(sim.EnemyHp, sim.EnemyMaxHp);
            }

            if (statusHintText != null)
            {
                statusHintText.text = sim.StatusHint;
            }

            if (specialReadyText != null)
            {
                specialReadyText.text = sim.SpecialUsed ? "ESPECIAL: USADO" : "[R] ESPADA ARCO-ÍRIS: PRONTO";
                specialReadyText.color = sim.SpecialUsed
                    ? new Color(0.55f, 0.52f, 0.48f)
                    : new Color(1f, 0.88f, 0.45f);
            }

            if (specialMobileButton != null)
            {
                specialMobileButton.interactable = !sim.SpecialUsed && !sim.IsSpecialActive;
            }

            UpdateSpecialOverlay(sim);
            UpdateMobileControlsVisibility();
        }

        public void SetPausedState(bool paused)
        {
            IsPaused = paused;
            if (pausePanel != null)
            {
                pausePanel.SetActive(paused);
            }
        }

        public void ShowOutcome(ArenaBattleOutcome outcome)
        {
            if (outcomePanel == null || outcome == ArenaBattleOutcome.Ongoing)
            {
                return;
            }

            outcomePanel.SetActive(true);
            if (outcome == ArenaBattleOutcome.Victory)
            {
                outcomeTitleText.text = "Vitória na Arena!";
                outcomeBodyText.text = "O Guardião de Pelúcia caiu! Você dominou a Espada de Brinquedo.";
            }
            else
            {
                outcomeTitleText.text = "Derrota na Arena";
                outcomeBodyText.text = "Os brinquedos venceram desta vez. Segure para trás para defender os golpes!";
            }
        }

        private void UpdateSpecialOverlay(ArenaCombatSimulation sim)
        {
            if (cutinRoot == null)
            {
                return;
            }

            if (!sim.IsSpecialActive)
            {
                cutinRoot.SetActive(false);
                return;
            }

            cutinRoot.SetActive(true);
            bool showEyes = sim.SpecialPhase == ArenaSpecialPhase.Charge || sim.SpecialPhase == ArenaSpecialPhase.Pullback;
            if (cutinImage != null)
            {
                cutinImage.enabled = showEyes && eyesCutinSprite != null;
                float t = Mathf.Clamp01(sim.SpecialTick / (float)ArenaSpecialSpec.ChargeEndTick);
                cutinImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.08f, 1.0f, t);
            }

            bool showBeam = sim.SpecialPhase == ArenaSpecialPhase.Launch
                || sim.SpecialPhase == ArenaSpecialPhase.Impact
                || sim.SpecialPhase == ArenaSpecialPhase.Fade;
            if (rainbowBeamImage != null)
            {
                rainbowBeamImage.enabled = showBeam;
                if (showBeam)
                {
                    float hue = Mathf.Repeat(Time.time * 2.4f, 1f);
                    Color beamColor = Color.HSVToRGB(hue, 0.78f, 1f);
                    beamColor.a = sim.SpecialPhase == ArenaSpecialPhase.Fade ? 0.35f : 0.78f;
                    rainbowBeamImage.color = beamColor;
                }
            }
        }

        private void UpdateMobileControlsVisibility()
        {
            if (mobileControlsRoot == null)
            {
                return;
            }

            bool isTouch = Application.isMobilePlatform || Touchscreen.current != null;
            mobileControlsRoot.SetActive(isTouch && !IsPaused);
        }

        private void BuildHud()
        {
            var canvasGo = new GameObject("Arena_HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            hudCanvas = canvasGo.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 50;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var root = canvasGo.transform;

            playerHeartsText = CreateText("Yuna_Hearts", root, "YUNA  ♥♥♥", 26, new Color(1f, 0.92f, 0.82f), TextAnchor.UpperLeft);
            SetRect(playerHeartsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -18f), new Vector2(460f, 36f));

            specialReadyText = CreateText("Special_Badge", root, "[R] ESPADA ARCO-ÍRIS: PRONTO", 16, new Color(1f, 0.88f, 0.45f), TextAnchor.UpperLeft);
            SetRect(specialReadyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -56f), new Vector2(460f, 26f));

            enemyTitleText = CreateText("Enemy_Title", root, "Locomotiva de Cerco", 22, new Color(1f, 0.92f, 0.82f), TextAnchor.UpperRight);
            SetRect(enemyTitleText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-140f, -18f), new Vector2(460f, 32f));

            enemyHeartsText = CreateText("Enemy_Hearts", root, "♥♥♥", 26, new Color(1f, 0.48f, 0.48f), TextAnchor.UpperRight);
            SetRect(enemyHeartsText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-140f, -50f), new Vector2(460f, 34f));

            statusHintText = CreateText("Status_Hint", root, string.Empty, 20, new Color(0.98f, 0.94f, 0.82f), TextAnchor.LowerCenter);
            SetRect(statusHintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(760f, 36f));

            CreateTopButton("Btn_Mute", root, "SOM", new Vector2(-78f, -18f), () => OnMuteToggled?.Invoke());
            CreateTopButton("Btn_Pause", root, "II", new Vector2(-22f, -18f), () => OnPauseToggled?.Invoke(!IsPaused));

            BuildSpecialOverlay(root);
            BuildMobileControls(root);
            BuildPausePanel(root);
            BuildOutcomePanel(root);
            BuildFadeOverlay(root);
        }

        private void Update()
        {
            if (fadeOverlay != null && fadeAlpha > 0f)
            {
                fadeAlpha = Mathf.Max(0f, fadeAlpha - Time.unscaledDeltaTime * 1.6f);
                fadeOverlay.color = new Color(0.02f, 0.01f, 0.03f, fadeAlpha);
                if (fadeAlpha <= 0f)
                {
                    fadeOverlay.gameObject.SetActive(false);
                }
            }
        }

        private void BuildFadeOverlay(Transform root)
        {
            var fadeGo = new GameObject("Fade_In_Overlay", typeof(RectTransform), typeof(Image));
            fadeGo.transform.SetParent(root, false);
            Stretch(fadeGo.GetComponent<RectTransform>());
            fadeOverlay = fadeGo.GetComponent<Image>();
            fadeOverlay.color = new Color(0.02f, 0.01f, 0.03f, 1f);
            fadeOverlay.raycastTarget = false;
            fadeAlpha = 1f;
        }

        private void BuildSpecialOverlay(Transform root)
        {
            cutinRoot = new GameObject("Special_Cutin_Overlay", typeof(RectTransform));
            cutinRoot.transform.SetParent(root, false);
            Stretch(cutinRoot.GetComponent<RectTransform>());

            var eyesGo = new GameObject("Eyes_Cutin", typeof(RectTransform), typeof(Image));
            eyesGo.transform.SetParent(cutinRoot.transform, false);
            var eyesRect = eyesGo.GetComponent<RectTransform>();
            eyesRect.anchorMin = new Vector2(0f, 0.25f);
            eyesRect.anchorMax = new Vector2(1f, 0.75f);
            eyesRect.offsetMin = Vector2.zero;
            eyesRect.offsetMax = Vector2.zero;
            cutinImage = eyesGo.GetComponent<Image>();
            cutinImage.sprite = eyesCutinSprite;
            cutinImage.preserveAspect = true;
            cutinImage.raycastTarget = false;

            var beamGo = new GameObject("Rainbow_Beam", typeof(RectTransform), typeof(Image));
            beamGo.transform.SetParent(cutinRoot.transform, false);
            var beamRect = beamGo.GetComponent<RectTransform>();
            beamRect.anchorMin = new Vector2(0f, 0.38f);
            beamRect.anchorMax = new Vector2(1f, 0.62f);
            beamRect.offsetMin = Vector2.zero;
            beamRect.offsetMax = Vector2.zero;
            rainbowBeamImage = beamGo.GetComponent<Image>();
            rainbowBeamImage.raycastTarget = false;
            rainbowBeamImage.enabled = false;

            cutinRoot.SetActive(false);
        }

        private void BuildMobileControls(Transform root)
        {
            mobileControlsRoot = new GameObject("Mobile_Controls", typeof(RectTransform));
            mobileControlsRoot.transform.SetParent(root, false);
            Stretch(mobileControlsRoot.GetComponent<RectTransform>());

            virtualDPad = ArenaVirtualDPad.CreateVisualDPad(mobileControlsRoot.transform, uiFont);

            CreateActionButton("Btn_Attack", mobileControlsRoot.transform, "attack", new Vector2(-34f, 34f), new Vector2(128f, 128f),
                new Color(0.84f, 0.28f, 0.24f, 0.88f), () => touchAttackQueued = true);

            specialMobileButton = CreateActionButton("Btn_Special", mobileControlsRoot.transform, "special", new Vector2(-178f, 44f), new Vector2(114f, 84f),
                new Color(0.28f, 0.56f, 0.94f, 0.88f), () => touchSpecialQueued = true);

            mobileControlsRoot.SetActive(false);
        }

        private void BuildPausePanel(Transform root)
        {
            pausePanel = CreateModalPanel("Pause_Panel", root, "Arena Pausada", out var container);
            CreateModalButton("Continuar", container, new Vector2(0f, 26f), () => OnPauseToggled?.Invoke(false));
            CreateModalButton("Reiniciar Arena", container, new Vector2(0f, -34f), () => OnRestartRequested?.Invoke());
            CreateModalButton("Voltar ao Menu", container, new Vector2(0f, -94f), () => OnReturnToTitleRequested?.Invoke());
            pausePanel.SetActive(false);
        }

        private void BuildOutcomePanel(Transform root)
        {
            outcomePanel = CreateModalPanel("Outcome_Panel", root, "Resultado", out var container);
            outcomeTitleText = container.Find("Modal_Title").GetComponent<Text>();
            outcomeBodyText = CreateText("Outcome_Body", container, string.Empty, 18, new Color(0.90f, 0.85f, 0.76f), TextAnchor.MiddleCenter);
            SetRect(outcomeBodyText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 38f), new Vector2(440f, 72f));

            CreateModalButton("Jogar Novamente", container, new Vector2(0f, -34f), () => OnRestartRequested?.Invoke());
            CreateModalButton("Voltar ao Menu", container, new Vector2(0f, -94f), () => OnNextStageRequested?.Invoke());
            outcomePanel.SetActive(false);
        }

        private GameObject CreateModalPanel(string name, Transform root, string title, out Transform container)
        {
            var overlay = new GameObject(name, typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(root, false);
            Stretch(overlay.GetComponent<RectTransform>());
            overlay.GetComponent<Image>().color = new Color(0.03f, 0.02f, 0.04f, 0.78f);

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(overlay.transform, false);
            SetRect(box.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 320f));
            box.GetComponent<Image>().color = new Color(0.09f, 0.06f, 0.11f, 0.96f);
            container = box.transform;

            var titleText = CreateText("Modal_Title", container, title, 30, new Color(1f, 0.90f, 0.68f), TextAnchor.MiddleCenter);
            SetRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(440f, 44f));
            return overlay;
        }

        private void CreateModalButton(string label, Transform parent, Vector2 pos, Action onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            SetRect(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(260f, 46f));
            go.GetComponent<Image>().color = new Color(0.24f, 0.18f, 0.28f, 1f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

            var text = CreateText("Label", go.transform, label, 20, Color.white, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
        }

        private void CreateTopButton(string name, Transform root, string label, Vector2 anchoredPos, Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(root, false);
            SetRect(go.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), anchoredPos, new Vector2(48f, 36f));
            go.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.14f, 0.82f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

            var text = CreateText("Label", go.transform, label, 15, new Color(1f, 0.92f, 0.78f), TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
        }

        private Button CreateActionButton(string name, Transform root, string label, Vector2 pos, Vector2 size, Color bgColor, Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(root, false);
            SetRect(go.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), pos, size);
            go.GetComponent<Image>().color = bgColor;
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            var text = CreateText("Label", go.transform, label, 18, Color.white, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return btn;
        }

        private void CreateHoldButton(string name, Transform root, string label, Vector2 anchor, Vector2 pos, Vector2 size, Action onDown, Action onUp)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(EventTrigger));
            go.transform.SetParent(root, false);
            SetRect(go.GetComponent<RectTransform>(), anchor, anchor, anchor, pos, size);
            go.GetComponent<Image>().color = new Color(0.14f, 0.12f, 0.18f, 0.72f);

            var trigger = go.GetComponent<EventTrigger>();
            AddTrigger(trigger, EventTriggerType.PointerDown, _ => onDown?.Invoke());
            AddTrigger(trigger, EventTriggerType.PointerUp, _ => onUp?.Invoke());
            AddTrigger(trigger, EventTriggerType.PointerExit, _ => onUp?.Invoke());

            var text = CreateText("Label", go.transform, label, 28, Color.white, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
        }

        private static void AddTrigger(EventTrigger trigger, EventTriggerType type, Action<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(data => callback(data));
            trigger.triggers.Add(entry);
        }

        private Text CreateText(string name, Transform parent, string content, int size, Color color, TextAnchor align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = uiFont;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        private static string FormatHearts(int hp, int maxHp)
        {
            var sb = new System.Text.StringBuilder(maxHp);
            for (int i = 0; i < maxHp; i++)
            {
                sb.Append(i < hp ? '♥' : '♡');
            }

            return sb.ToString();
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
