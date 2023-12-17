using UnityEngine;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;

namespace RTFunctions.Functions.Components
{
    /// <summary>
    /// Component for handling drag scale.
    /// </summary>
    public class RTScaler : MonoBehaviour
    {
        public static float ScalerOffset { get; set; } = 6f;
        public static float ScalerScale { get; set; } = 1.6f;

        public RTObject refObject;

        bool dragging;

        bool setKeyframeValues;
        Vector2 dragKeyframeValues;
        Vector2 dragOffset;

        public RTObject.Axis axis = RTObject.Axis.Static;

        void Update()
        {
            switch (axis)
            {
                case RTObject.Axis.PosX:
                    transform.localPosition = new Vector3(ScalerOffset, 0f);
                    transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    break;
                case RTObject.Axis.PosY:
                    transform.localPosition = new Vector3(0f, ScalerOffset);
                    transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    break;
                case RTObject.Axis.NegX:
                    transform.localPosition = new Vector3(-ScalerOffset, 0f);
                    transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                    break;
                case RTObject.Axis.NegY:
                    transform.localPosition = new Vector3(0f, -ScalerOffset);
                    transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                    break;
            }
            transform.localScale = new Vector3(ScalerScale, ScalerScale, 1f);
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
                var vector2 = Camera.main.ScreenToWorldPoint(vector) * 0.2f;
                var vector3 = new Vector3(RTMath.RoundToNearestDecimal(vector2.x, 1), RTMath.RoundToNearestDecimal(vector2.y, 1), transform.localPosition.z);
                if (!dragging)
                {
                    dragging = true;
                    refObject.SetCurrentKeyframe(1);
                }

                if (refObject.selectedKeyframe != null)
                {
                    if (!setKeyframeValues)
                    {
                        setKeyframeValues = true;
                        dragKeyframeValues = new Vector2(refObject.selectedKeyframe.eventValues[0], refObject.selectedKeyframe.eventValues[1]);
                        dragOffset = Input.GetKey(KeyCode.LeftShift) ? vector3 : vector2;
                    }

                    var finalVector = Input.GetKey(KeyCode.LeftShift) ? vector3 : vector2;

                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        float total = Vector2.Distance(finalVector, dragOffset);

                        if (axis == RTObject.Axis.PosX && dragOffset.x - finalVector.x > 0f)
                            total = -total;
                        
                        if (axis == RTObject.Axis.PosY && dragOffset.y - finalVector.y > 0f)
                            total = -total;
                        
                        if (axis == RTObject.Axis.NegX && dragOffset.x - finalVector.x < 0f)
                            total = -total;
                        
                        if (axis == RTObject.Axis.NegY && dragOffset.y - finalVector.y < 0f)
                            total = -total;

                        refObject.selectedKeyframe.eventValues[0] = dragKeyframeValues.x + total;
                        refObject.selectedKeyframe.eventValues[1] = dragKeyframeValues.y + total;
                    }
                    else
                    {
                        if (axis == RTObject.Axis.PosX)
                            refObject.selectedKeyframe.eventValues[0] = dragKeyframeValues.x - dragOffset.x + finalVector.x;
                        if (axis == RTObject.Axis.NegX)
                            refObject.selectedKeyframe.eventValues[0] = dragKeyframeValues.x + dragOffset.x - finalVector.x;
                        if (axis == RTObject.Axis.PosY)
                            refObject.selectedKeyframe.eventValues[1] = dragKeyframeValues.y - dragOffset.y + finalVector.y;
                        if (axis == RTObject.Axis.NegY)
                            refObject.selectedKeyframe.eventValues[1] = dragKeyframeValues.y + dragOffset.y - finalVector.y;
                    }

                    Updater.UpdateProcessor(refObject.beatmapObject, "Keyframes");
                }
            }
        }
    }
}
