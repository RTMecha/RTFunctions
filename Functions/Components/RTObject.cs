using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using LSFunctions;

using RTFunctions.Functions;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

using BeatmapObject = DataManager.GameData.BeatmapObject;

namespace RTFunctions.Functions.Components
{
    public class RTObject : MonoBehaviour
    {
        public bool selected;
		public bool tipEnabled;
		public string id;

		BeatmapObject beatmapObject;

		Renderer renderer;

		public Color highlightColor;
		public Color highlightDoubleColor;
		public bool highlightObjects;
		public float layerOpacity = 0.5f;
		public bool showObjectsOnlyOnLayer;

		void Awake()
        {
			if (EditorManager.inst == null)
				Destroy(this);

			if (GetComponent<Renderer>())
				renderer = GetComponent<Renderer>();
        }

		public void SetObject(string id)
		{
			this.id = id;
			if (!string.IsNullOrEmpty(id))
			{
				beatmapObject = DataManager.inst.gameData.beatmapObjects.Find(x => x.id == id);
			}

			if (beatmapObject == null || beatmapObject.objectType == BeatmapObject.ObjectType.Empty)
			{
				Destroy(this);
			}
		}

		public void OnMouseDown()
        {
			if (EditorManager.inst != null && EditorManager.inst.isEditing && DataManager.inst.gameData.beatmapObjects.Count > 0 && !string.IsNullOrEmpty(id) && !LSHelpers.IsUsingInputField() && !EventSystem.current.IsPointerOverGameObject())
			{
				if (ModCompatibility.mods.ContainsKey("EditorManagement"))
				{
					var mod = ModCompatibility.mods["EditorManagement"];
					if (mod.Methods.ContainsKey("SetCurrentObject") && !Input.GetKey(KeyCode.LeftShift))
						mod.Methods["SetCurrentObject"].DynamicInvoke(new TimelineObject(beatmapObject), true);
					if (mod.Methods.ContainsKey("AddSelectedObject") && Input.GetKey(KeyCode.LeftShift))
						mod.Methods["AddSelectedObject"].DynamicInvoke(new TimelineObject(beatmapObject));
				}
            }
        }

        public void OnMouseEnter()
        {
            selected = true;

            if (tipEnabled && EditorManager.inst != null)
			{
				DataManager.Language enumTmp = DataManager.inst.GetCurrentLanguageEnum();
				int num = tooltipLanguages.FindIndex(x => x.language == enumTmp);
				if (num != -1)
				{
					var tooltip = tooltipLanguages[num];
					EditorManager.inst.SetTooltip(tooltip.keys, tooltip.desc, tooltip.hint);
					return;
				}
				EditorManager.inst.SetTooltip(new List<string>(), "No tooltip added yet!", gameObject.name);
			}
		}

        public void OnMouseExit()
        {
            selected = false;
			if (tipEnabled && EditorManager.inst != null)
			{
				EditorManager.inst.SetTooltipDisappear(0.5f);
			}
		}

		public void Update()
		{
			var m = 0f;

			if (beatmapObject != null && showObjectsOnlyOnLayer && beatmapObject.editorData.Layer != EditorManager.inst.layer)
				m = -layerOpacity;

			if (EditorManager.inst != null && EditorManager.inst.isEditing && !selected && renderer != null && renderer.material.HasProperty("_Color"))
            {
				renderer.material.color += new Color(0f, 0f, 0f, m);
            }

			if (EditorManager.inst != null && EditorManager.inst.isEditing && highlightObjects && selected && renderer != null && renderer.material.HasProperty("_Color"))
			{
				var color = Input.GetKey(KeyCode.LeftShift) ? new Color(
					renderer.material.color.r > 0.9f ? -highlightDoubleColor.r : highlightDoubleColor.r,
					renderer.material.color.g > 0.9f ? -highlightDoubleColor.g : highlightDoubleColor.g,
					renderer.material.color.b > 0.9f ? -highlightDoubleColor.b : highlightDoubleColor.b,
					0f) : new Color(
					renderer.material.color.r > 0.9f ? -highlightColor.r : highlightColor.r,
					renderer.material.color.g > 0.9f ? -highlightColor.g : highlightColor.g,
					renderer.material.color.b > 0.9f ? -highlightColor.b : highlightColor.b,
					0f);

				renderer.material.color += color;
			}

			if (EditorManager.inst != null && EditorManager.inst.showHelp && beatmapObject != null)
			{
				tipEnabled = true;

				if (tooltipLanguages.Count == 0)
				{
					tooltipLanguages.Add(RTHelpers.NewTooltip(beatmapObject.name + " [ " + beatmapObject.StartTime + " ]", "", new List<string>()));
				}

				string parent = "";
				if (!string.IsNullOrEmpty(beatmapObject.parent))
				{
					parent = "<br>P: " + beatmapObject.parent + " (" + beatmapObject.GetParentType() + ")";
				}
				else
				{
					parent = "<br>P: No Parent" + " (" + beatmapObject.GetParentType() + ")";
				}

				string text = "";
				if (beatmapObject.shape != 4 || beatmapObject.shape != 6)
				{
					text = "<br>S: " + RTHelpers.GetShape(beatmapObject.shape, beatmapObject.shapeOption).Replace("eight_circle", "eighth_circle").Replace("eigth_circle_outline", "eighth_circle_outline");

					if (!string.IsNullOrEmpty(beatmapObject.text))
					{
						text += "<br>T: " + beatmapObject.text;
					}
				}
				if (beatmapObject.shape == 4)
				{
					text = "<br>S: Text" +
						"<br>T: " + beatmapObject.text;
				}
				if (beatmapObject.shape == 6)
				{
					text = "<br>S: Image" +
						"<br>T: " + beatmapObject.text;
				}

				string ptr = "";
				if (beatmapObject.fromPrefab && !string.IsNullOrEmpty(beatmapObject.prefabID) && !string.IsNullOrEmpty(beatmapObject.prefabInstanceID))
				{
					ptr = "<br>PID: " + beatmapObject.prefabID + " | " + beatmapObject.prefabInstanceID;
				}
				else
				{
					ptr = "<br>Not from prefab";
				}

				Color col = LSColors.transparent;
				if (renderer.material.HasProperty("_Color"))
                {
					col = renderer.material.color;
				}

				if (tooltipLanguages[0].desc != "N/ST: " + beatmapObject.name + " [ " + beatmapObject.StartTime + " ]")
				{
					tooltipLanguages[0].desc = "N/ST: " + beatmapObject.name + " [ " + beatmapObject.StartTime + " ]";
				}
				if (tooltipLanguages[0].hint != "ID: {" + beatmapObject.id + "}" +
					parent +
					"<br>O: {X: " + beatmapObject.origin.x + ", Y: " + beatmapObject.origin.y + "}" +
					text +
					"<br>D: " + beatmapObject.Depth +
					"<br>ED: {L: " + beatmapObject.editorData.Layer + ", B: " + beatmapObject.editorData.Bin + "}" +
					"<br>POS: {X: " + transform.position.x + ", Y: " + transform.position.y + ", Z: " + transform.position.z + "}" +
					"<br>SCA: {X: " + transform.localScale.x + ", Y: " + transform.localScale.y + "}" +
					"<br>ROT: " + transform.eulerAngles.z +
					"<br>COL: " + RTHelpers.ColorToHex(col) +
					ptr)
				{
					tooltipLanguages[0].hint = "ID: {" + beatmapObject.id + "}" +
						parent +
						"<br>O: {X: " + beatmapObject.origin.x + ", Y: " + beatmapObject.origin.y + "}" +
						text +
						"<br>D: " + beatmapObject.Depth +
						"<br>ED: {L: " + beatmapObject.editorData.Layer + ", B: " + beatmapObject.editorData.Bin + "}" +
						"<br>POS: {X: " + transform.position.x + ", Y: " + transform.position.y + ", Z: " + transform.position.z + "}" +
						"<br>SCA: {X: " + transform.localScale.x + ", Y: " + transform.localScale.y + "}" +
						"<br>ROT: " + transform.eulerAngles.z +
						"<br>COL: " + RTHelpers.ColorToHex(col) +
						ptr;
				}
			}
		}

		public List<HoverTooltip.Tooltip> tooltipLanguages = new List<HoverTooltip.Tooltip>();
	}
}
