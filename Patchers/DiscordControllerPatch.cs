using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using UnityEngine;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(DiscordController))]
    public class DiscordControllerPatch : MonoBehaviour
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        static void AwakePrefix(DiscordController __instance)
        {
            __instance.applicationId = "1176264603374735420";
        }
    }
}
