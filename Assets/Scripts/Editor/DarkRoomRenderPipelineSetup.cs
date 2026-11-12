using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomRenderPipelineSetup
    {
        private const string PipelinePath = "Assets/Settings/DarkRoomPipeline.asset";
        private const string RendererPath = "Assets/Settings/DarkRoomRenderer.asset";

        [MenuItem("YunaLegend/Configurar Renderizacao URP")]
        public static void Configure()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(
                    "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }
            // Repair existing renderers as well, so enabling the camera cannot leave a broken chain.
            if (renderer.postProcessData == null)
                renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(
                    "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            if (renderer.postProcessData == null)
                throw new InvalidOperationException("URP PostProcessData is missing.");

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.supportsHDR = true;
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }
            pipeline.supportsHDR = true;
            pipeline.colorGradingMode = ColorGradingMode.HighDynamicRange;
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            // No quality override may silently return the project to Built-in.
            int currentQuality = QualitySettings.GetQualityLevel();
            try
            {
                for (int i = 0; i < QualitySettings.names.Length; i++)
                {
                    QualitySettings.SetQualityLevel(i, false);
                    QualitySettings.renderPipeline = pipeline;
                }
            }
            finally
            {
                QualitySettings.SetQualityLevel(currentQuality, false);
            }
            AssetDatabase.SaveAssets();
            if (GraphicsSettings.defaultRenderPipeline != pipeline || QualitySettings.renderPipeline != pipeline)
                throw new InvalidOperationException("URP nao foi persistido em Graphics/Quality.");
            Debug.Log("DARKROOM_URP_CONFIGURED: " + PipelinePath);
        }
    }
}
