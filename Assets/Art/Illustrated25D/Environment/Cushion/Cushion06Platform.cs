using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Almofadao de Veludo Roxo (06): almofada capitone sobre suporte alto de madeira.
    /// </summary>
    public sealed class Cushion06Platform : MonoBehaviour
    {
        public const float FloorY = -9.1f;
        public const float LandingPixelY = 300f, FeetPixelY = 1125f;
        public const float CentrePixelX = 448f;
        public const float LipLeftPixelX = 150f, LipRightPixelX = 750f;
        public const float ContentPixelWidth = 752f;
        public const float TargetX = 33.80f;
        public const float TargetHeight = 4.87f; // TopY = -4.23 m
        public const float PixelsPerMetre = (FeetPixelY - LandingPixelY) / TargetHeight;
        public const float Height = TargetHeight;
        public const float TopY = FloorY + Height;
        public const float Depth = 2.07f;
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
                throw new System.InvalidOperationException("Cushion 06 painting dimensions changed.");
            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 6)
                throw new System.InvalidOperationException("Cushion 06 requires marker 6");
            float x = TargetX;
            transform.position = new Vector3(x, FloorY + Height * .5f, 0);
            marker.spec.x = x; marker.spec.y = transform.position.y;
            marker.spec.width = FullWidth; marker.spec.height = Height; marker.spec.depth = Depth;
            marker.spec.contact.offsetX = SupportOffsetX / PixelsPerMetre;

            var contact = transform.Find("Contacto_Fisico");
            if (contact == null) { contact = new GameObject("Contacto_Fisico").transform; contact.SetParent(transform, false); }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            var surface = new GameObject("Topo_Real_Almofadao_06"); surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x + marker.spec.contact.offsetX, TopY - .03f, 0);
            Top = surface.AddComponent<BoxCollider>(); Top.size = new Vector3(SupportWidth, .06f, Depth);

            var face = new GameObject("Almofadao_06_Pintura_Uniforme"); face.transform.SetParent(transform, false);
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
