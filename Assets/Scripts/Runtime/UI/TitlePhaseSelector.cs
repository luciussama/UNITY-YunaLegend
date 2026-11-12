using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YunaLegend.UI
{
    /// <summary>
    /// Bandeirinha no topo da tela de entrada: abre um dropdown para escolher a fase.
    /// Hoje existe apenas O Quarto Escuro; a lista e serializada para crescer sem codigo novo.
    /// </summary>
    public sealed class TitlePhaseSelector : MonoBehaviour
    {
        [SerializeField] private TitleScreenController controller;
        [SerializeField] private Button flagButton;
        [SerializeField] private GameObject dropdownRoot;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button[] itemButtons;
        [SerializeField] private Text[] itemLabels;
        [SerializeField] private string[] itemScenes;

        private static readonly Color Cream = new Color(0.95f, 0.90f, 0.78f);
        private static readonly Color Gold = new Color(1f, 0.84f, 0.55f);

        public bool IsOpen => dropdownRoot != null && dropdownRoot.activeSelf;
        public int ItemCount => itemScenes != null ? itemScenes.Length : 0;
        public string CurrentPhaseScene { get; private set; }

        private void Awake()
        {
            if (flagButton != null) flagButton.onClick.AddListener(Toggle);
            if (backdropButton != null) backdropButton.onClick.AddListener(Close);
        }

        private void Start()
        {
            Close();
            if (itemButtons != null)
            {
                for (int i = 0; i < itemButtons.Length; i++)
                {
                    int index = i;
                    if (itemButtons[i] != null) itemButtons[i].onClick.AddListener(() => Choose(index));
                }
            }
            CurrentPhaseScene = itemScenes != null && itemScenes.Length > 0 ? itemScenes[0] : null;
            UpdateSelection();
        }

        private void Update()
        {
            if (!IsOpen) return;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Close();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (dropdownRoot == null) return;
            dropdownRoot.SetActive(true);
            UpdateSelection();
            if (EventSystem.current != null && itemButtons != null && itemButtons.Length > 0 && itemButtons[0] != null)
                EventSystem.current.SetSelectedGameObject(itemButtons[0].gameObject);
        }

        public void Close()
        {
            if (dropdownRoot != null) dropdownRoot.SetActive(false);
            if (EventSystem.current != null && flagButton != null)
                EventSystem.current.SetSelectedGameObject(flagButton.gameObject);
        }

        public void Choose(int index)
        {
            if (itemScenes == null || index < 0 || index >= itemScenes.Length) return;
            CurrentPhaseScene = itemScenes[index];
            UpdateSelection();
            Close();
            if (controller != null) controller.StartPhase(CurrentPhaseScene);
        }

        private void UpdateSelection()
        {
            if (itemLabels == null || itemScenes == null) return;
            for (int i = 0; i < itemLabels.Length && i < itemScenes.Length; i++)
            {
                if (itemLabels[i] == null) continue;
                bool selected = itemScenes[i] == CurrentPhaseScene;
                itemLabels[i].color = selected ? Gold : Cream;
                itemLabels[i].text = selected ? "• " + PhaseLabel(i) : PhaseLabel(i);
            }
        }

        private string PhaseLabel(int index)
        {
            if (itemLabels[index] == null) return string.Empty;
            string text = itemLabels[index].text;
            return text.StartsWith("• ") ? text.Substring(2) : text;
        }

        public void Configure(TitleScreenController owner, Button flag, GameObject root, Button backdrop,
            Button[] buttons, Text[] labels, string[] scenes)
        {
            controller = owner;
            flagButton = flag;
            dropdownRoot = root;
            backdropButton = backdrop;
            itemButtons = buttons;
            itemLabels = labels;
            itemScenes = scenes;
            if (scenes != null && scenes.Length > 0) CurrentPhaseScene = scenes[0];
        }
    }
}
