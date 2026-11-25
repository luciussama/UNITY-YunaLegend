using System.Collections.Generic;
using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    public readonly struct AtlasFrameSpec
    {
        public readonly int X;
        public readonly int Y;
        public readonly int W;
        public readonly int H;
        public readonly float AnchorX;
        public readonly float AnchorY;

        public AtlasFrameSpec(int x, int y, int w, int h, float anchorX = -1f, float anchorY = -1f)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            AnchorX = anchorX < 0f ? w * 0.5f : anchorX;
            AnchorY = anchorY < 0f ? h : anchorY;
        }
    }

    /// <summary>
    /// Exact 1:1 sprite-rect catalog translated from gardenArt.js, gardenFearArt.js,
    /// gardenCinematicArt.js, gardenVisualArt.js, gardenInsectArt.js, gardenPicnicArt.js, and gardenBeetleArt.js.
    /// Slices Unity Sprites directly from legacy atlases in Resources/Picnic/ with exact bottom-foot pivots.
    /// </summary>
    public static class PicnicAtlasCatalog
    {
        public const float Ppu = PicnicLevelData.PixelsPerUnit;

        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static Mesh riverStone3DMesh;
        private static Mesh woodPlatform3DMesh;
        private static Mesh cloud3DMesh;

        public static Mesh LoadRiverStone3DMesh()
        {
            if (riverStone3DMesh == null)
            {
                riverStone3DMesh = LoadBinaryMesh("Picnic/river-stone-3d-mesh", "RiverStone3D_Hunyuan");
            }
            return riverStone3DMesh;
        }

        public static Mesh LoadWoodPlatform3DMesh()
        {
            if (woodPlatform3DMesh == null)
            {
                woodPlatform3DMesh = LoadBinaryMesh("Picnic/wood-platform-3d-mesh", "WoodPlatform3D_Hunyuan");
            }
            return woodPlatform3DMesh;
        }

        public static Mesh LoadCloud3DMesh()
        {
            if (cloud3DMesh == null)
            {
                cloud3DMesh = LoadBinaryMesh("Picnic/cloud-3d-mesh", "Cloud3D_Hunyuan");
            }
            return cloud3DMesh;
        }

        private static Mesh LoadBinaryMesh(string resourcePath, string meshName)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null || asset.bytes == null || asset.bytes.Length < 8)
            {
                Debug.LogError("[Picnic1B] Missing " + resourcePath + ".bytes");
                return null;
            }

            byte[] data = asset.bytes;
            int vCount = System.BitConverter.ToInt32(data, 0);
            int iCount = System.BitConverter.ToInt32(data, 4);
            int offset = 8;

            var posFloats = new float[vCount * 3];
            System.Buffer.BlockCopy(data, offset, posFloats, 0, posFloats.Length * 4);
            offset += posFloats.Length * 4;

            var normFloats = new float[vCount * 3];
            System.Buffer.BlockCopy(data, offset, normFloats, 0, normFloats.Length * 4);
            offset += normFloats.Length * 4;

            var uvFloats = new float[vCount * 2];
            System.Buffer.BlockCopy(data, offset, uvFloats, 0, uvFloats.Length * 4);
            offset += uvFloats.Length * 4;

            var indices = new int[iCount];
            System.Buffer.BlockCopy(data, offset, indices, 0, indices.Length * 4);

            var verts = new Vector3[vCount];
            var norms = new Vector3[vCount];
            var uvs = new Vector2[vCount];
            for (int i = 0; i < vCount; i++)
            {
                int i3 = i * 3;
                int i2 = i * 2;
                verts[i] = new Vector3(posFloats[i3], posFloats[i3 + 1], posFloats[i3 + 2]);
                norms[i] = new Vector3(normFloats[i3], normFloats[i3 + 1], normFloats[i3 + 2]);
                uvs[i] = new Vector2(uvFloats[i2], uvFloats[i2 + 1]);
            }

            var mesh = new Mesh
            {
                name = meshName,
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            };
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uv = uvs;
            mesh.triangles = indices;
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Vector3 ToUnity(float px, float py, float z = 0f)
        {
            return new Vector3(px / Ppu, (PicnicLevelData.GroundY - py) / Ppu, z);
        }

        public static Texture2D LoadTexture(string name)
        {
            if (!Textures.TryGetValue(name, out var tex) || tex == null)
            {
                tex = Resources.Load<Texture2D>("Picnic/" + name);
                if (tex == null)
                {
                    Debug.LogError("[Picnic1B] Missing texture in Resources/Picnic/: " + name);
                }
                else if (name != "wall-plant-v1" && name != "soil-continuous-v1" && name != "gravel-top-seamless")
                {
                    tex.wrapMode = TextureWrapMode.Clamp;
                }
                Textures[name] = tex;
            }
            return tex;
        }

        public static Sprite Slice(string textureName, AtlasFrameSpec f, float ppu = Ppu, string cacheKey = null)
        {
            string key = cacheKey ?? $"{textureName}:{f.X},{f.Y},{f.W},{f.H}:{f.AnchorX:F1},{f.AnchorY:F1}:{ppu:F1}";
            if (Sprites.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }
            var tex = LoadTexture(textureName);
            if (tex == null) return null;

            // Legacy rects are top-down; Unity Sprite.Create rects are bottom-up.
            float uy = tex.height - f.Y - f.H;
            var rect = new Rect(
                Mathf.Clamp(f.X, 0, tex.width - 1),
                Mathf.Clamp(uy, 0, tex.height - 1),
                Mathf.Min(f.W, tex.width - f.X),
                Mathf.Min(f.H, tex.height - Mathf.Max(0, uy)));
            var pivot = new Vector2(f.AnchorX / f.W, 1f - f.AnchorY / f.H);
            var sprite = Sprite.Create(tex, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
            Sprites[key] = sprite;
            return sprite;
        }

        public static Sprite FullSprite(string textureName, Vector2 pivot, float ppu = Ppu)
        {
            string key = $"full:{textureName}:{pivot.x:F2},{pivot.y:F2}:{ppu:F1}";
            if (Sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = LoadTexture(textureName);
            if (tex == null) return null;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu, 0, SpriteMeshType.FullRect);
            Sprites[key] = sprite;
            return sprite;
        }

        public static Sprite GetGirlSprite(string setName, int frameIndex, bool fear)
        {
            var table = fear ? FearSets : GirlSets;
            if (!table.TryGetValue(setName, out var frames)) frames = table["girl-running-v1"];
            int idx = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
            string file = fear ? "fear-v2-" + setName : setName;
            const int resolutionScale = 2;
            const int padding = 4;
            int columns = frames.Length <= 4 ? 2 : 4;
            int cellWidth = 0;
            int cellHeight = 0;
            foreach (AtlasFrameSpec frame in frames)
            {
                cellWidth = Mathf.Max(cellWidth, frame.W * resolutionScale + padding * 2);
                cellHeight = Mathf.Max(cellHeight, frame.H * resolutionScale + padding * 2);
            }

            AtlasFrameSpec original = frames[idx];
            var hdFrame = new AtlasFrameSpec(
                idx % columns * cellWidth + padding,
                idx / columns * cellHeight + padding,
                original.W * resolutionScale,
                original.H * resolutionScale,
                original.AnchorX * resolutionScale,
                original.AnchorY * resolutionScale);
            float pixelsPerUnit = 256f * resolutionScale / (PicnicLevelData.PlayerHeight / Ppu);
            return Slice("HD/" + file, hdFrame, pixelsPerUnit, $"girl:HD:{file}:{idx}");
        }

        public static Sprite GetBlueNandaFrame(int frameIndex)
        {
            int idx = ((frameIndex % 8) + 8) % 8;
            int col = idx % 4;
            int row = idx / 4;
            // Nanda_Flight_Atlas_v1 is 2048x1184 (4x2 grid of 512x592 cells)
            var spec = new AtlasFrameSpec(col * 512, row * 592, 512, 592, 256f, 296f);
            return Slice("nanda-blue-atlas", spec, 592f / 0.58f, $"nanda-blue:{idx}");
        }

        public static readonly AtlasFrameSpec[] ObjetosFrames =
        {
            new AtlasFrameSpec(20, 153, 399, 197, 199.5f, 0f),     // 0: soil
            new AtlasFrameSpec(433, 179, 388, 173, 194f, 0f),      // 1: wood
            new AtlasFrameSpec(827, 186, 405, 161, 202.5f, 0f),    // 2: stone
            new AtlasFrameSpec(31, 563, 385, 173, 192.5f, 0f),     // 3: cloud
            new AtlasFrameSpec(477, 482, 321, 272, 160.5f, 272f),  // 4: ant 0
            new AtlasFrameSpec(891, 487, 324, 266, 162f, 266f),    // 5: ant 1
            new AtlasFrameSpec(53, 919, 344, 255, 172f, 255f),     // 6: caterpillar 0
            new AtlasFrameSpec(449, 918, 340, 263, 170f, 263f),    // 7: caterpillar 1
            new AtlasFrameSpec(826, 789, 417, 446, 208.5f, 446f),  // 8: tree
        };

        public static readonly AtlasFrameSpec[] AmbienteFrames =
        {
            new AtlasFrameSpec(11, 8, 673, 540, 0f, 0f),           // 0: house
            new AtlasFrameSpec(727, 134, 785, 413, 0f, 0f),        // 1: grass
            new AtlasFrameSpec(12, 698, 701, 289, 0f, 0f),         // 2: gravel
            new AtlasFrameSpec(713, 590, 810, 416, 0f, 0f),        // 3: soil/shrub
        };

        public static readonly AtlasFrameSpec[] RedCaterpillarFrames =
        {
            new AtlasFrameSpec(165, 140, 784, 549, 392f, 549f),
            new AtlasFrameSpec(1065, 138, 751, 558, 375.5f, 558f),
        };

        public static readonly AtlasFrameSpec[] AquaticFrames =
        {
            new AtlasFrameSpec(4, 4, 312, 416, 156f, 0f),          // 0: rock pillar
            new AtlasFrameSpec(516, 4, 273, 124, 136.5f, 62f),     // 1: fish 1
            new AtlasFrameSpec(1028, 4, 279, 127, 139.5f, 63.5f),  // 2: fish 2
            new AtlasFrameSpec(4, 516, 283, 123, 141.5f, 61.5f),   // 3: fish 3
            new AtlasFrameSpec(516, 516, 235, 393, 117.5f, 393f),  // 4: plant 1
            new AtlasFrameSpec(1028, 516, 270, 268, 135f, 268f),   // 5: plant 2
        };

        public static readonly AtlasFrameSpec[] InsectFrames =
        {
            new AtlasFrameSpec(4, 4, 458, 400, 229f, 400f),
            new AtlasFrameSpec(684, 4, 473, 241, 236.5f, 241f),
            new AtlasFrameSpec(1364, 4, 481, 346, 240.5f, 346f),
            new AtlasFrameSpec(4, 516, 563, 344, 281.5f, 344f),
            new AtlasFrameSpec(684, 516, 332, 359, 166f, 359f),    // 4: worm
            new AtlasFrameSpec(1364, 516, 333, 304, 166.5f, 304f),
        };

        public static readonly AtlasFrameSpec PicnicLateralFrame = new AtlasFrameSpec(4, 4, 1753, 534, 0f, 454f);
        public static readonly AtlasFrameSpec BeeBodyFrame = new AtlasFrameSpec(56, 71, 960, 607, 539f, 303.5f);
        public static readonly AtlasFrameSpec BeeWingsFrame = new AtlasFrameSpec(1176, 224, 812, 417, 405.5f, 208.5f);
        public static readonly AtlasFrameSpec BeetleBodyFrame = new AtlasFrameSpec(128, 96, 996, 626, 498f, 313f);
        public static readonly AtlasFrameSpec BeetleWingsFrame = new AtlasFrameSpec(1284, 140, 738, 490, 369f, 245f);

        public static readonly AtlasFrameSpec[] CinematicActingFrames =
        {
            new AtlasFrameSpec(4, 4, 323, 548, 163.5f, 548f),
            new AtlasFrameSpec(644, 4, 318, 535, 161.5f, 535f),
            new AtlasFrameSpec(1284, 4, 315, 526, 164f, 526f),
            new AtlasFrameSpec(1924, 4, 415, 391, 162f, 391f),
            new AtlasFrameSpec(4, 644, 428, 399, 170.5f, 399f),
            new AtlasFrameSpec(644, 644, 434, 398, 169f, 398f),
            new AtlasFrameSpec(1284, 644, 297, 308, 112f, 308f),
            new AtlasFrameSpec(1924, 644, 322, 309, 119.5f, 309f),
        };

        private static readonly Dictionary<string, AtlasFrameSpec[]> GirlSets = new Dictionary<string, AtlasFrameSpec[]>
        {
            ["girl-running-v1"] = new[]
            {
                new AtlasFrameSpec(10, 10, 158, 256, 79f, 256f),
                new AtlasFrameSpec(394, 10, 158, 257, 79f, 257f),
                new AtlasFrameSpec(778, 10, 163, 255, 81.5f, 255f),
                new AtlasFrameSpec(1162, 10, 169, 255, 84.5f, 255f),
                new AtlasFrameSpec(10, 394, 165, 251, 82.5f, 251f),
                new AtlasFrameSpec(394, 394, 165, 257, 82.5f, 257f),
                new AtlasFrameSpec(778, 394, 167, 257, 83.5f, 257f),
                new AtlasFrameSpec(1162, 394, 175, 255, 87.5f, 255f),
            },
            ["girl-actions-v1"] = new[]
            {
                new AtlasFrameSpec(10, 10, 179, 243, 89.5f, 243f),
                new AtlasFrameSpec(394, 10, 173, 247, 86.5f, 247f),
                new AtlasFrameSpec(778, 10, 176, 211, 88f, 211f),
                new AtlasFrameSpec(1162, 10, 175, 244, 87.5f, 244f),
                new AtlasFrameSpec(10, 394, 178, 249, 89f, 249f),
                new AtlasFrameSpec(394, 394, 181, 251, 90.5f, 251f),
                new AtlasFrameSpec(778, 394, 202, 190, 101f, 190f),
                new AtlasFrameSpec(1162, 394, 160, 256, 80f, 256f),
            },
            ["girl-discomfort-v1"] = new[]
            {
                new AtlasFrameSpec(10, 10, 171, 258, 85.5f, 258f),
                new AtlasFrameSpec(394, 10, 163, 257, 81.5f, 257f),
                new AtlasFrameSpec(778, 10, 176, 255, 88f, 255f),
                new AtlasFrameSpec(1162, 10, 163, 257, 81.5f, 257f),
                new AtlasFrameSpec(10, 394, 195, 250, 97.5f, 250f),
                new AtlasFrameSpec(394, 394, 198, 245, 99f, 245f),
                new AtlasFrameSpec(778, 394, 188, 205, 94f, 205f),
                new AtlasFrameSpec(1162, 394, 158, 256, 79f, 256f),
            },
            ["girl-pebbles-v2"] = new[]
            {
                new AtlasFrameSpec(10, 10, 244, 254, 122f, 254f),
                new AtlasFrameSpec(394, 10, 196, 256, 98f, 256f),
                new AtlasFrameSpec(10, 394, 274, 259, 137f, 259f),
                new AtlasFrameSpec(394, 394, 193, 262, 96.5f, 262f),
            },
        };

        private static readonly Dictionary<string, AtlasFrameSpec[]> FearSets = new Dictionary<string, AtlasFrameSpec[]>
        {
            ["girl-running-v1"] = new[]
            {
                new AtlasFrameSpec(4, 4, 338, 256, 169f, 256f),
                new AtlasFrameSpec(516, 4, 338, 257, 169f, 257f),
                new AtlasFrameSpec(1028, 4, 343, 255, 171.5f, 255f),
                new AtlasFrameSpec(1540, 4, 349, 255, 174.5f, 255f),
                new AtlasFrameSpec(4, 516, 345, 251, 172.5f, 251f),
                new AtlasFrameSpec(516, 516, 345, 257, 172.5f, 257f),
                new AtlasFrameSpec(1028, 516, 347, 257, 173.5f, 257f),
                new AtlasFrameSpec(1540, 516, 355, 255, 177.5f, 255f),
            },
            ["girl-actions-v1"] = new[]
            {
                new AtlasFrameSpec(4, 4, 359, 243, 179.5f, 243f),
                new AtlasFrameSpec(516, 4, 353, 247, 176.5f, 247f),
                new AtlasFrameSpec(1028, 4, 356, 211, 178f, 211f),
                new AtlasFrameSpec(1540, 4, 355, 244, 177.5f, 244f),
                new AtlasFrameSpec(4, 516, 358, 249, 179f, 249f),
                new AtlasFrameSpec(516, 516, 361, 251, 180.5f, 251f),
                new AtlasFrameSpec(1028, 516, 382, 190, 191f, 190f),
                new AtlasFrameSpec(1540, 516, 340, 256, 170f, 256f),
            },
            ["girl-discomfort-v1"] = new[]
            {
                new AtlasFrameSpec(4, 4, 351, 258, 175.5f, 258f),
                new AtlasFrameSpec(516, 4, 343, 257, 171.5f, 257f),
                new AtlasFrameSpec(1028, 4, 356, 255, 178f, 255f),
                new AtlasFrameSpec(1540, 4, 343, 257, 171.5f, 257f),
                new AtlasFrameSpec(4, 516, 375, 250, 187.5f, 250f),
                new AtlasFrameSpec(516, 516, 378, 245, 189f, 245f),
                new AtlasFrameSpec(1028, 516, 368, 205, 184f, 205f),
                new AtlasFrameSpec(1540, 516, 338, 256, 169f, 256f),
            },
            ["girl-pebbles-v2"] = new[]
            {
                new AtlasFrameSpec(4, 4, 424, 254, 212f, 254f),
                new AtlasFrameSpec(516, 4, 376, 256, 188f, 256f),
                new AtlasFrameSpec(1028, 4, 454, 259, 227f, 259f),
                new AtlasFrameSpec(1540, 4, 373, 262, 186.5f, 262f),
            },
        };
    }
}
