using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using LSFunctions;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;

namespace RTFunctions.Functions.Components
{
	/// <summary>
	/// Component for selecting and dragging objects. Still needs a ton of work though.
	/// </summary>
    public class RTObject : MonoBehaviour
    {
		public bool CanDrag => ModCompatibility.sharedFunctions.ContainsKey("SelectedObjectCount") && ((int)ModCompatibility.sharedFunctions["SelectedObjectCount"]) < 2;
		public static bool Enabled { get; set; }

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

		public RTRotator rotator;

		public EmptyActiveHandler emptyHandler;

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

		public RTScaler top;
		public RTScaler left;
		public RTScaler right;
		public RTScaler bottom;

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
			if (GetComponent<Renderer>())
				renderer = GetComponent<Renderer>();
		}

		public void GenerateDraggers()
		{
			if (!EditorManager.inst || !Selected)
				return;

			if (!rotator)
			{
				var rotator = ObjectManager.inst.objectPrefabs[1].options[4].transform.GetChild(0).gameObject.Duplicate(transform.parent, "Rotator");
				Destroy(rotator.GetComponent<SelectObjectInEditor>());
				rotator.tag = "Helper";
				rotator.transform.localScale = new Vector3(2f, 2f, 1f);
				var rotatorRenderer = rotator.GetComponent<Renderer>();
				rotatorRenderer.enabled = true;
				rotatorRenderer.material.color = new Color(0f, 0f, 1f);
				rotator.GetComponent<Collider2D>().enabled = true;
				this.rotator = rotator.AddComponent<RTRotator>();
				this.rotator.refObject = this;

				rotator.SetActive(false);
			}

			if (!top)
			{
				top = CreateScaler(Axis.PosY, Color.green);
				top.gameObject.SetActive(false);
			}
			if (!left)
			{
				left = CreateScaler(Axis.PosX, Color.red);
				left.gameObject.SetActive(false);
			}
			if (!bottom)
			{
				bottom = CreateScaler(Axis.NegY, Color.green);
				bottom.gameObject.SetActive(false);
			}
			if (!right)
			{
				right = CreateScaler(Axis.NegX, Color.red);
				right.gameObject.SetActive(false);
			}
			if (!emptyHandler)
			{
				emptyHandler = transform.parent.gameObject.AddComponent<EmptyActiveHandler>();
				emptyHandler.refObject = this;
			}
		}

		RTScaler CreateScaler(Axis axis, Color color)
		{
			var scaler = ObjectManager.inst.objectPrefabs[3].options[0].transform.GetChild(0).gameObject.Duplicate(transform.parent, "Scaler");
			Destroy(scaler.GetComponent<SelectObjectInEditor>());
			scaler.tag = "Helper";
			scaler.transform.localScale = new Vector3(2f, 2f, 1f);
			scaler.GetComponent<Collider2D>().enabled = true;

			var scalerRenderer = scaler.GetComponent<Renderer>();
			scalerRenderer.enabled = true;
			scalerRenderer.material.color = color;

			var s = scaler.AddComponent<RTScaler>();
			s.refObject = this;
			s.axis = axis;
			return s;
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

					TimelineObject timelineObject;

					if (mod.Methods.ContainsKey("GetTimelineObject"))
						timelineObject = (TimelineObject)mod.Methods["GetTimelineObject"].DynamicInvoke(beatmapObject);
					else
						timelineObject = new TimelineObject(beatmapObject);

					if (mod.Methods.ContainsKey("RenderTimelineObjectVoid"))
						mod.Methods["RenderTimelineObjectVoid"].DynamicInvoke(timelineObject);

					if (!timelineObject.selected)
					{
						if (mod.Methods.ContainsKey("SetCurrentObject") && !Input.GetKey(KeyCode.LeftShift))
							mod.Methods["SetCurrentObject"].DynamicInvoke(timelineObject, Input.GetKey(KeyCode.LeftAlt));
						if (mod.Methods.ContainsKey("AddSelectedObject") && Input.GetKey(KeyCode.LeftShift))
							mod.Methods["AddSelectedObject"].DynamicInvoke(timelineObject);
					}
				}
			}
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
			if (EditorManager.inst && EditorManager.inst.isEditing && dragTime > startDragTime + 0.1f && CanDrag && Enabled)
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
			else
			{
				selectedKeyframe = EventKeyframe.DeepCopy((EventKeyframe)beatmapObject.events[type][nextIndex]);
				selectedKeyframe.eventTime = timeOffset;
				index = beatmapObject.events[type].Count;
				beatmapObject.events[type].Add(selectedKeyframe);
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
				return;

			var m = 0f;

			if (beatmapObject != null && ShowObjectsOnlyOnLayer && beatmapObject.editorData.layer != EditorManager.inst.layer)
				m = -renderer.material.color.a + LayerOpacity;

			if (!hovered && renderer != null && renderer.material.HasProperty("_Color"))
            {
				renderer.material.color += new Color(0f, 0f, 0f, m);
            }

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

			if (!EditorManager.inst || !Selected)
            {
				if (emptyHandler)
					Destroy(emptyHandler);
				if (rotator)
					Destroy(rotator.gameObject);
				if (top)
					Destroy(top.gameObject);
				if (left)
					Destroy(left.gameObject);
				if (bottom)
					Destroy(bottom.gameObject);
				if (right)
					Destroy(right.gameObject);
            }
		}

        public List<HoverTooltip.Tooltip> tooltipLanguages = new List<HoverTooltip.Tooltip>();
	}
}
