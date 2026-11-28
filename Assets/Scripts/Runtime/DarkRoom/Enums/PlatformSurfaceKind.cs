using UnityEngine;
using YunaLegend.Illustrated25D.Environment;

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
}
