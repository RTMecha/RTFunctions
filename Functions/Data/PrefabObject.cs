﻿using LSFunctions;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using BasePrefabObject = DataManager.GameData.PrefabObject;

namespace RTFunctions.Functions.Data
{
    public class PrefabObject : BasePrefabObject
    {
        public PrefabObject() : base()
        {
            events = new List<DataManager.GameData.EventKeyframe>
            {
                new EventKeyframe(),
                new EventKeyframe(),
                new EventKeyframe()
            };
        }

        public PrefabObject(string name, float startTime) : base(name, startTime)
        {
            events = new List<DataManager.GameData.EventKeyframe>
            {
                new EventKeyframe(),
                new EventKeyframe(),
                new EventKeyframe()
            };
        }

        public PrefabObject(BasePrefabObject prefabObject)
        {
            prefabID = prefabObject.prefabID;
        }

        public float speed = 1f;

        public Prefab Prefab => (Prefab)DataManager.inst.gameData.prefabs.Find(x => x.ID == prefabID);

        public List<BeatmapObject> ExpandedObjects => DataManager.inst.gameData.beatmapObjects.Where(x => x is BeatmapObject && x.fromPrefab && x.prefabInstanceID == ID).Select(x => x as BeatmapObject).ToList();

        public enum AutoKillType
        {
            Regular,
            StartTimeOffset,
            SongTime
        }

        public AutoKillType autoKillType = AutoKillType.Regular;

        public float autoKillOffset = -1f;

        public bool fromModifier;

        #region Methods

        public static PrefabObject DeepCopy(PrefabObject orig, bool _newID = true)
        {
            var prefabObject = new PrefabObject
            {
                active = orig.active,
                ID = _newID ? LSText.randomString(16) : orig.ID,
                prefabID = orig.prefabID,
                startTime = orig.StartTime,
                repeatCount = orig.repeatCount,
                repeatOffsetTime = orig.repeatOffsetTime,
                editorData = new ObjectEditorData
                {
                    Bin = orig.editorData.Bin,
                    layer = orig.editorData.layer,
                    locked = orig.editorData.locked,
                    collapse = orig.editorData.collapse
                },
                speed = orig.speed,
                autoKillOffset = orig.autoKillOffset,
                autoKillType = orig.autoKillType
            };

            if (prefabObject.events == null)
                prefabObject.events = new List<DataManager.GameData.EventKeyframe>();
            prefabObject.events.Clear();

            if (orig.events != null)
                foreach (var eventKeyframe in orig.events)
                    prefabObject.events.Add(EventKeyframe.DeepCopy((EventKeyframe)eventKeyframe, _newID));

            return prefabObject;
        }

        public static PrefabObject ParseVG(JSONNode jn)
        {
            var prefabObject = new PrefabObject();

            prefabObject.ID = jn["id"];
            prefabObject.prefabID = jn["pid"];
            prefabObject.StartTime = jn["t"] == null ? jn["st"].AsFloat : jn["t"].AsFloat;

            prefabObject.editorData = ObjectEditorData.ParseVG(jn["ed"]);

            prefabObject.events.Clear();

            if (jn["e"] != null)
            {
                try
                {
                    prefabObject.events.Add(new EventKeyframe
                    {
                        eventValues = new float[2]
                        {
                            jn["e"][0]["ev"][0].AsFloat,
                            jn["e"][0]["ev"][1].AsFloat,
                        }
                    });
                }
                catch (System.Exception)
                {
                    prefabObject.events.Add(new EventKeyframe
                    {
                        eventValues = new float[2]
                        {
                            0f,
                            0f,
                        }
                    });
                }

                try
                {
                    prefabObject.events.Add(new EventKeyframe
                    {
                        eventValues = new float[2]
                        {
                            jn["e"][1]["ev"][0].AsFloat,
                            jn["e"][1]["ev"][1].AsFloat,
                        }
                    });
                }
                catch (System.Exception)
                {
                    prefabObject.events.Add(new EventKeyframe
                    {
                        eventValues = new float[2]
                        {
                            0f,
                            0f,
                        }
                    });
                }

                try
                {
                    prefabObject.events.Add(new EventKeyframe
                    {
                        eventValues = new float[1]
                        {
                            jn["e"][1]["ev"][0].AsFloat,
                        }
                    });
                }
                catch (System.Exception)
                {
                    prefabObject.events.Add(new EventKeyframe
                    {
                        eventValues = new float[1]
                        {
                            0f,
                        }
                    });
                }
            }
            else
            {
                prefabObject.events.Add(new EventKeyframe(0f, new float[2] { 0f, 0f }, new float[3] { 0f, 0f, 0f }));
                prefabObject.events.Add(new EventKeyframe(0f, new float[2] { 0f, 0f }, new float[3] { 0f, 0f, 0f }));
                prefabObject.events.Add(new EventKeyframe(0f, new float[1] { 0f }, new float[3] { 0f, 0f, 0f }));
            }

            return prefabObject;
        }

        public static PrefabObject Parse(JSONNode jn)
        {
            var prefabObject = new PrefabObject();
            prefabObject.ID = jn["id"];
            prefabObject.prefabID = jn["pid"];
            prefabObject.StartTime = jn["st"].AsFloat;

            if (!string.IsNullOrEmpty(jn["rc"]))
                prefabObject.RepeatCount = jn["rc"].AsInt;

            if (!string.IsNullOrEmpty(jn["ro"]))
                prefabObject.RepeatOffsetTime = jn["ro"].AsFloat;

            prefabObject.ID = jn["id"] != null ? jn["id"] : LSText.randomString(16);

            if (jn["sp"] != null)
                prefabObject.speed = jn["sp"].AsFloat;

            if (jn["akt"] != null)
                prefabObject.autoKillType = (AutoKillType)jn["akt"].AsInt;

            if (jn["ako"] != null)
                prefabObject.autoKillOffset = jn["ako"].AsFloat;

            prefabObject.editorData = ObjectEditorData.Parse(jn["ed"]);

            prefabObject.events.Clear();

            if (jn["e"] != null)
            {
                if (jn["e"]["pos"] != null)
                {
                    var kf = new EventKeyframe();
                    var jnpos = jn["e"]["pos"];

                    kf.SetEventValues(new float[]
                    {
                        jnpos["x"].AsFloat,
                        jnpos["y"].AsFloat
                    });
                    kf.random = jnpos["r"].AsInt;
                    kf.SetEventRandomValues(new float[]
                    {
                        jnpos["rx"].AsFloat,
                        jnpos["ry"].AsFloat,
                        jnpos["rz"].AsFloat
                    });
                    kf.active = false;
                    prefabObject.events.Add(kf);
                }
                else
                {
                    prefabObject.events.Add(new EventKeyframe(new float[2] { 0f, 0f }, new float[3] { 0f, 0f, 0f }));
                }
                if (jn["e"]["sca"] != null)
                {
                    var kf = new EventKeyframe();
                    var jnsca = jn["e"]["sca"];
                    kf.SetEventValues(new float[]
                    {
                        jnsca["x"].AsFloat,
                        jnsca["y"].AsFloat
                    });
                    kf.random = jnsca["r"].AsInt;
                    kf.SetEventRandomValues(new float[]
                    {
                        jnsca["rx"].AsFloat,
                        jnsca["ry"].AsFloat,
                        jnsca["rz"].AsFloat
                    });
                    kf.active = false;
                    prefabObject.events.Add(kf);
                }
                else
                {
                    prefabObject.events.Add(new EventKeyframe(new float[2] { 1f, 1f }, new float[3] { 0f, 0f, 0f }));
                }
                if (jn["e"]["rot"] != null)
                {
                    var kf = new EventKeyframe();
                    var jnrot = jn["e"]["rot"];
                    kf.SetEventValues(new float[]
                    {
                        jnrot["x"].AsFloat
                    });
                    kf.random = jnrot["r"].AsInt;
                    kf.SetEventRandomValues(new float[]
                    {
                        jnrot["rx"].AsFloat,
                        0f,
                        jnrot["rz"].AsFloat
                    });
                    kf.active = false;
                    prefabObject.events.Add(kf);
                }
                else
                {
                    prefabObject.events.Add(new EventKeyframe(new float[1] { 0f }, new float[3] { 0f, 0f, 0f }));
                }
            }
            else
            {
                prefabObject.events = new List<DataManager.GameData.EventKeyframe>()
                {
                    new EventKeyframe(new float[2] { 0f, 0f }, new float[3] { 0f, 0f, 0f }),
                    new EventKeyframe(new float[2] { 1f, 1f }, new float[3] { 0f, 0f, 0f }),
                    new EventKeyframe(new float[1] { 0f }, new float[3] { 0f, 0f, 0f }),
                };
            }
            return prefabObject;
        }

        public JSONNode ToJSONVG()
        {
            var jn = JSON.Parse("{}");

            jn["id"] = ID;
            jn["pid"] = prefabID;

            jn["ed"] = ((ObjectEditorData)editorData).ToJSONVG();

            jn["e"][0]["ct"] = "Linear";
            jn["e"][0]["ev"][0] = events[0].eventValues[0];
            jn["e"][0]["ev"][1] = events[0].eventValues[1];

            jn["e"][1]["ct"] = "Linear";
            jn["e"][1]["ev"][0] = events[1].eventValues[0];
            jn["e"][1]["ev"][1] = events[1].eventValues[1];

            jn["e"][2]["ct"] = "Linear";
            jn["e"][2]["ev"][0] = events[2].eventValues[0];

            jn["t"] = StartTime;

            return jn;
        }

        public JSONNode ToJSON()
        {
            var jn = JSON.Parse("{}");

            jn["id"] = ID;
            jn["pid"] = prefabID;
            jn["st"] = StartTime.ToString();

            jn["sp"] = speed.ToString();

            jn["akt"] = ((int)autoKillType).ToString();

            jn["ako"] = autoKillOffset.ToString();

            if (RepeatCount > 0)
                jn["rc"] = RepeatCount.ToString();
            if (RepeatOffsetTime > 0f)
                jn["ro"] = RepeatOffsetTime.ToString();

            if (editorData.locked)
                jn["ed"]["locked"] = editorData.locked.ToString();
            if (editorData.collapse)
                jn["ed"]["shrink"] = editorData.collapse.ToString();

            jn["ed"]["layer"] = editorData.layer.ToString();
            jn["ed"]["bin"] = editorData.Bin.ToString();

            jn["e"]["pos"]["x"] = events[0].eventValues[0].ToString();
            jn["e"]["pos"]["y"] = events[0].eventValues[1].ToString();
            if (events[0].random != 0)
            {
                jn["e"]["pos"]["r"] = events[0].random.ToString();
                jn["e"]["pos"]["rx"] = events[0].eventRandomValues[0].ToString();
                jn["e"]["pos"]["ry"] = events[0].eventRandomValues[1].ToString();
                jn["e"]["pos"]["rz"] = events[0].eventRandomValues[2].ToString();
            }

            jn["e"]["sca"]["x"] = events[1].eventValues[0].ToString();
            jn["e"]["sca"]["y"] = events[1].eventValues[1].ToString();
            if (events[1].random != 0)
            {
                jn["e"]["sca"]["r"] = events[1].random.ToString();
                jn["e"]["sca"]["rx"] = events[1].eventRandomValues[0].ToString();
                jn["e"]["sca"]["ry"] = events[1].eventRandomValues[1].ToString();
                jn["e"]["sca"]["rz"] = events[1].eventRandomValues[2].ToString();
            }

            jn["e"]["rot"]["x"] = events[2].eventValues[0].ToString();
            if (events[1].random != 0)
            {
                jn["e"]["rot"]["r"] = events[2].random.ToString();
                jn["e"]["rot"]["rx"] = events[2].eventRandomValues[0].ToString();
                jn["e"]["rot"]["rz"] = events[2].eventRandomValues[2].ToString();
            }
            return jn;
        }

        #endregion

        #region Operators

        public static implicit operator bool(PrefabObject exists) => exists != null;

        public override bool Equals(object obj) => obj is PrefabObject && ID == (obj as PrefabObject).ID;

        public override string ToString() => ID;

        #endregion
    }
}
