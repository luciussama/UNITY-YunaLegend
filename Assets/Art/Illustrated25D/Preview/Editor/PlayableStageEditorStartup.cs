using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YunaLegend.Illustrated25D.Preview.Editor
{
    /// <summary>
    /// Configura o Unity Editor para abrir automaticamente na cena jogável completa 2.5D
    /// e iniciar o Play Mode nela por padrão (fora de batchmode/testes automatizados).
    /// </summary>
    [InitializeOnLoad]
    public static class PlayableStageEditorStartup
    {
        private const string AutoPlayPrefKey = "YunaLegend.AutoPlayFullStage";
        private const string OpenedSessionKey = "YunaLegend.OpenedPlayableSceneOnce";
        private const string ToggleMenuPath = "YunaLegend/Iniciar Play na fase completa (2.5D)";

        static PlayableStageEditorStartup()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.delayCall += ApplyStartupConfiguration;
        }

        private static bool IsEnabled
        {
            get => EditorPrefs.GetBool(AutoPlayPrefKey, false);
            set => EditorPrefs.SetBool(AutoPlayPrefKey, value);
        }

        [MenuItem(ToggleMenuPath, false, 10)]
        private static void ToggleAutoPlayFullStage()
        {
            IsEnabled = !IsEnabled;
            ApplyPlayModeStartScene();
        }

        [MenuItem(ToggleMenuPath, true)]
        private static bool ValidateToggleAutoPlayFullStage()
        {
            Menu.SetChecked(ToggleMenuPath, IsEnabled);
            return true;
        }

        private static void ApplyStartupConfiguration()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            ApplyPlayModeStartScene();

            if (!IsEnabled)
            {
                return;
            }

            if (SessionState.GetBool(OpenedSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(OpenedSessionKey, true);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == IllustratedPreviewBuild.PlayableFullStageScene)
            {
                return;
            }

            if (activeScene.isDirty)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(IllustratedPreviewBuild.PlayableFullStageScene) != null)
            {
                EditorSceneManager.OpenScene(IllustratedPreviewBuild.PlayableFullStageScene, OpenSceneMode.Single);
            }
        }

        private static void ApplyPlayModeStartScene()
        {
            if (!IsEnabled)
            {
                SceneAsset titleScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/TitleScreen.unity");
                EditorSceneManager.playModeStartScene = titleScene;
                return;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(IllustratedPreviewBuild.PlayableFullStageScene);
            if (sceneAsset != null)
            {
                EditorSceneManager.playModeStartScene = sceneAsset;
            }
        }
    }
}
