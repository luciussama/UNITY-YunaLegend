using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Gavetas Abertas do Armario (07): comoda alta de madeira com gavetas em escadinha.
    /// </summary>
    public sealed class Drawers07Platform : MonoBehaviour
    {
        public const float FloorY = -9.1f;
        public const float LandingPixelY = 175f, FeetPixelY = 1151f;
        public const float CentrePixelX = 467.5f;
        public const float LipLeftPixelX = 160f, LipRightPixelX = 720f;
        public const float ContentPixelWidth = 743f;
        public const float TargetX = 38.80f;
        public const float TargetHeight = 5.19f; // TopY = -3.91 m
        public const float PixelsPerMetre = (FeetPixelY - LandingPixelY) / TargetHeight;
        public const float Height = TargetHeight;
        public const float TopY = FloorY + Height;
        public const float Depth = 1.59f;
        public const float FullWidth = ContentPixelWidth / PixelsPerMetre;
        public const float SupportWidth = (LipRightPixelX - LipLeftPixelX) / PixelsPerMetre;
        public const float SupportOffsetX = (LipLeftPixelX + LipRightPixelX) * .5f - CentrePixelX;

        public BoxCollider Top { get; private set; }
        public SpriteRenderer Painting { get; private set; }
        Sprite ownedSprite;
        Material ownedMaterial;

        public void Install(Texture2D texture)
        {
            if (texture == null || texture.width != 896 || texture.height != 1200)
                throw new System.InvalidOperationException("Drawers 07 painting dimensions changed.");
            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 7)
                throw new System.InvalidOperationException("Drawers 07 requires marker 7");
            float x = TargetX;
            transform.position = new Vector3(x, FloorY + Height * .5f, 0);
            marker.spec.x = x; marker.spec.y = transform.position.y;
            marker.spec.width = FullWidth; marker.spec.height = Height; marker.spec.depth = Depth;
            marker.spec.contact.offsetX = SupportOffsetX / PixelsPerMetre;

            var contact = transform.Find("Contacto_Fisico");
            if (contact == null) { contact = new GameObject("Contacto_Fisico").transform; contact.SetParent(transform, false); }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            var surface = new GameObject("Topo_Real_Gavetas_07"); surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x + marker.spec.contact.offsetX, TopY - .03f, 0);
            Top = surface.AddComponent<BoxCollider>(); Top.size = new Vector3(SupportWidth, .06f, Depth);

            var face = new GameObject("Gavetas_07_Pintura_Uniforme"); face.transform.SetParent(transform, false);
            face.transform.position = new Vector3(x, FloorY, 0);
            ownedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(CentrePixelX / texture.width, (texture.height - FeetPixelY) / texture.height),
                PixelsPerMetre, 0, SpriteMeshType.FullRect);
            ownedMaterial = new Material(Shader.Find("Sprites/Default"));
            Painting = face.AddComponent<SpriteRenderer>(); Painting.sprite = ownedSprite;
            Painting.sharedMaterial = ownedMaterial;
            Painting.sortingOrder = -1;
            marker.InvalidateSupport(); Physics.SyncTransforms();
        }

        void OnDestroy()
        {
            if (ownedSprite != null) Destroy(ownedSprite);
            if (ownedMaterial != null) Destroy(ownedMaterial);
        }
    }
}
