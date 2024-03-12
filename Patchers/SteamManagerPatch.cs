using System.Linq;

using HarmonyLib;

using UnityEngine;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Managers.Networking;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(SteamManager))]
    public class SteamManagerPatch : MonoBehaviour
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void AwakePostfix(SteamManager __instance)
        {
            SteamWorkshopManager.Init(__instance);
        }
    }
}
