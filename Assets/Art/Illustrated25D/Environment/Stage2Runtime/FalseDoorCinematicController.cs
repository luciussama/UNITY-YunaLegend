using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>
    /// Adaptador fino que aplica a cinemática a 30 fps da Porta Falsa (FalseDoorCinematicTimeline)
    /// sobre o ator de queda sentada (FalseDoorFallActor), YunaIdleIllustration, câmera e Nanda.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class FalseDoorCinematicController : MonoBehaviour
    {
        private Yuna25DPlayerController player;
        private YunaIdleIllustration yunaIdle;
        private YunaIllustrationVisibilityResolver visibilityResolver;
        private FalseDoorFallActor fallActor;
        private SideScroll25DCamera cam;
        private PenumbraLightingT054 lighting;

        private StageFlowStateMachine.FlowPhase currentPhase;
        private float phaseElapsed;
        private bool activeCutscene;

        public void Initialize(
            Yuna25DPlayerController yunaPlayer,
            SideScroll25DCamera cameraDriver,
            PenumbraLightingT054 penumbraLighting)
        {
            player = yunaPlayer;
            yunaIdle = yunaPlayer != null ? yunaPlayer.GetComponentInChildren<YunaIdleIllustration>(true) : null;
            visibilityResolver = yunaPlayer != null ? yunaPlayer.GetComponentInChildren<YunaIllustrationVisibilityResolver>(true) : null;
            cam = cameraDriver;
            lighting = penumbraLighting;

            if (fallActor == null)
            {
                var actorGo = new GameObject("FalseDoorFallActor");
                actorGo.transform.SetParent(transform, false);
                fallActor = actorGo.AddComponent<FalseDoorFallActor>();
                fallActor.Initialize(lighting);
            }
        }

        public void OnPhaseChanged(StageFlowStateMachine.FlowPhase next)
        {
            currentPhase = next;
            phaseElapsed = 0f;
            activeCutscene = next == StageFlowStateMachine.FlowPhase.FalseDoorFalling
                || next == StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0
                || next == StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep1;

            if (!activeCutscene)
            {
                ClearOverrides();
                return;
            }

            ApplySample(FalseDoorCinematicTimeline.Evaluate(currentPhase, 0f));
        }

        private void LateUpdate()
        {
            if (!activeCutscene)
            {
                return;
            }

            phaseElapsed += Time.deltaTime;
            var sample = FalseDoorCinematicTimeline.Evaluate(currentPhase, phaseElapsed);
            ApplySample(sample);
        }

        private void ApplySample(FalseDoorCinematicTimeline.Sample sample)
        {
            if (visibilityResolver != null)
            {
                visibilityResolver.suppressAll = sample.UseFallSeatedActor;
            }

            if (fallActor != null && player != null)
            {
                fallActor.ApplySample(player.transform.position, sample);
            }

            if (player != null && sample.OverrideYunaPose)
            {
                player.SetRunDirection(sample.FacingDirection);
            }

            if (cam != null)
            {
                cam.SetIllustratedCinematicOffset(new Vector2(sample.CameraOffsetX, sample.CameraOffsetY));
                cam.SetIllustratedSpanScale(sample.CameraSpanScale);
            }

            if (lighting != null)
            {
                lighting.nandaStrength = Mathf.Clamp01(sample.NandaLightStrength);
            }

            if (yunaIdle != null)
            {
                yunaIdle.SetCinematicOverride(
                    sample.OverrideYunaPose,
                    sample.YunaBlink,
                    sample.YunaLean,
                    sample.YunaHeadTilt,
                    sample.YunaBreathe,
                    sample.YunaSquashX * sample.SmoothTurnScaleX,
                    sample.YunaSquashY);
                if (sample.OverrideYunaPose)
                {
                    yunaIdle.Evaluate(sample.QuantizedTime);
                }
            }
        }

        private void ClearOverrides()
        {
            if (visibilityResolver != null)
            {
                visibilityResolver.suppressAll = false;
            }

            if (fallActor != null)
            {
                fallActor.SetVisible(false);
            }

            if (cam != null)
            {
                cam.SetIllustratedCinematicOffset(Vector2.zero);
            }

            if (lighting != null)
            {
                lighting.nandaStrength = 1f;
            }

            if (yunaIdle != null)
            {
                yunaIdle.SetCinematicOverride(false, 0f, 0f, 0f, 0f, 1f, 1f);
            }
        }
    }
}
