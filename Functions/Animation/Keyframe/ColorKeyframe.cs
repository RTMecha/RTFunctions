﻿using UnityEngine;
using RTFunctions.Functions.IO;

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
            var second = (ColorKeyframe)other;
            return RTMath.Lerp(Value, second.Value, second.Ease(time));
        }
    }
}
