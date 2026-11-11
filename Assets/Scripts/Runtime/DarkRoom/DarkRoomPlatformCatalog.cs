using System;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public enum DarkRoomPhase
    {
        Phase1Ascent,
        Phase2Escape,
        Phase3ChaoticAscent
    }

    [Serializable]
    public struct PlatformContactSurface
    {
        /// <summary>Deslocamento do centro da superficie de pouso em relacao ao centro do volume.</summary>
        public float offsetX;
        public float offsetY;
        /// <summary>Inclinacao do topo em graus no eixo Z (objetos empilhados na diagonal).</summary>
        public float tiltDegrees;
        /// <summary>Extensao util de pouso; 0 usa a largura do volume como fallback.</summary>
        public float width;
        public float depth;

        public bool IsCustom =>
            Mathf.Abs(offsetX) > 0.0001f || Mathf.Abs(offsetY) > 0.0001f ||
            Mathf.Abs(tiltDegrees) > 0.01f || width > 0.0001f || depth > 0.0001f;

        public float EffectiveWidth(float fallback) => width > 0.001f ? width : fallback;
        public float EffectiveDepth(float fallback) => depth > 0.001f ? depth : fallback;
    }

    [Serializable]
    public struct PlatformSpec
    {
        public int index;
        public string name;
        public DarkRoomPhase phase;
        public float x;
        public float y;
        public float width;
        public float depth;
        public float height;
        public Color tint;
        public string visualDescription;
        /// <summary>Superficie real de pouso quando o objeto nao e uma caixa.</summary>
        public PlatformContactSurface contact;

        public PlatformSpec(int index, string name, DarkRoomPhase phase, float x, float y, float width,
            float depth, float height, Color tint, string visualDescription)
        {
            this.index = index;
            this.name = name;
            this.phase = phase;
            this.x = x;
            this.y = y;
            this.width = width;
            this.depth = depth;
            this.height = height;
            this.tint = tint;
            this.visualDescription = visualDescription;
            this.contact = default;
        }

        public Vector3 TopSurfaceCenter => new Vector3(x, y, 0f);
    }

    public static class DarkRoomPlatformCatalog
    {
        // 10 plataformas da Fase 1: Subida ao Castelo (0 a 9)
        public static readonly PlatformSpec[] Phase1Platforms = new[]
        {
            new PlatformSpec(0, "Cabeca do Urso de Pelucia", DarkRoomPhase.Phase1Ascent, 165f, 440f, 78f, 70f, 40f, new Color(0.76f, 0.60f, 0.42f), "Urso de pelucia bege macio sentado no chao"),
            new PlatformSpec(1, "Pilha de Livros Ilustrados", DarkRoomPhase.Phase1Ascent, 280f, 398f, 88f, 50f, 35f, new Color(0.20f, 0.35f, 0.60f), "Tres grossos livros de capa dura coloridos"),
            new PlatformSpec(2, "Penteadeira Encantada", DarkRoomPhase.Phase1Ascent, 410f, 350f, 100f, 60f, 50f, new Color(0.85f, 0.78f, 0.68f), "Penteadeira classica de madeira clara com espelho oval"),
            new PlatformSpec(3, "Comoda Pequena de Madeira", DarkRoomPhase.Phase1Ascent, 548f, 302f, 92f, 55f, 45f, new Color(0.48f, 0.30f, 0.16f), "Criado-mudo de carvalho com puxadores torneados"),
            new PlatformSpec(4, "Pilha de Blocos ABC", DarkRoomPhase.Phase1Ascent, 678f, 258f, 84f, 50f, 40f, new Color(0.85f, 0.28f, 0.22f), "Cubos infantis de madeira pintada A, B, C"),
            new PlatformSpec(5, "Tamborito de Marcha", DarkRoomPhase.Phase1Ascent, 800f, 310f, 85f, 85f, 50f, new Color(0.80f, 0.15f, 0.15f), "Tambor cilindrico vermelho e branco com cordoes"),
            new PlatformSpec(6, "Almofadao de Veludo", DarkRoomPhase.Phase1Ascent, 922f, 262f, 92f, 65f, 30f, new Color(0.55f, 0.18f, 0.50f), "Grande almofada redonda de veludo roxo com botao central"),
            new PlatformSpec(7, "Gavetas da Comoda", DarkRoomPhase.Phase1Ascent, 1052f, 215f, 90f, 50f, 25f, new Color(0.58f, 0.38f, 0.22f), "Gaveta de madeira puxada de armario alto"),
            new PlatformSpec(8, "Caixa de Musica da Bailarina", DarkRoomPhase.Phase1Ascent, 1180f, 175f, 95f, 55f, 35f, new Color(0.82f, 0.65f, 0.70f), "Caixa aberta em madeira rosada e engrenagens douradas"),
            new PlatformSpec(9, "Castelinho de Blocos", DarkRoomPhase.Phase1Ascent, 1320f, 225f, 145f, 80f, 60f, new Color(0.25f, 0.45f, 0.75f), "Fortaleza com blocos de madeira e torres azuis pontiagudas")
        };

        // 12 plataformas da Fase 2: A Fuga (10 a 21)
        public static readonly PlatformSpec[] Phase2Platforms = new[]
        {
            new PlatformSpec(10, "Pista Elevada do Trem", DarkRoomPhase.Phase2Escape, 1520f, 195f, 105f, 40f, 20f, new Color(0.70f, 0.45f, 0.25f), "Viaduto curvo de trilhos de madeira"),
            new PlatformSpec(11, "Prateleira de Brinquedos", DarkRoomPhase.Phase2Escape, 1685f, 160f, 110f, 45f, 22f, new Color(0.60f, 0.40f, 0.22f), "Prateleira presa a parede com maos-francesas"),
            new PlatformSpec(12, "Abajur Cogumelo", DarkRoomPhase.Phase2Escape, 1855f, 220f, 95f, 70f, 45f, new Color(0.90f, 0.25f, 0.20f), "Abajur vermelho com pintinhas brancas e luz quente"),
            new PlatformSpec(13, "Telhado da Casa de Bonecas", DarkRoomPhase.Phase2Escape, 2015f, 170f, 115f, 60f, 40f, new Color(0.85f, 0.50f, 0.65f), "Telhas cor-de-rosa de casa vitoriana de bonecas"),
            new PlatformSpec(14, "Globo Terrestre Ilustrado", DarkRoomPhase.Phase2Escape, 2195f, 230f, 92f, 70f, 45f, new Color(0.35f, 0.60f, 0.75f), "Globo antigo sepia e azul preso em arco de latao"),
            new PlatformSpec(15, "Pipa Encantada de Bambu", DarkRoomPhase.Phase2Escape, 2355f, 178f, 100f, 50f, 15f, new Color(0.95f, 0.75f, 0.20f), "Pipa losangular de seda e rabiola de fitas"),
            new PlatformSpec(16, "Livro de Gravuras Flutuante", DarkRoomPhase.Phase2Escape, 2525f, 140f, 108f, 60f, 25f, new Color(0.30f, 0.65f, 0.45f), "Livro aberto no ar com paginas arqueadas"),
            new PlatformSpec(17, "Lustre de Cristais", DarkRoomPhase.Phase2Escape, 2705f, 198f, 112f, 70f, 40f, new Color(0.88f, 0.85f, 0.70f), "Candelabro com pingentes de cristal facetado"),
            new PlatformSpec(18, "Varao de Cortina Estrelada", DarkRoomPhase.Phase2Escape, 2890f, 148f, 118f, 35f, 18f, new Color(0.80f, 0.65f, 0.30f), "Varao cilindrico de madeira dourada no alto da janela"),
            new PlatformSpec(19, "Relogio Cuco Vintage", DarkRoomPhase.Phase2Escape, 3080f, 205f, 102f, 50f, 35f, new Color(0.42f, 0.26f, 0.15f), "Relogio cuco entalhado com base solida incorporada a parede"),
            new PlatformSpec(20, "Beiral do Guarda-Roupa", DarkRoomPhase.Phase2Escape, 3255f, 155f, 125f, 60f, 30f, new Color(0.38f, 0.22f, 0.12f), "Cornija superior esculpida de guarda-roupa de mogno"),
            new PlatformSpec(21, "Portal dos Sonhos (Porta Falsa)", DarkRoomPhase.Phase2Escape, 3455f, 195f, 185f, 75f, 40f, new Color(0.60f, 0.35f, 0.75f), "Amplo patamar cerimonial com a Porta Falsa de Saida")
        };

        // 16 plataformas da Fase 3: A Subida Caotica (15 niveis + Portal Verdadeiro)
        public static readonly PlatformSpec[] Phase3Platforms = new[]
        {
            new PlatformSpec(22, "Pilha de Blocos Tombada", DarkRoomPhase.Phase3ChaoticAscent, 3310f, 410f, 95f, 50f, 35f, new Color(0.82f, 0.40f, 0.25f), "Blocos revirados no chao formando primeiro degrau da volta"),
            new PlatformSpec(23, "Boneca de Pano Desconjuntada", DarkRoomPhase.Phase3ChaoticAscent, 3110f, 350f, 95f, 55f, 40f, new Color(0.85f, 0.30f, 0.35f), "Boneca de pano com vestido xadrez sentada torta"),
            new PlatformSpec(24, "Caixa de Giz de Cera Aberta", DarkRoomPhase.Phase3ChaoticAscent, 2905f, 290f, 100f, 50f, 30f, new Color(0.92f, 0.80f, 0.20f), "Caixa aberta com gizes coloridos apontando"),
            new PlatformSpec(25, "Pilha Torta de Contos", DarkRoomPhase.Phase3ChaoticAscent, 2695f, 355f, 95f, 50f, 35f, new Color(0.30f, 0.40f, 0.55f), "Livros antigos em angulo louco"),
            new PlatformSpec(26, "Tamborzinho Amassado", DarkRoomPhase.Phase3ChaoticAscent, 2485f, 280f, 92f, 60f, 35f, new Color(0.75f, 0.22f, 0.22f), "Tambor de lata inclinado com baquetas cruzadas"),
            new PlatformSpec(27, "Urso Desmoronado", DarkRoomPhase.Phase3ChaoticAscent, 2275f, 215f, 98f, 60f, 30f, new Color(0.70f, 0.55f, 0.38f), "Urso deitado de brucos sobre prateleira alta"),
            new PlatformSpec(28, "Xilofone Colorido Inclinado", DarkRoomPhase.Phase3ChaoticAscent, 2060f, 275f, 105f, 50f, 25f, new Color(0.40f, 0.75f, 0.60f), "Teclas metalicas arco-iris com brilho suave"),
            new PlatformSpec(29, "Locomotiva Descarrilada", DarkRoomPhase.Phase3ChaoticAscent, 1845f, 200f, 102f, 45f, 35f, new Color(0.20f, 0.20f, 0.25f), "Vagao de madeira vermelha e preta tombado sobre livros"),
            new PlatformSpec(30, "Castelo de Cartas Bamboleante", DarkRoomPhase.Phase3ChaoticAscent, 1630f, 145f, 96f, 50f, 30f, new Color(0.90f, 0.88f, 0.82f), "Cartas de baralho gigantes ilustradas"),
            new PlatformSpec(31, "Caixa de Musica Desregulada", DarkRoomPhase.Phase3ChaoticAscent, 1415f, 210f, 100f, 50f, 35f, new Color(0.78f, 0.60f, 0.45f), "Caixa inclinada com molas aparentes"),
            new PlatformSpec(32, "Robo de Lata Desparafusado", DarkRoomPhase.Phase3ChaoticAscent, 1200f, 150f, 95f, 50f, 40f, new Color(0.40f, 0.55f, 0.70f), "Robo retro azul com antenas tortas"),
            new PlatformSpec(33, "Piao de Madeira Rodopiante", DarkRoomPhase.Phase3ChaoticAscent, 985f, 215f, 92f, 70f, 40f, new Color(0.68f, 0.45f, 0.28f), "Grande piao de madeira listrada flutuando"),
            new PlatformSpec(34, "Carretel com Fita Flutuante", DarkRoomPhase.Phase3ChaoticAscent, 770f, 150f, 95f, 50f, 30f, new Color(0.88f, 0.50f, 0.65f), "Carretel de madeira com fita de cetim rosa"),
            new PlatformSpec(35, "Mobile Desequilibrado", DarkRoomPhase.Phase3ChaoticAscent, 555f, 195f, 98f, 50f, 30f, new Color(0.85f, 0.75f, 0.35f), "Estrutura de luas, nuvens e estrelas de feltro"),
            new PlatformSpec(36, "Livro de Feiticos no Vacuo", DarkRoomPhase.Phase3ChaoticAscent, 335f, 140f, 102f, 55f, 28f, new Color(0.45f, 0.25f, 0.55f), "Grimorio infantil iluminado flutuando no alto"),
            new PlatformSpec(37, "O Verdadeiro Portal dos Sonhos", DarkRoomPhase.Phase3ChaoticAscent, 45f, 180f, 195f, 80f, 50f, new Color(0.95f, 0.85f, 0.40f), "Grande balcao real iluminado com a Porta Verdadeira")
        };
    }
}
