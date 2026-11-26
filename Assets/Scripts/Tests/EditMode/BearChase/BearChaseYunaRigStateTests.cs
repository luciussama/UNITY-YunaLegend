using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Tests
{
    public sealed class BearChaseYunaRigStateTests
    {
        [Test]
        public void ApproachWalk_AlternatesLegsAndBendsKneeOnRecovery()
        {
            var sampleA = BearChaseEntranceTimeline.Evaluate(1.10f);
            var poseA = BearChaseYunaRigState.Evaluate(1.10f, sampleA);
            var sampleB = BearChaseEntranceTimeline.Evaluate(1.48f);
            var poseB = BearChaseYunaRigState.Evaluate(1.48f, sampleB);

            Assert.AreEqual(BearChaseEntranceBeat.Approach, poseA.Beat);
            Assert.AreNotEqual(System.Math.Sign(poseA.FrontLegDeg), System.Math.Sign(poseA.BackLegDeg),
                "Front and back legs must alternate in opposite directions during approach walk.");
            Assert.IsTrue(poseA.FrontKneeDeg < -10f || poseA.BackKneeDeg < -10f ||
                          poseB.FrontKneeDeg < -10f || poseB.BackKneeDeg < -10f,
                "Walk cycle must bend knees during stride.");
        }

        [Test]
        public void ReachBeat_BendsWaistForwardTiltsHeadDownAndExtendsBackArm()
        {
            var sample = BearChaseEntranceTimeline.Evaluate(4.85f);
            var pose = BearChaseYunaRigState.Evaluate(4.85f, sample);

            Assert.AreEqual(BearChaseEntranceBeat.Reach, pose.Beat);
            Assert.AreEqual(BearChaseYunaExpression.Focused, pose.Expression);
            Assert.Less(pose.ChestBendDeg, -15f, "Waist must bend forward when reaching for the bear.");
            Assert.Less(pose.HeadTiltDeg, -10f, "Head must tilt downward toward the sleeping bear.");
            Assert.Greater(pose.BackArmDeg, 50f, "Free arm must extend forward to reach the bear.");
        }

        [Test]
        public void BearWakeUp_SwitchesToShockedExpressionWithRecoilHopAndLandingCrouch()
        {
            var sampleCrouch = BearChaseEntranceTimeline.Evaluate(5.07f);
            var poseCrouch = BearChaseYunaRigState.Evaluate(5.07f, sampleCrouch);
            var sampleHop = BearChaseEntranceTimeline.Evaluate(5.24f);
            var poseHop = BearChaseYunaRigState.Evaluate(5.24f, sampleHop);
            var sampleLand = BearChaseEntranceTimeline.Evaluate(5.42f);
            var poseLand = BearChaseYunaRigState.Evaluate(5.42f, sampleLand);

            Assert.AreEqual(BearChaseYunaExpression.Shocked, poseCrouch.Expression);
            Assert.Less(poseCrouch.HipsBobY, -0.04f, "Startle recoil begins with quick anticipation crouch.");

            Assert.AreEqual(BearChaseYunaExpression.Shocked, poseHop.Expression);
            Assert.Greater(poseHop.ChestBendDeg, 10f, "Backward startled hop arches upper torso at waist.");
            Assert.Greater(poseHop.BackElbowDeg, 90f, "Free arm pulls defensively to chest in shock.");

            Assert.Less(poseLand.HipsBobY, -0.04f, "Startle hop lands in cushioned crouch.");
            Assert.Less(poseLand.ScaleY, 0.94f, "Landing applies squash.");
        }
    }
}
