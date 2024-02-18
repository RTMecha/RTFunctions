using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;

using LSFunctions;

using SimpleJSON;

using RTFunctions.Functions;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Managers.Networking;
using RTFunctions.Functions.Animation;
using RTFunctions.Functions.IO;

using BeatmapObject = DataManager.GameData.BeatmapObject;
using Prefab = DataManager.GameData.Prefab;
using BeatmapTheme = DataManager.BeatmapTheme;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(DataManager))]
    public class DataManagerPatch : MonoBehaviour
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPostfix(DataManager __instance, ref Dictionary<string, int> ___languagesToIndex, ref Dictionary<int, string> ___indexToLangauge)
        {
            var systemManager = SystemManager.inst;

            AlephNetworkManager.Init();

            var modCompatibility = new GameObject("ModCompatibility");
            modCompatibility.transform.SetParent(systemManager.transform);
            modCompatibility.AddComponent<ModCompatibility>();

            var objects = new GameObject("ShapeManager");
            objects.transform.SetParent(systemManager.transform);
            objects.AddComponent<ShapeManager>();

            var uiManager = new GameObject("UIManager");
            uiManager.transform.SetParent(systemManager.transform);
            uiManager.AddComponent<UIManager>();

            var quickElements = new GameObject("QuickElementsManager");
            quickElements.transform.SetParent(systemManager.transform);
            quickElements.AddComponent<QuickElementManager>();

            var spriteManager = new GameObject("SpriteManager");
            spriteManager.transform.SetParent(systemManager.transform);
            spriteManager.AddComponent<SpriteManager>();

            var fontManager = new GameObject("FontManager");
            fontManager.transform.SetParent(systemManager.transform);
            fontManager.AddComponent<FontManager>();

            var assetManager = new GameObject("AssetManager");
            assetManager.transform.SetParent(systemManager.transform);
            assetManager.AddComponent<AssetManager>();
            
            var levelManager = new GameObject("LevelManager");
            levelManager.transform.SetParent(systemManager.transform);
            levelManager.AddComponent<LevelManager>();
            
            var playerManager = new GameObject("PlayerManager");
            playerManager.transform.SetParent(systemManager.transform);
            playerManager.AddComponent<PlayerManager>();

            //AlephNetworkManager.Init();

            try
            {
                RTCode.Init();
            }
            catch (Exception ex)
            {
                Debug.LogError($"RTCode Evaluator failed to initialize.\n{ex}");
            }

            AnimationManager.Init();

            RTVideoManager.Init();

            // Test to see if this is even necessary. If not, then feel free to remove this.
            //EnumPatcher.AddEnumValue<BeatmapObject.ObjectType>("Solid");
            //EnumPatcher.AddEnumValue<DataManager.GameData.BackgroundObject.ReactiveType>("CUSTOM");

            //EnumPatcher.AddEnumValue<DataManager.Language>("japanese");
            //EnumPatcher.AddEnumValue<DataManager.Language>("thai");
            //EnumPatcher.AddEnumValue<DataManager.Language>("russian");
            //EnumPatcher.AddEnumValue<DataManager.Language>("pirate");

            ___languagesToIndex.Add("japanese", 2);
            ___languagesToIndex.Add("thai", 3);
            ___languagesToIndex.Add("russian", 4);
            ___languagesToIndex.Add("pirate", 5);

            ___indexToLangauge.Add(2, "japanese");
            ___indexToLangauge.Add(3, "thai");
            ___indexToLangauge.Add(4, "russian");
            ___indexToLangauge.Add(5, "pirate");

            __instance.difficulties = new List<DataManager.Difficulty>
            {
                new DataManager.Difficulty("Easy", LSColors.GetThemeColor("easy")),
                new DataManager.Difficulty("Normal", LSColors.GetThemeColor("normal")),
                new DataManager.Difficulty("Hard", LSColors.GetThemeColor("hard")),
                new DataManager.Difficulty("Expert", LSColors.GetThemeColor("expert")),
                new DataManager.Difficulty("Expert+", LSColors.GetThemeColor("expert+")),
                new DataManager.Difficulty("Master", new Color(0.25f, 0.01f, 0.01f)),
                new DataManager.Difficulty("Animation", LSColors.GetThemeColor("none"))
            };

            __instance.linkTypes = new List<DataManager.LinkType>
            {
                new DataManager.LinkType("Spotify", "https://open.spotify.com/artist/{0}"),
                new DataManager.LinkType("SoundCloud", "https://soundcloud.com/{0}"),
                new DataManager.LinkType("Bandcamp", "https://{0}.bandcamp.com"),
                new DataManager.LinkType("YouTube", "https://www.youtube.com/c/{0}"),
                new DataManager.LinkType("Newgrounds", "https://{0}.newgrounds.com/")
            };

            //if (__instance.AnimationList[1].Animation.keys[1].m_Time != 0.9999f)
            //{
            //    __instance.AnimationList[1].Animation.keys[1].m_Time = 0.9999f;
            //    __instance.AnimationList[1].Animation.keys[1].m_Value = 0f;
            //}

            //Themes
            __instance.BeatmapThemes[0].name = "PA Machine";
            __instance.BeatmapThemes[1].name = "PA Anarchy";
            __instance.BeatmapThemes[2].name = "PA Day Night";
            __instance.BeatmapThemes[3].name = "PA Donuts";
            __instance.BeatmapThemes[4].name = "PA Classic";
            __instance.BeatmapThemes[5].name = "PA New";
            __instance.BeatmapThemes[6].name = "PA Dark";
            __instance.BeatmapThemes[7].name = "PA White On Black";
            __instance.BeatmapThemes[8].name = "PA Black On White";

            __instance.BeatmapThemes.Add(DataManager.inst.CreateTheme("PA Example Theme", "9",
                LSColors.HexToColor("212121"),
                LSColors.HexToColorAlpha("504040FF"),
                new List<Color>
                {
                    LSColors.HexToColorAlpha("E57373FF"),
                    LSColors.HexToColorAlpha("64B5F6FF"),
                    LSColors.HexToColorAlpha("81C784FF"),
                    LSColors.HexToColorAlpha("FFB74DFF")
                }, new List<Color>
                {
                    LSColors.HexToColorAlpha("3F59FCFF"),
                    LSColors.HexToColorAlpha("3AD4F5FF"),
                    LSColors.HexToColorAlpha("E91E63FF"),
                    LSColors.HexToColorAlpha("E91E63FF"),
                    LSColors.HexToColorAlpha("E91E63FF"),
                    LSColors.HexToColorAlpha("E91E63FF"),
                    LSColors.HexToColorAlpha("E91E6345"),
                    LSColors.HexToColorAlpha("FFFFFFFF"),
                    LSColors.HexToColorAlpha("000000FF")
                }, new List<Color>
                {
                    LSColors.HexToColor("212121"),
                    LSColors.HexToColor("E91E63"),
                    LSColors.HexToColor("E91E63"),
                    LSColors.HexToColor("E91E63"),
                    LSColors.HexToColor("E91E63"),
                    LSColors.HexToColor("E91E63"),
                    LSColors.HexToColor("E91E63"),
                    LSColors.HexToColor("E91E63"),
                    LSColors.HexToColor("E91E63")
                }));

            foreach (var beatmapTheme in __instance.BeatmapThemes)
            {
                if (beatmapTheme.objectColors.Count < 18)
                    while (beatmapTheme.objectColors.Count < 18)
                    {
                        beatmapTheme.objectColors.Add(beatmapTheme.objectColors[beatmapTheme.objectColors.Count - 1]);
                    }
                if (beatmapTheme.backgroundColors.Count < 9)
                    while (beatmapTheme.backgroundColors.Count < 9)
                    {
                        beatmapTheme.backgroundColors.Add(beatmapTheme.backgroundColors[beatmapTheme.backgroundColors.Count - 1]);
                    }

                beatmapTheme.backgroundColor = LSColors.fadeColor(beatmapTheme.backgroundColor, 1f);

                for (int i = 0; i < beatmapTheme.backgroundColors.Count; i++)
                {
                    beatmapTheme.backgroundColors[i] = LSColors.fadeColor(beatmapTheme.backgroundColors[i], 1f);
                }
            }

            for (int i = 0; i < __instance.BeatmapThemes.Count; i++)
            {
                var beatmapTheme = __instance.BeatmapThemes[i];
                __instance.BeatmapThemes[i] = new Functions.Data.BeatmapTheme
                {
                    id = beatmapTheme.id,
                    name = beatmapTheme.name,
                    expanded = beatmapTheme.expanded,
                    backgroundColor = beatmapTheme.backgroundColor,
                    guiAccentColor = beatmapTheme.guiColor,
                    guiColor = beatmapTheme.guiColor,
                    playerColors = beatmapTheme.playerColors,
                    objectColors = beatmapTheme.objectColors,
                    backgroundColors = beatmapTheme.backgroundColors,
                    effectColors = beatmapTheme.objectColors.Clone(),
                };
            }

            __instance.UpdateSettingString("versionNumber", "4.1.16");

            FunctionsPlugin.ParseProfile();
        }

        [HarmonyPatch("SaveData", typeof(string), typeof(DataManager.GameData))]
        [HarmonyPrefix]
        static bool SaveDataPrefix(DataManager __instance, ref IEnumerator __result, string __0, DataManager.GameData __1)
        {
            Debug.Log($"{__instance.className}GameData is modded: {__1 is Functions.Data.GameData}");
            __result = ProjectData.Writer.SaveData(__0, (Functions.Data.GameData)__1);
            return false;
        }

        [HarmonyPatch("SaveMetadata", typeof(string), typeof(DataManager.MetaData))]
        [HarmonyPrefix]
        static bool SaveMetadataPrefix(ref LSError __result, DataManager __instance, string __0, DataManager.MetaData __1)
        {
            var result = new LSError(false, "");
            JSONNode jn;
            try
            {
                if (__1 is Functions.Data.MetaData)
                {
                    jn = ((Functions.Data.MetaData)__1).ToJSON();

                    Debug.Log($"{__instance.className}Saving Metadata Full");
                    RTFile.WriteToFile(__0, jn.ToString());
                }
                else
                {
                    jn = JSON.Parse("{}");
                    jn["artist"]["name"] = __1.artist.Name;
                    jn["artist"]["link"] = __1.artist.Link;
                    jn["artist"]["linkType"] = __1.artist.LinkType.ToString();
                    jn["creator"]["steam_name"] = __1.creator.steam_name;
                    jn["creator"]["steam_id"] = __1.creator.steam_id.ToString();
                    jn["song"]["title"] = __1.song.title;
                    jn["song"]["difficulty"] = __1.song.difficulty.ToString();
                    jn["song"]["description"] = __1.song.description;
                    jn["song"]["bpm"] = __1.song.BPM.ToString();
                    jn["song"]["t"] = __1.song.time.ToString();
                    jn["song"]["preview_start"] = __1.song.BPM.ToString();
                    jn["song"]["preview_length"] = __1.song.time.ToString();
                    jn["beatmap"]["date_edited"] = __1.beatmap.date_edited;
                    jn["beatmap"]["version_number"] = __1.beatmap.version_number.ToString();
                    jn["beatmap"]["game_version"] = __1.beatmap.game_version;
                    jn["beatmap"]["workshop_id"] = __1.beatmap.workshop_id.ToString();

                    Debug.Log($"{__instance.className}Saving Metadata");
                    LSFile.WriteToFile(__0, jn.ToString());
                }
            }
            catch (System.Exception)
            {
                jn = JSON.Parse("{}");
                jn["artist"]["name"] = __1.artist.Name;
                jn["artist"]["link"] = __1.artist.Link;
                jn["artist"]["linkType"] = __1.artist.LinkType.ToString();
                jn["creator"]["steam_name"] = __1.creator.steam_name;
                jn["creator"]["steam_id"] = __1.creator.steam_id.ToString();
                jn["song"]["title"] = __1.song.title;
                jn["song"]["difficulty"] = __1.song.difficulty.ToString();
                jn["song"]["description"] = __1.song.description;
                jn["song"]["bpm"] = __1.song.BPM.ToString();
                jn["song"]["t"] = __1.song.time.ToString();
                jn["song"]["preview_start"] = __1.song.BPM.ToString();
                jn["song"]["preview_length"] = __1.song.time.ToString();
                jn["beatmap"]["date_edited"] = __1.beatmap.date_edited;
                jn["beatmap"]["version_number"] = __1.beatmap.version_number.ToString();
                jn["beatmap"]["game_version"] = __1.beatmap.game_version;
                jn["beatmap"]["workshop_id"] = __1.beatmap.workshop_id.ToString();

                Debug.Log($"{__instance.className}Saving Metadata");
                RTFile.WriteToFile(__0, jn.ToString());
            }

            __result = result;

            return false;
        }

        [HarmonyPatch("GeneratePrefabJSON")]
        [HarmonyPrefix]
        static bool GeneratePrefabJSON(ref JSONNode __result, Prefab __0)
        {
            __result = ((Functions.Data.Prefab)__0).ToJSON();
            return false;
        }
    }

    [HarmonyPatch(typeof(DataManager.GameData))]
    public class DataManagerGameDataPatch : MonoBehaviour
    {
        [HarmonyPatch("ParseBeatmap")]
        [HarmonyPrefix]
        static bool ParseBeatmapPatch(string _json) => false;
    }

    [HarmonyPatch(typeof(BeatmapTheme))]
    public class DataManagerBeatmapThemePatch
    {
        [HarmonyPatch("Lerp")]
        [HarmonyPrefix]
        static bool Lerp(BeatmapTheme __instance, ref BeatmapTheme _start, ref BeatmapTheme _end, float _val)
        {
            __instance.guiColor = Color.Lerp(_start.guiColor, _end.guiColor, _val);
            __instance.backgroundColor = Color.Lerp(_start.backgroundColor, _end.backgroundColor, _val);
            for (int i = 0; i < 4; i++)
            {
                if (_start.playerColors[i] != null && _end.playerColors[i] != null)
                {
                    __instance.playerColors[i] = Color.Lerp(_start.GetPlayerColor(i), _end.GetPlayerColor(i), _val);
                }
            }

            int maxObj = 9;
            if (_start.objectColors.Count > 9 && _end.objectColors.Count > 9)
            {
                maxObj = 18;
            }

            for (int j = 0; j < maxObj; j++)
            {
                if (_start.objectColors[j] != null && _end.objectColors[j] != null)
                {
                    __instance.objectColors[j] = Color.Lerp(_start.GetObjColor(j), _end.GetObjColor(j), _val);
                }
            }
            for (int k = 0; k < 9; k++)
            {
                if (_start.backgroundColors[k] != null && _end.backgroundColors[k] != null)
                {
                    __instance.backgroundColors[k] = Color.Lerp(_start.GetBGColor(k), _end.GetBGColor(k), _val);
                }
            }
            return false;
        }

        [HarmonyPatch("Parse")]
        [HarmonyPrefix]
        static bool ParsePrefix(BeatmapTheme __instance, ref BeatmapTheme __result, JSONNode __0, bool __1)
        {
            BeatmapTheme beatmapTheme = new BeatmapTheme();
            beatmapTheme.id = DataManager.inst.AllThemes.Count().ToString();
            if (__0["id"] != null)
                beatmapTheme.id = __0["id"];
            beatmapTheme.name = "name your themes!";
            if (__0["name"] != null)
                beatmapTheme.name = __0["name"];
            beatmapTheme.guiColor = LSColors.gray800;
            if (__0["gui"] != null)
                beatmapTheme.guiColor = LSColors.HexToColorAlpha(__0["gui"]);
            beatmapTheme.backgroundColor = LSColors.gray100;
            if (__0["bg"] != null)
                beatmapTheme.backgroundColor = LSColors.HexToColor(__0["bg"]);
            if (__0["players"] == null)
            {
                beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha("E57373FF"));
                beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha("64B5F6FF"));
                beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha("81C784FF"));
                beatmapTheme.playerColors.Add(LSColors.HexToColorAlpha("FFB74DFF"));
            }
            else
            {
                int num = 0;
                foreach (KeyValuePair<string, JSONNode> keyValuePair in __0["players"].AsArray)
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
            if (__0["objs"] == null)
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
                foreach (KeyValuePair<string, JSONNode> keyValuePair in __0["objs"].AsArray)
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
            if (__0["bgs"] == null)
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
                foreach (KeyValuePair<string, JSONNode> keyValuePair in __0["bgs"].AsArray)
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
            if (__1)
            {
                DataManager.inst.CustomBeatmapThemes.Add(beatmapTheme);
                if (DataManager.inst.BeatmapThemeIDToIndex.ContainsKey(int.Parse(beatmapTheme.id)))
                {
                    string str = "";
                    for (int i = 0; i < DataManager.inst.AllThemes.Count; i++)
                    {
                        if (DataManager.inst.AllThemes[i].id == beatmapTheme.id)
                        {
                            str += DataManager.inst.AllThemes[i].name;
                            if (i != DataManager.inst.AllThemes.Count - 1)
                            {
                                str += ", ";
                            }
                        }
                    }

                    if (EditorManager.inst != null)
                    {
                        EditorManager.inst.DisplayNotification("Unable to Load theme [" + beatmapTheme.id + "-" + beatmapTheme.name + "]\nDue to conflicting themes: " + str, 2f, EditorManager.NotificationType.Error);
                    }

                    Debug.LogErrorFormat("{0}Unable to load theme {1} due to the id ({2}) conflicting with these other themes: {3}.", FunctionsPlugin.className, beatmapTheme.name, beatmapTheme.id, str);
                }
                else
                {
                    DataManager.inst.BeatmapThemeIndexToID.Add(DataManager.inst.AllThemes.Count() - 1, int.Parse(beatmapTheme.id));
                    DataManager.inst.BeatmapThemeIDToIndex.Add(int.Parse(beatmapTheme.id), DataManager.inst.AllThemes.Count() - 1);
                }
            }
            __result = beatmapTheme;
            return false;
        }

        [HarmonyPatch("DeepCopy")]
        [HarmonyPrefix]
        static bool DeepCopyPatch(ref BeatmapTheme __result, BeatmapTheme __0, bool __1 = false)
        {
            var themeCopy = new BeatmapTheme();
            themeCopy.name = __0.name;
            themeCopy.playerColors = new List<Color>((from cols in __0.playerColors
                                                      select new Color(cols.r, cols.g, cols.b, cols.a)).ToList());
            themeCopy.objectColors = new List<Color>((from cols in __0.objectColors
                                                      select new Color(cols.r, cols.g, cols.b, cols.a)).ToList());
            themeCopy.guiColor = __0.guiColor;
            themeCopy.backgroundColor = __0.backgroundColor;
            themeCopy.backgroundColors = new List<Color>((from cols in __0.backgroundColors
                                                          select new Color(cols.r, cols.g, cols.b, cols.a)).ToList());
            AccessTools.Field(typeof(BeatmapTheme), "expanded").SetValue(themeCopy, AccessTools.Field(typeof(BeatmapTheme), "expanded").GetValue(__0));
            if (__1)
            {
                themeCopy.id = __0.id;
            }
            if (themeCopy.objectColors.Count < __0.objectColors.Count)
            {
                Color item = themeCopy.objectColors.Last();
                while (themeCopy.objectColors.Count < __0.objectColors.Count)
                {
                    themeCopy.objectColors.Add(item);
                }
            }

            while (themeCopy.objectColors.Count < 18)
            {
                themeCopy.objectColors.Add(themeCopy.objectColors[themeCopy.objectColors.Count - 1]);
            }

            if (themeCopy.backgroundColors.Count < 9)
            {
                Color item2 = themeCopy.backgroundColors.Last();
                while (themeCopy.backgroundColors.Count < 9)
                {
                    themeCopy.backgroundColors.Add(item2);
                }
            }
            __result = themeCopy;
            return false;
        }
    }

    [HarmonyPatch(typeof(BeatmapObject))]
    public class DataManagerBeatmapObjectPatch
    {
        [HarmonyPatch("ParseGameObject")]
        [HarmonyPrefix]
        static bool ParseGameObjectPrefix(ref BeatmapObject __result, JSONNode __0)
        {
            __result = Functions.Data.BeatmapObject.Parse(__0);
            return false;
        }
    }

    [HarmonyPatch(typeof(Prefab))]
    public class DataManagerPrefabPatch
    {
        [HarmonyPatch("DeepCopy")]
        [HarmonyPrefix]
        static bool DeepCopyPrefix(ref Prefab __result, Prefab __0, bool __1 = true)
        {
            Prefab prefab = new Prefab();
            prefab.Name = __0.Name;
            prefab.ID = (__1 ? LSText.randomString(16) : __0.ID);
            prefab.MainObjectID = __0.MainObjectID;
            prefab.Type = __0.Type;
            prefab.Offset = __0.Offset;
            prefab.objects = new List<BeatmapObject>((from obj in __0.objects
                                                                           select DataManager.GameData.BeatmapObject.DeepCopy(obj, false)).ToList());

            prefab.prefabObjects = new List<DataManager.GameData.PrefabObject>((from obj in __0.prefabObjects
                                                                                select DataManager.GameData.PrefabObject.DeepCopy(obj, false)).ToList());

            __result = prefab;
            return false;
        }
    }
}
