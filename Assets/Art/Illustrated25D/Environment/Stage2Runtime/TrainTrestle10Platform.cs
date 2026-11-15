using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Plataforma 10 (Fase 2 - A Fuga): Pista Elevada do Trem.
    /// Mantem silhueta propria e apoio real sobre os dormentes/trilhos (LandingPixelY = 522).
    /// </summary>
    public sealed class TrainTrestle10Platform : MonoBehaviour
    {
        public const float FloorY = -9.1f;
        public const float LandingPixelY = 522f;
        public const float FeetPixelY = 807f;
        public const float CentrePixelX = 448f;
        public const float LipLeftPixelX = 22f;
        public const float LipRightPixelX = 881f;
        public const float ContentPixelWidth = 865f;
        public const float TargetHeight = 5.45f; // TopY = -3.65 m (+0.30 m acima do Castelo 09)
        public const float PixelsPerMetre = (FeetPixelY - LandingPixelY) / TargetHeight;
        public const float Height = TargetHeight;
        public const float TopY = FloorY + Height;
        public const float Depth = 2.0f;
        public const float FullWidth = ContentPixelWidth / PixelsPerMetre;
        public const float SupportWidth = (LipRightPixelX - LipLeftPixelX) / PixelsPerMetre;
        public const float SupportOffsetX = ((LipLeftPixelX + LipRightPixelX) * 0.5f - CentrePixelX) / PixelsPerMetre;
        // Castle09 MaxX (~59.955f) + Stage2OpeningGap (2.115f) = 62.07f minX
        public const float TargetX = 62.07f + SupportWidth * 0.5f - SupportOffsetX;

        public BoxCollider Top { get; private set; }
        public SpriteRenderer Painting { get; private set; }

        private Sprite ownedSprite;
        private Material ownedMaterial;

        public void Install(Texture2D texture)
        {
            if (texture == null || texture.width != 896 || texture.height != 1200)
                throw new System.InvalidOperationException("Train Trestle 10 painting dimensions changed.");

            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 10)
                throw new System.InvalidOperationException("Train Trestle 10 requires marker 10.");

            float x = TargetX;
            transform.position = new Vector3(x, FloorY + Height * 0.5f, 0f);
            marker.spec.x = x;
            marker.spec.y = transform.position.y;
            marker.spec.width = FullWidth;
            marker.spec.height = Height;
            marker.spec.depth = Depth;
            marker.spec.contact.offsetX = SupportOffsetX;

            var contact = transform.Find("Contacto_Fisico");
            if (contact == null)
            {
                contact = new GameObject("Contacto_Fisico").transform;
                contact.SetParent(transform, false);
            }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true)) c.enabled = false;

            var surface = new GameObject("Topo_Real_Trem_10");
            surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x + SupportOffsetX, TopY - 0.03f, 0f);
            Top = surface.AddComponent<BoxCollider>();
            Top.size = new Vector3(SupportWidth, 0.06f, Depth);

            var pivot = new Vector2(CentrePixelX / texture.width, (texture.height - FeetPixelY) / texture.height);
            ownedMaterial = new Material(Shader.Find("Sprites/Default"));

            var face = new GameObject("Trem_10_Pintura");
            face.transform.SetParent(transform, false);
            face.transform.position = new Vector3(x, FloorY, 0f);
            ownedSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                pivot,
                PixelsPerMetre,
                0,
                SpriteMeshType.FullRect);

            Painting = face.AddComponent<SpriteRenderer>();
            Painting.sprite = ownedSprite;
            Painting.sharedMaterial = ownedMaterial;
            Painting.sortingOrder = -1;
        }

        private void OnDestroy()
        {
            if (ownedSprite != null) Destroy(ownedSprite);
            if (ownedMaterial != null) Destroy(ownedMaterial);
        }
    }
}
