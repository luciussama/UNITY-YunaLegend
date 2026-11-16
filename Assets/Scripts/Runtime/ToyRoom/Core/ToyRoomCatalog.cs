using System.Collections.Generic;

namespace YunaLegend.ToyRoom.Core
{
    /// <summary>
    /// Catalogo canonico de dimensoes, 7 moveis, 8 brinquedos e pontos perifericos da Fase 2 (Toy Room).
    /// </summary>
    public static class ToyRoomCatalog
    {
        public const float RoomWidth = 16.0f;
        public const float RoomDepth = 12.0f;
        public const float HalfWidth = RoomWidth * 0.5f;
        public const float HalfDepth = RoomDepth * 0.5f;
        public const int TotalToyCount = 8;

        public static ToyRoomPoint FromLegacyCoordinates(float legacyX, float legacyY)
        {
            return new ToyRoomPoint((legacyX - 800f) / 100f, (600f - legacyY) / 100f);
        }

        public static readonly IReadOnlyList<FurnitureSpec> Furniture = new List<FurnitureSpec>
        {
            new FurnitureSpec("toy-chest", "Bau de Brinquedos", FromLegacyCoordinates(1040f, 190f), 1.90f, 1.15f, isToyChest: true),
            new FurnitureSpec("bookshelf", "Estante de Livros", FromLegacyCoordinates(380f, 170f), 2.20f, 0.90f),
            new FurnitureSpec("wardrobe", "Guarda-Roupa", FromLegacyCoordinates(740f, 170f), 1.40f, 0.90f),
            new FurnitureSpec("rocking-horse", "Cavalinho de Pau", FromLegacyCoordinates(480f, 420f), 1.00f, 0.80f),
            new FurnitureSpec("table", "Mesinha Infantil", FromLegacyCoordinates(620f, 780f), 1.60f, 1.10f),
            new FurnitureSpec("armchair", "Poltrona Aconchegante", FromLegacyCoordinates(180f, 840f), 1.30f, 1.20f),
            new FurnitureSpec("block-fortress", "Fortaleza de Blocos", FromLegacyCoordinates(1240f, 800f), 2.20f, 1.70f)
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
            new ToyRoomPoint(-6.4f, -4.5f), // Canto sudoeste atras da poltrona
            new ToyRoomPoint(6.3f, -4.4f),  // Canto sudeste perto da fortaleza de blocos
            new ToyRoomPoint(-6.2f, 2.2f),  // Lateral oeste proximo a estante
            new ToyRoomPoint(6.4f, 1.8f),   // Lateral leste longe do bau
            new ToyRoomPoint(-2.8f, -4.8f), // Borda sul-oeste
            new ToyRoomPoint(2.9f, -4.7f)   // Borda sul-leste
        };
    }
}
