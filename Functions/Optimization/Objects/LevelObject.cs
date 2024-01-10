using System.Collections.Generic;
using System.Linq;

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

        public readonly List<Transform> transformChain = new List<Transform>();

        Data.BeatmapObject beatmapObject;

        public bool cameraParent;
        public bool positionParent;
        public bool scaleParent;
        public bool rotationParent;

        public float positionParentOffset;
        public float scaleParentOffset;
        public float rotationParentOffset;

        public Vector3 topPositionOffset;
        public Vector3 topScaleOffset;
        public Vector3 topRotationOffset;

        public Vector3 prefabOffsetPosition;
        public Vector3 prefabOffsetScale;
        public Vector3 prefabOffsetRotation;

        public void SetSequences(Sequence<Color> colorSequence, Sequence<float> opacitySequence, Sequence<float> hueSequence, Sequence<float> satSequence, Sequence<float> valSequence)
        {
            this.colorSequence = colorSequence;
            this.opacitySequence = opacitySequence;
            this.hueSequence = hueSequence;
            this.satSequence = satSequence;
            this.valSequence = valSequence;
        }

        public LevelObject(Data.BeatmapObject beatmapObject, Sequence<Color> colorSequence, List<LevelParentObject> parentObjects, VisualObject visualObject,
            Sequence<float> opacitySequence, Sequence<float> hueSequence, Sequence<float> satSequence, Sequence<float> valSequence,
            Vector3 prefabOffsetPosition, Vector3 prefabOffsetScale, Vector3 prefabOffsetRotation)
        {
            this.beatmapObject = beatmapObject;

            ID = beatmapObject.id;
            StartTime = beatmapObject.StartTime;
            KillTime = beatmapObject.StartTime + beatmapObject.GetObjectLifeLength(_oldStyle: true);
            depth = beatmapObject.depth;

            this.parentObjects = parentObjects;
            this.visualObject = visualObject;

            this.colorSequence = colorSequence;
            this.opacitySequence = opacitySequence;
            this.hueSequence = hueSequence;
            this.satSequence = satSequence;
            this.valSequence = valSequence;

            this.prefabOffsetPosition = prefabOffsetPosition;
            this.prefabOffsetScale = prefabOffsetScale;
            this.prefabOffsetRotation = prefabOffsetRotation;

            try
            {
                this.parentObjects.Reverse();

                transformChain.Add(this.parentObjects[0].Transform.parent);

                transformChain.AddRange(this.parentObjects.Select(x => x.Transform));

                this.parentObjects.Reverse();

                if (this.visualObject != null && this.visualObject.GameObject)
                    transformChain.Add(this.visualObject.GameObject.transform);

                var pc = beatmapObject.GetParentChain();

                if (pc != null && pc.Count > 0)
                {
                    var beatmapParent = (Data.BeatmapObject)pc[pc.Count - 1];

                    cameraParent = beatmapParent.parent == "CAMERA_PARENT";

                    positionParent = beatmapParent.GetParentType(0);
                    scaleParent = beatmapParent.GetParentType(1);
                    rotationParent = beatmapParent.GetParentType(2);

                    positionParentOffset = beatmapParent.parallaxSettings[0];
                    scaleParentOffset = beatmapParent.parallaxSettings[1];
                    rotationParentOffset = beatmapParent.parallaxSettings[2];
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{Updater.className}a\n{ex}");
            }
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
                //var list = new List<Transform>();
                //var tf1 = visualObject.GameObject.transform;

                //while (tf1.parent != null && tf1.parent.gameObject.name != "GameObjects")
                //{
                //    tf1 = tf1.parent;
                //}

                //list.Add(tf1);

                //while (tf1.childCount != 0 && tf1.GetChild(0) != null)
                //{
                //    tf1 = tf1.GetChild(0);
                //    list.Add(tf1);
                //}

                //transformChain = list;

                this.parentObjects.Reverse();

                transformChain.Add(this.parentObjects[0].Transform.parent);

                transformChain.AddRange(this.parentObjects.Select(x => x.Transform));

                this.parentObjects.Reverse();

                if (this.visualObject != null && this.visualObject.GameObject)
                    transformChain.Add(this.visualObject.GameObject.transform);

                topPositionOffset = transformChain[0].localPosition;
                topScaleOffset = transformChain[0].localScale;
                topRotationOffset = transformChain[0].localRotation.eulerAngles;

                var pc = beatmapObject.GetParentChain();

                if (pc != null && pc.Count > 0)
                {
                    var beatmapParent = (Data.BeatmapObject)pc[pc.Count - 1];

                    cameraParent = beatmapParent.parent == "CAMERA_PARENT";

                    positionParent = beatmapParent.GetParentType(0);
                    scaleParent = beatmapParent.GetParentType(1);
                    rotationParent = beatmapParent.GetParentType(2);

                    positionParentOffset = beatmapParent.parallaxSettings[0];
                    scaleParentOffset = beatmapParent.parallaxSettings[1];
                    rotationParentOffset = beatmapParent.parallaxSettings[2];
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{Updater.className}a\n{ex}");
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

            // Update Camera Parent
            if (positionParent && cameraParent)
            {
                var x = EventManager.inst.cam.transform.position.x;
                var y = EventManager.inst.cam.transform.position.y;

                transformChain[0].localPosition = (new Vector3(x, y, 0f) * positionParentOffset) + prefabOffsetPosition + topPositionOffset;
            }
            else
                transformChain[0].localPosition = prefabOffsetPosition + topPositionOffset;

            if (scaleParent && cameraParent)
            {
                float camOrthoZoom = EventManager.inst.cam.orthographicSize / 20f;

                transformChain[0].localScale = (new Vector3(camOrthoZoom, camOrthoZoom, 1f) * scaleParentOffset) + prefabOffsetScale + topScaleOffset;
            }
            else
                transformChain[0].localScale = prefabOffsetScale + topScaleOffset;

            if (rotationParent && cameraParent)
            {
                var camRot = EventManager.inst.camParent.transform.rotation.eulerAngles;

                transformChain[0].localRotation = Quaternion.Euler((camRot * rotationParentOffset) + prefabOffsetRotation + topRotationOffset);
            }
            else
                transformChain[0].localRotation = Quaternion.Euler(prefabOffsetRotation + topRotationOffset);

            // Update parents
            float positionOffset = 0.0f;
            float scaleOffset = 0.0f;
            float rotationOffset = 0.0f;

            float positionAddedOffset = 0.0f;
            float scaleAddedOffset = 0.0f;
            float rotationAddedOffset = 0.0f;

            bool animatePosition = true;
            bool animateScale = true;
            bool animateRotation = true;

            float positionParallax = 1f;
            float scaleParallax = 1f;
            float rotationParallax = 1f;

            int num = 0;
            foreach (var parentObject in parentObjects)
            {
                if (parentObject.ParentAdditivePosition)
                    positionAddedOffset += parentObject.ParentOffsetPosition;
                if (parentObject.ParentAdditiveScale)
                    scaleAddedOffset += parentObject.ParentOffsetScale;
                if (parentObject.ParentAdditiveRotation)
                    rotationAddedOffset += parentObject.ParentOffsetRotation;

                // If last parent is position parented, animate position
                if (animatePosition)
                {
                    if (parentObject.Position3DSequence != null)
                    {
                        var value = parentObject.Position3DSequence.Interpolate(time - parentObject.TimeOffset - (positionOffset + positionAddedOffset));

                        float z = depth * 0.0005f + (value.z / 10f);

                        parentObject.Transform.localPosition = new Vector3(value.x * positionParallax, value.y * positionParallax, z);
                    }
                    else
                    {
                        var value = parentObject.PositionSequence.Interpolate(time - parentObject.TimeOffset - (positionOffset + positionAddedOffset));
                        parentObject.Transform.localPosition = new Vector3(value.x * positionParallax, value.y * positionParallax, depth * 0.0005f);
                    }
                }

                // If last parent is scale parented, animate scale
                if (animateScale)
                {
                    var value = parentObject.ScaleSequence.Interpolate(time - parentObject.TimeOffset - (scaleOffset + scaleAddedOffset));
                    parentObject.Transform.localScale = new Vector3(value.x * scaleParallax, value.y * scaleParallax, 1.0f);
                }

                // If last parent is rotation parented, animate rotation
                if (animateRotation)
                {
                    parentObject.Transform.localRotation = Quaternion.AngleAxis(
                        parentObject.RotationSequence.Interpolate(time - parentObject.TimeOffset - (rotationOffset + rotationAddedOffset)) * rotationParallax,
                        Vector3.forward);
                }

                // Cache parent values to use for next parent
                positionOffset = parentObject.ParentOffsetPosition;
                scaleOffset = parentObject.ParentOffsetScale;
                rotationOffset = parentObject.ParentOffsetRotation;

                animatePosition = parentObject.ParentAnimatePosition;
                animateScale = parentObject.ParentAnimateScale;
                animateRotation = parentObject.ParentAnimateRotation;

                //positionParallax = parentObject.ParentAdditivePosition ? positionParallax + parentObject.ParentParallaxPosition : parentObject.ParentParallaxPosition;
                //scaleParallax = parentObject.ParentAdditiveScale ? scaleParallax + parentObject.ParentParallaxScale : parentObject.ParentParallaxScale;
                //rotationParallax = parentObject.ParentAdditiveRotation ? rotationParallax + parentObject.ParentParallaxRotation : parentObject.ParentParallaxRotation;

                positionParallax = parentObject.ParentParallaxPosition;
                scaleParallax = parentObject.ParentParallaxScale;
                rotationParallax = parentObject.ParentParallaxRotation;
                num++;
            }
        }
    }
}
