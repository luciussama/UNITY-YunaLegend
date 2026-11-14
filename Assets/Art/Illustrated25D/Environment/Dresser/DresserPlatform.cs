using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>Authored front landing rim and floor anchor, independent of transparent padding.</summary>
    public sealed class DresserPlatform : MonoBehaviour
    {
        public const float FloorY = -9.1f, TopY = -6.05f, Height = 3.05f;
        public const float SupportWidth = 2.5f, Depth = 1.749f, Gap = 1.35f;
        public const float LandingPixelY = 130.5f, FeetPixelY = 1183.5f, CentrePixelX = 621.5f;
        public const float PixelsPerMetre = (FeetPixelY - LandingPixelY) / Height;
        public BoxCollider Top { get; private set; }
        public SpriteRenderer Painting { get; private set; }
        Sprite ownedSprite;
        Material ownedMaterial;

        public void Install(Texture2D texture, DarkRoomPlatformMarker previous)
        {
            if (texture == null || texture.width != 1254 || texture.height != 1254)
                throw new System.InvalidOperationException("Dresser painting dimensions changed; remeasure its anchors.");
            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 3) throw new System.InvalidOperationException("Dresser requires marker3");
            float x = previous.Support.MaxX + Gap + SupportWidth * .5f;
            transform.position = new Vector3(x, FloorY + Height * .5f, 0);
            marker.spec.x = x; marker.spec.y = transform.position.y;
            marker.spec.width = 1003f / PixelsPerMetre; marker.spec.height = Height;
            marker.spec.depth = Depth;
            var contact = transform.Find("Contacto_Fisico");
            if (contact == null) { contact = new GameObject("Contacto_Fisico").transform; contact.SetParent(transform, false); }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            var surface = new GameObject("Tampo_Real_Comoda"); surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x, TopY - .03f, 0);
            Top = surface.AddComponent<BoxCollider>(); Top.size = new Vector3(SupportWidth, .06f, Depth);
            var face = new GameObject("Comoda_Pintura_Uniforme"); face.transform.SetParent(transform, false);
            face.transform.position = new Vector3(x, FloorY, 0);
            ownedSprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height),
                new Vector2(CentrePixelX/texture.width, (texture.height-FeetPixelY)/texture.height),
                PixelsPerMetre, 0, SpriteMeshType.FullRect);
            ownedMaterial = new Material(Shader.Find("Sprites/Default"));
            Painting = face.AddComponent<SpriteRenderer>(); Painting.sprite = ownedSprite;
            Painting.sharedMaterial = ownedMaterial; Painting.sortingOrder = 20;
            marker.InvalidateSupport(); Physics.SyncTransforms();
        }
        void OnDestroy()
        {
            if (ownedSprite != null) Destroy(ownedSprite);
            if (ownedMaterial != null) Destroy(ownedMaterial);
        }
    }
}
