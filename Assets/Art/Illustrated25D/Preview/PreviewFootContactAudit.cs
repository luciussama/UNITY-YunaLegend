using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>Read-only opt-in audit of the painted shoe against actual 3D support.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class PreviewFootContactAudit : MonoBehaviour
    {
        [Serializable] public class Sample
        {
            public int frame, support, pose;
            public string source, jumpState, stumbleState, image;
            public bool expectedContact, soleSupported, anchorSupported;
            public float rootGap, soleGap, anchorGap, screenGap, x, soleX, soleY, supportY, skinWidth, sag;
        }
        [Serializable] public class Audit
        {
            public string method = "60Hz real controller; lowest red shoe pixel alpha>=128, original texture; deformed mesh UV interpolation for Idle. Positive gap means above real collider. Foot outside support is reported separately. No gameplay/pose correction.";
            public List<Sample> samples = new List<Sample>();
        }
        readonly Audit report = new Audit();
        readonly Dictionary<Texture2D, Color32[]> pixels = new Dictionary<Texture2D, Color32[]>();
        readonly Dictionary<Sprite, Vector2> shoe = new Dictionary<Sprite, Vector2>();
        readonly HashSet<string> captured = new HashSet<string>();
        IllustratedPreview preview;
        string output;
        int captures;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-previewFootAudit");
            if (at < 0 || at + 1 >= args.Length || FindFirstObjectByType<PreviewFootContactAudit>() != null) return;
            var go = new GameObject("ReadOnly_FootContactAudit");
            DontDestroyOnLoad(go);
            var audit = go.AddComponent<PreviewFootContactAudit>();
            audit.output = args[at + 1]; Directory.CreateDirectory(audit.output);
        }

        void LateUpdate()
        {
            if (preview == null) preview = FindFirstObjectByType<IllustratedPreview>();
            if (preview == null || !preview.Ready || !preview.Player.IsGrounded) return;
            var p = preview.Player;
            var r = p.GetComponent<YunaIllustrationVisibilityResolver>();
            string source = r.VisibleSource;
            int pose = source == "Run" ? r.run.CurrentFrame : source == "Jump" ? r.jump.Pose : source == "Stumble" ? r.stumble.Pose : 0;
            Sprite sprite;
            Transform visual;
            bool flip;
            Vector3 anchor;
            if (source == "Idle")
            {
                sprite = r.idle.skin.sprite; visual = r.idle.transform; flip = false;
                anchor = visual.TransformPoint(Vector3.zero);
            }
            else
            {
                var renderer = source == "Run" ? r.run.painting : source == "Jump" ? r.jump.painting : r.stumble.painting;
                sprite = renderer.sprite; visual = renderer.transform; flip = renderer.flipX;
                anchor = source == "Run" ? new Vector3(r.run.FootWorldX, r.run.FootWorldY, 0)
                    : source == "Jump" ? new Vector3(r.jump.ContactX, r.jump.ContactY, 0)
                    : new Vector3(r.stumble.ContactX, r.stumble.ContactY, 0);
            }
            Vector2 pixel = Shoe(sprite);
            Vector3 local = new Vector3((pixel.x - sprite.pivot.x) / sprite.pixelsPerUnit,
                (pixel.y - sprite.pivot.y) / sprite.pixelsPerUnit, 0);
            if (source == "Idle") local = MeshPoint(r.idle.skin.GetComponent<MeshFilter>().sharedMesh,
                new Vector2((sprite.rect.x + pixel.x) / sprite.texture.width, (sprite.rect.y + pixel.y) / sprite.texture.height));
            else if (flip) local.x = -local.x;
            Vector3 sole = visual.TransformPoint(local);
            var s = new Sample { frame = Time.frameCount, support = p.CurrentSupportIndex, source = source, pose = pose,
                jumpState = r.jump.State, stumbleState = r.stumble.State, x = p.transform.position.x,
                soleX = sole.x, soleY = sole.y, sag = preview.Furniture.CurrentSag,
                skinWidth = p.GetComponent<CharacterController>().skinWidth,
                expectedContact = source != "Run" || r.run.ContactPose };
            RaycastHit hit;
            s.soleSupported = Support(sole, p, out hit);
            if (s.soleSupported)
            {
                s.supportY = hit.point.y; s.soleGap = sole.y - hit.point.y;
                s.screenGap = Camera.main.WorldToScreenPoint(sole).y - Camera.main.WorldToScreenPoint(hit.point).y;
            }
            s.anchorSupported = Support(anchor, p, out hit);
            if (s.anchorSupported) s.anchorGap = anchor.y - hit.point.y;
            if (Support(p.transform.position, p, out hit)) s.rootGap = p.transform.position.y - hit.point.y;
            string key = s.support + "_" + source + "_" + pose;
            if (captured.Add(key) && captures < 48)
            {
                s.image = "frame-" + s.frame + "_" + key + ".png"; captures++;
                StartCoroutine(Shot(s.image));
            }
            report.samples.Add(s);
            if (report.samples.Count % 60 == 0) Save();
        }

        bool Support(Vector3 point, Yuna25DPlayerController p, out RaycastHit hit)
        {
            hit = default; float nearest = float.MaxValue;
            foreach (var h in Physics.RaycastAll(point + Vector3.up * .5f, Vector3.down, 1.5f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.transform.IsChildOf(p.transform) || h.normal.y < .5f) continue;
                var marker = h.collider.GetComponentInParent<DarkRoomPlatformMarker>();
                if (p.CurrentSupportIndex >= 0 && (marker == null || marker.spec.index != p.CurrentSupportIndex)) continue;
                if (h.distance < nearest) { hit = h; nearest = h.distance; }
            }
            return nearest < float.MaxValue;
        }

        Vector2 Shoe(Sprite sprite)
        {
            if (shoe.TryGetValue(sprite, out var found)) return found;
            var texture = sprite.texture;
            if (!pixels.TryGetValue(texture, out var data))
            {
                var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
                var previous = RenderTexture.active;
                Graphics.Blit(texture, rt); RenderTexture.active = rt;
                var copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0); copy.Apply();
                data = copy.GetPixels32(); pixels.Add(texture, data);
                Destroy(copy); RenderTexture.active = previous; RenderTexture.ReleaseTemporary(rt);
            }
            var rect = sprite.rect;
            int bottom = -1, count = 0; float sum = 0;
            for (int y = 0; y < rect.height * .35f; y++)
            {
                for (int x = 0; x < rect.width; x++)
                {
                    var c = data[((int)rect.y + y) * texture.width + (int)rect.x + x];
                    if (c.a < 128 || c.r < c.g * 1.15f || c.r < c.b * 1.1f) continue;
                    if (bottom < 0) bottom = y;
                    if (y <= bottom + 2) { sum += x + .5f; count++; }
                }
                if (bottom >= 0 && y > bottom + 2) break;
            }
            if (count == 0) throw new InvalidOperationException("No opaque red shoe found: " + sprite.name);
            found = new Vector2(sum / count, bottom + .5f); shoe.Add(sprite, found); return found;
        }

        static Vector3 MeshPoint(Mesh mesh, Vector2 uv)
        {
            var v = mesh.vertices; var u = mesh.uv; var t = mesh.triangles;
            for (int i = 0; i < t.Length; i += 3)
            {
                int a = t[i], b = t[i + 1], c = t[i + 2];
                Vector2 e = u[b] - u[a], f = u[c] - u[a], q = uv - u[a];
                float d = e.x * f.y - e.y * f.x;
                if (Mathf.Abs(d) < 1e-10f) continue;
                float wb = (q.x * f.y - q.y * f.x) / d, wc = (e.x * q.y - e.y * q.x) / d;
                if (wb >= -.0001f && wc >= -.0001f && wb + wc <= 1.0001f)
                    return v[a] * (1 - wb - wc) + v[b] * wb + v[c] * wc;
            }
            throw new InvalidOperationException("Shoe UV outside deformed mesh");
        }
        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(output, name), texture.EncodeToPNG()); Destroy(texture);
        }
        void Save() { if (output != null) File.WriteAllText(Path.Combine(output, "foot-contact.json"), JsonUtility.ToJson(report, true)); }
        void OnApplicationQuit() => Save();
    }
}
