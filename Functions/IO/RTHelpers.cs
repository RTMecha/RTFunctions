﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using LSFunctions;
using InControl;

namespace RTFunctions.Functions.IO
{
    public static class RTHelpers
	{
		public static float perspectiveZoom = 1f;
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

		public static bool Paused => GameManager.inst && GameManager.inst.gameState == GameManager.State.Paused;
		public static bool Playing => GameManager.inst && GameManager.inst.gameState == GameManager.State.Playing;

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

		public static EventTrigger.Entry ScrollDelta(InputField _if, float _amount, float _divide, bool _multi = false, List<float> clamp = null)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Scroll;
			entry.callback.AddListener(delegate (BaseEventData eventData)
			{
				PointerEventData pointerEventData = (PointerEventData)eventData;

				if (float.TryParse(_if.text, out float result))
				{
					if (!_multi)
					{
						//Small
						if (Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								float x = result;
								x -= _amount / _divide;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								float x = result;
								x += _amount / _divide;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
							}
						}

						//Big
						if (!Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								float x = result;
								x -= _amount * _divide;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								float x = result;
								x += _amount * _divide;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
							}
						}

						//Normal
						if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								float x = result;
								x -= _amount;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								float x = result;
								x += _amount;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
							}
						}
					}
					else if (!Input.GetKey(KeyCode.LeftShift))
					{
						//Small
						if (Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								float x = result;
								x -= _amount / _divide;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								float x = result;
								x += _amount / _divide;
								_if.text = x.ToString("f2");
							}
						}

						//Big
						if (!Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								float x = result;
								x -= _amount * _divide;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								float x = result;
								x += _amount * _divide;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
							}
						}

						//Normal
						if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								float x = result;
								x -= _amount;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								float x = result;
								x += _amount;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString("f2");
							}
						}
					}
				}
			});
			return entry;
		}

		public static EventTrigger.Entry ScrollDeltaInt(InputField _if, int _amount, bool _multi = false, List<int> clamp = null)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Scroll;
			entry.callback.AddListener(delegate (BaseEventData eventData)
			{
				PointerEventData pointerEventData = (PointerEventData)eventData;

				if (int.TryParse(_if.text, out int result))
				{
					if (!_multi)
					{
						if (Input.GetKey(KeyCode.LeftControl))
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								int x = result;
								x -= _amount * 10;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString();
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								int x = result;
								x += _amount * 10;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString();
							}
						}
						else
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								int x = result;
								x -= _amount;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString();
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								int x = result;
								x += _amount;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString();
							}
						}
					}
					else if (!Input.GetKey(KeyCode.LeftShift))
					{
						if (Input.GetKey(KeyCode.LeftControl))
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								int x = result;
								x -= _amount * 10;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString();
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								int x = result;
								x += _amount * 10;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString();
							}
						}
						else
						{
							if (pointerEventData.scrollDelta.y < 0f)
							{
								int x = result;
								x -= _amount;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString();
								return;
							}
							if (pointerEventData.scrollDelta.y > 0f)
							{
								int x = result;
								x += _amount;

								if (clamp != null)
								{
									x = Mathf.Clamp(x, clamp[0], clamp[1]);
								}

								_if.text = x.ToString();
							}
						}
					}
				}
			});
			return entry;
		}

		public static EventTrigger.Entry ScrollDeltaVector2(InputField _ifX, InputField _ifY, float _amount, float _divide, List<float> clamp = null)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Scroll;
			entry.callback.AddListener(delegate (BaseEventData eventData)
			{
				PointerEventData pointerEventData = (PointerEventData)eventData;
				if (Input.GetKey(KeyCode.LeftShift))
				{
					//Small
					if (Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
					{
						if (pointerEventData.scrollDelta.y < 0f)
						{
							float x = float.Parse(_ifX.text);
							float y = float.Parse(_ifY.text);

							x -= _amount / _divide;
							y -= _amount / _divide;

							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString("f2");
							_ifY.text = y.ToString("f2");
							return;
						}
						if (pointerEventData.scrollDelta.y > 0f)
						{
							float x = float.Parse(_ifX.text);
							float y = float.Parse(_ifY.text);

							x += _amount / _divide;
							y += _amount / _divide;

							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString("f2");
							_ifY.text = y.ToString("f2");
						}
					}

					//Big
					if (!Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
					{
						if (pointerEventData.scrollDelta.y < 0f)
						{
							float x = float.Parse(_ifX.text);
							float y = float.Parse(_ifY.text);

							x -= _amount * _divide;
							y -= _amount * _divide;


							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString("f2");
							_ifY.text = y.ToString("f2");
							return;
						}
						if (pointerEventData.scrollDelta.y > 0f)
						{
							float x = float.Parse(_ifX.text);
							float y = float.Parse(_ifY.text);

							x += _amount * _divide;
							y += _amount * _divide;

							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString("f2");
							_ifY.text = y.ToString("f2");
						}
					}

					//Normal
					if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
					{
						if (pointerEventData.scrollDelta.y < 0f)
						{
							float x = float.Parse(_ifX.text);
							float y = float.Parse(_ifY.text);

							x -= _amount;
							y -= _amount;

							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString("f2");
							_ifY.text = y.ToString("f2");
							return;
						}
						if (pointerEventData.scrollDelta.y > 0f)
						{
							float x = float.Parse(_ifX.text);
							float y = float.Parse(_ifY.text);

							x += _amount;
							y += _amount;

							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString("f2");
							_ifY.text = y.ToString("f2");
						}
					}
				}
			});
			return entry;
		}

		public static EventTrigger.Entry ScrollDeltaVector2Int(InputField _ifX, InputField _ifY, int _amount, List<int> clamp = null)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Scroll;
			entry.callback.AddListener(delegate (BaseEventData eventData)
			{
				PointerEventData pointerEventData = (PointerEventData)eventData;
				if (Input.GetKey(KeyCode.LeftShift))
				{
					//Big
					if (Input.GetKey(KeyCode.LeftControl))
					{
						if (pointerEventData.scrollDelta.y < 0f)
						{
							int x = int.Parse(_ifX.text);
							int y = int.Parse(_ifY.text);

							x -= _amount * 10;
							y -= _amount * 10;


							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString();
							_ifY.text = y.ToString();
							return;
						}
						if (pointerEventData.scrollDelta.y > 0f)
						{
							int x = int.Parse(_ifX.text);
							int y = int.Parse(_ifY.text);

							x += _amount * 10;
							y += _amount * 10;

							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString();
							_ifY.text = y.ToString();
						}
					}

					//Normal
					if (!Input.GetKey(KeyCode.LeftControl))
					{
						if (pointerEventData.scrollDelta.y < 0f)
						{
							int x = int.Parse(_ifX.text);
							int y = int.Parse(_ifY.text);

							x -= _amount;
							y -= _amount;

							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString();
							_ifY.text = y.ToString();
							return;
						}
						if (pointerEventData.scrollDelta.y > 0f)
						{
							int x = int.Parse(_ifX.text);
							int y = int.Parse(_ifY.text);

							x += _amount;
							y += _amount;

							if (clamp != null)
							{
								x = Mathf.Clamp(x, clamp[0], clamp[1]);
								if (clamp.Count == 2)
									y = Mathf.Clamp(y, clamp[0], clamp[1]);
								else
									y = Mathf.Clamp(y, clamp[2], clamp[3]);
							}

							_ifX.text = x.ToString();
							_ifY.text = y.ToString();
						}
					}
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

		public static bool SearchString(string a, string searchTerm) => a.ToLower().Contains(searchTerm.ToLower()) || string.IsNullOrEmpty(searchTerm);

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

		#region Cipher Encryptions because heck it

		public static string AlphabetBinaryEncrypt(string c)
		{
			var t = c;
			var str = "";

			foreach (var ch in t)
			{
				var pl = ch.ToString();
				pl = AlphabetBinaryEncryptChar(ch.ToString());
				str += pl + " ";
			}
			return str;
		}

		public static string AlphabetBinaryEncryptChar(string c)
		{
			var t = c;

			if (alphabetLowercase.Contains(t.ToLower()))
			{
				return binary[alphabetLowercase.IndexOf(t.ToLower())];
			}

			return t;
		}

		public static string AlphabetByteEncrypt(string c)
		{
			var t = c;
			var str = "";

			foreach (var ch in t)
			{
				var pl = ch.ToString();
				pl = AlphabetByteEncryptChar(ch.ToString());
				str += pl + " ";
			}
			return str;
		}

		public static string AlphabetByteEncryptChar(string c)
		{
			var t = c;

			char ch = t.ToLower()[0];

			if (alphabetLowercase.Contains(t.ToLower()))
			{
				return ((byte)ch).ToString();
			}

			return t;
		}
		
		public static string AlphabetKevinEncrypt(string c)
		{
			var t = c;
			var str = "";

			foreach (var ch in t)
			{
				var pl = ch.ToString();
				pl = AlphabetKevinEncryptChar(ch.ToString());
				str += pl;
			}
			return str;
		}

		public static string AlphabetKevinEncryptChar(string c)
		{
			var t = c;

			if (alphabetLowercase.Contains(t.ToLower()))
			{
				return kevin[alphabetLowercase.IndexOf(t.ToLower())];
			}

			return t;
		}

		public static string AlphabetA1Z26Encrypt(string c)
		{
			var t = c;
			var str = "";
			foreach (var ch in t)
			{
				var pl = ch.ToString();
				pl = AlphabetA1Z26EncryptChar(ch.ToString());
				str += pl + " ";
			}
			return str;
		}

		public static string AlphabetA1Z26EncryptChar(string c)
		{
			var t = c;

			if (alphabetLowercase.Contains(t.ToLower()))
			{
				return (alphabetLowercase.IndexOf(t.ToLower()) + 1).ToString();
			}

			return t;
		}

		public static string AlphabetCaesarEncrypt(string c)
		{
			var t = c;
			var str = "";
			foreach (var ch in t)
			{
				var pl = ch.ToString();
				pl = AlphabetCaesarEncryptChar(ch.ToString());
				str += pl;
			}
			return str;
		}

		public static string AlphabetCaesarEncryptChar(string c)
		{
			var t = c;

			if (alphabetLowercase.Contains(t))
            {
				var index = alphabetLowercase.IndexOf(t) - 3;
				if (index < 0)
					index += 26;

				if (index < alphabetLowercase.Count && index >= 0)
				{
					return alphabetLowercase[index];
				}
			}

			if (alphabetUppercase.Contains(t))
            {
				var index = alphabetUppercase.IndexOf(t) - 3;
				if (index < 0)
					index += 26;

				if (index < alphabetUppercase.Count && index >= 0)
				{
					return alphabetUppercase[index];
				}
			}

			return t;
		}

		public static string AlphabetAtbashEncrypt(string c)
        {
			var t = c;
			var str = "";
			foreach (var ch in t)
            {
				var pl = ch.ToString();
                pl = AlphabetAtbashEncryptChar(ch.ToString());
				str += pl;
            }
			return str;
        }

		public static string AlphabetAtbashEncryptChar(string c)
        {
			var t = c;

			if (alphabetLowercase.Contains(t))
            {
				var index = -(alphabetLowercase.IndexOf(t) - alphabetLowercase.Count + 1);
				if (index < alphabetLowercase.Count && index >= 0)
                {
					return alphabetLowercase[index];
                }
            }

			if (alphabetUppercase.Contains(t))
            {
				var index = -(alphabetUppercase.IndexOf(t) - alphabetUppercase.Count + 1);
				if (index < alphabetUppercase.Count && index >= 0)
                {
					return alphabetUppercase[index];
                }
            }

			return t;
        }

		public static List<string> alphabetLowercase = new List<string>
		{
			"a",
			"b",
			"c",
			"d",
			"e",
			"f",
			"g",
			"h",
			"i",
			"j",
			"k",
			"l",
			"m",
			"n",
			"o",
			"p",
			"q",
			"r",
			"s",
			"t",
			"u",
			"v",
			"w",
			"x",
			"y",
			"z"
		};

		public static List<string> alphabetUppercase = new List<string>
		{
			"A",
			"B",
			"C",
			"D",
			"E",
			"F",
			"G",
			"H",
			"I",
			"J",
			"K",
			"L",
			"M",
			"N",
			"O",
			"P",
			"Q",
			"R",
			"S",
			"T",
			"U",
			"V",
			"W",
			"X",
			"Y",
			"Z"
		};

		public static List<string> kevin = new List<string>
		{
			"@",
			"|}",
			"(",
			"|)",
			"[-",
			"T-",
			"&",
			"|-|",
			"!",
			"_/",
			"|<",
			"|",
			"^^",
			"^",
			"0",
			"/>",
			"\\<",
			"|-",
			"5",
			"-|-",
			"(_)",
			"\\/",
			"\\/\\/",
			"*",
			"-/",
			"-/_"
		};

		public static List<string> binary = new List<string>
		{
			"01100001", // a
			"01100010", // b
			"01100011", // c
			"01100100", // d
			"01100101", // e
			"01100110", // f
			"01100111", // g
			"01101000", // h
			"01001001", // i
			"01001001", // i
			"01001010", // j
			"01001011", // k
			"01001100", // l
			"01001101", // m
			"01001110", // n
			"01001111", // o
			"01010000", // p
			"01010001", // q
			"01010010", // r
			"01010011", // s
			"01010100", // t
			"01010101", // u
			"01010110", // v
			"01010111", // w
			"01011000", // x
			"01011001", // y
			"01011010", // z
		};

        #endregion
    }
}
