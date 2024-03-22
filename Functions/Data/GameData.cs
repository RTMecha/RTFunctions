﻿using LSFunctions;
using RTFunctions.Functions.Components.Player;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaseBackgroundObject = DataManager.GameData.BackgroundObject;
using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BaseBeatmapTheme = DataManager.BeatmapTheme;
using BaseEventKeyframe = DataManager.GameData.EventKeyframe;
using BaseGameData = DataManager.GameData;
using Modifier = RTFunctions.Functions.Data.BeatmapObject.Modifier;

namespace RTFunctions.Functions.Data
{
    public class GameData : BaseGameData
	{
		public GameData()
		{

		}

		public static GameData Current => (GameData)DataManager.inst.gameData;

		public Dictionary<string, BaseBeatmapTheme> beatmapThemes = new Dictionary<string, BaseBeatmapTheme>();

		public List<LevelModifier> levelModifiers = new List<LevelModifier>();

		/// <summary>
		/// Class for alpha EventTrigger support.
		/// Known Triggers:
		/// - Time
		/// Known Event Types:
		/// - Player_Heartrate
		/// - Player Bubble
		/// </summary>
		public class LevelModifier
        {
			public Modifier TriggerModifier { get; set; }
			public Modifier ActionModifier { get; set; }

			public int retriggerAmount = -1;
			public int current;

			public void AssignModifier(Modifier.Type type, int i)
			{
				if (type == Modifier.Type.Trigger)
					AssignTrigger(i);
				if (type == Modifier.Type.Action)
					AssignAction(i);
            }

			public void AssignModifier(int action, int trigger)
			{
				AssignTrigger(trigger);
				AssignAction(action);
            }

			public void AssignTrigger(int i)
            {
				TriggerModifier = DefaultTriggers[Mathf.Clamp(i, 0, DefaultTriggers.Length - 1)];
            }
			
			public void AssignAction(int i)
            {
				ActionModifier = DefaultActions[Mathf.Clamp(i, 0, DefaultActions.Length - 1)];
            }

			public void Activate()
            {
				var trigger = Trigger();
				if ((trigger && !TriggerModifier.not || !trigger && TriggerModifier.not) && !TriggerModifier.active)
				{
					Action();

					if (!TriggerModifier.constant)
						TriggerModifier.active = true;
				}
				else if (!(trigger && !TriggerModifier.not || !trigger && TriggerModifier.not))
					TriggerModifier.active = false;
            }

			public bool Trigger()
            {
				var modiifer = TriggerModifier;

				if (modiifer == null || retriggerAmount != -1 && current > retriggerAmount)
					return false;

				current++;

				var time = Updater.CurrentTime;

				switch (modiifer.commands[0].ToLower())
                {
					case "none":
                        {
							return true;
                        }
					case "time":
                        {
							return modiifer.commands.Count > 2 && float.TryParse(modiifer.commands[1], out float min) && float.TryParse(modiifer.commands[2], out float max)
								&& time >= min - 0.01f && time <= max + 0.1f;
						}
					case "playerhit":
                        {
							return PlayerManager.Players.Any(x => x.Player != null && x.Player.isTakingHit);
						}
					case "playerdeath":
                        {
							return PlayerManager.Players.Any(x => x.Player != null && x.Player.isDead);
						}
					case "levelstart":
                        {
							return AudioManager.inst.CurrentAudioSource.time < 0.1f;
						}
                }

				return false;
            }

			public void Action()
            {
				var modifier = ActionModifier;

				if (modifier == null)
					return;

				switch (modifier.commands[0].ToLower().Replace(" ", "").Replace("_", ""))
                {
					case "playerlocation":
						{
							float x = 0f;
							float y = 0f;
							float t = 0f;

							if (modifier.commands.Count > 1 && float.TryParse(modifier.commands[1], out float xResult))
								x = xResult;
							
							if (modifier.commands.Count > 2 && float.TryParse(modifier.commands[2], out float yResult))
								y = yResult;
							
							if (modifier.commands.Count > 2 && float.TryParse(modifier.commands[2], out float tResult))
								t = tResult;

							foreach (var player in PlayerManager.Players)
							{
								if (!player.Player || !player.Player.playerObjects.ContainsKey("RB Parent") || !player.Player.playerObjects["RB Parent"].gameObject)
									continue;

								var tf = player.Player.playerObjects["RB Parent"].gameObject.transform;

								if (t == 0f)
								{
									tf.localPosition = new Vector3(x, y, 0f);
									continue;
								}

								var animation = new Animation.AnimationManager.Animation("Player Move");

								animation.vector2Animations = new List<Animation.AnimationManager.Animation.AnimationObject<Vector2>>
								{
									new Animation.AnimationManager.Animation.AnimationObject<Vector2>(new List<Animation.Keyframe.IKeyframe<Vector2>>
									{
										new Animation.Keyframe.Vector2Keyframe(0f, tf.localPosition, Animation.Ease.Linear),
										new Animation.Keyframe.Vector2Keyframe(t, new Vector2(x, y), Animation.Ease.Linear),
										new Animation.Keyframe.Vector2Keyframe(t + 0.02f, new Vector2(x, y), Animation.Ease.Linear),
									}, delegate (Vector2 x)
									{
										if (!tf)
											return;

										tf.localPosition = x;
									}),
								};
							}

							break;
						}
					case "playerboostlock":
                        {
							if (modifier.commands.Count > 3 && !string.IsNullOrEmpty(modifier.commands[1]) && bool.TryParse(modifier.commands[1], out bool lockBoost))
                            {
								RTPlayer.LockBoost = lockBoost;
							}

							break;
                        }

				}
            }

			public static Modifier[] DefaultTriggers => new Modifier[]
			{
				new Modifier
                {
					type = Modifier.Type.Trigger,
					constant = true,
					commands = new List<string>
					{
						"time",
						"0", // Activation Time Range Min
						"0", // Activation Time Range Max
					},
					value = "",
				}, // time
				new Modifier
                {
					type = Modifier.Type.Trigger,
					constant = false,
					commands = new List<string>
					{
						"playerHit",
						"0", // Activation Time Range Min
						"0", // Activation Time Range Max
					},
					value = "",
				}, // playerHit
				new Modifier
                {
					type = Modifier.Type.Trigger,
					constant = false,
					commands = new List<string>
					{
						"playerDeath",
						"0", // Activation Time Range Min
						"0", // Activation Time Range Max
					},
					value = "",
				}, // playerDeath
				new Modifier
                {
					type = Modifier.Type.Trigger,
					constant = false,
					commands = new List<string>
					{
						"levelStart",
						"0", // Activation Time Range Min
						"0", // Activation Time Range Max
					},
					value = "",
				}, // levelStart
			};
			public static Modifier[] DefaultActions => new Modifier[]
			{
				new Modifier
                {
					type = Modifier.Type.Action,
					constant = false,
					commands = new List<string>
                    {
						"vnInk"
                    },
					value = "",
                }, // vnInk
				new Modifier
                {
					type = Modifier.Type.Action,
					constant = false,
					commands = new List<string>
                    {
						"vnTimeline"
                    },
					value = "",
				}, // vnTimeline
				new Modifier
                {
					type = Modifier.Type.Action,
					constant = false,
					commands = new List<string>
                    {
						"playerBubble",
						"Text", // Text
						"0", // Time
                    },
					value = "",
				}, // playerBubble (Probably won't have support for this yet)
				new Modifier
                {
					type = Modifier.Type.Action,
					constant = false,
					commands = new List<string>
                    {
						"playerLocation",
						"0", // X
						"0", // Y
						"0", // Time
                    },
					value = "",
				}, // playerLocation
				new Modifier
                {
					type = Modifier.Type.Action,
					constant = false,
					commands = new List<string>
                    {
						"playerBoostLock",
						"False", // Lock Enabled
						"", // Show Bubble
						"", // Bubble Time
                    },
					value = "",
				}, // playerBoostLock
			};
        }

		#region Methods

		public static GameData DeepCopy(GameData orig)
		{
			if (orig.beatmapObjects == null)
				orig.beatmapObjects = new List<BaseBeatmapObject>();
			if (orig.eventObjects == null)
			{
				orig.eventObjects = new EventObjects();
			}
			if (orig.backgroundObjects == null)
			{
				orig.backgroundObjects = new List<BaseBackgroundObject>();
			}

			var gameData = new GameData();
			var beatmapData = new BeatmapData();
			beatmapData.editorData = new BeatmapData.EditorData
			{
				timelinePos = orig.beatmapData.editorData.timelinePos,
				mainTimelineZoom = orig.beatmapData.editorData.mainTimelineZoom
			};
			beatmapData.levelData = new BeatmapData.LevelData
			{
				levelVersion = orig.beatmapData.levelData.levelVersion,
				backgroundColor = orig.beatmapData.levelData.backgroundColor,
				followPlayer = orig.beatmapData.levelData.followPlayer,
				showIntro = orig.beatmapData.levelData.showIntro
			};
			beatmapData.checkpoints = orig.beatmapData.checkpoints.Select(x => BeatmapData.Checkpoint.DeepCopy(x)).ToList();
			beatmapData.markers = orig.beatmapData.markers.Select(x => new BeatmapData.Marker(x.active, x.name, x.desc, x.color, x.time)).ToList();

			gameData.beatmapData = beatmapData;
			gameData.beatmapObjects = new List<BaseBeatmapObject>((from obj in orig.beatmapObjects
																   select Data.BeatmapObject.DeepCopy((Data.BeatmapObject)obj, false)).ToList());
			gameData.backgroundObjects = new List<BaseBackgroundObject>((from obj in orig.backgroundObjects
																		 select Data.BackgroundObject.DeepCopy((Data.BackgroundObject)obj)).ToList());
			gameData.eventObjects = EventObjects.DeepCopy(orig.eventObjects);
			return gameData;
		}

		public static GameData ConvertedGameData { get; set; }

		public static GameData ParseVG(JSONNode jn, bool parseThemes = true)
		{
			var gameData = new GameData();

			for (int i = 0; i < jn["triggers"].Count; i++)
            {
				var levelModifier = new LevelModifier();

				levelModifier.retriggerAmount = jn["triggers"][i]["event_retrigger"];

				levelModifier.AssignModifier(jn["triggers"][i]["event_type"].AsInt, jn["triggers"][i]["event_trigger"].AsInt);

				for (int j = 0; j < jn["triggers"][i]["event_data"].Count; j++)
                {
					var data = jn["triggers"][i]["event_data"][j];

					if (levelModifier.ActionModifier.commands.Count - 2 < data.Count)
                    {
						levelModifier.ActionModifier.commands.Add(data);
					}
					else
                    {
						levelModifier.ActionModifier.commands[j + 1] = data;
					}
				}

				levelModifier.TriggerModifier.commands[1] = jn["triggers"][i]["event_trigger_time"]["x"].AsFloat.ToString();
				levelModifier.TriggerModifier.commands[2] = jn["triggers"][i]["event_trigger_time"]["y"].AsFloat.ToString();

				gameData.levelModifiers.Add(levelModifier);
			}

			Debug.Log($"{FunctionsPlugin.className}Parsing BeatmapData");
			gameData.beatmapData = LevelBeatmapData.ParseVG(jn);

			gameData.beatmapData.markers = gameData.beatmapData.markers.OrderBy(x => x.time).ToList();

			Debug.Log($"{FunctionsPlugin.className}Parsing Checkpoints");
			for (int i = 0; i < jn["checkpoints"].Count; i++)
			{
				var name = jn["checkpoints"][i]["n"] == null ? "" : (string)jn["checkpoints"][i]["n"];
				var time = jn["checkpoints"][i]["t"] == null ? 0f : jn["checkpoints"][i]["t"].AsFloat;
				var pos = jn["checkpoints"][i]["p"] == null ? Vector2.zero : new Vector2(jn["checkpoints"][i]["p"]["x"] == null ? 0f : jn["checkpoints"][i]["p"]["x"].AsFloat, jn["checkpoints"][i]["p"]["y"] == null ? 0f : jn["checkpoints"][i]["p"]["y"].AsFloat);
				gameData.beatmapData.checkpoints.Add(new BeatmapData.Checkpoint(true, name, time, pos));
			}

			Debug.Log($"{FunctionsPlugin.className}Parsing Objects");
			for (int i = 0; i < jn["objects"].Count; i++)
				gameData.beatmapObjects.Add(Data.BeatmapObject.ParseVG(jn["objects"][i]));

			Debug.Log($"{FunctionsPlugin.className}Parsing Prefab Objects");
			for (int i = 0; i < jn["prefab_objects"].Count; i++)
				gameData.prefabObjects.Add(Data.PrefabObject.ParseVG(jn["prefab_objects"][i]));

			Debug.Log($"{FunctionsPlugin.className}Parsing Prefabs");
			for (int i = 0; i < jn["prefabs"].Count; i++)
				gameData.prefabs.Add(Data.Prefab.ParseVG(jn["prefabs"][i]));

			Dictionary<string, string> idConversion = new Dictionary<string, string>();

			if (jn["themes"] != null)
			{
				Debug.Log($"{FunctionsPlugin.className}Parsing Beatmap Themes");

				if (parseThemes)
                {
					DataManager.inst.CustomBeatmapThemes.Clear();
					DataManager.inst.BeatmapThemeIndexToID.Clear();
					DataManager.inst.BeatmapThemeIDToIndex.Clear();
				}

				for (int i = 0; i < jn["themes"].Count; i++)
				{
					var beatmapTheme = BeatmapTheme.ParseVG(jn["themes"][i]);

					if (!string.IsNullOrEmpty(beatmapTheme.VGID) && !idConversion.ContainsKey(beatmapTheme.VGID))
					{
						idConversion.Add(beatmapTheme.VGID, beatmapTheme.id);
					}

					if (!gameData.beatmapThemes.ContainsKey(beatmapTheme.id))
						gameData.beatmapThemes.Add(beatmapTheme.id, beatmapTheme);

					if (parseThemes)
					{

						DataManager.inst.CustomBeatmapThemes.Add(beatmapTheme);
						if (DataManager.inst.BeatmapThemeIDToIndex.ContainsKey(int.Parse(beatmapTheme.id)))
						{
							var list = DataManager.inst.CustomBeatmapThemes.Where(x => x.id == beatmapTheme.id).ToList();
							var str = "";
							for (int j = 0; j < list.Count; j++)
							{
								str += list[j].name;
								if (i != list.Count - 1)
									str += ", ";
							}

							if (EditorManager.inst != null)
								EditorManager.inst.DisplayNotification($"Unable to Load theme [{beatmapTheme.name}] due to conflicting themes: {str}", 2f, EditorManager.NotificationType.Error);
						}
						else
						{
							DataManager.inst.BeatmapThemeIndexToID.Add(DataManager.inst.AllThemes.Count - 1, int.Parse(beatmapTheme.id));
							DataManager.inst.BeatmapThemeIDToIndex.Add(int.Parse(beatmapTheme.id), DataManager.inst.AllThemes.Count - 1);
						}
					}

					beatmapTheme = null;
				}
			}

			gameData.backgroundObjects.Add(new Data.BackgroundObject
			{
				active = false,
				pos = new Vector2(9999f, 9999f),
			});

			gameData.eventObjects = new EventObjects();
			gameData.eventObjects.allEvents = new List<List<BaseEventKeyframe>>();

			string breakContext = "";
			try
			{
				Debug.Log($"{FunctionsPlugin.className}Parsing VG Event Keyframes");
				// Move
				breakContext = "Move";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][0].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][0][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(kfjn["ev"][0].AsFloat, kfjn["ev"][1].AsFloat);

					gameData.eventObjects.allEvents[0].Add(eventKeyframe);
				}

				// Zoom
				breakContext = "Zoom";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][1].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][1][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(kfjn["ev"][0].AsFloat);

					gameData.eventObjects.allEvents[1].Add(eventKeyframe);
				}

				// Rotate
				breakContext = "Rotate";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][2].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][2][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(kfjn["ev"][0].AsFloat);

					gameData.eventObjects.allEvents[2].Add(eventKeyframe);
				}

				// Shake
				breakContext = "Shake";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][3].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][3][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(kfjn["ev"][0].AsFloat, 1f, 1f);

					gameData.eventObjects.allEvents[3].Add(eventKeyframe);
				}

				// Theme
				breakContext = "Theme";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][4].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][4][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					// Since theme keyframes use random string IDs as their value instead of numbers (wtf), we have to convert the new IDs to numbers.
					if (!string.IsNullOrEmpty(kfjn["evs"][0]) && idConversion.ContainsKey(kfjn["evs"][0]))
						eventKeyframe.SetEventValues(Parser.TryParse(idConversion[kfjn["evs"][0]], 0f));
					else
						eventKeyframe.SetEventValues(0f);

					gameData.eventObjects.allEvents[4].Add(eventKeyframe);
				}

				// Chroma
				breakContext = "Chroma";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][5].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][5][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(kfjn["ev"][0].AsFloat);

					gameData.eventObjects.allEvents[5].Add(eventKeyframe);
				}

				// Bloom
				breakContext = "Bloom";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][6].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][6][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(
						kfjn["ev"][0].AsFloat,
						kfjn["ev"][1].AsFloat,
						1f,
						0f,
						kfjn["ev"][2].AsFloat == 9f ? 18f : kfjn["ev"][2].AsFloat);

					gameData.eventObjects.allEvents[6].Add(eventKeyframe);
				}

				// Vignette
				breakContext = "Vignette";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][7].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][7][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(
						kfjn["ev"][0].AsFloat,
						kfjn["ev"][1].AsFloat,
						kfjn["ev"][2].AsFloat,
						1f,
						kfjn["ev"][4].AsFloat,
						kfjn["ev"][5].AsFloat,
						kfjn["ev"][6].AsFloat == 9f ? 18f : kfjn["ev"][6].AsFloat);

					gameData.eventObjects.allEvents[7].Add(eventKeyframe);
				}

				// Lens
				breakContext = "Lens";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][8].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][8][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(
						kfjn["ev"][0].AsFloat,
						kfjn["ev"][1].AsFloat,
						kfjn["ev"][2].AsFloat,
						1f,
						1f,
						1f);

					gameData.eventObjects.allEvents[8].Add(eventKeyframe);
				}

				// Grain
				breakContext = "Grain";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][9].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][9][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(
						kfjn["ev"][0].AsFloat,
						kfjn["ev"][1].AsFloat,
						kfjn["ev"][2].AsFloat);

					gameData.eventObjects.allEvents[9].Add(eventKeyframe);
				}

				// Hue
				breakContext = "Hue";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][12].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][12][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(
						kfjn["ev"][0].AsFloat);

					gameData.eventObjects.allEvents[10].Add(eventKeyframe);
				}

				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				gameData.eventObjects.allEvents[11].Add(Data.EventKeyframe.DeepCopy((Data.EventKeyframe)DefaultKeyframes[11]));
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				gameData.eventObjects.allEvents[12].Add(Data.EventKeyframe.DeepCopy((Data.EventKeyframe)DefaultKeyframes[12]));
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				gameData.eventObjects.allEvents[13].Add(Data.EventKeyframe.DeepCopy((Data.EventKeyframe)DefaultKeyframes[13]));
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				gameData.eventObjects.allEvents[14].Add(Data.EventKeyframe.DeepCopy((Data.EventKeyframe)DefaultKeyframes[14]));

				// Gradient
				breakContext = "Gradient";
				gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["events"][10].Count; i++)
				{
					var eventKeyframe = new Data.EventKeyframe();
					var kfjn = jn["events"][10][i];

					eventKeyframe.id = LSText.randomNumString(8);

					if (kfjn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(kfjn["ct"]))
						eventKeyframe.curveType = DataManager.inst.AnimationListDictionaryStr[kfjn["ct"]];

					eventKeyframe.eventTime = kfjn["t"].AsFloat;
					eventKeyframe.SetEventValues(
						kfjn["ev"][0].AsFloat,
						kfjn["ev"][1].AsFloat,
						kfjn["ev"][2].AsFloat == 9f ? 19f : kfjn["ev"][2].AsFloat,
						kfjn["ev"][3].AsFloat == 9f ? 19f : kfjn["ev"][3].AsFloat,
						kfjn["ev"].Count > 4 ? kfjn["ev"][4].AsFloat : 0f);

					gameData.eventObjects.allEvents[15].Add(eventKeyframe);
				}

				for (int i = 16; i < DefaultKeyframes.Count; i++)
				{
					gameData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());
					gameData.eventObjects.allEvents[i].Add(Data.EventKeyframe.DeepCopy((Data.EventKeyframe)DefaultKeyframes[i]));
				}
			}
			catch (System.Exception ex)
			{
				EditorManager.inst?.DisplayNotification($"There was an error in parsing VG Event Keyframes. Parsing got caught at {breakContext}", 4f, EditorManager.NotificationType.Error);
				if (!EditorManager.inst)
				{
					Debug.LogError($"There was an error in parsing VG Event Keyframes. Parsing got caught at {breakContext}.\n {ex}");
				}
				else
				{
					Debug.LogError($"{ex}");
				}
			}

			Debug.Log($"{FunctionsPlugin.className}Checking keyframe counts");
			ProjectData.Reader.ClampEventListValues(gameData.eventObjects.allEvents, EventCount);

			ConvertedGameData = gameData;

			return gameData;
		}

		public static GameData Parse(JSONNode jn, bool parseThemes = true)
		{
			var gameData = new GameData();

			for (int i = 0; i < jn["modifiers"].Count; i++)
            {
				var modifier = jn["modifiers"][i];

				var levelModifier = new LevelModifier();

				levelModifier.ActionModifier = Modifier.Parse(modifier["action"]);
				levelModifier.TriggerModifier = Modifier.Parse(modifier["trigger"]);
				levelModifier.retriggerAmount = modifier["retrigger"].AsInt;

				gameData.levelModifiers.Add(levelModifier);
            }

			gameData.beatmapData = LevelBeatmapData.Parse(jn);

			gameData.beatmapData.markers = gameData.beatmapData.markers.OrderBy(x => x.time).ToList();

			for (int i = 0; i < jn["checkpoints"].Count; i++)
				gameData.beatmapData.checkpoints.Add(ProjectData.Reader.ParseCheckpoint(jn["checkpoints"][i]));

			gameData.beatmapData.checkpoints = gameData.beatmapData.checkpoints.OrderBy(x => x.time).ToList();

			for (int i = 0; i < jn["prefabs"].Count; i++)
			{
				var prefab = Data.Prefab.Parse(jn["prefabs"][i]);
				if (gameData.prefabs.Find(x => x.ID == prefab.ID) == null)
					gameData.prefabs.Add(prefab);
			}

			for (int i = 0; i < jn["prefab_objects"].Count; i++)
			{
				var prefab = Data.PrefabObject.Parse(jn["prefab_objects"][i]);
				if (gameData.prefabObjects.Find(x => x.ID == prefab.ID) == null)
					gameData.prefabObjects.Add(prefab);
			}

			if (parseThemes)
			{
				foreach (var theme in DataManager.inst.BeatmapThemes)
					gameData.beatmapThemes.Add(theme.id, theme);

				DataManager.inst.CustomBeatmapThemes.Clear();
				DataManager.inst.BeatmapThemeIndexToID.Clear();
				DataManager.inst.BeatmapThemeIDToIndex.Clear();
				for (int i = 0; i < jn["themes"].Count; i++)
				{
					var beatmapTheme = BeatmapTheme.Parse(jn["themes"][i]);

					DataManager.inst.CustomBeatmapThemes.Add(beatmapTheme);
					if (DataManager.inst.BeatmapThemeIDToIndex.ContainsKey(int.Parse(beatmapTheme.id)))
					{
						var list = DataManager.inst.CustomBeatmapThemes.Where(x => x.id == beatmapTheme.id).ToList();
						var str = "";
						for (int j = 0; j < list.Count; j++)
						{
							str += list[j].name;
							if (i != list.Count - 1)
								str += ", ";
						}

						if (EditorManager.inst != null)
							EditorManager.inst.DisplayNotification($"Unable to Load theme [{beatmapTheme.name}] due to conflicting themes: {str}", 2f, EditorManager.NotificationType.Error);
					}
					else
					{
						DataManager.inst.BeatmapThemeIndexToID.Add(DataManager.inst.AllThemes.Count - 1, int.Parse(beatmapTheme.id));
						DataManager.inst.BeatmapThemeIDToIndex.Add(int.Parse(beatmapTheme.id), DataManager.inst.AllThemes.Count - 1);
					}

					if (!gameData.beatmapThemes.ContainsKey(jn["themes"][i]["id"]))
						gameData.beatmapThemes.Add(jn["themes"][i]["id"], beatmapTheme);
				}

			}

			for (int i = 0; i < jn["beatmap_objects"].Count; i++)
				gameData.beatmapObjects.Add(Data.BeatmapObject.Parse(jn["beatmap_objects"][i]));

			AssetManager.SpriteAssets.Clear();
			if (jn["assets"] != null && jn["assets"]["spr"] != null)
			{
				for (int i = 0; i < jn["assets"]["spr"].Count; i++)
				{
					var name = jn["assets"]["spr"][i]["n"];
					var data = jn["assets"]["spr"][i]["d"];

					if (!AssetManager.SpriteAssets.ContainsKey(name) && gameData.beatmapObjects.Has(x => x.text == name))
					{
						byte[] imageData = new byte[data.Count];
						for (int j = 0; j < data.Count; j++)
						{
							imageData[j] = (byte)data[j].AsInt;
						}

						var texture2d = new Texture2D(2, 2, TextureFormat.ARGB32, false);
						texture2d.LoadImage(imageData);

						texture2d.wrapMode = TextureWrapMode.Clamp;
						texture2d.filterMode = FilterMode.Point;
						texture2d.Apply();

						AssetManager.SpriteAssets.Add(name, SpriteManager.CreateSprite(texture2d));
					}
				}
			}

			for (int i = 0; i < jn["bg_objects"].Count; i++)
				gameData.backgroundObjects.Add(Data.BackgroundObject.Parse(jn["bg_objects"][i]));

			gameData.eventObjects.allEvents = ProjectData.Reader.ParseEventkeyframes(jn["events"], false);

			ProjectData.Reader.ClampEventListValues(gameData.eventObjects.allEvents, EventCount);

			return gameData;
		}

		public static int EventCount => ModCompatibility.mods.ContainsKey("EventsCore") ? DefaultKeyframes.Count : 10;

		public JSONNode ToJSONVG()
		{
			var jn = JSON.Parse("{}");

			jn["editor"]["bpm"]["snap"]["objects"] = true;
			jn["editor"]["bpm"]["bpm_value"] = 140f;
			jn["editor"]["bpm"]["bpm_offset"] = 0f;
			jn["editor"]["bpm"]["BPMValue"] = 140f;
			jn["editor"]["grid"]["scale"]["x"] = 2f;
			jn["editor"]["grid"]["scale"]["y"] = 2f;
			jn["editor"]["general"]["complexity"] = 0;
			jn["editor"]["general"]["theme"] = 0;
			jn["editor"]["general"]["test_mode"] = 0;
			jn["editor"]["preview"]["cam_zoom_offset"] = 0f;
			jn["editor"]["preview"]["cam_zoom_offset_color"] = 0;

			for (int i = 0; i < 6; i++)
				jn["editor_prefab_spawn"][i] = new JSONObject();

			for (int i = 1; i < 6; i++)
			{
				jn["parallax_settings"]["l"][i - 1]["d"] = 100 * i;
				jn["parallax_settings"]["l"][i - 1]["c"] = 1 * i;
			}

			for (int i = 0; i < levelModifiers.Count; i++)
			{
				var levelModifier = levelModifiers[i];

				var triggerIndex = LevelModifier.DefaultTriggers.ToList().FindIndex(x => x.commands[0] == levelModifier.TriggerModifier.commands[0]);
				var actionIndex = LevelModifier.DefaultActions.ToList().FindIndex(x => x.commands[0] == levelModifier.ActionModifier.commands[0]);

				if (triggerIndex < 0 || actionIndex < 0)
					continue;

				jn["triggers"][i]["event_trigger"] = triggerIndex;
				jn["triggers"][i]["event_trigger_time"]["x"] = Parser.TryParse(levelModifier.TriggerModifier.commands[1], 0f);
				jn["triggers"][i]["event_trigger_time"]["y"] = Parser.TryParse(levelModifier.TriggerModifier.commands[2], 0f);
				jn["triggers"][i]["event_retrigger"] = levelModifier.retriggerAmount;

				jn["triggers"][i]["event_type"] = actionIndex;

				for (int j = 1; j < levelModifier.ActionModifier.commands.Count; j++)
					jn["triggers"][i]["event_data"][j - 1] = levelModifier.ActionModifier.commands[j];
			}

			for (int i = 0; i < beatmapData.checkpoints.Count; i++)
			{
				var checkpoint = beatmapData.checkpoints[i];
				jn["checkpoints"][i]["n"] = checkpoint.name;
				jn["checkpoints"][i]["t"] = checkpoint.time;
				jn["checkpoints"][i]["p"]["X"] = checkpoint.pos.x;
				jn["checkpoints"][i]["p"]["y"] = checkpoint.pos.y;
			}

			for (int i = 0; i < beatmapObjects.Count; i++)
			{
				jn["objects"][i] = ((Data.BeatmapObject)beatmapObjects[i]).ToJSONVG();
			}

			if (prefabObjects.Count > 0)
				for (int i = 0; i < prefabObjects.Count; i++)
				{
					jn["prefab_objects"][i] = ((Data.PrefabObject)prefabObjects[i]).ToJSONVG();
				}
			else
				jn["prefab_objects"] = new JSONArray();

			if (prefabs.Count > 0)
				for (int i = 0; i < prefabs.Count; i++)
				{
					jn["prefabs"][i] = ((Data.Prefab)prefabs[i]).ToJSONVG();
				}
			else
				jn["prefabs"] = new JSONArray();

			Dictionary<string, string> idsConverter = new Dictionary<string, string>();

			int themeIndex = 0;
			var themes = DataManager.inst.CustomBeatmapThemes.Select(x => x as BeatmapTheme).Where(x => eventObjects.allEvents[4].Has(y => int.TryParse(x.id, out int id) && id == y.eventValues[0]));
			if (themes.Count() > 0)
				foreach (var beatmapTheme in themes)
				{
					beatmapTheme.VGID = LSText.randomString(16);

					if (!idsConverter.ContainsKey(Parser.TryParse(beatmapTheme.id, 0f).ToString()))
					{
						idsConverter.Add(Parser.TryParse(beatmapTheme.id, 0f).ToString(), beatmapTheme.VGID);
					}

					jn["themes"][themeIndex] = beatmapTheme.ToJSONVG();
					themeIndex++;
				}
			else
				jn["themes"] = new JSONArray();

			if (beatmapData.markers.Count > 0)
				for (int i = 0; i < beatmapData.markers.Count; i++)
				{
					jn["markers"][i] = beatmapData.markers[i].ToJSONVG();
				}
			else
				jn["markers"] = new JSONArray();

			// Event Handlers
			{
				// Move
				for (int i = 0; i < eventObjects.allEvents[0].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[0][i];
					jn["events"][0][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][0][i]["t"] = eventKeyframe.eventTime;
					jn["events"][0][i]["ev"][0] = eventKeyframe.eventValues[0];
					jn["events"][0][i]["ev"][1] = eventKeyframe.eventValues[1];
				}

				// Zoom
				for (int i = 0; i < eventObjects.allEvents[1].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[1][i];
					jn["events"][1][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][1][i]["t"] = eventKeyframe.eventTime;
					jn["events"][1][i]["ev"][0] = eventKeyframe.eventValues[0];
				}

				// Rotate
				for (int i = 0; i < eventObjects.allEvents[2].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[2][i];
					jn["events"][2][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][2][i]["t"] = eventKeyframe.eventTime;
					jn["events"][2][i]["ev"][0] = eventKeyframe.eventValues[0];
				}

				// Shake
				for (int i = 0; i < eventObjects.allEvents[3].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[3][i];
					jn["events"][3][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][3][i]["t"] = eventKeyframe.eventTime;
					jn["events"][3][i]["ev"][0] = eventKeyframe.eventValues[0];
				}

				// Themes
				for (int i = 0; i < eventObjects.allEvents[4].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[4][i];
					jn["events"][4][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][4][i]["t"] = eventKeyframe.eventTime;
					jn["events"][4][i]["evs"][0] = idsConverter.ContainsKey(eventKeyframe.eventValues[0].ToString()) ? idsConverter[eventKeyframe.eventValues[0].ToString()] : eventKeyframe.eventValues[0].ToString();
				}

				// Chroma
				for (int i = 0; i < eventObjects.allEvents[5].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[5][i];
					jn["events"][5][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][5][i]["t"] = eventKeyframe.eventTime;
					jn["events"][5][i]["ev"][0] = eventKeyframe.eventValues[0];
				}

				// Bloom
				for (int i = 0; i < eventObjects.allEvents[6].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[6][i];
					jn["events"][6][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][6][i]["t"] = eventKeyframe.eventTime;
					jn["events"][6][i]["ev"][0] = eventKeyframe.eventValues[0];
					jn["events"][6][i]["ev"][1] = eventKeyframe.eventValues[1];
					jn["events"][6][i]["ev"][2] = Mathf.Clamp(eventKeyframe.eventValues[4], 0f, 9f);
				}

				// Vignette
				for (int i = 0; i < eventObjects.allEvents[7].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[7][i];
					jn["events"][7][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][7][i]["t"] = eventKeyframe.eventTime;
					jn["events"][7][i]["ev"][0] = eventKeyframe.eventValues[0];
					jn["events"][7][i]["ev"][1] = eventKeyframe.eventValues[1];
					jn["events"][7][i]["ev"][2] = eventKeyframe.eventValues[2];
					jn["events"][7][i]["ev"][3] = eventKeyframe.eventValues[3];
					jn["events"][7][i]["ev"][4] = eventKeyframe.eventValues[4];
					jn["events"][7][i]["ev"][5] = eventKeyframe.eventValues[5];
					jn["events"][7][i]["ev"][6] = Mathf.Clamp(eventKeyframe.eventValues[6], 0f, 9f);
				}

				// Lens
				for (int i = 0; i < eventObjects.allEvents[8].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[8][i];
					jn["events"][8][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][8][i]["t"] = eventKeyframe.eventTime;
					jn["events"][8][i]["ev"][0] = eventKeyframe.eventValues[0];
					jn["events"][8][i]["ev"][1] = eventKeyframe.eventValues[1];
					jn["events"][8][i]["ev"][2] = eventKeyframe.eventValues[2];
				}

				// Grain
				for (int i = 0; i < eventObjects.allEvents[9].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[9][i];
					jn["events"][9][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][9][i]["t"] = eventKeyframe.eventTime;
					jn["events"][9][i]["ev"][0] = eventKeyframe.eventValues[0];
					jn["events"][9][i]["ev"][1] = eventKeyframe.eventValues[1];
					jn["events"][9][i]["ev"][2] = eventKeyframe.eventValues[2];
					jn["events"][9][i]["ev"][3] = 1f;
				}

				// Gradient
				for (int i = 0; i < eventObjects.allEvents[15].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[15][i];
					jn["events"][10][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][10][i]["t"] = eventKeyframe.eventTime;
					jn["events"][10][i]["ev"][0] = eventKeyframe.eventValues[0];
					jn["events"][10][i]["ev"][1] = eventKeyframe.eventValues[1];
					jn["events"][10][i]["ev"][2] = Mathf.Clamp(eventKeyframe.eventValues[2], 0f, 9f);
					jn["events"][10][i]["ev"][3] = Mathf.Clamp(eventKeyframe.eventValues[3], 0f, 9f);
					jn["events"][10][i]["ev"][4] = eventKeyframe.eventValues[4];
				}

				jn["events"][11][0]["ct"] = "Linear";
				jn["events"][11][0]["t"] = 0f;
				jn["events"][11][0]["ev"][0] = 0f;
				jn["events"][11][0]["ev"][1] = 0f;
				jn["events"][11][0]["ev"][2] = 0f;

				// Hueshift
				for (int i = 0; i < eventObjects.allEvents[10].Count; i++)
				{
					var eventKeyframe = eventObjects.allEvents[10][i];
					jn["events"][12][i]["ct"] = eventKeyframe.curveType.Name;
					jn["events"][12][i]["t"] = eventKeyframe.eventTime;
					jn["events"][12][i]["ev"][0] = eventKeyframe.eventValues[0];
				}

				jn["events"][13][0]["ct"] = "Linear";
				jn["events"][13][0]["t"] = 0f;
				jn["events"][13][0]["ev"][0] = 0f;
				jn["events"][13][0]["ev"][1] = 0f;
				jn["events"][13][0]["ev"][2] = 0f;
			}

			return jn;
		}

		public JSONNode ToJSON()
		{
			var jn = JSON.Parse("{}");

			jn["ed"]["timeline_pos"] = AudioManager.inst.CurrentAudioSource.time.ToString();
			for (int i = 0; i < beatmapData.markers.Count; i++)
			{
				jn["ed"]["markers"][i]["name"] = beatmapData.markers[i].name.ToString();
				jn["ed"]["markers"][i]["desc"] = beatmapData.markers[i].desc.ToString();
				jn["ed"]["markers"][i]["col"] = beatmapData.markers[i].color.ToString();
				jn["ed"]["markers"][i]["t"] = beatmapData.markers[i].time.ToString();
			}

			for (int i = 0; i < AssetManager.SpriteAssets.Count; i++)
            {
				jn["assets"]["spr"][i]["n"] = AssetManager.SpriteAssets.ElementAt(i).Key;
				var imageData = AssetManager.SpriteAssets.ElementAt(i).Value.texture.EncodeToPNG();
				for (int j = 0; j < imageData.Length; j++)
				{
					jn["assets"]["spr"][i]["d"][j] = imageData[j];
				}
            }

			for (int i = 0; i < levelModifiers.Count; i++)
			{
				var levelModifier = levelModifiers[i];

				jn["modifiers"][i]["action"] = levelModifier.ActionModifier.ToJSON();
				jn["modifiers"][i]["trigger"] = levelModifier.TriggerModifier.ToJSON();
				jn["modifiers"][i]["retrigger"] = levelModifier.retriggerAmount.ToString();
			}

			for (int i = 0; i < prefabObjects.Count; i++)
				if (!((Data.PrefabObject)prefabObjects[i]).fromModifier)
					jn["prefab_objects"][i] = ((Data.PrefabObject)prefabObjects[i]).ToJSON();

			jn["level_data"] = LevelBeatmapData.ModLevelData.ToJSON();

			for (int i = 0; i < prefabs.Count; i++)
				jn["prefabs"][i] = ((Data.Prefab)prefabs[i]).ToJSON();
			if (beatmapThemes != null)
			{
				var levelThemes = new List<BaseBeatmapTheme>();

				for (int i = 0; i < beatmapThemes.Count; i++)
				{
					var beatmapTheme = beatmapThemes.ElementAt(i).Value;

					string id = beatmapTheme.id;

					foreach (var keyframe in DataManager.inst.gameData.eventObjects.allEvents[4])
					{
						var eventValue = keyframe.eventValues[0].ToString();

						if (int.TryParse(id, out int num) && (int)keyframe.eventValues[0] == num && levelThemes.Find(x => int.TryParse(x.id, out int xid) && xid == (int)keyframe.eventValues[0]) == null)
						{
							levelThemes.Add(beatmapTheme);
						}
					}
				}

				for (int i = 0; i < levelThemes.Count; i++)
				{
					var beatmapTheme = (BeatmapTheme)levelThemes[i];

					jn["themes"][i] = beatmapTheme.ToJSON();
				}
			}

			for (int i = 0; i < beatmapData.checkpoints.Count; i++)
			{
				jn["checkpoints"][i]["active"] = "False";
				jn["checkpoints"][i]["name"] = beatmapData.checkpoints[i].name;
				jn["checkpoints"][i]["t"] = beatmapData.checkpoints[i].time.ToString();
				jn["checkpoints"][i]["pos"]["x"] = beatmapData.checkpoints[i].pos.x.ToString();
				jn["checkpoints"][i]["pos"]["y"] = beatmapData.checkpoints[i].pos.y.ToString();
			}

			for (int i = 0; i < beatmapObjects.Count; i++)
				jn["beatmap_objects"][i] = BeatmapObjects[i].ToJSON();

			for (int i = 0; i < backgroundObjects.Count; i++)
				jn["bg_objects"][i] = BackgroundObjects[i].ToJSON();

			for (int i = 0; i < eventObjects.allEvents.Count; i++)
				for (int j = 0; j < eventObjects.allEvents[i].Count; j++)
					jn["events"][EventTypes[i]][j] = ((Data.EventKeyframe)eventObjects.allEvents[i][j]).ToJSON();

			return jn;
		}

		#endregion

		public bool Modded
        {
            get
            {
				return
					BeatmapObjects.Has(x => x.modifiers.Count > 0
					|| x.objectType == Data.BeatmapObject.ObjectType.Solid
					|| x.desync
					|| x.background
					|| x.LDM
					|| x.parallaxSettings.Any(y => y != 1f)
					|| x.parentAdditive != "000"
					|| x.shape > 5
					|| x.shapeOption >= UnmoddedShapeOptions[Mathf.Clamp(x.shape, 0, UnmoddedShapeOptions.Length)]
					|| ArePositionKeyframesModded(x.events[0])
					|| AreScaleKeyframesModded(x.events[1])
					|| AreRotationKeyframesModded(x.events[2])
					|| AreColorKeyframesModded(x.events[3])) ||
					eventObjects.allEvents.Count > 24 && eventObjects.allEvents[24].Count > 0 &&
					eventObjects.allEvents[24].Any(x => x.eventValues.Length > 3 && x.eventValues[0] == 1f && (x.eventValues[1] == 1f || x.eventValues[2] == 1f));
            }
        }

		static bool ArePositionKeyframesModded(List<BaseEventKeyframe> eventKeyframes)
			=> eventKeyframes.Any(x => x.random > 4 || x.eventValues.Length > 2 && x.eventValues[2] != 0f || ((Data.EventKeyframe)x).relative);
		
		static bool AreScaleKeyframesModded(List<BaseEventKeyframe> eventKeyframes)
			=> eventKeyframes.Any(x => ((Data.EventKeyframe)x).relative);

		static bool AreRotationKeyframesModded(List<BaseEventKeyframe> eventKeyframes)
			=> eventKeyframes.Any(x => x.random > 4 || !((Data.EventKeyframe)x).relative);
		
		static bool AreColorKeyframesModded(List<BaseEventKeyframe> eventKeyframes)
			=> eventKeyframes.Any(x => x.random > 4 || x.eventValues[0] > 8f || x.eventValues[2] != 0f || x.eventValues[3] != 0f || x.eventValues[4] != 0f);

		public static int[] UnmoddedShapeOptions => new int[]
		{
			3,
			9,
			4,
			2,
			1,
			6
		};

		public static string[] EventTypes => new string[]
		{
			"pos", // 0
			"zoom", // 1
			"rot", // 2
			"shake", // 3
			"theme", // 4
			"chroma", // 5
			"bloom", // 6
			"vignette", // 7
			"lens", // 8
			"grain", // 9
			"cg", // 10
			"rip", // 11
			"rb", // 12
			"cs", // 13
			"offset", // 14
			"grd", // 15
			"dbv", // 16
			"scan", // 17
			"blur", // 18
			"pixel", // 19
			"bg", // 20
			"invert", // 21
			"timeline", // 22
			"player", // 23
			"follow_player", // 24
			"audio", // 25
			"vidbg_p", // 26
			"vidbg", // 27
			"sharp", // 28
			"bars", // 29
			"danger", // 30
			"xyrot", // 31
			"camdepth", // 32
		};

		public static List<BaseEventKeyframe> DefaultKeyframes = new List<BaseEventKeyframe>
		{
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2],
				id = LSText.randomNumString(8),
			}, // Move
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1]
				{ 20f },
				id = LSText.randomNumString(8),
			}, // Zoom
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1],
				id = LSText.randomNumString(8),
			}, // Rotate
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[5]
				{
					0f, // Shake Intensity
					1f, // Shake X
					1f, // Shake Y
					1f, // Shake Interpolation
					1f, // Shake Speed
                },
				id = LSText.randomNumString(8),
			}, // Shake
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1],
				id = LSText.randomNumString(8),
			}, // Theme
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1],
				id = LSText.randomNumString(8),
			}, // Chroma
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[5]
				{
					0f, // Bloom Intensity
					7f, // Bloom Diffusion
					1f, // Bloom Threshold
					0f, // Bloom Anamorphic Ratio
					18f // Bloom Color
				},
				id = LSText.randomNumString(8),
			}, // Bloom
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[7]
				{
					0f, // Vignette Intensity
					0f, // Vignette Smoothness
					0f, // Vignette Rounded
					0f, // Vignette Roundness
					0f, // Vignette Center X
					0f, // Vignette Center Y
					18f // Vignette Color
                },
				id = LSText.randomNumString(8),
			}, // Vignette
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[6]
				{
					0f,
					0f,
					0f,
					1f,
					1f,
					1f
				},
				id = LSText.randomNumString(8),
			}, // Lens
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[3],
				id = LSText.randomNumString(8),
			}, // Grain
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[9],
				id = LSText.randomNumString(8),
			}, // ColorGrading
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[5]
				{
					0f,
					0f,
					1f,
					0f,
					0f
				},
				id = LSText.randomNumString(8),
			}, // Ripples
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2]
				{
					0f,
					6f
				},
				id = LSText.randomNumString(8),
			}, // RadialBlur
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1],
				id = LSText.randomNumString(8),
			}, // ColorSplit
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2],
				id = LSText.randomNumString(8),
			}, // Offset
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[5]
				{
					0f,
					0f,
					18f,
					18f,
					0f,
				},
				id = LSText.randomNumString(8),
			}, // Gradient
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1],
				id = LSText.randomNumString(8),
			}, // DoubleVision
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[3],
				id = LSText.randomNumString(8),
			}, // ScanLines
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2],
				id = LSText.randomNumString(8),
			}, // Blur
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1],
				id = LSText.randomNumString(8),
			}, // Pixelize
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2]
				{
					18f, // Color
					0f // Active
				},
				id = LSText.randomNumString(8),
			}, // BG
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1],
				id = LSText.randomNumString(8),
			}, // Invert
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[7]
				{
					0f,
					0f,
					-342f,
					1f,
					1f,
					0f,
					18f
				},
				id = LSText.randomNumString(8),
			}, // Timeline
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[6],
				id = LSText.randomNumString(8),
			}, // Player
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[10]
				{
					0f, // Active
					0f, // Move
					0f, // Rotate
					0.5f,
					0f,
					9999f,
					-9999f,
					9999f,
					-9999f,
					1f,
				},
				id = LSText.randomNumString(8),
			}, // Follow Player
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2]
				{
					1f,
					1f
				},
				id = LSText.randomNumString(8),
			}, // Audio
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[9]
				{
					0f, // Position X
                    0f, // Position Y
                    0f, // Position Z
                    1f, // Scale X
                    1f, // Scale Y
                    1f, // Scale Z
                    0f, // Rotation X
                    0f, // Rotation Y
                    0f, // Rotation Z
                },
				id = LSText.randomNumString(8),
			}, // Video BG Parent
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[10]
				{
					0f, // Position X
                    0f, // Position Y
                    120f, // Position Z
                    240f, // Scale X
                    135f, // Scale Y
                    1f, // Scale Z
                    0f, // Rotation X
                    0f, // Rotation Y
                    0f, // Rotation Z
                    0f, // Render Layer (Foreground / Background)
                },
				id = LSText.randomNumString(8),
			}, // Video BG
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[1]
				{
					0f, // Sharpen Amount
                },
				id = LSText.randomNumString(8),
			}, // Sharpen
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2]
				{
					0f, // Amount
					0f, // Mode
                },
				id = LSText.randomNumString(8),
			}, // Bars
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[3]
				{
					0f, // Intensity
					0f, // Size
					18f, // Color
                },
				id = LSText.randomNumString(8),
			}, // Danger
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2]
				{
					0f, // X
					0f, // Y
                },
				id = LSText.randomNumString(8),
			}, // 3D Rotation
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[2]
				{
					-10f, // Depth
					0f, // Zoom
                },
				id = LSText.randomNumString(8),
			}, // Camera Depth
		};

		public static bool SaveOpacityToThemes { get; set; } = false;

		public LevelBeatmapData LevelBeatmapData => (LevelBeatmapData)beatmapData;

		public List<Data.BeatmapObject> BeatmapObjects
		{
			get => beatmapObjects.Select(x => (Data.BeatmapObject)x).ToList();
			set
			{
				beatmapObjects.Clear();
				beatmapObjects.AddRange(value);
			}
		}

		public List<Data.BackgroundObject> BackgroundObjects
		{
			get => backgroundObjects.Select(x => (Data.BackgroundObject)x).ToList();
			set
			{
				backgroundObjects.Clear();
				backgroundObjects.AddRange(value);
			}
		}
	}
}