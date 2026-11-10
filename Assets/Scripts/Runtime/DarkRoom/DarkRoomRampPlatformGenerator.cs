using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public static class DarkRoomRampPlatformGenerator
    {
        /// <summary>
        /// Gera a trilha da Subida Constante:
        /// 22 plataformas subindo em rampa uniforme da esquerda para a direita,
        /// com vãos calculados exatamente para timing de pulo na velocidade de 5.2 m/s.
        /// Culmina na Plataforma 21 (O Poster na Parede).
        /// </summary>
        public static List<PlatformSpec> GenerateAscentRampPlatforms()
        {
            var list = new List<PlatformSpec>();

            // Nomes tematicos da subida infantil (brinquedos e moveis)
            string[] names = new[]
            {
                "00_Cabeca_Urso_Pelucia",
                "01_Pilha_Livros_Grossos",
                "02_Penteadeira_Madeira_Clara",
                "03_Comoda_Carvalho",
                "04_Blocos_ABC_Coloridos",
                "05_Tamborito_Marcha",
                "06_Almofadao_Veludo_Roxo",
                "07_Gaveta_Aberta_Armario",
                "08_Caixa_Musica_Bailarina",
                "09_Castelinho_Blocos_Madeira",
                "10_Viaduto_Trilhos_Trem",
                "11_Prateleira_Bibeis",
                "12_Abajur_Cogumelo_Luz",
                "13_Telhado_Casa_Bonecas",
                "14_Globo_Terrestre_Latao",
                "15_Pipa_Bambu_Encantada",
                "16_Livro_Gravuras_Aberto",
                "17_Lustre_Cristais",
                "18_Varao_Cortina_Janela",
                "19_Relogio_Cuco_Floresta",
                "20_Cornija_Guarda_Roupa",
                "21_Poster_Porta_Falsa_Parede"
            };

            float startX = 2.0f;
            float startY = -7.0f; // Bem acima do chao (-9.2f)

            // A cada plataforma, a menina sobe suavemente em altura
            // Vao de salto calibrado: largura da plataforma = 2.2m, gap = 2.4m -> total passo = 4.6m
            // A 5.2 m/s, o tempo de voo e ~0.46s, exigindo salto no momento certo
            float stepX = 4.6f;
            float stepY = 0.55f; // Rampa ascendente constante

            for (int i = 0; i < names.Length; i++)
            {
                float x = startX + (i * stepX);
                float y = startY + (i * stepY);
                float w = (i == 21) ? 3.0f : 2.2f;
                float d = 1.6f;
                float h = 0.8f;

                Color tint = (i == 21)
                    ? new Color(0.95f, 0.35f, 0.30f) // Poster vermelho/laranja
                    : Color.HSVToRGB((i * 0.045f) % 1.0f, 0.6f, 0.85f);

                string desc = (i == 21)
                    ? "Poster de papel pintado colado na parede com desenho de uma porta mágica"
                    : $"Plataforma de subida rítmica #{i}";

                // Guardamos x e y já diretamente em unidades Unity (ou divididas se o builder usar escala)
                list.Add(new PlatformSpec(i, names[i], DarkRoomPhase.Phase1Ascent, x, y, w, d, h, tint, desc));
            }

            return list;
        }

        /// <summary>
        /// Gera a trilha da Corrida Reversa (Fase 3):
        /// Apos cair no poster da parede (em X ~ 98m), Yuna cai e corre da direita para a esquerda,
        /// saltando sobre objetos caoticos revirados ate o Verdadeiro Portal dos Sonhos.
        /// </summary>
        public static List<PlatformSpec> GenerateReturnChaoticPlatforms(float wallX = 98.6f)
        {
            var list = new List<PlatformSpec>();

            string[] names = new[]
            {
                "22_Pilha_Blocos_Tombada",
                "23_Boneca_Pano_Revirada",
                "24_Caixa_Giz_Aberta",
                "25_Livros_Inclinados",
                "26_Tambor_Amassado",
                "27_Urso_Desmoronado",
                "28_Xilofone_Metalico",
                "29_Locomotiva_Tombada",
                "30_Castelo_Cartas",
                "31_Molas_Caixa_Musica",
                "32_Robo_Lata_Torto",
                "33_Piao_Madeira_Voador",
                "34_Carretel_Fita_Rosa",
                "35_Mobile_Luas_Estrelas",
                "36_Grimorio_Flutuante",
                "37_Verdadeiro_Portal_Sonhos"
            };

            float startX = wallX - 5.0f;
            float startY = -5.0f;
            float stepX = 4.8f;

            for (int i = 0; i < names.Length; i++)
            {
                int index = 22 + i;
                float x = startX - (i * stepX); // Caminha no sentido X negativo (para a esquerda)
                float y = startY - (i * 0.15f); // Descida suave ou ondulacao
                float w = (i == names.Length - 1) ? 3.5f : 2.0f;
                float d = 1.6f;
                float h = 0.8f;

                Color tint = (i == names.Length - 1)
                    ? new Color(1.0f, 0.90f, 0.40f) // Portal Verdadeiro dourado
                    : Color.HSVToRGB((0.5f + (i * 0.035f)) % 1.0f, 0.65f, 0.75f);

                list.Add(new PlatformSpec(index, names[i], DarkRoomPhase.Phase3ChaoticAscent, x, y, w, d, h, tint, "Plataforma caotica reversa"));
            }

            return list;
        }
    }
}
