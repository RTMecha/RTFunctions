﻿using LSFunctions;
using RTFunctions.Functions.Components.Player;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Optimization;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RTFunctions.Functions.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager inst;
        public static string className = "[<color=#7F00FF>LevelManager</color>] " + PluginInfo.PLUGIN_VERSION + "\n";

        #region Path

        public static string Path
        {
            get => path;
            set => path = value;
        }

        static string path = "arcade";
        public static string ListPath => $"beatmaps/{Path}";
        public static string ListSlash => $"beatmaps/{Path}/";

        #endregion

        public static bool LevelEnded { get; set; }

        public static bool LoadingFromHere { get; set; }

        public static int CurrentLevelMode { get; set; }

        public static bool InEditor => EditorManager.inst;
        public static bool InGame => GameManager.inst;

        public static Level CurrentLevel { get; set; }

        public static List<Level> Levels { get; set; }

        public static List<Level> EditorLevels { get; set; }

        public static List<Level> ArcadeQueue { get; set; }

        public static int current;

        public static bool finished = false;

        public static int BoostCount { get; set; }

        public static Action OnLevelEnd { get; set; }

        public static int PlayedLevelCount { get; set; }

        void Awake()
        {
            inst = this;
            Levels = new List<Level>();
            EditorLevels = new List<Level>();
            ArcadeQueue = new List<Level>();

            if (!RTFile.FileExists(RTFile.ApplicationDirectory + "profile/saves.les") && RTFile.FileExists(RTFile.ApplicationDirectory + "settings/save.lss"))
                UpgradeProgress();
            else
                LoadProgress();
        }

        void Update()
        {
            if (!InEditor)
                EditorLevels.Clear();

            if (InEditor && EditorManager.inst.isEditing)
                BoostCount = 0;
        }

        public static IEnumerator Play(Level level)
        {
            LoadingFromHere = true;
            LevelEnded = false;

            if (level.playerData == null && Saves.Has(x => x.ID == level.id))
            {
                level.playerData = Saves.Find(x => x.ID == level.id);
            }

            CurrentLevel = level;

            Debug.Log($"{className}Switching to Game scene");

            bool inGame = RTHelpers.InGame;
            if (!inGame || EditorManager.inst)
                SceneManager.inst.LoadScene("Game");

            Debug.Log($"{className}Loading music...");

            if (!level.music)
                level.LoadAudioClip();

            if (ShapeManager.inst.loadedShapes)
                ShapeManager.inst.Load();

            if (RTHelpers.InEditor || !RTHelpers.InGame || !ShapeManager.inst.loadedShapes)
            {
                while (RTHelpers.InEditor || !RTHelpers.InGame || !ShapeManager.inst.loadedShapes)
                    yield return null;
            }

            WindowController.ResetResolution();
            WindowController.ResetTitle();

            Debug.Log($"{className}Parsing level...");

            GameManager.inst.gameState = GameManager.State.Parsing;
            var levelMode = level.LevelModes[Mathf.Clamp(CurrentLevelMode, 0, level.LevelModes.Length - 1)];
            Debug.Log($"{className}Level Mode: {levelMode}...");

            var rawJSON = RTFile.ReadFromFile(level.path + levelMode);
            rawJSON = UpdateBeatmap(rawJSON, level.metadata.beatmap.game_version);
            DataManager.inst.gameData = levelMode.Contains(".vgd") ? GameData.ParseVG(JSON.Parse(rawJSON)) : GameData.Parse(JSONNode.Parse(rawJSON));

            Debug.Log($"{className}Setting paths...");

            DataManager.inst.metaData = level.metadata;
            GameManager.inst.currentLevelName = level.metadata.song.title;
            GameManager.inst.basePath = level.path;

            Debug.Log($"{className}Updating states...");

            FunctionsPlugin.UpdateDiscordStatus($"Level: {level.metadata.song.title}", "In Arcade", "arcade");
            DataManager.inst.UpdateSettingBool("IsArcade", true);

            while (!GameManager.inst.introTitle && !GameManager.inst.introArtist)
                yield return null;

            GameManager.inst.introTitle.text = level.metadata.song.title;
            GameManager.inst.introArtist.text = level.metadata.artist.Name;

            Debug.Log($"{className}Playing music...");

            while (level.music == null)
                yield return null;

            AudioManager.inst.PlayMusic(null, level.music, true, 0.5f, false);
            AudioManager.inst.SetPitch(GameManager.inst.getPitch());
            GameManager.inst.songLength = level.music.length;

            Debug.Log($"{className}Setting Camera sizes...");

            if (RTFile.FileExists(level.path + "bg.mp4") && FunctionsPlugin.EnableVideoBackground.Value)
            {
                RTVideoManager.inst.Play(level.path + "bg.mp4", 1f);
                while (!RTVideoManager.inst.videoPlayer.isPrepared)
                    yield return null;
            }
            else if (RTFile.FileExists(level.path + "bg.mov") && FunctionsPlugin.EnableVideoBackground.Value)
            {
                RTVideoManager.inst.Play(level.path + "bg.mov", 1f);
                while (!RTVideoManager.inst.videoPlayer.isPrepared)
                    yield return null;
            }
            else
            {
                RTVideoManager.inst.Stop();
            }

            EventManager.inst.cam.rect = new Rect(0f, 0f, 1f, 1f);
            EventManager.inst.camPer.rect = new Rect(0f, 0f, 1f, 1f);

            Debug.Log($"{className}Updating checkpoints...");

            GameManager.inst.UpdateTimeline();
            GameManager.inst.ResetCheckpoints();

            Debug.Log($"{className}Spawning...");
            BoostCount = 0;
            if (InputDataManager.inst.players.Count == 0)
            {
                var customPlayer = new Data.Player.CustomPlayer(true, 0, null);
                InputDataManager.inst.players.Add(customPlayer);
            }

            PlayerManager.allowController = InputDataManager.inst.players.Count == 0;

            PlayerManager.LoadLocalModels?.Invoke();

            PlayerManager.AssignPlayerModels();

            RTPlayer.LockBoost = false;
            RTPlayer.SpeedMultiplier = 1f;

            GameManager.inst.introAnimator.SetTrigger("play");
            GameManager.inst.SpawnPlayers(DataManager.inst.gameData.beatmapData.checkpoints[0].pos);

            EventManager.inst?.updateEvents();
            if (ModCompatibility.sharedFunctions.ContainsKey("EventsCoreResetOffsets"))
            {
                ((Action)ModCompatibility.sharedFunctions["EventsCoreResetOffsets"])?.Invoke();
            }

            yield return inst.StartCoroutine(Updater.IUpdateObjects(true));

            //ObjectManager.inst.updateObjects();

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
                        Clear();
                        Updater.OnLevelEnd();
                        SceneManager.inst.LoadScene("Main Menu");
                    };

                var level = new Level(path.Replace("level.lsb", "").Replace("level.vgd", ""));
                inst.StartCoroutine(Play(level));
                return;
            }

            Debug.LogError($"{className}Couldn't load level from {path} as it doesn't exist.");
        }

        public static void Clear()
        {
            DG.Tweening.DOTween.Clear();
            DataManager.inst.gameData = null;
            DataManager.inst.gameData = new GameData();
            InputDataManager.inst.SetAllControllerRumble(0f);
        }

        public static List<Level> SortLevels(List<Level> levels, int orderby, bool ascend)
        {
            switch (orderby)
            {
                case 0: return
                            (ascend ? levels.OrderBy(x => x.icon != SteamWorkshop.inst.defaultSteamImageSprite) :
                            levels.OrderByDescending(x => x.icon != SteamWorkshop.inst.defaultSteamImageSprite)).ToList();
                case 1: return
                            (ascend ? levels.OrderBy(x => x.metadata.artist.Name) :
                            levels.OrderByDescending(x => x.metadata.artist.Name)).ToList();
                case 2: return
                            (ascend ? levels.OrderBy(x => x.metadata.creator.steam_name) :
                            levels.OrderByDescending(x => x.metadata.creator.steam_name)).ToList();
                case 3: return
                            (ascend ? levels.OrderBy(x => System.IO.Path.GetFileName(x.path)) :
                            levels.OrderByDescending(x => System.IO.Path.GetFileName(x.path))).ToList();
                case 4: return
                            (ascend ? levels.OrderBy(x => x.metadata.song.title) :
                            levels.OrderByDescending(x => x.metadata.song.title)).ToList();
                case 5: return
                            (ascend ? levels.OrderBy(x => x.metadata.song.difficulty) :
                            levels.OrderByDescending(x => x.metadata.song.difficulty)).ToList();
                case 6: return
                            (ascend ? levels.OrderBy(x => x.metadata.beatmap.date_edited) :
                            levels.OrderByDescending(x => x.metadata.beatmap.date_edited)).ToList();
            }

            return levels;
        }

        public static void Sort(int orderby, bool ascend)
        {
            switch (orderby)
            {
                case 0:
                    {
                        Levels =
                            (ascend ? Levels.OrderBy(x => x.icon != SteamWorkshop.inst.defaultSteamImageSprite) :
                            Levels.OrderByDescending(x => x.icon != SteamWorkshop.inst.defaultSteamImageSprite)).ToList();
                        break;
                    }
                case 1:
                    {
                        Levels =
                            (ascend ? Levels.OrderBy(x => x.metadata.artist.Name) :
                            Levels.OrderByDescending(x => x.metadata.artist.Name)).ToList();
                        break;
                    }
                case 2:
                    {
                        Levels =
                            (ascend ? Levels.OrderBy(x => x.metadata.creator.steam_name) :
                            Levels.OrderByDescending(x => x.metadata.creator.steam_name)).ToList();
                        break;
                    }
                case 3:
                    {
                        Levels =
                            (ascend ? Levels.OrderBy(x => System.IO.Path.GetFileName(x.path)) :
                            Levels.OrderByDescending(x => System.IO.Path.GetFileName(x.path))).ToList();
                        break;
                    }
                case 4:
                    {
                        Levels =
                            (ascend ? Levels.OrderBy(x => x.metadata.song.title) :
                            Levels.OrderByDescending(x => x.metadata.song.title)).ToList();
                        break;
                    }
                case 5:
                    {
                        Levels =
                            (ascend ? Levels.OrderBy(x => x.metadata.song.difficulty) :
                            Levels.OrderByDescending(x => x.metadata.song.difficulty)).ToList();
                        break;
                    }
                case 6:
                    {
                        Levels =
                            (ascend ? Levels.OrderBy(x => x.metadata.beatmap.date_edited) :
                            Levels.OrderByDescending(x => x.metadata.beatmap.date_edited)).ToList();
                        break;
                    }
            }
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

            // To fix alpha screwing the font up
            _json = _json.Replace("<font=LiberationSans>", "<font=LiberationSans SDF>").Replace("<font=\"LiberationSans\">", "<font=LiberationSans SDF>");

            return _json;
        }

        public static void UpgradeProgress()
        {
            if (!RTFile.FileExists(RTFile.ApplicationDirectory + "profile/saves.les") && RTFile.FileExists(RTFile.ApplicationDirectory + "settings/save.lss"))
            {
                var decryptedJSON = LSEncryption.DecryptText(RTFile.ReadFromFile(RTFile.ApplicationDirectory + "settings/saves.lss"), SaveManager.inst.encryptionKey);

                var jn = JSON.Parse(decryptedJSON);

                for (int i = 0; i < jn["arcade"].Count; i++)
                {
                    var js = jn["arcade"][i];

                    Saves.Add(new PlayerData
                    {
                        ID = js["level_data"]["id"],
                        Completed = js["play_data"]["finished"].AsBool,
                        Hits = js["play_data"]["hits"].AsInt,
                        Deaths = js["play_data"]["deaths"].AsInt,
                    });
                }

                SaveProgress();
            }
        }

        public static void SaveProgress()
        {
            var jn = JSON.Parse("{}");
            for (int i = 0; i < Saves.Count; i++)
            {
                jn["lvl"][i] = Saves[i].ToJSON();
            }

            jn["played_count"] = PlayedLevelCount.ToString();

            if (!RTFile.DirectoryExists(RTFile.ApplicationDirectory + "profile"))
                Directory.CreateDirectory(RTFile.ApplicationDirectory + "profile");

            var json = jn.ToString();
            json = LSEncryption.EncryptText(json, SaveManager.inst.encryptionKey);
            RTFile.WriteToFile(RTFile.ApplicationDirectory + "profile/saves.les", json);
        }

        public static void LoadProgress()
        {
            if (!RTFile.FileExists(RTFile.ApplicationDirectory + "profile/saves.les"))
                return;

            Saves.Clear();

            string decryptedJSON = LSEncryption.DecryptText(RTFile.ReadFromFile(RTFile.ApplicationDirectory + "profile/saves.les"), SaveManager.inst.encryptionKey);

            var jn = JSON.Parse(decryptedJSON);

            for (int i = 0; i < jn["lvl"].Count; i++)
            {
                Saves.Add(PlayerData.Parse(jn["lvl"][i]));
            }

            if (!string.IsNullOrEmpty(jn["played_count"]))
            {
                PlayedLevelCount = jn["played_count"].AsInt;
            }
        }
        
        public static PlayerData GetPlayerData(string id) => Saves.Find(x => x.ID == id);

        public static DataManager.LevelRank GetLevelRank(Level level)
            => level.playerData != null && DataManager.inst.levelRanks.Has(LevelRankPredicate(level)) ? DataManager.inst.levelRanks.Find(LevelRankPredicate(level)) : DataManager.inst.levelRanks[0];

        public static float CalculateAccuracy(int hits, float length)
            => 100f / ((hits / (length / PlayerManager.AcurracyDivisionAmount)) + 1f);

        public static Predicate<DataManager.LevelRank> LevelRankPredicate(Level level)
             => x => level.playerData != null && level.playerData.Hits >= x.minHits && level.playerData.Hits <= x.maxHits;

        public static List<PlayerData> Saves { get; set; } = new List<PlayerData>();
        public class PlayerData
        {
            public string ID { get; set; }
            public bool Completed { get; set; }
            public int Hits { get; set; }
            public int Deaths { get; set; }
            public int Boosts { get; set; }
            public int PlayedTimes { get; set; }
            public float TimeInLevel { get; set; }
            public float Percentage { get; set; }
            public float LevelLength { get; set; }

            public void Update()
            {
                if (Hits > GameManager.inst.hits.Count)
                    Hits = GameManager.inst.hits.Count;

                if (Deaths > GameManager.inst.deaths.Count)
                    Deaths = GameManager.inst.deaths.Count;

                var l = AudioManager.inst.CurrentAudioSource.clip.length;
                if (LevelLength != l)
                    LevelLength = l;

                float calc = AudioManager.inst.CurrentAudioSource.time / AudioManager.inst.CurrentAudioSource.clip.length * 100f;

                if (Percentage < calc)
                    Percentage = calc;
            }

            public static PlayerData Parse(JSONNode jn)
            {
                var playerData = new PlayerData();
                playerData.ID = jn["id"];
                playerData.Completed = jn["c"].AsBool;
                playerData.Hits = jn["h"].AsInt;
                playerData.Deaths = jn["d"].AsInt;
                playerData.Boosts = jn["b"].AsInt;
                playerData.PlayedTimes = jn["pt"].AsInt;
                playerData.TimeInLevel = jn["t"].AsFloat;
                playerData.Percentage = jn["p"].AsFloat;
                playerData.LevelLength = jn["l"].AsFloat;
                return playerData;
            }

            public JSONNode ToJSON()
            {
                var jn = JSON.Parse("{}");
                jn["id"] = ID;
                jn["c"] = Completed.ToString();
                jn["h"] = Hits.ToString();
                jn["d"] = Deaths.ToString();
                jn["b"] = Boosts.ToString();
                jn["pt"] = PlayedTimes.ToString();
                jn["t"] = TimeInLevel.ToString();
                jn["p"] = Percentage.ToString();
                jn["l"] = LevelLength.ToString();
                return jn;
            }

            public override string ToString() => ID;
        }
    }
}
