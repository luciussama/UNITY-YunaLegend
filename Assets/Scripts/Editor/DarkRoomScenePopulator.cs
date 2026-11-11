using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomScenePopulator
    {
        [MenuItem("YunaLegend/Construir Cena Quarto Escuro")]
        public static void PopulateDarkRoomScene()
        {
            var scenePath = "Assets/Scenes/DarkRoom_Gameplay.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            DarkRoomRenderPipelineSetup.Configure();
            DarkRoomArtAssetBuilder.GenerateAllArtAssets();
            DarkRoomAudioAssetBuilder.GenerateAllSfx();
            DarkRoomVfxBuilder.GenerateAllVfx();

            // 1. Configurar Camera 2.5D
            var camGo = GameObject.Find("Main Camera (2.5D SideScroll)");
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera (2.5D SideScroll)");
                camGo.AddComponent<Camera>();
            }
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.fieldOfView = 45f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.10f); // Azul-indigo profundo noturno
            var sideCam = camGo.GetComponent<SideScroll25DCamera>();
            if (sideCam == null) sideCam = camGo.AddComponent<SideScroll25DCamera>();

            // Adicionar UniversalAdditionalCameraData para URP
            var camData = camGo.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null) camData = camGo.AddComponent<UniversalAdditionalCameraData>();

            // Adicionar AudioListener obrigatorio da Unity para ouvir os sons do jogo
            var listener = camGo.GetComponent<AudioListener>();
            if (listener == null) listener = camGo.AddComponent<AudioListener>();

            // 2. Penumbra Global do Quarto + Luz de luar tênue de preenchimento
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.06f);

            var lightGo = GameObject.Find("Moonlight Directional Light");
            if (lightGo == null) lightGo = new GameObject("Moonlight Directional Light");
            var dirLight = lightGo.GetComponent<Light>();
            if (dirLight == null) dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.38f, 0.46f, 0.70f);
            dirLight.intensity = 0.10f;
            lightGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

            // 2c. Foco de Luar Real Difuso na Janela 1 (unica fonte de luz ambiental que incide na sala e se espalha pelos objetos)
            var moonSpotGo = GameObject.Find("Window_Moonlight_Spot");
            if (moonSpotGo == null) moonSpotGo = new GameObject("Window_Moonlight_Spot");
            moonSpotGo.transform.position = new Vector3(29.5f, 2.2f, 6.1f);
            moonSpotGo.transform.LookAt(new Vector3(35.0f, -9.1f, 2.0f));

            var spotLight = moonSpotGo.GetComponent<Light>();
            if (spotLight == null) spotLight = moonSpotGo.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.color = new Color(0.66f, 0.82f, 1.0f);
            spotLight.intensity = 3.60f;
            spotLight.range = 22f;
            spotLight.spotAngle = 72f;
            spotLight.innerSpotAngle = 22f; // Difusao gradual e suave nas bordas do feixe
            spotLight.shadows = LightShadows.Soft; // Sombras suaves reais da engine
            spotLight.cookie = null;

            // 2d. Foco de Luar Suave na 2a Janela Canonica (NIGHT_WINDOWS[1] = 1950 -> X=73.75m)
            var moonSpot2Go = GameObject.Find("Window_Moonlight_Spot_2");
            if (moonSpot2Go == null) moonSpot2Go = new GameObject("Window_Moonlight_Spot_2");
            moonSpot2Go.transform.position = new Vector3(73.75f, 2.2f, 6.1f);
            moonSpot2Go.transform.LookAt(new Vector3(79.25f, -9.1f, 2.0f));

            var spotLight2 = moonSpot2Go.GetComponent<Light>();
            if (spotLight2 == null) spotLight2 = moonSpot2Go.AddComponent<Light>();
            spotLight2.type = LightType.Spot;
            spotLight2.color = new Color(0.66f, 0.82f, 1.0f);
            spotLight2.intensity = 3.60f;
            spotLight2.range = 22f;
            spotLight2.spotAngle = 72f;
            spotLight2.innerSpotAngle = 22f;
            spotLight2.shadows = LightShadows.Soft;
            spotLight2.cookie = null;

            var oldBeam = GameObject.Find("Window_Moonbeam_DiffuseShaft");
            if (oldBeam != null) Object.DestroyImmediate(oldBeam);

            // 2b. Persistir a cadeia camera/Volume e o mesmo perfil utilizado na geracao de arte.
            DarkRoomPostProcessingSetup.ConfigureScene(cam);

            // 3. Carregar Materiais e Gerar Blockout das 38 plataformas + piso + cenografia
            var blockoutRoot = GameObject.Find("DarkRoom_Blockout_Root");
            if (blockoutRoot == null) blockoutRoot = new GameObject("DarkRoom_Blockout_Root");
            var builder = blockoutRoot.GetComponent<DarkRoomBlockoutBuilder>();
            if (builder == null) builder = blockoutRoot.AddComponent<DarkRoomBlockoutBuilder>();

            // Injetar materiais estilizados
            builder.ConfigureMaterials(
                LoadMat("M_DarkOak_Floor"),
                LoadMat("M_Background_Wall"),
                LoadMat("M_GiantWardrobe"),
                LoadMat("M_Moonlit_Window"),
                LoadMat("M_TeddyPlush"),
                LoadMat("M_ToyBlock_Red"),
                LoadMat("M_ToyBlock_Yellow"),
                LoadMat("M_ToyBlock_Blue"),
                LoadMat("M_ToyBlock_Green"),
                LoadMat("M_ToyBlock_ABC"),
                LoadMat("M_CircusDrum"),
                LoadMat("M_RoyalVelvet_Purple"),
                LoadMat("M_MusicBox_Rosewood"),
                LoadMat("M_Brass_Gold"),
                LoadMat("M_Dresser_Oak"),
                LoadMat("M_Book_Blue"),
                LoadMat("M_Book_Burgundy"),
                LoadMat("M_Book_Green"),
                LoadMat("M_Book_GoldPages"),
                LoadMat("M_Castle_Blocks"),
                LoadMat("M_Castle_TurretBlue"),
                LoadMat("M_TrainTrack_Wood"),
                LoadMat("M_FalseDoor_Poster"),
                LoadMat("M_DreamPortal_Gold"),
                LoadMat("M_Moonbeam_Shaft"),
                LoadMat("M_AmbientDust_Particle"),
                LoadMat("M_Moonlight_Floor_Projection"),
                LoadMat("M_Wall_Baseboard"),
                LoadMat("M_Wall_Bunting"),
                LoadMat("M_Floor_Rug_Mandala"),
                LoadMat("M_Floor_Rug_Striped"),
                LoadMat("M_FloorProp_Red"),
                LoadMat("M_FloorProp_Yellow"),
                LoadMat("M_FloorProp_Blue"),
                LoadMat("M_FloorProp_Green"),
                LoadMat("M_FloorProp_Cream"),
                LoadMat("M_FloorProp_Burgundy"),
                LoadMat("M_FloorProp_Plush"),
                new[] {
                    LoadMat("M_Bear_Fur"),
                    LoadMat("M_Bear_Muzzle"),
                    LoadMat("M_Bear_Wood"),
                    LoadMat("M_Bear_Velvet"),
                    LoadMat("M_Bear_Button"),
                    LoadMat("M_Bear_Thread"),
                    LoadMat("M_Bear_Nose"),
                    LoadMat("M_Book_Leather_Brown"),
                    LoadMat("M_Book_Leather_Burgundy"),
                    LoadMat("M_Book_Leather_Green"),
                    LoadMat("M_Book_Leather_Dark"),
                    LoadMat("M_Book_Pages"),
                    LoadMat("M_Brass_Gold"),
                    LoadMat("M_Bear_Velvet"),
                    LoadMat("M_Book_Leather_Ochre"),
                    LoadMat("M_Book_Spines"),
                    LoadMat("M_Book_Painted_Cover"),
                    LoadMat("M_Furniture_Wood"), LoadMat("M_Furniture_Shade"),
                    LoadMat("M_Furniture_Trim"), LoadMat("M_Furniture_Brass")
                }
            );

            builder.ConfigureWallDecorations(
                LoadMat("M_WallDecor_Wood"), LoadMat("M_WallDecor_Lilac"),
                LoadMat("M_WallDecor_Glass"), new[] {
                    LoadMat("M_WallDrawing_Sun"), LoadMat("M_WallDrawing_Castle"),
                    LoadMat("M_WallDrawing_Rainbow"), LoadMat("M_WallDrawing_Cat"),
                    LoadMat("M_WallCostume_Hat"), LoadMat("M_WallCostume_Cape"),
                    LoadMat("M_WallCostume_Scarf") });
            builder.ConfigureWallSail(LoadMat("M_WallDecor_Sail"));
            builder.ConfigureFurnitureCrack(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/wood_furniture_crack.wav"));
            builder.BuildBlockout();

            // 4. Instanciar Yuna (jogadora 2.5D) com visual estilizado e CharacterController
            var yunaGo = GameObject.Find("Yuna_Player");
            if (yunaGo == null)
            {
                yunaGo = new GameObject("Yuna_Player");
                yunaGo.transform.position = DarkRoomStartLayout.Position; // Largada no piso, antes do urso

                var cc = yunaGo.AddComponent<CharacterController>();
                cc.height = 1.4f;
                cc.radius = 0.35f;
                cc.center = new Vector3(0f, 0.7f, 0f);

                // Visual estilizado provisorio (vestidinho creme com laço carmim)
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body_Visual";
                body.transform.SetParent(yunaGo.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
                body.transform.localScale = new Vector3(0.6f, 0.7f, 0.6f);
                var bodyCol = body.GetComponent<Collider>();
                if (bodyCol != null) Object.DestroyImmediate(bodyCol);
                ApplyMat(body, LoadMat("M_Yuna_Dress"));

                // Lacinho carmim no topo
                var bow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bow.name = "Yuna_Ribbon";
                bow.transform.SetParent(body.transform, false);
                bow.transform.localPosition = new Vector3(0f, 0.78f, -0.05f);
                bow.transform.localScale = new Vector3(0.40f, 0.18f, 0.22f);
                var bowCol = bow.GetComponent<Collider>();
                if (bowCol != null) Object.DestroyImmediate(bowCol);
                ApplyMat(bow, LoadMat("M_Yuna_Ribbon"));

                // Luz halo muito tenue da Yuna (contorno luminoso proximo)
                var haloGo = new GameObject("Yuna_FillLight");
                haloGo.transform.SetParent(yunaGo.transform, false);
                haloGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                var fillLight = haloGo.AddComponent<Light>();
                fillLight.type = LightType.Point;
                fillLight.color = new Color(1.0f, 0.86f, 0.74f); // Tom pessego suave
                fillLight.intensity = 0.55f;
                fillLight.range = 2.6f;

                yunaGo.AddComponent<AudioSource>();
                yunaGo.AddComponent<Yuna25DPlayerController>();
            }
            else
            {
                yunaGo.transform.position = DarkRoomStartLayout.Position;
                var body = yunaGo.transform.Find("Body_Visual");
                if (body != null && body.GetComponent<YunaCharacterView>() == null)
                {
                    ApplyMat(body.gameObject, LoadMat("M_Yuna_Dress"));
                    var bow = body.Find("Yuna_Ribbon");
                    if (bow == null)
                    {
                        var newBow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        newBow.name = "Yuna_Ribbon";
                        newBow.transform.SetParent(body, false);
                        newBow.transform.localPosition = new Vector3(0f, 0.78f, -0.05f);
                        newBow.transform.localScale = new Vector3(0.40f, 0.18f, 0.22f);
                        var bowCol = newBow.GetComponent<Collider>();
                        if (bowCol != null) Object.DestroyImmediate(bowCol);
                        ApplyMat(newBow, LoadMat("M_Yuna_Ribbon"));
                    }
                }
                var haloTrans = yunaGo.transform.Find("Yuna_FillLight");
                if (haloTrans != null)
                {
                    var fillLight = haloTrans.GetComponent<Light>();
                    if (fillLight != null)
                    {
                        fillLight.color = new Color(1.0f, 0.86f, 0.74f);
                        fillLight.intensity = 0.55f;
                        fillLight.range = 2.6f;
                    }
                }
            }

            // Reinstall the authored visual when available; rebuilding the room must not restore the capsule.
            YunaCharacterModelBuilder.Install(yunaGo.GetComponent<Yuna25DPlayerController>());

            // 5. Instanciar Nanda (Fadinha Guia: maior fonte de luz do quarto escuro)
            var nandaGo = GameObject.Find("Nanda_FairyGuide");
            if (nandaGo == null)
            {
                nandaGo = new GameObject("Nanda_FairyGuide");
                nandaGo.transform.position = new Vector3(DarkRoomStartLayout.FirstPlatformX, -7.35f, -0.15f);

                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "Sparkle_Body";
                sphere.transform.SetParent(nandaGo.transform, false);
                sphere.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
                var sphereCol = sphere.GetComponent<Collider>();
                if (sphereCol != null) Object.DestroyImmediate(sphereCol);
                ApplyMat(sphere, LoadMat("M_Nanda_Glow"));

                var fairyLight = nandaGo.AddComponent<Light>();
                fairyLight.type = LightType.Point;
                fairyLight.color = new Color(1.0f, 0.85f, 0.38f); // Ambar dourado quente
                fairyLight.intensity = 6.0f;
                fairyLight.range = 11.5f;

                nandaGo.AddComponent<NandaFairyGuide>();
            }
            else
            {
                nandaGo.transform.position = new Vector3(DarkRoomStartLayout.FirstPlatformX, -7.35f, -0.15f);
                var sphere = nandaGo.transform.Find("Sparkle_Body");
                if (sphere != null)
                {
                    sphere.localScale = new Vector3(0.28f, 0.28f, 0.28f);
                    ApplyMat(sphere.gameObject, LoadMat("M_Nanda_Glow"));
                }
                var fairyLight = nandaGo.GetComponent<Light>();
                if (fairyLight != null)
                {
                    fairyLight.color = new Color(1.0f, 0.85f, 0.38f);
                    fairyLight.intensity = 6.0f;
                    fairyLight.range = 11.5f;
                }
            }

            // 6. Conectar Camera ao Transform da Yuna
            sideCam.SetTarget(yunaGo.transform);
            sideCam.ConfigureSection(DarkRoomStartLayout.CameraY);
            sideCam.SnapToTargetX(DarkRoomStartLayout.StartX, DarkRoomStartLayout.CameraOffset);

            var yunaCtrl = yunaGo.GetComponent<Yuna25DPlayerController>();
            var yunaAudioSource = yunaGo.GetComponent<AudioSource>();
            if (yunaAudioSource == null) yunaAudioSource = yunaGo.AddComponent<AudioSource>();
            yunaAudioSource.playOnAwake = false;
            yunaAudioSource.spatialBlend = 0f; // Audio 2D claro e presente

            var nandaGuide = nandaGo.GetComponent<NandaFairyGuide>();
            if (nandaGuide != null)
            {
                nandaGuide.SetYuna(yunaGo.transform);
                var fairyTwinkleClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/fairy_twinkle.wav");
                nandaGuide.ConfigureAudio(fairyTwinkleClip);

                var soNanda = new SerializedObject(nandaGuide);
                soNanda.FindProperty("twinkleSfx").objectReferenceValue = fairyTwinkleClip;
                soNanda.FindProperty("baseIntensity").floatValue = 6.0f;
                soNanda.FindProperty("lightRange").floatValue = 11.5f;
                soNanda.ApplyModifiedProperties();
                EditorUtility.SetDirty(nandaGuide);
            }

            // Injetar e persistir AudioClips dinamicos em Yuna via SerializedObject
            if (yunaCtrl != null)
            {
                var jumpClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/jump-girl.wav");
                if (jumpClip == null) jumpClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/jump_airy.wav");
                var landWoodClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/land_wood.wav");
                var footstep1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/footstep_wood_1.wav");
                var footstep2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/footstep_wood_2.wav");
                var footstep3 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/footstep_wood_3.wav");
                var landPlushClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/land_plush.wav");
                var drumBounceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/drum_strike.wav");
                var comicFallClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/comic_fall_thud.wav");

                var footsteps = new AudioClip[] { footstep1, footstep2, footstep3 };
                yunaCtrl.ConfigureAudioClips(jumpClip, landWoodClip, footsteps, landPlushClip, drumBounceClip, comicFallClip);

                var soYuna = new SerializedObject(yunaCtrl);
                soYuna.FindProperty("jumpSfx").objectReferenceValue = jumpClip;
                soYuna.FindProperty("landSfx").objectReferenceValue = landWoodClip;
                soYuna.FindProperty("landPlushSfx").objectReferenceValue = landPlushClip;
                soYuna.FindProperty("drumBounceSfx").objectReferenceValue = drumBounceClip;
                soYuna.FindProperty("comicFallThudSfx").objectReferenceValue = comicFallClip;

                var footstepsProp = soYuna.FindProperty("footstepWoodClips");
                footstepsProp.arraySize = 3;
                footstepsProp.GetArrayElementAtIndex(0).objectReferenceValue = footstep1;
                footstepsProp.GetArrayElementAtIndex(1).objectReferenceValue = footstep2;
                footstepsProp.GetArrayElementAtIndex(2).objectReferenceValue = footstep3;

                soYuna.ApplyModifiedProperties();
                EditorUtility.SetDirty(yunaCtrl);
            }

            // 7. Configurar Gerenciador de Abertura e Progressao e Trilha Sonora (BGM)
            var managerGo = GameObject.Find("DarkRoom_GameManager");
            if (managerGo == null) managerGo = new GameObject("DarkRoom_GameManager");
            var progression = managerGo.GetComponent<DarkRoomProgressionManager>();
            if (progression == null) progression = managerGo.AddComponent<DarkRoomProgressionManager>();
            var opening = managerGo.GetComponent<DarkRoomOpeningSequence>();
            if (opening == null) opening = managerGo.AddComponent<DarkRoomOpeningSequence>();

            // Configurar AudioSource para Trilha Sonora (BGM) em looping continuo
            var bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/High-Frequency Violin.mp3");
            var bgmAudioSource = managerGo.GetComponent<AudioSource>();
            if (bgmAudioSource == null) bgmAudioSource = managerGo.AddComponent<AudioSource>();
            bgmAudioSource.playOnAwake = true;
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = 0.50f;
            bgmAudioSource.spatialBlend = 0f; // Som 2D envolvente
            if (bgmClip != null) bgmAudioSource.clip = bgmClip;

            // Vinculacao via SerializedObject para persistir referencias na cena
            var soProgression = new SerializedObject(progression);
            soProgression.FindProperty("yuna").objectReferenceValue = yunaCtrl;
            soProgression.FindProperty("nanda").objectReferenceValue = nandaGuide;
            soProgression.FindProperty("openingSequence").objectReferenceValue = opening;
            soProgression.FindProperty("cameraRunner").objectReferenceValue = sideCam;
            soProgression.FindProperty("blockoutBuilder").objectReferenceValue = builder;
            soProgression.FindProperty("bgmAudioSource").objectReferenceValue = bgmAudioSource;
            soProgression.FindProperty("bgmMusicClip").objectReferenceValue = bgmClip;

            // Injetar e persistir a fonte manuscrita Caveat.ttf na interface
            var caveatFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Caveat.ttf");
            if (caveatFont != null)
            {
                progression.ConfigureUiFont(caveatFont);
                soProgression.FindProperty("caveatFont").objectReferenceValue = caveatFont;
            }

            soProgression.ApplyModifiedProperties();
            EditorUtility.SetDirty(progression);

            var soOpening = new SerializedObject(opening);
            soOpening.FindProperty("yuna").objectReferenceValue = yunaCtrl;
            soOpening.FindProperty("nanda").objectReferenceValue = nandaGuide;
            soOpening.ApplyModifiedProperties();

            DarkRoomValueHierarchySetup.ConfigureScene();
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[DarkRoomScenePopulator] Cena DarkRoom_Gameplay populada com Direção Artística e salva com sucesso!");
        }

        private static Material LoadMat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");
        }

        private static void ApplyMat(GameObject go, Material mat)
        {
            if (mat == null) return;
            var r = go.GetComponent<MeshRenderer>();
            if (r != null) r.sharedMaterial = mat;
        }
    }
}

