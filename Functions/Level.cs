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
            else if (RTFile.FileExists($"{path}metadata.vgm"))
                metadata = Metadata.Parse(JSON.Parse(RTFile.ReadFromFile($"{path}metadata.vgm")));

            icon = RTFile.FileExists($"{path}level.jpg") ? SpriteManager.LoadSprite($"{path}level.jpg") : RTFile.FileExists($"{path}cover.jpg") ? SpriteManager.LoadSprite($"{path}cover.jpg") : ArcadeManager.inst.defaultImage;

            if (metadata)
                id = metadata.LevelBeatmap.beatmap_id;

            if (RTFile.FileExists($"{path}modes.lsms"))
            {
                var jn = JSON.Parse(RTFile.ReadFromFile($"{path}modes.lsms"));
                LevelModes = new string[jn["paths"].Count + 1];
                LevelModes[0] = RTFile.FileExists($"{path}level.lsb") ? "level.lsb" : "level.vgd";
                for (int i = 1; i < jn["paths"].Count + 1; i++)
                {
                    LevelModes[i] = jn["paths"][i - 1];
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
            else if (RTFile.FileExists(path + "level.wav") && !music)
            {
                FunctionsPlugin.inst.StartCoroutine(AlephNetworkManager.DownloadAudioClip("file://" + path + "level.wav", AudioType.WAV, delegate (AudioClip audioClip)
                {
                    music = audioClip;
                }));
            }
            else if (RTFile.FileExists(path + "level.mp3") && !music)
            {
                music = LSFunctions.LSAudio.CreateAudioClipUsingMP3File(path + "level.mp3");
            }
            else if (RTFile.FileExists(path + "audio.ogg") && !music)
            {
                FunctionsPlugin.inst.StartCoroutine(AlephNetworkManager.DownloadAudioClip("file://" + path + "audio.ogg", AudioType.OGGVORBIS, delegate (AudioClip audioClip)
                {
                    music = audioClip;
                }));
            }
            else if (RTFile.FileExists(path + "audio.wav") && !music)
            {
                FunctionsPlugin.inst.StartCoroutine(AlephNetworkManager.DownloadAudioClip("file://" + path + "audio.wav", AudioType.WAV, delegate (AudioClip audioClip)
                {
                    music = audioClip;
                }));
            }
            else if (RTFile.FileExists(path + "audio.mp3") && !music)
            {
                music = LSFunctions.LSAudio.CreateAudioClipUsingMP3File(path + "audio.mp3");
            }
        }

        public LevelManager.PlayerData playerData;

        public override string ToString() => System.IO.Path.GetFileName(path);
    }
}
