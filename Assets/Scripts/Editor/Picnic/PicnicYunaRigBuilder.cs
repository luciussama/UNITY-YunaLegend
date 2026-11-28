using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

namespace YunaLegend.EditorScripts.Picnic
{
    /// <summary>
    /// Builds Unity 2D Animation assets (Prefab, AnimatorController, and 60 FPS AnimationClips)
    /// for Yuna's skeletal 2D rig in the Garden (Picnic) stage, and captures a multi-pose visual review sheet.
    /// </summary>
    public static class PicnicYunaRigBuilder
    {
        private const string RigAssetDir = "Assets/Art/Picnic/YunaRig2D";
        private const string EvidenceDir = "Evidence/PicnicYunaRig2D";

        [MenuItem("YunaLegend/Fase 5 - Um Dia no Jardim/Reconstruir Rig 2D e Clipes", false, 50)]
        public static void BuildAllAndCapture()
        {
            ConfigurePartImporters();
            BuildPrefabAndClips();
            CaptureReviewSheet();
            Debug.Log("PICNIC_YUNA_RIG2D_OK prefab, clips and 12-pose review captured");
        }

        private static void ConfigurePartImporters()
        {
            string[] partFiles = Directory.GetFiles("Assets/Art/Picnic/Resources/Picnic/YunaRig2D", "*.png");
            foreach (string path in partFiles)
            {
                string assetPath = path.Replace("\\", "/");
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void BuildPrefabAndClips()
        {
            Directory.CreateDirectory(RigAssetDir);

            // Generate 60 FPS Unity AnimationClips from PicnicYunaRigState
            AnimationClip idleClip = BuildClip("Yuna_Idle", 2.24f, true, t =>
                PicnicYunaRigState.Evaluate(true, 0f, 0f, PicnicTerrain.Wood, 0f, t, false, false, 0f));
            AnimationClip runClip = BuildClip("Yuna_Run", 1f / 2.35f, true, t =>
                PicnicYunaRigState.Evaluate(true, 210f, 0f, PicnicTerrain.Wood, 0f, t, false, false, 0f));
            AnimationClip jumpClip = BuildClip("Yuna_JumpRise", 0.35f, false, t =>
                PicnicYunaRigState.Evaluate(false, 180f, -300f, PicnicTerrain.Wood, 0f, t, false, false, 0f));
            AnimationClip fallClip = BuildClip("Yuna_Fall", 0.35f, false, t =>
                PicnicYunaRigState.Evaluate(false, 180f, 220f, PicnicTerrain.Wood, 0f, t, false, false, 0f));
            AnimationClip panicClip = BuildClip("Yuna_PanicRun", 1f / 3.35f, true, t =>
                PicnicYunaRigState.Evaluate(true, 280f, 0f, PicnicTerrain.Wood, 0f, t, false, true, 0f));
            AnimationClip grassClip = BuildClip("Yuna_GrassTrudge", 1f / 1.35f, true, t =>
                PicnicYunaRigState.Evaluate(true, 90f, 0f, PicnicTerrain.Grass, 0.8f, t, false, false, 0f));
            AnimationClip gravelClip = BuildClip("Yuna_GravelSlip", 1f / 2.15f, true, t =>
                PicnicYunaRigState.Evaluate(true, 120f, 0f, PicnicTerrain.Gravel, 0.8f, t, false, false, 0f));

            string controllerPath = RigAssetDir + "/YunaRig2D.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            var idleState = sm.AddState("Idle"); idleState.motion = idleClip;
            var runState = sm.AddState("Run"); runState.motion = runClip;
            var jumpState = sm.AddState("JumpRise"); jumpState.motion = jumpClip;
            var fallState = sm.AddState("Fall"); fallState.motion = fallClip;
            var grassState = sm.AddState("GrassTrudge"); grassState.motion = grassClip;
            var gravelState = sm.AddState("GravelSlip"); gravelState.motion = gravelClip;
            var panicState = sm.AddState("PanicRun"); panicState.motion = panicClip;
            sm.defaultState = idleState;

            // Build editable prefab with Animator attached
            var host = new GameObject("YunaRig2D_Prefab");
            var view = host.AddComponent<PicnicYunaRigView>();
            if (!view.TryBuild(12))
            {
                UnityEngine.Object.DestroyImmediate(host);
                throw new InvalidOperationException("Failed to build PicnicYunaRigView from Resources/Picnic/YunaRig2D.");
            }
            var animator = host.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            string prefabPath = RigAssetDir + "/YunaRig2D.prefab";
            PrefabUtility.SaveAsPrefabAsset(host, prefabPath);
            UnityEngine.Object.DestroyImmediate(host);
            AssetDatabase.SaveAssets();
        }

        private static AnimationClip BuildClip(string clipName, float duration, bool loop, Func<float, PicnicYunaRigPose> sampler)
        {
            var clip = new AnimationClip { name = clipName, frameRate = 60f };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            int samples = 24;
            var hipsY = new AnimationCurve();
            var torsoZ = new AnimationCurve();
            var headZ = new AnimationCurve();
            var pigBackZ = new AnimationCurve();
            var pigFarZ = new AnimationCurve();
            var legFrontZ = new AnimationCurve();
            var legBackZ = new AnimationCurve();
            var armFrontZ = new AnimationCurve();
            var armBackZ = new AnimationCurve();

            for (int i = 0; i <= samples; i++)
            {
                float t = (duration * i) / samples;
                PicnicYunaRigPose p = sampler(t);
                hipsY.AddKey(t, 0.44f + p.HipsBobY);
                torsoZ.AddKey(t, p.TorsoLeanDeg);
                headZ.AddKey(t, p.HeadTiltDeg);
                pigBackZ.AddKey(t, p.BackPigtailDeg);
                pigFarZ.AddKey(t, -p.FrontPigtailDeg * 0.85f);
                legFrontZ.AddKey(t, p.FrontLegDeg);
                legBackZ.AddKey(t, p.BackLegDeg);
                armFrontZ.AddKey(t, p.FrontArmDeg);
                armBackZ.AddKey(t, p.BackArmDeg);
            }

            const string hips = "YunaRig2D/Bone_Hips";
            const string torso = hips + "/Bone_Torso";
            const string chest = torso + "/Bone_Chest";
            const string head = chest + "/Bone_Head";

            clip.SetCurve(hips, typeof(Transform), "m_LocalPosition.y", hipsY);
            clip.SetCurve(torso, typeof(Transform), "localEulerAnglesRaw.z", torsoZ);
            clip.SetCurve(head, typeof(Transform), "localEulerAnglesRaw.z", headZ);
            clip.SetCurve(head + "/Bone_PigtailBack", typeof(Transform), "localEulerAnglesRaw.z", pigBackZ);
            clip.SetCurve(head + "/Bone_PigtailFar", typeof(Transform), "localEulerAnglesRaw.z", pigFarZ);
            clip.SetCurve(hips + "/Bone_LegFront", typeof(Transform), "localEulerAnglesRaw.z", legFrontZ);
            clip.SetCurve(hips + "/Bone_LegBack", typeof(Transform), "localEulerAnglesRaw.z", legBackZ);
            clip.SetCurve(chest + "/Bone_ArmFront", typeof(Transform), "localEulerAnglesRaw.z", armFrontZ);
            clip.SetCurve(chest + "/Bone_ArmBack", typeof(Transform), "localEulerAnglesRaw.z", armBackZ);

            string clipPath = RigAssetDir + "/" + clipName + ".anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            {
                AssetDatabase.DeleteAsset(clipPath);
            }
            AssetDatabase.CreateAsset(clip, clipPath);
            return clip;
        }

        private static void CaptureReviewSheet()
        {
            Directory.CreateDirectory(EvidenceDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("ReviewCam", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 1.65f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.86f, 0.93f, 0.84f, 1f);
            cam.transform.position = new Vector3(0f, 0.05f, -10f);

            var lightGo = new GameObject("DirLight", typeof(Light));
            lightGo.GetComponent<Light>().type = LightType.Directional;

            float runCyclePx = PicnicYunaRigState.StridePixelsPerCycle;
            var poses = new PicnicYunaRigPose[]
            {
                // Top row: 6 evenly-spaced Run cycle frames (Contact, Down, Passing, Push-off, High-Knee Recovery, Swing)
                PicnicYunaRigState.Evaluate(true, 210f, 0f, PicnicTerrain.Wood, 0f, 0f, false, false, 0f, runCyclePx * 0f / 6f),
                PicnicYunaRigState.Evaluate(true, 210f, 0f, PicnicTerrain.Wood, 0f, 0f, false, false, 0f, runCyclePx * 1f / 6f),
                PicnicYunaRigState.Evaluate(true, 210f, 0f, PicnicTerrain.Wood, 0f, 0f, false, false, 0f, runCyclePx * 2f / 6f),
                PicnicYunaRigState.Evaluate(true, 210f, 0f, PicnicTerrain.Wood, 0f, 0f, false, false, 0f, runCyclePx * 3f / 6f),
                PicnicYunaRigState.Evaluate(true, 210f, 0f, PicnicTerrain.Wood, 0f, 0f, false, false, 0f, runCyclePx * 4f / 6f),
                PicnicYunaRigState.Evaluate(true, 210f, 0f, PicnicTerrain.Wood, 0f, 0f, false, false, 0f, runCyclePx * 5f / 6f),
                // Bottom row: Idle, JumpRise, JumpApex, Fall, GravelStumble, PanicRun
                PicnicYunaRigState.Evaluate(true, 0f, 0f, PicnicTerrain.Wood, 0f, 0.25f, false, false, 0f),
                PicnicYunaRigState.Evaluate(false, 180f, -300f, PicnicTerrain.Wood, 0f, 0.2f, false, false, 0f),
                PicnicYunaRigState.Evaluate(false, 180f, 0f, PicnicTerrain.Wood, 0f, 0.3f, false, false, 0f),
                PicnicYunaRigState.Evaluate(false, 180f, 220f, PicnicTerrain.Wood, 0f, 0.4f, false, false, 0f),
                PicnicYunaRigState.Evaluate(true, 120f, 0f, PicnicTerrain.Gravel, 0.8f, 0.18f, false, false, 0f, runCyclePx * 0.62f),
                PicnicYunaRigState.Evaluate(true, 280f, 0f, PicnicTerrain.Wood, 0f, 0.08f, false, true, 0f, runCyclePx * 0.42f),
            };

            for (int i = 0; i < poses.Length; i++)
            {
                int col = i % 6;
                int row = i / 6;
                float x = (col - 2.5f) * 0.96f;
                float y = (row == 0 ? 0.32f : -1.18f);

                var host = new GameObject("YunaPose_" + i);
                host.transform.position = new Vector3(x, y, 0f);
                var view = host.AddComponent<PicnicYunaRigView>();
                view.TryBuild(10 + i * 10);
                view.ApplyPose(poses[i], 1, 1f, 0f);
            }

            int width = 1800;
            int height = 900;
            var rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            string outPath = Path.Combine(EvidenceDir, "yuna-rig2d-cycle-review.png");
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        [MenuItem("YunaLegend/Fase 5 - Um Dia no Jardim/Capturar Vídeo Caminhada 2D", false, 51)]
        public static void CaptureWalkVideoFrames()
        {
            string framesDir = Path.Combine(EvidenceDir, "video_frames");
            if (Directory.Exists(framesDir))
            {
                Directory.Delete(framesDir, true);
            }
            Directory.CreateDirectory(framesDir);

            EditorSceneManager.OpenScene("Assets/Scenes/Picnic_Gameplay.unity");
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var scenery = UnityEngine.Object.FindFirstObjectByType<PicnicSceneryController>();
            var platforms = UnityEngine.Object.FindFirstObjectByType<PicnicPlatformController>();
            var actors = UnityEngine.Object.FindFirstObjectByType<PicnicActorController>();

            var sim = new PicnicSimulation();
            sim.SkipStory();
            platforms.Build(sim.Level);
            scenery.Build(cam, sim.Level);
            actors.Build(sim);

            // Start Yuna on the wide wooden porch so she has plenty of surface to run, jump, and land cleanly
            var propX = typeof(PicnicSimulation).GetProperty("PlayerX").GetSetMethod(true);
            var propY = typeof(PicnicSimulation).GetProperty("PlayerY").GetSetMethod(true);
            var propSup = typeof(PicnicSimulation).GetProperty("SupportId").GetSetMethod(true);
            var propGnd = typeof(PicnicSimulation).GetProperty("OnGround").GetSetMethod(true);
            PicnicPlatformSpec porch = sim.Level.PlatformById["entrada"];
            propX.Invoke(sim, new object[] { 260f });
            propY.Invoke(sim, new object[] { PicnicLevelData.SurfaceY(porch, 260f) });
            propSup.Invoke(sim, new object[] { "entrada" });
            propGnd.Invoke(sim, new object[] { true });

            // Settle camera nicely on Yuna before frame 0
            for (int i = 0; i < 60; i++)
            {
                scenery.Apply(sim, 1f / 60f);
            }

            int width = 960;
            int height = 540;
            cam.aspect = width / (float)height;
            var rt = new RenderTexture(width, height, 24);
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            const int totalFrames = 135; // 4.5 seconds at 30 FPS
            const float frameDt = 1f / 30f;

            for (int f = 0; f < totalFrames; f++)
            {
                bool right = (f >= 12 && f < 95);
                bool left = (f >= 100 && f < 126);
                bool jump = (f == 60 || f == 61 || f == 62);

                float axis = right ? 1f : (left ? -1f : 0f);
                // Step physics at 60Hz (2 substeps per 30fps video frame)
                for (int s = 0; s < 2; s++)
                {
                    var input = new PicnicInput(axis, jump);
                    sim.Step(frameDt * 0.5f, input);
                    scenery.Apply(sim, frameDt * 0.5f);
                }

                platforms.Apply(sim);
                actors.Apply(sim, frameDt);

                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                cam.targetTexture = null;
                RenderTexture.active = null;

                string framePath = Path.Combine(framesDir, $"frame_{f:D4}.png");
                File.WriteAllBytes(framePath, tex.EncodeToPNG());
            }

            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);
            Debug.Log($"PICNIC_YUNA_RIG2D_VIDEO_FRAMES_OK {totalFrames} frames -> {framesDir}");
        }

        [MenuItem("YunaLegend/Fase 5 - Um Dia no Jardim/Capturar Vídeo 3 Animações", false, 52)]
        public static void CaptureThreeAnimationsVideoFrames()
        {
            string framesDir = Path.Combine(EvidenceDir, "video_3anims_frames");
            if (Directory.Exists(framesDir))
            {
                Directory.Delete(framesDir, true);
            }
            Directory.CreateDirectory(framesDir);

            EditorSceneManager.OpenScene("Assets/Scenes/Picnic_Gameplay.unity");
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var scenery = UnityEngine.Object.FindFirstObjectByType<PicnicSceneryController>();
            var platforms = UnityEngine.Object.FindFirstObjectByType<PicnicPlatformController>();
            var actors = UnityEngine.Object.FindFirstObjectByType<PicnicActorController>();

            var sim = new PicnicSimulation();
            sim.SkipStory();
            platforms.Build(sim.Level);
            scenery.Build(cam, sim.Level);
            actors.Build(sim);

            var propX = typeof(PicnicSimulation).GetProperty("PlayerX").GetSetMethod(true);
            var propY = typeof(PicnicSimulation).GetProperty("PlayerY").GetSetMethod(true);
            var propSup = typeof(PicnicSimulation).GetProperty("SupportId").GetSetMethod(true);
            var propGnd = typeof(PicnicSimulation).GetProperty("OnGround").GetSetMethod(true);
            var propDisc = typeof(PicnicSimulation).GetProperty("Discomfort").GetSetMethod(true);

            int width = 960;
            int height = 540;
            cam.aspect = width / (float)height;
            var rt = new RenderTexture(width, height, 24);
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            const float frameDt = 1f / 30f;
            int frameIdx = 0;

            void PlaceOn(string supportId, float x, float discomfort, bool panic)
            {
                PicnicPlatformSpec spec = sim.Level.PlatformById[supportId];
                propX.Invoke(sim, new object[] { x });
                propY.Invoke(sim, new object[] { PicnicLevelData.SurfaceY(spec, x) });
                propSup.Invoke(sim, new object[] { supportId });
                propGnd.Invoke(sim, new object[] { true });
                propDisc.Invoke(sim, new object[] { discomfort });
                if (panic)
                {
                    sim.Encounters.Panic = new PicnicPanicState
                    {
                        Id = "review-beetle",
                        Remaining = 8f,
                        Elapsed = 0f,
                        X = x - 140f,
                        Y = PicnicLevelData.SurfaceY(spec, x) - 64f,
                    };
                }
                else
                {
                    sim.Encounters.Panic = null;
                }
                for (int i = 0; i < 60; i++) scenery.Apply(sim, 1f / 60f);
            }

            void RecordSegment(int frames, float discomfortFloor, bool keepBeetleNear, int jumpAtFrame = -1)
            {
                for (int f = 0; f < frames; f++)
                {
                    bool doJump = jumpAtFrame >= 0 && f >= jumpAtFrame && f <= jumpAtFrame + 3;
                    for (int s = 0; s < 2; s++)
                    {
                        sim.Step(frameDt * 0.5f, new PicnicInput(1f, doJump));
                        if (discomfortFloor > 0f && sim.Discomfort < discomfortFloor)
                        {
                            propDisc.Invoke(sim, new object[] { discomfortFloor });
                        }
                        if (keepBeetleNear && sim.Encounters.Panic != null)
                        {
                            sim.Encounters.Panic.X = sim.PlayerX - 115f;
                            sim.Encounters.Panic.Y = sim.PlayerY - 58f + Mathf.Sin(sim.Time * 14f) * 8f;
                        }
                        scenery.Apply(sim, frameDt * 0.5f);
                    }
                    platforms.Apply(sim);
                    actors.Apply(sim, frameDt);

                    cam.targetTexture = rt;
                    cam.Render();
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    tex.Apply();
                    cam.targetTexture = null;
                    RenderTexture.active = null;

                    File.WriteAllBytes(Path.Combine(framesDir, $"frame_{frameIdx:D4}.png"), tex.EncodeToPNG());
                    frameIdx++;
                }
            }

            // Segment 1 (2.5s): Heavy trudging through flat tall grass (grama-da-margem) with strain face
            PlaceOn("grama-da-margem", 3260f, 0.85f, false);
            RecordSegment(75, 0.85f, false);

            // Segment 2 (2.2s): Slipping on flat loose gravel pebbles (pedrinhas-da-roda) with scared face & windmilling arms
            PlaceOn("pedrinhas-da-roda", 3980f, 0.85f, false);
            RecordSegment(65, 0.85f, false);

            // Segment 3 (2.2s): Frantic panic sprint fleeing the giant beetle on flat wooden porch
            PlaceOn("entrada", 140f, 0f, true);
            RecordSegment(65, 0f, true);

            // Segment 4 (2.0s): Normal run + waist-arched Jump & Landing on wooden porch
            PlaceOn("entrada", 220f, 0f, false);
            RecordSegment(60, 0f, false, jumpAtFrame: 14);

            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);
            Debug.Log($"PICNIC_YUNA_RIG2D_3ANIMS_VIDEO_OK {frameIdx} frames -> {framesDir}");
        }
    }
}
