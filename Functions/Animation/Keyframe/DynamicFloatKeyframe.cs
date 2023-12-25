using System.Linq;

using UnityEngine;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.Animation.Keyframe
{
    public struct DynamicFloatKeyframe : IKeyframe<float>
    {
        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public float Value { get; set; }

        public Transform Target
        {
            get
            {
                if (PlayerManager.Players.Count > 0)
                {
                    var value = Value;
                    var player = PlayerManager.Players
                        .Where(x => x.Player && x.Player.transform.Find("Player"))
                        .OrderBy(x => x.Player.transform.Find("Player").localPosition.x)
                        .OrderBy(x => x.Player.transform.Find("Player").localPosition.y)
                        .ToList()[0];

                    if (player && player.Player)
                    {
                        return player.Player.transform.Find("Player");
                    }
                }

                return null;
            }
        }

        public Transform RefTransform { get; set; }

        public DynamicFloatKeyframe(float time, float value, EaseFunction ease, Transform refTransform)
        {
            Time = time;
            Value = value;
            Ease = ease;
            RefTransform = refTransform;
        }

        public float Interpolate(IKeyframe<float> other, float time)
        {
            var secondValue = other is DynamicFloatKeyframe keyframe ? keyframe.Value : ((FloatKeyframe)other).Value;
            var secondEase = other is DynamicFloatKeyframe keyframe1 ? keyframe1.Ease(time) : ((FloatKeyframe)other).Ease(time);

            return RTMath.Lerp(Value, RTMath.VectorAngle(RefTransform?.localPosition ?? Vector3.zero, Target?.localPosition ?? Vector3.zero) + secondValue, secondEase);
        }
    }
}
