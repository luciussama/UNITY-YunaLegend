using System;
using UnityEngine;

namespace YunaLegend.Dialogue
{
    public enum SpeakerId
    {
        Nanda,
        Menina,
        Yuna,
        PoeticNarrator
    }

    public enum NandaMood
    {
        Normal,
        Happy,
        Annoyed,
        Determined,
        Facepalm
    }

    [Serializable]
    public class DialogueLine
    {
        public SpeakerId speaker;
        public string text;
        public NandaMood mood;
        public bool isPlotTwistStyle;

        public DialogueLine(SpeakerId speaker, string text, NandaMood mood = NandaMood.Normal, bool isPlotTwistStyle = false)
        {
            this.speaker = speaker;
            this.text = text;
            this.mood = mood;
            this.isPlotTwistStyle = isPlotTwistStyle;
        }
    }

    public static class DarkRoomDialogueScript
    {
        // Falas da Abertura (42.5s)
        public static readonly DialogueLine OpeningLine1 = new DialogueLine(
            SpeakerId.Nanda,
            "Levanta, dorminhoca.\nTemos que brincar.\nVoce dormiu demais.",
            NandaMood.Normal
        );

        public static readonly string PoeticOpening1 = "O quarto era o mesmo.\nSo nao a reconhecia.";
        public static readonly string PoeticOpening2 = "Cada coisa guardava seu nome.\nEntre elas, nada dizia por onde ir.";

        public static readonly DialogueLine OpeningLine2 = new DialogueLine(
            SpeakerId.Nanda,
            "LEVANTA, SUA PREGUICOSA!",
            NandaMood.Determined
        );

        public static readonly DialogueLine OpeningLine3 = new DialogueLine(
            SpeakerId.Nanda,
            "SENAO A GENTE NAO SAI DAQUI NUNQUINHA!",
            NandaMood.Determined
        );

        // Cutscene do Castelo (Plataforma 9)
        public static readonly string PoeticCastle = "De longe, a porta ainda cumpria sua promessa.";
        public static readonly DialogueLine CastleNanda = new DialogueLine(SpeakerId.Nanda, "A porta e por ali. Anda, vem comigo!");
        public static readonly DialogueLine CastleYuna = new DialogueLine(SpeakerId.Menina, "Daqui parece perto. La embaixo, nao.");

        // Plot Twist da Porta Falsa (Plataforma 21)
        public static readonly DialogueLine FalseDoorYuna = new DialogueLine(
            SpeakerId.Menina,
            "Mas era ali. Eu tinha certeza.",
            NandaMood.Normal,
            isPlotTwistStyle: true
        );

        public static readonly string PoeticFalseDoor = "A madeira caiu.\nA promessa ficou de pe.";

        public static readonly DialogueLine FalseDoorNanda = new DialogueLine(
            SpeakerId.Nanda,
            "Eu tambem achei. Droga... Vamos olhar por ali.",
            NandaMood.Annoyed
        );

        // Conclusao do Portal Verdadeiro (Fim do Quarto Escuro)
        public static readonly string PoeticTruePortal = "E la estava ela: a porta que so se abre para quem nao desiste de procurar.";
    }
}
