using System;

namespace YunaLegend.Picnic.Core
{
    public readonly struct PicnicNarrativeCard
    {
        public string Speaker { get; }
        public string Text { get; }
        public float Duration { get; }

        public PicnicNarrativeCard(string speaker, string text, float duration)
        {
            Speaker = speaker;
            Text = text;
            Duration = duration;
        }
    }

    public readonly struct PicnicCinematicState
    {
        public bool IsEnding { get; }
        public int CardIndex { get; }
        public PicnicNarrativeCard Card { get; }
        public float CardTime { get; }
        public float Zoom { get; }
        public float FocusX { get; }
        public int YunaActingFrame { get; }
        public int YunaPairFrameB { get; }
        public float YunaPairBlend { get; }
        public bool YunaSleeping { get; }
        public float YunaSleepBlend { get; }
        public float YunaX { get; }
        public float YunaBaseY { get; }
        public float FairyX { get; }
        public float FairyY { get; }
        public bool FairySitting { get; }
        public bool FairySleeping { get; }
        public bool ShowBeat4Insect { get; }
        public float InsectX { get; }
        public float InsectY { get; }
        public float InsectAlpha { get; }

        public PicnicCinematicState(bool isEnding, int cardIndex, PicnicNarrativeCard card, float cardTime,
            float zoom, float focusX, int yunaActingFrame, int yunaPairFrameB, float yunaPairBlend,
            bool yunaSleeping, float yunaSleepBlend, float yunaX, float yunaBaseY,
            float fairyX, float fairyY, bool fairySitting, bool fairySleeping,
            bool showBeat4Insect, float insectX, float insectY, float insectAlpha)
        {
            IsEnding = isEnding;
            CardIndex = cardIndex;
            Card = card;
            CardTime = cardTime;
            Zoom = zoom;
            FocusX = focusX;
            YunaActingFrame = yunaActingFrame;
            YunaPairFrameB = yunaPairFrameB;
            YunaPairBlend = yunaPairBlend;
            YunaSleeping = yunaSleeping;
            YunaSleepBlend = yunaSleepBlend;
            YunaX = yunaX;
            YunaBaseY = yunaBaseY;
            FairyX = fairyX;
            FairyY = fairyY;
            FairySitting = fairySitting;
            FairySleeping = fairySleeping;
            ShowBeat4Insect = showBeat4Insect;
            InsectX = insectX;
            InsectY = insectY;
            InsectAlpha = insectAlpha;
        }
    }

    /// <summary>
    /// 1:1 faithful C# port of gardenNarrative.js and gardenCinematic.js (pure layout/acting math).
    /// </summary>
    public static class PicnicNarrativeData
    {
        public static readonly PicnicNarrativeCard[] Opening =
        {
            new PicnicNarrativeCard("UM DIA NO JARDIM", "A porta se abriu para uma manhã cheia de coisas.\nO sol aquecia o último degrau.", 5f),
            new PicnicNarrativeCard("NANDA", "Vamos, Yuna, vamos no piquenique, vai ser divertido!", 5f),
            new PicnicNarrativeCard("UM PRIMEIRO PASSO", "Yuna.\nSeu nome atravessou a porta antes dos seus pés.", 5f),
            new PicnicNarrativeCard("UM PRIMEIRO PASSO", "Ela queria o piquenique, os amigos, o pão quentinho.\nMas o jardim chegava todo de uma vez.", 6f),
            new PicnicNarrativeCard("UM PRIMEIRO PASSO", "A grama roçava suas pernas. As pedrinhas faziam cada passo parecer diferente.\nUm inseto levantou voo, e seus ombros subiram junto.", 8f),
            new PicnicNarrativeCard("UM PRIMEIRO PASSO", "Dentro dela, a alegria dava pulinhos.\nO medo também.", 5f),
            new PicnicNarrativeCard("YUNA", "Eu quero ir… mas o jardim faz tanta coisa ao mesmo tempo.", 5f),
            new PicnicNarrativeCard("NANDA", "Então vamos um pedacinho de cada vez. Eu vou com você.", 5f),
            new PicnicNarrativeCard("UM PRIMEIRO PASSO", "A vontade de ir e a vontade de ficar couberam no mesmo passo.\nE ela desceu.", 6f),
        };

        public static readonly PicnicNarrativeCard[] Ending =
        {
            new PicnicNarrativeCard("UM LUGAR PARA DESCANSAR", "Quando a toalha apareceu, Yuna soltou um suspiro comprido.\nTinha trazido o jardim inteiro nas pernas.", 6f),
            new PicnicNarrativeCard("UM LUGAR PARA DESCANSAR", "Sentou-se devagar entre os amigos.\nPor um instante, só ficou ali, sentindo o chão quietinho debaixo dela.", 6f),
            new PicnicNarrativeCard("YUNA", "Cheguei… mas minhas pernas ainda estão correndo.", 5f),
            new PicnicNarrativeCard("NANDA", "Então deixa que elas descansem. Hoje eu sento aqui.", 5f),
            new PicnicNarrativeCard("UM LUGAR PARA DESCANSAR", "Nanda pousou ao seu lado e dobrou as perninhas.\nPara ela havia um pão do tamanho de sua mãozinha e um copinho de suco miudinho.", 8f),
            new PicnicNarrativeCard("UM LUGAR PARA DESCANSAR", "Um piquenique inteiro cabia naquele pedacinho de toalha.\nYuna segurou seu pão.", 5f),
            new PicnicNarrativeCard("YUNA", "O jardim foi bonito. E foi muito.", 4f),
            new PicnicNarrativeCard("NANDA", "Eu sei.", 3f),
            new PicnicNarrativeCard("UM LUGAR PARA DESCANSAR", "As folhas continuaram conversando lá no alto.\nO pão estava morno. O suco, fresquinho.", 6f),
            new PicnicNarrativeCard("UM LUGAR PARA DESCANSAR", "Por enquanto, Yuna não precisava dar mais nenhum passo.\nFicou ali, comendo devagar, com a fada sentada ao seu lado.", 8f),
            new PicnicNarrativeCard("MAIS UM PEDACINHO", "Veio outro pão na chapa. E mais um gole de laranja.\nNa toalha, a tarde foi ficando macia.", 7f),
            new PicnicNarrativeCard("A TARDE FECHOU OS OLHOS", "A barriga ficou quentinha. As pálpebras, pesadas.\nYuna tentou guardar um último sorriso.", 7f),
            new PicnicNarrativeCard("A TARDE FECHOU OS OLHOS", "O sono chegou sem fazer barulho.\nNanda dobrou as asas e se aconchegou ao lado dela.", 7f),
            new PicnicNarrativeCard("UM JARDIM POR DENTRO", "Lá fora, o jardim continuou do mesmo tamanho.\nDentro do sonho, uma folha já podia ser floresta.", 7f),
        };

        public const float OpeningDuration = 50f;
        public const float EndingDuration = 84f;

        public static PicnicCinematicState Evaluate(bool isEnding, int cardIndex, float cardTime, float stageAge, float totalTime, bool portrait)
        {
            var cards = isEnding ? Ending : Opening;
            int idx = Math.Max(0, Math.Min(cards.Length - 1, cardIndex));
            var card = cards[idx];
            float p = Smooth(cardTime / Math.Max(0.001f, card.Duration));
            bool close = isEnding ? (idx >= 2 && idx <= 7) : (idx == 1 || (idx >= 4 && idx <= 7));
            float zoom = 1f + (close ? 0.035f : 0f) + p * 0.015f;
            bool sleepShot = isEnding && idx >= 11;
            float sleepBlend = idx == 11 ? Smooth(cardTime / 2f) : 1f;
            float focusX = isEnding
                ? (portrait ? 940f - (sleepShot ? 90f * sleepBlend : 0f) : 1030f)
                : (portrait ? 680f : 710f);
            if (isEnding && idx == 0)
            {
                focusX = portrait ? 1250f - 310f * p : focusX + 45f * (1f - p);
            }

            if (!isEnding)
            {
                int pose = idx >= 8 ? 2 : idx >= 4 ? 1 : 0;
                float approach = Smooth(stageAge / 4f);
                bool look = idx >= 4 && idx <= 7;
                float fx = 875f + (1f - approach) * 140f + (look ? -32f : (float)Math.Sin(totalTime * 0.8f) * 24f) - (portrait ? 55f : 0f);
                float fy = 370f + (float)Math.Sin(totalTime * 2.4f) * 9f;
                bool showInsect = idx == 4;
                float q = Smooth(cardTime / 5f);
                float ix = 730f + q * 180f;
                float iy = 430f - (float)Math.Sin(q * Math.PI) * 70f;
                float ia = showInsect ? (float)Math.Sin(q * Math.PI) * 0.8f : 0f;
                return new PicnicCinematicState(false, idx, card, cardTime, zoom, focusX,
                    pose, -1, 0f, false, 0f, 590f, 700f, fx, fy, false, false, showInsect, ix, iy, ia);
            }

            float settling = Smooth(stageAge / 2f);
            float baseY = 735f - 12f * (1f - settling);
            bool eating = idx >= 5 && idx < 11;
            int yunaFrame = idx >= 11 ? 3 : eating ? 4 : 3;
            int pairB = eating ? 5 : -1;
            float pairBlend = 0f;
            if (eating)
            {
                float cycle = totalTime % 3.4f;
                pairBlend = Smooth((cycle - 0.8f) / 0.28f) * (1f - Smooth((cycle - 2.2f) / 0.28f));
            }
            bool yunaSleeping = idx >= 11;
            float yunaSleepBlend = idx >= 12 ? Smooth(cardTime / 1.8f) : 0f;

            bool fairySleeping = idx >= 12;
            bool fairySitting = !fairySleeping && idx > 4;
            float efX = fairySleeping ? 700f : fairySitting ? 790f : 760f + 25f * (idx == 4 ? Smooth(cardTime / 2f) : 0f);
            float efY = fairySleeping ? 710f : fairySitting ? 710f : 520f + 135f * (idx == 4 ? Smooth(cardTime / 2f) : 0f);

            return new PicnicCinematicState(true, idx, card, cardTime, zoom, focusX,
                yunaFrame, pairB, pairBlend, yunaSleeping, yunaSleepBlend, 895f, baseY,
                efX, efY, fairySitting, fairySleeping, false, 0f, 0f, 0f);
        }

        private static float Smooth(float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return t * t * (3f - 2f * t);
        }
    }
}
