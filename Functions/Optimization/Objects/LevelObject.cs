using System.Collections.Generic;

using UnityEngine;

using RTFunctions.Functions.Animation;

using RTFunctions.Functions.Optimization.Objects.Visual;

namespace RTFunctions.Functions.Optimization.Objects
{
    public class LevelObject : Exists, ILevelObject
    {
        public float StartTime { get; set; }
        public float KillTime { get; set; }

        public string ID { get; }
        Sequence<Color> colorSequence;
        Sequence<float> opacitySequence;
        Sequence<float> hueSequence;
        Sequence<float> satSequence;
        Sequence<float> valSequence;

        public float depth;
        public List<LevelParentObject> parentObjects;
        public readonly VisualObject visualObject;

        public readonly List<Transform> transformChain;

        Data.BeatmapObject beatmapObject;

        public void SetSequences(Sequence<Color> colorSequence, Sequence<float> opacitySequence, Sequence<float> hueSequence, Sequence<float> satSequence, Sequence<float> valSequence)
        {
            this.colorSequence = colorSequence;
            this.opacitySequence = opacitySequence;
            this.hueSequence = hueSequence;
            this.satSequence = satSequence;
            this.valSequence = valSequence;
        }

        public LevelObject(Data.BeatmapObject beatmapObject, Sequence<Color> colorSequence, List<LevelParentObject> parentObjects, VisualObject visualObject, Sequence<float> opacitySequence, Sequence<float> hueSequence, Sequence<float> satSequence, Sequence<float> valSequence)
        {
            this.beatmapObject = beatmapObject;
            ID = beatmapObject.id;
            StartTime = beatmapObject.StartTime;
            KillTime = beatmapObject.StartTime + beatmapObject.GetObjectLifeLength(_oldStyle: true);
            this.colorSequence = colorSequence;
            depth = beatmapObject.depth;
            this.opacitySequence = opacitySequence;
            this.hueSequence = hueSequence;
            this.satSequence = satSequence;
            this.valSequence = valSequence;
        }

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
                parentObjects[parentObjects.Count - 1].GameObject?.SetActive(active);
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

                float b = a >= 0f && a <= 1f ? color.a * a : color.a;
                //if (a >= 0f && a <= 1f)
                //{
                //    b = color.a * a;
                //}
                //else
                //{
                //    b = color.a;
                //}

                visualObject.SetColor(LSFunctions.LSColors.fadeColor(ChangeColorHSV(color, hue, sat, val), b));
            }
            else if (opacitySequence != null)
            {
                Color color = colorSequence.Interpolate(time - StartTime);
                float opacity = opacitySequence.Interpolate(time - StartTime);
                
                float a = opacity - 1f;

                a = -a;

                float b = a >= 0f && a <= 1f ? color.a * a : color.a;
                //if (a >= 0f && a <= 1f)
                //{
                //    b = color.a * a;
                //}
                //else
                //{
                //    b = color.a;
                //}

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

            float positionParallax = 1f;
            float scaleParallax = 1f;
            float rotationParallax = 1f;

            foreach (var parentObject in parentObjects)
            {
                // If last parent is position parented, animate position
                if (animatePosition)
                {
                    if (parentObject.Position3DSequence != null)
                    {
                        var value = parentObject.Position3DSequence.Interpolate(time - parentObject.TimeOffset - positionOffset);
                        //float z = depth * 0.0005f;
                        //float calc = value.z / 10f;
                        //z = z + calc;

                        float z = depth * 0.0005f + (value.z / 10f);

                        parentObject.Transform.localPosition = new Vector3(value.x * positionParallax, value.y * positionParallax, z);
                    }
                    else
                    {
                        Vector2 value = parentObject.PositionSequence.Interpolate(time - parentObject.TimeOffset - positionOffset);
                        parentObject.Transform.localPosition = new Vector3(value.x * positionParallax, value.y * positionParallax, depth * 0.0005f);
                    }
                }

                // If last parent is scale parented, animate scale
                if (animateScale)
                {
                    var value = parentObject.ScaleSequence.Interpolate(time - parentObject.TimeOffset - scaleOffset);
                    parentObject.Transform.localScale = new Vector3(value.x * scaleParallax, value.y * scaleParallax, 1.0f);
                }

                // If last parent is rotation parented, animate rotation
                if (animateRotation)
                {
                    parentObject.Transform.localRotation = Quaternion.AngleAxis(
                        parentObject.RotationSequence.Interpolate(time - parentObject.TimeOffset - rotationOffset) * rotationParallax,
                        Vector3.forward);
                }

                // Cache parent values to use for next parent
                positionOffset = parentObject.ParentAdditivePosition ? positionOffset + parentObject.ParentOffsetPosition : parentObject.ParentOffsetPosition;
                scaleOffset = parentObject.ParentAdditiveScale ? scaleOffset + parentObject.ParentOffsetScale : parentObject.ParentOffsetScale;
                rotationOffset = parentObject.ParentAdditiveRotation ? rotationOffset + parentObject.ParentOffsetRotation : parentObject.ParentOffsetRotation;

                animatePosition = parentObject.ParentAnimatePosition;
                animateScale = parentObject.ParentAnimateScale;
                animateRotation = parentObject.ParentAnimateRotation;

                //positionParallax = parentObject.ParentAdditivePosition ? positionParallax + parentObject.ParentParallaxPosition : parentObject.ParentParallaxPosition;
                //scaleParallax = parentObject.ParentAdditiveScale ? scaleParallax + parentObject.ParentParallaxScale : parentObject.ParentParallaxScale;
                //rotationParallax = parentObject.ParentAdditiveRotation ? rotationParallax + parentObject.ParentParallaxRotation : parentObject.ParentParallaxRotation;

                positionParallax = parentObject.ParentParallaxPosition;
                scaleParallax = parentObject.ParentParallaxScale;
                rotationParallax = parentObject.ParentParallaxRotation;
            }
        }
    }
}
