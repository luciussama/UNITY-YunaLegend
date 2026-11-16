using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Gera malhas 3D organicas esculpidas com UV, normais faciais suavizadas (estilo ArcSys)
    /// e texturas pintadas a mao (T_Yuna_Dress_Painted, T_Yuna_Hair_Painted) fieis a Yuna_Profile_Runtime_v1.png.
    /// Tambem suporta substituir automaticamente por FBX/GLB externo em Assets/Art/ToyRoom/Models/ se presente.
    /// </summary>
    public static class ToyRoomYunaSculptor
    {
        public const string ModelsFolder = "Assets/Art/ToyRoom/Models";
        public const string CustomModelPathFbx = ModelsFolder + "/Yuna_ToyRoom.fbx";
        public const string CustomModelPathGlb = ModelsFolder + "/Yuna_ToyRoom.glb";
        private const string DressTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_Dress_Painted.png";
        private const string HairTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_Hair_Painted.png";

        private static float Pi => Mathf.PI;

        public static void BuildYuna3D(
            Transform parent,
            out Transform bodyRoot,
            out Transform legLeftPivot,
            out Transform legRightPivot,
            out Transform armLeftPivot,
            out Transform armRightPivot,
            out Transform ponytailPivot)
        {
            Directory.CreateDirectory(ModelsFolder);
            EnsurePaintedTextures();

            var bodyGo = new GameObject("Yuna_BodyRoot");
            bodyGo.transform.SetParent(parent, false);
            bodyRoot = bodyGo.transform;

            // Se houver um FBX/GLB externo importado pelo artista, usa-o prioritariamente
            var customPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CustomModelPathFbx)
                ?? AssetDatabase.LoadAssetAtPath<GameObject>(CustomModelPathGlb);
            if (customPrefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(customPrefab, bodyRoot);
                inst.name = "Yuna_ImportedMesh";
                legLeftPivot = FindOrCreatePivot(bodyRoot, "Leg_L_Pivot", new Vector3(-0.08f, 0.38f, 0f));
                legRightPivot = FindOrCreatePivot(bodyRoot, "Leg_R_Pivot", new Vector3(0.08f, 0.38f, 0f));
                armLeftPivot = FindOrCreatePivot(bodyRoot, "Arm_L_Pivot", new Vector3(-0.14f, 0.86f, 0f));
                armRightPivot = FindOrCreatePivot(bodyRoot, "Arm_R_Pivot", new Vector3(0.14f, 0.86f, 0f));
                ponytailPivot = FindOrCreatePivot(bodyRoot, "Ponytail_Pivot", new Vector3(0f, 1.32f, -0.14f));
                return;
            }

            // Materiais pictoricos com texturas pintadas a mao e contorno sepia suave
            var skinMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.97f, 0.80f, 0.69f), outlineWidth: 0.0045f);
            var blushMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.94f, 0.56f, 0.52f), outlineWidth: 0f);
            var scleraMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.99f, 0.96f, 0.90f), outlineWidth: 0f);
            var irisMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.52f, 0.28f, 0.14f), outlineWidth: 0f);
            var pupilMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.14f, 0.08f, 0.06f), outlineWidth: 0f);
            var lashMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.22f, 0.12f, 0.09f), outlineWidth: 0f);
            var glintMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, emission: Color.white * 0.65f, outlineWidth: 0f);

            var hairMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, HairTexPath, outlineWidth: 0.0055f);
            var dressMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, DressTexPath, outlineWidth: 0.005f);
            var ivoryClothMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.95f, 0.89f, 0.76f), outlineWidth: 0.0045f);
            var coralMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.80f, 0.29f, 0.30f), outlineWidth: 0.004f);
            var scrunchieMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.87f, 0.48f, 0.47f), outlineWidth: 0.0045f);
            var sockMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.93f, 0.87f, 0.75f), outlineWidth: 0.004f);
            var shoeMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.52f, 0.14f, 0.15f), outlineWidth: 0.0045f);
            var soleMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.24f, 0.14f, 0.11f), outlineWidth: 0.003f);

            // 1. Corpo / Vestido Esculpido (Busto + Saia Plissada em Sino + Gola Peter Pan + Laco no Peito)
            BuildTorsoAndSkirt(bodyRoot, dressMat, ivoryClothMat, coralMat, skinMat);

            // 2. Cabeca Chibi Expressiva (levemente inclinada -10 graus para leitura perfeita na camera isometrica)
            var headRoot = new GameObject("Head_Root").transform;
            headRoot.SetParent(bodyRoot, false);
            headRoot.localPosition = new Vector3(0f, 0.96f, 0f);
            headRoot.localRotation = Quaternion.Euler(-10f, 0f, 0f);

            BuildSculptedHeadAndFace(headRoot, skinMat, blushMat, scleraMat, irisMat, pupilMat, lashMat, glintMat, hairMat);

            // 3. Rabo de Cavalo Alto Cacheado Volumoso + Xuxinha Rosa Ondulada
            ponytailPivot = new GameObject("Ponytail_Pivot").transform;
            ponytailPivot.SetParent(headRoot, false);
            ponytailPivot.localPosition = new Vector3(0f, 0.34f, -0.14f);
            BuildScrunchieAndCurlyPonytail(ponytailPivot, scrunchieMat, hairMat);

            // 4. Bracos Articulados com Mangas Bufantes Plissadas
            armLeftPivot = new GameObject("Arm_L_Pivot").transform;
            armLeftPivot.SetParent(bodyRoot, false);
            armLeftPivot.localPosition = new Vector3(-0.135f, 0.88f, 0f);
            BuildSculptedArm(armLeftPivot, -1, ivoryClothMat, coralMat, skinMat);

            armRightPivot = new GameObject("Arm_R_Pivot").transform;
            armRightPivot.SetParent(bodyRoot, false);
            armRightPivot.localPosition = new Vector3(0.135f, 0.88f, 0f);
            BuildSculptedArm(armRightPivot, 1, ivoryClothMat, coralMat, skinMat);

            // 5. Pernas Articuladas com Meias de Babado e Sapatos Mary Jane Burgundy
            legLeftPivot = new GameObject("Leg_L_Pivot").transform;
            legLeftPivot.SetParent(bodyRoot, false);
            legLeftPivot.localPosition = new Vector3(-0.075f, 0.44f, 0f);
            BuildSculptedLegAndShoe(legLeftPivot, -1, skinMat, sockMat, shoeMat, soleMat);

            legRightPivot = new GameObject("Leg_R_Pivot").transform;
            legRightPivot.SetParent(bodyRoot, false);
            legRightPivot.localPosition = new Vector3(0.075f, 0.44f, 0f);
            BuildSculptedLegAndShoe(legRightPivot, 1, skinMat, sockMat, shoeMat, soleMat);
        }

        public static void BuildNandaFairySculpted(Transform parent)
        {
            var coreMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.58f, 0.94f, 1.0f),
                emission: new Color(0.28f, 0.78f, 1.0f),
                outlineWidth: 0f);
            var wingMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.76f, 0.96f, 1.0f),
                emission: new Color(0.22f, 0.64f, 0.92f),
                outlineWidth: 0f);

            var bodyMesh = CreateParametricMesh("Nanda_Body", 24, 16, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = u * 2f * Pi;
                return new Vector3(0.09f * Mathf.Sin(p) * Mathf.Cos(a), 0.11f * Mathf.Cos(p), 0.09f * Mathf.Sin(p) * Mathf.Sin(a));
            });
            AttachMesh("Nanda_Core", parent, bodyMesh, coreMat);

            // 4 asas organicas em gota de fada (sem caixas retangulares!)
            for (int side = -1; side <= 1; side += 2)
            {
                var upperWing = CreateParametricMesh($"Nanda_UpperWing_{side}", 20, 12, (u, v) =>
                {
                    float r = Mathf.Sin(v * Pi);
                    float x = side * (0.04f + 0.22f * v);
                    float y = 0.06f * v + 0.07f * r * Mathf.Sin(u * 2f * Pi);
                    float z = -0.03f - 0.04f * v + 0.012f * r * Mathf.Cos(u * 2f * Pi);
                    return new Vector3(x, y, z);
                });
                AttachMesh($"Wing_Upper_{side}", parent, upperWing, wingMat);

                var lowerWing = CreateParametricMesh($"Nanda_LowerWing_{side}", 16, 10, (u, v) =>
                {
                    float r = Mathf.Sin(v * Pi);
                    float x = side * (0.03f + 0.15f * v);
                    float y = -0.05f * v + 0.045f * r * Mathf.Sin(u * 2f * Pi);
                    float z = -0.03f - 0.03f * v + 0.01f * r * Mathf.Cos(u * 2f * Pi);
                    return new Vector3(x, y, z);
                });
                AttachMesh($"Wing_Lower_{side}", parent, lowerWing, wingMat);
            }
        }

        private static void BuildTorsoAndSkirt(Transform bodyRoot, Material dressMat, Material ivoryMat, Material coralMat, Material skinMat)
        {
            // Pescoco
            var neckMesh = CreateParametricMesh("Yuna_Neck", 24, 8, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = Mathf.Lerp(0.89f, 0.98f, v);
                return new Vector3(0.046f * Mathf.Cos(a), y, 0.044f * Mathf.Sin(a));
            });
            AttachMesh("Neck", bodyRoot, neckMesh, skinMat);

            // Busto + Saia Plissada Continua em Sino (zero degrau cilindrico na cintura!)
            var dressMesh = CreateParametricMesh("Yuna_DressBell", 72, 36, (u, v) =>
            {
                float a = u * 2f * Pi;
                if (v < 0.26f)
                {
                    float tv = v / 0.26f;
                    float y = Mathf.Lerp(0.91f, 0.72f, tv);
                    float rx = Mathf.Lerp(0.088f, 0.112f, Mathf.Sin(tv * Pi * 0.5f));
                    float rz = Mathf.Lerp(0.072f, 0.088f, Mathf.Sin(tv * Pi * 0.5f));
                    return new Vector3(rx * Mathf.Cos(a), y, rz * Mathf.Sin(a));
                }
                else
                {
                    float sv = (v - 0.26f) / 0.74f;
                    float y = Mathf.Lerp(0.72f, 0.34f, sv);
                    float flare = Mathf.Pow(sv, 0.85f);
                    float baseRx = Mathf.Lerp(0.112f, 0.285f, flare);
                    float baseRz = Mathf.Lerp(0.088f, 0.255f, flare);
                    float pleats = 1f + (0.016f + 0.032f * sv) * Mathf.Cos(a * 14f);
                    float hemWave = sv > 0.82f ? 0.010f * Mathf.Sin(a * 28f) * ((sv - 0.82f) / 0.18f) : 0f;
                    return new Vector3(baseRx * pleats * Mathf.Cos(a), y + hemWave, baseRz * pleats * Mathf.Sin(a));
                }
            });
            AttachMesh("Dress_BellSkirt", bodyRoot, dressMesh, dressMat);

            // Faixa da cintura
            var sashMesh = CreateParametricMesh("Yuna_WaistSash", 48, 6, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = Mathf.Lerp(0.735f, 0.705f, v);
                return new Vector3(0.116f * Mathf.Cos(a), y, 0.092f * Mathf.Sin(a));
            });
            AttachMesh("Waist_Sash", bodyRoot, sashMesh, ivoryMat);

            // Gola Peter Pan ondulada com borda coral
            var collarMesh = CreateParametricMesh("Yuna_Collar", 56, 10, (u, v) =>
            {
                float a = u * 2f * Pi;
                float r = Mathf.Lerp(0.072f, 0.145f, v) * (1f + 0.04f * Mathf.Cos(a * 16f));
                float y = 0.905f - 0.032f * v + 0.005f * Mathf.Sin(a * 16f) * v;
                return new Vector3(r * Mathf.Cos(a), y, r * 0.86f * Mathf.Sin(a));
            });
            AttachMesh("PeterPan_Collar", bodyRoot, collarMesh, dressMat);

            // Laco coral no peito (2 lacos + no + 2 fitas caidas)
            for (int side = -1; side <= 1; side += 2)
            {
                var loopMesh = CreateParametricMesh($"ChestBow_Loop_{side}", 20, 12, (u, v) =>
                {
                    float s = Mathf.Sin(v * Pi);
                    return new Vector3(
                        side * (0.01f + 0.048f * v),
                        0.865f + 0.022f * s * Mathf.Sin(u * 2f * Pi),
                        0.092f + 0.012f * s * Mathf.Cos(u * 2f * Pi));
                });
                AttachMesh($"ChestBow_Loop_{side}", bodyRoot, loopMesh, coralMat);

                var ribbonMesh = CreateCurveTubeMesh(
                    $"ChestBow_Tail_{side}",
                    new Vector3(side * 0.012f, 0.855f, 0.095f),
                    new Vector3(side * 0.028f, 0.815f, 0.105f),
                    new Vector3(side * 0.036f, 0.775f, 0.108f),
                    new Vector3(side * 0.042f, 0.740f, 0.102f),
                    0.012f,
                    0.35f,
                    12,
                    14);
                AttachMesh($"ChestBow_Tail_{side}", bodyRoot, ribbonMesh, coralMat);
            }
        }

        private static void BuildSculptedHeadAndFace(
            Transform headRoot,
            Material skinMat,
            Material blushMat,
            Material scleraMat,
            Material irisMat,
            Material pupilMat,
            Material lashMat,
            Material glintMat,
            Material hairMat)
        {
            // Cabeca com bochechas infantis e normais faciais suavizadas estilo ArcSys
            var headMesh = CreateParametricMesh("Yuna_Head", 64, 32, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = u * 2f * Pi;
                Vector3 q = new Vector3(Mathf.Sin(p) * Mathf.Cos(a), Mathf.Cos(p), Mathf.Sin(p) * Mathf.Sin(a));
                float cheekBulge = 1f + 0.08f * Mathf.Exp(-Mathf.Pow((q.y + 0.18f) * 3.5f, 2f)) * Mathf.Max(0f, q.z);
                float chinTaper = Mathf.SmoothStep(0.78f, 1.0f, Mathf.InverseLerp(-1.0f, -0.15f, q.y));
                q.x *= chinTaper * cheekBulge;
                q.z *= cheekBulge;

                Vector3 pos = new Vector3(0f, 0.17f, 0.01f) + Vector3.Scale(q, new Vector3(0.185f, 0.195f, 0.175f));
                // Narizinho delicado
                if (q.z > 0.2f)
                {
                    float nose = 0.016f * Mathf.Exp(-Mathf.Pow(pos.x / 0.020f, 2f) - Mathf.Pow((pos.y - 0.145f) / 0.024f, 2f));
                    pos.z += nose;
                }

                return pos;
            }, blendArcSysFaceNormals: true);
            AttachMesh("Head_Mesh", headRoot, headMesh, skinMat);

            // Orelhas, Olhos Grandes Expressivos e Blush
            for (int side = -1; side <= 1; side += 2)
            {
                float ex = side * 0.076f;
                var earMesh = CreateEllipsoidMesh($"Ear_{side}", new Vector3(side * 0.182f, 0.15f, -0.01f), new Vector3(0.028f, 0.042f, 0.022f), 18);
                AttachMesh($"Ear_{side}", headRoot, earMesh, skinMat);

                var cheekMesh = CreateFacePatchMesh($"Blush_{side}", side * 0.105f, 0.118f, 0.034f, 0.020f, 0.003f);
                AttachMesh($"Blush_{side}", headRoot, cheekMesh, blushMat);

                var scleraMesh = CreateFacePatchMesh($"Sclera_{side}", ex, 0.168f, 0.044f, 0.048f, 0.003f);
                AttachMesh($"Sclera_{side}", headRoot, scleraMesh, scleraMat);

                var irisMesh = CreateFacePatchMesh($"Iris_{side}", ex, 0.168f, 0.030f, 0.036f, 0.005f);
                AttachMesh($"Iris_{side}", headRoot, irisMesh, irisMat);

                var pupilMesh = CreateFacePatchMesh($"Pupil_{side}", ex, 0.168f, 0.017f, 0.022f, 0.007f);
                AttachMesh($"Pupil_{side}", headRoot, pupilMesh, pupilMat);

                var glint1 = CreateFacePatchMesh($"Glint1_{side}", ex - 0.009f, 0.182f, 0.009f, 0.010f, 0.009f);
                AttachMesh($"Glint1_{side}", headRoot, glint1, glintMat);

                var glint2 = CreateFacePatchMesh($"Glint2_{side}", ex + 0.009f, 0.155f, 0.0045f, 0.005f, 0.009f);
                AttachMesh($"Glint2_{side}", headRoot, glint2, glintMat);

                var lashMesh = CreateFacePatchMesh($"UpperLash_{side}", ex, 0.198f, 0.046f, 0.014f, 0.006f);
                AttachMesh($"UpperLash_{side}", headRoot, lashMesh, lashMat);
            }

            // Domo de Cabelo Volumoso + Franja Esculpida em Mechas + Mechas Laterais
            var hairCap = CreateParametricMesh("Yuna_HairCap", 64, 28, (u, v) =>
            {
                float a = u * 2f * Pi;
                float frontCut = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(a)), 0.6f);
                float phi = 0.001f + v * Mathf.Lerp(2.22f, 1.05f, frontCut);
                float grooves = 1f + 0.022f * Mathf.Cos(a * 16f + phi * 2.5f);
                return new Vector3(
                    0.205f * Mathf.Sin(phi) * Mathf.Cos(a) * grooves,
                    0.18f + 0.225f * Mathf.Cos(phi),
                    -0.005f + 0.195f * Mathf.Sin(phi) * Mathf.Sin(a) * grooves);
            });
            AttachMesh("Hair_Cap", headRoot, hairCap, hairMat);

            // 7 mechas de franja curvas sobre a testa
            for (int i = 0; i < 7; i++)
            {
                float t = (i - 3f) / 3f;
                float x = t * 0.135f;
                var bangMesh = CreateCurveTubeMesh(
                    $"Bang_{i}",
                    new Vector3(x * 0.55f, 0.36f, 0.05f),
                    new Vector3(x * 0.90f, 0.38f, 0.145f),
                    new Vector3(x * 1.04f + 0.01f, 0.29f, 0.192f),
                    new Vector3(x * 0.98f, 0.225f + Mathf.Abs(t) * 0.015f, 0.172f),
                    0.036f,
                    0.42f,
                    14,
                    18);
                AttachMesh($"Bang_{i}", headRoot, bangMesh, hairMat);
            }

            // Mechas laterais emoldurando as bochechas
            for (int side = -1; side <= 1; side += 2)
            {
                var sideLock = CreateCurveTubeMesh(
                    $"SideLock_{side}",
                    new Vector3(side * 0.14f, 0.32f, 0.06f),
                    new Vector3(side * 0.21f, 0.25f, 0.08f),
                    new Vector3(side * 0.20f, 0.15f, 0.07f),
                    new Vector3(side * 0.16f, 0.07f, 0.04f),
                    0.034f,
                    0.52f,
                    14,
                    18);
                AttachMesh($"SideLock_{side}", headRoot, sideLock, hairMat);
            }
        }

        private static void BuildScrunchieAndCurlyPonytail(Transform ponytailPivot, Material scrunchieMat, Material hairMat)
        {
            // Xuxinha rosa ondulada (torus frisado com 14 ondas)
            var scrunchieMesh = CreateParametricMesh("Yuna_Scrunchie", 48, 18, (u, v) =>
            {
                float a = u * 2f * Pi;
                float b = v * 2f * Pi;
                float ruffle = 1f + 0.22f * Mathf.Sin(a * 14f);
                float majorR = 0.068f;
                float minorR = 0.026f * ruffle;
                float r = majorR + minorR * Mathf.Cos(b);
                return new Vector3(
                    r * Mathf.Cos(a),
                    minorR * Mathf.Sin(b) + 0.01f * Mathf.Cos(a * 14f),
                    r * Mathf.Sin(a) * 0.85f);
            });
            var scGo = AttachMesh("Ruffled_Scrunchie", ponytailPivot, scrunchieMesh, scrunchieMat);
            scGo.transform.localRotation = Quaternion.Euler(52f, 0f, 0f);

            // Nucleo volumoso do rabo de cavalo + 11 cachos em S (fiel a Yuna_Profile_Runtime_v1.png)
            var coreTail = CreateCurveTubeMesh(
                "Ponytail_Core",
                new Vector3(0f, 0.01f, -0.02f),
                new Vector3(0f, 0.14f, -0.18f),
                new Vector3(0f, -0.12f, -0.32f),
                new Vector3(0f, -0.32f, -0.22f),
                0.095f,
                0.82f,
                20,
                24);
            AttachMesh("Ponytail_Core", ponytailPivot, coreTail, hairMat);

            for (int i = 0; i < 11; i++)
            {
                float angle = i * 2f * Pi / 11f;
                float ox = Mathf.Cos(angle) * 0.055f;
                float oy = Mathf.Sin(angle) * 0.045f;
                float curlTwist = (i % 2 == 0 ? 1f : -1f) * 0.04f;

                var curlMesh = CreateCurveTubeMesh(
                    $"Ponytail_Curl_{i}",
                    new Vector3(ox * 0.4f, 0.01f + oy * 0.3f, -0.02f),
                    new Vector3(ox * 1.65f, 0.15f + oy, -0.18f),
                    new Vector3(ox * 1.45f + curlTwist, -0.14f + oy * 0.5f, -0.34f - Mathf.Abs(ox) * 0.3f),
                    new Vector3(ox * 0.7f - curlTwist, -0.34f + (i % 3) * 0.025f, -0.22f),
                    0.048f,
                    0.85f,
                    14,
                    22);
                AttachMesh($"Ponytail_Curl_{i}", ponytailPivot, curlMesh, hairMat);
            }
        }

        private static void BuildSculptedArm(Transform armPivot, int side, Material ivoryMat, Material coralMat, Material skinMat)
        {
            // Manga bufante plissada em gota + punho com babado coral
            var sleeveMesh = CreateParametricMesh($"Sleeve_{side}", 32, 16, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = u * 2f * Pi;
                float gathers = 1f + 0.06f * Mathf.Cos(a * 10f) * Mathf.Sin(p);
                return new Vector3(
                    side * 0.025f + 0.068f * Mathf.Sin(p) * Mathf.Cos(a) * gathers,
                    -0.04f + 0.068f * Mathf.Cos(p),
                    0.068f * Mathf.Sin(p) * Mathf.Sin(a) * gathers);
            });
            AttachMesh("Puff_Sleeve", armPivot, sleeveMesh, ivoryMat);

            var cuffMesh = CreateParametricMesh($"SleeveCuff_{side}", 24, 6, (u, v) =>
            {
                float a = u * 2f * Pi;
                float r = 0.046f + 0.006f * v + 0.004f * Mathf.Sin(a * 12f);
                return new Vector3(side * 0.032f + r * Mathf.Cos(a), -0.105f - 0.018f * v, r * Mathf.Sin(a));
            });
            AttachMesh("Sleeve_Cuff", armPivot, cuffMesh, coralMat);

            var forearm = CreateCurveTubeMesh(
                $"Forearm_{side}",
                new Vector3(side * 0.035f, -0.11f, 0f),
                new Vector3(side * 0.045f, -0.18f, 0.01f),
                new Vector3(side * 0.050f, -0.24f, 0.02f),
                new Vector3(side * 0.048f, -0.29f, 0.03f),
                0.034f,
                0.88f,
                14,
                14);
            AttachMesh("Forearm", armPivot, forearm, skinMat);
        }

        private static void BuildSculptedLegAndShoe(Transform legPivot, int side, Material skinMat, Material sockMat, Material shoeMat, Material soleMat)
        {
            var legMesh = CreateParametricMesh($"Leg_{side}", 24, 14, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = Mathf.Lerp(0f, -0.22f, v);
                float r = Mathf.Lerp(0.046f, 0.034f, v);
                return new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
            });
            AttachMesh("Leg_Upper", legPivot, legMesh, skinMat);

            var sockMesh = CreateParametricMesh($"Sock_{side}", 28, 12, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = Mathf.Lerp(-0.20f, -0.36f, v);
                float ruffle = v < 0.18f ? 0.006f * Mathf.Sin(a * 14f) : 0.0015f * Mathf.Cos(a * 18f);
                float r = 0.038f + ruffle;
                return new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
            });
            AttachMesh("Ribbed_Sock", legPivot, sockMesh, sockMat);

            var shoeUpper = CreateParametricMesh($"Shoe_{side}", 32, 16, (u, v) =>
            {
                float p = Mathf.Max(0.001f, v) * Pi * 0.5f;
                float a = u * 2f * Pi;
                return new Vector3(
                    0.056f * Mathf.Sin(p) * Mathf.Cos(a),
                    -0.425f + 0.082f * Mathf.Cos(p),
                    0.032f + 0.098f * Mathf.Sin(p) * Mathf.Sin(a));
            });
            AttachMesh("MaryJane_Shoe", legPivot, shoeUpper, shoeMat);

            var soleMesh = CreateEllipsoidMesh($"Sole_{side}", new Vector3(0f, -0.425f, 0.032f), new Vector3(0.058f, 0.014f, 0.102f), 24);
            AttachMesh("Shoe_Sole", legPivot, soleMesh, soleMat);
        }

        private static void EnsurePaintedTextures()
        {
            Directory.CreateDirectory("Assets/Art/ToyRoom/Materials");

            if (!File.Exists(DressTexPath))
            {
                GeneratePaintedDressTexture(DressTexPath);
            }

            if (!File.Exists(HairTexPath))
            {
                GeneratePaintedHairTexture(HairTexPath);
            }

            AssetDatabase.ImportAsset(DressTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(HairTexPath, ImportAssetOptions.ForceUpdate);
        }

        private static void GeneratePaintedDressTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseCream = new Color(0.95f, 0.89f, 0.76f);
            Color foldShadow = new Color(0.84f, 0.74f, 0.59f);
            Color coralRed = new Color(0.79f, 0.28f, 0.29f);
            Color coralFlower = new Color(0.87f, 0.48f, 0.46f);
            Color laceCream = new Color(0.98f, 0.94f, 0.84f);
            Color sepiaLine = new Color(0.38f, 0.22f, 0.16f);

            for (int y = 0; y < size; y++)
            {
                float v = 1f - (float)y / (size - 1); // v=0 topo, v=1 barra
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float pleat = 0.5f + 0.5f * Mathf.Sin(u * Pi * 2f * 14f);
                    float grain = (Mathf.PerlinNoise(u * 42f, v * 42f) - 0.5f) * 0.04f;
                    Color col = Color.Lerp(foldShadow, baseCream, Mathf.Lerp(0.35f, 1f, pleat));
                    col.r = Mathf.Clamp01(col.r + grain);
                    col.g = Mathf.Clamp01(col.g + grain);
                    col.b = Mathf.Clamp01(col.b + grain);

                    // Estampa de florzinhas de 5 petalas coral (fiel a Yuna_Profile_Runtime_v1.png)
                    if (v > 0.60f && v < 0.76f)
                    {
                        float cellU = Mathf.Repeat(u * 12f, 1f) - 0.5f;
                        float cellV = (v - 0.68f) / 0.07f;
                        float r = Mathf.Sqrt(cellU * cellU + cellV * cellV);
                        float ang = Mathf.Atan2(cellV, cellU);
                        float petalRadius = 0.26f + 0.11f * Mathf.Cos(ang * 5f);
                        if (r < 0.08f)
                        {
                            col = baseCream;
                        }
                        else if (r < petalRadius)
                        {
                            col = coralFlower;
                        }
                    }

                    // Barra ondulada coral-carmim + babado de renda creme
                    float wave = 0.018f * Mathf.Sin(u * Pi * 2f * 18f);
                    float vWave = v + wave;
                    if (vWave >= 0.79f && vWave <= 0.90f)
                    {
                        col = coralRed;
                        if (Mathf.Abs(vWave - 0.79f) < 0.008f || Mathf.Abs(vWave - 0.90f) < 0.008f)
                        {
                            col = Color.Lerp(col, sepiaLine, 0.55f);
                        }
                    }
                    else if (vWave > 0.90f)
                    {
                        float lacePleat = 0.5f + 0.5f * Mathf.Sin(u * Pi * 2f * 36f);
                        col = Color.Lerp(foldShadow, laceCream, lacePleat);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GeneratePaintedHairTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color darkBrown = new Color(0.21f, 0.10f, 0.06f);
            Color midChestnut = new Color(0.36f, 0.19f, 0.11f);
            Color warmHighlight = new Color(0.56f, 0.33f, 0.19f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float strands = Mathf.PerlinNoise(u * 64f, v * 8f) * 0.65f
                        + Mathf.PerlinNoise(u * 140f, v * 16f) * 0.35f;
                    Color col = Color.Lerp(darkBrown, midChestnut, strands);

                    float sheenBand = Mathf.Exp(-Mathf.Pow((v - 0.58f - 0.04f * Mathf.Sin(u * Pi * 16f)) / 0.11f, 2f));
                    col = Color.Lerp(col, warmHighlight, sheenBand * strands * 0.85f);
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static Vector3 EvaluateHeadPoint(float x, float y, float offset)
        {
            float qy = (y - 0.17f) / 0.195f;
            float chinTaper = Mathf.SmoothStep(0.78f, 1.0f, Mathf.InverseLerp(-1.0f, -0.15f, qy));
            float qx = x / (0.185f * chinTaper);
            float zSq = Mathf.Max(0f, 1f - qx * qx - qy * qy);
            float z = 0.01f + 0.175f * Mathf.Sqrt(zSq);
            return new Vector3(x, y, z + offset);
        }

        private static Mesh CreateFacePatchMesh(string name, float cx, float cy, float rx, float ry, float zOffset)
        {
            return CreateParametricMesh(name, 24, 8, (u, v) =>
            {
                float r = Mathf.Clamp(1f - v, 0.001f, 0.999f);
                float a = u * 2f * Pi;
                return EvaluateHeadPoint(cx + rx * r * Mathf.Cos(a), cy + ry * r * Mathf.Sin(a), zOffset);
            }, blendArcSysFaceNormals: true);
        }

        private static Mesh CreateEllipsoidMesh(string name, Vector3 center, Vector3 radius, int segments)
        {
            return CreateParametricMesh(name, segments, segments / 2, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = u * 2f * Pi;
                return center + new Vector3(
                    radius.x * Mathf.Sin(p) * Mathf.Cos(a),
                    radius.y * Mathf.Cos(p),
                    radius.z * Mathf.Sin(p) * Mathf.Sin(a));
            });
        }

        private static Mesh CreateCurveTubeMesh(
            string name,
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float radius,
            float flattenZ,
            int sides,
            int segments)
        {
            return CreateParametricMesh(name, sides, segments, (u, v) =>
            {
                float t = Mathf.Clamp(v, 0.001f, 0.999f);
                Vector3 center = Bezier(p0, p1, p2, p3, t);
                Vector3 tangent = (Bezier(p0, p1, p2, p3, Mathf.Min(1f, t + 0.002f))
                    - Bezier(p0, p1, p2, p3, Mathf.Max(0f, t - 0.002f))).normalized;
                Vector3 refAxis = Mathf.Abs(Vector3.Dot(tangent, Vector3.right)) > 0.95f ? Vector3.up : Vector3.right;
                Vector3 bx = Vector3.Cross(tangent, refAxis).normalized;
                Vector3 bz = Vector3.Cross(tangent, bx).normalized;
                float taper = Mathf.Pow(Mathf.Sin(Pi * t), 0.38f) * (1f - 0.45f * t);
                float r = radius * taper;
                float a = u * 2f * Pi;
                return center + r * (Mathf.Cos(a) * bx + Mathf.Sin(a) * flattenZ * bz);
            });
        }

        private static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float s = 1f - t;
            return s * s * s * a + 3f * s * s * t * b + 3f * s * t * t * c + t * t * t * d;
        }

        public static Mesh CreateParametricMesh(
            string meshName,
            int nu,
            int nv,
            Func<float, float, Vector3> evalPoint,
            bool blendArcSysFaceNormals = false)
        {
            var verts = new List<Vector3>((nu + 1) * (nv + 1));
            var norms = new List<Vector3>((nu + 1) * (nv + 1));
            var uvs = new List<Vector2>((nu + 1) * (nv + 1));
            var tris = new List<int>(nu * nv * 6);

            const float eps = 0.001f;
            for (int j = 0; j <= nv; j++)
            {
                float v = (float)j / nv;
                for (int i = 0; i <= nu; i++)
                {
                    float u = (float)i / nu;
                    Vector3 p = evalPoint(u, v);
                    Vector3 du = evalPoint(u + eps, v) - evalPoint(u - eps, v);
                    Vector3 dv = evalPoint(u, Mathf.Min(1f, v + eps)) - evalPoint(u, Mathf.Max(0f, v - eps));
                    Vector3 n = Vector3.Cross(du.normalized, dv.normalized).normalized;
                    if (n.sqrMagnitude < 0.1f)
                    {
                        n = p.normalized;
                    }

                    if (blendArcSysFaceNormals && p.z > 0f)
                    {
                        Vector3 proxyNormal = new Vector3(p.x * 1.6f, (p.y - 0.15f) * 0.9f + 0.25f, 0.85f).normalized;
                        n = Vector3.Slerp(n, proxyNormal, 0.68f).normalized;
                    }

                    verts.Add(p);
                    norms.Add(n);
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int j = 0; j < nv; j++)
            {
                for (int i = 0; i < nu; i++)
                {
                    int a = j * (nu + 1) + i;
                    int b = a + nu + 1;
                    tris.Add(a);
                    tris.Add(a + 1);
                    tris.Add(b);
                    tris.Add(a + 1);
                    tris.Add(b + 1);
                    tris.Add(b);
                }
            }

            var mesh = new Mesh { name = meshName };
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static GameObject AttachMesh(string name, Transform parent, Mesh mesh, Material mat)
        {
            Directory.CreateDirectory(ModelsFolder + "/GeneratedMeshes");
            string assetPath = $"{ModelsFolder}/GeneratedMeshes/{mesh.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, assetPath);
                existing = mesh;
            }
            else
            {
                existing.Clear();
                existing.vertices = mesh.vertices;
                existing.normals = mesh.normals;
                existing.uv = mesh.uv;
                existing.triangles = mesh.triangles;
                existing.RecalculateBounds();
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(mesh);
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = existing;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static Transform FindOrCreatePivot(Transform root, string name, Vector3 fallbackLocalPos)
        {
            var found = root.Find(name);
            if (found != null)
            {
                return found;
            }

            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.localPosition = fallbackLocalPos;
            return go.transform;
        }
    }
}
