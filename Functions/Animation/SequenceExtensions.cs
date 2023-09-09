using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using RTFunctions.Functions.Animation.Keyframe;

using EventKeyframe = DataManager.GameData.EventKeyframe;

namespace RTFunctions.Functions.Animation
{
    public static class SequenceExtensions
    {
        public enum Axis
        {
            X, Y, Z
        }

        public static SequenceManager inst
        {
            get
            {
                return SequenceManager.Instance;
            }
        }

        public static Sequence<T> CreateSequence<T>(params IKeyframe<T>[] keyframes) => new Sequence<T>(keyframes);

        public static void Test()
        {
            var sequence = CreateSequence(new Vector3Keyframe(0f, Vector3.zero, Ease.Linear), new Vector3Keyframe(1f, new Vector3(0f, 10f, 0f), Ease.SineOut));

            
        }

        public static Sequence<Vector3> ToVector3Sequence(this List<EventKeyframe> eventKeyframes)
        {
            var keyframes = new List<IKeyframe<Vector3>>();
            foreach (var kf in eventKeyframes)
            {
                keyframes.Add(new Vector3Keyframe(kf.eventTime, new Vector3(kf.eventValues[0], kf.eventValues[1], kf.eventValues[2]), Ease.GetEaseFunction(kf.curveType.Name)));
            }
            return new Sequence<Vector3>(keyframes);
        }

        public static Sequence<Vector2> ToVector2Sequence(this Sequence<Vector3> sequence)
        {
            var keyframes = new List<IKeyframe<Vector2>>();
            foreach (var kf in sequence.keyframes)
            {
                var vectorKF = (Vector3Keyframe)kf;
                keyframes.Add(new Vector2Keyframe(vectorKF.Time, new Vector2(vectorKF.Value.x, vectorKF.Value.y), vectorKF.Ease));
            }
            return new Sequence<Vector2>(keyframes);
        }
        
        public static Sequence<Vector3> ToVector3Sequence(this Sequence<Vector2> sequence)
        {
            var keyframes = new List<IKeyframe<Vector3>>();
            foreach (var kf in sequence.keyframes)
            {
                var vectorKF = (Vector2Keyframe)kf;
                keyframes.Add(new Vector3Keyframe(vectorKF.Time, new Vector3(vectorKF.Value.x, vectorKF.Value.y, 1f), vectorKF.Ease));
            }
            return new Sequence<Vector3>(keyframes);
        }
        
        public static Sequence<Vector2> ToVector2Sequence(this List<EventKeyframe> eventKeyframes)
        {
            var keyframes = new List<IKeyframe<Vector2>>();
            foreach (var kf in eventKeyframes)
            {
                keyframes.Add(new Vector2Keyframe(kf.eventTime, new Vector2(kf.eventValues[0], kf.eventValues[1]), Ease.GetEaseFunction(kf.curveType.Name)));
            }
            return new Sequence<Vector2>(keyframes);
        }
        
        public static Sequence<float> ToFloatSequence(this List<EventKeyframe> eventKeyframes)
        {
            var keyframes = new List<IKeyframe<float>>();
            foreach (var kf in eventKeyframes)
            {
                keyframes.Add(new FloatKeyframe(kf.eventTime, kf.eventValues[0], Ease.GetEaseFunction(kf.curveType.Name)));
            }
            return new Sequence<float>(keyframes);
        }

        public static Sequence<Vector3> ToVector3Sequence(this Sequence<float> sequence, Axis axis)
        {
            var keyframes = new List<IKeyframe<Vector3>>();
            foreach (var kf in sequence.keyframes)
            {
                var vectorKF = (FloatKeyframe)kf;

                Vector3 vector3 = Vector3.zero;
                switch (axis)
                {
                    case Axis.X:
                        {
                            vector3 = new Vector3(vectorKF.Value, 0f, 0f);
                            break;
                        }
                    case Axis.Y:
                        {
                            vector3 = new Vector3(0f, vectorKF.Value, 0f);
                            break;
                        }
                    case Axis.Z:
                        {
                            vector3 = new Vector3(0f, 0f, vectorKF.Value);
                            break;
                        }
                }

                keyframes.Add(new Vector3Keyframe(vectorKF.Time, vector3, vectorKF.Ease));
            }
            return new Sequence<Vector3>(keyframes);
        }
        
        public static Sequence<Color> ToColorSequence(this List<EventKeyframe> eventKeyframes)
        {
            var keyframes = new List<IKeyframe<Color>>();
            foreach (var kf in eventKeyframes)
            {
                keyframes.Add(new ThemeKeyframe(kf.eventTime, (int)kf.eventValues[0], Ease.GetEaseFunction(kf.curveType.Name)));
            }
            return new Sequence<Color> (keyframes);
        }
        
        public static Sequence<float> ToOpacitySequence(this List<EventKeyframe> eventKeyframes)
        {
            var keyframes = new List<IKeyframe<float>>();
            foreach (var kf in eventKeyframes)
            {
                keyframes.Add(new FloatKeyframe(kf.eventTime, kf.eventValues[1], Ease.GetEaseFunction(kf.curveType.Name)));
            }
            return new Sequence<float> (keyframes);
        }
        
        public static Sequence<float> ToHueSequence(this List<EventKeyframe> eventKeyframes)
        {
            var keyframes = new List<IKeyframe<float>>();
            foreach (var kf in eventKeyframes)
            {
                keyframes.Add(new FloatKeyframe(kf.eventTime, kf.eventValues[2], Ease.GetEaseFunction(kf.curveType.Name)));
            }
            return new Sequence<float> (keyframes);
        }
        
        public static Sequence<float> ToSatSequence(this List<EventKeyframe> eventKeyframes)
        {
            var keyframes = new List<IKeyframe<float>>();
            foreach (var kf in eventKeyframes)
            {
                keyframes.Add(new FloatKeyframe(kf.eventTime, kf.eventValues[3], Ease.GetEaseFunction(kf.curveType.Name)));
            }
            return new Sequence<float> (keyframes);
        }
        
        public static Sequence<float> ToValSequence(this List<EventKeyframe> eventKeyframes)
        {
            var keyframes = new List<IKeyframe<float>>();
            foreach (var kf in eventKeyframes)
            {
                keyframes.Add(new FloatKeyframe(kf.eventTime, kf.eventValues[4], Ease.GetEaseFunction(kf.curveType.Name)));
            }
            return new Sequence<float> (keyframes);
        }

        public static void AnimateLocalPosition(this Transform tf, Sequence<Vector3> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            inst.AnimateLocalPosition(tf, sequence, length, action, onComplete);
        }

        public static void AnimateLocalScale(this Transform tf, Sequence<Vector3> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            inst.AnimateLocalScale(tf, sequence, length, action, onComplete);
        }

        public static void AnimateLocalRotation(this Transform tf, Sequence<Vector3> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            inst.AnimateLocalRotation(tf, sequence, length, action, onComplete);
        }

        public static void AnimateColor(this Material mat, Sequence<Color> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            inst.AnimateColor(mat, sequence, length, action, onComplete);
        }

    }
}
