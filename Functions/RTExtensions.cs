using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using LSFunctions;
using SimpleJSON;

using RTFunctions.Functions.Animation;
using RTFunctions.Functions.Animation.Keyframe;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Optimization;

using BeatmapObject = DataManager.GameData.BeatmapObject;
using EventKeyframe = DataManager.GameData.EventKeyframe;
using Prefab = DataManager.GameData.Prefab;
using PrefabObject = DataManager.GameData.PrefabObject;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;

using Object = UnityEngine.Object;

namespace RTFunctions.Functions
{
	public static class RTExtensions
	{
		#region Scene

		/// <summary>
		/// Tries to get the game object associated with the beatmap object.
		/// </summary>
		/// <param name="_beatmapObject"></param>
		/// <param name="result"></param>
		/// <returns>True if GameObject is not null, otherwise returns false.</returns>
		public static bool TryGetGameObject(this BeatmapObject _beatmapObject, out GameObject result)
		{
			var b = _beatmapObject.GetGameObject();
			if (b != null)
			{
				result = b;
				return true;
			}
			result = null;
			return false;
		}

		/// <summary>
		/// Gets the game object associated with the beatmap object.
		/// </summary>
		/// <param name="_beatmapObject"></param>
		/// <returns>GameObject from beatmapGameObjects if Catalyst is not installed, otherwise returns VisualObject from ILevelObject within Catalyst.</returns>
		public static GameObject GetGameObject(this BeatmapObject _beatmapObject) => Updater.GetGameObject(_beatmapObject);

		public static bool TryGetTransformChain(this BeatmapObject _beatmapObject, out List<Transform> result)
		{
			var tf = _beatmapObject.GetTransformChain();
			if (tf != null && tf.Count > 0 && !tf.Any(x => x == null))
			{
				result = tf;
				return true;
			}
			result = null;
			return false;
		}

		/// <summary>
		/// Gets the transform parent chain associated with the beatmap object.
		/// </summary>
		/// <param name="_beatmapObject"></param>
		/// <returns>List of transforms ordered by the base parent to the actual game object.</returns>
		public static List<Transform> GetTransformChain(this BeatmapObject _beatmapObject)
		{
			var list = new List<Transform>();
			var tf1 = _beatmapObject.GetGameObject().transform;

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

			return list;
		}

		public static Color GetObjectColor(this BeatmapObject _beatmapObject, bool _ignoreTransparency)
		{
			if (_beatmapObject.objectType == ObjectType.Empty)
			{
				return Color.white;
			}

			if (_beatmapObject.TryGetGameObject(out GameObject gameObject) && gameObject.TryGetComponent(out Renderer renderer))
			{
				Color color = Color.white;
				if (AudioManager.inst.CurrentAudioSource.time < _beatmapObject.StartTime)
				{
					color = GameManager.inst.LiveTheme.objectColors[(int)_beatmapObject.events[3][0].eventValues[0]];
				}
				else if (AudioManager.inst.CurrentAudioSource.time > _beatmapObject.StartTime + _beatmapObject.GetObjectLifeLength() && _beatmapObject.autoKillType != AutoKillType.OldStyleNoAutokill)
				{
					color = GameManager.inst.LiveTheme.objectColors[(int)_beatmapObject.events[3][_beatmapObject.events[3].Count - 1].eventValues[0]];
				}
				else
				{
					color = renderer.material.color;
				}
				if (_ignoreTransparency)
				{
					color.a = 1f;
				}
				return color;
			}

			return Color.white;
		}

		public static bool TryFind(string find, out GameObject result)
		{
			var e = GameObject.Find(find);
			if (e != null)
			{
				result = e;
				return true;
			}
			result = null;
			return false;
		}

		public static bool TryFind(this Transform tf, string find, out Transform result)
		{
			var e = tf.Find(find);
			if (e != null)
			{
				result = e;
				return true;
			}
			result = null;
			return false;
		}

		public static List<Transform> ChildList(this Transform transform)
        {
			var list = new List<Transform>();
			foreach (var obj in transform)
				list.Add((Transform)obj);
			return list;
		}

		public static void DeleteChildren(this Transform tf, bool instant = false) => LSHelpers.DeleteChildren(tf, instant);

		public static GameObject Duplicate(this GameObject gameObject, Transform parent)
		{
			var copy = Object.Instantiate(gameObject);
			copy.transform.SetParent(parent);
			copy.transform.localPosition = gameObject.transform.localPosition;
			copy.transform.localScale = gameObject.transform.localScale;

			return copy;
		}

		public static GameObject Duplicate(this GameObject gameObject, Transform parent, string name)
        {
			var copy = gameObject.Duplicate(parent);
			copy.name = name;
			return copy;
        }
		
		public static GameObject Duplicate(this GameObject gameObject, Transform parent, string name, int index)
        {
			var copy = gameObject.Duplicate(parent);
			copy.name = name;
			copy.transform.SetSiblingIndex(index);
			return copy;
        }

		public static void GetComponentAndPerformAction<T>(this GameObject gameObject, Action<T> action)
        {
			if (gameObject.TryGetComponent(out T result))
				action(result);
        }
		
		public static void GetComponentAndPerformAction<T>(this Transform transform, Action<T> action)
        {
			if (transform.gameObject.TryGetComponent(out T result))
				action(result);
        }

		public static void GetComponentsAndPerformActions(this GameObject gameObject, Type[] types, Action<Component>[] actions)
        {
			for (int i = 0; i < types.Length; i++)
            {
				var comp = gameObject.GetComponent(types[i]);
				if (comp)
					actions[i]?.Invoke(comp);
            }
        }

		public static RectTransform GetChildRT(this Transform transform, int index)
        {
			var child = transform.GetChild(index);
			if (child is RectTransform)
				return (RectTransform)child;
			return null;
        }

		public static RectTransform FindRT(this Transform transform, string n)
        {
			var find = transform.Find(n);
			if (find is RectTransform)
				return (RectTransform)find;
			return null;
        }

		public static RectTransform AsRT(this Transform transform) => (RectTransform)transform;

		#endregion

		#region Data

		/// <summary>
		/// Gets the entire parent chain, including the beatmap object itself.
		/// </summary>
		/// <param name="_beatmapObject"></param>
		/// <returns>List of parents ordered by the current beatmap object to the base parent with no other parents.</returns>
		public static List<BeatmapObject> GetParentChain(this BeatmapObject _beatmapObject)
		{
			List<BeatmapObject> beatmapObjects = new List<BeatmapObject>();

			if (_beatmapObject != null)
			{
				var orig = _beatmapObject;
				beatmapObjects.Add(orig);

				while (!string.IsNullOrEmpty(orig.parent))
				{
					if (orig == null || DataManager.inst.gameData.beatmapObjects.Find(x => x.id == orig.parent) == null)
						break;
					var select = DataManager.inst.gameData.beatmapObjects.Find(x => x.id == orig.parent);
					beatmapObjects.Add(select);
					orig = select;
				}
			}

			return beatmapObjects;
		}

		public static List<BeatmapObject> GetParentChainSimple(this BeatmapObject beatmapObject)
		{
			List<BeatmapObject> beatmapObjects = new List<BeatmapObject>();

			var orig = beatmapObject;
			beatmapObjects.Add(orig);

			while (!string.IsNullOrEmpty(orig.parent))
			{
				orig = DataManager.inst.gameData.beatmapObjects.Find(x => x.id == orig.parent);
				beatmapObjects.Add(orig);
			}

			return beatmapObjects;
		}

		/// <summary>
		/// Gets the every child connected to the beatmap object.
		/// </summary>
		/// <param name="_beatmapObject"></param>
		/// <returns>A full list tree with every child object.</returns>
		public static List<List<BeatmapObject>> GetChildChain(this BeatmapObject _beatmapObject)
		{
			var lists = new List<List<BeatmapObject>>();
			foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
			{
				if (beatmapObject.GetParentChain() != null && beatmapObject.GetParentChain().Count > 0)
				{
					var parentChain = beatmapObject.GetParentChain();
					foreach (var parent in parentChain)
					{
						if (parent.id == _beatmapObject.id)
						{
							lists.Add(parentChain);
						}
					}
				}
			}
			return lists;
		}

		/// <summary>
		/// Checks whether the current time is within the objects' lifespan / if the object is alive.
		/// </summary>
		/// <returns>If alive returns true, otherwise returns false.</returns>
		public static bool TimeWithinLifespan(this BeatmapObject _beatmapObject)
		{
			var time = AudioManager.inst.CurrentAudioSource.time;
			var st = _beatmapObject.StartTime;
			var akt = _beatmapObject.autoKillType;
			var ako = _beatmapObject.autoKillOffset;
			var l = _beatmapObject.GetObjectLifeLength(_oldStyle: true);
			if (time >= st && (time <= l + st && akt != AutoKillType.OldStyleNoAutokill && akt != AutoKillType.SongTime || akt == AutoKillType.OldStyleNoAutokill || time < ako && _beatmapObject.autoKillType == AutoKillType.SongTime))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets every child connected to the beatmap objects' base parent.
		/// </summary>
		/// <param name="_beatmapObject"></param>
		/// <returns>A full list tree with every child object rooted from the base parent.</returns>
		public static List<List<BeatmapObject>> GetStartTree(this BeatmapObject _beatmapObject)
		{
			var parentChain = _beatmapObject.GetParentChain();
			var parentTop = parentChain[parentChain.Count - 1];

			return parentTop.GetChildChain();
		}

		/// <summary>
		/// Gets beatmap object by id from any beatmap object list.
		/// </summary>
		/// <param name="_bms"></param>
		/// <param name="_id"></param>
		/// <returns>Beatmap object from list.</returns>
		public static BeatmapObject ID(this List<BeatmapObject> _bms, string _id) => _bms.Find(x => x.id == _id);

		/// <summary>
		/// Gets all beatmap objects that match the provided name.
		/// </summary>
		/// <param name="_bms"></param>
		/// <param name="_name"></param>
		/// <returns>A list of beatmap objects with a specific name.</returns>
		public static List<BeatmapObject> AllName(this List<BeatmapObject> _bms, string _name) => _bms.FindAll(x => x.name == _name);

		/// <summary>
		/// Gets all beatmap objects that contain the provided name.
		/// </summary>
		/// <param name="_bms"></param>
		/// <param name="_name"></param>
		/// <returns>A list of beatmap objects with a specified name contained in its own.</returns>
		public static List<BeatmapObject> AllNameContains(this List<BeatmapObject> _bms, string _name) => _bms.FindAll(x => x.name.Contains(_name));

		/// <summary>
		/// Tries to get the parent of the beatmap object.
		/// </summary>
		/// <param name="beatmapObject"></param>
		/// <param name="result"></param>
		/// <returns>True if parent is not null, otherwise false.</returns>
		public static bool TryGetParent(this BeatmapObject beatmapObject, out BeatmapObject result)
		{
			var p = beatmapObject.GetParent();
			if (p != null)
			{
				result = p;
				return true;
			}
			result = null;
			return false;
		}

		/// <summary>
		/// Gets the parent of the beatmap object.
		/// </summary>
		/// <param name="_beatmapObject"></param>
		/// <returns>Parent of the beatmap object.</returns>
		public static BeatmapObject GetParent(this BeatmapObject _beatmapObject) => DataManager.inst.gameData.beatmapObjects.Find(x => x.id == _beatmapObject.parent);

		public static bool TrySetParent(this BeatmapObject _beatmapObject, string id)
		{
			if (DataManager.inst.gameData.beatmapObjects.Find(x => x.id == id) != null)
			{
				_beatmapObject.parent = id;
				_beatmapObject.updateObject();
				return true;
			}
			return false;
		}

		public static ObjEditor.ObjectSelection ToObjectSelection(this BeatmapObject beatmapObject)
		{
			var objectSelection = new ObjEditor.ObjectSelection(ObjEditor.ObjectSelection.SelectionType.Object, beatmapObject.id);
			objectSelection.Index = DataManager.inst.gameData.beatmapObjects.IndexOf(beatmapObject);
			return objectSelection;
		}

		public static void updateObject(this BeatmapObject beatmapObject, bool reinsert = true)
		{
			var objectSelection = new ObjEditor.ObjectSelection(ObjEditor.ObjectSelection.SelectionType.Object, beatmapObject.id);
			objectSelection.Index = DataManager.inst.gameData.beatmapObjects.IndexOf(beatmapObject);

			Updater.updateProcessor(objectSelection, reinsert);
		}

		public static DataManager.BeatmapTheme CreateTheme(this DataManager dataManager, string _name, string _id, Color _bg, Color _gui, List<Color> _players, List<Color> _objects, List<Color> _bgs)
		{
			DataManager.BeatmapTheme beatmapTheme = new DataManager.BeatmapTheme();

			beatmapTheme.name = _name;
			beatmapTheme.id = _id;
			beatmapTheme.backgroundColor = _bg;
			beatmapTheme.guiColor = _gui;
			beatmapTheme.playerColors = _players;
			beatmapTheme.objectColors = _objects;
			beatmapTheme.backgroundColors = _bgs;

			return beatmapTheme;
		}

		public static EventKeyframe NextEventKeyframe(this BeatmapObject beatmapObject, int type) => beatmapObject.events[type].Find(x => x.eventTime > AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime);

		public static EventKeyframe PrevEventKeyframe(this BeatmapObject beatmapObject, int type)
		{
			var index = beatmapObject.events[type].FindIndex(x => x.eventTime > AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime) - 1;
			if (index < 0)
			{
				index = 0;
			}

			return beatmapObject.events[type][index];
		}

		public static GameObject GetShape(this List<ObjectManager.ObjectPrefabHolder> prefabs, int s, int so, bool includeShape = true)
		{
			int _s = Mathf.Clamp(s, 0, prefabs.Count - 1);
			int _so = Mathf.Clamp(so, 0, prefabs[_s].options.Count - 1);

			if (!includeShape && (_s == 4 || _s == 6))
			{
				_s = 0;
				_so = 0;
			}

			return prefabs[_s].options[_so];
		}

		public static Prefab GetPrefab(this PrefabObject prefabObject) => DataManager.inst.gameData.prefabs.Find(x => x.ID == prefabObject.prefabID);

		public static List<T> Clone<T>(this List<T> list)
        {
			var array = new T[list.Count];
			list.CopyTo(array);
			return array.ToList();
        }

		public static T[] Copy<T>(this T[] ts)
        {
			var array = new T[ts.Length];
			for (int i = 0; i < ts.Length; i++)
				array[i] = ts[i];
			return array;
        }

		public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> keyValuePairs)
        {
			var dictionary = new Dictionary<TKey, TValue>();
			foreach (var d in keyValuePairs)
				dictionary.Add(d.Key, d.Value);
			return dictionary;
        }

        public static float Interpolate(this BeatmapObject beatmapObject, int type, int value)
        {
            var time = AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime;

            var nextKFIndex = beatmapObject.events[type].FindIndex(x => x.eventTime > time);

            if (nextKFIndex >= 0)
            {
                var prevKFIndex = nextKFIndex - 1;
                if (prevKFIndex < 0)
                    prevKFIndex = 0;

                var nextKF = beatmapObject.events[type][nextKFIndex];
                var prevKF = beatmapObject.events[type][prevKFIndex];

                var next = nextKF.eventValues[value];
                var prev = prevKF.eventValues[value];

                if (float.IsNaN(prev))
                    prev = 0f;

                if (float.IsNaN(next))
                    next = 0f;

                var x = RTMath.Lerp(prev, next, Ease.GetEaseFunction(nextKF.curveType.Name)(RTMath.InverseLerp(prevKF.eventTime, nextKF.eventTime, time)));

                if (prevKFIndex == nextKFIndex)
                    x = next;

                if (float.IsNaN(x) || float.IsInfinity(x))
                    x = next;

                return x;
            }
            else
            {
                var x = beatmapObject.events[type][beatmapObject.events[type].Count - 1].eventValues[value];

                if (float.IsNaN(x))
                    x = 0f;

                if (float.IsNaN(x) || float.IsInfinity(x))
                    x = beatmapObject.events[type][beatmapObject.events[type].Count - 1].eventValues[value];

                return x;
            }
        }

        #endregion

        #region Catalyst

        [Obsolete("Editor Catalyst is fully implemented with RTFunctions, no need to use this.")]
		public static object GetILevelObject(this BeatmapObject _beatmapObject) => null;

		#endregion

		#region Event Keyframes

		public static EventKeyframe ClosestEventKeyframe(int _type, object n = null) => DataManager.inst.gameData.eventObjects.allEvents[_type][ClosestEventKeyframe(_type)];

		/// <summary>
		/// Gets closest event keyframe to current time.
		/// </summary>
		/// <param name="_type">Event Keyframe Type</param>
		/// <returns>Event Keyframe Index</returns>
		public static int ClosestEventKeyframe(int _type)
		{
			var allEvents = DataManager.inst.gameData.eventObjects.allEvents;
			float time = AudioManager.inst.CurrentAudioSource.time;
			if (allEvents[_type].Find(x => x.eventTime > time) != null)
			{
				var nextKFE = allEvents[_type].Find(x => x.eventTime > time);
				var nextKF = allEvents[_type].IndexOf(nextKFE);
				var prevKF = nextKF - 1;

				if (nextKF == 0)
				{
					prevKF = 0;
				}
				else
				{
					var v1 = new Vector2(allEvents[_type][prevKF].eventTime, 0f);
					var v2 = new Vector2(allEvents[_type][nextKF].eventTime, 0f);

					float dis = Vector2.Distance(v1, v2) / 2f;

					bool prevClose = time > dis + allEvents[_type][prevKF].eventTime;
					bool nextClose = time < allEvents[_type][nextKF].eventTime - dis;

					if (!prevClose)
					{
						return prevKF;
					}
					if (!nextClose)
					{
						return nextKF;
					}
				}
			}
			return 0;
		}

		public static EventKeyframe ClosestKeyframe(this BeatmapObject beatmapObject, int _type, object n = null) => beatmapObject.events[_type][beatmapObject.ClosestKeyframe(_type)];

		/// <summary>
		/// Gets closest event keyframe to current time within a beatmap object.
		/// </summary>
		/// <param name="beatmapObject"></param>
		/// <param name="_type">Event Keyframe Type</param>
		/// <returns>Event Keyframe Index</returns>
		public static int ClosestKeyframe(this BeatmapObject beatmapObject, int _type)
		{
			if (beatmapObject.events[_type].Find(x => x.eventTime > AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime) != null)
			{
				var nextKFE = beatmapObject.events[_type].Find(x => x.eventTime > AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime);
				var nextKF = beatmapObject.events[_type].IndexOf(nextKFE);
				var prevKF = nextKF - 1;

				if (prevKF < 0)
					prevKF = 0;

				var prevKFE = beatmapObject.events[_type][prevKF];

				if (nextKF == 0)
				{
					prevKF = 0;
				}
				else
				{
					var v1 = new Vector2(beatmapObject.events[_type][prevKF].eventTime, 0f);
					var v2 = new Vector2(beatmapObject.events[_type][nextKF].eventTime, 0f);

					float dis = Vector2.Distance(v1, v2);
					float time = AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime;

					bool prevClose = time > dis + beatmapObject.events[_type][prevKF].eventTime / 2f;
					bool nextClose = time < beatmapObject.events[_type][nextKF].eventTime - dis / 2f;

					if (!prevClose)
					{
						return prevKF;
					}
					if (!nextClose)
					{
						return nextKF;
					}
				}
				{
					var dis = RTMath.Distance(nextKFE.eventTime, prevKFE.eventTime);
					var time = AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime;

					var prevClose = time > dis + prevKFE.eventTime / 2f;
					var nextClose = time < nextKFE.eventTime - dis / 2f;

					
                }
			}
			return 0;
		}

		public static bool TryGetValue(this EventKeyframe eventKeyframe, int index, out float result)
        {
			if (eventKeyframe.eventValues.Length > index)
            {
				result = eventKeyframe.eventValues[index];
				return true;
            }
			result = 0f;
			return false;
        }

        #endregion

        #region Data Extensions

		public static byte[] ToBytes(this string str) => Encoding.ASCII.GetBytes(str);
		public static string ToString(this byte[] bytes) => Encoding.ASCII.GetString(bytes);

		public static bool Has<T>(this List<T> ts, Predicate<T> predicate) => ts.Find(predicate) != null;

		public static Dictionary<TKey, TValue> ToDictionary<T, TKey, TValue>(this List<T> ts, Func<T, TKey> key, Func<T, TValue> value)
        {
			var dictionary = new Dictionary<TKey, TValue>();

			var keys = ts.Select(key).ToList();
			var values = ts.Select(value).ToList();

			for (int i = 0; i < keys.Count; i++)
				if (!dictionary.ContainsKey(keys[i]))
					dictionary.Add(keys[i], values[i]);

			return dictionary;
        }

		static void Test()
        {
			DataManager.inst.gameData.beatmapObjects.ToDictionary(x => x.id, x => x);
        }

		public static List<string> GetLines(this string str) => str.Split(new string[] { "\n", "\n\r", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

        #endregion

        #region JSON

		public static JSONNode ToJSON(this Vector2 vector2)
        {
			var jn = JSON.Parse("{}");

			jn["x"] = vector2.x.ToString();
			jn["y"] = vector2.y.ToString();

			return jn;
        }
		
		public static JSONNode ToJSON(this Vector3 vector3)
        {
			var jn = JSON.Parse("{}");

			jn["x"] = vector3.x.ToString();
			jn["y"] = vector3.y.ToString();
			jn["z"] = vector3.z.ToString();

			return jn;
        }
		
		public static JSONNode ToJSON(this Vector4 vector4)
        {
			var jn = JSON.Parse("{}");

			jn["x"] = vector4.x.ToString();
			jn["y"] = vector4.y.ToString();
			jn["z"] = vector4.z.ToString();
			jn["w"] = vector4.w.ToString();

			return jn;
        }

		public static Vector2 AsVector2(this JSONNode jn) => new Vector2(jn["x"].AsFloat, jn["y"].AsFloat);

		public static Vector3 AsVector3(this JSONNode jn) => new Vector3(jn["x"].AsFloat, jn["y"].AsFloat, jn["z"].AsFloat);

		public static Vector3 AsVector4(this JSONNode jn) => new Vector4(jn["x"].AsFloat, jn["y"].AsFloat, jn["z"].AsFloat, jn["w"].AsFloat);

		#endregion

		#region Misc

		public static ColorBlock SetColorBlock(this ColorBlock cb, Color normal, Color highlighted, Color pressed, Color selected, Color disabled, float fade = 0.2f)
		{
			cb.normalColor = normal;
			cb.highlightedColor = highlighted;
			cb.pressedColor = pressed;
			cb.selectedColor = selected;
			cb.disabledColor = disabled;
			cb.fadeDuration = fade;
			return cb;
		}

		public static void Save(this Sprite sprite, string path) => SpriteManager.SaveSprite(sprite, path);

        public static EventKeyframe GetEventKeyframe(this List<List<EventKeyframe>> eventKeyframes, int type, int index) => eventKeyframes[RTMath.Clamp(type, 0, eventKeyframes.Count - 1)].GetEventKeyframe(index);
		public static EventKeyframe GetEventKeyframe(this List<EventKeyframe> eventKeyframes, int index) => eventKeyframes[RTMath.Clamp(index, 0, eventKeyframes.Count - 1)];

		public static void SetColor(this Material material, Color color) => material.color = color;

		public static string ColorToHex(Color32 color) => color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");

		public static bool TryGetComponent<T>(this GameObject gameObject, out T result)
        {
			var t = gameObject.GetComponent<T>();
			if (t != null)
            {
				result = t;
				return true;
            }
			result = default(T);
			return false;
        }

		public static void ClearAll(this Button.ButtonClickedEvent b)
        {
			b.m_Calls.m_ExecutingCalls.Clear();
			b.m_Calls.m_PersistentCalls.Clear();
			b.m_PersistentCalls.m_Calls.Clear();
			b.RemoveAllListeners();
		}

		public static void ClearAll(this InputField.OnChangeEvent i)
        {
			i.m_Calls.m_ExecutingCalls.Clear();
			i.m_Calls.m_PersistentCalls.Clear();
			i.m_PersistentCalls.m_Calls.Clear();
			i.RemoveAllListeners();
		}

		public static void ClearAll(this InputField.SubmitEvent s)
        {
			s.m_Calls.m_ExecutingCalls.Clear();
			s.m_Calls.m_PersistentCalls.Clear();
			s.m_PersistentCalls.m_Calls.Clear();
			s.RemoveAllListeners();
        }
		
		public static void ClearAll(this Toggle.ToggleEvent i)
        {
			i.m_Calls.m_ExecutingCalls.Clear();
			i.m_Calls.m_PersistentCalls.Clear();
			i.m_PersistentCalls.m_Calls.Clear();
			i.RemoveAllListeners();
		}

		public static void ClearAll(this Dropdown.DropdownEvent d)
        {
			d.m_Calls.m_ExecutingCalls.Clear();
			d.m_Calls.m_PersistentCalls.Clear();
			d.m_PersistentCalls.m_Calls.Clear();
			d.RemoveAllListeners();
        }
		
		public static void ClearAll(this Slider.SliderEvent s)
        {
			s.m_Calls.m_ExecutingCalls.Clear();
			s.m_Calls.m_PersistentCalls.Clear();
			s.m_PersistentCalls.m_Calls.Clear();
			s.RemoveAllListeners();
        }

		public static void NewOnClickListener(this Button b, UnityAction unityAction)
        {
			b.onClick.ClearAll();
			b.onClick.AddListener(unityAction);
        }

		public static void NewValueChangedListener(this InputField i, string value, UnityAction<string> unityAction)
        {
			i.onValueChanged.ClearAll();
			i.text = value;
			i.onValueChanged.AddListener(unityAction);
        }
		
		public static void NewValueChangedListener(this Toggle i, bool value, UnityAction<bool> unityAction)
        {
			i.onValueChanged.ClearAll();
			i.isOn = value;
			i.onValueChanged.AddListener(unityAction);
        }
		
		public static void NewValueChangedListener(this Dropdown d, int value, UnityAction<int> unityAction)
        {
			d.onValueChanged.ClearAll();
			d.value = value;
			d.onValueChanged.AddListener(unityAction);
        }

		public static void NewValueChangedListener(this Slider slider, float value, UnityAction<float> unityAction)
        {
			slider.onValueChanged.ClearAll();
			slider.value = value;
			slider.onValueChanged.AddListener(unityAction);
        }

		public static Component ReplaceComponent(this Component component, Component newComponent)
        {
			var gameObject = component.gameObject;

			Object.DestroyImmediate(component);

			if (gameObject != null)
				return gameObject.AddComponent(newComponent.GetType());
			return null;
        }

		public static float[] ConvertByteToFloat(byte[] array)
		{
			float[] floatArr = new float[array.Length / 4];
			for (int i = 0; i < floatArr.Length; i++)
			{
				if (BitConverter.IsLittleEndian)
					Array.Reverse(array, i * 4, 4);
				floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
			}
			return floatArr;
		}

		public static byte[] ConvertFloatToByte(float[] array)
		{
			byte[] byteArr = new byte[array.Length * 4];
			for (int i = 0; i < array.Length; i++)
			{
				var bytes = BitConverter.GetBytes(array[i] * 0x80000000);
				Array.Copy(bytes, 0, byteArr, i * 4, bytes.Length);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(byteArr, i * 4, 4);
			}
			return byteArr;
		}

		public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> keyValuePair)
		{
			dictionary.Add(keyValuePair.Key, keyValuePair.Value);
		}

		public static KeyValuePair<TKey, TValue> NewKeyValuePair<TKey, TValue>(TKey key, TValue value)
		{
			return new KeyValuePair<TKey, TValue>(key, value);
		}

		public static bool TryFind<T>(this List<T> ts, Predicate<T> match, out T item)
        {
			var t = ts.Find(match);
			if (t != null)
            {
				item = t;
				return true;
            }
			item = default(T);
			return true;
        }

		public static Type[] ToTypes<T>(this T[] ts)
        {
			var t = new Type[ts.Length];
			for (int i = 0; i < t.Length; i++)
				t[i] = ts[i].GetType();
			return t;
        }

		#endregion
	}
}
