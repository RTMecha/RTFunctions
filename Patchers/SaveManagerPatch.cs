using UnityEngine;

using HarmonyLib;

using SimpleJSON;

using RTFunctions.Functions.IO;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(SaveManager))]
    public class SaveManagerPatch : MonoBehaviour
    {
        [HarmonyPatch("ApplySettingsFile")]
        [HarmonyPostfix]
        static void ApplySettingsFilePostfix(SaveManager __instance)
        {
            FunctionsPlugin.prevFullscreen = FunctionsPlugin.Fullscreen.Value;
            FunctionsPlugin.Fullscreen.Value = DataManager.inst.GetSettingBool("FullScreen", false);

            FunctionsPlugin.prevResolution = FunctionsPlugin.Resolution.Value;
            FunctionsPlugin.Resolution.Value = (Resolutions)DataManager.inst.GetSettingInt("Resolution_i", 5);

            FunctionsPlugin.prevMasterVol = FunctionsPlugin.MasterVol.Value;
            FunctionsPlugin.MasterVol.Value = DataManager.inst.GetSettingInt("MasterVolume", 9);

            FunctionsPlugin.prevMusicVol = FunctionsPlugin.MusicVol.Value;
            FunctionsPlugin.MusicVol.Value = DataManager.inst.GetSettingInt("MusicVolume", 9);

            FunctionsPlugin.prevSFXVol = FunctionsPlugin.SFXVol.Value;
            FunctionsPlugin.SFXVol.Value = DataManager.inst.GetSettingInt("EffectsVolume", 9);

            FunctionsPlugin.prevLanguage = FunctionsPlugin.Language.Value;
            FunctionsPlugin.Language.Value = (FunctionsPlugin.Lang)DataManager.inst.GetSettingInt("Language_i", 0);

            FunctionsPlugin.prevControllerRumble = FunctionsPlugin.ControllerRumble.Value;
            FunctionsPlugin.ControllerRumble.Value = DataManager.inst.GetSettingBool("ControllerVibrate", true);

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

        [HarmonyPatch("OnApplicationQuit")]
        [HarmonyPrefix]
        static bool OnApplicationQuitPrefix()
        {
            DiscordController.inst.OnDisableDiscord();
            Debug.Log("Run Quit Function");
            PlayerPrefs.DeleteAll();
            return false;
        }

        [HarmonyPatch("LoadSavesFile")]
        [HarmonyPrefix]
        static bool LoadSavesFilePrefix() => false;
    }
}
