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
                Assert.AreEqual("DreamForest_Gameplay", controller.GameplaySceneName);

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

                var selector = UnityEngine.Object.FindFirstObjectByType<TitlePhaseSelector>();
                Assert.IsNotNull(selector, "Bandeirinha de fases ausente");
                Assert.GreaterOrEqual(selector.ItemCount, 1, "Dropdown precisa listar ao menos uma fase");
                Assert.AreEqual("DreamForest_Gameplay", selector.CurrentPhaseScene);
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
            Assert.AreEqual("Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity", scenes[1].path);
        }

        [Test]
        public void PhaseSelectorInitialization_PreservesNewGameFocus_ButCloseRestoresFlag()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/TitleScreen.unity");
            try
            {
                var controller = UnityEngine.Object.FindFirstObjectByType<TitleScreenController>();
                var selector = UnityEngine.Object.FindFirstObjectByType<TitlePhaseSelector>();
                // Reproduce the startup order that previously replaced New Game with the flag.
                const System.Reflection.BindingFlags lifecycle =
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var events = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
                typeof(EventSystem).GetMethod("OnEnable", lifecycle).Invoke(events, null);
                typeof(TitleScreenController).GetMethod("Start", lifecycle).Invoke(controller, null);
                Assert.AreEqual("Botao_Novojogo", EventSystem.current.currentSelectedGameObject.name);
                typeof(TitlePhaseSelector).GetMethod("Start", lifecycle).Invoke(selector, null);
                Assert.IsFalse(selector.IsOpen);
                Assert.AreEqual("Botao_Novojogo", EventSystem.current.currentSelectedGameObject.name);
                selector.Open();
                Assert.IsTrue(selector.IsOpen);
                selector.Close();
                Assert.IsFalse(selector.IsOpen);
                Assert.AreEqual("Bandeira_Fases", EventSystem.current.currentSelectedGameObject.name);
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
            }
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

        [Test]
        public void TitleScreenResponsiveLayout_SwitchesBetweenPortraitAndLandscape()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            var canvasRect = canvasGo.GetComponent<RectTransform>();
            var artGo = new GameObject("Art_Container", typeof(RectTransform));
            artGo.transform.SetParent(canvasGo.transform, false);
            var artRect = artGo.GetComponent<RectTransform>();
            var leftShade = new GameObject("Sombra_Lateral");
            var bottomScrim = new GameObject("Sombra_Inferior");
            var titleGo = new GameObject("Titulo", typeof(RectTransform), typeof(Text));
            var newGameGo = new GameObject("Novo", typeof(RectTransform));
            var footerGo = new GameObject("Rodape", typeof(RectTransform));

            try
            {
                var layout = canvasGo.AddComponent<TitleScreenResponsiveLayout>();
                layout.Configure(
                    canvasRect,
                    artRect,
                    leftShade,
                    bottomScrim,
                    titleGo.GetComponent<Text>(),
                    newGameGo.GetComponent<RectTransform>(),
                    null,
                    null,
                    null,
                    footerGo.GetComponent<RectTransform>(),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null);

                canvasRect.sizeDelta = new Vector2(1920f, 1080f);
                layout.ApplyLayout(force: true);
                Assert.IsTrue(leftShade.activeSelf, "Paisagem deve manter sombra lateral");
                Assert.IsFalse(bottomScrim.activeSelf, "Paisagem nao deve ativar gradiente inferior");
                Assert.AreEqual(0f, artRect.anchoredPosition.x, 0.5f);

                canvasRect.sizeDelta = new Vector2(979f, 2118f);
                layout.ApplyLayout(force: true);
                Assert.IsFalse(leftShade.activeSelf, "Retrato deve ocultar sombra lateral");
                Assert.IsTrue(bottomScrim.activeSelf, "Retrato deve ativar gradiente superior");
                Assert.Less(artRect.anchoredPosition.x, -100f, "Retrato deve centralizar a menina na poltrona");
                Assert.Greater(artRect.sizeDelta.x, canvasRect.sizeDelta.x);
                Assert.AreEqual(1f, newGameGo.GetComponent<RectTransform>().anchorMin.y, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvasGo);
                UnityEngine.Object.DestroyImmediate(leftShade);
                UnityEngine.Object.DestroyImmediate(bottomScrim);
                UnityEngine.Object.DestroyImmediate(titleGo);
                UnityEngine.Object.DestroyImmediate(newGameGo);
                UnityEngine.Object.DestroyImmediate(footerGo);
            }
        }

        [Test]
        public void TitleScene_PhaseSelector_ListsArenaAndSetsDestination()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/TitleScreen.unity");
            try
            {
                var controller = UnityEngine.Object.FindFirstObjectByType<TitleScreenController>();
                var selector = UnityEngine.Object.FindFirstObjectByType<TitlePhaseSelector>();
                Assert.IsNotNull(controller);
                Assert.IsNotNull(selector);
                Assert.GreaterOrEqual(selector.ItemCount, 3, "Bandeirinha deve listar Fase 1, Toy Room e Arena.");

                selector.Open();
                Assert.IsTrue(selector.IsOpen);
                var item2 = UnityEngine.GameObject.Find("Fase_02")?.GetComponent<Button>();
                Assert.IsNotNull(item2, "Botao Fase_02 (Arena Imaginaria) deve existir no dropdown.");
                var label2 = item2.GetComponentInChildren<Text>();
                Assert.IsNotNull(label2);
                StringAssert.Contains("Arena", label2.text);
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
            }
        }
    }
}
