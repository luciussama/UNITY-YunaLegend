using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Illustrated25D.Environment;
using Object = UnityEngine.Object;

namespace YunaLegend.Illustrated25D.Environment.Editor
{
    /// <summary>Adds the runtime room-composition installer to DarkRoom_Gameplay and saves the scene.</summary>
    public static class RoomCompositionRuntimeSetup
    {
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Room = "Assets/Art/Illustrated25D/Environment/Room/";
        const string RootName = "T054_Composicao_Runtime";

        public static void Run()
        {
            EditorSceneManager.OpenScene(Gameplay, OpenSceneMode.Single);
            var root = GameObject.Find(RootName);
            if (root == null) root = new GameObject(RootName);
            var installer = root.GetComponent<RoomCompositionRuntimeInstaller>();
            if (installer == null) installer = root.AddComponent<RoomCompositionRuntimeInstaller>();

            installer.panorama = Load<Texture2D>(Room + "room-panorama-v4.png");
            installer.penumbraShader = Load<Shader>(Room + "PenumbraT054.shader");
            installer.floorInsetFraction = 82f / 724f;
            installer.farTravel = .35f;
            installer.installPenumbra = false; // pending: runtime penumbra tints the approved Yuna; keep identity first
            EditorUtility.SetDirty(installer);
            EditorSceneManager.SaveScene(root.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("T054_RUNTIME_SAVED root=" + root.name);
        }

        static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Missing asset: " + path);
            return asset;
        }
    }
}

