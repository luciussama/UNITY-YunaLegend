using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

public static class PicnicYunaDetailCapture
{
    public static void Capture()
    {
        const string output = "Evidence/PicnicYunaDetail/after";
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
        ValidateFrames();

        var cases = new List<(string name, float x, bool moving, bool jumping)>
        {
            ("idle-wood", 338f, false, false),
            ("run-grass", 1624f, true, false),
            ("run-gravel", 3970f, true, false),
            ("jump", 1624f, true, true),
        };
        foreach (PicnicPlatformSpec platform in sim.Level.StaticPlatforms)
        {
            if (platform.Hill > 0f)
            {
                cases.Add(("hill", platform.X + platform.Width * 0.2f, false, false));
                break;
            }
        }
        foreach (var sample in cases)
        {
            float y = PicnicLevelData.GroundY;
            PicnicTerrain terrain = PicnicTerrain.Wood;
            foreach (PicnicPlatformSpec platform in sim.Level.StaticPlatforms)
            {
                if (sample.x < platform.X || sample.x > platform.X + platform.Width)
                {
                    continue;
                }
                y = PicnicLevelData.SurfaceY(platform, sample.x);
                terrain = platform.Terrain;
                break;
            }
            Set(sim, "PlayerX", sample.x);
            Set(sim, "PlayerY", y - (sample.jumping ? 120f : 0f));
            Set(sim, "PlayerVx", sample.moving ? 150f : 0f);
            Set(sim, "PlayerVy", sample.jumping ? -100f : 0f);
            Set(sim, "OnGround", !sample.jumping);
            Set(sim, "Terrain", terrain);
            Set(sim, "Discomfort", sample.moving ? 0.7f : 0f);
            Set(sim, "Time", sample.moving ? 1.5f : 0f);
            Set(sim, "FairyX", sample.x + 80f);
            Set(sim, "FairyY", y - 125f);
            platforms.Apply(sim);
            actors.Apply(sim);
            foreach (Vector2Int size in new[] { new Vector2Int(1280, 720), new Vector2Int(390, 844) })
            {
                camera.aspect = (float)size.x / size.y;
                scenery.Apply(sim);
                Save(camera, size.x, size.y, Path.Combine(output, sample.name + "-" + size.x + ".png"));
            }
        }

        var renderer = actors.transform.Find("YunaPlayer/YunaSprite").GetComponent<SpriteRenderer>();
        var body = actors.transform.Find("YunaPlayer").GetComponent<CharacterController>();
        Debug.Log("YUNA_DETAIL_STATS visualScale=" + renderer.transform.localScale.x + " bodyHeight=" + body.height + " bodyRadius=" + body.radius);
        ValidateShadow(actors);
        Debug.Log("YUNA_DETAIL_CAPTURE_OK ten views; HD slices and pivots validated");
    }

    private static void ValidateShadow(PicnicActorController actors)
    {
        var mesh = actors.transform.Find("YunaPlayer/YunaContactShadow3D").GetComponent<MeshRenderer>();
        Texture source = mesh.sharedMaterial.mainTexture;
        var target = RenderTexture.GetTemporary(128, 128, 0, RenderTextureFormat.ARGB32);
        var image = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        RenderTexture previous = RenderTexture.active;
        try
        {
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            image.Apply();
            float corners = image.GetPixel(0, 0).a + image.GetPixel(127, 0).a
                + image.GetPixel(0, 127).a + image.GetPixel(127, 127).a;
            if (corners > 0.01f || image.GetPixel(64, 64).a < 0.9f)
            {
                throw new Exception("Contact shadow alpha is not radial/transparent at corners");
            }
            Debug.Log("YUNA_SHADOW_ALPHA_OK corners=" + corners + " center=" + image.GetPixel(64, 64).a);
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }

    private static void ValidateFrames()
    {
        int count = 0;
        foreach (bool fear in new[] { false, true })
        {
            string tableName = fear ? "FearSets" : "GirlSets";
            var table = (Dictionary<string, AtlasFrameSpec[]>)typeof(PicnicAtlasCatalog)
                .GetField(tableName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            foreach (var entry in table)
            {
                Color32[] atlasPixels = null;
                int atlasWidth = 0;
                for (int i = 0; i < entry.Value.Length; i++)
                {
                    Sprite sprite = PicnicAtlasCatalog.GetGirlSprite(entry.Key, i, fear);
                    AtlasFrameSpec original = entry.Value[i];
                    if (sprite == null || sprite.rect.width != original.W * 2 || sprite.rect.height != original.H * 2)
                    {
                        throw new Exception("HD frame import mismatch " + entry.Key + ":" + i);
                    }
                    float originalHeight = original.H / (256f / (PicnicLevelData.PlayerHeight / PicnicAtlasCatalog.Ppu));
                    if (Mathf.Abs(sprite.bounds.size.y - originalHeight) > 0.0001f)
                    {
                        throw new Exception("HD frame world size changed " + entry.Key + ":" + i);
                    }
                    Vector2 expectedPivot = new Vector2(original.AnchorX * 2f, (original.H - original.AnchorY) * 2f);
                    if ((sprite.pivot - expectedPivot).sqrMagnitude > 0.0001f)
                    {
                        throw new Exception("HD frame foot pivot changed " + entry.Key + ":" + i);
                    }
                    if (atlasPixels == null)
                    {
                        atlasWidth = sprite.texture.width;
                        atlasPixels = ReadTexturePixels(sprite.texture);
                    }
                    int visiblePixels = 0;
                    Rect rect = sprite.rect;
                    for (int y = (int)rect.yMin; y < (int)rect.yMax; y++)
                    {
                        for (int x = (int)rect.xMin; x < (int)rect.xMax; x++)
                        {
                            Color32 pixel = atlasPixels[y * atlasWidth + x];
                            if (pixel.a > 127 && pixel.r + pixel.g + pixel.b > 0)
                            {
                                visiblePixels++;
                            }
                        }
                    }
                    if (visiblePixels < rect.width * rect.height * 0.02f)
                    {
                        throw new Exception("HD frame transparent or empty " + entry.Key + ":" + i);
                    }
                    count++;
                }
            }
        }
        Debug.Log("YUNA_HD_FRAMES_OK count=" + count);
    }

    private static Color32[] ReadTexturePixels(Texture source)
    {
        var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        var image = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        RenderTexture previous = RenderTexture.active;
        try
        {
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            image.Apply();
            return image.GetPixels32();
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }

    private static void Set(PicnicSimulation sim, string name, object value)
    {
        typeof(PicnicSimulation).GetProperty(name).SetValue(sim, value);
    }

    private static void Save(Camera camera, int width, int height, string path)
    {
        var target = new RenderTexture(width, height, 24);
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture previous = RenderTexture.active;
        camera.targetTexture = target;
        try
        {
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }
}
