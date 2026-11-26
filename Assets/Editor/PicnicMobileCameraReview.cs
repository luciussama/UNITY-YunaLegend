using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

namespace YunaLegend.EditorScripts
{
    /// <summary>Staged camera/landing evidence, not a claim of manual or physical mobile play.</summary>
    public static class PicnicMobileCameraReview
    {
        private const string Output = "Logs/mobile_ui_audit";

        public static void Capture()
        {
            Directory.CreateDirectory(Output);
            var rows = new List<string> { "viewport,fixture,camera_z,world_width,player_x,landing_min_x,landing_max_x,landing_min_y,landing_max_y" };
            foreach (var viewport in new[] { new Vector2Int(390, 844), new Vector2Int(844, 390) })
            {
                CaptureViewport(viewport, rows);
            }
            File.WriteAllLines(Path.Combine(Output, "picnic-mobile-camera-planning.csv"), rows);
            Debug.Log("PICNIC_MOBILE_CAMERA_PLANNING_OK six staged views; player and next landing visible");
        }

        private static void CaptureViewport(Vector2Int viewport, List<string> rows)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Picnic_Gameplay.unity");
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var scenery = UnityEngine.Object.FindFirstObjectByType<PicnicSceneryController>();
            var platforms = UnityEngine.Object.FindFirstObjectByType<PicnicPlatformController>();
            var actors = UnityEngine.Object.FindFirstObjectByType<PicnicActorController>();
            var hud = UnityEngine.Object.FindFirstObjectByType<PicnicNarrativeAndHudController>();
            var sim = new PicnicSimulation();
            sim.SkipStory();
            platforms.Build(sim.Level);
            scenery.Build(camera, sim.Level);
            actors.Build(sim);
            hud.Build();
            camera.aspect = viewport.x / (float)viewport.y;
            scenery.SetMobileViewportOverride(true);
            var render = typeof(MobileUiCapture).GetMethod("RenderSceneWithUi", BindingFlags.Static | BindingFlags.NonPublic);
            var fixtures = new[] { (Name: "porch-edge", X: 660f, Support: "entrada"),
                (Name: "suspended", X: 925f, Support: "suspensa-1"),
                (Name: "river", X: 2540f, Support: "primeiro-riacho-pedra-0") };
            foreach (var fixture in fixtures)
            {
                PicnicPlatformSpec support = sim.Level.PlatformById[fixture.Support];
                Set(sim, "PlayerX", fixture.X);
                Set(sim, "PlayerY", PicnicLevelData.SurfaceY(support, fixture.X));
                Set(sim, "SupportId", fixture.Support);
                Set(sim, "OnGround", true);
                Set(sim, "PlayerFacing", 1);
                platforms.Apply(sim);
                actors.Apply(sim);
                hud.Apply(sim, false);
                for (int frame = 0; frame < 180; frame++) scenery.Apply(sim, 1f / 60f);
                string file = $"picnic-mobile-planning-{viewport.x}x{viewport.y}-{fixture.Name}.png";
                render.Invoke(null, new object[] { camera, viewport.x, viewport.y, (Action)(() =>
                {
                    camera.aspect = viewport.x / (float)viewport.y;
                    scenery.Apply(sim, 1f / 60f);
                    hud.SetViewportOverride(viewport.x, viewport.y);
                    hud.Apply(sim, false);
                    Canvas.ForceUpdateCanvases();
                    hud.SetViewportOverride(viewport.x, viewport.y);
                    var nearby = sim.Level.QueryPlatforms(sim.Time, sim.Clouds, sim.Difficulty, sim.PlayerX, 500f);
                    var landing = PicnicMobileCameraFraming.FindNextLanding(nearby, sim.SupportId, sim.PlayerX, sim.PlayerY, sim.PlayerFacing);
                    if (!landing.HasValue) throw new InvalidOperationException("Fixture has no next landing: " + fixture.Name);
                    Vector3 player = camera.WorldToViewportPoint(PicnicAtlasCatalog.ToUnity(sim.PlayerX, sim.PlayerY));
                    Vector3 a = camera.WorldToViewportPoint(new Vector3(landing.MinX, landing.Y, 0f));
                    Vector3 b = camera.WorldToViewportPoint(new Vector3(landing.MaxX, landing.Y, 0f));
                    if (player.x < 0.05f || player.x > 0.95f || a.x < 0f || b.x > 1f || a.y < 0f || a.y > 1f || b.y < 0f || b.y > 1f)
                    {
                        throw new InvalidOperationException($"Planning coverage failed: {fixture.Name} player={player} landingA={a} landingB={b}");
                    }
                    if (-camera.transform.position.z < PicnicMobileCameraFraming.MinimumDistance - 0.001f)
                    {
                        throw new InvalidOperationException("Camera distance below mobile floor");
                    }
                    float worldWidth = -camera.transform.position.z * 2f * Mathf.Tan(13.5f * Mathf.Deg2Rad) * camera.aspect;
                    rows.Add(string.Join(",", $"{viewport.x}x{viewport.y}", fixture.Name, F(camera.transform.position.z), F(worldWidth),
                        F(player.x), F(a.x), F(b.x), F(a.y), F(b.y)));
                }), file });
            }
            scenery.SetMobileViewportOverride(false);
            scenery.Apply(sim, 1f / 60f);
            if (Mathf.Abs(camera.transform.position.z + 13.5f) > 0.0001f)
            {
                throw new InvalidOperationException("Desktop camera distance changed");
            }
            Debug.Log("PICNIC_DESKTOP_CAMERA_DISTANCE_UNCHANGED " + viewport);
        }

        private static void Set(PicnicSimulation sim, string property, object value)
        {
            typeof(PicnicSimulation).GetProperty(property).GetSetMethod(true).Invoke(sim, new[] { value });
        }

        private static string F(float value)
        {
            return value.ToString("F4", CultureInfo.InvariantCulture);
        }
    }
}
