using System.Collections.Generic;
using System.Linq;

using SimpleJSON;
using LSFunctions;

using RTFunctions.Functions.IO;

using BaseGameData = DataManager.GameData;

using BaseEventKeyframe = DataManager.GameData.EventKeyframe;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BaseBackgroundObject = DataManager.GameData.BackgroundObject;
using BaseBeatmapTheme = DataManager.BeatmapTheme;

namespace RTFunctions.Functions.Data
{
	public class GameData : BaseGameData
	{
		public GameData()
		{

		}

		public static GameData Current => (GameData)DataManager.inst.gameData;

		public Dictionary<string, BaseBeatmapTheme> beatmapThemes = new Dictionary<string, BaseBeatmapTheme>();

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
			beatmapData.checkpoints = new List<BeatmapData.Checkpoint>((from checkpoint in orig.beatmapData.checkpoints
																		select new BeatmapData.Checkpoint
																		{
																			active = false,
																			name = checkpoint.name,
																			pos = checkpoint.pos,
																			time = checkpoint.time
																		}).ToList());
			beatmapData.markers = new List<BeatmapData.Marker>((from marker in orig.beatmapData.markers
																select new BeatmapData.Marker
																{
																	active = false,
																	time = marker.time,
																	name = marker.name,
																	color = marker.color,
																	desc = marker.desc
																}).ToList());
			gameData.beatmapData = beatmapData;
			gameData.beatmapObjects = new List<BaseBeatmapObject>((from obj in orig.beatmapObjects
																   select RTFunctions.Functions.Data.BeatmapObject.DeepCopy((Data.BeatmapObject)obj)).ToList());
			gameData.backgroundObjects = new List<BaseBackgroundObject>((from obj in orig.backgroundObjects
																		 select RTFunctions.Functions.Data.BackgroundObject.DeepCopy((Data.BackgroundObject)obj)).ToList());
			gameData.eventObjects = EventObjects.DeepCopy(orig.eventObjects);
			return gameData;
		}

		public static GameData Parse(JSONNode jn, bool parseThemes = true)
		{
			var gameData = new GameData();

			gameData.beatmapData = LevelBeatmapData.Parse(jn);

			UnityEngine.Debug.Log($"{DataManager.inst.className}Parsing Markers...");
			#region Markers

			//if (gameData.beatmapData.markers == null)
			//	gameData.beatmapData.markers = new List<BeatmapData.Marker>();

			//for (int i = 0; i < jn["ed"]["markers"].Count; i++)
			//	gameData.beatmapData.markers.Add(ProjectData.Reader.ParseMarker(jn["ed"]["markers"][i]));

			gameData.beatmapData.markers = gameData.beatmapData.markers.OrderBy(x => x.time).ToList();

			#endregion

			UnityEngine.Debug.Log($"{DataManager.inst.className}Parsing Checkpoints...");
			#region Checkpoints

			for (int i = 0; i < jn["checkpoints"].Count; i++)
				gameData.beatmapData.checkpoints.Add(ProjectData.Reader.ParseCheckpoint(jn["checkpoints"][i]));

			gameData.beatmapData.checkpoints = gameData.beatmapData.checkpoints.OrderBy(x => x.time).ToList();

			#endregion

			UnityEngine.Debug.Log($"{DataManager.inst.className}Parsing Prefabs...");
			#region Prefabs

			for (int i = 0; i < jn["prefabs"].Count; i++)
			{
				var prefab = Data.Prefab.Parse(jn["prefabs"][i]);
				if (gameData.prefabs.Find(x => x.ID == prefab.ID) == null)
					gameData.prefabs.Add(prefab);
			}

			#endregion

			UnityEngine.Debug.Log($"{DataManager.inst.className}Parsing PrefabObjects...");
			#region PrefabObjects

			for (int i = 0; i < jn["prefab_objects"].Count; i++)
			{
				var prefab = Data.PrefabObject.Parse(jn["prefab_objects"][i]);
				if (gameData.prefabObjects.Find(x => x.ID == prefab.ID) == null)
					gameData.prefabObjects.Add(prefab);
			}

			#endregion

			UnityEngine.Debug.Log($"{DataManager.inst.className}Parsing BeatmapThemes...");
			#region Themes

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

			#endregion

			UnityEngine.Debug.Log($"{DataManager.inst.className}Parsing BeatmapObjects...");
			#region Objects

			for (int i = 0; i < jn["beatmap_objects"].Count; i++)
				gameData.beatmapObjects.Add(Data.BeatmapObject.Parse(jn["beatmap_objects"][i]));

			#endregion

			UnityEngine.Debug.Log($"{DataManager.inst.className}Parsing BackgroundObjects...");
			#region Backgrounds

			for (int i = 0; i < jn["bg_objects"].Count; i++)
				gameData.backgroundObjects.Add(Data.BackgroundObject.Parse(jn["bg_objects"][i]));

			#endregion

			UnityEngine.Debug.Log($"{DataManager.inst.className}Parsing Events...");
			#region Events

			gameData.eventObjects.allEvents = ProjectData.Reader.ParseEventkeyframes(jn["events"], true);

			#endregion

			return gameData;
		}

		public JSONNode ToJSON()
		{
			var jn = JSON.Parse("{}");

			var _data = this;

			jn["ed"]["timeline_pos"] = AudioManager.inst.CurrentAudioSource.time.ToString();
			for (int i = 0; i < _data.beatmapData.markers.Count; i++)
			{
				//jn["ed"]["markers"][i]["active"] = "True";
				jn["ed"]["markers"][i]["name"] = _data.beatmapData.markers[i].name.ToString();
				jn["ed"]["markers"][i]["desc"] = _data.beatmapData.markers[i].desc.ToString();
				jn["ed"]["markers"][i]["col"] = _data.beatmapData.markers[i].color.ToString();
				jn["ed"]["markers"][i]["t"] = _data.beatmapData.markers[i].time.ToString();
			}

			for (int i = 0; i < prefabObjects.Count; i++)
				jn["prefab_objects"][i] = ((Data.PrefabObject)prefabObjects[i]).ToJSON();

			jn["level_data"]["level_version"] = _data.beatmapData.levelData.levelVersion.ToString();
			jn["level_data"]["background_color"] = _data.beatmapData.levelData.backgroundColor.ToString();
			jn["level_data"]["follow_player"] = _data.beatmapData.levelData.followPlayer.ToString();
			jn["level_data"]["show_intro"] = _data.beatmapData.levelData.showIntro.ToString();
			jn["level_data"]["bg_zoom"] = RTHelpers.perspectiveZoom.ToString();

			for (int i = 0; i < prefabs.Count; i++)
				jn["prefabs"][i] = ((Data.Prefab)prefabs[i]).ToJSON();
			if (DataManager.inst.CustomBeatmapThemes != null)
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

			for (int i = 0; i < _data.beatmapData.checkpoints.Count; i++)
			{
				jn["checkpoints"][i]["active"] = "False";
				jn["checkpoints"][i]["name"] = _data.beatmapData.checkpoints[i].name;
				jn["checkpoints"][i]["t"] = _data.beatmapData.checkpoints[i].time.ToString();
				jn["checkpoints"][i]["pos"]["x"] = _data.beatmapData.checkpoints[i].pos.x.ToString();
				jn["checkpoints"][i]["pos"]["y"] = _data.beatmapData.checkpoints[i].pos.y.ToString();
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

        public static string[] EventTypes => new string[]
		{
			"pos",
			"zoom",
			"rot",
			"shake",
			"theme",
			"chroma",
			"bloom",
			"vignette",
			"lens",
			"grain",
			"cg",
			"rip",
			"rb",
			"cs",
			"offset",
			"grd",
			"dbv",
			"scan",
			"blur",
			"pixel",
			"bg",
			"invert",
			"timeline",
			"player",
			"follow_player",
			"audio",
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
				eventValues = new float[3]
                {
					0f,
					1f,
					1f
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
					0f,
					7f,
					1f,
					0f,
					18f
				},
				id = LSText.randomNumString(8),
			}, // Bloom
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[7]
                {
					0f,
					0f,
					0f,
					0f,
					0f,
					0f,
					18f,
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
				eventValues = new float[1] { 18f },
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
				eventValues = new float[5],
				id = LSText.randomNumString(8),
			}, // Player
			new Data.EventKeyframe
			{
				eventTime = 0f,
				eventValues = new float[10]
                {
					0f,
					0f,
					0f,
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
		};

        public static bool SaveOpacityToThemes { get; set; } = false;

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
