using UnityEngine;
using YunaLegend.DreamForest.Core;

namespace YunaLegend.DreamForest.Runtime
{
    /// <summary>
    /// SRP Actor controller rendering Yuna (and Nanda on sunlit wake) across all Dream Forest states
    /// with 1:1 sprite parity against the legacy game:
    /// - Act 1: girl-running-v1 (8-frame cycle), girl-actions-v1 (idle/jump/fall), yuna-climbing-v2 (6-frame)
    /// - Glider: yuna-glider-v1 (velocity tilt + gust wobble), yuna-fall-wind-v1 (landing)
    /// - Act 2 Nightmare: yuna-stealth-v4 (with warm exposure tint + contact shadow), mud enter/swim/climb,
    ///   yuna-fright-standing-v2, yuna-cinematic-nightmare-v2
    /// - Sunlit Wake: yuna-waking-v2 (3-stage) + Nanda waking with blue-tinted flapping wings.
    /// </summary>
    public sealed class DreamForestActorController : MonoBehaviour
    {
        private SpriteRenderer yunaRenderer;
        private SpriteRenderer contactShadowRenderer;
        private SpriteRenderer releasedLeafRenderer;
        private SpriteRenderer wakeNandaRenderer;

        private Sprite gliderSprite;
        private Sprite fallWindSprite;
        private Sprite launchLeafSprite;
        private Sprite frightSprite0;
        private Sprite frightSprite1;
        private Sprite nandaSleepSprite;
        private Sprite nandaFairySprite;

        public void Build()
        {
            var go = new GameObject("YunaDreamActor", typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            yunaRenderer = go.GetComponent<SpriteRenderer>();
            yunaRenderer.sortingOrder = 10;

            var shadowGo = new GameObject("YunaContactShadow", typeof(SpriteRenderer));
            shadowGo.transform.SetParent(transform, false);
            contactShadowRenderer = shadowGo.GetComponent<SpriteRenderer>();
            contactShadowRenderer.sprite = DreamForestAtlasCatalog.RadialGlowSprite();
            contactShadowRenderer.color = new Color(0f, 0.01f, 0.04f, 0.45f);
            contactShadowRenderer.sortingOrder = 9;
            shadowGo.SetActive(false);

            var leafGo = new GameObject("ReleasedGliderLeaf", typeof(SpriteRenderer));
            leafGo.transform.SetParent(transform, false);
            releasedLeafRenderer = leafGo.GetComponent<SpriteRenderer>();
            releasedLeafRenderer.sortingOrder = 11;
            leafGo.SetActive(false);

            var nandaGo = new GameObject("WakeNandaFairy", typeof(SpriteRenderer));
            nandaGo.transform.SetParent(transform, false);
            wakeNandaRenderer = nandaGo.GetComponent<SpriteRenderer>();
            wakeNandaRenderer.sortingOrder = 11;
            nandaGo.SetActive(false);

            gliderSprite = DreamForestAtlasCatalog.FullSprite("yuna-glider-v1", new Vector2(0.5f, 0.5f));
            fallWindSprite = DreamForestAtlasCatalog.FullSprite("yuna-fall-wind-v1", new Vector2(0.5f, 0.5f));
            launchLeafSprite = DreamForestAtlasCatalog.SliceTopDown("yuna-glider-v1", 0, 0, 1294, 460, new Vector2(0.5f, 0.5f));
            releasedLeafRenderer.sprite = launchLeafSprite;
            frightSprite0 = DreamForestAtlasCatalog.SliceGrid("yuna-fright-standing-v2", 2, 1, 0, new Vector2(0.5f, 0f));
            frightSprite1 = DreamForestAtlasCatalog.SliceGrid("yuna-fright-standing-v2", 2, 1, 1, new Vector2(0.5f, 0f));
            nandaSleepSprite = DreamForestAtlasCatalog.SliceGrid("nanda-gestures-v1", 2, 2, 1, new Vector2(0.5f, 0f));
            nandaFairySprite = DreamForestAtlasCatalog.FullSprite("illustrated-fairy-v1", new Vector2(0.5f, 0.5f));
        }

        public void Apply(DreamForestSimulation sim)
        {
            if (yunaRenderer == null) return;

            // Opening is owned exclusively by Codex's DreamEntranceView
            if (sim.Stage == DreamForestStage.Opening)
            {
                yunaRenderer.gameObject.SetActive(false);
                contactShadowRenderer.gameObject.SetActive(false);
                releasedLeafRenderer.gameObject.SetActive(false);
                wakeNandaRenderer.gameObject.SetActive(false);
                return;
            }

            // Sunlit Wake & Complete on picnic blanket with Yuna + Nanda waking (blue-tinted fairy per AGENTS.md §3)
            if (sim.Stage == DreamForestStage.NightmareWake || sim.Stage == DreamForestStage.Complete)
            {
                ApplySunlitWake(sim);
                return;
            }

            wakeNandaRenderer.gameObject.SetActive(false);

            // Glider Landing cinematic into dark forest
            if (sim.Stage == DreamForestStage.GlideLanding)
            {
                ApplyGlideLanding(sim);
                return;
            }

            releasedLeafRenderer.gameObject.SetActive(false);
            yunaRenderer.gameObject.SetActive(true);

            bool isNightmare = sim.Nightmare != null;
            float px = isNightmare ? sim.Nightmare.X : sim.Player.X;
            float py = isNightmare ? sim.Nightmare.Y : sim.Player.Y;
            int facing = isNightmare ? sim.Nightmare.Facing : sim.Player.Facing;
            float anim = isNightmare ? sim.Nightmare.AnimTime : sim.Player.AnimTime;

            Sprite chosen;
            float targetH = isNightmare ? 1.68f : 1.18f;
            float rotZ = 0f;
            Color tint = Color.white;
            bool showContactShadow = false;

            if (!isNightmare)
            {
                if (sim.Stage == DreamForestStage.Gliding || sim.Stage == DreamForestStage.GlideGust)
                {
                    chosen = gliderSprite;
                    targetH = 1.72f;
                    float gustWobble = sim.Stage == DreamForestStage.GlideGust ? Mathf.Sin(sim.Age * 24f) * 2.0f : 0f;
                    rotZ = -Mathf.Clamp(sim.Player.Vy / 2200f, -0.18f, 0.18f) * Mathf.Rad2Deg + gustWobble;
                }
                else if (sim.Stage == DreamForestStage.GlideFalling)
                {
                    chosen = DreamForestAtlasCatalog.SliceGrid("yuna-fall-wind-v1", 2, 2, 1, new Vector2(0.5f, 0.5f)) ?? fallWindSprite;
                    targetH = 1.45f;
                    rotZ = Mathf.Sin(sim.Age * 10f) * 6f;
                }
                else if (sim.Player.Climb != null)
                {
                    int frame = Mathf.Abs(Mathf.FloorToInt(anim)) % DreamForestAtlasCatalog.ClimbCycleRects.Length;
                    RectInt r = DreamForestAtlasCatalog.ClimbCycleRects[frame];
                    Vector2 hand = DreamForestAtlasCatalog.ClimbHands[frame];
                    Vector2 handPivot = new Vector2(hand.x / Mathf.Max(1f, r.width), 1f - hand.y / Mathf.Max(1f, r.height));
                    chosen = DreamForestAtlasCatalog.SliceRect("yuna-climbing-v2", r, handPivot);
                    targetH = (DreamForestLevelData.BodyHeight / 100f) * (r.height / DreamForestAtlasCatalog.ClimbReferenceHeight);
                }
                else
                {
                    bool grounded = !string.IsNullOrEmpty(sim.Player.SupportId);
                    RectInt rect;
                    if (grounded && Mathf.Abs(sim.Player.Vx) > 1f)
                    {
                        int runIdx = Mathf.Abs(Mathf.FloorToInt(anim)) % DreamForestAtlasCatalog.GirlRunningRects.Length;
                        rect = DreamForestAtlasCatalog.GirlRunningRects[runIdx];
                        chosen = DreamForestAtlasCatalog.SliceRect("girl-running-v1", rect, new Vector2(0.5f, 0f));
                    }
                    else if (grounded)
                    {
                        rect = DreamForestAtlasCatalog.GirlActionRects[7];
                        chosen = DreamForestAtlasCatalog.SliceRect("girl-actions-v1", rect, new Vector2(0.5f, 0f));
                    }
                    else
                    {
                        int airIdx = sim.Player.Vy < 0f ? 1 : 3;
                        rect = DreamForestAtlasCatalog.GirlActionRects[airIdx];
                        chosen = DreamForestAtlasCatalog.SliceRect("girl-actions-v1", rect, new Vector2(0.5f, 0f));
                    }

                    // Exact legacy bodyHeight (88px) scaled by frame.h / referenceHeight (256px)
                    targetH = (DreamForestLevelData.BodyHeight / 100f) * (rect.height / DreamForestAtlasCatalog.GirlReferenceHeight);

                    if (sim.FallCause == "spider" && sim.Stage == DreamForestStage.Falling)
                    {
                        tint.a = Mathf.Sin(sim.Age * 45f) > 0f ? 0.45f : 1f;
                    }
                }
            }
            else
            {
                var n = sim.Nightmare;
                tint = new Color(0.84f, 0.87f, 0.94f, 1f); // cool nocturnal ambient tint matching legacy dark buffer

                if (sim.Stage == DreamForestStage.NightmareStrike || sim.Stage == DreamForestStage.GameOver)
                {
                    chosen = frightSprite0 ?? frightSprite1;
                    targetH = 1.72f;
                    if (sim.Stage == DreamForestStage.NightmareStrike && sim.Age > 0.72f)
                    {
                        float q = DreamForestLevelData.SmoothStep((sim.Age - 0.72f) / 0.7f);
                        px += 100f * q;
                        py -= Mathf.Sin(q * Mathf.PI) * 45f;
                        rotZ = -q * 37f;
                    }
                }
                else if (sim.Stage == DreamForestStage.NightmareFinal)
                {
                    chosen = sim.Age > 4.4f ? (frightSprite1 ?? frightSprite0) : (frightSprite0 ?? frightSprite1);
                    targetH = 1.78f;
                }
                else if (sim.Stage == DreamForestStage.NightmareFall || sim.Stage == DreamForestStage.NightmareIntro || sim.Stage == DreamForestStage.NightmareReveal)
                {
                    int cIdx = sim.Stage == DreamForestStage.NightmareReveal && sim.Age > 2.9f ? 3 : (Mathf.Abs(n.Vx) > 1f ? (Mathf.FloorToInt(sim.Age * 4f) % 2) : 2);
                    chosen = DreamForestAtlasCatalog.SliceGrid("yuna-cinematic-nightmare-v2", 2, 2, cIdx, new Vector2(0.5f, 0f)) ?? frightSprite0;
                    targetH = 1.75f;
                    if (sim.Stage == DreamForestStage.NightmareFall)
                    {
                        float fallQ = DreamForestLevelData.SmoothStep(sim.Age / 1.65f);
                        py -= 620f * (1f - fallQ);
                        rotZ = 8f * (1f - fallQ);
                    }
                }
                else if (n.MudMode == DreamForestMudMode.Enter)
                {
                    int frame = n.MudAge < 0.12f ? 0 : (n.MudAge < 0.48f ? 1 : (n.MudAge < 0.74f ? 2 : 3));
                    chosen = DreamForestAtlasCatalog.SliceGrid("yuna-jump-mud-v1", 2, 2, frame, new Vector2(0.5f, 0f));
                }
                else if (n.MudMode == DreamForestMudMode.Swim)
                {
                    int frame = Mathf.Abs(Mathf.FloorToInt(n.SwimDistance / 24f)) % 4;
                    chosen = DreamForestAtlasCatalog.SliceGrid("yuna-swimming-v2", 2, 2, frame, new Vector2(0.5f, 0.5f));
                }
                else if (n.MudMode == DreamForestMudMode.Climb)
                {
                    int frame = Mathf.Clamp(Mathf.FloorToInt(n.MudAge * 3f), 0, 5);
                    chosen = DreamForestAtlasCatalog.SliceGrid("yuna-exit-mud-v2", 3, 2, frame, new Vector2(0.5f, 0f));
                }
                else
                {
                    int frame = !n.Grounded ? 5 : (Mathf.Abs(n.Vx) > 1f ? (1 + Mathf.Abs(Mathf.FloorToInt(anim)) % 4) : 0);
                    chosen = DreamForestAtlasCatalog.SliceGrid("yuna-stealth-v4", 3, 2, frame, new Vector2(0.5f, 0f));
                    showContactShadow = n.Grounded;

                    // Warm amber exposure highlight when illuminated by the roach's spotlight
                    if (n.Exposure > 0f)
                    {
                        float glow = Mathf.Clamp01(n.Exposure / 0.12f) * 0.75f;
                        tint = Color.Lerp(tint, new Color(1f, 0.86f, 0.56f, 1f), glow);
                    }
                }
            }

            if (chosen != null)
            {
                yunaRenderer.sprite = chosen;
                float aspect = chosen.bounds.size.y > 0.001f ? chosen.bounds.size.x / chosen.bounds.size.y : 1f;
                DreamForestAtlasCatalog.FitSprite(yunaRenderer.transform, chosen, targetH * aspect, targetH);
            }

            yunaRenderer.color = tint;
            yunaRenderer.flipX = facing < 0;
            yunaRenderer.transform.position = DreamForestAtlasCatalog.ToUnity(px, py, -0.05f);
            yunaRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);

            contactShadowRenderer.gameObject.SetActive(showContactShadow);
            if (showContactShadow)
            {
                contactShadowRenderer.transform.position = DreamForestAtlasCatalog.ToUnity(px, py + 4f, 0.05f);
                contactShadowRenderer.transform.localScale = new Vector3(1.1f, 0.24f, 1f);
            }
        }

        private void ApplyGlideLanding(DreamForestSimulation sim)
        {
            var cam = Camera.main;
            Vector3 center = cam != null ? new Vector3(cam.transform.position.x, cam.transform.position.y, 0f) : Vector3.zero;
            float age = sim.Age;
            float fall = Mathf.Clamp01((age - 0.7f) / 1.7f);
            float startY = center.y + 1.4f;
            float groundY = center.y - 3.35f;
            float y = Mathf.Lerp(startY, groundY, fall * fall);
            float x = center.x + Mathf.Sin(age * 9f) * 0.08f * Mathf.Max(0f, 1f - age / 1.2f);

            int idx = age < 0.75f ? 0 : (age < 1.9f ? 1 : (age < 2.4f ? 2 : 3));
            Sprite s = DreamForestAtlasCatalog.SliceGrid("yuna-fall-wind-v1", 2, 2, idx, new Vector2(0.5f, 0f)) ?? fallWindSprite;
            yunaRenderer.gameObject.SetActive(true);
            yunaRenderer.sprite = s;
            yunaRenderer.flipX = false;
            yunaRenderer.color = new Color(0.82f, 0.85f, 0.92f, 1f);
            float aspect = s != null && s.bounds.size.y > 0.001f ? s.bounds.size.x / s.bounds.size.y : 1f;
            const float h = 3.1f;
            DreamForestAtlasCatalog.FitSprite(yunaRenderer.transform, s, h * aspect, h);
            yunaRenderer.transform.position = new Vector3(x, y, -0.05f);
            yunaRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, age < 2.4f ? Mathf.Sin(age * 8f) * 3f : 0f);

            // Growing contact shadow on forest floor
            contactShadowRenderer.gameObject.SetActive(true);
            contactShadowRenderer.transform.position = new Vector3(center.x, groundY, 0.05f);
            contactShadowRenderer.color = new Color(0f, 0f, 0f, 0.14f + 0.36f * fall);
            contactShadowRenderer.transform.localScale = new Vector3(1.8f - 0.7f * fall, 0.32f, 1f);

            // Drifting released leaf
            float released = Mathf.Max(0f, age - 0.75f);
            float leafAlpha = Mathf.Clamp01(1f - Mathf.Max(0f, released - 1.8f));
            releasedLeafRenderer.gameObject.SetActive(launchLeafSprite != null && leafAlpha > 0.01f);
            if (releasedLeafRenderer.gameObject.activeSelf)
            {
                float lx = center.x + 1.6f * (1f - Mathf.Exp(-released));
                float ly = (age < 0.75f ? y + 2.8f : startY + 2.8f - 0.35f * released - 0.45f * released * released);
                releasedLeafRenderer.transform.position = new Vector3(lx, ly, -0.1f);
                releasedLeafRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -(released * 40f + Mathf.Sin(age * 5f) * 4.5f));
                DreamForestAtlasCatalog.FitSprite(releasedLeafRenderer.transform, launchLeafSprite, 3.6f, 1.55f);
                releasedLeafRenderer.color = new Color(1f, 1f, 1f, leafAlpha);
            }
        }

        private void ApplySunlitWake(DreamForestSimulation sim)
        {
            var cam = Camera.main;
            Vector3 center = cam != null ? new Vector3(cam.transform.position.x, cam.transform.position.y, 0f) : Vector3.zero;
            contactShadowRenderer.gameObject.SetActive(false);
            releasedLeafRenderer.gameObject.SetActive(false);

            int wakeCell = sim.Stage == DreamForestStage.Complete
                ? 2
                : (sim.Nightmare != null ? (sim.Nightmare.WakeIndex < 1 ? 0 : (sim.Nightmare.WakeIndex == 1 ? 1 : 2)) : 2);
            Sprite pose = DreamForestAtlasCatalog.SliceGrid("yuna-waking-v2", 3, 1, wakeCell, new Vector2(0.5f, 0f));
            yunaRenderer.gameObject.SetActive(pose != null);
            if (pose != null)
            {
                float shake = sim.Stage == DreamForestStage.NightmareWake && sim.Age < 0.4f
                    ? Mathf.Sin(sim.Age * 38f) * (1f - sim.Age / 0.4f) * 0.06f
                    : 0f;
                yunaRenderer.sprite = pose;
                yunaRenderer.flipX = false;
                yunaRenderer.color = Color.white;
                float aspect = pose.bounds.size.y > 0.001f ? pose.bounds.size.x / pose.bounds.size.y : 1f;
                const float h = 3.25f;
                DreamForestAtlasCatalog.FitSprite(yunaRenderer.transform, pose, h * aspect, h);
                yunaRenderer.transform.position = new Vector3(center.x + 0.45f + shake, center.y - 2.35f, -0.05f);
                yunaRenderer.transform.localRotation = Quaternion.identity;
            }

            // Nanda waking up beside Yuna and floating gently with blue-tinted fairy aura (AGENTS.md §3)
            float awake = sim.Nightmare != null && sim.Nightmare.FairyAwake
                ? (sim.Stage == DreamForestStage.Complete ? 1f : DreamForestLevelData.SmoothStep((sim.Age - 0.55f) / 1.4f))
                : 0f;
            Sprite nandaSprite = awake > 0.25f ? (nandaFairySprite ?? nandaSleepSprite) : nandaSleepSprite;
            wakeNandaRenderer.gameObject.SetActive(nandaSprite != null);
            if (nandaSprite != null)
            {
                wakeNandaRenderer.sprite = nandaSprite;
                float t = sim.Nightmare != null ? sim.Nightmare.Time : sim.Time;
                float bobY = awake > 0.25f ? (1.15f * awake + Mathf.Sin(t * 2.2f) * 0.08f) : 0f;
                float wingBeat = awake > 0.25f ? (0.94f + 0.06f * Mathf.Abs(Mathf.Sin(t * 18f))) : 1f;
                float aspect = nandaSprite.bounds.size.y > 0.001f ? nandaSprite.bounds.size.x / nandaSprite.bounds.size.y : 1f;
                const float nh = 1.65f;
                DreamForestAtlasCatalog.FitSprite(wakeNandaRenderer.transform, nandaSprite, nh * aspect * wingBeat, nh);
                wakeNandaRenderer.transform.position = new Vector3(center.x - 1.55f, center.y - 1.55f + bobY, -0.06f);
                wakeNandaRenderer.color = new Color(0.82f, 0.94f, 1.0f, 1f); // cool blue-tinted Nanda per AGENTS.md §3
            }
        }
    }
}
