using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using RTFunctions.Functions.Animation.Keyframe;

namespace RTFunctions.Functions.Animation
{
    public static class AnimationExtensions
    {
        public static AnimationManager.Animation ToAnimation(this List<DataManager.GameData.EventKeyframe> eventKeyframe)
        {
            var animation = new AnimationManager.Animation("");

            return animation;
        }

        public static List<IKeyframe<float>> ToKeyframeFloat(this List<DataManager.GameData.EventKeyframe> eventKeyframes, bool relative)
        {
            var keyframes = new List<IKeyframe<float>>(eventKeyframes.Count);

            var currentValue = 0f;
            foreach (var eventKeyframe in eventKeyframes)
            {
                var value = eventKeyframe.eventValues[0];
                if (eventKeyframe.random != 0)
                    value = ObjectManager.inst.RandomFloatParser(eventKeyframe);

                if (eventKeyframe is Data.EventKeyframe)
                    currentValue = ((Data.EventKeyframe)eventKeyframe).relative ? currentValue + value : value;
                else
                    currentValue = relative ? currentValue + value : value;

                keyframes.Add(new FloatKeyframe(eventKeyframe.eventTime, value, Ease.GetEaseFunction(eventKeyframe.curveType.Name)));
            }
            return keyframes;
        }

        public static List<IKeyframe<Vector2>> ToKeyframeVector2(this List<DataManager.GameData.EventKeyframe> eventKeyframes, bool relative)
        {
            var keyframes = new List<IKeyframe<Vector2>>(eventKeyframes.Count);

            var currentValue = Vector2.zero;
            foreach (var eventKeyframe in eventKeyframes)
            {
                var value = new Vector2(eventKeyframe.eventValues[0], eventKeyframe.eventValues[1]);
                if (eventKeyframe.random != 0)
                    value = ObjectManager.inst.RandomVector2Parser(eventKeyframe);

                if (eventKeyframe is Data.EventKeyframe)
                    currentValue = ((Data.EventKeyframe)eventKeyframe).relative ? currentValue + value : value;
                else
                    currentValue = relative ? currentValue + value : value;

                keyframes.Add(new Vector2Keyframe(eventKeyframe.eventTime, value, Ease.GetEaseFunction(eventKeyframe.curveType.Name)));
            }
            return keyframes;
        }

        public static List<IKeyframe<Vector3>> ToKeyframeVector3(this List<DataManager.GameData.EventKeyframe> eventKeyframes, bool relative)
        {
            var keyframes = new List<IKeyframe<Vector3>>(eventKeyframes.Count);

            var currentValue = Vector3.zero;
            foreach (var eventKeyframe in eventKeyframes)
            {
                var value = new Vector3(eventKeyframe.eventValues[0], eventKeyframe.eventValues[1], eventKeyframe.eventValues[2]);
                if (eventKeyframe.random != 0)
                    value = ObjectManager.inst.RandomVector2Parser(eventKeyframe);

                if (eventKeyframe is Data.EventKeyframe)
                    currentValue = ((Data.EventKeyframe)eventKeyframe).relative ? currentValue + value : value;
                else
                    currentValue = relative ? currentValue + value : value;

                keyframes.Add(new Vector3Keyframe(eventKeyframe.eventTime, value, Ease.GetEaseFunction(eventKeyframe.curveType.Name)));
            }
            return keyframes;
        }
    }
}
