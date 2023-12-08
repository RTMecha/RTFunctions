using System;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using LSFunctions;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.Managers;

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
            var beatmapTheme = GameManager.inst.LiveTheme;
            GameManager.inst.LiveTheme = new BeatmapTheme
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
                effectColors = beatmapTheme.objectColors,
            };
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

        [HarmonyPatch("UpdateTheme")]
        [HarmonyPrefix]
        static bool UpdateThemePrefix(GameManager __instance)
        {
            if (!ModCompatibility.mods.ContainsKey("EventsCore"))
            {
                var beatmapTheme = RTHelpers.BeatmapTheme;
                if (__instance.CameraPerspective.GetComponent<Camera>().backgroundColor != beatmapTheme.backgroundColor)
                    __instance.CameraPerspective.GetComponent<Camera>().backgroundColor = beatmapTheme.backgroundColor;

                var componentsInChildren = __instance.timeline.GetComponentsInChildren<Image>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].color = beatmapTheme.guiColor;
                }
                int num = 0;
                foreach (var customPlayer in InputDataManager.inst.players)
                {
                    if (customPlayer != null && customPlayer.player != null)
                    {
                        customPlayer.player.SetColor(beatmapTheme.GetPlayerColor(num % 4), beatmapTheme.guiAccentColor);
                    }
                    num++;
                }
                if (EditorManager.inst == null && AudioManager.inst.CurrentAudioSource.time < 15f)
                {
                    if (__instance.introTitle.color != beatmapTheme.guiColor)
                        __instance.introTitle.color = beatmapTheme.guiColor;
                    if (__instance.introArtist.color != beatmapTheme.guiColor)
                        __instance.introArtist.color = beatmapTheme.guiColor;
                }
                foreach (var image in __instance.guiImages)
                {
                    if (image.color != beatmapTheme.guiColor)
                        image.color = beatmapTheme.guiColor;
                }
                var componentsInChildren2 = __instance.menuUI.GetComponentsInChildren<TextMeshProUGUI>();
                for (int i = 0; i < componentsInChildren2.Length; i++)
                {
                    componentsInChildren2[i].color = LSColors.InvertBlackWhiteColor(beatmapTheme.backgroundColor);
                }
            }
            else
                FunctionsPlugin.EventsCoreGameThemePrefix?.Invoke();

            return false;
        }
    }
}
