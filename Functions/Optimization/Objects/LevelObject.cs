using System.Collections.Generic;

using UnityEngine;

using RTFunctions.Functions.Animation;
using RTFunctions.Functions.Animation.Keyframe;

using RTFunctions.Functions.Optimization.Objects.Visual;

namespace RTFunctions.Functions.Optimization.Objects
{
    public class LevelObject : Exists, ILevelObject
    {
        public float StartTime { get; }
        public float KillTime { get; }

        public string ID { get; }
        readonly Sequence<Color> colorSequence;
        readonly Sequence<float> opacitySequence;
        readonly Sequence<float> hueSequence;
        readonly Sequence<float> satSequence;
        readonly Sequence<float> valSequence;

        readonly float depth;
        public List<LevelParentObject> parentObjects;
        public readonly VisualObject visualObject;

        public readonly List<Transform> transformChain;

        public LevelObject(string _id, float startTime, float killTime, Sequence<Color> colorSequence, float depth, List<LevelParentObject> parentObjects, VisualObject visualObject, Sequence<float> _os, Sequence<float> _hs, Sequence<float> _ss, Sequence<float> _vs)
        {
            ID = _id;
            StartTime = startTime;
            KillTime = killTime;
            this.colorSequence = colorSequence;
            this.depth = depth;
            this.parentObjects = parentObjects;
            this.visualObject = visualObject;
            opacitySequence = _os;
            hueSequence = _hs;
            satSequence = _ss;
            valSequence = _vs;

            try
            {
                var list = new List<Transform>();
                var tf1 = visualObject.GameObject.transform;

                while (tf1.parent != null && tf1.parent.gameObject.name != "GameObjects")
                {
                    tf1 = tf1.parent;
                }

                list.Add(tf1);

                while (tf1.childCount != 0 && tf1.GetChild(0) != null)
                {
                    tf1 = tf1.GetChild(0);
                    list.Add(tf1);
                }

                transformChain = list;
            }
            catch
            {

            }
        }

        public void SetActive(bool active)
        {
            if (parentObjects.Count > 0)
                parentObjects[parentObjects.Count - 1].GameObject.SetActive(active);
        }

        public static Color ChangeColorHSV(Color color, float hue, float sat, float val)
        {
            double num;
            double saturation;
            double value;
            LSFunctions.LSColors.ColorToHSV(color, out num, out saturation, out value);
            return LSFunctions.LSColors.ColorFromHSV(num + hue, saturation + sat, value + val);
        }

        public void Interpolate(float time)
        {
            // Set visual object color
            if (opacitySequence != null && hueSequence != null && satSequence != null && valSequence != null)
            {
                Color color = colorSequence.Interpolate(time - StartTime);
                float opacity = opacitySequence.Interpolate(time - StartTime);

                float hue = hueSequence.Interpolate(time - StartTime);
                float sat = satSequence.Interpolate(time - StartTime);
                float val = valSequence.Interpolate(time - StartTime);

                float a = opacity - 1f;

                a = -a;

                float b = 1f;
                if (a >= 0f && a <= 1f)
                {
                    b = color.a * a;
                }
                else
                {
                    b = color.a;
                }

                visualObject.SetColor(LSFunctions.LSColors.fadeColor(ChangeColorHSV(color, hue, sat, val), b));
            }
            else if (opacitySequence != null)
            {
                Color color = colorSequence.Interpolate(time - StartTime);
                float opacity = opacitySequence.Interpolate(time - StartTime);
                
                float a = opacity - 1f;

                a = -a;

                float b = 1f;
                if (a >= 0f && a <= 1f)
                {
                    b = color.a * a;
                }
                else
                {
                    b = color.a;
                }

                visualObject.SetColor(LSFunctions.LSColors.fadeColor(color, b));
            }
            else
            {
                Color color = colorSequence.Interpolate(time - StartTime);
                visualObject.SetColor(color);
            }

            // Update parents
            float positionOffset = 0.0f;
            float scaleOffset = 0.0f;
            float rotationOffset = 0.0f;

            bool animatePosition = true;
            bool animateScale = true;
            bool animateRotation = true;

            foreach (LevelParentObject parentObject in parentObjects)
            {
                // If last parent is position parented, animate position
                if (animatePosition)
                {
                    if (parentObject.Position3DSequence != null)
                    {
                        Vector3 value = parentObject.Position3DSequence.Interpolate(time - parentObject.TimeOffset - positionOffset);
                        float z = depth * 0.0005f;
                        float calc = value.z / 10f;
                        z = z + calc;
                        parentObject.Transform.localPosition = new Vector3(value.x, value.y, z);
                    }
                    else
                    {
                        Vector2 value = parentObject.PositionSequence.Interpolate(time - parentObject.TimeOffset - positionOffset);
                        parentObject.Transform.localPosition = new Vector3(value.x, value.y, depth * 0.0005f);
                    }
                }

                // If last parent is scale parented, animate scale
                if (animateScale)
                {
                    Vector2 value = parentObject.ScaleSequence.Interpolate(time - parentObject.TimeOffset - scaleOffset);
                    parentObject.Transform.localScale = new Vector3(value.x, value.y, 1.0f);
                }

                // If last parent is rotation parented, animate rotation
                if (animateRotation)
                {
                    parentObject.Transform.localRotation = Quaternion.AngleAxis(
                        parentObject.RotationSequence.Interpolate(time - parentObject.TimeOffset - rotationOffset),
                        Vector3.forward);
                }

                // Cache parent values to use for next parent
                positionOffset = parentObject.ParentOffsetPosition;
                scaleOffset = parentObject.ParentOffsetScale;
                rotationOffset = parentObject.ParentOffsetRotation;

                animatePosition = parentObject.ParentAnimatePosition;
                animateScale = parentObject.ParentAnimateScale;
                animateRotation = parentObject.ParentAnimateRotation;
            }
        }
    }
}
