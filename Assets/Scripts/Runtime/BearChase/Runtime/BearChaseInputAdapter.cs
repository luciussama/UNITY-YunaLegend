using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Adaptador fino de entrada (Keyboard, Gamepad e Pointer/Touch com guarda contra clique em UI).
    /// </summary>
    public sealed class BearChaseInputAdapter : MonoBehaviour
    {
        public float LateralAxis { get; private set; }

        public event Action OnPausePressed;
        public event Action OnMutePressed;

        public void PollInput()
        {
            PollUtilityHotkeys();
            LateralAxis = ReadLateralAxis();
        }

        private void PollUtilityHotkeys()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.pKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
                {
                    OnPausePressed?.Invoke();
                }

                if (kb.mKey.wasPressedThisFrame)
                {
                    OnMutePressed?.Invoke();
                }
            }

            Gamepad pad = Gamepad.current;
            if (pad != null && pad.startButton.wasPressedThisFrame)
            {
                OnPausePressed?.Invoke();
            }
        }

        private static float ReadLateralAxis()
        {
            float val = 0f;

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
                {
                    val -= 1f;
                }

                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
                {
                    val += 1f;
                }
            }

            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                float stickX = pad.leftStick.ReadValue().x;
                float dpadX = pad.dpad.ReadValue().x;
                if (Mathf.Abs(stickX) > 0.12f)
                {
                    val += stickX;
                }
                else if (Mathf.Abs(dpadX) > 0.1f)
                {
                    val += dpadX;
                }
            }

            Pointer pointer = Pointer.current;
            if (pointer != null && pointer.press.isPressed && Screen.width > 0 && !IsPointerOverUi(pointer))
            {
                float x = pointer.position.ReadValue().x;
                val += x < Screen.width * 0.5f ? -1f : 1f;
            }

            return Mathf.Clamp(val, -1f, 1f);
        }

        private static bool IsPointerOverUi(Pointer pointer)
        {
            EventSystem current = EventSystem.current;
            if (current == null)
            {
                return false;
            }

            if (current.IsPointerOverGameObject())
            {
                return true;
            }

            int deviceId = pointer.deviceId;
            return current.IsPointerOverGameObject(deviceId);
        }
    }
}
