using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public static class DarkRoomThreeStageCircuitGenerator
    {
        // Altura com que o modulo de layout da Etapa 1 foi calibrado (vaos, topos e camera).
        // A rota continua ancorada nesse modulo; a geometria visivel e o colisor de cada
        // plataforma usam as medidas canonicas de DarkRoomPlatformGeometry.
        public const float LayoutPlatformHeight = 0.7f;

        // Largura de layout da ultima plataforma da Etapa 1 (castelinho), usada para a
        // transicao calibrada rumo a Etapa 2.
        public const float LayoutLastStage1Width = 4.2f;

        /// <summary>
        /// Inicio da Etapa 2 preservando as distancias ja validadas: o modulo de layout da
        /// Etapa 1 continua sendo a referencia, e nao a largura visivel de cada objeto.
        /// </summary>
        public static float Stage2StartX(PlatformSpec lastStage1, float firstStage2Width, float gap)
        {
            return lastStage1.x + (LayoutLastStage1Width * 0.5f) + gap + (firstStage2Width * 0.5f);
        }

        /// <summary>
        /// Altura central da primeira plataforma da Etapa 2, derivada do topo real da ultima
        /// plataforma da Etapa 1 (os objetos podem ter alturas diferentes do modulo antigo).
        /// </summary>
        public static float Stage2StartY(PlatformSpec lastStage1, float verticalGap)
        {
            return (lastStage1.y + lastStage1.height * 0.5f) - (LayoutPlatformHeight * 0.5f) + verticalGap;
        }

        // -------------------------------------------------------------
        // ETAPA 1: A Subida ao Castelo (0 a 9) - Calma e Acolhedora
        // Velocidade base: 3.6 m/s, pulo base: 2.4m, gap uniforme: 1.4m.
        // Culmina na Plataforma 09 (Castelinho de Blocos), onde ocorre a cutscene.
        // -------------------------------------------------------------
        public static List<PlatformSpec> GenerateStage1Platforms()
        {
            var list = new List<PlatformSpec>();
            string[] names = new[]
            {
                "00_Cabeca_Urso_Pelucia",
                "01_Pilha_Livros_Ilustrados",
                "02_Penteadeira_Encantada",
                "03_Comoda_Pequena_Carvalho",
                "04_Blocos_ABC_Madeira",
                "05_Tamborito_Marcha",
                "06_Almofadao_Veludo_Roxo",
                "07_Gavetas_Abertas_Armario",
                "08_Caixa_Musica_Bailarina",
                "09_Castelinho_Blocos_Descanso"
            };

            float layoutX = DarkRoomStartLayout.FirstPlatformX;
            float currentY = -6.5f;

            for (int i = 0; i < names.Length; i++)
            {
                // Modulo de layout preservado (rota e camera ja validadas).
                float layoutW = (i == 9) ? 4.2f : (i == 0 ? 3.0f : 2.6f);

                // Geometria canonica do objeto: cada plataforma tem o seu proprio tamanho e
                // a sua superficie de pouso, em vez do antigo 2,6 m x 0,7 m uniforme.
                var geometry = DarkRoomPlatformGeometry.Get(i);
                float w = geometry.Width;
                float h = geometry.Height;
                float d = geometry.Depth;

                float nominalTopY = DarkRoomStartLayout.PlatformCenterY(i, currentY) + LayoutPlatformHeight * 0.5f;
                // A plataforma 00 encosta no piso: o topo nasce da altura real do objeto.
                float topY = i == 0 ? DarkRoomPlatformGeometry.FloorSurfaceY + h : nominalTopY;
                float centerY = topY - h * 0.5f;

                Color tint = (i == 9) ? new Color(0.25f, 0.45f, 0.75f) : Color.HSVToRGB(i * 0.08f, 0.5f, 0.85f);

                list.Add(new PlatformSpec(i, names[i], DarkRoomPhase.Phase1Ascent, layoutX, centerY, w, d, h, tint, "Etapa 1 - Subida Calma"));

                if (i < names.Length - 1)
                {
                    float nextLayoutW = (i + 1 == 9) ? 4.2f : 2.6f;
                    float gap = 1.4f; // Vao introdutorio para o salto balistico por toque.
                    layoutX += (layoutW * 0.5f) + gap + (nextLayoutW * 0.5f);
                    currentY += 0.32f; // Subida suave constante
                }
            }

            return list;
        }

        // -------------------------------------------------------------
        // ETAPA 2: A Fuga Acelerada (10 a 21) - Progressao de Velocidade
        // Velocidade escala de 4.0 m/s ate 6.0 m/s. Pulo escala de 2.5m ate 3.4m.
        // Culmina na Plataforma 21: O Poster da Porta Falsa na parede (x ~ 90.5m, y ~ 1.58m).
        // Conexao com Castelo: começa exatamente apos o fim do Castelinho (x=41.10m) + gap 1.6m.
        // -------------------------------------------------------------
        public static List<PlatformSpec> GenerateStage2Platforms(float startX = 44.42f, float startY = -3.27f)
        {
            var list = new List<PlatformSpec>();
            string[] names = new[]
            {
                "10_Pista_Elevada_Trem",
                "11_Prateleira_Brinquedos",
                "12_Abajur_Cogumelo",
                "13_Telhado_Casa_Bonecas",
                "14_Globo_Terrestre",
                "15_Pipa_Encantada_Bambu",
                "16_Livro_Gravuras_No_Ar",
                "17_Lustre_Cristais",
                "18_Varao_Cortina_Estrelada",
                "19_Relogio_Cuco_Vintage",
                "20_Beiral_Guarda_Roupa",
                "21_Poster_Porta_Falsa_Parede"
            };

            float currentX = startX;
            float currentY = startY;

            for (int i = 0; i < names.Length; i++)
            {
                int index = 10 + i;
                float progress = (float)i / (names.Length - 1); // 0 a 1

                float w = (i == 11) ? 3.8f : Mathf.Lerp(2.2f, 1.8f, progress);
                float h = (i == 11) ? 1.4f : 0.65f;
                float d = 1.8f;

                Color tint = (i == 11) ? new Color(0.95f, 0.30f, 0.25f) : Color.HSVToRGB(0.1f + progress * 0.4f, 0.7f, 0.9f);
                list.Add(new PlatformSpec(index, names[i], DarkRoomPhase.Phase2Escape, currentX, currentY, w, d, h, tint, "Etapa 2 - Fuga Acelerada"));

                if (i < names.Length - 1)
                {
                    float nextProgress = (float)(i + 1) / (names.Length - 1);
                    float nextW = (i + 1 == 11) ? 3.8f : Mathf.Lerp(2.2f, 1.8f, nextProgress);
                    // Vãos calibrados matematicamente pela parábola física real para aterrissar com margem segura sem overshoot
                    float gap = Mathf.Lerp(2.32f, 4.75f, progress);
                    float deltaY = Mathf.Lerp(0.32f, 0.48f, progress);

                    currentX += (w * 0.5f) + gap + (nextW * 0.5f);
                    currentY += deltaY;
                }
            }

            return list;
        }

        // -------------------------------------------------------------
        // ETAPA 3: A Subida Caotica Reversa (22 a 37) - Direita para Esquerda
        // Acentuacao progressiva: plataformas menores (2.2m -> 1.27m), gaps maiores (1.8m -> 4.6m).
        // Velocidade escala de 4.2 ate 6.6 m/s, pulo escala de 2.6m ate 3.9m.
        // Inicia no chao apos a queda da porta falsa (wallX - 3.2m), subindo para a ESQUERDA.
        // Termina na Plataforma 37: O Verdadeiro Portal dos Sonhos (largura 4.5m).
        // -------------------------------------------------------------
        public static List<PlatformSpec> GenerateStage3Platforms(float wallX = 104.26f, float floorDropY = DarkRoomReturnLayout.FloorY)
        {
            var list = new List<PlatformSpec>();
            string[] names = new[]
            {
                "22_Pilha_Blocos_Tombada",
                "23_Boneca_Pano_Desconjuntada",
                "24_Caixa_Giz_Aberta",
                "25_Pilha_Torta_Contos",
                "26_Tamborzinho_Amassado",
                "27_Urso_Pelucia_Desmoronado",
                "28_Xilofone_Colorido_Inclinado",
                "29_Locomotiva_Descarrilada",
                "30_Castelo_Cartas_Bamboleante",
                "31_Caixa_Musica_Desregulada",
                "32_Robo_Lata_Desparafusado",
                "33_Piao_Madeira_Rodopiante",
                "34_Carretel_Fita_Flutuante",
                "35_Mobile_Desequilibrado",
                "36_Livro_Feiticos_No_Vacuo",
                "37_O_Verdadeiro_Portal_Sonhos"
            };

            float currentX = wallX - DarkRoomReturnLayout.FirstPlatformOffset;
            float currentY = floorDropY + 0.40f;

            for (int i = 0; i < names.Length; i++)
            {
                int index = 22 + i;
                float progress = (float)i / (names.Length - 1); // 0 a 1

                float w = (i == 15) ? 4.5f : Mathf.Lerp(DarkRoomReturnLayout.FirstPlatformWidth, DarkRoomReturnLayout.EndPlatformWidth, progress);
                float h = (i == 15) ? 1.4f : 0.6f;
                float d = 1.8f;
                float centerY = (i == 15) ? (currentY - 0.40f) : currentY;

                Color tint = (i == 15) ? new Color(1.0f, 0.90f, 0.40f) : Color.HSVToRGB(0.55f + progress * 0.4f, 0.75f, 0.85f);
                list.Add(new PlatformSpec(index, names[i], DarkRoomPhase.Phase3ChaoticAscent, currentX, DarkRoomReturnLayout.PlatformCenterY(i, centerY), w, d, h, tint, "Etapa 3 - Subida Caotica Reversa"));

                if (i < names.Length - 1)
                {
                    float nextProgress = (float)(i + 1) / (names.Length - 1);
                    float nextW = (i + 1 == 15) ? 4.5f : Mathf.Lerp(DarkRoomReturnLayout.FirstPlatformWidth, DarkRoomReturnLayout.EndPlatformWidth, nextProgress);
                    // Vãos calibrados com a parabola real para que o pulo a partir da plataforma encaixe no centro da proxima (1.90m a 4.65m)
                    float gap = Mathf.Lerp(1.90f, 4.65f, progress);
                    float deltaY = Mathf.Lerp(0.36f, 0.46f, progress);

                    currentX -= ((w * 0.5f) + gap + (nextW * 0.5f)); // Sentido X decrescente (esquerda)
                    currentY += deltaY;
                }
            }

            return list;
        }
    }
}
