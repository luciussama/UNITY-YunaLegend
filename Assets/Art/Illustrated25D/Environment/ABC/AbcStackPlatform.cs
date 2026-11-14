using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Blocos ABC04: pilha 3-2-1 assentada no piso, com apoio no lip frontal do bloco A.
    /// A arte aprovada define lip, base e largura; a escala e uniforme e o colisor
    /// pertence ao objeto, sem caixa padrao nem alargamento do topo desenhado.
    /// </summary>
    public sealed class AbcStackPlatform : MonoBehaviour
    {
        public const float FloorY = -9.1f;
        // Linhas medidas na pintura aprovada abc-stack-v1.png (1278x1230).
        // Lip = borda visivel do tampo do bloco A, imediatamente acima do quadro
        // frontal; feet = base dos blocos inferiores. A escala uniforme e fixa e a
        // altura do objeto sai da diferenca entre as duas linhas.
        public const float LandingPixelY = 125f, FeetPixelY = 1177f;
        public const float CentrePixelX = 636.5f;
        public const float LipLeftPixelX = 417f, LipRightPixelX = 825f;
        public const float ContentPixelWidth = 1194f;
        public const float BasePixelsPerMetre = 245.1536643f;
        // Reducao de 5% pedida na revisao: a peca fica um pouco menor sem distorcer a pintura.
        public const float PixelsPerMetre = BasePixelsPerMetre * 1.10f;
        public const float Height = (FeetPixelY - LandingPixelY) / PixelsPerMetre;
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
            if (texture == null || texture.width != 1278 || texture.height != 1230)
                throw new System.InvalidOperationException("ABC painting dimensions changed; remeasure lip and base.");
            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 4)
                throw new System.InvalidOperationException("ABC stack requires marker4");
            float x = marker.spec.x;
            transform.position = new Vector3(x, FloorY + Height * .5f, 0);
            marker.spec.x = x; marker.spec.y = transform.position.y;
            marker.spec.width = FullWidth; marker.spec.height = Height; marker.spec.depth = Depth;
            marker.spec.contact.offsetX = SupportOffsetX / PixelsPerMetre;

            var contact = transform.Find("Contacto_Fisico");
            if (contact == null) { contact = new GameObject("Contacto_Fisico").transform; contact.SetParent(transform, false); }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            var surface = new GameObject("Topo_Real_Bloco_A"); surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x + marker.spec.contact.offsetX, TopY - .03f, 0);
            Top = surface.AddComponent<BoxCollider>(); Top.size = new Vector3(SupportWidth, .06f, Depth);

            var face = new GameObject("Blocos_ABC_Pintura_Uniforme"); face.transform.SetParent(transform, false);
            face.transform.position = new Vector3(x, FloorY, 0);
            ownedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(CentrePixelX / texture.width, (texture.height - FeetPixelY) / texture.height),
                PixelsPerMetre, 0, SpriteMeshType.FullRect);
            ownedMaterial = new Material(Shader.Find("Sprites/Default"));
            Painting = face.AddComponent<SpriteRenderer>(); Painting.sprite = ownedSprite;
            Painting.sharedMaterial = ownedMaterial;
            // The idle vignette is a mesh renderer at order 0; the painting must stay
            // behind it so the legs read on top of the deep A block instead of inside it.
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





