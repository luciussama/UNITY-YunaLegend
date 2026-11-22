using System.Collections.Generic;

namespace YunaLegend.ToyRoom.Core
{
    /// <summary>
    /// Catalogo canonico de dimensoes, 7 moveis, 8 brinquedos e pontos perifericos da Fase 2 (Toy Room).
    /// </summary>
    public static class ToyRoomCatalog
    {
        public const float RoomWidth = 28.0f;
        public const float RoomDepth = 20.0f;
        public const float HalfWidth = RoomWidth * 0.5f;
        public const float HalfDepth = RoomDepth * 0.5f;
        public const int TotalToyCount = 8;

        public static ToyRoomPoint FromLegacyCoordinates(float legacyX, float legacyY)
        {
            return new ToyRoomPoint((legacyX - 800f) / 60f, (600f - legacyY) / 60f);
        }

        public static readonly IReadOnlyList<FurnitureSpec> Furniture = new List<FurnitureSpec>
        {
            // Ordem da esquerda para a direita na parede do fundo igual a pintura oficial room-v1.png:
            // 1) Bau/Caixa de Brinquedos a esquerda (legacyX=520 -> X=-4.67m)
            // 2) Estante de Livros ao centro (legacyX=860 -> X=+1.00m)
            // 3) Guarda-Roupa / Armario a direita (legacyX=1280 -> X=+8.00m)
            new FurnitureSpec("toy-chest", "Bau de Brinquedos", new ToyRoomPoint(FromLegacyCoordinates(520f, 30f).X, HalfDepth - 1.35f * 0.5f - 0.06f), 2.30f, 1.35f, isToyChest: true),
            new FurnitureSpec("bookshelf", "Estante de Livros", new ToyRoomPoint(FromLegacyCoordinates(860f, 30f).X, HalfDepth - 1.05f * 0.5f - 0.06f), 2.80f, 1.05f),
            new FurnitureSpec("wardrobe", "Guarda-Roupa", new ToyRoomPoint(FromLegacyCoordinates(1280f, 30f).X, HalfDepth - 1.05f * 0.5f - 0.06f), 1.85f, 1.05f),
            new FurnitureSpec("rocking-horse", "Cavalinho de Pau", FromLegacyCoordinates(1120f, 420f), 1.25f, 0.95f),
            new FurnitureSpec("table", "Mesinha Infantil", FromLegacyCoordinates(620f, 780f), 2.00f, 1.35f),
            new FurnitureSpec("armchair", "Poltrona Aconchegante", FromLegacyCoordinates(180f, 840f), 1.65f, 1.45f),
            new FurnitureSpec("block-fortress", "Fortaleza de Blocos", FromLegacyCoordinates(1240f, 800f), 2.70f, 2.05f)
        };

        public static readonly IReadOnlyList<ToySpec> Toys = new List<ToySpec>
        {
            new ToySpec("teddy", "Ursinho Pipoca", 0.94f, FromLegacyCoordinates(340f, 480f)),
            new ToySpec("train", "Trenzinho Real a Vapor", 0.90f, FromLegacyCoordinates(760f, 520f)),
            new ToySpec("robot", "Robo Estelar Faisca", 0.92f, FromLegacyCoordinates(1140f, 540f)),
            new ToySpec("bunny", "Coelhinho de Algodao", 0.96f, FromLegacyCoordinates(460f, 980f)),
            new ToySpec("duck", "Patinho de Banho Imperial", 0.95f, FromLegacyCoordinates(940f, 840f)),
            new ToySpec("blocks", "Torre de Blocos Coloridos", 0.85f, FromLegacyCoordinates(1180f, 720f)),
            new ToySpec("drum", "Tamborzinho Encantado", 0.93f, FromLegacyCoordinates(640f, 960f)),
            new ToySpec("jack", "Caixinha de Surpresa da Fada", 0.91f, FromLegacyCoordinates(880f, 360f))
        };

        public static readonly IReadOnlyList<ToyRoomPoint> PeripheralRespawnCandidates = new List<ToyRoomPoint>
        {
            new ToyRoomPoint(-11.2f, -7.6f), // Canto sudoeste atras da poltrona
            new ToyRoomPoint(11.0f, -7.4f),  // Canto sudeste perto da fortaleza de blocos
            new ToyRoomPoint(-11.4f, 3.6f),  // Lateral oeste proximo a estante
            new ToyRoomPoint(11.4f, 2.8f),   // Lateral leste longe do bau
            new ToyRoomPoint(-4.8f, -8.0f),  // Borda sul-oeste
            new ToyRoomPoint(5.2f, -7.9f)    // Borda sul-leste
        };
    }
}
