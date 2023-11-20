using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleJSON;
using LSFunctions;

using RTFunctions.Functions;
using RTFunctions.Functions.IO;

using BaseGameData = DataManager.GameData;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using BaseEventKeyframe = DataManager.GameData.EventKeyframe;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BaseBackgroundObject = DataManager.GameData.BackgroundObject;
using BasePrefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;
using BaseBackground = DataManager.GameData.BackgroundObject;
using BaseEditorData = DataManager.GameData.BeatmapObject.EditorData;

namespace RTFunctions.Functions.Data
{
	public class GameData : BaseGameData
	{
		public GameData()
		{

		}

		public Dictionary<string, DataManager.BeatmapTheme> beatmapThemes = new Dictionary<string, DataManager.BeatmapTheme>();

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
																select new DataManager.GameData.BeatmapData.Marker
																{
																	active = false,
																	time = marker.time,
																	name = marker.name,
																	color = marker.color,
																	desc = marker.desc
																}).ToList());
			gameData.beatmapData = beatmapData;
			gameData.beatmapObjects = new List<BaseBeatmapObject>((from obj in orig.beatmapObjects
																   select RTFunctions.Functions.Data.BeatmapObject.DeepCopy((RTFunctions.Functions.Data.BeatmapObject)obj)).ToList());
			gameData.backgroundObjects = new List<BaseBackgroundObject>((from obj in orig.backgroundObjects
																		 select RTFunctions.Functions.Data.BackgroundObject.DeepCopy((RTFunctions.Functions.Data.BackgroundObject)obj)).ToList());
			gameData.eventObjects = EventObjects.DeepCopy(orig.eventObjects);
			return gameData;
		}

		public static GameData Parse(JSONNode jn)
		{
			var gameData = new GameData();
			for (int i = 0; i < jn["beatmap_objects"].Count; i++)
				gameData.beatmapObjects.Add(Data.BeatmapObject.Parse(jn["beatmap_objects"][i]));

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
				List<DataManager.BeatmapTheme> levelThemes = new List<DataManager.BeatmapTheme>();

				for (int i = 0; i < DataManager.inst.CustomBeatmapThemes.Count; i++)
				{
					var beatmapTheme = DataManager.inst.CustomBeatmapThemes[i];

					string id = beatmapTheme.id;

					foreach (var keyframe in DataManager.inst.gameData.eventObjects.allEvents[4])
					{
						var eventValue = keyframe.eventValues[0].ToString();

						if (int.TryParse(id, out int num) && (int)keyframe.eventValues[0] == num && levelThemes.Find(x => int.TryParse(x.id, out int xid) && xid == (int)keyframe.eventValues[0]) == null)
						//if (int.TryParse(id, out int num) && (int)keyframe.eventValues[0] == num && levelThemes.Find(x => x.IDToInt() == (int)keyframe.eventValues[0]) == null)
						{
							levelThemes.Add(beatmapTheme);
						}

						//if (eventValue.Length == 4 && id.Length == 6)
						//{
						//    eventValue = "00" + eventValue;
						//}
						//if (eventValue.Length == 5 && id.Length == 6)
						//{
						//    eventValue = "0" + eventValue;
						//}
						//if (beatmapTheme.id == eventValue && levelThemes.Find(x => x.id == eventValue) == null)
						//{
						//    levelThemes.Add(beatmapTheme);
						//}

						//if (beatmapTheme.id == eventValue && !savedBeatmapThemes.ContainsKey(id))
						//    savedBeatmapThemes.Add(id, beatmapTheme);
					}
				}

				for (int i = 0; i < levelThemes.Count; i++)
				{
					jn["themes"][i]["id"] = levelThemes[i].id;
					jn["themes"][i]["name"] = levelThemes[i].name;
					if (SaveOpacityToThemes)
						jn["themes"][i]["gui"] = RTHelpers.ColorToHex(levelThemes[i].guiColor);
					else
						jn["themes"][i]["gui"] = LSColors.ColorToHex(levelThemes[i].guiColor);
					jn["themes"][i]["bg"] = LSColors.ColorToHex(levelThemes[i].backgroundColor);
					for (int j = 0; j < levelThemes[i].playerColors.Count; j++)
					{
						if (SaveOpacityToThemes)
							jn["themes"][i]["players"][j] = RTHelpers.ColorToHex(levelThemes[i].playerColors[j]);
						else
							jn["themes"][i]["players"][j] = LSColors.ColorToHex(levelThemes[i].playerColors[j]);
					}
					for (int j = 0; j < levelThemes[i].objectColors.Count; j++)
					{
						if (SaveOpacityToThemes)
							jn["themes"][i]["objs"][j] = RTHelpers.ColorToHex(levelThemes[i].objectColors[j]);
						else
							jn["themes"][i]["objs"][j] = LSColors.ColorToHex(levelThemes[i].objectColors[j]);
					}
					for (int j = 0; j < levelThemes[i].backgroundColors.Count; j++)
					{
						jn["themes"][i]["bgs"][j] = LSColors.ColorToHex(levelThemes[i].backgroundColors[j]);
					}
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
