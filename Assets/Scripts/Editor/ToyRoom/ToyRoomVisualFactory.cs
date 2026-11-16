using UnityEditor;
using UnityEngine;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Fabrica de modelos 3D compostos e materiais pictoricos (Diablo IV x Legend of Mana)
    /// preservando a assinatura visual da Fase 1 em ambiente 100% 3D.
    /// </summary>
    public static class ToyRoomVisualFactory
    {
        private const string PainterlyShaderName = "YunaLegend/ToyRoom_Painterly3D";
        private const string MaterialsFolder = "Assets/Art/ToyRoom/Materials";
        private static int materialCounter;

        public static void ResetMaterialCounter()
        {
            materialCounter = 0;
            System.IO.Directory.CreateDirectory(MaterialsFolder);
        }

        public static Material CreatePainterlyMaterial(
            Color tint,
            string texturePath = null,
            Vector2? tiling = null,
            Color? emission = null,
            float outlineWidth = 0.011f)
        {
            var shader = Shader.Find(PainterlyShaderName) ?? Shader.Find("Universal Render Pipeline/Lit");
            string matPath = $"{MaterialsFolder}/M_ToyRoom_{materialCounter++:000}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_ShadowColor", new Color(0.36f, 0.30f, 0.46f, 1f));
            mat.SetColor("_RimColor", new Color(1.0f, 0.92f, 0.74f, 1f));
            mat.SetColor("_OutlineColor", new Color(0.24f, 0.14f, 0.10f, 1f));
            mat.SetFloat("_OutlineWidth", outlineWidth);

            Texture2D tex = null;
            if (!string.IsNullOrEmpty(texturePath))
            {
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            }

            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);

            if (tiling.HasValue)
            {
                mat.SetTextureScale("_BaseMap", tiling.Value);
            }
            else
            {
                mat.SetTextureScale("_BaseMap", Vector2.one);
            }

            mat.SetColor("_EmissionColor", emission ?? Color.black);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static GameObject CreateVisualPrimitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPos,
            Vector3 localScale,
            Material mat,
            Vector3? localEuler = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (localEuler.HasValue)
            {
                go.transform.localEulerAngles = localEuler.Value;
            }

            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && mat != null)
            {
                renderer.sharedMaterial = mat;
            }

            return go;
        }

        public static void BuildYunaCharacter3D(
            Transform parent,
            out Transform bodyRoot,
            out Transform legLeftPivot,
            out Transform legRightPivot,
            out Transform armLeftPivot,
            out Transform armRightPivot,
            out Transform ponytailPivot)
        {
            var skinMat = CreatePainterlyMaterial(new Color(0.98f, 0.82f, 0.71f), outlineWidth: 0.011f);
            var blushMat = CreatePainterlyMaterial(new Color(0.95f, 0.62f, 0.58f), outlineWidth: 0f);
            var hairMat = CreatePainterlyMaterial(new Color(0.36f, 0.20f, 0.12f), outlineWidth: 0.012f);
            var dressMat = CreatePainterlyMaterial(new Color(0.95f, 0.89f, 0.77f), outlineWidth: 0.011f);
            var sockMat = CreatePainterlyMaterial(new Color(0.93f, 0.87f, 0.76f), outlineWidth: 0.008f);
            var coralMat = CreatePainterlyMaterial(new Color(0.80f, 0.30f, 0.31f), outlineWidth: 0.009f);
            var scrunchieMat = CreatePainterlyMaterial(new Color(0.85f, 0.47f, 0.46f), outlineWidth: 0.009f);
            var shoeMat = CreatePainterlyMaterial(new Color(0.54f, 0.15f, 0.15f), outlineWidth: 0.010f);
            var eyeMat = CreatePainterlyMaterial(new Color(0.18f, 0.11f, 0.08f), outlineWidth: 0f);
            var glintMat = CreatePainterlyMaterial(Color.white, emission: Color.white * 0.4f, outlineWidth: 0f);

            var bodyGo = new GameObject("Yuna_BodyRoot");
            bodyGo.transform.SetParent(parent, false);
            bodyRoot = bodyGo.transform;

            // Pivots de pernas (meias creme + sapatinhos Mary Jane burgundy)
            legLeftPivot = new GameObject("Leg_L_Pivot").transform;
            legLeftPivot.SetParent(bodyRoot, false);
            legLeftPivot.localPosition = new Vector3(-0.10f, 0.36f, 0f);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_L", legLeftPivot, new Vector3(0f, -0.10f, 0f), new Vector3(0.09f, 0.10f, 0.09f), skinMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Sock_L", legLeftPivot, new Vector3(0f, -0.22f, 0f), new Vector3(0.10f, 0.09f, 0.10f), sockMat);
            CreateVisualPrimitive(PrimitiveType.Capsule, "Shoe_L", legLeftPivot, new Vector3(0f, -0.30f, 0.03f), new Vector3(0.12f, 0.08f, 0.18f), shoeMat, new Vector3(90f, 0f, 0f));

            legRightPivot = new GameObject("Leg_R_Pivot").transform;
            legRightPivot.SetParent(bodyRoot, false);
            legRightPivot.localPosition = new Vector3(0.10f, 0.36f, 0f);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_R", legRightPivot, new Vector3(0f, -0.10f, 0f), new Vector3(0.09f, 0.10f, 0.09f), skinMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Sock_R", legRightPivot, new Vector3(0f, -0.22f, 0f), new Vector3(0.10f, 0.09f, 0.10f), sockMat);
            CreateVisualPrimitive(PrimitiveType.Capsule, "Shoe_R", legRightPivot, new Vector3(0f, -0.30f, 0.03f), new Vector3(0.12f, 0.08f, 0.18f), shoeMat, new Vector3(90f, 0f, 0f));

            // Vestido vintage creme com babado coral-carmim e gola com laco
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Skirt_Base", bodyRoot, new Vector3(0f, 0.48f, 0f), new Vector3(0.50f, 0.18f, 0.50f), dressMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Skirt_CoralTrim", bodyRoot, new Vector3(0f, 0.35f, 0f), new Vector3(0.53f, 0.04f, 0.53f), coralMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Skirt_LaceBottom", bodyRoot, new Vector3(0f, 0.31f, 0f), new Vector3(0.54f, 0.02f, 0.54f), dressMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Torso", bodyRoot, new Vector3(0f, 0.72f, 0f), new Vector3(0.28f, 0.14f, 0.24f), dressMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "Collar", bodyRoot, new Vector3(0f, 0.85f, 0f), new Vector3(0.34f, 0.03f, 0.28f), dressMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "ChestBow", bodyRoot, new Vector3(0f, 0.82f, 0.14f), new Vector3(0.12f, 0.07f, 0.05f), coralMat);

            // Bracinhos com mangas bufantes
            armLeftPivot = new GameObject("Arm_L_Pivot").transform;
            armLeftPivot.SetParent(bodyRoot, false);
            armLeftPivot.localPosition = new Vector3(-0.19f, 0.80f, 0f);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Sleeve_L", armLeftPivot, new Vector3(-0.02f, -0.02f, 0f), new Vector3(0.15f, 0.14f, 0.15f), dressMat);
            CreateVisualPrimitive(PrimitiveType.Capsule, "Arm_L", armLeftPivot, new Vector3(-0.02f, -0.14f, 0.02f), new Vector3(0.08f, 0.12f, 0.08f), skinMat);

            armRightPivot = new GameObject("Arm_R_Pivot").transform;
            armRightPivot.SetParent(bodyRoot, false);
            armRightPivot.localPosition = new Vector3(0.19f, 0.80f, 0f);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Sleeve_R", armRightPivot, new Vector3(0.02f, -0.02f, 0f), new Vector3(0.15f, 0.14f, 0.15f), dressMat);
            CreateVisualPrimitive(PrimitiveType.Capsule, "Arm_R", armRightPivot, new Vector3(0.02f, -0.14f, 0.02f), new Vector3(0.08f, 0.12f, 0.08f), skinMat);

            // Cabeca chibi expressiva (42% da altura, olhos grandes com brilho, bochechas rosadas)
            CreateVisualPrimitive(PrimitiveType.Sphere, "Head", bodyRoot, new Vector3(0f, 1.16f, 0.02f), new Vector3(0.48f, 0.45f, 0.46f), skinMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Cheek_L", bodyRoot, new Vector3(-0.14f, 1.09f, 0.20f), new Vector3(0.08f, 0.05f, 0.04f), blushMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Cheek_R", bodyRoot, new Vector3(0.14f, 1.09f, 0.20f), new Vector3(0.08f, 0.05f, 0.04f), blushMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Eye_L", bodyRoot, new Vector3(-0.09f, 1.16f, 0.22f), new Vector3(0.065f, 0.085f, 0.04f), eyeMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Eye_R", bodyRoot, new Vector3(0.09f, 1.16f, 0.22f), new Vector3(0.065f, 0.085f, 0.04f), eyeMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Glint_L", bodyRoot, new Vector3(-0.08f, 1.18f, 0.24f), new Vector3(0.022f, 0.024f, 0.02f), glintMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Glint_R", bodyRoot, new Vector3(0.10f, 1.18f, 0.24f), new Vector3(0.022f, 0.024f, 0.02f), glintMat);

            // Cabelo castanho volumoso + franja + rabo de cavalo alto cacheado + xuxinha rosa ondulada
            CreateVisualPrimitive(PrimitiveType.Sphere, "Hair_Crown", bodyRoot, new Vector3(0f, 1.24f, -0.02f), new Vector3(0.52f, 0.42f, 0.49f), hairMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Hair_Bangs", bodyRoot, new Vector3(0f, 1.29f, 0.16f), new Vector3(0.42f, 0.20f, 0.22f), hairMat);

            ponytailPivot = new GameObject("Ponytail_Pivot").transform;
            ponytailPivot.SetParent(bodyRoot, false);
            ponytailPivot.localPosition = new Vector3(0f, 1.38f, -0.18f);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Scrunchie", ponytailPivot, new Vector3(0f, 0.02f, -0.02f), new Vector3(0.18f, 0.14f, 0.14f), scrunchieMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Curl_Top", ponytailPivot, new Vector3(0f, 0.04f, -0.14f), new Vector3(0.28f, 0.28f, 0.28f), hairMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Curl_Mid", ponytailPivot, new Vector3(0f, -0.12f, -0.22f), new Vector3(0.32f, 0.30f, 0.28f), hairMat);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Curl_Tip", ponytailPivot, new Vector3(0f, -0.26f, -0.20f), new Vector3(0.24f, 0.22f, 0.22f), hairMat);
        }

        public static void BuildNandaFairy3D(Transform parent)
        {
            var bodyMat = CreatePainterlyMaterial(new Color(0.42f, 0.88f, 1.0f), emission: new Color(0.20f, 0.65f, 0.90f));
            var wingMat = CreatePainterlyMaterial(new Color(0.75f, 0.96f, 1.0f), emission: new Color(0.30f, 0.72f, 0.95f));

            CreateVisualPrimitive(PrimitiveType.Sphere, "Nanda_Core", parent, Vector3.zero, Vector3.one * 0.24f, bodyMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "Wing_TopL", parent, new Vector3(-0.16f, 0.08f, -0.05f), new Vector3(0.24f, 0.12f, 0.02f), wingMat, new Vector3(0f, -18f, 22f));
            CreateVisualPrimitive(PrimitiveType.Cube, "Wing_TopR", parent, new Vector3(0.16f, 0.08f, -0.05f), new Vector3(0.24f, 0.12f, 0.02f), wingMat, new Vector3(0f, 18f, -22f));
            CreateVisualPrimitive(PrimitiveType.Cube, "Wing_BotL", parent, new Vector3(-0.13f, -0.06f, -0.05f), new Vector3(0.18f, 0.09f, 0.02f), wingMat, new Vector3(0f, -15f, -18f));
            CreateVisualPrimitive(PrimitiveType.Cube, "Wing_BotR", parent, new Vector3(0.13f, -0.06f, -0.05f), new Vector3(0.18f, 0.09f, 0.02f), wingMat, new Vector3(0f, 15f, 18f));
        }

        public static void BuildFurnitureDetail(Transform parent, FurnitureSpec spec)
        {
            var woodMat = CreatePainterlyMaterial(new Color(0.74f, 0.50f, 0.32f), "Assets/Materials/T_Furniture_Wood_Painted.png");
            var darkWoodMat = CreatePainterlyMaterial(new Color(0.48f, 0.30f, 0.18f), "Assets/Materials/T_DarkOak_Planks.png");
            var goldMat = CreatePainterlyMaterial(new Color(0.92f, 0.76f, 0.34f));

            switch (spec.Id)
            {
                case "bookshelf":
                    CreateVisualPrimitive(PrimitiveType.Cube, "Frame", parent, new Vector3(0f, 0.75f, 0f), new Vector3(spec.Width, 1.50f, spec.Depth), woodMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Books_Row1", parent, new Vector3(0f, 0.50f, -0.05f), new Vector3(spec.Width * 0.85f, 0.28f, spec.Depth * 0.7f), CreatePainterlyMaterial(new Color(0.32f, 0.56f, 0.74f), "Assets/Materials/T_Book_Spines_Painted.png"));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Books_Row2", parent, new Vector3(0f, 1.05f, -0.05f), new Vector3(spec.Width * 0.85f, 0.28f, spec.Depth * 0.7f), CreatePainterlyMaterial(new Color(0.78f, 0.34f, 0.34f), "Assets/Materials/T_Book_Spines_Painted.png"));
                    break;
                case "wardrobe":
                    CreateVisualPrimitive(PrimitiveType.Cube, "Cabinet", parent, new Vector3(0f, 0.90f, 0f), new Vector3(spec.Width, 1.80f, spec.Depth), woodMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Crown", parent, new Vector3(0f, 1.84f, 0f), new Vector3(spec.Width + 0.12f, 0.10f, spec.Depth + 0.10f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Knob_L", parent, new Vector3(-0.12f, 0.95f, -spec.Depth * 0.52f), Vector3.one * 0.08f, goldMat);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Knob_R", parent, new Vector3(0.12f, 0.95f, -spec.Depth * 0.52f), Vector3.one * 0.08f, goldMat);
                    break;
                case "rocking-horse":
                    var whiteWood = CreatePainterlyMaterial(new Color(0.94f, 0.90f, 0.84f));
                    var saddleMat = CreatePainterlyMaterial(new Color(0.82f, 0.26f, 0.26f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Rocker_L", parent, new Vector3(-0.22f, 0.06f, 0f), new Vector3(0.08f, 0.08f, spec.Depth), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Rocker_R", parent, new Vector3(0.22f, 0.06f, 0f), new Vector3(0.08f, 0.08f, spec.Depth), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Capsule, "Body", parent, new Vector3(0f, 0.52f, 0f), new Vector3(0.32f, 0.38f, 0.32f), whiteWood, new Vector3(90f, 0f, 0f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Saddle", parent, new Vector3(0f, 0.66f, 0f), new Vector3(0.26f, 0.08f, 0.28f), saddleMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Neck", parent, new Vector3(0f, 0.80f, 0.28f), new Vector3(0.18f, 0.42f, 0.22f), whiteWood, new Vector3(24f, 0f, 0f));
                    break;
                case "table":
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Top", parent, new Vector3(0f, 0.64f, 0f), new Vector3(spec.Width, 0.06f, spec.Depth), woodMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_1", parent, new Vector3(-0.45f, 0.32f, -0.30f), new Vector3(0.10f, 0.32f, 0.10f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_2", parent, new Vector3(0.45f, 0.32f, -0.30f), new Vector3(0.10f, 0.32f, 0.10f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_3", parent, new Vector3(-0.45f, 0.32f, 0.30f), new Vector3(0.10f, 0.32f, 0.10f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_4", parent, new Vector3(0.45f, 0.32f, 0.30f), new Vector3(0.10f, 0.32f, 0.10f), darkWoodMat);
                    break;
                case "armchair":
                    var velvetMat = CreatePainterlyMaterial(new Color(0.58f, 0.36f, 0.56f), "Assets/Materials/T_Velvet_Purple.png");
                    CreateVisualPrimitive(PrimitiveType.Cube, "Seat", parent, new Vector3(0f, 0.36f, 0f), new Vector3(spec.Width, 0.42f, spec.Depth), velvetMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Backrest", parent, new Vector3(0f, 0.80f, spec.Depth * 0.36f), new Vector3(spec.Width, 0.72f, 0.26f), velvetMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Arm_L", parent, new Vector3(-spec.Width * 0.42f, 0.54f, 0f), new Vector3(0.20f, 0.36f, spec.Depth * 0.92f), velvetMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Arm_R", parent, new Vector3(spec.Width * 0.42f, 0.54f, 0f), new Vector3(0.20f, 0.36f, spec.Depth * 0.92f), velvetMat);
                    break;
                case "block-fortress":
                    var blockMat = CreatePainterlyMaterial(new Color(0.92f, 0.85f, 0.76f), "Assets/Materials/T_ToyBlock_ABC.png");
                    var roofMat = CreatePainterlyMaterial(new Color(0.84f, 0.32f, 0.36f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Keep", parent, new Vector3(0f, 0.48f, 0f), new Vector3(spec.Width * 0.88f, 0.96f, spec.Depth * 0.82f), blockMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Tower_L", parent, new Vector3(-0.68f, 0.85f, 0.30f), new Vector3(0.52f, 1.50f, 0.52f), blockMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Tower_R", parent, new Vector3(0.68f, 0.95f, -0.20f), new Vector3(0.52f, 1.70f, 0.52f), blockMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Roof_L", parent, new Vector3(-0.68f, 1.72f, 0.30f), new Vector3(0.56f, 0.16f, 0.56f), roofMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Roof_R", parent, new Vector3(0.68f, 1.92f, -0.20f), new Vector3(0.56f, 0.16f, 0.56f), roofMat);
                    break;
            }
        }

        public static void BuildToyModel3D(Transform parent, ToySpec spec)
        {
            switch (spec.Id)
            {
                case "teddy":
                    var fur = CreatePainterlyMaterial(new Color(0.76f, 0.50f, 0.30f), "Assets/Materials/T_Bear_Fur_Painted.png", outlineWidth: 0.010f);
                    var muzzle = CreatePainterlyMaterial(new Color(0.95f, 0.87f, 0.74f), outlineWidth: 0.006f);
                    var buttonMat = CreatePainterlyMaterial(new Color(0.24f, 0.14f, 0.08f), outlineWidth: 0f);
                    var tie = CreatePainterlyMaterial(new Color(0.80f, 0.22f, 0.24f), outlineWidth: 0.008f);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Torso", parent, new Vector3(0f, 0.18f, 0f), new Vector3(0.34f, 0.34f, 0.28f), fur);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Head", parent, new Vector3(0f, 0.44f, 0f), new Vector3(0.32f, 0.28f, 0.28f), fur);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Ear_L", parent, new Vector3(-0.14f, 0.56f, 0f), Vector3.one * 0.12f, fur);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Ear_R", parent, new Vector3(0.14f, 0.56f, 0f), Vector3.one * 0.12f, fur);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Muzzle", parent, new Vector3(0f, 0.40f, 0.12f), new Vector3(0.14f, 0.10f, 0.08f), muzzle);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Eye_L", parent, new Vector3(-0.07f, 0.46f, 0.13f), Vector3.one * 0.045f, buttonMat);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Eye_R", parent, new Vector3(0.07f, 0.46f, 0.13f), Vector3.one * 0.045f, buttonMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Bowtie", parent, new Vector3(0f, 0.29f, 0.14f), new Vector3(0.15f, 0.07f, 0.05f), tie);
                    break;
                case "train":
                    var red = CreatePainterlyMaterial(new Color(0.84f, 0.22f, 0.22f));
                    var blk = CreatePainterlyMaterial(new Color(0.18f, 0.18f, 0.22f));
                    var gld = CreatePainterlyMaterial(new Color(0.92f, 0.76f, 0.30f));
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Boiler", parent, new Vector3(0f, 0.18f, 0.06f), new Vector3(0.24f, 0.22f, 0.24f), blk, new Vector3(90f, 0f, 0f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Cab", parent, new Vector3(0f, 0.24f, -0.14f), new Vector3(0.28f, 0.30f, 0.22f), red);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Chimney", parent, new Vector3(0f, 0.34f, 0.16f), new Vector3(0.08f, 0.10f, 0.08f), gld);
                    break;
                case "robot":
                    var blue = CreatePainterlyMaterial(new Color(0.26f, 0.56f, 0.88f));
                    var ylw = CreatePainterlyMaterial(new Color(0.95f, 0.80f, 0.28f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Body", parent, new Vector3(0f, 0.22f, 0f), new Vector3(0.26f, 0.30f, 0.20f), blue);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Head", parent, new Vector3(0f, 0.44f, 0f), new Vector3(0.22f, 0.18f, 0.18f), blue);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Visor", parent, new Vector3(0f, 0.45f, 0.10f), new Vector3(0.16f, 0.06f, 0.03f), ylw);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Antenna", parent, new Vector3(0f, 0.58f, 0f), new Vector3(0.03f, 0.08f, 0.03f), ylw);
                    break;
                case "bunny":
                    var wht = CreatePainterlyMaterial(new Color(0.96f, 0.94f, 0.92f));
                    var pnk = CreatePainterlyMaterial(new Color(0.96f, 0.68f, 0.76f));
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Body", parent, new Vector3(0f, 0.16f, 0f), new Vector3(0.26f, 0.28f, 0.26f), wht);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Head", parent, new Vector3(0f, 0.36f, 0f), Vector3.one * 0.22f, wht);
                    CreateVisualPrimitive(PrimitiveType.Capsule, "Ear_L", parent, new Vector3(-0.07f, 0.54f, 0f), new Vector3(0.06f, 0.14f, 0.06f), wht, new Vector3(0f, 0f, 15f));
                    CreateVisualPrimitive(PrimitiveType.Capsule, "Ear_R", parent, new Vector3(0.07f, 0.54f, 0f), new Vector3(0.06f, 0.14f, 0.06f), wht, new Vector3(0f, 0f, -15f));
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Nose", parent, new Vector3(0f, 0.35f, 0.11f), Vector3.one * 0.05f, pnk);
                    break;
                case "duck":
                    var duckY = CreatePainterlyMaterial(new Color(0.98f, 0.84f, 0.24f));
                    var bill = CreatePainterlyMaterial(new Color(0.96f, 0.52f, 0.18f));
                    var crn = CreatePainterlyMaterial(new Color(0.96f, 0.78f, 0.24f));
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Body", parent, new Vector3(0f, 0.16f, 0f), new Vector3(0.28f, 0.22f, 0.34f), duckY);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Head", parent, new Vector3(0f, 0.32f, 0.10f), Vector3.one * 0.20f, duckY);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Bill", parent, new Vector3(0f, 0.30f, 0.22f), new Vector3(0.12f, 0.05f, 0.10f), bill);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Crown", parent, new Vector3(0f, 0.44f, 0.10f), new Vector3(0.10f, 0.05f, 0.10f), crn);
                    break;
                case "blocks":
                    var b1 = CreatePainterlyMaterial(new Color(0.88f, 0.32f, 0.32f), "Assets/Materials/T_ToyBlock_ABC.png");
                    var b2 = CreatePainterlyMaterial(new Color(0.32f, 0.64f, 0.88f), "Assets/Materials/T_ToyBlock_ABC.png");
                    var b3 = CreatePainterlyMaterial(new Color(0.92f, 0.78f, 0.28f), "Assets/Materials/T_ToyBlock_ABC.png");
                    CreateVisualPrimitive(PrimitiveType.Cube, "Block_1", parent, new Vector3(-0.10f, 0.10f, 0f), Vector3.one * 0.20f, b1);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Block_2", parent, new Vector3(0.11f, 0.10f, 0.04f), Vector3.one * 0.20f, b2);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Block_3", parent, new Vector3(0f, 0.30f, 0.02f), Vector3.one * 0.20f, b3);
                    break;
                case "drum":
                    var drumMat = CreatePainterlyMaterial(Color.white, "Assets/Materials/T_CircusDrum.png", outlineWidth: 0.010f);
                    var hoopMat = CreatePainterlyMaterial(new Color(0.84f, 0.58f, 0.26f), outlineWidth: 0.008f);
                    var headMat = CreatePainterlyMaterial(new Color(0.95f, 0.90f, 0.80f), outlineWidth: 0f);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Shell", parent, new Vector3(0f, 0.18f, 0f), new Vector3(0.36f, 0.16f, 0.36f), drumMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Rim_Top", parent, new Vector3(0f, 0.34f, 0f), new Vector3(0.39f, 0.03f, 0.39f), hoopMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Rim_Bot", parent, new Vector3(0f, 0.04f, 0f), new Vector3(0.39f, 0.03f, 0.39f), hoopMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Drumhead", parent, new Vector3(0f, 0.355f, 0f), new Vector3(0.34f, 0.01f, 0.34f), headMat);
                    break;
                default: // jack-in-the-box
                    var boxMat = CreatePainterlyMaterial(new Color(0.74f, 0.38f, 0.82f), "Assets/Materials/T_Pastel_ToyWood.png");
                    var crank = CreatePainterlyMaterial(new Color(0.94f, 0.78f, 0.30f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Box", parent, new Vector3(0f, 0.18f, 0f), Vector3.one * 0.32f, boxMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Crank", parent, new Vector3(0.20f, 0.18f, 0f), new Vector3(0.04f, 0.08f, 0.04f), crank, new Vector3(0f, 0f, 90f));
                    break;
            }
        }
    }
}
