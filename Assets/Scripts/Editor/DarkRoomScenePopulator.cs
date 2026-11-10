using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.12f);
            var sideCam = camGo.GetComponent<SideScroll25DCamera>();
            if (sideCam == null) sideCam = camGo.AddComponent<SideScroll25DCamera>();

            // Adicionar UniversalAdditionalCameraData para URP
            var camData = camGo.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null) camData = camGo.AddComponent<UniversalAdditionalCameraData>();

            // 2. Luz de luar (Directional Light azulada)
            var lightGo = GameObject.Find("Moonlight Directional Light");
            if (lightGo == null) lightGo = new GameObject("Moonlight Directional Light");
            var dirLight = lightGo.GetComponent<Light>();
            if (dirLight == null) dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.55f, 0.65f, 0.95f);
            dirLight.intensity = 0.8f;
            lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // 3. Gerar Blockout Completo das 28 plataformas + piso corrido
            var blockoutRoot = GameObject.Find("DarkRoom_Blockout_Root");
            if (blockoutRoot == null) blockoutRoot = new GameObject("DarkRoom_Blockout_Root");
            var builder = blockoutRoot.GetComponent<DarkRoomBlockoutBuilder>();
            if (builder == null) builder = blockoutRoot.AddComponent<DarkRoomBlockoutBuilder>();
            builder.BuildBlockout();

            // 4. Instanciar Yuna (jogadora 2.5D) com visual e CharacterController
            var yunaGo = GameObject.Find("Yuna_Player");
            if (yunaGo == null)
            {
                yunaGo = new GameObject("Yuna_Player");
                yunaGo.transform.position = new Vector3(2.0f, -6.0f, 0f); // Topo da plataforma 0

                var cc = yunaGo.AddComponent<CharacterController>();
                cc.height = 1.4f;
                cc.radius = 0.35f;
                cc.center = new Vector3(0f, 0.7f, 0f);

                // Visual estilizado provisorio (capsula creme com laço carmim)
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body_Visual";
                body.transform.SetParent(yunaGo.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
                body.transform.localScale = new Vector3(0.6f, 0.7f, 0.6f);
                var bodyCol = body.GetComponent<Collider>();
                if (bodyCol != null) Object.DestroyImmediate(bodyCol);

                // Luz halo suave da Yuna (Roteiro 01 - fill light sutil)
                var haloGo = new GameObject("Yuna_FillLight");
                haloGo.transform.SetParent(yunaGo.transform, false);
                haloGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                var fillLight = haloGo.AddComponent<Light>();
                fillLight.type = LightType.Point;
                fillLight.color = new Color(1.0f, 0.85f, 0.75f); // Tom pessego suave
                fillLight.intensity = 0.8f;
                fillLight.range = 3.5f;

                yunaGo.AddComponent<AudioSource>();
                yunaGo.AddComponent<Yuna25DPlayerController>();
            }
            else
            {
                yunaGo.transform.position = new Vector3(2.0f, -6.0f, 0f);
            }

            // 5. Instanciar Nanda (Fadinha Guia com luz ambar)
            var nandaGo = GameObject.Find("Nanda_FairyGuide");
            if (nandaGo == null)
            {
                nandaGo = new GameObject("Nanda_FairyGuide");
                nandaGo.transform.position = new Vector3(2.8f, -5.2f, 0.2f);

                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "Sparkle_Body";
                sphere.transform.SetParent(nandaGo.transform, false);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                var sphereCol = sphere.GetComponent<Collider>();
                if (sphereCol != null) Object.DestroyImmediate(sphereCol);

                var fairyLight = nandaGo.AddComponent<Light>();
                fairyLight.type = LightType.Point;
                fairyLight.color = new Color(0.99f, 0.88f, 0.40f);
                fairyLight.intensity = 2.5f;
                fairyLight.range = 7.0f;

                nandaGo.AddComponent<NandaFairyGuide>();
            }
            else
            {
                nandaGo.transform.position = new Vector3(2.8f, -5.2f, 0.2f);
            }

            // 6. Conectar Camera ao Transform da Yuna
            sideCam.SetTarget(yunaGo.transform);

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
            soProgression.ApplyModifiedProperties();

            var soOpening = new SerializedObject(opening);
            soOpening.FindProperty("yuna").objectReferenceValue = yunaCtrl;
            soOpening.FindProperty("nanda").objectReferenceValue = nandaGuide;
            soOpening.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[DarkRoomScenePopulator] Cena DarkRoom_Gameplay populada e salva com sucesso!");
        }
    }
}
