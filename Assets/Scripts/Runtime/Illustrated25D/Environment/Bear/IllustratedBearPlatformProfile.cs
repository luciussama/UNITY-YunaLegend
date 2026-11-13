using System;
using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    [Serializable]
    public sealed class BearProfileTexture
    {
        public int width;
        public int height;
    }

    [Serializable]
    public sealed class BearProfileAlpha
    {
        public int[] bboxInclusive;
    }

    [Serializable]
    public sealed class BearProfileQuadBounds
    {
        public float left;
        public float right;
        public float bottom;
        public float top;
    }

    [Serializable]
    public sealed class BearProfileUvBounds
    {
        public float left;
        public float right;
        public float bottom;
        public float top;
    }

    [Serializable]
    public sealed class BearProfileMapping
    {
        public float crownHeightAboveFloor;
        public float crownTextureX;
        public float crownTextureY;
        public float metresPerTexturePixel;
        public float contactOffsetX;
        public float visibleWidth;
        public float visibleHeight;
        public BearProfileQuadBounds quadBoundsLocal;
        public BearProfileUvBounds uvBounds;
    }

    [Serializable]
    public sealed class BearProfilePoint
    {
        public int textureX;
        public int textureY;
        public float localX;
        public float localY;
    }

    [Serializable]
    public sealed class BearProfileDocument
    {
        public int schema;
        public string source;
        public string sourceSha256;
        public BearProfileTexture texture;
        public BearProfileAlpha alpha;
        public BearProfileMapping mapping;
        public BearProfilePoint[] topProfile;
    }

    public static class IllustratedBearPlatformProfile
    {
        public const string ResourceName = "Bear_Platform_Profile";
        public const float PhysicalDepth = 1.20f;
        public const float SlabThickness = 0.14f;

        private static BearProfileDocument document;
        private static Mesh contactMesh;
        private static Mesh visualMesh;

        public static BearProfileDocument Document
        {
            get
            {
                if (document != null) return document;
                var asset = Resources.Load<TextAsset>(ResourceName);
                if (asset == null)
                    throw new InvalidOperationException("Missing Resources/" + ResourceName + ".json for the illustrated bear.");
                document = JsonUtility.FromJson<BearProfileDocument>(asset.text);
                if (document == null || document.topProfile == null || document.topProfile.Length < 2)
                    throw new InvalidOperationException("Invalid illustrated bear profile: " + asset.name);
                return document;
            }
        }

        public static bool TryGetSupportShapes(out PlatformSupportShape[] shapes)
        {
            try
            {
                shapes = new[]
                {
                    PlatformSupportShape.MeshShape("Silhueta_Urso_Ilustrado", ContactMesh, Vector3.zero, Vector3.one)
                };
                return true;
            }
            catch (InvalidOperationException)
            {
                shapes = null;
                return false;
            }
        }

        public static Mesh ContactMesh => contactMesh != null ? contactMesh : contactMesh = BuildContactMesh();
        public static Mesh VisualMesh => visualMesh != null ? visualMesh : visualMesh = BuildVisualMesh();
        public static float ContactOffsetX => Document.mapping.contactOffsetX;
        public static float VisibleWidth => Document.mapping.visibleWidth;
        public static float CrownHeightAboveFloor => Document.mapping.crownHeightAboveFloor;

        private static Mesh BuildContactMesh()
        {
            var points = Document.topProfile;
            var vertices = new List<Vector3>((points.Length - 1) * 8);
            var triangles = new List<int>((points.Length - 1) * 36);
            float front = -PhysicalDepth * 0.5f;
            float back = PhysicalDepth * 0.5f;

            for (int i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                int start = vertices.Count;
                vertices.Add(new Vector3(a.localX, a.localY, front));
                vertices.Add(new Vector3(b.localX, b.localY, front));
                vertices.Add(new Vector3(b.localX, b.localY, back));
                vertices.Add(new Vector3(a.localX, a.localY, back));
                vertices.Add(new Vector3(a.localX, a.localY - SlabThickness, front));
                vertices.Add(new Vector3(b.localX, b.localY - SlabThickness, front));
                vertices.Add(new Vector3(b.localX, b.localY - SlabThickness, back));
                vertices.Add(new Vector3(a.localX, a.localY - SlabThickness, back));

                AddTriangle(triangles, start, start + 2, start + 1);
                AddTriangle(triangles, start, start + 3, start + 2);
                AddTriangle(triangles, start + 4, start + 5, start + 6);
                AddTriangle(triangles, start + 4, start + 6, start + 7);
                AddTriangle(triangles, start, start + 1, start + 5);
                AddTriangle(triangles, start, start + 5, start + 4);
                AddTriangle(triangles, start + 3, start + 7, start + 6);
                AddTriangle(triangles, start + 3, start + 6, start + 2);
                AddTriangle(triangles, start, start + 4, start + 7);
                AddTriangle(triangles, start, start + 7, start + 3);
                AddTriangle(triangles, start + 1, start + 2, start + 6);
                AddTriangle(triangles, start + 1, start + 6, start + 5);
            }

            var mesh = new Mesh { name = "Bear_Platform_Contact" };
            if (vertices.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void AddTriangle(List<int> triangles, int a, int b, int c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        private static Mesh BuildVisualMesh()
        {
            var mapping = Document.mapping;
            var bounds = mapping.quadBoundsLocal;
            var uv = mapping.uvBounds;
            var mesh = new Mesh { name = "Bear_Platform_Visual" };
            mesh.SetVertices(new[]
            {
                new Vector3(bounds.left, bounds.bottom, 0f),
                new Vector3(bounds.right, bounds.bottom, 0f),
                new Vector3(bounds.right, bounds.top, 0f),
                new Vector3(bounds.left, bounds.top, 0f)
            });
            mesh.SetUVs(0, new[]
            {
                new Vector2(uv.left, uv.bottom),
                new Vector2(uv.right, uv.bottom),
                new Vector2(uv.right, uv.top),
                new Vector2(uv.left, uv.top)
            });
            mesh.SetTriangles(new[] { 0, 2, 1, 0, 3, 2 }, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
