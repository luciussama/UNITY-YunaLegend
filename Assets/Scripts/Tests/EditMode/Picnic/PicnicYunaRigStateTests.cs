using NUnit.Framework;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Tests
{
    public sealed class PicnicYunaRigStateTests
    {
        [Test]
        public void RunCycle_AlternatesFrontAndBackLegsInOppositePhases()
        {
            var poseContact = PicnicYunaRigState.Evaluate(
                onGround: true, vx: 210f, vy: 0f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 0f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f,
                strideDistancePx: 0f);
            var poseOpposite = PicnicYunaRigState.Evaluate(
                onGround: true, vx: 210f, vy: 0f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 0.25f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f,
                strideDistancePx: PicnicYunaRigState.StridePixelsPerCycle * 0.5f);

            Assert.AreEqual(PicnicYunaMotionKind.Run, poseContact.Motion);
            Assert.AreEqual(PicnicYunaExpression.Happy, poseContact.Expression);
            Assert.Greater(poseContact.FrontLegDeg, 20f);
            Assert.Less(poseContact.BackLegDeg, -20f);

            Assert.Less(poseOpposite.FrontLegDeg, -20f);
            Assert.Greater(poseOpposite.BackLegDeg, 20f);
        }

        [Test]
        public void RunCycle_BendsKneeStronglyDuringRecoveryAndElbowsThroughoutRun()
        {
            var recovery = PicnicYunaRigState.Evaluate(
                onGround: true, vx: 210f, vy: 0f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 0f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f,
                strideDistancePx: PicnicYunaRigState.StridePixelsPerCycle * (5f / 8f));

            Assert.Less(recovery.FrontKneeDeg, -50f, "Recovery leg must bend knee sharply to lift foot.");
            Assert.Greater(recovery.FrontElbowDeg, 30f, "Running arms must stay naturally bent at the elbow.");
            Assert.Greater(recovery.BackElbowDeg, 30f);
        }

        [Test]
        public void RunCycle_SwingsArmsOppositeToSameSideLegs()
        {
            var pose = PicnicYunaRigState.Evaluate(
                onGround: true, vx: 210f, vy: 0f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 0f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f,
                strideDistancePx: 0f);

            Assert.Greater(pose.FrontLegDeg, 20f);
            Assert.Less(pose.FrontArmDeg, -18f);
            Assert.Less(pose.BackLegDeg, -20f);
            Assert.Greater(pose.BackArmDeg, 18f);
        }

        [Test]
        public void Idle_KeepsLegsGroundedAndBreathingSubtle()
        {
            var pose = PicnicYunaRigState.Evaluate(
                onGround: true, vx: 0f, vy: 0f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 0.4f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f);

            Assert.AreEqual(PicnicYunaMotionKind.Idle, pose.Motion);
            Assert.AreEqual(PicnicYunaExpression.Happy, pose.Expression);
            Assert.AreEqual(0f, pose.FrontLegDeg, 0.001f);
            Assert.AreEqual(0f, pose.BackLegDeg, 0.001f);
            Assert.IsFalse(pose.UseWorriedFace);
        }

        [Test]
        public void JumpAndFall_ClassifyAirborneStatesAndTrailPigtails()
        {
            var rise = PicnicYunaRigState.Evaluate(
                onGround: false, vx: 160f, vy: -300f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 1.0f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f);
            var fall = PicnicYunaRigState.Evaluate(
                onGround: false, vx: 160f, vy: 220f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 1.3f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f);

            Assert.AreEqual(PicnicYunaMotionKind.JumpRise, rise.Motion);
            Assert.Greater(rise.FrontPigtailDeg, 10f);
            Assert.Greater(rise.FrontPigtailTipDeg, rise.FrontPigtailDeg);
            Assert.AreEqual(PicnicYunaMotionKind.Fall, fall.Motion);
            Assert.Less(fall.FrontPigtailDeg, -10f);
            Assert.Less(fall.FrontPigtailTipDeg, fall.FrontPigtailDeg);
        }

        [Test]
        public void GrassTrudge_UsesStrainExpressionWithHeavyForwardLeanAndDeepKneeLift()
        {
            float grassCycle = PicnicYunaRigState.StridePixelsPerCycle * 0.72f;
            var lift = PicnicYunaRigState.Evaluate(
                onGround: true, vx: 90f, vy: 0f, terrain: PicnicTerrain.Grass,
                discomfort: 0.75f, time: 0f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f,
                strideDistancePx: grassCycle * (5f / 8f));

            Assert.AreEqual(PicnicYunaMotionKind.DiscomfortWalk, lift.Motion);
            Assert.AreEqual(PicnicYunaExpression.Strain, lift.Expression);
            Assert.IsTrue(lift.UseWorriedFace);
            Assert.Less(lift.TorsoLeanDeg, -8f, "Grass trudge should lean forward heavily.");
            Assert.Less(lift.FrontKneeDeg, -65f, "Grass trudge must lift knee deeply out of tall grass.");
        }

        [Test]
        public void GravelSlip_UsesScaredExpressionWithBackwardSlipAndRaisedBalanceArms()
        {
            var slip = PicnicYunaRigState.Evaluate(
                onGround: true, vx: 120f, vy: 0f, terrain: PicnicTerrain.Gravel,
                discomfort: 0.8f, time: 0f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f,
                strideDistancePx: 0f);

            Assert.AreEqual(PicnicYunaMotionKind.GravelStumble, slip.Motion);
            Assert.AreEqual(PicnicYunaExpression.Scared, slip.Expression);
            Assert.Greater(slip.TorsoLeanDeg, 10f, "Gravel slip should pitch torso backward on slide.");
            Assert.Greater(slip.FrontLegDeg, 30f, "Slipping foot shoots forward.");
            Assert.Greater(slip.FrontArmDeg, 60f, "Arms raise wide for balance during gravel slip.");
        }

        [Test]
        public void BeetlePanicSprint_UsesPanicExpressionWithSteepLeanAndExplosiveStride()
        {
            float panicCycle = PicnicYunaRigState.StridePixelsPerCycle * 0.80f;
            var sprint = PicnicYunaRigState.Evaluate(
                onGround: true, vx: 280f, vy: 0f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 0f, hasBalancePose: false, isPanic: true, hurtRemaining: 0f,
                strideDistancePx: panicCycle * (5f / 8f));

            Assert.AreEqual(PicnicYunaMotionKind.PanicRun, sprint.Motion);
            Assert.AreEqual(PicnicYunaExpression.Panic, sprint.Expression);
            Assert.Less(sprint.TorsoLeanDeg, -12f, "Beetle panic sprint must lean steeply forward.");
            Assert.Less(sprint.FrontKneeDeg, -75f, "Beetle panic sprint must have deep cartoon heel kick.");
            Assert.Greater(sprint.BackPigtailTipDeg, 20f, "Pigtails must trail strongly behind in panic sprint.");
        }

        [Test]
        public void JumpTakeoff_ArchesChestAtWaistForUpwardImpulse()
        {
            var launch = PicnicYunaRigState.Evaluate(
                onGround: false, vx: 180f, vy: -460f, terrain: PicnicTerrain.Wood,
                discomfort: 0f, time: 0.5f, hasBalancePose: false, isPanic: false, hurtRemaining: 0f);

            Assert.AreEqual(PicnicYunaMotionKind.JumpRise, launch.Motion);
            Assert.Greater(launch.ChestBendDeg, 20f, "Upper torso must arch backward/upward at the waist during jump impulse.");
            Assert.Greater(launch.ScaleY, 1.09f, "Jump launch should stretch vertically with upward momentum.");
            Assert.Greater(launch.FrontArmDeg, 55f, "Arms thrust upward during jump impulse.");
        }

        [Test]
        public void JumpAnticipationAndLanding_CrouchWithBentKneesAndForwardArms()
        {
            var anticipation = PicnicYunaRigState.EvaluateJumpAnticipation(PicnicYunaExpression.Happy);
            var landing = PicnicYunaRigState.EvaluateJumpLanding(PicnicYunaExpression.Happy);

            Assert.AreEqual(PicnicYunaMotionKind.JumpAnticipation, anticipation.Motion);
            Assert.Less(anticipation.HipsBobY, -0.05f, "Anticipation must crouch hips downward.");
            Assert.Less(anticipation.FrontKneeDeg, -55f, "Anticipation must flex knees deeply.");
            Assert.Greater(anticipation.FrontArmDeg, 40f, "Anticipation brings front arm forward to prepare impulse.");
            Assert.Greater(anticipation.BackArmDeg, 30f, "Anticipation brings back arm forward to prepare impulse.");

            Assert.AreEqual(PicnicYunaMotionKind.JumpLand, landing.Motion);
            Assert.Less(landing.HipsBobY, -0.05f, "Landing must crouch to absorb impact.");
            Assert.Less(landing.FrontKneeDeg, -50f, "Landing knees flex to cushion impact.");
            Assert.Less(landing.ScaleY, 0.90f, "Landing applies crisp squash.");
        }
    }
}
