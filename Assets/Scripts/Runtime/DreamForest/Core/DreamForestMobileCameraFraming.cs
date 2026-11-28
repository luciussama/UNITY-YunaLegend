using System;

namespace YunaLegend.DreamForest.Core
{
    public readonly struct DreamForestMobileCameraFrame
    {
        public readonly float CenterX;
        public readonly float CenterY;
        public readonly float OrthoSize;

        public DreamForestMobileCameraFrame(float centerX, float centerY, float orthoSize)
        {
            CenterX = centerX;
            CenterY = centerY;
            OrthoSize = orthoSize;
        }
    }

    /// <summary>
    /// Pure mobile camera framing evaluator for DreamForest_Gameplay (AGENTS.md §10),
    /// ensuring Yuna and upcoming climbing/glider/stealth hazards stay clearly framed on mobile screens.
    /// </summary>
    public static class DreamForestMobileCameraFraming
    {
        public static DreamForestMobileCameraFrame Evaluate(
            float playerWorldX,
            float playerWorldY,
            float worldWidth,
            float aspect,
            int facing,
            bool isNightmare,
            bool isGliding)
        {
            float safeAspect = Math.Max(0.45f, Math.Min(2.4f, aspect));
            bool portrait = safeAspect < 1f;
            float orthoSize = isGliding
                ? (portrait ? 6.2f : 4.6f)
                : (isNightmare ? (portrait ? 5.6f : 4.4f) : (portrait ? 5.8f : 4.5f));
            float halfW = orthoSize * safeAspect;
            float lookAhead = (isGliding ? 2.4f : 1.5f) * (facing >= 0 ? 1f : -1f);
            float cx = Math.Max(halfW, Math.Min(Math.Max(halfW, worldWidth - halfW), playerWorldX + lookAhead));
            float cy = isGliding ? playerWorldY + 0.4f : (isNightmare ? -0.4f : 0.2f);
            return new DreamForestMobileCameraFrame(cx, cy, orthoSize);
        }
    }
}
