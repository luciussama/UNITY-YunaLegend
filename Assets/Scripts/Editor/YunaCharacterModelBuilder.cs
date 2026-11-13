using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using YunaLegend.DarkRoom;

namespace YunaLegend.EditorScripts
{
    // Editable parametric study. The approved concept remains the art target.
    // All geometry, bind poses and materials are persisted as ordinary Unity assets.
    public static class YunaCharacterModelBuilder
    {
        public const string Folder = "Assets/Characters/Yuna";
        public const string PrefabPath = Folder + "/Yuna_Study.prefab";
        static readonly Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
        static readonly List<Transform> boneList = new List<Transform>();
        static readonly Dictionary<string, int> materialIds = new Dictionary<string, int>();
        static readonly List<Material> materials = new List<Material>();
        static readonly List<Vector3> vertices = new List<Vector3>();
        static readonly List<Vector3> normals = new List<Vector3>();
        static readonly List<Vector2> uv = new List<Vector2>();
        static readonly List<BoneWeight> weights = new List<BoneWeight>();
        static readonly List<List<int>> indices = new List<List<int>>();
        static Transform root;

        static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        static float Pi => Mathf.PI;
        static Transform Bone(string name, string parent, Vector3 position)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent == null ? root : bones[parent], false);
            t.position = position; bones.Add(name, t); boneList.Add(t); return t;
        }
        static BoneWeight W(string name) => new BoneWeight { boneIndex0 = boneList.IndexOf(bones[name]), weight0 = 1 };
        static BoneWeight Blend(string a, string b, float t)
        {
            t = Mathf.Clamp01(t); return new BoneWeight { boneIndex0 = boneList.IndexOf(bones[a]),
                boneIndex1 = boneList.IndexOf(bones[b]), weight0 = 1 - t, weight1 = t };
        }
        static void Material(string name, string hex, float smoothness = .2f, bool metal = false)
        {
            string path = Folder + "/Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            ColorUtility.TryParseHtmlString(hex, out var color);
            m.SetColor("_BaseColor", color); m.SetFloat("_Smoothness", smoothness); m.SetFloat("_Metallic", metal ? .55f : 0);
            // Same subdued readability floor as the previous Yuna materials, below Bloom's threshold.
            m.SetColor("_EmissionColor", color.linear * .018f); m.EnableKeyword("_EMISSION");
            m.SetFloat("_Cull", 0); EditorUtility.SetDirty(m);
            materialIds[name] = materials.Count; materials.Add(m); indices.Add(new List<int>());
        }

        // Analytic derivatives keep periodic seams smooth without welding UV seams.
        static void Surface(string material, int nu, int nv, Func<float, float, Vector3> point,
            Func<Vector3, BoneWeight> weight, bool wrap = true)
        {
            int offset = vertices.Count;
            for (int j = 0; j <= nv; j++) for (int i = 0; i <= nu; i++)
            {
                float u = (float)i / nu, v = (float)j / nv, e = .0005f;
                var p = point(u, v);
                float ua = wrap ? u - e : Mathf.Max(0, u - e), ub = wrap ? u + e : Mathf.Min(1, u + e);
                var du = point(ub, v) - point(ua, v);
                var dv = point(u, Mathf.Min(1, v + e)) - point(u, Mathf.Max(0, v - e));
                // Normalize tangents first: their finite-difference cross product is
                // smaller than Vector3.normalized's epsilon on this 1.4 m character.
                var normal = Vector3.Cross(du.normalized, dv.normalized).normalized;
                if (normal.sqrMagnitude < .1f) normal = Vector3.up;
                vertices.Add(p); normals.Add(normal); uv.Add(new Vector2(u, v)); weights.Add(weight(p));
            }
            var triangles = indices[materialIds[material]];
            for (int j = 0; j < nv; j++) for (int i = 0; i < nu; i++)
            {
                int a = offset + j * (nu + 1) + i, b = a + nu + 1;
                triangles.Add(a); triangles.Add(a + 1); triangles.Add(b);
                triangles.Add(a + 1); triangles.Add(b + 1); triangles.Add(b);
            }
        }
        static void Ellipsoid(string mat, Vector3 center, Vector3 radius, string bone, int resolution = 32,
            Func<Vector3, Vector3> deform = null)
        {
            Surface(mat, resolution, resolution / 2, (u, v) => {
                float p = Mathf.Clamp(v, .0001f, .9999f) * Pi, t = u * Pi * 2;
                var q = V(Mathf.Sin(p) * Mathf.Cos(t), Mathf.Cos(p), Mathf.Sin(p) * Mathf.Sin(t));
                if (deform != null) q = deform(q);
                return center + Vector3.Scale(q, radius);
            }, _ => W(bone));
        }
        static Vector3 Curve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float s = 1 - t; return s * s * s * a + 3 * s * s * t * b + 3 * s * t * t * c + t * t * t * d;
        }
        static void Strand(string mat, Vector3 a, Vector3 b, Vector3 c, Vector3 d, float radius,
            float flat, Func<Vector3, BoneWeight> weight, int sides = 12, int length = 20, bool taper = true, float tipScale = 1)
        {
            Surface(mat, sides, length, (u, v) => {
                float t = Mathf.Clamp(v, .0001f, .9999f);
                var p = Curve(a, b, c, d, t);
                var tangent = (Curve(a, b, c, d, Mathf.Min(1, t + .001f)) - Curve(a, b, c, d, Mathf.Max(0, t - .001f))).normalized;
                var axis = Mathf.Abs(Vector3.Dot(tangent, Vector3.right)) > .97f ? Vector3.up : Vector3.right;
                var x = Vector3.Cross(tangent, axis).normalized; var z = Vector3.Cross(tangent, x).normalized;
                float r = radius * Mathf.Lerp(1, tipScale, t) * (taper ? Mathf.Pow(Mathf.Sin(Pi * t), .4f) * (1 - .55f * t) : 1);
                return p + r * (Mathf.Cos(u * 2 * Pi) * x + Mathf.Sin(u * 2 * Pi) * flat * z);
            }, weight);
        }
        static void Line(string mat, Func<float, Vector3> path, float radius, string bone, int length = 24)
        {
            Surface(mat, 8, length, (u, v) => {
                var p = path(v); var tangent = (path(Mathf.Min(1, v + .001f)) - path(Mathf.Max(0, v - .001f))).normalized;
                var x = Vector3.Cross(tangent, Vector3.forward).normalized;
                if (x.sqrMagnitude < .1f) x = Vector3.right;
                var z = Vector3.Cross(tangent, x).normalized;
                return p + radius * (Mathf.Cos(u * 2 * Pi) * x + Mathf.Sin(u * 2 * Pi) * z);
            }, _ => W(bone));
        }

        static void Skeleton()
        {
            Bone("Hips", null, V(0, .60f, 0)); Bone("Spine", "Hips", V(0, .79f, 0));
            Bone("Chest", "Spine", V(0, .93f, 0)); Bone("Neck", "Chest", V(0, 1.005f, 0));
            Bone("Head", "Neck", V(0, 1.14f, 0));
            Bone("Ponytail", "Head", V(0, 1.30f, -.135f)); Bone("PonytailTip", "Ponytail", V(0, 1.09f, -.30f));
            Bone("Bow", "Head", V(0, 1.35f, -.13f));
            foreach (int side in new[] {-1, 1})
            {
                string s = side < 0 ? "Left" : "Right";
                Bone(s + "UpperArm", "Chest", V(side * .15f, .925f, 0));
                Bone(s + "LowerArm", s + "UpperArm", V(side * .215f, .765f, 0));
                Bone(s + "Hand", s + "LowerArm", V(side * .25f, .64f, .018f));
                Bone(s + "UpperLeg", "Hips", V(side * .075f, .56f, 0));
                Bone(s + "LowerLeg", s + "UpperLeg", V(side * .075f, .30f, 0));
                Bone(s + "Foot", s + "LowerLeg", V(side * .075f, .095f, 0));
                Bone(s + "Toes", s + "Foot", V(side * .075f, .05f, .11f));
                Bone(s + "Skirt", "Hips", V(side * .11f, .75f, 0));
            }
            Bone("FrontSkirt", "Hips", V(0, .75f, .07f)); Bone("BackSkirt", "Hips", V(0, .75f, -.07f));
        }
        static void Face()
        {
            Ellipsoid("Skin", V(0, 1.16f, .003f), V(.184f, .200f, .164f), "Head", 80, q => {
                // Broad toddler cheeks, smaller chin, slightly flattened eye plane.
                float chin = Mathf.SmoothStep(.76f, 1, Mathf.InverseLerp(-1, -.1f, q.y));
                q.x *= chin; if (q.z > 0) q.z *= 1 - .07f * Mathf.Exp(-Mathf.Pow((q.y - .12f) * 4, 2));
                if (q.z > 0) q.z += NoseHeight(q.x * .184f, 1.16f + q.y * .20f) / .164f;
                return q;
            });
            Ellipsoid("Skin", V(0, .99f, 0), V(.047f, .085f, .047f), "Neck");
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .077f;
                Ellipsoid("Skin", V(side * .18f, 1.137f, .005f), V(.025f, .040f, .019f), "Head");
                Ellipsoid("Ear", V(side * .19f, 1.138f, .021f), V(.013f, .024f, .004f), "Head", 24);
                FacePatch("EyeWhite", x, 1.172f, .047f, .052f, .002f);
                FacePatch("Lash", x, 1.173f, .027f, .033f, .004f);
                FacePatch("Iris", x, 1.173f, .025f, .031f, .005f);
                FacePatch("Pupil", x, 1.174f, .013f, .023f, .007f);
                FacePatch("EyeWhite", x - .008f, 1.187f, .006f, .008f, .009f);
                FacePatch("EyeWhite", x + .008f, 1.162f, .0025f, .003f, .009f);
                Line("Lash", t => FacePoint(x + .047f * Mathf.Cos(Pi * t), 1.172f + .052f * Mathf.Sin(Pi * t), .004f), .002f, "Head");
                Line("Skin", t => FacePoint(x + .047f * Mathf.Cos(Pi * (1 + t)), 1.172f + .052f * Mathf.Sin(Pi * (1 + t)), .003f), .002f, "Head");
                Line("HairDark", t => FacePoint(x + (t - .5f) * .077f, 1.248f + .010f * Mathf.Sin(Pi * t), .002f), .003f, "Head");
            }
            Line("Lip", t => FacePoint((t - .5f) * .051f, 1.078f + .010f * Mathf.Pow(2 * t - 1, 2), .002f), .0018f, "Head");
            Line("LipSoft", t => FacePoint((t - .5f) * .040f, 1.073f + .006f * Mathf.Pow(2 * t - 1, 2), .002f), .0014f, "Head");
        }
        static float NoseHeight(float x, float y) => .018f * Mathf.Exp(-Mathf.Pow(x / .019f, 2) - Mathf.Pow((y - 1.126f) / .025f, 2));
        static Vector3 FacePoint(float x, float y, float offset)
        {
            float qy = (y - 1.16f) / .20f;
            float chin = Mathf.SmoothStep(.76f, 1, Mathf.InverseLerp(-1, -.1f, qy));
            float qx = x / (.184f * chin);
            float z = .003f + .164f * Mathf.Sqrt(Mathf.Max(0, 1 - qx * qx - qy * qy))
                * (1 - .07f * Mathf.Exp(-Mathf.Pow((qy - .12f) * 4, 2)));
            return V(x, y, z + NoseHeight(x, y) + offset);
        }
        static void FacePatch(string mat, float x, float y, float rx, float ry, float offset)
        {
            Surface(mat, 40, 12, (u, v) => {
                float r = Mathf.Clamp(1 - v, .0001f, .9999f), a = u * 2 * Pi;
                return FacePoint(x + rx * r * Mathf.Cos(a), y + ry * r * Mathf.Sin(a), offset);
            }, _ => W("Head"));
        }
        static void Hair()
        {
            Surface("Hair", 64, 28, (u, v) => {
                float t = u * 2 * Pi, front = Mathf.Pow(Mathf.Max(0, Mathf.Sin(t)), .5f);
                float phi = (.001f + v * Mathf.Lerp(2.17f, 1.08f, front));
                float grooves = 1 + .014f * Mathf.Cos(t * 18 + phi * 2);
                return V(.191f * Mathf.Sin(phi) * Mathf.Cos(t) * grooves,
                    1.145f + .229f * Mathf.Cos(phi), .006f + .177f * Mathf.Sin(phi) * Mathf.Sin(t) * grooves);
            }, _ => W("Head"));
            for (int i = 0; i < 6; i++)
            {
                float x = -.15f + i * .052f;
                Strand("Hair", V(x * .65f - .035f, 1.34f, .07f),
                    V(x - .055f, 1.37f, .14f), V(x + .02f, 1.28f, .184f),
                    V(x + .035f, 1.245f - .025f * Mathf.Sin(i * .7f), .157f), .044f, .37f, _ => W("Head"));
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Strand("Hair", V(side * .14f, 1.31f, .09f), V(side * .22f, 1.26f, .10f),
                    V(side * .195f, 1.18f, .09f), V(side * .17f, 1.13f, .04f), .032f, .55f, _ => W("Head"));
                Strand("HairLight", V(side * .17f, 1.22f, .07f), V(side * .21f, 1.15f, .10f),
                    V(side * .14f, 1.10f, .11f), V(side * .175f, 1.075f, .07f), .011f, .6f, _ => W("Head"));
            }
            Strand("Hair", V(0, 1.31f, -.145f), V(0, 1.43f, -.31f), V(.03f, 1.08f, -.43f), V(0, .985f, -.30f),
                .11f, .75f, p => Blend("Ponytail", "PonytailTip", Mathf.InverseLerp(1.26f, 1.04f, p.y)), 24, 32);
            for (int i = 0; i < 9; i++)
            {
                float a = i * 2 * Pi / 9, x = Mathf.Cos(a) * .065f, z = Mathf.Sin(a) * .04f;
                Strand(i % 4 == 0 ? "HairLight" : "Hair",
                    V(x * .2f, 1.31f, -.145f), V(x * 1.7f, 1.40f, -.31f + z),
                    V(x + .04f, 1.08f, -.43f + z), V(x * .5f - .015f, .985f + i * .006f, -.30f),
                    .047f, .85f, p => Blend("Ponytail", "PonytailTip", Mathf.InverseLerp(1.26f, 1.04f, p.y)), 16, 30);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Surface("Ruby", 20, 20, (u, v) => {
                    float r = Mathf.Sin(Pi * Mathf.Clamp(v, .0001f, .9999f));
                    return V(side * (.008f + .103f * v), 1.365f + .025f * v + Mathf.Sin(u * 2 * Pi) * .039f * r,
                        -.139f + Mathf.Cos(u * 2 * Pi) * .021f * r);
                }, _ => W("Bow"));
                Strand("Ruby", V(side * .008f, 1.36f, -.15f), V(side * .075f, 1.32f, -.16f),
                    V(side * .055f, 1.27f, -.19f), V(side * .08f, 1.23f, -.20f), .019f, .13f, _ => W("Bow"), 10, 18, false);
            }
            Ellipsoid("Ruby", V(0, 1.366f, -.126f), V(.025f, .025f, .021f), "Bow");
        }
        static BoneWeight DressWeight(Vector3 p)
        {
            if (p.y > .77f) return Blend("Spine", "Chest", Mathf.InverseLerp(.77f, .94f, p.y));
            float down = Mathf.InverseLerp(.76f, .48f, p.y);
            float side = Mathf.Abs(p.x) / Mathf.Max(.0001f, Mathf.Abs(p.x) + Mathf.Abs(p.z));
            return new BoneWeight {
                boneIndex0 = boneList.IndexOf(bones["Hips"]), weight0 = 1 - down,
                boneIndex1 = boneList.IndexOf(bones[p.x < 0 ? "LeftSkirt" : "RightSkirt"]), weight1 = down * side,
                boneIndex2 = boneList.IndexOf(bones[p.z > 0 ? "FrontSkirt" : "BackSkirt"]), weight2 = down * (1 - side)
            };
        }
        static Vector3 SkirtPoint(float u, float v, float extra = 0)
        {
            float t = u * 2 * Pi, radius = Mathf.Lerp(.116f, .218f, v);
            float folds = 1 + (.018f + .023f * v) * Mathf.Cos(t * 16 + .2f * Mathf.Sin(t * 3));
            return V((radius * folds + extra) * Mathf.Cos(t), .765f - .303f * v,
                (radius * .74f * folds + extra) * Mathf.Sin(t));
        }
        static void Body()
        {
            Surface("Ivory", 64, 18, (u, v) => {
                float t = u * 2 * Pi, y = Mathf.Lerp(.97f, .75f, v);
                float width = Mathf.Lerp(.085f, .117f, Mathf.Sin(v * Pi * .5f));
                return V(width * Mathf.Cos(t), y, .072f * Mathf.Sin(t) * (1 + .025f * Mathf.Sin(t * 14)));
            }, DressWeight);
            Surface("Ivory", 80, 28, (u, v) => SkirtPoint(u, v), DressWeight);
            Surface("Ivory", 80, 8, (u, v) => {
                var p = SkirtPoint(u, .92f + .08f * v, .006f * Mathf.Sin(u * 2 * Pi * 24));
                p.y -= .012f * v; return p;
            }, DressWeight);
            Surface("Ruby", 80, 3, (u, v) => SkirtPoint(u, .84f + .018f * v, .0015f), DressWeight);
            Surface("Ruby", 64, 3, (u, v) => { float t = u * 2 * Pi;
                return V(.12f * Mathf.Cos(t), .777f - .022f * v, .076f * Mathf.Sin(t)); }, _ => W("Spine"));
            // Rounded collar leaves and a narrow ruby edge lie over the bodice.
            for (int side = -1; side <= 1; side += 2)
            {
                Surface("Ivory", 32, 12, (u, v) => {
                    float a = u * 2 * Pi, r = Mathf.Clamp(1 - v, .0001f, .9999f);
                    float y = .966f + .025f * r * Mathf.Sin(a);
                    return V(side * .040f + .040f * r * Mathf.Cos(a), y,
                        .073f + .008f * (1 - r * r) - .4f * (y - .955f));
                }, _ => W("Chest"));
                Line("Ruby", t => { float y = .966f + .025f * Mathf.Sin(t * 2 * Pi);
                    return V(side * .04f + .04f * Mathf.Cos(t * 2 * Pi), y, .074f - .4f * (y - .955f)); }, .0015f, "Chest");
            }
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "Left" : "Right";
                Ellipsoid("Ivory", V(side * .15f, .922f, 0), V(.075f, .072f, .073f), s + "UpperArm", 32,
                    q => V(q.x * (1 + .012f * Mathf.Sin(q.y * 20)), q.y, q.z));
                Strand("Skin", V(side * .178f, .887f, 0), V(side * .205f, .80f, 0),
                    V(side * .23f, .71f, .006f), V(side * .25f, .641f, .018f), .041f, .86f,
                    p => Blend(s + "UpperArm", s + "LowerArm", Mathf.InverseLerp(.81f, .73f, p.y)), 24, 24, false, .55f);
                Ellipsoid("Skin", V(side * .257f, .612f, .019f), V(.024f, .041f, .015f), s + "Hand");
                for (int f = 0; f < 4; f++)
                {
                    float x = side * (.237f + f * .012f), endY = .557f + .008f * Mathf.Abs(f - 1.5f);
                    Strand("Skin", V(x, .60f, .019f), V(x + side * .002f, .582f, .021f),
                        V(x + side * .003f, endY + .008f, .027f), V(x + side * .002f, endY, .031f),
                        .0065f, .90f, _ => W(s + "Hand"), 10, 12, false);
                    Ellipsoid("Skin", V(x + side * .002f, endY, .031f), V(.0065f, .0065f, .006f), s + "Hand", 12);
                }
                Strand("Skin", V(side * .238f, .63f, .025f), V(side * .222f, .619f, .041f),
                    V(side * .223f, .60f, .044f), V(side * .228f, .59f, .039f), .008f, .85f, _ => W(s + "Hand"), 10, 12, false);
                Surface("Skin", 32, 24, (u, v) => {
                    float y = Mathf.Lerp(.56f, .101f, v), r = Mathf.Lerp(.052f, .030f, v) + .003f * Mathf.Sin(v * 2 * Pi);
                    return V(side * .075f + r * Mathf.Cos(u * 2 * Pi), y, r * .92f * Mathf.Sin(u * 2 * Pi));
                }, p => p.y >= .30f ? Blend(s + "UpperLeg", s + "LowerLeg", Mathf.InverseLerp(.34f, .26f, p.y)) : W(s + "LowerLeg"));
                Surface("Sock", 40, 12, (u, v) => {
                    float t = u * 2 * Pi, y = Mathf.Lerp(.18f, .09f, v), r = .034f + .001f * Mathf.Cos(t * 24);
                    return V(side * .075f + r * Mathf.Cos(t), y, r * Mathf.Sin(t));
                }, _ => W(s + "LowerLeg"));
                Ellipsoid("Sole", V(side * .075f, .014f, .044f), V(.061f, .014f, .111f), s + "Foot", 40);
                Surface("Shoe", 40, 20, (u, v) => {
                    // A dome meeting the sole around its whole rim, rather than a
                    // full ellipsoid whose raised toe appears detached when lifted.
                    float p = Mathf.Max(.0001f, v) * Pi * .5f, a = u * 2 * Pi;
                    return V(side * .075f + .059f * Mathf.Sin(p) * Mathf.Cos(a),
                        .014f + .091f * Mathf.Cos(p), .041f + .108f * Mathf.Sin(p) * Mathf.Sin(a));
                }, _ => W(s + "Foot"));
                Line("Shoe", t => V(side * .075f + (t - .5f) * .106f, .095f + .012f * Mathf.Sin(t * Pi), .026f), .007f, s + "Foot");
                Ellipsoid("Brass", V(side * .13f, .097f, .029f), V(.006f, .009f, .009f), s + "Foot", 16);
            }
            for (int side = -1; side <= 1; side += 2)
                Ellipsoid("Ruby", V(side * .035f, .763f, -.083f), V(.036f, .024f, .011f), "Spine");
        }

        public static void Build()
        {
            System.IO.Directory.CreateDirectory(Folder + "/Materials"); AssetDatabase.Refresh();
            bones.Clear(); boneList.Clear(); materialIds.Clear(); materials.Clear(); vertices.Clear(); normals.Clear(); uv.Clear(); weights.Clear(); indices.Clear();
            var go = new GameObject("Yuna_Study"); root = go.transform;
            try
            {
                Material("Skin", "#F1BC95"); Material("Ear", "#D68E79"); Material("Blush", "#EAA28E");
                Material("EyeWhite", "#FFF5DF", .45f); Material("Lash", "#342019"); Material("Iris", "#87502C", .50f);
                Material("Pupil", "#1D110D", .50f); Material("Lip", "#8E5044"); Material("LipSoft", "#CB8C79");
                Material("Hair", "#60341F", .30f); Material("HairLight", "#78462A", .28f); Material("HairDark", "#4C291C", .28f);
                Material("Ruby", "#9D2834", .25f); Material("Ivory", "#E9DCC0", .12f); Material("Sock", "#E7E3D9", .10f);
                Material("Shoe", "#4F3024", .28f); Material("Sole", "#2F211C", .16f); Material("Brass", "#A98A47", .35f, true);
                Skeleton(); Face(); Hair(); Body();
                // Bring the first study toward the approved four-head silhouette:
                // a shorter face and narrower child shoulders, without rescaling feet or gameplay.
                var headBones = new HashSet<int>(new[] {"Head", "Ponytail", "PonytailTip", "Bow"}.Select(n => boneList.IndexOf(bones[n])));
                var armBones = new HashSet<int>(boneList.Select((b, i) => new { b.name, i })
                    .Where(b => b.name.Contains("Arm") || b.name.EndsWith("Hand")).Select(b => b.i));
                Vector3 neck = bones["Neck"].position;
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (headBones.Contains(weights[i].boneIndex0)) vertices[i] = neck + (vertices[i] - neck) * .88f;
                    if (armBones.Contains(weights[i].boneIndex0)) vertices[i] = V(vertices[i].x * .70f, vertices[i].y, vertices[i].z);
                }
                foreach (int i in headBones) boneList[i].localPosition *= .88f;
                foreach (int i in armBones) { var p = boneList[i].localPosition; p.x *= .70f; boneList[i].localPosition = p; }
                float minY = vertices.Min(p => p.y), maxY = vertices.Max(p => p.y), scale = 1.4f / (maxY - minY);
                for (int i = 0; i < vertices.Count; i++) vertices[i] = (vertices[i] - Vector3.up * minY) * scale;
                foreach (var b in boneList) b.localPosition *= scale;
                bones["Hips"].localPosition -= Vector3.up * minY * scale;
                var mesh = new Mesh { name = "Yuna_StudyMesh", indexFormat = IndexFormat.UInt32 };
                mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv); mesh.boneWeights = weights.ToArray();
                mesh.subMeshCount = materials.Count;
                for (int i = 0; i < indices.Count; i++) mesh.SetTriangles(indices[i], i);
                mesh.bindposes = boneList.Select(b => b.worldToLocalMatrix * root.localToWorldMatrix).ToArray(); mesh.RecalculateBounds();
                string meshPath = Folder + "/Yuna_StudyMesh.asset";
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (existing == null) AssetDatabase.CreateAsset(mesh, meshPath);
                else
                {
                    // Copy mesh buffers explicitly and mark the existing asset dirty;
                    // native Mesh buffers must survive regeneration and reimport.
                    existing.Clear(); existing.indexFormat = IndexFormat.UInt32;
                    existing.vertices = mesh.vertices; existing.normals = mesh.normals; existing.uv = mesh.uv;
                    existing.boneWeights = mesh.boneWeights; existing.bindposes = mesh.bindposes;
                    existing.subMeshCount = mesh.subMeshCount;
                    for (int i = 0; i < mesh.subMeshCount; i++) existing.SetTriangles(mesh.GetTriangles(i), i);
                    existing.RecalculateBounds(); EditorUtility.SetDirty(existing);
                    UnityEngine.Object.DestroyImmediate(mesh); mesh = existing;
                }
                var renderer = go.AddComponent<SkinnedMeshRenderer>(); renderer.sharedMesh = mesh; renderer.sharedMaterials = materials.ToArray();
                renderer.bones = boneList.ToArray(); renderer.rootBone = bones["Hips"];
                renderer.localBounds = new Bounds(V(0, .70f, 0), V(1.3f, 1.8f, 1.4f)); renderer.updateWhenOffscreen = false;
                var view = go.AddComponent<YunaCharacterView>(); view.Configure(scale);
                PrefabUtility.SaveAsPrefabAsset(go, PrefabPath); AssetDatabase.SaveAssets();
                Debug.Log($"YUNA_STUDY_BUILT vertices={mesh.vertexCount} triangles={mesh.triangles.Length/3} bones={boneList.Count} height={mesh.bounds.size.y:F4}");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        public static void ApplyToScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var player = UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>();
            Install(player); EditorSceneManager.SaveScene(player.gameObject.scene);
            Debug.Log("YUNA_STUDY_INSTALLED: visual replaced; CharacterController, lights, route, camera and audio retained.");
        }
        public static void Install(Yuna25DPlayerController player)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) return;
            var old = player.transform.Find("Body_Visual");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
            visual.name = "Body_Visual"; visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity; visual.transform.localScale = Vector3.one;
        }
    }
}
