using System;
using System.Collections.Generic;

namespace YunaLegend.DreamForest.Core
{

    public readonly struct DreamForestInput
    {
        public readonly float X;
        public readonly float Y;
        public readonly bool Jump;
        public readonly bool Advance;

        public DreamForestInput(float x, float y, bool jump, bool advance = false)
        {
            X = x;
            Y = y;
            Jump = jump;
            Advance = advance;
        }
    }

    public readonly struct DreamForestEvent
    {
        public readonly DreamForestEventKind Kind;
        public readonly string TargetId;

        public DreamForestEvent(DreamForestEventKind kind, string targetId = null)
        {
            Kind = kind;
            TargetId = targetId;
        }
    }

    public readonly struct DreamForestOpeningBeat
    {
        public readonly string Shot;
        public readonly string Speaker;
        public readonly string Text;
        public readonly float Duration;
        public readonly float MinDuration;

        public DreamForestOpeningBeat(string shot, string speaker, string text, float duration, float minDuration)
        {
            Shot = shot;
            Speaker = speaker ?? string.Empty;
            Text = text ?? string.Empty;
            Duration = duration;
            MinDuration = minDuration;
        }
    }

    public readonly struct DreamForestDialogueLine
    {
        public readonly string Speaker;
        public readonly string Text;
        public readonly float Duration;

        public DreamForestDialogueLine(string speaker, string text, float duration)
        {
            Speaker = speaker ?? string.Empty;
            Text = text ?? string.Empty;
            Duration = duration;
        }
    }

    public readonly struct DreamForestSectionSpec
    {
        public readonly int Index;
        public readonly string Name;
        public readonly string Kind;
        public readonly float StartX;

        public DreamForestSectionSpec(int index, string name, string kind, float startX)
        {
            Index = index;
            Name = name;
            Kind = kind;
            StartX = startX;
        }
    }

    public sealed class DreamForestFlightSpec
    {
        public float ToX;
        public float ToY;
        public float Duration;
        public float Delay;
        public float Arc;
        public float Wave;
        public string NextId;
    }

    public sealed class DreamForestObjectSpec
    {
        public string Id;
        public DreamForestObjectKind Kind;
        public float X;
        public float Y;
        public float W;
        public float H;
        public bool Checkpoint;
        public bool Finish;
        public int Section;
        public float Amplitude;
        public float Period;
        public float Phase;
        public float SpiderW;
        public float SpiderH;
        public DreamForestFlightSpec Flight;
    }

    public readonly struct DreamForestObjectFrame
    {
        public readonly DreamForestObjectSpec Spec;
        public readonly float X;
        public readonly float Y;
        public readonly float W;
        public readonly float H;
        public readonly float Sway;
        public readonly float SpiderY;
        public readonly float FlightProgress;
        public readonly float FlightEnergy;

        public DreamForestObjectFrame(
            DreamForestObjectSpec spec,
            float x,
            float y,
            float sway,
            float spiderY,
            float flightProgress,
            float flightEnergy)
        {
            Spec = spec;
            X = x;
            Y = y;
            W = spec.W;
            H = spec.H;
            Sway = sway;
            SpiderY = spiderY;
            FlightProgress = flightProgress;
            FlightEnergy = flightEnergy;
        }
    }

    public readonly struct DreamForestGliderThornSpec
    {
        public readonly float X;
        public readonly float Y;
        public readonly float W;
        public readonly float H;
        public readonly int Side;

        public DreamForestGliderThornSpec(float x, float y, float w, float h, int side)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            Side = side;
        }
    }

    public readonly struct DreamForestPoint2D
    {
        public readonly float X;
        public readonly float Y;

        public DreamForestPoint2D(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public sealed class DreamForestMudLakeSpec
    {
        public string Id;
        public int Index;
        public float PaintX;
        public float W;
        public float H;
        public float Top;
        public float From;
        public float To;
        public DreamForestPoint2D[] Path;
    }

    public readonly struct DreamForestStealthPlatformSpec
    {
        public readonly float From;
        public readonly float To;
        public readonly float Y;

        public DreamForestStealthPlatformSpec(float from, float to, float y)
        {
            From = from;
            To = to;
            Y = y;
        }
    }

    public sealed class DreamForestStealthCoverSpec
    {
        public string Id;
        public int Index;
        public bool IsRock;
        public int Frame;
        public float X;
        public float W;
        public float H;
        public float BaseY;
        public DreamForestPoint2D[] Polygon;
    }

    public readonly struct DreamForestSpotlight
    {
        public readonly float X;
        public readonly float Y;
        public readonly float AngleRad;
        public readonly float HalfAngleRad;

        public DreamForestSpotlight(float x, float y, float angleRad, float halfAngleRad)
        {
            X = x;
            Y = y;
            AngleRad = angleRad;
            HalfAngleRad = halfAngleRad;
        }
    }

    public sealed class DreamForestClimbState
    {
        public string ObjectId;
        public float U;
        public int Side;
    }

    public sealed class DreamForestPlayerState
    {
        public float X = 130f;
        public float Y = 650f;
        public float Vx;
        public float Vy;
        public int Facing = 1;
        public string SupportId = "rest-0";
        public DreamForestClimbState Climb;
        public string LockId;
        public float LockTime;
        public bool JumpHeld;
        public float AnimTime;
    }

    public sealed class DreamForestOpeningState
    {
        public int Index;
        public float Age;
        public float Time;
        public bool Dismissed;
    }

    public sealed class DreamForestGliderState
    {
        public float CameraY = 450f;
        public float LandingX;
        public float LandingY;
    }

    public sealed class DreamForestNightmareState
    {
        public float Time;
        public float X;
        public float Y;
        public float Vx;
        public float Vy;
        public int Facing = 1;
        public bool Grounded = true;
        public bool JumpHeld = true;
        public float AnimTime;
        public float Exposure;
        public int Checkpoint;
        public bool RoachAppeared;
        public int IntroIndex;
        public float IntroAge;
        public float ArrivalAge;
        public int WakeIndex = -1;
        public float WakeAge;
        public bool FairyAwake;
        public bool ScreamPlayed;
        public bool StrikeSound;
        public bool Completed;

        // Mud traversal state
        public DreamForestMudMode MudMode = DreamForestMudMode.Dry;
        public string MudLakeId;
        public float MudAge;
        public int MudSide;
        public float MudStartX;
        public float MudStartY = 850f;
        public float MudEntryTarget;
        public float MudStartDepth;
        public float MudDepth;
        public float SwimDistance;
    }
}
