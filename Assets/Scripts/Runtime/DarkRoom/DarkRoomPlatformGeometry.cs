using UnityEngine;

namespace YunaLegend.DarkRoom
{
    /// <summary>
    /// Natureza da superficie de pouso do objeto de cenografia.
    /// Serve para documentar o que a plataforma oferece como apoio e orientar a modelagem
    /// incremental (a superficie visivel precisa coincidir com a face superior do colisor).
    /// </summary>
    public enum PlatformSurfaceKind
    {
        /// <summary>Pelucia/almofada: massa macia, topo amplo e levemente abaulado.</summary>
        SoftCushion,
        /// <summary>Tampo rigido plano: moveis, livros empilhados, tabuleiros.</summary>
        FlatPad,
        /// <summary>Volume de madeira/papel com arestas: blocos, castelinho, gavetas.</summary>
        WrappedBox,
        /// <summary>Topo circular: tambor, globo, piao, lustre.</summary>
        RoundTop
    }

    /// <summary>
    /// Dimensoes canonicas de uma plataforma em metros da Unity, derivadas do
    /// <see cref="DarkRoomPlatformCatalog"/> (que guarda as medidas em pixels de design).
    /// </summary>
    public readonly struct PlatformGeometry
    {
        public readonly int Index;
        public readonly float Width;
        public readonly float Height;
        public readonly float Depth;
        public readonly PlatformSurfaceKind Surface;
        /// <summary>Chave do modelo dedicado; vazio enquanto o objeto permanece no blockout provisorio.</summary>
        public readonly string ArtKey;

        public PlatformGeometry(int index, float width, float height, float depth, PlatformSurfaceKind surface, string artKey)
        {
            Index = index;
            Width = width;
            Height = height;
            Depth = depth;
            Surface = surface;
            ArtKey = artKey;
        }

        public bool HasModel => !string.IsNullOrEmpty(ArtKey);
    }

    /// <summary>
    /// Fonte unica das medidas por plataforma.
    ///
    /// O catalogo de design registra largura/profundidade/altura em pixels do roteiro canonico.
    /// A regua da skill audiovisual e a altura de Yuna: 44 px = 1,40 m, portanto 1 px = 0,0318 m.
    /// Antes desta tabela, o gerador do circuito aplicava 2,6 m x 0,7 m a praticamente todas as
    /// plataformas, o que achatava objetos de proporcoes muito diferentes (urso, livros, castelinho).
    /// </summary>
    public static class DarkRoomPlatformGeometry
    {
        private static readonly System.Collections.Generic.Dictionary<string,PlatformSupportProfile> SupportCache =
            new System.Collections.Generic.Dictionary<string,PlatformSupportProfile>();

        public static PlatformSupportProfile GetSupportProfile(PlatformSpec spec)
        {
            string key=$"{spec.index}|{spec.width:R}|{spec.height:R}|{spec.depth:R}|{spec.contact.offsetX:R}|{spec.contact.offsetY:R}|{spec.contact.tiltDegrees:R}|{spec.contact.width:R}|{spec.contact.depth:R}";
            if(!SupportCache.TryGetValue(key,out var local))
            {
                local=PlatformSupportProfile.FromShapes(DarkRoomPlatformArt.GetSupportShapes(spec),Matrix4x4.identity);
                SupportCache[key]=local;
            }
            return local.Translated(new Vector3(spec.x,spec.y,0f));
        }
        /// <summary>44 px de Yuna = 1,40 m (regua antropometrica da skill de paridade).</summary>
        public const float MetersPerDesignPixel = 0.0318f;

        /// <summary>Face superior do piso jogavel: BuildFloor usa centro -9,6 m e espessura 1,0 m.</summary>
        public const float FloorSurfaceY = -9.1f;

        /// <summary>Chave do modelo da cabeca de urso de pelucia (plataforma 00).</summary>
        public const string TeddyHeadArtKey = "teddy-head";

        /// <summary>Chave do modelo da pilha diagonal de livros (plataforma 01).</summary>
        public const string BookStackArtKey = "book-stack";
        public const string YieldingFurnitureArtKey = "yielding-furniture";

        public static bool TryGet(int index, out PlatformGeometry geometry)
        {
            if (TryFindSpec(index, out var spec))
            {
                float width = PixelsToMeters(spec.width);
                float height = PixelsToMeters(spec.height);
                float depth = PixelsToMeters(spec.depth);

                // A plataforma 00 segue a arte conceitual aprovada: cabeca mais baixa e larga
                // sobre uma pe cilindrica de madeira. A altura do catalogo (40 px) deixava o
                // conjunto alto demais para a proporcao desenhada.
                if (index == 0) height = ConceptTeddyHeight;
                if (index == 2) { width = 8.4f; height = 2.3f; depth = 2.4f; } // User-authored long traversal furniture.

                geometry = new PlatformGeometry(
                    index,
                    width,
                    height,
                    depth,
                    ResolveSurface(index),
                    ResolveArtKey(index));
                return true;
            }

            geometry = default;
            return false;
        }

        /// <summary>Altura total (pelucia + pe de madeira) da plataforma 00, medida na arte conceitual.</summary>
        public const float ConceptTeddyHeight = 1.10f;

        public static PlatformGeometry Get(int index)
        {
            return TryGet(index, out var geometry) ? geometry : default;
        }

        /// <summary>
        /// Superficie real de pouso. A maioria dos objetos usa o topo da propria caixa,
        /// mas pilhas e pecas inclinadas declaram aqui o seu tampo verdadeiro — o colisor,
        /// o salto e a camera passam a seguir o objeto, e nao o contrario.
        /// </summary>
        public static PlatformContactSurface GetContact(int index)
        {
            switch (index)
            {
                case 1:
                    // Pilha diagonal de livros: o tampo e a capa do livro de cima, deslocada
                    // para a direita e inclinada no mesmo angulo da pilha.
                    return new PlatformContactSurface
                    {
                        offsetX = 0.24f,
                        offsetY = 0f,
                        tiltDegrees = -4f,
                        width = 1.85f,
                        depth = 1.15f
                    };
                default:
                    return default;
            }
        }

        public static float PixelsToMeters(float designPixels)
        {
            return Mathf.Round(designPixels * MetersPerDesignPixel * 100f) / 100f;
        }

        private static bool TryFindSpec(int index, out PlatformSpec spec)
        {
            foreach (var candidate in DarkRoomPlatformCatalog.Phase1Platforms)
                if (candidate.index == index) { spec = candidate; return true; }
            foreach (var candidate in DarkRoomPlatformCatalog.Phase2Platforms)
                if (candidate.index == index) { spec = candidate; return true; }
            foreach (var candidate in DarkRoomPlatformCatalog.Phase3Platforms)
                if (candidate.index == index) { spec = candidate; return true; }

            spec = default;
            return false;
        }

        private static PlatformSurfaceKind ResolveSurface(int index)
        {
            switch (index)
            {
                case 0: return PlatformSurfaceKind.SoftCushion;  // cabeca do urso de pelucia
                case 1: return PlatformSurfaceKind.FlatPad;      // pilha de livros
                case 2: return PlatformSurfaceKind.FlatPad;      // penteadeira
                case 3: return PlatformSurfaceKind.FlatPad;      // comoda
                case 4: return PlatformSurfaceKind.WrappedBox;   // blocos ABC
                case 5: return PlatformSurfaceKind.RoundTop;     // tamborito
                case 6: return PlatformSurfaceKind.SoftCushion;  // almofadao de veludo
                case 7: return PlatformSurfaceKind.WrappedBox;   // gaveta aberta
                case 8: return PlatformSurfaceKind.FlatPad;      // caixa de musica
                case 9: return PlatformSurfaceKind.WrappedBox;   // castelinho de blocos
                case 12: return PlatformSurfaceKind.RoundTop;    // abajur cogumelo
                case 14: return PlatformSurfaceKind.RoundTop;    // globo terrestre
                case 15: return PlatformSurfaceKind.FlatPad;     // pipa encantada
                case 16: return PlatformSurfaceKind.FlatPad;     // livro de gravuras
                case 17: return PlatformSurfaceKind.RoundTop;    // lustre de cristais
                case 18: return PlatformSurfaceKind.RoundTop;    // varao de cortina
                case 26: return PlatformSurfaceKind.RoundTop;    // tamborzinho amassado
                case 33: return PlatformSurfaceKind.RoundTop;    // piao de madeira
                case 34: return PlatformSurfaceKind.RoundTop;    // carretel
                default: return PlatformSurfaceKind.WrappedBox;
            }
        }

        private static string ResolveArtKey(int index)
        {
            switch (index)
            {
                case 0: return TeddyHeadArtKey;
                case 1: return BookStackArtKey;
                case 2: return YieldingFurnitureArtKey;
                default: return string.Empty;
            }
        }
    }
}
