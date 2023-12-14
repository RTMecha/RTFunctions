using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.Components
{
    public class EmptyActiveHandler : MonoBehaviour
    {
        public RTObject refObject;
        public BeatmapObject beatmapObject;

		void FixedUpdate()
        {
			if (refObject && beatmapObject)
			{
				refObject.rotator.gameObject.SetActive(Selected && refObject.CanDrag);
				refObject.top.gameObject.SetActive(Selected && refObject.CanDrag);
				refObject.left.gameObject.SetActive(Selected && refObject.CanDrag);
				refObject.bottom.gameObject.SetActive(Selected && refObject.CanDrag);
				refObject.right.gameObject.SetActive(Selected && refObject.CanDrag);

				if (beatmapObject.objectType == DataManager.GameData.BeatmapObject.ObjectType.Empty)
					refObject.gameObject.SetActive(Selected && refObject.CanDrag);
			}
		}

		public bool Selected
		{
			get
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

				return false;
			}
		}

	}
}
