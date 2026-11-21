using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>
    /// Adaptador fino que aplica a cinemática de abertura a 30 fps (OpeningCinematicTimeline)
    /// sobre câmera, ator de cama (OpeningBedActor), Yuna, Nanda e PenumbraLightingT054.
    /// Executa antes da câmera (0) e da penumbra (290) para aplicar os valores no mesmo frame.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class OpeningCinematicController : MonoBehaviour
    {
        private Yuna25DPlayerController player;
        private YunaIdleIllustration yunaIdle;
        private YunaIllustrationVisibilityResolver visibilityResolver;
        private OpeningBedActor bedActor;
        private NandaFairyGuide guide;
        private SideScroll25DCamera cam;
        private PenumbraLightingT054 lighting;

        private StageFlowStateMachine.FlowPhase currentPhase;
        private float phaseElapsed;
        private bool activeOpening;
        private bool skipBedHandoffOnNextPreJump;

        public float ChapterBannerAlpha { get; private set; }
        public float CurtainFadeAlpha { get; private set; }
        public int DialogueBeat { get; private set; }
        public bool IsYunaInBed { get; private set; }

        public void Initialize(
            Yuna25DPlayerController yunaPlayer,
            NandaFairyGuide nandaGuide,
            SideScroll25DCamera cameraDriver,
            PenumbraLightingT054 penumbraLighting)
        {
            player = yunaPlayer;
            yunaIdle = yunaPlayer != null ? yunaPlayer.GetComponentInChildren<YunaIdleIllustration>(true) : null;
            visibilityResolver = yunaPlayer != null ? yunaPlayer.GetComponentInChildren<YunaIllustrationVisibilityResolver>(true) : null;
            guide = nandaGuide;
            cam = cameraDriver;
            lighting = penumbraLighting;

            if (bedActor == null && yunaPlayer != null)
            {
                var bedGo = new GameObject("OpeningBedActor");
                bedGo.transform.SetParent(transform, false);
                bedActor = bedGo.AddComponent<OpeningBedActor>();
                bedActor.Initialize(yunaPlayer.transform.position, lighting);
            }

            OnPhaseChanged(StageFlowStateMachine.FlowPhase.OpeningNandaWake1);
        }

        public void PrepareRespawnAtPreJump()
        {
            skipBedHandoffOnNextPreJump = true;
        }

        public void OnPhaseChanged(StageFlowStateMachine.FlowPhase next)
        {
            currentPhase = next;
            phaseElapsed = 0f;
            if (next == StageFlowStateMachine.FlowPhase.PreJumpCue && skipBedHandoffOnNextPreJump)
            {
                skipBedHandoffOnNextPreJump = false;
                activeOpening = false;
                ClearOverrides();
                return;
            }

            skipBedHandoffOnNextPreJump = false;
            activeOpening = next == StageFlowStateMachine.FlowPhase.OpeningNandaWake1
                || next == StageFlowStateMachine.FlowPhase.OpeningPoeticIntro
                || next == StageFlowStateMachine.FlowPhase.OpeningNandaWake2
                || next == StageFlowStateMachine.FlowPhase.PreJumpCue;

            if (!activeOpening)
            {
                ClearOverrides();
                return;
            }

            ApplySample(OpeningCinematicTimeline.Evaluate(currentPhase, 0f));
        }

        private void LateUpdate()
        {
            if (!activeOpening)
            {
                return;
            }

            phaseElapsed += Time.deltaTime;
            var sample = OpeningCinematicTimeline.Evaluate(currentPhase, phaseElapsed);
            ApplySample(sample);
        }

        private void ApplySample(OpeningCinematicTimeline.Sample sample)
        {
            ChapterBannerAlpha = sample.ChapterBannerAlpha;
            CurtainFadeAlpha = sample.CurtainFadeAlpha;
            DialogueBeat = sample.DialogueBeat;
            IsYunaInBed = sample.YunaInBed;

            if (visibilityResolver != null)
            {
                visibilityResolver.suppressAll = sample.YunaInBed;
            }

            if (bedActor != null)
            {
                bedActor.ApplySample(sample);
            }

            if (cam != null)
            {
                cam.SetIllustratedCinematicOffset(new Vector2(sample.CameraOffsetX, sample.CameraOffsetY));
                if (sample.Active || currentPhase == StageFlowStateMachine.FlowPhase.PreJumpCue)
                {
                    cam.SetIllustratedSpanScale(sample.CameraSpanScale);
                }
            }

            if (guide != null)
            {
                bool lockNanda = activeOpening && (currentPhase != StageFlowStateMachine.FlowPhase.PreJumpCue || sample.Active);
                guide.ExternalPositionControl = lockNanda;
                if (lockNanda && player != null)
                {
                    guide.transform.position = player.transform.position + new Vector3(
                        sample.NandaOffsetX,
                        sample.NandaOffsetY,
                        sample.NandaOffsetZ);
                }
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
                    sample.YunaSquashX,
                    sample.YunaSquashY);
                yunaIdle.Evaluate(sample.QuantizedTime);
            }
        }

        private void ClearOverrides()
        {
            ChapterBannerAlpha = 0f;
            CurtainFadeAlpha = 0f;
            DialogueBeat = 0;
            IsYunaInBed = false;

            if (visibilityResolver != null)
            {
                visibilityResolver.suppressAll = false;
            }

            if (bedActor != null)
            {
                bedActor.SetVisible(false);
            }

            if (guide != null)
            {
                guide.ExternalPositionControl = false;
            }

            if (cam != null)
            {
                cam.SetIllustratedCinematicOffset(Vector2.zero);
                cam.SetIllustratedSpanScale(1f);
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
