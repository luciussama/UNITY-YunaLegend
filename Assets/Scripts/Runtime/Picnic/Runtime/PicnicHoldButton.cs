using UnityEngine;
using UnityEngine.EventSystems;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool Held { get; private set; }
        public void OnPointerDown(PointerEventData data) { Held = true; }
        public void OnPointerUp(PointerEventData data) { Held = false; }
        public void OnPointerExit(PointerEventData data) { Held = false; }
        private void OnDisable() { Held = false; }
    }
}
