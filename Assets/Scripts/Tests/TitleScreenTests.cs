using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YunaLegend.UI;

namespace YunaLegend.Tests
{
    /// <summary>
    /// Tela de entrada: menu canonico, cena de destino e persistencia de preferencias.
    /// </summary>
    [TestFixture]
    public class TitleScreenTests
    {
        [Test]
        public void TitleScene_HasCanonicalMenuAndController()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/TitleScreen.unity");
            try
            {
                var controller = UnityEngine.Object.FindFirstObjectByType<TitleScreenController>();
                Assert.IsNotNull(controller, "TitleScreenController ausente");
                Assert.AreEqual("DarkRoom_Gameplay", controller.GameplaySceneName);

                Assert.IsNotNull(UnityEngine.Object.FindFirstObjectByType<Canvas>(), "Canvas ausente");
                Assert.IsNotNull(UnityEngine.Object.FindFirstObjectByType<EventSystem>(), "EventSystem ausente");

                var texts = UnityEngine.Object.FindObjectsByType<Text>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                string joined = string.Join("|", Array.ConvertAll(texts, t => t.text));
                StringAssert.Contains("Novo jogo", joined);
                StringAssert.Contains("Carregar jogo", joined);
                StringAssert.Contains("Modo fácil", joined);
                StringAssert.Contains("Desativado", joined);

                var buttons = UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                Assert.GreaterOrEqual(buttons.Length, 4, "Menu canonico tem ao menos quatro botoes");

                var swing = UnityEngine.Object.FindFirstObjectByType<TitleLegSwing>();
                Assert.IsNotNull(swing, "Perna da menina deve balancar na tela de entrada");
                Assert.Greater(swing.AmplitudeDegrees, 0f);
                var legImage = swing.GetComponent<Image>();
                Assert.IsNotNull(legImage);
                Assert.IsNotNull(legImage.sprite, "Sprite da perna ausente");
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
            }
        }

        [Test]
        public void TitleScene_IsFirstInBuildSettings()
        {
            var scenes = UnityEditor.EditorBuildSettings.scenes;
            Assert.GreaterOrEqual(scenes.Length, 2, "Build Settings deve ter entrada + gameplay");
            Assert.AreEqual("Assets/Scenes/TitleScreen.unity", scenes[0].path);
            Assert.AreEqual("Assets/Scenes/DarkRoom_Gameplay.unity", scenes[1].path);
        }

        [Test]
        public void EasyModeAndVolume_ArePersistedInPlayerPrefs()
        {
            bool previousEasy = TitleScreenController.EasyMode;
            float previousVolume = TitleScreenController.MasterVolume;
            try
            {
                TitleScreenController.EasyMode = true;
                Assert.IsTrue(TitleScreenController.EasyMode);
                TitleScreenController.EasyMode = false;
                Assert.IsFalse(TitleScreenController.EasyMode);

                TitleScreenController.MasterVolume = 0.35f;
                Assert.AreEqual(0.35f, TitleScreenController.MasterVolume, 0.001f);
            }
            finally
            {
                TitleScreenController.EasyMode = previousEasy;
                TitleScreenController.MasterVolume = previousVolume;
            }
        }
    }
}
