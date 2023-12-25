using System.Linq;

using UnityEngine;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.Animation.Keyframe
{
    public struct DynamicVector2Keyframe : IKeyframe<Vector2>
    {
        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public Vector2 Value { get; set; }

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

        public DynamicVector2Keyframe(float time, Vector2 value, EaseFunction ease)
        {
            Time = time;
            Value = value;
            Ease = ease;
        }

        public Vector2 Interpolate(IKeyframe<Vector2> other, float time)
        {
            var second = (DynamicVector2Keyframe)other;
            return RTMath.Lerp(Value, Target?.localPosition.ToVector2() ?? Vector2.zero + second.Value, second.Ease(time));
        }
    }
}
