﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.Animation.Keyframe
{
    /// <summary>
    /// A keyframe that animates a (theme) color value.
    /// </summary>
    public struct DynamicThemeKeyframe : IKeyframe<Color>
    {
        public bool Active { get; set; }

        public float Time { get; set; }
        public EaseFunction Ease { get; set; }
        public int Value { get; set; }

        public int Home { get; set; }

        public float Delay { get; set; }
        public float MinRange { get; set; }
        public float MaxRange { get; set; }
        public bool Flee { get; set; }

        List<Color> Theme => RTHelpers.BeatmapTheme.objectColors;

        public Sequence<Vector3> PositionSequence { get; set; }

        public Color Current { get; set; }

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

        public DynamicThemeKeyframe(float time, int value, EaseFunction ease, float delay, float min, float max, bool flee, int home, Sequence<Vector3> positionSequence)
        {
            Time = time;
            Value = value;
            Ease = ease;
            Active = false;
            Delay = delay;
            MinRange = min;
            MaxRange = max;
            Flee = flee;
            Home = home;
            PositionSequence = positionSequence;

            Current = RTHelpers.BeatmapTheme.objectColors[value];
        }

        public void Start()
        {

        }

        public Color Interpolate(IKeyframe<Color> other, float time)
        {
            var value = other is ThemeKeyframe vector3Keyframe ? vector3Keyframe.Value : other is DynamicThemeKeyframe dynamicVector3Keyframe ? dynamicVector3Keyframe.Value : 0;
            var ease = other is ThemeKeyframe vector3Keyframe1 ? vector3Keyframe1.Ease(time) : other is DynamicThemeKeyframe dynamicVector3Keyframe1 ? dynamicVector3Keyframe1.Ease(time) : 0f;

            var distance = Vector2.Distance(Player.position, PositionSequence.Value);

            float max = MaxRange < 0.01f ? 10f : MaxRange;
            float t = (-(distance + MinRange) + max) / max;

            float pitch = RTHelpers.Pitch;

            float p = UnityEngine.Time.deltaTime * pitch;

            float po = 1f - Mathf.Pow(1f - Delay == 0f ? 1f : Mathf.Clamp(Delay, 0.001f, 1f), p);

            Current += (RTMath.Lerp(RTMath.Lerp(Theme[Value], Theme[value], ease), Theme[Home], Mathf.Clamp(t, 0f, 1f)) - Current) * po;

            return Current;
        }
    }
}
