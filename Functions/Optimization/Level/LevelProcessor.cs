using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using RTFunctions.Functions.Optimization.Objects;

using GameData = DataManager.GameData;

namespace RTFunctions.Functions.Optimization.Level
{
    public class LevelProcessor : Exists, IDisposable
    {
        public readonly LevelStorage level;
        public readonly Engine engine;
        public readonly ObjectConverter converter;

        public LevelProcessor(GameData gameData)
        {
            // Convert GameData to LevelObjects
            converter = new ObjectConverter(gameData);
            IEnumerable<ILevelObject> levelObjects = converter.ToLevelObjects();

            level = new LevelStorage(levelObjects);
            engine = new Engine(level);

            Debug.Log($"Loaded {level.Objects.Count} objects (original: {gameData.beatmapObjects.Count})");
        }

        public void Update(float time) => engine.Update(time);

        public void Dispose() => engine.Dispose();
    }
}
