using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using LSFunctions;

using RTFunctions.Functions;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;

namespace RTFunctions.Functions.Components
{
    public class RTObject : MonoBehaviour
    {
		public bool CanDrag => ModCompatibility.sharedFunctions.ContainsKey("SelectedObjectCount") && ((int)ModCompatibility.sharedFunctions["SelectedObjectCount"]) < 2;

		public static bool TipEnabled { get; set; }
		public string id;

		public BeatmapObject beatmapObject;

		Renderer renderer;

		public RTRotator rotator;

		public EmptyActiveHandler emptyHandler;

		#region Highlighting

		public bool hovered;

		public Color highlightColor;
		public Color highlightDoubleColor;
		public bool highlightObjects;
		public float layerOpacity = 0.5f;
		public bool showObjectsOnlyOnLayer;

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
			//if (EditorManager.inst == null)
			//	Destroy(this);

			if (GetComponent<Renderer>())
				renderer = GetComponent<Renderer>();

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

            {
                //var scalertop = ObjectManager.inst.objectPrefabs[3].options[0].transform.GetChild(0).gameObject.Duplicate(transform.parent);
                //Destroy(scalertop.GetComponent<SelectObjectInEditor>());
                //scalertop.tag = "Helper";
                //scalertop.transform.localScale = new Vector3(2f, 2f, 1f);
                //scalertop.GetComponent<Collider2D>().enabled = true;

                //var scalertopRenderer = scalertop.GetComponent<Renderer>();
                //scalertopRenderer.enabled = true;
                //scalertopRenderer.material.color = new Color(0f, 1f, 0f);

                //top = scalertop.AddComponent<RTScaler>();
                //top.refObject = this;
                //top.axis = Axis.PosY;

                //var scalerleft = ObjectManager.inst.objectPrefabs[3].options[0].transform.GetChild(0).gameObject.Duplicate(transform.parent);
                //Destroy(scalerleft.GetComponent<SelectObjectInEditor>());
                //scalerleft.tag = "Helper";
                //scalerleft.transform.localScale = new Vector3(2f, 2f, 1f);
                //scalerleft.GetComponent<Collider2D>().enabled = true;

                //var scalerleftRenderer = scalerleft.GetComponent<Renderer>();
                //scalerleftRenderer.enabled = true;
                //scalerleftRenderer.material.color = new Color(1f, 0f, 0f);

                //left = scalerleft.AddComponent<RTScaler>();
                //left.refObject = this;
                //left.axis = Axis.PosX;

                //var scalerbottom = ObjectManager.inst.objectPrefabs[3].options[0].transform.GetChild(0).gameObject.Duplicate(transform.parent);
                //Destroy(scalerbottom.GetComponent<SelectObjectInEditor>());
                //scalerbottom.tag = "Helper";
                //scalerbottom.transform.localScale = new Vector3(2f, 2f, 1f);
                //scalerbottom.GetComponent<Collider2D>().enabled = true;

                //var scalerbottomRenderer = scalerbottom.GetComponent<Renderer>();
                //scalerbottomRenderer.enabled = true;
                //scalerbottomRenderer.material.color = new Color(0f, 1f, 0f);

                //bottom = scalerbottom.AddComponent<RTScaler>();
                //bottom.refObject = this;
                //bottom.axis = Axis.NegY;

                //var scalerright = ObjectManager.inst.objectPrefabs[3].options[0].transform.GetChild(0).gameObject.Duplicate(transform.parent);
                //Destroy(scalerright.GetComponent<SelectObjectInEditor>());
                //scalerright.tag = "Helper";
                //scalerright.transform.localScale = new Vector3(2f, 2f, 1f);
                //scalerright.GetComponent<Collider2D>().enabled = true;

                //var scalerrightRenderer = scalerright.GetComponent<Renderer>();
                //scalerrightRenderer.enabled = true;
                //scalerrightRenderer.material.color = new Color(1f, 0f, 0f);

                //right = scalerright.AddComponent<RTScaler>();
                //right.refObject = this;
                //right.axis = Axis.NegX;

                top = CreateScaler(Axis.PosY, Color.green);
				left = CreateScaler(Axis.PosX, Color.red);
				bottom = CreateScaler(Axis.NegY, Color.green);
				right = CreateScaler(Axis.NegX, Color.red);
			}

            emptyHandler = transform.parent.gameObject.AddComponent<EmptyActiveHandler>();
			emptyHandler.refObject = this;
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
			this.beatmapObject = beatmapObject;
			emptyHandler.beatmapObject = beatmapObject;
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
			if (EditorManager.inst != null && EditorManager.inst.isEditing && DataManager.inst.gameData.beatmapObjects.Count > 0 && !string.IsNullOrEmpty(id) && !LSHelpers.IsUsingInputField() && !EventSystem.current.IsPointerOverGameObject())
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
			if (EditorManager.inst && EditorManager.inst.isEditing && dragTime > startDragTime + 0.1f && CanDrag)
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
			var m = 0f;

			if (beatmapObject != null && showObjectsOnlyOnLayer && beatmapObject.editorData.layer != EditorManager.inst.layer)
				m = -layerOpacity;

			if (EditorManager.inst != null && EditorManager.inst.isEditing && !hovered && renderer != null && renderer.material.HasProperty("_Color"))
            {
				renderer.material.color += new Color(0f, 0f, 0f, m);
            }

			if (EditorManager.inst != null && EditorManager.inst.isEditing && highlightObjects && hovered && renderer != null && renderer.material.HasProperty("_Color"))
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
