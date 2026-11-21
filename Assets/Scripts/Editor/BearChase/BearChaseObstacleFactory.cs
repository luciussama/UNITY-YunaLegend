using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;
using YunaLegend.Editor.ToyRoom;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.Editor.BearChase
{
    /// <summary>
    /// Fabrica unica de templates visuais dos brinquedos arremessaveis pelo urso
    /// (tanto para o pool em voo/piso quanto para o preview na pata do urso durante o windup).
    /// </summary>
    public static class BearChaseObstacleFactory
    {
        public static BearChaseObstacleController BuildObstacleController(BearChaseMaterialFactory matFactory)
        {
            var rootGo = new GameObject("BearChase_Obstacles");
            var ctrl = rootGo.AddComponent<BearChaseObstacleController>();

            var templatesRoot = new GameObject("Templates");
            templatesRoot.transform.SetParent(rootGo.transform, false);
            templatesRoot.SetActive(false);

            var entries = new List<BearChaseObstacleController.ObstacleTemplateEntry>();
            var defs = BearChaseObstacleCatalog.All;

            ToyRoomVisualFactory.BeginMaterialScope("Assets/Art/BearChase/Materials", "M_BearChase_Toy_");
            try
            {
                for (int i = 0; i < defs.Count; i++)
                {
                    BearChaseObstacleDef def = defs[i];
                    GameObject tpl = CreateToyTemplate(templatesRoot.transform, def, matFactory);
                    entries.Add(new BearChaseObstacleController.ObstacleTemplateEntry
                    {
                        Kind = def.Kind,
                        PrefabTemplate = tpl
                    });
                }
            }
            finally
            {
                ToyRoomVisualFactory.EndMaterialScope();
            }

            Material shadowMat = matFactory.GetOrCreateTransparentUnlit(
                "ThrownToyShadow",
                new Color(0.06f, 0.03f, 0.02f, 0.62f),
                BearChaseAssetProcessor.ContactShadowTexPath);

            ParticleSystem dust = CreateLandingDustTemplate(rootGo.transform, matFactory);
            ctrl.Configure(entries.ToArray(), shadowMat, dust);
            return ctrl;
        }

        public static BearChaseBearAnimator.ThrowPreviewEntry[] BuildBearHandPreviews(
            Transform handAnchor,
            BearChaseMaterialFactory matFactory)
        {
            if (handAnchor == null)
            {
                return System.Array.Empty<BearChaseBearAnimator.ThrowPreviewEntry>();
            }

            var list = new List<BearChaseBearAnimator.ThrowPreviewEntry>();
            var defs = BearChaseObstacleCatalog.All;

            ToyRoomVisualFactory.BeginMaterialScope("Assets/Art/BearChase/Materials", "M_BearChase_HandToy_");
            try
            {
                for (int i = 0; i < defs.Count; i++)
                {
                    BearChaseObstacleDef def = defs[i];
                    GameObject preview = CreateToyTemplate(handAnchor, def, matFactory);
                    preview.name = $"HandPreview_{def.Kind}";
                    preview.transform.localScale = Vector3.one * (def.VisualScale * 0.42f);
                    preview.transform.localPosition = new Vector3(0f, -0.04f, 0.06f);
                    preview.SetActive(false);
                    list.Add(new BearChaseBearAnimator.ThrowPreviewEntry
                    {
                        Kind = def.Kind,
                        PreviewObject = preview
                    });
                }
            }
            finally
            {
                ToyRoomVisualFactory.EndMaterialScope();
            }

            return list.ToArray();
        }

        private static GameObject CreateToyTemplate(
            Transform parent,
            BearChaseObstacleDef def,
            BearChaseMaterialFactory matFactory)
        {
            var go = new GameObject($"Template_{def.Kind}");
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * def.VisualScale;

            switch (def.Kind)
            {
                case BearChaseObstacleKind.ToyTrain:
                    if (!ToyRoomPropSculptor.TryAttachGeneratedPropModel(go.transform, "train", 0.72f, 1.05f, 1.25f))
                    {
                        ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("train", "Trem", 0.9f, new ToyRoomPoint(0f, 0f)));
                    }
                    break;

                case BearChaseObstacleKind.BlockTower:
                    ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("blocks", "Blocos", 0.85f, new ToyRoomPoint(0f, 0f)));
                    break;

                case BearChaseObstacleKind.PlushBunny:
                    ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("bunny", "Coelho", 0.96f, new ToyRoomPoint(0f, 0f)));
                    ApplyPainterlyTextureToRenderers(
                        go,
                        matFactory.GetOrCreatePainterly("BunnyToy", Color.white, BearChaseAssetProcessor.BunnyPlushTexPath, Vector2.one, outlineWidth: 0.002f),
                        onlyPrimaryPart: true);
                    break;

                case BearChaseObstacleKind.RubberDuck:
                    ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("duck", "Patinho", 0.95f, new ToyRoomPoint(0f, 0f)));
                    break;

                default:
                    BuildClosedCircusDrum(go.transform, matFactory);
                    break;
            }

            return go;
        }

        private static void BuildClosedCircusDrum(Transform parent, BearChaseMaterialFactory matFactory)
        {
            const string meshesDir = "Assets/Art/BearChase/Meshes";
            Directory.CreateDirectory(meshesDir);

            Material shellMat = matFactory.GetOrCreatePainterly(
                "DrumToyShell",
                new Color(1.0f, 0.96f, 0.88f),
                "Assets/Materials/T_CircusDrum.png",
                Vector2.one,
                emission: new Color(0.10f, 0.08f, 0.06f),
                outlineWidth: 0.002f,
                ambientScale: 1.15f);

            Material headMat = matFactory.GetOrCreatePainterly(
                "DrumToyHead",
                new Color(0.96f, 0.89f, 0.74f),
                BearChaseAssetProcessor.CabinetWoodTexPath,
                Vector2.one,
                emission: new Color(0.12f, 0.10f, 0.07f),
                outlineWidth: 0.002f,
                ambientScale: 1.18f);

            Material woodMat = matFactory.GetOrCreatePainterly(
                "DrumToyWood",
                new Color(0.92f, 0.68f, 0.30f),
                BearChaseAssetProcessor.CabinetWoodTexPath,
                Vector2.one,
                outlineWidth: 0.002f,
                ambientScale: 1.05f);

            const float radius = 0.22f;
            const float height = 0.32f;

            Mesh sideMesh = SaveOrUpdateDrumMesh(meshesDir, "BearChase_DrumSide", BuildCylinderSideMesh(radius, height, 32));
            Mesh capMesh = SaveOrUpdateDrumMesh(meshesDir, "BearChase_DrumCap", BuildDiscCapMesh(radius + 0.004f, 32));
            Mesh hoopMesh = SaveOrUpdateDrumMesh(meshesDir, "BearChase_DrumHoop", BuildTorusHoopMesh(radius + 0.004f, 0.022f, 32, 10));

            AttachStaticMesh("Drum_Shell", parent, Vector3.zero, Quaternion.identity, sideMesh, shellMat);
            AttachStaticMesh("Drum_TopHead", parent, new Vector3(0f, height, 0f), Quaternion.identity, capMesh, headMat);
            AttachStaticMesh("Drum_BottomHead", parent, Vector3.zero, Quaternion.Euler(180f, 0f, 0f), capMesh, headMat);
            AttachStaticMesh("Drum_TopHoop", parent, new Vector3(0f, height - 0.006f, 0f), Quaternion.identity, hoopMesh, woodMat);
            AttachStaticMesh("Drum_BottomHoop", parent, new Vector3(0f, 0.006f, 0f), Quaternion.identity, hoopMesh, woodMat);

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick.name = $"Drumstick_{side}";
                stick.transform.SetParent(parent, false);
                stick.transform.localPosition = new Vector3(0.01f * side, height + 0.035f, 0f);
                stick.transform.localRotation = Quaternion.Euler(0f, side * 34f, 90f);
                stick.transform.localScale = new Vector3(0.026f, 0.16f, 0.026f);
                Object.DestroyImmediate(stick.GetComponent<Collider>());
                stick.GetComponent<Renderer>().sharedMaterial = woodMat;
            }
        }

        private static void AttachStaticMesh(
            string name,
            Transform parent,
            Vector3 localPos,
            Quaternion localRot,
            Mesh mesh,
            Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static Mesh SaveOrUpdateDrumMesh(string dir, string assetName, Mesh generated)
        {
            string path = $"{dir}/{assetName}.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                generated.name = assetName;
                AssetDatabase.CreateAsset(generated, path);
                return generated;
            }

            existing.Clear();
            existing.vertices = generated.vertices;
            existing.normals = generated.normals;
            existing.uv = generated.uv;
            existing.triangles = generated.triangles;
            existing.RecalculateBounds();
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(generated);
            return existing;
        }

        private static Mesh BuildCylinderSideMesh(float radius, float height, int radialSegs)
        {
            int cols = radialSegs + 1;
            var verts = new List<Vector3>(cols * 2);
            var norms = new List<Vector3>(cols * 2);
            var uvs = new List<Vector2>(cols * 2);
            var tris = new List<int>(radialSegs * 6);

            for (int row = 0; row <= 1; row++)
            {
                float y = row * height;
                float v = row;
                for (int i = 0; i <= radialSegs; i++)
                {
                    float u = i / (float)radialSegs;
                    float angle = u * Mathf.PI * 2f;
                    float c = Mathf.Cos(angle);
                    float s = Mathf.Sin(angle);
                    verts.Add(new Vector3(radius * c, y, radius * s));
                    norms.Add(new Vector3(c, 0f, s));
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int i = 0; i < radialSegs; i++)
            {
                int a = i;
                int b = i + cols;
                tris.Add(a);
                tris.Add(b);
                tris.Add(a + 1);
                tris.Add(a + 1);
                tris.Add(b);
                tris.Add(b + 1);
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildDiscCapMesh(float radius, int radialSegs)
        {
            var verts = new List<Vector3>(radialSegs + 2);
            var norms = new List<Vector3>(radialSegs + 2);
            var uvs = new List<Vector2>(radialSegs + 2);
            var tris = new List<int>(radialSegs * 3);

            verts.Add(Vector3.zero);
            norms.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i <= radialSegs; i++)
            {
                float u = i / (float)radialSegs;
                float angle = u * Mathf.PI * 2f;
                float c = Mathf.Cos(angle);
                float s = Mathf.Sin(angle);
                verts.Add(new Vector3(radius * c, 0f, radius * s));
                norms.Add(Vector3.up);
                uvs.Add(new Vector2(0.5f + 0.45f * c, 0.5f + 0.45f * s));
            }

            for (int i = 1; i <= radialSegs; i++)
            {
                tris.Add(0);
                tris.Add(i + 1);
                tris.Add(i);
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildTorusHoopMesh(float majorRadius, float minorRadius, int majorSegs, int minorSegs)
        {
            int stride = minorSegs + 1;
            var verts = new List<Vector3>((majorSegs + 1) * stride);
            var norms = new List<Vector3>((majorSegs + 1) * stride);
            var uvs = new List<Vector2>((majorSegs + 1) * stride);
            var tris = new List<int>(majorSegs * minorSegs * 6);

            for (int i = 0; i <= majorSegs; i++)
            {
                float u = i / (float)majorSegs;
                float a = u * Mathf.PI * 2f;
                float ca = Mathf.Cos(a);
                float sa = Mathf.Sin(a);

                for (int j = 0; j <= minorSegs; j++)
                {
                    float v = j / (float)minorSegs;
                    float th = v * Mathf.PI * 2f;
                    float ct = Mathf.Cos(th);
                    float st = Mathf.Sin(th);

                    float r = majorRadius + minorRadius * ct;
                    verts.Add(new Vector3(r * ca, minorRadius * st, r * sa));
                    norms.Add(new Vector3(ct * ca, st, ct * sa));
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int i = 0; i < majorSegs; i++)
            {
                for (int j = 0; j < minorSegs; j++)
                {
                    int a = i * stride + j;
                    int b = (i + 1) * stride + j;
                    tris.Add(a);
                    tris.Add(a + 1);
                    tris.Add(b);
                    tris.Add(b);
                    tris.Add(a + 1);
                    tris.Add(b + 1);
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void ApplyPainterlyTextureToRenderers(GameObject root, Material mat, bool onlyPrimaryPart)
        {
            Renderer[] rends = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                if (!onlyPrimaryPart || rends[i].name.Contains("Body") || rends[i].name.Contains("Head") || rends[i].name.Contains("Ear"))
                {
                    rends[i].sharedMaterial = mat;
                }
            }
        }

        private static ParticleSystem CreateLandingDustTemplate(Transform parent, BearChaseMaterialFactory matFactory)
        {
            var go = new GameObject("LandingDustBurstTemplate");
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.45f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.4f, 2.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.44f);
            main.startColor = new Color(0.95f, 0.90f, 0.78f, 0.72f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0f, 10)
            });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = matFactory.GetOrCreateParticleUnlit(
                "DustParticle",
                new Color(0.96f, 0.91f, 0.80f, 0.75f),
                BearChaseAssetProcessor.SoftGlowTexPath,
                additive: false);

            return ps;
        }
    }
}
