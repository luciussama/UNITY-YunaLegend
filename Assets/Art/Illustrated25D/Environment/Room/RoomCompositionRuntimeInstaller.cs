using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Furniture;
using YunaLegend.NandaIllustration;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Installs the approved T05.4 composition at runtime for the playable pilot:
    /// panorama v4, contact shadows, illustrated furniture (T05.3), illustrated Nanda
    /// (T04.1) and the penumbra lighting (moon, Nanda light, faint phosphorescence).
    /// The legacy room renderers are hidden; markers, player, controllers and lights stay.
    /// </summary>
    [DefaultExecutionOrder(220)]
    public sealed class RoomCompositionRuntimeInstaller : MonoBehaviour
    {
        public Texture2D panorama;
        public Shader penumbraShader;
        public Texture2D furnitureBody;
        public Texture2D furnitureWood;
        public Sprite[] nandaFrames;
        public float floorInsetFraction = 82f / 724f;
        public float farTravel = .35f;
        public bool installPenumbra = true;

        RoomBackgroundT054 room;
        PenumbraLightingT054 lighting;
        SpriteRenderer nandaSprite;
        readonly List<Renderer> hidden = new List<Renderer>();

        void Start()
        {
            if (panorama == null)
            {
                Debug.LogError("RoomCompositionRuntimeInstaller: panorama missing.");
                return;
            }
            var allMarkers = FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allMarkers == null || allMarkers.Length == 0)
            {
                Debug.LogError("RoomCompositionRuntimeInstaller: no platform markers.");
                return;
            }
            // The approved composition covers the pilot stretch only (bear, books, furniture).
            var markers = new List<DarkRoomPlatformMarker>();
            foreach (var marker in allMarkers)
                if (marker.spec.index >= 0 && marker.spec.index <= 2) markers.Add(marker);
            if (markers.Count == 0) markers.AddRange(allMarkers);

            float floorY = float.MaxValue, minX = float.MaxValue, maxX = float.MinValue;
            var boundsByMarker = new Dictionary<DarkRoomPlatformMarker, Bounds>();
            foreach (var marker in markers)
            {
                var bounds = LocalBounds(marker);
                boundsByMarker[marker] = bounds;
                floorY = Mathf.Min(floorY, bounds.min.y);
                minX = Mathf.Min(minX, bounds.min.x);
                maxX = Mathf.Max(maxX, bounds.max.x);
            }
            float centreX = (minX + maxX) * .5f;

            var roomGo = new GameObject("T054_Quarto_Ilustrado_Runtime");
            room = roomGo.AddComponent<RoomBackgroundT054>();
            var material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                ?? Shader.Find("Sprites/Default")) { name = "Room_Painted_Runtime" };
            room.InstallPanels(new[] { panorama }, panorama.width / 32f, null, null,
                floorY, centreX, material, 1f, 1f, floorInsetFraction, farTravel);

            var shadows = new GameObject(ContactShadowT054.RootName);
            foreach (var marker in markers)
            {
                var bounds = boundsByMarker[marker];
                ContactShadowT054.Install(shadows.transform, bounds.center.x, floorY,
                    bounds.size.x, bounds.size.z, .18f);
            }

            InstallFurniture(allMarkers);
            var nanda = InstallNanda();
            HideLegacyRoom(allMarkers, roomGo, shadows);

            var player = FindFirstObjectByType<Yuna25DPlayerController>();
            if (installPenumbra && penumbraShader != null)
            {
                lighting = roomGo.AddComponent<PenumbraLightingT054>();
                lighting.room = room;
                lighting.lightingShader = penumbraShader;
                foreach (var panel in room.FarPanels) lighting.Bind(panel, 0);
                foreach (var marker in allMarkers)
                    foreach (var renderer in marker.GetComponentsInChildren<Renderer>(true))
                        if (renderer.enabled) lighting.Bind(renderer, 1);
                if (player != null)
                    foreach (var renderer in player.GetComponentsInChildren<Renderer>(true))
                    {
                        bool blink = renderer.sharedMaterial != null
                            && renderer.sharedMaterial.HasProperty("_BlinkTex");
                        lighting.Bind(renderer, 2, true, blink);
                    }
                if (nandaSprite != null)
                {
                    lighting.nanda = nandaSprite.transform;
                    lighting.Bind(nandaSprite, 3);
                }
            }

            var camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.007f, .009f, .018f);
            }
            Debug.Log("T054_ROOM_RUNTIME_INSTALLED floor=" + floorY.ToString("F3")
                + " centre=" + centreX.ToString("F3") + " hidden=" + hidden.Count
                + " far=" + room.FarPanels.Count + " penumbra=" + (lighting != null)
                + " lit=" + (lighting != null ? lighting.BoundRenderers : 0));
        }

        void LateUpdate()
        {
            var camera = Camera.main;
            if (room != null && camera != null) room.Follow(camera.transform.position);
        }

        void InstallFurniture(DarkRoomPlatformMarker[] markers)
        {
            if (furnitureBody == null || furnitureWood == null) return;
            foreach (var marker in markers)
            {
                if (marker.spec.index != 2) continue;
                if (marker.transform.Find(FurnitureIllustrationCandidate.RootName) != null) return;
                var candidate = marker.gameObject.AddComponent<FurnitureIllustrationCandidate>();
                candidate.Install(furnitureBody, furnitureWood);
                var root = marker.transform.Find(FurnitureIllustrationCandidate.RootName);
                foreach (var renderer in marker.GetComponentsInChildren<Renderer>(true))
                    if (root == null || !renderer.transform.IsChildOf(root)) renderer.enabled = false;
                return;
            }
        }

        Transform InstallNanda()
        {
            var guide = FindFirstObjectByType<NandaFairyGuide>();
            if (guide == null || nandaFrames == null || nandaFrames.Length == 0) return null;
            var go = new GameObject("Nanda_Ilustrada_T041");
            go.transform.SetParent(guide.transform, false);
            go.transform.localPosition = Vector3.zero;
            nandaSprite = go.AddComponent<SpriteRenderer>();
            nandaSprite.sprite = nandaFrames[0];
            nandaSprite.sortingOrder = 110;
            var driver = go.AddComponent<NandaFlightIllustration>();
            driver.painting = nandaSprite;
            driver.visualRoot = go.transform;
            driver.frames = nandaFrames;
            driver.previewClock = true;
            foreach (var renderer in guide.GetComponentsInChildren<Renderer>(true))
                if (!renderer.transform.IsChildOf(go.transform)) renderer.enabled = false;
            return go.transform;
        }

        void HideLegacyRoom(DarkRoomPlatformMarker[] markers, GameObject roomObject, GameObject shadows)
        {
            var keep = new List<Transform>();
            foreach (var marker in markers) keep.Add(marker.transform);
            var player = FindFirstObjectByType<Yuna25DPlayerController>();
            if (player != null) keep.Add(player.transform);
            var guide = FindFirstObjectByType<NandaFairyGuide>();
            if (guide != null) keep.Add(guide.transform);
            keep.Add(roomObject.transform);
            keep.Add(shadows.transform);

            foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!renderer.enabled) continue;
                bool allowed = false;
                foreach (var root in keep)
                    if (renderer.transform.IsChildOf(root)) { allowed = true; break; }
                if (allowed) continue;
                renderer.enabled = false;
                hidden.Add(renderer);
            }
        }

        static Bounds LocalBounds(DarkRoomPlatformMarker marker)
        {
            var renderers = marker.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(marker.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }
}

