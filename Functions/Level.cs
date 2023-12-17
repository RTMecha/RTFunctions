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

            if (RTFile.FileExists(path + "metadata.lsb"))
                metadata = Metadata.Parse(JSON.Parse(RTFile.ReadFromFile(path + "metadata.lsb")));

            icon = RTFile.FileExists(path + "level.jpg") ? SpriteManager.LoadSprite(path + "level.jpg") : ArcadeManager.inst.defaultImage;

            if (metadata)
                id = metadata.LevelBeatmap.beatmap_id;
        }

        public string path;

        public Sprite icon;

        public AudioClip music;

        public string id;

        public Metadata metadata;

        public void LoadAudioClip()
        {
            if (RTFile.FileExists(path + "level.ogg") && !music)
            {
                FunctionsPlugin.inst.StartCoroutine(AlephNetworkManager.DownloadAudioClip(path + "level.ogg", AudioType.OGGVORBIS, delegate (AudioClip audioClip)
                {
                    music = audioClip;
                }));
            }

        }
    }
}
