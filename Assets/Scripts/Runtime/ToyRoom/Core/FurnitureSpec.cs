namespace YunaLegend.ToyRoom.Core
{
    /// <summary>
    /// Descritor imutavel de um movel 3D com colisao na Sala de Brinquedos.
    /// </summary>
    public sealed class FurnitureSpec
    {
        public string Id { get; }
        public string DisplayName { get; }
        public ToyRoomPoint Center { get; }
        public float Width { get; }
        public float Depth { get; }
        public bool IsToyChest { get; }

        public FurnitureSpec(
            string id,
            string displayName,
            ToyRoomPoint center,
            float width,
            float depth,
            bool isToyChest = false)
        {
            Id = id;
            DisplayName = displayName;
            Center = center;
            Width = width;
            Depth = depth;
            IsToyChest = isToyChest;
        }
    }
}
