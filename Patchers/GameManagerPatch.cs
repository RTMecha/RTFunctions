using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;

using LSFunctions;

namespace RTFunctions.Patchers
{
    public delegate void LevelEventHandler();

    [HarmonyPatch(typeof(GameManager))]
    public class GameManagerPatch : MonoBehaviour
    {
        public static event LevelEventHandler LevelStart;
        public static event LevelEventHandler LevelEnd;

        [HarmonyPatch("PlayLevel")]
        [HarmonyPostfix]
        static void PlayLevelPostfix() => LevelStart?.Invoke();

        public static void StartInvoke() => LevelStart?.Invoke();

        public static void EndInvoke() => LevelEnd?.Invoke();

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPostfix()
        {
            FunctionsPlugin.SetCameraRenderDistance();
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(GameManager __instance)
        {
            if (__instance.LiveTheme.objectColors.Count == 9)
            {
                for (int i = 0; i < 9; i++)
                {
                    __instance.LiveTheme.objectColors.Add(LSColors.pink900);
                }
            }

            // I have no idea what this was doing here
            if (InputDataManager.inst.gameActions != null && InputDataManager.inst.gameActions.Escape.WasPressed)
            {
                __instance.UnPause();
            }
        }

        [HarmonyPatch("getPitch")]
        [HarmonyPrefix]
        static bool getPitch(ref float __result)
        {
            if (EditorManager.inst != null)
            {
                __result = 1f;
                return false;
            }
            __result = new List<float>
            {
                0.1f,
                0.5f,
                0.8f,
                1f,
                1.2f,
                1.5f,
                2f,
                3f
            }[Mathf.Clamp(0, DataManager.inst.GetSettingEnum("ArcadeGameSpeed", 3), 7)];
            return false;
        }
    }
}
