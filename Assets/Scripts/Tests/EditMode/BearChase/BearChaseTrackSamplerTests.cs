using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.Tests.EditMode.BearChase
{
    public sealed class BearChaseTrackSamplerTests
    {
        [Test]
        public void TrackSampler_HasSmoothZeroEnvelopeAtStartAndEnd()
        {
            BearChaseTrackSpec spec = BearChaseTrackSpec.Default;

            Assert.AreEqual(0f, BearChaseTrackSampler.CenterX(0f, spec), 0.0001f);
            Assert.AreEqual(0f, BearChaseTrackSampler.ElevationY(0f, spec), 0.0001f);

            Assert.AreEqual(0f, BearChaseTrackSampler.CenterX(spec.TrackLength, spec), 0.0001f);
            Assert.AreEqual(0f, BearChaseTrackSampler.ElevationY(spec.TrackLength, spec), 0.0001f);
        }

        [Test]
        public void TrackSampler_ProducesNonLinearLateralCurvesAndVerticalHillsAndDips()
        {
            BearChaseTrackSpec spec = BearChaseTrackSpec.Default;
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (float z = 10f; z <= spec.TrackLength - 10f; z += 4f)
            {
                float x = BearChaseTrackSampler.CenterX(z, spec);
                float y = BearChaseTrackSampler.ElevationY(z, spec);
                minX = System.Math.Min(minX, x);
                maxX = System.Math.Max(maxX, x);
                minY = System.Math.Min(minY, y);
                maxY = System.Math.Max(maxY, y);
            }

            Assert.Greater(maxX - minX, 8.0f, "Corredor deve apresentar curvas em S pronunciadas.");
            Assert.Greater(maxY, 1.2f, "Corredor deve conter subidas claras.");
            Assert.Less(minY, -1.2f, "Corredor deve conter descidas claras.");
        }

        [Test]
        public void TrackSampler_SlopesAndAnglesStayContinuousAndNavigable()
        {
            BearChaseTrackSpec spec = BearChaseTrackSpec.Default;
            for (float z = 1f; z < BearChaseTrackSpec.FinaleStartZ; z += 5f)
            {
                float slopeX = BearChaseTrackSampler.TangentSlopeX(z, spec);
                float slopeY = BearChaseTrackSampler.TangentSlopeY(z, spec);
                float bank = BearChaseTrackSampler.BankDegrees(z, spec);

                Assert.IsFalse(float.IsNaN(slopeX));
                Assert.IsFalse(float.IsNaN(slopeY));
                Assert.Less(System.Math.Abs(slopeY), 0.28f, "Inclinacao vertical deve permanecer confortavel.");
                Assert.LessOrEqual(System.Math.Abs(bank), 5.51f);
            }
        }

        [Test]
        public void Finale_ExtendsTrackWithTighterTurnsAndSteeperGrades()
        {
            BearChaseTrackSpec spec = BearChaseTrackSpec.Default;
            float peakGrade = 0f;
            float peakLateralSlope = 0f;
            float peakThrowPressure = 0f;

            for (float z = BearChaseTrackSpec.FinaleStartZ; z < spec.TrackLength; z += 0.5f)
            {
                peakGrade = System.Math.Max(peakGrade, System.Math.Abs(BearChaseTrackSampler.TangentSlopeY(z, spec)));
                peakLateralSlope = System.Math.Max(peakLateralSlope, System.Math.Abs(BearChaseTrackSampler.TangentSlopeX(z, spec)));
                peakThrowPressure = System.Math.Max(peakThrowPressure, BearChaseTrackSampler.ThrowPressure(z, spec));
            }

            Assert.Greater(spec.TrackLength - BearChaseTrackSpec.FinaleStartZ, 100f);
            Assert.Greater(peakGrade, 0.30f);
            Assert.Less(peakGrade, 0.40f);
            Assert.Greater(peakLateralSlope, 0.60f);
            Assert.Greater(peakThrowPressure, 0.8f);
            Assert.AreEqual(0f, BearChaseTrackSampler.ThrowPressure(400f, spec));
        }
    }
}
