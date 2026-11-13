using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D;

namespace YunaLegend.YunaIllustration
{
    /// <summary>
    /// Installs the approved illustrated Yuna (T03 idle/run/jump/stumble) on the real
    /// 3D controller at runtime, for the playable validation build. The 3D body is only
    /// hidden; the controller, collider and physics are untouched.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class YunaIllustrationInstaller : MonoBehaviour
    {
        public Sprite profile;
        public Texture2D blink;
        public Material idleMaterial;
        public Material runMaterial;
        public Material stumbleMaterial;
        public Sprite[] runFrames;
        public Sprite[] jumpFrames;
        public Sprite[] stumbleFrames;
        public bool hide3D = true;

        void Awake()
        {
            var player = GetComponent<Yuna25DPlayerController>();
            if (player == null) player = GetComponentInParent<Yuna25DPlayerController>();
            if (player == null)
            {
                Debug.LogError("YunaIllustrationInstaller: Yuna25DPlayerController not found.");
                return;
            }
            if (player.GetComponent<YunaIllustrationVisibilityResolver>() != null) return;

            if (hide3D)
            {
                foreach (var renderer in player.GetComponentsInChildren<Renderer>(true))
                    if (!(renderer is SpriteRenderer)) renderer.enabled = false;
            }

            var idle = InstallIdle(player);

            var runGo = MakeSprite(player, "Ilustrado_Run", runMaterial, runFrames, 60);
            var run = runGo.AddComponent<YunaRunIllustration>();
            run.player = player;
            run.painting = runGo.GetComponent<SpriteRenderer>();
            run.frames = runFrames;
            run.footPlant = true;
            runGo.SetActive(true);

            var jumpGo = MakeSprite(player, "Ilustrado_Jump", runMaterial, jumpFrames, 61);
            var jump = jumpGo.AddComponent<YunaJumpIllustration>();
            jump.player = player;
            jump.run = run;
            jump.painting = jumpGo.GetComponent<SpriteRenderer>();
            jump.frames = jumpFrames;
            jumpGo.SetActive(true);

            var stumbleGo = MakeSprite(player, "Ilustrado_Stumble", stumbleMaterial, stumbleFrames, 62);
            var stumble = stumbleGo.AddComponent<YunaStumbleIllustration>();
            stumble.player = player;
            stumble.run = run;
            stumble.painting = stumbleGo.GetComponent<SpriteRenderer>();
            stumble.frames = stumbleFrames;
            stumbleGo.SetActive(true);

            var resolver = player.gameObject.AddComponent<YunaIllustrationVisibilityResolver>();
            resolver.idle = idle;
            resolver.idleRenderer = idle.GetComponent<MeshRenderer>();
            resolver.run = run;
            resolver.jump = jump;
            resolver.stumble = stumble;
            resolver.showIdle = false;
            Debug.Log("YUNA_ILLUSTRATION_INSTALLED root=" + player.name);
        }

        YunaIdleIllustration InstallIdle(Yuna25DPlayerController player)
        {
            var go = new GameObject("Ilustrado_Idle");
            go.SetActive(false);
            go.transform.SetParent(player.transform, false);
            var skin = go.AddComponent<PaintedMeshDeformer>();
            if (profile != null && idleMaterial != null)
            {
                idleMaterial.SetTexture("_MainTex", profile.texture);
                if (blink != null) idleMaterial.SetTexture("_BlinkTex", blink);
                idleMaterial.SetFloat("_Blink", 0f);
                skin.Build(profile, idleMaterial);
            }
            var driver = go.AddComponent<YunaIdleIllustration>();
            driver.skin = skin;
            driver.player = player;
            driver.previewClock = false;
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;
            go.SetActive(true);
            driver.Evaluate(0f);
            return driver;
        }

        static GameObject MakeSprite(Yuna25DPlayerController player, string name, Material material,
            Sprite[] frames, int sortingOrder)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.SetParent(player.transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            if (material != null) renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.sprite = frames != null && frames.Length > 0 ? frames[frames.Length - 1] : null;
            renderer.enabled = false;
            return go;
        }
    }
}
