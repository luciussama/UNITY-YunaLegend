using System;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Descritor puro de estágio para cadastrar novas fases e conjuntos de plataformas
    /// orientados a dados sem alterar o Composition Root.
    /// </summary>
    [Serializable]
    public sealed class StageCatalogSpec
    {
        public string stageId;
        public string displayTitle;
        public IllustratedPlatformSpec[] platforms;
    }
}
