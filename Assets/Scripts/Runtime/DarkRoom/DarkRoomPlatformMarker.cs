using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public class DarkRoomPlatformMarker : MonoBehaviour
    {
        public PlatformSpec spec;

        private Collider[] supportColliders;
        private int supportStamp;
        private PlatformSupportProfile support;

        private void OnEnable() => InvalidateSupport();
        private void OnValidate() => InvalidateSupport();

        public void InvalidateSupport()
        {
            supportColliders = null; support = null;
        }

        public PlatformSupportProfile Support
        {
            get
            {
                if (supportColliders == null)
                {
                    var contact = transform.Find("Contacto_Fisico");
                    supportColliders = (contact != null ? contact : transform).GetComponentsInChildren<Collider>(true);
                }
                int stamp = transform.localToWorldMatrix.GetHashCode();
                unchecked
                {
                    foreach (var c in supportColliders)
                    {
                        if (c == null) continue;
                        stamp = stamp*31+c.transform.localToWorldMatrix.GetHashCode();
                        stamp = stamp*31+(c.enabled ? 1 : 0);
                        if (c is BoxCollider box) stamp=stamp*31+box.size.GetHashCode()+box.center.GetHashCode();
                        if (c is MeshCollider mesh && mesh.sharedMesh != null) stamp=stamp*31+mesh.sharedMesh.GetInstanceID();
                    }
                }
                if (support == null || supportStamp != stamp)
                {
                    support = supportColliders.Length > 0
                        ? PlatformSupportProfile.FromColliders(supportColliders)
                        : PlatformSupportProfile.FromShapes(DarkRoomPlatformArt.GetSupportShapes(spec), transform.localToWorldMatrix);
                    supportStamp = stamp;
                }
                return support;
            }
        }

        public Vector3 GetTopCenterWorld() => Support.PointNear(transform.position.x + spec.contact.offsetX);

        public Vector3 GetTakeoffPoint(float direction, float inset)
        {
            var region=spec.index==YieldingWoodFurniture.PlatformIndex
                ? (direction>0f?Support.Regions[Support.Regions.Count-1]:Support.Regions[0])
                : Support.RegionNear(GetTopCenterWorld().x);
            return region.At(direction>0f?Mathf.Max(region.MinX,region.MaxX-inset):Mathf.Min(region.MaxX,region.MinX+inset));
        }

        public Vector3 GetLandingPoint(float direction, float insetFraction = .12f)
        {
            if(spec.index==YieldingWoodFurniture.PlatformIndex)
            {
                var region=direction>0f?Support.Regions[0]:Support.Regions[Support.Regions.Count-1];
                return region.At(direction>0f?region.MinX+Mathf.Min(1f,region.Width*.25f):region.MaxX-Mathf.Min(1f,region.Width*.25f));
            }
            return Support.LandingPoint(transform.position.x+spec.contact.offsetX,direction,insetFraction);
        }

        /// <summary>Rotacao da superficie de pouso (pilhas diagonais inclinam o topo).</summary>
        public Quaternion GetContactRotation()
        {
            float x=GetTopCenterWorld().x;
            foreach(var segment in Support.Segments)
                if(x>=segment.left.x-.0001f&&x<=segment.right.x+.0001f)
                    return Quaternion.FromToRotation(Vector3.up,segment.normal);
            return Quaternion.identity;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = spec.tint;
            foreach (var segment in Support.Segments) Gizmos.DrawLine(segment.left, segment.right);
        }
    }
}
