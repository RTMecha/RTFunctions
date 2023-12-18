using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Managers.Networking;

namespace RTFunctions.Functions
{
    /// <summary>
    /// The class for handling a level. Make sure the path ends in a "/"
    /// </summary>
    public class Level : Exists
    {
        public Level(string path)
        {
            this.path = path;

            if (RTFile.FileExists($"{path}metadata.lsb"))
                metadata = Metadata.Parse(JSON.Parse(RTFile.ReadFromFile($"{path}metadata.lsb")));

            icon = RTFile.FileExists($"{path}level.jpg") ? SpriteManager.LoadSprite($"{path}level.jpg") : ArcadeManager.inst.defaultImage;

            if (metadata)
                id = metadata.LevelBeatmap.beatmap_id;

            if (RTFile.FileExists($"{path}modes.lsb"))
            {
                var jn = JSON.Parse(RTFile.ReadFromFile($"{path}modes.lsb"));
                LevelModes = new string[jn["paths"].Count];
                for (int i = 0; i < jn["paths"].Count; i++)
                {
                    LevelModes[i] = jn["paths"][i];
                }
            }
            else
                LevelModes = new string[1]
                {
                    "level.lsb",
                };
        }

        public string path;

        public Sprite icon;

        public AudioClip music;

        public string id;

        public Metadata metadata;

        public int currentMode = 0;
        public string[] LevelModes { get; set; }

        public void LoadAudioClip()
        {
            if (RTFile.FileExists(path + "level.ogg") && !music)
            {
                FunctionsPlugin.inst.StartCoroutine(AlephNetworkManager.DownloadAudioClip("file://" + path + "level.ogg", AudioType.OGGVORBIS, delegate (AudioClip audioClip)
                {
                    music = audioClip;
                }));
            }
            if (RTFile.FileExists(path + "level.wav") && !music)
            {
                FunctionsPlugin.inst.StartCoroutine(AlephNetworkManager.DownloadAudioClip("file://" + path + "level.wav", AudioType.WAV, delegate (AudioClip audioClip)
                {
                    music = audioClip;
                }));
            }
        }

        public PlayerData playerData = new PlayerData();
        public class PlayerData
        {
            public PlayerData()
            {

            }
            
            public PlayerData(int hits, int deaths, int boosts, bool completed, int version)
            {
                this.hits = hits;
                this.deaths = deaths;
                this.boosts = boosts;
                this.completed = completed;
                this.version = version;
            }

            public int hits = -1;
            public int deaths = -1;
            public int boosts = -1;
            public bool completed;
            public int version;
        }
    }
}
