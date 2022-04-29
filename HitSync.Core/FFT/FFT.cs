using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord;
using NAudio;
using NAudio.Wave;

namespace HitSync.Core.FFT
{
    internal static class FFTHandler
    {
        public static double[][] PerformFFT(float[] data)
        {
            float[] pcmL = data.Where((x, index) => index % 2 == 0).Select(x => x * 100.00f).ToArray();
            float[] pcmR = data.Where((x, index) => index % 2 != 0).Select(x => x * 100.00f).ToArray();

            double[] fftL;
            double[] fftR;

            double[] fftRealL = new double[pcmL.Length / 2];
            double[] fftRealR = new double[pcmR.Length / 2];

            fftL = DoFastFourierTransformation(pcmL);
            fftR = DoFastFourierTransformation(pcmR);

            Array.Copy(fftL, fftRealL, fftRealL.Length);
            Array.Copy(fftR, fftRealR, fftRealR.Length);

            return new double[][] { fftRealL, fftRealR };
        }

        private static double[] DoFastFourierTransformation(float[] data)
        {
            System.Numerics.Complex[] fft = data.Select(x => new System.Numerics.Complex(x, 0.0)).ToArray();
            Accord.Math.FourierTransform.FFT(fft, Accord.Math.FourierTransform.Direction.Forward);

            return fft.Select(x => x.Magnitude).ToArray();
        }
    }
}
