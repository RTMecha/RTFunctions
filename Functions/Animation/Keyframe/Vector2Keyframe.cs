using UnityEngine;
using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Animation.Keyframe
{
    /// <summary>
    /// A keyframe that animates a Vector2 value.
    /// </summary>
    public struct Vector2Keyframe : IKeyframe<Vector2>
    {
        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public Vector2 Value { get; set; }

        public Vector2Keyframe(float time, Vector2 value, EaseFunction ease)
        {
            Time = time;
            Value = value;
            Ease = ease;
        }

        public Vector2 Interpolate(IKeyframe<Vector2> other, float time)
        {
            var second = (Vector2Keyframe)other;
            return RTMath.Lerp(Value, second.Value, second.Ease(time));
        }
    }
}

