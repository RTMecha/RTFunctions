using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using LSFunctions;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.Data.Player;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Patchers
{
    public delegate void LevelEventHandler();

    [HarmonyPatch(typeof(GameManager))]
    public class GameManagerPatch : MonoBehaviour
    {
        public static event LevelEventHandler LevelStart;
        public static event LevelEventHandler LevelEnd;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPostfix(GameManager __instance)
        {
            FunctionsPlugin.SetCameraRenderDistance();
            FunctionsPlugin.SetAntiAliasing();
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
                objectColors = new List<Color>
                {
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                },
                backgroundColors = beatmapTheme.backgroundColors,
                effectColors = new List<Color>
                {
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                    LSColors.pink500,
                },
            };

            var pm1 = new PlayerModel(__instance.PlayerPrefabs[0]);
            var pm2 = new PlayerModel(__instance.PlayerPrefabs[1]);

            pm1.values["Base ID"] = "0";
            pm2.values["Base ID"] = "1";

            if (!PlayerManager.PlayerModels.ContainsKey("0"))
                PlayerManager.PlayerModels.Add("0", pm1);
            else
                PlayerManager.PlayerModels["0"] = pm1;

            if (!PlayerManager.PlayerModels.ContainsKey("1"))
                PlayerManager.PlayerModels.Add("1", pm2);
            else
                PlayerManager.PlayerModels["1"] = pm1;

            //if (EditorManager.inst != null || PlayerManager.LoadFromGlobalPlayersInArcade)
            //    PlayerManager.LoadGlobalModels?.Invoke();
            //else
            //    PlayerManager.LoadLocalModels?.Invoke();

            (EditorManager.inst != null || PlayerManager.LoadFromGlobalPlayersInArcade ? PlayerManager.LoadGlobalModels : PlayerManager.LoadLocalModels)?.Invoke();

            PlayerManager.SetupImages(__instance);

            __instance.gameObject.AddComponent<GameStorageManager>();
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void UpdatePrefix(GameManager __instance)
        {
            if (__instance.gameState == GameManager.State.Playing)
            {
                if (EditorManager.inst == null)
                {
                    foreach (InputDataManager.CustomPlayer player in InputDataManager.inst.players)
                    {
                        if (player.player && player.player.Actions.Pause.WasPressed)
                            __instance.Pause();
                    }
                }
                if (__instance.checkpointsActivated != null && __instance.checkpointsActivated.Length != 0 && AudioManager.inst.CurrentAudioSource.time >= (double)__instance.UpcomingCheckpoint.time && !__instance.playingCheckpointAnimation && __instance.UpcomingCheckpointIndex != -1 && !__instance.checkpointsActivated[__instance.UpcomingCheckpointIndex] && (EditorManager.inst != null && !EditorManager.inst.isEditing || EditorManager.inst == null))
                {
                    __instance.playingCheckpointAnimation = true;
                    __instance.SpawnPlayers(__instance.UpcomingCheckpoint.pos);
                    __instance.StartCoroutine(__instance.PlayCheckpointAnimation(__instance.UpcomingCheckpointIndex));
                }
            }
            if (__instance.gameState == GameManager.State.Reversing && !__instance.isReversing)
            {
                __instance.StartCoroutine(__instance.ReverseToCheckpointLoop());
            }
            else if (__instance.gameState == GameManager.State.Playing)
            {
                if (AudioManager.inst.CurrentAudioSource.clip != null &&
                    EditorManager.inst == null &&
                    AudioManager.inst.CurrentAudioSource.time + 0.1f >= __instance.songLength)
                    __instance.GoToNextLevel();
            }

            if (__instance.gameState == GameManager.State.Playing || __instance.gameState == GameManager.State.Reversing)
                __instance.UpdateEventSequenceTime();

            if (AudioManager.inst.CurrentAudioSource.clip != null)
                __instance.prevAudioTime = AudioManager.inst.CurrentAudioSource.time;
        }

        [HarmonyPatch("PlayLevel")]
        [HarmonyPostfix]
        static void PlayLevelPostfix() => LevelStart?.Invoke();

        public static void StartInvoke() => LevelStart?.Invoke();

        public static void EndInvoke() => LevelEnd?.Invoke();

        [HarmonyPatch("LoadLevelCurrent")]
        [HarmonyPrefix]
        static bool LoadLevelCurrentPrefix(GameManager __instance)
        {
            if (!LevelManager.LoadingFromHere && LevelManager.CurrentLevel)
            {
                LevelManager.finished = false;
                __instance.StartCoroutine(LevelManager.Play(LevelManager.CurrentLevel));
            }
            return false;
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
                GameStorageManager.inst.perspectiveCam.backgroundColor = beatmapTheme.backgroundColor;

                var componentsInChildren = __instance.timeline.GetComponentsInChildren<Image>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].color = beatmapTheme.guiColor;
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
                FunctionsPlugin.EventsCoreGameThemePrefix?.Invoke(__instance);

            return false;
        }

        [HarmonyPatch("GoToNextLevelLoop")]
        [HarmonyPrefix]
        static bool GoToNextLevelLoopPrefix(GameManager __instance, ref IEnumerator __result)
        {
            __result = GoToNextLevelLoop(__instance);
            return false;
        }

        static IEnumerator GoToNextLevelLoop(GameManager __instance)
        {
            if (AudioManager.inst.masterVol <= 0f)
                SteamWrapper.inst.achievements.SetAchievement("NO_AUDIO");

            __instance.gameState = GameManager.State.Finish;
            Time.timeScale = 1f;
            DG.Tweening.DOTween.Clear();
            InputDataManager.inst.SetAllControllerRumble(0f);
            LevelManager.finished = true;
            LevelManager.OnLevelEnd?.Invoke();
            yield break;
        }


        [HarmonyPatch("SpawnPlayers")]
        [HarmonyPrefix]
        static bool SpawnPlayersPrefix(GameManager __instance, Vector3 __0)
        {
            foreach (var customPlayer in InputDataManager.inst.players.Select(x => x as CustomPlayer))
            {
                if (customPlayer.Player == null)
                {
                    PlayerManager.SpawnPlayer(customPlayer, __0);
                }
                else
                {
                    Debug.LogFormat("{0}Player {1} already exists!", FunctionsPlugin.className, customPlayer.index);
                }
            }
            return false;
        }

        [HarmonyPatch("Pause")]
        [HarmonyPrefix]
        static bool PausePrefix(GameManager __instance)
        {
            if (__instance.gameState == GameManager.State.Playing)
            {
                __instance.menuUI.GetComponent<InterfaceController>().SwitchBranch("main");
                __instance.menuUI.GetComponentInChildren<Image>().enabled = true;
                AudioManager.inst.CurrentAudioSource.Pause();
                InputDataManager.inst.SetAllControllerRumble(0f);
                __instance.gameState = GameManager.State.Paused;
            }
            return false;
        }

        [HarmonyPatch("UnPause")]
        [HarmonyPrefix]
        static bool UnPausePrefix(GameManager __instance)
        {
            if (__instance.gameState == GameManager.State.Paused)
            {
                __instance.menuUI.GetComponent<InterfaceController>().SwitchBranch("empty");
                __instance.menuUI.GetComponentInChildren<Image>().enabled = false;
                AudioManager.inst.CurrentAudioSource.UnPause();
                __instance.gameState = GameManager.State.Playing;
            }
            return false;
        }
    }
}
