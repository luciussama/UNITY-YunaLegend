using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Camera25D
{
    /// <summary>
    /// Recorte lateral fechado, orientado ao proximo salto. Acompanha a altura do
    /// percurso sem perseguir o arco de cada pulo; na queda acompanha a menina.
    /// FOV e inclinacao permanecem estaveis. Direcao revisada pelo usuario em 11/11/2026.
    /// </summary>
    [ExecuteAlways]
    public class SideScroll25DCamera : MonoBehaviour
    {
        [Header("Alvo")]
        [SerializeField] private Yuna25DPlayerController playerTarget;

        [Header("Modo de Rolagem Constante (Runner)")]
        [Tooltip("Velocidade constante de avanco da camera no eixo X")]
        [SerializeField] private float scrollSpeed = 5.2f;

        [Tooltip("Direcao do avanco: +1 para direita (Subida), -1 para esquerda (Volta Caotica)")]
        [SerializeField] private float scrollDirection = 1f;

        [Tooltip("Define se a camera esta em avanco automatico")]
        [SerializeField] private bool autoScrollActive = false;

        [Header("Margem de Esmagamento da Tela (Screen Crush)")]
        [Tooltip("Define se a camera causa derrota por esmagamento ao ficar para tras")]
        [SerializeField] private bool screenCrushActive = false;

        [Tooltip("Distancia maxima atras do centro da camera antes de considerar esmagamento")]
        [SerializeField] private float maxDistanceBehindCamera = 7.5f;

        [Header("Enquadramento de referencia / fallback")]
        [SerializeField] private float fixedY = -4.5f;
        [SerializeField] private float fixedZ = -16.0f;
        [SerializeField] private float fixedFov = 45.0f;

        [Header("Recorte fechado do proximo salto")]
        [SerializeField] private bool closeRouteFraming = true;
        [SerializeField] private float minimumViewWidth = 10.5f;
        [SerializeField] private float maximumViewWidth = 20f;
        [SerializeField] private float verticalFramingOffset = 1.9f;
        [SerializeField] private float framingSmoothTime = 0.28f;

        [SerializeField, Min(1f)] private float routeDistanceScale = 1.05f;

        [Header("Prova ilustrada — opt-in, sem alterar a câmera da fase principal")]
        [SerializeField] private bool illustratedPilotFraming;
        [SerializeField, Min(4f)] private float illustratedVerticalSpan = 7.5f;
        [SerializeField, Min(.05f)] private float illustratedFollowTime = .45f;
        private Vector3 illustratedVelocity;
        public bool IllustratedPilotFraming => illustratedPilotFraming;

        public void ConfigureIllustratedPilot(bool active, float verticalSpan = 7.5f)
        {
            illustratedPilotFraming = active;
            illustratedVerticalSpan = Mathf.Max(4f, verticalSpan);
            illustratedVelocity = Vector3.zero;
            if (active && playerTarget != null) ApplyIllustratedFrame(true);
        }

        public float RouteDistanceScale { get => routeDistanceScale; set { routeDistanceScale = Mathf.Max(1f,value); framedTarget=-1; } }

        private NandaFairyGuide guide;
        private DarkRoomPlatformMarker[] platforms;
        private int framedTarget = -1;
        private float routeWidth;
        private float routeY;
        private float framingYVelocity;
        private float framingZVelocity;
        private bool routeOverview;
        private bool returningFromOverview;
        private Vector3 overviewPosition;
        private Vector3 overviewVelocity;

        public bool IsRouteOverview => routeOverview;

        public float ViewWidth => 2f * Mathf.Abs(transform.position.z) * Mathf.Tan(fixedFov * Mathf.Deg2Rad * 0.5f) * Aspect;
        private float Aspect => targetCamera != null ? Mathf.Max(.2f, targetCamera.aspect) : 16f / 9f;
        private float framedAspect;
        // Use viewport geometry rather than a user-agent guess: phones may rotate,
        // and desktop windows can also be narrow. Never crop the next landing.
        public static float ResponsiveMinimumWidth(float aspect, float desktopWidth)
            => Mathf.Lerp(7f, desktopWidth, Mathf.InverseLerp(.6f, 1.6f, aspect));
        private float ResponsiveMinimum => ResponsiveMinimumWidth(Aspect, minimumViewWidth);

        private Camera targetCamera;

        public bool AutoScrollActive { get => autoScrollActive; set => autoScrollActive = value; }
        public float ScrollDirection { get => scrollDirection; set => scrollDirection = Mathf.Sign(value); }
        public bool ScreenCrushActive { get => screenCrushActive; set => screenCrushActive = value; }
        public float FixedY => fixedY;
        public float FixedZ => fixedZ;
        public float FixedFov => fixedFov;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            ApplyFixedProperties();
        }

        private void LateUpdate()
        {
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
            if (Mathf.Abs(framedAspect - Aspect) > .005f)
            {
                framedAspect = Aspect;
                framedTarget = -1;
            }
            if (routeOverview)
            {
                transform.position = Vector3.SmoothDamp(transform.position, overviewPosition, ref overviewVelocity, 0.55f);
                return;
            }
            if (illustratedPilotFraming && playerTarget != null)
            {
                ApplyIllustratedFrame(false);
                if (autoScrollActive) CheckScreenCrush();
                return;
            }
            if (autoScrollActive)
            {
                // Rolagem horizontal do runner, com espaco de antecipacao proporcional ao recorte.
                float newX = transform.position.x + (scrollDirection * scrollSpeed * Time.deltaTime);
                if (playerTarget != null && playerTarget.AutoRunActive && !playerTarget.IsDead)
                {
                    float lead = closeRouteFraming ? ViewWidth * 0.18f : 3.5f;
                    float desiredX = playerTarget.transform.position.x + (scrollDirection * lead);
                    newX = Mathf.MoveTowards(newX, desiredX, scrollSpeed * 0.65f * Time.deltaTime);
                }
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);

                UpdateRouteFraming(false);

                // Deteccao de Esmagamento pela borda da tela
                CheckScreenCrush();
            }
            else if (playerTarget != null)
            {
                if (!Application.isPlaying)
                    SnapToTargetX(playerTarget.transform.position.x, DarkRoomStartLayout.CameraOffset);
                else
                    UpdateRouteFraming(false);
            }
        }

        private DarkRoomPlatformMarker FindPlatform(int index)
        {
            if (platforms == null)
                platforms = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var platform in platforms)
                if (platform != null && platform.spec.index == index) return platform;
            return null;
        }

        private void SelectRouteFrame(int index)
        {
            if (playerTarget == null) return;
            var target = FindPlatform(index);
            // At the beginning of both runs, the current support is the room floor.
            var support = index == 0 || index == 22 ? null : FindPlatform(index - 1);
            float anchorX = support != null ? (support.spec.index==YieldingWoodFurniture.PlatformIndex?support.GetTakeoffPoint(scrollDirection,.35f).x:support.GetTopCenterWorld().x) : playerTarget.transform.position.x;
            float anchorY = support != null ? support.GetTopCenterWorld().y : playerTarget.transform.position.y;
            float reach = target != null ? (target.spec.index==YieldingWoodFurniture.PlatformIndex
                ? Mathf.Abs(target.GetLandingPoint(scrollDirection).x-anchorX)+1.9f
                : Mathf.Max(Mathf.Abs(target.Support.MinX-anchorX),Mathf.Abs(target.Support.MaxX-anchorX))+.7f) : 0f;
            routeWidth = Mathf.Clamp(reach / 0.68f, ResponsiveMinimum, maximumViewWidth) * routeDistanceScale;
            routeY = (target != null ? Mathf.Lerp(anchorY, target.GetTopCenterWorld().y, 0.3f) : anchorY) + verticalFramingOffset;
            framedTarget = index;
        }

        private void UpdateRouteFraming(bool snap)
        {
            if (illustratedPilotFraming && playerTarget != null)
            {
                ApplyIllustratedFrame(snap);
                return;
            }
            if (!closeRouteFraming || playerTarget == null)
            {
                transform.position = new Vector3(transform.position.x, fixedY, fixedZ);
                return;
            }
            var current=FindPlatform(playerTarget.CurrentSupportIndex);
            var traversal=current!=null?current.GetComponent<YieldingWoodFurniture>():null;
            if(traversal!=null&&traversal.IsTraversing)
            {
                routeWidth=ResponsiveMinimum*routeDistanceScale;routeY=traversal.RestTopY+verticalFramingOffset;
                framedTarget=-2;ApplyRouteFrame(snap);return;
            }
            if (guide == null) guide = Object.FindFirstObjectByType<NandaFairyGuide>();
            int index = guide != null ? guide.CurrentTargetPlatformIndex : 0;
            if (framedTarget != index || routeWidth <= 0f) SelectRouteFrame(index);
            ApplyRouteFrame(snap);
        }

        private void ApplyRouteFrame(bool snap)
        {
            float desiredY = playerTarget.IsCinematicFalling || !autoScrollActive
                ? playerTarget.transform.position.y + verticalFramingOffset : routeY;
            float desiredZ = -routeWidth / (2f * Mathf.Tan(fixedFov * Mathf.Deg2Rad * 0.5f) * Aspect);
            float y = snap ? desiredY : Mathf.SmoothDamp(transform.position.y, desiredY, ref framingYVelocity, framingSmoothTime);
            float z = snap ? desiredZ : Mathf.SmoothDamp(transform.position.z, desiredZ, ref framingZVelocity, framingSmoothTime);
            transform.position = new Vector3(transform.position.x, y, z);
            if (snap) framingYVelocity = framingZVelocity = 0f;
        }

        private void ApplyIllustratedFrame(bool snap)
        {
            // A fixed distance preserves the painting's scale. Only X/Y follow the route.
            float depth = illustratedVerticalSpan / (2f * Mathf.Tan(fixedFov * Mathf.Deg2Rad * .5f));
            float direction = Mathf.Sign(playerTarget.RunDirection);
            float width = illustratedVerticalSpan * Aspect;
            var current = FindPlatform(playerTarget.CurrentSupportIndex);
            float groundY = DarkRoomPlatformGeometry.FloorSurfaceY;
            if (current != null && !current.Support.IsEmpty)
            {
                groundY = current.Support.PointNear(playerTarget.transform.position.x).y;
            }
            // The landing belongs to an actual support region, never the volume's center/width.
            int nextIndex = playerTarget.CurrentSupportIndex < 0 ? 0 : playerTarget.CurrentSupportIndex + 1;
            var next = FindPlatform(nextIndex);
            float nextY = next != null && !next.Support.IsEmpty ? next.GetLandingPoint(direction).y : groundY;
            float desiredY = groundY + 1.8f + Mathf.Clamp((nextY - groundY) * .25f, -.5f, .5f);
            // Track the girl's ascent before landing on the next support. This is a
            // vertical translation only: depth, FOV and painting scale stay fixed.
            // The route remains the lower framing bound during an ordinary jump.
            desiredY = Mathf.Max(desiredY, playerTarget.transform.position.y + 1.8f);
            if (playerTarget.IsCinematicFalling || playerTarget.IsDead)
                desiredY = playerTarget.transform.position.y + 1.8f;
            // Compensate the lag of a damped camera following constant-speed travel.
            float anticipation = playerTarget.AutoRunActive && !playerTarget.IsDead
                ? direction * playerTarget.RunSpeed * illustratedFollowTime : 0f;
            var desired = new Vector3(playerTarget.transform.position.x + direction * width * .18f + anticipation, desiredY, -depth);
            var position = snap ? desired : Vector3.SmoothDamp(transform.position, desired, ref illustratedVelocity, illustratedFollowTime);
            position.z = -depth;
            transform.position = position;
            if (snap) illustratedVelocity = Vector3.zero;
        }

        // Also used by editor evidence capture: selects an actual route target, never new geometry.
        public void SnapToRouteFraming(int nextPlatformIndex)
        {
            if (playerTarget == null) return;
            if (illustratedPilotFraming) { ApplyIllustratedFrame(true); return; }
            SelectRouteFrame(nextPlatformIndex);
            ApplyRouteFrame(true);
            float direction = nextPlatformIndex >= 22 ? -1f : 1f;
            transform.position = new Vector3(playerTarget.transform.position.x + direction * routeWidth * 0.18f, transform.position.y, transform.position.z);
        }

        public void BeginCastleRouteOverview()
        {
            var castle = FindPlatform(9);
            var door = FindPlatform(21);
            if (castle == null || door == null) return;
            float left = castle.Support.MinX;
            float right = door.Support.MaxX;
            float width = right - left + 3f;
            float depth = width / (2f * Mathf.Tan(fixedFov * Mathf.Deg2Rad * 0.5f) * Aspect);
            overviewPosition = new Vector3((left + right) * 0.5f, (castle.GetTopCenterWorld().y + door.GetTopCenterWorld().y) * 0.5f + 1f, -depth);
            overviewVelocity = Vector3.zero;
            routeOverview = true;
        }

        public void EndRouteOverview()
        {
            returningFromOverview = routeOverview;
            routeOverview = false;
            framedTarget = -1;
        }

        private void CheckScreenCrush()
        {
            if (!screenCrushActive || playerTarget == null || playerTarget.IsDead) return;

            float camX = transform.position.x;
            float playerX = playerTarget.transform.position.x;
            float margin = closeRouteFraming ? Mathf.Min(maxDistanceBehindCamera, ViewWidth * 0.5f - 0.6f) : maxDistanceBehindCamera;

            if (scrollDirection > 0f)
            {
                // Andando para a direita: se o jogador ficar muito atras da borda esquerda
                if (playerX < camX - margin)
                {
                    playerTarget.TriggerDeath("Yuna foi engolida pela penumbra da tela!");
                }
            }
            else
            {
                // Andando para a esquerda (Volta Caotica): se o jogador ficar para tras na borda direita
                if (playerX > camX + margin)
                {
                    playerTarget.TriggerDeath("Yuna foi engolida pela penumbra da tela!");
                }
            }
        }

        public void ConfigureSection(float targetFixedY, float targetFixedZ = -16.0f, float targetFov = 45.0f)
        {
            fixedY = targetFixedY;
            fixedZ = targetFixedZ;
            fixedFov = targetFov;
            ApplyFixedProperties();
            if (!returningFromOverview)
                transform.position = new Vector3(transform.position.x, fixedY, fixedZ);
        }

        private void ApplyFixedProperties()
        {
            if (targetCamera != null)
            {
                targetCamera.fieldOfView = fixedFov;
            }
            transform.rotation = Quaternion.identity;
        }

        public void SetTarget(Transform newTarget)
        {
            if (newTarget != null)
            {
                playerTarget = newTarget.GetComponent<Yuna25DPlayerController>();
                framedTarget = -1;
                platforms = null;
                guide = null;
            }
        }

        public void SnapToTargetX(float targetX, float offset = 3.5f)
        {
            if (illustratedPilotFraming && playerTarget != null) { ApplyIllustratedFrame(true); return; }
            Vector3 previous = transform.position;
            transform.position = new Vector3(targetX + offset, fixedY, fixedZ);
            if (playerTarget != null && closeRouteFraming)
            {
                UpdateRouteFraming(true);
                transform.position = new Vector3(targetX + Mathf.Sign(offset) * routeWidth * 0.18f, transform.position.y, transform.position.z);
            }
            if (returningFromOverview)
                transform.position = new Vector3(transform.position.x, previous.y, previous.z);
            returningFromOverview = false;
        }

        public void SetScrollState(bool active, float dir = 1f, float speed = 5.2f)
        {
            autoScrollActive = active;
            scrollDirection = Mathf.Sign(dir);
            scrollSpeed = speed;
        }
    }

    /// <summary>Opt-in evidence of vertical following and constant projection; never changes the camera.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class CameraAscentProof : MonoBehaviour
    {
        [System.Serializable] public class Sample
        {
            public int frame, support, screenWidth, screenHeight;
            public bool grounded;
            public float playerY, velocityY, cameraY, cameraZ, fov, pixelsPerMetre, footViewportY;
        }
        [System.Serializable] public class Report
        {
            public string method = "Real player traversal; opt-in read-only camera trace. Fixed Z/FOV and one-metre projection at the character plane, independent of animation pose.";
            public int risingCameraFrames;
            public int capturedFrames, captureWidth, captureHeight;
            public float minZ = float.MaxValue, maxZ = float.MinValue,
                minFov = float.MaxValue, maxFov = float.MinValue,
                minPixelsPerMetre = float.MaxValue, maxPixelsPerMetre = float.MinValue;
            public bool fixedDistanceAndScale;
            public System.Collections.Generic.List<Sample> samples = new System.Collections.Generic.List<Sample>();
        }
        readonly Report report = new Report();
        string output;
        float previousY;
        System.Diagnostics.Process encoder;
        string encoderPath;
        int simulationHz = 60;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs();
            int at = System.Array.IndexOf(args, "-cameraAscentProof");
            if (at < 0 || at + 1 >= args.Length || FindFirstObjectByType<CameraAscentProof>() != null) return;
            var go = new GameObject("CameraAscent_ReadOnlyProof"); DontDestroyOnLoad(go);
            var proof = go.AddComponent<CameraAscentProof>(); proof.output = args[at + 1];
            System.IO.Directory.CreateDirectory(proof.output);
            int videoAt = System.Array.IndexOf(args, "-cameraAscentVideoEncoder");
            int hzAt = System.Array.IndexOf(args, "-previewHz");
            if (hzAt >= 0) proof.simulationHz = int.Parse(args[hzAt + 1]);
            if (videoAt >= 0)
            {
                proof.encoderPath = args[videoAt + 1];
                proof.StartCoroutine(proof.Capture());
            }
        }
        void LateUpdate()
        {
            var camera = Camera.main;
            if (camera == null) return;
            var driver = camera.GetComponent<SideScroll25DCamera>();
            var player = FindFirstObjectByType<Yuna25DPlayerController>();
            if (driver == null || !driver.IllustratedPilotFraming || player == null) return;
            Vector3 p = player.transform.position;
            var s = new Sample { frame = Time.frameCount, support = player.CurrentSupportIndex,
                screenWidth = Screen.width, screenHeight = Screen.height,
                grounded = player.IsGrounded, playerY = p.y, velocityY = player.Velocity.y,
                cameraY = camera.transform.position.y, cameraZ = camera.transform.position.z,
                fov = camera.fieldOfView,
                pixelsPerMetre = camera.WorldToScreenPoint(p + Vector3.up).y - camera.WorldToScreenPoint(p).y,
                footViewportY = camera.WorldToViewportPoint(p).y };
            if (report.samples.Count > 0 && s.velocityY > 1f && s.cameraY > previousY + .00001f)
                report.risingCameraFrames++;
            previousY = s.cameraY; report.samples.Add(s);
            report.minZ = Mathf.Min(report.minZ, s.cameraZ); report.maxZ = Mathf.Max(report.maxZ, s.cameraZ);
            report.minFov = Mathf.Min(report.minFov, s.fov); report.maxFov = Mathf.Max(report.maxFov, s.fov);
            report.minPixelsPerMetre = Mathf.Min(report.minPixelsPerMetre, s.pixelsPerMetre);
            report.maxPixelsPerMetre = Mathf.Max(report.maxPixelsPerMetre, s.pixelsPerMetre);
            if (report.samples.Count % 60 == 0) Save();
        }
        void Save()
        {
            if (output == null) return;
            report.fixedDistanceAndScale = report.samples.Count > 0 && report.maxZ - report.minZ < .0001f
                && report.maxFov - report.minFov < .0001f
                && report.maxPixelsPerMetre - report.minPixelsPerMetre < .01f;
            System.IO.File.WriteAllText(System.IO.Path.Combine(output, "camera-ascent.json"), JsonUtility.ToJson(report, true));
        }
        System.Collections.IEnumerator Capture()
        {
            int frame = 0;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (frame++ % Mathf.Max(1, simulationHz / 30) != 0) continue;
                var image = ScreenCapture.CaptureScreenshotAsTexture();
                if (encoder == null)
                {
                    report.captureWidth = image.width; report.captureHeight = image.height;
                    encoder = new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo {
                        FileName = encoderPath,
                        Arguments = "-y -loglevel error -f rawvideo -pixel_format rgba -video_size "
                            + image.width + "x" + image.height + " -framerate 30 -i pipe:0 -vf vflip -c:v libx264 -preset fast -crf 20 -pix_fmt yuv420p -movflags +faststart \""
                            + System.IO.Path.Combine(output, "camera-ascent.mp4") + "\"",
                        UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true } };
                    encoder.Start();
                }
                if (image.width != report.captureWidth || image.height != report.captureHeight)
                    throw new System.InvalidOperationException("Evidence window resized during camera capture");
                var pixels = image.GetPixels32(); var raw = new byte[pixels.Length * 4];
                for (int i = 0; i < pixels.Length; i++)
                { raw[i*4] = pixels[i].r; raw[i*4+1] = pixels[i].g; raw[i*4+2] = pixels[i].b; raw[i*4+3] = 255; }
                encoder.StandardInput.BaseStream.Write(raw, 0, raw.Length); report.capturedFrames++;
                Destroy(image);
            }
        }
        void OnApplicationQuit()
        {
            if (encoder != null)
            {
                encoder.StandardInput.Close(); encoder.WaitForExit(10000); encoder.Dispose(); encoder = null;
            }
            Save();
        }
    }
}
