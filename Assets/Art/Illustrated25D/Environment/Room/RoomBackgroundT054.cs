using UnityEngine;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Camadas pintadas do quarto atras e a frente do percurso. Nao carrega logica de
    /// plataforma nem de personagem: apenas posiciona os tres planos e aplica parallax.
    /// As linhas de piso das tres artes coincidem com o piso fisico dos apoios.
    /// </summary>
    [DisallowMultipleComponent]
    public class RoomBackgroundT054 : MonoBehaviour
    {
        public const string FarName = "Camada_Fundo_Distante";
        public const string MidName = "Camada_Meio";
        public const string NearName = "Camada_Primeiro_Plano";

        /// <summary>1536 px de largura da arte divididos por 72 px/m = 21,3333 m.</summary>
        public const float PixelsPerUnit = 72f;
        /// <summary>
        /// Linha de piso pintada: 696/768 da altura a partir do topo, ou seja 9,375% acima
        /// da borda inferior. Usar fracao, nao pixels: o importador pode reescalar a textura.
        /// </summary>
        public const float FloorInsetFraction = .09375f;

        public const float FarTravel = .35f;
        public const float MidTravel = .45f;
        public const float NearTravel = 0f;

        public const int FarOrder = -300;
        public const int MidOrder = -290;
        public const int NearOrder = 40;

        [SerializeField] private SpriteRenderer far;
        [SerializeField] private SpriteRenderer mid;
        [SerializeField] private SpriteRenderer near;
        [SerializeField] private float floorWorldY;
        [SerializeField] private float centreWorldX;

        public SpriteRenderer Far => far;
        public SpriteRenderer Mid => mid;
        public SpriteRenderer Near => near;
        public float FloorWorldY => floorWorldY;
        public float CentreWorldX => centreWorldX;

        /// <summary>
        /// Monta as tres camadas em memoria. floorY e o y fisico onde os apoios encostam;
        /// centreX e o meio horizontal do trecho percorrido.
        /// </summary>
        public void Install(Texture2D farTexture, Texture2D midTexture, Texture2D nearTexture,
            float floorY, float centreX, Material material = null)
        {
            floorWorldY = floorY;
            centreWorldX = centreX;
            far = Build(FarName, farTexture, FarOrder, material);
            mid = Build(MidName, midTexture, MidOrder, material);
            near = Build(NearName, nearTexture, NearOrder, material);
        }

        /// <summary>Deslocamento relativo ao centro do trecho, como fracao do avanco da camera.</summary>
        public void Follow(Vector3 cameraPosition)
        {
            Place(far, cameraPosition.x, FarTravel);
            Place(mid, cameraPosition.x, MidTravel);
            Place(near, cameraPosition.x, NearTravel);
        }

        private void Place(SpriteRenderer layer, float cameraX, float travel)
        {
            if (layer == null) return;
            var position = layer.transform.position;
            position.x = centreWorldX + (cameraX - centreWorldX) * travel;
            layer.transform.position = position;
        }

        private SpriteRenderer Build(string layerName, Texture2D texture, int order, Material material)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(texture,
                new Rect(0f, 0f, texture.width, texture.height), new Vector2(.5f, .5f), PixelsPerUnit,
                0, SpriteMeshType.FullRect);
            renderer.sortingOrder = order;
            if (material != null) renderer.sharedMaterial = material;
            float height = texture.height / PixelsPerUnit;
            go.transform.position = new Vector3(centreWorldX,
                floorWorldY + height * (.5f - FloorInsetFraction), 0f);
            return renderer;
        }
    }
}
