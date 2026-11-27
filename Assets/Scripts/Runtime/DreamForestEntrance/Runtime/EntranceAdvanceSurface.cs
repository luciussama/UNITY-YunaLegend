using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YunaLegend.DreamForestEntrance.Runtime
{
    /// <summary>Pointer-down reading gesture; keyboard advancement belongs to the bootstrap, once per press.</summary>
    public sealed class EntranceAdvanceSurface : MonoBehaviour, IPointerDownHandler
    {
        public Action Advance;

        public static bool ControlHasKeyboardFocus
        {
            get
            {
                var selected=EventSystem.current!=null ? EventSystem.current.currentSelectedGameObject : null;
                return selected!=null && selected.GetComponent<Selectable>()!=null;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            if(data.button!=PointerEventData.InputButton.Left) return;
            EventSystem.current?.SetSelectedGameObject(null);
            Advance?.Invoke();
        }
    }
}
