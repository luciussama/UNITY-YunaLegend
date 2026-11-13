using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomPaintedPlatformSetup
    {
        static Texture2D Texture(string name, bool repeat)
        {
            string path = "Assets/Materials/" + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.sRGBTexture = true;
            importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 4;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) throw new System.InvalidOperationException("Texture2D not imported: " + path);
            return texture;
        }

        public static void ConfigureMaterials()
        {
            var fur = Texture("T_Bear_Fur_Painted", true);
            var spines = Texture("T_Book_Spines_Painted", false);
            var cover = Texture("T_Book_Cover_Painted", false);
            if (fur == null || spines == null || cover == null) throw new System.InvalidOperationException("Painted platform textures are required.");
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Bear_Fur.mat");
            material.SetTexture("_BaseMap", fur);
            material.SetTexture("_BumpMap", null); material.DisableKeyword("_NORMALMAP");
            material.SetColor("_EmissionColor", new Color(.045f, .025f, .01f));
            EditorUtility.SetDirty(material);
            var spine = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Book_Spines.mat");
            spine.SetTexture("_BaseMap", spines); spine.SetColor("_BaseColor", Color.white);
            spine.SetTexture("_EmissionMap", spines);
            spine.SetColor("_EmissionColor", new Color(.1f, .075f, .035f));
            EditorUtility.SetDirty(spine);
            string path = "Assets/Materials/M_Book_Painted_Cover.mat";
            var top = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (top == null) { top = new Material(spine); AssetDatabase.CreateAsset(top, path); }
            top.SetTexture("_BaseMap", cover); top.SetTexture("_EmissionMap", cover);
            top.SetColor("_BaseColor", Color.white);
            top.SetColor("_EmissionColor", new Color(.025f, .017f, .008f));
            EditorUtility.SetDirty(top);
        }

        public static void ApplyAndSave()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            ConfigureMaterials();
            var builder = Object.FindFirstObjectByType<DarkRoomBlockoutBuilder>();
            var serialized = new SerializedObject(builder);
            var array = serialized.FindProperty("platformArtMaterials");
            array.arraySize = Mathf.Max(17,array.arraySize);
            array.GetArrayElementAtIndex(16).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Book_Painted_Cover.mat");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var materials = Enumerable.Range(0, array.arraySize).Select(i => (Material)array.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
            var markers = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var marker in markers.Where(m => m.spec.index == 0 || m.spec.index == 1))
            {
                Object.DestroyImmediate(marker.transform.Find("Visual_3D").gameObject);
                var visual = DarkRoomPlatformArt.Build(DarkRoomPlatformGeometry.Get(marker.spec.index), marker.spec.contact, materials, marker.transform);
                if (marker.spec.index == 0)
                {
                    var contact = marker.transform.Find("Contacto_Fisico");
                    foreach (var collider in contact.GetComponents<Collider>()) Object.DestroyImmediate(collider);
                    while (contact.childCount > 0) Object.DestroyImmediate(contact.GetChild(0).gameObject);
                    contact.localPosition = Vector3.zero; contact.localRotation = Quaternion.identity; contact.localScale = Vector3.one;
                    DarkRoomPlatformArt.TryBuildShapeContact(0, visual, contact);
                }
            }
            EditorUtility.SetDirty(builder);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("PAINTED_PLATFORMS_SAVED: no Populate/audio regeneration; bear collision follows visible meshes.");
        }
    }
}
