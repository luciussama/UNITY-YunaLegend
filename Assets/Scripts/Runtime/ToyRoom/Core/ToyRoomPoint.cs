namespace YunaLegend.ToyRoom.Core
{
    /// <summary>
    /// Representa uma coordenada plana (X, Z) no piso da Sala de Brinquedos, sem dependencia de engine.
    /// </summary>
    public readonly struct ToyRoomPoint
    {
        public readonly float X;
        public readonly float Z;

        public ToyRoomPoint(float x, float z)
        {
            X = x;
            Z = z;
        }

        public float DistanceSquaredTo(ToyRoomPoint other)
        {
            float dx = X - other.X;
            float dz = Z - other.Z;
            return (dx * dx) + (dz * dz);
        }
    }
}
