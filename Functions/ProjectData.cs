﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

using SimpleJSON;
using LSFunctions;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

using BaseEventKeyframe = DataManager.GameData.EventKeyframe;
using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BasePrefab = DataManager.GameData.Prefab;
using BaseBeatmapTheme = DataManager.BeatmapTheme;
using BaseMarker = DataManager.GameData.BeatmapData.Marker;
using BaseCheckpoint = DataManager.GameData.BeatmapData.Checkpoint;

namespace RTFunctions.Functions
{
	public class ProjectData
    {
		public static class Converter
        {
            public static void ConvertPrefabToDAE(BasePrefab prefab)
            {

            }

			public static void ConvertToLS(JSONNode jn)
            {

            }

			public static void ConvertToVG(JSONNode jn)
            {

            }
        }

        public static class Combiner
        {
            #region Settings

            public static bool prioritizeFirstEvents = true;
			public static bool prioritizeFirstThemes = true;

			public static bool addFirstMarkers = true;
			public static bool addSecondMarkers = false;

			public static bool addFirstCheckpoints = true;
			public static bool addSecondCheckpoints = false;

			public static bool objectsWithMatchingIDAddKeyframes = false;

			#endregion

			//ProjectData.Combiner.Combine(RTFile.ApplicationDirectory + "beatmaps/editor/Classic Arrhythmia/Combine 1/level.lsb", RTFile.ApplicationDirectory + "beatmaps/editor/Classic Arrhythmia/Combine 2/level.lsb", RTFile.ApplicationDirectory + "beatmaps/editor/Classic Arrhythmia/Combined/level.lsb");
			public static void Combine(string path1, string path2, string saveTo)
			{
				if (!RTFile.FileExists(path1) || !RTFile.FileExists(path2))
					return;

				var directory = Path.GetDirectoryName(saveTo);
				if (!RTFile.DirectoryExists(directory))
					Directory.CreateDirectory(directory);

				//if (!RTFile.FileExists(saveTo))
				//	File.Create(saveTo);

				if (RTFile.FileExists(Path.GetDirectoryName(path1) + "/level.ogg") && !RTFile.FileExists(directory + "/level.ogg"))
					File.Copy(Path.GetDirectoryName(path1) + "/level.ogg", directory + "/level.ogg");
				if (RTFile.FileExists(Path.GetDirectoryName(path1) + "/level.jpg") && !RTFile.FileExists(directory + "/level.jpg"))
					File.Copy(Path.GetDirectoryName(path1) + "/level.jpg", directory + "/level.jpg");
				if (RTFile.FileExists(Path.GetDirectoryName(path1) + "/metadata.lsb") && !RTFile.FileExists(directory + "/metadata.lsb"))
					File.Copy(Path.GetDirectoryName(path1) + "/metadata.lsb", directory + "/metadata.lsb");

				FunctionsPlugin.inst.StartCoroutine(Writer.SaveData(saveTo, Combine(path1, path2)));
			}

			public static GameData Combine(string path1, string path2)
			{
				if (!RTFile.FileExists(path1) || !RTFile.FileExists(path2))
					return null;

				return Combine(JSON.Parse(FileManager.inst.LoadJSONFileRaw(path1)), JSON.Parse(FileManager.inst.LoadJSONFileRaw(path2)));
			}

			public static GameData Combine(JSONNode jn, JSONNode jn32)
            {
                var gameData = new GameData();

				#region Markers

				if (addFirstMarkers)
					for (int i = 0; i < jn["markers"].Count; i++)
						gameData.beatmapData.markers.Add(Reader.ParseMarker(jn["markers"][i]));
				
				if (addSecondMarkers)
					for (int i = 0; i < jn32["markers"].Count; i++)
						gameData.beatmapData.markers.Add(Reader.ParseMarker(jn32["markers"][i]));

				gameData.beatmapData.markers = gameData.beatmapData.markers.OrderBy(x => x.time).ToList();

                #endregion

                #region Checkpoints

				if (addFirstCheckpoints)
					for (int i = 0; i < jn["checkpoints"].Count; i++)
						gameData.beatmapData.checkpoints.Add(Reader.ParseCheckpoint(jn["checkpoints"][i]));

				if (addSecondCheckpoints)
					for (int i = 0; i < jn32["checkpoints"].Count; i++)
					{
						var checkpoint = Reader.ParseCheckpoint(jn32["checkpoints"][i]);
						if (gameData.beatmapData.checkpoints.Find(x => x.time == checkpoint.time) == null)
							gameData.beatmapData.checkpoints.Add(checkpoint);
					}

				gameData.beatmapData.checkpoints = gameData.beatmapData.checkpoints.OrderBy(x => x.time).ToList();

                #endregion

                #region Prefabs

				for (int i = 0; i < jn["prefabs"].Count; i++)
                {
					var prefab = Prefab.Parse(jn["prefabs"][i]);
					if (gameData.prefabs.Find(x => x.ID == prefab.ID) == null)
						gameData.prefabs.Add(prefab);
                }
				
				for (int i = 0; i < jn32["prefabs"].Count; i++)
                {
					var prefab = Prefab.Parse(jn32["prefabs"][i]);
					if (gameData.prefabs.Find(x => x.ID == prefab.ID) == null)
						gameData.prefabs.Add(prefab);
                }

                #endregion
				
                #region PrefabObjects

				for (int i = 0; i < jn["prefab_objects"].Count; i++)
                {
					var prefab = PrefabObject.Parse(jn["prefab_objects"][i]);
					if (gameData.prefabObjects.Find(x => x.ID == prefab.ID) == null)
						gameData.prefabObjects.Add(prefab);
                }
				
				for (int i = 0; i < jn32["prefab_objects"].Count; i++)
                {
					var prefab = PrefabObject.Parse(jn32["prefabs"][i]);
					if (gameData.prefabObjects.Find(x => x.ID == prefab.ID) == null)
						gameData.prefabObjects.Add(prefab);
                }

                #endregion

                #region Themes

                for (int i = 0; i < jn["themes"].Count; i++)
					if (!gameData.beatmapThemes.ContainsKey(jn["themes"][i]["id"]))
						gameData.beatmapThemes.Add(jn["themes"][i]["id"], Reader.ParseBeatmapTheme(jn["themes"][i], FileType.LS));

				for (int i = 0; i < jn32["themes"].Count; i++)
					if (!gameData.beatmapThemes.ContainsKey(jn32["themes"][i]["id"]))
						gameData.beatmapThemes.Add(jn32["themes"][i]["id"], Reader.ParseBeatmapTheme(jn32["themes"][i], FileType.LS));

                #endregion

                #region Objects

                for (int i = 0; i < jn["beatmap_objects"].Count; i++)
					gameData.beatmapObjects.Add(BeatmapObject.Parse(jn["beatmap_objects"][i]));

				for (int i = 0; i < jn32["beatmap_objects"].Count; i++)
				{
					var beatmapObject = BeatmapObject.Parse(jn32["beatmap_objects"][i]);

					if (!objectsWithMatchingIDAddKeyframes)
						gameData.beatmapObjects.Add(beatmapObject);
					else if (gameData.beatmapObjects.TryFind(x => x.id == beatmapObject.id, out BaseBeatmapObject modObject))
                    {
						for (int j = 0; j < modObject.events.Count; j++)
                        {
							beatmapObject.events[j].RemoveAt(0);
							modObject.events[j].AddRange(beatmapObject.events[j]);
						}
                    }
					else
						gameData.beatmapObjects.Add(beatmapObject);
				}

				#endregion

				#region Backgrounds

				for (int i = 0; i < jn["bg_objects"].Count; i++)
					gameData.backgroundObjects.Add(BackgroundObject.Parse(jn["bg_objects"][i]));
				
				for (int i = 0; i < jn32["bg_objects"].Count; i++)
					gameData.backgroundObjects.Add(BackgroundObject.Parse(jn32["bg_objects"][i]));

                #endregion

                #region Events

                gameData.eventObjects.allEvents = new List<List<BaseEventKeyframe>>();

				var l = Reader.ParseEventkeyframes(jn["events"]);
				var l32 = Reader.ParseEventkeyframes(jn32["events"]);

				for (int i = 0; i < l.Count; i++)
                {
					if (!prioritizeFirstEvents)
						l[i].RemoveAt(0);

					gameData.eventObjects.allEvents.Add(l[i]);
				}

				for (int i = 0; i < l32.Count; i++)
                {
					if (prioritizeFirstEvents)
						l32[i].RemoveAt(0);

					gameData.eventObjects.allEvents[i].AddRange(l32[i]);
				}

                //foreach (var kflist in gameData.eventObjects.allEvents)
                //{
                //    kflist.OrderBy(x => x.eventTime);
                //}

                #endregion

                return gameData;
			}
        }

		public static class Reader
		{
			public static BaseEventKeyframe ParseVGKeyframe(JSONNode jn)
            {
				var keyframe = new BaseEventKeyframe();

				float[] ev = new float[jn["ev"].Count];

				for (int k = 0; k < jn["ev"].Count; k++)
				{
					ev[k] = jn["ev"].AsFloat;
				}

				keyframe.eventValues = ev;

				float[] er = new float[jn["er"].Count];

				for (int k = 0; k < jn["er"].Count; k++)
				{
					er[k] = jn["er"].AsFloat;
				}

				keyframe.eventRandomValues = er;

				keyframe.eventTime = jn["t"] != null ? jn["t"].AsFloat : 0f;

				if (jn["ct"] != null && DataManager.inst.AnimationListDictionaryStr.ContainsKey(jn["ct"]))
					keyframe.curveType = DataManager.inst.AnimationListDictionaryStr[jn["ct"]];

				return keyframe;
			}

			public static BeatmapTheme ParseBeatmapTheme(JSONNode jn, FileType fileType) => fileType == FileType.LS ? BeatmapTheme.Parse(jn) : ParseVGBeatmapTheme(jn);

			public static BeatmapTheme ParseVGBeatmapTheme(JSONNode jn)
            {
				var beatmapTheme = new BeatmapTheme();

				beatmapTheme.id = jn["id"] != null ? jn["id"] : LSText.randomNumString(6);
				beatmapTheme.name = jn["name"] != null ? jn["name"] : "name your themes!";

				beatmapTheme.playerColors = BeatmapTheme.SetColors(jn["pla"], 4, "Uh oh, something went wrong with converting VG to LS!");
				beatmapTheme.objectColors = BeatmapTheme.SetColors(jn["obj"], 18, "Uh oh, something went wrong with converting VG to LS!");
				beatmapTheme.effectColors = BeatmapTheme.SetColors(jn["fx"], 18, "Uh oh, something went wrong with converting VG to LS!");
				beatmapTheme.backgroundColors = BeatmapTheme.SetColors(jn["fx"], 18, "Uh oh, something went wrong with converting VG to LS!");

				beatmapTheme.backgroundColor = jn["base_bg"] != null ? LSColors.HexToColor(jn["base_bg"]) : LSColors.gray100;
				beatmapTheme.guiColor = jn["base_gui"] != null ? LSColors.HexToColor(jn["base_gui"]) : LSColors.gray800;
				beatmapTheme.guiAccentColor = jn["base_gui_accent"] != null ? LSColors.HexToColor(jn["base_gui_accent"]) : LSColors.gray800;

				return beatmapTheme;
            }

			public static Prefab ParseVGPrefab(JSONNode jn)
            {
				var prefab = new Prefab();

				return prefab;
            }

			public static BeatmapObject ParseVGBeatmapObject(JSONNode jn)
            {
				var beatmapObject = new BeatmapObject();

				return beatmapObject;
            }

			public static List<List<BaseEventKeyframe>> ParseEventkeyframes(JSONNode jn, bool orderTime = false)
			{
				var allEvents = new List<List<BaseEventKeyframe>>();

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["pos"].Count; i++)
					allEvents[0].Add(EventKeyframe.Parse(jn["pos"][i], 2));

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["zoom"].Count; i++)
					allEvents[1].Add(EventKeyframe.Parse(jn["zoom"][i], 1));

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["rot"].Count; i++)
					allEvents[2].Add(EventKeyframe.Parse(jn["rot"][i], 1));

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["shake"].Count; i++)
				{
					var eventKeyframe = EventKeyframe.Parse(jn["shake"][i], 3);
					if (string.IsNullOrEmpty(jn["shake"][i]["y"]) || string.IsNullOrEmpty(jn["shake"][i]["z"]))
					{
						eventKeyframe.SetEventValues(new float[]
						{
							jn["shake"][i]["x"].AsFloat,
							1f,
							1f,
						});
					}
					allEvents[3].Add(eventKeyframe);
				}

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["theme"].Count; i++)
					allEvents[4].Add(EventKeyframe.Parse(jn["theme"][i], 1));

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["chroma"].Count; i++)
					allEvents[5].Add(EventKeyframe.Parse(jn["chroma"][i], 1));

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["bloom"].Count; i++)
				{
					var eventKeyframe = EventKeyframe.Parse(jn["bloom"][i], 5);
					if (string.IsNullOrEmpty(jn["bloom"][i]["y"]))
					{
						eventKeyframe.SetEventValues(new float[]
						{
							jn["bloom"][i]["x"].AsFloat,
							7f,
							1f,
							0f,
							18f
						});
					}
					allEvents[6].Add(eventKeyframe);
				}

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["vignette"].Count; i++)
				{
					var eventKeyframe = EventKeyframe.Parse(jn["vignette"][i], 7);
					if (string.IsNullOrEmpty(jn["vignette"][i]["x3"]))
					{
						eventKeyframe.SetEventValues(new float[]
						{
							jn["vignette"][i]["x"].AsFloat,
							jn["vignette"][i]["y"].AsFloat,
							jn["vignette"][i]["z"].AsFloat,
							jn["vignette"][i]["x2"].AsFloat,
							jn["vignette"][i]["y2"].AsFloat,
							jn["vignette"][i]["z2"].AsFloat,
							18f
						});
					}
					allEvents[7].Add(eventKeyframe);
				}

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["lens"].Count; i++)
				{
					var eventKeyframe = EventKeyframe.Parse(jn["lens"][i], 6);
					if (string.IsNullOrEmpty(jn["lens"][i]["y"]))
					{
						eventKeyframe.SetEventValues(new float[]
						{
							jn["lens"][i]["x"].AsFloat,
							0f,
							0f,
							1f,
							1f,
							1f
						});
					}
					allEvents[8].Add(eventKeyframe);
				}

				allEvents.Add(new List<BaseEventKeyframe>());
				for (int i = 0; i < jn["grain"].Count; i++)
					allEvents[9].Add(EventKeyframe.Parse(jn["grain"][i], 3));

				if (ModCompatibility.mods.ContainsKey("EventsCore"))
				{
					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["cg"] != null)
						for (int i = 0; i < jn["cg"].Count; i++)
							allEvents[10].Add(EventKeyframe.Parse(jn["cg"][i], 9));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["rip"] != null)
						for (int i = 0; i < jn["rip"].Count; i++)
							allEvents[11].Add(EventKeyframe.Parse(jn["rip"][i], 5));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["rb"] != null)
						for (int i = 0; i < jn["rb"].Count; i++)
						allEvents[12].Add(EventKeyframe.Parse(jn["rb"][i], 2));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["cs"] != null)
						for (int i = 0; i < jn["cs"].Count; i++)
						allEvents[13].Add(EventKeyframe.Parse(jn["cs"][i], 1));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["offset"] != null)
						for (int i = 0; i < jn["offset"].Count; i++)
						allEvents[14].Add(EventKeyframe.Parse(jn["offset"][i], 2));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["grd"] != null)
						for (int i = 0; i < jn["grd"].Count; i++)
						allEvents[15].Add(EventKeyframe.Parse(jn["grd"][i], 5));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["dbv"] != null)
						for (int i = 0; i < jn["dbv"].Count; i++)
						allEvents[16].Add(EventKeyframe.Parse(jn["dbv"][i], 1));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["scan"] != null)
						for (int i = 0; i < jn["scan"].Count; i++)
						allEvents[17].Add(EventKeyframe.Parse(jn["scan"][i], 3));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["blur"] != null)
						for (int i = 0; i < jn["blur"].Count; i++)
						allEvents[18].Add(EventKeyframe.Parse(jn["blur"][i], 2));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["pixel"] != null)
						for (int i = 0; i < jn["pixel"].Count; i++)
						allEvents[19].Add(EventKeyframe.Parse(jn["pixel"][i], 1));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["bg"] != null)
						for (int i = 0; i < jn["bg"].Count; i++)
						allEvents[20].Add(EventKeyframe.Parse(jn["bg"][i], 1));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["invert"] != null)
						for (int i = 0; i < jn["invert"].Count; i++)
                            allEvents[21].Add(EventKeyframe.Parse(jn["invert"][i], 1));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["timeline"] != null)
						for (int i = 0; i < jn["timeline"].Count; i++)
						allEvents[22].Add(EventKeyframe.Parse(jn["timeline"][i], 7));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["player"] != null)
						for (int i = 0; i < jn["player"].Count; i++)
						allEvents[23].Add(EventKeyframe.Parse(jn["player"][i], 4));

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["follow_player"] != null)
						for (int i = 0; i < jn["follow_player"].Count; i++)
						{
							var eventKeyframe = EventKeyframe.Parse(jn["follow_player"][i], 10);
							if (string.IsNullOrEmpty(jn["follow_player"][i]["z2"]))
							{
								eventKeyframe.SetEventValues(new float[]
								{
									jn["follow_player"][i]["x"].AsFloat,
									jn["follow_player"][i]["y"].AsFloat,
									jn["follow_player"][i]["z"].AsFloat,
									jn["follow_player"][i]["x2"].AsFloat,
									jn["follow_player"][i]["y2"].AsFloat,
									9999f,
									-9999f,
									9999f,
									-9999f,
									1f
								});
							}
							allEvents[24].Add(eventKeyframe);
						}

					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn["audio"] != null)
						for (int i = 0; i < jn["audio"].Count; i++)
							allEvents[25].Add(EventKeyframe.Parse(jn["audio"][i], 2));
				}

				ClampEventListValues(allEvents, ModCompatibility.mods.ContainsKey("EventsCore") ? 26 : 10);

				if (orderTime)
					allEvents.ForEach(x => x = x.OrderBy(x => x.eventTime).ToList());

				return allEvents;
            }

			public static void ClampEventListValues(List<List<BaseEventKeyframe>> eventKeyframes, int totalTypes)
            {
				for (int type = 0; type < totalTypes; type++)
                {
					//Debug.Log($"{FunctionsPlugin.className}EventKeyframes Count: {eventKeyframes.Count}\nType: {type}");
					if (eventKeyframes.Count < type + 1)
						eventKeyframes.Add(new List<BaseEventKeyframe>());

					if (eventKeyframes[type].Count < 1)
						eventKeyframes[type].Add(EventKeyframe.DeepCopy((EventKeyframe)GameData.DefaultKeyframes[type]));

					for (int index = 0; index < eventKeyframes[type].Count; index++)
                    {
						var array = (float[])eventKeyframes[type][index].eventValues.Clone();
						if (array.Length != GameData.DefaultKeyframes[type].eventValues.Length)
                        {
							array = new float[GameData.DefaultKeyframes[type].eventValues.Length];
							for (int i = 0; i < GameData.DefaultKeyframes[type].eventValues.Length; i++)
								array[i] = i < eventKeyframes[type][index].eventValues.Length ? eventKeyframes[type][index].eventValues[i] : GameData.DefaultKeyframes[type].eventValues[i];
                        }
						eventKeyframes[type][index].eventValues = array;
                    }
                }
            }

			public static BaseMarker ParseMarker(JSONNode jn)
            {
                try
				{
					bool active = jn["active"].AsBool;

					string name = jn["name"] != null ? jn["name"] : "Marker";

					string desc = jn["desc"] != null ? jn["desc"] : "";
					float time = jn["t"].AsFloat;

					int color = jn["col"] != null ? jn["col"].AsInt : 0;

					return new BaseMarker(active, name, desc, color, time);
				}
                catch
                {
					return new BaseMarker(true, "Marker", "", 0, 0f);
                }
			}

			public static BaseCheckpoint ParseCheckpoint(JSONNode jn)
            {
				bool active = jn["active"].AsBool;
				string name = jn["name"];
				Vector2 pos = new Vector2(jn["pos"]["x"].AsFloat, jn["pos"]["y"].AsFloat);
				float time = jn["t"].AsFloat;

				return new BaseCheckpoint(active, name, time, pos);
			}
        }

		public static class Writer
        {
			public static Action onSave;
			public static Action addedOnSave = delegate ()
			{
				// Empty delegate to add
			};

			public static IEnumerator SaveData(string _path, GameData _data)
			{
				Debug.Log("Saving Beatmap");
				var jn = JSON.Parse("{}");

				Debug.Log($"{FunctionsPlugin.className}Saving Editor Data");
				jn["ed"]["timeline_pos"] = "0";

				Debug.Log($"{FunctionsPlugin.className}Saving Markers");
				for (int i = 0; i < _data.beatmapData.markers.Count; i++)
				{
					jn["ed"]["markers"][i]["active"] = "True";
					jn["ed"]["markers"][i]["name"] = _data.beatmapData.markers[i].name.ToString();
					jn["ed"]["markers"][i]["desc"] = _data.beatmapData.markers[i].desc.ToString();
					jn["ed"]["markers"][i]["col"] = _data.beatmapData.markers[i].color.ToString();
					jn["ed"]["markers"][i]["t"] = _data.beatmapData.markers[i].time.ToString();
				}

				Debug.Log($"{FunctionsPlugin.className}Saving Object Prefabs");
				for (int i = 0; i < _data.prefabObjects.Count; i++)
				{
					jn["prefab_objects"][i] = ((PrefabObject)_data.prefabObjects[i]).ToJSON();
				}

                Debug.Log($"{FunctionsPlugin.className}Saving Level Data");
                {
					jn["level_data"]["mod_version"] = FunctionsPlugin.CurrentVersion.ToString();
                    jn["level_data"]["level_version"] = "4.1.16";
                    jn["level_data"]["background_color"] = "0";
                    jn["level_data"]["follow_player"] = "False";
                    jn["level_data"]["show_intro"] = "False";
                    jn["level_data"]["bg_zoom"] = "1";
                }
                Debug.Log($"{FunctionsPlugin.className}Saving prefabs");
                if (_data.prefabs != null)
                {
                    for (int i = 0; i < _data.prefabs.Count; i++)
                    {
						jn["prefabs"][i] = ((Prefab)_data.prefabs[i]).ToJSON();
					}
                }
                Debug.Log($"Saving themes");
				if (_data.beatmapThemes != null)
				{
					var levelThemes = new List<BaseBeatmapTheme>();

					for (int i = 0; i < _data.beatmapThemes.Count; i++)
					{
						var beatmapTheme = _data.beatmapThemes.ElementAt(i).Value;

						string id = beatmapTheme.id;

						foreach (var keyframe in _data.eventObjects.allEvents[4])
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
						Debug.LogFormat("{0}Saving " + levelThemes[i].id + " - " + levelThemes[i].name + " to level!", FunctionsPlugin.className);
						jn["themes"][i] = ((BeatmapTheme)levelThemes[i]).ToJSON();
						//jn["themes"][i]["id"] = levelThemes[i].id;
						//jn["themes"][i]["name"] = levelThemes[i].name;
						////if (ConfigEntries.SaveOpacityToThemes.Value)
						//	jn["themes"][i]["gui"] = RTHelpers.ColorToHex(levelThemes[i].guiColor);
						////else
						////	jn["themes"][i]["gui"] = LSColors.ColorToHex(levelThemes[i].guiColor);
						//jn["themes"][i]["bg"] = LSColors.ColorToHex(levelThemes[i].backgroundColor);
						//for (int j = 0; j < levelThemes[i].playerColors.Count; j++)
						//{
						//	//if (ConfigEntries.SaveOpacityToThemes.Value)
						//		jn["themes"][i]["players"][j] = RTHelpers.ColorToHex(levelThemes[i].playerColors[j]);
						//	//else
						//	//	jn["themes"][i]["players"][j] = LSColors.ColorToHex(levelThemes[i].playerColors[j]);
						//}
						//for (int j = 0; j < levelThemes[i].objectColors.Count; j++)
						//{
						//	//if (ConfigEntries.SaveOpacityToThemes.Value)
						//		jn["themes"][i]["objs"][j] = RTHelpers.ColorToHex(levelThemes[i].objectColors[j]);
						//	//else
						//	//	jn["themes"][i]["objs"][j] = LSColors.ColorToHex(levelThemes[i].objectColors[j]);
						//}
						//for (int j = 0; j < levelThemes[i].backgroundColors.Count; j++)
						//{
						//	jn["themes"][i]["bgs"][j] = LSColors.ColorToHex(levelThemes[i].backgroundColors[j]);
						//}
					}
				}

				Debug.Log($"{FunctionsPlugin.className}Saving Checkpoints");
				for (int i = 0; i < _data.beatmapData.checkpoints.Count; i++)
				{
					jn["checkpoints"][i]["active"] = "False";
					jn["checkpoints"][i]["name"] = _data.beatmapData.checkpoints[i].name;
					jn["checkpoints"][i]["t"] = _data.beatmapData.checkpoints[i].time.ToString();
					jn["checkpoints"][i]["pos"]["x"] = _data.beatmapData.checkpoints[i].pos.x.ToString();
					jn["checkpoints"][i]["pos"]["y"] = _data.beatmapData.checkpoints[i].pos.y.ToString();
				}

				Debug.Log($"{FunctionsPlugin.className}Saving Beatmap Objects");
				if (_data.beatmapObjects != null)
				{
					List<BaseBeatmapObject> list = _data.beatmapObjects.FindAll(x => !x.fromPrefab);
					jn["beatmap_objects"] = new JSONArray();
					for (int i = 0; i < list.Count; i++)
                    {
						jn["beatmap_objects"][i] = ((BeatmapObject)list[i]).ToJSON();
                    }
				}
				else
				{
					Debug.Log("skipping objects");
					jn["beatmap_objects"] = new JSONArray();
				}

				Debug.Log($"{FunctionsPlugin.className}Saving Background Objects");
				for (int i = 0; i < _data.backgroundObjects.Count; i++)
				{
					jn["bg_objects"][i] = ((BackgroundObject)_data.backgroundObjects[i]).ToJSON();
				}

				Debug.Log($"{FunctionsPlugin.className}Saving Event Objects");
				{
					for (int i = 0; i < _data.eventObjects.allEvents.Count; i++)
                    {
						for (int j = 0; j < _data.eventObjects.allEvents[i].Count; j++)
                        {
							if (GameData.EventTypes.Length > i)
							{
								//Debug.Log($"{FunctionsPlugin.className}Saving keyframe: {((EventKeyframe)_data.eventObjects.allEvents[i][j])}");
								jn["events"][GameData.EventTypes[i]][j] = ((EventKeyframe)_data.eventObjects.allEvents[i][j]).ToJSON();
							}
                        }
                    }
				}

				Debug.Log($"{FunctionsPlugin.className}Saving Entire Beatmap to {_path}");
				RTFile.WriteToFile(_path, jn.ToString());

                onSave?.Invoke();

                //yield return new WaitForSeconds(0.5f);

                //if (GameObject.Find("BepInEx_Manager").GetComponentByName("PlayerPlugin"))
                //{
                //	var playerPlugin = GameObject.Find("BepInEx_Manager").GetComponentByName("PlayerPlugin");
                //	var c = playerPlugin.GetType().GetField("className").GetValue(playerPlugin);

                //	if (c != null)
                //	{
                //		playerPlugin.GetType().GetMethod("SaveLocalModels").Invoke(playerPlugin, new object[] { });
                //	}
                //}

                yield break;
			}
		}
    }
}
