using System;

namespace HitSync.Core.Animation
{
    internal static partial class AnimationManager
    {
        private static ColorRangeManager _colorfulColorRangeManager = new ColorRangeManager();

        internal static void CalculateColorfulAnimation(double[][] input, double masterPeak) // Newer Animation
        {
            if (input == null) return;

            double[] fftDataL = input[0];
            double[] fftDataR = input[1];

            double maxL = -20.0;
            int indexMaxL = 0;
            Corrections[0] = -1;
            for (int i = 1; i < fftDataL.Length; i++)
            {
                double correction = GetCorrection(i);
                double value = fftDataL[i] - correction;

                Corrections[i] = value - 1;

                if (value > maxL)
                {
                    maxL = value;
                    indexMaxL = i;
                }
            }

            double briL = GetBrightness(fftDataL[indexMaxL], GetCorrection(indexMaxL));

            double maxR = -20.0;
            int indexMaxR = 0;
            for (int i = 1; i < fftDataR.Length; i++)
            {
                double correction = GetCorrection(i);
                double value = fftDataR[i] - correction;

                if (value > maxR)
                {
                    maxR = value;
                    indexMaxR = i;
                }
            }

            double briR = GetBrightness(fftDataR[indexMaxR], GetCorrection(indexMaxR));

            double briM = Math.Min(briL, briR);
            //double maxM = (maxL + maxR) / 2;
            float indexMaxM = (float)(indexMaxL + indexMaxR) / 2;

            int newColorL = (int)(((float)indexMaxL / fftDataL.Length) * (_colorfulColorRangeManager.Size - 1));
            int newColorM = (int)(((float)indexMaxM / fftDataL.Length) * (_colorfulColorRangeManager.Size - 1));
            int newColorR = (int)(((float)indexMaxR / fftDataR.Length) * (_colorfulColorRangeManager.Size - 1));

            if (newColorL > maxColorLUp) newColorL = maxColorLUp;
            if (newColorL < maxColorLDown) newColorL = maxColorLDown;

            if (newColorM > maxColorMUp) newColorM = maxColorMUp;
            if (newColorM < maxColorMDown) newColorM = maxColorMDown;

            if (newColorR > maxColorRUp) newColorR = maxColorRUp;
            if (newColorR < maxColorRDown) newColorR = maxColorRDown;

            colorLHis = newColorL;
            colorMHis = newColorM;
            colorRHis = newColorR;

            int[] colorL = _colorfulColorRangeManager.Range[newColorL];
            int[] colorM = _colorfulColorRangeManager.Range[newColorM];
            int[] colorR = _colorfulColorRangeManager.Range[newColorR];

            EntertainmentOperator.SetLeft(briL, colorL[0], colorL[1], colorL[2]);

            EntertainmentOperator.SetMid(briM, colorM[0], colorM[1], colorM[2]);

            EntertainmentOperator.SetRight(briR, colorR[0], colorR[1], colorR[2]);


            EntertainmentOperator.SetMasterPeakLeft(1.0, 1.0, 1.0, 1.0, briL, colorL[0], colorL[1], colorL[2], (int)((masterPeak / 1.0) * 7));

            EntertainmentOperator.SetMasterPeakMid(1.0, 1.0, 1.0, 1.0, briM, colorM[0], colorM[1], colorM[2], (int)((masterPeak / 1.0) * 7));

            EntertainmentOperator.SetMasterPeakRight(1.0, 1.0, 1.0, 1.0, briR, colorR[0], colorR[1], colorR[2], (int)((masterPeak / 1.0) * 7));
        }

        internal static double[] Corrections = new double[2048];

        private static double GetCorrection(int i)
        {
            double correction = Math.Pow(i - 0.097, -0.3) - 0.113656;

            if (correction < 0) correction = 0;
            return correction;
        }

        private static double GetBrightness(double peak, double correction)
        {
            double newBri = peak / (correction * 2);

            if (correction < 0.4) newBri -= 0.7;

            if (newBri < maxbriJumpDown)
                newBri = maxbriJumpDown;

            if (double.IsNaN(newBri)) newBri = 0.0;

            brightnessHis = newBri;
            return newBri;
        }
    }
}
