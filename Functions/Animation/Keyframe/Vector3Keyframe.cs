using UnityEngine;
using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Animation.Keyframe
{
    /// <summary>
    /// A keyframe that animates a Vector3 value.
    /// </summary>
    public struct Vector3Keyframe : IKeyframe<Vector3>
    {
        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public Vector3 Value { get; set; }

        public Vector3Keyframe(float time, Vector3 value, EaseFunction ease)
        {
            Time = time;
            Value = value;
            Ease = ease;
        }

        public Vector3 Interpolate(IKeyframe<Vector3> other, float time)
        {
            var second = (Vector3Keyframe)other;
            return RTMath.Lerp(Value, second.Value, second.Ease(time));
        }
    }
}

