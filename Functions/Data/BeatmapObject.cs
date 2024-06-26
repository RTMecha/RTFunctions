﻿using LSFunctions;
using RTFunctions.Functions.Components;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BaseEventKeyframe = DataManager.GameData.EventKeyframe;

namespace RTFunctions.Functions.Data
{
	public class BeatmapObject : BaseBeatmapObject
	{
		public BeatmapObject() : base()
		{
			editorData = new ObjectEditorData();
		}

		public BeatmapObject(bool active, float startTime, string name, int shape, string text, List<List<BaseEventKeyframe>> eventKeyframes) : base(active, startTime, name, shape, text, eventKeyframes)
		{
			editorData = new ObjectEditorData();
		}

		public BeatmapObject(float startTime) : base(startTime)
		{
			editorData = new ObjectEditorData();
		}

		public BeatmapObject(BaseBeatmapObject beatmapObject)
		{
			id = beatmapObject.id;
			parent = beatmapObject.parent;
			name = beatmapObject.name;
			active = beatmapObject.active;
			autoKillOffset = beatmapObject.autoKillOffset;
			autoKillType = beatmapObject.autoKillType;
			depth = beatmapObject.depth;
			editorData = new ObjectEditorData();
			editorData.Bin = beatmapObject.editorData.Bin;
			editorData.layer = beatmapObject.editorData.layer;
			editorData.collapse = beatmapObject.editorData.collapse;
			editorData.locked = beatmapObject.editorData.locked;
			fromPrefab = beatmapObject.fromPrefab;
			objectType = (ObjectType)beatmapObject.objectType;
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
			depth = beatmapObject.depth;
			editorData = new ObjectEditorData();
			editorData.Bin = beatmapObject.editorData.Bin;
			editorData.layer = beatmapObject.editorData.layer;
			editorData.collapse = beatmapObject.editorData.collapse;
			editorData.locked = beatmapObject.editorData.locked;
			fromPrefab = beatmapObject.fromPrefab;
			objectType = (ObjectType)beatmapObject.objectType;
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

		public List<string> tags = new List<string>();

		public new int Depth
		{
			get => depth;
			set => depth = value;
		}

		public bool opacityCollision = false;

		public bool background;

		public bool ignoreLifespan = false;

		public bool desync = false;

		public List<Modifier> modifiers = new List<Modifier>();
        public List<Component> components = new List<Component>();

		public ParticleSystem particleSystem;
		public TrailRenderer trailRenderer;
		public RTObject RTObject { get; set; }

		public Optimization.Objects.LevelObject levelObject;

		public TimelineObject timelineObject;

		public Detector detector;

        public int integerVariable;
        public float floatVariable;
        public string stringVariable = "";

		public Vector3 reactivePositionOffset = Vector3.zero;
		public Vector3 reactiveScaleOffset = Vector3.zero;
		public float reactiveRotationOffset = 0f;

		public Vector3 positionOffset = Vector3.zero;
		public Vector3 scaleOffset = Vector3.zero;
		public Vector3 rotationOffset = Vector3.zero;

		public string originalID;

		public bool Alive
        {
			get
			{
				var time = AudioManager.inst.CurrentAudioSource.time;
				var st = StartTime;
				var akt = autoKillType;
				var ako = autoKillOffset;
				var l = GetObjectLifeLength(_oldStyle: true);
				return time >= st && (time <= l + st && akt != AutoKillType.OldStyleNoAutokill && akt != AutoKillType.SongTime || akt == AutoKillType.OldStyleNoAutokill || time < ako && akt == AutoKillType.SongTime);
			}
        }

		public int KeyframeCount
        {
			get
            {
				int result = -1;
				if (events != null && events.Count > 0)
                {
					for (int i = 0; i < events.Count; i++)
					{
						if (events[i] != null && events[i].Count > 0)
							result += events[i].Count;
					}
				}

				return result;
            }
        }

        public class Modifier
        {
            public Modifier()
            {
            }

            public Modifier(Type type, string value)
            {
                this.type = type;
				this.value = value;
            }

            public Modifier(Type type, string command, string value, BeatmapObject beatmapObject)
            {
                commands[0] = command;
                this.type = type;
				this.value = value;
                modifierObject = beatmapObject;
            }

            public Modifier(BeatmapObject beatmapObject)
            {
                modifierObject = beatmapObject;
            }

			public bool IsValid(List<Modifier> modifiers) => commands.Count > 0 && modifiers.Has(x => x.commands[0] == commands[0]);
			public bool verified = false;

            public BeatmapObject modifierObject;
			public BackgroundObject bgModifierObject;

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

			public static Modifier DeepCopy(Modifier orig, BeatmapObject beatmapObject = null)
			{
				var modifier = new Modifier();
				modifier.type = orig.type;
				modifier.commands = new List<string>();
				foreach (var l in orig.commands)
				{
					modifier.commands.Add(l);
				}
				modifier.value = orig.value;
				modifier.modifierObject = beatmapObject ?? orig.modifierObject;
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

		public new ObjectType objectType;

		public new enum ObjectType
		{
			Normal,
			Helper,
			Decoration,
			Empty,
			Solid
		}

		#region Methods

		public static BeatmapObject DeepCopy(BeatmapObject orig, bool newID = true, bool copyVariables = true)
        {
			var beatmapObject = new BeatmapObject
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
					layer = orig.editorData.layer,
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
				parentType = orig.parentType,
				parentOffsets = orig.parentOffsets.Clone(),
				parentAdditive = orig.parentAdditive,
				parallaxSettings = orig.parallaxSettings.Copy(),
				integerVariable = copyVariables ? orig.integerVariable : 0,
				floatVariable = copyVariables ? orig.floatVariable : 0f,
				stringVariable = copyVariables ? orig.stringVariable : "",
				tags = orig.tags.Count > 0 ? orig.tags.Clone() : new List<string>(),
				background = orig.background,
				ignoreLifespan = orig.ignoreLifespan,
				opacityCollision = orig.opacityCollision,
				desync = orig.desync
			};

			for (int i = 0; i < beatmapObject.events.Count; i++)
            {
				beatmapObject.events[i].AddRange(orig.events[i].Select(x => EventKeyframe.DeepCopy((EventKeyframe)x)));
            }

			beatmapObject.modifiers = orig.modifiers.Count > 0 ? orig.modifiers.Select(x => Modifier.DeepCopy(x, beatmapObject)).ToList() : new List<Modifier>();
			return beatmapObject;
		}

		public static BeatmapObject ParseVG(JSONNode jn)
		{
			var beatmapObject = new BeatmapObject();

			var events = new List<List<BaseEventKeyframe>>();
			events.Add(new List<BaseEventKeyframe>());
			events.Add(new List<BaseEventKeyframe>());
			events.Add(new List<BaseEventKeyframe>());
			events.Add(new List<BaseEventKeyframe>());

			if (jn["e"] != null)
            {
				// Position
				{
					for (int j = 0; j < jn["e"][0]["k"].Count; j++)
                    {
						var eventKeyframe = new EventKeyframe();
						var kfjn = jn["e"][0]["k"][j];

						eventKeyframe.id = LSText.randomNumString(8);

						eventKeyframe.eventTime = kfjn["t"].AsFloat;

						if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
							eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

						eventKeyframe.SetEventValues(
							kfjn["ev"][0].AsFloat,
							kfjn["ev"][1].AsFloat,
							0f);

						eventKeyframe.random = kfjn["r"].AsInt;

						eventKeyframe.SetEventRandomValues(
							kfjn["er"][0].AsFloat,
							kfjn["er"][1].AsFloat,
							kfjn["er"][2].AsFloat);

						eventKeyframe.relative = false;
						eventKeyframe.active = false;
						events[0].Add(eventKeyframe);
					}
				}
				
				// Scale
				{
					for (int j = 0; j < jn["e"][1]["k"].Count; j++)
                    {
						var eventKeyframe = new EventKeyframe();
						var kfjn = jn["e"][1]["k"][j];

						eventKeyframe.id = LSText.randomNumString(8);

						eventKeyframe.eventTime = kfjn["t"].AsFloat;

						if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
							eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

						eventKeyframe.SetEventValues(
							kfjn["ev"][0].AsFloat,
							kfjn["ev"][1].AsFloat);

						eventKeyframe.random = kfjn["r"].AsInt;

						eventKeyframe.SetEventRandomValues(
							kfjn["er"][0].AsFloat,
							kfjn["er"][1].AsFloat,
							kfjn["er"][2].AsFloat);

						eventKeyframe.relative = false;
						eventKeyframe.active = false;
						events[1].Add(eventKeyframe);
					}
				}

				// Rotation
				{
					for (int j = 0; j < jn["e"][2]["k"].Count; j++)
					{
						var eventKeyframe = new EventKeyframe();
						var kfjn = jn["e"][2]["k"][j];

						eventKeyframe.id = LSText.randomNumString(8);

						eventKeyframe.eventTime = kfjn["t"].AsFloat;

						if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
							eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

						eventKeyframe.SetEventValues(
							kfjn["ev"][0].AsFloat);

						eventKeyframe.random = kfjn["r"].AsInt;

						eventKeyframe.SetEventRandomValues(
							kfjn["er"][0].AsFloat,
							kfjn["er"][1].AsFloat,
							kfjn["er"][2].AsFloat);

						eventKeyframe.relative = true;
						eventKeyframe.active = false;
						events[2].Add(eventKeyframe);
					}
				}

				// Color
				{
					for (int j = 0; j < jn["e"][3]["k"].Count; j++)
                    {
						var eventKeyframe = new EventKeyframe();
						var kfjn = jn["e"][3]["k"][j];

						eventKeyframe.id = LSText.randomNumString(8);

						eventKeyframe.eventTime = kfjn["t"].AsFloat;

						if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
							eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

						eventKeyframe.SetEventValues(
							kfjn["ev"][0].AsFloat,
							kfjn["ev"].Count == 1 ? 0f : (-(kfjn["ev"][1].AsFloat / 100f) + 1f),
							0f,
							0f,
							0f);

						eventKeyframe.random = kfjn["r"].AsInt;

						eventKeyframe.SetEventRandomValues(
							kfjn["er"][0].AsFloat,
							kfjn["er"][1].AsFloat,
							kfjn["er"][2].AsFloat,
							0f);

						eventKeyframe.relative = false;
						eventKeyframe.active = false;
						events[3].Add(eventKeyframe);
					}
				}
            }

			beatmapObject.events = events;

			beatmapObject.id = jn["id"] != null ? jn["id"] : LSText.randomString(16);

			beatmapObject.opacityCollision = true;

			if (jn["pre_iid"] != null)
				beatmapObject.prefabInstanceID = jn["pre_iid"];

			if (jn["pre_id"] != null)
				beatmapObject.prefabID = jn["pre_id"];

			if (jn["p_id"] != null)
				beatmapObject.parent = jn["p_id"] == "camera" ? "CAMERA_PARENT" : jn["p_id"];

			if (jn["p_t"] != null)
				beatmapObject.parentType = jn["p_t"];
			if (jn["p_id"] == "camera")
				beatmapObject.parentType = "111";

			if (jn["p_o"] != null && jn["p_id"] != "camera")
			{
				beatmapObject.parentOffsets = new List<float>(from n in jn["p_o"].AsArray.Children
															  select n.AsFloat).ToList();
			}
            else if (jn["p_id"] == "camera")
            {
				beatmapObject.parentOffsets = new List<float> { 0f, 0f, 0f };
            }

			if (jn["ot"] != null)
			{
				var ot = jn["ot"].AsInt;

				beatmapObject.objectType = ot == 4 ? ObjectType.Normal : ot == 5 ? ObjectType.Decoration : ot == 6 ? ObjectType.Empty : (ObjectType)ot;
			}

			if (jn["st"] != null)
				beatmapObject.startTime = jn["st"].AsFloat;

			if (jn["n"] != null)
				beatmapObject.name = jn["n"];

			if (jn["d"] != null)
				beatmapObject.depth = jn["d"].AsInt;

			if (jn["s"] != null)
				beatmapObject.shape = jn["s"].AsInt;

			if (jn["so"] != null)
				beatmapObject.shapeOption = jn["so"].AsInt;

			if (jn["text"] != null)
				beatmapObject.text = jn["text"];

			if (jn["ak_t"] != null)
				beatmapObject.autoKillType = (AutoKillType)jn["ak_t"].AsInt;

			if (jn["ak_o"] != null)
				beatmapObject.autoKillOffset = jn["ak_o"].AsFloat;

			if (jn["o"] != null)
				beatmapObject.origin = new Vector2(jn["o"]["x"].AsFloat, jn["o"]["y"].AsFloat);

			if (jn["ed"] != null)
			{
				beatmapObject.editorData = ObjectEditorData.ParseVG(jn["ed"]);
			}

			return beatmapObject;
		}

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
						kfjn["rx"].AsFloat, // Random Value X
						kfjn["ry"].AsFloat, // Random Value Y
						kfjn["rz"].AsFloat, // Random Interval
						kfjn["rx2"].AsFloat, // Random Axis
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
						kfjn["ry"].AsFloat,
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
							kfjn["rx"].AsFloat,
							kfjn["ry"].AsFloat,
							kfjn["rz"].AsFloat,
							kfjn["rx2"].AsFloat,
					});

					eventKeyframe.active = false;
					events[3].Add(eventKeyframe);
				}
			}

			beatmapObject.events = events;

			beatmapObject.id = jn["id"] ?? LSText.randomString(16);

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

			if (jn["rdt"] != null)
				beatmapObject.background = jn["rdt"].AsInt == 1;
			
			if (jn["opcol"] != null)
				beatmapObject.opacityCollision = jn["opcol"].AsBool;
			
			if (jn["iglif"] != null)
				beatmapObject.ignoreLifespan = jn["iglif"].AsBool;
			
			if (jn["desync"] != null)
				beatmapObject.desync = jn["desync"].AsBool;

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

			if (jn["tags"] != null)
				for (int i = 0; i < jn["tags"].Count; i++)
					beatmapObject.tags.Add(jn["tags"][i]);

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

			if (jn["ed"]["locked"] != null)
				beatmapObject.editorData.locked = jn["ed"]["locked"].AsBool;

			if (jn["ed"]["shrink"] != null)
				beatmapObject.editorData.collapse = jn["ed"]["shrink"].AsBool;

			if (jn["ed"]["bin"] != null)
				beatmapObject.editorData.Bin = jn["ed"]["bin"].AsInt;

			if (jn["ed"]["layer"] != null)
				beatmapObject.editorData.layer = Mathf.Clamp(jn["ed"]["layer"].AsInt, 0, int.MaxValue);

			for (int i = 0; i < jn["modifiers"].Count; i++)
			{
				var modifier = Modifier.Parse(jn["modifiers"][i]);
				modifier.modifierObject = beatmapObject;
				beatmapObject.modifiers.Add(modifier);
			}

			return beatmapObject;
		}

		public JSONNode ToJSONVG()
		{
			var jn = JSON.Parse("{}");

			jn["id"] = id;
			if (!string.IsNullOrEmpty(prefabID))
				jn["pre_id"] = prefabID;

			if (!string.IsNullOrEmpty(prefabInstanceID))
				jn["pre_iid"] = prefabInstanceID;

			if (parentType != "101")
				jn["p_t"] = parentType;

			if (parentOffsets.FindIndex(x => x != 0f) != -1)
			{
				int index = 0;
				foreach (float offset in parentOffsets)
				{
					jn["p_o"][index] = offset;
					index++;
				}
			}

			if (!string.IsNullOrEmpty(parent))
				jn["p_id"] = parent == "CAMERA_PARENT" ? "camera" : parent;

			jn["d"] = depth;

			jn["st"] = StartTime;

			if (!string.IsNullOrEmpty(name))
				jn["n"] = name;

			jn["ot"] = (int)objectType == 4 ? 0 : (int)objectType;
			jn["ak_t"] = (int)autoKillType;
			jn["ak_o"] = autoKillOffset;

			if (shape != 0)
				jn["s"] = shape;

			if (shapeOption != 0)
				jn["so"] = shapeOption;

			if (!string.IsNullOrEmpty(text))
				jn["text"] = text;

			jn["o"]["x"] = origin.x;
			jn["o"]["y"] = origin.y;

			if (editorData.locked)
				jn["ed"]["lk"] = editorData.locked;

			if (editorData.collapse)
				jn["ed"]["co"] = editorData.collapse;

			jn["ed"]["b"] = editorData.Bin;
			
			jn["ed"]["l"].AsInt = Mathf.Clamp(editorData.layer, 0, 5);

			// Events
            {
				// Position
				for (int j = 0; j < events[0].Count; j++)
				{
					var eventKeyframe = events[0][j];
					jn["e"][0]["k"][j]["t"] = eventKeyframe.eventTime;
					jn["e"][0]["k"][j]["ct"] = eventKeyframe.curveType.Name;

					jn["e"][0]["k"][j]["ev"][0] = eventKeyframe.eventValues[0];
					jn["e"][0]["k"][j]["ev"][1] = eventKeyframe.eventValues[1];

					jn["e"][0]["k"][j]["r"] = eventKeyframe.random;

					jn["e"][0]["k"][j]["er"][0] = eventKeyframe.eventRandomValues[0];
					jn["e"][0]["k"][j]["er"][1] = eventKeyframe.eventRandomValues[1];
					jn["e"][0]["k"][j]["er"][2] = eventKeyframe.eventRandomValues[2];
				}

				// Scale
				for (int j = 0; j < events[1].Count; j++)
				{
					var eventKeyframe = events[1][j];
					jn["e"][1]["k"][j]["t"] = eventKeyframe.eventTime;
					jn["e"][1]["k"][j]["ct"] = eventKeyframe.curveType.Name;

					jn["e"][1]["k"][j]["ev"][0] = eventKeyframe.eventValues[0];
					jn["e"][1]["k"][j]["ev"][1] = eventKeyframe.eventValues[1];

					jn["e"][1]["k"][j]["r"] = eventKeyframe.random;

					jn["e"][1]["k"][j]["er"][0] = eventKeyframe.eventRandomValues[0];
					jn["e"][1]["k"][j]["er"][1] = eventKeyframe.eventRandomValues[1];
					jn["e"][1]["k"][j]["er"][2] = eventKeyframe.eventRandomValues[2];
				}

				// Rotation
				for (int j = 0; j < events[2].Count; j++)
				{
					var eventKeyframe = events[2][j];
					jn["e"][2]["k"][j]["t"] = eventKeyframe.eventTime;
					jn["e"][2]["k"][j]["ct"] = eventKeyframe.curveType.Name;

					jn["e"][2]["k"][j]["ev"][0] = eventKeyframe.eventValues[0];

					jn["e"][2]["k"][j]["r"] = eventKeyframe.random;

					jn["e"][2]["k"][j]["er"][0] = eventKeyframe.eventRandomValues[0];
					jn["e"][2]["k"][j]["er"][1] = eventKeyframe.eventRandomValues[1];
					jn["e"][2]["k"][j]["er"][2] = eventKeyframe.eventRandomValues[2];
				}

				// Color
				for (int j = 0; j < events[3].Count; j++)
				{
					var eventKeyframe = events[3][j];
					jn["e"][3]["k"][j]["t"] = eventKeyframe.eventTime;
					jn["e"][3]["k"][j]["ct"] = eventKeyframe.curveType.Name;

					jn["e"][3]["k"][j]["ev"][0] = Mathf.Clamp(eventKeyframe.eventValues[0], 0, 8);
					jn["e"][3]["k"][j]["ev"][1] = (-eventKeyframe.eventValues[1] + 1f) * 100f;
				}
			}

			return jn;
		}

		public JSONNode ToJSON()
        {
			var jn = JSON.Parse("{}");

			jn["id"] = id;
			if (!string.IsNullOrEmpty(prefabID))
				jn["pid"] = prefabID;

			if (!string.IsNullOrEmpty(prefabInstanceID))
				jn["piid"] = prefabInstanceID;

			if (parentType != "101")
				jn["pt"] = parentType;

			if (parentOffsets.FindIndex(x => x != 0f) != -1)
			{
				int index = 0;
				foreach (var offset in parentOffsets)
				{
					jn["po"][index] = offset.ToString();
					index++;
				}
			}

			if (parallaxSettings.ToList().FindIndex(x => x != 1f) != -1)
            {
				for (int i = 0; i < parallaxSettings.Length; i++)
					jn["ps"][i] = parallaxSettings[i].ToString();
            }

			if (parentAdditive != "000")
				jn["pa"] = parentAdditive;

			if (!string.IsNullOrEmpty(parent))
				jn["p"] = parent;

			jn["d"] = depth.ToString();
			if (background)
				jn["rdt"] = "1";
			if (opacityCollision)
				jn["opcol"] = opacityCollision.ToString();
			if (ignoreLifespan)
				jn["iglif"] = ignoreLifespan.ToString();
			if (desync)
				jn["desync"] = desync.ToString();

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

			if (tags != null && tags.Count > 0)
				for (int i = 0; i < tags.Count; i++)
					jn["tags"][i] = tags[i];

			jn["o"]["x"] = origin.x.ToString();
			jn["o"]["y"] = origin.y.ToString();
			if (editorData.locked)
				jn["ed"]["locked"] = editorData.locked.ToString();
			if (editorData.collapse)
				jn["ed"]["shrink"] = editorData.collapse.ToString();

			jn["ed"]["bin"] = editorData.Bin.ToString();
			jn["ed"]["layer"] = editorData.layer.ToString();

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

		public void SetParentAdditive(int _index, bool _new)
		{
			var stringBuilder = new StringBuilder(parentAdditive);
			stringBuilder[_index] = _new ? '1' : '0';
			parentAdditive = stringBuilder.ToString();
			Debug.Log($"{FunctionsPlugin.className}Set Parent Additive: {parentAdditive}");
		}

		#endregion

		#region Operators

		public static implicit operator bool(BeatmapObject exists) => exists != null;

        public override bool Equals(object obj) => obj is BeatmapObject && id == (obj as BeatmapObject).id;

        public override string ToString() => id;

        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
