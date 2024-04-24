using LSFunctions;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RTFunctions.Functions.Components
{
    /// <summary>
    /// Component for selecting and dragging objects. Still needs a ton of work though.
    /// </summary>
    public class RTObject : MonoBehaviour
    {
		public bool CanDrag => ModCompatibility.sharedFunctions.ContainsKey("SelectedObjectCount") && ((int)ModCompatibility.sharedFunctions["SelectedObjectCount"]) < 2;
		public static bool Enabled { get; set; }
		public static bool CreateKeyframe { get; set; }

		public bool Selected
		{
			get
			{
                try
				{
					if (ModCompatibility.mods.ContainsKey("EditorManagement"))
					{
						var mod = ModCompatibility.mods["EditorManagement"];

						if (mod.Methods.ContainsKey("GetTimelineObject"))
						{
							var timelineObject = (TimelineObject)mod.Methods["GetTimelineObject"].DynamicInvoke(beatmapObject);
							return timelineObject.ID == beatmapObject.id && timelineObject.selected;
						}
					}
				}
                catch
                {

                }

				return false;
			}
		}

		public static bool TipEnabled { get; set; }
		public string id;

		public BeatmapObject beatmapObject;

		Renderer renderer;

		#region Highlighting

		public bool hovered;

		public static Color HighlightColor { get; set; }
		public static Color HighlightDoubleColor { get; set; }
		public static bool HighlightObjects { get; set; }
		public static float LayerOpacity { get; set; }
		public static bool ShowObjectsOnlyOnLayer { get; set; }

        #endregion

        #region Dragging

        bool dragging;

		bool setKeyframeValues;
		Vector2 dragKeyframeValues;
		public EventKeyframe selectedKeyframe;
		Vector2 dragOffset;
		Axis firstDirection = Axis.Static;

		public enum Axis
		{
			Static,
			PosX,
			PosY,
			NegX,
			NegY
		}

		#endregion

		#region Delegates

		public Action onMouseDown;
		public Action onMouseUp;
		public Action onMouseEnter;
		public Action onMouseExit;
		public Action onMouseDrag;

		#endregion

		void Awake()
		{
			var renderer = GetComponent<Renderer>();
			if (renderer)
				this.renderer = renderer;
		}

		public void GenerateDraggers()
		{
			if (!Enabled || !EditorManager.inst || !Selected)
				return;

			GameStorageManager.inst.objectRotator.refObject = this;
			GameStorageManager.inst.objectScalerTop.refObject = this;
			GameStorageManager.inst.objectScalerLeft.refObject = this;
			GameStorageManager.inst.objectScalerBottom.refObject = this;
			GameStorageManager.inst.objectScalerRight.refObject = this;
		}

		public void SetObject(BeatmapObject beatmapObject)
        {
			id = beatmapObject.id;
			beatmapObject.RTObject = this;
			this.beatmapObject = beatmapObject;
		}

		void OnMouseUp()
        {
			onMouseUp?.Invoke();
			dragging = false;
			selectedKeyframe = null;
			setKeyframeValues = false;
			firstDirection = Axis.Static;
		}

		void OnMouseDown()
		{
			onMouseDown?.Invoke();
			if (EditorManager.inst && EditorManager.inst.isEditing && DataManager.inst.gameData.beatmapObjects.Count > 0 && !string.IsNullOrEmpty(id) && !LSHelpers.IsUsingInputField() && !EventSystem.current.IsPointerOverGameObject())
			{
				startDragTime = Time.time;
				if (ModCompatibility.mods.ContainsKey("EditorManagement"))
				{
					var mod = ModCompatibility.mods["EditorManagement"];

					if ((!ModCompatibility.sharedFunctions.ContainsKey("ParentPickerActive") || !(bool)ModCompatibility.sharedFunctions["ParentPickerActive"]) &&
						(!ModCompatibility.sharedFunctions.ContainsKey("PrefabPickerActive") || !(bool)ModCompatibility.sharedFunctions["PrefabPickerActive"]))
					{
						TimelineObject timelineObject;

						if (mod.Methods.ContainsKey("GetTimelineObject"))
							timelineObject = (TimelineObject)mod.Methods["GetTimelineObject"].DynamicInvoke(beatmapObject);
						else
							timelineObject = new TimelineObject(beatmapObject);

						if (mod.Methods.ContainsKey("RenderTimelineObjectVoid"))
							mod.Methods["RenderTimelineObjectVoid"].DynamicInvoke(timelineObject);

						if (!timelineObject.selected)
						{
							EditorManager.inst.ClearDialogs();

							if (mod.Methods.ContainsKey("SetCurrentObject") && !Input.GetKey(KeyCode.LeftShift))
								mod.Methods["SetCurrentObject"].DynamicInvoke(timelineObject, Input.GetKey(KeyCode.LeftAlt));
							if (mod.Methods.ContainsKey("AddSelectedObject") && Input.GetKey(KeyCode.LeftShift))
								mod.Methods["AddSelectedObject"].DynamicInvoke(timelineObject);
						}
						return;
					}

					if (!ModCompatibility.sharedFunctions.ContainsKey("SelectedObjects") || ModCompatibility.sharedFunctions["SelectedObjects"] is not List<TimelineObject> ||
						!ModCompatibility.sharedFunctions.ContainsKey("ParentPickerDisable") || !ModCompatibility.sharedFunctions.ContainsKey("RefreshObjectGUI"))
						return;

					var currentSelection = (TimelineObject)ModCompatibility.sharedFunctions["CurrentSelection"];
					var selectedObjects = (List<TimelineObject>)ModCompatibility.sharedFunctions["SelectedObjects"];

					if (ModCompatibility.sharedFunctions.ContainsKey("PrefabPickerActive")
						&& (bool)ModCompatibility.sharedFunctions["PrefabPickerActive"])
					{
						if (string.IsNullOrEmpty(beatmapObject.prefabInstanceID))
						{
							EditorManager.inst.DisplayNotification("Object is not assigned to a prefab!", 2f, EditorManager.NotificationType.Error);
							return;
						}

						if (ModCompatibility.sharedFunctions.ContainsKey("SelectinMultiple")
						&& (bool)ModCompatibility.sharedFunctions["SelectinMultiple"])
						{
							foreach (var otherTimelineObject in selectedObjects.Where(x => x.IsBeatmapObject))
							{
								var otherBeatmapObject = otherTimelineObject.GetData<BeatmapObject>();

								otherBeatmapObject.prefabID = beatmapObject.prefabID;
								otherBeatmapObject.prefabInstanceID = beatmapObject.prefabInstanceID;

								if (mod.Methods.ContainsKey("RenderTimelineObjectVoid"))
									mod.Methods["RenderTimelineObjectVoid"].DynamicInvoke(otherTimelineObject);
							}
						}
						else if (currentSelection.IsBeatmapObject)
						{
							var currentBeatmapObject = currentSelection.GetData<BeatmapObject>();

							currentBeatmapObject.prefabID = beatmapObject.prefabID;
							currentBeatmapObject.prefabInstanceID = beatmapObject.prefabInstanceID;

							if (mod.Methods.ContainsKey("RenderTimelineObjectVoid"))
								mod.Methods["RenderTimelineObjectVoid"].DynamicInvoke(currentSelection);
							((Action<BeatmapObject>)ModCompatibility.sharedFunctions["RefreshObjectGUI"])?.Invoke(currentBeatmapObject);
						}

						((Action)ModCompatibility.sharedFunctions["ParentPickerDisable"])?.Invoke();

						return;
					}

					if (ModCompatibility.sharedFunctions.ContainsKey("ParentPickerActive")
						&& (bool)ModCompatibility.sharedFunctions["ParentPickerActive"]
						 && ModCompatibility.sharedFunctions.ContainsKey("ParentPickerDisable")
						 && ModCompatibility.sharedFunctions.ContainsKey("RefreshObjectGUI"))
					{
						if (ModCompatibility.sharedFunctions.ContainsKey("SelectinMultiple")
						&& (bool)ModCompatibility.sharedFunctions["SelectinMultiple"])
						{
							bool success = false;
							foreach (var otherTimelineObject in selectedObjects.Where(x => x.IsBeatmapObject))
							{
								success = SetParent(otherTimelineObject, beatmapObject);
							}

							if (!success)
								EditorManager.inst.DisplayNotification("Cannot set parent to child / self!", 1f, EditorManager.NotificationType.Warning);
							else
								((Action)ModCompatibility.sharedFunctions["ParentPickerDisable"])?.Invoke();

							return;
						}

						var tryParent = SetParent(currentSelection, beatmapObject);

						if (!tryParent)
							EditorManager.inst.DisplayNotification("Cannot set parent to child / self!", 1f, EditorManager.NotificationType.Warning);
						else
							((Action)ModCompatibility.sharedFunctions["ParentPickerDisable"])?.Invoke();
					}
				}
			}
		}

		public static bool SetParent(TimelineObject currentSelection, BeatmapObject beatmapObjectToParentTo)
		{
			var dictionary = new Dictionary<string, bool>();

			foreach (var obj in DataManager.inst.gameData.beatmapObjects)
			{
				bool flag = true;
				if (!string.IsNullOrEmpty(obj.parent))
				{
					string parentID = currentSelection.ID;
					while (!string.IsNullOrEmpty(parentID))
					{
						if (parentID == obj.parent)
						{
							flag = false;
							break;
						}
						int num2 = DataManager.inst.gameData.beatmapObjects.FindIndex(x => x.parent == parentID);
						if (num2 != -1)
						{
							parentID = DataManager.inst.gameData.beatmapObjects[num2].id;
						}
						else
						{
							parentID = null;
						}
					}
				}
				if (!dictionary.ContainsKey(obj.id))
					dictionary.Add(obj.id, flag);
			}

			if (dictionary.ContainsKey(currentSelection.ID))
				dictionary[currentSelection.ID] = false;

			if (dictionary.ContainsKey(beatmapObjectToParentTo.id) && dictionary[beatmapObjectToParentTo.id])
			{
				currentSelection.GetData<BeatmapObject>().parent = beatmapObjectToParentTo.id;
				var bm = currentSelection.GetData<BeatmapObject>();
				Updater.UpdateProcessor(bm);
				((Action<BeatmapObject>)ModCompatibility.sharedFunctions["RefreshObjectGUI"])?.Invoke(bm);
			}

			return dictionary.ContainsKey(beatmapObjectToParentTo.id) && dictionary[beatmapObjectToParentTo.id];
		}

		void OnMouseEnter()
        {
            hovered = true;
			onMouseEnter?.Invoke();

            if (TipEnabled && EditorManager.inst != null)
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

        void OnMouseExit()
        {
            hovered = false;
			onMouseExit?.Invoke();
			if (TipEnabled && EditorManager.inst != null)
			{
				EditorManager.inst.SetTooltipDisappear(0.5f);
			}
		}

		void OnMouseDrag()
        {
			onMouseDrag?.Invoke();

			dragTime = Time.time;
			if (EditorManager.inst && EditorManager.inst.isEditing && dragTime > startDragTime + 0.1f && CanDrag && Enabled && !EventSystem.current.IsPointerOverGameObject())
            {
				var vector = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.localPosition.z);
				var vector2 = Camera.main.ScreenToWorldPoint(vector);
				var vector3 = new Vector3((float)((int)vector2.x), (float)((int)vector2.y), transform.localPosition.z);

				if (!dragging && selectedKeyframe == null)
				{
					dragging = true;
					SetCurrentKeyframe(0);
				}

				if (selectedKeyframe != null)
				{
					if (!setKeyframeValues)
                    {
						setKeyframeValues = true;
						dragKeyframeValues = new Vector2(selectedKeyframe.eventValues[0], selectedKeyframe.eventValues[1]);
						dragOffset = Input.GetKey(KeyCode.LeftShift) ? vector3 : vector2;
					}

					var finalVector = Input.GetKey(KeyCode.LeftShift) ? vector3 : vector2;

					if (Input.GetKey(KeyCode.LeftControl) && firstDirection == Axis.Static)
					{
						if (dragOffset.x > finalVector.x)
							firstDirection = Axis.PosX;

						if (dragOffset.x < finalVector.x)
							firstDirection = Axis.NegX;

						if (dragOffset.y > finalVector.y)
							firstDirection = Axis.PosY;

						if (dragOffset.y < finalVector.y)
							firstDirection = Axis.NegY;
					}

					if (firstDirection == Axis.Static || firstDirection == Axis.PosX || firstDirection == Axis.NegX)
						selectedKeyframe.eventValues[0] = dragKeyframeValues.x - dragOffset.x + (Input.GetKey(KeyCode.LeftShift) ? vector3.x : vector2.x);
					if (firstDirection == Axis.Static || firstDirection == Axis.PosY || firstDirection == Axis.NegY)
						selectedKeyframe.eventValues[1] = dragKeyframeValues.y - dragOffset.y + (Input.GetKey(KeyCode.LeftShift) ? vector3.y : vector2.y);
					Updater.UpdateProcessor(beatmapObject, "Keyframes");
				}
			}
		}

		float startDragTime;
		float dragTime;

		public void SetCurrentKeyframe(int type)
		{
			var timeOffset = AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime;
			int nextIndex = beatmapObject.events[type].FindIndex(x => x.eventTime >= timeOffset);
			if (nextIndex < 0)
				nextIndex = beatmapObject.events[type].Count - 1;

			int index;
			if (beatmapObject.events[type].Has(x => x.eventTime > timeOffset - 0.1f && x.eventTime < timeOffset + 0.1f))
			{
				selectedKeyframe = (EventKeyframe)beatmapObject.events[type].Find(x => x.eventTime > timeOffset - 0.1f && x.eventTime < timeOffset + 0.1f);
				index = beatmapObject.events[type].FindIndex(x => x.eventTime > timeOffset - 0.1f && x.eventTime < timeOffset + 0.1f);
				AudioManager.inst.CurrentAudioSource.time = selectedKeyframe.eventTime + beatmapObject.StartTime;
			}
			else if (CreateKeyframe)
			{
				selectedKeyframe = EventKeyframe.DeepCopy((EventKeyframe)beatmapObject.events[type][nextIndex]);
				selectedKeyframe.eventTime = timeOffset;
				index = beatmapObject.events[type].Count;
				beatmapObject.events[type].Add(selectedKeyframe);
			}
			else
			{
				index = beatmapObject.events[type].FindLastIndex(x => x.eventTime < timeOffset);
				selectedKeyframe = (EventKeyframe)beatmapObject.events[type][index];
			}

			if (ModCompatibility.mods.ContainsKey("EditorManagement"))
			{
				var mod = ModCompatibility.mods["EditorManagement"];
				if (mod.Methods.ContainsKey("RenderKeyframes"))
					mod.Methods["RenderKeyframes"].DynamicInvoke(beatmapObject);
				if (mod.Methods.ContainsKey("SetCurrentKeyframe"))
					mod.Methods["SetCurrentKeyframe"].DynamicInvoke(beatmapObject, type, index, false, false);
			}
		}

        void Update()
		{
			if (!EditorManager.inst || !EditorManager.inst.isEditing)
			{
				hovered = false;
				return;
			}

			if (ModCompatibility.sharedFunctions.ContainsKey("CurrentSelection") && ModCompatibility.sharedFunctions["CurrentSelection"] is TimelineObject currentSelection &&
				currentSelection.ID == beatmapObject.id)
			{
				GameStorageManager.inst.objectDragger.position = new Vector3(transform.parent.position.x, transform.parent.position.y, transform.parent.position.z - 10f);
				GameStorageManager.inst.objectDragger.rotation = transform.parent.rotation;
			}

			var m = 0f;

			if (beatmapObject != null && ShowObjectsOnlyOnLayer && beatmapObject.editorData.layer != EditorManager.inst.layer)
				m = -renderer.material.color.a + LayerOpacity;

			if (!hovered && renderer != null && renderer.material.HasProperty("_Color"))
				renderer.material.color += new Color(0f, 0f, 0f, m);

			if (HighlightObjects && hovered && renderer != null && renderer.material.HasProperty("_Color"))
			{
				var color = Input.GetKey(KeyCode.LeftShift) ? new Color(
					renderer.material.color.r > 0.9f ? -HighlightDoubleColor.r : HighlightDoubleColor.r,
					renderer.material.color.g > 0.9f ? -HighlightDoubleColor.g : HighlightDoubleColor.g,
					renderer.material.color.b > 0.9f ? -HighlightDoubleColor.b : HighlightDoubleColor.b,
					0f) : new Color(
					renderer.material.color.r > 0.9f ? -HighlightColor.r : HighlightColor.r,
					renderer.material.color.g > 0.9f ? -HighlightColor.g : HighlightColor.g,
					renderer.material.color.b > 0.9f ? -HighlightColor.b : HighlightColor.b,
					0f);

				renderer.material.color += color;
			}

			if (EditorManager.inst.showHelp && beatmapObject != null)
			{
				TipEnabled = true;

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
					"<br>ED: {L: " + beatmapObject.editorData.layer + ", B: " + beatmapObject.editorData.Bin + "}" +
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
						"<br>ED: {L: " + beatmapObject.editorData.layer + ", B: " + beatmapObject.editorData.Bin + "}" +
						"<br>POS: {X: " + transform.position.x + ", Y: " + transform.position.y + ", Z: " + transform.position.z + "}" +
						"<br>SCA: {X: " + transform.localScale.x + ", Y: " + transform.localScale.y + "}" +
						"<br>ROT: " + transform.eulerAngles.z +
						"<br>COL: " + RTHelpers.ColorToHex(col) +
						ptr;
				}
			}
		}

		void FixedUpdate()
		{
			if (ModCompatibility.mods.ContainsKey("EditorManagement"))
			{
				var mod = ModCompatibility.mods["EditorManagement"];

				if (dragging && mod.Methods.ContainsKey("RenderKeyframeDialog"))
					mod.Methods["RenderKeyframeDialog"].DynamicInvoke(beatmapObject);
			}
		}

        public List<HoverTooltip.Tooltip> tooltipLanguages = new List<HoverTooltip.Tooltip>();
	}
}
