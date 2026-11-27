using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.DreamForest.Core;
using YunaLegend.DreamForestEntrance.Core;

namespace YunaLegend.DreamForestEntrance.Runtime
{
    /// <summary>Native Unity presentation of the entrance only; the existing simulation owns all clocks.</summary>
    public sealed class DreamEntranceView : MonoBehaviour
    {
        public event Action AdvanceRequested;
        public event Action PauseRequested;
        public event Action MuteRequested;
        public event Action<string> StageRequested;

        public Camera RenderCamera { get; private set; }
        public bool Visible => root != null && root.activeSelf;
        private GameObject root;
        private Camera paintingCamera;
        private Canvas paintingCanvas;
        private RenderTexture paintingTarget;
        private RawImage paintingOutput;
        private RawImage forest;
        private RawImage shade;
        private RawImage fairy;
        private RawImage girl;
        private RawImage shadow;
        private RawImage fade;
        private GameObject card;
        private Text heading;
        private readonly List<Text> lines = new List<Text>();
        private Text autoHint;
        private Text inputHint;
        private RawImage pauseIcon;
        private RawImage muteIcon;
        private RawImage pausedShade;
        private EntrancePhaseMenuView phaseMenu;
        private bool wasPaused;
        private float pauseStart;
        private Font caveat;
        private Font sans;
        private Material actorMaterial;
        private readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private double dialogueBottom;
        private int previousWidth;
        private int previousHeight;
        private float cardWidth;
        private float inset;
        private float textSize;
        private float lineHeight;

        public void Build()
        {
            caveat = Resources.Load<Font>("DreamForestEntrance/Caveat600");
            sans = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            actorMaterial = new Material(Resources.Load<Shader>("DreamForestEntrance/LegacyActor"))
            {
                hideFlags=HideFlags.HideAndDontSave
            };
            foreach (string asset in new[] { "Forest", "Yuna0", "Yuna1", "Yuna2", "Yuna3", "Look0", "NandaSleeping", "ContactShadow", "Control", "FlagControl", "PauseControl", "PauseIcon", "PlayIcon", "SoundIcon", "MuteIcon" })
            {
                textures.Add(asset, Resources.Load<Texture2D>("DreamForestEntrance/"+asset));
            }

            root = new GameObject("DreamEntrancePresentation");
            root.transform.SetParent(transform, false);
            var cameraObject = new GameObject("EntranceCamera", typeof(Camera));
            cameraObject.transform.SetParent(root.transform, false);
            RenderCamera = cameraObject.GetComponent<Camera>();
            RenderCamera.clearFlags = CameraClearFlags.Depth;
            RenderCamera.orthographic = true;
            RenderCamera.orthographicSize = 5;
            RenderCamera.nearClipPlane = 0.1f;
            RenderCamera.farClipPlane = 10;
            RenderCamera.cullingMask = 1 << 30;
            RenderCamera.depth = 100;
            RenderCamera.allowHDR = false;
            RenderCamera.allowMSAA = false;
            cameraObject.transform.position = new Vector3(0,0,-5);

            var paintingObject=new GameObject("EntrancePaintingCamera",typeof(Camera));
            paintingObject.transform.SetParent(root.transform,false);
            paintingCamera=paintingObject.GetComponent<Camera>();
            paintingCamera.CopyFrom(RenderCamera);
            paintingCamera.depth=99;
            paintingCamera.clearFlags=CameraClearFlags.SolidColor;
            paintingCamera.backgroundColor=Color.black;
            paintingObject.transform.position=new Vector3(0,0,-5);

            var canvasGo = new GameObject("EntranceCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(root.transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            paintingCanvas=canvas;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = paintingCamera;
            canvas.planeDistance = 1;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = 80;
            forest = Image("Forest",canvasGo.transform,textures["Forest"]);
            shade = Image("ForestShade",canvasGo.transform,null);
            shadow = Image("NandaContactShadow",canvasGo.transform,textures["ContactShadow"]);
            fairy = Image("NandaSleeping",canvasGo.transform,textures["NandaSleeping"]);
            fairy.material = actorMaterial;
            girl = Image("YunaEntrance",canvasGo.transform,textures["Yuna3"]);
            girl.material = actorMaterial;
            card = new GameObject("EntranceNarration",typeof(RectTransform),typeof(EntrancePaperGraphic));
            card.transform.SetParent(canvasGo.transform,false);
            var paper = card.GetComponent<EntrancePaperGraphic>();
            paper.raycastTarget = true;
            heading = Label("EntranceSpeaker",card.transform,caveat,21,new Color(132/255f,100/255f,103/255f));
            for (int i=0; i<8; i++)
            {
                lines.Add(Label("EntranceLine"+i,card.transform,caveat,25,new Color(81/255f,62/255f,56/255f)));
            }
            autoHint = Label("LegacyAutoReading",card.transform,sans,11,new Color(132/255f,100/255f,103/255f));
            autoHint.fontStyle = FontStyle.Bold;
            autoHint.text = "Avança sozinho";
            inputHint = Label("LegacyAdvanceReading",card.transform,sans,11,new Color(132/255f,100/255f,103/255f));
            inputHint.text = "Clique/toque ou Enter adianta";
            fade = Image("DepartureFade",canvasGo.transform,null);
            pausedShade = Image("EntrancePauseVeil",canvasGo.transform,null);
            pausedShade.color = new Color(2/255f,5/255f,14/255f,.5f);
            Stretch(pausedShade.rectTransform);

            // A modal entrance consumes a gesture anywhere, without producing gameplay input.
            var outputCanvasGo=new GameObject("EntranceOutputCanvas",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
            outputCanvasGo.transform.SetParent(root.transform,false);
            var outputCanvas=outputCanvasGo.GetComponent<Canvas>();
            outputCanvas.renderMode=RenderMode.ScreenSpaceCamera;
            outputCanvas.worldCamera=RenderCamera;
            outputCanvas.planeDistance=1;
            outputCanvas.sortingOrder=81;
            paintingOutput=Image("Authored540Canvas",outputCanvasGo.transform,null);
            Stretch(paintingOutput.rectTransform);

            var advance = Image("EntranceAdvanceSurface",outputCanvasGo.transform,null);
            advance.color = Color.clear;
            advance.raycastTarget = true;
            Stretch(advance.rectTransform);
            var advanceSurface = advance.gameObject.AddComponent<EntranceAdvanceSurface>();
            advanceSurface.Advance=()=>
            {
                if (!phaseMenu.Close()) AdvanceRequested?.Invoke();
            };
            var phaseMenuGo = new GameObject("EntrancePhaseMenu",typeof(RectTransform),typeof(EntrancePhaseMenuView));
            phaseMenuGo.transform.SetParent(outputCanvasGo.transform,false);
            Stretch(phaseMenuGo.GetComponent<RectTransform>());
            phaseMenu = phaseMenuGo.GetComponent<EntrancePhaseMenuView>();
            phaseMenu.Build(sans,scene=>StageRequested?.Invoke(scene));
            var phaseControl = CreateControl(outputCanvasGo.transform,"Phases",118,null,()=>phaseMenu.Toggle());
            phaseControl.transform.parent.GetComponent<RawImage>().texture = textures["FlagControl"];
            var flagObject = new GameObject("PhaseFlag",typeof(RectTransform),typeof(EntranceFlagGraphic));
            flagObject.transform.SetParent(phaseControl.transform,false);
            Stretch(flagObject.GetComponent<RectTransform>());
            var flag = flagObject.GetComponent<EntranceFlagGraphic>();
            flag.raycastTarget = false;
            pauseIcon = CreateControl(outputCanvasGo.transform,"Pause",66,textures["PauseIcon"],()=>PauseRequested?.Invoke());
            muteIcon = CreateControl(outputCanvasGo.transform,"Sound",14,textures["SoundIcon"],()=>MuteRequested?.Invoke());
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = 30;
            }
            foreach (Transform child in outputCanvasGo.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer=29;
            }
            RenderCamera.cullingMask=1<<29;
        }

        public void Apply(DreamForestSimulation sim, bool paused, bool muted, int width = 0, int height = 0)
        {
            bool opening = sim.Stage == DreamForestStage.Opening && sim.Opening != null;
            root.SetActive(opening);
            if (!opening)
            {
                return;
            }
            if (width <= 0) width = Screen.width;
            if (height <= 0) height = Screen.height;
            if (width != previousWidth || height != previousHeight)
            {
                SetRasterTarget(width,height);
                MeasureLayout(width,height);
            }
            var o = sim.Opening;
            var composition = DreamEntranceComposition.Evaluate(o.Index,o.Age,o.Time,width,height,dialogueBottom);
            PlacePainting(forest,0,0,1536,1024,composition,width,height);
            Stretch(shade.rectTransform);
            shade.color = new Color(2/255f,10/255f,24/255f,(float)composition.ForestShade);
            ApplyActor(fairy,composition.Nanda,composition,width,height);
            ApplyActor(girl,composition.Yuna,composition,width,height);
            PlacePainting(shadow,720-577*composition.Nanda.Scale*.44-8,900,
                577*composition.Nanda.Scale*.88+16,40*composition.ActorFactor,composition,width,height);
            shadow.rectTransform.SetSiblingIndex(fairy.rectTransform.GetSiblingIndex()-1);
            fade.color = new Color(5/255f,13/255f,30/255f,(float)composition.Fade);
            Stretch(fade.rectTransform);
            ApplyDialogue(sim,width,height);
            pauseIcon.texture = textures[paused ? "PlayIcon" : "PauseIcon"];
            muteIcon.texture = textures[muted ? "MuteIcon" : "SoundIcon"];
            pausedShade.gameObject.SetActive(paused);
            if (paused && !wasPaused) pauseStart = Time.unscaledTime;
            wasPaused = paused;
            var pauseButton = pauseIcon.transform.parent.GetComponent<RawImage>();
            pauseButton.texture = textures[paused ? "PauseControl" : "Control"];
            float pulse = paused ? 1.04f-.04f*Mathf.Cos((Time.unscaledTime-pauseStart)*Mathf.PI*2/1.8f) : 1;
            pauseButton.transform.localScale = Vector3.one*pulse;
        }

        public bool ClosePhaseMenu()
        {
            return phaseMenu.Close();
        }

        private void SetRasterTarget(int width,int height)
        {
            if(paintingTarget!=null)
            {
                paintingTarget.Release();
                DestroyOwned(paintingTarget);
            }
            var raster=new DreamEntranceRasterProfile(width,height);
            paintingTarget=new RenderTexture(raster.Width,raster.Height,24,RenderTextureFormat.ARGB32)
            {
                name="DreamEntranceAuthoredCanvas",
                hideFlags=HideFlags.HideAndDontSave,
                filterMode=FilterMode.Bilinear,
                wrapMode=TextureWrapMode.Clamp
            };
            paintingTarget.Create();
            paintingCamera.targetTexture=paintingTarget;
            paintingOutput.texture=paintingTarget;
            paintingCanvas.scaleFactor=raster.Height/(float)height;
            Canvas.ForceUpdateCanvases();
        }

        public void RenderPainting()
        {
            paintingCamera.Render();
        }

        private void MeasureLayout(int width, int height)
        {
            previousWidth = width;
            previousHeight = height;
            inset = 68;
            bool low = width>height && height<480;
            float scale = low ? .8f : 1;
            cardWidth = Mathf.Min(680*scale,width-32*scale);
            textSize = Mathf.Min(25*scale,width*.061f);
            lineHeight = textSize*1.22f;
            int maxLines = 0;
            foreach (var beat in DreamForestLevelData.OpeningBeats)
            {
                if (!string.IsNullOrEmpty(beat.Text))
                {
                    maxLines = Mathf.Max(maxLines,Wrap(beat.Text,cardWidth-56*scale).Count);
                }
            }
            dialogueBottom = inset+16*scale+maxLines*lineHeight+68*scale+48;
        }

        private void ApplyDialogue(DreamForestSimulation sim, int width, int height)
        {
            var beat = DreamForestLevelData.OpeningBeats[sim.Opening.Index];
            bool visible = !string.IsNullOrEmpty(beat.Text) && !sim.Opening.Dismissed;
            card.SetActive(visible);
            if (!visible) return;
            float scale = width>height && height<480 ? .8f : 1;
            var wrapped = Wrap(beat.Text,cardWidth-56*scale);
            float cardHeight = wrapped.Count*lineHeight+68*scale+30;
            float top = inset+16*scale;
            SetRect(card.GetComponent<RectTransform>(),new Vector2(.5f,1),new Vector2(.5f,1),0,-top,cardWidth,cardHeight);
            heading.text = beat.Speaker;
            heading.fontSize = Mathf.RoundToInt(Mathf.Min(21*scale,textSize));
            SetLine(heading,25*scale,cardWidth,40);
            for (int i=0; i<lines.Count; i++)
            {
                lines[i].gameObject.SetActive(i<wrapped.Count);
                if (i>=wrapped.Count) continue;
                lines[i].fontSize = Mathf.RoundToInt(textSize);
                lines[i].text = wrapped[i];
                SetLine(lines[i],49*scale+lineHeight*(i+.5f),cardWidth-40*scale,lineHeight+12);
            }
            SetLine(autoHint,cardHeight-27,cardWidth-36,18);
            SetLine(inputHint,cardHeight-14,cardWidth-36,18);
        }

        private List<string> Wrap(string text, float maxWidth)
        {
            var result = new List<string>();
            var measure = lines[0];
            measure.fontSize = Mathf.RoundToInt(textSize);
            foreach (string paragraph in text.Split('\n'))
            {
                string line = "";
                foreach (string word in paragraph.Split(' '))
                {
                    string next = string.IsNullOrEmpty(line) ? word : line+" "+word;
                    measure.text = next;
                    if (line.Length>0 && measure.preferredWidth>maxWidth)
                    {
                        result.Add(line);
                        line = word;
                    }
                    else line = next;
                }
                result.Add(line);
            }
            return result;
        }

        private void ApplyActor(RawImage image, EntranceActorPose pose, DreamEntranceComposition composition, int width, int height)
        {
            var texture = textures[pose.Asset];
            image.texture = texture;
            var rt = image.rectTransform;
            rt.pivot = new Vector2((float)(pose.PivotX/texture.width),1-(float)(pose.PivotY/texture.height));
            rt.anchorMin = rt.anchorMax = new Vector2(0,1);
            rt.anchoredPosition = new Vector2((float)(width/2.0+(pose.X-composition.FocusX)*composition.Scale),
                -(float)(height/2.0+(pose.Y-composition.FocusY)*composition.Scale));
            rt.sizeDelta = new Vector2(texture.width,texture.height);
            rt.localScale = new Vector3((float)(pose.Scale*composition.Scale)*(pose.Flip?-1:1),
                (float)(pose.Scale*composition.Scale*pose.Stretch),1);
            rt.localRotation = Quaternion.Euler(0,0,-(float)(pose.Rotation*180/Math.PI));
        }

        private static void PlacePainting(RawImage image, double x, double y, double w, double h,
            DreamEntranceComposition composition, int width, int height)
        {
            SetRect(image.rectTransform,new Vector2(0,1),new Vector2(0,1),
                (float)(width/2.0+(x-composition.FocusX)*composition.Scale),
                -(float)(height/2.0+(y-composition.FocusY)*composition.Scale),
                (float)(w*composition.Scale),(float)(h*composition.Scale));
        }

        private static RawImage Image(string name, Transform parent, Texture texture)
        {
            var go = new GameObject(name,typeof(RectTransform),typeof(RawImage));
            go.transform.SetParent(parent,false);
            var image = go.GetComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            return image;
        }

        private static Text Label(string name, Transform parent, Font font, int size, Color color)
        {
            var go = new GameObject(name,typeof(RectTransform),typeof(Text));
            go.transform.SetParent(parent,false);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private RawImage CreateControl(Transform parent, string name, float right, Texture iconTexture, Action action)
        {
            var image = Image(name,parent,textures["Control"]);
            image.raycastTarget = true;
            SetRect(image.rectTransform,new Vector2(1,1),new Vector2(1,1),-right,-14,42,42);
            var button = image.gameObject.AddComponent<Button>();
            button.onClick.AddListener(()=>action());
            var colors = button.colors;
            colors.highlightedColor = new Color(1.35f,1.35f,1.35f,1);
            button.colors = colors;
            var icon = Image(name+"Icon",image.transform,iconTexture);
            if (iconTexture == null) icon.color = Color.clear;
            SetRect(icon.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),0,0,20,20);
            return icon;
        }

        private static void SetLine(Text line, float centerY, float width, float height)
        {
            SetRect(line.rectTransform,new Vector2(.5f,1),new Vector2(.5f,.5f),0,-centerY,width,height);
        }

        private static void SetRect(RectTransform rt, Vector2 anchor, Vector2 pivot, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = new Vector2(x,y);
            rt.sizeDelta = new Vector2(w,h);
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (actorMaterial != null) DestroyOwned(actorMaterial);
            if(paintingTarget!=null)
            {
                paintingTarget.Release();
                DestroyOwned(paintingTarget);
            }
        }

        private static void DestroyOwned(UnityEngine.Object owned)
        {
            if(Application.isPlaying) Destroy(owned);
            else DestroyImmediate(owned);
        }
    }
}
