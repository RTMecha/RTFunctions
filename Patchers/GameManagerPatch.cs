﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using LSFunctions;

using RTFunctions.Functions;
using RTFunctions.Functions.Animation;
using RTFunctions.Functions.Animation.Keyframe;
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
        public static GameManager Instance { get => GameManager.inst; set => GameManager.inst = value; }

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
        static bool UpdatePrefix(GameManager __instance)
        {
            if (__instance.gameState == GameManager.State.Paused && !LevelManager.LevelEnded && InputDataManager.inst.menuActions.Cancel.WasPressed)
            {
                __instance.menuUI?.GetComponent<InterfaceController>()?.SwitchBranch("unpause");
            }

            if (__instance.gameState == GameManager.State.Playing)
            {
                for (int i = 0; i < GameData.Current.levelModifiers.Count; i++)
                {
                    GameData.Current.levelModifiers[i].Activate();
                }

                if (EditorManager.inst == null)
                {
                    foreach (var player in PlayerManager.Players)
                    {
                        if (player.Player && player.Player.Actions.Pause.WasPressed)
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
                __instance.StartCoroutine(ReverseToCheckpointLoop(__instance));
            }
            else if (__instance.gameState == GameManager.State.Playing)
            {
                if (AudioManager.inst.CurrentAudioSource.clip != null && EditorManager.inst == null
                    && AudioManager.inst.CurrentAudioSource.time /*+ 0.1f*/ >= __instance.songLength - 0.1f)
                    if (!LevelManager.LevelEnded)
                        __instance.GoToNextLevel();
            }
            else if (__instance.gameState == GameManager.State.Finish)
            {
                if (AudioManager.inst.CurrentAudioSource.clip != null && EditorManager.inst == null
                    && AudioManager.inst.CurrentAudioSource.time /*+ 0.1f*/ >= __instance.songLength - 0.1f
                    && FunctionsPlugin.ReplayLevel.Value && LevelManager.LevelEnded)
                    AudioManager.inst.SetMusicTime(0f);
            }

            if (__instance.gameState == GameManager.State.Playing || __instance.gameState == GameManager.State.Reversing)
                __instance.UpdateEventSequenceTime();

            if (AudioManager.inst.CurrentAudioSource.clip != null)
                __instance.prevAudioTime = AudioManager.inst.CurrentAudioSource.time;

            return false;
        }

        public static IEnumerator ReverseToCheckpointLoop(GameManager __instance)
        {
            if (!__instance.isReversing)
            {
                __instance.playingCheckpointAnimation = true;
                __instance.isReversing = true;

                int index = DataManager.inst.gameData.beatmapData.checkpoints.FindLastIndex(x => x.time < AudioManager.inst.CurrentAudioSource.time);
                if (index < 0)
                    index = 0;

                var checkpoint = DataManager.inst.gameData.beatmapData.checkpoints[index];

                var animation = new AnimationManager.Animation("Reverse");
                animation.floatAnimations = new List<AnimationManager.Animation.AnimationObject<float>>
                {
                    new AnimationManager.Animation.AnimationObject<float>(new List<IKeyframe<float>>
                    {
                        new FloatKeyframe(0f, AudioManager.inst.CurrentAudioSource.pitch, Ease.Linear),
                        new FloatKeyframe(1f, -1.5f, Ease.CircIn)
                    }, delegate (float x)
                    {
                        if (AudioManager.inst.CurrentAudioSource.time > 1f)
                            AudioManager.inst.SetPitch(x);
                        else
                            AudioManager.inst.CurrentAudioSource.time = 1f;
                    }),
                };

                animation.onComplete = delegate ()
                {
                    AnimationManager.inst.RemoveID(animation.id);
                };

                AnimationManager.inst.Play(animation);

                //AudioManager.inst.SetPitch(-1.5f);
                AudioManager.inst.PlaySound("rewind");

                yield return new WaitForSeconds(2f);

                float time = Mathf.Clamp(checkpoint.time + 0.01f, 0.1f, AudioManager.inst.CurrentAudioSource.clip.length);
                if (EditorManager.inst == null && (DataManager.inst.GetSettingInt("ArcadeDifficulty", 0) == 2 || DataManager.inst.GetSettingInt("ArcadeDifficulty", 0) == 3))
                    time = 0.1f;

                AudioManager.inst.CurrentAudioSource.time = time;
                __instance.gameState = GameManager.State.Playing;

                AudioManager.inst.CurrentAudioSource.Play();
                AudioManager.inst.SetPitch(__instance.getPitch());

                __instance.UpdateEventSequenceTime();
                __instance.isReversing = false;

                yield return new WaitForSeconds(0.1f);

                __instance.SpawnPlayers(checkpoint.pos);
                __instance.playingCheckpointAnimation = false;
                checkpoint = null;
            }
            yield break;
        }

        [HarmonyPatch("FixedUpdate")]
        [HarmonyPrefix]
        static bool FixedUpdatePrefix()
        {
            if (DataManager.inst && DataManager.inst.gameData != null && DataManager.inst.gameData.beatmapData != null && DataManager.inst.gameData.beatmapData.checkpoints != null &&
                DataManager.inst.gameData.beatmapData.checkpoints.Count > 0 && Instance.gameState == GameManager.State.Playing)
            {
                Instance.UpcomingCheckpoint = Instance.GetClosestIndex(DataManager.inst.gameData.beatmapData.checkpoints, AudioManager.inst.CurrentAudioSource.time);
                Instance.UpcomingCheckpointIndex = DataManager.inst.gameData.beatmapData.checkpoints.FindIndex(x => x == Instance.UpcomingCheckpoint);
                if (Instance.timeline && AudioManager.inst.CurrentAudioSource.clip != null && Instance.gameState == GameManager.State.Playing)
                {
                    float num = AudioManager.inst.CurrentAudioSource.time * 400f / AudioManager.inst.CurrentAudioSource.clip.length;
                    if (Instance.timeline.transform.Find("Base/position"))
                        Instance.timeline.transform.Find("Base/position").AsRT().anchoredPosition = new Vector2(num, 0f);
                    else
                        Instance.UpdateTimeline();
                }
                Instance.lastCheckpointState = DataManager.inst.gameData.beatmapData.GetWhichCheckpointBasedOnTime(AudioManager.inst.CurrentAudioSource.time);
            }
            Instance.playerGUI.SetActive((EditorManager.inst && !EditorManager.inst.isEditing) || !EditorManager.inst);
            return false;
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

                if (GameStorageManager.inst)
                {
                    if (GameStorageManager.inst.checkpointImages.Count > 0)
                        foreach (var image in GameStorageManager.inst.checkpointImages)
                        {
                            image.color = beatmapTheme.guiColor;
                        }

                    GameStorageManager.inst.timelinePlayer.color = beatmapTheme.guiColor;
                    GameStorageManager.inst.timelineLeftCap.color = beatmapTheme.guiColor;
                    GameStorageManager.inst.timelineRightCap.color = beatmapTheme.guiColor;
                    GameStorageManager.inst.timelineLine.color = beatmapTheme.guiColor;
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
            LevelManager.LevelEnded = true;
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

        [HarmonyPatch("UpdateTimeline")]
        [HarmonyPrefix]
        static bool UpdateTimelinePrefix()
        {
            if (Instance.timeline && AudioManager.inst.CurrentAudioSource.clip != null && DataManager.inst.gameData.beatmapData != null)
            {
                if (GameStorageManager.inst)
                    GameStorageManager.inst.checkpointImages.Clear();

                LSHelpers.DeleteChildren(Instance.timeline.transform.Find("elements"), true);
                foreach (var checkpoint in DataManager.inst.gameData.beatmapData.checkpoints)
                {
                    if (checkpoint.time > 0.5f)
                    {
                        var gameObject = Instantiate(Instance.checkpointPrefab);
                        gameObject.name = string.Concat(new object[] { "Checkpoint [", checkpoint.name, "] - [", checkpoint.time, "]" });
                        gameObject.transform.SetParent(Instance.timeline.transform.Find("elements"));
                        float num = checkpoint.time * 400f / AudioManager.inst.CurrentAudioSource.clip.length;
                        gameObject.transform.AsRT().anchoredPosition = new Vector2(num, 0f);
                        if (GameStorageManager.inst)
                            GameStorageManager.inst.checkpointImages.Add(gameObject.GetComponent<Image>());
                    }
                }
            }
            return false;
        }
    }
}
