using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace RTFunctions.Functions.Animation.Keyframe
{
    /// <summary>
    /// A keyframe that animates a color value.
    /// </summary>
    public struct ColorKeyframe : IKeyframe<Color>
    {
        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public Color Value { get; set; }

        public ColorKeyframe(float time, Color value, EaseFunction ease)
        {
            Time = time;
            Value = value;
            Ease = ease;
        }

        public Color Interpolate(IKeyframe<Color> other, float time)
        {
            ColorKeyframe second = (ColorKeyframe)other;
            return Lerp(Value, second.Value, second.Ease(time));
        }

        Color Lerp(Color x, Color y, float t) => x + (y - x) * t;
    }
}
