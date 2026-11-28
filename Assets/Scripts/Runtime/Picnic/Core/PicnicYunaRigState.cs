using System;

namespace YunaLegend.Picnic.Core
{

    /// <summary>
    /// Articulated 16-joint 2D skeletal pose for Yuna (hips, chest, skirt, head, 2-bone pigtails,
    /// 3-bone legs with hip/knee/ankle, and 2-bone arms with shoulder/elbow) plus facial expression.
    /// </summary>
    public readonly struct PicnicYunaRigPose
    {
        public PicnicYunaMotionKind Motion { get; }
        public float HipsBobY { get; }
        public float TorsoLeanDeg { get; }
        public float ChestBendDeg { get; }
        public float SkirtSwayDeg { get; }
        public float SkirtFlare { get; }
        public float HeadTiltDeg { get; }

        public float FrontPigtailDeg { get; }
        public float FrontPigtailTipDeg { get; }
        public float BackPigtailDeg { get; }
        public float BackPigtailTipDeg { get; }

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
        public PicnicYunaExpression Expression { get; }
        public bool UseWorriedFace => Expression != PicnicYunaExpression.Happy;

        public PicnicYunaRigPose(
            PicnicYunaMotionKind motion,
            float hipsBobY,
            float torsoLeanDeg,
            float chestBendDeg,
            float skirtSwayDeg,
            float skirtFlare,
            float headTiltDeg,
            float frontPigtailDeg,
            float frontPigtailTipDeg,
            float backPigtailDeg,
            float backPigtailTipDeg,
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
            PicnicYunaExpression expression)
        {
            Motion = motion;
            HipsBobY = hipsBobY;
            TorsoLeanDeg = torsoLeanDeg;
            ChestBendDeg = chestBendDeg;
            SkirtSwayDeg = skirtSwayDeg;
            SkirtFlare = skirtFlare;
            HeadTiltDeg = headTiltDeg;
            FrontPigtailDeg = frontPigtailDeg;
            FrontPigtailTipDeg = frontPigtailTipDeg;
            BackPigtailDeg = backPigtailDeg;
            BackPigtailTipDeg = backPigtailTipDeg;
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
    /// Pure deterministic 16-joint cartoon 2D skeletal pose evaluator for Yuna.
    /// Includes dedicated biomechanics for normal run, heavy grass trudging (strain),
    /// unstable gravel slipping (fear + windmilling arms), and frantic beetle fleeing (panic sprint).
    /// </summary>
    public static class PicnicYunaRigState
    {
        private const float TwoPi = (float)(Math.PI * 2.0);
        public const float StridePixelsPerCycle = 96f;

        // 1. Normal Run (Contact -> Down -> Passing -> Push-off -> Toe-off -> High-Knee Recovery -> Swing -> Pre-contact)
        private static readonly float[] RunHipDeg         = {  28f,  15f,  -4f, -24f, -30f, -14f,  10f,  26f };
        private static readonly float[] RunKneeDeg        = {  -8f, -26f, -14f, -12f, -34f, -66f, -44f, -16f };
        private static readonly float[] RunAnkleDeg       = {  16f,   2f,  -4f, -24f, -28f, -14f,   2f,  12f };
        private static readonly float[] RunArmShoulderDeg = { -26f, -18f,   0f,  22f,  30f,  18f,  -2f, -22f };
        private static readonly float[] RunArmElbowDeg    = {  34f,  40f,  48f,  58f,  62f,  52f,  42f,  36f };
        private static readonly float[] RunHipsBobY       = { 0.004f, -0.028f, -0.004f, 0.044f, 0.004f, -0.028f, -0.004f, 0.044f };
        private static readonly float[] RunSquashY        = { 1.000f,  0.955f,  0.995f, 1.042f, 1.000f,  0.955f,  0.995f, 1.042f };

        // 2. Heavy Grass Trudge (dragging feet through thick tall grass: high knee pull, heavy sink, forward strain)
        private static readonly float[] GrassHipDeg         = {  24f,  16f,   2f, -16f, -24f,  -4f,  22f,  28f };
        private static readonly float[] GrassKneeDeg        = { -14f, -34f, -22f, -18f, -42f, -76f, -58f, -22f };
        private static readonly float[] GrassAnkleDeg       = {  12f,   0f,  -6f, -22f, -34f, -28f, -10f,   8f };
        private static readonly float[] GrassArmShoulderDeg = { -16f,  -8f,  10f,  26f,  34f,  18f,  -2f, -14f };
        private static readonly float[] GrassArmElbowDeg    = {  48f,  56f,  62f,  66f,  64f,  58f,  52f,  48f };
        private static readonly float[] GrassHipsBobY       = { -0.012f, -0.044f, -0.024f, 0.018f, -0.012f, -0.044f, -0.024f, 0.018f };

        // 3. Gravel Slip & Stumble (foot slides forward -> body pitches backward -> windmilling arms catch balance)
        private static readonly float[] GravelHipDeg         = {  38f,  22f,  -4f, -28f, -22f,  -6f,  18f,  34f };
        private static readonly float[] GravelKneeDeg        = {  -4f, -32f, -18f, -12f, -46f, -58f, -26f,  -6f };
        private static readonly float[] GravelAnkleDeg       = {  30f,  10f,  -6f, -28f, -20f,  -8f,  14f,  26f };
        private static readonly float[] GravelTorsoLeanDeg   = {  14f,   6f,  -8f, -12f,  12f,   4f, -10f,   8f };
        private static readonly float[] GravelArmShoulderDeg = {  76f,  48f,  18f, -32f,  84f,  52f,  12f, -26f };
        private static readonly float[] GravelArmElbowDeg    = {  18f,  26f,  36f,  24f,  16f,  28f,  34f,  22f };
        private static readonly float[] GravelHipsBobY       = { -0.022f, -0.042f, 0.016f, 0.032f, -0.026f, -0.038f, 0.014f, 0.028f };

        // 4. Beetle Panic Sprint (steep forward lean, explosive stride, deep -82 deg heel kick, frantic high arms)
        private static readonly float[] PanicHipDeg         = {  40f,  20f,  -8f, -36f, -42f, -18f,  16f,  38f };
        private static readonly float[] PanicKneeDeg        = {  -8f, -30f, -16f, -14f, -48f, -84f, -56f, -18f };
        private static readonly float[] PanicAnkleDeg       = {  22f,   0f,  -8f, -32f, -36f, -18f,   4f,  16f };
        private static readonly float[] PanicArmShoulderDeg = { -40f, -24f,  10f,  44f,  54f,  30f,  -6f, -34f };
        private static readonly float[] PanicArmElbowDeg    = {  52f,  60f,  68f,  74f,  76f,  66f,  58f,  52f };
        private static readonly float[] PanicHipsBobY       = { 0.008f, -0.034f, 0.002f, 0.058f, 0.008f, -0.034f, 0.002f, 0.058f };
        private static readonly float[] PanicSquashY        = { 1.000f,  0.935f, 1.000f, 1.065f, 1.000f,  0.935f, 1.000f, 1.065f };

        public static PicnicYunaMotionKind ClassifyMotion(
            bool onGround,
            float vx,
            float vy,
            PicnicTerrain terrain,
            float discomfort,
            bool hasBalancePose,
            bool isPanic,
            float hurtRemaining)
        {
            if (hurtRemaining > 0f)
            {
                return PicnicYunaMotionKind.Hurt;
            }

            if (!onGround)
            {
                if (vy < -180f)
                {
                    return PicnicYunaMotionKind.JumpRise;
                }

                if (vy < 80f)
                {
                    return PicnicYunaMotionKind.JumpApex;
                }

                return PicnicYunaMotionKind.Fall;
            }

            if (hasBalancePose)
            {
                return PicnicYunaMotionKind.Balance;
            }

            bool moving = Math.Abs(vx) > 15f;
            if (!moving)
            {
                return PicnicYunaMotionKind.Idle;
            }

            if (isPanic)
            {
                return PicnicYunaMotionKind.PanicRun;
            }

            if (discomfort > 0.4f)
            {
                return terrain == PicnicTerrain.Gravel
                    ? PicnicYunaMotionKind.GravelStumble
                    : PicnicYunaMotionKind.DiscomfortWalk;
            }

            return PicnicYunaMotionKind.Run;
        }

        public static PicnicYunaRigPose Evaluate(
            bool onGround,
            float vx,
            float vy,
            PicnicTerrain terrain,
            float discomfort,
            float time,
            bool hasBalancePose,
            bool isPanic,
            float hurtRemaining,
            float? strideDistancePx = null)
        {
            PicnicYunaMotionKind motion = ClassifyMotion(
                onGround, vx, vy, terrain, discomfort, hasBalancePose, isPanic, hurtRemaining);
            PicnicYunaExpression defaultExpr = ResolveExpression(motion, terrain, discomfort, isPanic, hurtRemaining);

            switch (motion)
            {
                case PicnicYunaMotionKind.Idle:
                    return EvaluateIdle(time, defaultExpr);

                case PicnicYunaMotionKind.Run:
                    return EvaluateRun(time, vx, defaultExpr, strideDistancePx);

                case PicnicYunaMotionKind.PanicRun:
                    return EvaluatePanicRun(time, vx, strideDistancePx);

                case PicnicYunaMotionKind.DiscomfortWalk:
                    return EvaluateGrassTrudge(time, strideDistancePx);

                case PicnicYunaMotionKind.GravelStumble:
                    return EvaluateGravelSlip(time, strideDistancePx);

                case PicnicYunaMotionKind.Balance:
                    return EvaluateBalance(time, defaultExpr);

                case PicnicYunaMotionKind.JumpRise:
                    // Velocity-driven takeoff impulse arch at the waist: strong upward chest arch
                    // at launch (vy ~ -480) smoothly easing as Yuna approaches apex (vy ~ -180).
                    float launchT = Clamp((-vy - 180f) / 300f, 0f, 1f);
                    return new PicnicYunaRigPose(
                        motion,
                        hipsBobY: 0.025f + launchT * 0.028f,
                        torsoLeanDeg: -6f - launchT * 8f,
                        chestBendDeg: 10f + launchT * 14f,
                        skirtSwayDeg: -8f - launchT * 8f,
                        skirtFlare: -0.04f - launchT * 0.05f,
                        headTiltDeg: 4f + launchT * 6f,
                        frontPigtailDeg: 14f + launchT * 12f,
                        frontPigtailTipDeg: 22f + launchT * 18f,
                        backPigtailDeg: 18f + launchT * 14f,
                        backPigtailTipDeg: 26f + launchT * 20f,
                        frontLegDeg: 28f + launchT * 14f,
                        frontKneeDeg: -42f - launchT * 18f,
                        frontAnkleDeg: -12f - launchT * 10f,
                        backLegDeg: -16f - launchT * 14f,
                        backKneeDeg: -32f + launchT * 14f,
                        backAnkleDeg: -20f - launchT * 14f,
                        frontArmDeg: 38f + launchT * 26f,
                        frontElbowDeg: 42f - launchT * 12f,
                        backArmDeg: 12f + launchT * 24f,
                        backElbowDeg: 34f - launchT * 10f,
                        scaleX: 0.96f - launchT * 0.06f,
                        scaleY: 1.05f + launchT * 0.07f,
                        expression: defaultExpr);

                case PicnicYunaMotionKind.JumpApex:
                    return new PicnicYunaRigPose(
                        motion,
                        hipsBobY: 0.02f,
                        torsoLeanDeg: -2f,
                        chestBendDeg: 2f,
                        skirtSwayDeg: 0f,
                        skirtFlare: 0.06f,
                        headTiltDeg: 1f,
                        frontPigtailDeg: 4f,
                        frontPigtailTipDeg: 12f,
                        backPigtailDeg: 6f,
                        backPigtailTipDeg: 14f,
                        frontLegDeg: 24f,
                        frontKneeDeg: -38f,
                        frontAnkleDeg: -8f,
                        backLegDeg: -14f,
                        backKneeDeg: -28f,
                        backAnkleDeg: -12f,
                        frontArmDeg: 28f,
                        frontElbowDeg: 34f,
                        backArmDeg: -16f,
                        backElbowDeg: 28f,
                        scaleX: 1.02f,
                        scaleY: 0.98f,
                        expression: defaultExpr);

                case PicnicYunaMotionKind.Fall:
                    return new PicnicYunaRigPose(
                        motion,
                        hipsBobY: -0.01f,
                        torsoLeanDeg: 5f,
                        chestBendDeg: -4f,
                        skirtSwayDeg: 14f,
                        skirtFlare: 0.14f,
                        headTiltDeg: -4f,
                        frontPigtailDeg: -24f,
                        frontPigtailTipDeg: -30f,
                        backPigtailDeg: -28f,
                        backPigtailTipDeg: -34f,
                        frontLegDeg: 16f,
                        frontKneeDeg: -22f,
                        frontAnkleDeg: 10f,
                        backLegDeg: -16f,
                        backKneeDeg: -36f,
                        backAnkleDeg: -10f,
                        frontArmDeg: 52f,
                        frontElbowDeg: 28f,
                        backArmDeg: 34f,
                        backElbowDeg: 24f,
                        scaleX: 0.96f,
                        scaleY: 1.05f,
                        expression: defaultExpr);

                case PicnicYunaMotionKind.Hurt:
                default:
                    float wobble = (float)Math.Sin(time * 28f) * 8f;
                    return new PicnicYunaRigPose(
                        PicnicYunaMotionKind.Hurt,
                        hipsBobY: 0.02f,
                        torsoLeanDeg: 12f + wobble * 0.4f,
                        chestBendDeg: -6f,
                        skirtSwayDeg: -12f + wobble,
                        skirtFlare: 0.08f,
                        headTiltDeg: -10f + wobble * 0.5f,
                        frontPigtailDeg: -18f + wobble,
                        frontPigtailTipDeg: -24f - wobble,
                        backPigtailDeg: -22f - wobble,
                        backPigtailTipDeg: -28f + wobble,
                        frontLegDeg: -14f,
                        frontKneeDeg: -28f,
                        frontAnkleDeg: -10f,
                        backLegDeg: 14f,
                        backKneeDeg: -22f,
                        backAnkleDeg: 8f,
                        frontArmDeg: 44f,
                        frontElbowDeg: 42f,
                        backArmDeg: -32f,
                        backElbowDeg: 36f,
                        scaleX: 1.05f,
                        scaleY: 0.95f,
                        expression: PicnicYunaExpression.Panic);
            }
        }

        private static PicnicYunaExpression ResolveExpression(
            PicnicYunaMotionKind motion,
            PicnicTerrain terrain,
            float discomfort,
            bool isPanic,
            float hurtRemaining)
        {
            if (isPanic || hurtRemaining > 0f || motion == PicnicYunaMotionKind.PanicRun || motion == PicnicYunaMotionKind.Hurt)
            {
                return PicnicYunaExpression.Panic;
            }

            if (motion == PicnicYunaMotionKind.GravelStumble || (terrain == PicnicTerrain.Gravel && discomfort > 0.4f))
            {
                return PicnicYunaExpression.Scared;
            }

            if (motion == PicnicYunaMotionKind.DiscomfortWalk || discomfort > 0.4f)
            {
                return PicnicYunaExpression.Strain;
            }

            return PicnicYunaExpression.Happy;
        }

        private static PicnicYunaRigPose EvaluateIdle(float time, PicnicYunaExpression expression)
        {
            float breathe = (float)Math.Sin(time * 2.8f);
            float chestLag = (float)Math.Sin(time * 2.8f - 0.5f);
            float hairRoot = (float)Math.Sin(time * 2.8f - 0.8f);
            float hairTip = (float)Math.Sin(time * 2.8f - 1.5f);
            return new PicnicYunaRigPose(
                PicnicYunaMotionKind.Idle,
                hipsBobY: breathe * 0.010f,
                torsoLeanDeg: breathe * 1.1f,
                chestBendDeg: chestLag * 1.6f,
                skirtSwayDeg: hairRoot * 2.5f,
                skirtFlare: breathe * 0.015f,
                headTiltDeg: -chestLag * 1.8f,
                frontPigtailDeg: hairRoot * 4.5f,
                frontPigtailTipDeg: hairTip * 7.5f,
                backPigtailDeg: hairRoot * 5.5f,
                backPigtailTipDeg: hairTip * 9.0f,
                frontLegDeg: 0f,
                frontKneeDeg: -3f,
                frontAnkleDeg: 3f,
                backLegDeg: 0f,
                backKneeDeg: -3f,
                backAnkleDeg: 3f,
                frontArmDeg: breathe * 3.0f,
                frontElbowDeg: 10f + breathe * 3.5f,
                backArmDeg: -breathe * 2.5f,
                backElbowDeg: 8f - breathe * 2.5f,
                scaleX: 1f - breathe * 0.010f,
                scaleY: 1f + breathe * 0.014f,
                expression: expression);
        }

        private static PicnicYunaRigPose EvaluateRun(
            float time,
            float vx,
            PicnicYunaExpression expression,
            float? strideDistancePx)
        {
            float phase = strideDistancePx.HasValue
                ? PositiveMod(strideDistancePx.Value / StridePixelsPerCycle, 1f)
                : PositiveMod(time * 2.25f * Clamp(Math.Abs(vx) / 210f, 0.75f, 1.35f), 1f);
            float opposite = PositiveMod(phase + 0.5f, 1f);

            float bobY = SampleCyclic(RunHipsBobY, phase);
            float scaleY = SampleCyclic(RunSquashY, phase);
            float scaleX = 1f / scaleY;
            float doubleRad = phase * 2f * TwoPi;
            float hairRootWave = (float)Math.Sin(doubleRad - 0.90f);
            float hairTipWave = (float)Math.Sin(doubleRad - 1.85f);

            return new PicnicYunaRigPose(
                PicnicYunaMotionKind.Run,
                hipsBobY: bobY,
                torsoLeanDeg: -7.0f + (float)Math.Sin(doubleRad) * 2.5f,
                chestBendDeg: (float)Math.Sin(doubleRad - 0.55f) * 4.0f,
                skirtSwayDeg: (float)Math.Sin(phase * TwoPi - 0.55f) * 10f,
                skirtFlare: Math.Max(0f, -bobY * 3.2f) + 0.02f,
                headTiltDeg: 3.0f + (float)Math.Sin(doubleRad - 1.05f) * 3.5f,
                frontPigtailDeg: 10f + hairRootWave * 12f,
                frontPigtailTipDeg: 14f + hairTipWave * 20f,
                backPigtailDeg: 14f + hairRootWave * 15f,
                backPigtailTipDeg: 18f + hairTipWave * 24f,
                frontLegDeg: SampleCyclic(RunHipDeg, phase),
                frontKneeDeg: SampleCyclic(RunKneeDeg, phase),
                frontAnkleDeg: SampleCyclic(RunAnkleDeg, phase),
                backLegDeg: SampleCyclic(RunHipDeg, opposite),
                backKneeDeg: SampleCyclic(RunKneeDeg, opposite),
                backAnkleDeg: SampleCyclic(RunAnkleDeg, opposite),
                frontArmDeg: SampleCyclic(RunArmShoulderDeg, phase),
                frontElbowDeg: SampleCyclic(RunArmElbowDeg, phase),
                backArmDeg: SampleCyclic(RunArmShoulderDeg, opposite),
                backElbowDeg: SampleCyclic(RunArmElbowDeg, opposite),
                scaleX: scaleX,
                scaleY: scaleY,
                expression: expression);
        }

        private static PicnicYunaRigPose EvaluateGrassTrudge(float time, float? strideDistancePx)
        {
            float phase = strideDistancePx.HasValue
                ? PositiveMod(strideDistancePx.Value / (StridePixelsPerCycle * 0.72f), 1f)
                : PositiveMod(time * 1.35f, 1f);
            float opposite = PositiveMod(phase + 0.5f, 1f);
            float doubleRad = phase * 2f * TwoPi;
            float effortRock = (float)Math.Sin(doubleRad);
            float bobY = SampleCyclic(GrassHipsBobY, phase);

            return new PicnicYunaRigPose(
                PicnicYunaMotionKind.DiscomfortWalk,
                hipsBobY: bobY,
                torsoLeanDeg: -13.5f + effortRock * 4.5f,
                chestBendDeg: -4.5f + (float)Math.Sin(doubleRad - 0.5f) * 5.5f,
                skirtSwayDeg: -6f + (float)Math.Sin(phase * TwoPi - 0.6f) * 8f,
                skirtFlare: 0.04f + Math.Max(0f, -bobY * 2.4f),
                headTiltDeg: -7.5f + (float)Math.Sin(doubleRad - 0.9f) * 4.5f,
                frontPigtailDeg: 6f + (float)Math.Sin(doubleRad - 0.9f) * 9f,
                frontPigtailTipDeg: 10f + (float)Math.Sin(doubleRad - 1.7f) * 15f,
                backPigtailDeg: 9f + (float)Math.Sin(doubleRad - 0.9f) * 11f,
                backPigtailTipDeg: 13f + (float)Math.Sin(doubleRad - 1.7f) * 18f,
                frontLegDeg: SampleCyclic(GrassHipDeg, phase),
                frontKneeDeg: SampleCyclic(GrassKneeDeg, phase),
                frontAnkleDeg: SampleCyclic(GrassAnkleDeg, phase),
                backLegDeg: SampleCyclic(GrassHipDeg, opposite),
                backKneeDeg: SampleCyclic(GrassKneeDeg, opposite),
                backAnkleDeg: SampleCyclic(GrassAnkleDeg, opposite),
                frontArmDeg: SampleCyclic(GrassArmShoulderDeg, phase),
                frontElbowDeg: SampleCyclic(GrassArmElbowDeg, phase),
                backArmDeg: SampleCyclic(GrassArmShoulderDeg, opposite),
                backElbowDeg: SampleCyclic(GrassArmElbowDeg, opposite),
                scaleX: 1.025f - effortRock * 0.02f,
                scaleY: 0.975f + effortRock * 0.02f,
                expression: PicnicYunaExpression.Strain);
        }

        private static PicnicYunaRigPose EvaluateGravelSlip(float time, float? strideDistancePx)
        {
            float phase = strideDistancePx.HasValue
                ? PositiveMod(strideDistancePx.Value / (StridePixelsPerCycle * 0.78f), 1f)
                : PositiveMod(time * 2.15f, 1f);
            float opposite = PositiveMod(phase + 0.5f, 1f);
            float doubleRad = phase * 2f * TwoPi;
            float torsoLean = SampleCyclic(GravelTorsoLeanDeg, phase);
            float bobY = SampleCyclic(GravelHipsBobY, phase);

            return new PicnicYunaRigPose(
                PicnicYunaMotionKind.GravelStumble,
                hipsBobY: bobY,
                torsoLeanDeg: torsoLean,
                chestBendDeg: -torsoLean * 0.45f + (float)Math.Sin(doubleRad - 0.4f) * 6f,
                skirtSwayDeg: -torsoLean * 0.9f + (float)Math.Sin(phase * TwoPi - 0.5f) * 12f,
                skirtFlare: 0.08f + Math.Max(0f, -bobY * 2.5f),
                headTiltDeg: -torsoLean * 0.55f + (float)Math.Sin(doubleRad - 0.9f) * 5f,
                frontPigtailDeg: -torsoLean * 0.8f + (float)Math.Sin(doubleRad - 0.8f) * 16f,
                frontPigtailTipDeg: -torsoLean * 1.1f + (float)Math.Sin(doubleRad - 1.7f) * 26f,
                backPigtailDeg: -torsoLean * 0.9f + (float)Math.Sin(doubleRad - 0.8f) * 18f,
                backPigtailTipDeg: -torsoLean * 1.2f + (float)Math.Sin(doubleRad - 1.7f) * 28f,
                frontLegDeg: SampleCyclic(GravelHipDeg, phase),
                frontKneeDeg: SampleCyclic(GravelKneeDeg, phase),
                frontAnkleDeg: SampleCyclic(GravelAnkleDeg, phase),
                backLegDeg: SampleCyclic(GravelHipDeg, opposite),
                backKneeDeg: SampleCyclic(GravelKneeDeg, opposite),
                backAnkleDeg: SampleCyclic(GravelAnkleDeg, opposite),
                frontArmDeg: SampleCyclic(GravelArmShoulderDeg, phase),
                frontElbowDeg: SampleCyclic(GravelArmElbowDeg, phase),
                backArmDeg: SampleCyclic(GravelArmShoulderDeg, opposite),
                backElbowDeg: SampleCyclic(GravelArmElbowDeg, opposite),
                scaleX: 1.03f,
                scaleY: 0.97f,
                expression: PicnicYunaExpression.Scared);
        }

        private static PicnicYunaRigPose EvaluatePanicRun(float time, float vx, float? strideDistancePx)
        {
            float phase = strideDistancePx.HasValue
                ? PositiveMod(strideDistancePx.Value / (StridePixelsPerCycle * 0.80f), 1f)
                : PositiveMod(time * 3.35f * Clamp(Math.Abs(vx) / 240f, 0.85f, 1.45f), 1f);
            float opposite = PositiveMod(phase + 0.5f, 1f);

            float bobY = SampleCyclic(PanicHipsBobY, phase);
            float scaleY = SampleCyclic(PanicSquashY, phase);
            float scaleX = 1f / scaleY;
            float doubleRad = phase * 2f * TwoPi;
            float hairRootWave = (float)Math.Sin(doubleRad - 0.85f);
            float hairTipWave = (float)Math.Sin(doubleRad - 1.80f);

            return new PicnicYunaRigPose(
                PicnicYunaMotionKind.PanicRun,
                hipsBobY: bobY,
                torsoLeanDeg: -17.5f + (float)Math.Sin(doubleRad) * 3.8f,
                chestBendDeg: (float)Math.Sin(doubleRad - 0.5f) * 6.5f,
                skirtSwayDeg: -14f + (float)Math.Sin(phase * TwoPi - 0.5f) * 14f,
                skirtFlare: 0.08f + Math.Max(0f, -bobY * 3.5f),
                headTiltDeg: 8.5f + (float)Math.Sin(doubleRad - 1.0f) * 5.0f,
                frontPigtailDeg: 24f + hairRootWave * 16f,
                frontPigtailTipDeg: 36f + hairTipWave * 28f,
                backPigtailDeg: 28f + hairRootWave * 18f,
                backPigtailTipDeg: 42f + hairTipWave * 32f,
                frontLegDeg: SampleCyclic(PanicHipDeg, phase),
                frontKneeDeg: SampleCyclic(PanicKneeDeg, phase),
                frontAnkleDeg: SampleCyclic(PanicAnkleDeg, phase),
                backLegDeg: SampleCyclic(PanicHipDeg, opposite),
                backKneeDeg: SampleCyclic(PanicKneeDeg, opposite),
                backAnkleDeg: SampleCyclic(PanicAnkleDeg, opposite),
                frontArmDeg: SampleCyclic(PanicArmShoulderDeg, phase),
                frontElbowDeg: SampleCyclic(PanicArmElbowDeg, phase),
                backArmDeg: SampleCyclic(PanicArmShoulderDeg, opposite),
                backElbowDeg: SampleCyclic(PanicArmElbowDeg, opposite),
                scaleX: scaleX,
                scaleY: scaleY,
                expression: PicnicYunaExpression.Panic);
        }

        private static PicnicYunaRigPose EvaluateBalance(float time, PicnicYunaExpression expression)
        {
            float sway = (float)Math.Sin(time * 6.5f);
            return new PicnicYunaRigPose(
                PicnicYunaMotionKind.Balance,
                hipsBobY: 0.01f,
                torsoLeanDeg: sway * 7f,
                chestBendDeg: -sway * 5f,
                skirtSwayDeg: sway * 9f,
                skirtFlare: 0.03f,
                headTiltDeg: -sway * 5f,
                frontPigtailDeg: sway * 12f,
                frontPigtailTipDeg: (float)Math.Sin(time * 6.5f - 0.9f) * 18f,
                backPigtailDeg: sway * 14f,
                backPigtailTipDeg: (float)Math.Sin(time * 6.5f - 0.9f) * 20f,
                frontLegDeg: 10f,
                frontKneeDeg: -14f,
                frontAnkleDeg: 6f,
                backLegDeg: -10f,
                backKneeDeg: -14f,
                backAnkleDeg: -6f,
                frontArmDeg: 58f + sway * 12f,
                frontElbowDeg: 22f,
                backArmDeg: -52f - sway * 12f,
                backElbowDeg: 22f,
                scaleX: 1f,
                scaleY: 1f,
                expression: expression);
        }

        public static PicnicYunaRigPose EvaluateJumpAnticipation(PicnicYunaExpression expression)
        {
            // Quick coiled crouch right before/at takeoff: knees flexed, torso leaning forward,
            // both arms reaching forward to gather upward momentum.
            return new PicnicYunaRigPose(
                PicnicYunaMotionKind.JumpAnticipation,
                hipsBobY: -0.075f,
                torsoLeanDeg: -16f,
                chestBendDeg: -10f,
                skirtSwayDeg: -10f,
                skirtFlare: 0.11f,
                headTiltDeg: 8f,
                frontPigtailDeg: -12f,
                frontPigtailTipDeg: -18f,
                backPigtailDeg: -14f,
                backPigtailTipDeg: -22f,
                frontLegDeg: 32f,
                frontKneeDeg: -64f,
                frontAnkleDeg: 28f,
                backLegDeg: 18f,
                backKneeDeg: -58f,
                backAnkleDeg: 24f,
                frontArmDeg: 48f,
                frontElbowDeg: 36f,
                backArmDeg: 36f,
                backElbowDeg: 32f,
                scaleX: 1.11f,
                scaleY: 0.89f,
                expression: expression);
        }

        public static PicnicYunaRigPose EvaluateJumpLanding(PicnicYunaExpression expression)
        {
            // Cushioned landing crouch absorbing downward impact before smoothly rising back to run/idle.
            return new PicnicYunaRigPose(
                PicnicYunaMotionKind.JumpLand,
                hipsBobY: -0.072f,
                torsoLeanDeg: -14f,
                chestBendDeg: -9f,
                skirtSwayDeg: 6f,
                skirtFlare: 0.15f,
                headTiltDeg: -6f,
                frontPigtailDeg: -16f,
                frontPigtailTipDeg: -24f,
                backPigtailDeg: -18f,
                backPigtailTipDeg: -26f,
                frontLegDeg: 30f,
                frontKneeDeg: -60f,
                frontAnkleDeg: 24f,
                backLegDeg: 16f,
                backKneeDeg: -54f,
                backAnkleDeg: 20f,
                frontArmDeg: 36f,
                frontElbowDeg: 40f,
                backArmDeg: -22f,
                backElbowDeg: 34f,
                scaleX: 1.13f,
                scaleY: 0.87f,
                expression: expression);
        }

        /// <summary>
        /// Cyclic Catmull-Rom spline sampling across an evenly-spaced periodic keyframe table.
        /// </summary>
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

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
