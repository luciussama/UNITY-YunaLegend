using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YunaLegend.DreamForest.Runtime
{
    public sealed class DreamForestHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action<bool> OnStateChanged;
        public void OnPointerDown(PointerEventData eventData) => OnStateChanged?.Invoke(true);
        public void OnPointerUp(PointerEventData eventData) => OnStateChanged?.Invoke(false);
        public void OnPointerExit(PointerEventData eventData) => OnStateChanged?.Invoke(false);
    }
}
