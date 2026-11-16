using System.IO;
using UnityEditor;
using UnityEngine;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Fabrica de modelos 3D esculpidos e materiais pictoricos (Diablo IV x Legend of Mana)
    /// preservando a assinatura visual da Fase 1 em ambiente 100% 3D, com suporte a FBX/GLB em Assets/Art/ToyRoom/Models/.
    /// </summary>
    public static class ToyRoomVisualFactory
    {
        private const string PainterlyShaderName = "YunaLegend/ToyRoom_Painterly3D";
        private const string MaterialsFolder = "Assets/Art/ToyRoom/Materials";
        private static int materialCounter;

        public static void ResetMaterialCounter()
        {
            materialCounter = 0;
            Directory.CreateDirectory(MaterialsFolder);
            Directory.CreateDirectory(ToyRoomYunaSculptor.ModelsFolder);
        }

        public static Material CreatePainterlyMaterial(
            Color tint,
            string texturePath = null,
            Vector2? tiling = null,
            Color? emission = null,
            float outlineWidth = 0.005f)
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
            mat.SetColor("_ShadowColor", new Color(0.42f, 0.34f, 0.50f, 1f));
            mat.SetColor("_RimColor", new Color(1.0f, 0.92f, 0.76f, 1f));
            mat.SetColor("_OutlineColor", new Color(0.20f, 0.11f, 0.08f, 1f));
            mat.SetFloat("_OutlineWidth", outlineWidth);

            Texture2D tex = null;
            if (!string.IsNullOrEmpty(texturePath))
            {
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            }

            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            mat.SetTextureScale("_BaseMap", tiling ?? Vector2.one);
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
            ToyRoomYunaSculptor.BuildYuna3D(
                parent,
                out bodyRoot,
                out legLeftPivot,
                out legRightPivot,
                out armLeftPivot,
                out armRightPivot,
                out ponytailPivot);
        }

        public static void BuildNandaFairy3D(Transform parent)
        {
            ToyRoomYunaSculptor.BuildNandaFairySculpted(parent);
        }

        public static void BuildFurnitureDetail(Transform parent, FurnitureSpec spec)
        {
            if (TryInstantiateCustomModel(parent, $"Furniture_{spec.Id}"))
            {
                return;
            }

            var woodMat = CreatePainterlyMaterial(new Color(0.76f, 0.52f, 0.34f), "Assets/Materials/T_Furniture_Wood_Painted.png", outlineWidth: 0.005f);
            var darkWoodMat = CreatePainterlyMaterial(new Color(0.50f, 0.32f, 0.19f), "Assets/Materials/T_DarkOak_Planks.png", outlineWidth: 0.005f);
            var goldMat = CreatePainterlyMaterial(new Color(0.94f, 0.78f, 0.34f), outlineWidth: 0.004f);

            switch (spec.Id)
            {
                case "bookshelf":
                    CreateVisualPrimitive(PrimitiveType.Cube, "Frame", parent, new Vector3(0f, 0.75f, 0f), new Vector3(spec.Width, 1.50f, spec.Depth), woodMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Crown", parent, new Vector3(0f, 1.54f, 0f), new Vector3(spec.Width + 0.10f, 0.08f, spec.Depth + 0.08f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Books_Row1", parent, new Vector3(0f, 0.50f, -0.05f), new Vector3(spec.Width * 0.86f, 0.32f, spec.Depth * 0.72f), CreatePainterlyMaterial(new Color(0.42f, 0.64f, 0.84f), "Assets/Materials/T_Book_Spines_Painted.png", outlineWidth: 0.004f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Books_Row2", parent, new Vector3(0f, 1.05f, -0.05f), new Vector3(spec.Width * 0.86f, 0.32f, spec.Depth * 0.72f), CreatePainterlyMaterial(new Color(0.84f, 0.38f, 0.38f), "Assets/Materials/T_Book_Spines_Painted.png", outlineWidth: 0.004f));
                    break;
                case "wardrobe":
                    CreateVisualPrimitive(PrimitiveType.Cube, "Cabinet", parent, new Vector3(0f, 0.90f, 0f), new Vector3(spec.Width, 1.80f, spec.Depth), woodMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Crown", parent, new Vector3(0f, 1.84f, 0f), new Vector3(spec.Width + 0.14f, 0.10f, spec.Depth + 0.10f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Knob_L", parent, new Vector3(-0.12f, 0.95f, -spec.Depth * 0.52f), Vector3.one * 0.09f, goldMat);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Knob_R", parent, new Vector3(0.12f, 0.95f, -spec.Depth * 0.52f), Vector3.one * 0.09f, goldMat);
                    break;
                case "rocking-horse":
                    var whiteWood = CreatePainterlyMaterial(new Color(0.95f, 0.91f, 0.84f), "Assets/Materials/T_Pastel_ToyWood.png", outlineWidth: 0.005f);
                    var saddleMat = CreatePainterlyMaterial(new Color(0.82f, 0.26f, 0.26f), outlineWidth: 0.004f);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Rocker_L", parent, new Vector3(-0.22f, 0.06f, 0f), new Vector3(0.08f, 0.08f, spec.Depth), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Rocker_R", parent, new Vector3(0.22f, 0.06f, 0f), new Vector3(0.08f, 0.08f, spec.Depth), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Capsule, "Body", parent, new Vector3(0f, 0.52f, 0f), new Vector3(0.34f, 0.40f, 0.34f), whiteWood, new Vector3(90f, 0f, 0f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Saddle", parent, new Vector3(0f, 0.68f, 0f), new Vector3(0.28f, 0.08f, 0.30f), saddleMat);
                    CreateVisualPrimitive(PrimitiveType.Capsule, "Neck", parent, new Vector3(0f, 0.82f, 0.28f), new Vector3(0.20f, 0.26f, 0.22f), whiteWood, new Vector3(24f, 0f, 0f));
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Head", parent, new Vector3(0f, 1.02f, 0.40f), new Vector3(0.20f, 0.20f, 0.28f), whiteWood);
                    break;
                case "table":
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Top", parent, new Vector3(0f, 0.64f, 0f), new Vector3(spec.Width, 0.06f, spec.Depth), woodMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_1", parent, new Vector3(-0.48f, 0.32f, -0.32f), new Vector3(0.10f, 0.32f, 0.10f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_2", parent, new Vector3(0.48f, 0.32f, -0.32f), new Vector3(0.10f, 0.32f, 0.10f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_3", parent, new Vector3(-0.48f, 0.32f, 0.32f), new Vector3(0.10f, 0.32f, 0.10f), darkWoodMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Leg_4", parent, new Vector3(0.48f, 0.32f, 0.32f), new Vector3(0.10f, 0.32f, 0.10f), darkWoodMat);
                    break;
                case "armchair":
                    var velvetMat = CreatePainterlyMaterial(new Color(0.64f, 0.40f, 0.62f), "Assets/Materials/T_Velvet_Purple.png", outlineWidth: 0.005f);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Seat", parent, new Vector3(0f, 0.36f, 0f), new Vector3(spec.Width, 0.42f, spec.Depth), velvetMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Backrest", parent, new Vector3(0f, 0.82f, spec.Depth * 0.36f), new Vector3(spec.Width, 0.74f, 0.28f), velvetMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Arm_L", parent, new Vector3(-spec.Width * 0.42f, 0.56f, 0f), new Vector3(0.22f, 0.36f, spec.Depth * 0.92f), velvetMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Arm_R", parent, new Vector3(spec.Width * 0.42f, 0.56f, 0f), new Vector3(0.22f, 0.36f, spec.Depth * 0.92f), velvetMat);
                    break;
                case "block-fortress":
                    var blockMat = CreatePainterlyMaterial(new Color(0.95f, 0.88f, 0.78f), "Assets/Materials/T_ToyBlock_ABC.png", outlineWidth: 0.005f);
                    var roofMat = CreatePainterlyMaterial(new Color(0.84f, 0.30f, 0.34f), outlineWidth: 0.005f);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Keep", parent, new Vector3(0f, 0.48f, 0f), new Vector3(spec.Width * 0.88f, 0.96f, spec.Depth * 0.82f), blockMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Tower_L", parent, new Vector3(-0.74f, 0.85f, 0.32f), new Vector3(0.56f, 1.50f, 0.56f), blockMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Tower_R", parent, new Vector3(0.74f, 0.95f, -0.22f), new Vector3(0.56f, 1.70f, 0.56f), blockMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Roof_L", parent, new Vector3(-0.74f, 1.72f, 0.32f), new Vector3(0.60f, 0.16f, 0.60f), roofMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Roof_R", parent, new Vector3(0.74f, 1.92f, -0.22f), new Vector3(0.60f, 0.16f, 0.60f), roofMat);
                    break;
            }
        }

        public static void BuildToyModel3D(Transform parent, ToySpec spec)
        {
            if (TryInstantiateCustomModel(parent, $"Toy_{spec.Id}"))
            {
                return;
            }

            switch (spec.Id)
            {
                case "teddy":
                    var fur = CreatePainterlyMaterial(new Color(0.80f, 0.54f, 0.32f), "Assets/Materials/T_Bear_Fur_Painted.png", outlineWidth: 0.005f);
                    var muzzle = CreatePainterlyMaterial(new Color(0.96f, 0.88f, 0.76f), outlineWidth: 0.003f);
                    var buttonMat = CreatePainterlyMaterial(new Color(0.22f, 0.12f, 0.07f), outlineWidth: 0f);
                    var tie = CreatePainterlyMaterial(new Color(0.82f, 0.22f, 0.24f), outlineWidth: 0.004f);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Torso", parent, new Vector3(0f, 0.20f, 0f), new Vector3(0.36f, 0.36f, 0.30f), fur);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Head", parent, new Vector3(0f, 0.46f, 0f), new Vector3(0.36f, 0.30f, 0.30f), fur);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Ear_L", parent, new Vector3(-0.15f, 0.58f, 0f), new Vector3(0.14f, 0.14f, 0.09f), fur);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Ear_R", parent, new Vector3(0.15f, 0.58f, 0f), new Vector3(0.14f, 0.14f, 0.09f), fur);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Muzzle", parent, new Vector3(0f, 0.42f, 0.13f), new Vector3(0.15f, 0.11f, 0.09f), muzzle);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Eye_L", parent, new Vector3(-0.075f, 0.48f, 0.14f), Vector3.one * 0.048f, buttonMat);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Eye_R", parent, new Vector3(0.075f, 0.48f, 0.14f), Vector3.one * 0.048f, buttonMat);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Bowtie", parent, new Vector3(0f, 0.30f, 0.15f), new Vector3(0.16f, 0.07f, 0.05f), tie);
                    break;
                case "train":
                    var red = CreatePainterlyMaterial(new Color(0.84f, 0.24f, 0.24f), "Assets/Materials/T_Pastel_ToyWood.png", outlineWidth: 0.005f);
                    var blk = CreatePainterlyMaterial(new Color(0.22f, 0.20f, 0.24f), outlineWidth: 0.005f);
                    var gld = CreatePainterlyMaterial(new Color(0.94f, 0.78f, 0.32f), outlineWidth: 0.004f);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Boiler", parent, new Vector3(0f, 0.18f, 0.06f), new Vector3(0.24f, 0.22f, 0.24f), blk, new Vector3(90f, 0f, 0f));
                    CreateVisualPrimitive(PrimitiveType.Cube, "Cab", parent, new Vector3(0f, 0.24f, -0.14f), new Vector3(0.28f, 0.30f, 0.22f), red);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Chimney", parent, new Vector3(0f, 0.34f, 0.16f), new Vector3(0.08f, 0.10f, 0.08f), gld);
                    break;
                case "robot":
                    var blue = CreatePainterlyMaterial(new Color(0.30f, 0.60f, 0.90f), "Assets/Materials/T_Pastel_ToyWood.png", outlineWidth: 0.005f);
                    var ylw = CreatePainterlyMaterial(new Color(0.96f, 0.82f, 0.30f), outlineWidth: 0.004f);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Body", parent, new Vector3(0f, 0.22f, 0f), new Vector3(0.26f, 0.30f, 0.20f), blue);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Head", parent, new Vector3(0f, 0.44f, 0f), new Vector3(0.22f, 0.18f, 0.18f), blue);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Visor", parent, new Vector3(0f, 0.45f, 0.10f), new Vector3(0.16f, 0.06f, 0.03f), ylw);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Antenna", parent, new Vector3(0f, 0.58f, 0f), new Vector3(0.03f, 0.08f, 0.03f), ylw);
                    break;
                case "bunny":
                    var wht = CreatePainterlyMaterial(new Color(0.97f, 0.94f, 0.90f), "Assets/Materials/T_TeddyPlush.png", outlineWidth: 0.005f);
                    var pnk = CreatePainterlyMaterial(new Color(0.96f, 0.66f, 0.74f), outlineWidth: 0.003f);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Body", parent, new Vector3(0f, 0.16f, 0f), new Vector3(0.28f, 0.28f, 0.28f), wht);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Head", parent, new Vector3(0f, 0.36f, 0f), Vector3.one * 0.24f, wht);
                    CreateVisualPrimitive(PrimitiveType.Capsule, "Ear_L", parent, new Vector3(-0.07f, 0.54f, 0f), new Vector3(0.07f, 0.15f, 0.06f), wht, new Vector3(0f, 0f, 15f));
                    CreateVisualPrimitive(PrimitiveType.Capsule, "Ear_R", parent, new Vector3(0.07f, 0.54f, 0f), new Vector3(0.07f, 0.15f, 0.06f), wht, new Vector3(0f, 0f, -15f));
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Nose", parent, new Vector3(0f, 0.35f, 0.12f), Vector3.one * 0.05f, pnk);
                    break;
                case "duck":
                    var duckY = CreatePainterlyMaterial(new Color(0.98f, 0.84f, 0.26f), "Assets/Materials/T_Pastel_ToyWood.png", outlineWidth: 0.005f);
                    var bill = CreatePainterlyMaterial(new Color(0.96f, 0.52f, 0.18f), outlineWidth: 0.004f);
                    var crn = CreatePainterlyMaterial(new Color(0.96f, 0.78f, 0.24f), outlineWidth: 0.003f);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Body", parent, new Vector3(0f, 0.16f, 0f), new Vector3(0.28f, 0.22f, 0.34f), duckY);
                    CreateVisualPrimitive(PrimitiveType.Sphere, "Head", parent, new Vector3(0f, 0.32f, 0.10f), Vector3.one * 0.20f, duckY);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Bill", parent, new Vector3(0f, 0.30f, 0.22f), new Vector3(0.12f, 0.05f, 0.10f), bill);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Crown", parent, new Vector3(0f, 0.44f, 0.10f), new Vector3(0.10f, 0.05f, 0.10f), crn);
                    break;
                case "blocks":
                    var b1 = CreatePainterlyMaterial(new Color(0.90f, 0.34f, 0.34f), "Assets/Materials/T_ToyBlock_ABC.png", outlineWidth: 0.005f);
                    var b2 = CreatePainterlyMaterial(new Color(0.34f, 0.66f, 0.90f), "Assets/Materials/T_ToyBlock_ABC.png", outlineWidth: 0.005f);
                    var b3 = CreatePainterlyMaterial(new Color(0.94f, 0.80f, 0.30f), "Assets/Materials/T_ToyBlock_ABC.png", outlineWidth: 0.005f);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Block_1", parent, new Vector3(-0.10f, 0.10f, 0f), Vector3.one * 0.20f, b1);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Block_2", parent, new Vector3(0.11f, 0.10f, 0.04f), Vector3.one * 0.20f, b2);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Block_3", parent, new Vector3(0f, 0.30f, 0.02f), Vector3.one * 0.20f, b3);
                    break;
                case "drum":
                    var drumMat = CreatePainterlyMaterial(Color.white, "Assets/Materials/T_CircusDrum.png", outlineWidth: 0.005f);
                    var hoopMat = CreatePainterlyMaterial(new Color(0.84f, 0.58f, 0.26f), "Assets/Materials/T_Furniture_Wood_Painted.png", outlineWidth: 0.004f);
                    var headMat = CreatePainterlyMaterial(new Color(0.96f, 0.91f, 0.82f), outlineWidth: 0f);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Shell", parent, new Vector3(0f, 0.18f, 0f), new Vector3(0.36f, 0.16f, 0.36f), drumMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Rim_Top", parent, new Vector3(0f, 0.34f, 0f), new Vector3(0.39f, 0.03f, 0.39f), hoopMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Rim_Bot", parent, new Vector3(0f, 0.04f, 0f), new Vector3(0.39f, 0.03f, 0.39f), hoopMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Drumhead", parent, new Vector3(0f, 0.355f, 0f), new Vector3(0.34f, 0.01f, 0.34f), headMat);
                    break;
                default:
                    var boxMat = CreatePainterlyMaterial(new Color(0.76f, 0.40f, 0.84f), "Assets/Materials/T_Pastel_ToyWood.png", outlineWidth: 0.005f);
                    var crank = CreatePainterlyMaterial(new Color(0.94f, 0.78f, 0.30f), outlineWidth: 0.004f);
                    CreateVisualPrimitive(PrimitiveType.Cube, "Box", parent, new Vector3(0f, 0.18f, 0f), Vector3.one * 0.32f, boxMat);
                    CreateVisualPrimitive(PrimitiveType.Cylinder, "Crank", parent, new Vector3(0.20f, 0.18f, 0f), new Vector3(0.04f, 0.08f, 0.04f), crank, new Vector3(0f, 0f, 90f));
                    break;
            }
        }

        private static bool TryInstantiateCustomModel(Transform parent, string baseName)
        {
            string fbxPath = $"{ToyRoomYunaSculptor.ModelsFolder}/{baseName}.fbx";
            string glbPath = $"{ToyRoomYunaSculptor.ModelsFolder}/{baseName}.glb";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath)
                ?? AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            if (prefab == null)
            {
                return false;
            }

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            inst.name = $"{baseName}_ImportedMesh";
            return true;
        }
    }
}
