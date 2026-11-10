using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.DarkRoom;
using YunaLegend.Camera25D;

namespace YunaLegend.Tests
{
    public class DarkRoomReturnInputTests
    {
        private Keyboard keyboard;
        private Mouse mouse;
        private Touchscreen touch;
        private float capture;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorInput;
#endif
        [SetUp]
        public void Setup()
        {
            capture = Time.captureDeltaTime;
            background = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            touch = InputSystem.AddDevice<Touchscreen>();
        }
        [TearDown]
        public void Cleanup()
        {
            Time.captureDeltaTime = capture;
            InputSystem.settings.backgroundBehavior = background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
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
        private IEnumerator Load(int fps)
        {
            Time.captureDeltaTime = 1f / fps;
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/DarkRoom_Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/DarkRoom_Gameplay.unity");
#endif
            yield return null;
            yield return null;
        }
        private DarkRoomPlatformMarker Marker(int index) => Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(m => m.spec.index == index);
        private IEnumerator Attempt(int fps, int device, float delay, bool retry)
        {
            yield return Load(fps);
            var manager = Object.FindFirstObjectByType<DarkRoomProgressionManager>();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var nanda = Object.FindFirstObjectByType<NandaFairyGuide>();
            player.ResetPlayer(new Vector3(Marker(21).transform.position.x, DarkRoomReturnLayout.StartY, 0f));
            player.AutoRunActive = false;
            manager.HandleCinematicFallLanded();
            Assert.IsFalse(manager.ReturnJumpCueVisible, "No tutorial during dialogue");
            manager.AdvanceCutscene(); manager.AdvanceCutscene(); manager.AdvanceCutscene();
            if (retry) { player.TriggerDeath("retry test"); manager.RetryCurrentCheckpoint(); }
            float startX = player.transform.position.x;
            Assert.AreEqual(Marker(21).transform.position.x, startX, 0.01f, "No horizontal teleport after fall");
            Assert.AreEqual(22, nanda.CurrentTargetPlatformIndex);
            Assert.IsTrue(manager.ReturnJumpCueVisible);
            Assert.AreEqual("Pule aqui", DarkRoomReturnLayout.JumpCue);
            Assert.AreEqual(Marker(22).GetTopCenterWorld(), manager.ReturnJumpCuePosition);
            float elapsed = 0f;
            bool pressed = false, released = false, framed = false;
            float hold = 1f / fps;
            while (elapsed < 5f && manager.CurrentPlatformIndex == 21 && !player.IsDead)
            {
                if (!pressed && elapsed >= delay) { Input(device, true); pressed = true; }
                if (pressed && !released && elapsed >= delay + hold) { Input(device, false); released = true; }
                var viewport = Camera.main.WorldToViewportPoint(manager.ReturnJumpCuePosition);
                if (elapsed >= 1f && elapsed < delay) framed |= viewport.z > 0 && viewport.x > 0.05f && viewport.x < 0.95f && viewport.y > 0.05f && viewport.y < 0.95f;
                yield return null; elapsed += Time.deltaTime;
            }
            Input(device, false);
            Debug.Log($"RETURN_INPUT fps={fps} device={device} delay={delay:F2} retry={retry} elapsed={elapsed:F3} x={player.transform.position.x:F2} y={player.transform.position.y:F2} landed={manager.CurrentPlatformIndex} dead={player.IsDead} framed={framed}");
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(22, manager.CurrentPlatformIndex);
            Assert.Greater(elapsed, 2.5f);
            Assert.IsTrue(framed, "Obstacle must be in camera before takeoff");
            Assert.IsFalse(manager.ReturnJumpCueVisible);
            Assert.AreEqual(23, nanda.CurrentTargetPlatformIndex);
            yield return null; yield return null;
            Assert.IsFalse(keyboard.spaceKey.isPressed);
            Assert.IsFalse(mouse.leftButton.isPressed);
            Assert.IsFalse(touch.primaryTouch.press.isPressed);
            {
                Assert.Less(Vector3.Distance(nanda.transform.position, Marker(22).GetTopCenterWorld() + Vector3.up * 1.2f), 2f,
                    "Nanda must have flown to the first obstacle during the approach");
                float takeoff = Marker(23).transform.position.x + 3.0f;
                while (player.transform.position.x > takeoff && !player.IsDead) yield return null;
                Input(device, true);
                float held = 0;
                while (held < hold) { yield return null; held += Time.deltaTime; }
                Input(device, false);
                float timeout = 0;
                while (manager.CurrentPlatformIndex == 22 && !player.IsDead && timeout < 1.5f) { yield return null; timeout += Time.deltaTime; }
                Assert.IsFalse(player.IsDead, $"Second brief jump fps={fps}");
                Assert.AreEqual(23, manager.CurrentPlatformIndex);
            }
            while (!player.IsDead && elapsed < 9f) { yield return null; elapsed += Time.deltaTime; }
            Assert.IsTrue(manager.IsGameOver, "No auto jump after release");
            Assert.IsFalse(manager.ReturnJumpCueVisible);
            manager.RetryCurrentCheckpoint();
            Assert.AreEqual(startX, player.transform.position.x, 0.01f);
            Assert.IsTrue(manager.ReturnJumpCueVisible);
            Assert.AreEqual(22, nanda.CurrentTargetPlatformIndex);
            player.AutoRunActive = false;
        }
        [UnityTest]
        public IEnumerator HumanApproachRealInputAndRetry()
        {
            foreach (int fps in new[] { 30, 60, 120 })
                foreach (int device in new[] { 0, 1, 2, 3 })
                    foreach (float delay in new[] { 2.75f, 2.85f, 2.95f, 3.05f })
                        yield return Attempt(fps, device, delay, delay > 2.9f);
        }
        [UnityTest]
        public IEnumerator NoInputDefeatAndScreenCrushRemainActive()
        {
            foreach (int fps in new[] { 30, 60, 120 })
            {
                yield return Load(fps);
                var manager = Object.FindFirstObjectByType<DarkRoomProgressionManager>();
                var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                player.ResetPlayer(new Vector3(Marker(21).transform.position.x, DarkRoomReturnLayout.StartY, 0f));
                manager.StartPhase3AfterPlotTwist();
                var camera = Object.FindFirstObjectByType<SideScroll25DCamera>();
                Assert.IsTrue(camera.ScreenCrushActive);
                float elapsed = 0;
                while (!player.IsDead && elapsed < 6f) { yield return null; elapsed += Time.deltaTime; }
                Debug.Log($"RETURN_NO_INPUT fps={fps} defeat={elapsed:F3}");
                Assert.Greater(elapsed, 2.8f);
                Assert.IsTrue(manager.IsGameOver);
                manager.RetryCurrentCheckpoint();
                player.AutoRunActive = false;
                for (int i = 0; i < fps * 2 && !player.IsDead; i++) yield return null;
                Assert.IsTrue(player.IsDead, "ScreenCrush still lethal");
            }
        }
    }
}
