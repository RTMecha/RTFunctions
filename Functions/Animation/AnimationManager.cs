﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using LSFunctions;

using RTFunctions.Functions.Animation.Keyframe;

using UnityTime = UnityEngine.Time;

namespace RTFunctions.Functions.Animation
{
	/// <summary>
	/// All animation code aside from this is from https://github.com/Reimnop/Catalyst
	/// </summary>
	public class AnimationManager : MonoBehaviour
    {
		public static AnimationManager inst;

		public static void Init()
        {
			var gameObject = new GameObject("AnimationManager");
			gameObject.AddComponent<AnimationManager>();
			gameObject.transform.SetParent(SystemManager.inst.transform);
        }

        void Awake() => inst = this;

		void Update()
        {
			for (int i = 0; i < animations.Count; i++)
			{
				if (animations[i].playing)
					animations[i].Update();
			}
		}

		public void Play(Animation animation)
        {
			if (!animations.Has(x => x.id == animation.id))
				animations.Add(animation);
			animation.ResetTime();
			animation.Play();
        }

		public void RemoveName(string name)
        {
			animations.RemoveAll(x => x.name == name);
        }

		public void RemoveID(string id)
        {
			animations.RemoveAll(x => x.id == id);
        }

		public List<Animation> animations = new List<Animation>();

		// I plan on making an animation library at some point. This will allow creators to reuse animations or play them with a Modifier as an Animation Object.
		public static List<Animation> animationLibrary = new List<Animation>();

		public class Animation
		{
			public Animation(string name)
			{
				this.name = name;
				id = LSText.randomNumString(16);
				timeOffset = UnityTime.time;
			}

			public void ResetTime()
            {
                time = 0f;
				timeOffset = useRealTime ? UnityTime.time : AudioManager.inst.CurrentAudioSource.time;
                for (int i = 0; i < completed.Length; i++)
                    completed[i] = false;
            }

            public void Stop()
            {
                playing = false;
                for (int i = 0; i < completed.Length; i++)
                    completed[i] = true;
            }

			public void Play() => playing = true;

			public void Update()
            {
                Time = useRealTime ? UnityTime.time - timeOffset : AudioManager.inst.CurrentAudioSource.time - timeOffset;

				if (floatAnimations == null || floatAnimations.Count < 1)
					completed[0] = true;

				for (int i = 0; i < floatAnimations.Count; i++)
				{
					var anim = floatAnimations[i];
					if (anim.Length >= time)
					{
						anim.completed = false;
                        anim.action?.Invoke(anim.sequence.Interpolate(time));
                    }
					else if (!anim.completed)
					{
						anim.completed = true;
						anim.Completed();
					}
				}

				if (floatAnimations.All(x => x.completed) && !completed[0])
					completed[0] = true;

				if (vector2Animations == null || vector2Animations.Count < 1)
					completed[1] = true;

				for (int i = 0; i < vector2Animations.Count; i++)
				{
					var anim = vector2Animations[i];
					if (anim.Length >= time)
					{
						anim.completed = false;
                        anim.action?.Invoke(anim.sequence.Interpolate(time));
                    }
					else if (!anim.completed)
					{
						anim.completed = true;
						anim.Completed();
					}
				}

				if (vector2Animations.All(x => x.completed) && !completed[1])
					completed[1] = true;

				if (vector3Animations == null || vector3Animations.Count < 1)
					completed[2] = true;

				for (int i = 0; i < vector3Animations.Count; i++)
				{
					var anim = vector3Animations[i];
					if (anim.Length >= time)
					{
						anim.completed = false;
                        anim.action?.Invoke(anim.sequence.Interpolate(time));
                    }
					else if (!anim.completed)
					{
						anim.completed = true;
						anim.Completed();
					}
				}

				if (vector3Animations.All(x => x.completed) && !completed[2])
					completed[2] = true;

				if (colorAnimations == null || colorAnimations.Count < 1)
					completed[3] = true;

				for (int i = 0; i < colorAnimations.Count; i++)
				{
					var anim = colorAnimations[i];
					if (anim.Length >= time)
					{
						anim.completed = false;
                        anim.action?.Invoke(anim.sequence.Interpolate(time));
                    }
					else if (!anim.completed)
					{
						anim.completed = true;
						anim.Completed();
					}
				}

				if (colorAnimations.All(x => x.completed) && !completed[3])
					completed[3] = true;

				if (completed.All(x => x == true) && playing)
				{
					playing = false;
					onComplete?.Invoke();

					if (loop)
                    {
						ResetTime();
						Play();
					}
				}
			}

			public string id;
			public string name;

			public bool useRealTime = true;

			public bool loop;

			float time;
            public float Time
            {
                get => time;
                private set => time = value;
            }

            float timeOffset;

            public Action onComplete;

            public bool playing = false;

            public bool[] completed = new bool[4]
            {
                false,
                false,
                false,
                false
            };

            public List<AnimationObject<float>> floatAnimations = new List<AnimationObject<float>>();
            public List<AnimationObject<Vector2>> vector2Animations = new List<AnimationObject<Vector2>>();
            public List<AnimationObject<Vector3>> vector3Animations = new List<AnimationObject<Vector3>>();
            public List<AnimationObject<Color>> colorAnimations = new List<AnimationObject<Color>>();

            public class AnimationObject<T>
			{
				public AnimationObject(List<IKeyframe<T>> keyframes, Action<T> action, Action onComplete = null)
				{
					this.keyframes = keyframes;
					sequence = new Sequence<T>(this.keyframes);
					this.action = action;
					this.onComplete = onComplete;
				}

				public float currentTime;

				public List<IKeyframe<T>> keyframes;

				public Sequence<T> sequence;

				public Action<T> action;

				public Action onComplete;

				public bool completed = false;

				public void Completed()
				{
					if (completed)
						return;

					completed = true;
                    onComplete?.Invoke();
				}

				public float Length
				{
					get
					{
						float t = 0f;

						if (keyframes.Count > 0)
						{
							var x = keyframes.OrderBy(x => x.Time).ToList();
							t = x[x.Count - 1].Time;
						}

						return t;
					}
				}
			}
        }
    }
}
