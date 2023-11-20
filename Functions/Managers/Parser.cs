using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.IO;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using SimpleJSON;
using DG.Tweening;
using LSFunctions;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BaseEventKeyframe = DataManager.GameData.EventKeyframe;
using BasePrefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;

namespace RTFunctions.Functions.Managers
{
    public static class Parser
	{
		public static IEnumerator ParseRTFunctionsData(JSONNode _rt)
        {
			if (!string.IsNullOrEmpty(_rt["v"]))
            {
				RTHelpers.levelVersion = _rt["v"];
            }

			yield break;
        }

		public static IEnumerator ParseBeatmap(string _json, bool editor = false)
		{
			JSONNode jn = JSON.Parse(_json);
			if (!editor)
			{
				DataManager.inst.gameData.ParseThemeData(jn["themes"]);
			}

			if (jn["mod"] != null)
            {
				yield return DataManager.inst.StartCoroutine(ParseRTFunctionsData(jn["mod"]));
            }

			DataManager.inst.gameData.ParseEditorData(jn["ed"]);
			ParseLevelData(jn["level_data"]);
			DataManager.inst.gameData.ParseCheckpointData(jn["checkpoints"]);
			DataManager.inst.StartCoroutine(ParsePrefabs(jn["prefabs"]));
			DataManager.inst.StartCoroutine(ParsePrefabObjects(jn["prefab_objects"]));
			DataManager.inst.StartCoroutine(ParseBeatmapObjects(jn["beatmap_objects"]));
			DataManager.inst.StartCoroutine(ParseBackgroundObjects(jn["bg_objects"]));
			DataManager.inst.StartCoroutine(ParseEventObjects(jn["events"]));

			yield break;
		}

		public static IEnumerator ParseThemeData(JSONNode _themeData)
		{
			UnityEngine.Debug.LogFormat("{0}Parse Theme Data", FunctionsPlugin.className);
			DataManager.inst.CustomBeatmapThemes.Clear();
			DataManager.inst.BeatmapThemeIDToIndex.Clear();
			DataManager.inst.BeatmapThemeIndexToID.Clear();
			int num = 0;
			foreach (DataManager.BeatmapTheme beatmapTheme in DataManager.inst.BeatmapThemes)
			{
				DataManager.inst.BeatmapThemeIDToIndex.Add(num, num);
				DataManager.inst.BeatmapThemeIndexToID.Add(num, num);
				num++;
			}
			if (DataManager.inst.gameData.beatmapData == null)
			{
				DataManager.inst.gameData.beatmapData = new DataManager.GameData.BeatmapData();
			}
			if (_themeData != null)
			{
				DataManager.BeatmapTheme.ParseMulti(_themeData, true);
			}
			yield break;
		}

		public static IEnumerator ParseEditorData(JSONNode _editorData)
		{
			if (DataManager.inst.gameData.beatmapData == null)
			{
				DataManager.inst.gameData.beatmapData = new DataManager.GameData.BeatmapData();
			}
			DataManager.inst.gameData.beatmapData.editorData = new DataManager.GameData.BeatmapData.EditorData();
			if (!string.IsNullOrEmpty(_editorData["timeline_pos"]))
			{
				DataManager.inst.gameData.beatmapData.editorData.timelinePos = _editorData["timeline_pos"].AsFloat;
			}
			else
			{
				DataManager.inst.gameData.beatmapData.editorData.timelinePos = 0f;
			}
			DataManager.inst.gameData.beatmapData.markers.Clear();
			for (int i = 0; i < _editorData["markers"].Count; i++)
			{
				bool asBool = _editorData["markers"][i]["active"].AsBool;
				string name = "Marker";
				if (_editorData["markers"][i]["name"] != null)
				{
					name = _editorData["markers"][i]["name"];
				}
				string desc = "";
				if (_editorData["markers"][i]["desc"] != null)
				{
					desc = _editorData["markers"][i]["desc"];
				}
				float asFloat = _editorData["markers"][i]["t"].AsFloat;
				int color = 0;
				if (_editorData["markers"][i]["col"] != null)
				{
					color = _editorData["markers"][i]["col"].AsInt;
				}
				DataManager.inst.gameData.beatmapData.markers.Add(new DataManager.GameData.BeatmapData.Marker(asBool, name, desc, color, asFloat));
			}
			yield break;
		}

		public static void ParseLevelData(JSONNode _levelData)
		{
			if (DataManager.inst.gameData.beatmapData == null)
			{
				DataManager.inst.gameData.beatmapData = new DataManager.GameData.BeatmapData();
			}
			if (DataManager.inst.gameData.beatmapData.levelData == null)
			{
				DataManager.inst.gameData.beatmapData.levelData = new DataManager.GameData.BeatmapData.LevelData();
			}
			if (_levelData["follow_player"] != null)
			{
				DataManager.inst.gameData.beatmapData.levelData.followPlayer = _levelData["follow_player"].AsBool;
			}
			if (_levelData["show_intro"] != null)
			{
				DataManager.inst.gameData.beatmapData.levelData.showIntro = _levelData["show_intro"].AsBool;
			}

			if (_levelData["bg_zoom"] != null)
				RTHelpers.perspectiveZoom = _levelData["bg_zoom"].AsFloat;
		}

		public static IEnumerator ParseCheckpointData(JSONNode _checkpointData)
		{
			if (DataManager.inst.gameData.beatmapData == null)
			{
				DataManager.inst.gameData.beatmapData = new DataManager.GameData.BeatmapData();
			}
			if (DataManager.inst.gameData.beatmapData.checkpoints == null)
			{
				DataManager.inst.gameData.beatmapData.checkpoints = new List<DataManager.GameData.BeatmapData.Checkpoint>();
			}
			DataManager.inst.gameData.beatmapData.checkpoints.Clear();
			for (int i = 0; i < _checkpointData.Count; i++)
			{
				bool asBool = _checkpointData[i]["active"].AsBool;
				string name = _checkpointData[i]["name"];
				Vector2 pos = new Vector2(_checkpointData[i]["pos"]["x"].AsFloat, _checkpointData[i]["pos"]["y"].AsFloat);
				float time = _checkpointData[i]["t"].AsFloat;
				if (DataManager.inst.gameData.beatmapData.checkpoints.FindIndex((DataManager.GameData.BeatmapData.Checkpoint x) => x.time == time) == -1)
				{
					DataManager.inst.gameData.beatmapData.checkpoints.Add(new DataManager.GameData.BeatmapData.Checkpoint(asBool, name, time, pos));
				}
			}
			DataManager.inst.gameData.beatmapData.checkpoints = (from x in DataManager.inst.gameData.beatmapData.checkpoints
																 orderby x.time
																 select x).ToList();
			yield break;
		}

		public static IEnumerator ParsePrefabs(JSONNode _prefabs)
		{
			if (DataManager.inst.gameData.prefabs == null)
				DataManager.inst.gameData.prefabs = new List<BasePrefab>();
			DataManager.inst.gameData.prefabs.Clear();

			for (int i = 0; i < _prefabs.Count; i++)
				DataManager.inst.gameData.prefabs.Add(Prefab.Parse(_prefabs[i]));

			yield break;
		}

        public static IEnumerator ParsePrefabObjects(JSONNode jn)
		{
			if (DataManager.inst.gameData.prefabObjects == null)
				DataManager.inst.gameData.prefabObjects = new List<BasePrefabObject>();
			DataManager.inst.gameData.prefabObjects.Clear();

			for (int i = 0; i < jn.Count; i++)
				DataManager.inst.gameData.prefabObjects.Add(PrefabObject.Parse(jn[i]));

			yield break;
		}

		public static IEnumerator ParseBeatmapObjects(JSONNode jn)
		{
			if (DataManager.inst.gameData.beatmapObjects == null)
				DataManager.inst.gameData.beatmapObjects = new List<BaseBeatmapObject>();
			DataManager.inst.gameData.beatmapObjects.Clear();

			for (int i = 0; i < jn.Count; i++)
				DataManager.inst.gameData.beatmapObjects.Add(BeatmapObject.Parse(jn[i]));

			yield break;
		}

		public static IEnumerator ParseBackgroundObjects(JSONNode jn)
		{
			if (DataManager.inst.gameData.backgroundObjects == null)
				DataManager.inst.gameData.backgroundObjects = new List<DataManager.GameData.BackgroundObject>();
            DataManager.inst.gameData.backgroundObjects.Clear();

            for (int i = 0; i < jn.Count; i++)
				DataManager.inst.gameData.backgroundObjects.Add(BackgroundObject.Parse(jn[i]));

			yield break;
		}

		public static IEnumerator ParseEventObjects(JSONNode _events)
		{
			if (DataManager.inst.gameData.eventObjects == null)
				DataManager.inst.gameData.eventObjects = new DataManager.GameData.EventObjects();

			var allEvents = DataManager.inst.gameData.eventObjects.allEvents;

			for (int i = 0; i < _events["pos"].Count; i++)
			{
                EventKeyframe eventKeyframe = new EventKeyframe();
				JSONNode jsonnode = _events["pos"][i];
				eventKeyframe.eventTime = jsonnode["t"].AsFloat;
				eventKeyframe.SetEventValues(new float[]
				{
					jsonnode["x"].AsFloat,
					jsonnode["y"].AsFloat
				});
				eventKeyframe.random = jsonnode["r"].AsInt;
				DataManager.LSAnimation curveType = DataManager.inst.AnimationList[0];
				if (jsonnode["ct"] != null)
				{
					curveType = DataManager.inst.AnimationListDictionaryStr[jsonnode["ct"]];
					eventKeyframe.curveType = curveType;
				}
				eventKeyframe.SetEventRandomValues(new float[]
				{
					jsonnode["rx"].AsFloat,
					jsonnode["ry"].AsFloat
				});
				eventKeyframe.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[0].Add(eventKeyframe);
			}
			for (int j = 0; j < _events["zoom"].Count; j++)
			{
                EventKeyframe eventKeyframe2 = new EventKeyframe();
				JSONNode jsonnode2 = _events["zoom"][j];
				eventKeyframe2.eventTime = jsonnode2["t"].AsFloat;
				eventKeyframe2.SetEventValues(new float[]
				{
					jsonnode2["x"].AsFloat
				});
				eventKeyframe2.random = jsonnode2["r"].AsInt;
				DataManager.LSAnimation curveType2 = DataManager.inst.AnimationList[0];
				if (jsonnode2["ct"] != null)
				{
					curveType2 = DataManager.inst.AnimationListDictionaryStr[jsonnode2["ct"]];
					eventKeyframe2.curveType = curveType2;
				}
				eventKeyframe2.SetEventRandomValues(new float[]
				{
					jsonnode2["rx"].AsFloat
				});
				eventKeyframe2.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[1].Add(eventKeyframe2);
			}
			for (int k = 0; k < _events["rot"].Count; k++)
			{
                EventKeyframe eventKeyframe3 = new EventKeyframe();
				JSONNode jsonnode3 = _events["rot"][k];
				eventKeyframe3.eventTime = jsonnode3["t"].AsFloat;
				eventKeyframe3.SetEventValues(new float[]
				{
					jsonnode3["x"].AsFloat
				});
				eventKeyframe3.random = jsonnode3["r"].AsInt;
				DataManager.LSAnimation curveType3 = DataManager.inst.AnimationList[0];
				if (jsonnode3["ct"] != null)
				{
					curveType3 = DataManager.inst.AnimationListDictionaryStr[jsonnode3["ct"]];
					eventKeyframe3.curveType = curveType3;
				}
				eventKeyframe3.SetEventRandomValues(new float[]
				{
					jsonnode3["rx"].AsFloat
				});
				eventKeyframe3.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[2].Add(eventKeyframe3);
			}
			for (int l = 0; l < _events["shake"].Count; l++)
			{
                EventKeyframe eventKeyframe4 = new EventKeyframe();
				JSONNode jsonnode4 = _events["shake"][l];
				eventKeyframe4.eventTime = jsonnode4["t"].AsFloat;
				if (!string.IsNullOrEmpty(jsonnode4["z"]))
				{
					eventKeyframe4.SetEventValues(new float[]
					{
						jsonnode4["x"].AsFloat,
						jsonnode4["y"].AsFloat,
						jsonnode4["z"].AsFloat
					});
				}
				else
				{
					eventKeyframe4.SetEventValues(new float[]
					{
						jsonnode4["x"].AsFloat,
						1f,
						1f
					});
				}
				eventKeyframe4.random = jsonnode4["r"].AsInt;
				DataManager.LSAnimation curveType4 = DataManager.inst.AnimationList[0];
				if (jsonnode4["ct"] != null)
				{
					curveType4 = DataManager.inst.AnimationListDictionaryStr[jsonnode4["ct"]];
					eventKeyframe4.curveType = curveType4;
				}
				eventKeyframe4.SetEventRandomValues(new float[]
				{
					jsonnode4["rx"].AsFloat
				});
				eventKeyframe4.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[3].Add(eventKeyframe4);
			}
			for (int m = 0; m < _events["theme"].Count; m++)
			{
                EventKeyframe eventKeyframe5 = new EventKeyframe();
				JSONNode jsonnode5 = _events["theme"][m];
				eventKeyframe5.eventTime = jsonnode5["t"].AsFloat;
				eventKeyframe5.SetEventValues(new float[]
				{
					jsonnode5["x"].AsFloat
				});
				eventKeyframe5.random = jsonnode5["r"].AsInt;
				DataManager.LSAnimation curveType5 = DataManager.inst.AnimationList[0];
				if (jsonnode5["ct"] != null)
				{
					curveType5 = DataManager.inst.AnimationListDictionaryStr[jsonnode5["ct"]];
					eventKeyframe5.curveType = curveType5;
				}
				eventKeyframe5.SetEventRandomValues(new float[]
				{
					jsonnode5["rx"].AsFloat
				});
				eventKeyframe5.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[4].Add(eventKeyframe5);
			}
			for (int n = 0; n < _events["chroma"].Count; n++)
			{
                EventKeyframe eventKeyframe6 = new EventKeyframe();
				JSONNode jsonnode6 = _events["chroma"][n];
				eventKeyframe6.eventTime = jsonnode6["t"].AsFloat;
				eventKeyframe6.SetEventValues(new float[]
				{
					jsonnode6["x"].AsFloat
				});
				eventKeyframe6.random = jsonnode6["r"].AsInt;
				DataManager.LSAnimation curveType6 = DataManager.inst.AnimationList[0];
				if (jsonnode6["ct"] != null)
				{
					curveType6 = DataManager.inst.AnimationListDictionaryStr[jsonnode6["ct"]];
					eventKeyframe6.curveType = curveType6;
				}
				eventKeyframe6.SetEventRandomValues(new float[]
				{
					jsonnode6["rx"].AsFloat
				});
				eventKeyframe6.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[5].Add(eventKeyframe6);
			}
			for (int num = 0; num < _events["bloom"].Count; num++)
			{
                EventKeyframe eventKeyframe7 = new EventKeyframe();
				JSONNode jsonnode7 = _events["bloom"][num];
				eventKeyframe7.eventTime = jsonnode7["t"].AsFloat;
				if (!string.IsNullOrEmpty(jsonnode7["y"]))
				{
					eventKeyframe7.SetEventValues(new float[]
					{
						jsonnode7["x"].AsFloat,
						jsonnode7["y"].AsFloat,
						jsonnode7["z"].AsFloat,
						jsonnode7["x2"].AsFloat,
						jsonnode7["y2"].AsFloat,
					});
				}
				else
				{
					eventKeyframe7.SetEventValues(new float[]
					{
						jsonnode7["x"].AsFloat,
						7f,
						1f,
						0f,
						18f
					});
				}
				eventKeyframe7.random = jsonnode7["r"].AsInt;
				DataManager.LSAnimation curveType7 = DataManager.inst.AnimationList[0];
				if (jsonnode7["ct"] != null)
				{
					curveType7 = DataManager.inst.AnimationListDictionaryStr[jsonnode7["ct"]];
					eventKeyframe7.curveType = curveType7;
				}
				eventKeyframe7.SetEventRandomValues(new float[]
				{
					jsonnode7["rx"].AsFloat
				});
				eventKeyframe7.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[6].Add(eventKeyframe7);
			}
			for (int num2 = 0; num2 < _events["vignette"].Count; num2++)
			{
                EventKeyframe eventKeyframe8 = new EventKeyframe();
				JSONNode jsonnode8 = _events["vignette"][num2];
				eventKeyframe8.eventTime = jsonnode8["t"].AsFloat;
				if (!string.IsNullOrEmpty(jsonnode8["x3"]))
				{
					eventKeyframe8.SetEventValues(new float[]
					{
						jsonnode8["x"].AsFloat,
						jsonnode8["y"].AsFloat,
						jsonnode8["z"].AsFloat,
						jsonnode8["x2"].AsFloat,
						jsonnode8["y2"].AsFloat,
						jsonnode8["z2"].AsFloat,
						jsonnode8["x3"].AsFloat
					});
				}
				else
				{
					eventKeyframe8.SetEventValues(new float[]
					{
						jsonnode8["x"].AsFloat,
						jsonnode8["y"].AsFloat,
						jsonnode8["z"].AsFloat,
						jsonnode8["x2"].AsFloat,
						jsonnode8["y2"].AsFloat,
						jsonnode8["z2"].AsFloat,
						18f
					});
				}
				eventKeyframe8.random = jsonnode8["r"].AsInt;
				DataManager.LSAnimation curveType8 = DataManager.inst.AnimationList[0];
				if (jsonnode8["ct"] != null)
				{
					curveType8 = DataManager.inst.AnimationListDictionaryStr[jsonnode8["ct"]];
					eventKeyframe8.curveType = curveType8;
				}
				eventKeyframe8.SetEventRandomValues(new float[]
				{
					jsonnode8["rx"].AsFloat,
					jsonnode8["ry"].AsFloat,
					jsonnode8["value_random_z"].AsFloat,
					jsonnode8["value_random_x2"].AsFloat,
					jsonnode8["value_random_y2"].AsFloat,
					jsonnode8["value_random_z2"].AsFloat
				});
				eventKeyframe8.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[7].Add(eventKeyframe8);
			}
			for (int num3 = 0; num3 < _events["lens"].Count; num3++)
			{
                EventKeyframe eventKeyframe9 = new EventKeyframe();
				JSONNode jsonnode9 = _events["lens"][num3];
				eventKeyframe9.eventTime = jsonnode9["t"].AsFloat;
				if (!string.IsNullOrEmpty(jsonnode9["y"]))
				{
					eventKeyframe9.SetEventValues(new float[]
					{
						jsonnode9["x"].AsFloat,
						jsonnode9["y"].AsFloat,
						jsonnode9["z"].AsFloat,
						jsonnode9["x2"].AsFloat,
						jsonnode9["y2"].AsFloat,
						jsonnode9["z2"].AsFloat,
					});
				}
				else
				{
					eventKeyframe9.SetEventValues(new float[]
					{
						jsonnode9["x"].AsFloat,
						0f,
						0f,
						1f,
						1f,
						1f,
					});
				}
				eventKeyframe9.random = jsonnode9["r"].AsInt;
				DataManager.LSAnimation curveType9 = DataManager.inst.AnimationList[0];
				if (jsonnode9["ct"] != null)
				{
					curveType9 = DataManager.inst.AnimationListDictionaryStr[jsonnode9["ct"]];
					eventKeyframe9.curveType = curveType9;
				}
				eventKeyframe9.SetEventRandomValues(new float[]
				{
					jsonnode9["rx"].AsFloat,
					jsonnode9["ry"].AsFloat,
					jsonnode9["value_random_z"].AsFloat
				});
				eventKeyframe9.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[8].Add(eventKeyframe9);
			}
			for (int num4 = 0; num4 < _events["grain"].Count; num4++)
			{
                EventKeyframe eventKeyframe10 = new EventKeyframe();
				JSONNode jsonnode10 = _events["grain"][num4];
				eventKeyframe10.eventTime = jsonnode10["t"].AsFloat;
				eventKeyframe10.SetEventValues(new float[]
				{
					jsonnode10["x"].AsFloat,
					jsonnode10["y"].AsFloat,
					jsonnode10["z"].AsFloat
				});
				eventKeyframe10.random = jsonnode10["r"].AsInt;
				DataManager.LSAnimation curveType10 = DataManager.inst.AnimationList[0];
				if (jsonnode10["ct"] != null)
				{
					curveType10 = DataManager.inst.AnimationListDictionaryStr[jsonnode10["ct"]];
					eventKeyframe10.curveType = curveType10;
				}
				eventKeyframe10.SetEventRandomValues(new float[]
				{
					jsonnode10["rx"].AsFloat,
					jsonnode10["ry"].AsFloat,
					jsonnode10["value_random_z"].AsFloat
				});
				eventKeyframe10.active = false;
				DataManager.inst.gameData.eventObjects.allEvents[9].Add(eventKeyframe10);
			}
			if (allEvents.Count > 10)
			{
				for (int num4 = 0; num4 < _events["cg"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["cg"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat,
						jsonnode11["z"].AsFloat,
						jsonnode11["x2"].AsFloat,
						jsonnode11["y2"].AsFloat,
						jsonnode11["z2"].AsFloat,
						jsonnode11["x3"].AsFloat,
						jsonnode11["y3"].AsFloat,
						jsonnode11["z3"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat,
						jsonnode11["value_random_z"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[10].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["rip"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["rip"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat,
						jsonnode11["z"].AsFloat,
						jsonnode11["x2"].AsFloat,
						jsonnode11["y2"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[11].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["rb"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["rb"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[12].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["cs"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["cs"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[13].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["offset"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["offset"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[14].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["grd"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["grd"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat,
						jsonnode11["z"].AsFloat,
						jsonnode11["x2"].AsFloat,
						jsonnode11["y2"].AsFloat,
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[15].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["dbv"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["dbv"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[16].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["scan"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["scan"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat,
						jsonnode11["z"].AsFloat,
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[17].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["blur"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["blur"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[18].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["pixel"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["pixel"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[19].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["bg"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["bg"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[20].Add(eventKeyframe11);
				}

				if (_events["invert"] != null)
				{
					for (int num4 = 0; num4 < _events["invert"].Count; num4++)
					{
						var eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = _events["invert"][num4];
						eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
						eventKeyframe11.SetEventValues(new float[]
						{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
						});
						eventKeyframe11.random = jsonnode11["r"].AsInt;
						DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
						if (jsonnode11["ct"] != null)
						{
							curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
							eventKeyframe11.curveType = curveType11;
						}
						eventKeyframe11.SetEventRandomValues(new float[]
						{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
						});
						eventKeyframe11.active = false;
						DataManager.inst.gameData.eventObjects.allEvents[21].Add(eventKeyframe11);
					}
				}

				for (int num4 = 0; num4 < _events["timeline"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["timeline"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat,
						jsonnode11["z"].AsFloat,
						jsonnode11["x2"].AsFloat,
						jsonnode11["y2"].AsFloat,
						jsonnode11["z2"].AsFloat,
						jsonnode11["x3"].AsFloat,
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[22].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["player"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["player"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat,
						jsonnode11["z"].AsFloat,
						jsonnode11["x2"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[23].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["follow_player"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["follow_player"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					if (!string.IsNullOrEmpty(jsonnode11["z2"]))
					{
						eventKeyframe11.SetEventValues(new float[]
						{
							jsonnode11["x"].AsFloat,
							jsonnode11["y"].AsFloat,
							jsonnode11["z"].AsFloat,
							jsonnode11["x2"].AsFloat,
							jsonnode11["y2"].AsFloat,
							jsonnode11["z2"].AsFloat,
							jsonnode11["x3"].AsFloat,
							jsonnode11["y3"].AsFloat,
							jsonnode11["z3"].AsFloat,
							jsonnode11["x4"].AsFloat,
						});
					}
					else
					{
						eventKeyframe11.SetEventValues(new float[]
						{
							jsonnode11["x"].AsFloat,
							jsonnode11["y"].AsFloat,
							jsonnode11["z"].AsFloat,
							jsonnode11["x2"].AsFloat,
							jsonnode11["y2"].AsFloat,
							9999f,
							-9999f,
							9999f,
							-9999f,
							1f
						});
					}
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[24].Add(eventKeyframe11);
				}

				for (int num4 = 0; num4 < _events["audio"].Count; num4++)
				{
                    EventKeyframe eventKeyframe11 = new EventKeyframe();
					JSONNode jsonnode11 = _events["audio"][num4];
					eventKeyframe11.eventTime = jsonnode11["t"].AsFloat;
					eventKeyframe11.SetEventValues(new float[]
					{
						jsonnode11["x"].AsFloat,
						jsonnode11["y"].AsFloat
					});
					eventKeyframe11.random = jsonnode11["r"].AsInt;
					DataManager.LSAnimation curveType11 = DataManager.inst.AnimationList[0];
					if (jsonnode11["ct"] != null)
					{
						curveType11 = DataManager.inst.AnimationListDictionaryStr[jsonnode11["ct"]];
						eventKeyframe11.curveType = curveType11;
					}
					eventKeyframe11.SetEventRandomValues(new float[]
					{
						jsonnode11["rx"].AsFloat,
						jsonnode11["ry"].AsFloat
					});
					eventKeyframe11.active = false;
					DataManager.inst.gameData.eventObjects.allEvents[25].Add(eventKeyframe11);
				}
			}

			for (int type = 0; type < allEvents.Count; type++)
			{
				if (allEvents[type].Count < 1)
				{
					allEvents[type].Add(new EventKeyframe
                    {
						eventValues = new float[10],
						eventTime = 0f
					});
					if (type == 11)
					{
						allEvents[type][0].eventValues[2] = 1f;
					}
					if (type == 12)
					{
						allEvents[type][0].eventValues[1] = 6f;
					}
					if (type == 15)
					{
						allEvents[type][0].eventValues[2] = 18f;
						allEvents[type][0].eventValues[3] = 18f;
					}
					if (type == 18)
					{
						allEvents[type][0].eventValues[1] = 6f;
					}
					if (type == 20)
					{
						allEvents[type][0].eventValues[0] = 18f;
					}
					if (type == 21)
					{
						allEvents[type][0].eventValues[0] = 0f;
					}
					if (type == 22)
					{
						//-532?
						allEvents[type][0].eventValues[2] = -342f;
						allEvents[type][0].eventValues[3] = 1f;
						allEvents[type][0].eventValues[4] = 1f;
						allEvents[type][0].eventValues[6] = 18f;
					}
					if (type == 24)
					{
						allEvents[type][0].eventValues[3] = 0.5f;
						allEvents[type][0].eventValues[5] = 9999f;
						allEvents[type][0].eventValues[6] = -9999f;
						allEvents[type][0].eventValues[7] = 9999f;
						allEvents[type][0].eventValues[8] = -9999f;
						allEvents[type][0].eventValues[9] = 1f;
					}
					if (type == 25)
					{
						allEvents[type][0].eventValues[0] = 1f;
						allEvents[type][0].eventValues[1] = 1f;
					}
				}
			}

			allEvents.ForEach(x => x = x.OrderBy(y => y.eventTime).ToList());

			EventManager.inst.updateEvents();
			yield break;
		}
	}
}
