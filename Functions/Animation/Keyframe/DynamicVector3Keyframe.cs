using System.Linq;

using UnityEngine;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.Animation.Keyframe
{
    public struct DynamicVector3Keyframe : IKeyframe<Vector3>
    {
        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public Vector3 Value { get; set; }

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

        public DynamicVector3Keyframe(float time, Vector3 value, EaseFunction ease)
        {
            Time = time;
            Value = value;
            Ease = ease;
        }

        public Vector3 Interpolate(IKeyframe<Vector3> other, float time)
        {
            var secondValue = other is DynamicVector3Keyframe keyframe ? keyframe.Value : ((Vector3Keyframe)other).Value;
            var secondEase = other is DynamicVector3Keyframe keyframe1 ? keyframe1.Ease(time) : ((Vector3Keyframe)other).Ease(time);

            return RTMath.Lerp(Value, new Vector3(Target?.localPosition.x ?? 0f, Target?.localPosition.y ?? 0f, 0f) + secondValue, secondEase);
        }
    }
}
