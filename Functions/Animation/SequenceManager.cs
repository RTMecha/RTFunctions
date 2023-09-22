using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using RTFunctions.Functions.Animation.Keyframe;

namespace RTFunctions.Functions.Animation
{
    public class SequenceManager : MonoBehaviour
    {
        public static SequenceManager inst;
        public static SequenceManager Instance
        {
            get
            {
                if (inst != null)
                    return inst;

                var gameObject = new GameObject("SequenceManager");
                gameObject.transform.SetParent(SystemManager.inst.transform);
                var e = gameObject.AddComponent<SequenceManager>();

                Debug.LogFormat("{0}Init() => SequenceManager", FunctionsPlugin.className);

                return e;
            }
        }

        float speed = 10f;
        float add = 0.001f;
        float time = 0f;

        public static void Init()
        {
            if (inst != null) { Debug.LogFormat("{0}SequenceManager already exists!", FunctionsPlugin.className); return; };

            var gameObject = new GameObject("SequenceManager");
            gameObject.transform.SetParent(SystemManager.inst.transform);
            gameObject.AddComponent<SequenceManager>();

            Debug.LogFormat("{0}Init() => SequenceManager", FunctionsPlugin.className);
        }

        void Awake() => inst = this;

        void Update()
        {
            time += add * speed;

            for (int i = 0; i < localPositionSequence.Count; i++)
            {
                var sequence = localPositionSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    localPositionSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance.localPosition = sequence.sequence.Interpolate(time);
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }
            
            for (int i = 0; i < localScaleSequence.Count; i++)
            {
                var sequence = localScaleSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    localScaleSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance.localScale = sequence.sequence.Interpolate(time);
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }
            
            for (int i = 0; i < localRotationSequence.Count; i++)
            {
                var sequence = localRotationSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    localRotationSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance.localRotation = Quaternion.Euler(sequence.sequence.Interpolate(time));
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }

            for (int i = 0; i < anchoredPositionSequence.Count; i++)
            {
                var sequence = anchoredPositionSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    anchoredPositionSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance.anchoredPosition = sequence.sequence.Interpolate(time);
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }
            
            for (int i = 0; i < sizeDeltaSequence.Count; i++)
            {
                var sequence = sizeDeltaSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    sizeDeltaSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance.sizeDelta = sequence.sequence.Interpolate(time);
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }

            for (int i = 0; i < materialColorSequence.Count; i++)
            {
                var sequence = materialColorSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    materialColorSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance.color = sequence.sequence.Interpolate(time);
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }

            for (int i = 0; i < floatDelegateSequence.Count; i++)
            {
                var sequence = floatDelegateSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    floatDelegateSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance(sequence.sequence.Interpolate(time));
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }

            for (int i = 0; i < vector2DelegateSequence.Count; i++)
            {
                var sequence = vector2DelegateSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    vector2DelegateSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance(sequence.sequence.Interpolate(time));
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }

            for (int i = 0; i < vector3DelegateSequence.Count; i++)
            {
                var sequence = vector3DelegateSequence[i];
                if (sequence.length < time)
                {
                    if (sequence.onComplete != null)
                        sequence.onComplete();

                    vector3DelegateSequence.RemoveAt(i);

                    sequence = null;
                }
                else
                {
                    sequence.currentTime = time;
                    if (sequence.instance != null)
                        sequence.instance(sequence.sequence.Interpolate(time));
                    else if (sequence.action != null)
                        sequence.action(time);
                }
            }
        }

        public void ResetTime() => time = 0f;

        public void SetSpeeds(float add = 0.001f, float speed = 10f)
        {
            this.add = add;
            this.speed = speed;
        }

        public void AnimateLocalPosition(Transform tf, Sequence<Vector3> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            var t = length;
            if (t == 0f)
            {
                foreach (var kf in sequence.keyframes)
                    t += kf.Time;
            }

            var seq = new SequenceObject<Vector3, Transform>(sequence, tf, t, action, onComplete);
            localPositionSequence.Add(seq);
        }
        
        public void AnimateLocalScale(Transform tf, Sequence<Vector3> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            var t = length;
            if (t == 0f)
            {
                foreach (var kf in sequence.keyframes)
                    t += kf.Time;
            }

            var seq = new SequenceObject<Vector3, Transform>(sequence, tf, t, action, onComplete);
            localScaleSequence.Add(seq);
        }
        
        public void AnimateLocalRotation(Transform tf, Sequence<Vector3> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            var t = length;
            if (t == 0f)
            {
                foreach (var kf in sequence.keyframes)
                    t += kf.Time;
            }

            var seq = new SequenceObject<Vector3, Transform>(sequence, tf, t, action, onComplete);
            localRotationSequence.Add(seq);
        }

        public void AnimateColor(Material mat, Sequence<Color> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            var t = length;
            if (t == 0f)
            {
                foreach (var kf in sequence.keyframes)
                    t += kf.Time;
            }

            var seq = new SequenceObject<Color, Material>(sequence, mat, t, action, onComplete);
            materialColorSequence.Add(seq);
        }

        public void AnimateFloat(Action<float> f, Sequence<float> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            var t = length;
            if (t == 0f)
            {
                foreach (var kf in sequence.keyframes)
                    t += kf.Time;
            }

            var seq = new SequenceObject<float, Action<float>>(sequence, f, t, action, onComplete);
            floatDelegateSequence.Add(seq);
        }
        
        public void AnimateVector2(Action<Vector2> f, Sequence<Vector2> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            var t = length;
            if (t == 0f)
            {
                foreach (var kf in sequence.keyframes)
                    t += kf.Time;
            }

            var seq = new SequenceObject<Vector2, Action<Vector2>>(sequence, f, t, action, onComplete);
            vector2DelegateSequence.Add(seq);
        }
        
        public void AnimateVector3(Action<Vector3> f, Sequence<Vector3> sequence, float length = 0f, Action<float> action = null, Action onComplete = null)
        {
            var t = length;
            if (t == 0f)
            {
                foreach (var kf in sequence.keyframes)
                    t += kf.Time;
            }

            var seq = new SequenceObject<Vector3, Action<Vector3>>(sequence, f, t, action, onComplete);
            vector3DelegateSequence.Add(seq);
        }

        public List<SequenceObject<Vector3, Transform>> localPositionSequence = new List<SequenceObject<Vector3, Transform>>();
        public List<SequenceObject<Vector3, Transform>> localScaleSequence = new List<SequenceObject<Vector3, Transform>>();
        public List<SequenceObject<Vector3, Transform>> localRotationSequence = new List<SequenceObject<Vector3, Transform>>();

        public List<SequenceObject<Vector2, RectTransform>> anchoredPositionSequence = new List<SequenceObject<Vector2, RectTransform>>();
        public List<SequenceObject<Vector2, RectTransform>> sizeDeltaSequence = new List<SequenceObject<Vector2, RectTransform>>();

        public List<SequenceObject<Color, Material>> materialColorSequence = new List<SequenceObject<Color, Material>>();

        public List<SequenceObject<float, Action<float>>> floatDelegateSequence = new List<SequenceObject<float, Action<float>>>();
        public List<SequenceObject<Vector2, Action<Vector2>>> vector2DelegateSequence = new List<SequenceObject<Vector2, Action<Vector2>>>();
        public List<SequenceObject<Vector3, Action<Vector3>>> vector3DelegateSequence = new List<SequenceObject<Vector3, Action<Vector3>>>();

        public class SequenceObject<T, TInstance>
        {
            public SequenceObject(Sequence<T> sequence, TInstance instance, float length, Action<float> action, Action onComplete)
            {
                this.sequence = sequence;
                this.instance = instance;
                this.length = length;
                this.action = action;
                this.onComplete = onComplete;
            }

            public TInstance instance;

            public Sequence<T> sequence;

            public float currentTime;

            public float length;

            public Action<float> action;
            public Action onComplete;
        }
    }
}
