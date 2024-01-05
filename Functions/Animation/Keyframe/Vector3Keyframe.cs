﻿using UnityEngine;
using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Animation.Keyframe
{
    /// <summary>
    /// A keyframe that animates a Vector3 value.
    /// </summary>
    public struct Vector3Keyframe : IKeyframe<Vector3>
    {
        public bool Active { get; set; }

        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public Vector3 Value { get; set; }
        public IKeyframe<Vector3> PreviousKeyframe { get; set; }

        public Vector3Keyframe(float time, Vector3 value, EaseFunction ease, IKeyframe<Vector3> previousKeyframe = null)
        {
            Time = time;
            Value = value;
            Ease = ease;
            Active = false;
            PreviousKeyframe = previousKeyframe;
        }
        
        public void Start()
        {

        }

        public Vector3 Interpolate(IKeyframe<Vector3> other, float time)
        {
            var value = other is Vector3Keyframe vector3Keyframe ? vector3Keyframe.Value : other is DynamicVector3Keyframe dynamicVector3Keyframe ? dynamicVector3Keyframe.Value : other is StaticVector3Keyframe staticVector3Keyframe ? staticVector3Keyframe.Value : Vector3.zero;
            var ease = other is Vector3Keyframe vector3Keyframe1 ? vector3Keyframe1.Ease(time) : other is DynamicVector3Keyframe dynamicVector3Keyframe1 ? dynamicVector3Keyframe1.Ease(time) : other is StaticVector3Keyframe staticVector3Keyframe1 ? staticVector3Keyframe1.Ease(time) : 0f;

            var prevtarget = PreviousKeyframe != null && PreviousKeyframe is StaticVector3Keyframe ? ((StaticVector3Keyframe)PreviousKeyframe).Target : Vector2.zero;

            return RTMath.Lerp(new Vector3(prevtarget.x, prevtarget.y, 0f) + Value, value, ease);
        }
    }
}

