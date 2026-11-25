using System;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

// Static presentation harness. Copy into the isolated project's Assets/Editor to run.
public static class GrassSurfaceReview
{
    public static void Capture()
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-grassReview");
        string version = index >= 0 ? args[index + 1] : "after";
        string output = Path.Combine("Evidence/PicnicGrassAlignment", version);
        Directory.CreateDirectory(output);
        EditorSceneManager.OpenScene("Assets/Scenes/Picnic_Gameplay.unity");
        var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
        var platforms = UnityEngine.Object.FindFirstObjectByType<PicnicPlatformController>();
        var scenery = UnityEngine.Object.FindFirstObjectByType<PicnicSceneryController>();
        var actors = UnityEngine.Object.FindFirstObjectByType<PicnicActorController>();
        var sim = new PicnicSimulation();
        sim.SkipStory();
        platforms.Build(sim.Level);
        scenery.Build(camera, sim.Level);
        actors.Build(sim);
        var target = new RenderTexture(1280, 720, 24);
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        camera.targetTexture = target;
        camera.aspect = 1280f / 720f;
        int hillCount = 0;
        int captures = 0;
        try
        {
            foreach (var platform in sim.Level.StaticPlatforms)
            {
                if (platform.Hill <= 0f || platform.GrassPatches.Length == 0)
                {
                    continue;
                }

                foreach (var patch in platform.GrassPatches)
                {
                    float x = platform.X + platform.Width * (patch.Start + patch.End) * 0.5f;
                    float y = PicnicLevelData.SurfaceY(platform, x);
                    typeof(PicnicSimulation).GetProperty("PlayerX").SetValue(sim, x);
                    typeof(PicnicSimulation).GetProperty("PlayerY").SetValue(sim, y);
                    typeof(PicnicSimulation).GetProperty("FairyX").SetValue(sim, x + 80f);
                    typeof(PicnicSimulation).GetProperty("FairyY").SetValue(sim, y - 125f);
                    platforms.Apply(sim);
                    actors.Apply(sim);
                    scenery.Apply(sim);
                    camera.Render();
                    RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                    image.Apply();
                    string file = (++captures).ToString("00") + "-" + platform.Id + ".png";
                    File.WriteAllBytes(Path.Combine(output, file), image.EncodeToPNG());
                    Debug.Log("GRASS_REVIEW " + file + " x=" + x + " hill=" + platform.Hill);
                }

                if (++hillCount == 3)
                {
                    break;
                }
            }
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(image);
        }

        Debug.Log("GRASS_REVIEW_OK captures=" + captures);
    }
}
