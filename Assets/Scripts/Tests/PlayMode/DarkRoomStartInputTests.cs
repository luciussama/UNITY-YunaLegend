using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.DarkRoom;

namespace YunaLegend.Tests
{
    public class DarkRoomStartInputTests
    {
        private Keyboard keyboard;
        private Mouse mouse;
        private Touchscreen touch;
        private float previousCapture;
        private InputSettings.BackgroundBehavior previousBackground;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode previousEditorBehavior;
#endif

        [SetUp]
        public void Setup()
        {
            previousCapture = Time.captureDeltaTime;
            previousBackground = InputSystem.settings.backgroundBehavior;
#if UNITY_EDITOR
            previousEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
#endif
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            touch = InputSystem.AddDevice<Touchscreen>();
        }

        [TearDown]
        public void Cleanup()
        {
            Time.captureDeltaTime = previousCapture;
            InputSystem.settings.backgroundBehavior = previousBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
#endif
            InputSystem.RemoveDevice(keyboard);
            InputSystem.RemoveDevice(mouse);
            InputSystem.RemoveDevice(touch);
        }

        private void Input(int device, bool pressed)
        {
            if (device == 0) InputSystem.QueueStateEvent(keyboard, pressed ? new KeyboardState(Key.Space) : new KeyboardState());
            if (device == 1) InputSystem.QueueStateEvent(mouse, new MouseState { buttons = (ushort)(pressed ? 1 : 0) });
            if (device == 3) InputSystem.QueueStateEvent(keyboard, pressed ? new KeyboardState(Key.W) : new KeyboardState());
            if (device == 2) InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, phase = pressed ? UnityEngine.InputSystem.TouchPhase.Began : UnityEngine.InputSystem.TouchPhase.Ended, position = new Vector2(400, 300) });
        }

        private IEnumerator Attempt(int fps, int device, float delay, float hold, bool expectSuccess)
        {
            Time.captureDeltaTime = 1f / fps;
            #if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/DarkRoom_Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/DarkRoom_Gameplay.unity");
#endif
            yield return null;
            yield return null;
            var manager = Object.FindFirstObjectByType<DarkRoomProgressionManager>();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            manager.RetryCurrentCheckpoint();
            Time.captureDeltaTime = 1f / fps;
            float startX = player.transform.position.x;
            float elapsed = 0f;
            bool pressed = false, released = false;
            while (elapsed < delay + 1.2f && manager.CurrentPlatformIndex < 0 && !player.IsDead)
            {
                if (!pressed && elapsed >= delay) { Input(device, true); pressed = true; }
                if (pressed && !released && elapsed >= delay + hold) { Input(device, false); released = true; }
                yield return null;
                elapsed += Time.deltaTime;

            }
            Input(device, false);
            bool success = manager.CurrentPlatformIndex == 0 && !player.IsDead;
            Debug.Log($"START_INPUT fps={fps} device={device} delay={delay:F3} hold={hold:F3} startX={startX:F2} elapsed={elapsed:F3} x={player.transform.position.x:F2} y={player.transform.position.y:F2} success={success}");
            if (expectSuccess) Assert.IsTrue(success, $"fps={fps} device={device} delay={delay} hold={hold}");
            else
            {
                Assert.IsFalse(success);
                while (!player.IsDead) yield return null;
                Assert.IsTrue(manager.IsGameOver, "Early jump must fail, not silently jump again");
            }
            Assert.AreEqual(1f / fps, Time.deltaTime, 0.0001f, "Simulation must use the requested frame duration");
            if (!released) Input(device, false);
            yield return null;
            yield return null;
            Assert.IsFalse(keyboard.spaceKey.isPressed);
            Assert.IsFalse(mouse.leftButton.isPressed);
            Assert.IsFalse(touch.primaryTouch.press.isPressed);
            if (expectSuccess)
            {
                while (!player.IsDead && manager.CurrentPlatformIndex == 0) yield return null;
                Assert.IsTrue(manager.IsGameOver, "Releasing input must not auto-jump the next gap");
                manager.RetryCurrentCheckpoint();
                Assert.AreEqual(DarkRoomStartLayout.Position, player.transform.position);
            }
            player.AutoRunActive = false;
            yield return null;
        }

        [UnityTest]
        public IEnumerator FirstTwoStepsWithBriefKeyboardPresses()
        {
            foreach (int fps in new[] { 30, 60, 120 })
            {
#if UNITY_EDITOR
                yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/DarkRoom_Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
                yield return SceneManager.LoadSceneAsync("Assets/Scenes/DarkRoom_Gameplay.unity");
#endif
                yield return null;
                yield return null;
                Time.captureDeltaTime = 1f / fps;
                var manager = Object.FindFirstObjectByType<DarkRoomProgressionManager>();
                var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                manager.RetryCurrentCheckpoint();
                foreach (float takeoffX in new[] { -0.64f, 2.90f })
                {
                    while (player.transform.position.x < takeoffX && !player.IsDead) yield return null;
                    Assert.IsFalse(player.IsDead);
                    Input(0, true);
                    float held = 0;
                    while (held < 0.15f) { yield return null; held += Time.deltaTime; }
                    Input(0, false);
                    int target = takeoffX < 0 ? 0 : 1;
                    float timeout = 0;
                    while (manager.CurrentPlatformIndex < target && !player.IsDead && timeout < 1f)
                    { yield return null; timeout += Time.deltaTime; }
                    Assert.IsFalse(player.IsDead, $"fps={fps} target={target}");
                    Assert.AreEqual(target, manager.CurrentPlatformIndex);
                }
                player.AutoRunActive = false;
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator SceneWithoutInputFailsAfterUsefulApproach()
        {
            Time.captureDeltaTime = 1f / 60f;
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/DarkRoom_Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/DarkRoom_Gameplay.unity");
#endif
            yield return null;
            yield return null;
            var manager = Object.FindFirstObjectByType<DarkRoomProgressionManager>();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            manager.RetryCurrentCheckpoint();
            Time.captureDeltaTime = 1f / 60f;
            float elapsed = 0;
            while (!player.IsDead && elapsed < 5f) { yield return null; elapsed += Time.deltaTime; }
            Assert.Greater(elapsed, 2f);
            Assert.IsTrue(manager.IsGameOver);
            Assert.AreEqual(-1, manager.CurrentPlatformIndex);
            Debug.Log($"START_NO_INPUT deathAfter={elapsed:F3}s");
        }

        [UnityTest]
        public IEnumerator TapAndHoldHaveIdenticalBallisticArcOnEveryDevice()
        {
            foreach (int fps in new[] { 30, 60, 120 })
            {
                float referenceApex = 0f;
                foreach (int device in new[] { 0, 1, 2, 3 })
                foreach (float hold in new[] { 1f / fps, 0.7f })
                {
                    Time.captureDeltaTime = 1f / fps;
#if UNITY_EDITOR
                    yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/DarkRoom_Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
                    yield return SceneManager.LoadSceneAsync("Assets/Scenes/DarkRoom_Gameplay.unity");
#endif
                    yield return null; yield return null;
                    var manager = Object.FindFirstObjectByType<DarkRoomProgressionManager>();
                    var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                    manager.RetryCurrentCheckpoint();
                    for (int frame = 0; frame < fps * 2.5f; frame++) yield return null;
                    float startY = player.transform.position.y;
                    float apex = startY;
                    Input(device, true);
                    int heldFrames = Mathf.Max(1, Mathf.RoundToInt(hold * fps));
                    for (int frame = 0; frame < fps; frame++)
                    {
                        if (frame == heldFrames) Input(device, false);
                        yield return null;
                        apex = Mathf.Max(apex, player.transform.position.y);
                    }
                    Input(device, false);
                    Assert.IsFalse(player.IsDead);
                    Assert.AreEqual(0, manager.CurrentPlatformIndex);
                    Assert.AreEqual(DarkRoomStartLayout.JumpHeight, apex - startY, 0.06f);
                    if (device == 0 && hold < 0.1f) referenceApex = apex;
                    Assert.AreEqual(referenceApex, apex, 0.005f, $"fps={fps} device={device} hold={hold}");
                    Debug.Log($"BALLISTIC_EQUIVALENCE fps={fps} device={device} hold={hold:F3} height={apex-startY:F3}");
                    player.AutoRunActive = false;
                    yield return null; yield return null;
                }
            }
        }

        [UnityTest]
        public IEnumerator EarlyJumpDoesNotAutoJumpAgain()
        {
            yield return Attempt(60, 0, 0.60f, 0.10f, false);
        }

        [UnityTest]
        public IEnumerator SceneRealInputWindowAt30And60And120Fps()
        {
            foreach (int fps in new[] { 30, 60, 120 })
                foreach (int device in new[] { 0, 1, 2, 3 })
                                foreach (float delay in new[] { 2.40f, 2.50f, 2.60f, 2.70f })
                                    yield return Attempt(fps, device, delay, 1f / fps, true);
        }
    }
}
