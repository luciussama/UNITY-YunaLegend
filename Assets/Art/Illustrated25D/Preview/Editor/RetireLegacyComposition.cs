using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YunaLegend.Illustrated25D.Preview.Editor
{
    /// <summary>
    /// One-shot migration to the official accompaniment preview:
    /// removes the retired T054 composition object from both scenes and deletes its assets.
    /// Safe to run twice (idempotent).
    /// </summary>
    public static class RetireLegacyComposition
    {
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Preview = "Assets/Scenes/DarkRoom_IllustratedPreview.unity";
        const string ObjectName = "T054_Composicao_Runtime";
        static readonly string[] RetiredAssets =
        {
            "Assets/Art/Illustrated25D/Environment/Room/RoomCompositionRuntimeInstaller.cs",
            "Assets/Art/Illustrated25D/Environment/Room/Editor/RoomCompositionRuntimeSetup.cs"
        };

        public static void Run()
        {
            int removed = 0;
            foreach (var scene in new[] { Gameplay, Preview })
            {
                EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
                var target = GameObject.Find(ObjectName);
                if (target == null)
                {
                    Debug.Log("RETIRE_SKIP " + scene);
                    continue;
                }
                Object.DestroyImmediate(target);
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                removed++;
                Debug.Log("RETIRE_REMOVED " + ObjectName + " from " + scene);
            }
            AssetDatabase.Refresh();
            foreach (var asset in RetiredAssets)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(asset) == null) continue;
                if (!AssetDatabase.DeleteAsset(asset))
                    throw new InvalidOperationException("Could not delete " + asset);
                Debug.Log("RETIRE_DELETED " + asset);
            }
            AssetDatabase.Refresh();
            Debug.Log("RETIRE_DONE scenes=" + removed);
        }
    }
}
