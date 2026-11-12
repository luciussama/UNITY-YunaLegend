using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public partial class DarkRoomBlockoutBuilder
    {
        [Header("Decoracao de parede (sem colisao)")]
        [SerializeField] private Material wallDecorWood;
        [SerializeField] private Material wallDecorLilac;
        [SerializeField] private Material wallDecorGlass;
        // Sol, castelo, arco-iris, gato, chapeu, capa e cachecol.
        [SerializeField] private Material[] wallIllustrations;

        public void ConfigureWallDecorations(Material wood, Material lilac, Material glass, Material[] illustrations)
        {
            wallDecorWood = wood;
            wallDecorLilac = lilac;
            wallDecorGlass = glass;
            wallIllustrations = illustrations;
        }

        private Transform WallGroup(string name, Transform parent, Vector3 position)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            return root;
        }

        private void BuildWallDecorations()
        {
            // Decoration stays behind the running lane and away from both window apertures.
            // All pieces use CreateVisualOnlyObject: no physics components or new lights.
            if (wallDecorWood == null || wallIllustrations == null || wallIllustrations.Length != 7)
                return;

            var root = WallGroup("Decoracao_Parede_Infantil", sceneryRoot, Vector3.zero);
            CreateWallShelf(root, "Prateleira_Livros", 21.95f, -0.45f, 0);
            CreateWallShelf(root, "Prateleira_Globo_Neve", 52.95f, 0.15f, 1);
            CreateWallShelf(root, "Prateleira_Barquinho", 79.42f, -0.30f, 2);

            CreateWallPeg(root, "Gancho_Chapeu_Bruxinho", 11.35f, -1.0f, 4, 1.35f, 1.55f);
            CreateWallPeg(root, "Gancho_Capa_Heroi", 47.27f, -1.35f, 5, 1.35f, 2.0f);
            CreateWallPeg(root, "Gancho_Cachecol", 83.2f, -1.1f, 6, 1.05f, 1.85f);

            CreateCrayonFrame(root, "Quadro_Sol_Sorridente", 15.88f, -2.25f, 0, -4f);
            CreateCrayonFrame(root, "Quadro_Castelo", 37.06f, -1.75f, 1, 3f);
            CreateCrayonFrame(root, "Quadro_ArcoIris", 60.51f, -2.30f, 2, -2f);
            CreateCrayonFrame(root, "Quadro_Gatinho", 89.26f, -1.90f, 3, 4f);
        }

        private void CreateWallShelf(Transform parent, string name, float x, float y, int variation)
        {
            var root = WallGroup(name, parent, new Vector3(x, y, 5.65f));
            CreateVisualOnlyObject("Tabua_Carvalho", root, Vector3.zero, new Vector3(3.3f, 0.17f, 0.65f), wallDecorWood, PrimitiveType.Cube);
            CreateVisualOnlyObject("Friso_Frontal", root, new Vector3(0f, -0.03f, -0.34f), new Vector3(3.35f, 0.09f, 0.06f), floorPropCreamMaterial, PrimitiveType.Cube);
            foreach (float side in new[] { -1.15f, 1.15f })
            {
                CreateVisualOnlyObject("Suporte_Vertical", root, new Vector3(side, -0.32f, 0.23f), new Vector3(0.11f, 0.60f, 0.10f), wallDecorWood, PrimitiveType.Cube);
                CreateVisualOnlyObject("Mao_Francesa", root, new Vector3(side, -0.26f, -0.02f), new Vector3(0.10f, 0.67f, 0.10f), wallDecorWood, PrimitiveType.Cube, Quaternion.Euler(44f, 0f, 0f));
            }
            for (int i = 0; i < 3; i++)
            {
                var book = WallGroup("Livrinho_" + i, root, new Vector3(-1.05f + i * 0.24f, 0.47f, -0.05f));
                book.localRotation = Quaternion.Euler(0f, 0f, i == 2 ? 12f : -4f);
                var mat = i == 1 ? floorPropBurgundyMaterial : floorPropBlueMaterial;
                CreateVisualOnlyObject("Capa", book, Vector3.zero, new Vector3(0.20f, 0.74f + i * 0.06f, 0.40f), mat, PrimitiveType.Cube);
                CreateVisualOnlyObject("Lombada_Dourada", book, new Vector3(0f, 0.17f, -0.215f), new Vector3(0.16f, 0.035f, 0.025f), floorPropYellowMaterial, PrimitiveType.Cube);
                CreateVisualOnlyObject("Lombada_Inferior", book, new Vector3(0f, -0.23f, -0.215f), new Vector3(0.16f, 0.035f, 0.025f), floorPropYellowMaterial, PrimitiveType.Cube);
            }

            if (variation != 2)
            {
                var globe = WallGroup("Globo_De_Neve", root, new Vector3(0.65f, 0.10f, -0.05f));
                CreateVisualOnlyObject("Base_Torneada", globe, new Vector3(0f, 0.08f, 0f), new Vector3(0.74f, 0.16f, 0.58f), wallDecorWood, PrimitiveType.Cylinder);
                CreateVisualOnlyObject("Cupula_Vidro_Azulado", globe, new Vector3(0f, 0.51f, 0f), new Vector3(0.82f, 0.82f, 0.70f), wallDecorGlass, PrimitiveType.Sphere);
                CreateVisualOnlyObject("Neve_Base", globe, new Vector3(0f, 0.22f, -0.28f), new Vector3(0.52f, 0.12f, 0.12f), floorPropCreamMaterial, PrimitiveType.Sphere);
                CreateVisualOnlyObject("Boneco_Corpo", globe, new Vector3(0f, 0.42f, -0.32f), new Vector3(0.24f, 0.27f, 0.12f), floorPropCreamMaterial, PrimitiveType.Sphere);
                CreateVisualOnlyObject("Boneco_Cabeca", globe, new Vector3(0f, 0.62f, -0.32f), new Vector3(0.18f, 0.18f, 0.12f), floorPropCreamMaterial, PrimitiveType.Sphere);
                for (int i = 0; i < 7; i++)
                    CreateVisualOnlyObject("Floco_" + i, globe, new Vector3(Mathf.Sin(i * 2.4f) * 0.27f, 0.32f + i * 0.075f, -0.34f), Vector3.one * 0.035f, floorPropCreamMaterial, PrimitiveType.Sphere);
                CreateVisualOnlyObject("Reflexo_Vidro", globe, new Vector3(-0.23f, 0.66f, -0.27f), new Vector3(0.055f, 0.18f, 0.035f), floorPropCreamMaterial, PrimitiveType.Sphere, Quaternion.Euler(0f, 0f, -25f));
            }
            else
            {
                var boat = WallGroup("Barquinho_A_Vela", root, new Vector3(0.55f, 0.16f, -0.05f));
                CreateVisualOnlyObject("Casco", boat, new Vector3(0f, 0.11f, 0f), new Vector3(1.25f, 0.34f, 0.38f), floorPropBlueMaterial, PrimitiveType.Sphere);
                CreateVisualOnlyObject("Borda_Casco", boat, new Vector3(0f, 0.24f, -0.03f), new Vector3(1.18f, 0.08f, 0.35f), wallDecorWood, PrimitiveType.Cube);
                CreateVisualOnlyObject("Mastro", boat, new Vector3(0f, 0.73f, 0f), new Vector3(0.055f, 1.08f, 0.055f), wallDecorWood, PrimitiveType.Cube);
                // A triangular sail assembled from a reusable built-in quad clipped by its texture.
                CreateVisualOnlyObject("Vela_Tecido", boat, new Vector3(0.30f, 0.79f, -0.05f), new Vector3(0.55f, 0.91f, 1f), wallSailMaterial, PrimitiveType.Quad);
                CreateVisualOnlyObject("Bandeirola", boat, new Vector3(-0.13f, 1.25f, 0f), new Vector3(0.29f, 0.12f, 0.045f), floorPropBurgundyMaterial, PrimitiveType.Cube);
            }
        }

        [SerializeField] private Material wallSailMaterial;
        public void ConfigureWallSail(Material material) => wallSailMaterial = material;

        private void CreateWallPeg(Transform parent, string name, float x, float y, int art, float width, float height)
        {
            var root = WallGroup(name, parent, new Vector3(x, y, 5.65f));
            CreateVisualOnlyObject("Roseta_Madeira", root, Vector3.zero, new Vector3(0.30f, 0.30f, 0.10f), wallDecorWood, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Pino", root, new Vector3(0f, -0.06f, -0.14f), new Vector3(0.10f, 0.26f, 0.10f), floorPropYellowMaterial, PrimitiveType.Cylinder, Quaternion.Euler(65f, 0f, 0f));
            CreateVisualOnlyObject("Traje_Pendurado", root, new Vector3(0f, -height * 0.45f, -0.27f), new Vector3(width, height, 1f), wallIllustrations[art], PrimitiveType.Quad);
        }

        private void CreateCrayonFrame(Transform parent, string name, float x, float y, int art, float tilt)
        {
            var root = WallGroup(name, parent, new Vector3(x, y, 5.65f));
            root.localRotation = Quaternion.Euler(0f, 0f, tilt);
            CreateVisualOnlyObject("Fundo_Moldura", root, Vector3.zero, new Vector3(1.88f, 1.58f, 0.10f), wallDecorWood, PrimitiveType.Cube);
            CreateVisualOnlyObject("Passepartout_Lilas", root, new Vector3(0f, 0f, -0.065f), new Vector3(1.72f, 1.42f, 0.025f), wallDecorLilac, PrimitiveType.Cube);
            CreateVisualOnlyObject("Desenho_Giz_De_Cera", root, new Vector3(0f, 0f, -0.09f), new Vector3(1.54f, 1.24f, 1f), wallIllustrations[art], PrimitiveType.Quad);
            foreach (float side in new[] { -1f, 1f })
                CreateVisualOnlyObject("Friso_Lateral", root, new Vector3(side * 0.90f, 0f, -0.08f), new Vector3(0.07f, 1.58f, 0.04f), floorPropCreamMaterial, PrimitiveType.Cube);
        }
    }
}
