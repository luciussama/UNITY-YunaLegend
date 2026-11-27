using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YunaLegend.DreamForestEntrance.Runtime
{
    /// <summary>Local phase chooser: opening it never changes the active scene or the cinematic clock.</summary>
    public sealed class EntrancePhaseMenuView : MonoBehaviour
    {
        private static readonly string[,] Stages =
        {
            { "Quarto escuro", "Fase-Jogavel-Completa" },
            { "Sala de brinquedos", "ToyRoom_Gameplay" },
            { "Arena de batalha", "Arena_Gameplay" },
            { "Perseguição 3D", "BearChase_Gameplay" },
            { "Um dia no jardim", "Picnic_Gameplay" },
            { "Jardim dos sonhos", "DreamForest_Gameplay" }
        };

        private GameObject panel;
        private Dropdown dropdown;
        private Action<string> choose;
        private Sprite panelSprite;
        private Sprite selectSprite;
        public bool IsOpen => panel != null && panel.activeSelf;

        public void Build(Font font, Action<string> onChoose)
        {
            choose = onChoose;
            panelSprite = LoadSprite("MenuPanel");
            selectSprite = LoadSprite("SelectBox");
            panel = new GameObject("EntrancePhasePanel",typeof(RectTransform),typeof(Image));
            panel.transform.SetParent(transform,false);
            var image = panel.GetComponent<Image>();
            image.sprite = panelSprite;
            image.type = Image.Type.Sliced;
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1,1);
            rt.pivot = new Vector2(1,1);
            rt.anchoredPosition = new Vector2(-118,-64);
            rt.sizeDelta = new Vector2(220,84);

            var titleGo = new GameObject("PhaseMenuHeading",typeof(RectTransform),typeof(Text));
            titleGo.transform.SetParent(panel.transform,false);
            var title = titleGo.GetComponent<Text>();
            title.font = font;
            title.fontSize = 11;
            title.color = new Color(211/255f,202/255f,218/255f);
            title.text = "Revisar fase desde a cena inicial";
            title.alignment = TextAnchor.MiddleLeft;
            title.raycastTarget = false;
            TopRect(title.rectTransform,12,12,196,14);

            var dropdownGo = DefaultControls.CreateDropdown(new DefaultControls.Resources
            {
                standard = selectSprite,
                background = panelSprite
            });
            dropdownGo.name = "EntrancePhaseSelect";
            dropdownGo.transform.SetParent(panel.transform,false);
            TopRect(dropdownGo.GetComponent<RectTransform>(),12,30,196,42);
            dropdown = dropdownGo.GetComponent<Dropdown>();
            dropdown.ClearOptions();
            var options = new List<string> { "Iniciar fase…" };
            for (int i = 0; i < Stages.GetLength(0); i++)
            {
                options.Add(Stages[i,0]);
            }
            dropdown.AddOptions(options);
            foreach (var text in dropdownGo.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
                text.fontSize = 13;
                text.fontStyle = FontStyle.Normal;
                text.color = new Color(245/255f,232/255f,200/255f);
            }
            var arrow = dropdownGo.transform.Find("Arrow");
            arrow.GetComponent<Image>().enabled = false;
            var arrowText = new GameObject("ArrowGlyph",typeof(RectTransform),typeof(EntranceDropdownArrow));
            arrowText.transform.SetParent(arrow,false);
            var glyph = arrowText.GetComponent<EntranceDropdownArrow>();
            glyph.raycastTarget = false;
            var ar = glyph.rectTransform;
            ar.anchorMin = Vector2.zero;
            ar.anchorMax = Vector2.one;
            ar.offsetMin = ar.offsetMax = Vector2.zero;
            dropdown.onValueChanged.AddListener(OnSelection);
            panel.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
                return;
            }
            panel.SetActive(true);
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(dropdown.gameObject);
            }
        }

        public bool Close()
        {
            if (!IsOpen) return false;
            dropdown.Hide();
            panel.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            return true;
        }

        private void OnSelection(int index)
        {
            if (index <= 0 || index > Stages.GetLength(0)) return;
            string scene = Stages[index-1,1];
            Close();
            dropdown.SetValueWithoutNotify(0);
            choose?.Invoke(scene);
        }

        private static Sprite LoadSprite(string name)
        {
            var texture = Resources.Load<Texture2D>("DreamForestEntrance/"+name);
            return Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100,0,
                SpriteMeshType.FullRect,new Vector4(16,16,16,16));
        }

        private static void TopRect(RectTransform rt,float x,float y,float w,float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0,1);
            rt.pivot = new Vector2(0,1);
            rt.anchoredPosition = new Vector2(x,-y);
            rt.sizeDelta = new Vector2(w,h);
            rt.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            if (panelSprite != null) Release(panelSprite);
            if (selectSprite != null) Release(selectSprite);
        }

        private static void Release(UnityEngine.Object resource)
        {
            if (Application.isPlaying) Destroy(resource);
            else DestroyImmediate(resource);
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class EntranceDropdownArrow : MaskableGraphic
    {
        protected EntranceDropdownArrow()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Color ink = new Color(245/255f,232/255f,200/255f);
            vh.AddVert(new Vector2(r.center.x-4,r.center.y+2),ink,Vector2.zero);
            vh.AddVert(new Vector2(r.center.x+4,r.center.y+2),ink,Vector2.zero);
            vh.AddVert(new Vector2(r.center.x,r.center.y-2),ink,Vector2.zero);
            vh.AddTriangle(0,1,2);
        }
    }
}
