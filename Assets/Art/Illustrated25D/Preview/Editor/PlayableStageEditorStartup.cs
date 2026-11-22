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
        private const string ToggleMenuPath = "YunaLegend/Fase 1 - Dark Room/Iniciar Play na fase completa (2.5D)";

        static PlayableStageEditorStartup()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChangedInEditMode;
            EditorApplication.delayCall += ApplyStartupConfiguration;
        }

        private static void OnActiveSceneChangedInEditMode(Scene previousScene, Scene newScene)
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            ApplyPlayModeStartScene();
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
            if (activeScene.path == IllustratedPreviewBuild.PlayableFullStageScene
                || activeScene.path == "Assets/Scenes/ToyRoom_Gameplay.unity"
                || activeScene.path == "Assets/Scenes/Arena_Gameplay.unity"
                || activeScene.path == "Assets/Scenes/Arena_3D_Showcase.unity"
                || activeScene.path == "Assets/Scenes/BearChase_Gameplay.unity")
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
            // Se o usuario abriu explicitamente a Toy Room (Fase 2) ou a Arena (Fase 3), o Play deve iniciar nela
            string activePath = SceneManager.GetActiveScene().path;
            if (activePath == "Assets/Scenes/ToyRoom_Gameplay.unity"
                || activePath == "Assets/Scenes/Arena_Gameplay.unity"
                || activePath == "Assets/Scenes/Arena_3D_Showcase.unity"
                || activePath == "Assets/Scenes/BearChase_Gameplay.unity")
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

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
