using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Caixa de Musica da Bailarina (08): mesa larga de mogno com a caixa de musica
    /// aberta recuada atras da faixa de corrida de Yuna.
    /// </summary>
    public sealed class MusicBox08Platform : MonoBehaviour
    {
        public const float FloorY = -9.1f;
        public const float LandingPixelY = 560f, FeetPixelY = 1095f;
        public const float CentrePixelX = 448f;
        public const float LipLeftPixelX = 75f, LipRightPixelX = 820f;
        public const float ContentPixelWidth = 803f;
        public const float TargetX = 45.96f;
        public const float TargetHeight = 5.55f; // TopY = -3.55 m
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
            if (texture == null || texture.width != 896 || texture.height != 1200)
                throw new System.InvalidOperationException("Music Box 08 painting dimensions changed.");
            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 8)
                throw new System.InvalidOperationException("Music Box 08 requires marker 8");
            float x = TargetX;
            transform.position = new Vector3(x, FloorY + Height * .5f, 0);
            marker.spec.x = x; marker.spec.y = transform.position.y;
            marker.spec.width = FullWidth; marker.spec.height = Height; marker.spec.depth = Depth;
            marker.spec.contact.offsetX = SupportOffsetX / PixelsPerMetre;

            var contact = transform.Find("Contacto_Fisico");
            if (contact == null) { contact = new GameObject("Contacto_Fisico").transform; contact.SetParent(transform, false); }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            var surface = new GameObject("Topo_Real_Caixa_Musica_08"); surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x + marker.spec.contact.offsetX, TopY - .03f, 0);
            Top = surface.AddComponent<BoxCollider>(); Top.size = new Vector3(SupportWidth, .06f, Depth);

            var face = new GameObject("Caixa_Musica_08_Pintura_Uniforme"); face.transform.SetParent(transform, false);
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
