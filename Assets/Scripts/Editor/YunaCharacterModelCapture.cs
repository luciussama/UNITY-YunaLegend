using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using YunaLegend.DarkRoom;
using YunaLegend.Camera25D;

namespace YunaLegend.EditorScripts
{
    public static class YunaCharacterModelCapture
    {
        static string Output
        {
            get { var args = System.Environment.GetCommandLineArgs(); int i = System.Array.IndexOf(args, "-yunaModelOutput");
                return i >= 0 ? args[i + 1] : "Evidence/yuna-model-study-2026-11-11"; }
        }
        static void Save(Camera camera, string name, int width = 1024, int height = 1024)
        {
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, width, height);
            File.WriteAllBytes(Path.Combine(Output, name + ".png"), image.EncodeToPNG()); Object.DestroyImmediate(image);
        }
        static void Lamp(string name, Vector3 angles, float intensity, Color color)
        {
            var light = new GameObject(name).AddComponent<Light>(); light.type = LightType.Directional;
            light.intensity = intensity; light.color = color; light.transform.rotation = Quaternion.Euler(angles);
        }
        [MenuItem("YunaLegend/Yuna/Capturar estudo 3D")]
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.27f, .28f, .31f);
            Lamp("StudioKey", new Vector3(35, 165, 0), 1.15f, new Color(1, .94f, .85f));
            Lamp("StudioFill", new Vector3(20, -55, 0), .45f, new Color(.84f, .90f, 1));
            Lamp("StudioRim", new Vector3(15, 10, 0), .65f, new Color(1, .88f, .73f));
            var camera = new GameObject("StudyCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.34f, .32f, .30f);
            camera.orthographic = true; camera.orthographicSize = .82f; camera.aspect = 1;
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(YunaCharacterModelBuilder.PrefabPath));
            var view = model.GetComponent<YunaCharacterView>();
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.transform.position = Vector3.down * .004f;
            var floor = new Material(Shader.Find("Universal Render Pipeline/Lit")); floor.SetColor("_BaseColor", new Color(.34f, .32f, .30f));
            floor.SetFloat("_Smoothness", 0); ground.GetComponent<Renderer>().sharedMaterial = floor;
            var target = new Vector3(0, .70f, 0);
            camera.transform.position = new Vector3(0, .78f, 3); camera.transform.LookAt(target); Save(camera, "01-front");
            camera.transform.position = new Vector3(1.6f, .95f, 2.8f); camera.transform.LookAt(target); Save(camera, "02-three-quarter");
            camera.transform.position = new Vector3(3, .78f, 0); camera.transform.LookAt(target); Save(camera, "03-profile");
            camera.transform.position = new Vector3(0, .78f, -3); camera.transform.LookAt(target); Save(camera, "04-back");
            camera.orthographicSize = .285f; camera.transform.position = new Vector3(.15f, 1.15f, 2); camera.transform.LookAt(new Vector3(0, 1.14f, 0)); Save(camera, "05-face");
            camera.orthographicSize = .82f; camera.transform.position = new Vector3(1.6f, .95f, 2.8f); camera.transform.LookAt(target);
            view.Evaluate(YunaStudyPose.Run, .18f); Save(camera, "06-run");
            view.Evaluate(YunaStudyPose.Jump, .18f); Save(camera, "07-jump");
            view.Evaluate(YunaStudyPose.Stumble, .18f, 1); Save(camera, "08-stumble");
            var skin = model.GetComponent<SkinnedMeshRenderer>(); var baked = new Mesh();
            view.ResetPose(); skin.BakeMesh(baked);
            File.WriteAllText(Path.Combine(Output, "model-report.json"), JsonUtility.ToJson(new ModelReport {
                vertices = skin.sharedMesh.vertexCount, triangles = skin.sharedMesh.triangles.Length / 3,
                bones = skin.bones.Length, materials = skin.sharedMaterials.Length,
                restBounds = baked.bounds, source = YunaCharacterModelBuilder.PrefabPath,
                method = "Actual Unity SkinnedMeshRenderer renders, neutral inspection lights; poses applied to bones. Not ImageGen. First study, no final likeness claim." }, true));
            Object.DestroyImmediate(baked); Object.DestroyImmediate(floor);
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>(); YunaCharacterModelBuilder.Install(player);
            player.transform.rotation = Quaternion.Euler(0, 90, 0);
            var bear = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsSortMode.None).Single(p => p.spec.index == 0);
            player.transform.position = bear.GetTopCenterWorld();
            var actualView = player.GetComponentInChildren<YunaCharacterView>(); actualView.Evaluate(YunaStudyPose.Idle, 0);
            camera = Camera.main; camera.aspect = 16f / 9f;
            var side = camera.GetComponent<SideScroll25DCamera>(); side.ScreenCrushActive = false; side.SnapToRouteFraming(1);
            Save(camera, "09-gameplay-bear", 1280, 720);
            var cabinet = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsSortMode.None).Single(p => p.spec.index == 2);
            player.transform.position = cabinet.Support.PointNear(cabinet.transform.position.x - 1);
            actualView.Evaluate(YunaStudyPose.Stumble, .18f, 1); side.SnapToRouteFraming(3);
            Save(camera, "10-gameplay-stumble", 1280, 720);
            Debug.Log("YUNA_MODEL_CAPTURE_COMPLETE:10 images; source scene not saved.");
        }
        [System.Serializable] class ModelReport
        {
            public int vertices, triangles, bones, materials; public Bounds restBounds; public string source, method;
        }
    }
}
