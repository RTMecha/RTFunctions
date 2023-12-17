﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using RTFunctions.Functions.Animation.Keyframe;

namespace RTFunctions.Functions.Animation
{
    /// <summary>
    /// Sequence class. Stores, manages and interpolates between keyframes.
    /// </summary>
    public class Sequence<T>
    {
        public readonly IKeyframe<T>[] keyframes;

        public Sequence(IEnumerable<IKeyframe<T>> keyframes)
        {
            this.keyframes = keyframes.ToArray();
            Array.Sort(this.keyframes, (x, y) => x.Time.CompareTo(y.Time));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Interpolate(float time)
        {
            if (keyframes.Length == 0)
            {
                throw new NoKeyframeException("Cannot interpolate in an empty sequence!");
            }

            if (keyframes.Length == 1)
            {
                return ResultFromSingleKeyframe(keyframes[0]);
            }

            if (time < keyframes[0].Time)
            {
                return ResultFromSingleKeyframe(keyframes[0]);
            }

            if (time >= keyframes[keyframes.Length - 1].Time)
            {
                return ResultFromSingleKeyframe(keyframes[keyframes.Length - 1]);
            }

            int index = Search(time);
            IKeyframe<T> first = keyframes[index];
            IKeyframe<T> second = keyframes[index + 1];

            float t = Mathf.InverseLerp(first.Time, second.Time, time);
            return first.Interpolate(second, t);
        }

        // Binary search for the keyframe pair that contains the given time
        int Search(float time)
        {
            int low = 0;
            int high = keyframes.Length - 1;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                float midTime = keyframes[mid].Time;

                if (time < midTime)
                {
                    high = mid - 1;
                }
                else if (time > midTime)
                {
                    low = mid + 1;
                }
                else
                {
                    return mid;
                }
            }

            return low - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T ResultFromSingleKeyframe(IKeyframe<T> keyframe)
        {
            return keyframe.Interpolate(keyframe, 0.0f);
        }
    }
}
