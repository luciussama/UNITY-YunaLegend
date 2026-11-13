using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;
using YunaLegend.Illustrated25D.Furniture;
using YunaLegend.NandaIllustration;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>Dedicated accompaniment build. Keeps the original scene and physical controller intact.</summary>
    [DefaultExecutionOrder(260)]
    public sealed class IllustratedPreview : MonoBehaviour
    {
        public Texture2D panorama, furnitureBody, furnitureWood;
        public Sprite[] nandaFrames;
        public Shader[] requiredShaders;
        public Yuna25DPlayerController Player { get; private set; }
        public RoomBackgroundT054 Room { get; private set; }
        public PenumbraLightingT054 Lighting { get; private set; }
        public NandaFlightIllustration Flight { get; private set; }
        public YieldingWoodFurniture Furniture { get; private set; }
        public int LegacyRenderersVisible { get; private set; }
        public bool Finished { get; private set; }
        public bool Started { get; private set; }
        public bool Ready { get; private set; }
        public string Failure { get; private set; }
        DarkRoomPlatformMarker[] pilot;
        Renderer[] legacy;
        NandaFairyGuide guide;
        YunaIllustrationVisibilityResolver resolver;
        SideScroll25DCamera cameraDriver;
        bool paused;
        float clock;

        void Start()
        {
            try { Install(); Ready = true; Debug.Log("ILLUSTRATED_PREVIEW_READY legacyVisible=0"); }
            catch (Exception e)
            {
                Failure = "Não foi possível montar a cena ilustrada. Consulte o log da build.";
                Debug.LogException(e);
                // A missing approved asset must never reveal the old presentation.
                foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None)) r.enabled = false;
                if (Player != null) Player.AutoRunActive = false;
            }
        }

        void Install()
        {
            if (panorama == null || furnitureBody == null || furnitureWood == null || nandaFrames == null
                || nandaFrames.Length != 8 || nandaFrames.Any(f => f == null))
                throw new InvalidOperationException("Approved preview assets missing");
            Player = FindFirstObjectByType<Yuna25DPlayerController>();
            guide = FindFirstObjectByType<NandaFairyGuide>();
            resolver = Player.GetComponent<YunaIllustrationVisibilityResolver>();
            if (resolver == null || guide == null) throw new InvalidOperationException("Illustrated characters missing");
            cameraDriver = Camera.main.GetComponent<SideScroll25DCamera>();
            var markers = FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            pilot = Enumerable.Range(0,3).Select(i => markers.Single(m => m.spec.index == i)).ToArray();
            foreach (var m in pilot) m.gameObject.SetActive(true);
            foreach (var m in markers.Where(m => m.spec.index > 2)) m.gameObject.SetActive(false);

            // Snapshot the old presentation before adding the approved runtime paintings.
            var bearPainting=pilot[0].transform.Find("Visual_Ilustrado_Urso");
            var bookPainting=pilot[1].transform.Find("Visual_Ilustrado_Livros");
            if(bearPainting==null || bookPainting==null) throw new InvalidOperationException("Approved platforms missing");
            legacy = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(r => r!=resolver.idleRenderer && r!=resolver.run.painting
                    && r!=resolver.jump.painting && r!=resolver.stumble.painting
                    && !r.transform.IsChildOf(bearPainting) && !r.transform.IsChildOf(bookPainting)).ToArray();
            foreach (var r in legacy) r.enabled = false;
            foreach (var lamp in FindObjectsByType<Light>(FindObjectsSortMode.None)) lamp.enabled = false;
            foreach (var trail in guide.GetComponentsInChildren<TrailRenderer>(true)) { trail.emitting = false; trail.Clear(); }

            Furniture = pilot[2].GetComponent<YieldingWoodFurniture>();
            pilot[2].gameObject.AddComponent<FurnitureIllustrationCandidate>().Install(furnitureBody, furnitureWood);
            float floor = Mathf.Min(Bounds(pilot[0]).min.y, Bounds(pilot[1]).min.y);
            var books = pilot[1].transform.Find("Visual_Ilustrado_Livros");
            if (books == null) throw new InvalidOperationException("Approved books missing");
            float baseY = books.TransformPoint(new Vector3(0,
                IllustratedBookStackPlatformProfile.Document.mapping.quadBoundsLocal.bottom,0)).y;
            if (baseY > floor + .001f)
            {
                var lower = Instantiate(books.gameObject,pilot[1].transform);
                lower.name = "Livros_Base_T054";
                lower.transform.position = books.position + Vector3.down * (baseY-floor);
                lower.GetComponent<Renderer>().sortingOrder = books.GetComponent<Renderer>().sortingOrder-1;
            }
            float min = pilot.Min(p => Bounds(p).min.x), max = pilot.Max(p => Bounds(p).max.x);
            Room = new GameObject("Quarto_Ilustrado_Aprovado").AddComponent<RoomBackgroundT054>();
            var spriteMaterial = new Material(Shader.Find("Sprites/Default"));
            Room.InstallPanels(new[]{panorama},panorama.width/32f,null,null,floor,(min+max)*.5f,
                spriteMaterial,1,1,82f/724f,.35f);
            var shadows = new GameObject(ContactShadowT054.RootName);
            foreach (var p in pilot)
            {
                var b = Bounds(p);
                ContactShadowT054.Install(shadows.transform,b.center.x,floor,b.size.x,b.size.z,.18f);
            }

            var visual = new GameObject("Nanda_Ilustrada"); visual.SetActive(false);
            visual.transform.SetParent(guide.transform,false);
            var painting = visual.AddComponent<SpriteRenderer>();
            painting.sprite = nandaFrames[0]; painting.sortingOrder = 110;
            Flight = guide.gameObject.AddComponent<NandaFlightIllustration>();
            Flight.painting = painting; Flight.visualRoot = visual.transform; Flight.frames = nandaFrames;
            visual.SetActive(true);
            var lampGo = new GameObject("Nanda_Luz_Azul"); lampGo.transform.SetParent(guide.transform,false);
            var lampBlue = lampGo.AddComponent<Light>(); lampBlue.type = LightType.Point;
            var trailGo = new GameObject("Nanda_Trilha_Azul"); trailGo.transform.SetParent(guide.transform,false);
            var ribbon = trailGo.AddComponent<TrailRenderer>(); ribbon.sharedMaterial = spriteMaterial;
            var accompaniment = guide.gameObject.AddComponent<NandaAccompaniment>();
            accompaniment.guide = guide; accompaniment.player = Player; accompaniment.flight = Flight;
            accompaniment.blueLight = lampBlue; accompaniment.blueTrail = ribbon;
            // Awake precedes assignment on an active guide; re-enable to initialize trail settings.
            accompaniment.enabled = false;
            ConfigureTrail(ribbon);
            accompaniment.enabled = true;
            accompaniment.ResetPresentation();

            Lighting = Room.gameObject.AddComponent<PenumbraLightingT054>();
            Lighting.lightingShader = requiredShaders.Single(s => s != null && s.name == "YunaLegend/RoomPenumbraT054");
            Lighting.room = Room; Lighting.nanda = painting.transform;
            foreach (var r in Room.FarPanels) Lighting.Bind(r,0);
            foreach (var p in pilot)
                foreach (var r in p.GetComponentsInChildren<Renderer>()) if (r.enabled) Lighting.Bind(r,1);
            Lighting.Bind(resolver.idleRenderer,2,true,true);
            Lighting.Bind(resolver.run.painting,2,true);
            Lighting.Bind(resolver.jump.painting,2,true);
            Lighting.Bind(resolver.stumble.painting,2,true,false,true);
            Lighting.Bind(painting,3);
            resolver.idle.previewClock = true;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(.007f,.009f,.018f);
            cameraDriver.SetTarget(Player.transform);
            cameraDriver.ConfigureIllustratedPilot(true);
            cameraDriver.ScreenCrushActive = false;
            cameraDriver.AutoScrollActive = false;
            Player.PrepareForOpening(DarkRoomStartLayout.Position);
            Player.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
            Player.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
            Player.SetFloorDeathActive(true);
            guide.SetYuna(Player.transform); guide.ReturnToYuna();
            guide.transform.position = Player.transform.position + new Vector3(1.65f,1.4f,-.15f);
            accompaniment.ResetPresentation();
            cameraDriver.SnapToTargetX(Player.transform.position.x);
            Room.Follow(Camera.main.transform.position); Lighting.Apply();
        }

        static Bounds Bounds(DarkRoomPlatformMarker m)
        {
            var colliders = m.GetComponentsInChildren<Collider>(true).Where(c => c.enabled).ToArray();
            var b = colliders[0].bounds; foreach (var c in colliders) b.Encapsulate(c.bounds); return b;
        }
        static void ConfigureTrail(TrailRenderer ribbon)
        {
            ribbon.time=.32f; ribbon.minVertexDistance=.04f;
            ribbon.widthCurve=AnimationCurve.Linear(0,.028f,1,0); ribbon.widthMultiplier=1;
            ribbon.sortingOrder=109; ribbon.numCapVertices=3;
            var g=new Gradient(); g.SetKeys(new[]{new GradientColorKey(new Color(.34f,.63f,1),0),
                new GradientColorKey(new Color(.25f,.48f,.92f),1)},
                new[]{new GradientAlphaKey(.24f,0),new GradientAlphaKey(0,1)});
            ribbon.colorGradient=g;
        }
        public void Begin()
        {
            if (!Ready || Started) return;
            Started=true; Player.AutoRunActive=true;
            Player.AllowFloorLaunch(Player.transform.position.x);
        }
        public void Retry()
        {
            Time.timeScale=1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        void Update()
        {
            if (!Ready) return;
            var k=Keyboard.current;
            if (k != null && k.rKey.wasPressedThisFrame) { Retry(); return; }
            if (k != null && (k.pKey.wasPressedThisFrame || k.escapeKey.wasPressedThisFrame) && Started && !Finished)
            { paused=!paused; Time.timeScale=paused?0:1; }
            if (!Started && k != null && (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame)) Begin();
            if (!Finished && Player.CurrentSupportIndex==2 && Player.IsGrounded && Furniture.HasCracked
                && !Player.IsStumbling && Player.transform.position.x>pilot[2].transform.position.x+2f)
            { Finished=true; Player.AutoRunActive=false; cameraDriver.AutoScrollActive=false; }
        }
        void LateUpdate()
        {
            if (!Ready) return;
            clock+=Time.deltaTime;
            resolver.showIdle=!Started || Finished || Player.IsDead;
            Room.Follow(Camera.main.transform.position);
            foreach (var r in legacy) if (r != null) r.enabled=false;
            LegacyRenderersVisible=legacy.Count(r=>r!=null && r.enabled && r.gameObject.activeInHierarchy);
        }
        void OnGUI()
        {
            GUI.skin.label.fontSize=Mathf.Max(14,Screen.height/45);
            GUI.skin.button.fontSize=GUI.skin.label.fontSize;
            GUI.Label(new Rect(18,12,Screen.width-36,32),"Yuna Legend · acompanhamento ilustrado · urso → livros → móvel");
            GUI.Label(new Rect(18,44,Screen.width-36,32),"Espaço / ↑ / clique: pular     P: pausa     R: recomeçar");
            string message=Failure;
            if (message==null && !Started) message="O quarto ilustrado\nUma versão para acompanhar o que já construímos.";
            if (Player != null && Player.IsDead) message="Vamos tentar outra vez?";
            if (Finished) message="Você chegou ao fim do trecho atual!\nAs próximas plataformas ainda estão em produção.";
            if (paused) message="Pausado";
            if (message==null) return;
            float w=Mathf.Min(560,Screen.width-40),x=(Screen.width-w)*.5f,y=Screen.height*.28f;
            GUI.Box(new Rect(x,y,w,170),"");
            GUI.Label(new Rect(x+22,y+20,w-44,75),message);
            if (Failure!=null) return;
            if (GUI.Button(new Rect(x+22,y+108,w-44,42),!Started?"Começar":paused?"Continuar":"Recomeçar"))
            { if (!Started) Begin(); else if(paused) {paused=false;Time.timeScale=1;} else Retry(); }
        }
        void OnDestroy() { Time.timeScale=1; }
    }
}
