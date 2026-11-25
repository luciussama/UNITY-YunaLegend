using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using YunaLegend.Dialogue;
using YunaLegend.Presentation;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Controller especialista de UI/HUD para o fluxo jogavel do Quarto Escuro
    /// em uGUI responsivo para celular e desktop, preservando a estetica vitoriana/livro ilustrado.
    /// </summary>
    public sealed class StageHudController : MonoBehaviour
    {
        public Func<StageFlowStateMachine.FlowPhase> GetPhase;
        public Func<bool> IsPaused;
        public Func<bool> IsMuted;
        public Func<bool> IsPlayerDead;
        public Func<bool> IsFalseDoorDialogueReady;
        public Func<string> GetFailure;
        public Func<float> GetStage2BannerTimer;
        public Func<float> GetOpeningChapterBannerAlpha;
        public Func<float> GetOpeningCurtainAlpha;
        public Func<int> GetOpeningDialogueBeat;
        public Func<bool> IsOpeningYunaInBed;
        public Func<Vector3> GetPlayerWorldPosition;
        public Func<Vector3> GetBearTopWorldPosition;

        public event Action OnAdvanceRequested;
        public event Action OnTogglePauseRequested;
        public event Action OnToggleMuteRequested;
        public event Action OnRetryFromBeginningRequested;
        public event Action OnRetryFromCheckpointRequested;

        private Canvas hudCanvas;
        private RectTransform canvasRect;
        private Font uiFont;
        private Sprite circleFillSprite;
        private Sprite circleRingSprite;

        private Image curtainOverlay;

        private RectTransform topButtonsRoot;
        private Button retryBtn;
        private Button pauseBtn;
        private Text pauseBtnLabel;
        private Image pauseBtnBg;
        private Button muteBtn;
        private Text muteBtnLabel;

        // Cartao poetico (pergaminho com moldura dupla rosa/salvia e flores nos cantos)
        private RectTransform poeticRoot;
        private RectTransform poeticInnerBorder;
        private RectTransform flowerLeftRect;
        private RectTransform flowerRightRect;
        private Text poeticBodyText;
        private Text poeticFooterText;

        // Caixa de dialogo vitoriana com retrato circular (NANDA / YUNA)
        private RectTransform dialogueRoot;
        private RectTransform dialogueInnerBorder;
        private RectTransform portraitHaloRect;
        private RectTransform portraitDiscRect;
        private RectTransform portraitRingRect;
        private Image portraitHaloImg;
        private Image portraitRingImg;
        private Text portraitInitialText;
        private Text dialogueSpeakerText;
        private Text dialogueBodyText;
        private Text dialogueFooterText;

        private RectTransform bannerRoot;
        private CanvasGroup bannerGroup;
        private Text bannerText;

        private RectTransform preJumpRoot;
        private Text preJumpText;

        private RectTransform modalOverlay;
        private RectTransform modalCard;
        private Text modalHeaderText;
        private Text modalTitleText;
        private Text modalSubtitleText;
        private RectTransform modalPrimaryBtnRect;
        private Text modalPrimaryBtnLabel;
        private RectTransform modalSecondaryBtnRect;
        private Text modalSecondaryBtnLabel;

        private Action currentPrimaryAction;
        private Action currentSecondaryAction;

        private Vector2 lastCanvasSize = new Vector2(-1f, -1f);
        private float forcedLogicalWidth = -1f;
        private float forcedLogicalHeight = -1f;
        private bool built;

        private void Awake()
        {
            EnsureBuilt();
        }

        private void LateUpdate()
        {
            RefreshHud();
        }

        public void SetViewportOverride(float logicalWidth, float logicalHeight)
        {
            EnsureBuilt();
            forcedLogicalWidth = logicalWidth;
            forcedLogicalHeight = logicalHeight;
            lastCanvasSize = new Vector2(-1f, -1f);
            RefreshHud();
        }

        public void ClearViewportOverride()
        {
            forcedLogicalWidth = -1f;
            forcedLogicalHeight = -1f;
            lastCanvasSize = new Vector2(-1f, -1f);
            RefreshHud();
        }

        public void RefreshHud()
        {
            EnsureBuilt();
            if (GetPhase == null || canvasRect == null)
            {
                return;
            }

            Vector2 size = canvasRect.rect.size;
            if (size.x > 1f && size.y > 1f && (!Mathf.Approximately(size.x, lastCanvasSize.x) || !Mathf.Approximately(size.y, lastCanvasSize.y)))
            {
                lastCanvasSize = size;
                ApplyResponsiveLayout(size);
            }

            bool muted = IsMuted != null && IsMuted();
            bool paused = IsPaused != null && IsPaused();
            if (muteBtnLabel != null)
            {
                muteBtnLabel.text = muted ? "OFF" : "SOM";
            }

            if (pauseBtnLabel != null && pauseBtnBg != null)
            {
                pauseBtnLabel.text = paused ? "▶" : "II";
                pauseBtnBg.color = paused
                    ? new Color(0.95f, 0.76f, 0.25f, 0.95f)
                    : HudTheme.Plum;
                pauseBtnLabel.color = paused
                    ? Color.black
                    : new Color(0.992f, 0.878f, 0.278f, 1f);
            }

            HideAllPanels();

            string failure = GetFailure != null ? GetFailure() : null;
            if (!string.IsNullOrEmpty(failure))
            {
                ShowModal("✦ ERRO NA CENA ✦", failure, null, null, null);
                return;
            }

            if (paused)
            {
                ShowModal(
                    "✦ PAUSADO ✦",
                    "Jogo pausado",
                    "Toque em Continuar para retomar.",
                    "Continuar",
                    () => OnTogglePauseRequested?.Invoke(),
                    "Voltar ao início",
                    () => OnRetryFromBeginningRequested?.Invoke());
                return;
            }

            if (IsPlayerDead != null && IsPlayerDead())
            {
                ShowModal(
                    "✦ QUARTO ESCURO ✦",
                    "Você não conseguiu sair do quarto.",
                    "A penumbra engoliu o caminho... Mas a luz da fadinha ainda chama por você.",
                    "Tentar de novo",
                    () => OnRetryFromCheckpointRequested?.Invoke(),
                    "Voltar ao início",
                    () => OnRetryFromBeginningRequested?.Invoke());
                return;
            }

            float curtainAlpha = GetOpeningCurtainAlpha != null ? Mathf.Clamp01(GetOpeningCurtainAlpha()) : 0f;
            if (curtainOverlay != null)
            {
                curtainOverlay.gameObject.SetActive(curtainAlpha > 0.01f);
                curtainOverlay.color = new Color(0.012f, 0.020f, 0.047f, curtainAlpha);
            }

            bool isMobile = IsMobileViewport();
            string autoFooter = isMobile ? "Toque na tela para adiantar" : "Avança sozinho · Clique ou Enter adianta";
            string clickFooter = isMobile ? "Toque na tela para continuar" : "Clique ou pressione Espaço / Enter para continuar";

            int openingBeat = GetOpeningDialogueBeat != null ? GetOpeningDialogueBeat() : 0;
            var phase = GetPhase();

            switch (phase)
            {
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake1:
                    ShowDialogue(
                        "NANDA",
                        "Levanta, dorminhoca.\nTemos que brincar.\nVocê dormiu demais.",
                        autoFooter,
                        boldBody: false,
                        stackedBelowPoetic: false);
                    break;

                case StageFlowStateMachine.FlowPhase.OpeningPoeticIntro:
                    ShowPoetic(
                        openingBeat == 0
                            ? "O quarto era o mesmo.\nSó não a reconhecia."
                            : "Cada coisa guardava seu nome.\nEntre elas, nada dizia por onde ir.",
                        autoFooter);
                    break;

                case StageFlowStateMachine.FlowPhase.OpeningNandaWake2:
                    ShowDialogue(
                        "NANDA",
                        openingBeat == 0
                            ? "LEVANTA, SUA PREGUIÇOSA!"
                            : "SENÃO A GENTE NÃO SAI DAQUI NUNQUINHA!",
                        autoFooter,
                        boldBody: true,
                        stackedBelowPoetic: false);
                    break;

                case StageFlowStateMachine.FlowPhase.PreJumpCue:
                    float chapterAlpha = GetOpeningChapterBannerAlpha != null ? Mathf.Clamp01(GetOpeningChapterBannerAlpha()) : 0f;
                    if (chapterAlpha > 0.01f)
                    {
                        ShowBanner("CAPÍTULO 1 — Quarto de brinquedos", chapterAlpha);
                    }

                    if ((IsOpeningYunaInBed == null || !IsOpeningYunaInBed()) && curtainAlpha < 0.25f)
                    {
                        ShowPreJumpCue(isMobile ? "TOQUE NA TELA PARA PULAR" : "[ESPAÇO]  PRESSIONE ESPAÇO", chapterAlpha > 0.01f);
                    }
                    break;

                case StageFlowStateMachine.FlowPhase.CastleCutsceneStep0:
                    // Sem rodape duplicado no cartao poetico superior quando ha dialogo empilhado abaixo
                    ShowPoetic(DarkRoomDialogueScript.PoeticCastle, string.Empty);
                    ShowDialogue("NANDA", DarkRoomDialogueScript.CastleNanda.text, clickFooter, boldBody: false, stackedBelowPoetic: true);
                    break;

                case StageFlowStateMachine.FlowPhase.CastleCutsceneStep1:
                    ShowDialogue(
                        "YUNA",
                        DarkRoomDialogueScript.CastleYuna.text,
                        isMobile ? "Toque para iniciar A Fuga" : "Clique ou pressione Espaço / Enter para iniciar A Fuga",
                        boldBody: false,
                        stackedBelowPoetic: false);
                    break;

                case StageFlowStateMachine.FlowPhase.Stage2Gameplay:
                    if (GetStage2BannerTimer != null && GetStage2BannerTimer() > 0f)
                    {
                        ShowBanner("FASE 2 — A FUGA", 1f);
                    }
                    break;

                case StageFlowStateMachine.FlowPhase.Stage2Completed:
                    ShowModal(
                        "✦ A FUGA — PLATAFORMA 10 ✦",
                        "Você alcançou a Pista Elevada do Trem!",
                        "Fim do trecho com plataformas desenhadas em sequência (00 → 10).",
                        "Jogar novamente",
                        () => OnRetryFromBeginningRequested?.Invoke());
                    break;

                case StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0:
                    if (IsFalseDoorDialogueReady == null || IsFalseDoorDialogueReady())
                    {
                        ShowDialogue("YUNA", "Mas a porta não era ali...", clickFooter, boldBody: false, stackedBelowPoetic: false);
                    }
                    break;

                case StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep1:
                    ShowPoetic(DarkRoomDialogueScript.PoeticFalseDoor, string.Empty);
                    ShowDialogue(
                        "NANDA",
                        DarkRoomDialogueScript.FalseDoorNanda.text,
                        isMobile ? "Toque para iniciar A Subida Caótica" : "Clique ou pressione Espaço / Enter para iniciar A Subida Caótica",
                        boldBody: false,
                        stackedBelowPoetic: true);
                    break;

                case StageFlowStateMachine.FlowPhase.Stage3Gameplay:
                    if (GetStage2BannerTimer != null && GetStage2BannerTimer() > 0f)
                    {
                        ShowBanner("FASE 3 — A SUBIDA CAÓTICA", 1f);
                    }
                    break;

                case StageFlowStateMachine.FlowPhase.TruePortalVictory:
                    break;
            }
        }

        private void EnsureBuilt()
        {
            if (built)
            {
                return;
            }

            built = true;
            EnsureEventSystem();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            circleFillSprite = CreateCircleSprite(128, 0f, 0.96f);
            circleRingSprite = CreateCircleSprite(128, 0.80f, 0.98f);

            var canvasGo = new GameObject("DarkRoom_HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            hudCanvas = canvasGo.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 60;
            canvasRect = canvasGo.GetComponent<RectTransform>();

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            BuildCurtain(canvasGo.transform);
            BuildTopRightButtons(canvasGo.transform);
            BuildPoeticCard(canvasGo.transform);
            BuildDialogueBox(canvasGo.transform);
            BuildStageBanner(canvasGo.transform);
            BuildPreJumpCue(canvasGo.transform);
            BuildModalCard(canvasGo.transform);

            HideAllPanels();
            ApplyResponsiveLayout(canvasRect.rect.size.x > 1f ? canvasRect.rect.size : new Vector2(1280f, 720f));
        }

        private void BuildCurtain(Transform parent)
        {
            var go = new GameObject("OpeningCurtain", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>());
            curtainOverlay = go.GetComponent<Image>();
            curtainOverlay.color = new Color(0.012f, 0.020f, 0.047f, 0f);
            curtainOverlay.raycastTarget = false;
            go.SetActive(false);
        }

        private void BuildTopRightButtons(Transform parent)
        {
            var container = new GameObject("TopRightButtons", typeof(RectTransform));
            container.transform.SetParent(parent, false);
            topButtonsRoot = container.GetComponent<RectTransform>();
            SetAnchorTopRight(topButtonsRoot, -14f, -10f, 160f, 36f);

            muteBtn = CreateTopUtilityButton("Btn_Mute", container.transform, "SOM", out muteBtnLabel, out _, () => OnToggleMuteRequested?.Invoke());
            pauseBtn = CreateTopUtilityButton("Btn_Pause", container.transform, "II", out pauseBtnLabel, out pauseBtnBg, () => OnTogglePauseRequested?.Invoke());
            retryBtn = CreateTopUtilityButton("Btn_Retry", container.transform, "R", out _, out _, () => OnRetryFromBeginningRequested?.Invoke());
        }

        private Button CreateTopUtilityButton(string name, Transform parent, string label, out Text txt, out Image bg, Action onClick)
        {
            var go = CreateBorderedBox(
                name,
                parent,
                new Color(0.102f, 0.063f, 0.149f, 0.90f),
                new Color(0.980f, 0.800f, 0.082f, 0.90f),
                1.5f);
            bg = go.GetComponent<Image>();
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onClick?.Invoke());

            HudTheme.Button(btn);
            txt = CreateText("Label", go.transform, label, 16, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(txt.rectTransform);
            return btn;
        }

        private void BuildPoeticCard(Transform parent)
        {
            var go = CreateBorderedBox(
                "PoeticCard",
                parent,
                new Color(1.0f, 0.961f, 0.898f, 0.96f),
                new Color(0.839f, 0.682f, 0.694f, 0.96f),
                2.5f);
            poeticRoot = go.GetComponent<RectTransform>();

            var inner = CreateBorderedBox(
                "InnerBorder",
                go.transform,
                new Color(0f, 0f, 0f, 0f),
                new Color(0.722f, 0.788f, 0.733f, 0.95f),
                1.5f);
            poeticInnerBorder = inner.GetComponent<RectTransform>();
            poeticInnerBorder.anchorMin = Vector2.zero;
            poeticInnerBorder.anchorMax = Vector2.one;
            poeticInnerBorder.offsetMin = new Vector2(6f, 6f);
            poeticInnerBorder.offsetMax = new Vector2(-6f, -6f);
            inner.GetComponent<Image>().raycastTarget = false;

            flowerLeftRect = BuildCornerFlower(go.transform, new Vector2(0f, 1f), new Vector2(14f, -14f));
            flowerRightRect = BuildCornerFlower(go.transform, new Vector2(1f, 1f), new Vector2(-14f, -14f));

            poeticBodyText = CreateText("PoeticBody", go.transform, string.Empty, 16, new Color(0.224f, 0.141f, 0.133f, 1f), TextAnchor.MiddleCenter, FontStyle.Italic);
            poeticFooterText = CreateText("PoeticFooter", go.transform, string.Empty, 14, new Color(0.518f, 0.392f, 0.404f, 0.92f), TextAnchor.LowerCenter, FontStyle.Normal);

            var btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => OnAdvanceRequested?.Invoke());
        }

        private void BuildDialogueBox(Transform parent)
        {
            var go = CreateBorderedBox(
                "DialogueBox",
                parent,
                new Color(0.086f, 0.063f, 0.141f, 0.95f),
                new Color(0.992f, 0.878f, 0.278f, 0.95f),
                2.2f);
            dialogueRoot = go.GetComponent<RectTransform>();

            var inner = CreateBorderedBox(
                "InnerBorder",
                go.transform,
                new Color(0f, 0f, 0f, 0f),
                new Color(1f, 1f, 1f, 0.25f),
                1f);
            dialogueInnerBorder = inner.GetComponent<RectTransform>();
            dialogueInnerBorder.anchorMin = Vector2.zero;
            dialogueInnerBorder.anchorMax = Vector2.one;
            dialogueInnerBorder.offsetMin = new Vector2(5f, 5f);
            dialogueInnerBorder.offsetMax = new Vector2(-5f, -5f);
            inner.GetComponent<Image>().raycastTarget = false;

            // Retrato circular vitoriano (halo + disco escuro + anel dourado/rosa + inicial)
            var haloGo = new GameObject("PortraitHalo", typeof(RectTransform), typeof(Image));
            haloGo.transform.SetParent(go.transform, false);
            portraitHaloRect = haloGo.GetComponent<RectTransform>();
            portraitHaloRect.anchorMin = new Vector2(0f, 0.5f);
            portraitHaloRect.anchorMax = new Vector2(0f, 0.5f);
            portraitHaloRect.pivot = new Vector2(0.5f, 0.5f);
            portraitHaloImg = haloGo.GetComponent<Image>();
            portraitHaloImg.sprite = circleFillSprite;
            portraitHaloImg.color = new Color(0.02f, 0.71f, 0.83f, 0.35f);
            portraitHaloImg.raycastTarget = false;

            var discGo = new GameObject("PortraitDisc", typeof(RectTransform), typeof(Image));
            discGo.transform.SetParent(go.transform, false);
            portraitDiscRect = discGo.GetComponent<RectTransform>();
            portraitDiscRect.anchorMin = new Vector2(0f, 0.5f);
            portraitDiscRect.anchorMax = new Vector2(0f, 0.5f);
            portraitDiscRect.pivot = new Vector2(0.5f, 0.5f);
            var discImg = discGo.GetComponent<Image>();
            discImg.sprite = circleFillSprite;
            discImg.color = new Color(0.086f, 0.063f, 0.141f, 1f);
            discImg.raycastTarget = false;

            var ringGo = new GameObject("PortraitRing", typeof(RectTransform), typeof(Image));
            ringGo.transform.SetParent(discGo.transform, false);
            portraitRingRect = ringGo.GetComponent<RectTransform>();
            Stretch(portraitRingRect);
            portraitRingImg = ringGo.GetComponent<Image>();
            portraitRingImg.sprite = circleRingSprite;
            portraitRingImg.color = new Color(0.992f, 0.878f, 0.278f, 1f);
            portraitRingImg.raycastTarget = false;

            portraitInitialText = CreateText("PortraitInitial", discGo.transform, "N", 16, new Color(0.22f, 0.74f, 0.97f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(portraitInitialText.rectTransform);

            dialogueSpeakerText = CreateText("Speaker", go.transform, "NANDA", 14, new Color(0.22f, 0.74f, 0.97f, 1f), TextAnchor.UpperLeft, FontStyle.Bold);
            dialogueBodyText = CreateText("Body", go.transform, string.Empty, 17, HudTheme.Ink, TextAnchor.MiddleLeft, FontStyle.Italic);
            dialogueFooterText = CreateText("Footer", go.transform, string.Empty, 16, HudTheme.Plum, TextAnchor.LowerRight, FontStyle.Normal);

            var btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => OnAdvanceRequested?.Invoke());
        }

        private void BuildStageBanner(Transform parent)
        {
            var go = CreateBorderedBox(
                "StageBanner",
                parent,
                new Color(0.086f, 0.063f, 0.141f, 0.94f),
                new Color(0.980f, 0.800f, 0.082f, 0.95f),
                1.8f);
            bannerRoot = go.GetComponent<RectTransform>();
            bannerGroup = go.AddComponent<CanvasGroup>();
            bannerText = CreateText("BannerText", go.transform, string.Empty, 16, new Color(0.992f, 0.878f, 0.278f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(bannerText.rectTransform);
        }

        private void BuildPreJumpCue(Transform parent)
        {
            var go = CreateBorderedBox(
                "PreJumpCue",
                parent,
                new Color(0.086f, 0.063f, 0.141f, 0.92f),
                new Color(0.980f, 0.800f, 0.082f, 0.90f),
                1.5f);
            preJumpRoot = go.GetComponent<RectTransform>();
            preJumpText = CreateText("CueText", go.transform, string.Empty, 15, new Color(0.992f, 0.94f, 0.82f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(preJumpText.rectTransform);
        }

        private void BuildModalCard(Transform parent)
        {
            var overlayGo = new GameObject("ModalOverlay", typeof(RectTransform), typeof(Image));
            overlayGo.transform.SetParent(parent, false);
            modalOverlay = overlayGo.GetComponent<RectTransform>();
            Stretch(modalOverlay);
            overlayGo.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.72f);

            var cardGo = CreateBorderedBox(
                "ModalCard",
                overlayGo.transform,
                new Color(0.102f, 0.063f, 0.149f, 0.96f),
                new Color(0.980f, 0.800f, 0.082f, 0.95f),
                2f);
            modalCard = cardGo.GetComponent<RectTransform>();
            modalCard.anchorMin = new Vector2(0.5f, 0.5f);
            modalCard.anchorMax = new Vector2(0.5f, 0.5f);
            modalCard.pivot = new Vector2(0.5f, 0.5f);

            modalHeaderText = CreateText("Header", cardGo.transform, string.Empty, 16, new Color(0.48f, 0.20f, 0.29f, 1f), TextAnchor.UpperCenter, FontStyle.Bold);
            modalTitleText = CreateText("Title", cardGo.transform, string.Empty, 18, HudTheme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            modalSubtitleText = CreateText("Subtitle", cardGo.transform, string.Empty, 16, HudTheme.Plum, TextAnchor.MiddleCenter, FontStyle.Italic);

            var primaryGo = CreateBorderedBox(
                "PrimaryBtn",
                cardGo.transform,
                new Color(0.96f, 0.76f, 0.24f, 1f),
                new Color(1f, 0.92f, 0.58f, 0.95f),
                1.2f);
            modalPrimaryBtnRect = primaryGo.GetComponent<RectTransform>();
            var pBtn = primaryGo.AddComponent<Button>();
            pBtn.targetGraphic = primaryGo.GetComponent<Image>();
            pBtn.onClick.AddListener(() => currentPrimaryAction?.Invoke());
            modalPrimaryBtnLabel = CreateText("Label", primaryGo.transform, string.Empty, 14, new Color(0.10f, 0.08f, 0.05f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(modalPrimaryBtnLabel.rectTransform);

            var secondaryGo = CreateBorderedBox(
                "SecondaryBtn",
                cardGo.transform,
                new Color(0.18f, 0.12f, 0.26f, 1f),
                new Color(0.980f, 0.800f, 0.082f, 0.85f),
                1.2f);
            modalSecondaryBtnRect = secondaryGo.GetComponent<RectTransform>();
            var sBtn = secondaryGo.AddComponent<Button>();
            sBtn.targetGraphic = secondaryGo.GetComponent<Image>();
            sBtn.onClick.AddListener(() => currentSecondaryAction?.Invoke());
            HudTheme.Button(sBtn);
            modalSecondaryBtnLabel = CreateText("Label", secondaryGo.transform, string.Empty, 14, new Color(0.996f, 0.980f, 0.880f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(modalSecondaryBtnLabel.rectTransform);
        }

        private void ApplyResponsiveLayout(Vector2 canvasSize)
        {
            float px = ComputePxToCanvas(canvasSize);
            bool isMobile = IsMobileViewport();
            bool isLandscape = canvasSize.x > canvasSize.y;
            float marginX = 12f * px;
            float maxCardW = Mathf.Min(isMobile ? (isLandscape ? 520f * px : canvasSize.x - marginX * 2f) : 560f * px, canvasSize.x - marginX * 2f);

            int f14 = Mathf.RoundToInt(16f * px);
            int f15 = Mathf.RoundToInt(17f * px);
            int f16 = Mathf.RoundToInt(18f * px);

            // Botoes superiores direitos (R, II, SOM)
            float btnH = 48f * px;
            float btnGap = 6f * px;
            float muteW = 58f * px;
            float sqW = 48f * px;
            float topBarY = -8f * px;
            SetAnchorTopRight(topButtonsRoot, -marginX, topBarY, muteW + sqW * 2f + btnGap * 2f, btnH);

            SetAnchorTopRight(muteBtn.GetComponent<RectTransform>(), 0f, 0f, muteW, btnH);
            SetAnchorTopRight(pauseBtn.GetComponent<RectTransform>(), -(muteW + btnGap), 0f, sqW, btnH);
            SetAnchorTopRight(retryBtn.GetComponent<RectTransform>(), -(muteW + sqW + btnGap * 2f), 0f, sqW, btnH);
            muteBtnLabel.fontSize = f14;
            pauseBtnLabel.fontSize = f14;
            retryBtn.GetComponentInChildren<Text>().fontSize = f14;

            float topContentY = -(btnH + 14f * px);

            // Cartao poetico em pergaminho com flores nos cantos
            float poeticH = 104f * px;
            SetAnchorTopCenter(poeticRoot, 0f, topContentY, maxCardW, poeticH);
            poeticInnerBorder.offsetMin = new Vector2(5f * px, 5f * px);
            poeticInnerBorder.offsetMax = new Vector2(-5f * px, -5f * px);

            float flowerOffset = 12f * px;
            float flowerScale = Mathf.Clamp(px * 0.72f, 0.65f, 1.4f);
            flowerLeftRect.anchoredPosition = new Vector2(flowerOffset, -flowerOffset);
            flowerLeftRect.localScale = Vector3.one * flowerScale;
            flowerRightRect.anchoredPosition = new Vector2(-flowerOffset, -flowerOffset);
            flowerRightRect.localScale = Vector3.one * flowerScale;

            poeticBodyText.fontSize = f16;
            SetInsetRect(poeticBodyText.rectTransform, 18f * px, 28f * px, 18f * px, 10f * px);
            poeticFooterText.fontSize = f14;
            SetInsetRect(poeticFooterText.rectTransform, 14f * px, 9f * px, 14f * px, poeticH - 27f * px);

            // Caixa de dialogo vitoriana com retrato circular — NO TOPO
            float dlgH = isLandscape ? 104f * px : 124f * px;
            SetAnchorTopCenter(dialogueRoot, 0f, topContentY, maxCardW, dlgH);
            dialogueInnerBorder.offsetMin = new Vector2(4f * px, 4f * px);
            dialogueInnerBorder.offsetMax = new Vector2(-4f * px, -4f * px);

            float portraitSize = isLandscape ? 42f * px : 48f * px;
            float portraitX = 34f * px;
            portraitHaloRect.anchoredPosition = new Vector2(portraitX, 0f);
            portraitHaloRect.sizeDelta = new Vector2(portraitSize + 10f * px, portraitSize + 10f * px);
            portraitDiscRect.anchoredPosition = new Vector2(portraitX, 0f);
            portraitDiscRect.sizeDelta = new Vector2(portraitSize, portraitSize);
            portraitInitialText.fontSize = f16;

            float textLeft = portraitX + portraitSize * 0.5f + 12f * px;
            dialogueSpeakerText.fontSize = f14;
            SetInsetRect(dialogueSpeakerText.rectTransform, textLeft, dlgH - 24f * px, 14f * px, 8f * px);
            dialogueBodyText.fontSize = f15;
            SetInsetRect(dialogueBodyText.rectTransform, textLeft, 28f * px, 14f * px, 26f * px);
            dialogueFooterText.fontSize = f14;
            SetInsetRect(dialogueFooterText.rectTransform, textLeft, 9f * px, 14f * px, dlgH - 27f * px);

            // Banner de fase
            float bannerW = Mathf.Min(340f * px, maxCardW);
            float bannerH = 38f * px;
            SetAnchorTopCenter(bannerRoot, 0f, topContentY, bannerW, bannerH);
            bannerText.fontSize = f16;

            // Cue pre-pulo
            float cueW = Mathf.Min(280f * px, maxCardW);
            float cueH = 36f * px;
            SetAnchorTopCenter(preJumpRoot, 0f, topContentY, cueW, cueH);
            preJumpText.fontSize = f15;

            // Modal centralizado
            float modalW = Mathf.Min(356f * px, canvasSize.x - marginX * 2f);
            float modalH = 244f * px;
            modalCard.sizeDelta = new Vector2(modalW, modalH);
            modalCard.anchoredPosition = Vector2.zero;

            modalHeaderText.fontSize = f14;
            SetInsetRect(modalHeaderText.rectTransform, 12f * px, modalH - 30f * px, 12f * px, 10f * px);
            modalTitleText.fontSize = f16;
            SetInsetRect(modalTitleText.rectTransform, 14f * px, modalH - 80f * px, 14f * px, 34f * px);
            modalSubtitleText.fontSize = f14;
            SetInsetRect(modalSubtitleText.rectTransform, 14f * px, 64f * px, 14f * px, 84f * px);

            modalPrimaryBtnLabel.fontSize = f14;
            modalSecondaryBtnLabel.fontSize = f14;
        }

        private void HideAllPanels()
        {
            if (curtainOverlay != null) curtainOverlay.gameObject.SetActive(false);
            if (poeticRoot != null) poeticRoot.gameObject.SetActive(false);
            if (dialogueRoot != null) dialogueRoot.gameObject.SetActive(false);
            if (bannerRoot != null) bannerRoot.gameObject.SetActive(false);
            if (preJumpRoot != null) preJumpRoot.gameObject.SetActive(false);
            if (modalOverlay != null) modalOverlay.gameObject.SetActive(false);
        }

        private void ShowPoetic(string text, string footer)
        {
            poeticRoot.gameObject.SetActive(true);
            poeticBodyText.text = text;
            bool hasFooter = !string.IsNullOrEmpty(footer);
            poeticFooterText.gameObject.SetActive(hasFooter);
            poeticFooterText.text = footer ?? string.Empty;

            float px = ComputePxToCanvas(canvasRect.rect.size);
            float h = hasFooter ? 104f * px : 80f * px;
            poeticRoot.sizeDelta = new Vector2(poeticRoot.sizeDelta.x, h);
            SetInsetRect(poeticBodyText.rectTransform, 18f * px, hasFooter ? 28f * px : 10f * px, 18f * px, 10f * px);
        }

        private void ShowDialogue(string speaker, string body, string footer, bool boldBody, bool stackedBelowPoetic)
        {
            dialogueRoot.gameObject.SetActive(true);
            bool isNanda = string.Equals(speaker, "NANDA", StringComparison.OrdinalIgnoreCase);

            dialogueSpeakerText.text = speaker;
            dialogueSpeakerText.color = isNanda
                ? new Color(0.08f, 0.34f, 0.48f, 1f)
                : new Color(0.55f, 0.19f, 0.28f, 1f);
            portraitRingImg.color = isNanda
                ? new Color(0.992f, 0.878f, 0.278f, 1f)
                : new Color(0.96f, 0.45f, 0.71f, 1f);
            portraitHaloImg.color = isNanda
                ? new Color(0.02f, 0.71f, 0.83f, 0.35f)
                : new Color(0.96f, 0.45f, 0.71f, 0.35f);
            portraitInitialText.text = isNanda ? "N" : "Y";
            portraitInitialText.color = isNanda ? new Color(0.45f, 0.88f, 1f, 1f) : new Color(1f, 0.72f, 0.78f, 1f);

            dialogueBodyText.text = body;
            dialogueBodyText.fontStyle = boldBody ? FontStyle.Bold : FontStyle.Italic;
            dialogueFooterText.text = footer;

            float px = ComputePxToCanvas(canvasRect.rect.size);
            float topContentY = -(48f * px + 14f * px);
            float y = stackedBelowPoetic ? topContentY - poeticRoot.sizeDelta.y - 6f * px : topContentY;
            dialogueRoot.anchoredPosition = new Vector2(0f, y);
        }

        private void ShowBanner(string title, float alpha)
        {
            bannerRoot.gameObject.SetActive(true);
            bannerGroup.alpha = alpha;
            bannerText.text = title;
        }

        private void ShowPreJumpCue(string label, bool belowBanner)
        {
            preJumpRoot.gameObject.SetActive(true);
            preJumpText.text = label;
            float px = ComputePxToCanvas(canvasRect.rect.size);
            float topContentY = -(48f * px + 14f * px);
            float y = belowBanner ? topContentY - bannerRoot.sizeDelta.y - 6f * px : topContentY;
            preJumpRoot.anchoredPosition = new Vector2(0f, y);
        }

        private void ShowModal(
            string header,
            string title,
            string subtitle,
            string primaryLabel,
            Action primaryAction,
            string secondaryLabel = null,
            Action secondaryAction = null)
        {
            modalOverlay.gameObject.SetActive(true);
            modalHeaderText.text = header;
            modalTitleText.text = title;
            modalSubtitleText.text = subtitle ?? string.Empty;
            modalSubtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));

            currentPrimaryAction = primaryAction;
            currentSecondaryAction = secondaryAction;

            bool hasPrimary = !string.IsNullOrEmpty(primaryLabel) && primaryAction != null;
            bool hasSecondary = !string.IsNullOrEmpty(secondaryLabel) && secondaryAction != null;

            modalPrimaryBtnRect.gameObject.SetActive(hasPrimary);
            modalSecondaryBtnRect.gameObject.SetActive(hasSecondary);

            float px = ComputePxToCanvas(canvasRect.rect.size);
            float cardW = modalCard.sizeDelta.x;
            float btnH = 48f * px;
            float bottomY = 14f * px;

            if (hasPrimary && hasSecondary)
            {
                float gap = 10f * px;
                float bw = (cardW - 28f * px - gap) * 0.5f;
                modalPrimaryBtnLabel.text = primaryLabel;
                modalSecondaryBtnLabel.text = secondaryLabel;

                modalPrimaryBtnRect.anchorMin = new Vector2(0.5f, 0f);
                modalPrimaryBtnRect.anchorMax = new Vector2(0.5f, 0f);
                modalPrimaryBtnRect.pivot = new Vector2(1f, 0f);
                modalPrimaryBtnRect.anchoredPosition = new Vector2(-gap * 0.5f, bottomY);
                modalPrimaryBtnRect.sizeDelta = new Vector2(bw, btnH);

                modalSecondaryBtnRect.anchorMin = new Vector2(0.5f, 0f);
                modalSecondaryBtnRect.anchorMax = new Vector2(0.5f, 0f);
                modalSecondaryBtnRect.pivot = new Vector2(0f, 0f);
                modalSecondaryBtnRect.anchoredPosition = new Vector2(gap * 0.5f, bottomY);
                modalSecondaryBtnRect.sizeDelta = new Vector2(bw, btnH);
            }
            else if (hasPrimary)
            {
                float bw = Mathf.Min(210f * px, cardW - 28f * px);
                modalPrimaryBtnLabel.text = primaryLabel;

                modalPrimaryBtnRect.anchorMin = new Vector2(0.5f, 0f);
                modalPrimaryBtnRect.anchorMax = new Vector2(0.5f, 0f);
                modalPrimaryBtnRect.pivot = new Vector2(0.5f, 0f);
                modalPrimaryBtnRect.anchoredPosition = new Vector2(0f, bottomY);
                modalPrimaryBtnRect.sizeDelta = new Vector2(bw, btnH);
            }
        }

        private float ComputePxToCanvas(Vector2 canvasSize)
        {
            float logicalW = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float logicalH = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;

            if (logicalW <= 1f || logicalH <= 1f || canvasSize.x <= 1f)
            {
                return 1f;
            }

            float minDim = Mathf.Min(logicalW, logicalH);
            if (forcedLogicalWidth <= 0f && Application.isMobilePlatform && minDim >= 700f)
            {
                float dpr = Mathf.Clamp(minDim / 390f, 1.5f, 3.5f);
                logicalW /= dpr;
            }

            return Mathf.Clamp(canvasSize.x / Mathf.Max(280f, logicalW), 0.5f, 4.0f);
        }

        private bool IsMobileViewport()
        {
            float w = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float h = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;
            return Application.isMobilePlatform || Touchscreen.current != null || w <= 960f || h <= 540f;
        }

        private static RectTransform BuildCornerFlower(Transform parent, Vector2 anchor, Vector2 offset)
        {
            var flower = new GameObject("CornerFlower", typeof(RectTransform));
            flower.transform.SetParent(parent, false);
            var rt = flower.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(16f, 16f);

            var petalColor = new Color(0.898f, 0.745f, 0.765f, 1f);
            Vector2[] offsets = { new(0f, 4.5f), new(0f, -4.5f), new(-4.5f, 0f), new(4.5f, 0f) };
            for (int i = 0; i < offsets.Length; i++)
            {
                var p = new GameObject($"Petal_{i}", typeof(RectTransform), typeof(Image));
                p.transform.SetParent(flower.transform, false);
                var prt = p.GetComponent<RectTransform>();
                prt.anchorMin = new Vector2(0.5f, 0.5f);
                prt.anchorMax = new Vector2(0.5f, 0.5f);
                prt.pivot = new Vector2(0.5f, 0.5f);
                prt.anchoredPosition = offsets[i];
                prt.sizeDelta = new Vector2(6f, 6f);
                var pImg = p.GetComponent<Image>();
                pImg.color = petalColor;
                pImg.raycastTarget = false;
            }

            var center = new GameObject("Center", typeof(RectTransform), typeof(Image));
            center.transform.SetParent(flower.transform, false);
            var crt = center.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(5f, 5f);
            var cImg = center.GetComponent<Image>();
            cImg.color = new Color(0.776f, 0.682f, 0.475f, 1f);
            cImg.raycastTarget = false;
            return rt;
        }

        private static Sprite CreateCircleSprite(int size, float innerRadiusRatio, float outerRadiusRatio)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            float half = (size - 1) * 0.5f;
            float invHalf = 1f / half;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                float dy = (y - half) * invHalf;
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) * invHalf;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float outer = 1f - SmoothRange(outerRadiusRatio - 0.04f, outerRadiusRatio, r);
                    float inner = innerRadiusRatio > 0f ? SmoothRange(innerRadiusRatio - 0.04f, innerRadiusRatio, r) : 1f;
                    byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(outer * inner * 255f), 0, 255);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float SmoothRange(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static GameObject CreateBorderedBox(string name, Transform parent, Color bgColor, Color borderColor, float borderWidth)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = bgColor;
            HudTheme.Round(img);
            if (name == "DialogueBox" || name == "ModalCard")
            {
                img.color = HudTheme.Paper;
            }
            else if (bgColor.a > 0f && bgColor.maxColorComponent < 0.3f)
            {
                img.color = HudTheme.Plum;
            }
            var outline = go.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(borderWidth, -borderWidth);
            return go;
        }

        private Text CreateText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor align, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var txt = go.GetComponent<Text>();
            txt.font = uiFont;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = style;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetAnchorTopCenter(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void SetAnchorTopRight(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void SetInsetRect(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }
    }
}
