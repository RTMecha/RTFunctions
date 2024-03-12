using System.Linq;

using HarmonyLib;

using UnityEngine;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Managers.Networking;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(SteamWrapper.Achievements))]
    public class SteamWrapperAchievementsPatch
    {
        [HarmonyPatch("SetAchievement")]
        [HarmonyPrefix]
        static bool SetAchievementPrefix(string __0)
        {
            if (!SteamWorkshopManager.inst || !SteamWorkshopManager.inst.Initialized)
                return false;

            SteamWorkshopManager.inst.steamUser.SetAchievement(__0);
            return false;
        }

        [HarmonyPatch("GetAchievement")]
        [HarmonyPrefix]
        static bool GetAchievementPrefix(ref bool __result, string __0)
        {
            __result = SteamWorkshopManager.inst.steamUser.GetAchievement(__0);
            return false;
        }

        [HarmonyPatch("ClearAchievement")]
        [HarmonyPrefix]
        static bool ClearAchievementPrefix(string __0)
        {
            if (!SteamWorkshopManager.inst || !SteamWorkshopManager.inst.Initialized)
                return false;

            SteamWorkshopManager.inst.steamUser.ClearAchievement(__0);
            return false;
        }
    }
}
