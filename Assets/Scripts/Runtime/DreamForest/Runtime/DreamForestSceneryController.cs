using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DreamForest.Core;

namespace YunaLegend.DreamForest.Runtime
{
    /// <summary>
    /// SRP Scenery, Lighting &amp; Camera controller for DreamForest_Gameplay with 1:1 visual parity
    /// against the legacy Dream Forest:
    /// - Act 1: parallax-scrolling moonlit forest, lower blue mist, 14 fireflies, leaf platforms with
    ///   painted vertical stems, swaying grass blades with stem roots, animated flapping bee wings,
    ///   animated dual-wing beetles, wind branch with full vertical bark trunk + swaying launch leaf,
    ///   spiders with silk thread.
    /// - Glider: 21 thorn gates extending off-screen + dark forest landing backdrop.
    /// - Act 2 Nightmare: midnight forest, soft dark floor tiles, swamp trunks wrapping the roach,
    ///   soil hills, rock/trunk covers with contact shadows, 4 mud lakes with foreground rim + splash
    ///   + swim ripples, giant roach with step shake, two golden eye spotlights clamped to covers &amp;
    ///   surface + eye flares + ground glow pools.
    /// - Sunlit Wake: sunlit picnic blanket backdrop (picnic-without-yuna-v1).
    /// </summary>
    public sealed class DreamForestSceneryController : MonoBehaviour
    {
        private Camera stageCamera;
        private bool? mobileOverride;

        private GameObject dreamRoot;
        private GameObject nightmareRoot;

        // Parallax backdrops (2 seamless tiles for horizontal scroll)
        private SpriteRenderer[] backdropTiles;
        private SpriteRenderer lowerMistRenderer;
        private SpriteRenderer[] fireflies;

        private Sprite dreamBgSprite;
        private Sprite nightmareBgSprite;
        private Sprite picnicWakeBgSprite;
        private Sprite radialGlowSprite;

        private readonly List<ObjectVisual> dreamObjects = new List<ObjectVisual>();
        private readonly List<GameObject> thornObjects = new List<GameObject>();
        private Transform launchLeafTransform;

        // Act 2 Nightmare visuals
        private SpriteRenderer roachRenderer;
        private readonly List<MeshFilter> spotlightFilters = new List<MeshFilter>();
        private readonly List<SpriteRenderer> eyeFlares = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> groundPools = new List<SpriteRenderer>();
        private readonly List<DreamForestSpotlight> spotlightBuffer = new List<DreamForestSpotlight>(2);
        private Material spotlightMat;
        private Material grassRibbonMat;
        private SpriteRenderer mudSplashRenderer;
        private SpriteRenderer mudRippleRenderer;

        private sealed class ObjectVisual
        {
            public DreamForestObjectSpec Spec;
            public GameObject Root;
            public Transform MainTransform;
            public Transform SpiderTransform;
            public Transform WingA;
            public Transform WingB;
            public Transform TrunkTransform;
            public MeshFilter GrassMeshFilter;
        }

        public void SetMobileViewportOverride(bool? isMobile) => mobileOverride = isMobile;

        public void Build(Camera cam, DreamForestLevelData level)
        {
            stageCamera = cam;
            cam.orthographic = true;
            cam.orthographicSize = 4.6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.063f, 0.157f, 0.231f, 1f); // #10283b matching DreamRenderer.js

            dreamBgSprite = DreamForestAtlasCatalog.FeatheredDreamBackdropSprite();
            nightmareBgSprite = DreamForestAtlasCatalog.FullSprite("forest-cinematic-v1", new Vector2(0.5f, 0.5f));
            picnicWakeBgSprite = DreamForestAtlasCatalog.FullSprite("picnic-without-yuna-v1", new Vector2(0.5f, 0.5f));
            radialGlowSprite = DreamForestAtlasCatalog.RadialGlowSprite();

            // 4 overlapping feathered parallax tiles (step = 0.74 * w, zero flipX mirroring)
            backdropTiles = new SpriteRenderer[4];
            for (int i = 0; i < backdropTiles.Length; i++)
            {
                var bgGo = new GameObject("DreamBackdrop_" + i, typeof(SpriteRenderer));
                bgGo.transform.SetParent(cam.transform, false);
                var sr = bgGo.GetComponent<SpriteRenderer>();
                sr.sprite = dreamBgSprite;
                sr.sortingOrder = -30;
                bgGo.transform.localPosition = new Vector3(0f, 0f, 18f);
                backdropTiles[i] = sr;
            }

            // Soft vertical gradient mist band on lower 32% (no hard top edge)
            var mistGo = new GameObject("DreamLowerMist", typeof(SpriteRenderer));
            mistGo.transform.SetParent(cam.transform, false);
            lowerMistRenderer = mistGo.GetComponent<SpriteRenderer>();
            lowerMistRenderer.sprite = DreamForestAtlasCatalog.LowerMistGradientSprite();
            lowerMistRenderer.color = Color.white;
            lowerMistRenderer.sortingOrder = 15;
            mistGo.transform.localPosition = new Vector3(0f, -3.2f, 12f);

            // 14 drifting glowing fireflies matching DreamRenderer.js
            fireflies = new SpriteRenderer[14];
            for (int i = 0; i < fireflies.Length; i++)
            {
                var ffGo = new GameObject("Firefly_" + i, typeof(SpriteRenderer));
                ffGo.transform.SetParent(cam.transform, false);
                var sr = ffGo.GetComponent<SpriteRenderer>();
                sr.sprite = radialGlowSprite;
                sr.color = new Color(0.72f, 0.85f, 0.84f, 0.45f);
                sr.sortingOrder = 16;
                ffGo.transform.localScale = new Vector3(0.14f, 0.14f, 1f);
                fireflies[i] = sr;
            }

            BuildDreamAct1(level);
            BuildNightmareAct2(level);
        }

        private void BuildDreamAct1(DreamForestLevelData level)
        {
            dreamRoot = new GameObject("DreamAct1Scenery");
            dreamRoot.transform.SetParent(transform, false);

            Sprite leafSprite = DreamForestAtlasCatalog.SliceTopDown("leaf-platform-v1", 27, 121, 1724, 279, new Vector2(0.5f, 1f));
            Sprite leafStemSprite = DreamForestAtlasCatalog.SliceTopDown("leaf-platform-v1", 850, 400, 130, 487, new Vector2(0.5f, 1f));
            Sprite beeBodySprite = DreamForestAtlasCatalog.SliceTopDown("friendly-bee-v1", 46, 70, 1107, 663, new Vector2(0.5f, 0.5f));
            Sprite beeWingSprite = DreamForestAtlasCatalog.SliceTopDown("friendly-bee-v1", 1183, 185, 806, 464, new Vector2(0.5f, 0f));
            Sprite beetleBodySprite = DreamForestAtlasCatalog.SliceTopDown("beetle-layers-v2", 128, 96, 996, 626, new Vector2(0.5f, 0.5f));
            Sprite beetleWingSprite = DreamForestAtlasCatalog.SliceTopDown("beetle-layers-v2", 1284, 140, 738, 490, new Vector2(1f, 0.3f));
            Sprite spiderSprite = DreamForestAtlasCatalog.SliceTopDown("spider-v1", 111, 154, 1268, 765, new Vector2(0.5f, 0.5f));
            Sprite branchSprite = DreamForestAtlasCatalog.SliceTopDown("branch-wind-v1", 380, 275, 1359, 352, new Vector2(0.5f, 1f));
            Sprite trunkSprite = DreamForestAtlasCatalog.SliceTopDown("branch-wind-v1", 0, 0, 300, 887, new Vector2(0.5f, 0.5f));
            Sprite launchLeafSprite = DreamForestAtlasCatalog.SliceTopDown("yuna-glider-v1", 0, 0, 1294, 460, new Vector2(0.5f, 0.5f));
            Sprite thornSprite = DreamForestAtlasCatalog.FullSprite("branch-thorns-v1", new Vector2(0.5f, 0.5f));
            Sprite thornBaseStrip = DreamForestAtlasCatalog.SliceTopDown("branch-thorns-v1", 0, 0, 1024, 80, new Vector2(0.5f, 0.5f));
            Sprite white = DreamForestAtlasCatalog.SolidWhiteSprite();

            grassRibbonMat = new Material(Shader.Find("Sprites/Default"))
            {
                mainTexture = DreamForestAtlasCatalog.GrassRibbonTexture(),
                color = Color.white,
            };

            foreach (var o in level.Objects)
            {
                var root = new GameObject(o.Id);
                root.transform.SetParent(dreamRoot.transform, false);
                var entry = new ObjectVisual { Spec = o, Root = root, MainTransform = root.transform };

                if (o.Kind == DreamForestObjectKind.Grass)
                {
                    // Continuous fixed-root curved ribbon mesh from top f.Y down to GrassRoot (1800px)
                    var meshGo = new GameObject("GrassRibbonMesh", typeof(MeshFilter), typeof(MeshRenderer));
                    meshGo.transform.SetParent(root.transform, false);
                    var mr = meshGo.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = grassRibbonMat;
                    mr.sortingOrder = 2;
                    entry.GrassMeshFilter = meshGo.GetComponent<MeshFilter>();
                    entry.MainTransform = meshGo.transform;
                }
                else if (o.Kind == DreamForestObjectKind.Web)
                {
                    var threadGo = new GameObject("WebThread", typeof(SpriteRenderer));
                    threadGo.transform.SetParent(root.transform, false);
                    var sr = threadGo.GetComponent<SpriteRenderer>();
                    sr.sprite = white;
                    sr.color = new Color(0.84f, 0.90f, 0.95f, 0.80f);
                    sr.sortingOrder = 2;
                    DreamForestAtlasCatalog.FitSprite(threadGo.transform, white, 0.032f, o.H / 100f);
                    threadGo.transform.localPosition = new Vector3(0f, -o.H / 200f, 0f);

                    var spGo = new GameObject("Spider", typeof(SpriteRenderer));
                    spGo.transform.SetParent(root.transform, false);
                    var ssr = spGo.GetComponent<SpriteRenderer>();
                    ssr.sprite = spiderSprite;
                    ssr.sortingOrder = 8;
                    DreamForestAtlasCatalog.FitSprite(spGo.transform, spiderSprite, o.SpiderW / 100f, o.SpiderH / 100f);
                    entry.SpiderTransform = spGo.transform;
                }
                else if (o.Kind == DreamForestObjectKind.Leaf || o.Kind == DreamForestObjectKind.Plateau)
                {
                    if (o.Kind == DreamForestObjectKind.Leaf)
                    {
                        // Painted leaf stem (850,400,130,487) anchored at 49%, width 0.27u, extending from y+52 down to GrassRoot
                        var stemGo = new GameObject("LeafStem", typeof(SpriteRenderer));
                        stemGo.transform.SetParent(root.transform, false);
                        var stemSr = stemGo.GetComponent<SpriteRenderer>();
                        stemSr.sprite = leafStemSprite ?? white;
                        stemSr.sortingOrder = 2;
                        float stemLen = Mathf.Max(4f, (DreamForestLevelData.GrassRoot - (o.Y + 52f)) / 100f);
                        DreamForestAtlasCatalog.FitSprite(stemGo.transform, stemSr.sprite, 0.27f, stemLen);
                        stemGo.transform.localPosition = new Vector3(-o.W * 0.01f / 100f, -0.52f, 0.05f);

                        // Uniform-scale leaf blade (1724x279) so leaf is never stretched vertically
                        var bladeGo = new GameObject("LeafBlade", typeof(SpriteRenderer));
                        bladeGo.transform.SetParent(root.transform, false);
                        var bsr = bladeGo.GetComponent<SpriteRenderer>();
                        bsr.sortingOrder = 3;
                        bsr.sprite = leafSprite ?? white;
                        float leafW = o.W / 100f;
                        float leafH = leafW * (279f / 1724f);
                        DreamForestAtlasCatalog.FitSprite(bladeGo.transform, bsr.sprite, leafW, leafH);
                    }
                }
                else if (o.Kind == DreamForestObjectKind.Branch)
                {
                    // Vertical anchored tree trunk extending above and below screen
                    if (trunkSprite != null)
                    {
                        var trunkGo = new GameObject("BranchTrunk");
                        trunkGo.transform.SetParent(dreamRoot.transform, false);
                        for (int k = -1; k <= 1; k++)
                        {
                            var segGo = new GameObject("TrunkSeg_" + k, typeof(SpriteRenderer));
                            segGo.transform.SetParent(trunkGo.transform, false);
                            var tsr = segGo.GetComponent<SpriteRenderer>();
                            tsr.sprite = trunkSprite;
                            tsr.sortingOrder = 2;
                            DreamForestAtlasCatalog.FitSprite(segGo.transform, trunkSprite, 1.35f, 5.2f);
                            segGo.transform.localPosition = new Vector3(0f, k * 5.0f, 0.08f);
                        }
                        entry.TrunkTransform = trunkGo.transform;
                    }

                    var sr = root.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 3;
                    sr.sprite = branchSprite ?? leafSprite ?? white;
                    DreamForestAtlasCatalog.FitSprite(root.transform, sr.sprite, o.W / 100f, o.H / 100f);
                }
                else if (o.Kind == DreamForestObjectKind.Bee)
                {
                    var bodyGo = new GameObject("BeeBody", typeof(SpriteRenderer));
                    bodyGo.transform.SetParent(root.transform, false);
                    var bsr = bodyGo.GetComponent<SpriteRenderer>();
                    bsr.sprite = beeBodySprite ?? white;
                    bsr.sortingOrder = 6;
                    DreamForestAtlasCatalog.FitSprite(bodyGo.transform, bsr.sprite, o.W / 100f, o.H / 100f);

                    if (beeWingSprite != null)
                    {
                        var wingGo = new GameObject("BeeWings", typeof(SpriteRenderer));
                        wingGo.transform.SetParent(root.transform, false);
                        var wsr = wingGo.GetComponent<SpriteRenderer>();
                        wsr.sprite = beeWingSprite;
                        wsr.sortingOrder = 5;
                        wsr.color = new Color(0.95f, 0.98f, 1f, 0.88f);
                        float wingW = o.W * 0.88f / 100f;
                        float wingH = wingW * (464f / 806f);
                        DreamForestAtlasCatalog.FitSprite(wingGo.transform, beeWingSprite, wingW, wingH);
                        wingGo.transform.localPosition = new Vector3(-o.W * 0.08f / 100f, o.H * 0.14f / 100f, 0.02f);
                        entry.WingA = wingGo.transform;
                    }
                }
                else
                {
                    // Multi-layer Beetle with 2 separately flapping translucent wings behind carapace
                    var bodyGo = new GameObject("BeetleBody", typeof(SpriteRenderer));
                    bodyGo.transform.SetParent(root.transform, false);
                    var bsr = bodyGo.GetComponent<SpriteRenderer>();
                    bsr.sprite = beetleBodySprite ?? beeBodySprite ?? white;
                    bsr.sortingOrder = 6;
                    DreamForestAtlasCatalog.FitSprite(bodyGo.transform, bsr.sprite, o.W / 100f, o.H / 100f);

                    if (beetleWingSprite != null)
                    {
                        for (int wIdx = 0; wIdx < 2; wIdx++)
                        {
                            var wingGo = new GameObject("BeetleWing_" + wIdx, typeof(SpriteRenderer));
                            wingGo.transform.SetParent(root.transform, false);
                            var wsr = wingGo.GetComponent<SpriteRenderer>();
                            wsr.sprite = beetleWingSprite;
                            wsr.sortingOrder = 5;
                            wsr.color = new Color(0.92f, 0.96f, 1f, 0.66f + wIdx * 0.12f);
                            float ww = o.W * 0.65f / 100f;
                            float wh = ww * (490f / 738f);
                            DreamForestAtlasCatalog.FitSprite(wingGo.transform, beetleWingSprite, ww, wh);
                            wingGo.transform.localPosition = new Vector3((0.05f + wIdx * 0.04f) * o.W / 100f, (0.12f - wIdx * 0.03f) * o.H / 100f, 0.02f);
                            if (wIdx == 0) entry.WingA = wingGo.transform;
                            else entry.WingB = wingGo.transform;
                        }
                    }
                }

                dreamObjects.Add(entry);
            }

            // Launch leaf perched on branch rest-5 until Yuna glides
            if (launchLeafSprite != null)
            {
                var llGo = new GameObject("DreamLaunchLeaf", typeof(SpriteRenderer));
                llGo.transform.SetParent(dreamRoot.transform, false);
                var lsr = llGo.GetComponent<SpriteRenderer>();
                lsr.sprite = launchLeafSprite;
                lsr.sortingOrder = 7;
                DreamForestAtlasCatalog.FitSprite(llGo.transform, launchLeafSprite, 1.6f, 0.62f);
                launchLeafTransform = llGo.transform;
            }

            // Glider thorns with tiled 80px base-bark strips extending off-screen (no duplicated tips)
            for (int i = 0; i < level.GlideThorns.Length; i++)
            {
                var th = level.GlideThorns[i];
                var group = new GameObject("GliderThorn_" + i);
                group.transform.SetParent(dreamRoot.transform, false);

                var mainGo = new GameObject("Tip", typeof(SpriteRenderer));
                mainGo.transform.SetParent(group.transform, false);
                var sr = mainGo.GetComponent<SpriteRenderer>();
                sr.sprite = thornSprite ?? white;
                sr.sortingOrder = 5;
                sr.flipY = th.Side > 0;
                mainGo.transform.position = DreamForestAtlasCatalog.ToUnity(th.X + th.W * 0.5f, th.Y + th.H * 0.5f, 0f);
                DreamForestAtlasCatalog.FitSprite(mainGo.transform, sr.sprite, th.W / 100f, th.H / 100f);

                // Tile 80px base strip outward toward top (Side < 0) or bottom (Side > 0)
                Sprite stripSprite = thornBaseStrip ?? thornSprite ?? white;
                float stripH = Mathf.Max(0.45f, (80f * th.H / 1536f) / 100f) * 3.0f;
                float startY = th.Side < 0 ? th.Y : (th.Y + th.H);
                float dir = th.Side < 0 ? -1f : 1f;
                for (int k = 0; k < 5; k++)
                {
                    var extGo = new GameObject("RootStrip_" + k, typeof(SpriteRenderer));
                    extGo.transform.SetParent(group.transform, false);
                    var esr = extGo.GetComponent<SpriteRenderer>();
                    esr.sprite = stripSprite;
                    esr.sortingOrder = 4;
                    esr.flipY = (th.Side > 0) ^ ((k & 1) == 0);
                    float py = startY + dir * (k + 0.5f) * (stripH * 100f);
                    extGo.transform.position = DreamForestAtlasCatalog.ToUnity(th.X + th.W * 0.5f, py, 0.04f);
                    DreamForestAtlasCatalog.FitSprite(extGo.transform, stripSprite, th.W / 100f, stripH + 0.02f);
                }

                thornObjects.Add(group);
            }
        }

        private void BuildNightmareAct2(DreamForestLevelData level)
        {
            nightmareRoot = new GameObject("DreamAct2NightmareScenery");
            nightmareRoot.transform.SetParent(transform, false);

            Sprite floorSprite = DreamForestAtlasCatalog.FullSprite("floor-forest-dark-v1", new Vector2(0f, 1f));
            Sprite hillSprite = DreamForestAtlasCatalog.FullSprite("hill-soil-v1", new Vector2(0.5f, 1f));
            Sprite shelterSprite0 = DreamForestAtlasCatalog.SliceGrid("shelters-forest-v2", 2, 2, 0, new Vector2(0.5f, 0f));
            Sprite shelterSprite1 = DreamForestAtlasCatalog.SliceGrid("shelters-forest-v2", 2, 2, 1, new Vector2(0.5f, 0f));
            Sprite mudSprite = DreamForestAtlasCatalog.FullSprite("lakes-mud-v2", new Vector2(0.5f, 0.5f));
            Sprite splashSprite = DreamForestAtlasCatalog.FullSprite("splash-mud-v1", new Vector2(0.5f, 0f));
            Sprite roachSprite = DreamForestAtlasCatalog.FullSprite("roach-nightmare-v1", new Vector2(0.5f, 0.5f));
            Sprite white = DreamForestAtlasCatalog.SolidWhiteSprite();

            // Repeating dark forest floor tiles
            for (float x = -400f; x < DreamForestLevelData.StealthWidth + 800f; x += 1200f)
            {
                var floorGo = new GameObject("DarkFloorTile", typeof(SpriteRenderer));
                floorGo.transform.SetParent(nightmareRoot.transform, false);
                var sr = floorGo.GetComponent<SpriteRenderer>();
                sr.sprite = floorSprite ?? white;
                sr.color = new Color(0.20f, 0.19f, 0.28f, 1f);
                sr.sortingOrder = 0;
                floorGo.transform.position = DreamForestAtlasCatalog.ToUnity(x, DreamForestLevelData.StealthGroundY - 30f, 0.5f);
                DreamForestAtlasCatalog.FitSprite(floorGo.transform, sr.sprite, 12.2f, 4.2f);
            }

            // Swamp background & foreground tree trunks wrapping around the roach's legs (matching swampLayers)
            const float swampStride = 1450f;
            for (float sx = 0f; sx < DreamForestLevelData.StealthWidth + 800f; sx += swampStride)
            {
                if (shelterSprite0 != null)
                {
                    var backTrunk = new GameObject("SwampBackTrunk", typeof(SpriteRenderer));
                    backTrunk.transform.SetParent(nightmareRoot.transform, false);
                    var bsr = backTrunk.GetComponent<SpriteRenderer>();
                    bsr.sprite = shelterSprite0;
                    bsr.color = new Color(0.55f, 0.58f, 0.70f, 0.58f);
                    bsr.sortingOrder = 0;
                    backTrunk.transform.position = DreamForestAtlasCatalog.ToUnity(sx + 240f, 610f, 1.4f);
                    DreamForestAtlasCatalog.FitSprite(backTrunk.transform, shelterSprite0, 3.3f, 4.2f);
                }
                if (shelterSprite1 != null)
                {
                    var frontTrunk = new GameObject("SwampFrontTrunk", typeof(SpriteRenderer));
                    frontTrunk.transform.SetParent(nightmareRoot.transform, false);
                    var fsr = frontTrunk.GetComponent<SpriteRenderer>();
                    fsr.sprite = shelterSprite1;
                    fsr.color = new Color(0.65f, 0.68f, 0.78f, 0.78f);
                    fsr.sortingOrder = 2;
                    frontTrunk.transform.position = DreamForestAtlasCatalog.ToUnity(sx + 1080f, 760f, 0.9f);
                    DreamForestAtlasCatalog.FitSprite(frontTrunk.transform, shelterSprite1, 4.3f, 4.8f);
                }
            }

            // Soil hills
            for (int i = 0; i < DreamForestLevelData.StealthPlatforms.Length; i++)
            {
                var p = DreamForestLevelData.StealthPlatforms[i];
                var hillGo = new GameObject("SoilHill_" + i, typeof(SpriteRenderer));
                hillGo.transform.SetParent(nightmareRoot.transform, false);
                var sr = hillGo.GetComponent<SpriteRenderer>();
                sr.sprite = hillSprite ?? white;
                sr.color = new Color(0.76f, 0.78f, 0.84f, 1f);
                sr.sortingOrder = 2;
                float cx = (p.From + p.To) * 0.5f;
                hillGo.transform.position = DreamForestAtlasCatalog.ToUnity(cx, p.Y, 0.3f);
                DreamForestAtlasCatalog.FitSprite(hillGo.transform, sr.sprite, (p.To - p.From + 160f) / 100f, 2.2f);
            }

            // Mud lakes (back layer + foreground rim occluding submerged Yuna)
            for (int i = 0; i < level.MudLakes.Length; i++)
            {
                var lake = level.MudLakes[i];
                var mudGo = new GameObject(lake.Id, typeof(SpriteRenderer));
                mudGo.transform.SetParent(nightmareRoot.transform, false);
                var sr = mudGo.GetComponent<SpriteRenderer>();
                sr.sprite = DreamForestAtlasCatalog.SliceGrid("lakes-mud-v2", 2, 1, i % 2, new Vector2(0.5f, 0.5f)) ?? mudSprite ?? white;
                sr.sortingOrder = 12;
                sr.color = new Color(0.84f, 0.80f, 0.78f, 0.92f);
                mudGo.transform.position = DreamForestAtlasCatalog.ToUnity(lake.PaintX + lake.W * 0.5f, lake.Top + lake.H * 0.5f, -0.1f);
                DreamForestAtlasCatalog.FitSprite(mudGo.transform, sr.sprite, lake.W / 100f, lake.H / 100f);
            }

            // Mud splash burst & swim ripples
            var splashGo = new GameObject("MudSplashBurst", typeof(SpriteRenderer));
            splashGo.transform.SetParent(nightmareRoot.transform, false);
            mudSplashRenderer = splashGo.GetComponent<SpriteRenderer>();
            mudSplashRenderer.sprite = splashSprite ?? radialGlowSprite;
            mudSplashRenderer.sortingOrder = 13;
            splashGo.SetActive(false);

            var rippleGo = new GameObject("MudSwimRipple", typeof(SpriteRenderer));
            rippleGo.transform.SetParent(nightmareRoot.transform, false);
            mudRippleRenderer = rippleGo.GetComponent<SpriteRenderer>();
            mudRippleRenderer.sprite = radialGlowSprite;
            mudRippleRenderer.color = new Color(0.62f, 0.68f, 0.64f, 0.45f);
            mudRippleRenderer.sortingOrder = 13;
            rippleGo.SetActive(false);

            // Rocks & tree trunks providing real shadow cover + ground contact shadows
            for (int i = 0; i < level.StealthCovers.Length; i++)
            {
                var c = level.StealthCovers[i];

                var shGo = new GameObject(c.Id + "_Shadow", typeof(SpriteRenderer));
                shGo.transform.SetParent(nightmareRoot.transform, false);
                var shSr = shGo.GetComponent<SpriteRenderer>();
                shSr.sprite = radialGlowSprite;
                shSr.color = new Color(0f, 0f, 0f, 0.48f);
                shSr.sortingOrder = 3;
                shGo.transform.position = DreamForestAtlasCatalog.ToUnity(c.X, c.BaseY + 4f, 0.25f);
                shGo.transform.localScale = new Vector3(c.W * 0.96f / 100f, 0.26f, 1f);

                var coverGo = new GameObject(c.Id, typeof(SpriteRenderer));
                coverGo.transform.SetParent(nightmareRoot.transform, false);
                var sr = coverGo.GetComponent<SpriteRenderer>();
                if (c.IsRock)
                {
                    int rIdx = ((c.Frame % 3) + 3) % 3;
                    sr.sprite = DreamForestAtlasCatalog.SliceRect("rocks-nightmare-v1", DreamForestAtlasCatalog.NightmareRockRects[rIdx], new Vector2(0.5f, 0f));
                }
                else
                {
                    sr.sprite = DreamForestAtlasCatalog.SliceGrid("shelters-forest-v2", 2, 2, c.Frame, new Vector2(0.5f, 0f)) ?? shelterSprite0;
                }
                sr.sortingOrder = 4;
                coverGo.transform.position = DreamForestAtlasCatalog.ToUnity(c.X, c.BaseY, 0.2f);
                DreamForestAtlasCatalog.FitSprite(coverGo.transform, sr.sprite, c.W / 100f, c.H / 100f);
            }

            // Giant Nightmare Cockroach
            var roachGo = new GameObject("GiantNightmareRoach", typeof(SpriteRenderer));
            roachGo.transform.SetParent(nightmareRoot.transform, false);
            roachRenderer = roachGo.GetComponent<SpriteRenderer>();
            roachRenderer.sprite = roachSprite ?? white;
            roachRenderer.sortingOrder = 1;
            roachRenderer.color = new Color(0.84f, 0.78f, 0.86f, 0.94f);

            // Two spotlight beam cones + eye glow flares + ground hit pools
            spotlightMat = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(1.0f, 0.88f, 0.56f, 0.26f),
            };
            for (int i = 0; i < 2; i++)
            {
                var beamGo = new GameObject("RoachSpotlight_" + i, typeof(MeshFilter), typeof(MeshRenderer));
                beamGo.transform.SetParent(nightmareRoot.transform, false);
                var mr = beamGo.GetComponent<MeshRenderer>();
                mr.sharedMaterial = spotlightMat;
                mr.sortingOrder = 14;
                spotlightFilters.Add(beamGo.GetComponent<MeshFilter>());

                var flareGo = new GameObject("RoachEyeFlare_" + i, typeof(SpriteRenderer));
                flareGo.transform.SetParent(nightmareRoot.transform, false);
                var fsr = flareGo.GetComponent<SpriteRenderer>();
                fsr.sprite = radialGlowSprite;
                fsr.color = new Color(1f, 0.92f, 0.62f, 0.85f);
                fsr.sortingOrder = 15;
                flareGo.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
                eyeFlares.Add(fsr);

                var poolGo = new GameObject("SpotlightGroundPool_" + i, typeof(SpriteRenderer));
                poolGo.transform.SetParent(nightmareRoot.transform, false);
                var psr = poolGo.GetComponent<SpriteRenderer>();
                psr.sprite = radialGlowSprite;
                psr.color = new Color(1f, 0.88f, 0.58f, 0.36f);
                psr.sortingOrder = 13;
                poolGo.transform.localScale = new Vector3(2.2f, 0.45f, 1f);
                groundPools.Add(psr);
            }

            nightmareRoot.SetActive(false);
        }

        public void Apply(DreamForestSimulation sim)
        {
            bool isNightmare = sim.Nightmare != null;
            bool isWake = sim.Stage == DreamForestStage.NightmareWake || sim.Stage == DreamForestStage.Complete;
            bool isLanding = sim.Stage == DreamForestStage.GlideLanding;

            dreamRoot.SetActive(!isNightmare && !isLanding && sim.Stage != DreamForestStage.Opening);
            nightmareRoot.SetActive(isNightmare && !isWake);

            bool showAct1Atmosphere = !isNightmare && !isLanding && sim.Stage != DreamForestStage.Opening;
            if (lowerMistRenderer != null) lowerMistRenderer.gameObject.SetActive(showAct1Atmosphere);
            for (int i = 0; i < fireflies.Length; i++)
            {
                if (fireflies[i] != null) fireflies[i].gameObject.SetActive(showAct1Atmosphere);
            }

            UpdateCameraAndBackdrops(sim, isNightmare, isWake, isLanding);

            if (showAct1Atmosphere)
            {
                UpdateDreamAct1Objects(sim);
            }
            else if (isNightmare && !isWake)
            {
                UpdateNightmareAct2Objects(sim);
            }
        }

        private void UpdateDreamAct1Objects(DreamForestSimulation sim)
        {
            float px = sim.Player.X;
            for (int i = 0; i < dreamObjects.Count; i++)
            {
                var entry = dreamObjects[i];
                var f = DreamForestLevelData.EvaluateObjectFrame(entry.Spec, sim.Time, sim.Rides);
                bool near = f.X + f.W >= px - 1600f && f.X <= px + 1800f;
                if (entry.Root.activeSelf != near) entry.Root.SetActive(near);
                if (entry.TrunkTransform != null && entry.TrunkTransform.gameObject.activeSelf != near)
                {
                    entry.TrunkTransform.gameObject.SetActive(near);
                }
                if (!near) continue;

                if (entry.Spec.Kind == DreamForestObjectKind.Grass)
                {
                    entry.Root.transform.position = Vector3.zero;
                    entry.Root.transform.localRotation = Quaternion.identity;
                    if (entry.GrassMeshFilter != null)
                    {
                        UpdateGrassRibbonMesh(entry.GrassMeshFilter, f);
                    }
                }
                else if (entry.Spec.Kind == DreamForestObjectKind.Web)
                {
                    entry.Root.transform.position = DreamForestAtlasCatalog.ToUnity(f.X, f.Y, 0.1f);
                    if (entry.SpiderTransform != null)
                    {
                        entry.SpiderTransform.position = DreamForestAtlasCatalog.ToUnity(f.X, f.SpiderY + f.Spec.SpiderH * 0.5f, 0f);
                        float legBob = Mathf.Sin(sim.Time * 4.4f + f.Spec.Phase) * 3.5f;
                        entry.SpiderTransform.localRotation = Quaternion.Euler(0f, 0f, legBob);
                    }
                }
                else if (entry.Spec.Kind == DreamForestObjectKind.Leaf || entry.Spec.Kind == DreamForestObjectKind.Plateau)
                {
                    entry.Root.transform.position = DreamForestAtlasCatalog.ToUnity(f.X + f.W * 0.5f, f.Y, 0.1f);
                }
                else if (entry.Spec.Kind == DreamForestObjectKind.Branch)
                {
                    entry.Root.transform.position = DreamForestAtlasCatalog.ToUnity(f.X + f.W * 0.5f, f.Y, 0.1f);
                    if (entry.TrunkTransform != null)
                    {
                        entry.TrunkTransform.position = DreamForestAtlasCatalog.ToUnity(f.Spec.X - 45f, f.Y + 40f, 0.15f);
                    }
                }
                else if (entry.Spec.Kind == DreamForestObjectKind.Bee)
                {
                    entry.Root.transform.position = DreamForestAtlasCatalog.ToUnity(f.X + f.W * 0.5f, f.Y + f.H * 0.5f, 0f);
                    if (entry.WingA != null)
                    {
                        // Exact dreamBeeWingPose (7 Hz beat)
                        float beat = Mathf.Sin(sim.Time * Mathf.PI * 2f * 7f + f.Spec.Phase);
                        float angleDeg = (-0.15f + beat * 0.14f) * Mathf.Rad2Deg;
                        float spread = 0.22f + (beat + 1f) * 0.36f;
                        entry.WingA.localRotation = Quaternion.Euler(0f, 0f, -angleDeg);
                        Vector3 baseScale = entry.WingA.localScale;
                        entry.WingA.localScale = new Vector3(baseScale.x, Mathf.Sign(baseScale.y == 0f ? 1f : baseScale.y) * Mathf.Abs(baseScale.x) * (464f / 806f) * spread, 1f);
                    }
                }
                else
                {
                    // Multi-layer Beetle with energy-driven flapping wings
                    entry.Root.transform.position = DreamForestAtlasCatalog.ToUnity(f.X + f.W * 0.5f, f.Y + f.H * 0.5f, 0f);
                    float energy = f.FlightEnergy;
                    float beatFreq = 4f + energy * 3f;
                    float beat = sim.Time * Mathf.PI * 2f * beatFreq;
                    if (entry.WingA != null)
                    {
                        float w0 = Mathf.Sin(beat);
                        entry.WingA.localRotation = Quaternion.Euler(0f, 0f, (0.16f - w0 * 0.18f * energy) * Mathf.Rad2Deg);
                    }
                    if (entry.WingB != null)
                    {
                        float w1 = Mathf.Sin(beat + 0.85f);
                        entry.WingB.localRotation = Quaternion.Euler(0f, 0f, (-0.06f - w1 * 0.18f * energy) * Mathf.Rad2Deg);
                    }
                }
            }

            // Swaying launch leaf perched on rest-5 before glider launch
            if (launchLeafTransform != null)
            {
                bool showLaunchLeaf = sim.Glider == null || sim.Stage == DreamForestStage.GlideApproach;
                launchLeafTransform.gameObject.SetActive(showLaunchLeaf);
                if (showLaunchLeaf && sim.Level.ById.TryGetValue("rest-5", out var bSpec))
                {
                    var bf = DreamForestLevelData.EvaluateObjectFrame(bSpec, sim.Time, sim.Rides);
                    launchLeafTransform.position = DreamForestAtlasCatalog.ToUnity(bf.X + bf.W * 0.84f, bf.Y - 26f, -0.02f);
                    launchLeafTransform.localRotation = Quaternion.Euler(0f, 0f, (0.15f - Mathf.Sin(sim.Time * 2f) * 0.025f) * Mathf.Rad2Deg);
                }
            }

            for (int i = 0; i < thornObjects.Count; i++)
            {
                var th = sim.Level.GlideThorns[i];
                bool near = th.X + th.W >= px - 1400f && th.X <= px + 1600f;
                if (thornObjects[i].activeSelf != near) thornObjects[i].SetActive(near);
            }

            // Animate 14 glowing fireflies in camera space
            float halfH = stageCamera != null ? stageCamera.orthographicSize : 4.6f;
            float halfW = halfH * (stageCamera != null ? stageCamera.aspect : 1.77f);
            float spanW = halfW * 2f;
            for (int i = 0; i < fireflies.Length; i++)
            {
                if (fireflies[i] == null) continue;
                float rawX = ((i * 1.39f - px * 0.0006f + Mathf.Sin(sim.Time * 0.4f + i) * 0.15f) % spanW + spanW) % spanW;
                float fx = rawX - halfW;
                float fy = halfH - (0.18f + (i % 6) * 0.12f) * (halfH * 2f);
                fireflies[i].transform.localPosition = new Vector3(fx, fy, 11f);
                float alpha = 0.25f + 0.25f * Mathf.Sin(sim.Time + i);
                fireflies[i].color = new Color(0.72f, 0.85f, 0.84f, alpha);
            }
        }

        private void UpdateNightmareAct2Objects(DreamForestSimulation sim)
        {
            var n = sim.Nightmare;
            bool showRoach = n.RoachAppeared && sim.Stage != DreamForestStage.NightmareWake && sim.Stage != DreamForestStage.Complete;
            roachRenderer.gameObject.SetActive(showRoach);
            if (showRoach)
            {
                sim.EvaluateRoachPatrol(out float rx, out float ry, out float rScale, out _);
                float walkBob = Mathf.Sin(n.Time * Mathf.PI * 2f / DreamForestLevelData.RoachStepSeconds) * 8f;
                roachRenderer.transform.position = DreamForestAtlasCatalog.ToUnity(rx + 85f * rScale, ry + 15f * rScale + walkBob, 1.2f);
                DreamForestAtlasCatalog.FitSprite(roachRenderer.transform, roachRenderer.sprite, 13.5f * rScale, 9.0f * rScale);
            }

            sim.EvaluateSpotlights(spotlightBuffer);
            for (int i = 0; i < spotlightFilters.Count; i++)
            {
                bool active = i < spotlightBuffer.Count && showRoach;
                spotlightFilters[i].gameObject.SetActive(active);
                eyeFlares[i].gameObject.SetActive(active);
                groundPools[i].gameObject.SetActive(active);
                if (!active) continue;

                var light = spotlightBuffer[i];
                UpdateSpotlightMesh(spotlightFilters[i], sim, light, out Vector3 hitCenter, out bool hasHit);
                eyeFlares[i].transform.position = DreamForestAtlasCatalog.ToUnity(light.X, light.Y, -0.25f);
                groundPools[i].gameObject.SetActive(hasHit);
                if (hasHit)
                {
                    groundPools[i].transform.position = hitCenter;
                }
            }

            // Mud splash burst on impact (MudMode == Enter && MudAge >= 0.66)
            bool showSplash = n.MudMode == DreamForestMudMode.Enter && n.MudAge >= 0.66f;
            mudSplashRenderer.gameObject.SetActive(showSplash);
            if (showSplash)
            {
                var lake = FindLake(sim.Level, n.MudLakeId);
                if (lake != null)
                {
                    float q = Mathf.Clamp01((n.MudAge - 0.66f) / 0.44f);
                    float impact = DreamForestLevelData.SmoothStep(0.68f / 1.1f);
                    float sx = n.MudStartX + (n.MudEntryTarget - n.MudStartX) * impact;
                    float sy = DreamForestLevelData.MudWaterline(lake, sx);
                    float w = 2.4f * (0.55f + 0.45f * DreamForestLevelData.SmoothStep(q));
                    mudSplashRenderer.transform.position = DreamForestAtlasCatalog.ToUnity(sx, sy + 20f, -0.15f);
                    DreamForestAtlasCatalog.FitSprite(mudSplashRenderer.transform, mudSplashRenderer.sprite, w, w * 0.68f);
                    mudSplashRenderer.color = new Color(0.88f, 0.84f, 0.78f, Mathf.Pow(Mathf.Sin(Mathf.PI * q), 0.65f));
                }
            }

            // Animated swim ripple ring
            bool showRipple = sim.Stage == DreamForestStage.Stealth && n.MudMode == DreamForestMudMode.Swim;
            mudRippleRenderer.gameObject.SetActive(showRipple);
            if (showRipple)
            {
                var lake = FindLake(sim.Level, n.MudLakeId);
                if (lake != null)
                {
                    float sy = DreamForestLevelData.MudWaterline(lake, n.X);
                    float stroke = 1f + Mathf.Min(1f, Mathf.Abs(n.Vx) / 78f);
                    mudRippleRenderer.transform.position = DreamForestAtlasCatalog.ToUnity(n.X - n.Facing * 10f, sy + 6f, -0.14f);
                    mudRippleRenderer.transform.localScale = new Vector3((0.9f * stroke + Mathf.Sin(n.Time * 3f) * 0.08f), 0.16f, 1f);
                }
            }
        }

        private void UpdateCameraAndBackdrops(DreamForestSimulation sim, bool isNightmare, bool isWake, bool isLanding)
        {
            if (stageCamera == null) return;
            bool isGliding = sim.Stage == DreamForestStage.Gliding;
            float worldW = (isNightmare ? DreamForestLevelData.StealthWidth : sim.Level.WorldWidth) / 100f;
            float px = (isNightmare ? sim.Nightmare.X : sim.Player.X) / 100f;
            float py = (450f - (isNightmare ? sim.Nightmare.Y : (sim.Glider != null ? sim.Glider.CameraY : 450f))) / 100f;
            int facing = isNightmare ? sim.Nightmare.Facing : sim.Player.Facing;

            // Roach step screen shake during Nightmare
            Vector2 shake = Vector2.zero;
            if (isNightmare && !isWake && sim.Nightmare.RoachAppeared)
            {
                float stepPhase = (sim.Nightmare.Time % DreamForestLevelData.RoachStepSeconds) / DreamForestLevelData.RoachStepSeconds;
                float pulse = stepPhase < 0.18f ? Mathf.Sin((stepPhase / 0.18f) * Mathf.PI) * 0.045f : 0f;
                if (sim.Stage == DreamForestStage.NightmareStrike && sim.Age >= 0.72f)
                {
                    pulse += Mathf.Max(0f, 1f - (sim.Age - 0.72f) / 0.45f) * 0.14f;
                }
                shake = new Vector2(Mathf.Sin(sim.Nightmare.Time * 42f) * pulse, Mathf.Cos(sim.Nightmare.Time * 37f) * pulse);
            }

            bool isMobile = mobileOverride ?? Application.isMobilePlatform;
            if (isMobile)
            {
                var frame = DreamForestMobileCameraFraming.Evaluate(px, py, worldW, stageCamera.aspect, facing, isNightmare, isGliding);
                stageCamera.orthographicSize = frame.OrthoSize;
                stageCamera.transform.position = new Vector3(frame.CenterX + shake.x, frame.CenterY + shake.y, -10f);
            }
            else
            {
                stageCamera.orthographicSize = isGliding ? 5.2f : 4.6f;
                float halfW = stageCamera.orthographicSize * stageCamera.aspect;
                float cx = Mathf.Clamp(px + 1.8f, halfW, Mathf.Max(halfW, worldW - halfW));
                float cy = isGliding ? py : (isNightmare ? -0.45f : 0f);
                stageCamera.transform.position = new Vector3(cx + shake.x, cy + shake.y, -10f);
            }

            // Update parallax backdrops
            Sprite chosenBg = isWake
                ? (picnicWakeBgSprite ?? dreamBgSprite)
                : (isLanding || isNightmare ? (nightmareBgSprite ?? dreamBgSprite) : dreamBgSprite);
            Color bgTint = isWake
                ? Color.white
                : (isNightmare ? new Color(0.28f, 0.28f, 0.42f, 1f) : Color.white);

            if (backdropTiles != null && chosenBg != null)
            {
                float h = stageCamera.orthographicSize * 2.18f;
                float natAspect = chosenBg.bounds.size.x / Mathf.Max(0.001f, chosenBg.bounds.size.y);
                float w = h * Mathf.Max(stageCamera.aspect, natAspect);
                bool parallax = !isWake && !isLanding;
                // Overlapping feathered tiles at step = 0.74 * w, parallax = x * 0.1, zero flipX mirroring
                float step = w * 0.74f;
                float scroll = parallax ? ((px * 0.10f) % step + step) % step : 0f;

                for (int i = 0; i < backdropTiles.Length; i++)
                {
                    var sr = backdropTiles[i];
                    sr.sprite = chosenBg;
                    sr.color = bgTint;
                    sr.flipX = false;
                    sr.gameObject.SetActive(i == 0 || parallax);
                    DreamForestAtlasCatalog.FitSprite(sr.transform, chosenBg, w, h);
                    sr.transform.localPosition = new Vector3(parallax ? (-scroll + (i - 1) * step) : 0f, 0f, 18f);
                }

                if (lowerMistRenderer != null)
                {
                    float camW = stageCamera.orthographicSize * stageCamera.aspect * 2.1f;
                    float mistH = stageCamera.orthographicSize * 0.64f;
                    DreamForestAtlasCatalog.FitSprite(lowerMistRenderer.transform, lowerMistRenderer.sprite, camW, mistH);
                    lowerMistRenderer.transform.localPosition = new Vector3(0f, -stageCamera.orthographicSize + mistH * 0.5f, 12f);
                }
            }
        }

        private static void UpdateSpotlightMesh(MeshFilter mf, DreamForestSimulation sim, DreamForestSpotlight light, out Vector3 hitCenter, out bool hasHit)
        {
            const int Segs = 20;
            Mesh mesh = mf.sharedMesh;
            if (mesh == null)
            {
                mesh = new Mesh { name = "RoachSpotlightCone" };
                mf.sharedMesh = mesh;
            }

            var verts = new Vector3[Segs + 2];
            var tris = new int[Segs * 3];
            verts[0] = DreamForestAtlasCatalog.ToUnity(light.X, light.Y, -0.2f);
            hitCenter = Vector3.zero;
            hasHit = false;

            for (int i = 0; i <= Segs; i++)
            {
                float ang = light.AngleRad - light.HalfAngleRad + (2f * light.HalfAngleRad * i) / Segs;
                float dist = SurfaceClampedRayDistance(sim, light, ang);
                float ex = light.X + Mathf.Cos(ang) * dist;
                float ey = light.Y + Mathf.Sin(ang) * dist;
                verts[i + 1] = DreamForestAtlasCatalog.ToUnity(ex, ey, -0.2f);
                if (i == Segs / 2 && dist < DreamForestLevelData.NightmareLightRange * 0.98f)
                {
                    hitCenter = DreamForestAtlasCatalog.ToUnity(ex, ey, -0.18f);
                    hasHit = true;
                }
                if (i < Segs)
                {
                    tris[i * 3] = 0;
                    tris[i * 3 + 1] = i + 1;
                    tris[i * 3 + 2] = i + 2;
                }
            }
            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
        }

        /// <summary>
        /// Clamps each spotlight ray to BOTH rock/trunk covers AND the ground/mud waterline
        /// (matching DreamNightmareLighting.surfaceEnd so beams never pierce underground).
        /// </summary>
        private static float SurfaceClampedRayDistance(DreamForestSimulation sim, DreamForestSpotlight light, float angleRad)
        {
            float coverDist = sim.RaycastCoverDistance(light, angleRad);
            float dx = Mathf.Cos(angleRad);
            float dy = Mathf.Sin(angleRad);
            if (dy <= 0.01f) return coverDist;

            for (float d = 16f; d <= coverDist; d += 12f)
            {
                float px = light.X + dx * d;
                float py = light.Y + dy * d;
                float surf = SurfaceHeightAt(sim.Level, px);
                if (py >= surf)
                {
                    return Mathf.Min(coverDist, d);
                }
            }
            return coverDist;
        }

        private static float SurfaceHeightAt(DreamForestLevelData level, float x)
        {
            for (int i = 0; i < level.MudLakes.Length; i++)
            {
                var lake = level.MudLakes[i];
                if (x >= lake.From && x <= lake.To)
                {
                    return DreamForestLevelData.MudWaterline(lake, x);
                }
            }
            return level.StealthGroundYAt(x);
        }

        private static DreamForestMudLakeSpec FindLake(DreamForestLevelData level, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < level.MudLakes.Length; i++)
            {
                if (level.MudLakes[i].Id == id) return level.MudLakes[i];
            }
            return null;
        }

        private static void UpdateGrassRibbonMesh(MeshFilter mf, DreamForestObjectFrame f)
        {
            const int BladeSegs = 20;
            const int StemSegs = 12;
            const int TotalSegs = BladeSegs + StemSegs;
            Mesh mesh = mf.sharedMesh;
            if (mesh == null)
            {
                mesh = new Mesh { name = "GrassRibbon_" + f.Spec.Id };
                mf.sharedMesh = mesh;
            }

            var verts = new Vector3[(TotalSegs + 1) * 2];
            var uvs = new Vector2[(TotalSegs + 1) * 2];
            var tris = new int[TotalSegs * 6];

            float topY = f.Y;
            float midY = f.Y + f.H;
            float botY = Mathf.Max(midY + 200f, DreamForestLevelData.GrassRoot);
            float bladeMaxW = 42f + Mathf.Sin(f.Spec.Phase) * 5f;
            float bend = 5f + Mathf.Sin(f.Spec.Phase) * 3f;

            for (int i = 0; i <= TotalSegs; i++)
            {
                float y;
                float cx;
                float wPx;
                float v;
                if (i <= BladeSegs)
                {
                    float u = i / (float)BladeSegs; // 0 = tip, 1 = blade base
                    y = Mathf.Lerp(topY, midY, u);
                    cx = DreamForestLevelData.GrassX(f, y);
                    // Smooth taper from pointed tip (3px) -> belly (42px) -> smooth junction to stem (10px)
                    float tipShape = Mathf.Sin(Mathf.Pow(u, 0.72f) * Mathf.PI * 0.88f);
                    wPx = Mathf.Lerp(3f, bladeMaxW, tipShape);
                    if (u > 0.78f)
                    {
                        float jt = (u - 0.78f) / 0.22f;
                        wPx = Mathf.Lerp(wPx, 10f, jt * jt);
                    }
                    v = Mathf.Lerp(1f, 0.35f, u);
                }
                else
                {
                    float s = (i - BladeSegs) / (float)StemSegs; // 0 = blade base, 1 = GrassRoot
                    y = Mathf.Lerp(midY, botY, s);
                    float below = Mathf.Max(0f, y - midY);
                    cx = f.X + Mathf.Sin(Mathf.Min(1f, below / 1200f) * Mathf.PI) * bend;
                    wPx = 10f + Mathf.Min(1f, below / 1000f) * 12f; // widens downward 10 -> 22px
                    v = Mathf.Lerp(0.35f, 0f, s);
                }

                float halfW = wPx * 0.5f;
                verts[i * 2] = DreamForestAtlasCatalog.ToUnity(cx - halfW, y, 0.2f);
                verts[i * 2 + 1] = DreamForestAtlasCatalog.ToUnity(cx + halfW, y, 0.2f);
                uvs[i * 2] = new Vector2(0f, v);
                uvs[i * 2 + 1] = new Vector2(1f, v);

                if (i < TotalSegs)
                {
                    int v0 = i * 2;
                    int ti = i * 6;
                    tris[ti] = v0;
                    tris[ti + 1] = v0 + 1;
                    tris[ti + 2] = v0 + 2;
                    tris[ti + 3] = v0 + 1;
                    tris[ti + 4] = v0 + 3;
                    tris[ti + 5] = v0 + 2;
                }
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (spotlightMat != null) Destroy(spotlightMat);
            if (grassRibbonMat != null) Destroy(grassRibbonMat);
        }
    }
}
