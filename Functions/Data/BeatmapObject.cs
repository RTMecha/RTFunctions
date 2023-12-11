﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using UnityEngine;

using SimpleJSON;
using LSFunctions;

using RTFunctions.Functions.Components;
using RTFunctions.Functions.Optimization;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using BaseEventKeyframe = DataManager.GameData.EventKeyframe;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BasePrefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;
using BaseBackground = DataManager.GameData.BackgroundObject;
using BaseEditorData = DataManager.GameData.BeatmapObject.EditorData;

namespace RTFunctions.Functions.Data
{
	public class BeatmapObject : BaseBeatmapObject
	{
		public BeatmapObject()
		{

		}

		public BeatmapObject(bool active, float startTime, string name, int shape, string text, List<List<BaseEventKeyframe>> eventKeyframes) : base(active, startTime, name, shape, text, eventKeyframes)
		{

		}

		public BeatmapObject(float startTime) : base(startTime)
		{

		}

		public BeatmapObject(BaseBeatmapObject beatmapObject)
		{
			id = beatmapObject.id;
			parent = beatmapObject.parent;
			name = beatmapObject.name;
			active = beatmapObject.active;
			autoKillOffset = beatmapObject.autoKillOffset;
			autoKillType = beatmapObject.autoKillType;
			Depth = beatmapObject.Depth;
			editorData = new EditorData();
			editorData.Bin = beatmapObject.editorData.Bin;
			editorData.Layer = beatmapObject.editorData.Layer;
			editorData.collapse = beatmapObject.editorData.collapse;
			editorData.locked = beatmapObject.editorData.locked;
			fromPrefab = beatmapObject.fromPrefab;
			objectType = beatmapObject.objectType;
			origin = beatmapObject.origin;
			prefabID = beatmapObject.prefabID;
			prefabInstanceID = beatmapObject.prefabInstanceID;
			shape = beatmapObject.shape;
			shapeOption = beatmapObject.shapeOption;
			StartTime = beatmapObject.StartTime;
			text = beatmapObject.text;

			events = beatmapObject.events.Clone();
		}

		public BeatmapObject(BaseBeatmapObject beatmapObject, bool ldm, List<Modifier> modifiers)
		{
			id = beatmapObject.id;
			parent = beatmapObject.parent;
			name = beatmapObject.name;
			active = beatmapObject.active;
			autoKillOffset = beatmapObject.autoKillOffset;
			autoKillType = beatmapObject.autoKillType;
			Depth = beatmapObject.Depth;
			editorData = new EditorData();
			editorData.Bin = beatmapObject.editorData.Bin;
			editorData.Layer = beatmapObject.editorData.Layer;
			editorData.collapse = beatmapObject.editorData.collapse;
			editorData.locked = beatmapObject.editorData.locked;
			fromPrefab = beatmapObject.fromPrefab;
			objectType = beatmapObject.objectType;
			origin = beatmapObject.origin;
			prefabID = beatmapObject.prefabID;
			prefabInstanceID = beatmapObject.prefabInstanceID;
			shape = beatmapObject.shape;
			shapeOption = beatmapObject.shapeOption;
			StartTime = beatmapObject.StartTime;
			text = beatmapObject.text;

			var evs = new List<BaseEventKeyframe>[beatmapObject.events.Count];
			beatmapObject.events.CopyTo(evs);
			events = evs.ToList();

			LDM = ldm;
			this.modifiers = modifiers;
		}

		public bool LDM { get; set; }

		public float[] parallaxSettings = new float[3]
		{
			1f,
			1f,
			1f
		};

		public string parentAdditive = "000";

        public List<Modifier> modifiers = new List<Modifier>();
        public List<Component> components = new List<Component>();

        public int integerVariable;
        public float floatVariable;
        public string stringVariable = "";

		public new int Depth
        {
			get => depth;
			set => depth = value;
        }

        public class Modifier
        {
            public Modifier()
            {
            }

            public Modifier(Type _type, string _value)
            {
                type = _type;
                value = _value;
            }

            public Modifier(Type _type, string _command, string _value, BeatmapObject _beatmapObject)
            {
                commands[0] = _command;
                type = _type;
                value = _value;
                modifierObject = _beatmapObject;
            }

            public Modifier(BeatmapObject _beatmapObject)
            {
                modifierObject = _beatmapObject;
            }

            public BeatmapObject modifierObject;

            public bool constant = true;

            public enum Type
            {
                Trigger,
                Action
            }

            public Type type = Type.Action;
            public string value;
            public bool active = false;
            public List<string> commands = new List<string>
            {
				""
            };

            public bool not = false;

            public Action<Modifier> Action { get; set; }
            public Patchers.PrefixMethod<Modifier> Trigger { get; set; }

			public Action<Modifier> Inactive { get; set; }

            public object Result { get; set; }

			public bool hasChanged;

			#region Methods

			public static Modifier DeepCopy(Modifier orig)
			{
				var modifier = new Modifier();
				modifier.type = orig.type;
				modifier.commands = new List<string>();
				foreach (var l in orig.commands)
				{
					modifier.commands.Add(l);
				}
				modifier.value = orig.value;
				modifier.modifierObject = orig.modifierObject;
				modifier.not = orig.not;
				modifier.constant = orig.constant;

				return modifier;
			}

			public static Modifier Parse(JSONNode jn)
            {
				var modifier = new Modifier();
				modifier.type = (Type)jn["type"].AsInt;
				modifier.not = jn["not"].AsBool;

				modifier.commands.Clear();
				for (int i = 0; i < jn["commands"].Count; i++)
					modifier.commands.Add(jn["commands"][i]);

				modifier.constant = jn["const"].AsBool;
				if (!string.IsNullOrEmpty(jn["value"]))
					modifier.value = jn["value"];
				else
					modifier.value = "";

				return modifier;
            }

			public JSONNode ToJSON()
            {
				var jn = JSON.Parse("{}");

				jn["type"] = (int)type;

				if (not)
					jn["not"] = not.ToString();

				for (int j = 0; j < commands.Count; j++)
					jn["commands"][j] = commands[j];

				jn["value"] = value;

				jn["const"] = constant.ToString();

				return jn;
			}

			#endregion
		}

		#region Methods

		public static BeatmapObject DeepCopy(BeatmapObject orig, bool newID = true, bool copyVariables = true) => new BeatmapObject
        {
            id = newID ? LSText.randomString(16) : orig.id,
            parent = orig.parent,
            name = orig.name,
            active = orig.active,
            autoKillOffset = orig.autoKillOffset,
            autoKillType = orig.autoKillType,
            Depth = orig.Depth,
            editorData = new ObjectEditorData()
            {
                Bin = orig.editorData.Bin,
                Layer = orig.editorData.Layer,
                collapse = orig.editorData.collapse,
                locked = orig.editorData.locked
            },
            fromPrefab = orig.fromPrefab,
            objectType = orig.objectType,
            origin = orig.origin,
            prefabID = orig.prefabID,
            prefabInstanceID = orig.prefabInstanceID,
            shape = orig.shape,
            shapeOption = orig.shapeOption,
            StartTime = orig.StartTime,
            text = orig.text,
            LDM = orig.LDM,
            modifiers = orig.modifiers.Clone(),
			events = orig.events.Clone(),
			parentType = orig.parentType,
			parentOffsets = orig.parentOffsets,
			integerVariable = copyVariables ? orig.integerVariable : 0,
            floatVariable = copyVariables ? orig.floatVariable : 0f,
            stringVariable = copyVariables ? orig.stringVariable : ""
        };

        public static BeatmapObject Parse(JSONNode jn)
		{
			var beatmapObject = new BeatmapObject();

			var events = new List<List<BaseEventKeyframe>>();
			events.Add(new List<BaseEventKeyframe>());
			events.Add(new List<BaseEventKeyframe>());
			events.Add(new List<BaseEventKeyframe>());
			events.Add(new List<BaseEventKeyframe>());
			if (jn["events"] != null)
			{
				// Position
				for (int i = 0; i < jn["events"]["pos"].Count; i++)
				{
					var eventKeyframe = new EventKeyframe();
					var kfjn = jn["events"]["pos"][i];

					eventKeyframe.id = !string.IsNullOrEmpty(kfjn["id"]) ? kfjn["id"] : LSText.randomNumString(8);

					eventKeyframe.eventTime = kfjn["t"].AsFloat;

					if (kfjn["ct"] != null)
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					try
					{
						eventKeyframe.SetEventValues(new float[]
						{
							kfjn["x"].AsFloat,
							kfjn["y"].AsFloat,
							kfjn["z"].AsFloat
						});
					}
                    catch
					{
						eventKeyframe.SetEventValues(new float[]
						{
							// If all values end up as zero, then we definitely know Z axis didn't load for whatever reason.
							0f,
							0f,
							0f
						});
					}

					eventKeyframe.random = kfjn["r"].AsInt;
					eventKeyframe.SetEventRandomValues(new float[]
					{
							kfjn["rx"].AsFloat,
							kfjn["ry"].AsFloat,
							kfjn["rz"].AsFloat
					});

					eventKeyframe.relative = !string.IsNullOrEmpty(kfjn["rel"]) && kfjn["rel"].AsBool;

					eventKeyframe.active = false;
					events[0].Add(eventKeyframe);
				}

				// Scale
				for (int j = 0; j < jn["events"]["sca"].Count; j++)
				{
					var eventKeyframe = new EventKeyframe();
					var kfjn = jn["events"]["sca"][j];

					eventKeyframe.id = !string.IsNullOrEmpty(kfjn["id"]) ? kfjn["id"] : LSText.randomNumString(8);

					eventKeyframe.eventTime = kfjn["t"].AsFloat;

					if (kfjn["ct"] != null)
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.SetEventValues(new float[]
					{
						kfjn["x"].AsFloat,
						kfjn["y"].AsFloat
					});

					eventKeyframe.random = kfjn["r"].AsInt;
					eventKeyframe.SetEventRandomValues(new float[]
					{
						kfjn["rx"].AsFloat,
						kfjn["ry"].AsFloat,
						kfjn["rz"].AsFloat
					});

					eventKeyframe.relative = !string.IsNullOrEmpty(kfjn["rel"]) && kfjn["rel"].AsBool;

					eventKeyframe.active = false;
					events[1].Add(eventKeyframe);
				}
				
				// Rotation
				for (int k = 0; k < jn["events"]["rot"].Count; k++)
				{
					var eventKeyframe = new EventKeyframe();
					var kfjn = jn["events"]["rot"][k];

					eventKeyframe.id = !string.IsNullOrEmpty(kfjn["id"]) ? kfjn["id"] : LSText.randomNumString(8);

					eventKeyframe.eventTime = kfjn["t"].AsFloat;

					if (kfjn["ct"] != null)
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.SetEventValues(new float[]
					{
						kfjn["x"].AsFloat
					});

					eventKeyframe.random = kfjn["r"].AsInt;
					eventKeyframe.SetEventRandomValues(new float[]
					{
						kfjn["rx"].AsFloat,
						0f,
						kfjn["rz"].AsFloat
					});

					eventKeyframe.relative = string.IsNullOrEmpty(kfjn["rel"]) || kfjn["rel"].AsBool;

					eventKeyframe.active = false;
					events[2].Add(eventKeyframe);
				}

				// Color
				for (int l = 0; l < jn["events"]["col"].Count; l++)
				{
					var eventKeyframe = new EventKeyframe();
					var kfjn = jn["events"]["col"][l];

					eventKeyframe.id = !string.IsNullOrEmpty(kfjn["id"]) ? kfjn["id"] : LSText.randomNumString(8);

					eventKeyframe.eventTime = kfjn["t"].AsFloat;

					if (kfjn["ct"] != null)
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.SetEventValues(new float[]
					{
						kfjn["x"].AsFloat,
						kfjn["y"].AsFloat,
						kfjn["z"].AsFloat,
						kfjn["x2"].AsFloat,
						kfjn["y2"].AsFloat,
					});
					
					eventKeyframe.random = kfjn["r"].AsInt;
					eventKeyframe.SetEventRandomValues(new float[]
					{
							kfjn["rx"].AsFloat
					});

					eventKeyframe.active = false;
					events[3].Add(eventKeyframe);
				}
			}

			beatmapObject.events = events;

			beatmapObject.id = jn["id"] != null ? jn["id"] : LSText.randomString(16);

			if (jn["piid"] != null)
				beatmapObject.prefabInstanceID = jn["piid"];

			if (jn["pid"] != null)
				beatmapObject.prefabID = jn["pid"];

			if (jn["p"] != null)
				beatmapObject.parent = jn["p"];

			if (jn["pt"] != null)
				beatmapObject.parentType = jn["pt"];

			if (jn["po"] != null)
			{
				beatmapObject.parentOffsets = new List<float>(from n in jn["po"].AsArray.Children
															  select n.AsFloat).ToList();
			}

			if (jn["ps"] != null)
            {
				for (int i = 0; i < beatmapObject.parallaxSettings.Length; i++)
                {
					if (jn["ps"].Count > i && jn["ps"][i] != null)
						beatmapObject.parallaxSettings[i] = jn["ps"][i].AsFloat;
                }
            }

			if (jn["pa"] != null)
				beatmapObject.parentAdditive = jn["pa"];

			if (jn["d"] != null)
				beatmapObject.depth = jn["d"].AsInt;

			if (jn["empty"] != null)
				beatmapObject.objectType = jn["empty"].AsBool ? ObjectType.Empty : ObjectType.Normal;
			else if (jn["h"] != null)
				beatmapObject.objectType = jn["h"].AsBool ? ObjectType.Helper : ObjectType.Normal;
			else if (jn["ot"] != null)
				beatmapObject.objectType = (ObjectType)jn["ot"].AsInt;

			if (jn["ldm"] != null)
				beatmapObject.LDM = jn["ldm"].AsBool;

			if (jn["st"] != null)
				beatmapObject.startTime = jn["st"].AsFloat;

			if (jn["name"] != null)
				beatmapObject.name = jn["name"];

			if (jn["s"] != null)
				beatmapObject.shape = jn["s"].AsInt;

			if (jn["shape"] != null)
				beatmapObject.shape = jn["shape"].AsInt;

			if (jn["so"] != null)
				beatmapObject.shapeOption = jn["so"].AsInt;

			if (jn["text"] != null)
				beatmapObject.text = jn["text"];

			if (jn["ak"] != null)
				beatmapObject.autoKillType = jn["ak"].AsBool ? AutoKillType.LastKeyframe : AutoKillType.OldStyleNoAutokill;
			else if (jn["akt"] != null)
				beatmapObject.autoKillType = (AutoKillType)jn["akt"].AsInt;

			if (jn["ako"] != null)
				beatmapObject.autoKillOffset = jn["ako"].AsFloat;

			if (jn["o"] != null)
				beatmapObject.origin = new Vector2(jn["o"]["x"].AsFloat, jn["o"]["y"].AsFloat);

			if (jn["ed"]["bin"] != null)
				beatmapObject.editorData.locked = jn["ed"]["locked"].AsBool;

			if (jn["ed"]["bin"] != null)
				beatmapObject.editorData.collapse = jn["ed"]["shrink"].AsBool;

			if (jn["ed"]["bin"] != null)
				beatmapObject.editorData.Bin = jn["ed"]["bin"].AsInt;

			if (jn["ed"]["layer"] != null)
				beatmapObject.editorData.Layer = jn["ed"]["layer"].AsInt;

			for (int i = 0; i < jn["modifiers"].Count; i++)
			{
				var modifier = Modifier.Parse(jn["modifiers"][i]);
				modifier.modifierObject = beatmapObject;
				beatmapObject.modifiers.Add(modifier);
			}

			return beatmapObject;
		}

		public JSONNode ToJSON()
        {
			var jn = JSON.Parse("{}");

			jn["id"] = id;
			if (!string.IsNullOrEmpty(prefabID))
				jn["pid"] = prefabID;

			if (!string.IsNullOrEmpty(prefabInstanceID))
				jn["piid"] = prefabInstanceID;

			if (GetParentType() != "101")
				jn["pt"] = GetParentType();

			if (getParentOffsets().FindIndex(x => x != 0f) != -1)
			{
				int num4 = 0;
				foreach (float num5 in getParentOffsets())
				{
					jn["po"][num4] = num5.ToString();
					num4++;
				}
			}

			if (parallaxSettings.ToList().FindIndex(x => x != 1f) != -1)
            {
				for (int i = 0; i < parallaxSettings.Length; i++)
					jn["ps"][i] = parallaxSettings[i].ToString();
            }

			if (parentAdditive != "111")
				jn["pa"] = parentAdditive;

			jn["p"] = parent.ToString();

			jn["d"] = Depth.ToString();

			if (LDM)
				jn["ldm"] = LDM.ToString();

			jn["st"] = StartTime.ToString();

			if (!string.IsNullOrEmpty(name))
				jn["name"] = name;

			jn["ot"] = (int)objectType;
			jn["akt"] = (int)autoKillType;
			jn["ako"] = autoKillOffset;

			if (shape != 0)
				jn["shape"] = shape.ToString();

			if (shapeOption != 0)
				jn["so"] = shapeOption.ToString();

			if (!string.IsNullOrEmpty(text))
				jn["text"] = text;

			jn["o"]["x"] = origin.x.ToString();
			jn["o"]["y"] = origin.y.ToString();
			if (editorData.locked)
				jn["ed"]["locked"] = editorData.locked.ToString();
			if (editorData.collapse)
				jn["ed"]["shrink"] = editorData.collapse.ToString();

			jn["ed"]["bin"] = editorData.Bin.ToString();
			jn["ed"]["layer"] = editorData.Layer.ToString();

			for (int i = 0; i < events[0].Count; i++)
				jn["events"]["pos"][i] = ((EventKeyframe)events[0][i]).ToJSON();
			for (int i = 0; i < events[1].Count; i++)
				jn["events"]["sca"][i] = ((EventKeyframe)events[1][i]).ToJSON();
			for (int i = 0; i < events[2].Count; i++)
				jn["events"]["rot"][i] = ((EventKeyframe)events[2][i]).ToJSON();
			for (int i = 0; i < events[3].Count; i++)
				jn["events"]["col"][i] = ((EventKeyframe)events[3][i]).ToJSON();

			for (int i = 0; i < modifiers.Count; i++)
				jn["modifiers"][i] = modifiers[i].ToJSON();

			return jn;
		}

        #endregion
        
        #region Operators

        public static implicit operator bool(BeatmapObject exists) => exists != null;

        //public static bool operator ==(BeatmapObject a, BeatmapObject b) => a && b && a.id == b.id;
        //public static bool operator !=(BeatmapObject a, BeatmapObject b) => a == null || b == null || a.id != b.id;

        public override bool Equals(object obj) => obj is BeatmapObject && this == (BeatmapObject)obj;

        public override string ToString() => id;

        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }

}