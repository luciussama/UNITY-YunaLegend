using System.Diagnostics;
using UnityEngine;

namespace YunaLegend.Illustrated25D
{
    /// <summary>
    /// T02.3 proof only: one approved painting deforms as a proportional 2D skin instead of a rigid card.
    /// It rewrites only its own mesh vertices; the player root, collider, pivot and physics are never moved.
    /// The sole line (local y = 0, where the painting pivot sits) is the anchor: squash and leg compression
    /// never cross it, so the feet cannot sink through the support.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class PaintedMeshDeformer : MonoBehaviour
    {
        [Header("Pintura aprovada")]
        public Sprite sprite;
        public Material material;

        [Header("Malha da prova")]
        [Range(4, 24)] public int columns = 12;
        [Range(8, 64)] public int rows = 36;

        [Header("Pose provisoria")]
        [Range(-30f, 30f)] public float lean;
        [Range(-25f, 25f)] public float sideBandSwing;
        [Range(-12f, 12f)] public float headTilt;
        [Range(.8f, 1.2f)] public float squashY = 1f;
        [Range(.8f, 1.2f)] public float squashX = 1f;
        [Range(-.06f, .06f)] public float breathe;
        [Range(0f, .12f)] public float bodyCompression;
        public bool flipX;

        public float Width { get; private set; }
        public float Height { get; private set; }
        public float BottomLocalY { get; private set; }
        public float MinLocalY { get; private set; }
        public float MaxLocalY { get; private set; }
        public float MaxAbsLocalX { get; private set; }
        /// <summary>Lowest deformed vertex among the vertices that start on or above the sole line y = 0.</summary>
        public float MinSoleLocalY { get; private set; }
        public int VertexCount { get; private set; }
        public int TriangleCount { get; private set; }
        public double LastUpdateMilliseconds { get; private set; }
        public double AverageUpdateMilliseconds => updates == 0 ? 0 : totalMilliseconds / updates;
        public double MaxUpdateMilliseconds => maxMilliseconds;
        public int UpdateCount => updates;

        Mesh mesh;
        Vector3[] bind;
        Vector2[] uvs;
        Vector3[] buffer;
        int[] triangles;
        double totalMilliseconds, maxMilliseconds;
        int updates;
        bool built;

        void Awake() { if (sprite != null) Build(sprite, material); }
        void OnValidate() { if (sprite != null) { Build(sprite, material); } }
        void OnDestroy()
        {
            if (mesh == null) return;
            if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh);
            mesh = null;
        }

        public void Build(Sprite source, Material sourceMaterial)
        {
            if (source == null)
            {
                built = false;
                return;
            }
            sprite = source;
            material = sourceMaterial;
            var bounds = source.bounds;
            Width = bounds.size.x;
            Height = bounds.size.y;
            BottomLocalY = bounds.min.y;
            int stride = columns + 1, heightCount = rows + 1;
            bind = new Vector3[stride * heightCount];
            uvs = new Vector2[stride * heightCount];
            buffer = new Vector3[stride * heightCount];
            var rect = source.rect;
            var texture = source.texture;
            for (int j = 0; j < heightCount; j++)
                for (int i = 0; i < stride; i++)
                {
                    float u = (float)i / columns, v = (float)j / rows;
                    int index = j * stride + i;
                    bind[index] = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, u), Mathf.Lerp(bounds.min.y, bounds.max.y, v), 0f);
                    uvs[index] = new Vector2((rect.x + u * rect.width) / texture.width, (rect.y + v * rect.height) / texture.height);
                }
            triangles = new int[columns * rows * 6];
            int t = 0;
            for (int j = 0; j < rows; j++)
                for (int i = 0; i < columns; i++)
                {
                    int a = j * stride + i, b = a + 1, c = a + stride, d = c + 1;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
                }
            VertexCount = bind.Length;
            TriangleCount = triangles.Length / 3;
            if (mesh == null) mesh = new Mesh { name = "PaintedMesh_PROOF", hideFlags = HideFlags.DontSave };
            mesh.Clear();
            mesh.SetVertices(bind);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            var white = new Color32[bind.Length];
            for (int i = 0; i < white.Length; i++) white[i] = new Color32(255, 255, 255, 255);
            mesh.colors32 = white;
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            built = true;
            Refresh();
        }

        /// <summary>Writes the deformed vertices for the current pose. Called by the proof driver, once per frame.</summary>
        public void Refresh()
        {
            if (!built) return;
            var watch = Stopwatch.StartNew();
            float minimumY = float.MaxValue, maximumY = float.MinValue, maximumX = 0f, minimumSole = float.MaxValue;
            for (int i = 0; i < bind.Length; i++)
            {
                var deformed = Deform(bind[i]);
                buffer[i] = deformed;
                minimumY = Mathf.Min(minimumY, deformed.y);
                maximumY = Mathf.Max(maximumY, deformed.y);
                maximumX = Mathf.Max(maximumX, Mathf.Abs(deformed.x));
                if (bind[i].y >= 0f) minimumSole = Mathf.Min(minimumSole, deformed.y);
            }
            mesh.SetVertices(buffer);
            mesh.RecalculateBounds();
            MinLocalY = minimumY; MaxLocalY = maximumY; MaxAbsLocalX = maximumX;
            MinSoleLocalY = minimumSole == float.MaxValue ? 0f : minimumSole;
            watch.Stop();
            LastUpdateMilliseconds = watch.Elapsed.TotalMilliseconds;
            totalMilliseconds += LastUpdateMilliseconds;
            maxMilliseconds = System.Math.Max(maxMilliseconds, LastUpdateMilliseconds);
            updates++;
        }

        public void ResetPose()
        {
            lean = sideBandSwing = headTilt = breathe = bodyCompression = 0f;
            squashX = squashY = 1f;
            flipX = false;
            Refresh();
        }

        Vector3 Deform(Vector3 point)
        {
            float t = Height > 0f ? Mathf.Clamp01(point.y / Height) : 0f;
            float halfWidth = Width * .5f;
            float normalisedX = halfWidth > 0f ? point.x / halfWidth : 0f;
            float side = normalisedX >= 0f ? 1f : -1f;

            // Side band at shoulder height: the front and back outline edges of the silhouette, not the painted
            // arms. Rotating the two edges in opposite directions only shears the torso; no arm articulates.
            float bandWeight = Smooth(.52f, .74f, Mathf.Abs(normalisedX)) * Smooth(.34f, .46f, t) * (1f - Smooth(.74f, .86f, t));
            if (bandWeight > 1e-4f)
                point = Rotate(point, new Vector2(side * halfWidth * .42f, Height * .70f), side * sideBandSwing, bandWeight);

            // Torso tilt about the hips, then a smaller head counter-tilt about the neck.
            // Positive lean tips the top towards +X, the facing direction of the approved painting.
            point = Rotate(point, new Vector2(0f, Height * .44f), -lean, Smooth(.44f, .62f, t));
            point = Rotate(point, new Vector2(0f, Height * .82f), -headTilt, Smooth(.80f, .92f, t));

            // Breathing widens only the chest band.
            if (breathe != 0f)
            {
                float chest = Smooth(.48f, .66f, t) * (1f - Smooth(.80f, .90f, t));
                point.x += point.x * breathe * chest;
            }

            // Squash and leg compression are anchored on the sole line y = 0, so the contact never crosses the floor.
            point.x *= squashX;
            point.y *= squashY * (1f - bodyCompression * Smooth(0f, .5f, t));

            if (flipX) point.x = -point.x;
            return point;
        }

        static Vector3 Rotate(Vector3 point, Vector2 pivot, float degrees, float weight)
        {
            float angle = degrees * Mathf.Deg2Rad * weight;
            if (Mathf.Abs(angle) < 1e-5f) return point;
            float sin = Mathf.Sin(angle), cos = Mathf.Cos(angle);
            float dx = point.x - pivot.x, dy = point.y - pivot.y;
            return new Vector3(pivot.x + dx * cos - dy * sin, pivot.y + dx * sin + dy * cos, point.z);
        }

        static float Smooth(float from, float to, float value)
        {
            if (to <= from) return value >= to ? 1f : 0f;
            float u = Mathf.Clamp01((value - from) / (to - from));
            return u * u * (3f - 2f * u);
        }
    }
}
