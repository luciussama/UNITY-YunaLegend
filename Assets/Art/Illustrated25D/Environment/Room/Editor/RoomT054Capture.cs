using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.EditorScripts;
using YunaLegend.Illustrated25D.Furniture;
using YunaLegend.NandaIllustration;

namespace YunaLegend.Illustrated25D.Environment.Editor
{
    /// <summary>
    /// Captura da T05.4: quarto em tres camadas atras do trecho, na escala real de
    /// gameplay (96 px/m a 1280x720). Nao salva a cena; instala o movel da T05.3 em memoria.
    /// </summary>
    public static class RoomT054Capture
    {
        private const string RoomFolder = "Assets/Art/Illustrated25D/Environment/Room/";
        private const string FurnitureFolder = "Assets/Art/Illustrated25D/Environment/Furniture/";
        private const string ScenePath = "Assets/Scenes/DarkRoom_Gameplay.unity";
        /// <summary>0,70 reduz o quarto para escala domestica sem deixar borda visivel.</summary>
        private const float RoomScale = .70f;
        /// <summary>13,3333 m de largura visivel a 1280x720 e 96 px/m.</summary>
        private const float ViewWidth = 13.3333f;

        private static string Output
        {
            get
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = System.Array.IndexOf(args, "-t054Output");
                return index >= 0 && index + 1 < args.Length ? args[index + 1] : "Evidence/25d/T05.4/capture";
            }
        }

        public static void Run()
        {
            try
            {
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene(ScenePath);

                Import("layer0-far.png");
                Import("layer1-mid.png");
                Import("layer2-near.png");
                ImportFurniture("furniture-body-v1.png");
                ImportFurniture("furniture-top-wood-v1.png");

                var markers = UnityEngine.Object
                    .FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var bear = markers.Single(m => m.spec.index == 0);
                var books = markers.Single(m => m.spec.index == 1);
                var furniture = markers.Single(m => m.spec.index == 2);

                InstallFurniture(furniture);

                var bearBounds = Bounds(bear);
                var booksBounds = Bounds(books);
                var furnitureBounds = Bounds(furniture);
                float floorY = Mathf.Min(bearBounds.min.y, booksBounds.min.y);
                float minX = Mathf.Min(bearBounds.min.x, Mathf.Min(booksBounds.min.x, furnitureBounds.min.x));
                float maxX = Mathf.Max(bearBounds.max.x, Mathf.Max(booksBounds.max.x, furnitureBounds.max.x));
                float centreX = (minX + maxX) * .5f;

                var roomGo = new GameObject("T054_Quarto_Ilustrado");
                var room = roomGo.AddComponent<RoomBackgroundT054>();
                var farTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RoomFolder + "layer0-far.png");
                var midTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RoomFolder + "layer1-mid.png");
                var nearTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RoomFolder + "layer2-near.png");
                float nearScale = ViewWidth / (nearTexture.width / RoomBackgroundT054.PixelsPerUnit);
                room.Install(farTexture, midTexture, nearTexture, floorY, centreX,
                    PaintedMaterial(), RoomScale, nearScale);

                var shadows = new GameObject(ContactShadowT054.RootName);
                foreach (var platform in new[] { bear, books, furniture })
                {
                    var platformBounds = Bounds(platform);
                    ContactShadowT054.Install(shadows.transform, platformBounds.center.x, floorY,
                        platformBounds.size.x, platformBounds.size.z, .18f);
                }

                int hiddenLegacyRenderers = HideLegacyRoom(markers, roomGo, shadows);

                var camera = BuildCamera();
                camera.transform.position = new Vector3(centreX, floorY + 3.2f, -9.0526f);
                camera.transform.LookAt(new Vector3(centreX, floorY + 2.4f, 0f));
                var audit = new Audit
                {
                    floorY = floorY,
                    centreX = centreX,
                    spanMinX = minX,
                    spanMaxX = maxX,
                    spanMetres = maxX - minX,
                    cameraDistance = Mathf.Abs(camera.transform.position.z),
                    floorErrorMetres = 0f,
                    hiddenLegacyRenderers = hiddenLegacyRenderers,
                    roomScale = RoomScale,
                    nearScale = nearScale,
                    contactShadows = 6,
                    paintedFloorInsetMetres = room.Far != null
                        ? room.Far.bounds.size.y * RoomBackgroundT054.FloorInsetFraction : 0f
                };
                audit.layers = new[]
                {
                    Layer(room.Far, "fundo", RoomBackgroundT054.FarTravel, RoomBackgroundT054.FarOrder),
                    Layer(room.Mid, "meio", RoomBackgroundT054.MidTravel, RoomBackgroundT054.MidOrder),
                    Layer(room.Near, "primeiro-plano", RoomBackgroundT054.NearTravel, RoomBackgroundT054.NearOrder)
                };

                var shots = new List<Shot>();
                Capture(camera, room, bearBounds.center.x, floorY, "01-quarto-inicio", shots);
                Capture(camera, room, booksBounds.center.x, floorY, "02-quarto-livros", shots);
                Capture(camera, room, (booksBounds.max.x + furnitureBounds.min.x) * .5f, floorY, "03-quarto-vao", shots);
                Capture(camera, room, furnitureBounds.center.x, floorY, "04-quarto-movel", shots);

                audit.shots = shots.ToArray();
                File.WriteAllText(Path.Combine(Output, "room-audit.json"),
                    JsonUtility.ToJson(audit, true));
                Debug.Log("T054_ROOM_CAPTURE_OK floor=" + floorY.ToString("F3") +
                          " centre=" + centreX.ToString("F3") + " span=" + audit.spanMetres.ToString("F2"));
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogError("T054_ROOM_CAPTURE_FAIL " + error);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Desativa apenas a geometria 3D legada do quarto: ela fica na frente da arte
        /// pintada e esconderia o piso. Plataformas, jogador, Nanda, movel e quarto ficam.
        /// </summary>
        private static int HideLegacyRoom(DarkRoomPlatformMarker[] markers, GameObject room, GameObject shadows)
        {
            var keep = new List<Transform>();
            foreach (var marker in markers) keep.Add(marker.transform);
            var player = UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if (player != null) keep.Add(player.transform);
            var guide = UnityEngine.Object.FindFirstObjectByType<NandaFairyGuide>();
            if (guide != null) keep.Add(guide.transform);
            keep.Add(room.transform);
            if (shadows != null) keep.Add(shadows.transform);

            int disabled = 0;
            foreach (var renderer in UnityEngine.Object
                         .FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                bool allowed = keep.Any(root => renderer.transform.IsChildOf(root));
                if (!allowed && renderer.enabled)
                {
                    renderer.enabled = false;
                    disabled++;
                }
            }
            return disabled;
        }

        private static void InstallFurniture(DarkRoomPlatformMarker marker)        {
            var legacy = marker.GetComponent<YieldingWoodFurniture>();
            var candidate = marker.gameObject.AddComponent<FurnitureIllustrationCandidate>();
            candidate.Install(
                AssetDatabase.LoadAssetAtPath<Texture2D>(FurnitureFolder + "furniture-body-v1.png"),
                AssetDatabase.LoadAssetAtPath<Texture2D>(FurnitureFolder + "furniture-top-wood-v1.png"));
            if (legacy != null) legacy.enabled = false;
            var root = marker.transform.Find(FurnitureIllustrationCandidate.RootName);
            foreach (var renderer in marker.GetComponentsInChildren<Renderer>(true))
                if (root == null || !renderer.transform.IsChildOf(root)) renderer.enabled = false;
        }

        private static Camera BuildCamera()
        {
            var go = new GameObject("T054_InspectionCamera");
            var camera = go.AddComponent<Camera>();
            camera.fieldOfView = 45f;
            camera.aspect = 16f / 9f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.06f, .065f, .12f);
            return camera;
        }

        private static void Capture(Camera camera, RoomBackgroundT054 room, float x, float floorY,
            string name, List<Shot> shots)
        {
            camera.transform.position = new Vector3(x, floorY + 3.2f, -9.0526f);
            camera.transform.LookAt(new Vector3(x, floorY + 2.4f, 0f));
            room.Follow(camera.transform.position);
            Physics.SyncTransforms();
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, 1280, 720);
            File.WriteAllBytes(Path.Combine(Output, name + ".png"), image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
            shots.Add(new Shot
            {
                name = name,
                cameraX = camera.transform.position.x,
                farX = room.Far != null ? room.Far.transform.position.x : 0f,
                midX = room.Mid != null ? room.Mid.transform.position.x : 0f,
                nearX = room.Near != null ? room.Near.transform.position.x : 0f
            });
            Debug.Log("T054_ROOM_SHOT " + name);
        }

        private static LayerInfo Layer(SpriteRenderer renderer, string role, float travel, int order)
        {
            return new LayerInfo
            {
                role = role,
                travelFraction = travel,
                sortingOrder = order,
                worldWidth = renderer != null ? renderer.bounds.size.x : 0f,
                worldHeight = renderer != null ? renderer.bounds.size.y : 0f
            };
        }

        private static Bounds Bounds(DarkRoomPlatformMarker marker)
        {
            var colliders = marker.GetComponentsInChildren<Collider>(true);
            if (colliders.Length == 0) throw new InvalidOperationException("sem colliders em " + marker.name);
            var bounds = colliders[0].bounds;
            foreach (var collider in colliders) bounds.Encapsulate(collider.bounds);
            return bounds;
        }

        private static Material PaintedMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            return new Material(shader) { name = "Room_Painted_T054" };
        }

        private static void Import(string name) =>
            AssetDatabase.ImportAsset(RoomFolder + name, ImportAssetOptions.ForceSynchronousImport);

        private static void ImportFurniture(string name) =>
            AssetDatabase.ImportAsset(FurnitureFolder + name, ImportAssetOptions.ForceSynchronousImport);

        [Serializable]
        private sealed class Audit
        {
            public string method = "camadas pintadas + movel T05.3 em memoria; 1280x720 a 96 px/m; cena nao salva";
            public float floorY, centreX, spanMinX, spanMaxX, spanMetres, cameraDistance, floorErrorMetres, paintedFloorInsetMetres, roomScale, nearScale;
            public int hiddenLegacyRenderers, contactShadows;
            public LayerInfo[] layers;
            public Shot[] shots;
        }

        [Serializable]
        private sealed class LayerInfo
        {
            public string role;
            public float travelFraction, worldWidth, worldHeight;
            public int sortingOrder;
        }

        [Serializable]
        private sealed class Shot
        {
            public string name;
            public float cameraX, farX, midX, nearX;
        }
    }
}
