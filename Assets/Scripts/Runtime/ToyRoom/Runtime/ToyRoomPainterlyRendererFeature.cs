using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// URP ScriptableRendererFeature que aplica o filtro Kuwahara Anisotropico,
    /// Paper Grain de aquarela e contorno Sobel de nanquim sepia na Toy Room.
    /// </summary>
    public sealed class ToyRoomPainterlyRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material passMaterial;
        [SerializeField] private bool applyOnlyInToyRoomScene = true;

        private PainterlyRenderPass renderPass;

        public Material PassMaterial
        {
            get => passMaterial;
            set => passMaterial = value;
        }

        public override void Create()
        {
            renderPass = new PainterlyRenderPass("ToyRoom_PainterlyKuwaharaPass");
            renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (passMaterial == null)
            {
                return;
            }

            if (renderingData.cameraData.cameraType != CameraType.Game
                && renderingData.cameraData.cameraType != CameraType.SceneView)
            {
                return;
            }

            if (applyOnlyInToyRoomScene && SceneManager.GetActiveScene().name != "ToyRoom_Gameplay")
            {
                if (renderingData.cameraData.camera == null
                    || renderingData.cameraData.camera.GetComponent<IsometricToyRoomCamera>() == null)
                {
                    return;
                }
            }

            renderPass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
            renderPass.Setup(passMaterial);
            renderer.EnqueuePass(renderPass);
        }

        private sealed class PainterlyRenderPass : ScriptableRenderPass
        {
            private Material material;

            public PainterlyRenderPass(string passName)
            {
                profilingSampler = new ProfilingSampler(passName);
                requiresIntermediateTexture = true;
            }

            public void Setup(Material mat)
            {
                material = mat;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null)
                {
                    return;
                }

                var resourcesData = frameData.Get<UniversalResourceData>();
                if (resourcesData.isActiveTargetBackBuffer || !resourcesData.activeColorTexture.IsValid())
                {
                    return;
                }

                var source = resourcesData.activeColorTexture;
                var targetDesc = renderGraph.GetTextureDesc(source);
                targetDesc.name = "_ToyRoomPainterlyTempColor";
                targetDesc.clearBuffer = false;

                TextureHandle tempCopy = renderGraph.CreateTexture(targetDesc);
                renderGraph.AddBlitPass(source, tempCopy, Vector2.one, Vector2.zero, passName: "ToyRoom Copy Color");

                var blitParams = new RenderGraphUtils.BlitMaterialParameters(tempCopy, source, material, 0);
                renderGraph.AddBlitPass(blitParams, passName: "ToyRoom Painterly Kuwahara + Ink");
            }
        }
    }
}
