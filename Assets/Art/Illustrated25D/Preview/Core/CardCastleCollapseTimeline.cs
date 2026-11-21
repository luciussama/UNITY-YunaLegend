using System;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    public readonly struct CardCastleTierSample
    {
        public float OffsetXMetres { get; }
        public float OffsetYMetres { get; }
        public float RotationDegrees { get; }
        public float Alpha { get; }

        public CardCastleTierSample(
            float offsetXMetres,
            float offsetYMetres,
            float rotationDegrees,
            float alpha)
        {
            OffsetXMetres = offsetXMetres;
            OffsetYMetres = offsetYMetres;
            RotationDegrees = rotationDegrees;
            Alpha = alpha;
        }
    }

    public readonly struct CardCastleFlutterCardSample
    {
        public float LocalXMetres { get; }
        public float LocalYMetres { get; }
        public float RotationDegrees { get; }
        public float ScaleX { get; }
        public float Alpha { get; }

        public CardCastleFlutterCardSample(
            float localXMetres,
            float localYMetres,
            float rotationDegrees,
            float scaleX,
            float alpha)
        {
            LocalXMetres = localXMetres;
            LocalYMetres = localYMetres;
            RotationDegrees = rotationDegrees;
            ScaleX = scaleX;
            Alpha = alpha;
        }
    }

    /// <summary>
    /// Avaliador matematico puro (Core, sem dependencia de UnityEngine) para a animacao
    /// da pilha/castelo de cartas de baralho (Plataforma 30) bambolendo ao ser pisada
    /// e desmoronando em 15 cartas de baralho inteiras e individuais logo apos a menina pular.
    /// </summary>
    public static class CardCastleCollapseTimeline
    {
        public const int PlatformIndex = 30;
        public const int TierCount = 4;
        public const int IndividualCardCount = 15;
        public const int FlutterCardCount = IndividualCardCount;
        public const float CollapseDurationSeconds = 1.45f;

        // Divisoes verticais em pixels (usadas apenas para o bamboleio sutil quando Yuna pisa na estrutura intacta):
        public static readonly int[] TierTopPixelY = { 780, 475, 215, 0 };
        public static readonly int[] TierBottomPixelY = { 1200, 780, 475, 215 };

        public readonly struct InitialCardLayoutSpec
        {
            public int Tier { get; }
            public float StartXMetres { get; }
            public float StartYMetres { get; }
            public float StartRotDegrees { get; }
            public bool IsHorizontalBridge { get; }
            public string RankText { get; }
            public string SuitText { get; }
            public bool IsRedSuit { get; }
            public bool IsCourtCard { get; }

            public InitialCardLayoutSpec(
                int tier,
                float startXMetres,
                float startYMetres,
                float startRotDegrees,
                bool isHorizontalBridge,
                string rankText,
                string suitText,
                bool isRedSuit,
                bool isCourtCard)
            {
                Tier = tier;
                StartXMetres = startXMetres;
                StartYMetres = startYMetres;
                StartRotDegrees = startRotDegrees;
                IsHorizontalBridge = isHorizontalBridge;
                RankText = rankText;
                SuitText = suitText;
                IsRedSuit = isRedSuit;
                IsCourtCard = isCourtCard;
            }
        }

        // As 15 cartas individuais que formam exatamente os 3 andares + 3 pontes horizontais de 30-card-castle-v1.png
        public static readonly InitialCardLayoutSpec[] Cards =
        {
            // Andar 0 (Base: 4 cartas inclinadas em V invertido: K♠, A♠, 4♠, 7♠)
            new InitialCardLayoutSpec(0, -0.82f, 0.72f, -14f, false, "K", "♠", false, true),
            new InitialCardLayoutSpec(0, -0.32f, 0.72f,  16f, false, "A", "♠", false, false),
            new InitialCardLayoutSpec(0,  0.30f, 0.74f, -15f, false, "4", "♠", false, false),
            new InitialCardLayoutSpec(0,  0.80f, 0.74f,  15f, false, "7", "♠", false, false),

            // Ponte horizontal 1 (entre Andar 0 e Andar 1)
            new InitialCardLayoutSpec(1,  0.00f, 1.44f,   3f, true,  "A", "♠", false, false),

            // Andar 1 (Meio: 4 cartas inclinadas: 3♥, A♥, Q♥, 7♠)
            new InitialCardLayoutSpec(1, -0.68f, 2.08f, -13f, false, "3", "♥", true,  false),
            new InitialCardLayoutSpec(1, -0.22f, 2.08f,  16f, false, "A", "♥", true,  false),
            new InitialCardLayoutSpec(1,  0.32f, 2.10f, -14f, false, "Q", "♥", true,  true),
            new InitialCardLayoutSpec(1,  0.74f, 2.10f,  13f, false, "7", "♠", false, false),

            // Ponte horizontal 2 (entre Andar 1 e Andar 2)
            new InitialCardLayoutSpec(2,  0.02f, 2.76f,   2f, true,  "2", "♥", true,  false),

            // Andar 2 (Superior: 4 cartas inclinadas: K♠, A♠, 3♥, 3♠)
            new InitialCardLayoutSpec(2, -0.60f, 3.36f, -13f, false, "K", "♠", false, true),
            new InitialCardLayoutSpec(2, -0.18f, 3.36f,  15f, false, "A", "♠", false, false),
            new InitialCardLayoutSpec(2,  0.26f, 3.38f, -14f, false, "3", "♥", true,  false),
            new InitialCardLayoutSpec(2,  0.64f, 3.38f,  15f, false, "3", "♠", false, false),

            // Ponte horizontal 3 (Carta A♠ do topo onde a menina pisa e impulsa o pulo)
            new InitialCardLayoutSpec(3,  0.00f, 4.02f,   2f, true,  "A", "♠", false, false)
        };

        public static CardCastleTierSample EvaluateSteppedWobble(int tierIndex, float steppedTimeSeconds)
        {
            int idx = ClampTier(tierIndex);
            float t = Math.Max(0f, steppedTimeSeconds);
            float tierAmp = (idx + 1) * 0.35f;
            float decay = (float)Math.Exp(-t * 2.2f);
            float wave = (float)Math.Sin(t * 24.0f + (idx * 0.55f));
            float rot = wave * (1.25f * tierAmp) * (0.45f + (0.55f * decay));
            float dx = wave * (0.018f * tierAmp);
            float dy = -0.012f * (idx + 1) * (float)Math.Abs(Math.Sin(t * 12f));
            return new CardCastleTierSample(dx, dy, rot, 1f);
        }

        public static CardCastleTierSample EvaluateCollapseTier(int tierIndex, float collapseElapsedSeconds)
        {
            int idx = ClampTier(tierIndex);
            float elapsed = Math.Max(0f, collapseElapsedSeconds);
            float delay = (3 - idx) * 0.055f;
            float t = Math.Max(0f, elapsed - delay);
            if (t <= 0f)
            {
                return new CardCastleTierSample(0f, 0f, 0f, 1f);
            }

            float dir = (idx % 2 == 0) ? 1f : -1f;
            if (idx == 3)
            {
                dir = 1.25f;
            }

            float slideVx = dir * (0.85f + (idx * 0.42f));
            float gravity = 8.4f + (idx * 1.6f);
            float dx = (slideVx * t) + ((float)Math.Sin(t * 14f + idx) * 0.08f * t);
            float dy = -0.5f * gravity * t * t;
            float rotSpeed = dir * (68f + (idx * 28f));
            float rot = rotSpeed * t;

            float fadeStart = CollapseDurationSeconds * 0.78f;
            float alpha = elapsed <= fadeStart
                ? 1f
                : Clamp01(1f - ((elapsed - fadeStart) / (CollapseDurationSeconds - fadeStart)));

            return new CardCastleTierSample(dx, dy, rot, alpha);
        }

        public static CardCastleFlutterCardSample EvaluateFlutterCard(int cardIndex, float collapseElapsedSeconds)
        {
            int idx = Math.Max(0, Math.Min(IndividualCardCount - 1, cardIndex));
            InitialCardLayoutSpec spec = Cards[idx];

            // Cascata de cima para baixo: o topo (Tier 3) se solta imediatamente (delay=0) com o impulso do pulo,
            // seguido pelo Tier 2 (0.045s), Tier 1 (0.105s) e Tier 0 (0.175s).
            float tierDelay = (3 - spec.Tier) * 0.055f + ((idx % 4) * 0.014f);
            float t = Math.Max(0f, collapseElapsedSeconds - tierDelay);

            if (t <= 0f)
            {
                return new CardCastleFlutterCardSample(
                    spec.StartXMetres,
                    spec.StartYMetres,
                    spec.StartRotDegrees,
                    1f,
                    1f);
            }

            // Cada carta desliza, abre para os lados e plana com resistencia do ar ate pousar espalhada no chao (y = 0.08..0.24m)
            float sideSign = spec.StartXMetres < -0.05f ? -1f : (spec.StartXMetres > 0.05f ? 1f : ((idx % 2 == 0) ? 1f : -1f));
            if (spec.Tier == 3)
            {
                // A carta do topo e chutada para tras (+X, direita) quando a menina pula para a esquerda
                sideSign = 1.35f;
            }

            float outwardSpeed = (0.75f + (spec.Tier * 0.32f) + ((idx % 3) * 0.18f)) * sideSign;
            float initialUpPop = spec.Tier == 3 ? 1.15f : (0.35f + (spec.Tier * 0.15f));
            float gravity = 7.4f;

            float rawY = spec.StartYMetres + (initialUpPop * t) - (0.5f * gravity * t * t);
            float floorRestY = 0.08f + ((idx % 5) * 0.032f);

            float y;
            float x;
            float rot;
            float flipScaleX;

            if (rawY > floorRestY)
            {
                y = rawY;
                float airFlutterX = (float)Math.Sin((t * 8.5f) + (idx * 1.1f)) * (0.22f + (spec.Tier * 0.06f)) * Math.Min(1f, t * 3f);
                x = spec.StartXMetres + (outwardSpeed * t) + airFlutterX;

                float targetFallAngle = spec.IsHorizontalBridge
                    ? (sideSign * 165f * t)
                    : (spec.StartRotDegrees + (sideSign * (95f + (idx % 3) * 32f) * t));
                rot = targetFallAngle + ((float)Math.Sin(t * 10f + idx) * 14f);
                flipScaleX = 0.42f + (0.58f * (float)Math.Abs(Math.Cos((t * 5.8f) + (idx * 0.6f))));
            }
            else
            {
                // Pousou no chao: desliza suavemente com atrito e fica deitada sobre o piso
                y = floorRestY;
                float discriminant = (initialUpPop * initialUpPop) + (2f * gravity * Math.Max(0.01f, spec.StartYMetres - floorRestY));
                float tLand = (initialUpPop + (float)Math.Sqrt(discriminant)) / gravity;
                float slideAfterLand = Math.Min(0.18f, Math.Max(0f, t - tLand)) * outwardSpeed * 0.35f;
                float landFlutterX = (float)Math.Sin((tLand * 8.5f) + (idx * 1.1f)) * (0.22f + (spec.Tier * 0.06f));
                x = spec.StartXMetres + (outwardSpeed * tLand) + landFlutterX + slideAfterLand;

                // Deita quase horizontal no chao (82..96 graus para cartas verticais, 4..14 graus para pontes horizontais)
                float restAngle = spec.IsHorizontalBridge
                    ? (sideSign * (6f + (idx % 3) * 4f))
                    : (sideSign * (84f + ((idx % 4) * 4f)));
                float settleT = Clamp01((t - tLand) / 0.16f);
                float angleAtLand = spec.IsHorizontalBridge
                    ? (sideSign * 165f * tLand)
                    : (spec.StartRotDegrees + (sideSign * (95f + (idx % 3) * 32f) * tLand));
                rot = angleAtLand + ((restAngle - angleAtLand) * settleT);
                flipScaleX = 0.85f + (0.15f * settleT);
            }

            float fadeStart = CollapseDurationSeconds * 0.86f;
            float alpha = collapseElapsedSeconds <= fadeStart
                ? 1f
                : Clamp01(1f - ((collapseElapsedSeconds - fadeStart) / (CollapseDurationSeconds - fadeStart)));

            return new CardCastleFlutterCardSample(x, y, rot, flipScaleX, alpha);
        }

        private static int ClampTier(int tierIndex)
        {
            if (tierIndex < 0) return 0;
            if (tierIndex >= TierCount) return TierCount - 1;
            return tierIndex;
        }

        private static float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }
    }
}
