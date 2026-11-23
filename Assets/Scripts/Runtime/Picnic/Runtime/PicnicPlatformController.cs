using System.Collections.Generic;
using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>One factory for every support. Surface geometry is authored before artwork.</summary>
    public sealed class PicnicPlatformController : MonoBehaviour
    {
        private readonly List<SpriteRenderer> paintings = new List<SpriteRenderer>();
        private readonly List<BoxCollider[]> contacts = new List<BoxCollider[]>();
        private readonly List<GameObject> roots = new List<GameObject>();
        private PicnicStageSpec stage;
        private Material vegetation;

        public void Build(PicnicStageSpec spec)
        {
            stage = spec;
            vegetation = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            vegetation.mainTexture = Resources.Load<Texture2D>("Picnic/wall");
            for (int i = 0; i < spec.Supports.Count; i++) CreateSupport(i);
            for (int wheel = 0; wheel < 4; wheel++) CreateWheelFrame(wheel);
        }

        public void Apply(PicnicSimulation sim)
        {
            for (int i = 0; i < roots.Count; i++)
            {
                var support = stage.Supports[i];
                bool near = Mathf.Abs(support.Center(sim.Time) - sim.X) < 19f + support.Width * 0.5f;
                roots[i].SetActive(near && sim.IsSupportActive(i));
                if (!near) continue;
                if (support.Wheel >= 0)
                {
                    roots[i].transform.position = new Vector3(support.Center(sim.Time), support.Height(sim.Time, support.Center(sim.Time)), 0f);
                }
                float age = sim.CloudTimers[i];
                paintings[i].color = new Color(1f, 1f, 1f,
                    age >= 0f && Mathf.Sin(age * (9f + age * 5f)) < 0f ? 0.3f : 1f);
                // One-way 3D supports: a jumping character may pass up through an underside.
                foreach (var collider in contacts[i])
                {
                    float surface = support.Height(sim.Time, collider.transform.position.x);
                    collider.enabled = sim.VelocityY <= 0.01f && sim.Y >= surface - 0.08f;
                }
            }
        }

        private void CreateSupport(int index)
        {
            var support = stage.Supports[index];
            var root = new GameObject(support.Id);
            root.transform.SetParent(transform, false);
            float initialX = support.Center(0f);
            float initialY = support.Wheel >= 0 ? support.Height(0f, initialX) : support.Y;
            root.transform.position = new Vector3(initialX, initialY, 0f);
            roots.Add(root);
            string asset = support.Terrain == PicnicTerrain.Stone ? "stone" : support.Terrain == PicnicTerrain.Cloud ? "cloud" : support.Terrain == PicnicTerrain.Wood ? "wood" : "grass";
            float artWidth = support.Ground ? Mathf.Min(3.3f, support.Width + 0.18f) : support.Width + 0.25f;
            var painting = PicnicArt.Create("Contact painting", root.transform,
                new Vector3(initialX, initialY + 0.09f, -0.04f), artWidth, 4, asset);
            // Crop-independent uniform scaling; never stretch the object to match a box.
            paintings.Add(painting);
            var boxes = new List<BoxCollider>();
            int slices = support.Ground ? Mathf.CeilToInt(support.Width / 0.16f) : 1;
            for (int n = 0; n < slices; n++)
            {
                float x = support.Left(0f) + (n + 0.5f) * support.Width / slices;
                float y = support.Height(0f, x);
                var contact = new GameObject("Top_" + n);
                contact.transform.SetParent(root.transform, false);
                contact.transform.localPosition = new Vector3(x - initialX, y - initialY - 0.04f, 0f);
                var box = contact.AddComponent<BoxCollider>();
                box.size = new Vector3(support.Width / slices, 0.08f, 0.8f);
                boxes.Add(box);
            }
            contacts.Add(boxes.ToArray());
            if (support.Ground) BuildGround(root.transform, support);
            if (support.Terrain == PicnicTerrain.Stone)
            {
                var pillar = PicnicArt.Create("Submerged natural pillar", root.transform,
                    new Vector3(support.X, support.Y - 1.15f, 0.15f), support.Width * 0.68f, 1, "aquatic0");
                pillar.color = new Color(0.8f, 0.95f, 0.9f, 0.8f);
            }
        }

        private void BuildGround(Transform root, PicnicSupportSpec support)
        {
            int columns = Mathf.CeilToInt(support.Width / 0.16f);
            var vertices = new Vector3[(columns + 1) * 2];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[columns * 6];
            for (int i = 0; i <= columns; i++)
            {
                float x = support.Left(0f) + support.Width * i / columns;
                float y = support.Height(0f, x);
                vertices[i * 2] = new Vector3(x - support.X, y - support.Y - 0.04f, 0.08f);
                vertices[i * 2 + 1] = new Vector3(x - support.X, -4f - support.Y, 0.08f);
                uv[i * 2] = new Vector2(x / 3.2f, y / 3.2f);
                uv[i * 2 + 1] = new Vector2(x / 3.2f, -4f / 3.2f);
                if (i == columns) continue;
                int v = i * 2;
                int t = i * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }
            var mesh = new Mesh { name = support.Id + "_curved_surface" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            var go = new GameObject("Vegetated earth", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(root, false);
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = vegetation;
            // Repeated natural tufts follow precisely the same analytic surface as contact.
            for (float left = support.Left(0f); left < support.Right(0f); left += 2.3f)
            {
                float x = Mathf.Min(left + 1.1f, support.Right(0f) - 0.1f);
                var grass = PicnicArt.Create("Surface foliage", root,
                    new Vector3(x, support.Height(0f, x) - 0.1f, -0.06f), 2.5f, 5, "grass");
                if (support.TerrainAt(x) == PicnicTerrain.Grass)
                {
                    var tuft = PicnicArt.Create("Sensory tall grass", root, new Vector3(x, support.Height(0f, x) - 0.16f, -0.4f), 2.3f, 15, "tall-grass");
                    PicnicArt.SetHeight(tuft, 0.5f);
                }
                if (support.Terrain == PicnicTerrain.Gravel)
                {
                    PicnicArt.Create("Loose pebbles", root, new Vector3(x, support.Height(0f, x) - 0.13f, -0.12f), 2.4f, 6, "gravel");
                }
            }
        }

        private void CreateWheelFrame(int index)
        {
            PicnicSupportSpec first = null;
            foreach (var support in stage.Supports) if (support.Wheel == index) { first = support; break; }
            if (first == null) return;
            var go = new GameObject("Wheel_" + index);
            go.transform.SetParent(transform, false);
            var wheel = go.AddComponent<PicnicWheelView>();
            wheel.Initialize(first.X, first.Y, index);
        }
        private void OnDestroy()
        {
            if (vegetation != null) Destroy(vegetation);
        }
    }
}
