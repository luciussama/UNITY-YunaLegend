namespace YunaLegend.ToyRoom.Core
{
    /// <summary>
    /// Descritor imutavel de um brinquedo interativo da Sala de Brinquedos.
    /// </summary>
    public sealed class ToySpec
    {
        public string Id { get; }
        public string DisplayName { get; }
        public float WeightMultiplier { get; }
        public ToyRoomPoint InitialPosition { get; }

        public ToySpec(string id, string displayName, float weightMultiplier, ToyRoomPoint initialPosition)
        {
            Id = id;
            DisplayName = displayName;
            WeightMultiplier = weightMultiplier;
            InitialPosition = initialPosition;
        }
    }
}
