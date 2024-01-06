﻿using System.Linq;

using UnityEngine;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.Animation.Keyframe
{
    public struct DynamicFloatKeyframe : IKeyframe<float>
    {
        public bool Active { get; set; }

        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public float Value { get; set; }
        public float OriginalValue { get; set; }

        public float Delay { get; set; }
        public float MinRange { get; set; }
        public float MaxRange { get; set; }
        public bool Flee { get; set; }

        public float Angle { get; set; }
        public float Angle360 { get; set; }

        public Vector3 Target { get; set; }

        public Sequence<Vector3> PositionSequence { get; set; }

        public Transform Player
        {
            get
            {
                if (PlayerManager.Players.Count > 0)
                {
                    var value = PositionSequence.Value;
                    var orderedList = PlayerManager.Players
                        .Where(x => x.Player && x.Player.transform.Find("Player"))
                        .OrderBy(x => Vector2.Distance(x.Player.transform.Find("Player").localPosition, value))
                        .ToList();

                    if (orderedList.Count > 0)
                    {
                        var player = orderedList[0];

                        if (player && player.Player)
                        {
                            return player.Player.transform.Find("Player");
                        }
                    }
                    return null;
                }

                return null;
            }
        }

        public DynamicFloatKeyframe(float time, float value, EaseFunction ease, float delay, float min, float max, bool flee, Sequence<Vector3> positionSequence)
        {
            Time = time;
            Value = value;
            OriginalValue = value;
            Ease = ease;
            Active = false;
            Delay = delay;
            MinRange = min;
            MaxRange = max;
            Flee = flee;
            Angle360 = 0f;
            Angle = 0f;
            Target = Vector3.zero;
            PositionSequence = positionSequence;
        }

        public void Start()
        {
            Value = OriginalValue;
            Target = Player?.localPosition ?? Vector3.zero;
            Angle360 = 0f;
            Angle = 0f;
        }

        public float Interpolate(IKeyframe<float> other, float time)
        {
            var secondValue = other is DynamicFloatKeyframe keyframe ? keyframe.Value : ((FloatKeyframe)other).Value;
            var secondEase = other is DynamicFloatKeyframe keyframe1 ? keyframe1.Ease(time) : ((FloatKeyframe)other).Ease(time);

            var vector = Player?.localPosition ?? Vector3.zero;
            var angle = -RTMath.VectorAngle(PositionSequence.Value, vector);

            // Calculation for rotation looping so it doesn't flip around with a lower delay.
            //if (Target.x < vector.x && vector.x > PositionSequence.Value.x && Angle > angle)
            //{
            //    Angle360 += 360f;
            //}
            //if (Target.x > vector.x && vector.x < PositionSequence.Value.x && Angle < angle)
            //{
            //    Angle360 -= 360f;
            //}

            Angle = angle + Angle360;
            Target = Player?.localPosition ?? Vector3.zero;

            float pitch = RTHelpers.Pitch;

            float p = UnityEngine.Time.deltaTime * pitch;

            float po = 1f - Mathf.Pow(1f - Mathf.Clamp(Delay, 0.001f, 1f), p);

            if (MinRange == 0f && MaxRange == 0f || Vector2.Distance(vector, PositionSequence.Value) > MinRange && Vector2.Distance(vector, PositionSequence.Value) < MaxRange)
                Value += Flee ? (Angle + Value) * po : (Angle - Value) * po;

            //return RTMath.Lerp(Value + OriginalValue, secondValue, secondEase);
            return Value;
        }
    }
}
