using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;
using Object = UnityEngine.Object;

namespace YunaLegend.Illustrated25D.Preview.Editor
{
    /// <summary>
    /// Orquestra a montagem das cenas de acompanhamento e a geração de builds Windows64
    /// para cada marco do circuito 2.5D ilustrado.
    /// </summary>
    public static class IllustratedPreviewBuild
    {
        public const string Scene = "Assets/Scenes/DarkRoom_IllustratedPreview.unity";
        public const string ContinuationScene = "Assets/Art/Illustrated25D/Environment/Dresser/Preview-Continuacao.unity";
        public const string BlocksScene = "Assets/Art/Illustrated25D/Environment/ABC/Preview-Continuacao-04.unity";
        public const string DrumScene = "Assets/Art/Illustrated25D/Environment/Drum/Preview-Continuacao-05.unity";
        public const string Drawers07Scene = "Assets/Art/Illustrated25D/Environment/Drawers/Preview-Continuacao-07.unity";
        public const string Castle09Scene = "Assets/Art/Illustrated25D/Environment/Castle/Preview-Continuacao-09.unity";
        public const string Playable0010Scene = "Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-00-10.unity";
        public const string PlayableFullStageScene = "Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity";

        public const string Output = "Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe";
        public const string ContinuationOutput = "Build/Windows/YunaLegend-Continuacao/YunaLegend.exe";
        public const string BlocksOutput = "Build/Windows/YunaLegend-Continuacao-04/YunaLegend.exe";
        public const string DrumOutput = "Build/Windows/YunaLegend-Continuacao-05/YunaLegend.exe";
        public const string Drawers07Output = "Build/Windows/YunaLegend-Continuacao-07/YunaLegend.exe";
        public const string Castle09Output = "Build/Windows/YunaLegend-Continuacao-09/YunaLegend.exe";
        public const string Playable0010Output = "Build/Windows/YunaLegend-Jogavel-00-10/YunaLegend.exe";
        public const string PlayableFullStageOutput = "Build/Windows/YunaLegend-Jogavel-Completa/YunaLegend.exe";
        public const string PlayableFullStageMacOutput = "Build/macOS/YunaLegend-Fase-Completa.app";

        private const string BaseGameplayScene = "Assets/Scenes/DarkRoom_Gameplay.unity";
        private const string RoomArtRoot = "Assets/Art/Illustrated25D/Environment/Room/";
        private const string FurnitureArtRoot = "Assets/Art/Illustrated25D/Environment/Furniture/";

        private enum Route
        {
            Accompaniment,
            Dresser,
            Blocks,
            Drum,
            Drawers07,
            Castle09,
            PlayableStage2P10,
            PlayableFullStage
        }

        private readonly struct RouteSpec
        {
            public readonly string ScenePath;
            public readonly string OutputPath;
            public readonly int MaxPlatformIndex;
            public readonly bool PlayableLegacyMode;

            public RouteSpec(string scenePath, string outputPath, int maxPlatformIndex, bool playableLegacyMode = false)
            {
                ScenePath = scenePath;
                OutputPath = outputPath;
                MaxPlatformIndex = maxPlatformIndex;
                PlayableLegacyMode = playableLegacyMode;
            }

            public bool IncludeDresser => MaxPlatformIndex >= 3;
            public bool IncludeBlocks => MaxPlatformIndex >= 4;
            public bool IncludeDrum => MaxPlatformIndex >= 5;
            public bool IncludeDrawers07 => MaxPlatformIndex >= 7;
            public bool IncludeCastle09 => MaxPlatformIndex >= 9;
            public bool IncludeStage2P10 => MaxPlatformIndex >= 10;
            public bool IncludeFullStage => MaxPlatformIndex >= 37;
        }

        private static readonly Dictionary<Route, RouteSpec> RouteTable = new Dictionary<Route, RouteSpec>
        {
            { Route.Accompaniment, new RouteSpec(Scene, Output, 2) },
            { Route.Dresser, new RouteSpec(ContinuationScene, ContinuationOutput, 3) },
            { Route.Blocks, new RouteSpec(BlocksScene, BlocksOutput, 4) },
            { Route.Drum, new RouteSpec(DrumScene, DrumOutput, 5) },
            { Route.Drawers07, new RouteSpec(Drawers07Scene, Drawers07Output, 7) },
            { Route.Castle09, new RouteSpec(Castle09Scene, Castle09Output, 9) },
            { Route.PlayableStage2P10, new RouteSpec(Playable0010Scene, Playable0010Output, 10, playableLegacyMode: true) },
            { Route.PlayableFullStage, new RouteSpec(PlayableFullStageScene, PlayableFullStageOutput, 37, playableLegacyMode: true) }
        };

        public static void OpenPreview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(Scene);
            }
        }

        [MenuItem("YunaLegend/Fase 1 - Dark Room/Jogar (Play Direto)", false, 10)]
        public static void PlayPlayableFullStage()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(PlayableFullStageScene, OpenSceneMode.Single);
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.isPlaying = true;
        }

        [MenuItem("YunaLegend/Fase 1 - Dark Room/Abrir Cena", false, 20)]
        public static void OpenPlayableFullStage()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.playModeStartScene = null;
                EditorSceneManager.OpenScene(PlayableFullStageScene, OpenSceneMode.Single);
            }
        }

        public static void BuildFromEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var previousScenes = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var report = BuildPlayer(Route.Accompaniment, Output);
                if (report.summary.result == BuildResult.Succeeded)
                {
                    OpenBuildFolder();
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousScenes);
            }
        }

        public static void OpenBuildFolder()
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(Output));
            if (Directory.Exists(folder))
            {
                EditorUtility.RevealInFinder(folder);
            }
            else
            {
                Debug.LogWarning("A build ainda não foi gerada. Use YunaLegend > Fase 1 - Dark Room > Gerar build Windows de acompanhamento.");
            }
        }

        public static void Setup() => Setup(Route.Accompaniment);
        public static void SetupPlayableStage2P10() => Setup(Route.PlayableStage2P10);
        public static void SetupPlayableFullStage() => Setup(Route.PlayableFullStage);

        public static void Run() => Exit(BuildPlayer(Route.Accompaniment, Output));
        public static void RunContinuation() => Exit(BuildPlayer(Route.Dresser, ContinuationOutput));
        public static void RunContinuationBlocks() => Exit(BuildPlayer(Route.Blocks, BlocksOutput));
        public static void RunContinuationDrum() => Exit(BuildPlayer(Route.Drum, DrumOutput));
        public static void RunContinuationDrawers07() => Exit(BuildPlayer(Route.Drawers07, Drawers07Output));
        public static void RunContinuationCastle09() => Exit(BuildPlayer(Route.Castle09, Castle09Output));
        public static void RunPlayableStage2P10() => Exit(BuildPlayer(Route.PlayableStage2P10, Playable0010Output));
        public static void RunPlayableFullStage() => Exit(BuildPlayer(Route.PlayableFullStage, PlayableFullStageOutput));
        public static void RunPlayableFullStageMac() => Exit(BuildPlayer(Route.PlayableFullStage, PlayableFullStageMacOutput, BuildTarget.StandaloneOSX));

        private static void Setup(Route route)
        {
            var spec = RouteTable[route];
            EditorSceneManager.OpenScene(BaseGameplayScene);

            DisableProductionFlowManagers();

            var previewRoot = new GameObject("Acompanhamento_Ilustrado");
            var preview = previewRoot.AddComponent<IllustratedPreview>();
            ConfigureRouteFlags(preview, spec);
            AssignPlatformTextures(preview, spec);
            AssignSharedAssets(preview, spec);

            previewRoot.AddComponent<PreviewVerification>();
            previewRoot.AddComponent<PreviewRetryVerification>();

            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if (player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationInstaller>() == null)
            {
                throw new InvalidOperationException("Approved Yuna installer missing");
            }

            EditorSceneManager.SaveScene(player.gameObject.scene, spec.ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("ILLUSTRATED_PREVIEW_SCENE_SAVED " + spec.ScenePath);
        }

        private static void DisableProductionFlowManagers()
        {
            // A dedicated scene preserves production narrative, menu and the original checkpoint.
            foreach (var c in Object.FindObjectsByType<DarkRoomOpeningSequence>(FindObjectsSortMode.None))
            {
                c.enabled = false;
            }
            foreach (var c in Object.FindObjectsByType<DarkRoomProgressionManager>(FindObjectsSortMode.None))
            {
                c.enabled = false;
            }
            foreach (var c in Object.FindObjectsByType<DarkRoomBlockoutBuilder>(FindObjectsSortMode.None))
            {
                c.enabled = false;
            }
        }

        private static void ConfigureRouteFlags(IllustratedPreview preview, RouteSpec spec)
        {
            preview.includeNextDresser = spec.IncludeDresser;
            preview.includeNextBlocks = spec.IncludeBlocks;
            preview.includeNextDrum = spec.IncludeDrum;
            preview.includeNextDrawers07 = spec.IncludeDrawers07;
            preview.includeNextCastle09 = spec.IncludeCastle09;
            preview.includeNextStage2Platform10 = spec.IncludeStage2P10;
            preview.includeFullStage = spec.IncludeFullStage;
            preview.playableLegacyMode = spec.PlayableLegacyMode;
        }

        private static void AssignPlatformTextures(IllustratedPreview preview, RouteSpec spec)
        {
            if (spec.IncludeDresser)
            {
                preview.dresserPainting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/Dresser/dresser-v1.png");
            }
            if (spec.IncludeBlocks)
            {
                preview.blocksPainting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/ABC/abc-stack-v1.png");
            }
            if (spec.IncludeDrum)
            {
                preview.drumPainting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/Drum/drum-05-v1.png");
            }
            if (spec.IncludeDrawers07)
            {
                preview.cushionPainting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/Cushion/cushion-06-v1.png");
                preview.drawersPainting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/Drawers/drawers-07-v1.png");
            }
            if (spec.IncludeCastle09)
            {
                preview.musicBoxPainting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/MusicBox/music-box-08-v2.png");
                preview.castlePainting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/Castle/block-castle-09-v1.png");
                preview.castleFrontTowersPainting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/Castle/block-castle-09-front-towers.png");
                preview.roomExtension = ImportUncompressedTexture(RoomArtRoot + "room-wall-extension-v1.png", maxSize: 4096, alphaIsTransparency: false);
            }
            if (spec.IncludeStage2P10)
            {
                preview.trainTrestle10Painting = ImportUncompressedTexture("Assets/Art/Illustrated25D/Environment/Stage2/10-train-trestle-v1.png");
            }
            if (spec.IncludeFullStage)
            {
                preview.fullStagePaintings = new Texture2D[27];
                for (int index = 11; index <= 37; index++)
                {
                    string assetPath = Controllers.DarkRoomFullStageCatalog.GetTextureAssetPath(index);
                    preview.fullStagePaintings[index - 11] = ImportUncompressedTexture(assetPath);
                }
            }
        }

        private static void AssignSharedAssets(IllustratedPreview preview, RouteSpec spec)
        {
            preview.panorama = Load<Texture2D>(RoomArtRoot + "room-panorama-v4.png");
            preview.furnitureBody = Load<Texture2D>(FurnitureArtRoot + "furniture-body-v1.png");
            preview.furnitureWood = Load<Texture2D>(FurnitureArtRoot + "furniture-top-wood-v1.png");

            preview.nandaFrames = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                preview.nandaFrames[i] = Load<Sprite>(
                    $"Assets/Art/Illustrated25D/Nanda/Flight/Runtime/Frames/NandaFlight{i}.asset");
            }

            preview.requiredShaders = CollectRequiredShaders(spec.IncludeDresser);
        }

        private static Shader[] CollectRequiredShaders(bool includeFloorReceiver)
        {
            var shaders = includeFloorReceiver
                ? new[]
                {
                    Shader.Find("YunaLegend/RoomPenumbraT054"),
                    Shader.Find("Sprites/Default"),
                    Shader.Find("Unlit/Texture"),
                    Shader.Find("YunaLegend/DresserFloorReceiver")
                }
                : new[]
                {
                    Shader.Find("YunaLegend/RoomPenumbraT054"),
                    Shader.Find("Sprites/Default"),
                    Shader.Find("Unlit/Texture")
                };

            foreach (var shader in shaders)
            {
                if (shader == null || ShaderUtil.ShaderHasError(shader))
                {
                    throw new InvalidOperationException("Required shader missing/invalid");
                }
            }

            return shaders;
        }

        private static Texture2D ImportUncompressedTexture(string assetPath, int maxSize = 2048, bool alphaIsTransparency = true)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = maxSize;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = alphaIsTransparency;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return Load<Texture2D>(assetPath);
        }

        private static BuildReport BuildPlayer(Route route, string destination, BuildTarget target = BuildTarget.StandaloneWindows64)
        {
            Setup(route);
            var spec = RouteTable[route];
            var output = Path.GetFullPath(destination);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (target == BuildTarget.StandaloneOSX && Directory.Exists(output))
            {
                Directory.Delete(output, true);
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { spec.ScenePath },
                locationPathName = output,
                target = target,
                options = BuildOptions.None
            });

            Debug.Log($"ILLUSTRATED_PREVIEW_BUILD {report.summary.result} target={target} errors={report.summary.totalErrors} bytes={report.summary.totalSize}");
            return report;
        }

        private static void Exit(BuildReport report)
        {
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static T Load<T>(string path) where T : Object
        {
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value == null)
            {
                throw new InvalidOperationException(path);
            }
            return value;
        }
    }
}
