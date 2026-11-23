using UnityEngine;
using UnityEngine.InputSystem;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicInputAdapter : MonoBehaviour
    {
        public float Axis { get; private set; }
        public bool Jump { get; private set; }
        public bool Advance { get; private set; }
        public bool Pause { get; private set; }
        public void Poll(float touchAxis, bool touchJump)
        {
            var keyboard = Keyboard.current;
            Axis = touchAxis;
            Jump = touchJump;
            Advance = false;
            Pause = false;
            if (keyboard == null) return;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) Axis -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) Axis += 1f;
            Jump |= keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
            Advance = keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame;
            Pause = keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame;
            Axis = Mathf.Clamp(Axis, -1f, 1f);
        }
    }
}
