﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Configuration;

using UnityEngine;
using UnityEngine.UI;

using RTFunctions.Functions.Components.Player;
using RTFunctions.Functions.Data.Player;
using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager inst;

        void Awake()
        {
            inst = this;
        }

        public static bool LoadFromGlobalPlayersInArcade { get; set; }

        public static Dictionary<string, PlayerModel> PlayerModels { get; set; } = new Dictionary<string, PlayerModel>();

        public static Dictionary<int, string> PlayerModelsIndex { get; set; } = new Dictionary<int, string>
        {
            { 0, "0" },
            { 1, "0" },
            { 2, "0" },
            { 3, "0" },
        };

        public static List<ConfigEntry<string>> PlayerIndexes { get; set; } = new List<ConfigEntry<string>>();

        public static List<CustomPlayer> Players => InputDataManager.inst.players.Select(x => x as CustomPlayer).ToList();

        public static GameObject healthImages;
        public static Transform healthParent;
        public static Sprite healthSprite;

        public static void SetupImages(GameManager __instance)
        {
            var health = __instance.playerGUI.transform.Find("Health");
            health.gameObject.SetActive(true);
            health.GetChild(0).gameObject.SetActive(true);
            for (int i = 1; i < 4; i++)
            {
                Destroy(health.GetChild(i).gameObject);
            }

            for (int i = 3; i < 5; i++)
            {
                Destroy(health.GetChild(0).GetChild(i).gameObject);
            }
            var gm = health.GetChild(0).gameObject;
            healthImages = gm;
            var text = gm.AddComponent<Text>();

            text.alignment = TextAnchor.MiddleCenter;
            text.font = Font.GetDefault();
            text.enabled = false;

            if (gm.transform.Find("Image"))
            {
                healthSprite = gm.transform.Find("Image").GetComponent<Image>().sprite;
            }

            gm.transform.SetParent(null);
            healthParent = health;
        }

        public static Action LoadGlobalModels { get; set; }
        public static Action LoadLocalModels { get; set; }
        public static Action SaveGlobalModels { get; set; }
        public static Action SaveLocalModels { get; set; }

        public static Action LoadIndexes { get; set; }
        public static Action ClearPlayerModels { get; set; }

        public static Action CreateNewPlayerModel { get; set; }
        public static Action<string> DuplicatePlayerModel { get; set; }
        public static Action<int, string> SetPlayerModel { get; set; }

        public static string GetPlayerModelIndex(int index) => PlayerModelsIndex[index];

        public static void SetPlayerModelIndex(int index, int _id)
        {
            string e = PlayerModels.ElementAt(_id).Key;

            PlayerModelsIndex[index] = e;
        }

        public static int GetPlayerModelInt(PlayerModel _model) => PlayerModels.Values.ToList().IndexOf(_model);

        public static void UpdatePlayers()
        {
            if (InputDataManager.inst)
                foreach (var player in Players.Where(x => x.Player).Select(x => x.Player))
                {
                    if (EditorManager.inst != null || DataManager.inst.GetSettingEnum("ArcadeDifficulty", 1) == 0)
                        player.UpdatePlayer();
                }
        }

        public static void SpawnPlayer(CustomPlayer customPlayer, Vector3 pos)
        {
            var gameObject = GameManager.inst.PlayerPrefabs[0].Duplicate(GameManager.inst.players.transform, "Player " + (customPlayer.index + 1));
            gameObject.layer = 8;
            gameObject.SetActive(true);
            Destroy(gameObject.GetComponent<Player>());
            Destroy(gameObject.GetComponentInChildren<PlayerTrail>());

            gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            gameObject.transform.Find("Player").localPosition = new Vector3(pos.x, pos.y, 0f);
            gameObject.transform.localRotation = Quaternion.identity;

            var player = gameObject.GetComponent<RTPlayer>();

            if (!player)
                player = gameObject.AddComponent<RTPlayer>();

            player.CustomPlayer = customPlayer;
            player.PlayerModel = customPlayer.PlayerModel;
            player.playerIndex = customPlayer.index;
            customPlayer.Player = player;
            customPlayer.GameObject = player.gameObject;

            player.Spawn();

            player.UpdatePlayer();

            if (customPlayer.device == null)
            {
                player.Actions = EditorManager.inst && InputDataManager.inst.players.Count == 1 ? RTHelpers.CreateWithBothBindings() : InputDataManager.inst.keyboardListener;
                player.isKeyboard = true;
                player.faceController = EditorManager.inst != null && InputDataManager.inst.players.Count == 1 ? FaceController.CreateWithBothBindings() : FaceController.CreateWithKeyboardBindings();
            }
            else
            {
                var myGameActions = MyGameActions.CreateWithJoystickBindings();
                myGameActions.Device = customPlayer.device;
                player.Actions = myGameActions;
                player.isKeyboard = false;

                var faceController = FaceController.CreateWithJoystickBindings();
                faceController.Device = customPlayer.device;
                player.faceController = faceController;
            }

            foreach (var path in player.path)
            {
                if (path.transform != null)
                {
                    path.pos = new Vector3(pos.x, pos.y);
                }
            }

            if (EditorManager.inst == null)
            {
                if (DataManager.inst.GetSettingInt("ArcadeDifficulty", 0) == 3 || DataManager.inst.GetSettingInt("ArcadeDifficulty", 0) == 2)
                {
                    player.playerDeathEvent += delegate (Vector3 _val)
                    {
                        if (InputDataManager.inst.players.All(x => x is CustomPlayer && (x as CustomPlayer).Player == null || !(x as CustomPlayer).Player.PlayerAlive))
                        {
                            GameManager.inst.lastCheckpointState = -1;
                            GameManager.inst.ResetCheckpoints();
                            GameManager.inst.hits.Clear();
                            GameManager.inst.deaths.Clear();
                            GameManager.inst.gameState = GameManager.State.Reversing;
                        }
                    };
                }
                else
                {
                    player.playerDeathEvent += delegate (Vector3 _val)
                    {
                        if (InputDataManager.inst.players.All(x => x is CustomPlayer && (x as CustomPlayer).Player == null || !(x as CustomPlayer).Player.PlayerAlive))
                        {
                            GameManager.inst.gameState = GameManager.State.Reversing;
                        }
                    };
                }
                if (player.playerIndex == 0)
                {
                    player.playerDeathEvent += delegate (Vector3 _val)
                    {
                        GameManager.inst.deaths.Add(new SaveManager.SaveGroup.Save.PlayerDataPoint(_val, GameManager.inst.UpcomingCheckpointIndex, AudioManager.inst.CurrentAudioSource.time));
                    };
                    player.playerHitEvent += delegate (int _health, Vector3 _val)
                    {
                        GameManager.inst.hits.Add(new SaveManager.SaveGroup.Save.PlayerDataPoint(_val, GameManager.inst.UpcomingCheckpointIndex, AudioManager.inst.CurrentAudioSource.time));
                    };
                }
            }
            else
            {
                player.playerDeathEvent += delegate (Vector3 _val)
                {
                    if (InputDataManager.inst.players.All(x => x is CustomPlayer && (x as CustomPlayer).Player == null || !(x as CustomPlayer).Player.PlayerAlive))
                    {
                        //__instance.ResetCheckpoints();
                        GameManager.inst.gameState = GameManager.State.Reversing;
                    }
                };
            }

            customPlayer.active = true;
        }

        public static void SpawnPlayer(PlayerModel playerModel, Transform transform, int index, Vector3 pos)
        {
            var gameObject = GameManager.inst.PlayerPrefabs[0].Duplicate(transform, "Player");
            gameObject.layer = 8;
            gameObject.SetActive(true);
            Destroy(gameObject.GetComponent<Player>());
            Destroy(gameObject.GetComponentInChildren<PlayerTrail>());

            gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            gameObject.transform.Find("Player").localPosition = new Vector3(pos.x, pos.y, 0f);
            gameObject.transform.localRotation = Quaternion.identity;

            var player = gameObject.GetComponent<RTPlayer>();

            if (!player)
                player = gameObject.AddComponent<RTPlayer>();

            player.PlayerModel = playerModel;
            player.playerIndex = index;

            player.Spawn();

            player.UpdatePlayer();

            foreach (var path in player.path)
            {
                if (path.transform != null)
                {
                    path.pos = new Vector3(pos.x, pos.y);
                }
            }
        }

        public static void RespawnPlayers()
        {
            foreach (var player in Players.Where(x => x.Player).Select(x => x.Player))
            {
                DestroyImmediate(player.health);
                DestroyImmediate(player.gameObject);
            }

            AssignPlayerModels();

            var nextIndex = DataManager.inst.gameData.beatmapData.checkpoints.FindIndex(x => x.time > AudioManager.inst.CurrentAudioSource.time);
            var prevIndex = nextIndex - 1;
            if (prevIndex < 0)
                prevIndex = 0;

            GameManager.inst.SpawnPlayers(DataManager.inst.gameData.beatmapData.checkpoints.Count > prevIndex && DataManager.inst.gameData.beatmapData.checkpoints[prevIndex] != null ?
                DataManager.inst.gameData.beatmapData.checkpoints[prevIndex].pos : EventManager.inst.cam.transform.position);
        }

        public static void RespawnPlayer(int index)
        {
            DestroyImmediate(Players.Where(x => x.Player).Select(x => x.Player).ToList()[index].health);
            DestroyImmediate(Players.Where(x => x.Player).Select(x => x.Player).ToList()[index].gameObject);

            if (PlayerModelsIndex.Count > index && PlayerModels.ContainsKey(PlayerModelsIndex[index]))
                Players[index].CurrentPlayerModel = PlayerModelsIndex[index];

            var nextIndex = DataManager.inst.gameData.beatmapData.checkpoints.FindIndex(x => x.time > AudioManager.inst.CurrentAudioSource.time);
            var prevIndex = nextIndex - 1;
            if (prevIndex < 0)
                prevIndex = 0;

            GameManager.inst.SpawnPlayers(DataManager.inst.gameData.beatmapData.checkpoints.Count > prevIndex && DataManager.inst.gameData.beatmapData.checkpoints[prevIndex] != null ?
                DataManager.inst.gameData.beatmapData.checkpoints[prevIndex].pos : EventManager.inst.cam.transform.position);
        }

        public static void AssignPlayerModels()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (PlayerModelsIndex.Count > i && PlayerModels.ContainsKey(PlayerModelsIndex[i]))
                    Players[i].CurrentPlayerModel = PlayerModelsIndex[i];
            }
        }
    }
}