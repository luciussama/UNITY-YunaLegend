using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.YunaIllustration;
using Object = UnityEngine.Object;

namespace YunaLegend.YunaIllustration.Editor
{
    /// <summary>Adds the runtime installer to the real player in DarkRoom_Gameplay and saves the scene.</summary>
    public static class YunaIllustrationInstallerSetup
    {
        const string Yuna = "Assets/Art/Illustrated25D/Yuna";
        const string Runtime = Yuna + "/Joint/Runtime";
        const string Frames = Runtime + "/Frames";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";

        public static void Run()
        {
            EditorSceneManager.OpenScene(Gameplay, OpenSceneMode.Single);
            var player = Object.FindFirstObjectByType<YunaLegend.DarkRoom.Yuna25DPlayerController>();
            if (player == null) throw new InvalidOperationException("Yuna25DPlayerController not found in gameplay.");

            var installer = player.GetComponent<YunaIllustrationInstaller>();
            if (installer == null) installer = player.gameObject.AddComponent<YunaIllustrationInstaller>();

            installer.profile = Load<Sprite>(Runtime + "/Yuna_Profile_Runtime_v1.png");
            installer.blink = Load<Texture2D>(Runtime + "/Yuna_Blink_Runtime_v1.png");
            installer.idleMaterial = Load<Material>(Runtime + "/Idle_Painted_Runtime.mat");
            installer.runMaterial = Load<Material>(Yuna + "/Run/Run_Painted.mat");
            installer.stumbleMaterial = Load<Material>(Yuna + "/Stumble/Stumble_Painted.mat");
            installer.runFrames = FramesOf("Run");
            installer.jumpFrames = FramesOf("Jump");
            installer.stumbleFrames = FramesOf("Stumble");
            installer.hide3D = true;
            EditorUtility.SetDirty(installer);
            EditorSceneManager.SaveScene(player.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("T061_INSTALLER_SAVED player=" + player.name);
        }

        static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Missing asset: " + path);
            return asset;
        }

        static Sprite[] FramesOf(string action)
        {
            var frames = new Sprite[8];
            for (int i = 0; i < 8; i++) frames[i] = Load<Sprite>(Frames + "/" + action + i + ".asset");
            return frames;
        }
    }
}
