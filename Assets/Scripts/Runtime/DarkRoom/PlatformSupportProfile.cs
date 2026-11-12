using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    /// <summary>A physical shape authored by the object, shared with its visual mesh.</summary>
    public struct PlatformSupportShape
    {
        public string name;
        public Mesh mesh;
        public Vector3 position, scale, boxSize;
        public Quaternion rotation;
        public Matrix4x4 Matrix => Matrix4x4.TRS(position, rotation, scale);

        public static PlatformSupportShape Box(Vector3 position, Quaternion rotation, Vector3 size) =>
            new PlatformSupportShape { name = "Surface", position = position, rotation = rotation, scale = Vector3.one, boxSize = size };

        public static PlatformSupportShape MeshShape(string name, Mesh mesh, Vector3 position, Vector3 scale) =>
            new PlatformSupportShape { name = name, mesh = mesh, position = position, rotation = Quaternion.identity, scale = scale };
    }

    public readonly struct PlatformSupportSegment
    {
        public readonly Vector3 left, right, normal;
        public PlatformSupportSegment(Vector3 a, Vector3 b, Vector3 normal)
        {
            left = a.x <= b.x ? a : b;
            right = a.x <= b.x ? b : a;
            this.normal = normal;
        }
        public float Width => right.x - left.x;
        public Vector3 At(float x) => Vector3.Lerp(left, right, Mathf.InverseLerp(left.x, right.x, x));
    }

    public sealed class PlatformSupportRegion
    {
        public readonly List<PlatformSupportSegment> segments = new List<PlatformSupportSegment>();
        public float MinX => segments[0].left.x;
        public float MaxX => segments[segments.Count - 1].right.x;
        public float Width => MaxX - MinX;
        public Vector3 At(float x)
        {
            foreach (var s in segments) if (x <= s.right.x + .0001f) return s.At(x);
            return segments[segments.Count - 1].right;
        }
    }

    /// <summary>
    /// Exposed support in the runner's Z plane. Curves are actual mesh triangles;
    /// gaps remain gaps and hidden lower faces cannot become phantom landings.
    /// </summary>
    public sealed class PlatformSupportProfile
    {
        public const float MinimumLandingNormalY = .5f;
        const float Epsilon = .0001f;
        readonly List<PlatformSupportSegment> segments = new List<PlatformSupportSegment>();
        readonly List<PlatformSupportRegion> regions = new List<PlatformSupportRegion>();
        public IReadOnlyList<PlatformSupportSegment> Segments => segments;
        public IReadOnlyList<PlatformSupportRegion> Regions => regions;
        public bool IsEmpty => segments.Count == 0;
        public float MinX => IsEmpty ? 0f : segments[0].left.x;
        public float MaxX => IsEmpty ? 0f : segments[segments.Count - 1].right.x;
        public float MinY { get; private set; }
        public float MaxY { get; private set; }

        public bool TryGetPoint(float x, out Vector3 point)
        {
            foreach (var s in segments)
                if (x >= s.left.x - Epsilon && x <= s.right.x + Epsilon) { point = s.At(x); return true; }
            point = default; return false;
        }

        public PlatformSupportRegion RegionNear(float x)
        {
            PlatformSupportRegion best = null; float distance = float.MaxValue;
            foreach (var region in regions)
            {
                float d = Mathf.Abs(x - Mathf.Clamp(x, region.MinX, region.MaxX));
                if (d < distance) { distance = d; best = region; }
            }
            return best;
        }

        public Vector3 PointNear(float x)
        {
            var region = RegionNear(x);
            return region != null ? region.At(Mathf.Clamp(x, region.MinX, region.MaxX)) : Vector3.zero;
        }

        public Vector3 LandingPoint(float referenceX, float direction, float insetFraction = .12f)
        {
            var region = RegionNear(referenceX);
            if (region == null) return Vector3.zero;
            float x = Mathf.Clamp(referenceX - direction * region.Width * insetFraction,
                region.MinX + region.Width * .1f, region.MaxX - region.Width * .1f);
            return region.At(x);
        }

        public static float Gap(PlatformSupportProfile from, PlatformSupportProfile to, float direction) =>
            direction > 0f ? to.MinX - from.MaxX : from.MinX - to.MaxX;

        public PlatformSupportProfile Translated(Vector3 offset)
        {
            var result=new PlatformSupportProfile();
            foreach(var s in segments) result.segments.Add(new PlatformSupportSegment(s.left+offset,s.right+offset,s.normal));
            result.BuildRegions();return result;
        }

        public static PlatformSupportProfile FromShapes(IEnumerable<PlatformSupportShape> shapes, Matrix4x4 root, float planeZ = 0f)
        {
            var raw = new List<PlatformSupportSegment>();
            foreach (var shape in shapes) AddShape(raw, shape, root * shape.Matrix, planeZ);
            return UpperEnvelope(raw);
        }

        public static PlatformSupportProfile FromColliders(IEnumerable<Collider> colliders, float planeZ = 0f)
        {
            var raw = new List<PlatformSupportSegment>();
            foreach (var collider in colliders)
            {
                if (collider == null || !collider.enabled || collider.isTrigger) continue;
                if (collider is BoxCollider box)
                    AddShape(raw, PlatformSupportShape.Box(Vector3.zero, Quaternion.identity, box.size),
                        box.transform.localToWorldMatrix * Matrix4x4.Translate(box.center), planeZ);
                else if (collider is MeshCollider mesh && mesh.sharedMesh != null)
                    AddShape(raw, PlatformSupportShape.MeshShape(mesh.name, mesh.sharedMesh, Vector3.zero, Vector3.one),
                        mesh.transform.localToWorldMatrix, planeZ);
            }
            return UpperEnvelope(raw);
        }

        static void AddShape(List<PlatformSupportSegment> result, PlatformSupportShape shape, Matrix4x4 matrix, float z)
        {
            if (shape.mesh == null)
            {
                var half = shape.boxSize * .5f;
                Vector3 a = matrix.MultiplyPoint3x4(new Vector3(-half.x, half.y, -half.z));
                Vector3 b = matrix.MultiplyPoint3x4(new Vector3(-half.x, half.y, half.z));
                Vector3 c = matrix.MultiplyPoint3x4(new Vector3(half.x, half.y, half.z));
                Vector3 d = matrix.MultiplyPoint3x4(new Vector3(half.x, half.y, -half.z));
                AddTriangle(result, a, b, c, z); AddTriangle(result, a, c, d, z);
                return;
            }
            var vertices = shape.mesh.vertices; var triangles = shape.mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
                AddTriangle(result, matrix.MultiplyPoint3x4(vertices[triangles[i]]),
                    matrix.MultiplyPoint3x4(vertices[triangles[i+1]]), matrix.MultiplyPoint3x4(vertices[triangles[i+2]]), z);
        }

        static void AddTriangle(List<PlatformSupportSegment> result, Vector3 a, Vector3 b, Vector3 c, float z)
        {
            var normal = Vector3.Cross(b-a, c-a).normalized;
            if (normal.y <= Epsilon) return;
            var points = new List<Vector3>(3);
            Intersect(a,b,z,points); Intersect(b,c,z,points); Intersect(c,a,z,points);
            if (points.Count < 2) return;
            Vector3 left = points[0], right = points[0];
            foreach (var p in points) { if (p.x < left.x) left=p; if (p.x > right.x) right=p; }
            if (right.x-left.x > Epsilon) result.Add(new PlatformSupportSegment(left,right,normal));
        }

        static void Intersect(Vector3 a, Vector3 b, float z, List<Vector3> points)
        {
            if (Mathf.Abs(a.z-z) < Epsilon) points.Add(new Vector3(a.x,a.y,z));
            if ((a.z-z)*(b.z-z) < 0f)
                points.Add(Vector3.Lerp(a,b,(z-a.z)/(b.z-a.z)));
        }

        static PlatformSupportProfile UpperEnvelope(List<PlatformSupportSegment> raw)
        {
            var result = new PlatformSupportProfile();
            var cuts = new List<float>();
            foreach (var s in raw) { cuts.Add(s.left.x); cuts.Add(s.right.x); }
            for(int i=0;i<raw.Count;i++) for(int j=i+1;j<raw.Count;j++)
            {
                float lo=Mathf.Max(raw[i].left.x,raw[j].left.x), hi=Mathf.Min(raw[i].right.x,raw[j].right.x);
                if(hi-lo<=Epsilon) continue;
                float d0=raw[i].At(lo).y-raw[j].At(lo).y, d1=raw[i].At(hi).y-raw[j].At(hi).y;
                if(d0*d1<0f)cuts.Add(Mathf.Lerp(lo,hi,d0/(d0-d1)));
            }
            cuts.Sort();
            for(int i=0;i<cuts.Count-1;i++)
            {
                float lo=cuts[i],hi=cuts[i+1];if(hi-lo<=Epsilon)continue;
                float x=(lo+hi)*.5f, top=float.NegativeInfinity;
                PlatformSupportSegment selected=default;bool found=false;
                foreach(var s in raw)
                    if(x>=s.left.x-Epsilon&&x<=s.right.x+Epsilon&&s.At(x).y>top)
                    {top=s.At(x).y;selected=s;found=true;}
                if(!found||selected.normal.y<=MinimumLandingNormalY)continue;
                result.segments.Add(new PlatformSupportSegment(selected.At(lo),selected.At(hi),selected.normal));
            }
            result.BuildRegions();
            return result;
        }

        void BuildRegions()
        {
            MinY=float.PositiveInfinity; MaxY=float.NegativeInfinity;
            PlatformSupportRegion region=null;
            foreach(var s in segments)
            {
                if(region==null||Vector3.Distance(region.segments[region.segments.Count-1].right,s.left)>.002f)
                {region=new PlatformSupportRegion();regions.Add(region);}
                region.segments.Add(s);
                MinY=Mathf.Min(MinY,s.left.y,s.right.y);
                MaxY=Mathf.Max(MaxY,s.left.y,s.right.y);
            }
            if(IsEmpty)MinY=MaxY=0f;
        }
    }
}
