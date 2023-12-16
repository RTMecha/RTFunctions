using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using UnityEngine;

namespace RTFunctions.Patchers
{
    public class DiscordControllerPatch : MonoBehaviour
    {
        static DiscordController Instance { get => DiscordController.inst; set => DiscordController.inst = value; }
        public static void Init()
        {
            Patcher.CreatePatch(AccessTools.Method(typeof(DiscordController), "Awake"), PatchType.Postfix, (Action)AwakePostfix);
        }

        static void AwakePostfix()
        {
            Instance.applicationId = "1176264603374735420";
        }
    }
}
