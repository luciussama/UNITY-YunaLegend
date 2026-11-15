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
        public const string ContinuationScene = "Assets/Art/Illustrated25D/Environment/Dresser/Preview-Continuacao.unity";
        public const string BlocksScene = "Assets/Art/Illustrated25D/Environment/ABC/Preview-Continuacao-04.unity";
        public const string DrumScene = "Assets/Art/Illustrated25D/Environment/Drum/Preview-Continuacao-05.unity";
        public const string Drawers07Scene = "Assets/Art/Illustrated25D/Environment/Drawers/Preview-Continuacao-07.unity";
        public const string Castle09Scene = "Assets/Art/Illustrated25D/Environment/Castle/Preview-Continuacao-09.unity";
        public const string Playable0010Scene = "Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-00-10.unity";
        public const string Output = "Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe";
        public const string ContinuationOutput = "Build/Windows/YunaLegend-Continuacao/YunaLegend.exe";
        public const string BlocksOutput = "Build/Windows/YunaLegend-Continuacao-04/YunaLegend.exe";
        public const string DrumOutput = "Build/Windows/YunaLegend-Continuacao-05/YunaLegend.exe";
        public const string Drawers07Output = "Build/Windows/YunaLegend-Continuacao-07/YunaLegend.exe";
        public const string Castle09Output = "Build/Windows/YunaLegend-Continuacao-09/YunaLegend.exe";
        public const string Playable0010Output = "Build/Windows/YunaLegend-Jogavel-00-10/YunaLegend.exe";

        enum Route { Accompaniment, Dresser, Blocks, Drum, Drawers07, Castle09, PlayableStage2P10 }

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
                var report = BuildPlayer(Route.Accompaniment, Output);
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

        public static void Setup() { Setup(Route.Accompaniment); }

        static void Setup(Route route)
        {
            bool continuation = route != Route.Accompaniment;
            bool playable0010 = route == Route.PlayableStage2P10;
            bool castle09 = route == Route.Castle09 || playable0010;
            bool drawers07 = route == Route.Drawers07 || castle09;
            bool drum = route == Route.Drum || drawers07;
            bool blocks = route == Route.Blocks || drum;
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            // A dedicated scene preserves production narrative, menu and the original checkpoint.
            foreach (var c in Object.FindObjectsByType<DarkRoomOpeningSequence>(FindObjectsSortMode.None)) c.enabled=false;
            foreach (var c in Object.FindObjectsByType<DarkRoomProgressionManager>(FindObjectsSortMode.None)) c.enabled=false;
            foreach (var c in Object.FindObjectsByType<DarkRoomBlockoutBuilder>(FindObjectsSortMode.None)) c.enabled=false;
            var go=new GameObject("Acompanhamento_Ilustrado");
            var preview=go.AddComponent<IllustratedPreview>();
            preview.includeNextDresser=continuation;
            preview.includeNextBlocks=blocks;
            preview.includeNextDrum=drum;
            preview.includeNextDrawers07=drawers07;
            preview.includeNextCastle09=castle09;
            preview.includeNextStage2Platform10=playable0010;
            preview.playableLegacyMode=playable0010;
            if(continuation)
            {
                const string path="Assets/Art/Illustrated25D/Environment/Dresser/dresser-v1.png";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
                importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                importer.SaveAndReimport();preview.dresserPainting=Load<Texture2D>(path);
            }
            if(blocks)
            {
                const string path="Assets/Art/Illustrated25D/Environment/ABC/abc-stack-v1.png";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
                importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                importer.SaveAndReimport();preview.blocksPainting=Load<Texture2D>(path);
            }
            if(drum)
            {
                const string path="Assets/Art/Illustrated25D/Environment/Drum/drum-05-v1.png";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
                importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                importer.SaveAndReimport();preview.drumPainting=Load<Texture2D>(path);
            }
            if(drawers07)
            {
                foreach(var p in new[]{"Assets/Art/Illustrated25D/Environment/Cushion/cushion-06-v1.png","Assets/Art/Illustrated25D/Environment/Drawers/drawers-07-v1.png"})
                {
                    AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);
                    var importer=(TextureImporter)AssetImporter.GetAtPath(p);
                    importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
                    importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                    importer.SaveAndReimport();
                }
                preview.cushionPainting=Load<Texture2D>("Assets/Art/Illustrated25D/Environment/Cushion/cushion-06-v1.png");
                preview.drawersPainting=Load<Texture2D>("Assets/Art/Illustrated25D/Environment/Drawers/drawers-07-v1.png");
            }
            if(castle09)
            {
                foreach(var p in new[]{"Assets/Art/Illustrated25D/Environment/MusicBox/music-box-08-v2.png","Assets/Art/Illustrated25D/Environment/Castle/block-castle-09-v1.png","Assets/Art/Illustrated25D/Environment/Castle/block-castle-09-front-towers.png"})
                {
                    AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);
                    var importer=(TextureImporter)AssetImporter.GetAtPath(p);
                    importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
                    importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                    importer.SaveAndReimport();
                }
                preview.musicBoxPainting=Load<Texture2D>("Assets/Art/Illustrated25D/Environment/MusicBox/music-box-08-v2.png");
                preview.castlePainting=Load<Texture2D>("Assets/Art/Illustrated25D/Environment/Castle/block-castle-09-v1.png");
                preview.castleFrontTowersPainting=Load<Texture2D>("Assets/Art/Illustrated25D/Environment/Castle/block-castle-09-front-towers.png");
            }
            if(playable0010)
            {
                const string p10 = "Assets/Art/Illustrated25D/Environment/Stage2/10-train-trestle-v1.png";
                AssetDatabase.ImportAsset(p10, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(p10);
                importer.textureType = TextureImporterType.Default; importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
                preview.trainTrestle10Painting = Load<Texture2D>(p10);
            }
            const string room="Assets/Art/Illustrated25D/Environment/Room/";
            const string furniture="Assets/Art/Illustrated25D/Environment/Furniture/";
            preview.panorama=Load<Texture2D>(room+"room-panorama-v4.png");
            if(castle09)
            {
                const string extPath = room + "room-wall-extension-v1.png";
                AssetDatabase.ImportAsset(extPath, ImportAssetOptions.ForceSynchronousImport);
                var extImp = (TextureImporter)AssetImporter.GetAtPath(extPath);
                extImp.textureType = TextureImporterType.Default; extImp.npotScale = TextureImporterNPOTScale.None;
                extImp.maxTextureSize = 4096; extImp.textureCompression = TextureImporterCompression.Uncompressed;
                extImp.mipmapEnabled = false; extImp.wrapMode = TextureWrapMode.Clamp;
                extImp.SaveAndReimport();
                preview.roomExtension = Load<Texture2D>(extPath);
            }
            preview.furnitureBody=Load<Texture2D>(furniture+"furniture-body-v1.png");
            preview.furnitureWood=Load<Texture2D>(furniture+"furniture-top-wood-v1.png");
            preview.nandaFrames=new Sprite[8];
            for(int i=0;i<8;i++) preview.nandaFrames[i]=Load<Sprite>(
                "Assets/Art/Illustrated25D/Nanda/Flight/Runtime/Frames/NandaFlight"+i+".asset");
            preview.requiredShaders=new[]{Shader.Find("YunaLegend/RoomPenumbraT054"),
                Shader.Find("Sprites/Default"),Shader.Find("Unlit/Texture")};
            if(continuation) preview.requiredShaders=new[]{preview.requiredShaders[0],preview.requiredShaders[1],
                preview.requiredShaders[2],Shader.Find("YunaLegend/DresserFloorReceiver")};
            foreach(var shader in preview.requiredShaders)
                if(shader==null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Required shader missing/invalid");
            go.AddComponent<PreviewVerification>();
            go.AddComponent<PreviewRetryVerification>();
            var player=Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if(player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationInstaller>()==null)
                throw new InvalidOperationException("Approved Yuna installer missing");
            string scenePath=playable0010?Playable0010Scene:castle09?Castle09Scene:drawers07?Drawers07Scene:drum?DrumScene:blocks?BlocksScene:continuation?ContinuationScene:Scene;
            EditorSceneManager.SaveScene(player.gameObject.scene,scenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("ILLUSTRATED_PREVIEW_SCENE_SAVED "+scenePath);
        }
        public static void SetupPlayableStage2P10() { Setup(Route.PlayableStage2P10); }
        public static void Run() { Exit(BuildPlayer(Route.Accompaniment,Output)); }
        public static void RunContinuation() { Exit(BuildPlayer(Route.Dresser,ContinuationOutput)); }
        public static void RunContinuationBlocks() { Exit(BuildPlayer(Route.Blocks,BlocksOutput)); }
        public static void RunContinuationDrum() { Exit(BuildPlayer(Route.Drum,DrumOutput)); }
        public static void RunContinuationDrawers07() { Exit(BuildPlayer(Route.Drawers07,Drawers07Output)); }
        public static void RunContinuationCastle09() { Exit(BuildPlayer(Route.Castle09,Castle09Output)); }
        public static void RunPlayableStage2P10() { Exit(BuildPlayer(Route.PlayableStage2P10,Playable0010Output)); }
        static void Exit(BuildReport report) => EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
        static BuildReport BuildPlayer(Route route,string destination)
        {
            Setup(route);
            var output=Path.GetFullPath(destination);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var scene=route==Route.PlayableStage2P10?Playable0010Scene:route==Route.Castle09?Castle09Scene:route==Route.Drawers07?Drawers07Scene:route==Route.Drum?DrumScene:route==Route.Blocks?BlocksScene:route==Route.Dresser?ContinuationScene:Scene;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{scene}, locationPathName=output,target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.None});
            Debug.Log("ILLUSTRATED_PREVIEW_BUILD "+report.summary.result+" errors="+report.summary.totalErrors+" bytes="+report.summary.totalSize);
            return report;
        }
        static T Load<T>(string path) where T:Object
        { var value=AssetDatabase.LoadAssetAtPath<T>(path);if(value==null)throw new InvalidOperationException(path);return value; }
    }
}
