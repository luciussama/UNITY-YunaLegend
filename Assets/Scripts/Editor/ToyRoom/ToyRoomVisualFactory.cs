using System.IO;
using UnityEditor;
using UnityEngine;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Fabrica de modelos 3D esculpidos e materiais pictoricos (Diablo IV x Legend of Mana)
    /// da Fase 2 (Toy Room), integrando ToyRoomYunaSculptor e ToyRoomPropSculptor.
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
            ToyRoomYunaSculptor.EnsurePaintedTextures(forceRegenerate: true);
            ToyRoomPropSculptor.EnsurePaintedPropTextures(forceRegenerate: true);
        }

        public static Material CreatePainterlyMaterial(
            Color tint,
            string texturePath = null,
            Vector2? tiling = null,
            Color? emission = null,
            float outlineWidth = 0.0045f)
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
            mat.SetColor("_ShadowColor", new Color(0.66f, 0.56f, 0.64f, 1f));
            mat.SetColor("_RimColor", new Color(1.0f, 0.94f, 0.80f, 1f));
            mat.SetColor("_OutlineColor", new Color(0.22f, 0.12f, 0.09f, 1f));
            mat.SetFloat("_OutlineWidth", outlineWidth);
            mat.SetFloat("_BrushNoiseStrength", 0.045f);

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

            ToyRoomPropSculptor.BuildSculptedFurniture(parent, spec);
        }

        public static void BuildToyModel3D(Transform parent, ToySpec spec)
        {
            if (TryInstantiateCustomModel(parent, $"Toy_{spec.Id}"))
            {
                return;
            }

            ToyRoomPropSculptor.BuildSculptedToy(parent, spec);
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
