using UnityEngine;

namespace RTFunctions.Functions.Components
{
	/// <summary>
	/// Component for handling empty objects.
	/// </summary>
    public class EmptyActiveHandler : MonoBehaviour
    {
        public RTObject refObject;

		void FixedUpdate()
        {
			if (refObject && refObject.beatmapObject)
			{
				refObject.rotator.gameObject.SetActive(refObject.Selected && RTObject.Enabled && refObject.CanDrag);
				refObject.top.gameObject.SetActive(refObject.Selected && RTObject.Enabled && refObject.CanDrag);
				refObject.left.gameObject.SetActive(refObject.Selected && RTObject.Enabled && refObject.CanDrag);
				refObject.bottom.gameObject.SetActive(refObject.Selected && RTObject.Enabled && refObject.CanDrag);
				refObject.right.gameObject.SetActive(refObject.Selected && RTObject.Enabled && refObject.CanDrag);

				if (refObject.beatmapObject.objectType == DataManager.GameData.BeatmapObject.ObjectType.Empty)
					refObject.gameObject.SetActive(refObject.Selected && RTObject.Enabled && refObject.CanDrag);
			}
		}

	}
}
