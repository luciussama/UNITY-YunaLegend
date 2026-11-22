using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using YunaLegend.Arena.Core;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;
using YunaLegend.Editor.Arena;
using YunaLegend.Editor.ToyRoom;

namespace YunaLegend.Editor.BearChase
{
    /// <summary>
    /// Orquestrador de construcao da cena BearChase_Gameplay.unity:
    /// conecta cena de entrada ilustrada 2D, corredor curvo/ondulado continuo,
    /// iluminacao atmosferica real, brinquedos arremessaveis pelo urso, Yuna, Urso, Nanda, audio e HUD.
    /// </summary>
    public static class BearChaseSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/BearChase_Gameplay.unity";
        private const string BearObjPath = "Assets/Art/Arena/Models/bear_hunyuan.obj";
        private const string BearTexPath = "Assets/Art/Arena/Models/T_Bear_Albedo.png";

        [MenuItem("YunaLegend/Fase 4 - Bear Chase/Jogar (Play Direto)", false, 10)]
        public static void PlayBearChaseDirectly()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.isPlaying = true;
        }

        [MenuItem("YunaLegend/Fase 4 - Bear Chase/Abrir Cena", false, 20)]
        public static void OpenBearChaseScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[BearChaseSceneBuilder] Saia do Play Mode antes de abrir a cena.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.playModeStartScene = null;
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        [MenuItem("YunaLegend/Fase 4 - Bear Chase/Reconstruir Cena", false, 30)]
        public static void BuildBearChaseScene()
        {
            EditorSceneManager.playModeStartScene = null;
            BearChaseAssetProcessor.EnsureProcessedAssets();
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var matFactory = new BearChaseMaterialFactory();

            Light sunLight = CreateMainSunLight();
            CreatePostProcessVolume(matFactory);
            Camera cam = CreateMainCamera(out BearChaseCameraController camCtrl);

            var chaseOrigin = new Vector3(0f, 0f, 40f);
            GameObject corridorRoot = BearChaseTrackMeshBuilder.BuildCorridor(
                matFactory,
                chaseOrigin,
                out BearChaseLightingController.WindowLightAnchor[] windowAnchors);
            corridorRoot.SetActive(false);

            BearChaseLightingController lightingCtrl = CreateLightingController(sunLight, windowAnchors);
            lightingCtrl.ApplyAtmosphere();

            BearChaseEntranceController entranceCtrl = CreateEntranceController();
            BearChaseObstacleController obstacleCtrl = BearChaseObstacleFactory.BuildObstacleController(matFactory);

            BearChasePlayerController playerCtrl = CreatePlayer(matFactory);
            BearChaseBearAnimator bearCtrl = CreateRunawayBear(matFactory);
            BearChaseNandaController nandaCtrl = CreateNanda(playerCtrl.transform, matFactory);

            playerCtrl.gameObject.SetActive(false);
            bearCtrl.gameObject.SetActive(false);
            nandaCtrl.gameObject.SetActive(false);

            BearChaseHudController hudCtrl = CreateHud();
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            BearChaseAudioController audioCtrl = CreateAudioController();

            var inputGo = new GameObject("BearChase_Input");
            var inputAdapter = inputGo.AddComponent<BearChaseInputAdapter>();

            var sceneryGo = new GameObject("BearChase_Scenery");
            var sceneryCtrl = sceneryGo.AddComponent<BearChaseSceneryController>();
            sceneryCtrl.Configure(corridorRoot, playerCtrl.gameObject, bearCtrl.gameObject, nandaCtrl.gameObject);

            var bootstrapGo = new GameObject("BearChase_Bootstrap");
            var bootstrap = bootstrapGo.AddComponent<BearChaseStageBootstrap>();
            bootstrap.Configure(
                inputAdapter,
                entranceCtrl,
                sceneryCtrl,
                lightingCtrl,
                obstacleCtrl,
                camCtrl,
                playerCtrl,
                bearCtrl,
                nandaCtrl,
                hudCtrl,
                audioCtrl);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterSceneInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log($"[BearChaseSceneBuilder] Cena da Fase 4 salva em {ScenePath}");
        }

        private static Light CreateMainSunLight()
        {
            var sunGo = new GameObject("MorningSun_DirectionalLight");
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.93f, 0.80f);
            sun.intensity = 0.48f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.42f;
            sunGo.transform.rotation = Quaternion.Euler(48f, -24f, 0f);
            return sun;
        }

        private static void CreatePostProcessVolume(BearChaseMaterialFactory matFactory)
        {
            var volGo = new GameObject("BearChase_PostProcessVolume");
            Volume volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = matFactory.GetOrCreateVolumeProfile();
        }

        private static Camera CreateMainCamera(out BearChaseCameraController camCtrl)
        {
            var camGo = new GameObject("BearChase_MainCamera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.95f, 0.89f, 0.76f);
            cam.fieldOfView = 48f;
            cam.nearClipPlane = 0.25f;
            cam.farClipPlane = 85f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camCtrl = camGo.AddComponent<BearChaseCameraController>();
            camCtrl.Configure(cam);
            return cam;
        }

        private static BearChaseLightingController CreateLightingController(
            Light sunLight,
            BearChaseLightingController.WindowLightAnchor[] windowAnchors)
        {
            var root = new GameObject("BearChase_Lighting");
            var lighting = root.AddComponent<BearChaseLightingController>();

            var spotPool = new Light[3];
            for (int i = 0; i < spotPool.Length; i++)
            {
                var spotGo = new GameObject($"WindowSpotPool_{i}");
                spotGo.transform.SetParent(root.transform, false);
                Light spot = spotGo.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.color = new Color(1.0f, 0.89f, 0.64f);
                spot.range = 19f;
                spot.spotAngle = 86f;
                spot.innerSpotAngle = 44f;
                spot.intensity = 13.5f;
                spot.shadows = LightShadows.None;
                spot.renderMode = LightRenderMode.ForcePixel;
                spot.enabled = false;
                spotPool[i] = spot;
            }

            lighting.Configure(sunLight, windowAnchors, spotPool);
            return lighting;
        }

        private static BearChaseEntranceController CreateEntranceController()
        {
            var go = new GameObject("BearChase_Entrance");
            var ctrl = go.AddComponent<BearChaseEntranceController>();
            return ctrl;
        }

        private static BearChasePlayerController CreatePlayer(BearChaseMaterialFactory matFactory)
        {
            var playerGo = new GameObject("Yuna_BearChase");
            Transform bodyRoot = null;
            Transform ponytail = null;

            ToyRoomVisualFactory.BeginMaterialScope("Assets/Art/BearChase/Materials", "M_BearChase_Yuna_");
            try
            {
                ToyRoomVisualFactory.BuildYunaCharacter3D(
                    playerGo.transform,
                    out bodyRoot,
                    out _,
                    out _,
                    out _,
                    out _,
                    out ponytail);
            }
            finally
            {
                ToyRoomVisualFactory.EndMaterialScope();
            }

            BearChaseRigBones rig = ExtractYunaRigBones(bodyRoot);
            Transform shadowTransform = CreateContactShadow(playerGo.transform, "Yuna_ContactShadow", matFactory, 0.95f);
            ParticleSystem dust = CreateFootstepDust(playerGo.transform, matFactory);

            var ctrl = playerGo.AddComponent<BearChasePlayerController>();
            ctrl.Configure(bodyRoot, ponytail, rig, shadowTransform, dust);
            return ctrl;
        }

        private static BearChaseBearAnimator CreateRunawayBear(BearChaseMaterialFactory matFactory)
        {
            Material bearMat = matFactory.GetOrCreatePainterly(
                "RunawayBear",
                Color.white,
                BearTexPath,
                Vector2.one,
                outlineWidth: 0.0038f,
                ambientScale: 0.82f);

            GameObject bearGo = ArenaEnemyRigBuilder.BuildRiggedEnemy(
                "Bear_Runaway",
                ArenaEnemyId.Bear,
                null,
                BearObjPath,
                bearMat,
                1.12f);

            Transform head = FindDeepChild(bearGo.transform, "Head");
            Transform eyelidsRoot = null;
            Transform wideEyesRoot = null;
            if (head != null)
            {
                Material darkMat = matFactory.GetOrCreatePainterly("BearEyeDark", new Color(0.14f, 0.09f, 0.07f), null, Vector2.one, outlineWidth: 0f);
                eyelidsRoot = new GameObject("SleepingEyelids").transform;
                eyelidsRoot.SetParent(head, false);
                CreateVisualBox("LidL", eyelidsRoot, new Vector3(-0.07f, 0.12f, 0.18f), new Vector3(0.055f, 0.014f, 0.02f), darkMat, new Vector3(0f, 0f, 8f));
                CreateVisualBox("LidR", eyelidsRoot, new Vector3(0.07f, 0.12f, 0.18f), new Vector3(0.055f, 0.014f, 0.02f), darkMat, new Vector3(0f, 0f, -8f));
                eyelidsRoot.gameObject.SetActive(false);
            }

            BearChaseRigBones bearRig = ExtractBearRigBones(bearGo.transform);
            Transform handAnchor = null;
            BearChaseBearAnimator.ThrowPreviewEntry[] previews = System.Array.Empty<BearChaseBearAnimator.ThrowPreviewEntry>();
            if (bearRig.HandR != null)
            {
                var anchorGo = new GameObject("ThrowToyAnchor");
                anchorGo.transform.SetParent(bearRig.HandR, false);
                anchorGo.transform.localPosition = new Vector3(0.04f, -0.02f, 0.08f);
                handAnchor = anchorGo.transform;
                previews = BearChaseObstacleFactory.BuildBearHandPreviews(handAnchor, matFactory);
            }

            Transform shadowTransform = CreateContactShadow(bearGo.transform, "Bear_ContactShadow", matFactory, 1.18f);

            var animator = bearGo.AddComponent<BearChaseBearAnimator>();
            var animRig = bearGo.GetComponent<YunaLegend.Arena.Runtime.ArenaEnemyRigAnimator>();
            if (animRig != null)
            {
                Object.DestroyImmediate(animRig);
            }

            animator.Configure(bearRig, eyelidsRoot, wideEyesRoot, handAnchor, previews, shadowTransform);
            return animator;
        }

        private static BearChaseNandaController CreateNanda(Transform yunaTransform, BearChaseMaterialFactory matFactory)
        {
            var nandaGo = new GameObject("Nanda_BlueGuide");
            nandaGo.transform.position = yunaTransform.position + new Vector3(-0.78f, 1.18f, 0.16f);

            ToyRoomVisualFactory.BeginMaterialScope("Assets/Art/BearChase/Materials", "M_BearChase_Nanda_");
            try
            {
                ToyRoomVisualFactory.BuildNandaFairy3D(nandaGo.transform);
            }
            finally
            {
                ToyRoomVisualFactory.EndMaterialScope();
            }

            Transform fairyBody = nandaGo.transform.Find("Nanda_FairyBody");
            if (fairyBody != null)
            {
                fairyBody.localScale = Vector3.one * 0.26f;
            }

            var lightGo = new GameObject("Nanda_AuraLight");
            lightGo.transform.SetParent(nandaGo.transform, false);
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.22f, 0.52f, 1.0f);
            light.range = 2.65f;
            light.intensity = 2.2f;
            light.renderMode = LightRenderMode.ForcePixel;

            GameObject bodyHalo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bodyHalo.name = "Nanda_BodyCyanHalo";
            bodyHalo.transform.SetParent(nandaGo.transform, false);
            bodyHalo.transform.localPosition = new Vector3(0f, 0.03f, -0.06f);
            bodyHalo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            Object.DestroyImmediate(bodyHalo.GetComponent<Collider>());
            Renderer haloRend = bodyHalo.GetComponent<Renderer>();
            haloRend.sharedMaterial = matFactory.GetOrCreateRadialGlow(
                "NandaBodyHalo",
                new Color(0.28f, 0.64f, 1.0f, 0.56f),
                BearChaseAssetProcessor.SoftGlowTexPath);
            haloRend.shadowCastingMode = ShadowCastingMode.Off;

            GameObject blueFloorGlow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            blueFloorGlow.name = "Nanda_FloorBlueGlow";
            blueFloorGlow.transform.SetParent(nandaGo.transform, false);
            blueFloorGlow.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
            Object.DestroyImmediate(blueFloorGlow.GetComponent<Collider>());
            Renderer glowRend = blueFloorGlow.GetComponent<Renderer>();
            glowRend.sharedMaterial = matFactory.GetOrCreateRadialGlow(
                "NandaFloorGlow",
                new Color(0.20f, 0.52f, 1.0f, 0.28f),
                BearChaseAssetProcessor.SoftGlowTexPath);
            glowRend.shadowCastingMode = ShadowCastingMode.Off;

            ParticleSystem trail = CreateNandaSparkleTrail(nandaGo.transform, matFactory);
            var ctrl = nandaGo.AddComponent<BearChaseNandaController>();
            ctrl.Configure(yunaTransform, light, trail, blueFloorGlow.transform, bodyHalo.transform);
            return ctrl;
        }

        private static BearChaseHudController CreateHud()
        {
            var hudGo = new GameObject("BearChase_HUD");
            var hud = hudGo.AddComponent<BearChaseHudController>();
            Font caveatFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Caveat-Variable.ttf");
            if (caveatFont != null)
            {
                hud.ConfigureFont(caveatFont);
            }

            return hud;
        }

        private static BearChaseAudioController CreateAudioController()
        {
            var audioGo = new GameObject("BearChase_Audio");
            var audioCtrl = audioGo.AddComponent<BearChaseAudioController>();

            AudioClip chaseClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BearChase/Chase.mp3");
            var girlSteps = new AudioClip[6];
            var bearSteps = new AudioClip[6];
            for (int i = 0; i < 6; i++)
            {
                girlSteps[i] = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/BearChase/footsteps/girl-0{i + 1}.wav");
                bearSteps[i] = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/BearChase/footsteps/bear-0{i + 1}.wav");
            }

            AudioClip wakeClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Arena/bear-attack.mp3");
            AudioClip throwClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/jump_airy.wav");
            AudioClip landClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/land_wood.wav");
            AudioClip hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/comic_fall_thud.wav");
            AudioClip defeatClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/game_over_fall.wav");
            AudioClip victoryClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/fairy_twinkle.wav");

            audioCtrl.Configure(
                chaseClip,
                girlSteps,
                bearSteps,
                wakeClip,
                throwClip,
                landClip,
                hitClip,
                defeatClip,
                victoryClip);
            return audioCtrl;
        }

        private static Transform CreateContactShadow(
            Transform parent,
            string name,
            BearChaseMaterialFactory matFactory,
            float size)
        {
            Material shadowMat = matFactory.GetOrCreateTransparentUnlit(
                "ActorContactShadow",
                new Color(0.06f, 0.03f, 0.02f, 0.65f),
                BearChaseAssetProcessor.ContactShadowTexPath);

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localScale = new Vector3(size, size, 1f);
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            Renderer rend = quad.GetComponent<Renderer>();
            rend.sharedMaterial = shadowMat;
            rend.shadowCastingMode = ShadowCastingMode.Off;
            return quad.transform;
        }

        private static ParticleSystem CreateFootstepDust(Transform parent, BearChaseMaterialFactory matFactory)
        {
            var go = new GameObject("Yuna_FootstepDust");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.08f, -0.15f);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = true;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.42f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.30f);
            main.startColor = new Color(0.95f, 0.90f, 0.80f, 0.48f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 16f;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = matFactory.GetOrCreateParticleUnlit(
                "FootDustParticle",
                new Color(0.95f, 0.90f, 0.80f, 0.55f),
                BearChaseAssetProcessor.SoftGlowTexPath,
                additive: false);
            return ps;
        }

        private static ParticleSystem CreateNandaSparkleTrail(Transform parent, BearChaseMaterialFactory matFactory)
        {
            var go = new GameObject("Nanda_CyanTrail");
            go.transform.SetParent(parent, false);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = true;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.32f, 0.58f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.50f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.20f);
            main.startColor = new Color(0.42f, 0.76f, 1.0f, 0.88f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 28f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.14f;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = matFactory.GetOrCreateParticleUnlit(
                "NandaSparkleParticle",
                new Color(0.45f, 0.92f, 1.0f, 0.95f),
                BearChaseAssetProcessor.SoftGlowTexPath,
                additive: true);
            return ps;
        }

        private static BearChaseRigBones ExtractYunaRigBones(Transform bodyRoot)
        {
            if (bodyRoot == null)
            {
                return default;
            }

            Transform rigRoot = bodyRoot.Find("Yuna_RigRoot");
            if (rigRoot == null)
            {
                return default;
            }

            return ExtractStandardBones(rigRoot);
        }

        private static BearChaseRigBones ExtractBearRigBones(Transform bearRoot)
        {
            if (bearRoot == null)
            {
                return default;
            }

            Transform rigRoot = bearRoot.Find("RigScaleRoot");
            if (rigRoot == null)
            {
                return default;
            }

            return ExtractStandardBones(rigRoot);
        }

        private static BearChaseRigBones ExtractStandardBones(Transform rigRoot)
        {
            Transform hips = rigRoot.Find("Hips");
            Transform spine = hips != null ? hips.Find("Spine") : null;
            Transform chest = spine != null ? spine.Find("Chest") : null;
            Transform neck = chest != null ? chest.Find("Neck") : null;
            Transform head = (neck != null ? neck.Find("Head") : null) ?? (chest != null ? chest.Find("Head") : null);
            Transform upperArmL = chest != null ? chest.Find("UpperArm_L") : null;
            Transform forearmL = upperArmL != null ? upperArmL.Find("Forearm_L") : null;
            Transform upperArmR = chest != null ? chest.Find("UpperArm_R") : null;
            Transform forearmR = upperArmR != null ? upperArmR.Find("Forearm_R") : null;
            Transform thighL = hips != null ? hips.Find("Thigh_L") : null;
            Transform shinL = thighL != null ? thighL.Find("Shin_L") : null;
            Transform thighR = hips != null ? hips.Find("Thigh_R") : null;
            Transform shinR = thighR != null ? thighR.Find("Shin_R") : null;

            return new BearChaseRigBones
            {
                Hips = hips,
                Spine = spine,
                Chest = chest,
                Neck = neck,
                Head = head,
                UpperArmL = upperArmL,
                ForearmL = forearmL,
                HandL = forearmL != null ? forearmL.Find("Hand_L") : null,
                UpperArmR = upperArmR,
                ForearmR = forearmR,
                HandR = forearmR != null ? forearmR.Find("Hand_R") : null,
                ThighL = thighL,
                ShinL = shinL,
                FootL = shinL != null ? shinL.Find("Foot_L") : null,
                ThighR = thighR,
                ShinR = shinR,
                FootR = shinR != null ? shinR.Find("Foot_R") : null
            };
        }

        private static void CreateVisualBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3 euler)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.transform.localEulerAngles = euler;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void RegisterSceneInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == ScenePath)
                {
                    if (!scenes[i].enabled)
                    {
                        scenes[i].enabled = true;
                        EditorBuildSettings.scenes = scenes.ToArray();
                    }

                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
