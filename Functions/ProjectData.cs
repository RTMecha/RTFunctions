using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using HarmonyLib;

using SimpleJSON;
using LSFunctions;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using EventKeyframe = DataManager.GameData.EventKeyframe;

using OGBeatmapObject = DataManager.GameData.BeatmapObject;
using Prefab = DataManager.GameData.Prefab;
using PrefabObject = DataManager.GameData.PrefabObject;
using OGBackgroundObject = DataManager.GameData.BackgroundObject;
using BeatmapTheme = DataManager.BeatmapTheme;
using Marker = DataManager.GameData.BeatmapData.Marker;
using Checkpoint = DataManager.GameData.BeatmapData.Checkpoint;

namespace RTFunctions.Functions
{
    public enum TitleFormat
    {
        ArtistTitle,
        TitleArtist
    }

    public class ProjectData
    {
        public static List<Level> levels;
        public static List<Collection> collections;

        public class Level
        {
            public Song song;
            public Beatmap beatmap;
        }

        public class Song
        {
            public Song(string[] artists, string title, bool remix, string[] remixArtists)
            {
                this.artists = artists;
                this.title = title;
                this.remix = remix;
                this.remixArtists = remixArtists;
            }

            public string[] artists;
            public string title;
            public bool remix;
            public string[] remixArtists;

            public string genre;

            public override string ToString() => title;
        }

        public class Beatmap
        {
            public Beatmap(string[] creators, string name)
            {
                this.creators = creators;
                this.name = name;
            }

            public string[] creators;
            public string name;
            public string id;
            public string tags;

            public string refCollectionID;

            #region Difficulty

            public static List<string> DifficultyNames = new List<string>
            {
                "Animation",
                "Easy",
                "Normal",
                "Hard",
                "Expert",
                "Expert+",
                "Master",
            };
            public static List<Color> DifficultyColors = new List<Color>
            {

            };
            public static int MaxDifficulty => 6;

            int difficulty;
            public int Difficulty
            {
                get => Mathf.Clamp(difficulty, 0, MaxDifficulty);
                set => difficulty = Mathf.Clamp(value, 0, MaxDifficulty);
            }

            public string DifficultyName => DifficultyNames[Difficulty];
            public Color DifficultyColor => DifficultyColors[Difficulty];

            #endregion

            public Beatmap GetNextLevel()
            {
                if (collections.Find(x => x.id == refCollectionID) != null)
                {
                    var collection = collections.Find(x => x.id == refCollectionID);
                    int index = collection.levels.IndexOf(this) + 1;
                    if (index > 0 && index < collection.levels.Count)
                        return collection.levels[index];
                }

                return null;
            }
            
            public Beatmap GetPrevLevel()
            {
                if (collections.Find(x => x.id == refCollectionID) != null)
                {
                    var collection = collections.Find(x => x.id == refCollectionID);
                    int index = collection.levels.IndexOf(this) - 1;
                    if (index > 0 && index < collection.levels.Count)
                        return collection.levels[index];
                }

                return null;
            }

            public override string ToString() => name;
        }

        public class Collection
        {
            public Collection(List<Beatmap> levels, string name, string id)
            {
                this.levels = levels;
                this.name = name;
                this.id = id;
            }

            public List<Beatmap> levels;
            public string name;
            public string id;

            public override string ToString() => name;
        }

		public class GameData
        {
			public List<Objects.BeatmapObject> beatmapObjects = new List<Objects.BeatmapObject>();
			public List<Objects.BackgroundObject> backgroundObjects = new List<Objects.BackgroundObject>();
			public Dictionary<string, BeatmapTheme> beatmapThemes = new Dictionary<string, BeatmapTheme>();
			public List<List<EventKeyframe>> events = new List<List<EventKeyframe>>();
			public List<Marker> markers = new List<Marker>();
			public List<Checkpoint> checkpoints = new List<Checkpoint>();

			public List<Objects.Prefab> prefabs = new List<Objects.Prefab>();
			public List<Objects.PrefabObject> prefabObjects = new List<Objects.PrefabObject>();
        }

		public static class Converter
        {
			public static void ConvertPrefabToDAE(Prefab prefab)
            {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				sb.AppendLine("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
				sb.AppendLine("  <asset>");
				sb.AppendLine("    <contributor>");
				sb.AppendLine("      <author>RTMecha</author>");
				sb.AppendLine("      <authoring_tool>Project Arrhythmia</authoring_tool>");
				sb.AppendLine("    </contributor>");
				sb.AppendLine("    <created>2023-09-25T00:03:52</created>");
				sb.AppendLine("    <modified>2023-09-25T00:03:52</modified>");
				sb.AppendLine("    <unit name=\"meter\" meter=\"1\"/>");
				sb.AppendLine("    <up_axis>Z_UP</up_axis>");
				sb.AppendLine("  </asset>");
				sb.AppendLine("  <library_geometries>");
				sb.AppendLine("    <geometry id=\"Beatmap-mesh\" name=\"Beatmap\">");
				sb.AppendLine("      <mesh>");

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
						gameData.markers.Add(Reader.ParseMarker(jn["markers"][i]));
				
				if (addSecondMarkers)
					for (int i = 0; i < jn32["markers"].Count; i++)
						gameData.markers.Add(Reader.ParseMarker(jn32["markers"][i]));

				gameData.markers = gameData.markers.OrderBy(x => x.time).ToList();

                #endregion

                #region Checkpoints

				if (addFirstCheckpoints)
					for (int i = 0; i < jn["checkpoints"].Count; i++)
						gameData.checkpoints.Add(Reader.ParseCheckpoint(jn["checkpoints"][i]));

				if (addSecondCheckpoints)
					for (int i = 0; i < jn32["checkpoints"].Count; i++)
					{
						var checkpoint = Reader.ParseCheckpoint(jn32["checkpoints"][i]);
						if (gameData.checkpoints.Find(x => x.time == checkpoint.time) == null)
							gameData.checkpoints.Add(checkpoint);
					}

				gameData.checkpoints = gameData.checkpoints.OrderBy(x => x.time).ToList();

                #endregion

                #region Prefabs

				for (int i = 0; i < jn["prefabs"].Count; i++)
                {
					var prefab = Reader.ParsePrefab(jn["prefabs"][i]);
					if (gameData.prefabs.Find(x => x.prefab.ID == prefab.prefab.ID) == null)
						gameData.prefabs.Add(prefab);
                }
				
				for (int i = 0; i < jn32["prefabs"].Count; i++)
                {
					var prefab = Reader.ParsePrefab(jn32["prefabs"][i]);
					if (gameData.prefabs.Find(x => x.prefab.ID == prefab.prefab.ID) == null)
						gameData.prefabs.Add(prefab);
                }

                #endregion
				
                #region PrefabObjects

				for (int i = 0; i < jn["prefab_objects"].Count; i++)
                {
					var prefab = Reader.ParsePrefabObject(jn["prefab_objects"][i]);
					if (gameData.prefabObjects.Find(x => x.prefabObject.ID == prefab.prefabObject.ID) == null)
						gameData.prefabObjects.Add(prefab);
                }
				
				for (int i = 0; i < jn32["prefab_objects"].Count; i++)
                {
					var prefab = Reader.ParsePrefabObject(jn32["prefabs"][i]);
					if (gameData.prefabObjects.Find(x => x.prefabObject.ID == prefab.prefabObject.ID) == null)
						gameData.prefabObjects.Add(prefab);
                }

                #endregion

                #region Themes

                for (int i = 0; i < jn["themes"].Count; i++)
					if (!gameData.beatmapThemes.ContainsKey(jn["themes"][i]["id"]))
						gameData.beatmapThemes.Add(jn["themes"][i]["id"], Reader.ParseBeatmapTheme(jn["themes"][i]));

				for (int i = 0; i < jn32["themes"].Count; i++)
					if (!gameData.beatmapThemes.ContainsKey(jn32["themes"][i]["id"]))
						gameData.beatmapThemes.Add(jn32["themes"][i]["id"], Reader.ParseBeatmapTheme(jn32["themes"][i]));

                #endregion

                #region Objects

                for (int i = 0; i < jn["beatmap_objects"].Count; i++)
					gameData.beatmapObjects.Add(Reader.ParseBeatmapObject(jn["beatmap_objects"][i]));

				for (int i = 0; i < jn32["beatmap_objects"].Count; i++)
					gameData.beatmapObjects.Add(Reader.ParseBeatmapObject(jn32["beatmap_objects"][i]));

				#endregion

				#region Backgrounds

				for (int i = 0; i < jn["bg_objects"].Count; i++)
					gameData.backgroundObjects.Add(Reader.ParseBackgroundObject(jn["bg_objects"][i]));
				
				for (int i = 0; i < jn32["bg_objects"].Count; i++)
					gameData.backgroundObjects.Add(Reader.ParseBackgroundObject(jn32["bg_objects"][i]));

                #endregion

                #region Events

                gameData.events = new List<List<EventKeyframe>>();

				var l = Reader.ParseEventkeyframes(jn["events"]);
				var l32 = Reader.ParseEventkeyframes(jn32["events"]);

				for (int i = 0; i < l.Count; i++)
                {
					if (!prioritizeFirstEvents)
						l[i].RemoveAt(0);

					gameData.events.Add(l[i]);
				}

				for (int i = 0; i < l32.Count; i++)
                {
					if (prioritizeFirstEvents)
						l32[i].RemoveAt(0);

					gameData.events[i].AddRange(l32[i]);
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
			public static Objects.BeatmapObject ParseBeatmapObject(JSONNode jn)
            {
				int num = 0;
				OGBeatmapObject beatmapObject = new OGBeatmapObject();

				if (jn["id"] != null)
					beatmapObject.id = jn["id"];
				else
					beatmapObject.id = LSText.randomString(16);

                #region Events

                List<List<EventKeyframe>> list = new List<List<EventKeyframe>>();
				list.Add(new List<EventKeyframe>());
				list.Add(new List<EventKeyframe>());
				list.Add(new List<EventKeyframe>());
				list.Add(new List<EventKeyframe>());
				if (jn["events"] != null)
				{
					for (int i = 0; i < jn["events"]["pos"].Count; i++)
					{
						EventKeyframe eventKeyframe = new EventKeyframe();
						JSONNode jsonnode = jn["events"]["pos"][i];
						eventKeyframe.eventTime = jsonnode["t"].AsFloat;
						if (!string.IsNullOrEmpty(jsonnode["z"]))
						{
							eventKeyframe.SetEventValues(new float[]
							{
							jsonnode["x"].AsFloat,
							jsonnode["y"].AsFloat,
							jsonnode["z"].AsFloat
							});
						}
						else
						{
							eventKeyframe.SetEventValues(new float[]
							{
								jsonnode["x"].AsFloat,
								jsonnode["y"].AsFloat,
								0f
							});
						}
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
							jsonnode["ry"].AsFloat,
							jsonnode["rz"].AsFloat
						});
						eventKeyframe.active = false;
						list[0].Add(eventKeyframe);
					}
					for (int j = 0; j < jn["events"]["sca"].Count; j++)
					{
						EventKeyframe eventKeyframe2 = new EventKeyframe();
						JSONNode jsonnode2 = jn["events"]["sca"][j];
						eventKeyframe2.eventTime = jsonnode2["t"].AsFloat;
						eventKeyframe2.SetEventValues(new float[]
						{
							jsonnode2["x"].AsFloat,
							jsonnode2["y"].AsFloat
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
							jsonnode2["rx"].AsFloat,
							jsonnode2["ry"].AsFloat,
							jsonnode2["rz"].AsFloat
						});
						list[1].Add(eventKeyframe2);
					}
					for (int k = 0; k < jn["events"]["rot"].Count; k++)
					{
						EventKeyframe eventKeyframe3 = new EventKeyframe();
						JSONNode jsonnode3 = jn["events"]["rot"][k];
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
							jsonnode3["rx"].AsFloat,
							0f,
							jsonnode3["rz"].AsFloat
						});
						list[2].Add(eventKeyframe3);
					}
					for (int l = 0; l < jn["events"]["col"].Count; l++)
					{
						EventKeyframe eventKeyframe4 = new EventKeyframe();
						JSONNode jsonnode4 = jn["events"]["col"][l];
						eventKeyframe4.eventTime = jsonnode4["t"].AsFloat;
						if (!string.IsNullOrEmpty(jsonnode4["y"]) && !string.IsNullOrEmpty(jsonnode4["z"]))
						{
							eventKeyframe4.SetEventValues(new float[]
							{
							jsonnode4["x"].AsFloat,
							jsonnode4["y"].AsFloat,
							jsonnode4["z"].AsFloat,
							jsonnode4["x2"].AsFloat,
							jsonnode4["y2"].AsFloat,
							});
						}
						else if (!string.IsNullOrEmpty(jsonnode4["y"]))
						{
							eventKeyframe4.SetEventValues(new float[]
							{
							jsonnode4["x"].AsFloat,
							jsonnode4["y"].AsFloat,
							0f,
							0f,
							0f
							});
						}
						else
						{
							eventKeyframe4.SetEventValues(new float[]
							{
							jsonnode4["x"].AsFloat,
							0f,
							0f,
							0f,
							0f
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
						list[3].Add(eventKeyframe4);
					}
				}
				else
                {
					list[0].Add(new EventKeyframe(0f, new float[3], new float[3], 0));
					list[1].Add(new EventKeyframe(0f, new float[2] { 1f, 1f }, new float[3], 0));
					list[2].Add(new EventKeyframe(0f, new float[2], new float[3], 0));
					list[3].Add(new EventKeyframe(0f, new float[5], new float[5], 0));
                }

				beatmapObject.events = list;

                #endregion

                #region ID

                if (jn["piid"] != null)
					beatmapObject.prefabInstanceID = jn["piid"];
				if (jn["pid"] != null)
					beatmapObject.prefabID = jn["pid"];
				if (jn["p"] != null)
					beatmapObject.parent = jn["p"];

                #endregion

                #region Other Data

                if (jn["pt"] != null)
				{
					string pt = jn["pt"];
					AccessTools.Field(typeof(OGBeatmapObject), "parentType").SetValue(beatmapObject, pt);
				}
				if (jn["po"] != null)
				{
					AccessTools.Field(typeof(OGBeatmapObject), "parentOffsets").SetValue(beatmapObject, new List<float>(from n in jn["po"].AsArray.Children
																													  select n.AsFloat).ToList());
				}
				if (jn["d"] != null)
					AccessTools.Field(typeof(OGBeatmapObject), "depth").SetValue(beatmapObject, jn["d"].AsInt);
				else
					num++;
				if (jn["empty"] != null)
					beatmapObject.objectType = (jn["empty"].AsBool ? ObjectType.Empty : ObjectType.Normal);
				else if (jn["h"] != null)
					beatmapObject.objectType = (jn["h"].AsBool ? ObjectType.Helper : ObjectType.Normal);
				else if (jn["ot"] != null)
					beatmapObject.objectType = (ObjectType)jn["ot"].AsInt;
				if (jn["st"] != null)
					beatmapObject.StartTime = jn["st"].AsFloat;
				else
					beatmapObject.StartTime = 0f;
				if (jn["name"] != null)
					beatmapObject.name = jn["name"];
				if (jn["shape"] != null)
					beatmapObject.shape = jn["shape"].AsInt;
				if (jn["so"] != null)
					beatmapObject.shapeOption = jn["so"].AsInt;
				if (jn["text"] != null)
					beatmapObject.text = jn["text"];
				if (jn["ak"] != null)
					beatmapObject.autoKillType = (jn["ak"].AsBool ? AutoKillType.LastKeyframe : AutoKillType.OldStyleNoAutokill);
				else if (jn["akt"] != null)
					beatmapObject.autoKillType = (AutoKillType)jn["akt"].AsInt;
				if (jn["ako"] != null)
					beatmapObject.autoKillOffset = jn["ako"].AsFloat;
				if (jn["o"] != null)
					beatmapObject.origin = new Vector2(jn["o"]["x"].AsFloat, jn["o"]["y"].AsFloat);
				else
					beatmapObject.origin = Vector2.zero;

                #endregion

                if (jn["ed"] != null)
				{
					if (jn["ed"]["bin"] != null)
						beatmapObject.editorData.locked = jn["ed"]["locked"].AsBool;
					if (jn["ed"]["bin"] != null)
						beatmapObject.editorData.collapse = jn["ed"]["shrink"].AsBool;
					if (jn["ed"]["bin"] != null)
						beatmapObject.editorData.Bin = jn["ed"]["bin"].AsInt;
					if (jn["ed"]["layer"] != null)
						beatmapObject.editorData.Layer = jn["ed"]["layer"].AsInt;
				}

				var obj = new Objects.BeatmapObject(beatmapObject);

				if (jn["modifiers"] != null)
                {
					for (int j = 0; j < jn["modifiers"].Count; j++)
					{
						var dictionary = new Dictionary<string, object>();

						dictionary.Add("type", int.Parse(jn["modifiers"][j]["type"]));

						if (!string.IsNullOrEmpty(jn["modifiers"][j]["not"]))
						{
							dictionary.Add("not", bool.Parse(jn["modifiers"][j]["not"]));
						}
						else
						{
							dictionary.Add("not", false);
						}

						var list2 = new List<string>();

						for (int k = 0; k < jn["modifiers"][j]["commands"].Count; k++)
						{
							list2.Add(jn["modifiers"][j]["commands"][k]);
						}

						dictionary.Add("commands", list2);

						dictionary.Add("constant", bool.Parse(jn["modifiers"][j]["const"]));

						if (!string.IsNullOrEmpty(jn["modifiers"][j]["value"]))
							dictionary.Add("value", (string)jn["modifiers"][j]["value"]);
						else
							dictionary.Add("value", "0");

						obj.modifiers.Add(dictionary);
					}
				}

				return obj;
			}

			public static List<List<EventKeyframe>> ParseEventkeyframes(JSONNode jn, bool orderTime = false)
			{
				var allEvents = new List<List<EventKeyframe>>();

				allEvents.Add(new List<EventKeyframe>());
				for (int i = 0; i < jn["pos"].Count; i++)
				{
					EventKeyframe eventKeyframe = new EventKeyframe();
					JSONNode jsonnode = jn["pos"][i];
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
					allEvents[0].Add(eventKeyframe);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int j = 0; j < jn["zoom"].Count; j++)
				{
					EventKeyframe eventKeyframe2 = new EventKeyframe();
					JSONNode jsonnode2 = jn["zoom"][j];
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
					allEvents[1].Add(eventKeyframe2);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int k = 0; k < jn["rot"].Count; k++)
				{
					EventKeyframe eventKeyframe3 = new EventKeyframe();
					JSONNode jsonnode3 = jn["rot"][k];
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
					allEvents[2].Add(eventKeyframe3);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int l = 0; l < jn["shake"].Count; l++)
				{
					EventKeyframe eventKeyframe4 = new EventKeyframe();
					JSONNode jsonnode4 = jn["shake"][l];
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
					allEvents[3].Add(eventKeyframe4);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int m = 0; m < jn["theme"].Count; m++)
				{
					EventKeyframe eventKeyframe5 = new EventKeyframe();
					JSONNode jsonnode5 = jn["theme"][m];
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
					allEvents[4].Add(eventKeyframe5);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int n = 0; n < jn["chroma"].Count; n++)
				{
					EventKeyframe eventKeyframe6 = new EventKeyframe();
					JSONNode jsonnode6 = jn["chroma"][n];
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
					allEvents[5].Add(eventKeyframe6);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int num = 0; num < jn["bloom"].Count; num++)
				{
					EventKeyframe eventKeyframe7 = new EventKeyframe();
					JSONNode jsonnode7 = jn["bloom"][num];
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
					allEvents[6].Add(eventKeyframe7);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int num2 = 0; num2 < jn["vignette"].Count; num2++)
				{
					EventKeyframe eventKeyframe8 = new EventKeyframe();
					JSONNode jsonnode8 = jn["vignette"][num2];
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
					allEvents[7].Add(eventKeyframe8);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int num3 = 0; num3 < jn["lens"].Count; num3++)
				{
					EventKeyframe eventKeyframe9 = new EventKeyframe();
					JSONNode jsonnode9 = jn["lens"][num3];
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
					allEvents[8].Add(eventKeyframe9);
				}

				allEvents.Add(new List<EventKeyframe>());
				for (int num4 = 0; num4 < jn["grain"].Count; num4++)
				{
					EventKeyframe eventKeyframe10 = new EventKeyframe();
					JSONNode jsonnode10 = jn["grain"][num4];
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
					allEvents[9].Add(eventKeyframe10);
				}

				if (jn["cg"] != null)
				{
					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["cg"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["cg"][num4];
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
						allEvents[10].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["rip"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["rip"][num4];
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
						allEvents[11].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["rb"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["rb"][num4];
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
						allEvents[12].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["cs"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["cs"][num4];
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
						allEvents[13].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["offset"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["offset"][num4];
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
						allEvents[14].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["grd"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["grd"][num4];
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
						allEvents[15].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["dbv"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["dbv"][num4];
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
						allEvents[16].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["scan"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["scan"][num4];
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
						allEvents[17].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["blur"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["blur"][num4];
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
						allEvents[18].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["pixel"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["pixel"][num4];
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
						allEvents[19].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["bg"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["bg"][num4];
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
						allEvents[20].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					if (jn["invert"] != null)
					{
						for (int num4 = 0; num4 < jn["invert"].Count; num4++)
						{
							var eventKeyframe11 = new EventKeyframe();
							JSONNode jsonnode11 = jn["invert"][num4];
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
							allEvents[21].Add(eventKeyframe11);
						}
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["timeline"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["timeline"][num4];
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
						allEvents[22].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["player"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["player"][num4];
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
						allEvents[23].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["follow_player"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["follow_player"][num4];
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
						allEvents[24].Add(eventKeyframe11);
					}

					allEvents.Add(new List<EventKeyframe>());
					for (int num4 = 0; num4 < jn["audio"].Count; num4++)
					{
						EventKeyframe eventKeyframe11 = new EventKeyframe();
						JSONNode jsonnode11 = jn["audio"][num4];
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
						allEvents[25].Add(eventKeyframe11);
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

				if (orderTime)
					allEvents.ForEach(x => x = x.OrderBy(x => x.eventTime).ToList());

				return allEvents;
            }

			public static BeatmapTheme ParseBeatmapTheme(JSONNode jn)
            {
				BeatmapTheme beatmapTheme = new BeatmapTheme();
				beatmapTheme.id = DataManager.inst.AllThemes.Count().ToString();

				if (jn["id"] != null)
					beatmapTheme.id = jn["id"];

				beatmapTheme.name = "name your themes!";

				if (jn["name"] != null)
					beatmapTheme.name = jn["name"];

				beatmapTheme.guiColor = LSColors.gray800;
				if (jn["gui"] != null)
					beatmapTheme.guiColor = LSColors.HexToColorAlpha(jn["gui"]);

				beatmapTheme.backgroundColor = LSColors.gray100;
				if (jn["bg"] != null)
					beatmapTheme.backgroundColor = LSColors.HexToColor(jn["bg"]);
				if (jn["players"] == null)
				{
					beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha("E57373FF"));
					beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha("64B5F6FF"));
					beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha("81C784FF"));
					beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha("FFB74DFF"));
				}
				else
				{
					int num = 0;
					foreach (KeyValuePair<string, JSONNode> keyValuePair in jn["players"].AsArray)
					{
						JSONNode hex = keyValuePair;
						if (num < 4)
						{
							if (hex != null)
							{
								beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha(hex));
							}
							else
								beatmapTheme.playerColors.Add(LSColors.pink500);
							++num;
						}
						else
							break;
					}
					while (beatmapTheme.playerColors.Count < 4)
						beatmapTheme.playerColors.Add(LSColors.pink500);
				}
				if (jn["objs"] == null)
				{
					beatmapTheme.objectColors.Add(LSColors.pink100);
					beatmapTheme.objectColors.Add(LSColors.pink200);
					beatmapTheme.objectColors.Add(LSColors.pink300);
					beatmapTheme.objectColors.Add(LSColors.pink400);
					beatmapTheme.objectColors.Add(LSColors.pink500);
					beatmapTheme.objectColors.Add(LSColors.pink600);
					beatmapTheme.objectColors.Add(LSColors.pink700);
					beatmapTheme.objectColors.Add(LSColors.pink800);
					beatmapTheme.objectColors.Add(LSColors.pink900);
					beatmapTheme.objectColors.Add(LSColors.pink100);
					beatmapTheme.objectColors.Add(LSColors.pink200);
					beatmapTheme.objectColors.Add(LSColors.pink300);
					beatmapTheme.objectColors.Add(LSColors.pink400);
					beatmapTheme.objectColors.Add(LSColors.pink500);
					beatmapTheme.objectColors.Add(LSColors.pink600);
					beatmapTheme.objectColors.Add(LSColors.pink700);
					beatmapTheme.objectColors.Add(LSColors.pink800);
					beatmapTheme.objectColors.Add(LSColors.pink900);
				}
				else
				{
					int num = 0;
					Color color = LSColors.pink500;
					foreach (KeyValuePair<string, JSONNode> keyValuePair in jn["objs"].AsArray)
					{
						JSONNode hex = keyValuePair;
						if (num < 18)
						{
							if (hex != null)
							{
								beatmapTheme.objectColors.Add(LSColors.HexToColorAlpha(hex));
								color = LSColors.HexToColorAlpha(hex);
							}
							else
							{
								Debug.LogErrorFormat("{0}Some kind of object error at {1} in {2}.", FunctionsPlugin.className, num, beatmapTheme.name);
								beatmapTheme.objectColors.Add(LSColors.pink500);
							}
							++num;
						}
						else
							break;
					}
					while (beatmapTheme.objectColors.Count < 18)
						beatmapTheme.objectColors.Add(color);
				}
				if (jn["bgs"] == null)
				{
					beatmapTheme.backgroundColors.Add(LSColors.gray100);
					beatmapTheme.backgroundColors.Add(LSColors.gray200);
					beatmapTheme.backgroundColors.Add(LSColors.gray300);
					beatmapTheme.backgroundColors.Add(LSColors.gray400);
					beatmapTheme.backgroundColors.Add(LSColors.gray500);
					beatmapTheme.backgroundColors.Add(LSColors.gray600);
					beatmapTheme.backgroundColors.Add(LSColors.gray700);
					beatmapTheme.backgroundColors.Add(LSColors.gray800);
					beatmapTheme.backgroundColors.Add(LSColors.gray900);
				}
				else
				{
					int num = 0;
					Color color = LSColors.pink500;
					foreach (KeyValuePair<string, JSONNode> keyValuePair in jn["bgs"].AsArray)
					{
						JSONNode hex = keyValuePair;
						if (num < 9)
						{
							if (hex != null)
							{
								beatmapTheme.backgroundColors.Add(LSColors.HexToColor(hex));
								color = LSColors.HexToColor(hex);
							}
							else
								beatmapTheme.backgroundColors.Add(LSColors.pink500);
							++num;
						}
						else
							break;
					}
					while (beatmapTheme.backgroundColors.Count < 9)
						beatmapTheme.backgroundColors.Add(color);
				}

				return beatmapTheme;
            }

			public static Objects.BackgroundObject ParseBackgroundObject(JSONNode jn)
			{
				bool active = true;
				if (jn["active"] != null)
					active = jn["active"].AsBool;

				string name;
				if (jn["name"] != null)
					name = jn["name"];
				else
					name = "Background";

				int kind;
				if (jn["kind"] != null)
					kind = jn["kind"].AsInt;
				else
					kind = 1;

				string text;
				if (jn["text"] != null)
					text = jn["text"];
				else
					text = "";
				
				Vector2 pos = new Vector2(jn["pos"]["x"].AsFloat, jn["pos"]["y"].AsFloat);
				Vector2 scale = new Vector2(jn["size"]["x"].AsFloat, jn["size"]["y"].AsFloat);

				float asFloat = jn["rot"].AsFloat;
				int asInt = jn["color"].AsInt;
				int asInt2 = jn["layer"].AsInt;

				bool reactive = false;
				if (jn["r_set"] != null)
					reactive = true;

				if (jn["r_set"]["active"] != null)
					reactive = jn["r_set"]["active"].AsBool;

				var reactiveType = OGBackgroundObject.ReactiveType.LOW;
				if (jn["r_set"]["type"] != null)
					reactiveType = (OGBackgroundObject.ReactiveType)Enum.Parse(typeof(OGBackgroundObject.ReactiveType), jn["r_set"]["type"]);

				float reactiveScale = 1f;
				if (jn["r_set"]["scale"] != null)
					reactiveScale = jn["r_set"]["scale"].AsFloat;

				bool drawFade = true;
				if (jn["fade"] != null)
					drawFade = jn["fade"].AsBool;

				var item = new OGBackgroundObject(active, name, kind, text, pos, scale, asFloat, asInt, asInt2, reactive, reactiveType, reactiveScale, drawFade);

				var bg = new Objects.BackgroundObject(item);
				if (jn["zscale"] != null)
					bg.zscale = jn["zscale"].AsFloat;

				if (jn["depth"] != null)
					bg.depth = jn["depth"].AsInt;

				if (jn["s"] != null && jn["so"] != null)
					bg.shape = Objects.GetShape3D(jn["s"].AsInt, jn["so"]);

				if (jn["r_offset"] != null && jn["r_offset"]["x"] != null && jn["r_offset"]["y"] != null)
					bg.rotation = new Vector2(jn["r_offset"]["x"].AsFloat, jn["r_offset"]["y"].AsFloat);
				if (jn["color_fade"] != null)
					bg.FadeColor = jn["color_fade"].AsInt;

				if (jn["rc"] != null)
				{
					try
					{
						if (jn["rc"]["pos"] != null && jn["rc"]["pos"]["i"] != null && jn["rc"]["pos"]["i"]["x"] != null && jn["rc"]["pos"]["i"]["y"] != null)
							bg.reactivePosIntensity = new Vector2(jn["rc"]["pos"]["i"]["x"].AsFloat, jn["rc"]["pos"]["i"]["y"].AsFloat);
						if (jn["rc"]["pos"] != null && jn["rc"]["pos"]["s"] != null && jn["rc"]["pos"]["s"]["x"] != null && jn["rc"]["pos"]["s"]["y"] != null)
							bg.reactivePosSamples = new Vector2Int(jn["rc"]["pos"]["s"]["x"].AsInt, jn["rc"]["pos"]["s"]["y"].AsInt);

						//if (jn["rc"]["z"] != null && jn["rc"]["active"] != null)
						//	bg.reactiveIncludesZ = jn["rc"]["z"]["active"].AsBool;

						if (jn["rc"]["z"] != null && jn["rc"]["z"]["i"] != null)
							bg.reactiveZIntensity = jn["rc"]["z"]["i"].AsFloat;
						if (jn["rc"]["z"] != null && jn["rc"]["z"]["s"] != null)
							bg.reactiveZSample = jn["rc"]["z"]["s"].AsInt;

						if (jn["rc"]["sca"] != null && jn["rc"]["sca"]["i"] != null && jn["rc"]["sca"]["i"]["x"] != null && jn["rc"]["sca"]["i"]["y"] != null)
							bg.reactiveScaIntensity = new Vector2(jn["rc"]["sca"]["i"]["x"].AsFloat, jn["rc"]["sca"]["i"]["y"].AsFloat);
						if (jn["rc"]["sca"] != null && jn["rc"]["sca"]["s"] != null && jn["rc"]["sca"]["s"]["x"] != null && jn["rc"]["sca"]["s"]["y"] != null)
							bg.reactiveScaSamples = new Vector2Int(jn["rc"]["sca"]["s"]["x"].AsInt, jn["rc"]["sca"]["s"]["y"].AsInt);

						if (jn["rc"]["rot"] != null && jn["rc"]["rot"]["i"] != null)
							bg.reactiveRotIntensity = jn["rc"]["rot"]["i"].AsFloat;
						if (jn["rc"]["rot"] != null && jn["rc"]["rot"]["s"] != null)
							bg.reactiveRotSample = jn["rc"]["rot"]["s"].AsInt;

						if (jn["rc"]["col"] != null && jn["rc"]["col"]["i"] != null)
							bg.reactiveColIntensity = jn["rc"]["col"]["i"].AsFloat;
						if (jn["rc"]["col"] != null && jn["rc"]["col"]["s"] != null)
							bg.reactiveColSample = jn["rc"]["col"]["s"].AsInt;
						if (jn["rc"]["col"] != null && jn["rc"]["col"]["c"] != null)
							bg.reactiveCol = jn["rc"]["col"]["c"].AsInt;
					}
					catch (Exception ex)
					{
                        Debug.Log($"{FunctionsPlugin.className}Failed to load settings.\nEXCEPTION: {ex.Message}\nSTACKTRACE: {ex.StackTrace}");
					}
				}

				return bg;
			}

			public static Marker ParseMarker(JSONNode jn)
            {
				bool active = jn["active"].AsBool;

				string name = "Marker";
				if (jn["name"] != null)
				{
					name = jn["name"];
				}

				string desc = "";
				if (jn["desc"] != null)
				{
					desc = jn["desc"];
				}
				float time = jn["t"].AsFloat;

				int color = 0;
				if (jn["col"] != null)
				{
					color = jn["col"].AsInt;
				}

				return new Marker(active, name, desc, color, time);
			}

			public static Checkpoint ParseCheckpoint(JSONNode jn)
            {
				bool active = jn["active"].AsBool;
				string name = jn["name"];
				Vector2 pos = new Vector2(jn["pos"]["x"].AsFloat, jn["pos"]["y"].AsFloat);
				float time = jn["t"].AsFloat;

				return new Checkpoint(active, name, time, pos);
			}

			public static Objects.Prefab ParsePrefab(JSONNode jn)
            {
				var list = new List<OGBeatmapObject>();
				for (int j = 0; j < jn["objects"].Count; j++)
				{
					var beatmapObject = ParseBeatmapObject(jn["objects"][j]);
					if (beatmapObject != null)
						list.Add(beatmapObject.bo);
				}

				List<PrefabObject> list2 = new List<PrefabObject>();
				for (int k = 0; k < jn["prefab_objects"].Count; k++)
				{
					list2.Add(DataManager.inst.gameData.ParsePrefabObject(jn["prefab_objects"][k]));
				}
				Prefab prefab = new Prefab(jn["name"], jn["type"].AsInt, jn["offset"].AsFloat, list, list2);
				prefab.ID = jn["id"];
				prefab.MainObjectID = jn["main_obj_id"];

				var modPrefab = new Objects.Prefab(prefab);
				return modPrefab;
			}

			public static Objects.PrefabObject ParsePrefabObject(JSONNode jn)
            {
				var prefabObject = new PrefabObject();
				prefabObject.prefabID = jn["pid"];
				prefabObject.StartTime = jn["st"].AsFloat;

				if (!string.IsNullOrEmpty(jn["rc"]))
					prefabObject.RepeatCount = int.Parse(jn["rc"]);

				if (!string.IsNullOrEmpty(jn["ro"]))
					prefabObject.RepeatOffsetTime = float.Parse(jn["ro"]);

				if (jn["id"] != null)
				{
					prefabObject.ID = jn["id"];
				}
				else
				{
					prefabObject.ID = LSText.randomString(16);
				}
				if (jn["ed"]["locked"] != null)
				{
					prefabObject.editorData.locked = jn["ed"]["locked"].AsBool;
				}
				if (jn["ed"]["shrink"] != null)
				{
					prefabObject.editorData.collapse = jn["ed"]["shrink"].AsBool;
				}
				if (jn["ed"]["bin"] != null)
				{
					prefabObject.editorData.Bin = jn["ed"]["bin"].AsInt;
				}
				if (jn["ed"]["layer"] != null)
				{
					prefabObject.editorData.Layer = jn["ed"]["layer"].AsInt;
				}
				if (jn["e"]["pos"] != null)
				{
					EventKeyframe eventKeyframe = new EventKeyframe();
					JSONNode jsonnode = jn["e"]["pos"];
					eventKeyframe.SetEventValues(new float[]
					{
					jsonnode["x"].AsFloat,
					jsonnode["y"].AsFloat
					});
					eventKeyframe.random = jsonnode["r"].AsInt;
					eventKeyframe.SetEventRandomValues(new float[]
					{
					jsonnode["rx"].AsFloat,
					jsonnode["ry"].AsFloat,
					jsonnode["rz"].AsFloat
					});
					eventKeyframe.active = false;
					prefabObject.events[0] = eventKeyframe;
				}
				if (jn["e"]["sca"] != null)
				{
					EventKeyframe eventKeyframe2 = new EventKeyframe();
					JSONNode jsonnode2 = jn["e"]["sca"];
					eventKeyframe2.SetEventValues(new float[]
					{
					jsonnode2["x"].AsFloat,
					jsonnode2["y"].AsFloat
					});
					eventKeyframe2.random = jsonnode2["r"].AsInt;
					eventKeyframe2.SetEventRandomValues(new float[]
					{
					jsonnode2["rx"].AsFloat,
					jsonnode2["ry"].AsFloat,
					jsonnode2["rz"].AsFloat
					});
					eventKeyframe2.active = false;
					prefabObject.events[1] = eventKeyframe2;
				}
				if (jn["e"]["rot"] != null)
				{
					EventKeyframe eventKeyframe3 = new EventKeyframe();
					JSONNode jsonnode3 = jn["e"]["rot"];
					eventKeyframe3.SetEventValues(new float[]
					{
					jsonnode3["x"].AsFloat
					});
					eventKeyframe3.random = jsonnode3["r"].AsInt;
					eventKeyframe3.SetEventRandomValues(new float[]
					{
					jsonnode3["rx"].AsFloat,
					0f,
					jsonnode3["rz"].AsFloat
					});
					eventKeyframe3.active = false;
					prefabObject.events[2] = eventKeyframe3;
				}

				var modPrefabObject = new Objects.PrefabObject(prefabObject);
				return modPrefabObject;
			}
        }

		public static class Writer
        {
			public static Action onSave;

			public static IEnumerator SaveData(string _path, GameData _data)
			{
				Debug.Log("Saving Beatmap");
				JSONNode jn = JSON.Parse("{}");
				Debug.Log("Saving Editor Data");
				jn["ed"]["timeline_pos"] = "0";
				Debug.Log("Saving Markers");
				for (int i = 0; i < _data.markers.Count; i++)
				{
					jn["ed"]["markers"][i]["active"] = "True";
					jn["ed"]["markers"][i]["name"] = _data.markers[i].name.ToString();
					jn["ed"]["markers"][i]["desc"] = _data.markers[i].desc.ToString();
					jn["ed"]["markers"][i]["col"] = _data.markers[i].color.ToString();
					jn["ed"]["markers"][i]["t"] = _data.markers[i].time.ToString();
				}
				Debug.Log("Saving Object Prefabs");
				for (int i = 0; i < _data.prefabObjects.Count; i++)
				{
					var prefabObject = _data.prefabObjects[i].prefabObject;

					jn["prefab_objects"][i]["id"] = prefabObject.ID.ToString();
					jn["prefab_objects"][i]["pid"] = prefabObject.prefabID.ToString();
					jn["prefab_objects"][i]["st"] = prefabObject.StartTime.ToString();

					if (prefabObject.RepeatCount > 0)
						jn["prefab_objects"][i]["rc"] = prefabObject.RepeatCount.ToString();
					if (prefabObject.RepeatOffsetTime > 0f)
						jn["prefab_objects"][i]["ro"] = prefabObject.RepeatOffsetTime.ToString();

					if (prefabObject.editorData.locked)
					{
						jn["prefab_objects"][i]["ed"]["locked"] = prefabObject.editorData.locked.ToString();
					}
					if (prefabObject.editorData.collapse)
					{
						jn["prefab_objects"][i]["ed"]["shrink"] = prefabObject.editorData.collapse.ToString();
					}
					jn["prefab_objects"][i]["ed"]["layer"] = prefabObject.editorData.Layer.ToString();
					jn["prefab_objects"][i]["ed"]["bin"] = prefabObject.editorData.Bin.ToString();
					jn["prefab_objects"][i]["e"]["pos"]["x"] = prefabObject.events[0].eventValues[0].ToString();
					jn["prefab_objects"][i]["e"]["pos"]["y"] = prefabObject.events[0].eventValues[1].ToString();
					if (prefabObject.events[0].random != 0)
					{
						jn["prefab_objects"][i]["e"]["pos"]["r"] = prefabObject.events[0].random.ToString();
						jn["prefab_objects"][i]["e"]["pos"]["rx"] = prefabObject.events[0].eventRandomValues[0].ToString();
						jn["prefab_objects"][i]["e"]["pos"]["ry"] = prefabObject.events[0].eventRandomValues[1].ToString();
						jn["prefab_objects"][i]["e"]["pos"]["rz"] = prefabObject.events[0].eventRandomValues[2].ToString();
					}
					jn["prefab_objects"][i]["e"]["sca"]["x"] = prefabObject.events[1].eventValues[0].ToString();
					jn["prefab_objects"][i]["e"]["sca"]["y"] = prefabObject.events[1].eventValues[1].ToString();
					if (prefabObject.events[1].random != 0)
					{
						jn["prefab_objects"][i]["e"]["sca"]["r"] = prefabObject.events[1].random.ToString();
						jn["prefab_objects"][i]["e"]["sca"]["rx"] = prefabObject.events[1].eventRandomValues[0].ToString();
						jn["prefab_objects"][i]["e"]["sca"]["ry"] = prefabObject.events[1].eventRandomValues[1].ToString();
						jn["prefab_objects"][i]["e"]["sca"]["rz"] = prefabObject.events[1].eventRandomValues[2].ToString();
					}
					jn["prefab_objects"][i]["e"]["rot"]["x"] = prefabObject.events[2].eventValues[0].ToString();
					if (prefabObject.events[1].random != 0)
					{
						jn["prefab_objects"][i]["e"]["rot"]["r"] = prefabObject.events[2].random.ToString();
						jn["prefab_objects"][i]["e"]["rot"]["rx"] = prefabObject.events[2].eventRandomValues[0].ToString();
						jn["prefab_objects"][i]["e"]["rot"]["rz"] = prefabObject.events[2].eventRandomValues[2].ToString();
					}
				}
                Debug.Log("Saving Level Data");
                {
                    jn["level_data"]["level_version"] = "4.1.16";
                    jn["level_data"]["background_color"] = "0";
                    jn["level_data"]["follow_player"] = "False";
                    jn["level_data"]["show_intro"] = "False";
                    jn["level_data"]["bg_zoom"] = "1";
                }
                Debug.Log("Saving prefabs");
                if (_data.prefabs != null)
                {
                    for (int i = 0; i < _data.prefabs.Count; i++)
                    {
                        jn["prefabs"][i] = DataManager.inst.GeneratePrefabJSON(_data.prefabs[i].prefab);
                    }
                }
                Debug.Log("Saving themes");
				if (_data.beatmapThemes != null)
				{
					List<BeatmapTheme> levelThemes = new List<BeatmapTheme>();

					for (int i = 0; i < _data.beatmapThemes.Count; i++)
					{
						var beatmapTheme = _data.beatmapThemes.ElementAt(i).Value;

						string id = beatmapTheme.id;

						foreach (var keyframe in _data.events[4])
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
						jn["themes"][i]["id"] = levelThemes[i].id;
						jn["themes"][i]["name"] = levelThemes[i].name;
						//if (ConfigEntries.SaveOpacityToThemes.Value)
							jn["themes"][i]["gui"] = RTHelpers.ColorToHex(levelThemes[i].guiColor);
						//else
						//	jn["themes"][i]["gui"] = LSColors.ColorToHex(levelThemes[i].guiColor);
						jn["themes"][i]["bg"] = LSColors.ColorToHex(levelThemes[i].backgroundColor);
						for (int j = 0; j < levelThemes[i].playerColors.Count; j++)
						{
							//if (ConfigEntries.SaveOpacityToThemes.Value)
								jn["themes"][i]["players"][j] = RTHelpers.ColorToHex(levelThemes[i].playerColors[j]);
							//else
							//	jn["themes"][i]["players"][j] = LSColors.ColorToHex(levelThemes[i].playerColors[j]);
						}
						for (int j = 0; j < levelThemes[i].objectColors.Count; j++)
						{
							//if (ConfigEntries.SaveOpacityToThemes.Value)
								jn["themes"][i]["objs"][j] = RTHelpers.ColorToHex(levelThemes[i].objectColors[j]);
							//else
							//	jn["themes"][i]["objs"][j] = LSColors.ColorToHex(levelThemes[i].objectColors[j]);
						}
						for (int j = 0; j < levelThemes[i].backgroundColors.Count; j++)
						{
							jn["themes"][i]["bgs"][j] = LSColors.ColorToHex(levelThemes[i].backgroundColors[j]);
						}
					}
				}
				Debug.Log("Saving Checkpoints");
				for (int i = 0; i < _data.checkpoints.Count; i++)
				{
					jn["checkpoints"][i]["active"] = "False";
					jn["checkpoints"][i]["name"] = _data.checkpoints[i].name;
					jn["checkpoints"][i]["t"] = _data.checkpoints[i].time.ToString();
					jn["checkpoints"][i]["pos"]["x"] = _data.checkpoints[i].pos.x.ToString();
					jn["checkpoints"][i]["pos"]["y"] = _data.checkpoints[i].pos.y.ToString();
				}
				Debug.Log("Saving Beatmap Objects");
				if (_data.beatmapObjects != null)
				{
					List<Objects.BeatmapObject> list = _data.beatmapObjects.FindAll(x => !x.bo.fromPrefab);
					jn["beatmap_objects"] = new JSONArray();
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i] != null && list[i].bo.events != null && !list[i].bo.fromPrefab)
						{
							var bo = list[i].bo;

							jn["beatmap_objects"][i]["id"] = bo.id;
							if (!string.IsNullOrEmpty(list[i].bo.prefabID))
							{
								jn["beatmap_objects"][i]["pid"] = bo.prefabID;
							}
							if (!string.IsNullOrEmpty(bo.prefabInstanceID))
							{
								jn["beatmap_objects"][i]["piid"] = bo.prefabInstanceID;
							}
							if (bo.GetParentType().ToString() != "101")
							{
								jn["beatmap_objects"][i]["pt"] = bo.GetParentType().ToString();
							}
							if (bo.getParentOffsets().FindIndex((float x) => x != 0f) != -1)
							{
								int num4 = 0;
								foreach (float num5 in bo.getParentOffsets())
								{
									jn["beatmap_objects"][i]["po"][num4] = num5.ToString();
									num4++;
								}
							}
							jn["beatmap_objects"][i]["p"] = bo.parent.ToString();
							jn["beatmap_objects"][i]["d"] = bo.Depth.ToString();
							jn["beatmap_objects"][i]["st"] = bo.StartTime.ToString();
							if (!string.IsNullOrEmpty(bo.name))
							{
								jn["beatmap_objects"][i]["name"] = bo.name;
							}
							jn["beatmap_objects"][i]["ot"] = (int)bo.objectType;
							jn["beatmap_objects"][i]["akt"] = (int)bo.autoKillType;
							jn["beatmap_objects"][i]["ako"] = bo.autoKillOffset;
							if (bo.shape != 0)
							{
								jn["beatmap_objects"][i]["shape"] = bo.shape.ToString();
							}
							if (bo.shapeOption != 0)
							{
								jn["beatmap_objects"][i]["so"] = bo.shapeOption.ToString();
							}
							if (!string.IsNullOrEmpty(bo.text))
							{
								jn["beatmap_objects"][i]["text"] = bo.text;
							}
							jn["beatmap_objects"][i]["o"]["x"] = bo.origin.x.ToString();
							jn["beatmap_objects"][i]["o"]["y"] = bo.origin.y.ToString();
							if (bo.editorData.locked)
							{
								jn["beatmap_objects"][i]["ed"]["locked"] = bo.editorData.locked.ToString();
							}
							if (bo.editorData.collapse)
							{
								jn["beatmap_objects"][i]["ed"]["shrink"] = bo.editorData.collapse.ToString();
							}
							jn["beatmap_objects"][i]["ed"]["bin"] = bo.editorData.Bin.ToString();
							jn["beatmap_objects"][i]["ed"]["layer"] = bo.editorData.Layer.ToString();
							jn["beatmap_objects"][i]["events"]["pos"] = new JSONArray();
							for (int j = 0; j < bo.events[0].Count; j++)
							{
								jn["beatmap_objects"][i]["events"]["pos"][j]["t"] = bo.events[0][j].eventTime.ToString();
								jn["beatmap_objects"][i]["events"]["pos"][j]["x"] = bo.events[0][j].eventValues[0].ToString();
								jn["beatmap_objects"][i]["events"]["pos"][j]["y"] = bo.events[0][j].eventValues[1].ToString();

								//Position Z
								if (bo.events[0][j].eventValues.Length > 2)
								{
									jn["beatmap_objects"][i]["events"]["pos"][j]["z"] = bo.events[0][j].eventValues[2].ToString();
								}

								if (bo.events[0][j].curveType.Name != "Linear")
								{
									jn["beatmap_objects"][i]["events"]["pos"][j]["ct"] = bo.events[0][j].curveType.Name.ToString();
								}
								if (bo.events[0][j].random != 0)
								{
									jn["beatmap_objects"][i]["events"]["pos"][j]["r"] = bo.events[0][j].random.ToString();
									jn["beatmap_objects"][i]["events"]["pos"][j]["rx"] = bo.events[0][j].eventRandomValues[0].ToString();
									jn["beatmap_objects"][i]["events"]["pos"][j]["ry"] = bo.events[0][j].eventRandomValues[1].ToString();
									jn["beatmap_objects"][i]["events"]["pos"][j]["rz"] = bo.events[0][j].eventRandomValues[2].ToString();
								}
							}
							jn["beatmap_objects"][i]["events"]["sca"] = new JSONArray();
							for (int j = 0; j < bo.events[1].Count; j++)
							{
								jn["beatmap_objects"][i]["events"]["sca"][j]["t"] = bo.events[1][j].eventTime.ToString();
								jn["beatmap_objects"][i]["events"]["sca"][j]["x"] = bo.events[1][j].eventValues[0].ToString();
								jn["beatmap_objects"][i]["events"]["sca"][j]["y"] = bo.events[1][j].eventValues[1].ToString();
								if (bo.events[1][j].curveType.Name != "Linear")
								{
									jn["beatmap_objects"][i]["events"]["sca"][j]["ct"] = bo.events[1][j].curveType.Name.ToString();
								}
								if (bo.events[1][j].random != 0)
								{
									jn["beatmap_objects"][i]["events"]["sca"][j]["r"] = bo.events[1][j].random.ToString();
									jn["beatmap_objects"][i]["events"]["sca"][j]["rx"] = bo.events[1][j].eventRandomValues[0].ToString();
									jn["beatmap_objects"][i]["events"]["sca"][j]["ry"] = bo.events[1][j].eventRandomValues[1].ToString();
									jn["beatmap_objects"][i]["events"]["sca"][j]["rz"] = bo.events[1][j].eventRandomValues[2].ToString();
								}
							}
							jn["beatmap_objects"][i]["events"]["rot"] = new JSONArray();
							for (int j = 0; j < bo.events[2].Count; j++)
							{
								jn["beatmap_objects"][i]["events"]["rot"][j]["t"] = bo.events[2][j].eventTime.ToString();
								jn["beatmap_objects"][i]["events"]["rot"][j]["x"] = bo.events[2][j].eventValues[0].ToString();
								if (bo.events[2][j].curveType.Name != "Linear")
								{
									jn["beatmap_objects"][i]["events"]["rot"][j]["ct"] = bo.events[2][j].curveType.Name.ToString();
								}
								if (bo.events[2][j].random != 0)
								{
									jn["beatmap_objects"][i]["events"]["rot"][j]["r"] = bo.events[2][j].random.ToString();
									jn["beatmap_objects"][i]["events"]["rot"][j]["rx"] = bo.events[2][j].eventRandomValues[0].ToString();
									jn["beatmap_objects"][i]["events"]["rot"][j]["rz"] = bo.events[2][j].eventRandomValues[2].ToString();
								}
							}
							jn["beatmap_objects"][i]["events"]["col"] = new JSONArray();
							for (int j = 0; j < bo.events[3].Count; j++)
							{
								jn["beatmap_objects"][i]["events"]["col"][j]["t"] = bo.events[3][j].eventTime.ToString();
								jn["beatmap_objects"][i]["events"]["col"][j]["x"] = bo.events[3][j].eventValues[0].ToString();
								jn["beatmap_objects"][i]["events"]["col"][j]["y"] = bo.events[3][j].eventValues[1].ToString();
								jn["beatmap_objects"][i]["events"]["col"][j]["z"] = bo.events[3][j].eventValues[2].ToString();
								jn["beatmap_objects"][i]["events"]["col"][j]["x2"] = bo.events[3][j].eventValues[3].ToString();
								jn["beatmap_objects"][i]["events"]["col"][j]["y2"] = bo.events[3][j].eventValues[4].ToString();

								if (bo.events[3][j].curveType.Name != "Linear")
								{
									jn["beatmap_objects"][i]["events"]["col"][j]["ct"] = bo.events[3][j].curveType.Name.ToString();
								}
								if (bo.events[3][j].random != 0)
								{
									jn["beatmap_objects"][i]["events"]["col"][j]["r"] = bo.events[3][j].random.ToString();
									jn["beatmap_objects"][i]["events"]["col"][j]["rx"] = bo.events[3][j].eventRandomValues[0].ToString();
								}
							}

							for (int j = 0; j < _data.beatmapObjects[i].modifiers.Count; j++)
							{
								var modifier = (Dictionary<string, object>)_data.beatmapObjects[i].modifiers[j];

								if (modifier.ContainsKey("type"))
									jn["beatmap_objects"][i]["modifiers"][j]["type"] = (int)modifier["type"];

								if (modifier.ContainsKey("not") && (bool)modifier["not"])
									jn["beatmap_objects"][i]["modifiers"][j]["not"] = ((bool)modifier["not"]).ToString();

								if (modifier.ContainsKey("commands"))
								{
									var commands = ((List<string>)modifier["commands"]);
									for (int k = 0; k < commands.Count; k++)
									{
										jn["beatmap_objects"][i]["modifiers"][j]["commands"][k] = commands[k];
									}
								}

								if (modifier.ContainsKey("value"))
									jn["beatmap_objects"][i]["modifiers"][j]["value"] = (string)modifier["value"];

								if (modifier.ContainsKey("constant"))
									jn["beatmap_objects"][i]["modifiers"][j]["const"] = ((bool)modifier["constant"]).ToString();
							}
						}
					}
				}
				else
				{
					Debug.Log("skipping objects");
					jn["beatmap_objects"] = new JSONArray();
				}
				Debug.Log("Saving Background Objects");
				for (int i = 0; i < _data.backgroundObjects.Count; i++)
				{

					try
					{
						var bg = Objects.backgroundObjects[i];

						jn["bg_objects"][i]["active"] = bg.bg.active.ToString();
						jn["bg_objects"][i]["name"] = bg.bg.name.ToString();
						jn["bg_objects"][i]["kind"] = bg.bg.kind.ToString();
						jn["bg_objects"][i]["pos"]["x"] = bg.bg.pos.x.ToString();
						jn["bg_objects"][i]["pos"]["y"] = bg.bg.pos.y.ToString();
						jn["bg_objects"][i]["size"]["x"] = bg.bg.scale.x.ToString();
						jn["bg_objects"][i]["size"]["y"] = bg.bg.scale.y.ToString();
						jn["bg_objects"][i]["rot"] = bg.bg.rot.ToString();
						jn["bg_objects"][i]["color"] = bg.bg.color.ToString();
						jn["bg_objects"][i]["layer"] = bg.bg.layer.ToString();
						jn["bg_objects"][i]["fade"] = bg.bg.drawFade.ToString();

						if (bg.bg.reactive)
						{
							jn["bg_objects"][i]["r_set"]["type"] = bg.bg.reactiveType.ToString();
							jn["bg_objects"][i]["r_set"]["scale"] = bg.bg.reactiveScale.ToString();
						}
						jn["bg_objects"][i]["zscale"] = bg.zscale.ToString();
						jn["bg_objects"][i]["depth"] = bg.depth.ToString();
						jn["bg_objects"][i]["s"] = bg.shape.Type.ToString();
						jn["bg_objects"][i]["so"] = bg.shape.Option.ToString();
						jn["bg_objects"][i]["color_fade"] = bg.FadeColor.ToString();
						jn["bg_objects"][i]["r_offset"]["x"] = bg.rotation.x.ToString();
						jn["bg_objects"][i]["r_offset"]["y"] = bg.rotation.y.ToString();

						{
							jn["bg_objects"][i]["rc"]["pos"]["i"]["x"] = bg.reactivePosIntensity.x.ToString();
							jn["bg_objects"][i]["rc"]["pos"]["i"]["y"] = bg.reactivePosIntensity.y.ToString();
							jn["bg_objects"][i]["rc"]["pos"]["s"]["x"] = bg.reactivePosSamples.x.ToString();
							jn["bg_objects"][i]["rc"]["pos"]["s"]["y"] = bg.reactivePosSamples.y.ToString();

							//jn["bg_objects"][i]["rc"]["z"]["active"] = bg.reactiveIncludesZ.ToString();
							jn["bg_objects"][i]["rc"]["z"]["i"] = bg.reactiveZIntensity.ToString();
							jn["bg_objects"][i]["rc"]["z"]["s"] = bg.reactiveZSample.ToString();

							jn["bg_objects"][i]["rc"]["sca"]["i"]["x"] = bg.reactiveScaIntensity.x.ToString();
							jn["bg_objects"][i]["rc"]["sca"]["i"]["y"] = bg.reactiveScaIntensity.y.ToString();
							jn["bg_objects"][i]["rc"]["sca"]["s"]["x"] = bg.reactiveScaSamples.x.ToString();
							jn["bg_objects"][i]["rc"]["sca"]["s"]["y"] = bg.reactiveScaSamples.y.ToString();

							jn["bg_objects"][i]["rc"]["rot"]["i"] = bg.reactiveRotIntensity.ToString();
							jn["bg_objects"][i]["rc"]["rot"]["s"] = bg.reactiveRotSample.ToString();

							jn["bg_objects"][i]["rc"]["col"]["i"] = bg.reactiveColIntensity.ToString();
							jn["bg_objects"][i]["rc"]["col"]["s"] = bg.reactiveColSample.ToString();
							jn["bg_objects"][i]["rc"]["col"]["c"] = bg.reactiveCol.ToString();
						}
					}
					catch (Exception ex)
					{
						Debug.Log($"{FunctionsPlugin.className}BG Mod error!\nMESSAGE: {ex.Message}\nSTACKTRACE: {ex.StackTrace}");
					}

				}
				Debug.Log("Saving Event Objects");
				{
					for (int i = 0; i < _data.events[0].Count(); i++)
					{
						jn["events"]["pos"][i]["t"] = _data.events[0][i].eventTime.ToString();
						jn["events"]["pos"][i]["x"] = _data.events[0][i].eventValues[0].ToString();
						jn["events"]["pos"][i]["y"] = _data.events[0][i].eventValues[1].ToString();
						if (_data.events[0][i].curveType.Name != "Linear")
						{
							jn["events"]["pos"][i]["ct"] = _data.events[0][i].curveType.Name.ToString();
						}
						if (_data.events[0][i].random != 0)
						{
							jn["events"]["pos"][i]["r"] = _data.events[0][i].random.ToString();
							jn["events"]["pos"][i]["rx"] = _data.events[0][i].eventRandomValues[0].ToString();
							jn["events"]["pos"][i]["ry"] = _data.events[0][i].eventRandomValues[1].ToString();
						}
					}
					for (int i = 0; i < _data.events[1].Count(); i++)
					{
						jn["events"]["zoom"][i]["t"] = _data.events[1][i].eventTime.ToString();
						jn["events"]["zoom"][i]["x"] = _data.events[1][i].eventValues[0].ToString();
						if (_data.events[1][i].curveType.Name != "Linear")
						{
							jn["events"]["zoom"][i]["ct"] = _data.events[1][i].curveType.Name.ToString();
						}
						if (_data.events[1][i].random != 0)
						{
							jn["events"]["zoom"][i]["r"] = _data.events[1][i].random.ToString();
							jn["events"]["zoom"][i]["rx"] = _data.events[1][i].eventRandomValues[0].ToString();
						}
					}
					for (int i = 0; i < _data.events[2].Count(); i++)
					{
						jn["events"]["rot"][i]["t"] = _data.events[2][i].eventTime.ToString();
						jn["events"]["rot"][i]["x"] = _data.events[2][i].eventValues[0].ToString();
						if (_data.events[2][i].curveType.Name != "Linear")
						{
							jn["events"]["rot"][i]["ct"] = _data.events[2][i].curveType.Name.ToString();
						}
						if (_data.events[2][i].random != 0)
						{
							jn["events"]["rot"][i]["r"] = _data.events[2][i].random.ToString();
							jn["events"]["rot"][i]["rx"] = _data.events[2][i].eventRandomValues[0].ToString();
						}
					}
					for (int i = 0; i < _data.events[3].Count(); i++)
					{
						jn["events"]["shake"][i]["t"] = _data.events[3][i].eventTime.ToString();
						jn["events"]["shake"][i]["x"] = _data.events[3][i].eventValues[0].ToString();
						if (_data.events[3][i].eventValues.Length > 1)
							jn["events"]["shake"][i]["y"] = _data.events[3][i].eventValues[1].ToString();
						if (_data.events[3][i].eventValues.Length > 2)
							jn["events"]["shake"][i]["z"] = _data.events[3][i].eventValues[2].ToString();

						if (_data.events[3][i].curveType.Name != "Linear")
						{
							jn["events"]["shake"][i]["ct"] = _data.events[3][i].curveType.Name.ToString();
						}
						if (_data.events[3][i].random != 0)
						{
							jn["events"]["shake"][i]["r"] = _data.events[3][i].random.ToString();
							jn["events"]["shake"][i]["rx"] = _data.events[3][i].eventRandomValues[0].ToString();
							jn["events"]["shake"][i]["ry"] = _data.events[3][i].eventRandomValues[1].ToString();
						}
					}
					for (int i = 0; i < _data.events[4].Count(); i++)
					{
						jn["events"]["theme"][i]["t"] = _data.events[4][i].eventTime.ToString();
						jn["events"]["theme"][i]["x"] = _data.events[4][i].eventValues[0].ToString();
						if (_data.events[4][i].curveType.Name != "Linear")
						{
							jn["events"]["theme"][i]["ct"] = _data.events[4][i].curveType.Name.ToString();
						}
						if (_data.events[4][i].random != 0)
						{
							jn["events"]["theme"][i]["r"] = _data.events[4][i].random.ToString();
							jn["events"]["theme"][i]["rx"] = _data.events[4][i].eventRandomValues[0].ToString();
						}
					}
					for (int i = 0; i < _data.events[5].Count(); i++)
					{
						jn["events"]["chroma"][i]["t"] = _data.events[5][i].eventTime.ToString();
						jn["events"]["chroma"][i]["x"] = _data.events[5][i].eventValues[0].ToString();
						if (_data.events[5][i].curveType.Name != "Linear")
						{
							jn["events"]["chroma"][i]["ct"] = _data.events[5][i].curveType.Name.ToString();
						}
						if (_data.events[5][i].random != 0)
						{
							jn["events"]["chroma"][i]["r"] = _data.events[5][i].random.ToString();
							jn["events"]["chroma"][i]["rx"] = _data.events[5][i].eventRandomValues[0].ToString();
						}
					}
					for (int i = 0; i < _data.events[6].Count(); i++)
					{
						jn["events"]["bloom"][i]["t"] = _data.events[6][i].eventTime.ToString();
						jn["events"]["bloom"][i]["x"] = _data.events[6][i].eventValues[0].ToString();
						if (_data.events[6][i].eventValues.Length > 1)
							jn["events"]["bloom"][i]["y"] = _data.events[6][i].eventValues[1].ToString();
						if (_data.events[6][i].eventValues.Length > 2)
							jn["events"]["bloom"][i]["z"] = _data.events[6][i].eventValues[2].ToString();
						if (_data.events[6][i].eventValues.Length > 3)
							jn["events"]["bloom"][i]["x2"] = _data.events[6][i].eventValues[3].ToString();
						if (_data.events[6][i].eventValues.Length > 4)
							jn["events"]["bloom"][i]["y2"] = _data.events[6][i].eventValues[4].ToString();

						if (_data.events[6][i].curveType.Name != "Linear")
						{
							jn["events"]["bloom"][i]["ct"] = _data.events[6][i].curveType.Name.ToString();
						}
						if (_data.events[6][i].random != 0)
						{
							jn["events"]["bloom"][i]["r"] = _data.events[6][i].random.ToString();
							jn["events"]["bloom"][i]["rx"] = _data.events[6][i].eventRandomValues[0].ToString();
						}
					}
					for (int i = 0; i < _data.events[7].Count(); i++)
					{
						jn["events"]["vignette"][i]["t"] = _data.events[7][i].eventTime.ToString();
						jn["events"]["vignette"][i]["x"] = _data.events[7][i].eventValues[0].ToString();
						jn["events"]["vignette"][i]["y"] = _data.events[7][i].eventValues[1].ToString();
						jn["events"]["vignette"][i]["z"] = _data.events[7][i].eventValues[2].ToString();
						jn["events"]["vignette"][i]["x2"] = _data.events[7][i].eventValues[3].ToString();
						jn["events"]["vignette"][i]["y2"] = _data.events[7][i].eventValues[4].ToString();
						jn["events"]["vignette"][i]["z2"] = _data.events[7][i].eventValues[5].ToString();
						if (_data.events[7][i].eventValues.Length > 6)
							jn["events"]["vignette"][i]["x3"] = _data.events[7][i].eventValues[6].ToString();

						if (_data.events[7][i].curveType.Name != "Linear")
						{
							jn["events"]["vignette"][i]["ct"] = _data.events[7][i].curveType.Name.ToString();
						}
						if (_data.events[7][i].random != 0)
						{
							jn["events"]["vignette"][i]["r"] = _data.events[7][i].random.ToString();
							jn["events"]["vignette"][i]["rx"] = _data.events[7][i].eventRandomValues[0].ToString();
							jn["events"]["vignette"][i]["ry"] = _data.events[7][i].eventRandomValues[1].ToString();
							jn["events"]["vignette"][i]["value_random_z"] = _data.events[7][i].eventRandomValues[2].ToString();
							jn["events"]["vignette"][i]["value_random_x2"] = _data.events[7][i].eventRandomValues[3].ToString();
							jn["events"]["vignette"][i]["value_random_y2"] = _data.events[7][i].eventRandomValues[4].ToString();
							jn["events"]["vignette"][i]["value_random_z2"] = _data.events[7][i].eventRandomValues[5].ToString();
						}
					}
					for (int i = 0; i < _data.events[8].Count(); i++)
					{
						jn["events"]["lens"][i]["t"] = _data.events[8][i].eventTime.ToString();
						jn["events"]["lens"][i]["x"] = _data.events[8][i].eventValues[0].ToString();
						if (_data.events[8][i].eventValues.Length > 1)
							jn["events"]["lens"][i]["y"] = _data.events[8][i].eventValues[1].ToString();
						if (_data.events[8][i].eventValues.Length > 2)
							jn["events"]["lens"][i]["z"] = _data.events[8][i].eventValues[2].ToString();
						if (_data.events[8][i].eventValues.Length > 3)
							jn["events"]["lens"][i]["x2"] = _data.events[8][i].eventValues[3].ToString();
						if (_data.events[8][i].eventValues.Length > 4)
							jn["events"]["lens"][i]["y2"] = _data.events[8][i].eventValues[4].ToString();
						if (_data.events[8][i].eventValues.Length > 5)
							jn["events"]["lens"][i]["z2"] = _data.events[8][i].eventValues[5].ToString();

						if (_data.events[8][i].curveType.Name != "Linear")
						{
							jn["events"]["lens"][i]["ct"] = _data.events[8][i].curveType.Name.ToString();
						}
						if (_data.events[8][i].random != 0)
						{
							jn["events"]["lens"][i]["r"] = _data.events[8][i].random.ToString();
							jn["events"]["lens"][i]["rx"] = _data.events[8][i].eventRandomValues[0].ToString();
						}
					}
					for (int i = 0; i < _data.events[9].Count(); i++)
					{
						jn["events"]["grain"][i]["t"] = _data.events[9][i].eventTime.ToString();
						jn["events"]["grain"][i]["x"] = _data.events[9][i].eventValues[0].ToString();
						jn["events"]["grain"][i]["y"] = _data.events[9][i].eventValues[1].ToString();
						jn["events"]["grain"][i]["z"] = _data.events[9][i].eventValues[2].ToString();
						if (_data.events[9][i].curveType.Name != "Linear")
						{
							jn["events"]["grain"][i]["ct"] = _data.events[9][i].curveType.Name.ToString();
						}
						if (_data.events[9][i].random != 0)
						{
							jn["events"]["grain"][i]["r"] = _data.events[9][i].random.ToString();
							jn["events"]["grain"][i]["rx"] = _data.events[9][i].eventRandomValues[0].ToString();
							jn["events"]["grain"][i]["ry"] = _data.events[9][i].eventRandomValues[1].ToString();
							jn["events"]["grain"][i]["value_random_z"] = _data.events[9][i].eventRandomValues[2].ToString();
						}
					}
					if (_data.events.Count > 10)
					{
						for (int i = 0; i < _data.events[10].Count(); i++)
						{
							jn["events"]["cg"][i]["t"] = _data.events[10][i].eventTime.ToString();
							jn["events"]["cg"][i]["x"] = _data.events[10][i].eventValues[0].ToString();
							jn["events"]["cg"][i]["y"] = _data.events[10][i].eventValues[1].ToString();
							jn["events"]["cg"][i]["z"] = _data.events[10][i].eventValues[2].ToString();
							jn["events"]["cg"][i]["x2"] = _data.events[10][i].eventValues[3].ToString();
							jn["events"]["cg"][i]["y2"] = _data.events[10][i].eventValues[4].ToString();
							jn["events"]["cg"][i]["z2"] = _data.events[10][i].eventValues[5].ToString();
							jn["events"]["cg"][i]["x3"] = _data.events[10][i].eventValues[6].ToString();
							jn["events"]["cg"][i]["y3"] = _data.events[10][i].eventValues[7].ToString();
							jn["events"]["cg"][i]["z3"] = _data.events[10][i].eventValues[8].ToString();
							if (_data.events[10][i].curveType.Name != "Linear")
							{
								jn["events"]["cg"][i]["ct"] = _data.events[10][i].curveType.Name.ToString();
							}
							if (_data.events[10][i].random != 0)
							{
								jn["events"]["cg"][i]["r"] = _data.events[10][i].random.ToString();
								jn["events"]["cg"][i]["rx"] = _data.events[10][i].eventRandomValues[0].ToString();
								jn["events"]["cg"][i]["ry"] = _data.events[10][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 11)
					{
						for (int i = 0; i < _data.events[11].Count(); i++)
						{
							jn["events"]["rip"][i]["t"] = _data.events[11][i].eventTime.ToString();
							jn["events"]["rip"][i]["x"] = _data.events[11][i].eventValues[0].ToString();
							jn["events"]["rip"][i]["y"] = _data.events[11][i].eventValues[1].ToString();
							jn["events"]["rip"][i]["z"] = _data.events[11][i].eventValues[2].ToString();
							jn["events"]["rip"][i]["x2"] = _data.events[11][i].eventValues[3].ToString();
							jn["events"]["rip"][i]["y2"] = _data.events[11][i].eventValues[4].ToString();
							if (_data.events[11][i].curveType.Name != "Linear")
							{
								jn["events"]["rip"][i]["ct"] = _data.events[11][i].curveType.Name.ToString();
							}
							if (_data.events[11][i].random != 0)
							{
								jn["events"]["rip"][i]["r"] = _data.events[11][i].random.ToString();
								jn["events"]["rip"][i]["rx"] = _data.events[11][i].eventRandomValues[0].ToString();
								jn["events"]["rip"][i]["ry"] = _data.events[11][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 12)
					{
						for (int i = 0; i < _data.events[12].Count(); i++)
						{
							jn["events"]["rb"][i]["t"] = _data.events[12][i].eventTime.ToString();
							jn["events"]["rb"][i]["x"] = _data.events[12][i].eventValues[0].ToString();
							jn["events"]["rb"][i]["y"] = _data.events[12][i].eventValues[1].ToString();
							if (_data.events[12][i].curveType.Name != "Linear")
							{
								jn["events"]["rb"][i]["ct"] = _data.events[12][i].curveType.Name.ToString();
							}
							if (_data.events[12][i].random != 0)
							{
								jn["events"]["rb"][i]["r"] = _data.events[12][i].random.ToString();
								jn["events"]["rb"][i]["rx"] = _data.events[12][i].eventRandomValues[0].ToString();
								jn["events"]["rb"][i]["ry"] = _data.events[12][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 13)
					{
						for (int i = 0; i < _data.events[13].Count(); i++)
						{
							jn["events"]["cs"][i]["t"] = _data.events[13][i].eventTime.ToString();
							jn["events"]["cs"][i]["x"] = _data.events[13][i].eventValues[0].ToString();
							jn["events"]["cs"][i]["y"] = _data.events[13][i].eventValues[1].ToString();
							if (_data.events[13][i].curveType.Name != "Linear")
							{
								jn["events"]["cs"][i]["ct"] = _data.events[13][i].curveType.Name.ToString();
							}
							if (_data.events[13][i].random != 0)
							{
								jn["events"]["cs"][i]["r"] = _data.events[13][i].random.ToString();
								jn["events"]["cs"][i]["rx"] = _data.events[13][i].eventRandomValues[0].ToString();
								jn["events"]["cs"][i]["ry"] = _data.events[13][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 14)
					{
						for (int i = 0; i < _data.events[14].Count(); i++)
						{
							jn["events"]["offset"][i]["t"] = _data.events[14][i].eventTime.ToString();
							jn["events"]["offset"][i]["x"] = _data.events[14][i].eventValues[0].ToString();
							jn["events"]["offset"][i]["y"] = _data.events[14][i].eventValues[1].ToString();
							if (_data.events[14][i].curveType.Name != "Linear")
							{
								jn["events"]["offset"][i]["ct"] = _data.events[14][i].curveType.Name.ToString();
							}
							if (_data.events[14][i].random != 0)
							{
								jn["events"]["offset"][i]["r"] = _data.events[14][i].random.ToString();
								jn["events"]["offset"][i]["rx"] = _data.events[14][i].eventRandomValues[0].ToString();
								jn["events"]["offset"][i]["ry"] = _data.events[14][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 15)
					{
						for (int i = 0; i < _data.events[15].Count(); i++)
						{
							jn["events"]["grd"][i]["t"] = _data.events[15][i].eventTime.ToString();
							jn["events"]["grd"][i]["x"] = _data.events[15][i].eventValues[0].ToString();
							jn["events"]["grd"][i]["y"] = _data.events[15][i].eventValues[1].ToString();
							jn["events"]["grd"][i]["z"] = _data.events[15][i].eventValues[2].ToString();
							jn["events"]["grd"][i]["x2"] = _data.events[15][i].eventValues[3].ToString();
							jn["events"]["grd"][i]["y2"] = _data.events[15][i].eventValues[4].ToString();
							if (_data.events[15][i].curveType.Name != "Linear")
							{
								jn["events"]["grd"][i]["ct"] = _data.events[15][i].curveType.Name.ToString();
							}
							if (_data.events[15][i].random != 0)
							{
								jn["events"]["grd"][i]["r"] = _data.events[15][i].random.ToString();
								jn["events"]["grd"][i]["rx"] = _data.events[15][i].eventRandomValues[0].ToString();
								jn["events"]["grd"][i]["ry"] = _data.events[15][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 16)
					{
						for (int i = 0; i < _data.events[16].Count(); i++)
						{
							jn["events"]["dbv"][i]["t"] = _data.events[16][i].eventTime.ToString();
							jn["events"]["dbv"][i]["x"] = _data.events[16][i].eventValues[0].ToString();
							if (_data.events[16][i].curveType.Name != "Linear")
							{
								jn["events"]["dbv"][i]["ct"] = _data.events[16][i].curveType.Name.ToString();
							}
							if (_data.events[16][i].random != 0)
							{
								jn["events"]["dbv"][i]["r"] = _data.events[16][i].random.ToString();
								jn["events"]["dbv"][i]["rx"] = _data.events[16][i].eventRandomValues[0].ToString();
								jn["events"]["dbv"][i]["ry"] = _data.events[16][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 17)
					{
						for (int i = 0; i < _data.events[17].Count(); i++)
						{
							jn["events"]["scan"][i]["t"] = _data.events[17][i].eventTime.ToString();
							jn["events"]["scan"][i]["x"] = _data.events[17][i].eventValues[0].ToString();
							jn["events"]["scan"][i]["y"] = _data.events[17][i].eventValues[1].ToString();
							jn["events"]["scan"][i]["z"] = _data.events[17][i].eventValues[2].ToString();
							if (_data.events[17][i].curveType.Name != "Linear")
							{
								jn["events"]["scan"][i]["ct"] = _data.events[17][i].curveType.Name.ToString();
							}
							if (_data.events[17][i].random != 0)
							{
								jn["events"]["scan"][i]["r"] = _data.events[17][i].random.ToString();
								jn["events"]["scan"][i]["rx"] = _data.events[17][i].eventRandomValues[0].ToString();
								jn["events"]["scan"][i]["ry"] = _data.events[17][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 18)
					{
						for (int i = 0; i < _data.events[18].Count(); i++)
						{
							jn["events"]["blur"][i]["t"] = _data.events[18][i].eventTime.ToString();
							jn["events"]["blur"][i]["x"] = _data.events[18][i].eventValues[0].ToString();
							jn["events"]["blur"][i]["y"] = _data.events[18][i].eventValues[1].ToString();
							if (_data.events[18][i].curveType.Name != "Linear")
							{
								jn["events"]["blur"][i]["ct"] = _data.events[18][i].curveType.Name.ToString();
							}
							if (_data.events[18][i].random != 0)
							{
								jn["events"]["blur"][i]["r"] = _data.events[18][i].random.ToString();
								jn["events"]["blur"][i]["rx"] = _data.events[18][i].eventRandomValues[0].ToString();
								jn["events"]["blur"][i]["ry"] = _data.events[18][i].eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 19)
					{
						for (int i = 0; i < _data.events[19].Count(); i++)
						{
							var eventKeyframe = _data.events[19][i];
							jn["events"]["pixel"][i]["t"] = eventKeyframe.eventTime.ToString();
							jn["events"]["pixel"][i]["x"] = eventKeyframe.eventValues[0].ToString();
							if (eventKeyframe.curveType.Name != "Linear")
							{
								jn["events"]["pixel"][i]["ct"] = eventKeyframe.curveType.Name.ToString();
							}
							if (eventKeyframe.random != 0)
							{
								jn["events"]["pixel"][i]["r"] = eventKeyframe.random.ToString();
								jn["events"]["pixel"][i]["rx"] = eventKeyframe.eventRandomValues[0].ToString();
								jn["events"]["pixel"][i]["ry"] = eventKeyframe.eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 20)
					{
						for (int i = 0; i < _data.events[20].Count(); i++)
						{
							var eventKeyframe = _data.events[20][i];
							jn["events"]["bg"][i]["t"] = eventKeyframe.eventTime.ToString();
							jn["events"]["bg"][i]["x"] = eventKeyframe.eventValues[0].ToString();
							if (eventKeyframe.curveType.Name != "Linear")
							{
								jn["events"]["bg"][i]["ct"] = eventKeyframe.curveType.Name.ToString();
							}
							if (eventKeyframe.random != 0)
							{
								jn["events"]["bg"][i]["r"] = eventKeyframe.random.ToString();
								jn["events"]["bg"][i]["rx"] = eventKeyframe.eventRandomValues[0].ToString();
								jn["events"]["bg"][i]["ry"] = eventKeyframe.eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 21)
					{
						for (int i = 0; i < _data.events[21].Count(); i++)
						{
							var eventKeyframe = _data.events[21][i];
							jn["events"]["invert"][i]["t"] = eventKeyframe.eventTime.ToString();
							jn["events"]["invert"][i]["x"] = eventKeyframe.eventValues[0].ToString();
							jn["events"]["invert"][i]["y"] = eventKeyframe.eventValues[1].ToString();
							if (eventKeyframe.curveType.Name != "Linear")
							{
								jn["events"]["invert"][i]["ct"] = eventKeyframe.curveType.Name.ToString();
							}
							if (eventKeyframe.random != 0)
							{
								jn["events"]["invert"][i]["r"] = eventKeyframe.random.ToString();
								jn["events"]["invert"][i]["rx"] = eventKeyframe.eventRandomValues[0].ToString();
								jn["events"]["invert"][i]["ry"] = eventKeyframe.eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 22)
					{
						for (int i = 0; i < _data.events[22].Count(); i++)
						{
							var eventKeyframe = _data.events[22][i];
							jn["events"]["timeline"][i]["t"] = eventKeyframe.eventTime.ToString();
							jn["events"]["timeline"][i]["x"] = eventKeyframe.eventValues[0].ToString();
							jn["events"]["timeline"][i]["y"] = eventKeyframe.eventValues[1].ToString();
							jn["events"]["timeline"][i]["z"] = eventKeyframe.eventValues[2].ToString();
							jn["events"]["timeline"][i]["x2"] = eventKeyframe.eventValues[3].ToString();
							jn["events"]["timeline"][i]["y2"] = eventKeyframe.eventValues[4].ToString();
							jn["events"]["timeline"][i]["z2"] = eventKeyframe.eventValues[5].ToString();
							jn["events"]["timeline"][i]["x3"] = eventKeyframe.eventValues[6].ToString();
							if (eventKeyframe.curveType.Name != "Linear")
							{
								jn["events"]["timeline"][i]["ct"] = eventKeyframe.curveType.Name.ToString();
							}
							if (eventKeyframe.random != 0)
							{
								jn["events"]["timeline"][i]["r"] = eventKeyframe.random.ToString();
								jn["events"]["timeline"][i]["rx"] = eventKeyframe.eventRandomValues[0].ToString();
								jn["events"]["timeline"][i]["ry"] = eventKeyframe.eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 23)
					{
						for (int i = 0; i < _data.events[23].Count(); i++)
						{
							var eventKeyframe = _data.events[23][i];
							jn["events"]["player"][i]["t"] = eventKeyframe.eventTime.ToString();
							jn["events"]["player"][i]["x"] = eventKeyframe.eventValues[0].ToString();
							jn["events"]["player"][i]["y"] = eventKeyframe.eventValues[1].ToString();
							jn["events"]["player"][i]["z"] = eventKeyframe.eventValues[2].ToString();
							jn["events"]["player"][i]["x2"] = eventKeyframe.eventValues[3].ToString();
							if (eventKeyframe.curveType.Name != "Linear")
							{
								jn["events"]["player"][i]["ct"] = eventKeyframe.curveType.Name.ToString();
							}
							if (eventKeyframe.random != 0)
							{
								jn["events"]["player"][i]["r"] = eventKeyframe.random.ToString();
								jn["events"]["player"][i]["rx"] = eventKeyframe.eventRandomValues[0].ToString();
								jn["events"]["player"][i]["ry"] = eventKeyframe.eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 24)
					{
						for (int i = 0; i < _data.events[24].Count(); i++)
						{
							var eventKeyframe = _data.events[24][i];
							jn["events"]["follow_player"][i]["t"] = eventKeyframe.eventTime.ToString();
							jn["events"]["follow_player"][i]["x"] = eventKeyframe.eventValues[0].ToString();
							jn["events"]["follow_player"][i]["y"] = eventKeyframe.eventValues[1].ToString();
							jn["events"]["follow_player"][i]["z"] = eventKeyframe.eventValues[2].ToString();
							jn["events"]["follow_player"][i]["x2"] = eventKeyframe.eventValues[3].ToString();
							jn["events"]["follow_player"][i]["y2"] = eventKeyframe.eventValues[4].ToString();
							jn["events"]["follow_player"][i]["z2"] = eventKeyframe.eventValues[5].ToString();
							jn["events"]["follow_player"][i]["x3"] = eventKeyframe.eventValues[6].ToString();
							jn["events"]["follow_player"][i]["y3"] = eventKeyframe.eventValues[7].ToString();
							jn["events"]["follow_player"][i]["z3"] = eventKeyframe.eventValues[8].ToString();
							jn["events"]["follow_player"][i]["x4"] = eventKeyframe.eventValues[9].ToString();
							if (eventKeyframe.curveType.Name != "Linear")
							{
								jn["events"]["follow_player"][i]["ct"] = eventKeyframe.curveType.Name.ToString();
							}
							if (eventKeyframe.random != 0)
							{
								jn["events"]["follow_player"][i]["r"] = eventKeyframe.random.ToString();
								jn["events"]["follow_player"][i]["rx"] = eventKeyframe.eventRandomValues[0].ToString();
								jn["events"]["follow_player"][i]["ry"] = eventKeyframe.eventRandomValues[1].ToString();
							}
						}
					}
					if (_data.events.Count > 25)
					{
						for (int i = 0; i < _data.events[25].Count(); i++)
						{
							var eventKeyframe = _data.events[25][i];
							jn["events"]["audio"][i]["t"] = eventKeyframe.eventTime.ToString();
							jn["events"]["audio"][i]["x"] = eventKeyframe.eventValues[0].ToString();
							jn["events"]["audio"][i]["y"] = eventKeyframe.eventValues[1].ToString();
							if (eventKeyframe.curveType.Name != "Linear")
							{
								jn["events"]["audio"][i]["ct"] = eventKeyframe.curveType.Name.ToString();
							}
							if (eventKeyframe.random != 0)
							{
								jn["events"]["audio"][i]["r"] = eventKeyframe.random.ToString();
								jn["events"]["audio"][i]["rx"] = eventKeyframe.eventRandomValues[0].ToString();
								jn["events"]["audio"][i]["ry"] = eventKeyframe.eventRandomValues[1].ToString();
							}
						}
					}
				}

				Debug.Log($"{FunctionsPlugin.className}Saving Entire Beatmap");
				Debug.LogFormat("{0}Path: {1}", FunctionsPlugin.className, _path);
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
