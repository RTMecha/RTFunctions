using LSFunctions;
using RTFunctions.Functions.Managers;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaseBackgroundObject = DataManager.GameData.BackgroundObject;
using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BaseBeatmapTheme = DataManager.BeatmapTheme;
using BaseEventKeyframe = DataManager.GameData.EventKeyframe;
using BaseGameData = DataManager.GameData;

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

		public static GameData ParseVG(JSONNode jn, bool parseThemes = true)
		{
			var gameData = new GameData();

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
					eventKeyframe.SetEventValues(kfjn["ev"][0].AsFloat);

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
			return gameData;
		}

		public static GameData Parse(JSONNode jn, bool parseThemes = true)
		{
			var gameData = new GameData();

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

		// Previously 26, now 33
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
			"vidbg_p",
			"vidbg",
			"sharp",
			"bars",
			"danger",
			"xyrot",
			"camdepth",
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
				eventValues = new float[1]
				{
					18f
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
					0f, // Z
					0f, // Perspective
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