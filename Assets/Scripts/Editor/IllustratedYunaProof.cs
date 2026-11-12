using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using YunaLegend.DarkRoom;
using Object = UnityEngine.Object;

namespace YunaLegend.EditorScripts
{
    /// <summary>T02.1 only: an unchanged painting on the existing physical root, in a separate scene.</summary>
    [InitializeOnLoad]
    public static class IllustratedYunaProof
    {
        public const string Folder = "Assets/Art/Illustrated25D/Yuna";
        public const string TexturePath = Folder + "/Yuna_Profile_v1.png";
        public const string ScenePath = "Assets/Scenes/Validation/IllustratedYunaProof.unity";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string ActiveKey = "IllustratedYunaProof.Active";
        // Pixel coordinates measured on the approved 1024x1536 painting. No resampling/cropping.
        const float SolidTop = 38f, SoleEdge = 1507f, SoleX = 536f;
        static int frames, lastFrame = -1;
        static string Output => ReadArgument("-proofOutput", "Evidence/25d/T02.1");
        static string gameplayHash;

        [Serializable] class Report
        {
            public string scope = "Static PlayMode sprite proof; no final animation, lighting or gameplay camera decision.";
            public string sourceHash, importedHash, gameplayBefore, gameplayAfter, shader;
            public bool sourceUnchanged, gameplayUnchanged, originalControllerParametersPreserved;
            public bool physicalSupportRayHit, rearSpriteSortedBehind, foregroundMeshOccludes;
            public float characterHeight, radius, skinWidth, pixelsPerUnit, soleWorldY, supportWorldY, contactError;
            public Vector3 controllerCenter, rootScale;
            public Vector2 pivotPixels;
            public Color clearSample, rearSample, foregroundSample;
        }

        static IllustratedYunaProof()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(ActiveKey, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += CaptureTick;
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(ActiveKey, false);
                    EditorApplication.Exit(SessionState.GetInt(ActiveKey + ".Exit", 0));
                }
            };
        }
        static string ReadArgument(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }
        static string Hash(string path)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }
        [MenuItem("YunaLegend/25D/Build isolated static Yuna proof")]
        public static void BuildScene()
        {
            gameplayHash = Hash(Gameplay);
            Directory.CreateDirectory(Folder);
            Directory.CreateDirectory("Assets/Scenes/Validation");
            File.Copy("Evidence/25d/T01/yuna-profile-v1.png", TexturePath, true);
            AssetDatabase.Refresh();
            var sourceScene = EditorSceneManager.OpenScene(Gameplay);
            var original = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var root = Object.Instantiate(original.gameObject);
            root.name = "Yuna_OriginalPhysicsRoot";
            var originalPhysics = original.GetComponent<CharacterController>();
            float height = originalPhysics.height;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.MoveGameObjectToScene(root, scene);
            SceneManager.SetActiveScene(scene);
            for (int i = root.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            // Only the proof's placement changes. Controller shape/movement settings are copied intact.
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.GetComponent<Yuna25DPlayerController>().AutoRunActive = false;
            root.GetComponent<AudioSource>().mute = true;
            EditorSceneManager.CloseScene(sourceScene, true);

            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = (SoleEdge - SolidTop) / height;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(SoleX / 1024f, (1536f - SoleEdge) / 1536f);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) throw new InvalidOperationException("URP sprite shader missing.");
            var paint = MaterialAsset("Painting_Unlit", shader, Color.white);
            var body = new GameObject("Body_Visual");
            body.transform.SetParent(root.transform, false);
            var renderer = body.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
            renderer.sharedMaterial = paint;
            renderer.sortingOrder = 0;

            var solidShader = Shader.Find("Universal Render Pipeline/Unlit");
            var floorMat = MaterialAsset("TechnicalSupport", solidShader, new Color(.22f, .29f, .39f));
            var support = GameObject.CreatePrimitive(PrimitiveType.Cube);
            support.name = "TechnicalSupport_NotPlatformArt";
            support.transform.position = new Vector3(0, -.16f, .3f);
            support.transform.localScale = new Vector3(3.4f, .32f, 1.2f);
            support.GetComponent<Renderer>().sharedMaterial = floorMat;

            var camera = new GameObject("ProofCamera_Orthographic_NotFinalChoice", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(-.04f, .64f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 1.02f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.075f, .10f, .16f);
            camera.nearClipPlane = .1f; camera.farClipPlane = 30f;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;

            var foreground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foreground.name = "DiagnosticForeground";
            foreground.transform.position = new Vector3(0, .7f, -.25f);
            foreground.transform.localScale = new Vector3(.25f, .6f, .1f);
            foreground.GetComponent<Renderer>().sharedMaterial = MaterialAsset("DiagnosticForeground", solidShader, new Color(.15f, .65f, .75f));
            Object.DestroyImmediate(foreground.GetComponent<Collider>());
            foreground.SetActive(false);

            // A nearer sprite must still sort behind the character with its lower order.
            var rear = new GameObject("DiagnosticRear", typeof(SpriteRenderer));
            rear.transform.position = new Vector3(0, .7f, -.1f);
            rear.transform.localScale = new Vector3(.25f, .6f, 1f);
            var rearRenderer = rear.GetComponent<SpriteRenderer>();
            var whitePath = Folder + "/DiagnosticWhite.asset";
            if (!File.Exists(whitePath))
            {
                var white = new Texture2D(2, 2); white.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white }); white.Apply();
                AssetDatabase.CreateAsset(white, whitePath);
            }
            var spritePath = Folder + "/DiagnosticWhiteSprite.asset";
            if (!File.Exists(spritePath))
                AssetDatabase.CreateAsset(Sprite.Create(AssetDatabase.LoadAssetAtPath<Texture2D>(whitePath), new Rect(0, 0, 2, 2), Vector2.one * .5f, 2f), spritePath);
            rearRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            rearRenderer.sharedMaterial = paint;
            rearRenderer.color = Color.magenta;
            rearRenderer.sortingOrder = -10;
            rear.SetActive(false);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            if (Hash(Gameplay) != gameplayHash) throw new InvalidOperationException("Gameplay scene changed.");
            Debug.Log("ILLUSTRATED_YUNA_SCENE_SAVED " + ScenePath);
        }
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            BuildScene();
            SessionState.SetString(ActiveKey + ".GameplayHash", gameplayHash);
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetInt(ActiveKey + ".Exit", 0);
            EditorApplication.EnterPlaymode();
        }
        static Material MaterialAsset(string name, Shader shader, Color color)
        {
            string path = Folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            material.shader = shader;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
        }
        static Texture2D Capture(Camera camera, string name)
        {
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, 1280, 720);
            File.WriteAllBytes(Path.Combine(Output, name + ".png"), image.EncodeToPNG());
            return image;
        }
        static Color Sample(Camera camera, Texture2D image)
        {
            var p = camera.WorldToViewportPoint(new Vector3(0, .7f, 0));
            return image.GetPixel(Mathf.RoundToInt(p.x * (image.width - 1)), Mathf.RoundToInt(p.y * (image.height - 1)));
        }
        static void CaptureTick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            if (++frames < 60) return;
            EditorApplication.update -= CaptureTick;
            try
            {
                var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                var cc = player.GetComponent<CharacterController>();
                var sr = player.GetComponentInChildren<SpriteRenderer>();
                var support = GameObject.Find("TechnicalSupport_NotPlatformArt").GetComponent<Collider>();
                var camera = Camera.main;
                camera.aspect = 16f / 9f;
                var report = new Report
                {
                    sourceHash = Hash("Evidence/25d/T01/yuna-profile-v1.png"), importedHash = Hash(TexturePath),
                    gameplayBefore = SessionState.GetString(ActiveKey + ".GameplayHash", ""), gameplayAfter = Hash(Gameplay),
                    shader = sr.sharedMaterial.shader.name,
                    characterHeight = cc.height, radius = cc.radius, skinWidth = cc.skinWidth,
                    controllerCenter = cc.center, rootScale = player.transform.localScale,
                    pixelsPerUnit = sr.sprite.pixelsPerUnit, pivotPixels = sr.sprite.pivot,
                    soleWorldY = player.transform.position.y, supportWorldY = support.bounds.max.y
                };
                report.contactError = Mathf.Abs(report.soleWorldY - report.supportWorldY);
                report.sourceUnchanged = report.sourceHash == report.importedHash;
                report.gameplayUnchanged = report.gameplayBefore == report.gameplayAfter;
                report.originalControllerParametersPreserved = cc.height == 1.4f && cc.radius == .35f && cc.skinWidth == .08f && player.RunSpeed == 5.2f && player.JumpHeight == 2.6f;
                Physics.SyncTransforms();
                report.physicalSupportRayHit = Physics.Raycast(new Vector3(0, .02f, 0), Vector3.down, out var hit, .1f) && hit.collider == support;
                var clear = Capture(camera, "01-yuna-unity-dark"); report.clearSample = Sample(camera, clear); Object.DestroyImmediate(clear);
                camera.backgroundColor = new Color(.78f, .80f, .83f);
                Object.DestroyImmediate(Capture(camera, "02-yuna-unity-light"));
                camera.backgroundColor = new Color(.075f, .10f, .16f);
                GameObject rear = null, foreground = null;
                foreach (var obj in player.gameObject.scene.GetRootGameObjects())
                { if (obj.name == "DiagnosticRear") rear = obj; if (obj.name == "DiagnosticForeground") foreground = obj; }
                rear.SetActive(true);
                var rearImage = Capture(camera, "03-sorting-rear"); report.rearSample = Sample(camera, rearImage); Object.DestroyImmediate(rearImage);
                foreground.SetActive(true);
                var frontImage = Capture(camera, "04-occlusion-foreground"); report.foregroundSample = Sample(camera, frontImage); Object.DestroyImmediate(frontImage);
                report.rearSpriteSortedBehind = Vector4.Distance(report.clearSample, report.rearSample) < .025f;
                report.foregroundMeshOccludes = Vector4.Distance(report.clearSample, report.foregroundSample) > .15f;
                File.WriteAllText(Path.Combine(Output, "unity-proof.json"), JsonUtility.ToJson(report, true));
                if (!report.sourceUnchanged || !report.gameplayUnchanged || !report.originalControllerParametersPreserved || report.contactError > .002f || !report.physicalSupportRayHit || !report.rearSpriteSortedBehind || !report.foregroundMeshOccludes)
                    throw new InvalidOperationException("Static proof contract failed; see unity-proof.json.");
                Debug.Log("ILLUSTRATED_YUNA_PROOF_OK static alpha/scale/contact/sorting/occlusion; not animation approval.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex); File.WriteAllText(Path.Combine(Output, "failure.txt"), ex.ToString());
                SessionState.SetInt(ActiveKey + ".Exit", 1);
            }
            EditorApplication.ExitPlaymode();
        }
    }
}
