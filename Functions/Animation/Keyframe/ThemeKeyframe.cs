using System.Collections.Generic;
using UnityEngine;
using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Animation.Keyframe
{
    /// <summary>
    /// A keyframe that animates a (theme) color value.
    /// </summary>
    public struct ThemeKeyframe : IKeyframe<Color>
    {
        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public int Value { get; set; }

        List<Color> Theme => RTHelpers.BeatmapTheme.objectColors;

        public ThemeKeyframe(float time, int value, EaseFunction ease)
        {
            Time = time;
            Value = value;
            Ease = ease;
        }

        public Color Interpolate(IKeyframe<Color> other, float time)
        {
            var second = (ThemeKeyframe)other;
            return RTMath.Lerp(Theme[Value], Theme[second.Value], second.Ease(time));
        }
    }
}
