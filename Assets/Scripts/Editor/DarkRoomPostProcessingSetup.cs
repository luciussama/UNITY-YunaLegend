using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace YunaLegend.EditorScripts
{
    /// <summary>One persistent configuration for rebuilds and targeted look-development changes.</summary>
    public static class DarkRoomPostProcessingSetup
    {
        public const string ProfilePath = "Assets/DefaultVolumeProfile.asset";

        public static VolumeProfile ConfigureProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            var bloom = Effect<Bloom>(profile);
            bloom.threshold.Override(1.15f);
            bloom.intensity.Override(0.18f);
            bloom.scatter.Override(0.55f);
            bloom.clamp.Override(5f);
            bloom.tint.Override(Color.white);

            var vignette = Effect<Vignette>(profile);
            vignette.intensity.Override(0.14f);
            vignette.smoothness.Override(0.45f);
            vignette.color.Override(new Color(0.025f, 0.02f, 0.05f));

            var color = Effect<ColorAdjustments>(profile);
            color.postExposure.Override(0f);
            color.contrast.Override(2f);
            color.saturation.Override(-2f);
            color.colorFilter.Override(Color.white);
            color.hueShift.Override(0f);
            Effect<Tonemapping>(profile).mode.Override(TonemappingMode.Neutral);

            // The landing surface must stay sharp during running and camera transitions.
            Disable<MotionBlur>(profile);
            Disable<DepthOfField>(profile);
            Disable<ChromaticAberration>(profile);
            Disable<FilmGrain>(profile);
            Disable<LensDistortion>(profile);
            EditorUtility.SetDirty(profile);
            foreach (var component in profile.components) EditorUtility.SetDirty(component);
            return profile;
        }

        private static T Effect<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component)) component = profile.Add<T>(true);
            component.active = true;
            return component;
        }

        private static void Disable<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T component)) component.active = false;
        }

        public static void ConfigureScene(Camera camera)
        {
            if (camera == null) throw new InvalidOperationException("The Dark Room camera is missing.");
            var profile = ConfigureProfile();
            var volumeGo = GameObject.Find("DarkRoom_GlobalVolume");
            if (volumeGo == null) volumeGo = new GameObject("DarkRoom_GlobalVolume");
            var volume = volumeGo.GetComponent<Volume>();
            if (volume == null) volume = volumeGo.AddComponent<Volume>();
            volume.enabled = true;
            volume.isGlobal = true;
            volume.weight = 1f;
            volume.sharedProfile = profile;

            var data = camera.GetUniversalAdditionalCameraData();
            camera.allowHDR = true;
            data.renderPostProcessing = true;
            data.volumeLayerMask |= 1 << volumeGo.layer;
            data.volumeTrigger = camera.transform;
            camera.SetVolumeFrameworkUpdateMode(VolumeFrameworkUpdateMode.EveryFrame);
            data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            data.dithering = true;
            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(volume);
        }

        [MenuItem("YunaLegend/Configurar Pos-processamento do Quarto Escuro")]
        public static void ConfigureAndSaveScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            DarkRoomRenderPipelineSetup.Configure();
            ConfigureScene(Camera.main);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("DARKROOM_POST_PROCESSING_SAVED: HDR, camera post-processing, linked Volume, Neutral tonemapping, subtle Bloom/Vignette, FXAA.");
        }
    }
}
