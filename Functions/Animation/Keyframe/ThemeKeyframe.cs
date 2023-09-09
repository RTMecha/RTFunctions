using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace RTFunctions.Functions.Animation.Keyframe
{
    /// <summary>
    /// A keyframe that animates a color value.
    /// </summary>
    public struct ThemeKeyframe : IKeyframe<Color>
    {
        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public int Value { get; set; }

        List<Color> Theme
        {
            get
            {
                if (EditorManager.inst != null && EventEditor.inst.showTheme)
                    return EventEditor.inst.previewTheme.objectColors;
                return GameManager.inst.LiveTheme.objectColors;
            }
        }


        public ThemeKeyframe(float time, int value, EaseFunction ease)
        {
            Time = time;
            Value = value;
            Ease = ease;
        }

        public Color Interpolate(IKeyframe<Color> other, float time)
        {
            ThemeKeyframe second = (ThemeKeyframe)other;
            return Lerp(Theme[Value], Theme[second.Value], second.Ease(time));
        }

        Color Lerp(Color x, Color y, float t) => x + (y - x) * t;
    }
}
