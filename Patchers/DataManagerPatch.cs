using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;

using LSFunctions;

using SimpleJSON;

using RTFunctions.Functions;
using RTFunctions.Enums;
using RTFunctions.Functions.Managers;

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

            var modCompatibility = new GameObject("ModCompatibility");
            modCompatibility.transform.SetParent(systemManager.transform);
            modCompatibility.AddComponent<ModCompatibility>();

            var objects = new GameObject("Objects");
            objects.transform.SetParent(systemManager.transform);
            objects.AddComponent<Objects>();

            var uiManager = new GameObject("UIManager");
            uiManager.transform.SetParent(systemManager.transform);
            uiManager.AddComponent<UIManager>();

            var quickElements = new GameObject("QuickElementsManager");
            quickElements.transform.SetParent(systemManager.transform);
            quickElements.AddComponent<QuickElementManager>();

            var spriteManager = new GameObject("SpriteManager");
            spriteManager.transform.SetParent(systemManager.transform);
            spriteManager.AddComponent<RTSpriteManager>();

            var networkManager = new GameObject("NetworkManager");
            networkManager.transform.SetParent(systemManager.transform);
            networkManager.AddComponent<Functions.Managers.Networking.AlephNetworkManager>();
            networkManager.AddComponent<Functions.Managers.Networking.AlephNetworkEditorManager>();

            EnumPatcher.AddEnumValue<BeatmapObject.ObjectType>("Solid");
            EnumPatcher.AddEnumValue<DataManager.GameData.BackgroundObject.ReactiveType>("CUSTOM");

            EnumPatcher.AddEnumValue<DataManager.Language>("japanese");
            EnumPatcher.AddEnumValue<DataManager.Language>("thai");
            EnumPatcher.AddEnumValue<DataManager.Language>("russian");
            EnumPatcher.AddEnumValue<DataManager.Language>("pirate");

            ___languagesToIndex.Add("japanese", 2);
            ___languagesToIndex.Add("thai", 3);
            ___languagesToIndex.Add("russian", 4);
            ___languagesToIndex.Add("pirate", 5);

            ___indexToLangauge.Add(2, "japanese");
            ___indexToLangauge.Add(3, "thai");
            ___indexToLangauge.Add(4, "russian");
            ___indexToLangauge.Add(5, "pirate");

            if (__instance.difficulties.Count != 7)
            {
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
            }

            if (__instance.linkTypes[3].name != "YouTube")
            {
                __instance.linkTypes = new List<DataManager.LinkType>
                {
                    new DataManager.LinkType("Spotify", "https://open.spotify.com/artist/{0}"),
                    new DataManager.LinkType("SoundCloud", "https://soundcloud.com/{0}"),
                    new DataManager.LinkType("Bandcamp", "https://{0}.bandcamp.com"),
                    new DataManager.LinkType("Youtube", "https://www.youtube.com/{0}"),
                    new DataManager.LinkType("Newgrounds", "https://{0}.newgrounds.com/")
                };
            }

            if (__instance.AnimationList[1].Animation.keys[1].m_Time != 0.9999f)
            {
                __instance.AnimationList[1].Animation.keys[1].m_Time = 0.9999f;
                __instance.AnimationList[1].Animation.keys[1].m_Value = 0f;
            }

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

            __instance.UpdateSettingString("versionNumber", "4.1.16");

            FunctionsPlugin.ParseProfile();
        }

        [HarmonyPatch("GeneratePrefabJSON")]
        [HarmonyPrefix]
        static bool GeneratePrefabJSON(ref JSONNode __result, Prefab __0)
        {
            JSONNode jn = JSON.Parse("{}");
            jn["name"] = __0.Name;
            jn["type"] = __0.Type.ToString();

            if (__0.ID != null)
            {
                jn["id"] = __0.ID.ToString();
            }

            if (__0.MainObjectID != null)
            {
                jn["main_obj_id"] = __0.MainObjectID.ToString();
            }
            jn["offset"] = __0.Offset.ToString();
            for (int i = 0; i < __0.objects.Count; i++)
            {
                if (__0.objects[i] != null)
                {
                    jn["objects"][i]["id"] = __0.objects[i].id;
                    jn["objects"][i]["pid"] = __0.objects[i].prefabID;
                    jn["objects"][i]["piid"] = __0.objects[i].prefabInstanceID;

                    if (__0.objects[i].GetParentType().ToString() != "101")
                    {
                        jn["objects"][i]["pt"] = __0.objects[i].GetParentType().ToString();
                    }

                    if (__0.objects[i].getParentOffsets().FindIndex(x => x != 0f) != -1)
                    {
                        int num = 0;
                        foreach (float num2 in __0.objects[i].getParentOffsets())
                        {
                            jn["objects"][i]["po"][num] = num2.ToString();
                            num++;
                        }
                    }

                    jn["objects"][i]["p"] = __0.objects[i].parent.ToString();
                    jn["objects"][i]["d"] = __0.objects[i].Depth.ToString();
                    jn["objects"][i]["ot"] = (int)__0.objects[i].objectType;
                    jn["objects"][i]["st"] = __0.objects[i].StartTime.ToString();

                    if (!string.IsNullOrEmpty(__0.objects[i].text))
                    {
                        jn["objects"][i]["text"] = __0.objects[i].text;
                    }

                    jn["objects"][i]["name"] = __0.objects[i].name;

                    if (__0.objects[i].shape != 0)
                    {
                        jn["objects"][i]["shape"] = __0.objects[i].shape.ToString();
                    }

                    jn["objects"][i]["akt"] = (int)__0.objects[i].autoKillType;
                    jn["objects"][i]["ako"] = __0.objects[i].autoKillOffset;

                    if (__0.objects[i].shapeOption != 0)
                    {
                        jn["objects"][i]["so"] = __0.objects[i].shapeOption.ToString();
                    }

                    if (__0.objects[i].editorData.locked)
                    {
                        jn["objects"][i]["ed"]["locked"] = __0.objects[i].editorData.locked.ToString();
                    }

                    if (__0.objects[i].editorData.collapse)
                    {
                        jn["objects"][i]["ed"]["shrink"] = __0.objects[i].editorData.collapse.ToString();
                    }

                    jn["objects"][i]["o"]["x"] = __0.objects[i].origin.x.ToString();
                    jn["objects"][i]["o"]["y"] = __0.objects[i].origin.y.ToString();
                    jn["objects"][i]["ed"]["bin"] = __0.objects[i].editorData.Bin.ToString();
                    jn["objects"][i]["ed"]["layer"] = __0.objects[i].editorData.Layer.ToString();

                    for (int j = 0; j < __0.objects[i].events[0].Count; j++)
                    {
                        jn["objects"][i]["events"]["pos"][j]["t"] = __0.objects[i].events[0][j].eventTime.ToString();
                        jn["objects"][i]["events"]["pos"][j]["x"] = __0.objects[i].events[0][j].eventValues[0].ToString();
                        jn["objects"][i]["events"]["pos"][j]["y"] = __0.objects[i].events[0][j].eventValues[1].ToString();

                        if (__0.objects[i].events[0][j].eventValues.Length > 2)
                        {
                            jn["objects"][i]["events"]["pos"][j]["z"] = __0.objects[i].events[0][j].eventValues[2].ToString();
                        }

                        if (__0.objects[i].events[0][j].curveType.Name != DataManager.inst.AnimationList[0].Name)
                        {
                            jn["objects"][i]["events"]["pos"][j]["ct"] = __0.objects[i].events[0][j].curveType.Name.ToString();
                        }

                        if (__0.objects[i].events[0][j].random != 0)
                        {
                            jn["objects"][i]["events"]["pos"][j]["r"] = __0.objects[i].events[0][j].random.ToString();
                            jn["objects"][i]["events"]["pos"][j]["rx"] = __0.objects[i].events[0][j].eventRandomValues[0].ToString();
                            jn["objects"][i]["events"]["pos"][j]["ry"] = __0.objects[i].events[0][j].eventRandomValues[1].ToString();
                            jn["objects"][i]["events"]["pos"][j]["rz"] = __0.objects[i].events[0][j].eventRandomValues[2].ToString();
                        }
                    }
                    for (int j = 0; j < __0.objects[i].events[1].Count; j++)
                    {
                        jn["objects"][i]["events"]["sca"][j]["t"] = __0.objects[i].events[1][j].eventTime.ToString();
                        jn["objects"][i]["events"]["sca"][j]["x"] = __0.objects[i].events[1][j].eventValues[0].ToString();
                        jn["objects"][i]["events"]["sca"][j]["y"] = __0.objects[i].events[1][j].eventValues[1].ToString();

                        if (__0.objects[i].events[1][j].curveType.Name != DataManager.inst.AnimationList[0].Name)
                        {
                            jn["objects"][i]["events"]["sca"][j]["ct"] = __0.objects[i].events[1][j].curveType.Name.ToString();
                        }

                        if (__0.objects[i].events[1][j].random != 0)
                        {
                            jn["objects"][i]["events"]["sca"][j]["r"] = __0.objects[i].events[1][j].random.ToString();
                            jn["objects"][i]["events"]["sca"][j]["rx"] = __0.objects[i].events[1][j].eventRandomValues[0].ToString();
                            jn["objects"][i]["events"]["sca"][j]["ry"] = __0.objects[i].events[1][j].eventRandomValues[1].ToString();
                            jn["objects"][i]["events"]["sca"][j]["rz"] = __0.objects[i].events[1][j].eventRandomValues[2].ToString();
                        }
                    }
                    for (int j = 0; j < __0.objects[i].events[2].Count; j++)
                    {
                        jn["objects"][i]["events"]["rot"][j]["t"] = __0.objects[i].events[2][j].eventTime.ToString();
                        jn["objects"][i]["events"]["rot"][j]["x"] = __0.objects[i].events[2][j].eventValues[0].ToString();

                        if (__0.objects[i].events[2][j].curveType.Name != DataManager.inst.AnimationList[0].Name)
                        {
                            jn["objects"][i]["events"]["rot"][j]["ct"] = __0.objects[i].events[2][j].curveType.Name.ToString();
                        }

                        if (__0.objects[i].events[2][j].random != 0)
                        {
                            jn["objects"][i]["events"]["rot"][j]["r"] = __0.objects[i].events[2][j].random.ToString();
                            jn["objects"][i]["events"]["rot"][j]["rx"] = __0.objects[i].events[2][j].eventRandomValues[0].ToString();
                            jn["objects"][i]["events"]["rot"][j]["rz"] = __0.objects[i].events[2][j].eventRandomValues[2].ToString();
                        }
                    }
                    for (int j = 0; j < __0.objects[i].events[3].Count; j++)
                    {
                        jn["objects"][i]["events"]["col"][j]["t"] = __0.objects[i].events[3][j].eventTime.ToString();
                        jn["objects"][i]["events"]["col"][j]["x"] = __0.objects[i].events[3][j].eventValues[0].ToString();
                        if (__0.objects[i].events[3][j].eventValues.Length > 1)
                        {
                            jn["objects"][i]["events"]["col"][j]["y"] = __0.objects[i].events[3][j].eventValues[1].ToString();
                        }
                        if (__0.objects[i].events[3][j].eventValues.Length > 2)
                        {
                            jn["objects"][i]["events"]["col"][j]["z"] = __0.objects[i].events[3][j].eventValues[2].ToString();
                            jn["objects"][i]["events"]["col"][j]["x2"] = __0.objects[i].events[3][j].eventValues[3].ToString();
                            jn["objects"][i]["events"]["col"][j]["y2"] = __0.objects[i].events[3][j].eventValues[4].ToString();
                        }

                        if (__0.objects[i].events[3][j].curveType.Name != DataManager.inst.AnimationList[0].Name)
                        {
                            jn["objects"][i]["events"]["col"][j]["ct"] = __0.objects[i].events[3][j].curveType.Name.ToString();
                        }

                        if (__0.objects[i].events[3][j].random != 0)
                        {
                            jn["objects"][i]["events"]["col"][j]["r"] = __0.objects[i].events[3][j].random.ToString();
                            jn["objects"][i]["events"]["col"][j]["rx"] = __0.objects[i].events[3][j].eventRandomValues[0].ToString();
                        }
                    }

                    if (ModCompatibility.inst != null && ModCompatibility.objectModifiersPlugin != null)
                    {
                        var modifierObject = ModCompatibility.GetModifierObject(__0.objects[i]);

                        if (modifierObject != null)
                        {
                            for (int j = 0; j < ModCompatibility.GetModifierCount(__0.objects[i]); j++)
                            {
                                var modifier = ModCompatibility.GetModifierIndex(__0.objects[i], j);

                                var type = (int)modifier.GetType().GetField("type", BindingFlags.Public | BindingFlags.Instance).GetValue(modifier);

                                List<string> commands = (List<string>)modifier.GetType().GetField("command", BindingFlags.Public | BindingFlags.Instance).GetValue(modifier);

                                var value = (string)modifier.GetType().GetField("value", BindingFlags.Public | BindingFlags.Instance).GetValue(modifier);

                                var constant = ((bool)modifier.GetType().GetField("constant", BindingFlags.Public | BindingFlags.Instance).GetValue(modifier)).ToString();

                                if (commands.Count > 0 && !string.IsNullOrEmpty(commands[0]))
                                {
                                    //jn["objects"][i]["modifiers"][j] = new JSONArray();

                                    jn["objects"][i]["modifiers"][j]["type"] = type;
                                    if (type == 0)
                                    {
                                        jn["objects"][i]["modifiers"][j]["not"] = ((bool)modifier.GetType().GetField("not", BindingFlags.Public | BindingFlags.Instance).GetValue(modifier)).ToString();
                                    }

                                    for (int k = 0; k < commands.Count; k++)
                                    {
                                        if (!string.IsNullOrEmpty(commands[k]))
                                            jn["objects"][i]["modifiers"][j]["commands"][k] = commands[k];
                                    }

                                    jn["objects"][i]["modifiers"][j]["value"] = value;

                                    jn["objects"][i]["modifiers"][j]["const"] = constant;
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < __0.prefabObjects.Count; i++)
            {
                if (__0.prefabObjects[i] != null)
                {
                    jn["prefab_objects"][i]["id"] = __0.prefabObjects[i].ID;
                    jn["prefab_objects"][i]["pid"] = __0.prefabObjects[i].prefabID;
                    jn["prefab_objects"][i]["st"] = __0.prefabObjects[i].StartTime.ToString();

                    if (__0.prefabObjects[i].RepeatCount > 0)
                        jn["prefab_objects"][i]["rc"] = __0.prefabObjects[i].RepeatCount.ToString();
                    if (__0.prefabObjects[i].RepeatOffsetTime > 0f)
                        jn["prefab_objects"][i]["ro"] = __0.prefabObjects[i].RepeatOffsetTime.ToString();

                    jn["prefab_objects"][i]["ed"]["layer"] = __0.prefabObjects[i].editorData.Layer.ToString();
                    jn["prefab_objects"][i]["ed"]["bin"] = __0.prefabObjects[i].editorData.Bin.ToString();

                    if (__0.prefabObjects[i].editorData.locked)
                    {
                        jn["prefab_objects"][i]["ed"]["locked"] = __0.prefabObjects[i].editorData.locked.ToString();
                    }
                    if (__0.prefabObjects[i].editorData.collapse)
                    {
                        jn["prefab_objects"][i]["ed"]["shrink"] = __0.prefabObjects[i].editorData.collapse.ToString();
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        string type = "";
                        switch (j)
                        {
                            case 0:
                                {
                                    type = "pos";
                                    break;
                                }
                            case 1:
                                {
                                    type = "sca";
                                    break;
                                }
                            case 2:
                                {
                                    type = "rot";
                                    break;
                                }
                        }

                        if (type != "")
                        {
                            jn["prefab_objects"][i]["e"][j][type]["x"] = __0.prefabObjects[i].events[j].eventValues[0].ToString();
                            if (j != 2)
                            {
                                jn["prefab_objects"][i]["e"][j][type]["y"] = __0.prefabObjects[i].events[j].eventValues[1].ToString();
                            }
                        }
                    }
                }
            }
            __result = jn;
            return false;
        }
    }

    [HarmonyPatch(typeof(DataManager.GameData))]
    public class DataManagerGameDataPatch : MonoBehaviour
    {
        [HarmonyPatch("ParseBeatmap")]
        [HarmonyPrefix]
        static bool ParseBeatmapPatch(string _json)
        {
            Debug.LogFormat("{0}Parse Beatmap", FunctionsPlugin.className);
            DataManager.inst.StartCoroutine(Parser.ParseBeatmap(_json));
            return false;
        }
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
            BeatmapTheme themeCopy = new BeatmapTheme();
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
            BeatmapObject beatmapObject = null;
            DataManager.inst.StartCoroutine(Parser.ParseObject(__0, delegate (BeatmapObject _beatmapObject)
            {
                beatmapObject = _beatmapObject;
            }));

            __result = beatmapObject;
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
