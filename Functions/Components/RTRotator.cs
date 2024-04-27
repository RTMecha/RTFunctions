using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;
using UnityEngine;

namespace RTFunctions.Functions.Components
{
    /// <summary>
    /// Component for handling drag rotation.
    /// </summary>
    public class RTRotator : MonoBehaviour
    {
        public static float RotatorRadius { get; set; } = 22f;

        TimelineObject CurrentSelection => (TimelineObject)ModCompatibility.sharedFunctions["CurrentSelection"];
        EventKeyframe selectedKeyframe;
        bool dragging;
        float dragOffset;
        float dragKeyframeValues;
        bool setKeyframeValues;

        void Update()
        {
            transform.localScale = new Vector3(RotatorRadius, RotatorRadius, 1f);
        }

        void FixedUpdate()
        {
            if (!dragging || !ModCompatibility.mods.ContainsKey("EditorManagement"))
                return;

            var mod = ModCompatibility.mods["EditorManagement"];

            if (CurrentSelection.IsBeatmapObject && mod.Methods.ContainsKey("RenderKeyframeDialog"))
                mod.Methods["RenderKeyframeDialog"].DynamicInvoke(CurrentSelection.GetData<BeatmapObject>());
            else if (CurrentSelection.IsPrefabObject && mod.Methods.ContainsKey("RenderPrefabObjectDialog"))
                mod.Methods["RenderPrefabObjectDialog"].DynamicInvoke(CurrentSelection.GetData<PrefabObject>());
        }

        void OnMouseUp()
        {
            dragging = false;
            selectedKeyframe = null;
            setKeyframeValues = false;
        }

        void OnMouseDrag()
        {
            if (!EditorManager.inst || !EditorManager.inst.isEditing)
                return;

            var vector = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.localPosition.z);
            var vector2 = Camera.main.ScreenToWorldPoint(vector);

            if (CurrentSelection.IsPrefabObject)
            {
                selectedKeyframe = (EventKeyframe)CurrentSelection.GetData<PrefabObject>().events[2];

                dragging = true;

                Drag(vector2);

                return;
            }

            if (!dragging)
            {
                dragging = true;
                selectedKeyframe = RTObject.SetCurrentKeyframe(2, CurrentSelection.GetData<BeatmapObject>());
            }

            Drag(vector2);
        }

        void Drag(Vector3 vector2)
        {
            if (selectedKeyframe == null)
                return;

            var pos = new Vector3(
                CurrentSelection.IsPrefabObject ? CurrentSelection.GetData<PrefabObject>().events[0].eventValues[0] : transform.position.x,
                CurrentSelection.IsPrefabObject ? CurrentSelection.GetData<PrefabObject>().events[0].eventValues[1] : transform.position.y,
                0f);

            if (!setKeyframeValues)
            {
                setKeyframeValues = true;
                dragKeyframeValues = selectedKeyframe.eventValues[0];
                //dragOffset = Input.GetKey(KeyCode.LeftShift) ? RTMath.roundToNearest(vector2.y, 15f) : vector2.y;
                dragOffset = Input.GetKey(KeyCode.LeftShift) ? RTMath.roundToNearest(-RTMath.VectorAngle(pos, vector2), 15f) : -RTMath.VectorAngle(pos, vector2);
            }

            //refObject.selectedKeyframe.eventValues[0] =
            //    Input.GetKey(KeyCode.LeftShift) ? RTMath.roundToNearest(dragKeyframeValues - dragOffset + vector2.y, 15f) : dragKeyframeValues - dragOffset + vector2.y;
            selectedKeyframe.eventValues[0] =
                Input.GetKey(KeyCode.LeftShift) ? RTMath.roundToNearest(dragKeyframeValues - dragOffset + -RTMath.VectorAngle(pos, vector2), 15f) : dragKeyframeValues - dragOffset + -RTMath.VectorAngle(pos, vector2);

            if (CurrentSelection.IsPrefabObject)
                Updater.UpdatePrefab(CurrentSelection.GetData<PrefabObject>(), "Offset");
            else
                Updater.UpdateProcessor(CurrentSelection.GetData<BeatmapObject>(), "Keyframes");
        }
    }
}
