﻿using LSFunctions;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaseEventKeyframe = DataManager.GameData.EventKeyframe;

namespace RTFunctions.Functions.Data
{
    public class EventKeyframe : BaseEventKeyframe
    {
        public EventKeyframe() : base()
        {
            id = LSText.randomNumString(8);
        }
        
        public EventKeyframe(float[] eventValues, float[] eventRandomValues, int random = 0) : base(eventValues, eventRandomValues, random)
        {
            id = LSText.randomNumString(8);
        }
        
        public EventKeyframe(float eventTime, float[] eventValues, float[] eventRandomValues, int random = 0) : base(eventTime, eventValues, eventRandomValues, random)
        {
            id = LSText.randomNumString(8);
        }

        public EventKeyframe(BaseEventKeyframe eventKeyframe)
        {
            active = eventKeyframe.active;
            curveType = eventKeyframe.curveType;
            eventRandomValues = eventKeyframe.eventRandomValues;
            eventTime = eventKeyframe.eventTime;
            eventValues = eventKeyframe.eventValues;
            random = eventKeyframe.random;
            id = LSText.randomNumString(8);
        }

        public string id;
        public int index;
        public int type;
        public bool relative;

        public void SetCurve(string ease)
            => curveType = DataManager.inst.AnimationList.Has(x => x.Name == ease) ? DataManager.inst.AnimationList.Find(x => x.Name == ease) : DataManager.inst.AnimationList[0];
        public void SetCurve(int ease) => curveType = DataManager.inst.AnimationList[Mathf.Clamp(ease, 0, DataManager.inst.AnimationList.Count - 1)];
        public new void SetEventRandomValues(params float[] _vals)
        {
            if (_vals != null)
            {
                if (_vals.Length > eventRandomValues.Length)
                    eventRandomValues = new float[_vals.Length];
                for (int i = 0; i < _vals.Length; i++)
                    eventRandomValues[i] = _vals[i];
            }
        }

        #region Methods

        public static EventKeyframe DeepCopy(EventKeyframe eventKeyframe, bool newID = true) => new EventKeyframe
        {
            id = newID ? LSText.randomNumString(8) : eventKeyframe.id,
            active = eventKeyframe.active,
            curveType = eventKeyframe.curveType,
            eventRandomValues = eventKeyframe.eventRandomValues.ToList().Clone().ToArray(),
            eventTime = eventKeyframe.eventTime,
            eventValues = eventKeyframe.eventValues.ToList().Clone().ToArray(),
            random = eventKeyframe.random,
            relative = eventKeyframe.relative,
        };

        public static EventKeyframe Parse(JSONNode jn, int type, int valueCount)
        {
            var eventKeyframe = new EventKeyframe();

            eventKeyframe.eventTime = jn["t"].AsFloat;

            var curveType = DataManager.inst.AnimationList[0];
            if (jn["ct"] != null)
                curveType = DataManager.inst.AnimationListDictionaryStr[jn["ct"]];
            eventKeyframe.curveType = curveType;

            var eventValues = new List<float>();
            for (int i = 0; i < axis.Length; i++)
                if (jn[axis[i]] != null)
                    eventValues.Add(jn[axis[i]].AsFloat);

            while (eventValues.Count < valueCount)
                eventValues.Add(GameData.DefaultKeyframes[type].eventValues[eventValues.Count]);

            while (eventValues.Count > valueCount)
                eventValues.RemoveAt(eventValues.Count - 1);

            eventKeyframe.SetEventValues(eventValues.ToArray());

            var eventRandomValues = new List<float>();
            for (int i = 0; i < raxis.Length; i++)
                if (jn[raxis[i]] != null)
                    eventRandomValues.Add(jn[raxis[i]].AsFloat);

            eventKeyframe.random = jn["r"].AsInt;

            eventKeyframe.relative = !string.IsNullOrEmpty(jn["rel"]) && jn["rel"].AsBool;

            eventKeyframe.SetEventRandomValues(eventRandomValues.ToArray());

            return eventKeyframe;
        }

        public JSONNode ToJSON()
        {
            JSONNode jn = JSON.Parse("{}");
            jn["t"] = eventTime.ToString();

            for (int i = 0; i < eventValues.Length; i++)
                jn[axis[i]] = eventValues[i].ToString();

            if (curveType.Name != "Linear")
                jn["ct"] = curveType.Name;

            if (random != 0)
            {
                jn["r"] = random.ToString();
                for (int i = 0; i < eventRandomValues.Length; i++)
                    jn[raxis[i]] = eventRandomValues[i].ToString();
            }

            jn["rel"] = relative.ToString();

            return jn;
        }

        #endregion

        #region Operators

        public static implicit operator bool(EventKeyframe exists) => exists != null;

        public override string ToString()
        {
            string strs = "";
            for (int i = 0; i < eventValues.Length; i++)
            {
                strs += $"{eventValues[i]}";
                if (i != eventValues.Length - 1)
                    strs += ", ";
            }

            return $"{index}, {type}: {strs}";
        }

        public override bool Equals(object obj) => obj is EventKeyframe && id == (obj as EventKeyframe).id;

        #endregion

        static readonly string[] axis = new string[]
        {
            "x",
            "y",
            "z",
            "x2",
            "y2",
            "z2",
            "x3",
            "y3",
            "z3",
            "x4",
            "y4",
            "z4",
            "x5",
            "y5",
            "z5",
        };
        
        static readonly string[] raxis = new string[]
        {
            "rx",
            "ry",
            "rz",
            "rx2",
            "ry2",
            "rz2",
            "rx3",
            "ry3",
            "rz3",
            "rx4",
            "ry4",
            "rz4",
            "rx5",
            "ry5",
            "rz5",
        };
    }
}
