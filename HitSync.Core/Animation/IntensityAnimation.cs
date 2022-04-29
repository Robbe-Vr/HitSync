using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitSync.Core.Animation
{
    internal static partial class AnimationManager
    {
        private static ColorRangeManager _intensityColorRangeManager = new ColorRangeManager(500);

        private static double brightnessHis;
        private static double maxbriJumpDown { get { return brightnessHis - AnimationValues.BRIGHTNESS_AGGRESSION; } }

        private static double bassHis;
        private static double maxbassJumpDown { get { return bassHis - AnimationValues.BASS_AGGRESSION; } }

        internal static void CalculateIntensityAnimation(double[][] input, double masterPeak) // Older animation
        {
            if (input == null) return;

            double[] fftDataL = input[0];
            double[] fftDataR = input[1];

            double briL = fftDataL?.Length > 0 ? GetBrightness(fftDataL) : 0.0;
            double briR = fftDataR?.Length > 0 ? GetBrightness(fftDataR) : 0.0;

            double intensityL = fftDataL?.Length > 0 ? GetIntensity(fftDataL) : 0.0;
            double intensityR = fftDataR?.Length > 0 ? GetIntensity(fftDataR) : 0.0;

            double upperPeakL = fftDataL?.Length > 0 ? GetUpperPeak(fftDataL) * 0.92 : 0.0;
            double upperPeakR = fftDataR?.Length > 0 ? GetUpperPeak(fftDataR) * 0.92 : 0.0;

            double briM = Math.Min(briL, briR);
            double upperPeakM = Math.Max(upperPeakL, upperPeakR);
            double intensityM = Math.Max(intensityL, intensityR);

            double[] colorL = CalculateColor(intensityL, upperPeakL, 0);

            double[] colorM = CalculateColor(intensityM, upperPeakM, 2);

            double[] colorR = CalculateColor(intensityR, upperPeakR, 1);

            EntertainmentOperator.SetLeft(briL, colorL[0], colorL[1], colorL[2]);

            EntertainmentOperator.SetMid(briM, colorM[0], colorM[1], colorM[2]);

            EntertainmentOperator.SetRight(briR, colorR[0], colorR[1], colorR[2]);

            EntertainmentOperator.SetMasterPeakLeft(1.0, 1.0, 1.0, 1.0, briL, colorL[0], colorL[1], colorL[2], (int)((masterPeak / 1.0) * 7));

            EntertainmentOperator.SetMasterPeakMid(1.0, 1.0, 1.0, 1.0, briM, colorM[0], colorM[1], colorM[2], (int)((masterPeak / 1.0) * 7));

            EntertainmentOperator.SetMasterPeakRight(1.0, 1.0, 1.0, 1.0, briR, colorR[0], colorR[1], colorR[2], (int)((masterPeak / 1.0) * 7));
        }

        private static double[] CalculateColor(double intensity, double upperPeak, int channel)
        {
            double factor = (intensity / AnimationValues.BASS_AGGRESSION) / upperPeak;

            if (factor > 1.0) factor = 1.0;
            else if (factor < 0.0 || double.IsNaN(factor)) factor = 0.0;

            int newColor = (int)Math.Round((_intensityColorRangeManager.Size - 1) * factor);

            if (newColor > (channel == 0 ? maxColorLUp : channel == 1 ? maxColorRUp : maxColorMUp)) newColor = (channel == 0 ? maxColorLUp : channel == 1 ? maxColorRUp : maxColorMUp);
            if (newColor < (channel == 0 ? maxColorLDown : channel == 1 ? maxColorRDown : maxColorMDown)) newColor = (channel == 0 ? maxColorLDown : channel == 1 ? maxColorRDown : maxColorMDown);

            if (channel == 0) colorLHis = newColor;
            else if (channel == 1) colorRHis = newColor;
            else colorMHis = newColor;

            return _intensityColorRangeManager.Range[newColor].Select(x => x / 255.0).ToArray();
        }

        private static double GetUpperPeak(double[] fftData)
        {
            return (fftData.Skip(10).Max() / 3.00);
        }

        private static int[] MaximizeRGB(params int[] rgb)
        {
            int maxIndex = 0;
            int max = 0;
            for (int i = 0; i < 3; i++)
            {
                if (rgb[i] > max)
                {
                    max = rgb[i];
                    maxIndex = i;
                }
            }

            if (max > 0)
            {
                int color1 = maxIndex == 0 ? 1 : maxIndex == 1 ? 0 : 2;
                int color2 = maxIndex == 0 ? 2 : maxIndex == 1 ? 2 : 1;

                double factor1 = rgb[color1] / rgb[maxIndex];
                double factor2 = rgb[color2] / rgb[maxIndex];

                int[] maxedRgb = new int[3];

                maxedRgb[maxIndex] = 255;
                maxedRgb[color1] = (int)Math.Round(255 * factor1);
                maxedRgb[color2] = (int)Math.Round(255 * factor2);

                return maxedRgb;
            }

            return rgb;
        }

        private static double[] MaximizeRGB(params double[] rgb)
        {
            int maxIndex = 0;
            double max = 0.0;
            for (int i = 0; i < 3; i++)
            {
                if (rgb[i] > max)
                {
                    max = rgb[i];
                    maxIndex = i;
                }
            }


            if (max > 0.0)
            {
                int color1 = maxIndex == 0 ? 1 : maxIndex == 1 ? 0 : 2;
                int color2 = maxIndex == 0 ? 2 : maxIndex == 1 ? 2 : 1;

                double factor1 = rgb[color1] / rgb[maxIndex];
                double factor2 = rgb[color2] / rgb[maxIndex];

                double[] maxedRgb = new double[3];

                maxedRgb[maxIndex] = 255;
                maxedRgb[color1] = 255 * factor1;
                maxedRgb[color2] = 255 * factor2;

                return maxedRgb;
            }

            return rgb;
        }

        private static double GetIntensity(double[] fftData)
        {
            double newIntensity = fftData.Take(AnimationValues.BASS_INTENSITY_FIELD).Average() / 3.00;

            if (newIntensity < maxbassJumpDown)
                newIntensity = maxbassJumpDown;

            bassHis = newIntensity;
            return newIntensity;
        }

        private static double GetBrightness(double[] fftData)
        {
            double newBri = Math.Max(fftData.Take(AnimationValues.BRIGHTNESS_FIELD).Max() / 4.00, fftData.Skip(16).Take(44).Max() * 1.3);

            if (newBri > 1.0)
                newBri = 1.0;
            else if (newBri < 0.0)
                newBri = 0.0;

            if (newBri < maxbriJumpDown)
                newBri = maxbriJumpDown;

            brightnessHis = newBri;
            return newBri;
        }

        internal static int[][] GetIntensityColorRange()
        {
            return _intensityColorRangeManager.Range;
        }

        internal static int[][] GetColorfulColorRange()
        {
            return _colorfulColorRangeManager.Range;
        }
    }
}
