using InControl;
using LSFunctions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTFunctions.Functions.IO
{
    public static class RTHelpers
	{
		public static string levelVersion = FunctionsPlugin.CurrentVersion.ToString();

        public static float screenScale;
		public static float screenScaleInverse;

		public static Data.BeatmapTheme BeatmapTheme
        {
			get
            {
				var beatmapTheme = GameManager.inst?.LiveTheme;
				if (EditorManager.inst && EventEditor.inst.showTheme)
					beatmapTheme = EventEditor.inst.previewTheme;
				return (Data.BeatmapTheme)beatmapTheme;
			}
        }

		public static bool InEditor => EditorManager.inst;
		public static bool InGame => GameManager.inst;
		public static bool InMenu => ArcadeManager.inst.ic;

		public static float Pitch
		{
			get
			{
				float pitch = AudioManager.inst.CurrentAudioSource.pitch;
				if (pitch < 0f)
				{
					pitch = -pitch;
				}

				if (pitch == 0f)
					pitch = 0.0001f;

				return pitch;
			}
		}

		public static DataManager.Difficulty GetDifficulty(int difficulty)
			=> difficulty >= 0 && difficulty < DataManager.inst.difficulties.Count ?
			DataManager.inst.difficulties[difficulty] : new DataManager.Difficulty("Unknown Difficulty", LSColors.HexToColor("424242"));

		public static bool Paused => GameManager.inst && GameManager.inst.gameState == GameManager.State.Paused;
		public static bool Playing => GameManager.inst && GameManager.inst.gameState == GameManager.State.Playing;

		public static bool AprilFools => System.DateTime.Now.ToString("M") == "1 April";

		public static float getPitch()
        {
			if (EditorManager.inst != null)
				return 1f;

            return new List<float>
            {
                0.1f,
                0.5f,
                0.8f,
                1f,
                1.2f,
                1.5f,
                2f,
                3f,
            }[Mathf.Clamp(DataManager.inst.GetSettingEnum("ArcadeGameSpeed", 2), 0, 7)];
		}

		#region EventTriggers

		public static void AddEventTriggerParams(GameObject gameObject, params EventTrigger.Entry[] entries) => AddEventTrigger(gameObject, entries.ToList());

		public static void AddEventTrigger(GameObject _if, List<EventTrigger.Entry> entries, bool clear = true)
		{
			if (!_if.GetComponent<EventTrigger>())
			{
				_if.AddComponent<EventTrigger>();
			}
			var et = _if.GetComponent<EventTrigger>();
			if (clear)
				et.triggers.Clear();
			foreach (var entry in entries)
				et.triggers.Add(entry);
		}

		public static EventTrigger.Entry ScrollDelta(InputField inputField, float amount = 0.1f, float mutliply = 10f, float min = 0f, float max = 0f, bool multi = false)
		{
			var entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Scroll;
			entry.callback.AddListener(delegate (BaseEventData eventData)
			{
				var pointerEventData = (PointerEventData)eventData;

				if (float.TryParse(inputField.text, out float result))
				{
					if (!multi || !Input.GetKey(KeyCode.LeftShift))
					{
						var largeKey = KeyCode.LeftControl;
						var smallKey = KeyCode.LeftAlt;
						var regularKey = KeyCode.None;

						// Large Amount
						bool large = largeKey == KeyCode.None && !Input.GetKey(smallKey) && !Input.GetKey(regularKey) || Input.GetKey(largeKey);

						// Small Amount
						bool small = smallKey == KeyCode.None && !Input.GetKey(largeKey) && !Input.GetKey(regularKey) || Input.GetKey(smallKey);

						// Regular Amount
						bool regular = regularKey == KeyCode.None && !Input.GetKey(smallKey) && !Input.GetKey(largeKey) || Input.GetKey(regularKey);

						if (pointerEventData.scrollDelta.y < 0f)
							result -= small ? amount / mutliply : large ? amount * mutliply : regular ? amount : 0f;
						if (pointerEventData.scrollDelta.y > 0f)
							result += small ? amount / mutliply : large ? amount * mutliply : regular ? amount : 0f;

						if (min != 0f || max != 0f)
							result = Mathf.Clamp(result, min, max);

						inputField.text = result.ToString("f2");
					}
				}
			});
			return entry;
		}

		public static EventTrigger.Entry ScrollDeltaInt(InputField inputField, int amount = 1, int min = 0, int max = 0, bool multi = false)
		{
			var entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Scroll;
			entry.callback.AddListener(delegate (BaseEventData eventData)
			{
				var pointerEventData = (PointerEventData)eventData;

				if (int.TryParse(inputField.text, out int result))
				{
					if (!multi || !Input.GetKey(KeyCode.LeftShift))
					{
						var largeKey = KeyCode.LeftControl;
						var regularKey = KeyCode.None;

						// Large Amount
						bool large = largeKey == KeyCode.None && !Input.GetKey(regularKey) || Input.GetKey(largeKey);

						// Regular Amount
						bool regular = regularKey == KeyCode.None && !Input.GetKey(largeKey) || Input.GetKey(regularKey);

						if (pointerEventData.scrollDelta.y < 0f)
							result -= amount * (large ? 10 : regular ? 1 : 0);
						if (pointerEventData.scrollDelta.y > 0f)
							result += amount * (large ? 10 : regular ? 1 : 0);

						if (min != 0f || max != 0f)
							result = Mathf.Clamp(result, min, max);

						inputField.text = result.ToString();
					}
				}
			});
			return entry;
		}

		public static EventTrigger.Entry ScrollDeltaVector2(InputField ifx, InputField ify, float amount, float mutliply, List<float> clamp = null)
		{
			var entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Scroll;
			entry.callback.AddListener(delegate (BaseEventData eventData)
			{
				var pointerEventData = (PointerEventData)eventData;
				if (Input.GetKey(KeyCode.LeftShift) && float.TryParse(ifx.text, out float x) && float.TryParse(ify.text, out float y))
				{
					var largeKey = KeyCode.LeftControl;
					var smallKey = KeyCode.LeftAlt;
					var regularKey = KeyCode.None;

					// Large Amount
					bool large = largeKey == KeyCode.None && !Input.GetKey(smallKey) && !Input.GetKey(regularKey) || Input.GetKey(largeKey);

					// Small Amount
					bool small = smallKey == KeyCode.None && !Input.GetKey(largeKey) && !Input.GetKey(regularKey) || Input.GetKey(smallKey);

					// Regular Amount
					bool regular = regularKey == KeyCode.None && !Input.GetKey(smallKey) && !Input.GetKey(largeKey) || Input.GetKey(regularKey);

					if (pointerEventData.scrollDelta.y < 0f)
					{
						x -= small ? amount / mutliply : large ? amount * mutliply : regular ? amount : 0f;
						y -= small ? amount / mutliply : large ? amount * mutliply : regular ? amount : 0f;
					}

					if (pointerEventData.scrollDelta.y > 0f)
					{
						x += small ? amount / mutliply : large ? amount * mutliply : regular ? amount : 0f;
						y += small ? amount / mutliply : large ? amount * mutliply : regular ? amount : 0f;
					}

					if (clamp != null && clamp.Count > 1)
					{
						x = Mathf.Clamp(x, clamp[0], clamp[1]);
						if (clamp.Count == 2)
							y = Mathf.Clamp(y, clamp[0], clamp[1]);
						else
							y = Mathf.Clamp(y, clamp[2], clamp[3]);
					}

					ifx.text = x.ToString("f2");
					ify.text = y.ToString("f2");
				}
			});
			return entry;
		}

		public static EventTrigger.Entry ScrollDeltaVector2Int(InputField ifx, InputField ify, int amount, List<int> clamp = null)
		{
			var entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Scroll;
			entry.callback.AddListener(delegate (BaseEventData eventData)
			{
				var pointerEventData = (PointerEventData)eventData;
				if (Input.GetKey(KeyCode.LeftShift) && int.TryParse(ifx.text, out int x) && int.TryParse(ify.text, out int y))
				{
					var largeKey = KeyCode.LeftControl;
					var regularKey = KeyCode.None;

					// Large Amount
					bool large = largeKey == KeyCode.None && !Input.GetKey(regularKey) || Input.GetKey(largeKey);

					// Regular Amount
					bool regular = regularKey == KeyCode.None && !Input.GetKey(largeKey) || Input.GetKey(regularKey);

					if (pointerEventData.scrollDelta.y < 0f)
					{
						x -= large ? amount * 10 : regular ? amount : 0;
						y -= large ? amount * 10 : regular ? amount : 0;
					}

					if (pointerEventData.scrollDelta.y > 0f)
					{
						x += large ? amount * 10 : regular ? amount : 0;
						y += large ? amount * 10 : regular ? amount : 0;
					}

					if (clamp != null)
					{
						x = Mathf.Clamp(x, clamp[0], clamp[1]);
						if (clamp.Count == 2)
							y = Mathf.Clamp(y, clamp[0], clamp[1]);
						else
							y = Mathf.Clamp(y, clamp[2], clamp[3]);
					}

					ifx.text = x.ToString();
					ify.text = y.ToString();
				}
			});
			return entry;
		}

		#endregion

		#region Buttons

		public static void IncreaseDecreaseButtons(InputField _if, float _amount, float _divide, Transform t = null, List<float> clamp = null)
		{
			var tf = _if.transform;

			if (t != null)
			{
				tf = t;
			}

			float num = _amount;

			var btR = tf.Find("<").GetComponent<Button>();
			var btL = tf.Find(">").GetComponent<Button>();

			btR.onClick.RemoveAllListeners();
			btR.onClick.AddListener(delegate ()
			{
				//Small
				if (Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
				{
					num = _amount / _divide;
				}

				//Big
				if (!Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
				{
					num = _amount * _divide;
				}

				//Normal
				if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
				{
					num = _amount;
				}

				if (clamp == null)
					_if.text = (float.Parse(_if.text) - num).ToString();
				else
					_if.text = Mathf.Clamp(float.Parse(_if.text) - num, clamp[0], clamp[1]).ToString();
			});

			btL.onClick.RemoveAllListeners();
			btL.onClick.AddListener(delegate ()
			{
				//Small
				if (Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
				{
					num = _amount / _divide;
				}

				//Big
				if (!Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
				{
					num = _amount * _divide;
				}

				//Normal
				if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
				{
					num = _amount;
				}

				if (clamp == null)
					_if.text = (float.Parse(_if.text) + num).ToString();
				else
					_if.text = Mathf.Clamp(float.Parse(_if.text) + num, clamp[0], clamp[1]).ToString();
			});
		}

		public static void IncreaseDecreaseButtonsInt(InputField _if, int _amount, Transform t = null, List<int> clamp = null)
		{
			var tf = _if.transform;

			if (t != null)
			{
				tf = t;
			}

			var btR = tf.Find("<").GetComponent<Button>();
			var btL = tf.Find(">").GetComponent<Button>();

			btR.onClick.RemoveAllListeners();
			btR.onClick.AddListener(delegate ()
			{
				int num = int.Parse(_if.text);
				if (Input.GetKey(KeyCode.LeftControl))
					num -= _amount * 10;
				else
					num -= _amount;

				if (clamp != null)
				{
					num = Mathf.Clamp(num, clamp[0], clamp[1]);
				}

				_if.text = num.ToString();
			});

			btL.onClick.RemoveAllListeners();
			btL.onClick.AddListener(delegate ()
			{
				int num = int.Parse(_if.text);
				if (Input.GetKey(KeyCode.LeftControl))
					num += _amount * 10;
				else
					num += _amount;

				if (clamp != null)
				{
					num = Mathf.Clamp(num, clamp[0], clamp[1]);
				}

				_if.text = num.ToString();
			});
		}

        #endregion

        #region Tooltip

        public static void AddTooltip(GameObject _gameObject, string _desc, string _hint, List<string> _keys = null, DataManager.Language _language = DataManager.Language.english, bool clear = false)
		{
			if (!_gameObject.GetComponent<HoverTooltip>())
			{
				_gameObject.AddComponent<HoverTooltip>();
			}
			HoverTooltip hoverTooltip = _gameObject.GetComponent<HoverTooltip>();
			if (clear)
				hoverTooltip.tooltipLangauges.Clear();
			hoverTooltip.tooltipLangauges.Add(NewTooltip(_desc, _hint, _keys, _language));
		}

		public static HoverTooltip.Tooltip NewTooltip(string _desc, string _hint, List<string> _keys = null, DataManager.Language _language = DataManager.Language.english)
		{
			HoverTooltip.Tooltip tooltip = new HoverTooltip.Tooltip();
			tooltip.desc = _desc;
			tooltip.hint = _hint;

			if (_keys == null)
			{
				_keys = new List<string>();
			}

			tooltip.keys = _keys;
			tooltip.language = _language;
			return tooltip;
		}

		public static void SetTooltip(HoverTooltip.Tooltip _tooltip, string _desc, string _hint, List<string> _keys = null, DataManager.Language _language = DataManager.Language.english)
		{
			_tooltip.desc = _desc;
			_tooltip.hint = _hint;
			_tooltip.keys = _keys;
			_tooltip.language = _language;
		}

        #endregion

        public static string GetShape(int _shape, int _shapeOption)
		{
			if (ObjectManager.inst != null && ObjectManager.inst.objectPrefabs.Count > 0)
			{
				int s = Mathf.Clamp(_shape, 0, ObjectManager.inst.objectPrefabs.Count - 1);
				int so = Mathf.Clamp(_shapeOption, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);
				return ObjectManager.inst.objectPrefabs[s].options[so].name;
			}
			return "no shape";
		}

        #region Color

        public static string ColorToHex(Color32 color) => color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");

		public static Color ChangeColorHSV(Color color, float hue, float sat, float val)
		{
			double num;
			double saturation;
			double value;
			LSColors.ColorToHSV(color, out num, out saturation, out value);
			return LSColors.ColorFromHSV(num + hue, saturation + sat, value + val);
		}

		public static Color InvertColorHue(Color color)
		{
			double num;
			double saturation;
			double value;
			LSColors.ColorToHSV(color, out num, out saturation, out value);
			return LSColors.ColorFromHSV(num - 180.0, saturation, value);
		}

		public static Color InvertColorValue(Color color)
		{
			double num;
			double sat;
			double val;
			LSColors.ColorToHSV(color, out num, out sat, out val);

			if (val < 0.5)
			{
				val = -val + 1;
			}
			else
			{
				val = -(val - 1);
			}

			return LSColors.ColorFromHSV(num, sat, val);
		}

        #endregion

        public static void CreateCollider(this PolygonCollider2D collider2D, MeshFilter meshFilter)
        {
			if (meshFilter.mesh != null)
			{
				collider2D.points = new Vector2[meshFilter.mesh.vertices.Length];

				Vector2[] ps = new Vector2[meshFilter.mesh.vertices.Length];

				for (int i = 0; i < meshFilter.mesh.vertices.Length; i++)
                {
					ps[i] = new Vector2(meshFilter.mesh.vertices[i].x, meshFilter.mesh.vertices[i].y);
				}

				collider2D.points = ps;
            }				
        }

		public static float TimeCodeToFloat(string str)
        {
			if (RegexMatch(str, new Regex(@"([0-9]+):([0-9]+):([0-9.]+)"), out Match match1))
            {
				var hours = float.Parse(match1.Groups[1].ToString()) * 3600f;
				var minutes = float.Parse(match1.Groups[2].ToString()) * 60f;
				var seconds = float.Parse(match1.Groups[3].ToString());

				return hours + minutes + seconds;
            }
			else if (RegexMatch(str, new Regex(@"([0-9]+):([0-9.]+)"), out Match match2))
            {
				var minutes = float.Parse(match2.Groups[1].ToString()) * 60f;
				var seconds = float.Parse(match2.Groups[2].ToString());

				return minutes + seconds;
            }

			return 0f;
		}

		public static bool RegexMatch(string str, Regex regex, out Match match)
		{
			if (regex != null && regex.Match(str).Success)
			{
				match = regex.Match(str);
				return true;
			}

			match = null;
			return false;
		}

		public static string Flip(string str)
		{
			string s;
			s = str.Replace("Left", "LSLeft87344874")
				.Replace("Right", "LSRight87344874")
				.Replace("left", "LSleft87344874")
				.Replace("right", "LSright87344874")
				.Replace("LEFT", "LSLEFT87344874")
				.Replace("RIGHT", "LSRIGHT87344874");

			return s.Replace("LSLeft87344874", "Right")
				.Replace("LSRight87344874", "Left")
				.Replace("LSleft87344874", "right")
				.Replace("LSright87344874", "left")
				.Replace("LSLEFT87344874", "RIGHT")
				.Replace("LSRIGHT87344874", "LEFT");
		}

		public static bool ColorMatch(Color a, Color b, float range, bool alpha = false)
		{
			if (alpha)
			{
				if (a.r < b.r + range && a.r > b.r - range && a.g < b.g + range && a.g > b.g - range && a.b < b.b + range && a.b > b.b - range && a.a < b.a + range && a.a > b.a - range)
					return true;
			}
			else if (a.r < b.r + range && a.r > b.r - range && a.g < b.g + range && a.g > b.g - range && a.b < b.b + range && a.b > b.b - range)
				return true;

			return false;
		}

		public static bool SearchString(string a, string searchTerm) => string.IsNullOrEmpty(searchTerm) || a.ToLower().Contains(searchTerm.ToLower());

		public static string GetURL(int type, int site, string link)
        {
			bool isInstances = type == 0;

			string result;
			if (isInstances)
            {
				if (InstanceLinks[site].linkFormat.Contains("{1}"))
                {
					var split = link.Split(',');
					result = string.Format(InstanceLinks[site].linkFormat, split[0], split[1]);
				}
				else
				{
					result = string.Format(InstanceLinks[site].linkFormat, link);
				}
			}
			else
            {
				result = UserLinks[site].linkFormat;
			}

			return result;
        }

		public static List<DataManager.LinkType> InstanceLinks => new List<DataManager.LinkType>
		{
			new DataManager.LinkType("Spotify", "https://open.spotify.com/artist/{0}"),
			new DataManager.LinkType("SoundCloud", "https://soundcloud.com/{0}"),
			new DataManager.LinkType("Bandcamp", "https://{0}.bandcamp.com/{1}"),
			new DataManager.LinkType("YouTube", "https://youtube.com/watch?v={0}"),
			new DataManager.LinkType("Newgrounds", "https://newgrounds.com/audio/listen/{0}"),
		};

		public static List<DataManager.LinkType> UserLinks => new List<DataManager.LinkType>
		{
			new DataManager.LinkType("Spotify", "https://open.spotify.com/artist/{0}"),
			new DataManager.LinkType("SoundCloud", "https://soundcloud.com/{0}"),
			new DataManager.LinkType("Bandcamp", "https://{0}.bandcamp.com"),
			new DataManager.LinkType("YouTube", "https://youtube.com/c/{0}"),
			new DataManager.LinkType("Newgrounds", "https://{0}.newgrounds.com/"),
		};

		/// <summary>
		/// Assigns both Keyboard and Controller to actions.
		/// </summary>
		/// <returns>MyGameActions with both Keyboard and Controller inputs.</returns>
		public static MyGameActions CreateWithBothBindings()
		{
			var myGameActions = new MyGameActions();

			// Controller
			myGameActions.Up.AddDefaultBinding(InputControlType.DPadUp);
			myGameActions.Up.AddDefaultBinding(InputControlType.LeftStickUp);
			myGameActions.Down.AddDefaultBinding(InputControlType.DPadDown);
			myGameActions.Down.AddDefaultBinding(InputControlType.LeftStickDown);
			myGameActions.Left.AddDefaultBinding(InputControlType.DPadLeft);
			myGameActions.Left.AddDefaultBinding(InputControlType.LeftStickLeft);
			myGameActions.Right.AddDefaultBinding(InputControlType.DPadRight);
			myGameActions.Right.AddDefaultBinding(InputControlType.LeftStickRight);
			myGameActions.Boost.AddDefaultBinding(InputControlType.RightTrigger);
			myGameActions.Boost.AddDefaultBinding(InputControlType.RightBumper);
			myGameActions.Boost.AddDefaultBinding(InputControlType.Action1);
			myGameActions.Boost.AddDefaultBinding(InputControlType.Action3);
			myGameActions.Join.AddDefaultBinding(InputControlType.Action1);
			myGameActions.Join.AddDefaultBinding(InputControlType.Action2);
			myGameActions.Join.AddDefaultBinding(InputControlType.Action3);
			myGameActions.Join.AddDefaultBinding(InputControlType.Action4);
			myGameActions.Pause.AddDefaultBinding(InputControlType.Command);
			myGameActions.Escape.AddDefaultBinding(InputControlType.Action2);
			myGameActions.Escape.AddDefaultBinding(InputControlType.Action4);

			// Keyboard
			myGameActions.Up.AddDefaultBinding(new Key[] { Key.UpArrow });
			myGameActions.Up.AddDefaultBinding(new Key[] { Key.W });
			myGameActions.Down.AddDefaultBinding(new Key[] { Key.DownArrow });
			myGameActions.Down.AddDefaultBinding(new Key[] { Key.S });
			myGameActions.Left.AddDefaultBinding(new Key[] { Key.LeftArrow });
			myGameActions.Left.AddDefaultBinding(new Key[] { Key.A });
			myGameActions.Right.AddDefaultBinding(new Key[] { Key.RightArrow });
			myGameActions.Right.AddDefaultBinding(new Key[] { Key.D });
			myGameActions.Boost.AddDefaultBinding(new Key[] { Key.Space });
			myGameActions.Boost.AddDefaultBinding(new Key[] { Key.Return });
			myGameActions.Boost.AddDefaultBinding(new Key[] { Key.Z });
			myGameActions.Boost.AddDefaultBinding(new Key[] { Key.X });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.Space });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.A });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.S });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.D });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.W });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.LeftArrow });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.RightArrow });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.DownArrow });
			myGameActions.Join.AddDefaultBinding(new Key[] { Key.UpArrow });
			myGameActions.Pause.AddDefaultBinding(new Key[] { Key.Escape });
			myGameActions.Escape.AddDefaultBinding(new Key[] { Key.Escape });
			return myGameActions;
		}
    }
}
