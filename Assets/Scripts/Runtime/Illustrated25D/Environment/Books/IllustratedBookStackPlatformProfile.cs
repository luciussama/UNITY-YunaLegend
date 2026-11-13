using System;
using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    [Serializable]
    public sealed class BookProfileTexture
    {
        public int width;
        public int height;
    }

    [Serializable]
    public sealed class BookProfileAlpha
    {
        public int[] bboxInclusive;
    }

    [Serializable]
    public sealed class BookProfileQuadBounds
    {
        public float left;
        public float right;
        public float bottom;
        public float top;
    }

    [Serializable]
    public sealed class BookProfileUvBounds
    {
        public float left;
        public float right;
        public float bottom;
        public float top;
    }

    [Serializable]
    public sealed class BookProfileMapping
    {
        public float metresPerTexturePixel;
        public float stackHeight;
        public float supportWidth;
        public float supportTiltDegrees;
        public float contactOffsetX;
        public float supportHeightAboveCentre;
        public float physicalDepth;
        public float visibleWidth;
        public float visibleHeight;
        public BookProfileQuadBounds quadBoundsLocal;
        public BookProfileUvBounds uvBounds;
    }

    [Serializable]
    public sealed class BookProfileDocument
    {
        public int schema;
        public string source;
        public string sourceSha256;
        public BookProfileTexture texture;
        public BookProfileAlpha alpha;
        public BookProfileMapping mapping;
    }

    public static class IllustratedBookStackPlatformProfile
    {
        public const string ResourceName = "Books_Platform_Profile";
        public const float SlabThickness = 0.12f;

        private static BookProfileDocument document;
        private static Mesh contactMesh;
        private static Mesh visualMesh;

        public static BookProfileDocument Document
        {
            get
            {
                if (document != null) return document;
                var asset = Resources.Load<TextAsset>(ResourceName);
                if (asset == null)
                    throw new InvalidOperationException("Missing Resources/" + ResourceName + ".json for the illustrated books.");
                document = JsonUtility.FromJson<BookProfileDocument>(asset.text);
                if (document == null || document.mapping == null || document.mapping.supportWidth <= 0f)
                    throw new InvalidOperationException("Invalid illustrated books profile: " + asset.name);
                return document;
            }
        }

        public static bool TryGetSupportShapes(out PlatformSupportShape[] shapes)
        {
            try
            {
                shapes = new[]
                {
                    PlatformSupportShape.MeshShape("Capa_Livros_Ilustrada", ContactMesh, Vector3.zero, Vector3.one)
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
        public static float SupportWidth => Document.mapping.supportWidth;
        public static float SupportTiltDegrees => Document.mapping.supportTiltDegrees;
        public static float ContactOffsetX => Document.mapping.contactOffsetX;
        public static float SupportHeightAboveCentre => Document.mapping.supportHeightAboveCentre;
        public static float PhysicalDepth => Document.mapping.physicalDepth;
        public static float VisibleWidth => Document.mapping.visibleWidth;
        public static float VisibleHeight => Document.mapping.visibleHeight;

        private static Mesh BuildContactMesh()
        {
            var mapping = Document.mapping;
            float half = mapping.supportWidth * .5f;
            float halfDepth = mapping.physicalDepth * .5f;
            float slope = Mathf.Tan(mapping.supportTiltDegrees * Mathf.Deg2Rad);
            float centreX = mapping.contactOffsetX;
            float centreY = mapping.supportHeightAboveCentre;
            float leftY = centreY - slope * half;
            float rightY = centreY + slope * half;
            float bottomLeftY = leftY - SlabThickness;
            float bottomRightY = rightY - SlabThickness;

            var vertices = new List<Vector3>(8)
            {
                new Vector3(centreX - half, leftY, -halfDepth),
                new Vector3(centreX + half, rightY, -halfDepth),
                new Vector3(centreX + half, rightY, halfDepth),
                new Vector3(centreX - half, leftY, halfDepth),
                new Vector3(centreX - half, bottomLeftY, -halfDepth),
                new Vector3(centreX + half, bottomRightY, -halfDepth),
                new Vector3(centreX + half, bottomRightY, halfDepth),
                new Vector3(centreX - half, bottomLeftY, halfDepth),
            };
            var triangles = new List<int>(36)
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7,
            };

            var mesh = new Mesh { name = "Books_Platform_Contact" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Mesh BuildVisualMesh()
        {
            var mapping = Document.mapping;
            var bounds = mapping.quadBoundsLocal;
            var uv = mapping.uvBounds;
            var mesh = new Mesh { name = "Books_Platform_Visual" };
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
