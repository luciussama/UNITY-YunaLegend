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
        private static bool Penumbra => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-t054Penumbra") >= 0;
        private static bool HighWindow => Penumbra || System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-t054HighWindow") >= 0;
        private static bool Panorama => HighWindow || System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-t054Panorama") >= 0;
        private static PenumbraLightingT054 lighting;
        private static SpriteRenderer reviewYuna, reviewNanda;
        private static DarkRoomPlatformMarker[] reviewPlatforms;

        private static string Output
        {
            get
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = System.Array.IndexOf(args, "-t054Output");
                return index >= 0 && index + 1 < args.Length ? args[index + 1] : "Evidence/25d/T05.4/contact-v2";
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
                float bookBaseBefore = books.transform.TransformPoint(new Vector3(0f,
                    IllustratedBookStackPlatformProfile.Document.mapping.quadBoundsLocal.bottom, 0f)).y;
                float bookSupportBefore = books.GetTopCenterWorld().y;
                ExtendBookBase(books, floorY, bookBaseBefore);
                float minX = Mathf.Min(bearBounds.min.x, Mathf.Min(booksBounds.min.x, furnitureBounds.min.x));
                float maxX = Mathf.Max(bearBounds.max.x, Mathf.Max(booksBounds.max.x, furnitureBounds.max.x));
                float centreX = (minX + maxX) * .5f;

                var roomGo = new GameObject("T054_Quarto_Ilustrado");
                var room = roomGo.AddComponent<RoomBackgroundT054>();
                var farTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RoomFolder + "layer0-far.png");
                var midTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RoomFolder + "layer1-mid.png");
                var nearTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RoomFolder + "layer2-near.png");
                float nearScale = ViewWidth / (nearTexture.width / RoomBackgroundT054.PixelsPerUnit);
                if (Panorama)
                {
                    var panorama = LoadPanorama();
                    room.InstallPanels(new[] { panorama }, panorama.width / 32f, null, null,
                        floorY, centreX, PaintedMaterial(), 1f, 1f, 82f / 724f, .35f);
                }
                else
                    room.Install(farTexture, midTexture, null, floorY, centreX,
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
                if (Penumbra) InstallPenumbra(room, new[] { bear, books, furniture }, markers);
                if (Penumbra) camera.backgroundColor = new Color(.007f, .009f, .018f);
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
                    floorErrorMetres = Mathf.Abs(BookBase(books) - floorY),
                    bookBaseBefore = bookBaseBefore,
                    bookBaseAfter = BookBase(books),
                    bookSupportDelta = books.GetTopCenterWorld().y - bookSupportBefore,
                    foregroundEnabled = false,
                    backgroundVersion = HighWindow ? "window-v4" : Panorama ? "scale-v3" : "contact-v2",
                    penumbraEnabled = Penumbra,
                    lightingMethod = Penumbra ? "projected per-fragment 2.5D moon/Nanda; no dynamic shadow occlusion" : "inspection unlit",
                    phosphor = Penumbra ? lighting.phosphor : 0f,
                    hiddenLegacyRenderers = hiddenLegacyRenderers,
                    roomScale = Panorama ? 1f : RoomScale,
                    nearScale = nearScale,
                    contactShadows = 6,
                    paintedFloorInsetMetres = room.Far != null
                        ? room.Far.bounds.size.y * (Panorama ? 82f / 724f : RoomBackgroundT054.FloorInsetFraction) : 0f
                };
                audit.layers = new[]
                {
                    Layer(room.Far, "fundo", room.FarTravelFraction, RoomBackgroundT054.FarOrder),
                    Layer(room.Mid, "meio", RoomBackgroundT054.MidTravel, RoomBackgroundT054.MidOrder),
                    Layer(room.Near, "primeiro-plano", RoomBackgroundT054.NearTravel, RoomBackgroundT054.NearOrder)
                };

                var shots = new List<Shot>();
                Capture(camera, room, bearBounds.center.x, floorY, "01-quarto-inicio", shots);
                Capture(camera, room, booksBounds.center.x, floorY, "02-quarto-livros", shots);
                Capture(camera, room, (booksBounds.max.x + furnitureBounds.min.x) * .5f, floorY, "03-quarto-vao", shots);
                Capture(camera, room, furnitureBounds.center.x, floorY, "04-quarto-movel", shots);
                if (Penumbra)
                {
                    CaptureLightingProof(camera);
                    audit.litRenderers = lighting.BoundRenderers;
                    audit.moonPosition = lighting.MoonPosition;
                    audit.nandaPosition = reviewNanda.transform.position;
                }

                audit.shots = shots.ToArray();
                if (audit.floorErrorMetres > .001f || Mathf.Abs(audit.bookSupportDelta) > .00001f)
                    throw new InvalidOperationException("Books base or support changed outside contact tolerance.");
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

        private static void InstallPenumbra(RoomBackgroundT054 room,
            DarkRoomPlatformMarker[] pilot, DarkRoomPlatformMarker[] all)
        {
            reviewPlatforms = pilot;
            // Hide unrelated legacy characters and sources only inside this unsaved fixture.
            foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) light.enabled = false;
            var player = UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if (player != null)
                foreach (var r in player.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            var guide = UnityEngine.Object.FindFirstObjectByType<NandaFairyGuide>();
            if (guide != null)
                foreach (var r in guide.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            foreach (var marker in all)
                if (!pilot.Contains(marker))
                    foreach (var r in marker.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            lighting = room.gameObject.AddComponent<PenumbraLightingT054>();
            lighting.room = room;
            foreach (var r in room.FarPanels) lighting.Bind(r, 0);
            foreach (var marker in pilot)
                foreach (var r in marker.GetComponentsInChildren<Renderer>())
                    if (r.enabled) lighting.Bind(r, 1);
            reviewYuna = new GameObject("T054_ApprovedYuna_StaticLightingProbe").AddComponent<SpriteRenderer>();
            reviewYuna.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Illustrated25D/Yuna/Yuna_Profile_v1.png");
            reviewYuna.sortingOrder = 100;
            reviewNanda = new GameObject("T054_ApprovedNanda_StaticLightingProbe").AddComponent<SpriteRenderer>();
            reviewNanda.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Illustrated25D/Nanda/Flight/Runtime/Frames/NandaFlight0.asset");
            reviewNanda.sortingOrder = 110;
            if (reviewYuna.sprite == null || reviewNanda.sprite == null)
                throw new InvalidOperationException("Approved character sprites missing");
            lighting.nanda = reviewNanda.transform;
            lighting.Bind(reviewYuna, 2);
            lighting.Bind(reviewNanda, 3);
        }

        private static void CaptureLightingProof(Camera camera)
        {
            string proof = Path.Combine(Output, "proof");
            Directory.CreateDirectory(proof);
            Action<string> save = name =>
            {
                lighting.Apply();
                var image = DarkRoomStartCapture.RenderDisplayImage(camera, 1280, 720);
                File.WriteAllBytes(Path.Combine(proof, name + ".png"), image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            };
            var position = reviewNanda.transform.position;
            lighting.nandaStrength = 0f; reviewNanda.enabled = false;
            save("01-no-nanda");
            lighting.nandaStrength = 1f; reviewNanda.enabled = true;
            reviewNanda.transform.position = position + Vector3.right * 4f;
            save("02-nanda-moved");
            reviewNanda.transform.position = position;
            lighting.moonStrength = 0f;
            save("03-no-moon");
            lighting.nandaStrength = 0f; reviewNanda.enabled = false;
            save("04-phosphor-only");
            lighting.phosphor = 0f;
            save("05-all-sources-off");
            lighting.phosphor = .035f; lighting.moonStrength = 1f;
            lighting.nandaStrength = 1f; reviewNanda.enabled = true;
            lighting.Apply();
            var shader = Shader.Find("YunaLegend/RoomPenumbraT054");
            if (ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Penumbra shader compile error");
        }

        // Reuse the approved painting at its original scale behind the stack. Only the
        // lower books remain exposed; the playable cover and its collider stay untouched.
        private static void ExtendBookBase(DarkRoomPlatformMarker marker, float floorY, float baseY)
        {
            float gap = baseY - floorY;
            if (gap <= .001f) return;
            var source = marker.transform.Find("Visual_Ilustrado_Livros");
            if (source == null) throw new InvalidOperationException("Missing illustrated book stack.");
            var lower = UnityEngine.Object.Instantiate(source.gameObject, marker.transform);
            lower.name = "Livros_Base_T054";
            lower.transform.position = source.position + Vector3.down * gap;
            var renderer = lower.GetComponent<Renderer>();
            renderer.sortingOrder = source.GetComponent<Renderer>().sortingOrder - 1;
        }

        private static float BookBase(DarkRoomPlatformMarker marker)
        {
            var lower = marker.transform.Find("Livros_Base_T054");
            var visual = lower != null ? lower : marker.transform.Find("Visual_Ilustrado_Livros");
            return visual.TransformPoint(new Vector3(0f,
                IllustratedBookStackPlatformProfile.Document.mapping.quadBoundsLocal.bottom, 0f)).y;
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
            if (Penumbra)
            {
                int index = name.StartsWith("01") ? 0 : name.StartsWith("04") ? 2 : 1;
                var top = reviewPlatforms[index].GetTopCenterWorld();
                reviewYuna.transform.position = new Vector3(top.x - (index == 2 ? 1.5f : 0f),
                    top.y - reviewYuna.sprite.bounds.min.y, -.65f);
                reviewNanda.transform.position = reviewYuna.transform.position + new Vector3(1.5f, 1.4f, -.1f);
                lighting.Apply();
            }
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

        private static Texture2D LoadPanorama()
        {
            string path = RoomFolder + (HighWindow ? "room-panorama-v4.png" : "room-panorama-v3.png");
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void Import(string name) =>
            AssetDatabase.ImportAsset(RoomFolder + name, ImportAssetOptions.ForceSynchronousImport);

        private static void ImportFurniture(string name) =>
            AssetDatabase.ImportAsset(FurnitureFolder + name, ImportAssetOptions.ForceSynchronousImport);

        [Serializable]
        private sealed class Audit
        {
            public string method = "contato v2: livros com base adicional, sem primeiro plano; Unity estatica, cena nao salva";
            public float bookBaseBefore, bookBaseAfter, bookSupportDelta;
            public bool foregroundEnabled;
            public bool penumbraEnabled;
            public string lightingMethod;
            public float phosphor;
            public int litRenderers;
            public Vector3 moonPosition, nandaPosition;
            public string backgroundVersion;
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
