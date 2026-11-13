using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;
using Object = UnityEngine.Object;

namespace YunaLegend.Illustrated25D.Preview.Editor
{
    public static class IllustratedPreviewBuild
    {
        public const string Scene = "Assets/Scenes/DarkRoom_IllustratedPreview.unity";
        public const string Output = "Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe";

        [MenuItem("YunaLegend/Abrir cena de acompanhamento", false, 10)]
        public static void OpenPreview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(Scene);
        }

        [MenuItem("YunaLegend/Gerar build Windows de acompanhamento", false, 20)]
        public static void BuildFromEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var previousScenes = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var report = BuildPlayer();
                if (report.summary.result == BuildResult.Succeeded) OpenBuildFolder();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previousScenes); }
        }

        [MenuItem("YunaLegend/Abrir pasta da build", false, 30)]
        public static void OpenBuildFolder()
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(Output));
            if (Directory.Exists(folder)) EditorUtility.RevealInFinder(folder);
            else Debug.LogWarning("A build ainda não foi gerada. Use YunaLegend > Gerar build Windows de acompanhamento.");
        }

        public static void Setup()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            // A dedicated scene preserves production narrative, menu and the original checkpoint.
            foreach (var c in Object.FindObjectsByType<DarkRoomOpeningSequence>(FindObjectsSortMode.None)) c.enabled=false;
            foreach (var c in Object.FindObjectsByType<DarkRoomProgressionManager>(FindObjectsSortMode.None)) c.enabled=false;
            foreach (var c in Object.FindObjectsByType<DarkRoomBlockoutBuilder>(FindObjectsSortMode.None)) c.enabled=false;
            var go=new GameObject("Acompanhamento_Ilustrado");
            var preview=go.AddComponent<IllustratedPreview>();
            const string room="Assets/Art/Illustrated25D/Environment/Room/";
            const string furniture="Assets/Art/Illustrated25D/Environment/Furniture/";
            preview.panorama=Load<Texture2D>(room+"room-panorama-v4.png");
            preview.furnitureBody=Load<Texture2D>(furniture+"furniture-body-v1.png");
            preview.furnitureWood=Load<Texture2D>(furniture+"furniture-top-wood-v1.png");
            preview.nandaFrames=new Sprite[8];
            for(int i=0;i<8;i++) preview.nandaFrames[i]=Load<Sprite>(
                "Assets/Art/Illustrated25D/Nanda/Flight/Runtime/Frames/NandaFlight"+i+".asset");
            preview.requiredShaders=new[]{Shader.Find("YunaLegend/RoomPenumbraT054"),
                Shader.Find("Sprites/Default"),Shader.Find("Unlit/Texture")};
            foreach(var shader in preview.requiredShaders)
                if(shader==null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Required shader missing/invalid");
            go.AddComponent<PreviewVerification>();
            var player=Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if(player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationInstaller>()==null)
                throw new InvalidOperationException("Approved Yuna installer missing");
            EditorSceneManager.SaveScene(player.gameObject.scene,Scene);
            AssetDatabase.SaveAssets();
            Debug.Log("ILLUSTRATED_PREVIEW_SCENE_SAVED "+Scene);
        }
        public static void Run()
        {
            var report = BuildPlayer();
            EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
        }
        static BuildReport BuildPlayer()
        {
            Setup();
            var output=Path.GetFullPath(Output);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{Scene}, locationPathName=output,target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.None});
            Debug.Log("ILLUSTRATED_PREVIEW_BUILD "+report.summary.result+" errors="+report.summary.totalErrors+" bytes="+report.summary.totalSize);
            return report;
        }
        static T Load<T>(string path) where T:Object
        { var value=AssetDatabase.LoadAssetAtPath<T>(path);if(value==null)throw new InvalidOperationException(path);return value; }
    }
}
