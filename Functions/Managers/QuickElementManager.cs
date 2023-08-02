﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;

using TMPro;
using SimpleJSON;

using RTFunctions.Functions;

using BeatmapObject = DataManager.GameData.BeatmapObject;

namespace RTFunctions.Functions.Managers
{
    public class QuickElementManager : MonoBehaviour
    {
        void Awake()
        {

        }

        public static Dictionary<string, QuickElement> quickElements = new Dictionary<string, QuickElement>();

        public static void CreateNewQuickElement(string name)
        {
            var quickElement = new QuickElement();
            quickElement.name = name;

            var kf = new QuickElement.Keyframe();
            kf.text = "null";
            kf.time = 1f;
        }

        public static string ConvertQuickElement(BeatmapObject beatmapObject, string element)
        {
            if (quickElements.ContainsKey(element) && quickElements[element].keyframes.Count > 0)
            {
                var quickElement = quickElements[element];

                var times = new List<float>();
                var texts = new List<string>();

                float totaltime = 0f;
                foreach (var kf in quickElement.keyframes)
                {
                    texts.Add(kf.text);
                    times.Add(totaltime);
                    totaltime += kf.time;
                }

                var currentTime = AudioManager.inst.CurrentAudioSource.time - beatmapObject.StartTime;
                var index = times.FindIndex(x => x > currentTime) - 1;

                if (index >= 0 && texts.Count > index)
                    return texts[index];
                else if (texts.Count > 0)
                    return texts[texts.Count - 1];
                else
                    return "error";
            }

            return "";
        }

        public static void SaveExternalQuickElements()
        {
            var dictionary = new Dictionary<string, QuickElement>();

            if (Resources.LoadAll<QuickElement>("terminal/quick-elements") != null)
            {
                foreach (QuickElement quickElement in Resources.LoadAll<QuickElement>("terminal/quick-elements"))
                {
                    dictionary.Add(quickElement.name, quickElement);
                }
            }

            foreach (var quickElement in quickElements)
            {
                if (!dictionary.ContainsKey(quickElement.Key))
                {
                    var jn = JSON.Parse("{}");

                    jn["name"] = quickElement.Value.name;

                    for (int i = 0; i < quickElement.Value.keyframes.Count; i++)
                    {
                        jn["keys"][i]["text"] = quickElement.Value.keyframes[i].text;
                        jn["keys"][i]["time"] = quickElement.Value.keyframes[i].time.ToString();
                    }

                    RTFile.WriteToFile("beatmaps/quickelements" + quickElement.Value.name, jn.ToString(3));
                }
            }
        }

        public static IEnumerator LoadExternalQuickElements()
        {
            if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "beatmaps/quickelements"))
            {
                var files = Directory.GetFiles(RTFile.ApplicationDirectory + "beatmaps/quickelements", "*.lsqe");
                foreach (var file in files)
                {
                    var json = FileManager.inst.LoadJSONFileRaw(file);
                    var jn = JSON.Parse(json);

                    var quickElement = new QuickElement();

                    quickElement.name = Path.GetFileName(file).Replace(".lsqe", "");
                    if (!string.IsNullOrEmpty(jn["name"]))
                    {
                        quickElement.name = jn["name"];
                    }

                    if (jn["keys"] != null)
                    {
                        for (int i = 0; i < jn["keys"].Count; i++)
                        {
                            var keyframe = new QuickElement.Keyframe();
                            keyframe.text = jn["keys"][i]["text"];

                            keyframe.time = 1f;
                            if (float.TryParse(jn["keys"][i]["time"], out float result))
                            {
                                keyframe.time = result;
                            }
                        }
                    }
                }
            }

            yield break;
        }

        public static IEnumerator PlayQuickElement(TextMeshPro tmp, QuickElement quickElement)
        {
            string replaceStr = quickElement.keyframes[0].text;
            var currentKeyframe = quickElement.keyframes[0];

            for (int i = 0; i < quickElement.keyframes.Count; i++)
            {
                string newText = tmp.text;
                yield return new WaitForSeconds(currentKeyframe.time);

                currentKeyframe = quickElement.keyframes[i];
                newText = newText.Replace(replaceStr, currentKeyframe.text);
                if (tmp.text.Contains(replaceStr))
                    tmp.SetText(newText);

                replaceStr = quickElement.keyframes[i].text;
                newText = null;
            }

            yield break;
        }

        public static IEnumerator UpdateQuickElement(TextMeshProUGUI tmp, List<QuickElement.Keyframe> keyframes, int _instance)
        {
            string replaceStr = keyframes[0].text;
            QuickElement.Keyframe currentKeyframe = keyframes[0];
            int i = 0;
            while (tmp.text.Contains(replaceStr))
            {
                string newText = tmp.text;
                yield return new WaitForSeconds(currentKeyframe.time);
                int num;
                for (int inst = 0; inst <= _instance; inst = num + 1)
                {
                    yield return new WaitForEndOfFrame();
                    num = inst;
                }
                num = i;
                i = num + 1;
                if (i > keyframes.Count - 1)
                {
                    i = 0;
                }
                currentKeyframe = keyframes[i];
                newText = newText.Replace(replaceStr, currentKeyframe.text);
                if (tmp.text.Contains(replaceStr))
                {
                    tmp.SetText(newText);
                }
                replaceStr = keyframes[i].text;
                newText = null;
            }
            yield break;
        }
    }
}
