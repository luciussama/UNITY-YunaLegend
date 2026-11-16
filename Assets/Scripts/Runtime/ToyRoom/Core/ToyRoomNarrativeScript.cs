using System.Collections.Generic;

namespace YunaLegend.ToyRoom.Core
{
    /// <summary>
    /// Roteiro canonico de dialogos e narracoes poeticas da Fase 2 (Toy Room).
    /// </summary>
    public static class ToyRoomNarrativeScript
    {
        public static readonly IReadOnlyList<ToyRoomNarrativeBeat> IntroBeats = new List<ToyRoomNarrativeBeat>
        {
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Pause, ToyRoomSpeaker.None, string.Empty, 2.5f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Menina, "Aqui também...", 4.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Menina, "Eu queria espaço pra brincar.", 4.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Poetic, ToyRoomSpeaker.None, "A sala estava cheia.\nNenhum lugar esperava por ela.", 3.1f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Pause, ToyRoomSpeaker.None, string.Empty, 0.8f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Poetic, ToyRoomSpeaker.None, "A caixa tinha um fundo.\nO resto está vazio.", 3.1f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Nanda, "Vamos. Um de cada vez!", 2.5f)
        };

        public static readonly IReadOnlyList<ToyRoomNarrativeBeat> StrangeDialogueBeats = new List<ToyRoomNarrativeBeat>
        {
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Nanda, "Estranho...", 1.5f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Menina, "Acho que esse brinquedo já tinha sido guardado.", 4.5f)
        };

        public static readonly IReadOnlyList<ToyRoomNarrativeBeat> InvestigationBeats = new List<ToyRoomNarrativeBeat>
        {
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Nanda, "Tem alguma coisa estranha...", 3.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Pause, ToyRoomSpeaker.None, string.Empty, 1.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Menina, "Estamos vendo os mesmos brinquedos de novo.", 4.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Poetic, ToyRoomSpeaker.None, "Os brinquedos continuavam voltando.\nComo se o quarto esquecesse que alguém tinha acabado de guardá-los.", 6.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Pause, ToyRoomSpeaker.None, string.Empty, 2.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Poetic, ToyRoomSpeaker.None, "Foi então que uma coisa nova apareceu.\nOu talvez estivesse ali desde sempre.", 5.0f, revealsSword: true),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Menina, "Hm?", 1.5f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Nanda, "Que brinquedo é esse?", 3.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Menina, "Eu não lembro dele.", 2.5f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Nanda, "E eu lembraria.", 2.5f)
        };

        public static readonly IReadOnlyList<ToyRoomNarrativeBeat> SwordPickupBeats = new List<ToyRoomNarrativeBeat>
        {
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Poetic, ToyRoomSpeaker.None, "Ela não estava procurando por aquilo.", 2.0f),
            new ToyRoomNarrativeBeat(ToyRoomBeatKind.Poetic, ToyRoomSpeaker.None, "Mas algumas coisas parecem encontrar a gente primeiro.", 3.0f)
        };
    }
}
