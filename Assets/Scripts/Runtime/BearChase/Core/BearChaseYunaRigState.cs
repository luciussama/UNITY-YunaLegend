using System;

namespace YunaLegend.BearChase.Core
{

    /// <summary>
    /// Pure 16-joint articulated 2D skeletal pose for Yuna during the BearChase entrance cinematic
    /// (Establish -> Approach walk -> Settle -> Reach toward bear -> Startled recoil hop -> Transition).
    /// </summary>
    public readonly struct BearChaseYunaRigPose
    {
        public BearChaseEntranceBeat Beat { get; }
        public float HipsBobY { get; }
        public float TorsoLeanDeg { get; }
        public float ChestBendDeg { get; }
        public float SkirtSwayDeg { get; }
        public float SkirtFlare { get; }
        public float HeadTiltDeg { get; }

        public float PonytailRootDeg { get; }
        public float PonytailTipDeg { get; }

        public float FrontLegDeg { get; }
        public float FrontKneeDeg { get; }
        public float FrontAnkleDeg { get; }

        public float BackLegDeg { get; }
        public float BackKneeDeg { get; }
        public float BackAnkleDeg { get; }

        public float FrontArmDeg { get; }
        public float FrontElbowDeg { get; }

        public float BackArmDeg { get; }
        public float BackElbowDeg { get; }

        public float ScaleX { get; }
        public float ScaleY { get; }
        public BearChaseYunaExpression Expression { get; }

        public BearChaseYunaRigPose(
            BearChaseEntranceBeat beat,
            float hipsBobY,
            float torsoLeanDeg,
            float chestBendDeg,
            float skirtSwayDeg,
            float skirtFlare,
            float headTiltDeg,
            float ponytailRootDeg,
            float ponytailTipDeg,
            float frontLegDeg,
            float frontKneeDeg,
            float frontAnkleDeg,
            float backLegDeg,
            float backKneeDeg,
            float backAnkleDeg,
            float frontArmDeg,
            float frontElbowDeg,
            float backArmDeg,
            float backElbowDeg,
            float scaleX,
            float scaleY,
            BearChaseYunaExpression expression)
        {
            Beat = beat;
            HipsBobY = hipsBobY;
            TorsoLeanDeg = torsoLeanDeg;
            ChestBendDeg = chestBendDeg;
            SkirtSwayDeg = skirtSwayDeg;
            SkirtFlare = skirtFlare;
            HeadTiltDeg = headTiltDeg;
            PonytailRootDeg = ponytailRootDeg;
            PonytailTipDeg = ponytailTipDeg;
            FrontLegDeg = frontLegDeg;
            FrontKneeDeg = frontKneeDeg;
            FrontAnkleDeg = frontAnkleDeg;
            BackLegDeg = backLegDeg;
            BackKneeDeg = backKneeDeg;
            BackAnkleDeg = backAnkleDeg;
            FrontArmDeg = frontArmDeg;
            FrontElbowDeg = frontElbowDeg;
            BackArmDeg = backArmDeg;
            BackElbowDeg = backElbowDeg;
            ScaleX = scaleX;
            ScaleY = scaleY;
            Expression = expression;
        }
    }

    /// <summary>
    /// Deterministic 60 FPS skeletal evaluator for Yuna in the BearChase entrance cinematic.
    /// Produces true hip-pivoted alternating strides (coupled to NormalizedYunaX for zero foot sliding),
    /// waist-articulated forward reach toward the sleeping bear, expressive head tilts & face swaps,
    /// and a full anticipation -> startled recoil hop -> cushioned landing -> guard stance sequence.
    /// </summary>
    public static class BearChaseYunaRigState
    {
        private const float TwoPi = (float)(Math.PI * 2.0);
        private const float DegToRad = (float)(Math.PI / 180.0);
        private const float RadToDeg = (float)(180.0 / Math.PI);

        // Exact physical foot-lock coupling:
        // Total travel across 1920px canvas = (0.495 - 0.195) * 1920 = 576 px.
        // Hip-to-sole length L = 0.45 * 395 = 177.75 px; one step at ±24 deg covers 2 * 177.75 * sin(24 deg) = 144.6 px.
        // 576 / 144.6 = 3.98 steps -> exactly 2.0 full 2-step cycles (4 calm, cautious steps, zero foot sliding).
        public const float WalkCyclesAcrossApproach = 2.0f;
        public const float WalkStrideMaxDeg = 23.9f;
        private static readonly float WalkStrideSinMax = (float)Math.Sin(WalkStrideMaxDeg * DegToRad);

        // Arm swing tables synchronized with the 2.0-cycle cautious walk
        private static readonly float[] WalkFrontArmDeg    = { -14f, -10f,   0f,  10f,  14f,   8f,  -2f, -10f };
        private static readonly float[] WalkFrontElbowDeg  = {  20f,  24f,  28f,  32f,  34f,  28f,  24f,  20f };
        private static readonly float[] WalkBackArmDeg     = { -16f, -12f,   0f,  12f,  16f,  10f,  -2f, -12f };
        private static readonly float[] WalkBackElbowDeg   = {  22f,  26f,  30f,  36f,  38f,  32f,  26f,  22f };

        public static BearChaseYunaRigPose Evaluate(float elapsedSeconds, BearChaseEntranceSample sample)
        {
            float t = Math.Max(0f, Math.Min(BearChaseEntranceTimeline.TotalDuration, elapsedSeconds));

            // 1. Establish (0.00 -> 0.60s): calm breathing, gazing toward the sleeping bear
            if (t < BearChaseEntranceTimeline.EstablishEnd)
            {
                return EvaluateIdle(t, BearChaseEntranceBeat.Establish, BearChaseYunaExpression.Calm, headTiltBias: -2.5f);
            }

            // 2. Approach Walk (0.60 -> 3.70s) + Settle (3.70 -> 4.00s)
            if (t < BearChaseEntranceTimeline.ApproachEnd)
            {
                if (t >= BearChaseEntranceTimeline.WalkMoveEnd)
                {
                    float settleU = SmoothStep((t - BearChaseEntranceTimeline.WalkMoveEnd) /
                        (BearChaseEntranceTimeline.ApproachEnd - BearChaseEntranceTimeline.WalkMoveEnd));
                    var idlePose = EvaluateIdle(t, BearChaseEntranceBeat.Approach,
                        settleU > 0.45f ? BearChaseYunaExpression.Focused : BearChaseYunaExpression.Calm,
                        headTiltBias: Lerp(-3f, -7f, settleU));
                    return idlePose;
                }

                float travelSpan = BearChaseEntranceTimeline.YunaReachX - BearChaseEntranceTimeline.YunaStartX;
                float walkProgress = travelSpan > 0.0001f
                    ? Clamp01((sample.NormalizedYunaX - BearChaseEntranceTimeline.YunaStartX) / travelSpan)
                    : 0f;

                // Tight start/end envelope so full foot-locked stance engages immediately and plants cleanly at stop
                float startEnv = SmoothStep(Clamp01(walkProgress / 0.04f));
                float endEnv = SmoothStep(Clamp01((1f - walkProgress) / 0.06f));
                float env = startEnv * endEnv;

                float phase = PositiveMod(walkProgress * WalkCyclesAcrossApproach, 1f);
                float opposite = PositiveMod(phase + 0.5f, 1f);
                float doubleRad = phase * 2f * TwoPi;

                EvaluateFootLockedLeg(phase, out float fHip, out float fKnee, out float fAnkle);
                EvaluateFootLockedLeg(opposite, out float bHip, out float bKnee, out float bAnkle);

                // Geometric hip bob from supporting leg arc (highest at mid-stance theta=0, lowest at crossover)
                float stanceHipDeg = phase < 0.5f ? fHip : bHip;
                float cosStance = (float)Math.Cos(stanceHipDeg * DegToRad);
                float cosMax = (float)Math.Cos(WalkStrideMaxDeg * DegToRad);
                float bobY = (cosStance - cosMax) * 0.45f * env - 0.008f * env;

                float hairRoot = (float)Math.Sin(doubleRad - 0.85f);
                float hairTip = (float)Math.Sin(doubleRad - 1.75f);

                return new BearChaseYunaRigPose(
                    BearChaseEntranceBeat.Approach,
                    hipsBobY: bobY,
                    torsoLeanDeg: (-4.0f + (float)Math.Sin(doubleRad) * 1.4f) * env,
                    chestBendDeg: (float)Math.Sin(doubleRad - 0.5f) * 2.4f * env,
                    skirtSwayDeg: (float)Math.Sin(phase * TwoPi - 0.55f) * 6.0f * env,
                    skirtFlare: Math.Max(0f, -bobY * 1.6f) + 0.012f * env,
                    headTiltDeg: -3.0f + (float)Math.Sin(doubleRad - 1.0f) * 2.0f * env,
                    ponytailRootDeg: 7f * env + hairRoot * 8.5f * env,
                    ponytailTipDeg: 10f * env + hairTip * 14f * env,
                    frontLegDeg: fHip * env,
                    frontKneeDeg: Lerp(-4f, fKnee, env),
                    frontAnkleDeg: Lerp(4f, fAnkle, env),
                    backLegDeg: bHip * env,
                    backKneeDeg: Lerp(-4f, bKnee, env),
                    backAnkleDeg: Lerp(4f, bAnkle, env),
                    frontArmDeg: SampleCyclic(WalkFrontArmDeg, phase) * env,
                    frontElbowDeg: Lerp(14f, SampleCyclic(WalkFrontElbowDeg, phase), env),
                    backArmDeg: SampleCyclic(WalkBackArmDeg, opposite) * env,
                    backElbowDeg: Lerp(14f, SampleCyclic(WalkBackElbowDeg, opposite), env),
                    scaleX: 1f + Math.Max(0f, -bobY * 0.8f),
                    scaleY: 1f - Math.Max(0f, -bobY * 0.8f),
                    expression: walkProgress > 0.78f ? BearChaseYunaExpression.Focused : BearChaseYunaExpression.Calm);
            }

            // 3. Reach (4.00 -> 5.00s): feet planted, waist bends forward, head tilts down, back arm extends to touch bear
            if (t < BearChaseEntranceTimeline.ReachEnd)
            {
                float u = Clamp01((t - BearChaseEntranceTimeline.ApproachEnd) /
                    (BearChaseEntranceTimeline.ReachEnd - BearChaseEntranceTimeline.ApproachEnd));
                float reach = SmoothStep(u);
                float microBreath = (float)Math.Sin(t * 3.5f) * 1.2f;

                return new BearChaseYunaRigPose(
                    BearChaseEntranceBeat.Reach,
                    hipsBobY: -0.026f * reach,
                    torsoLeanDeg: -10f * reach,
                    chestBendDeg: -20f * reach,
                    skirtSwayDeg: -10f * reach,
                    skirtFlare: 0.04f * reach,
                    headTiltDeg: Lerp(-7f, -15f, reach) + microBreath * 0.5f,
                    ponytailRootDeg: 14f * reach + microBreath,
                    ponytailTipDeg: 22f * reach + microBreath * 1.5f,
                    frontLegDeg: 14f * reach,
                    frontKneeDeg: Lerp(-4f, -20f, reach),
                    frontAnkleDeg: Lerp(4f, 10f, reach),
                    backLegDeg: -12f * reach,
                    backKneeDeg: Lerp(-4f, -16f, reach),
                    backAnkleDeg: Lerp(4f, -8f, reach),
                    frontArmDeg: -22f * reach,
                    frontElbowDeg: Lerp(14f, 28f, reach),
                    backArmDeg: 64f * reach,
                    backElbowDeg: Lerp(16f, 8f, reach),
                    scaleX: 1f + 0.02f * reach,
                    scaleY: 1f - 0.02f * reach,
                    expression: BearChaseYunaExpression.Focused);
            }

            // 4. BearWakeUp (5.00 -> 6.00s): startled shock -> anticipation crouch -> backward hop -> cushioned landing -> guard stance
            if (t < BearChaseEntranceTimeline.WakeUpEnd)
            {
                float u = Clamp01((t - BearChaseEntranceTimeline.ReachEnd) /
                    (BearChaseEntranceTimeline.WakeUpEnd - BearChaseEntranceTimeline.ReachEnd));

                var guardPose = EvaluateShockedGuard(t, BearChaseEntranceBeat.BearWakeUp);

                if (u < 0.10f)
                {
                    // Snappy anticipation recoil crouch (yanks hand back to chest, head snaps up in shock)
                    float c = SmoothStep(u / 0.10f);
                    return new BearChaseYunaRigPose(
                        BearChaseEntranceBeat.BearWakeUp,
                        hipsBobY: Lerp(-0.026f, -0.068f, c),
                        torsoLeanDeg: Lerp(-10f, -6f, c),
                        chestBendDeg: Lerp(-20f, 4f, c),
                        skirtSwayDeg: Lerp(-10f, 8f, c),
                        skirtFlare: Lerp(0.04f, 0.12f, c),
                        headTiltDeg: Lerp(-15f, 10f, c),
                        ponytailRootDeg: Lerp(14f, -14f, c),
                        ponytailTipDeg: Lerp(22f, -22f, c),
                        frontLegDeg: Lerp(14f, 26f, c),
                        frontKneeDeg: Lerp(-20f, -52f, c),
                        frontAnkleDeg: Lerp(10f, 22f, c),
                        backLegDeg: Lerp(-12f, 10f, c),
                        backKneeDeg: Lerp(-16f, -46f, c),
                        backAnkleDeg: Lerp(-8f, 16f, c),
                        frontArmDeg: Lerp(-22f, -10f, c),
                        frontElbowDeg: Lerp(28f, 44f, c),
                        backArmDeg: Lerp(64f, 36f, c),
                        backElbowDeg: Lerp(8f, 96f, c),
                        scaleX: Lerp(1.02f, 1.10f, c),
                        scaleY: Lerp(0.98f, 0.90f, c),
                        expression: BearChaseYunaExpression.Shocked);
                }

                if (u < 0.38f)
                {
                    // Startled backward recoil hop in mid-air (waist arches back, arms defensive)
                    float hopU = (u - 0.10f) / 0.28f;
                    float arc = (float)Math.Sin(hopU * Math.PI);
                    return new BearChaseYunaRigPose(
                        BearChaseEntranceBeat.BearWakeUp,
                        hipsBobY: 0.030f * arc,
                        torsoLeanDeg: 8f * arc,
                        chestBendDeg: 16f * arc,
                        skirtSwayDeg: 14f * arc,
                        skirtFlare: 0.10f * arc,
                        headTiltDeg: 12f * arc + 4f,
                        ponytailRootDeg: -22f * arc,
                        ponytailTipDeg: -34f * arc,
                        frontLegDeg: 22f * arc,
                        frontKneeDeg: -44f * arc - 10f,
                        frontAnkleDeg: -16f * arc,
                        backLegDeg: -16f * arc,
                        backKneeDeg: -36f * arc - 8f,
                        backAnkleDeg: -14f * arc,
                        frontArmDeg: -16f - 10f * arc,
                        frontElbowDeg: 46f,
                        backArmDeg: 40f + 10f * arc,
                        backElbowDeg: 102f,
                        scaleX: 1f - 0.06f * arc,
                        scaleY: 1f + 0.07f * arc,
                        expression: BearChaseYunaExpression.Shocked);
                }

                if (u < 0.56f)
                {
                    // Cushioned landing crouch -> smooth recovery into startled guard stance
                    float landU = (u - 0.38f) / 0.18f;
                    var landCrouch = new BearChaseYunaRigPose(
                        BearChaseEntranceBeat.BearWakeUp,
                        hipsBobY: -0.062f,
                        torsoLeanDeg: -8f,
                        chestBendDeg: -6f,
                        skirtSwayDeg: -6f,
                        skirtFlare: 0.14f,
                        headTiltDeg: 6f,
                        ponytailRootDeg: -16f,
                        ponytailTipDeg: -24f,
                        frontLegDeg: 24f,
                        frontKneeDeg: -50f,
                        frontAnkleDeg: 20f,
                        backLegDeg: 8f,
                        backKneeDeg: -44f,
                        backAnkleDeg: 14f,
                        frontArmDeg: -12f,
                        frontElbowDeg: 44f,
                        backArmDeg: 36f,
                        backElbowDeg: 98f,
                        scaleX: 1.10f,
                        scaleY: 0.90f,
                        expression: BearChaseYunaExpression.Shocked);

                    return landU < 0.45f
                        ? landCrouch
                        : LerpPose(landCrouch, guardPose, SmoothStep((landU - 0.45f) / 0.55f));
                }

                return guardPose;
            }

            // 5. TransitionToChase (6.00 -> 6.50s): tense shocked guard stance during fade-out
            return EvaluateShockedGuard(t, BearChaseEntranceBeat.TransitionToChase);
        }

        /// <summary>
        /// Evaluates a single leg's hip, knee, and ankle angles so that during stance (phase 0..0.5),
        /// the planted foot moves backward at constant horizontal velocity matching the body's forward translation
        /// (zero foot sliding), while during swing (phase 0.5..1.0) the knee lifts cleanly off the floor.
        /// </summary>
        private static void EvaluateFootLockedLeg(float phase, out float hipDeg, out float kneeDeg, out float ankleDeg)
        {
            float p = PositiveMod(phase, 1f);
            if (p < 0.5f)
            {
                // Stance phase: linear horizontal sweep from +sinMax to -sinMax locks shoe to floorboard
                float stanceU = p * 2f;
                float sinTheta = Lerp(WalkStrideSinMax, -WalkStrideSinMax, stanceU);
                hipDeg = (float)Math.Asin(Math.Max(-0.95f, Math.Min(0.95f, sinTheta))) * RadToDeg;

                // Gentle knee cushioning at weight acceptance, straight through mid-stance
                float cushion = (float)Math.Sin(stanceU * Math.PI);
                kneeDeg = -4f - 5f * cushion;

                // Keep shoe sole flat on the floor during stance
                ankleDeg = -hipDeg * 0.72f - kneeDeg * 0.35f;
                return;
            }

            // Swing phase: lift knee clearly off the floor and bring foot forward to next contact
            float swingU = (p - 0.5f) * 2f;
            float lift = (float)Math.Sin(swingU * Math.PI);
            hipDeg = Lerp(-WalkStrideMaxDeg, WalkStrideMaxDeg, SmoothStep(swingU));
            kneeDeg = -6f - 44f * lift;
            ankleDeg = -14f * lift + (swingU > 0.75f ? Lerp(0f, 10f, (swingU - 0.75f) / 0.25f) : 0f);
        }

        private static BearChaseYunaRigPose EvaluateIdle(
            float t, BearChaseEntranceBeat beat, BearChaseYunaExpression expr, float headTiltBias)
        {
            float breathe = (float)Math.Sin(t * 2.8f);
            float chestLag = (float)Math.Sin(t * 2.8f - 0.5f);
            float hairRoot = (float)Math.Sin(t * 2.8f - 0.8f);
            float hairTip = (float)Math.Sin(t * 2.8f - 1.5f);
            return new BearChaseYunaRigPose(
                beat,
                hipsBobY: breathe * 0.008f,
                torsoLeanDeg: breathe * 0.9f,
                chestBendDeg: chestLag * 1.4f,
                skirtSwayDeg: hairRoot * 2.0f,
                skirtFlare: breathe * 0.012f,
                headTiltDeg: headTiltBias - chestLag * 1.5f,
                ponytailRootDeg: hairRoot * 4.5f,
                ponytailTipDeg: hairTip * 8.0f,
                frontLegDeg: 0f,
                frontKneeDeg: -4f,
                frontAnkleDeg: 4f,
                backLegDeg: 0f,
                backKneeDeg: -4f,
                backAnkleDeg: 4f,
                frontArmDeg: breathe * 2.5f,
                frontElbowDeg: 14f + breathe * 2.5f,
                backArmDeg: -breathe * 2.5f,
                backElbowDeg: 14f - breathe * 2.0f,
                scaleX: 1f - breathe * 0.008f,
                scaleY: 1f + breathe * 0.010f,
                expression: expr);
        }

        private static BearChaseYunaRigPose EvaluateShockedGuard(float t, BearChaseEntranceBeat beat)
        {
            float pant = (float)Math.Sin(t * 9.0f);
            return new BearChaseYunaRigPose(
                beat,
                hipsBobY: -0.012f + pant * 0.008f,
                torsoLeanDeg: 3f + pant * 1.2f,
                chestBendDeg: 4f - pant * 1.5f,
                skirtSwayDeg: pant * 3.5f,
                skirtFlare: 0.03f,
                headTiltDeg: 5f + pant * 1.5f,
                ponytailRootDeg: -6f + pant * 4f,
                ponytailTipDeg: -10f + pant * 7f,
                frontLegDeg: 10f,
                frontKneeDeg: -16f,
                frontAnkleDeg: 8f,
                backLegDeg: -10f,
                backKneeDeg: -14f,
                backAnkleDeg: -6f,
                frontArmDeg: -14f + pant * 2f,
                frontElbowDeg: 38f,
                backArmDeg: 36f + pant * 3f,
                backElbowDeg: 98f,
                scaleX: 1f - pant * 0.01f,
                scaleY: 1f + pant * 0.012f,
                expression: BearChaseYunaExpression.Shocked);
        }

        private static BearChaseYunaRigPose LerpPose(BearChaseYunaRigPose a, BearChaseYunaRigPose b, float t)
        {
            return new BearChaseYunaRigPose(
                b.Beat,
                Lerp(a.HipsBobY, b.HipsBobY, t),
                Lerp(a.TorsoLeanDeg, b.TorsoLeanDeg, t),
                Lerp(a.ChestBendDeg, b.ChestBendDeg, t),
                Lerp(a.SkirtSwayDeg, b.SkirtSwayDeg, t),
                Lerp(a.SkirtFlare, b.SkirtFlare, t),
                Lerp(a.HeadTiltDeg, b.HeadTiltDeg, t),
                Lerp(a.PonytailRootDeg, b.PonytailRootDeg, t),
                Lerp(a.PonytailTipDeg, b.PonytailTipDeg, t),
                Lerp(a.FrontLegDeg, b.FrontLegDeg, t),
                Lerp(a.FrontKneeDeg, b.FrontKneeDeg, t),
                Lerp(a.FrontAnkleDeg, b.FrontAnkleDeg, t),
                Lerp(a.BackLegDeg, b.BackLegDeg, t),
                Lerp(a.BackKneeDeg, b.BackKneeDeg, t),
                Lerp(a.BackAnkleDeg, b.BackAnkleDeg, t),
                Lerp(a.FrontArmDeg, b.FrontArmDeg, t),
                Lerp(a.FrontElbowDeg, b.FrontElbowDeg, t),
                Lerp(a.BackArmDeg, b.BackArmDeg, t),
                Lerp(a.BackElbowDeg, b.BackElbowDeg, t),
                Lerp(a.ScaleX, b.ScaleX, t),
                Lerp(a.ScaleY, b.ScaleY, t),
                b.Expression);
        }

        public static float SampleCyclic(float[] table, float normalizedPhase)
        {
            int n = table.Length;
            float scaled = PositiveMod(normalizedPhase, 1f) * n;
            int i1 = (int)Math.Floor(scaled) % n;
            float t = scaled - (float)Math.Floor(scaled);

            float p0 = table[(i1 - 1 + n) % n];
            float p1 = table[i1];
            float p2 = table[(i1 + 1) % n];
            float p3 = table[(i1 + 2) % n];

            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        private static float PositiveMod(float value, float period)
        {
            float m = value % period;
            return m < 0f ? m + period : m;
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
        private static float SmoothStep(float t)
        {
            float c = Clamp01(t);
            return c * c * (3f - 2f * c);
        }
    }
}
