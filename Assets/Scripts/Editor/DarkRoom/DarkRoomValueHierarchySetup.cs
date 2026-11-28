using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomValueHierarchySetup
    {
        public static void ConfigureMaterials()
        {
            foreach (string path in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" }))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(path));
                if (!mat.HasProperty("_Smoothness")) continue;
                bool actor = mat.name == "M_Nanda_Glow" || mat.name == "M_Yuna_Dress" || mat.name == "M_Yuna_Skin";
                // Only the playable supports carrying the contour map, never scenery/portal.
                var map = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;
                bool support = map != null && map.name == "T_Platform_ContourEmission" && !mat.name.StartsWith("M_Yuna");
                if (!actor && !support) continue;
                if (map != null && map.name == "T_Platform_ContourEmission" && !mat.name.StartsWith("M_Yuna"))
                {
                    mat.SetFloat("_Smoothness", .12f);
                    mat.SetFloat("_SpecularHighlights", 1f);
                    mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    mat.DisableKeyword("_SPECULAR_COLOR");
                    mat.DisableKeyword("_SPECGLOSSMAP");
                    mat.DisableKeyword("_GLOSSINESS_FROM_BASE_ALPHA");
                }
                if (mat.name == "M_Nanda_Glow")
                {
                    mat.SetColor("_BaseColor", new Color(.95f, .82f, .5f));
                    mat.SetColor("_EmissionColor", new Color(1.6f, 1.2f, .6f));
                    mat.SetFloat("_Smoothness", .12f);
                    mat.SetFloat("_SpecularHighlights", 1f);
                    mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    mat.DisableKeyword("_SPECULAR_COLOR");
                    mat.DisableKeyword("_SPECGLOSSMAP");
                    mat.DisableKeyword("_GLOSSINESS_FROM_BASE_ALPHA");
                }
                if (mat.name == "M_Yuna_Dress" || mat.name == "M_Yuna_Skin")
                {
                    mat.SetTexture("_EmissionMap", null);
                    mat.SetColor("_EmissionColor", new Color(.055f, .045f, .04f));
                    mat.EnableKeyword("_EMISSION");
                }
                EditorUtility.SetDirty(mat);
            }
        }

        public static void ConfigureScene()
        {
            var yuna = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var fill = yuna.transform.Find("Yuna_FillLight").GetComponent<Light>();
            // A light inside the capsule cannot illuminate its outward-facing normals.
            fill.transform.localPosition = new Vector3(-.6f, 1.25f, -1.6f);
            fill.intensity = 1.6f;
            fill.range = 3.2f;
            fill.shadows = LightShadows.None;
            var fairy = Object.FindFirstObjectByType<NandaFairyGuide>();
            var serialized = new SerializedObject(fairy);
            serialized.FindProperty("baseIntensity").floatValue = 2.2f;
            serialized.FindProperty("lightRange").floatValue = 7f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var light = fairy.GetComponent<Light>();
            light.intensity = 2.2f;
            light.range = 7f;
            var link = new SerializedObject(fairy);
            link.FindProperty("guideLight").objectReferenceValue = light;
            link.ApplyModifiedPropertiesWithoutUndo();
            fairy.ApplyMood(fairy.Mood);
            EditorUtility.SetDirty(fill);
            EditorUtility.SetDirty(fairy.GetComponent<Light>());
        }

        public static void ConfigureAndSave()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            ConfigureMaterials();
            ConfigureScene();
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("VALUE_HIERARCHY_SAVED");
        }
    }
}
