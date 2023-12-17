using System.Collections.Generic;

using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager inst;

        public static List<Level> Levels { get; set; } = new List<Level>();

        void Awake()
        {
            inst = this;
        }

        void Update()
        {

        }

        public void Play(Level level)
        {
            if (RTFile.FileExists(level.path + "level.lsb"))
            {
                var gameData = GameData.Parse(JSONNode.Parse(RTFile.ReadFromFile(level.path + "level.lsb")));
                DataManager.inst.gameData = gameData;


            }
        }
    }
}
