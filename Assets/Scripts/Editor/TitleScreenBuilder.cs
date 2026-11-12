using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using YunaLegend.EditorScripts;
using YunaLegend.UI;

namespace YunaLegend.EditorScripts
{
    /// <summary>
    /// Constroi a tela de entrada do Quarto dos Brinquedos a partir da arte canonica
    /// (fundo recortado, sem textos embutidos) e do menu canonico: Novo jogo,
    /// Carregar jogo, Modo facil, rodape de save e engrenagem de configuracoes.
    /// </summary>
    public static class TitleScreenBuilder
    {
        private const string ScenePath = "Assets/Scenes/TitleScreen.unity";
        private const string GameplayPath = "Assets/Scenes/DarkRoom_Gameplay.unity";
        private const string ArtFolder = "Assets/Art/UI";
        private const string EvidenceFolder = "Evidence/title-screen-2026-11-11";

        // Fases disponiveis no dropdown da bandeirinha. Novas fases entram aqui.
        private static readonly string[] PhaseLabels = { "O Quarto Escuro" };
        private static readonly string[] PhaseScenes = { "DarkRoom_Gameplay" };

        private static readonly Color Cream = new Color(0.95f, 0.90f, 0.78f);
        private static readonly Color Gold = new Color(1f, 0.84f, 0.55f);
        private static readonly Color Disabled = new Color(0.52f, 0.48f, 0.42f);
        private static readonly Color PanelColor = new Color(0.055f, 0.04f, 0.03f, 0.96f);

        private static Font uiFont;

        [MenuItem("YunaLegend/Construir Tela de Entrada")]
        public static void Build()
        {
            var background = LoadSprite($"{ArtFolder}/Title_Background.png", 2048);
            var shade = LoadSprite($"{ArtFolder}/Title_LeftShade.png", 2048);
            var gear = LoadSprite($"{ArtFolder}/Title_Gear.png", 256);
            var leg = LoadSprite($"{ArtFolder}/Title_Leg.png", 512);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Title_Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.012f, 0.008f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            var canvasGo = new GameObject("Title_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var root = canvasGo.transform;

            var backgroundImage = AddImage("Fundo_Canonico", root, background, Color.white);
            Stretch(backgroundImage.rectTransform);

            // Perna sobreposta (mesmo recorte da arte) animada como pendulo curto.
            var legImage = AddImage("Perna_Balanco", root, leg, Color.white);
            var legRect = legImage.rectTransform;
            // Recorte da perna na arte: x 1300..1540, y 820..1080 de 1920x1080.
            legRect.anchorMin = new Vector2(1300f / 1920f, 0f);
            legRect.anchorMax = new Vector2(1540f / 1920f, (1080f - 820f) / 1080f);
            legRect.offsetMin = Vector2.zero;
            legRect.offsetMax = Vector2.zero;
            legRect.pivot = new Vector2(0.34f, 0.97f);
            var swing = legImage.gameObject.AddComponent<TitleLegSwing>();
            swing.Configure(2f, 3.4f);

            var shadeImage = AddImage("Sombra_Lateral", root, shade, Color.white);
            Stretch(shadeImage.rectTransform);

            var title = AddText("Titulo", root, "O Quarto\ndos Brinquedos", 92, Cream, TextAnchor.UpperLeft);
            AnchorTopLeft(title.rectTransform, 130f, -180f, 1000f, 320f);
            title.lineSpacing = 0.95f;
            var titleShadow = title.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            titleShadow.effectDistance = new Vector2(3f, -3f);

            var newGame = AddMenuButton("Novo jogo", root, 140f, -486f, 560f);
            var loadGame = AddMenuButton("Carregar jogo", root, 140f, -556f, 560f);
            var easyMode = AddMenuButton("Modo fácil", root, 140f, -626f, 360f);

            var easyTag = AddText("ModoFacil_Estado", root, "Desativado", 22, new Color(0.66f, 0.62f, 0.55f), TextAnchor.MiddleLeft);
            AnchorTopLeft(easyTag.rectTransform, 408f, -626f, 220f, 56f);

            var focusMarker = AddText("Marcador_Foco", root, "›", 40, Gold, TextAnchor.MiddleRight);
            AnchorTopLeft(focusMarker.rectTransform, 92f, -486f, 26f, 56f);

            var footer = AddText("Rodape_Save", root, "Nenhuma aventura salva neste navegador.", 22,
                new Color(0.78f, 0.73f, 0.64f), TextAnchor.MiddleLeft);
            AnchorTopLeft(footer.rectTransform, 140f, -712f, 900f, 40f);

            var settingsButton = AddIconButton("Engrenagem_Configuracoes", root, gear);
            var settingsRect = settingsButton.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1f, 1f);
            settingsRect.anchorMax = new Vector2(1f, 1f);
            settingsRect.pivot = new Vector2(1f, 1f);
            settingsRect.anchoredPosition = new Vector2(-58f, -50f);
            settingsRect.sizeDelta = new Vector2(64f, 64f);

            var panel = BuildSettingsPanel(root, out var volumeSlider, out var closeButton);
            panel.SetActive(false);

            var controllerGo = new GameObject("TitleScreenController");
            var controller = controllerGo.AddComponent<TitleScreenController>();
            controller.Configure(
                newGame, loadGame, easyMode, easyTag, footer, focusMarker,
                settingsButton, panel, volumeSlider, closeButton, "DarkRoom_Gameplay");

            // Bandeirinha no topo: dropdown para escolher a fase.
            var flagSprite = LoadSprite($"{ArtFolder}/Title_Flag.png", 256);
            BuildPhaseSelector(root, flagSprite, out var flagButton, out var phasesRoot,
                out var phasesBackdrop, out var phaseButtons, out var phaseLabels);
            var selector = new GameObject("Seletor_Fase").AddComponent<TitlePhaseSelector>();
            selector.Configure(controller, flagButton, phasesRoot, phasesBackdrop, phaseButtons, phaseLabels, PhaseScenes);
            phasesRoot.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterBuildScenes();

            Debug.Log("TITLE_SCREEN_BUILT " + ScenePath);
        }

        /// <summary>Valida a cena gerada sem depender do Test Runner (usado em lote).</summary>
        public static void Validate()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var controller = Object.FindFirstObjectByType<TitleScreenController>();
            if (controller == null) throw new System.InvalidOperationException("TitleScreenController ausente.");
            if (controller.GameplaySceneName != "DarkRoom_Gameplay")
                throw new System.InvalidOperationException("Cena de destino inesperada: " + controller.GameplaySceneName);

            if (Object.FindFirstObjectByType<Canvas>() == null) throw new System.InvalidOperationException("Canvas ausente.");
            if (Object.FindFirstObjectByType<EventSystem>() == null) throw new System.InvalidOperationException("EventSystem ausente.");

            var texts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (string required in new[] { "O Quarto\ndos Brinquedos", "Novo jogo", "Carregar jogo", "Modo fácil", "Desativado" })
            {
                bool found = false;
                foreach (var text in texts) if (text.text == required) { found = true; break; }
                if (!found) throw new System.InvalidOperationException("Texto canonico ausente: " + required.Replace("\n", " "));
            }

            var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (buttons.Length < 4) throw new System.InvalidOperationException("Botoes insuficientes: " + buttons.Length);

            var swing = Object.FindFirstObjectByType<TitleLegSwing>();
            if (swing == null || swing.AmplitudeDegrees <= 0f)
                throw new System.InvalidOperationException("Perna animada ausente na tela de entrada.");

            var selector = Object.FindFirstObjectByType<TitlePhaseSelector>();
            if (selector == null) throw new System.InvalidOperationException("Seletor de fases ausente.");
            if (selector.ItemCount < 1) throw new System.InvalidOperationException("Dropdown de fases sem itens.");
            if (Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .All(b => b.name != "Bandeira_Fases"))
                throw new System.InvalidOperationException("Bandeirinha de fases ausente.");

            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length < 1 || scenes[0].path != ScenePath)
                throw new System.InvalidOperationException("Build Settings nao comeca pela tela de entrada.");

            Debug.Log("TITLE_SCREEN_VALIDATED buttons=" + buttons.Length + " texts=" + texts.Length);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _ = scene;
        }

        [MenuItem("YunaLegend/Capturar Tela de Entrada")]
        public static void Capture()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var camera = Object.FindFirstObjectByType<Camera>();
            camera.aspect = 16f / 9f;
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, 1920, 1080);
            System.IO.Directory.CreateDirectory(EvidenceFolder);
            System.IO.File.WriteAllBytes($"{EvidenceFolder}/01-title.png", image.EncodeToPNG());
            Object.DestroyImmediate(image);

            // Segundo quadro com a perna no extremo do balanco, para provar o movimento.
            var swing = Object.FindFirstObjectByType<TitleLegSwing>();
            if (swing != null)
            {
                swing.SetAngle(swing.AmplitudeDegrees);
                var swung = DarkRoomStartCapture.RenderDisplayImage(camera, 1920, 1080);
                System.IO.File.WriteAllBytes($"{EvidenceFolder}/02-title-leg-swing.png", swung.EncodeToPNG());
                Object.DestroyImmediate(swung);
                swing.SetAngle(0f);
            }

            // Terceiro quadro com o dropdown de fases aberto.
            var selector = Object.FindFirstObjectByType<TitlePhaseSelector>();
            if (selector != null)
            {
                selector.Open();
                var phases = DarkRoomStartCapture.RenderDisplayImage(camera, 1920, 1080);
                System.IO.File.WriteAllBytes($"{EvidenceFolder}/04-title-phases.png", phases.EncodeToPNG());
                Object.DestroyImmediate(phases);
                selector.Close();
            }

            Debug.Log("TITLE_SCREEN_CAPTURE " + EvidenceFolder + "/01-title.png");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _ = scene;
        }

        private static void BuildPhaseSelector(Transform root, Sprite flag, out Button flagButton,
            out GameObject dropdownRoot, out Button backdrop, out Button[] itemButtons, out Text[] itemLabels)
        {
            flagButton = AddIconButton("Bandeira_Fases", root, flag);
            var flagRect = flagButton.GetComponent<RectTransform>();
            flagRect.anchorMin = new Vector2(0.5f, 1f);
            flagRect.anchorMax = new Vector2(0.5f, 1f);
            flagRect.pivot = new Vector2(0.5f, 1f);
            flagRect.anchoredPosition = new Vector2(0f, -26f);
            flagRect.sizeDelta = new Vector2(72f, 90f);

            dropdownRoot = new GameObject("Fases_Dropdown", typeof(RectTransform));
            dropdownRoot.transform.SetParent(root, false);
            Stretch(dropdownRoot.GetComponent<RectTransform>());

            var backGo = new GameObject("Fundo_Clique", typeof(RectTransform), typeof(Image));
            backGo.transform.SetParent(dropdownRoot.transform, false);
            Stretch(backGo.GetComponent<RectTransform>());
            var backImage = backGo.GetComponent<Image>();
            // Alfa minimo: continua recebendo clique para fechar o dropdown.
            backImage.color = new Color(0f, 0f, 0f, 0.003f);
            backImage.raycastTarget = true;
            backdrop = backGo.AddComponent<Button>();
            backdrop.transition = Selectable.Transition.None;

            var panelGo = new GameObject("Painel_Fases", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(dropdownRoot.transform, false);
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -124f);
            panelRect.sizeDelta = new Vector2(420f, 104f);
            panelGo.GetComponent<Image>().color = new Color(0.05f, 0.038f, 0.028f, 0.97f);
            var border = panelGo.AddComponent<Outline>();
            border.effectColor = new Color(0.85f, 0.70f, 0.42f, 0.55f);
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var header = AddText("Titulo_Fases", panelGo.transform, "Escolha a fase", 18,
                new Color(0.72f, 0.66f, 0.55f), TextAnchor.UpperCenter);
            var headerRect = header.rectTransform;
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -12f);
            headerRect.sizeDelta = new Vector2(-24f, 26f);

            itemButtons = new Button[PhaseScenes.Length];
            itemLabels = new Text[PhaseScenes.Length];

            for (int i = 0; i < PhaseScenes.Length; i++)
            {
                var itemGo = new GameObject($"Fase_{i:D2}", typeof(RectTransform));
                itemGo.transform.SetParent(panelGo.transform, false);
                var itemRect = itemGo.GetComponent<RectTransform>();
                itemRect.anchorMin = new Vector2(0f, 1f);
                itemRect.anchorMax = new Vector2(1f, 1f);
                itemRect.pivot = new Vector2(0.5f, 1f);
                itemRect.anchoredPosition = new Vector2(0f, -(44f + i * 50f));
                itemRect.sizeDelta = new Vector2(-28f, 44f);

                var label = AddText("Rotulo", itemGo.transform, PhaseLabels[i], 28, Cream, TextAnchor.MiddleCenter);
                Stretch(label.rectTransform);

                var button = itemGo.AddComponent<Button>();
                button.targetGraphic = label;
                var colors = button.colors;
                colors.normalColor = Cream;
                colors.highlightedColor = Gold;
                colors.selectedColor = Gold;
                colors.pressedColor = new Color(0.80f, 0.64f, 0.40f);
                colors.fadeDuration = 0.08f;
                button.colors = colors;

                itemButtons[i] = button;
                itemLabels[i] = label;
            }
        }

        private static GameObject BuildSettingsPanel(Transform root, out Slider slider, out Button closeButton)
        {
            var panelGo = new GameObject("Painel_Configuracoes", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(root, false);
            var rect = panelGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(820f, 400f);
            panelGo.GetComponent<Image>().color = PanelColor;

            var heading = AddText("Titulo_Painel", panelGo.transform, "Configurações", 34, Cream, TextAnchor.MiddleLeft);
            AnchorTopLeft(heading.rectTransform, 40f, -30f, 600f, 60f);

            var volumeLabel = AddText("Rotulo_Volume", panelGo.transform, "Volume geral", 24, new Color(0.85f, 0.80f, 0.70f), TextAnchor.MiddleLeft);
            AnchorTopLeft(volumeLabel.rectTransform, 40f, -120f, 400f, 40f);

            slider = BuildSlider(panelGo.transform, 40f, -170f, 560f);

            var close = AddMenuButton("Fechar", panelGo.transform, 40f, -290f, 220f);
            closeButton = close;

            return panelGo;
        }

        private static Slider BuildSlider(Transform parent, float x, float y, float width)
        {
            var sliderGo = new GameObject("Volume", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(parent, false);
            var rect = sliderGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, 28f);

            var background = new GameObject("Fundo", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(sliderGo.transform, false);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.35f);
            bgRect.anchorMax = new Vector2(1f, 0.65f);
            bgRect.sizeDelta = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.20f, 0.16f, 0.12f);

            var fillArea = new GameObject("Area_Preenchimento", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.35f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.65f);
            fillAreaRect.offsetMin = new Vector2(6f, 0f);
            fillAreaRect.offsetMax = new Vector2(-12f, 0f);

            var fill = new GameObject("Preenchimento", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.sizeDelta = new Vector2(12f, 0f);
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.87f, 0.71f, 0.42f);

            var handleArea = new GameObject("Area_Alca", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(8f, 0f);
            handleAreaRect.offsetMax = new Vector2(-8f, 0f);

            var handle = new GameObject("Alca", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(24f, 24f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = Cream;

            var slider = sliderGo.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }

        private static Button AddMenuButton(string label, Transform parent, float x, float y, float width)
        {
            var go = new GameObject($"Botao_{label.Replace(" ", "")}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, 58f);

            var text = AddText("Rotulo", go.transform, label, 38, Cream, TextAnchor.MiddleLeft);
            Stretch(text.rectTransform);

            var button = go.AddComponent<Button>();
            button.targetGraphic = text;
            var colors = button.colors;
            colors.normalColor = Cream;
            colors.highlightedColor = Gold;
            colors.selectedColor = Gold;
            colors.pressedColor = new Color(0.80f, 0.64f, 0.40f);
            colors.disabledColor = Disabled;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            return button;
        }

        private static Button AddIconButton(string name, Transform parent, Sprite icon)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = icon;
            image.preserveAspect = true;
            image.color = new Color(0.92f, 0.86f, 0.72f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.92f, 0.75f);
            colors.pressedColor = new Color(0.8f, 0.72f, 0.55f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            return button;
        }

        private static Text AddText(string name, Transform parent, string content, int size, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Image AddImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = false;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void AnchorTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static Font Font
        {
            get
            {
                if (uiFont != null) return uiFont;
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (uiFont == null) uiFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
                return uiFont;
            }
        }

        private static Sprite LoadSprite(string path, int maxSize)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }
                if (importer.maxTextureSize < maxSize)
                {
                    importer.maxTextureSize = maxSize;
                    dirty = true;
                }
                if (importer.alphaIsTransparency == false && path.EndsWith("Gear.png"))
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }
                if (dirty) importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void RegisterBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == ScenePath || s.path == GameplayPath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayPath) != null)
            {
                scenes.Insert(1, new EditorBuildSettingsScene(GameplayPath, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
