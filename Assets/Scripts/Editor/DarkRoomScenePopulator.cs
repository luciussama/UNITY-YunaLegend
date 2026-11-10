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

            // 2. Luz de luar (Directional Light azulada de conto de fadas)
            var lightGo = GameObject.Find("Moonlight Directional Light");
            if (lightGo == null) lightGo = new GameObject("Moonlight Directional Light");
            var dirLight = lightGo.GetComponent<Light>();
            if (dirLight == null) dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.58f, 0.68f, 0.95f);
            dirLight.intensity = 0.85f;
            lightGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

            // 2b. Global Volume de Pos-Processamento URP (Bloom etereo, Vignette aconchegante)
            var volumeGo = GameObject.Find("DarkRoom_GlobalVolume");
            if (volumeGo == null) volumeGo = new GameObject("DarkRoom_GlobalVolume");
            var volume = volumeGo.GetComponent<Volume>();
            if (volume == null) volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/DefaultVolumeProfile.asset");
            if (profile != null) volume.profile = profile;

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
                LoadMat("M_DreamPortal_Gold")
            );

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

                // Luz halo suave da Yuna (Roteiro 01 - fill light sutil)
                var haloGo = new GameObject("Yuna_FillLight");
                haloGo.transform.SetParent(yunaGo.transform, false);
                haloGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                var fillLight = haloGo.AddComponent<Light>();
                fillLight.type = LightType.Point;
                fillLight.color = new Color(1.0f, 0.86f, 0.76f); // Tom pessego suave
                fillLight.intensity = 1.0f;
                fillLight.range = 4.2f;

                yunaGo.AddComponent<AudioSource>();
                yunaGo.AddComponent<Yuna25DPlayerController>();
            }
            else
            {
                yunaGo.transform.position = DarkRoomStartLayout.Position;
                var body = yunaGo.transform.Find("Body_Visual");
                if (body != null)
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
            }

            // 5. Instanciar Nanda (Fadinha Guia com nucleo emissivo ambar quente)
            var nandaGo = GameObject.Find("Nanda_FairyGuide");
            if (nandaGo == null)
            {
                nandaGo = new GameObject("Nanda_FairyGuide");
                nandaGo.transform.position = new Vector3(2.8f, -5.2f, 0.2f);

                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "Sparkle_Body";
                sphere.transform.SetParent(nandaGo.transform, false);
                sphere.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                var sphereCol = sphere.GetComponent<Collider>();
                if (sphereCol != null) Object.DestroyImmediate(sphereCol);
                ApplyMat(sphere, LoadMat("M_Nanda_Glow"));

                var fairyLight = nandaGo.AddComponent<Light>();
                fairyLight.type = LightType.Point;
                fairyLight.color = new Color(1.0f, 0.85f, 0.35f); // Ambar dourado quente
                fairyLight.intensity = 3.2f;
                fairyLight.range = 8.5f;

                nandaGo.AddComponent<NandaFairyGuide>();
            }
            else
            {
                nandaGo.transform.position = new Vector3(2.8f, -5.2f, 0.2f);
                var sphere = nandaGo.transform.Find("Sparkle_Body");
                if (sphere != null)
                {
                    ApplyMat(sphere.gameObject, LoadMat("M_Nanda_Glow"));
                }
                var fairyLight = nandaGo.GetComponent<Light>();
                if (fairyLight != null)
                {
                    fairyLight.color = new Color(1.0f, 0.85f, 0.35f);
                    fairyLight.intensity = 3.2f;
                    fairyLight.range = 8.5f;
                }
            }

            // 6. Conectar Camera ao Transform da Yuna
            sideCam.SetTarget(yunaGo.transform);
            sideCam.ConfigureSection(DarkRoomStartLayout.CameraY);
            sideCam.SnapToTargetX(DarkRoomStartLayout.StartX, DarkRoomStartLayout.CameraOffset);

            var yunaCtrl = yunaGo.GetComponent<Yuna25DPlayerController>();
            var nandaGuide = nandaGo.GetComponent<NandaFairyGuide>();
            if (nandaGuide != null)
            {
                nandaGuide.SetYuna(yunaGo.transform);
            }

            // 7. Configurar Gerenciador de Abertura e Progressao
            var managerGo = GameObject.Find("DarkRoom_GameManager");
            if (managerGo == null) managerGo = new GameObject("DarkRoom_GameManager");
            var progression = managerGo.GetComponent<DarkRoomProgressionManager>();
            if (progression == null) progression = managerGo.AddComponent<DarkRoomProgressionManager>();
            var opening = managerGo.GetComponent<DarkRoomOpeningSequence>();
            if (opening == null) opening = managerGo.AddComponent<DarkRoomOpeningSequence>();

            // Vinculacao via SerializedObject para persistir referencias na cena
            var soProgression = new SerializedObject(progression);
            soProgression.FindProperty("yuna").objectReferenceValue = yunaCtrl;
            soProgression.FindProperty("nanda").objectReferenceValue = nandaGuide;
            soProgression.FindProperty("openingSequence").objectReferenceValue = opening;
            soProgression.FindProperty("cameraRunner").objectReferenceValue = sideCam;
            soProgression.FindProperty("blockoutBuilder").objectReferenceValue = builder;
            soProgression.ApplyModifiedProperties();

            var soOpening = new SerializedObject(opening);
            soOpening.FindProperty("yuna").objectReferenceValue = yunaCtrl;
            soOpening.FindProperty("nanda").objectReferenceValue = nandaGuide;
            soOpening.ApplyModifiedProperties();

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

