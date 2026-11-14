using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Castelinho de Blocos (09): deck de madeira atras e torres frontais azuis em
    /// camada a frente de Yuna (sortingOrder = 25) para oclusao 2.5D na passagem.
    /// </summary>
    public sealed class Castle09Platform : MonoBehaviour
    {
        public const float FloorY = -9.1f;
        public const float LandingPixelY = 695f, FeetPixelY = 1124f;
        public const float CentrePixelX = 448f;
        public const float LipLeftPixelX = 140f, LipRightPixelX = 755f;
        public const float ContentPixelWidth = 782f;
        public const float TargetX = 56.27f; // Vao de tensao ~2.75 m apos 08
        public const float TargetHeight = 5.15f; // TopY = -3.95 m
        public const float PixelsPerMetre = (FeetPixelY - LandingPixelY) / TargetHeight;
        public const float Height = TargetHeight;
        public const float TopY = FloorY + Height;
        public const float Depth = 2.0f;
        public const float FullWidth = ContentPixelWidth / PixelsPerMetre;
        public const float SupportWidth = (LipRightPixelX - LipLeftPixelX) / PixelsPerMetre;
        public const float SupportOffsetX = (LipLeftPixelX + LipRightPixelX) * .5f - CentrePixelX;

        public BoxCollider Top { get; private set; }
        public SpriteRenderer Painting { get; private set; }
        public SpriteRenderer FrontTowers { get; private set; }
        Sprite ownedSprite, ownedFrontSprite;
        Material ownedMaterial;

        public void Install(Texture2D texture, Texture2D frontTowersTexture)
        {
            if (texture == null || texture.width != 896 || texture.height != 1200)
                throw new System.InvalidOperationException("Castle 09 painting dimensions changed.");
            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 9)
                throw new System.InvalidOperationException("Castle 09 requires marker 9");
            float x = TargetX;
            transform.position = new Vector3(x, FloorY + Height * .5f, 0);
            marker.spec.x = x; marker.spec.y = transform.position.y;
            marker.spec.width = FullWidth; marker.spec.height = Height; marker.spec.depth = Depth;
            marker.spec.contact.offsetX = SupportOffsetX / PixelsPerMetre;

            var contact = transform.Find("Contacto_Fisico");
            if (contact == null) { contact = new GameObject("Contacto_Fisico").transform; contact.SetParent(transform, false); }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            var surface = new GameObject("Topo_Real_Castelo_09"); surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x + marker.spec.contact.offsetX, TopY - .03f, 0);
            Top = surface.AddComponent<BoxCollider>(); Top.size = new Vector3(SupportWidth, .06f, Depth);

            var pivot = new Vector2(CentrePixelX / texture.width, (texture.height - FeetPixelY) / texture.height);
            ownedMaterial = new Material(Shader.Find("Sprites/Default"));

            var face = new GameObject("Castelo_09_Pintura_Fundo"); face.transform.SetParent(transform, false);
            face.transform.position = new Vector3(x, FloorY, 0);
            ownedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                pivot, PixelsPerMetre, 0, SpriteMeshType.FullRect);
            Painting = face.AddComponent<SpriteRenderer>(); Painting.sprite = ownedSprite;
            Painting.sharedMaterial = ownedMaterial;
            Painting.sortingOrder = -1;

            if (frontTowersTexture != null)
            {
                var front = new GameObject("Castelo_09_Torres_Frente"); front.transform.SetParent(transform, false);
                front.transform.position = new Vector3(x, FloorY, 0);
                ownedFrontSprite = Sprite.Create(frontTowersTexture,
                    new Rect(0, 0, frontTowersTexture.width, frontTowersTexture.height),
                    pivot, PixelsPerMetre, 0, SpriteMeshType.FullRect);
                FrontTowers = front.AddComponent<SpriteRenderer>(); FrontTowers.sprite = ownedFrontSprite;
                FrontTowers.sharedMaterial = ownedMaterial;
                FrontTowers.sortingOrder = 25;
            }

            marker.InvalidateSupport(); Physics.SyncTransforms();
        }

        void OnDestroy()
        {
            if (ownedSprite != null) Destroy(ownedSprite);
            if (ownedFrontSprite != null) Destroy(ownedFrontSprite);
            if (ownedMaterial != null) Destroy(ownedMaterial);
        }
    }
}
