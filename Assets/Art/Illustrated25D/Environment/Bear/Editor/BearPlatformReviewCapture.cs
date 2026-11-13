using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.EditorScripts;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.EditorScripts
{
    public static class BearPlatformReviewCapture
    {
        private static string Output
        {
            get
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = System.Array.IndexOf(args, "-yunaT05Output");
                return index >= 0 && index + 1 < args.Length ? args[index + 1] : "Evidence/25d/T05.1";
            }
        }

        public static void Run()
        {
            Directory.CreateDirectory(Output);
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            Physics.SyncTransforms();

            var camera = Camera.main;
            camera.aspect = 16f / 9f;
            var side = camera.GetComponent<SideScroll25DCamera>();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var guide = Object.FindFirstObjectByType<NandaFairyGuide>();
            var markers = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            DarkRoomPlatformMarker bear = null;
            DarkRoomPlatformMarker books = null;
            foreach (var marker in markers)
            {
                if (marker.spec.index == 0) bear = marker;
                if (marker.spec.index == 1) books = marker;
            }
            if (bear == null || books == null || player == null)
                throw new System.InvalidOperationException("T05.1 requires the persisted bear, books and player.");

            var report = BuildContactReport(camera, bear, books, player);

            bool playerWasActive = player.gameObject.activeSelf;
            bool sideWasEnabled = side != null && side.enabled;
            player.gameObject.SetActive(false);
            if (side != null) side.enabled = false;
            Vector3 crown = bear.GetTopCenterWorld();
            SetCamera(camera, crown + new Vector3(.18f, .70f, -4.15f), crown + new Vector3(0f, .02f, 0f));
            Save(camera, "02-bear-detail.png", 1280, 720);

            player.gameObject.SetActive(playerWasActive);
            player.transform.position = crown + Vector3.up * .03f;
            if (guide != null) guide.SetTargetPlatform(1);
            if (side != null)
            {
                side.enabled = sideWasEnabled;
                camera.transform.rotation = Quaternion.identity;
                side.SnapToRouteFraming(1);
            }
            Physics.SyncTransforms();
            report.yunaRoot = player.transform.position;
            report.yunaRootAboveCrown = player.transform.position.y - crown.y;
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) report.characterControllerBottom = controller.bounds.min.y;
            report.yunaViewport = camera.WorldToViewportPoint(player.transform.position + Vector3.up * .70f);
            report.crownViewport = camera.WorldToViewportPoint(crown);
            report.cameraPosition = camera.transform.position;
            report.cameraRotation = camera.transform.rotation;
            Save(camera, "01-gameplay-bear-contact.png", 1600, 900);

            if (side != null) side.enabled = false;
            SetCamera(camera, crown + new Vector3(2.10f, 2.25f, -5.00f), crown + new Vector3(.05f, .55f, 0f));
            Save(camera, "03-bear-contact-close.png", 1280, 720);

            Validate(report);
            File.WriteAllText(Path.Combine(Output, "contact-report.json"), JsonUtility.ToJson(report, true));
            Debug.Log(string.Format("T05_BEAR_REVIEW_COMPLETE crown=({0:F6},{1:F6}) gap={2:F6} reach={3:F6} supportError={4:F6} depthMisses={5}",
                crown.x, crown.y, report.routeGapToBooks, report.integrityJumpReach, report.supportMaxError, report.depthOutsideHits));
        }

        private static BearReviewReport BuildContactReport(Camera camera, DarkRoomPlatformMarker bear, DarkRoomPlatformMarker books, Yuna25DPlayerController player)
        {
            var profile = IllustratedBearPlatformProfile.Document;
            var support = bear.Support;
            Vector3 crown = bear.GetTopCenterWorld();
            var contactRoot = bear.transform.Find("Contacto_Fisico");
            var colliders = contactRoot != null ? contactRoot.GetComponentsInChildren<Collider>() : new Collider[0];
            var meshCollider = colliders.Length == 1 ? colliders[0] as MeshCollider : null;
            var visual = bear.transform.Find("Visual_Ilustrado_Urso");
            var renderer = visual != null ? visual.GetComponent<MeshRenderer>() : null;
            if (meshCollider == null || renderer == null)
                throw new System.InvalidOperationException("The persisted bear must have one MeshCollider and the painted visual.");

            var report = new BearReviewReport
            {
                method = "Unity 6000.3.28f1 EditMode; persisted DarkRoom_Gameplay scene; real final SideScroll25DCamera for gameplay capture; collider raycasts at Z=0; no scene save.",
                limitation = "Static contact and framing review only. This does not approve movement, jump/landing feel, mobile input, device performance, later books/furniture integration or the T05 aggregate.",
                platformIndex = bear.spec.index,
                sourceTexture = profile.source,
                sourceSha256 = profile.sourceSha256,
                textureWidth = profile.texture.width,
                textureHeight = profile.texture.height,
                profileSamples = profile.topProfile.Length,
                metresPerTexturePixel = profile.mapping.metresPerTexturePixel,
                visibleWidth = IllustratedBearPlatformProfile.VisibleWidth,
                visibleHeight = profile.mapping.visibleHeight,
                physicalDepth = IllustratedBearPlatformProfile.PhysicalDepth,
                crownOffsetX = IllustratedBearPlatformProfile.ContactOffsetX,
                crownHeightAboveFloor = IllustratedBearPlatformProfile.CrownHeightAboveFloor,
                markerPosition = bear.transform.position,
                crownWorld = crown,
                supportMinX = support.MinX,
                supportMaxX = support.MaxX,
                supportMinY = support.MinY,
                supportMaxY = support.MaxY,
                supportRegions = support.Regions.Count,
                supportSegments = support.Segments.Count,
                colliderCount = colliders.Length,
                colliderType = colliders.Length > 0 ? colliders[0].GetType().Name : string.Empty,
                colliderConvex = meshCollider.convex,
                colliderDepth = meshCollider.bounds.size.z,
                contactMeshVertices = meshCollider.sharedMesh != null ? meshCollider.sharedMesh.vertexCount : 0,
                visualLocalZ = visual.localPosition.z,
                visualRendererWidth = renderer.bounds.size.x,
                visualRendererHeight = renderer.bounds.size.y,
                visualTexture = renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null
                    ? renderer.sharedMaterial.mainTexture.name : string.Empty,
                legacyVisualActive = LegacyVisualActive(bear),
                cameraPosition = camera.transform.position
            };

            var depthSamples = new List<DepthSample>();
            foreach (float z in new[] { -.55f, -.30f, 0f, .30f, .55f, -.62f, .62f })
            {
                bool hit = meshCollider.Raycast(new Ray(new Vector3(crown.x, crown.y + .5f, z), Vector3.down), out var hitInfo, 1f);
                bool inside = Mathf.Abs(z) <= IllustratedBearPlatformProfile.PhysicalDepth * .5f;
                depthSamples.Add(new DepthSample { z = z, insidePhysicalDepth = inside, hit = hit, hitY = hit ? hitInfo.point.y : float.NaN });
                if (!inside && hit) report.depthOutsideHits++;
            }
            report.depthSamples = depthSamples.ToArray();

            var region = support.RegionNear(crown.x);
            if (region != null)
            {
                report.primaryRegionMinX = region.MinX;
                report.primaryRegionMaxX = region.MaxX;
                report.primaryRegionWidth = region.Width;
                float maxError = 0f;
                int samples = 0;
                for (float x = region.MinX + .02f; x < region.MaxX - .02f; x += .05f)
                {
                    if (!support.TryGetPoint(x, out var supportPoint)) continue;
                    if (!meshCollider.Raycast(new Ray(supportPoint + Vector3.up * .5f, Vector3.down), out var hit, 1f))
                        throw new System.InvalidOperationException("Support exposes a point without a collider hit at x=" + x);
                    maxError = Mathf.Max(maxError, Mathf.Abs(hit.point.y - supportPoint.y));
                    samples++;
                }
                report.supportMaxError = maxError;
                report.supportRaycastSamples = samples;
            }

            bool crownHit = meshCollider.Raycast(new Ray(crown + Vector3.up * .5f, Vector3.down), out var crownRay, 1f);
            report.crownRaycastHit = crownHit;
            report.crownRaycastY = crownHit ? crownRay.point.y : float.NaN;
            report.crownRaycastError = crownHit ? Mathf.Abs(crownRay.point.y - crown.y) : float.PositiveInfinity;

            report.routeGapToBooks = PlatformSupportProfile.Gap(support, books.Support, 1f);
            float deltaY = books.GetTopCenterWorld().y - crown.y;
            report.routeLandingAboveCrown = deltaY;
            report.integrityJumpReach = 3.6f * (Mathf.Sqrt(2f * 2.4f / 28f)
                + Mathf.Sqrt(2f * Mathf.Max(.05f, 2.4f - deltaY) / (28f * 1.8f)));
            report.sceneRunSpeed = player.RunSpeed;
            report.sceneJumpHeight = player.JumpHeight;
            return report;
        }

        private static bool LegacyVisualActive(DarkRoomPlatformMarker bear)
        {
            var legacy = bear.transform.Find("Visual_3D_Referencia_Desativada");
            return legacy != null && legacy.gameObject.activeSelf;
        }

        private static void Validate(BearReviewReport report)
        {
            if (report.colliderCount != 1 || report.colliderType != "MeshCollider")
                throw new System.InvalidOperationException("The support must remain one continuous MeshCollider.");
            if (report.crownRaycastError > .005f) throw new System.InvalidOperationException("Crown raycast misses the support plane.");
            if (report.supportMaxError > .005f) throw new System.InvalidOperationException("Support profile and collider disagree.");
            if (report.depthOutsideHits != 0) throw new System.InvalidOperationException("Invisible support exists outside physical depth.");
            if (report.visualTexture != "Bear_Platform_v1") throw new System.InvalidOperationException("Wrong painted texture.");
            if (Mathf.Abs(report.visualLocalZ) > .0001f) throw new System.InvalidOperationException("Visual and support planes differ.");
            if (report.legacyVisualActive) throw new System.InvalidOperationException("Legacy 3D bear must stay inactive.");
            if (report.integrityJumpReach <= report.routeGapToBooks)
                throw new System.InvalidOperationException("Bear-to-books gap exceeds the validated jump reach.");
        }

        private static void SetCamera(Camera camera, Vector3 position, Vector3 lookAt)
        {
            camera.transform.position = position;
            camera.transform.rotation = Quaternion.LookRotation(lookAt - position, Vector3.up);
        }

        private static void Save(Camera camera, string name, int width, int height)
        {
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, width, height);
            File.WriteAllBytes(Path.Combine(Output, name), image.EncodeToPNG());
            Object.DestroyImmediate(image);
            Debug.Log("T05_BEAR_SHOT " + name);
        }

        [System.Serializable]
        private sealed class BearReviewReport
        {
            public string method, limitation, sourceTexture, sourceSha256, colliderType, visualTexture;
            public int platformIndex, textureWidth, textureHeight, profileSamples, supportRegions, supportSegments;
            public int colliderCount, contactMeshVertices, supportRaycastSamples, depthOutsideHits;
            public float metresPerTexturePixel, visibleWidth, visibleHeight, physicalDepth, crownOffsetX, crownHeightAboveFloor;
            public float supportMinX, supportMaxX, supportMinY, supportMaxY, colliderDepth, visualLocalZ;
            public float visualRendererWidth, visualRendererHeight, primaryRegionMinX, primaryRegionMaxX, primaryRegionWidth;
            public float supportMaxError, crownRaycastY, crownRaycastError, routeGapToBooks, routeLandingAboveCrown;
            public float integrityJumpReach, sceneRunSpeed, sceneJumpHeight, yunaRootAboveCrown, characterControllerBottom;
            public bool colliderConvex, legacyVisualActive, crownRaycastHit;
            public Vector3 markerPosition, crownWorld, cameraPosition, yunaRoot, yunaViewport, crownViewport;
            public Quaternion cameraRotation;
            public DepthSample[] depthSamples;
        }

        [System.Serializable]
        private sealed class DepthSample
        {
            public float z, hitY;
            public bool insidePhysicalDepth, hit;
        }
    }
}
