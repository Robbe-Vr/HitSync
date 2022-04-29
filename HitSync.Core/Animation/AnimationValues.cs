using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitSync.Core.Animation
{
    internal static class AnimationValues
    {
        internal static float BRIGHTNESS_CEILING { get; set; } = 1.0f;

        internal static float COLOR_CHANGE_AGGRESSION { get; set; } = 0.05f;

        internal static int[] BASS_INTENSITY_COLOR { get; set; } = new int[] { 255, 0, 0 };

        internal static float BASS_AGGRESSION { get; set; } = 1.25f;

        internal static int BASS_INTENSITY_FIELD { get; set; } = 10;

        internal static float BASS_INTENSITY_THRESHOLD { get; set; }

        internal static float BRIGHTNESS_AGGRESSION { get; set; } = 0.02f;

        internal static int BRIGHTNESS_FIELD { get; set; } = 10;

        internal static float BRIGHTNESS_THRESHOLD { get; set; }

        internal static float UPPER_INTENSITY_THRESHOLD { get; set; }
    }
}
