using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.EditorScripts;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.EditorScripts
{
    public static class BooksPlatformReviewCapture
    {
        private const string OfficialYunaSpritePath = "Assets/Art/Illustrated25D/Yuna/Joint/Runtime/Yuna_Profile_Runtime_v1.png";
        private const string OfficialYunaMaterialPath = "Assets/Art/Illustrated25D/Yuna/Joint/Runtime/Idle_Painted_Runtime.mat";
        private const string ApprovedYunaSpriteSha256 = "0b9315b4dd525f23f2ef14db04682846b43b3ca7793f47670129afc88dfb9044";

        private static string Output
        {
            get
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = System.Array.IndexOf(args, "-yunaT05BooksOutput");
                return index >= 0 && index + 1 < args.Length ? args[index + 1] : "Evidence/25d/T05.2";
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
            DarkRoomPlatformMarker next = null;
            foreach (var marker in markers)
            {
                if (marker.spec.index == 0) bear = marker;
                if (marker.spec.index == 1) books = marker;
                if (marker.spec.index == 2) next = marker;
            }
            if (bear == null || books == null || next == null || player == null)
                throw new System.InvalidOperationException("T05.2 requires the persisted bear, books, next platform and player.");

            var report = BuildContactReport(camera, bear, books, next, player);
            Vector3 top = books.GetTopCenterWorld();

            bool playerWasActive = player.gameObject.activeSelf;
            bool sideWasEnabled = side != null && side.enabled;
            player.gameObject.SetActive(false);
            if (side != null) side.enabled = false;
            SetCamera(camera, top + new Vector3(.10f, .62f, -3.35f), top + new Vector3(0f, .02f, 0f));
            Save(camera, "02-books-detail.png", 1280, 720);

            player.gameObject.SetActive(playerWasActive);
            player.transform.position = top + Vector3.up * .03f;
            player.SetRunDirection(1f);

            var legacyYuna = player.transform.Find("Body_Visual");
            bool legacyYunaWasActive = legacyYuna != null && legacyYuna.gameObject.activeSelf;
            if (legacyYuna != null) legacyYuna.gameObject.SetActive(false);

            var officialSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OfficialYunaSpritePath);
            var officialMaterial = AssetDatabase.LoadAssetAtPath<Material>(OfficialYunaMaterialPath);
            if (officialSprite == null || officialMaterial == null)
                throw new System.InvalidOperationException("The approved illustrated Yuna idle sprite/material are missing for the T05.2 review.");
            var reviewVisual = new GameObject("T05_Review_Official_Yuna_Visual");
            reviewVisual.transform.SetParent(player.transform, false);
            var reviewRenderer = reviewVisual.AddComponent<SpriteRenderer>();
            reviewRenderer.sprite = officialSprite;
            reviewRenderer.sharedMaterial = officialMaterial;
            reviewRenderer.flipX = false;
            reviewRenderer.sortingOrder = 100;
            reviewVisual.transform.localPosition = new Vector3(
                0f,
                top.y - player.transform.position.y - officialSprite.bounds.min.y,
                -.05f);

            if (guide != null) guide.SetTargetPlatform(1);
            if (side != null)
            {
                side.enabled = sideWasEnabled;
                camera.transform.rotation = Quaternion.identity;
                side.SnapToRouteFraming(1);
            }
            Physics.SyncTransforms();
            report.yunaVisualSource = "Approved illustrated idle painting, installed only for this review capture and never saved into the scene.";
            report.legacyYunaActive = legacyYuna != null && legacyYuna.gameObject.activeSelf;
            report.officialYunaActive = reviewVisual.activeInHierarchy && reviewRenderer.enabled;
            report.officialSpriteName = officialSprite.name;
            report.officialSpriteSha256 = Sha256(OfficialYunaSpritePath);
            report.officialMaterialName = officialMaterial.name;
            report.officialSpriteFacingRight = !reviewRenderer.flipX && player.RunDirection > 0f;
            report.officialSpriteBottomWorldY = reviewRenderer.bounds.min.y;
            report.officialSpriteFeetError = reviewRenderer.bounds.min.y - top.y;
            report.officialSpriteHeight = reviewRenderer.bounds.size.y;
            report.yunaRoot = player.transform.position;
            report.yunaRootAboveTop = player.transform.position.y - top.y;
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) report.characterControllerBottom = controller.bounds.min.y;
            report.yunaViewport = camera.WorldToViewportPoint(player.transform.position + Vector3.up * .70f);
            report.topViewport = camera.WorldToViewportPoint(top);
            report.cameraPosition = camera.transform.position;
            report.cameraRotation = camera.transform.rotation;
            Save(camera, "01-gameplay-books-contact.png", 1600, 900);

            if (side != null) side.enabled = false;
            SetCamera(camera, top + new Vector3(2.10f, 1.45f, -4.30f), top + new Vector3(.02f, .25f, 0f));
            Save(camera, "03-books-contact-close.png", 1280, 720);

            Validate(report);
            File.WriteAllText(Path.Combine(Output, "contact-report.json"), JsonUtility.ToJson(report, true));
            Debug.Log(string.Format("T05_BOOKS_REVIEW_COMPLETE top=({0:F6},{1:F6}) gapBear={2:F6}/{3:F6} gapNext={4:F6}/{5:F6} supportError={6:F6} depthMisses={7} yunaSprite={8} legacy3D={9} feetError={10:F6}",
                top.x, top.y, report.routeGapFromBear, report.integrityJumpReachFromBear,
                report.routeGapToNext, report.integrityJumpReachToNext, report.supportMaxError, report.depthOutsideHits,
                report.officialSpriteName, report.legacyYunaActive, report.officialSpriteFeetError));

            Object.DestroyImmediate(reviewVisual);
            if (legacyYuna != null) legacyYuna.gameObject.SetActive(legacyYunaWasActive);
        }

        private static BookReviewReport BuildContactReport(Camera camera, DarkRoomPlatformMarker bear, DarkRoomPlatformMarker books, DarkRoomPlatformMarker next, Yuna25DPlayerController player)
        {
            var profile = IllustratedBookStackPlatformProfile.Document;
            var support = books.Support;
            Vector3 top = books.GetTopCenterWorld();
            var contactRoot = books.transform.Find("Contacto_Fisico");
            var colliders = contactRoot != null ? contactRoot.GetComponentsInChildren<Collider>() : new Collider[0];
            var meshCollider = colliders.Length == 1 ? colliders[0] as MeshCollider : null;
            var visual = books.transform.Find("Visual_Ilustrado_Livros");
            var renderer = visual != null ? visual.GetComponent<MeshRenderer>() : null;
            if (meshCollider == null || renderer == null)
                throw new System.InvalidOperationException("The persisted books must expose one MeshCollider and the illustrated visual.");

            var report = new BookReviewReport
            {
                method = "Persisted gameplay scene; support profile intersected at the runner plane z=0; raycasts against the persisted MeshCollider.",
                limitation = "Static contact and framing review only. Yuna is shown with the approved illustrated idle painting temporarily for this capture and the scene is not saved; permanent run/jump/stumble integration remains T06. This does not approve movement, jump/landing feel, mobile input, device performance, later furniture integration or the T05 aggregate.",
                sourceTexture = Path.GetFileName(profile.source),
                sourceSha256 = profile.sourceSha256,
                colliderType = meshCollider.GetType().Name,
                visualTexture = renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null
                    ? renderer.sharedMaterial.mainTexture.name : string.Empty,
                platformIndex = books.spec.index,
                textureWidth = profile.texture.width,
                textureHeight = profile.texture.height,
                profileSamples = 4,
                supportRegions = support.Regions.Count,
                supportSegments = support.Segments.Count,
                colliderCount = colliders.Length,
                contactMeshVertices = meshCollider.sharedMesh.vertexCount,
                depthOutsideHits = 0,
                metresPerTexturePixel = profile.mapping.metresPerTexturePixel,
                visibleWidth = profile.mapping.visibleWidth,
                visibleHeight = profile.mapping.visibleHeight,
                supportWidth = profile.mapping.supportWidth,
                supportTilt = profile.mapping.supportTiltDegrees,
                contactOffsetX = profile.mapping.contactOffsetX,
                supportHeightAboveCentre = profile.mapping.supportHeightAboveCentre,
                physicalDepth = profile.mapping.physicalDepth,
                supportMinX = support.MinX,
                supportMaxX = support.MaxX,
                supportMinY = support.MinY,
                supportMaxY = support.MaxY,
                colliderDepth = meshCollider.bounds.size.z,
                visualLocalZ = visual.localPosition.z,
                visualRendererWidth = renderer.bounds.size.x,
                visualRendererHeight = renderer.bounds.size.y,
                colliderConvex = meshCollider.convex,
                legacyVisualActive = LegacyVisualActive(books),
                markerPosition = books.transform.position,
                topWorld = top,
            };

            var region = support.RegionNear(top.x);
            if (region != null)
            {
                report.primaryRegionMinX = region.MinX;
                report.primaryRegionMaxX = region.MaxX;
                report.primaryRegionWidth = region.Width;
            }

            float maxError = 0f;
            int samples = 0;
            for (int i = 0; i <= 12; i++)
            {
                float x = Mathf.Lerp(support.MinX, support.MaxX, i / 12f);
                if (!support.TryGetPoint(x, out var supportPoint)) continue;
                if (!meshCollider.Raycast(new Ray(supportPoint + Vector3.up * .5f, Vector3.down), out var hit, 1f))
                    throw new System.InvalidOperationException("Support exposes a point without a collider hit at x=" + x);
                maxError = Mathf.Max(maxError, Mathf.Abs(hit.point.y - supportPoint.y));
                samples++;
            }
            report.supportMaxError = maxError;
            report.supportRaycastSamples = samples;

            var segments = support.Segments;
            if (segments.Count > 0)
            {
                var first = segments[0];
                report.supportMeasuredTilt = Mathf.Atan2(first.right.y - first.left.y, first.right.x - first.left.x) * Mathf.Rad2Deg;
                report.supportMeasuredWidth = support.MaxX - support.MinX;
            }

            bool topHit = meshCollider.Raycast(new Ray(top + Vector3.up * .5f, Vector3.down), out var topRay, 1f);
            report.topRaycastHit = topHit;
            report.topRaycastY = topHit ? topRay.point.y : float.NaN;
            report.topRaycastError = topHit ? Mathf.Abs(topRay.point.y - top.y) : float.PositiveInfinity;

            var depthSamples = new List<DepthSample>();
            foreach (float z in new[] { -.60f, -.55f, -.50f, 0f, .50f, .55f, .60f })
            {
                var origin = top + new Vector3(0f, .5f, z);
                bool hit = meshCollider.Raycast(new Ray(origin, Vector3.down), out var depthHit, 1f);
                bool inside = Mathf.Abs(z) <= profile.mapping.physicalDepth * .5f + .0001f;
                if (hit != inside) report.depthOutsideHits++;
                depthSamples.Add(new DepthSample { z = z, hit = hit, insidePhysicalDepth = inside, hitY = hit ? depthHit.point.y : float.NaN });
            }
            report.depthSamples = depthSamples.ToArray();

            report.routeGapFromBear = PlatformSupportProfile.Gap(bear.Support, support, 1f);
            report.routeGapToNext = PlatformSupportProfile.Gap(support, next.Support, 1f);
            float deltaFromBear = top.y - bear.GetTopCenterWorld().y;
            float deltaToNext = next.GetTopCenterWorld().y - top.y;
            report.integrityJumpReachFromBear = JumpReach(deltaFromBear, player);
            report.integrityJumpReachToNext = JumpReach(deltaToNext, player);
            report.routeLandingAboveTop = deltaToNext;
            report.sceneRunSpeed = player.RunSpeed;
            report.sceneJumpHeight = player.JumpHeight;
            return report;
        }

        private static float JumpReach(float deltaY, Yuna25DPlayerController player)
        {
            const float gravity = 28f;
            return player.RunSpeed * (Mathf.Sqrt(2f * player.JumpHeight / gravity)
                + Mathf.Sqrt(2f * Mathf.Max(.05f, player.JumpHeight - deltaY) / (gravity * 1.8f)));
        }

        private static bool LegacyVisualActive(DarkRoomPlatformMarker books)
        {
            var legacy = books.transform.Find("Visual_3D_Referencia_Desativada");
            return legacy != null && legacy.gameObject.activeSelf;
        }

        private static void Validate(BookReviewReport report)
        {
            if (report.colliderCount != 1 || report.colliderType != "MeshCollider")
                throw new System.InvalidOperationException("The books support must be one continuous MeshCollider.");
            if (report.colliderConvex) throw new System.InvalidOperationException("The books support must remain non-convex.");
            if (report.topRaycastError > .005f) throw new System.InvalidOperationException("Top-cover raycast misses the support plane.");
            if (report.supportMaxError > .005f) throw new System.InvalidOperationException("Support profile and collider disagree.");
            if (report.depthOutsideHits != 0) throw new System.InvalidOperationException("Invisible support exists outside physical depth.");
            if (report.visualTexture != "Books_Platform_v1") throw new System.InvalidOperationException("Wrong painted books texture.");
            if (Mathf.Abs(report.visualLocalZ) > .0001f) throw new System.InvalidOperationException("Visual and support planes differ.");
            if (report.legacyVisualActive) throw new System.InvalidOperationException("Legacy 3D books must stay inactive.");
            if (report.legacyYunaActive) throw new System.InvalidOperationException("The discarded 3D Yuna body must stay hidden in the T05.2 review.");
            if (!report.officialYunaActive) throw new System.InvalidOperationException("The approved illustrated Yuna sprite must be visible in the review.");
            if (report.officialSpriteName != "Yuna_Profile_Runtime_v1")
                throw new System.InvalidOperationException("The review is not using the approved Yuna idle sprite.");
            if (!string.Equals(report.officialSpriteSha256, ApprovedYunaSpriteSha256, System.StringComparison.OrdinalIgnoreCase))
                throw new System.InvalidOperationException("The review sprite bytes do not match the approved Yuna painting.");
            if (!report.officialSpriteFacingRight)
                throw new System.InvalidOperationException("The official Yuna review sprite must face right with RunDirection=1.");
            if (Mathf.Abs(report.officialSpriteFeetError) > .005f)
                throw new System.InvalidOperationException("The official Yuna sprite feet do not align with the measured top cover.");
            if (Mathf.Abs(report.supportMeasuredWidth - IllustratedBookStackPlatformProfile.SupportWidth) > .005f)
                throw new System.InvalidOperationException("Support width drifted from the measured top cover.");
            if (Mathf.Abs(report.supportMeasuredTilt - IllustratedBookStackPlatformProfile.SupportTiltDegrees) > .05f)
                throw new System.InvalidOperationException("Support tilt drifted from the measured top cover.");
            if (Mathf.Abs(report.contactOffsetX - IllustratedBookStackPlatformProfile.ContactOffsetX) > .001f)
                throw new System.InvalidOperationException("Top-cover offset drifted from the measured painting.");
            if (report.integrityJumpReachFromBear <= report.routeGapFromBear)
                throw new System.InvalidOperationException("Bear-to-books gap exceeds the validated jump reach.");
            if (report.integrityJumpReachToNext <= report.routeGapToNext)
                throw new System.InvalidOperationException("Books-to-next gap exceeds the validated jump reach.");
        }

        private static string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return System.BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
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
            Debug.Log("T05_BOOKS_SHOT " + name);
        }

        [System.Serializable]
        private sealed class BookReviewReport
        {
            public string method, limitation, sourceTexture, sourceSha256, colliderType, visualTexture;
            public string yunaVisualSource, officialSpriteName, officialSpriteSha256, officialMaterialName;
            public int platformIndex, textureWidth, textureHeight, profileSamples, supportRegions, supportSegments;
            public int colliderCount, contactMeshVertices, supportRaycastSamples, depthOutsideHits;
            public float metresPerTexturePixel, visibleWidth, visibleHeight, supportWidth, supportTilt, contactOffsetX;
            public float supportHeightAboveCentre, physicalDepth, supportMinX, supportMaxX, supportMinY, supportMaxY;
            public float colliderDepth, visualLocalZ, visualRendererWidth, visualRendererHeight;
            public float primaryRegionMinX, primaryRegionMaxX, primaryRegionWidth;
            public float supportMeasuredTilt, supportMeasuredWidth, supportMaxError;
            public float topRaycastY, topRaycastError, routeGapFromBear, routeGapToNext, routeLandingAboveTop;
            public float integrityJumpReachFromBear, integrityJumpReachToNext, sceneRunSpeed, sceneJumpHeight;
            public float yunaRootAboveTop, characterControllerBottom;
            public float officialSpriteBottomWorldY, officialSpriteFeetError, officialSpriteHeight;
            public bool colliderConvex, legacyVisualActive, legacyYunaActive, officialYunaActive, officialSpriteFacingRight, topRaycastHit;
            public Vector3 markerPosition, topWorld, cameraPosition, yunaRoot, yunaViewport, topViewport;
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
