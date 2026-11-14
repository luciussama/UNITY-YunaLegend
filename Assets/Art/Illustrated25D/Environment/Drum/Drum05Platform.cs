using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Tamborito de Marcha (05): tambor cilindrico assentado no piso, com superficie de
    /// corrida e pouso sobre a pele plana superior (acima do aro frontal).
    /// </summary>
    public sealed class Drum05Platform : MonoBehaviour
    {
        public const float FloorY = -9.1f;
        // Medicoes na pintura aprovada drum-05-v1.png (1024x1024).
        // LandingPixelY fica na faixa plana da pele do tambor, acima do aro frontal (y~360).
        public const float LandingPixelY = 265f, FeetPixelY = 930f;
        public const float CentrePixelX = 510.5f;
        public const float LipLeftPixelX = 215f, LipRightPixelX = 815f;
        public const float ContentPixelWidth = 750f;
        public const float TargetX = 28.25f;
        public const float TargetHeight = 4.55f; // TopY = -4.55 m (marcador base 05)
        public const float PixelsPerMetre = (FeetPixelY - LandingPixelY) / TargetHeight;
        public const float Height = TargetHeight;
        public const float TopY = FloorY + Height;
        public const float Depth = 2.0f;
        public const float FullWidth = ContentPixelWidth / PixelsPerMetre;
        public const float SupportWidth = (LipRightPixelX - LipLeftPixelX) / PixelsPerMetre;
        public const float SupportOffsetX = (LipLeftPixelX + LipRightPixelX) * .5f - CentrePixelX;

        public BoxCollider Top { get; private set; }
        public SpriteRenderer Painting { get; private set; }
        Sprite ownedSprite;
        Material ownedMaterial;

        public void Install(Texture2D texture)
        {
            if (texture == null || texture.width != 1024 || texture.height != 1024)
                throw new System.InvalidOperationException("Drum painting dimensions changed; remeasure drumhead and base.");
            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 5)
                throw new System.InvalidOperationException("Drum 05 requires marker 5");
            float x = TargetX;
            transform.position = new Vector3(x, FloorY + Height * .5f, 0);
            marker.spec.x = x; marker.spec.y = transform.position.y;
            marker.spec.width = FullWidth; marker.spec.height = Height; marker.spec.depth = Depth;
            marker.spec.contact.offsetX = SupportOffsetX / PixelsPerMetre;

            var contact = transform.Find("Contacto_Fisico");
            if (contact == null) { contact = new GameObject("Contacto_Fisico").transform; contact.SetParent(transform, false); }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            var surface = new GameObject("Topo_Real_Tambor_05"); surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x + marker.spec.contact.offsetX, TopY - .03f, 0);
            Top = surface.AddComponent<BoxCollider>(); Top.size = new Vector3(SupportWidth, .06f, Depth);

            var face = new GameObject("Tambor_05_Pintura_Uniforme"); face.transform.SetParent(transform, false);
            face.transform.position = new Vector3(x, FloorY, 0);
            ownedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(CentrePixelX / texture.width, (texture.height - FeetPixelY) / texture.height),
                PixelsPerMetre, 0, SpriteMeshType.FullRect);
            ownedMaterial = new Material(Shader.Find("Sprites/Default"));
            Painting = face.AddComponent<SpriteRenderer>(); Painting.sprite = ownedSprite;
            Painting.sharedMaterial = ownedMaterial;
            // Keep painting behind Yuna's renderers (order 0) so feet read cleanly on top of the drumhead.
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
