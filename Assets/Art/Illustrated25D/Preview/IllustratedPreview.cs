using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;
using YunaLegend.Illustrated25D.Furniture;
using YunaLegend.Illustrated25D.Preview.Controllers;
using YunaLegend.NandaIllustration;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>
    /// Composition Root enxuto da cena dedicada de acompanhamento ilustrado 2.5D.
    /// Delega plataformas, cenografia, iluminação e guia Nanda para controllers especialistas,
    /// preservando a cena base e o controlador físico 3D originais.
    /// </summary>
    [DefaultExecutionOrder(260)]
    public sealed class IllustratedPreview : MonoBehaviour
    {
        public Texture2D panorama;
        public Texture2D roomExtension;
        public Texture2D furnitureBody;
        public Texture2D furnitureWood;

        public bool includeNextDresser;
        public bool includeNextBlocks;
        public bool includeNextDrum;
        public bool includeNextDrawers07;
        public bool includeNextCastle09;
        public bool includeNextStage2Platform10;
        public bool includeFullStage;
        public bool playableLegacyMode;

        public Texture2D dresserPainting;
        public Texture2D blocksPainting;
        public Texture2D drumPainting;
        public Texture2D cushionPainting;
        public Texture2D drawersPainting;
        public Texture2D musicBoxPainting;
        public Texture2D castlePainting;
        public Texture2D castleFrontTowersPainting;
        public Texture2D trainTrestle10Painting;
        public Texture2D[] fullStagePaintings;

        public int LastPlatformIndex => ResolveLastPlatformIndex();

        public Texture2D GetFullStagePainting(int platformIndex)
        {
            int slot = platformIndex - 11;
            if (fullStagePaintings == null || slot < 0 || slot >= fullStagePaintings.Length)
            {
                return null;
            }
            return fullStagePaintings[slot];
        }

        public Sprite[] nandaFrames;
        public Shader[] requiredShaders;

        public StagePlatformController PlatformController { get; private set; }
        public StageSceneryController SceneryController { get; private set; }
        public StageLightingController LightingController { get; private set; }
        public NandaGuideController NandaController { get; private set; }
        public LegacyPlayableStageFlow PlayableFlow { get; private set; }

        public DresserPlatform Dresser => PlatformController != null ? PlatformController.Dresser : null;
        public AbcStackPlatform Blocks => PlatformController != null ? PlatformController.Blocks : null;
        public Drum05Platform Drum => PlatformController != null ? PlatformController.Drum : null;
        public Cushion06Platform Cushion => PlatformController != null ? PlatformController.Cushion : null;
        public Drawers07Platform Drawers => PlatformController != null ? PlatformController.Drawers : null;
        public MusicBox08Platform MusicBox => PlatformController != null ? PlatformController.MusicBox : null;
        public Castle09Platform Castle => PlatformController != null ? PlatformController.Castle : null;
        public TrainTrestle10Platform TrainTrestle10 => PlatformController != null ? PlatformController.TrainTrestle10 : null;
        public YieldingWoodFurniture Furniture => PlatformController != null ? PlatformController.Furniture : null;

        public Yuna25DPlayerController Player { get; private set; }
        public RoomBackgroundT054 Room => SceneryController != null ? SceneryController.Room : null;
        public PenumbraLightingT054 Lighting => LightingController != null ? LightingController.Lighting : null;
        public NandaFlightIllustration Flight => NandaController != null ? NandaController.Flight : null;
        public int LegacyRenderersVisible { get; private set; }
        public bool Finished { get; private set; }
        public bool Started { get; private set; }
        public bool Ready { get; private set; }
        public string Failure { get; private set; }

        private DarkRoomPlatformMarker[] pilot;
        private Renderer[] legacy;
        private NandaFairyGuide guide;
        private YunaIllustrationVisibilityResolver resolver;
        private SideScroll25DCamera cameraDriver;
        private bool paused;

        private void Start()
        {
            try
            {
                Install();
                Ready = true;
                Debug.Log("ILLUSTRATED_PREVIEW_READY legacyVisible=0");
            }
            catch (Exception e)
            {
                Failure = "Não foi possível montar a cena ilustrada. Consulte o log da build.";
                Debug.LogException(e);

                foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    r.enabled = false;
                }
                if (Player != null)
                {
                    Player.AutoRunActive = false;
                }
            }
        }

        public void Begin()
        {
            if (!Ready || Started)
            {
                return;
            }

            Started = true;
            Player.AutoRunActive = true;
            Player.AllowFloorLaunch(Player.transform.position.x);
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Update()
        {
            if (!Ready || playableLegacyMode)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                Retry();
                return;
            }

            if (keyboard != null
                && (keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                && Started
                && !Finished)
            {
                paused = !paused;
                Time.timeScale = paused ? 0f : 1f;
            }

            if (!Started && keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
            {
                Begin();
            }

            int endIndex = ResolveLastPlatformIndex();
            float finishOffset = ResolveFinishOffset();
            if (!Finished
                && Player.CurrentSupportIndex == endIndex
                && Player.IsGrounded
                && Furniture.HasCracked
                && !Player.IsStumbling
                && Player.transform.position.x > pilot[endIndex].transform.position.x + finishOffset)
            {
                Finished = true;
                Player.AutoRunActive = false;
                cameraDriver.AutoScrollActive = false;
            }
        }

        private void LateUpdate()
        {
            if (!Ready)
            {
                return;
            }

            resolver.showIdle = playableLegacyMode && PlayableFlow != null
                ? (PlayableFlow.ShowIdlePose || Player.IsDead)
                : (!Started || Finished || Player.IsDead);

            SceneryController.FollowCamera(Camera.main.transform.position);
            foreach (var r in legacy)
            {
                if (r != null)
                {
                    r.enabled = false;
                }
            }

            LegacyRenderersVisible = legacy.Count(r => r != null && r.enabled && r.gameObject.activeInHierarchy);
        }

        private void OnGUI()
        {
            if (playableLegacyMode)
            {
                return;
            }

            GUI.skin.label.fontSize = Mathf.Max(14, Screen.height / 45);
            GUI.skin.button.fontSize = GUI.skin.label.fontSize;

            GUI.Label(new Rect(18f, 12f, Screen.width - 36f, 32f), ResolvePreviewHeader());
            GUI.Label(new Rect(18f, 44f, Screen.width - 36f, 32f), "Espaço / ↑ / clique: pular     P: pausa     R: recomeçar");

            string message = ResolvePreviewModalMessage();
            if (message == null)
            {
                return;
            }

            float w = Mathf.Min(560f, Screen.width - 40f);
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.28f;

            GUI.Box(new Rect(x, y, w, 170f), string.Empty);
            GUI.Label(new Rect(x + 22f, y + 20f, w - 44f, 75f), message);
            if (Failure != null)
            {
                return;
            }

            string buttonLabel = !Started ? "Começar" : paused ? "Continuar" : "Recomeçar";
            if (GUI.Button(new Rect(x + 22f, y + 108f, w - 44f, 42f), buttonLabel))
            {
                if (!Started)
                {
                    Begin();
                }
                else if (paused)
                {
                    paused = false;
                    Time.timeScale = 1f;
                }
                else
                {
                    Retry();
                }
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        private void Install()
        {
            ValidateBaseAssets();

            Player = FindFirstObjectByType<Yuna25DPlayerController>();
            guide = FindFirstObjectByType<NandaFairyGuide>();
            resolver = Player.GetComponent<YunaIllustrationVisibilityResolver>();
            if (resolver == null || guide == null)
            {
                throw new InvalidOperationException("Illustrated characters missing");
            }

            cameraDriver = Camera.main.GetComponent<SideScroll25DCamera>();
            var markers = FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int last = ResolveLastPlatformIndex();
            pilot = Enumerable.Range(0, last + 1).Select(i => markers.Single(m => m.spec.index == i)).ToArray();
            foreach (var m in pilot)
            {
                m.gameObject.SetActive(true);
            }
            foreach (var m in markers.Where(m => m.spec.index > last))
            {
                m.gameObject.SetActive(false);
            }

            var bearPainting = pilot[0].transform.Find("Visual_Ilustrado_Urso");
            var booksPainting = pilot[1].transform.Find("Visual_Ilustrado_Livros");
            if (bearPainting == null || booksPainting == null)
            {
                throw new InvalidOperationException("Approved platforms missing");
            }

            HideLegacySceneRenderers(bearPainting, booksPainting);

            PlatformController = gameObject.AddComponent<StagePlatformController>();
            PlatformController.InstallSequence(this, pilot, Player);

            float floorY = bearPainting.TransformPoint(new Vector3(0f, IllustratedBearPlatformProfile.Document.mapping.quadBoundsLocal.bottom, 0f)).y;
            var spriteMaterial = new Material(Shader.Find("Sprites/Default"));

            SceneryController = gameObject.AddComponent<StageSceneryController>();
            SceneryController.ExtendBooksBaseToFloor(booksPainting, pilot[1].transform, floorY);
            SceneryController.InstallRoom(pilot, panorama, roomExtension, includeNextStage2Platform10, floorY, spriteMaterial);
            SceneryController.InstallContactShadows(
                pilot,
                floorY,
                (marker, fallback) => PlatformController.GetPaintingBounds(marker, bearPainting, booksPainting, fallback));

            NandaController = gameObject.AddComponent<NandaGuideController>();
            NandaController.Install(guide, Player, nandaFrames, spriteMaterial);

            LightingController = gameObject.AddComponent<StageLightingController>();
            LightingController.InstallPenumbra(
                SceneryController.Room,
                requiredShaders,
                pilot,
                floorY,
                resolver,
                NandaController.Renderer,
                (marker, fallback) => PlatformController.GetPaintingBounds(marker, bearPainting, booksPainting, fallback));
            LightingController.InstallFloorReflections(
                SceneryController.Room,
                pilot,
                includeNextDresser,
                includeNextBlocks,
                includeNextDrum,
                includeNextDrawers07,
                includeNextCastle09,
                includeNextStage2Platform10);

            ConfigureCameraAndActors();
        }

        private void ValidateBaseAssets()
        {
            if (panorama == null
                || furnitureBody == null
                || furnitureWood == null
                || nandaFrames == null
                || nandaFrames.Length != 8
                || nandaFrames.Any(f => f == null))
            {
                throw new InvalidOperationException("Approved preview assets missing");
            }
        }

        private void HideLegacySceneRenderers(Transform bearPainting, Transform booksPainting)
        {
            legacy = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(r => r != resolver.idleRenderer
                    && r != resolver.run.painting
                    && r != resolver.jump.painting
                    && r != resolver.stumble.painting
                    && !r.transform.IsChildOf(bearPainting)
                    && !r.transform.IsChildOf(booksPainting))
                .ToArray();

            foreach (var r in legacy)
            {
                r.enabled = false;
            }
            foreach (var lamp in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                lamp.enabled = false;
            }
        }

        private void ConfigureCameraAndActors()
        {
            resolver.idle.previewClock = true;

            cameraDriver.SetTarget(Player.transform);
            cameraDriver.ConfigureIllustratedPilot(true);
            cameraDriver.ScreenCrushActive = false;
            cameraDriver.AutoScrollActive = false;

            Player.PrepareForOpening(DarkRoomStartLayout.Position);
            Player.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
            Player.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
            Player.SetFloorDeathActive(true);

            NandaController.PositionAtStart(guide, Player.transform);

            cameraDriver.SnapToTargetX(Player.transform.position.x);
            SceneryController.FollowCamera(Camera.main.transform.position);
            LightingController.Apply();

            if (playableLegacyMode)
            {
                PlayableFlow = gameObject.AddComponent<LegacyPlayableStageFlow>();
                PlayableFlow.Initialize(this, Player, guide, cameraDriver, pilot[0].GetTopCenterWorld());
            }
        }

        private int ResolveLastPlatformIndex()
        {
            if (includeFullStage) return 37;
            if (includeNextStage2Platform10) return 10;
            if (includeNextCastle09) return 9;
            if (includeNextDrawers07) return 7;
            if (includeNextDrum) return 5;
            if (includeNextBlocks) return 4;
            if (includeNextDresser) return 3;
            return 2;
        }

        private float ResolveFinishOffset()
        {
            if (includeNextCastle09) return 0.6f;
            if (includeNextDrawers07) return 0.4f;
            if (includeNextDrum) return 0.6f;
            if (includeNextDresser) return 0.35f;
            return 2f;
        }

        private string ResolvePreviewHeader()
        {
            if (includeNextCastle09)
                return "Yuna Legend · urso → livros → móvel → cômoda → blocos ABC → tambor → almofadão → gavetas → caixa de música → castelinho";
            if (includeNextDrawers07)
                return "Yuna Legend · urso → livros → móvel → cômoda → blocos ABC → tambor → almofadão → gavetas";
            if (includeNextDrum)
                return "Yuna Legend · urso → livros → móvel → cômoda → blocos ABC → tambor";
            if (includeNextBlocks)
                return "Yuna Legend · urso → livros → móvel → cômoda → blocos ABC";
            if (includeNextDresser)
                return "Yuna Legend · urso → livros → móvel → cômoda";
            return "Yuna Legend · acompanhamento ilustrado · urso → livros → móvel";
        }

        private string ResolvePreviewModalMessage()
        {
            if (Failure != null) return Failure;
            if (!Started) return "O quarto ilustrado\nUma versão para acompanhar o que já construímos.";
            if (Player != null && Player.IsDead) return "Vamos tentar outra vez?";
            if (Finished) return "Você chegou ao fim do trecho atual!\nAs próximas plataformas ainda estão em produção.";
            if (paused) return "Pausado";
            return null;
        }
    }
}
