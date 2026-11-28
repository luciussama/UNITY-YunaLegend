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
    public sealed class Stage1PlayableJumpRegressionTests
    {
        private const string Stage1ScenePath = "Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity";

        private Keyboard keyboard;
        private InputSettings.BackgroundBehavior previousBackground;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode previousEditorBehavior;
#endif

        [SetUp]
        public void Setup()
        {
            previousBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            previousEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>();
        }

        [TearDown]
        public void Teardown()
        {
            InputSystem.settings.backgroundBehavior = previousBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
#endif
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        }

        [UnityTest]
        public IEnumerator Stage1_SpaceKeyTriggersJumpInFaseJogavelCompleta()
        {
            float prevCapture = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            try
            {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                Stage1ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Fase-Jogavel-Completa");
#endif
            yield return null;
            yield return null;

            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            Assert.IsNotNull(player, "Yuna25DPlayerController not found in Fase-Jogavel-Completa");

            // Advance through opening beats + PreJumpCue using real Space key presses until AutoRunActive is true
            for (int i = 0; i < 12 && !player.AutoRunActive; i++)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
                yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
            }

            Assert.IsTrue(player.AutoRunActive, "AutoRunActive should become true after advancing PreJumpCue with Space");

            // Let Yuna run grounded for 2 frames then press Space to jump
            yield return null;
            yield return null;
            float yBefore = player.transform.position.y;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());

            float peakVy = player.Velocity.y;
            float peakY = player.transform.position.y;
            for (int f = 0; f < 18; f++)
            {
                yield return null;
                if (player.Velocity.y > peakVy) peakVy = player.Velocity.y;
                if (player.transform.position.y > peakY) peakY = player.transform.position.y;
            }

            Debug.Log($"STAGE1_JUMP_REGRESSION_CHECK peakVy={peakVy:F2} yBefore={yBefore:F2} peakY={peakY:F2}");
            Assert.Greater(peakVy, 8.0f, $"Yuna did not launch upward on Space press (peakVy={peakVy:F2}, yBefore={yBefore:F2}, peakY={peakY:F2})");
            Assert.Greater(peakY, yBefore + 1.2f, $"Yuna did not rise cleanly above floor on Space press (yBefore={yBefore:F2}, peakY={peakY:F2})");
            }
            finally
            {
                Time.captureDeltaTime = prevCapture;
            }
        }
    }
}
