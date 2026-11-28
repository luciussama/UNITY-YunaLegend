using System;
using System.Collections.Generic;

namespace YunaLegend.DreamForest.Core
{
    /// <summary>
    /// Deterministic pure C# simulation for Dream Forest (Act 1: Opening, Grass/Beetle/Bee/Web
    /// traversal, Leaf Glider through thorns; Act 2: Nightmare Stealth, Raycast Roach Spotlights,
    /// Mud Lakes swimming/submerging, Climactic Capture &amp; Wake).
    /// </summary>
    public sealed class DreamForestSimulation
    {
        public DreamForestLevelData Level { get; }
        public DreamForestDifficulty Difficulty { get; }
        public DreamForestStage Stage { get; private set; }
        public float Time { get; private set; }
        public float Age { get; private set; }
        public float Progress { get; private set; }
        public int Falls { get; private set; }
        public int Hits { get; private set; }
        public string FallCause { get; private set; }
        public string CheckpointId { get; private set; }
        public int StoryIndex { get; private set; }
        public float StoryAge { get; private set; }
        public bool TutorialActive { get; private set; }

        public DreamForestPlayerState Player { get; } = new DreamForestPlayerState();
        public DreamForestOpeningState Opening { get; private set; } = new DreamForestOpeningState();
        public DreamForestGliderState Glider { get; private set; }
        public DreamForestNightmareState Nightmare { get; private set; }
        public Dictionary<string, float?> Rides { get; } = new Dictionary<string, float?>();

        private readonly List<DreamForestObjectSpec> nearbyBuffer = new List<DreamForestObjectSpec>(32);
        private readonly HashSet<string> nearbySeen = new HashSet<string>();
        private readonly List<DreamForestEvent> frameEvents = new List<DreamForestEvent>(8);

        public DreamForestSimulation(bool easyMode = false, DreamForestLevelData level = null)
        {
            Level = level ?? DreamForestLevelData.Default;
            Difficulty = easyMode ? DreamForestDifficulty.Easy : DreamForestDifficulty.Normal;
            ResetToBeginning();
        }

        public void ResetToBeginning()
        {
            Stage = DreamForestStage.Opening;
            Time = 0f;
            Age = 0f;
            Progress = 0f;
            Falls = 0;
            Hits = 0;
            FallCause = null;
            CheckpointId = "rest-0";
            StoryIndex = 0;
            StoryAge = 0f;
            TutorialActive = false;
            Opening = new DreamForestOpeningState();
            Glider = null;
            Nightmare = null;

            Rides.Clear();
            foreach (var o in Level.Objects)
            {
                if (o.Flight != null) Rides[o.Id] = null;
            }

            Player.X = 130f;
            Player.Y = 650f;
            Player.Vx = 0f;
            Player.Vy = 0f;
            Player.Facing = 1;
            Player.SupportId = "rest-0";
            Player.Climb = null;
            Player.LockId = null;
            Player.LockTime = 0f;
            Player.JumpHeld = false;
            Player.AnimTime = 0f;
        }

        public DreamForestMusicScene ActiveMusicScene()
        {
            if (Stage == DreamForestStage.Opening)
            {
                bool transition = Opening != null
                    && DreamForestLevelData.OpeningBeats[Opening.Index].Shot == "leave"
                    && Opening.Age >= 2.8f;
                return transition ? DreamForestMusicScene.Dream : DreamForestMusicScene.Silence;
            }
            if (Nightmare == null) return DreamForestMusicScene.Dream;
            if (Stage == DreamForestStage.NightmareWake || Stage == DreamForestStage.Complete)
            {
                return DreamForestMusicScene.PicnicWake;
            }
            return DreamForestMusicScene.Nightmare;
        }

        public string ActiveSectionName()
        {
            if (Nightmare != null) return "O Pesadelo no Bosque";
            float px = Player.X;
            string current = Level.Sections[0].Name;
            for (int i = 0; i < Level.Sections.Length; i++)
            {
                if (px >= Level.Sections[i].StartX) current = Level.Sections[i].Name;
            }
            return current;
        }

        public bool TryGetActiveDialogue(out string speaker, out string text)
        {
            if (Stage == DreamForestStage.Opening && Opening != null)
            {
                var beat = DreamForestLevelData.OpeningBeats[Opening.Index];
                if (!string.IsNullOrEmpty(beat.Text) && !Opening.Dismissed)
                {
                    speaker = beat.Speaker;
                    text = beat.Text;
                    return true;
                }
            }
            else if (Stage == DreamForestStage.Running && TutorialActive && StoryIndex < DreamForestLevelData.IntroLines.Length)
            {
                speaker = "Jardim dos Sonhos";
                text = DreamForestLevelData.IntroLines[StoryIndex];
                return true;
            }
            else if (Stage == DreamForestStage.NightmareIntro && Nightmare != null)
            {
                var line = DreamForestLevelData.NightmareIntroLines[Math.Min(Nightmare.IntroIndex, DreamForestLevelData.NightmareIntroLines.Length - 1)];
                speaker = line.Speaker;
                text = line.Text;
                return true;
            }
            else if (Stage == DreamForestStage.NightmareWake && Nightmare != null && Nightmare.WakeIndex >= 0)
            {
                var line = DreamForestLevelData.NightmareWakeLines[Math.Min(Nightmare.WakeIndex, DreamForestLevelData.NightmareWakeLines.Length - 1)];
                speaker = line.Speaker;
                text = line.Text;
                return true;
            }

            speaker = string.Empty;
            text = string.Empty;
            return false;
        }

        public bool AdvanceDialogue()
        {
            if (Stage == DreamForestStage.Opening && Opening != null)
            {
                var beat = DreamForestLevelData.OpeningBeats[Opening.Index];
                if (string.IsNullOrEmpty(beat.Text) || Opening.Dismissed) return false;
                Opening.Dismissed = true;
                return true;
            }

            if (Stage == DreamForestStage.Running && TutorialActive)
            {
                if (StoryIndex < 2)
                {
                    StoryIndex++;
                    StoryAge = 0f;
                }
                else
                {
                    TutorialActive = false;
                    StoryAge = 7f;
                }
                return true;
            }

            if (Nightmare != null)
            {
                if (Stage == DreamForestStage.NightmareIntro)
                {
                    Nightmare.IntroIndex++;
                    Nightmare.IntroAge = 0f;
                    if (Nightmare.IntroIndex >= DreamForestLevelData.NightmareIntroLines.Length)
                    {
                        Nightmare.IntroIndex = DreamForestLevelData.NightmareIntroLines.Length - 1;
                        Nightmare.RoachAppeared = true;
                        Nightmare.Vx = 0f;
                        Stage = DreamForestStage.NightmareReveal;
                        Age = 0f;
                    }
                    return true;
                }

                if (Stage == DreamForestStage.NightmareWake && Nightmare.WakeIndex >= 0)
                {
                    Nightmare.WakeAge = 0f;
                    if (Nightmare.WakeIndex < DreamForestLevelData.NightmareWakeLines.Length - 1)
                    {
                        Nightmare.WakeIndex++;
                    }
                    else
                    {
                        Nightmare.Completed = true;
                        Stage = DreamForestStage.Complete;
                        Age = 0f;
                        Progress = 1f;
                    }
                    return true;
                }
            }

            return false;
        }

        public bool Retry()
        {
            if (Nightmare != null)
            {
                if (Stage != DreamForestStage.GameOver) return false;
                BeginNightmare(retry: true, checkpoint: Nightmare.Checkpoint);
                return true;
            }

            if (Stage != DreamForestStage.GameOver && Stage != DreamForestStage.Rescuing) return false;
            var cp = Level.ById[CheckpointId];
            foreach (var o in Level.Objects)
            {
                if (o.Flight != null && o.X >= cp.X) Rides[o.Id] = null;
            }
            PlaceAtCheckpoint();
            Stage = DreamForestStage.Running;
            Age = 0f;
            FallCause = null;
            return true;
        }

        public void BeginGliderSectionForTesting()
        {
            CheckpointId = "rest-5";
            PlaceAtCheckpoint();
            BeginGlide();
        }

        public void BeginNightmareSectionForTesting(bool skipIntro = false)
        {
            BeginNightmare(retry: skipIntro, checkpoint: 0);
        }

        public void BeginSunlitWakeForTesting()
        {
            if (Nightmare == null) BeginNightmare(retry: true, checkpoint: 0);
            Stage = DreamForestStage.NightmareWake;
            Age = 2.2f;
            Nightmare.FairyAwake = true;
            Nightmare.WakeIndex = 1;
            Nightmare.WakeAge = 0.5f;
        }

        public IReadOnlyList<DreamForestEvent> Step(DreamForestInput input, float seconds)
        {
            frameEvents.Clear();
            float remaining = Math.Max(0f, Math.Min(0.2f, seconds));

            if (Nightmare != null)
            {
                StepNightmare(input, remaining);
                return frameEvents;
            }

            if (Stage == DreamForestStage.GameOver) return frameEvents;

            if (Stage == DreamForestStage.Opening)
            {
                StepOpening(input, remaining);
                return frameEvents;
            }

            if (Stage == DreamForestStage.Running && TutorialActive)
            {
                StoryAge += remaining;
                if (StoryAge >= 7f) AdvanceDialogue();
            }

            while (remaining > 1e-7f)
            {
                if (Stage == DreamForestStage.GameOver || Stage == DreamForestStage.Complete) break;
                float dt = Math.Min(1f / 120f, remaining);
                remaining -= dt;
                float prevTime = Time;
                Time += dt;
                Age += dt;

                if (Stage == DreamForestStage.GlideApproach || Stage == DreamForestStage.GlideGust
                    || Stage == DreamForestStage.Gliding || Stage == DreamForestStage.GlideLanding
                    || Stage == DreamForestStage.GlideFalling)
                {
                    StepGlider(input, dt);
                    if (Stage == DreamForestStage.Complete)
                    {
                        BeginNightmare(retry: false, checkpoint: 0);
                        frameEvents.Add(new DreamForestEvent(DreamForestEventKind.NightmareStart));
                        return frameEvents;
                    }
                    Progress = Math.Max(Progress, DreamForestLevelData.Clamp01(Player.X / (Level.WorldWidth - 300f)) * 0.65f);
                    continue;
                }

                if (Stage == DreamForestStage.Running && Player.SupportId == "rest-5")
                {
                    BeginGlide();
                    continue;
                }

                if (Stage == DreamForestStage.Falling)
                {
                    if (FallCause == "spider")
                    {
                        Player.X = Math.Max(20f, Math.Min(Level.WorldWidth - 20f, Player.X + Player.Vx * dt));
                    }
                    Player.Vy = Math.Min(2000f, Player.Vy + DreamForestLevelData.Gravity * dt);
                    Player.Y = Math.Min(DreamForestLevelData.Height + 400f, Player.Y + Player.Vy * dt);
                    if (Age >= 1.6f)
                    {
                        Stage = (Difficulty == DreamForestDifficulty.Easy && FallCause == "spider")
                            ? DreamForestStage.Rescuing
                            : DreamForestStage.GameOver;
                        Age = 0f;
                    }
                    continue;
                }

                if (Stage == DreamForestStage.Rescuing)
                {
                    if (Age >= 1.1f)
                    {
                        Retry();
                        frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Checkpoint));
                    }
                    continue;
                }

                Level.QueryNearby(Player.X - 500f, Player.X + 600f, nearbyBuffer, nearbySeen);
                float ix = Math.Max(-1f, Math.Min(1f, input.X));
                float iy = Math.Max(-1f, Math.Min(1f, input.Y));
                bool jump = input.Jump && !Player.JumpHeld;
                Player.JumpHeld = input.Jump;
                Player.LockTime = Math.Max(0f, Player.LockTime - dt);
                if (Math.Abs(ix) > 0.01f) Player.Facing = ix < 0f ? -1 : 1;

                if (CheckSpiderHit()) continue;

                if (Player.SupportId != null)
                {
                    BoardBeetle(Player.SupportId);
                    var o = Level.ById[Player.SupportId];
                    var a = DreamForestLevelData.EvaluateObjectFrame(o, prevTime, Rides);
                    var b = DreamForestLevelData.EvaluateObjectFrame(o, Time, Rides);
                    Player.X += b.X - a.X;
                    Player.Y += b.Y - a.Y;
                }

                if (Player.Climb != null)
                {
                    var o = Level.ById[Player.Climb.ObjectId];
                    var f = DreamForestLevelData.EvaluateObjectFrame(o, Time, Rides);
                    var old = DreamForestLevelData.EvaluateObjectFrame(o, prevTime, Rides);
                    bool isStem = o.Kind == DreamForestObjectKind.Grass || o.Kind == DreamForestObjectKind.Web;
                    float limit = isStem
                        ? Math.Max(Player.Climb.U, (DreamForestLevelData.Height - 64f - f.Y) / f.H)
                        : 0.96f;
                    float minU = o.Kind == DreamForestObjectKind.Web
                        ? (100f - f.Y) / f.H
                        : (o.Kind == DreamForestObjectKind.Grass ? 0.04f : 0f);
                    Player.Climb.U = Math.Max(minU, Math.Min(limit, Player.Climb.U + (iy * DreamForestLevelData.ClimbSpeed * dt) / f.H));

                    if (isStem)
                    {
                        float grip = f.Y + Player.Climb.U * f.H;
                        int side = Player.Climb.Side != 0 ? Player.Climb.Side : -Player.Facing;
                        Player.Facing = -side;
                        Player.X = DreamForestLevelData.GrassX(f, grip) + side * 19f;
                        Player.Y = grip + 64f;
                        Player.Vx = (DreamForestLevelData.GrassX(f, grip) - DreamForestLevelData.GrassX(old, grip)) / dt;
                    }
                    else
                    {
                        Player.X = f.X + (Player.Climb.Side < 0 ? 0.16f : 0.84f) * f.W;
                        Player.Y = f.Y + Player.Climb.U * f.H;
                        Player.Vx = (f.X - old.X) / dt;
                        float top = DreamForestLevelData.SurfaceY(f, Player.X);
                        if (Player.Y <= top + 2f)
                        {
                            Player.Y = top;
                            Player.SupportId = o.Id;
                            Player.Climb = null;
                            BoardBeetle(o.Id);
                        }
                    }

                    Player.Vy = 0f;
                    Player.AnimTime += Math.Abs(iy) * dt * 8f;
                    if (CheckSpiderHit()) continue;

                    if (jump)
                    {
                        if (Math.Abs(ix) > 0.01f) Player.Facing = ix < 0f ? -1 : 1;
                        Player.LockId = o.Id;
                        Player.LockTime = 0.38f;
                        Player.Climb = null;
                        Player.SupportId = null;
                        Player.Vx = Player.Facing * 280f + Math.Max(-60f, Math.Min(60f, Player.Vx));
                        Player.Vy = -(Difficulty == DreamForestDifficulty.Easy ? DreamForestLevelData.JumpSpeed + 25f : DreamForestLevelData.JumpSpeed);
                        frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Jump));
                    }
                    else
                    {
                        continue;
                    }
                }

                if (Player.SupportId != null && jump)
                {
                    Player.Vx = ix * DreamForestLevelData.Speed;
                    Player.SupportId = null;
                    Player.Vy = -(Difficulty == DreamForestDifficulty.Easy ? DreamForestLevelData.JumpSpeed + 25f : DreamForestLevelData.JumpSpeed);
                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Jump));
                }

                float oldX = Player.X;
                float oldY = Player.Y;
                Player.Vx = Player.SupportId != null
                    ? ix * DreamForestLevelData.Speed
                    : Player.Vx + Math.Max(-900f * dt, Math.Min(900f * dt, ix * DreamForestLevelData.Speed - Player.Vx));
                Player.X = Math.Max(20f, Math.Min(Level.WorldWidth - 20f, Player.X + Player.Vx * dt));
                Player.AnimTime += Math.Abs(Player.Vx) * dt / 32f;

                if (Player.SupportId != null)
                {
                    var f = DreamForestLevelData.EvaluateObjectFrame(Level.ById[Player.SupportId], Time, Rides);
                    if (Player.X >= f.X + f.W * 0.12f && Player.X <= f.X + f.W * 0.88f)
                    {
                        Player.Y = DreamForestLevelData.SurfaceY(f, Player.X);
                        Player.Vy = 0f;
                    }
                    else
                    {
                        Player.SupportId = null;
                    }
                }

                if (Player.SupportId == null)
                {
                    Player.Vy += DreamForestLevelData.Gravity * dt;
                    Player.Y += Player.Vy * dt;

                    for (int i = 0; i < nearbyBuffer.Count; i++)
                    {
                        var o = nearbyBuffer[i];
                        var f = DreamForestLevelData.EvaluateObjectFrame(o, Time, Rides);
                        bool isStem = o.Kind == DreamForestObjectKind.Grass || o.Kind == DreamForestObjectKind.Web;
                        if (isStem)
                        {
                            float grip = Player.Y - 64f;
                            if (Player.LockTime > 0f && Player.LockId == o.Id) continue;
                            float stemX = DreamForestLevelData.GrassX(f, grip);
                            var before = DreamForestLevelData.EvaluateObjectFrame(o, prevTime, Rides);
                            float from = oldX - DreamForestLevelData.GrassX(before, oldY - 64f);
                            float to = Player.X - stemX;
                            bool validY = o.Kind == DreamForestObjectKind.Web
                                ? DreamForestLevelData.WebCanGrab(f, grip)
                                : (grip >= f.Y + 10f && grip <= DreamForestLevelData.Height + 36f);
                            if (validY && (Math.Abs(to) <= DreamForestLevelData.BodyRadius + 28f || from * to < 0f))
                            {
                                Player.Facing = to > 0f ? -1 : 1;
                                Player.X = stemX - Player.Facing * 19f;
                                Player.Climb = new DreamForestClimbState
                                {
                                    ObjectId = o.Id,
                                    U = (grip - f.Y) / f.H,
                                    Side = -Player.Facing,
                                };
                                Player.SupportId = null;
                                Player.Vy = 0f;
                                frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Grip, o.Id));
                                break;
                            }
                        }
                        else
                        {
                            float top = DreamForestLevelData.SurfaceY(f, Player.X);
                            var before = DreamForestLevelData.EvaluateObjectFrame(o, prevTime, Rides);
                            float oldTop = DreamForestLevelData.SurfaceY(before, oldX);
                            if (Player.Vy >= 0f && Player.X >= f.X + f.W * 0.12f && Player.X <= f.X + f.W * 0.88f
                                && oldY <= oldTop + 3f && Player.Y >= top)
                            {
                                Player.Y = top;
                                Player.Vy = 0f;
                                Player.SupportId = o.Id;
                                BoardBeetle(o.Id);
                                if (o.Checkpoint && CheckpointId != o.Id)
                                {
                                    CheckpointId = o.Id;
                                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Checkpoint, o.Id));
                                }
                                if (o.Finish)
                                {
                                    Stage = DreamForestStage.Complete;
                                    Age = 0f;
                                    Player.Vx = 0f;
                                    Player.Vy = 0f;
                                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Complete));
                                }
                                break;
                            }
                        }
                    }
                }

                if (CheckSpiderHit()) continue;

                Progress = Math.Max(Progress, DreamForestLevelData.Clamp01(Player.X / (Level.WorldWidth - 300f)) * 0.65f);
                if (Player.Y > DreamForestLevelData.Height + 100f)
                {
                    FallCause = null;
                    Falls++;
                    Stage = Difficulty == DreamForestDifficulty.Easy ? DreamForestStage.Rescuing : DreamForestStage.Falling;
                    Age = 0f;
                    Player.Climb = null;
                    Player.SupportId = null;
                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Fall));
                }
            }

            return frameEvents;
        }

        private void StepOpening(DreamForestInput input, float seconds)
        {
            var beat = DreamForestLevelData.OpeningBeats[Opening.Index];
            Opening.Age += seconds;
            Opening.Time += seconds;
            if (input.Advance) AdvanceDialogue();
            if (Opening.Age < beat.Duration && !(Opening.Dismissed && Opening.Age >= beat.MinDuration)) return;

            int next = Opening.Index + 1;
            if (next < DreamForestLevelData.OpeningBeats.Length)
            {
                Opening.Index = next;
                Opening.Age = 0f;
                Opening.Dismissed = false;
                return;
            }

            Stage = DreamForestStage.Running;
            Age = 0f;
            Opening = null;
            StoryIndex = 1;
            StoryAge = 0f;
            TutorialActive = true;
            Player.JumpHeld = true;
        }

        private void BeginGlide()
        {
            Opening = null;
            TutorialActive = false;
            Stage = DreamForestStage.GlideApproach;
            Age = 0f;
            Glider = new DreamForestGliderState { CameraY = 450f };
            CheckpointId = "rest-5";
            FallCause = null;
            Player.Climb = null;
        }

        private void StepGlider(DreamForestInput input, float dt)
        {
            var branch = DreamForestLevelData.EvaluateObjectFrame(Level.ById["rest-5"], Time, Rides);
            var finish = DreamForestLevelData.EvaluateObjectFrame(Level.ById["dream-finish"], Time, Rides);
            float tip = branch.X + branch.W * 0.84f;

            if (Stage == DreamForestStage.GlideApproach)
            {
                Player.X = Math.Min(tip, Player.X + 115f * dt);
                Player.Y = DreamForestLevelData.SurfaceY(branch, Player.X);
                Player.Vx = 115f;
                Player.Vy = 0f;
                Player.Facing = 1;
                Player.AnimTime += 115f * dt / 32f;
                if (Player.X >= tip)
                {
                    Stage = DreamForestStage.GlideGust;
                    Age = 0f;
                    Player.Vx = 0f;
                }
                return;
            }

            if (Stage == DreamForestStage.GlideGust)
            {
                Player.X = tip;
                Player.Y = DreamForestLevelData.SurfaceY(branch, Player.X);
                if (Age >= 1.1f)
                {
                    Stage = DreamForestStage.Gliding;
                    Age = 0f;
                    Player.SupportId = null;
                    Player.Vx = DreamForestLevelData.GlideSpeed;
                    Player.Vy = -110f;
                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Jump));
                }
                return;
            }

            if (Stage == DreamForestStage.GlideLanding)
            {
                float t = Math.Min(1f, Age / 2.4f);
                float tx = finish.X + finish.W * 0.45f;
                float ty = DreamForestLevelData.SurfaceY(finish, tx);
                Player.X = Glider.LandingX + (tx - Glider.LandingX) * (1f - (1f - t) * (1f - t) * (1f - t));
                Player.Y = Glider.LandingY + (ty - Glider.LandingY) * (float)Math.Pow(t, 1.4);
                Glider.CameraY += (Player.Y - 100f - Glider.CameraY) * Math.Min(1f, dt * 3f);
                if (t >= 1f) Player.SupportId = finish.Spec.Id;
                if (Age >= 3.6f)
                {
                    Stage = DreamForestStage.Complete;
                    Age = 0f;
                }
                return;
            }

            if (Stage == DreamForestStage.GlideFalling)
            {
                Player.Vy += 700f * dt;
                Player.Y += Player.Vy * dt;
                Player.X += 70f * dt;
                if (Age >= 1.35f)
                {
                    Player.X = branch.X + branch.W * 0.45f;
                    Player.Y = DreamForestLevelData.SurfaceY(branch, Player.X);
                    Player.SupportId = branch.Spec.Id;
                    Player.Vx = 0f;
                    Player.Vy = 0f;
                    BeginGlide();
                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Checkpoint));
                }
                return;
            }

            // Gliding flight
            float iy = Math.Max(-1f, Math.Min(1f, input.Y));
            float targetVy = iy * DreamForestLevelData.GlideMaxVertical + (Math.Abs(iy) < 0.05f ? 35f : 0f);
            float change = Math.Max(-DreamForestLevelData.GlideAcceleration * dt,
                Math.Min(DreamForestLevelData.GlideAcceleration * dt, (targetVy - Player.Vy) * 1.7f));
            Player.Vy += change;
            Player.Vx = DreamForestLevelData.GlideSpeed;
            Player.X += Player.Vx * dt;
            Player.Y += Player.Vy * dt;
            Glider.CameraY += (Player.Y - 100f - Glider.CameraY) * Math.Min(1f, dt * 3f);

            if (CheckGliderThornContact() || Player.Y < -1700f || Player.Y > 1900f)
            {
                Stage = DreamForestStage.GlideFalling;
                Age = 0f;
                Falls++;
                Hits++;
                FallCause = "thorn";
                Player.Vy = 80f;
                frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Hurt));
                frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Fall));
                return;
            }

            if (Player.X >= DreamForestLevelData.GlideStart + DreamForestLevelData.GlideLength)
            {
                Stage = DreamForestStage.GlideLanding;
                Age = 0f;
                Glider.LandingX = Player.X;
                Glider.LandingY = Player.Y;
            }
        }

        private bool CheckGliderThornContact()
        {
            const float halfW = 34f;
            const float halfH = 38f;
            for (int i = 0; i < Level.GlideThorns.Length; i++)
            {
                var th = Level.GlideThorns[i];
                if (Player.X + halfW < th.X + 42f || Player.X - halfW > th.X + th.W - 42f) continue;
                if (Player.Y + halfH > th.Y + 35f && Player.Y - halfH < th.Y + th.H - 35f)
                {
                    return true;
                }
            }
            return false;
        }

        private void BeginNightmare(bool retry, int checkpoint)
        {
            Opening = null;
            TutorialActive = false;
            float spawnX = retry
                ? (checkpoint == 0 ? 220f : checkpoint == 1 ? 2380f : (checkpoint == 2 ? Level.MudLakes[0].From + 100f : Level.MudLakes[2].From + 100f))
                : DreamForestLevelData.StealthEntryX - 90f;

            Nightmare = new DreamForestNightmareState
            {
                X = spawnX,
                Y = Level.StealthGroundYAt(spawnX),
                Checkpoint = retry ? checkpoint : 0,
                RoachAppeared = retry,
            };

            if (retry && checkpoint >= 2)
            {
                var lake = Level.MudLakes[checkpoint >= 3 ? 2 : 0];
                EnterMud(lake, 1, submerged: true);
            }

            Stage = retry ? DreamForestStage.Stealth : DreamForestStage.NightmareFall;
            Age = 0f;
            FallCause = null;
        }

        public void EvaluateRoachPatrol(out float roachX, out float roachY, out float roachScale, out float aimX)
        {
            var n = Nightmare;
            bool isFinal = Stage == DreamForestStage.NightmareFinal;
            bool isStrike = Stage == DreamForestStage.NightmareStrike;
            bool isReveal = Stage == DreamForestStage.NightmareReveal;
            float approach = DreamForestLevelData.SmoothStep(Age / DreamForestLevelData.NightmareRevealDuration);

            roachX = (isFinal || isStrike)
                ? n.X - 160f * (1f - DreamForestLevelData.SmoothStep(Age / 4f))
                : n.X + 180f * (float)Math.Sin(n.Time * 0.4f - 1.2f);
            roachScale = isReveal
                ? 0.32f + 0.68f * approach
                : (isFinal ? 1f + DreamForestLevelData.SmoothStep((Age - 2.6f) / 3f) * 0.35f
                    : (isStrike ? 1f + DreamForestLevelData.SmoothStep(Age / 1.3f) * 0.25f : 1f));
            roachY = isReveal ? 50f + 190f * approach : 240f;
            aimX = (isFinal || isStrike) ? n.X : roachX + 720f * (float)Math.Sin(n.Time * 0.6f - 1f);
        }

        public void EvaluateSpotlights(List<DreamForestSpotlight> output)
        {
            output.Clear();
            if (Nightmare == null || !Nightmare.RoachAppeared
                || Stage == DreamForestStage.NightmareFall || Stage == DreamForestStage.NightmareIntro)
            {
                return;
            }
            EvaluateRoachPatrol(out float rx, out float ry, out float rScale, out float aimX);
            float half = Difficulty == DreamForestDifficulty.Easy ? 0.17f : 0.21f;
            float eyeOffsetX = 92f * rScale;
            float eyeY = ry + 36f * rScale;

            for (int side = -1; side <= 1; side += 2)
            {
                float ex = rx + side * eyeOffsetX;
                float angle = (float)Math.Atan2(DreamForestLevelData.StealthGroundY - eyeY, aimX - ex);
                output.Add(new DreamForestSpotlight(ex, eyeY, angle, half));
            }
        }

        public float RaycastCoverDistance(DreamForestSpotlight light, float angleRad, float limit = DreamForestLevelData.NightmareLightRange)
        {
            float dx = (float)Math.Cos(angleRad);
            float dy = (float)Math.Sin(angleRad);
            float dist = limit;

            float minX = Math.Min(light.X, light.X + dx * limit);
            float maxX = Math.Max(light.X, light.X + dx * limit);

            for (int c = 0; c < Level.StealthCovers.Length; c++)
            {
                var cover = Level.StealthCovers[c];
                if (cover.X + cover.W * 0.5f < minX || cover.X - cover.W * 0.5f > maxX) continue;
                var poly = cover.Polygon;
                for (int i = 0; i < poly.Length; i++)
                {
                    var a = poly[i];
                    var b = poly[(i + 1) % poly.Length];
                    float sx = b.X - a.X;
                    float sy = b.Y - a.Y;
                    float den = dx * sy - dy * sx;
                    if (Math.Abs(den) < 1e-8f) continue;
                    float ax = a.X - light.X;
                    float ay = a.Y - light.Y;
                    float t = (ax * sy - ay * sx) / den;
                    float u = (ax * dy - ay * dx) / den;
                    if (t >= 0f && t < dist && u >= 0f && u <= 1f) dist = t;
                }
            }
            return dist;
        }

        public bool IsNightmarePlayerIlluminated(bool detection = true)
        {
            if (Nightmare == null || !Nightmare.RoachAppeared) return false;
            var lights = new List<DreamForestSpotlight>(2);
            EvaluateSpotlights(lights);

            // Sample head, torso, and lower body points; mud occlusion hides submerged points
            float[] sampleOffsetsY = { -130f, -75f, -20f };
            var lake = Level.MudLakeAt(Nightmare.X);
            float waterY = lake != null ? DreamForestLevelData.MudWaterline(lake, Nightmare.X) : float.MaxValue;

            for (int li = 0; li < lights.Count; li++)
            {
                var light = lights[li];
                float half = (detection && Difficulty == DreamForestDifficulty.Easy) ? 0.045f : light.HalfAngleRad;
                for (int si = 0; si < sampleOffsetsY.Length; si++)
                {
                    float py = Nightmare.Y + sampleOffsetsY[si];
                    if (lake != null && py >= waterY) continue; // Occluded beneath mud surface!

                    float dx = Nightmare.X - light.X;
                    float dy = py - light.Y;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist >= DreamForestLevelData.NightmareLightRange) continue;

                    float ang = (float)Math.Atan2(dy, dx);
                    float diff = (float)Math.Atan2(Math.Sin(ang - light.AngleRad), Math.Cos(ang - light.AngleRad));
                    if (Math.Abs(diff) > half) continue;

                    float blockedDist = RaycastCoverDistance(light, ang, dist + 1f);
                    if (blockedDist >= dist - 0.01f) return true;
                }
            }
            return false;
        }

        private void StepNightmare(DreamForestInput input, float remaining)
        {
            var n = Nightmare;
            if (Stage == DreamForestStage.GameOver) return;
            if (input.Advance) AdvanceDialogue();

            while (remaining > 1e-7f)
            {
                if (Stage == DreamForestStage.GameOver) break;
                float dt = Math.Min(1f / 120f, remaining);
                float beforeAge = Age;
                float beforeTime = n.Time;
                remaining -= dt;
                Time += dt;
                Age += dt;
                if (Stage != DreamForestStage.NightmareIntro) n.Time += dt;

                if (n.RoachAppeared && Stage != DreamForestStage.NightmareWake && Stage != DreamForestStage.Complete
                    && !(Stage == DreamForestStage.NightmareFinal && Age < 2.7f)
                    && Math.Floor(n.Time / (DreamForestLevelData.RoachStepSeconds * 0.5f)) > Math.Floor(beforeTime / (DreamForestLevelData.RoachStepSeconds * 0.5f)))
                {
                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.RoachStep));
                }

                if (Stage == DreamForestStage.NightmareFall)
                {
                    if (Age >= 1.8f)
                    {
                        Stage = DreamForestStage.NightmareIntro;
                        Age = 0f;
                    }
                    continue;
                }

                if (Stage == DreamForestStage.NightmareIntro)
                {
                    n.IntroAge += dt;
                    n.ArrivalAge += dt;
                    float oldX = n.X;
                    n.X = DreamForestLevelData.StealthEntryX - 90f + 90f * DreamForestLevelData.SmoothStep(n.ArrivalAge / 3f);
                    n.Vx = (n.X - oldX) / dt;
                    n.AnimTime += Math.Abs(n.X - oldX) / 30f;
                    if (n.IntroAge >= DreamForestLevelData.NightmareIntroLines[n.IntroIndex].Duration) AdvanceDialogue();
                    continue;
                }

                if (Stage == DreamForestStage.NightmareReveal)
                {
                    n.ArrivalAge += dt;
                    float oldX = n.X;
                    n.X = DreamForestLevelData.StealthEntryX - 90f + 90f * DreamForestLevelData.SmoothStep(n.ArrivalAge / 3f);
                    n.Vx = (n.X - oldX) / dt;
                    n.Facing = n.ArrivalAge < 3f ? 1 : -1;
                    if (Age >= DreamForestLevelData.NightmareRevealDuration)
                    {
                        Stage = DreamForestStage.Stealth;
                        Age = 0f;
                        n.Facing = 1;
                        n.JumpHeld = input.Jump;
                    }
                    continue;
                }

                if (Stage == DreamForestStage.NightmareStrike)
                {
                    if (beforeAge < 0.72f && Age >= 0.72f && !n.StrikeSound)
                    {
                        n.StrikeSound = true;
                        frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Hurt));
                        frameEvents.Add(new DreamForestEvent(DreamForestEventKind.RoachStomp));
                    }
                    if (Age >= 1.45f)
                    {
                        Stage = DreamForestStage.GameOver;
                        Age = 0f;
                    }
                    continue;
                }

                if (Stage == DreamForestStage.NightmareFinal)
                {
                    if (beforeAge < 5.65f && Age >= 5.65f)
                    {
                        frameEvents.Add(new DreamForestEvent(DreamForestEventKind.RoachStomp));
                    }
                    float prevX = n.X;
                    n.X += 350f * (DreamForestLevelData.SmoothStep(Age / 2.7f) - DreamForestLevelData.SmoothStep(beforeAge / 2.7f));
                    n.Y = Level.StealthGroundYAt(n.X);
                    n.Vx = (n.X - prevX) / dt;
                    n.AnimTime += Math.Abs(n.X - prevX) / 30f;
                    n.Facing = Age > 2.7f ? -1 : 1;
                    if (Age >= 6f)
                    {
                        Stage = DreamForestStage.NightmareWake;
                        Age = 0f;
                        n.Vx = 0f;
                        n.ScreamPlayed = true;
                        frameEvents.Add(new DreamForestEvent(DreamForestEventKind.NightmareScream));
                    }
                    continue;
                }

                if (Stage == DreamForestStage.NightmareWake)
                {
                    if (Age >= 0.55f) n.FairyAwake = true;
                    if (n.WakeIndex < 0)
                    {
                        if (Age >= 1.5f)
                        {
                            n.WakeIndex = 0;
                            n.WakeAge = 0f;
                        }
                    }
                    else
                    {
                        n.WakeAge += dt;
                        if (n.WakeAge >= DreamForestLevelData.NightmareWakeLines[n.WakeIndex].Duration) AdvanceDialogue();
                    }
                    continue;
                }

                if (Stage == DreamForestStage.Complete) continue;

                // STEALTH gameplay
                float oldPlayerX = n.X;
                float oldPlayerY = n.Y;
                var mudBefore = n.MudMode;
                float mudAgeBefore = n.MudAge;

                if (!StepMud(input, dt))
                {
                    float ix = Math.Max(-1f, Math.Min(1f, input.X));
                    n.Vx = ix * DreamForestLevelData.StealthSpeed;
                    if (input.Jump && !n.JumpHeld && n.Grounded)
                    {
                        n.Vy = -DreamForestLevelData.StealthJumpSpeed;
                        n.Grounded = false;
                        frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Jump));
                    }
                    n.JumpHeld = input.Jump;

                    float nextX = Math.Max(60f, Math.Min(DreamForestLevelData.StealthExitX, n.X + n.Vx * dt));
                    float ground = Level.StealthGroundYAt(nextX);
                    if (n.Grounded && n.Y - ground > 12f)
                    {
                        nextX = n.X;
                        n.Vx = 0f;
                        ground = Level.StealthGroundYAt(n.X);
                    }

                    var lake = Level.MudLakeAt(nextX);
                    if (lake != null && n.Y >= DreamForestLevelData.MudWaterline(lake, nextX) - 35f)
                    {
                        n.X = nextX;
                        EnterMud(lake, n.Vx < 0f ? -1 : 1, submerged: false);
                    }
                    else
                    {
                        if (n.Grounded)
                        {
                            n.Y = ground;
                        }
                        else
                        {
                            n.Vy += DreamForestLevelData.StealthGravity * dt;
                            n.Y += n.Vy * dt;
                            if (n.Vy >= 0f && oldPlayerY <= ground + 4f && n.Y >= ground)
                            {
                                n.Y = ground;
                                n.Vy = 0f;
                                n.Grounded = true;
                            }
                        }
                        n.X = nextX;
                        var wet = Level.MudLakeAt(n.X);
                        if (wet != null && n.Y >= DreamForestLevelData.MudWaterline(wet, n.X) - 35f)
                        {
                            EnterMud(wet, ix < 0f ? -1 : 1, submerged: false);
                        }
                    }
                    n.AnimTime += Math.Abs(n.X - oldPlayerX) / 30f;
                    if (Math.Abs(ix) > 0.01f) n.Facing = ix < 0f ? -1 : 1;
                }

                if (mudBefore == DreamForestMudMode.Enter && n.MudMode == DreamForestMudMode.Enter)
                {
                    if (mudAgeBefore < 0.12f && n.MudAge >= 0.12f) frameEvents.Add(new DreamForestEvent(DreamForestEventKind.MudTakeoff));
                    if (mudAgeBefore < 0.68f && n.MudAge >= 0.68f) frameEvents.Add(new DreamForestEvent(DreamForestEventKind.MudImpact));
                }

                if (n.X >= 2380f && n.Grounded && Math.Abs(n.X - 2380f) < 70f) n.Checkpoint = Math.Max(n.Checkpoint, 1);
                if (n.MudMode == DreamForestMudMode.Swim && n.MudDepth >= 0.94f)
                {
                    var l = FindMudLake(n.MudLakeId);
                    if (l != null) n.Checkpoint = Math.Max(n.Checkpoint, l.Index >= 2 ? 3 : 2);
                }

                Progress = Math.Max(Progress, 0.65f + 0.35f * DreamForestLevelData.Clamp01(n.X / DreamForestLevelData.StealthExitX));

                var lastLake = Level.MudLakes[Level.MudLakes.Length - 1];
                if (n.Grounded && n.MudMode == DreamForestMudMode.Dry && n.X >= lastLake.To + 11.9f)
                {
                    Stage = DreamForestStage.NightmareFinal;
                    Age = 0f;
                    n.Vx = 0f;
                    n.Exposure = 0f;
                    continue;
                }

                bool lit = IsNightmarePlayerIlluminated(detection: true);
                n.Exposure = lit ? n.Exposure + dt : 0f;
                float limit = Difficulty == DreamForestDifficulty.Easy ? 0.24f : 0.12f;
                if (n.Exposure >= limit)
                {
                    Stage = DreamForestStage.NightmareStrike;
                    Age = 0f;
                    n.Vx = 0f;
                    n.Vy = 0f;
                    Hits++;
                    Falls++;
                    FallCause = "roach";
                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.NightmareDetected));
                }
            }
        }

        private void EnterMud(DreamForestMudLakeSpec lake, int side, bool submerged)
        {
            var n = Nightmare;
            int dir = side < 0 ? -1 : 1;
            n.MudLakeId = lake.Id;
            n.MudMode = submerged ? DreamForestMudMode.Swim : DreamForestMudMode.Enter;
            n.MudAge = 0f;
            n.MudSide = dir;
            n.MudStartX = n.X;
            n.MudStartY = n.Y;
            n.MudEntryTarget = dir > 0 ? lake.From + 85f : lake.To - 85f;
            n.SwimDistance = 0f;
            n.Grounded = false;
            n.Vy = 0f;
            n.Vx = 0f;
            if (submerged)
            {
                n.X = Math.Max(lake.From + 85f, Math.Min(lake.To - 85f, n.X));
                n.MudDepth = 1f;
                n.Y = DreamForestLevelData.MudWaterline(lake, n.X) + 156.8f;
            }
        }

        private bool StepMud(DreamForestInput input, float dt)
        {
            var n = Nightmare;
            if (n.MudMode == DreamForestMudMode.Dry) return false;
            var lake = FindMudLake(n.MudLakeId);
            if (lake == null) return false;

            float oldX = n.X;
            n.MudAge += dt;
            n.JumpHeld = input.Jump;
            n.Grounded = false;
            n.Vy = 0f;

            if (n.MudMode == DreamForestMudMode.Enter)
            {
                float q = DreamForestLevelData.SmoothStep(n.MudAge / 1.1f);
                n.X = n.MudStartX + (n.MudEntryTarget - n.MudStartX) * q;
                float surface = DreamForestLevelData.MudWaterline(lake, n.X);
                float flight = DreamForestLevelData.Clamp01((n.MudAge - 0.12f) / 0.56f);
                float sink = DreamForestLevelData.SmoothStep((n.MudAge - 0.68f) / 0.42f);
                float rise = n.MudStartY < DreamForestLevelData.MudWaterline(lake, n.MudStartX) - 24f ? 0f : 110f;
                n.Y = n.MudAge < 0.12f
                    ? n.MudStartY
                    : n.MudStartY + (surface + 7f - n.MudStartY) * flight - rise * 4f * flight * (1f - flight) + 149.8f * sink;
                n.MudDepth = sink;
                if (n.MudAge >= 1.1f)
                {
                    n.MudMode = DreamForestMudMode.Swim;
                    n.MudAge = 0f;
                    n.MudDepth = 1f;
                    n.Y = surface + 156.8f;
                }
            }
            else if (n.MudMode == DreamForestMudMode.Climb)
            {
                float q = DreamForestLevelData.SmoothStep(n.MudAge / 1.9f);
                float bank = n.MudSide < 0 ? lake.From : lake.To;
                n.X = n.MudStartX + (bank + n.MudSide * 12f - n.MudStartX) * q;
                n.Y = n.MudStartY + (DreamForestLevelData.MudBankY(lake, n.MudSide) - n.MudStartY) * q;
                n.MudDepth = 1f - q;
                n.Facing = n.MudSide;
                if (q >= 1f)
                {
                    n.MudMode = DreamForestMudMode.Dry;
                    n.MudLakeId = null;
                    n.MudDepth = 0f;
                    n.Grounded = true;
                    n.Y = Level.MudGroundAt(n.X) ?? DreamForestLevelData.StealthGroundY;
                    n.MudAge = 0f;
                    n.MudSide = 0;
                }
            }
            else
            {
                float dir = Math.Max(-1f, Math.Min(1f, input.X));
                bool down = input.Y > 0.3f;
                float target = (down || Math.Abs(dir) < 0.05f) ? 1f : 0.68f;
                float delta = Math.Max(-dt * 0.6f, Math.Min(dt * (down ? 1.8f : 0.65f), target - n.MudDepth));
                n.MudDepth += delta;
                n.X = Math.Max(lake.From + 75f, Math.Min(lake.To - 75f, n.X + dir * (down ? 78f : 105f) * dt));
                n.Y = DreamForestLevelData.MudWaterline(lake, n.X) + 80f + Math.Max(0f, n.MudDepth - 0.68f) * 240f;
                if (Math.Abs(dir) > 0.01f) n.Facing = dir < 0f ? -1 : 1;
                n.SwimDistance += Math.Abs(n.X - oldX);

                int side = n.X <= lake.From + 77f ? -1 : (n.X >= lake.To - 77f ? 1 : 0);
                bool wantsExit = side != 0 && (dir * side > 0.1f || input.Jump || input.Y < -0.3f);
                if (wantsExit && !down)
                {
                    n.MudMode = DreamForestMudMode.Climb;
                    n.MudAge = 0f;
                    n.MudSide = side;
                    n.MudStartX = n.X;
                    n.MudStartY = n.Y;
                    n.MudStartDepth = n.MudDepth;
                }
            }

            n.Vx = (n.X - oldX) / dt;
            n.AnimTime += Math.Abs(n.X - oldX) / 30f;
            return true;
        }

        private DreamForestMudLakeSpec FindMudLake(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Level.MudLakes.Length; i++)
            {
                if (Level.MudLakes[i].Id == id) return Level.MudLakes[i];
            }
            return null;
        }

        private void BoardBeetle(string id)
        {
            if (Level.ById.TryGetValue(id, out var o) && o.Flight != null && (!Rides.TryGetValue(id, out var s) || !s.HasValue))
            {
                Rides[id] = Time;
                frameEvents.Add(new DreamForestEvent(DreamForestEventKind.BeetleBoarded, id));
            }
        }

        private void PlaceAtCheckpoint()
        {
            var o = DreamForestLevelData.EvaluateObjectFrame(Level.ById[CheckpointId], Time, Rides);
            Player.X = o.X + o.W * 0.45f;
            Player.Y = DreamForestLevelData.SurfaceY(o, Player.X);
            Player.Vx = 0f;
            Player.Vy = 0f;
            Player.SupportId = o.Spec.Id;
            Player.Climb = null;
            Player.LockId = null;
            Player.LockTime = 0f;
            Player.JumpHeld = true;
        }

        private bool CheckSpiderHit()
        {
            if (Stage != DreamForestStage.Running) return false;
            for (int i = 0; i < nearbyBuffer.Count; i++)
            {
                var o = nearbyBuffer[i];
                if (o.Kind != DreamForestObjectKind.Web) continue;
                var f = DreamForestLevelData.EvaluateObjectFrame(o, Time, Rides);
                if (DreamForestLevelData.SpiderContact(f, Player.X, Player.Y))
                {
                    Hits++;
                    Falls++;
                    FallCause = "spider";
                    Stage = DreamForestStage.Falling;
                    Age = 0f;
                    Player.SupportId = null;
                    Player.Climb = null;
                    Player.LockId = o.Id;
                    Player.LockTime = 0.38f;
                    Player.Vx = Player.X < f.X ? -170f : 170f;
                    Player.Vy = -150f;
                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Hurt, o.Id));
                    frameEvents.Add(new DreamForestEvent(DreamForestEventKind.Fall));
                    return true;
                }
            }
            return false;
        }
    }
}
