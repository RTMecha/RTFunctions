using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;

namespace RTFunctions.Functions.Components
{
    public class RTRotator : MonoBehaviour
    {
        public static float RotatorRadius { get; set; } = 22f;

        public RTObject refObject;

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
            if (ModCompatibility.mods.ContainsKey("EditorManagement"))
            {
                var mod = ModCompatibility.mods["EditorManagement"];

                if (dragging && mod.Methods.ContainsKey("RenderKeyframeDialog"))
                    mod.Methods["RenderKeyframeDialog"].DynamicInvoke(refObject.beatmapObject);
            }
        }

        void OnMouseUp()
        {
            dragging = false;
            refObject.selectedKeyframe = null;
            setKeyframeValues = false;
        }

        void OnMouseDrag()
        {
            if (EditorManager.inst && EditorManager.inst.isEditing)
            {
                var vector = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.localPosition.z);
                var vector2 = Camera.main.ScreenToWorldPoint(vector) * 2f;

                if (!dragging)
                {
                    dragging = true;
                    refObject.SetCurrentKeyframe(2);
                }

                if (refObject.selectedKeyframe != null)
                {
                    if (!setKeyframeValues)
                    {
                        setKeyframeValues = true;
                        dragKeyframeValues = refObject.selectedKeyframe.eventValues[0];
                        dragOffset = Input.GetKey(KeyCode.LeftShift) ? RTMath.roundToNearest(vector2.y, 15f) : vector2.y;
                    }

                    refObject.selectedKeyframe.eventValues[0] = Input.GetKey(KeyCode.LeftShift) ? RTMath.roundToNearest(dragKeyframeValues - dragOffset + vector2.y, 15f) : dragKeyframeValues - dragOffset + vector2.y;
                    //refObject.selectedKeyframe.eventValues[0] = dragKeyframeValues - dragOffset + (Input.GetKey(KeyCode.LeftShift) ? RTMath.roundToNearest(vector2.y, 15f) : vector2.y);
                    Updater.UpdateProcessor(refObject.beatmapObject, "Keyframes");
                }
            }
        }
    }
}
