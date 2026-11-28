using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace YunaLegend.Tests
{
    /// <summary>
    /// QA PlayMode smoke suite verifying that TitleScreen and all playable stages load cleanly
    /// in PlayMode with zero missing scripts and an active MainCamera after the code-review cleanup.
    /// </summary>
    public sealed class StageLoadSmokePlayModeTests
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/TitleScreen.unity",
            "Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity",
            "Assets/Scenes/ToyRoom_Gameplay.unity",
            "Assets/Scenes/Arena_Gameplay.unity",
            "Assets/Scenes/BearChase_Gameplay.unity",
            "Assets/Scenes/Picnic_Gameplay.unity",
            "Assets/Scenes/DreamForest_Gameplay.unity",
        };

        [UnityTest]
        public IEnumerator AllSixStagesAndTitleScreen_LoadCleanlyInPlayModeWithZeroMissingScripts()
        {
            for (int i = 0; i < ScenePaths.Length; i++)
            {
                string path = ScenePaths[i];
#if UNITY_EDITOR
                yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    path,
                    new LoadSceneParameters(LoadSceneMode.Single));
#else
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                yield return SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
#endif
                // Step 5 frames so Awake/Start/Update/LateUpdate all run cleanly
                for (int f = 0; f < 5; f++)
                {
                    yield return null;
                }

                // Verify zero missing MonoBehaviour scripts on any GameObject in the loaded scene
                var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                int missingCount = 0;
                for (int o = 0; o < allObjects.Length; o++)
                {
                    var comps = allObjects[o].GetComponents<Component>();
                    for (int c = 0; c < comps.Length; c++)
                    {
                        if (comps[c] == null)
                        {
                            missingCount++;
                            Debug.LogError($"[QA_SMOKE] Missing script on '{allObjects[o].name}' in {path}");
                        }
                    }
                }

                Assert.AreEqual(0, missingCount, $"Missing scripts found in {path}");
                Assert.IsNotNull(Camera.main, $"Camera.main missing in {path}");
                Debug.Log($"[QA_SMOKE_OK] {path} (objects={allObjects.Length}, missing=0)");
            }
        }
    }
}
