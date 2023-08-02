using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using HarmonyLib;

using SimpleJSON;

using RTFunctions.Functions;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(SaveManager))]
    public class SaveManagerPatch : MonoBehaviour
    {
        [HarmonyPatch("ApplySettingsFile")]
        [HarmonyPostfix]
        private static void ApplySettingsFilePostfix(SaveManager __instance)
        {
            if (RTFile.FileExists(RTFile.ApplicationDirectory + "settings/functions.lss"))
            {
                string rawProfileJSON = FileManager.inst.LoadJSONFile("settings/functions.lss");

                JSONNode jn = JSON.Parse(rawProfileJSON);

                if (string.IsNullOrEmpty(jn["general"]["updated_speed"]))
                {
                    jn["general"]["updated_speed"] = "True";
                    DataManager.inst.UpdateSettingEnum("ArcadeGameSpeed", DataManager.inst.GetSettingEnum("ArcadeGameSpeed", 2) + 1);

                    RTFile.WriteToFile("settings/functions.lss", jn.ToString(3));
                }
            }
            else
            {
                JSONNode jn = JSON.Parse("{}");

                jn["general"]["updated_speed"] = "True";
                DataManager.inst.UpdateSettingEnum("ArcadeGameSpeed", DataManager.inst.GetSettingEnum("ArcadeGameSpeed", 2) + 1);

                RTFile.WriteToFile("settings/functions.lss", jn.ToString(3));
            }
        }
    }
}
