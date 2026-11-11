using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace YunaLegend.UI
{
    /// <summary>
    /// Tela de entrada (boas-vindas) do Quarto dos Brinquedos.
    /// Menu canonico: Novo jogo, Carregar jogo (desabilitado sem save), Modo facil,
    /// rodape de estado e engrenagem de configuracoes com volume.
    /// </summary>
    public sealed class TitleScreenController : MonoBehaviour
    {
        public const string SaveKey = "YunaLegend.HasSave";
        public const string EasyModeKey = "YunaLegend.EasyMode";
        public const string VolumeKey = "YunaLegend.MasterVolume";

        [Header("Cena de destino")]
        [SerializeField] private string gameplaySceneName = "DarkRoom_Gameplay";

        [Header("Menu")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button easyModeButton;
        [SerializeField] private Text easyModeTag;
        [SerializeField] private Text footerText;
        [SerializeField] private Text focusMarker;

        [Header("Configuracoes")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button closeSettingsButton;

        [Header("Cores do modo facil")]
        [SerializeField] private Color easyOnColor = new Color(0.56f, 0.80f, 0.52f);
        [SerializeField] private Color easyOffColor = new Color(0.66f, 0.62f, 0.55f);
        [SerializeField] private Color menuNormalColor = new Color(0.95f, 0.90f, 0.78f);
        [SerializeField] private Color menuDisabledColor = new Color(0.52f, 0.48f, 0.42f);

        /// <summary>Ha uma aventura salva? (o save real ainda nao existe; o flag fica em PlayerPrefs).</summary>
        public static bool HasSave
        {
            get => PlayerPrefs.GetInt(SaveKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(SaveKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Modo facil escolhido na tela de entrada, disponivel para o gameplay.</summary>
        public static bool EasyMode
        {
            get => PlayerPrefs.GetInt(EasyModeKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(EasyModeKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(VolumeKey, 1f);
            set
            {
                PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        private void Awake()
        {
            if (newGameButton != null) newGameButton.onClick.AddListener(StartNewGame);
            if (loadGameButton != null) loadGameButton.onClick.AddListener(ContinueGame);
            if (easyModeButton != null) easyModeButton.onClick.AddListener(ToggleEasyMode);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);

            if (volumeSlider != null)
            {
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 1f;
                volumeSlider.SetValueWithoutNotify(MasterVolume);
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            AudioListener.volume = MasterVolume;
            RefreshState();
        }

        private void Start()
        {
            if (EventSystem.current != null && newGameButton != null)
            {
                EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
            }

            if (settingsPanel != null) settingsPanel.SetActive(false);
            UpdateFocusMarker();
        }

        private void Update()
        {
            UpdateFocusMarker();

            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) CloseSettings();
            }
        }

        private void RefreshState()
        {
            bool hasSave = HasSave;
            if (loadGameButton != null)
            {
                loadGameButton.interactable = hasSave;
                var label = loadGameButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.color = hasSave ? menuNormalColor : menuDisabledColor;
                }
            }

            if (footerText != null)
            {
                footerText.text = hasSave
                    ? "Aventura salva encontrada neste navegador."
                    : "Nenhuma aventura salva neste navegador.";
            }

            if (easyModeTag != null)
            {
                bool on = EasyMode;
                easyModeTag.text = on ? "Ativado" : "Desativado";
                easyModeTag.color = on ? easyOnColor : easyOffColor;
            }
        }

        public void StartNewGame()
        {
            LoadGameplay();
        }

        public void ContinueGame()
        {
            if (!HasSave) return;
            LoadGameplay();
        }

        private void LoadGameplay()
        {
            if (string.IsNullOrEmpty(gameplaySceneName))
            {
                Debug.LogError("[TitleScreen] Cena de destino nao configurada.");
                return;
            }

            SceneManager.LoadScene(gameplaySceneName);
        }

        public void ToggleEasyMode()
        {
            EasyMode = !EasyMode;
            RefreshState();
        }

        public void OpenSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
            if (settingsPanel != null && EventSystem.current != null)
            {
                var first = volumeSlider != null ? volumeSlider.gameObject : settingsPanel;
                if (closeSettingsButton != null) EventSystem.current.SetSelectedGameObject(closeSettingsButton.gameObject);
                else EventSystem.current.SetSelectedGameObject(first);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (EventSystem.current != null && newGameButton != null)
            {
                EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
            }
        }

        private void OnVolumeChanged(float value)
        {
            MasterVolume = value;
            AudioListener.volume = value;
        }

        /// <summary>Marcador ">" que acompanha o item focado (teclado/gamepad).</summary>
        private void UpdateFocusMarker()
        {
            if (focusMarker == null) return;

            var eventSystem = EventSystem.current;
            var selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
            if (selected == null || settingsPanel != null && settingsPanel.activeSelf)
            {
                focusMarker.enabled = false;
                return;
            }

            var rect = selected.transform as RectTransform;
            if (rect == null || rect.parent != focusMarker.rectTransform.parent)
            {
                focusMarker.enabled = false;
                return;
            }

            focusMarker.enabled = true;
            focusMarker.rectTransform.position = rect.TransformPoint(new Vector3(rect.rect.xMin - 34f, rect.rect.center.y, 0f));
        }

        /// <summary>Usado pelos testes de EditMode/PlayMode.</summary>
        public void Configure(Button newGame, Button loadGame, Button easyMode, Text easyTag, Text footer,
            Text marker, Button settings, GameObject panel, Slider volume, Button close, string sceneName)
        {
            newGameButton = newGame;
            loadGameButton = loadGame;
            easyModeButton = easyMode;
            easyModeTag = easyTag;
            footerText = footer;
            focusMarker = marker;
            settingsButton = settings;
            settingsPanel = panel;
            volumeSlider = volume;
            closeSettingsButton = close;
            gameplaySceneName = sceneName;
        }

        public string GameplaySceneName => gameplaySceneName;
    }
}
