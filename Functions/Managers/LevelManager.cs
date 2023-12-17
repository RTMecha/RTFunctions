﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Optimization;

namespace RTFunctions.Functions.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager inst;
        public static string className = "[<color=#7F00FF>LevelManager</color>] " + PluginInfo.PLUGIN_VERSION + "\n";

        public static bool LoadingFromHere { get; set; }

        public static bool InEditor => EditorManager.inst;
        public static bool InGame => GameManager.inst;

        public static Level CurrentLevel { get; set; }

        public static List<Level> Levels { get; set; }

        public static List<Level> EditorLevels { get; set; }

        public static Action OnLevelEnd { get; set; }

        void Awake()
        {
            inst = this;
            Levels = new List<Level>();
            EditorLevels = new List<Level>();
        }

        void Update()
        {
            if (!InEditor)
                EditorLevels.Clear();
        }

        public static IEnumerator Play(Level level)
        {
            LoadingFromHere = true;

            CurrentLevel = level;

            Debug.Log($"{className}Switching to Game scene");

            if (!GameManager.inst || EditorManager.inst)
                SceneManager.inst.LoadScene("Game");

            Debug.Log($"{className}");

            level.LoadAudioClip();

            while (!GameManager.inst)
                yield return null;

            Debug.Log($"{className}Parsing level...");

            GameManager.inst.gameState = GameManager.State.Parsing;
            var rawJSON = RTFile.ReadFromFile(level.path + "level.lsb");
            rawJSON = UpdateBeatmap(rawJSON, level.metadata.beatmap.game_version);
            DataManager.inst.gameData = GameData.Parse(JSONNode.Parse(rawJSON));

            Debug.Log($"{className}Setting paths...");

            GameManager.inst.currentLevelName = level.metadata.song.title;
            GameManager.inst.basePath = level.path;

            Debug.Log($"{className}Updating states...");

            DiscordController.inst.OnStateChange("Level: " + GameManager.inst.currentLevelName);
            DataManager.inst.UpdateSettingBool("IsArcade", true);

            while (!GameManager.inst.introTitle && !GameManager.inst.introArtist)
                yield return null;

            GameManager.inst.introTitle.text = level.metadata.song.title;
            GameManager.inst.introArtist.text = level.metadata.artist.Name;

            Debug.Log($"{className}Playing music...");

            AudioManager.inst.PlayMusic(null, level.music, true, 0.5f, false);
            AudioManager.inst.SetPitch(GameManager.inst.getPitch());
            GameManager.inst.songLength = level.music.length;

            Debug.Log($"{className}Setting Camera sizes...");

            GameManager.inst.Camera.GetComponent<Camera>().rect = new Rect(0f, 0f, 1f, 1f);
            GameManager.inst.CameraPerspective.GetComponent<Camera>().rect = new Rect(0f, 0f, 1f, 1f);

            Debug.Log($"{className}Updating checkpoints...");

            GameManager.inst.UpdateTimeline();
            GameManager.inst.ResetCheckpoints();

            Debug.Log($"{className}Spawning...");

            GameManager.inst.introAnimator.SetTrigger("play");
            GameManager.inst.SpawnPlayers(DataManager.inst.gameData.beatmapData.checkpoints[0].pos);

            ObjectManager.inst.updateObjects();

            Debug.Log($"{className}Done!");

            GameManager.inst.gameState = GameManager.State.Playing;

            LoadingFromHere = false;
        }

        /// <summary>
        /// Loads a level from anywhere. For example: LevelManager.Load("E:/4.1.16/beatmaps/story/Apocrypha/level.lsb");
        /// </summary>
        /// <param name="path"></param>
        public static void Load(string path, bool setLevelEnd = true)
        {
            if (RTFile.FileExists(path))
            {
                Debug.Log($"{className}Loading level from {path}");

                if (setLevelEnd)
                    OnLevelEnd = delegate ()
                    {
                        DG.Tweening.DOTween.Clear();
                        DataManager.inst.gameData = null;
                        DataManager.inst.gameData = new GameData();
                        Updater.OnLevelEnd();
                        SceneManager.inst.LoadScene("Main Menu");
                    };

                var level = new Level(path.Replace("level.lsb", ""));
                inst.StartCoroutine(Play(level));
                return;
            }

            Debug.LogError($"{className}Couldn't load level from {path} as it doesn't exist.");
        }

        public static string UpdateBeatmap(string _json, string _version)
        {
            Debug.Log("[ -- Updating Beatmap! -- ] - [" + _version + "]");
            if (DataManager.GetVersion(_version, 0) <= 3 && DataManager.GetVersion(_version, 1) <= 7 && DataManager.GetVersion(_version, 2) <= 26)
            {
                Debug.Log("value_x -> x & value_y -> y");
                _json = _json.Replace("\"value_x\"", "\"x\"");
                _json = _json.Replace("\"value_y\"", "\"y\"");
            }
            if (DataManager.GetVersion(_version, 0) <= 3 && DataManager.GetVersion(_version, 1) <= 7 && DataManager.GetVersion(_version, 2) <= 42)
            {
                Debug.Log("text 4 -> 5");
                _json = _json.Replace("\"shape\": \"4\"", "\"shape\": \"5\"");
            }
            if (DataManager.GetVersion(_version, 0) <= 3 && DataManager.GetVersion(_version, 1) <= 8 && DataManager.GetVersion(_version, 2) <= 15)
            {
                Debug.Log("Add parent relationship if none");
            }
            if (DataManager.GetVersion(_version, 0) <= 3 && DataManager.GetVersion(_version, 1) <= 8 && DataManager.GetVersion(_version, 2) <= 25)
            {
                Debug.Log("background_objects -> bg_objects");
                _json = _json.Replace("\"background_objects\"", "\"bg_objects\"");
                Debug.Log("reactive_settings -> r_set");
                _json = _json.Replace("\"reactive_settings\"", "\"r_set\"");
            }
            if (DataManager.GetVersion(_version, 0) <= 3 && DataManager.GetVersion(_version, 1) <= 8 && DataManager.GetVersion(_version, 2) <= 48)
            {
                Debug.Log("is_random -> r");
                _json = _json.Replace("\"is_random\":\"False\"", "\"r\":\"0\"").Replace("\"is_random\":\"True\"", "\"r\":\"1\"");
                _json = _json.Replace("\"is_random\": \"False\"", "\"r\": \"0\"").Replace("\"is_random\": \"True\"", "\"r\": \"1\"");
                Debug.Log("origin -> o");
                _json = _json.Replace("\"origin\"", "\"o\"");
                Debug.Log("time -> t");
                _json = _json.Replace("\"time\"", "\"t\"");
                Debug.Log("start_time -> st");
                _json = _json.Replace("\"start_time\"", "\"st\"");
                Debug.Log("editor_data -> ed");
                _json = _json.Replace("\"editor_data\"", "\"ed\"");
                Debug.Log("value_random_x -> rx");
                _json = _json.Replace("\"value_random_x\"", "\"rx\"");
                Debug.Log("value_random_y -> ry");
                _json = _json.Replace("\"value_random_y\"", "\"ry\"");
                Debug.Log("value_z -> z");
                _json = _json.Replace("\"value_z\"", "\"z\"").Replace("\"value_z2\"", "\"z2\"");
                Debug.Log("curve_type -> ct");
                _json = _json.Replace("\"curve_type\"", "\"ct\"");
                Debug.Log("p_type -> pt");
                _json = _json.Replace("\"p_type\"", "\"pt\"");
                Debug.Log("parent -> p");
                _json = _json.Replace("\"parent\"", "\"p\"");
                Debug.Log("helper -> h");
                _json = _json.Replace("\"helper\"", "\"h\"");
                Debug.Log("depth -> d");
                _json = _json.Replace("\"depth\"", "\"d\"");
                Debug.Log("prefab_id -> pid");
                _json = _json.Replace("\"prefab_id\"", "\"pid\"");
                Debug.Log("prefab_inst_id -> piid");
                _json = _json.Replace("\"prefab_inst_id\"", "\"piid\"");
                Debug.Log("shape_option -> so");
                _json = _json.Replace("\"shape_option\"", "\"so\"");
            }
            return _json;
        }
    }
}
