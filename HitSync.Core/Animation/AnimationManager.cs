using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitSync.Core.Animation
{
    public enum AnimationMode
    {
        Intensity,
        Colorful,
    }

    internal static partial class AnimationManager
    {
        internal static AnimationMode Mode { get; private set; } = AnimationMode.Colorful;

        private static int maxColorRangeChange
        {
            get
            {
                return (int)Math.Round(
                    (Mode == AnimationMode.Intensity ?
                        _intensityColorRangeManager.Size
                        :
                     Mode == AnimationMode.Colorful ?
                        _colorfulColorRangeManager.Size
                        :
                        0
                    )
                    * AnimationValues.COLOR_CHANGE_AGGRESSION);
            }
        }

        private static int colorLHis;
        private static int maxColorLUp { get { return colorLHis + maxColorRangeChange; } }
        private static int maxColorLDown { get { return colorLHis - maxColorRangeChange; } }

        private static int colorMHis;
        private static int maxColorMUp { get { return colorMHis + maxColorRangeChange; } }
        private static int maxColorMDown { get { return colorMHis - maxColorRangeChange; } }

        private static int colorRHis;
        private static int maxColorRUp { get { return colorRHis + maxColorRangeChange; } }
        private static int maxColorRDown { get { return colorRHis - maxColorRangeChange; } }

        internal static void SetAnimationMode(AnimationMode mode)
        {
            Mode = mode;
        }

        internal static int[][] CreateSimpleColorRange(params int[][] colorPoints)
        {
            return Mode switch
            {
                AnimationMode.Intensity => _intensityColorRangeManager.CreateSimpleColorRange(colorPoints),
                AnimationMode.Colorful => _colorfulColorRangeManager.CreateSimpleColorRange(colorPoints),
                _ => Array.Empty<int[]>(),
            };
        }

        internal static int[][] CreateFrequencyDependantColorRange(int[] bassColorStartingPoint, int[] deepMidColorStartingPoint, int[] standardMidColorStartingPoint, int[] higherMidColorStartingPoint, int[] highestMidColorStartingPoint, int[] trebleColorStartingPoint, int[] highTrebleColorStartingPoint, int[] finalColorPoint = null)
        {
            return Mode switch
            {
                AnimationMode.Intensity => _intensityColorRangeManager.CreateFrequencyDependantColorRange(bassColorStartingPoint, deepMidColorStartingPoint, standardMidColorStartingPoint, higherMidColorStartingPoint, highestMidColorStartingPoint, trebleColorStartingPoint, highTrebleColorStartingPoint, finalColorPoint),
                AnimationMode.Colorful => _colorfulColorRangeManager.CreateFrequencyDependantColorRange(bassColorStartingPoint, deepMidColorStartingPoint, standardMidColorStartingPoint, higherMidColorStartingPoint, highestMidColorStartingPoint, trebleColorStartingPoint, highTrebleColorStartingPoint, finalColorPoint),
                _ => Array.Empty<int[]>(),
            };
        }

        internal static void CalculateAnimation(double[][] input, double masterPeak) // Older animation
        {
            switch (Mode)
            {
                case AnimationMode.Intensity:
                    CalculateIntensityAnimation(input, masterPeak);
                    return;

                case AnimationMode.Colorful:
                    CalculateColorfulAnimation(input, masterPeak);
                    return;
            }
        }
    }
}
