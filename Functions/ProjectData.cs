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

			/// <summary>
			/// Combines multiple GameDatas together.
			/// </summary>
			/// <param name="gameDatas">Array of GameData to combine together.</param>
			/// <returns>Combined GameData.</returns>
			public static GameData Combine(params GameData[] gameDatas)
            {
				var baseData = new GameData();
				baseData.beatmapData = new LevelBeatmapData();
				baseData.beatmapData.editorData = new LevelEditorData();
				baseData.beatmapData.levelData = new DataManager.GameData.BeatmapData.LevelData();

				if (gameDatas != null && gameDatas.Length > 0)
					for (int i = 0; i < gameDatas.Length; i++)
					{
						if (gameDatas[i].beatmapData != null && baseData.beatmapData != null)
                        {
							if (baseData.beatmapData.checkpoints == null)
								baseData.beatmapData.checkpoints = new List<BaseCheckpoint>();
							if (baseData.beatmapData.markers == null)
								baseData.beatmapData.markers = new List<BaseMarker>();

							baseData.beatmapData.checkpoints.AddRange(gameDatas[i].beatmapData.checkpoints.Where(x => !baseData.beatmapData.checkpoints.Has(y => y.time == x.time)));
							baseData.beatmapData.markers.AddRange(gameDatas[i].beatmapData.markers.Where(x => !baseData.beatmapData.markers.Has(y => y.time == x.time)));
						}

						if (baseData.beatmapObjects == null)
							baseData.beatmapObjects = new List<BaseBeatmapObject>();

						baseData.beatmapObjects.AddRange(gameDatas[i].BeatmapObjects.Where(x => !baseData.BeatmapObjects.Has(y => y.id == x.id)));

						if (baseData.prefabObjects == null)
							baseData.prefabObjects = new List<DataManager.GameData.PrefabObject>();

						baseData.prefabObjects.AddRange(gameDatas[i].prefabObjects.Where(x => !baseData.prefabObjects.Has(y => y.ID == x.ID)));

						if (baseData.prefabs == null)
							baseData.prefabs = new List<BasePrefab>();

						baseData.prefabs.AddRange(gameDatas[i].prefabs.Where(x => !baseData.prefabs.Has(y => y.ID == x.ID)));

						baseData.backgroundObjects.AddRange(gameDatas[i].BackgroundObjects.Where(x => !baseData.BackgroundObjects.Has(y =>
						{
							return y.active == x.active &&
									y.color == x.color &&
									y.depth == x.depth &&
									y.drawFade == x.drawFade &&
									y.FadeColor == x.FadeColor &&
									y.layer == x.layer &&
									y.name == x.name &&
									y.pos == x.pos &&
									y.reactive == x.reactive &&
									y.reactiveCol == x.reactiveCol &&
									y.reactiveColIntensity == x.reactiveColIntensity &&
									y.reactiveColSample == x.reactiveColSample &&
									y.reactiveIncludesZ == x.reactiveIncludesZ &&
									y.reactivePosIntensity == x.reactivePosIntensity &&
									y.reactivePosSamples == x.reactivePosSamples &&
									y.reactiveRotIntensity == x.reactiveRotIntensity &&
									y.reactiveRotSample == x.reactiveRotSample &&
									y.reactiveScaIntensity == x.reactiveScaIntensity &&
									y.reactiveScale == x.reactiveScale &&
									y.reactiveScaSamples == x.reactiveScaSamples &&
									y.reactiveSize == x.reactiveSize &&
									y.reactiveType == x.reactiveType &&
									y.reactiveZIntensity == x.reactiveZIntensity &&
									y.reactiveZSample == x.reactiveZSample &&
									y.rot == x.rot &&
									y.rotation == x.rotation &&
									y.scale == x.scale &&
									y.text == x.text &&
									y.zscale == x.zscale;
						})));

						if (baseData.eventObjects == null)
							baseData.eventObjects = new DataManager.GameData.EventObjects();

						for (int j = 0; j < gameDatas[i].eventObjects.allEvents.Count; j++)
                        {
							if (baseData.eventObjects.allEvents.Count <= j)
								baseData.eventObjects.allEvents.Add(new List<BaseEventKeyframe>());

							baseData.eventObjects.allEvents[j].AddRange(gameDatas[i].eventObjects.allEvents[j].Where(x => !baseData.eventObjects.allEvents[j].Has(y => y.eventTime == x.eventTime)));
                        }

						foreach (var beatmapTheme in gameDatas[i].beatmapThemes)
                        {
							if (!baseData.beatmapThemes.ContainsKey(beatmapTheme.Key))
								baseData.beatmapThemes.Add(beatmapTheme.Key, beatmapTheme.Value);
                        }

						// Clearing
						{
							for (int j = 0; j < gameDatas[i].beatmapData.checkpoints.Count; j++)
								gameDatas[i].beatmapData.checkpoints[j] = null;
							gameDatas[i].beatmapData.checkpoints.Clear();

							for (int j = 0; j < gameDatas[i].beatmapData.markers.Count; j++)
								gameDatas[i].beatmapData.markers[j] = null;
							gameDatas[i].beatmapData.markers.Clear();
							
							for (int j = 0; j < gameDatas[i].beatmapObjects.Count; j++)
								gameDatas[i].beatmapObjects[j] = null;
							gameDatas[i].beatmapObjects.Clear();
							
							for (int j = 0; j < gameDatas[i].backgroundObjects.Count; j++)
								gameDatas[i].backgroundObjects[j] = null;
							gameDatas[i].backgroundObjects.Clear();
							
							for (int j = 0; j < gameDatas[i].prefabObjects.Count; j++)
								gameDatas[i].prefabObjects[j] = null;
							gameDatas[i].prefabObjects.Clear();
							
							for (int j = 0; j < gameDatas[i].prefabs.Count; j++)
								gameDatas[i].prefabs[j] = null;
							gameDatas[i].prefabs.Clear();
							
							gameDatas[i].beatmapThemes.Clear();

							for (int j = 0; j < gameDatas[i].eventObjects.allEvents.Count; j++)
								gameDatas[i].eventObjects.allEvents[j] = null;
							gameDatas[i].eventObjects.allEvents.Clear();

							gameDatas[i] = null;
						}
					}

				gameDatas = null;

				return baseData;
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

			public static BeatmapTheme ParseBeatmapTheme(JSONNode jn, FileType fileType) => fileType == FileType.LS ? BeatmapTheme.Parse(jn) : BeatmapTheme.ParseVG(jn);

			public static List<List<BaseEventKeyframe>> ParseEventkeyframes(JSONNode jn, bool clamp = true)
			{
				var allEvents = new List<List<BaseEventKeyframe>>();

				for (int i = 0; i < GameData.EventCount; i++)
				{
					allEvents.Add(new List<BaseEventKeyframe>());
					if (jn[GameData.EventTypes[i]] != null)
						for (int j = 0; j < jn[GameData.EventTypes[i]].Count; j++)
						allEvents[i].Add(EventKeyframe.Parse(jn[GameData.EventTypes[i]][j], i, GameData.DefaultKeyframes[i].eventValues.Length));
				}

				if (clamp)
					ClampEventListValues(allEvents, GameData.EventCount);

				allEvents.ForEach(x => x = x.OrderBy(x => x.eventTime).ToList());

				return allEvents;
            }

			public static void ClampEventListValues(List<List<BaseEventKeyframe>> eventKeyframes, int totalTypes)
            {
				while (eventKeyframes.Count > totalTypes)
					eventKeyframes.RemoveAt(eventKeyframes.Count - 1);

				for (int type = 0; type < totalTypes; type++)
				{
					if (eventKeyframes.Count < type + 1)
						eventKeyframes.Add(new List<BaseEventKeyframe>());

					if (eventKeyframes[type].Count < 1)
						eventKeyframes[type].Add(EventKeyframe.DeepCopy((EventKeyframe)GameData.DefaultKeyframes[type]));

					for (int index = 0; index < eventKeyframes[type].Count; index++)
					{
						var array = eventKeyframes[type][index].eventValues;
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
			public static IEnumerator SaveData(string _path, GameData _data, Action onSave = null)
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

				for (int i = 0; i < _data.levelModifiers.Count; i++)
				{
					var levelModifier = _data.levelModifiers[i];

					jn["modifiers"][i]["action"] = levelModifier.ActionModifier.ToJSON();
					jn["modifiers"][i]["trigger"] = levelModifier.TriggerModifier.ToJSON();
					jn["modifiers"][i]["retrigger"] = levelModifier.retriggerAmount.ToString();
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

				Debug.Log($"{FunctionsPlugin.className}Saving Object Prefabs");
				for (int i = 0; i < _data.prefabObjects.Count; i++)
				{
					if (!((PrefabObject)_data.prefabObjects[i]).fromModifier)
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
