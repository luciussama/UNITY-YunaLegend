using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>Keeps each press until that finger lifts, even if the thumb drifts off the art.</summary>
    public sealed class PicnicTouchButton : Button
    {
        public Action<int> Pressed;
        public Action<int> Released;
        private readonly HashSet<int> pointers = new HashSet<int>();

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!IsActive() || !IsInteractable() || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if (pointers.Add(eventData.pointerId))
            {
                Pressed?.Invoke(eventData.pointerId);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (pointers.Remove(eventData.pointerId))
            {
                Released?.Invoke(eventData.pointerId);
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
            {
                ReleasePointers();
            }
        }

        protected override void OnDisable()
        {
            ReleasePointers();
            base.OnDisable();
        }

        private void ReleasePointers()
        {
            foreach (int pointer in pointers)
            {
                Released?.Invoke(pointer);
            }
            pointers.Clear();
        }
    }
}
