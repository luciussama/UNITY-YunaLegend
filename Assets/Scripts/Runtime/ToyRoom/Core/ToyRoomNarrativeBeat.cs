namespace YunaLegend.ToyRoom.Core
{

    /// <summary>
    /// Descritor imutavel de um beat narrativo (dialogo, narracao poetica ou respiro visual).
    /// </summary>
    public readonly struct ToyRoomNarrativeBeat
    {
        public ToyRoomBeatKind Kind { get; }
        public ToyRoomSpeaker Speaker { get; }
        public string Text { get; }
        public float DurationSeconds { get; }
        public bool RevealsSword { get; }

        public ToyRoomNarrativeBeat(
            ToyRoomBeatKind kind,
            ToyRoomSpeaker speaker,
            string text,
            float durationSeconds,
            bool revealsSword = false)
        {
            Kind = kind;
            Speaker = speaker;
            Text = text;
            DurationSeconds = durationSeconds;
            RevealsSword = revealsSword;
        }
    }
}
