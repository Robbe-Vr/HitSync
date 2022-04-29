using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitSync.Core.Animation
{
    public enum ColorRangeMode
    {
        Simple,
        FrequencyDependant
    }

    internal class ColorRangeManager
    {
        private static readonly int[] defaultFinalColorPoint = { 255, 255, 255 };

        private int pointToPointRangeSize = 1000;

        private static readonly float bassRangeFieldIndex = 15;
        private float bassRange = bassRangeFieldIndex / 2048;

        private static readonly float deepMidRangeFieldIndex = 45;
        private float deepMidRange = deepMidRangeFieldIndex / 2048;

        private static readonly float standardMidRangeFieldIndex = 70;
        private float standardMidRange = standardMidRangeFieldIndex / 2048;

        private static readonly float higherMidRangeFieldIndex = 120;
        private float higherMidRange = higherMidRangeFieldIndex / 2048;

        private static readonly float highestMidRangeFieldIndex = 200;
        private float highestMidRange = highestMidRangeFieldIndex / 2048;

        private static readonly float trebleRangeFieldIndex = 400;
        private float trebleRange = trebleRangeFieldIndex / 2048;

        private static readonly float highTrebleRangeFieldIndex = 2048;
        private float highTrebleRange = highTrebleRangeFieldIndex / 2048;


        internal ColorRangeMode RangeMode { get; private set; }
        internal int[][] Range { get; private set; }
        internal int Size { get { return Range?.Length ?? 0; } }

        internal ColorRangeManager(int rangeSize = 0)
        {
            if (rangeSize > 1)
            {
                pointToPointRangeSize = rangeSize;
            }
        }

        internal int[][] CreateFrequencyDependantColorRange(int[] bassColorStartingPoint, int[] deepMidColorStartingPoint, int[] standardMidColorStartingPoint, int[] higherMidColorStartingPoint, int[] highestMidColorStartingPoint, int[] trebleColorStartingPoint, int[] highTrebleColorStartingPoint, int[] finalColorPoint = null, bool useStaticBass = false)
        {
            if (finalColorPoint == null) finalColorPoint = defaultFinalColorPoint;

            List<int[]> range = new List<int[]>();

            int points = (int)(bassRange * pointToPointRangeSize);
            range.Add(bassColorStartingPoint);
            range.AddRange(
                useStaticBass ? CalculateBassRange(bassColorStartingPoint, deepMidColorStartingPoint, points) : CalculateRange(bassColorStartingPoint, deepMidColorStartingPoint, points)
            );

            points = (int)((deepMidRange - bassRange) * pointToPointRangeSize);
            range.Add(deepMidColorStartingPoint);
            range.AddRange(
                CalculateRange(deepMidColorStartingPoint, standardMidColorStartingPoint, points)
            );

            points = (int)((standardMidRange - deepMidRange) * pointToPointRangeSize);
            range.Add(standardMidColorStartingPoint);
            range.AddRange(
                CalculateRange(standardMidColorStartingPoint, higherMidColorStartingPoint, points)
            );

            points = (int)((higherMidRange - standardMidRange) * pointToPointRangeSize);
            range.Add(higherMidColorStartingPoint);
            range.AddRange(
                CalculateRange(higherMidColorStartingPoint, highestMidColorStartingPoint, points)
            );

            points = (int)((highestMidRange - higherMidRange) * pointToPointRangeSize);
            range.Add(highestMidColorStartingPoint);
            range.AddRange(
                CalculateRange(highestMidColorStartingPoint, trebleColorStartingPoint, points)
            );

            points = (int)((trebleRange - highestMidRange) * pointToPointRangeSize);
            range.Add(trebleColorStartingPoint);
            range.AddRange(
                CalculateRange(trebleColorStartingPoint, highTrebleColorStartingPoint, points)
            );

            points = (int)((highTrebleRange - trebleRange) * pointToPointRangeSize);
            range.Add(highTrebleColorStartingPoint);
            range.AddRange(
                CalculateRange(highTrebleColorStartingPoint, finalColorPoint, points)
            );

            RangeMode = ColorRangeMode.FrequencyDependant;

            return Range = range.ToArray();
        }

        internal int[][] CreateSimpleColorRange(params int[][] colorPoints)
        {
            List<int[]> range = new List<int[]>();

            int points = pointToPointRangeSize / colorPoints.Length;

            for (int i = 1; i < colorPoints.Length; i++)
            {
                range.Add(colorPoints[i - 1]);
                range.AddRange(
                    CalculateRange(colorPoints[i - 1], colorPoints[i], points)
                );
            }

            RangeMode = ColorRangeMode.Simple;

            return Range = range.ToArray();
        }

        private int[][] CalculateRange(int[] pointStart, int[] pointEnd, int pointsCount)
        {
            int[][] range = new int[pointsCount][];

            for (int i = 0; i < pointsCount; i++)
            {
                int redDiff = pointStart[0] - pointEnd[0];
                int greenDiff = pointStart[1] - pointEnd[1];
                int blueDiff = pointStart[2] - pointEnd[2];

                float factor = (float)i / pointsCount;

                range[i] = new int[]
                {
                    // red value
                    pointStart[0] - (int)(redDiff * factor),

                    // green value
                    pointStart[1] - (int)(greenDiff * factor),

                    // blue value
                    pointStart[2] - (int)(blueDiff * factor),
                };
            }

            return range;
        }

        private int[][] CalculateBassRange(int[] pointStart, int[] pointEnd, int pointsCount)
        {
            int[][] range = new int[pointsCount][];

            for (int i = 0; i < pointsCount; i++)
            {
                bool r = false;
                bool g = false;
                bool b = false;

                if (pointStart[0] > pointStart[1] && pointStart[0] > pointStart[2]) r = true;
                if (pointStart[1] > pointStart[0] && pointStart[1] > pointStart[2]) g = true;
                if (pointStart[2] > pointStart[0] && pointStart[2] > pointStart[1]) b = true;

                if (r && pointStart[0] == pointStart[1]) g = true;
                if (r && pointStart[0] == pointStart[2]) b = true;

                if (g && pointStart[1] == pointStart[0]) r = true;
                if (g && pointStart[1] == pointStart[2]) b = true;

                if (b && pointStart[2] == pointStart[0]) r = true;
                if (b && pointStart[2] == pointStart[1]) g = true;

                int redDiff = pointStart[0] - pointEnd[0];
                int greenDiff = pointStart[1] - pointEnd[1];
                int blueDiff = pointStart[2] - pointEnd[2];

                float factor = (float)(i / pointsCount) * 2;

                range[i] = new int[]
                {
                    // red value
                    pointStart[0] - (r && i >= pointsCount ? (int)(redDiff * factor) : 0),

                    // green value
                    pointStart[1] - (g && i >= pointsCount ? (int)(greenDiff * factor) : 0),

                    // blue value
                    pointStart[2] - (b && i >= pointsCount ? (int)(blueDiff * factor) : 0),
                };
            }

            return range;
        }
    }
}
