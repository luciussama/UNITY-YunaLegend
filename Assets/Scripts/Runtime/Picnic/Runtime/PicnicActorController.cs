using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// 1:1 faithful Unity renderer of Yuna (all 4 normal + 4 fear-v2 spritesheets),
    /// approved blue Nanda, and all legacy creatures (ants, green/red caterpillars, bees, worms, chasing beetles).
    /// </summary>
    public sealed class PicnicActorController : MonoBehaviour
    {
        private CharacterController body;
        private Transform yunaRoot;
        private SpriteRenderer yunaRenderer;
        private PicnicYunaRigView yunaRigView;
        private float lastStridePlayerX;
        private bool hasLastStrideX;
        private float accumulatedStridePx;
        private Transform yunaShadow;
        private Material yunaShadowMat;
        private Texture2D yunaShadowTexture;
        private const float YunaArtScale = 1.15f;
        private SpriteRenderer[] sweatDrops;
        private Transform nandaRoot;
        private SpriteRenderer nandaRenderer;

        private SpriteRenderer[] enemyRenderers;
        private Transform[] enemyRoots;
        private Sprite[] antSprites;
        private Sprite[] greenCatSprites;
        private Sprite[] redCatSprites;

        private Transform[] beeRoots;
        private SpriteRenderer[] beeBodies;
        private SpriteRenderer[] beeWings;

        private Transform[] wormRoots;
        private SpriteRenderer[] wormRenderers;

        private Transform beetleRoot;
        private SpriteRenderer beetleBody;
        private SpriteRenderer beetleWings;

        public void Build(PicnicSimulation sim)
        {
            // Yuna with 3D CharacterController synced to Core simulation
            var yunaGo = new GameObject("YunaPlayer");
            yunaGo.transform.SetParent(transform, false);
            yunaRoot = yunaGo.transform;
            body = yunaGo.AddComponent<CharacterController>();
            body.height = 0.85f;
            body.radius = 0.16f;
            body.center = new Vector3(0f, 0.45f, 0f);
            body.skinWidth = 0.015f;
            // Core simulation is authoritative; keeping CharacterController disabled avoids per-frame PhysX actor rebuilds.
            body.enabled = false;

            var yunaArt = new GameObject("YunaSprite", typeof(SpriteRenderer));
            yunaArt.transform.SetParent(yunaRoot, false);
            yunaRenderer = yunaArt.GetComponent<SpriteRenderer>();
            yunaRenderer.sortingOrder = 12;
            yunaArt.transform.localScale = Vector3.one * YunaArtScale;

            var rigHost = new GameObject("YunaRigHost");
            rigHost.transform.SetParent(yunaRoot, false);
            rigHost.transform.localScale = Vector3.one * YunaArtScale;
            yunaRigView = rigHost.AddComponent<PicnicYunaRigView>();
            if (yunaRigView.TryBuild(12))
            {
                yunaRenderer.enabled = false;
            }
            else
            {
                yunaRigView = null;
            }

            // Soft 3D contact shadow disc on the 3D ground surface beneath Yuna's feet
            var shadowQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadowQuad.name = "YunaContactShadow3D";
            shadowQuad.transform.SetParent(yunaRoot, false);
            Destroy(shadowQuad.GetComponent<Collider>());
            shadowQuad.transform.localPosition = new Vector3(0f, 0.014f, 0.30f);
            shadowQuad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadowQuad.transform.localScale = new Vector3(0.72f * YunaArtScale, 0.34f * YunaArtScale, 1f);
            yunaShadowTexture = CreateContactShadowTexture();
            yunaShadowMat = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0.08f, 0.14f, 0.06f, 0.34f),
                mainTexture = yunaShadowTexture,
            };
            var shadowRenderer = shadowQuad.GetComponent<MeshRenderer>();
            shadowRenderer.sharedMaterial = yunaShadowMat;
            shadowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shadowRenderer.receiveShadows = false;
            yunaShadow = shadowQuad.transform;

            // 3 animated sweat droplets for grass/gravel sensory discomfort (GardenRenderer.effort)
            Sprite dropSprite = CreateSweatDropSprite();
            sweatDrops = new SpriteRenderer[3];
            for (int i = 0; i < sweatDrops.Length; i++)
            {
                var dGo = new GameObject("SweatDrop_" + i, typeof(SpriteRenderer));
                dGo.transform.SetParent(yunaRoot, false);
                var dr = dGo.GetComponent<SpriteRenderer>();
                dr.sprite = dropSprite;
                dr.sortingOrder = 16;
                dGo.transform.localScale = new Vector3(0.065f, 0.085f, 1f);
                dGo.SetActive(false);
                sweatDrops[i] = dr;
            }

            // Blue Nanda (approved illustrated blue fairy)
            var nandaGo = new GameObject("NandaBlue", typeof(SpriteRenderer));
            nandaGo.transform.SetParent(transform, false);
            nandaRoot = nandaGo.transform;
            nandaRenderer = nandaGo.GetComponent<SpriteRenderer>();
            nandaRenderer.sortingOrder = 14;

            // Ground & river enemies
            antSprites = new[]
            {
                PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[4], 100f, "en:ant0"),
                PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[5], 100f, "en:ant1"),
            };
            greenCatSprites = new[]
            {
                PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[6], 100f, "en:cat0"),
                PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[7], 100f, "en:cat1"),
            };
            redCatSprites = new[]
            {
                PicnicAtlasCatalog.Slice("caterpillar-red-v1", PicnicAtlasCatalog.RedCaterpillarFrames[0], 100f, "en:rcat0"),
                PicnicAtlasCatalog.Slice("caterpillar-red-v1", PicnicAtlasCatalog.RedCaterpillarFrames[1], 100f, "en:rcat1"),
            };

            enemyRoots = new Transform[sim.Enemies.Length];
            enemyRenderers = new SpriteRenderer[sim.Enemies.Length];
            for (int i = 0; i < sim.Enemies.Length; i++)
            {
                var go = new GameObject(sim.Enemies[i].Spec.Id, typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                enemyRoots[i] = go.transform;
                enemyRenderers[i] = go.GetComponent<SpriteRenderer>();
                enemyRenderers[i].sortingOrder = 10;
            }

            // Bees (separate body + flapping wings)
            var beeBodySpr = PicnicAtlasCatalog.Slice("bee-layers-v2", PicnicAtlasCatalog.BeeBodyFrame, 100f, "en:beebody");
            var beeWingSpr = PicnicAtlasCatalog.Slice("bee-layers-v2", PicnicAtlasCatalog.BeeWingsFrame, 100f, "en:beewings");
            beeRoots = new Transform[sim.Encounters.Bees.Length];
            beeBodies = new SpriteRenderer[sim.Encounters.Bees.Length];
            beeWings = new SpriteRenderer[sim.Encounters.Bees.Length];
            for (int i = 0; i < sim.Encounters.Bees.Length; i++)
            {
                var root = new GameObject(sim.Encounters.Bees[i].Id);
                root.transform.SetParent(transform, false);
                beeRoots[i] = root.transform;

                var wGo = new GameObject("Wings", typeof(SpriteRenderer));
                wGo.transform.SetParent(root.transform, false);
                var wr = wGo.GetComponent<SpriteRenderer>();
                wr.sprite = beeWingSpr;
                wr.sortingOrder = 10;
                FitSprite(wGo.transform, beeWingSpr, 0.52f, 0.27f);
                wGo.transform.localPosition = new Vector3(0f, 0.14f, 0.01f);
                beeWings[i] = wr;

                var bGo = new GameObject("Body", typeof(SpriteRenderer));
                bGo.transform.SetParent(root.transform, false);
                var br = bGo.GetComponent<SpriteRenderer>();
                br.sprite = beeBodySpr;
                br.sortingOrder = 11;
                FitSprite(bGo.transform, beeBodySpr, 0.62f, 0.39f);
                beeBodies[i] = br;
            }

            // Worms (Proposal A: burrowed inside the middle of the pebble bed with back dirt mound + front pebble collar)
            var wormSpr = PicnicAtlasCatalog.Slice("insects-v1", PicnicAtlasCatalog.InsectFrames[4], 100f, "en:worm");
            var wormFrame = PicnicAtlasCatalog.InsectFrames[4];
            var moundFrame = new AtlasFrameSpec(wormFrame.X, wormFrame.Y + Mathf.RoundToInt(wormFrame.H * 0.72f),
                wormFrame.W, Mathf.RoundToInt(wormFrame.H * 0.28f), wormFrame.W * 0.5f, Mathf.RoundToInt(wormFrame.H * 0.22f));
            var moundSpr = PicnicAtlasCatalog.Slice("insects-v1", moundFrame, 100f, "en:worm-mound-v2");

            wormRoots = new Transform[sim.Encounters.Worms.Length];
            wormRenderers = new SpriteRenderer[sim.Encounters.Worms.Length];
            for (int i = 0; i < sim.Encounters.Worms.Length; i++)
            {
                var w = sim.Encounters.Worms[i];
                var container = new GameObject(w.Id + "_Root");
                container.transform.SetParent(transform, false);
                // Seated right on the 3D sculpted pebble surface (w.Y + 2f in pixel space, Z = -0.10f)
                container.transform.position = PicnicAtlasCatalog.ToUnity(w.X, w.Y + 2f, -0.10f);

                var backMoundGo = new GameObject("BackDirtMound", typeof(SpriteRenderer));
                backMoundGo.transform.SetParent(container.transform, false);
                var bmr = backMoundGo.GetComponent<SpriteRenderer>();
                bmr.sprite = moundSpr;
                bmr.sortingOrder = 8;
                bmr.color = new Color(0.86f, 0.82f, 0.78f, 1f);
                backMoundGo.transform.localPosition = new Vector3(0f, 0.03f, 0.02f);
                FitSprite(backMoundGo.transform, moundSpr, 0.82f, 0.24f);

                var go = new GameObject(w.Id, typeof(SpriteRenderer));
                go.transform.SetParent(container.transform, false);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = wormSpr;
                sr.sortingOrder = 9;
                FitSprite(go.transform, wormSpr, 0.56f, 0.76f);
                go.SetActive(false);
                wormRoots[i] = go.transform;
                wormRenderers[i] = sr;

                // Front pebble & soil collar in front of the worm's lower base so it emerges from inside the pebbles
                var frontMoundGo = new GameObject("FrontPebbleCollar", typeof(SpriteRenderer));
                frontMoundGo.transform.SetParent(container.transform, false);
                var fmr = frontMoundGo.GetComponent<SpriteRenderer>();
                fmr.sprite = moundSpr;
                fmr.sortingOrder = 10;
                frontMoundGo.transform.localPosition = new Vector3(0f, -0.02f, -0.02f);
                FitSprite(frontMoundGo.transform, moundSpr, 0.76f, 0.21f);
            }

            // Chasing beetle
            var bRoot = new GameObject("ChasingBeetle");
            bRoot.transform.SetParent(transform, false);
            beetleRoot = bRoot.transform;
            var bwGo = new GameObject("Wings", typeof(SpriteRenderer));
            bwGo.transform.SetParent(beetleRoot, false);
            beetleWings = bwGo.GetComponent<SpriteRenderer>();
            beetleWings.sprite = PicnicAtlasCatalog.Slice("beetle-layers-v2", PicnicAtlasCatalog.BeetleWingsFrame, 100f, "en:btlwings");
            beetleWings.sortingOrder = 12;
            FitSprite(bwGo.transform, beetleWings.sprite, 0.70f, 0.46f);
            bwGo.transform.localPosition = new Vector3(-0.08f, 0.16f, 0.01f);

            var bbGo = new GameObject("Body", typeof(SpriteRenderer));
            bbGo.transform.SetParent(beetleRoot, false);
            beetleBody = bbGo.GetComponent<SpriteRenderer>();
            beetleBody.sprite = PicnicAtlasCatalog.Slice("beetle-layers-v2", PicnicAtlasCatalog.BeetleBodyFrame, 100f, "en:btlbody");
            beetleBody.sortingOrder = 13;
            FitSprite(bbGo.transform, beetleBody.sprite, 0.90f, 0.56f);
            beetleRoot.gameObject.SetActive(false);
        }

        public void Apply(PicnicSimulation sim, float? deltaTimeOverride = null)
        {
            // Yuna (direct transform placement without toggling CharacterController every frame)
            yunaRoot.position = PicnicAtlasCatalog.ToUnity(sim.PlayerX, sim.PlayerY, -0.15f);

            if (!hasLastStrideX)
            {
                lastStridePlayerX = sim.PlayerX;
                hasLastStrideX = true;
            }
            else if (sim.OnGround)
            {
                float dx = Mathf.Abs(sim.PlayerX - lastStridePlayerX);
                if (dx < 120f)
                {
                    accumulatedStridePx += dx;
                }
                lastStridePlayerX = sim.PlayerX;
            }
            else
            {
                lastStridePlayerX = sim.PlayerX;
            }

            var pose = PicnicContact.SelectGirlPose(sim.OnGround, sim.PlayerVx, sim.PlayerVy,
                sim.Terrain, sim.Discomfort, sim.Time, sim.BalancePose);
            yunaRenderer.sprite = PicnicAtlasCatalog.GetGirlSprite(pose.SetName, pose.FrameIndex, sim.IsPanic);
            yunaRenderer.flipX = sim.PlayerFacing < 0;
            bool blink = sim.Immunity > 0f && Mathf.Sin(sim.Time * 22f) < 0f;
            yunaRenderer.color = new Color(1f, 1f, 1f, blink ? 0.45f : 1f);

            // Clumsy stumble wobble on gravel, subtle struggle tilt on tall grass, and agitation on panic/hurt
            bool moving = Mathf.Abs(sim.PlayerVx) > 15f;
            float tiltDeg = 0f;
            float stepBounceY = 0f;
            if (sim.HurtRemaining > 0f)
            {
                tiltDeg = -sim.HurtDirection * 10f * (sim.HurtRemaining / 0.48f);
            }
            else if (sim.OnGround && sim.Terrain == PicnicTerrain.Gravel && moving)
            {
                // Unsteady, almost-tripping wobble on loose pebbles
                tiltDeg = Mathf.Sin(sim.Time * 15f) * 7.5f;
                stepBounceY = Mathf.Abs(Mathf.Sin(sim.Time * 15f)) * 0.035f;
            }
            else if (sim.OnGround && sim.Terrain == PicnicTerrain.Grass && moving)
            {
                // Slow heavy-legged discomfort sway in tall grass
                tiltDeg = Mathf.Sin(sim.Time * 6f) * 3.5f;
            }
            else if (sim.IsPanic)
            {
                tiltDeg = -sim.PlayerFacing * (5f + Mathf.Sin(sim.Time * 18f) * 3f);
            }
            if (yunaRigView != null && yunaRigView.IsBuilt)
            {
                var rigPose = PicnicYunaRigState.Evaluate(
                    sim.OnGround,
                    sim.PlayerVx,
                    sim.PlayerVy,
                    sim.Terrain,
                    sim.Discomfort,
                    sim.Time,
                    sim.BalancePose.HasValue,
                    sim.IsPanic,
                    sim.HurtRemaining,
                    accumulatedStridePx);
                float dt = deltaTimeOverride.HasValue ? deltaTimeOverride.Value : Time.deltaTime;
                yunaRigView.ApplyPose(rigPose, sim.PlayerFacing, blink ? 0.45f : 1f, dt, tiltDeg, stepBounceY, sim.OnGround);
            }
            else
            {
                yunaRenderer.transform.localPosition = new Vector3(0f, stepBounceY, 0f);
                yunaRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, tiltDeg);
            }
            ApplyContactShadow(sim);

            // Sweat droplets falling from forehead during grass or gravel sensory discomfort
            bool showSweat = sim.Stage == PicnicStage.Running
                && sim.Discomfort > 0.35f
                && (sim.Terrain == PicnicTerrain.Grass || sim.Terrain == PicnicTerrain.Gravel)
                && moving;
            if (sweatDrops != null)
            {
                for (int i = 0; i < sweatDrops.Length; i++)
                {
                    sweatDrops[i].gameObject.SetActive(showSweat);
                    if (!showSweat) continue;
                    float cycle = (sim.Time * 1.4f + i * 0.32f) % 1f;
                    float sx = sim.PlayerFacing * (0.21f + i * 0.055f);
                    float sy = 0.80f - cycle * 0.16f;
                    sweatDrops[i].transform.localPosition = new Vector3(sx * YunaArtScale, sy * YunaArtScale, -0.02f);
                    float alpha = Mathf.Sin(cycle * Mathf.PI) * 0.92f;
                    sweatDrops[i].color = new Color(1f, 1f, 1f, alpha);
                }
            }

            // Blue Nanda
            nandaRoot.position = PicnicAtlasCatalog.ToUnity(sim.FairyX, sim.FairyY, -0.20f);
            nandaRenderer.sprite = PicnicAtlasCatalog.GetBlueNandaFrame(Mathf.FloorToInt(sim.Time * 12f));
            nandaRenderer.flipX = sim.PlayerFacing < 0;

            // Ground & river enemies
            for (int i = 0; i < sim.Enemies.Length; i++)
            {
                var e = sim.Enemies[i];
                bool near = Mathf.Abs(e.X - sim.PlayerX) < 1100f && (e.Alive || e.DefeatAge < 0.7f);
                if (enemyRoots[i].gameObject.activeSelf != near) enemyRoots[i].gameObject.SetActive(near);
                if (!near) continue;

                enemyRoots[i].position = PicnicAtlasCatalog.ToUnity(e.X, e.Y + 4f, -0.05f);
                int frame = e.Alive ? Mathf.FloorToInt(sim.Time * (e.Spec.Kind == PicnicEnemyKind.Caterpillar && e.Hp == 1 ? 12f : 7f)) % 2 : 0;
                Sprite spr = e.Spec.Kind == PicnicEnemyKind.Ant
                    ? antSprites[frame]
                    : (e.Hp == 1 || !e.Alive ? redCatSprites[frame] : greenCatSprites[frame]);
                var sr = enemyRenderers[i];
                sr.sprite = spr;
                sr.flipX = e.Direction < 0;
                float squash = e.Alive ? 1f : Mathf.Lerp(1f, 0.25f, Mathf.Clamp01(e.DefeatAge / 0.25f));
                float alpha = e.Alive ? 1f : Mathf.Clamp01(1f - e.DefeatAge / 0.7f);
                sr.color = new Color(1f, 1f, 1f, alpha);
                FitSprite(enemyRoots[i], spr, e.Spec.Width / 100f, (e.Spec.Height / 100f) * squash);
            }

            // Bees
            for (int i = 0; i < sim.Encounters.Bees.Length; i++)
            {
                var b = sim.Encounters.Bees[i];
                bool near = Mathf.Abs(b.X - sim.PlayerX) < 1100f && b.Alive;
                if (beeRoots[i].gameObject.activeSelf != near) beeRoots[i].gameObject.SetActive(near);
                if (!near) continue;
                beeRoots[i].position = PicnicAtlasCatalog.ToUnity(b.X, b.Y, -0.06f);
                beeRoots[i].localScale = new Vector3(b.Direction < 0 ? 1f : -1f, 1f, 1f);
                float flap = 0.72f + 0.28f * Mathf.Abs(Mathf.Sin(sim.Time * 34f));
                var ws = beeWings[i].transform.localScale;
                beeWings[i].transform.localScale = new Vector3(ws.x, Mathf.Abs(ws.y) > 0.001f ? Mathf.Sign(ws.y) * (0.27f / beeWings[i].sprite.bounds.size.y) * flap : ws.y, 1f);
            }

            // Worms (anchored cleanly inside dirt mound on ground surface, rising smoothly by Y scale without floating)
            for (int i = 0; i < sim.Encounters.Worms.Length; i++)
            {
                var w = sim.Encounters.Worms[i];
                bool visible = Mathf.Abs(w.X - sim.PlayerX) < 1100f && w.Exposure > 0.02f;
                if (wormRoots[i].gameObject.activeSelf != visible) wormRoots[i].gameObject.SetActive(visible);
                if (!visible) continue;
                var spr = wormRenderers[i].sprite;
                if (spr != null && spr.bounds.size.y > 0.0001f)
                {
                    float sx = 0.52f / spr.bounds.size.x;
                    float sy = (0.72f * Mathf.Clamp01(w.Exposure)) / spr.bounds.size.y;
                    wormRoots[i].localScale = new Vector3(sx, sy, 1f);
                }
            }

            // Chasing Beetle
            if (sim.Encounters.Panic != null)
            {
                beetleRoot.gameObject.SetActive(true);
                beetleRoot.position = PicnicAtlasCatalog.ToUnity(sim.Encounters.Panic.X, sim.Encounters.Panic.Y, -0.18f);
            }
            else if (sim.Encounters.Exit != null)
            {
                beetleRoot.gameObject.SetActive(true);
                beetleRoot.position = PicnicAtlasCatalog.ToUnity(sim.Encounters.Exit.X, sim.Encounters.Exit.Y, -0.18f);
            }
            else
            {
                beetleRoot.gameObject.SetActive(false);
            }
        }

        public void ShowAllForEditorPreview(PicnicSimulation sim)
        {
            Apply(sim);
            for (int i = 0; i < sim.Enemies.Length; i++)
            {
                var e = sim.Enemies[i];
                enemyRoots[i].gameObject.SetActive(true);
                enemyRoots[i].position = PicnicAtlasCatalog.ToUnity(e.X, e.Y + 4f, -0.05f);
                Sprite spr = e.Spec.Kind == PicnicEnemyKind.Ant ? antSprites[0] : greenCatSprites[0];
                enemyRenderers[i].sprite = spr;
                enemyRenderers[i].color = Color.white;
                FitSprite(enemyRoots[i], spr, e.Spec.Width / 100f, e.Spec.Height / 100f);
            }
            for (int i = 0; i < sim.Encounters.Bees.Length; i++)
            {
                var b = sim.Encounters.Bees[i];
                beeRoots[i].gameObject.SetActive(true);
                beeRoots[i].position = PicnicAtlasCatalog.ToUnity(b.HomeX, b.HomeY, -0.06f);
            }
            for (int i = 0; i < sim.Encounters.Worms.Length; i++)
            {
                var w = sim.Encounters.Worms[i];
                wormRoots[i].gameObject.SetActive(true);
                wormRoots[i].position = PicnicAtlasCatalog.ToUnity(w.X, w.Y, -0.04f);
            }
        }

        private static Sprite CreateSweatDropSprite()
        {
            const int w = 24;
            const int h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var fill = new Color(0.557f, 0.835f, 0.890f, 1f);   // #8ed5e3
            var edge = new Color(0.965f, 1.000f, 1.000f, 1f);   // #f6ffff
            var clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < h; y++)
            {
                float ny = y / (float)(h - 1); // 0 = bottom round, 1 = top tip
                float halfWidth = ny < 0.35f
                    ? Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Pow((0.35f - ny) / 0.35f, 2f))) * 0.44f
                    : (1f - (ny - 0.35f) / 0.65f) * 0.44f;
                for (int x = 0; x < w; x++)
                {
                    float nx = Mathf.Abs(x / (float)(w - 1) - 0.5f);
                    if (nx <= halfWidth - 0.08f) tex.SetPixel(x, y, fill);
                    else if (nx <= halfWidth) tex.SetPixel(x, y, edge);
                    else tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 24f);
        }

        private void ApplyContactShadow(PicnicSimulation sim)
        {
            yunaShadow.gameObject.SetActive(sim.OnGround);
            if (!sim.OnGround)
            {
                return;
            }

            float angle = 0f;
            foreach (PicnicPlatformSpec platform in sim.Level.StaticPlatforms)
            {
                if (sim.PlayerX < platform.X || sim.PlayerX > platform.X + platform.Width)
                {
                    continue;
                }

                float left = PicnicLevelData.SurfaceY(platform, sim.PlayerX - 1f);
                float right = PicnicLevelData.SurfaceY(platform, sim.PlayerX + 1f);
                angle = Mathf.Atan2(left - right, 2f) * Mathf.Rad2Deg;
                break;
            }
            yunaShadow.localRotation = Quaternion.AngleAxis(angle, Vector3.forward) * Quaternion.Euler(90f, 0f, 0f);
        }

        private static Texture2D CreateContactShadowTexture()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "YunaSoftContactShadow",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f) / size * 2f - 1f;
                    float ny = (y + 0.5f) / size * 2f - 1f;
                    float radius = Mathf.Sqrt(nx * nx + ny * ny);
                    float alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.12f, 1f, radius));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                Destroy(yunaShadowMat);
                Destroy(yunaShadowTexture);
            }
            else
            {
                DestroyImmediate(yunaShadowMat);
                DestroyImmediate(yunaShadowTexture);
            }
        }

        private static void FitSprite(Transform t, Sprite s, float worldW, float worldH)
        {
            if (s == null || s.bounds.size.x <= 0.0001f || s.bounds.size.y <= 0.0001f) return;
            t.localScale = new Vector3(worldW / s.bounds.size.x, worldH / s.bounds.size.y, 1f);
        }
    }
}
