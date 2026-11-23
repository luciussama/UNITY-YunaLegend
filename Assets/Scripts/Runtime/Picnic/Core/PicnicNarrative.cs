using System;

namespace YunaLegend.Picnic.Core
{
    public sealed class PicnicBeat
    {
        public string Title { get; }
        public string Text { get; }
        public float Duration { get; }
        public PicnicBeat(string title, string text, float duration)
        {
            Title = title;
            Text = text;
            Duration = duration;
        }
    }

    public sealed class PicnicNarrative
    {
        public PicnicBeat[] Beats { get; }
        public float Duration { get; }
        public PicnicNarrative(params PicnicBeat[] beats)
        {
            Beats = beats;
            foreach (var beat in beats) Duration += beat.Duration;
        }
        public int IndexAt(float time)
        {
            float cursor = 0f;
            for (int i = 0; i < Beats.Length; i++)
            {
                cursor += Beats[i].Duration;
                if (time < cursor) return i;
            }
            return Beats.Length - 1;
        }
        public float NextBeat(float time)
        {
            float cursor = 0f;
            foreach (var beat in Beats)
            {
                cursor += beat.Duration;
                if (cursor > time + 0.01f) return cursor;
            }
            return Duration;
        }
        public static readonly PicnicNarrative Opening = new PicnicNarrative(
            new PicnicBeat("UM DIA NO JARDIM", "A porta se abriu para uma manhã cheia de coisas.\nO sol aquecia o último degrau.", 5f),
            new PicnicBeat("NANDA", "Vamos, Yuna, vamos no piquenique, vai ser divertido!", 5f),
            new PicnicBeat("UM PRIMEIRO PASSO", "Yuna.\nSeu nome atravessou a porta antes dos seus pés.", 5f),
            new PicnicBeat("UM PRIMEIRO PASSO", "Ela queria o piquenique, os amigos, o pão quentinho.\nMas o jardim chegava todo de uma vez.", 6f),
            new PicnicBeat("UM PRIMEIRO PASSO", "A grama roçava suas pernas. As pedrinhas faziam cada passo parecer diferente.\nUm inseto levantou voo, e seus ombros subiram junto.", 8f),
            new PicnicBeat("UM PRIMEIRO PASSO", "Dentro dela, a alegria dava pulinhos.\nO medo também.", 5f),
            new PicnicBeat("YUNA", "Eu quero ir… mas o jardim faz tanta coisa ao mesmo tempo.", 5f),
            new PicnicBeat("NANDA", "Então vamos um pedacinho de cada vez. Eu vou com você.", 5f),
            new PicnicBeat("UM PRIMEIRO PASSO", "A vontade de ir e a vontade de ficar couberam no mesmo passo.\nE ela desceu.", 6f));
        public static readonly PicnicNarrative Ending = new PicnicNarrative(
            new PicnicBeat("UM LUGAR PARA DESCANSAR", "Quando a toalha apareceu, Yuna soltou um suspiro comprido.\nTinha trazido o jardim inteiro nas pernas.", 6f),
            new PicnicBeat("UM LUGAR PARA DESCANSAR", "Sentou-se devagar entre os amigos.\nPor um instante, só ficou ali, sentindo o chão quietinho debaixo dela.", 6f),
            new PicnicBeat("YUNA", "Cheguei… mas minhas pernas ainda estão correndo.", 5f),
            new PicnicBeat("NANDA", "Então deixa que elas descansem. Hoje eu sento aqui.", 5f),
            new PicnicBeat("UM LUGAR PARA DESCANSAR", "Nanda pousou ao seu lado e dobrou as perninhas.\nPara ela havia um pão do tamanho de sua mãozinha e um copinho de suco miudinho.", 8f),
            new PicnicBeat("UM LUGAR PARA DESCANSAR", "Um piquenique inteiro cabia naquele pedacinho de toalha.\nYuna segurou seu pão.", 5f),
            new PicnicBeat("YUNA", "O jardim foi bonito. E foi muito.", 4f),
            new PicnicBeat("NANDA", "Eu sei.", 3f),
            new PicnicBeat("UM LUGAR PARA DESCANSAR", "As folhas continuaram conversando lá no alto.\nO pão estava morno. O suco, fresquinho.", 6f),
            new PicnicBeat("UM LUGAR PARA DESCANSAR", "Por enquanto, Yuna não precisava dar mais nenhum passo.\nFicou ali, comendo devagar, com a fada sentada ao seu lado.", 8f),
            new PicnicBeat("MAIS UM PEDACINHO", "Veio outro pão na chapa. E mais um gole de laranja.\nNa toalha, a tarde foi ficando macia.", 7f),
            new PicnicBeat("A TARDE FECHOU OS OLHOS", "A barriga ficou quentinha. As pálpebras, pesadas.\nYuna tentou guardar um último sorriso.", 7f),
            new PicnicBeat("A TARDE FECHOU OS OLHOS", "O sono chegou sem fazer barulho.\nNanda dobrou as asas e se aconchegou ao lado dela.", 7f),
            new PicnicBeat("UM JARDIM POR DENTRO", "Lá fora, o jardim continuou do mesmo tamanho.\nDentro do sonho, uma folha já podia ser floresta.", 7f));
    }
}
