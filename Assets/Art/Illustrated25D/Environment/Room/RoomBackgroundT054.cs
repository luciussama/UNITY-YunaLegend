using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Camadas pintadas do quarto atras e a frente do percurso. Nao carrega logica de
    /// plataforma nem de personagem: posiciona planos e aplica parallax.
    /// A linha de piso pintada coincide com o piso fisico dos apoios.
    /// </summary>
    [DisallowMultipleComponent]
    public class RoomBackgroundT054 : MonoBehaviour
    {
        public const string FarName = "Camada_Fundo";
        public const string MidName = "Camada_Meio";
        public const string NearName = "Camada_Primeiro_Plano";

        /// <summary>PPU da arte v1 (1536x768). A v2 usa 144 px/m e entra por InstallPanels.</summary>
        public const float PixelsPerUnit = 72f;

        /// <summary>
        /// Linha de piso pintada: 9,375% acima da borda inferior nas artes da T05.4.
        /// Fracao, nao pixels: o importador pode reescalar a textura.
        /// </summary>
        public const float FloorInsetFraction = .09375f;

        // Panorama v4 is 2172x724. The lower edge of the wooden trim is at row
        // 654 (top-origin); contact belongs on the first floorboard, row 660.
        // The former 82/724 inset selected row 642, inside the trim.
        public const float PanoramaFloorInsetFraction = 64f / 724f;

        /// <summary>Fracao do avanco da camera que a camada acompanha. 1 = tela, 0 = mundo.</summary>
        public const float FarTravel = .60f;
        public const float MidTravel = .70f;
        public const float NearTravel = 1f;

        public const int FarOrder = -300;
        public const int MidOrder = -290;
        public const int NearOrder = 40;

        [SerializeField] private List<SpriteRenderer> far = new List<SpriteRenderer>();
        [SerializeField] private List<float> farOffsets = new List<float>();
        [SerializeField] private SpriteRenderer mid;
        [SerializeField] private SpriteRenderer near;
        [SerializeField] private float floorWorldY;
        [SerializeField] private float centreWorldX;
        [SerializeField] private float roomScale = 1f;
        [SerializeField] private float nearScale = 1f;
        [SerializeField] private float floorInset = FloorInsetFraction;
        [SerializeField] private float farTravelFraction = FarTravel;
        public float FarTravelFraction => farTravelFraction;

        public SpriteRenderer Far => far.Count > 0 ? far[0] : null;
        public IReadOnlyList<SpriteRenderer> FarPanels => far;
        public SpriteRenderer Mid => mid;
        public SpriteRenderer Near => near;
        public float FloorWorldY => floorWorldY;
        public float CentreWorldX => centreWorldX;
        public float RoomScale => roomScale;
        public float NearScale => nearScale;

        /// <summary>Uma unica arte de fundo (v1).</summary>
        public void Install(Texture2D farTexture, Texture2D midTexture, Texture2D nearTexture,
            float floorY, float centreX, Material material = null, float worldScale = 1f, float nearWorldScale = 1f)
        {
            InstallPanels(new[] { farTexture }, PixelsPerUnit, midTexture, nearTexture,
                floorY, centreX, material, worldScale, nearWorldScale);
        }

        /// <summary>
        /// Fundo em paineis continuos lado a lado (v2: tres paineis de 13,333 m).
        /// O painel do meio fica centrado no trecho; os outros se alinham pelas bordas.
        /// </summary>
        public void InstallPanels(Texture2D[] panels, float panelPixelsPerUnit, Texture2D midTexture,
            Texture2D nearTexture, float floorY, float centreX, Material material = null,
            float worldScale = 1f, float nearWorldScale = 1f,
            float floorInsetFraction = FloorInsetFraction, float travelFraction = FarTravel)
        {
            floorWorldY = floorY;
            floorInset = floorInsetFraction;
            farTravelFraction = travelFraction;
            centreWorldX = centreX;
            roomScale = Mathf.Max(.05f, worldScale);
            nearScale = Mathf.Max(.05f, nearWorldScale);

            foreach (var existing in far)
                if (existing != null) DestroyImmediate(existing.gameObject);
            far.Clear();
            farOffsets.Clear();

            float panelWidth = panels.Length > 0
                ? panels[0].width / panelPixelsPerUnit * roomScale
                : 0f;
            for (int i = 0; i < panels.Length; i++)
            {
                var renderer = Build(FarName + "_" + i, panels[i], FarOrder + i, material,
                    roomScale, panelPixelsPerUnit);
                float offset = (i - (panels.Length - 1) * .5f) * panelWidth;
                farOffsets.Add(offset);
                far.Add(renderer);
            }

            if (midTexture != null) mid = Build(MidName, midTexture, MidOrder, material, roomScale);
            if (nearTexture != null) near = Build(NearName, nearTexture, NearOrder, material, nearScale);
        }

        /// <summary>Reposiciona as camadas conforme a camera. Chamar depois de mover a camera.</summary>
        public void Follow(Vector3 cameraPosition)
        {
            float shift = (cameraPosition.x - centreWorldX) * farTravelFraction;
            for (int i = 0; i < far.Count; i++)
                PlaceAt(far[i], centreWorldX + farOffsets[i] + shift);
            PlaceAt(mid, centreWorldX + (cameraPosition.x - centreWorldX) * MidTravel);
            PlaceAt(near, centreWorldX + (cameraPosition.x - centreWorldX) * NearTravel);
        }

        private static void PlaceAt(SpriteRenderer layer, float x)
        {
            if (layer == null) return;
            var position = layer.transform.position;
            position.x = x;
            layer.transform.position = position;
        }

        private SpriteRenderer Build(string layerName, Texture2D texture, int order, Material material,
            float scale, float pixelsPerUnit = PixelsPerUnit)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(texture,
                new Rect(0f, 0f, texture.width, texture.height), new Vector2(.5f, .5f), pixelsPerUnit,
                0, SpriteMeshType.FullRect);
            renderer.sortingOrder = order;
            if (material != null) renderer.sharedMaterial = material;
            float height = texture.height / pixelsPerUnit * scale;
            go.transform.localScale = Vector3.one * scale;
            go.transform.position = new Vector3(centreWorldX,
                floorWorldY + height * (.5f - floorInset), 0f);
            return renderer;
        }
    }
}
