using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Arena.Core;
using YunaLegend.Arena.Runtime;

namespace YunaLegend.EditorScripts
{
    /// <summary>Staged visual fixtures, not a claim of combat gameplay or physical mobile testing.</summary>
    public static class ArenaMobileCameraReview
    {
        private const string Output = "Logs/mobile_ui_audit";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        public static void Capture()
        {
            Directory.CreateDirectory(Output);
            var rows = new List<string> { "viewport,separation_m,camera_distance_m,player_height_px,enemy_height_px,transition_max_camera_delta_m,player_xmin_px,player_xmax_px,enemy_xmin_px,enemy_xmax_px" };
            foreach (var viewport in new[] { new Vector2Int(390, 844), new Vector2Int(844, 390) })
            {
                CaptureViewport(viewport, rows);
            }
            File.WriteAllLines(Path.Combine(Output, "arena-mobile-near-far.csv"), rows);
            Debug.Log("ARENA_MOBILE_NEAR_FAR_CAPTURE_OK six staged views with actual actor transforms and camera smoothing");
        }

        private static void CaptureViewport(Vector2Int viewport, List<string> rows)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Arena_Gameplay.unity");
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<ArenaStageBootstrap>();
            var player = Field<ArenaActorView>(bootstrap, "playerView");
            var enemy = Field<ArenaActorView>(bootstrap, "enemyView");
            var catalog = Field<ArenaSpriteCatalog>(bootstrap, "spriteCatalog");
            var framing = UnityEngine.Object.FindFirstObjectByType<ArenaCameraController>();
            var hud = UnityEngine.Object.FindFirstObjectByType<ArenaHudController>();
            if (camera == null || player == null || enemy == null || framing == null)
            {
                throw new InvalidOperationException("Arena camera or serialized actor views missing");
            }
            framing.Configure(camera);
            framing.SetMobileViewportOverride(true);
            camera.aspect = viewport.x / (float)viewport.y;
            if (hud.GetComponentInChildren<Canvas>(true) == null)
            {
                typeof(ArenaHudController).GetMethod("Awake", PrivateInstance)?.Invoke(hud, null);
            }
            hud.ForceShowMobileControlsForCapture(true);
            hud.SkipFadeForCapture();
            var sim = new ArenaCombatSimulation(false, true);
            Set(sim, "EnemyState", ArenaEnemyState.Idle);
            Set(sim, "EnemySpawnScale", 1f);
            var render = typeof(MobileUiCapture).GetMethod("RenderSceneWithUi", BindingFlags.Static | BindingFlags.NonPublic);
            float previousSeparation = 2f;
            foreach (float separation in new[] { 2f, 5f, 9f })
            {
                float maxDelta = 0f;
                for (int frame = 0; frame < 180; frame++)
                {
                    float progress = Mathf.Clamp01(frame / 119f);
                    float current = Mathf.Lerp(previousSeparation, separation, Mathf.SmoothStep(0f, 1f, progress));
                    ApplyFixture(sim, current, player, enemy, catalog, 1f / 60f);
                    Vector3 before = framing.transform.position;
                    framing.UpdateFraming(sim, 1f / 60f);
                    if (frame > 0 || previousSeparation != separation)
                    {
                        maxDelta = Mathf.Max(maxDelta, Vector3.Distance(before, framing.transform.position));
                    }
                }
                string file = $"arena-mobile-{viewport.x}x{viewport.y}-separation-{separation:0}m.png";
                render.Invoke(null, new object[] { camera, viewport.x, viewport.y, (Action)(() =>
                {
                    hud.SetViewportOverride(viewport.x, viewport.y);
                    hud.Refresh(sim);
                    Canvas.ForceUpdateCanvases();
                    hud.SetViewportOverride(viewport.x, viewport.y);
                    ApplyFixture(sim, separation, player, enemy, catalog, 0f);
                    framing.UpdateFraming(sim, 1f / 60f);
                    Vector2 playerExtent = ProjectedHorizontalExtent(camera, player);
                    Vector2 enemyExtent = ProjectedHorizontalExtent(camera, enemy);
                    rows.Add(string.Join(",", $"{viewport.x}x{viewport.y}", F(separation), F(-framing.transform.position.z),
                        F(PixelHeight(camera, player, viewport.y)), F(PixelHeight(camera, enemy, viewport.y)), F(maxDelta),
                        F(playerExtent.x * viewport.x), F(playerExtent.y * viewport.x),
                        F(enemyExtent.x * viewport.x), F(enemyExtent.y * viewport.x)));
                    File.WriteAllLines(Path.Combine(Output, "arena-mobile-near-far.csv"), rows);
                    AssertHorizontalCoverage(player.name, playerExtent, viewport, separation);
                    AssertHorizontalCoverage(enemy.name, enemyExtent, viewport, separation);
                }), file });
                previousSeparation = separation;
            }
        }

        private static void ApplyFixture(ArenaCombatSimulation sim, float separation, ArenaActorView player, ArenaActorView enemy, ArenaSpriteCatalog catalog, float dt)
        {
            Set(sim, "PlayerX", 800f - separation * 50f);
            Set(sim, "EnemyX", 800f + separation * 50f);
            Set(sim, "PlayerY", sim.Tuning.GroundY);
            Set(sim, "EnemyY", sim.Tuning.GroundY);
            player.UpdatePlayerVisual(sim, catalog, 0f, dt);
            enemy.UpdateEnemyVisual(sim, catalog, dt);
        }

        private static T Field<T>(object target, string name) where T : class
        {
            return typeof(ArenaStageBootstrap).GetField(name, PrivateInstance)?.GetValue(target) as T;
        }

        private static void Set(ArenaCombatSimulation target, string name, object value)
        {
            typeof(ArenaCombatSimulation).GetProperty(name).GetSetMethod(true).Invoke(target, new[] { value });
        }

        private static float PixelHeight(Camera camera, ArenaActorView actor, int viewportHeight)
        {
            bool found = false;
            Bounds bounds = new Bounds();
            foreach (Renderer renderer in actor.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled || renderer is ParticleSystemRenderer)
                {
                    continue;
                }
                if (!found) bounds = renderer.bounds;
                else bounds.Encapsulate(renderer.bounds);
                found = true;
            }
            if (!found) return 0f;
            float bottom = camera.WorldToViewportPoint(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)).y;
            float top = camera.WorldToViewportPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z)).y;
            return Mathf.Abs(top - bottom) * viewportHeight;
        }

        private static Vector2 ProjectedHorizontalExtent(Camera camera, ArenaActorView actor)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            foreach (var renderer in actor.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy
                    || (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)))
                {
                    continue;
                }
                Bounds bounds = renderer.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 position = bounds.center + Vector3.Scale(bounds.extents, new Vector3(
                        (corner & 1) == 0 ? -1f : 1f,
                        (corner & 2) == 0 ? -1f : 1f,
                        (corner & 4) == 0 ? -1f : 1f));
                    float x = camera.WorldToViewportPoint(position).x;
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                }
            }
            if (!float.IsFinite(minX) || !float.IsFinite(maxX))
            {
                throw new InvalidOperationException("No active actor mesh bounds for " + actor.name);
            }
            return new Vector2(minX, maxX);
        }

        private static void AssertHorizontalCoverage(string actorName, Vector2 extent, Vector2Int viewport, float separation)
        {
            if (extent.x < 0f || extent.y > 1f)
            {
                throw new InvalidOperationException($"Actor clipped: {actorName} {viewport} separation={separation} viewportX={extent}");
            }
        }

        private static string F(float value)
        {
            return value.ToString("F4", CultureInfo.InvariantCulture);
        }
    }
}
