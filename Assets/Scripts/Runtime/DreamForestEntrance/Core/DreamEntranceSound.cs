using System;

namespace YunaLegend.DreamForestEntrance.Core
{
    /// <summary>Seeded one-second noise loop and two quiet partials from the authored silence layer.</summary>
    public static class DreamEntranceSound
    {
        public static float[] CreateSamples(int sampleRate)
        {
            if (sampleRate<8000) throw new ArgumentOutOfRangeException(nameof(sampleRate));
            var samples=new float[sampleRate];
            double omega=2*Math.PI*1100/sampleRate;
            double alpha=Math.Sin(omega)/(2*.6);
            double a0=1+alpha;
            double b0=alpha/a0;
            double b2=-alpha/a0;
            double a1=-2*Math.Cos(omega)/a0;
            double a2=(1-alpha)/a0;
            double x1=0,x2=0,y1=0,y2=0;
            uint seed=173;
            for(int i=0;i<samples.Length;i++)
            {
                seed=unchecked(seed*1664525+1013904223);
                double x=seed/2147483648.0-1;
                double y=b0*x+b2*x2-a1*y1-a2*y2;
                x2=x1;
                x1=x;
                y2=y1;
                y1=y;
                double t=i/(double)sampleRate;
                samples[i]=(float)(y*.018+Math.Sin(t*2*Math.PI*2450)*.008+Math.Sin(t*2*Math.PI*3120)*.004);
            }
            return samples;
        }

        public static float MusicMix(int beat, float age)
        {
            return beat==6 ? Math.Max(0,Math.Min(1,(age-2.8f)/1.2f)) : 0;
        }
    }
}
