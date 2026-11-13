using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Installs the approved T05.4 room composition (panorama, contact shadows, penumbra)
    /// at runtime, so the playable build and the review captures use the same look.
    /// Only the legacy 3D room renderers are hidden; markers, player, Nanda and lights stay.
    /// </summary>
    [DefaultExecutionOrder(220)]
    public sealed class RoomCompositionRuntimeInstaller : MonoBehaviour
    {
        public Texture2D panorama;
        public Shader penumbraShader;
        public float floorInsetFraction = 82f / 724f;
        public float farTravel = .35f;
        public bool installPenumbra = true;
        public bool tintCharacters = false;

        RoomBackgroundT054 room;
        PenumbraLightingT054 lighting;
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

            HideLegacyRoom(allMarkers, roomGo, shadows);

            var player = FindFirstObjectByType<Yuna25DPlayerController>();
            var guide = FindFirstObjectByType<NandaFairyGuide>();
            if (installPenumbra && penumbraShader != null)
            {
                lighting = roomGo.AddComponent<PenumbraLightingT054>();
                lighting.room = room;
                lighting.lightingShader = penumbraShader;
                foreach (var panel in room.FarPanels) lighting.Bind(panel, 0);
                foreach (var marker in markers)
                    foreach (var renderer in marker.GetComponentsInChildren<Renderer>(true))
                        if (renderer.enabled) lighting.Bind(renderer, 1);
                if (player != null)
                    foreach (var renderer in player.GetComponentsInChildren<Renderer>(true))
                    {
                        bool blink = renderer.sharedMaterial != null
                            && renderer.sharedMaterial.HasProperty("_BlinkTex");
                        lighting.Bind(renderer, 2, true, blink);
                    }
                if (guide != null) lighting.nanda = guide.transform;
            }

            var camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.007f, .009f, .018f);
            }
            Debug.Log("T054_ROOM_RUNTIME_INSTALLED floor=" + floorY.ToString("F3")
                + " centre=" + centreX.ToString("F3") + " hidden=" + hidden.Count
                + " far=" + room.FarPanels.Count + " tint=" + tintCharacters + " lit=" + (lighting != null ? lighting.BoundRenderers : 0));
        }

        void LateUpdate()
        {
            var camera = Camera.main;
            if (room != null && camera != null) room.Follow(camera.transform.position);
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




