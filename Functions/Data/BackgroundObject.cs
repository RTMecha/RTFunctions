using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions.Managers;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using EventKeyframe = DataManager.GameData.EventKeyframe;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BasePrefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;
using BaseBackground = DataManager.GameData.BackgroundObject;
using BaseEditorData = DataManager.GameData.BeatmapObject.EditorData;

namespace RTFunctions.Functions.Data
{
    public class BackgroundObject : BaseBackground
    {
        public BackgroundObject()
        {
            shape = Objects.Shapes3D[0];
        }

		public BackgroundObject(
			bool _active,
			string _name,
			int _kind,
			string _text,
			Vector2 _pos,
			Vector2 _scale,
			float _rot,
			int _color,
			int _layer,
			bool _reactive,
			ReactiveType _reactiveType,
			float _reactiveScale,
			bool _drawFade) : base(_active, _name, _kind, _text, _pos, _scale, _rot, _color, _layer, _reactive, _reactiveType, _reactiveScale, _drawFade)
        {
			zPosition = _layer;
		}
		
		public BackgroundObject(
			bool _active,
			string _name,
			int _kind,
			string _text,
			Vector2 _pos,
			Vector2 _scale,
			float _rot,
			int _color,
			float _layer,
			bool _reactive,
			ReactiveType _reactiveType,
			float _reactiveScale,
			bool _drawFade) : base(_active, _name, _kind, _text, _pos, _scale, _rot, _color, (int)_layer, _reactive, _reactiveType, _reactiveScale, _drawFade)
        {
			zPosition = _layer;
		}

        public BackgroundObject(BaseBackground bg)
        {
            active = bg.active;
            color = bg.color;
            drawFade = bg.drawFade;
            kind = bg.kind;
            layer = bg.layer;
            name = bg.name;
            pos = bg.pos;
            reactive = bg.reactive;
            reactiveScale = bg.reactiveScale;
            reactiveSize = bg.reactiveSize;
            reactiveType = bg.reactiveType;
            rot = bg.rot;
            scale = bg.scale;
            text = bg.text;
            
            shape = Objects.Shapes3D[0];
        }

        public void SetShape(int shape)
        {
            this.shape = Shape.DeepCopy(Objects.Shapes3D[shape]);
            foreach (var gameObject in gameObjects)
            {
                if (gameObject.TryGetComponent(out MeshFilter meshFilter) && this.shape.mesh)
                    meshFilter.mesh = this.shape.mesh;
            }
        }

        public void SetShape(int shape, int shapeOption)
        {
            this.shape = Shape.DeepCopy(Objects.GetShape3D(shape, shapeOption));
            foreach (var gameObject in gameObjects)
            {
                if (gameObject.TryGetComponent(out MeshFilter meshFilter) && this.shape.mesh)
                    meshFilter.mesh = this.shape.mesh;
            }
        }

        public GameObject BaseObject => gameObjects[0];

        public List<GameObject> gameObjects = new List<GameObject>();
        public List<Transform> transforms = new List<Transform>();
        public List<Renderer> renderers = new List<Renderer>();

        public Vector2Int reactivePosSamples;
        public Vector2Int reactiveScaSamples;
        public int reactiveRotSample;
        public int reactiveColSample;
        public int reactiveCol;

        public Vector2 reactivePosIntensity;
        public Vector2 reactiveScaIntensity;
        public float reactiveRotIntensity;
        public float reactiveColIntensity;

        public Vector2 rotation = Vector2.zero;
        public Shape shape;
        public float zscale = 10f;
        public int depth = 9;
		public float zPosition;

        int fadeColor;
        public int FadeColor
        {
            get => Mathf.Clamp(fadeColor, 0, 8);
            set => fadeColor = Mathf.Clamp(value, 0, 8);
        }

        public bool reactiveIncludesZ;
        public float reactiveZIntensity;
        public int reactiveZSample;

		#region Methods

		public static BackgroundObject DeepCopy(BackgroundObject bg) => new BackgroundObject()
		{
			active = bg.active,
			color = bg.color,
			drawFade = bg.drawFade,
			kind = bg.kind,
			layer = bg.layer,
			name = bg.name,
			pos = bg.pos,
			reactive = bg.reactive,
			reactiveScale = bg.reactiveScale,
			reactiveSize = bg.reactiveSize,
			reactiveType = bg.reactiveType,
			rot = bg.rot,
			scale = bg.scale,
			text = bg.text,
			depth = bg.depth,
			shape = bg.shape,
			zscale = bg.zscale,
			rotation = bg.rotation,

			reactiveCol = bg.reactiveCol,
			reactiveColSample = bg.reactiveColSample,
			reactiveColIntensity = bg.reactiveColIntensity,
			reactivePosSamples = bg.reactivePosSamples,
			reactivePosIntensity = bg.reactivePosIntensity,
			reactiveRotSample = bg.reactiveRotSample,
			reactiveRotIntensity = bg.reactiveRotIntensity,
			reactiveScaSamples = bg.reactiveScaSamples,
			reactiveScaIntensity = bg.reactiveScaIntensity,
			reactiveZIntensity = bg.reactiveZIntensity,
			reactiveZSample = bg.reactiveZSample,
		};

		public static BackgroundObject Parse(JSONNode jn)
		{
			var active = true;
			if (jn["active"] != null)
				active = jn["active"].AsBool;

			string name;
			if (jn["name"] != null)
				name = jn["name"];
			else
				name = "Background";

			int kind;
			if (jn["kind"] != null)
				kind = jn["kind"].AsInt;
			else
				kind = 1;

			string text;
			if (jn["text"] != null)
				text = jn["text"];
			else
				text = "";

			var pos = new Vector2(jn["pos"]["x"].AsFloat, jn["pos"]["y"].AsFloat);
			var scale = new Vector2(jn["size"]["x"].AsFloat, jn["size"]["y"].AsFloat);

			var rot = jn["rot"].AsFloat;
			var color = jn["color"].AsInt;
			var layer = jn["layer"].AsFloat;

			var reactive = false;
			if (jn["r_set"] != null)
				reactive = true;

			if (jn["r_set"]["active"] != null)
				reactive = jn["r_set"]["active"].AsBool;

			var reactiveType = ReactiveType.LOW;
			if (jn["r_set"]["type"] != null)
				reactiveType = (ReactiveType)Enum.Parse(typeof(ReactiveType), jn["r_set"]["type"]);

			float reactiveScale = 1f;
			if (jn["r_set"]["scale"] != null)
				reactiveScale = jn["r_set"]["scale"].AsFloat;

			bool drawFade = true;
			if (jn["fade"] != null)
				drawFade = jn["fade"].AsBool;

			#region New stuff

			float zscale = 10f;
			if (jn["zscale"] != null)
				zscale = jn["zscale"].AsFloat;

			int depth = 9;
			if (jn["depth"] != null)
				depth = jn["depth"].AsInt;

			Shape shape = Objects.Shapes3D[0];
			if (jn["s"] != null && jn["so"] != null)
				shape = Objects.GetShape3D(jn["s"].AsInt, jn["so"].AsInt);

			Vector2 rotation = Vector2.zero;
			if (jn["r_offset"] != null && jn["r_offset"]["x"] != null && jn["r_offset"]["y"] != null)
				rotation = new Vector2(jn["r_offset"]["x"].AsFloat, jn["r_offset"]["y"].AsFloat);

			int fadeColor = 0;
			if (jn["color_fade"] != null)
				fadeColor = jn["color_fade"].AsInt;

			var reactivePosIntensity = Vector2.zero;
			var reactivePosSamples = Vector2Int.zero;
			var reactiveZIntensity = 0f;
			var reactiveZSample = 0;
			var reactiveScaIntensity = Vector2.zero;
			var reactiveScaSamples = Vector2Int.zero;
			var reactiveRotIntensity = 0f;
			var reactiveRotSample = 0;
			var reactiveColIntensity = 0f;
			var reactiveColSample = 0;
			var reactiveCol = 0;

			if (jn["rc"] != null)
			{
				try
				{
					if (jn["rc"]["pos"] != null && jn["rc"]["pos"]["i"] != null && jn["rc"]["pos"]["i"]["x"] != null && jn["rc"]["pos"]["i"]["y"] != null)
						reactivePosIntensity = new Vector2(jn["rc"]["pos"]["i"]["x"].AsFloat, jn["rc"]["pos"]["i"]["y"].AsFloat);
					if (jn["rc"]["pos"] != null && jn["rc"]["pos"]["s"] != null && jn["rc"]["pos"]["s"]["x"] != null && jn["rc"]["pos"]["s"]["y"] != null)
						reactivePosSamples = new Vector2Int(jn["rc"]["pos"]["s"]["x"].AsInt, jn["rc"]["pos"]["s"]["y"].AsInt);

					//if (jn["rc"]["z"] != null && jn["rc"]["active"] != null)
					//	bg.reactiveIncludesZ = jn["rc"]["z"]["active"].AsBool;

					if (jn["rc"]["z"] != null && jn["rc"]["z"]["i"] != null)
						reactiveZIntensity = jn["rc"]["z"]["i"].AsFloat;
					if (jn["rc"]["z"] != null && jn["rc"]["z"]["s"] != null)
						reactiveZSample = jn["rc"]["z"]["s"].AsInt;

					if (jn["rc"]["sca"] != null && jn["rc"]["sca"]["i"] != null && jn["rc"]["sca"]["i"]["x"] != null && jn["rc"]["sca"]["i"]["y"] != null)
						reactiveScaIntensity = new Vector2(jn["rc"]["sca"]["i"]["x"].AsFloat, jn["rc"]["sca"]["i"]["y"].AsFloat);
					if (jn["rc"]["sca"] != null && jn["rc"]["sca"]["s"] != null && jn["rc"]["sca"]["s"]["x"] != null && jn["rc"]["sca"]["s"]["y"] != null)
						reactiveScaSamples = new Vector2Int(jn["rc"]["sca"]["s"]["x"].AsInt, jn["rc"]["sca"]["s"]["y"].AsInt);

					if (jn["rc"]["rot"] != null && jn["rc"]["rot"]["i"] != null)
						reactiveRotIntensity = jn["rc"]["rot"]["i"].AsFloat;
					if (jn["rc"]["rot"] != null && jn["rc"]["rot"]["s"] != null)
						reactiveRotSample = jn["rc"]["rot"]["s"].AsInt;

					if (jn["rc"]["col"] != null && jn["rc"]["col"]["i"] != null)
						reactiveColIntensity = jn["rc"]["col"]["i"].AsFloat;
					if (jn["rc"]["col"] != null && jn["rc"]["col"]["s"] != null)
						reactiveColSample = jn["rc"]["col"]["s"].AsInt;
					if (jn["rc"]["col"] != null && jn["rc"]["col"]["c"] != null)
						reactiveCol = jn["rc"]["col"]["c"].AsInt;
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.Log($"{FunctionsPlugin.className}Failed to load settings.\nEXCEPTION: {ex.Message}\nSTACKTRACE: {ex.StackTrace}");
				}
			}

			#endregion

			return new BackgroundObject
			{
				active = active,
				name = name,
				kind = kind,
				text = text,
				pos = pos,
				scale = scale,
				rot = rot,
				color = color,
				zPosition = layer,
				reactive = reactive,
				reactiveType = reactiveType,
				reactiveScale = reactiveScale,
				drawFade = drawFade,

				zscale = zscale,
				depth = depth,
				shape = shape,
				rotation = rotation,
				FadeColor = fadeColor,
				reactivePosIntensity = reactivePosIntensity,
				reactivePosSamples = reactivePosSamples,
				reactiveZIntensity = reactiveZIntensity,
				reactiveZSample = reactiveZSample,
				reactiveScaIntensity = reactiveScaIntensity,
				reactiveScaSamples = reactiveScaSamples,
				reactiveRotIntensity = reactiveRotIntensity,
				reactiveRotSample = reactiveRotSample,
				reactiveColIntensity = reactiveColIntensity,
				reactiveColSample = reactiveColSample,
				reactiveCol = reactiveCol,
			};
		}

		public JSONNode ToJSON()
		{
			var jn = JSON.Parse("{}");

			jn["active"] = active.ToString();
			jn["name"] = name.ToString();
			jn["kind"] = kind.ToString();
			jn["pos"]["x"] = pos.x.ToString();
			jn["pos"]["y"] = pos.y.ToString();
			jn["size"]["x"] = scale.x.ToString();
			jn["size"]["y"] = scale.y.ToString();
			jn["rot"] = rot.ToString();
			jn["color"] = color.ToString();
			jn["layer"] = layer.ToString();
			jn["fade"] = drawFade.ToString();

			jn["zscale"] = zscale.ToString();
			jn["depth"] = depth.ToString();
			jn["s"] = shape.Type.ToString();
			jn["so"] = shape.Option.ToString();
			jn["color_fade"] = FadeColor.ToString();
			jn["r_offset"]["x"] = rotation.x.ToString();
			jn["r_offset"]["y"] = rotation.y.ToString();

			jn["rc"]["pos"]["i"]["x"] = reactivePosIntensity.x.ToString();
			jn["rc"]["pos"]["i"]["y"] = reactivePosIntensity.y.ToString();
			jn["rc"]["pos"]["s"]["x"] = reactivePosSamples.x.ToString();
			jn["rc"]["pos"]["s"]["y"] = reactivePosSamples.y.ToString();

			jn["rc"]["z"]["i"] = reactiveZIntensity.ToString();
			jn["rc"]["z"]["s"] = reactiveZSample.ToString();

			jn["rc"]["sca"]["i"]["x"] = reactiveScaIntensity.x.ToString();
			jn["rc"]["sca"]["i"]["y"] = reactiveScaIntensity.y.ToString();
			jn["rc"]["sca"]["s"]["x"] = reactiveScaSamples.x.ToString();
			jn["rc"]["sca"]["s"]["y"] = reactiveScaSamples.y.ToString();

			jn["rc"]["rot"]["i"] = reactiveRotIntensity.ToString();
			jn["rc"]["rot"]["s"] = reactiveRotSample.ToString();

			jn["rc"]["col"]["i"] = reactiveColIntensity.ToString();
			jn["rc"]["col"]["s"] = reactiveColSample.ToString();
			jn["rc"]["col"]["c"] = reactiveCol.ToString();

			if (reactive)
			{
				jn["r_set"]["type"] = reactiveType.ToString();
				jn["r_set"]["scale"] = reactiveScale.ToString();
			}

			return jn;
		}

		#endregion
	}
}
