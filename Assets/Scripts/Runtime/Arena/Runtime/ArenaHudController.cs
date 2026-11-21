using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Interface da Arena Imaginaria com layout responsivo mobile:
    /// - Prioridade 1: todos os textos e dicas na parte superior, sem cobrir Yuna/inimigos.
    /// - Prioridade 2: fontes entre 14 e 16px logicos no celular.
    /// - Prioridade 3: preserva estetica original (coracoes creme/coral, D-Pad cruz, botoes X/Y).
    /// - Prioridade 4: paineis e controles cabem integralmente no enquadramento do celular.
    /// </summary>
    public sealed class ArenaHudController : MonoBehaviour
    {
        [SerializeField] private Font uiFont;
        [SerializeField] private Sprite eyesCutinSprite;

        private Canvas hudCanvas;
        private RectTransform canvasRect;
        private Text playerHeartsText;
        private Text enemyTitleText;
        private Text enemyHeartsText;
        private Text statusHintText;
        private Text specialReadyText;
        private RectTransform muteBtnRect;
        private Text muteBtnLabel;
        private RectTransform pauseBtnRect;
        private Text pauseBtnLabel;

        private Button attackMobileButton;
        private Button specialMobileButton;

        private GameObject cutinRoot;
        private Image cutinImage;
        private Image rainbowBeamImage;

        private GameObject pausePanel;
        private RectTransform pauseBoxRect;
        private GameObject outcomePanel;
        private RectTransform outcomeBoxRect;
        private Text outcomeTitleText;
        private Text outcomeBodyText;

        private GameObject mobileControlsRoot;
        private ArenaVirtualDPad virtualDPad;
        private Image fadeOverlay;
        private float fadeAlpha = 1f;
        private bool touchAttackQueued;
        private bool touchSpecialQueued;

        private Vector2 lastCanvasSize = new Vector2(-1f, -1f);
        private float forcedLogicalWidth = -1f;
        private float forcedLogicalHeight = -1f;
        private bool forceMobileControls;

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

        public void SetViewportOverride(float logicalWidth, float logicalHeight)
        {
            forcedLogicalWidth = logicalWidth;
            forcedLogicalHeight = logicalHeight;
            lastCanvasSize = new Vector2(-1f, -1f);
            if (canvasRect != null)
            {
                ApplyResponsiveLayout(canvasRect.rect.size);
            }
        }

        public void ClearViewportOverride()
        {
            forcedLogicalWidth = -1f;
            forcedLogicalHeight = -1f;
            lastCanvasSize = new Vector2(-1f, -1f);
        }

        public void ForceShowMobileControlsForCapture(bool force)
        {
            forceMobileControls = force;
            UpdateMobileControlsVisibility();
        }

        public void SkipFadeForCapture()
        {
            fadeAlpha = 0f;
            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(false);
            }
        }

        private void Awake()
        {
            // Usa fonte de interface nitida e legivel em 14-16px no celular (evita fonte cursiva fina)
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
                bool isMobile = IsMobileViewport();
                specialReadyText.text = sim.SpecialUsed
                    ? "ESPECIAL: USADO"
                    : (isMobile ? "ESPECIAL ARCO-ÍRIS: PRONTO" : "[R] ESPADA ARCO-ÍRIS: PRONTO");
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

            UpdateMobileControlsVisibility();
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

            bool isTouch = forceMobileControls || Application.isMobilePlatform || Touchscreen.current != null;
            mobileControlsRoot.SetActive(isTouch && !IsPaused);
        }

        private void BuildHud()
        {
            var canvasGo = new GameObject("Arena_HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            hudCanvas = canvasGo.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 50;
            canvasRect = canvasGo.GetComponent<RectTransform>();

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var root = canvasGo.transform;

            var yunaPill = CreateHudPill("Yuna_HudPill", root);
            playerHeartsText = CreateText("Yuna_Hearts", yunaPill.transform, "YUNA  ♥♥♥", 26, new Color(1f, 0.92f, 0.82f), TextAnchor.UpperLeft);
            playerHeartsText.fontStyle = FontStyle.Bold;
            SetRect(playerHeartsText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f, -4f), new Vector2(-16f, 24f));

            specialReadyText = CreateText("Special_Badge", yunaPill.transform, "[R] ESPADA ARCO-ÍRIS: PRONTO", 16, new Color(1f, 0.88f, 0.45f), TextAnchor.UpperLeft);
            specialReadyText.fontStyle = FontStyle.Bold;
            SetRect(specialReadyText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f, -26f), new Vector2(-16f, 22f));

            var enemyPill = CreateHudPill("Enemy_HudPill", root);
            enemyTitleText = CreateText("Enemy_Title", enemyPill.transform, "Locomotiva de Cerco", 22, new Color(1f, 0.92f, 0.82f), TextAnchor.UpperRight);
            enemyTitleText.fontStyle = FontStyle.Bold;
            SetRect(enemyTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f, -4f), new Vector2(-16f, 24f));

            enemyHeartsText = CreateText("Enemy_Hearts", enemyPill.transform, "♥♥♥", 26, new Color(1f, 0.48f, 0.48f), TextAnchor.UpperRight);
            enemyHeartsText.fontStyle = FontStyle.Bold;
            SetRect(enemyHeartsText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f, -26f), new Vector2(-16f, 22f));

            statusHintText = CreateText("Status_Hint", root, string.Empty, 20, new Color(0.98f, 0.94f, 0.82f), TextAnchor.LowerCenter);
            statusHintText.fontStyle = FontStyle.Bold;
            SetRect(statusHintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(760f, 36f));

            muteBtnRect = CreateTopButton("Btn_Mute", root, "SOM", new Vector2(-78f, -18f), out muteBtnLabel, () => OnMuteToggled?.Invoke());
            pauseBtnRect = CreateTopButton("Btn_Pause", root, "II", new Vector2(-22f, -18f), out pauseBtnLabel, () => OnPauseToggled?.Invoke(!IsPaused));

            BuildSpecialOverlay(root);
            BuildMobileControls(root);
            BuildPausePanel(root);
            BuildOutcomePanel(root);
            BuildFadeOverlay(root);

            ApplyResponsiveLayout(canvasRect.rect.size.x > 1f ? canvasRect.rect.size : new Vector2(1280f, 720f));
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

            if (canvasRect != null)
            {
                Vector2 size = canvasRect.rect.size;
                if (size.x > 1f && size.y > 1f && (!Mathf.Approximately(size.x, lastCanvasSize.x) || !Mathf.Approximately(size.y, lastCanvasSize.y)))
                {
                    lastCanvasSize = size;
                    ApplyResponsiveLayout(size);
                }
            }
        }

        private void ApplyResponsiveLayout(Vector2 canvasSize)
        {
            if (canvasSize.x <= 1f || canvasSize.y <= 1f)
            {
                return;
            }

            if (!IsMobileViewport())
            {
                return;
            }

            float px = ComputePxToCanvas(canvasSize);
            bool isPortrait = (canvasSize.x / canvasSize.y) < 1.1f;
            float marginX = 12f * px;
            float topY = -10f * px;
            float usableW = Mathf.Max(220f * px, canvasSize.x - marginX * 2f);

            int f14 = Mathf.RoundToInt(14f * px);
            int f15 = Mathf.RoundToInt(15f * px);
            int f16 = Mathf.RoundToInt(16f * px);

            // Top-right utility buttons (SOM, II)
            float muteW = 46f * px;
            float pauseW = 36f * px;
            float btnH = 28f * px;
            float btnGap = 6f * px;
            SetRect(pauseBtnRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-marginX, topY), new Vector2(pauseW, btnH));
            SetRect(muteBtnRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-(marginX + pauseW + btnGap), topY), new Vector2(muteW, btnH));
            muteBtnLabel.fontSize = f14;
            pauseBtnLabel.fontSize = f14;

            float buttonsTotalW = muteW + pauseW + btnGap + 8f * px;

            var yunaPillRect = playerHeartsText.transform.parent as RectTransform;
            var enemyPillRect = enemyTitleText.transform.parent as RectTransform;

            if (isPortrait)
            {
                float pillH = 44f * px;
                float leftW = Mathf.Max(220f * px, usableW - buttonsTotalW);
                SetRect(yunaPillRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(marginX, topY), new Vector2(leftW, pillH));
                playerHeartsText.fontSize = f15;
                playerHeartsText.alignment = TextAnchor.UpperLeft;
                SetRect(playerHeartsText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f * px, -4f * px), new Vector2(-16f * px, 20f * px));

                specialReadyText.fontSize = f14;
                specialReadyText.alignment = TextAnchor.UpperLeft;
                SetRect(specialReadyText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f * px, -23f * px), new Vector2(-16f * px, 18f * px));

                SetRect(enemyPillRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(marginX, topY - pillH - 6f * px), new Vector2(usableW, pillH));
                enemyTitleText.fontSize = f15;
                enemyTitleText.alignment = TextAnchor.UpperLeft;
                SetRect(enemyTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f * px, -4f * px), new Vector2(-16f * px, 20f * px));

                enemyHeartsText.fontSize = f15;
                enemyHeartsText.alignment = TextAnchor.UpperLeft;
                SetRect(enemyHeartsText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f * px, -23f * px), new Vector2(-16f * px, 20f * px));

                // Status Hint NO TOPO (Prioridade 1), acima da arena e dos controles
                statusHintText.fontSize = f14;
                statusHintText.alignment = TextAnchor.UpperCenter;
                SetRect(statusHintText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, topY - pillH * 2f - 14f * px), new Vector2(usableW, 38f * px));
            }
            else
            {
                float pillH = 44f * px;
                float colW = Mathf.Max(220f * px, (usableW - buttonsTotalW - 12f * px) * 0.48f);
                SetRect(yunaPillRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(marginX, topY), new Vector2(colW, pillH));
                playerHeartsText.fontSize = f15;
                playerHeartsText.alignment = TextAnchor.UpperLeft;
                SetRect(playerHeartsText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f * px, -4f * px), new Vector2(-16f * px, 20f * px));

                specialReadyText.fontSize = f14;
                specialReadyText.alignment = TextAnchor.UpperLeft;
                SetRect(specialReadyText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f * px, -23f * px), new Vector2(-16f * px, 18f * px));

                float rightOffset = -(marginX + buttonsTotalW);
                SetRect(enemyPillRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(rightOffset, topY), new Vector2(colW, pillH));
                enemyTitleText.fontSize = f15;
                enemyTitleText.alignment = TextAnchor.UpperRight;
                SetRect(enemyTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f * px, -4f * px), new Vector2(-16f * px, 20f * px));

                enemyHeartsText.fontSize = f15;
                enemyHeartsText.alignment = TextAnchor.UpperRight;
                SetRect(enemyHeartsText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8f * px, -23f * px), new Vector2(-16f * px, 20f * px));

                statusHintText.fontSize = f14;
                statusHintText.alignment = TextAnchor.UpperCenter;
                SetRect(statusHintText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, topY - pillH - 8f * px), new Vector2(usableW, 34f * px));
            }

            // Controles touch inferiores com margem esquerda e inferior seguras para todos os rotulos do D-Pad
            if (virtualDPad != null)
            {
                float dpadSize = isPortrait ? 108f * px : 106f * px;
                float dpadX = isPortrait ? (marginX + dpadSize * 0.5f + 18f * px) : (marginX + dpadSize * 0.5f + 28f * px);
                float dpadY = isPortrait ? 94f * px : 92f * px;
                virtualDPad.ApplyMobileSizing(dpadSize, new Vector2(dpadX, dpadY), f14, px);
            }

            float combatW = isPortrait ? 80f * px : 92f * px;
            float combatH = isPortrait ? 62f * px : 68f * px;
            float combatGap = 8f * px;
            float combatBottomY = 24f * px;

            if (specialMobileButton != null)
            {
                ResizeCombatButton(specialMobileButton.GetComponent<RectTransform>(), new Vector2(-marginX, combatBottomY), new Vector2(combatW, combatH), f14, f15, px);
            }

            if (attackMobileButton != null)
            {
                ResizeCombatButton(attackMobileButton.GetComponent<RectTransform>(), new Vector2(-(marginX + combatW + combatGap), combatBottomY), new Vector2(combatW, combatH), f14, f15, px);
            }

            // Modais (Pause e Outcome) cabendo integralmente no celular
            ResizeModalBox(pauseBoxRect, usableW, px, f16, f15, f14);
            ResizeModalBox(outcomeBoxRect, usableW, px, f16, f15, f14);
        }

        private static void ResizeCombatButton(RectTransform rt, Vector2 pos, Vector2 size, int f14, int f15, float px)
        {
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var key = rt.Find("KeyHint")?.GetComponent<Text>();
            if (key != null)
            {
                key.fontSize = f14;
                SetRect(key.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f * px), new Vector2(size.x - 8f * px, 18f * px));
            }

            var main = rt.Find("MainLabel")?.GetComponent<Text>();
            if (main != null)
            {
                main.fontSize = f14;
                SetRect(main.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(size.x - 6f * px, 22f * px));
            }

            var sub = rt.Find("SubLabel")?.GetComponent<Text>();
            if (sub != null)
            {
                sub.fontSize = f14;
                SetRect(sub.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f * px), new Vector2(size.x - 8f * px, 18f * px));
            }
        }

        private static void ResizeModalBox(RectTransform box, float usableW, float px, int f16, int f15, int f14)
        {
            if (box == null)
            {
                return;
            }

            float w = Mathf.Min(330f * px, usableW);
            float h = 240f * px;
            box.sizeDelta = new Vector2(w, h);

            var title = box.Find("Modal_Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.fontSize = f16;
                SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f * px), new Vector2(w - 24f * px, 28f * px));
            }

            var body = box.Find("Outcome_Body")?.GetComponent<Text>();
            if (body != null)
            {
                body.fontSize = f14;
                SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 34f * px), new Vector2(w - 28f * px, 58f * px));
            }

            float btnW = w - 40f * px;
            float btnH = 38f * px;
            int btnIndex = 0;
            for (int i = 0; i < box.childCount; i++)
            {
                var child = box.GetChild(i) as RectTransform;
                if (child == null || !child.name.StartsWith("Btn_"))
                {
                    continue;
                }

                float yOffset = body != null ? (-24f - btnIndex * 46f) * px : (34f - btnIndex * 48f) * px;
                child.anchoredPosition = new Vector2(0f, yOffset);
                child.sizeDelta = new Vector2(btnW, btnH);
                var lbl = child.GetComponentInChildren<Text>();
                if (lbl != null)
                {
                    lbl.fontSize = f15;
                }

                btnIndex++;
            }
        }

        private float ComputePxToCanvas(Vector2 canvasSize)
        {
            float logicalW = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float logicalH = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;

            if (logicalW <= 1f || logicalH <= 1f)
            {
                return canvasSize.x / 390f;
            }

            float minDim = Mathf.Min(logicalW, logicalH);
            if (forcedLogicalWidth <= 0f && Application.isMobilePlatform && minDim >= 700f)
            {
                float dpr = Mathf.Clamp(minDim / 390f, 1.5f, 3.5f);
                logicalW /= dpr;
            }

            return Mathf.Clamp(canvasSize.x / Mathf.Max(280f, logicalW), 0.5f, 4.5f);
        }

        private bool IsMobileViewport()
        {
            float w = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float h = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;
            return Application.isMobilePlatform || Touchscreen.current != null || w <= 960f || h <= 540f;
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

            attackMobileButton = CreateLegacyCombatButton(
                "Btn_Attack",
                mobileControlsRoot.transform,
                "X",
                "ESPADA",
                string.Empty,
                new Vector2(-188f, 26f),
                new Vector2(156f, 114f),
                new Color(0.950f, 0.847f, 0.533f, 0.94f),
                new Color(1f, 1f, 1f, 0.96f),
                new Color(0.12f, 0.08f, 0.05f, 1f),
                () => touchAttackQueued = true);

            specialMobileButton = CreateLegacyCombatButton(
                "Btn_Special",
                mobileControlsRoot.transform,
                "Y",
                "ESPECIAL",
                "1 USO",
                new Vector2(-20f, 26f),
                new Vector2(156f, 114f),
                new Color(0.290f, 0.275f, 0.337f, 0.92f),
                new Color(0.90f, 0.89f, 0.94f, 0.96f),
                Color.white,
                () => touchSpecialQueued = true);

            mobileControlsRoot.SetActive(false);
        }

        private void BuildPausePanel(Transform root)
        {
            pausePanel = CreateModalPanel("Pause_Panel", root, "Arena Pausada", out var container);
            pauseBoxRect = container as RectTransform;
            CreateModalButton("Continuar", container, new Vector2(0f, 26f), () => OnPauseToggled?.Invoke(false));
            CreateModalButton("Reiniciar Arena", container, new Vector2(0f, -34f), () => OnRestartRequested?.Invoke());
            CreateModalButton("Voltar ao Menu", container, new Vector2(0f, -94f), () => OnReturnToTitleRequested?.Invoke());
            pausePanel.SetActive(false);
        }

        private void BuildOutcomePanel(Transform root)
        {
            outcomePanel = CreateModalPanel("Outcome_Panel", root, "Resultado", out var container);
            outcomeBoxRect = container as RectTransform;
            outcomeTitleText = container.Find("Modal_Title").GetComponent<Text>();
            outcomeBodyText = CreateText("Outcome_Body", container, string.Empty, 18, new Color(0.90f, 0.85f, 0.76f), TextAnchor.MiddleCenter);
            outcomeBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
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
            var outline = box.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.80f, 0.42f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
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
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.80f, 0.42f, 0.75f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

            var text = CreateText("Label", go.transform, label, 20, Color.white, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
        }

        private RectTransform CreateTopButton(string name, Transform root, string label, Vector2 anchoredPos, out Text labelText, Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(root, false);
            var rt = go.GetComponent<RectTransform>();
            SetRect(rt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), anchoredPos, new Vector2(48f, 36f));
            go.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.14f, 0.86f);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.90f, 0.78f, 0.48f, 0.75f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

            labelText = CreateText("Label", go.transform, label, 15, new Color(1f, 0.92f, 0.78f), TextAnchor.MiddleCenter);
            Stretch(labelText.rectTransform);
            return rt;
        }

        private Button CreateLegacyCombatButton(
            string name,
            Transform root,
            string keyHint,
            string mainLabel,
            string subLabel,
            Vector2 pos,
            Vector2 size,
            Color fillColor,
            Color borderColor,
            Color textColor,
            Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(root, false);
            SetRect(go.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), pos, size);
            var img = go.GetComponent<Image>();
            img.sprite = CreateRoundedRectSprite(256, 192, 26f, 5f, fillColor, borderColor);
            img.color = Color.white;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var keyText = CreateTextNoShadow("KeyHint", go.transform, keyHint, 18, textColor, TextAnchor.UpperCenter);
            keyText.fontStyle = FontStyle.Bold;
            SetRect(keyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(size.x - 16f, 24f));

            var mainText = CreateTextNoShadow("MainLabel", go.transform, mainLabel, 22, textColor, TextAnchor.MiddleCenter);
            mainText.fontStyle = FontStyle.Bold;
            SetRect(mainText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, string.IsNullOrEmpty(subLabel) ? -2f : 2f), new Vector2(size.x - 14f, 32f));

            if (!string.IsNullOrEmpty(subLabel))
            {
                Color subColor = new Color(textColor.r, textColor.g, textColor.b, 0.78f);
                var subText = CreateTextNoShadow("SubLabel", go.transform, subLabel, 13, subColor, TextAnchor.LowerCenter);
                subText.fontStyle = FontStyle.Bold;
                SetRect(subText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(size.x - 16f, 20f));
            }

            return btn;
        }

        private Text CreateTextNoShadow(string name, Transform parent, string content, int size, Color color, TextAnchor align)
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
            return text;
        }

        private static Sprite CreateRoundedRectSprite(int width, int height, float radiusPx, float borderPx, Color fillColor, Color borderColor)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float hx = width * 0.5f;
            float hy = height * 0.5f;
            float bx = hx - radiusPx - 2f;
            float by = hy - radiusPx - 2f;
            var pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                float py = y + 0.5f - hy;
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f - hx;
                    float dx = Mathf.Abs(px) - bx;
                    float dy = Mathf.Abs(py) - by;
                    float d = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f))
                        + Mathf.Min(Mathf.Max(dx, dy), 0f) - radiusPx;

                    if (d > 1.5f)
                    {
                        pixels[y * width + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float outerAlpha = 1f - Mathf.SmoothStep(-0.75f, 1.0f, d);
                    float borderMask = Mathf.SmoothStep(-borderPx - 0.75f, -borderPx + 0.75f, d);
                    Color c = Color.Lerp(fillColor, borderColor, borderMask);
                    c.a *= outerAlpha;
                    pixels[y * width + x] = (Color32)c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
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
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        private static GameObject CreateHudPill(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.102f, 0.063f, 0.149f, 0.86f);
            img.raycastTarget = false;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.980f, 0.800f, 0.082f, 0.88f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return go;
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
