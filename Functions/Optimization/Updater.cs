using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using RTFunctions.Patchers;
using RTFunctions.Functions.Optimization.Level;
using RTFunctions.Functions.Optimization.Objects;
using RTFunctions.Functions.Animation;
using RTFunctions.Functions.Animation.Keyframe;

using BeatmapObject = DataManager.GameData.BeatmapObject;
using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;

namespace RTFunctions.Functions.Optimization
{
    public class Updater
    {
        public static LevelProcessor levelProcessor;

        public static bool Active
        {
            get
            {
                return levelProcessor && levelProcessor.level;
            }
        }

        public static bool HasObject(BeatmapObject beatmapObject) => Active && (LevelObject)levelProcessor.level.objects.Find(x => x.ID == beatmapObject.id);

        public static bool TryGetObject(BeatmapObject beatmapObject, out LevelObject levelObject)
        {
            if (HasObject(beatmapObject))
            {
                levelObject = (LevelObject)levelProcessor.level.objects.Find(x => x.ID == beatmapObject.id);
                return true;
            }

            levelObject = null;
            return false;
        }

        public static void OnLevelStart()
        {
            Debug.Log("Loading level");

            levelProcessor = new LevelProcessor(DataManager.inst.gameData);
        }

        public static void OnLevelEnd()
        {
            Debug.Log("Cleaning up level");

            levelProcessor.Dispose();
            levelProcessor = null;
        }

        public static void OnLevelTick() => levelProcessor?.Update(AudioManager.inst.CurrentAudioSource.time);

        /// <summary>
        /// Gets a BeatmapObjects associated LevelObject. Useful for other mods that want to retrieve this data.
        /// </summary>
        /// <param name="_beatmapObject"></param>
        /// <returns>ILevelObject from specified BeatmapObject.</returns>
        public static ILevelObject GetLevelObject(BeatmapObject _beatmapObject)
        {
            if (levelProcessor == null || levelProcessor.level == null)
                return null;

            var objects = levelProcessor.level.objects;
            if (objects == null || objects.Count < 1)
            {
                return null;
            }

            return objects.Find(x => x.ID == _beatmapObject.id);
        }

        /// <summary>
        /// Gets the BeatmapObjects' Unity GameObject.
        /// </summary>
        /// <param name="beatmapObject"></param>
        /// <returns>GameObject from specified BeatmapObject. Null if object does not exist.</returns>
        public static GameObject GetGameObject(BeatmapObject beatmapObject)
        {
            try
            {
                // If LevelProcessor is null, then no level has been initialized.
                if (levelProcessor == null)
                    return null;

                // If Level is null, then same as above.
                if (levelProcessor.level == null)
                    return null;

                var objects = levelProcessor.level.objects;

                // If Objects list is null
                if (objects == null)
                    return null;

                // If list is empty
                if (objects.Count < 1)
                    return null;

                // If Objects list does not contain a matching item to the BeatmapObjects' ID.
                if (objects.Find(x => x.ID != null && x.ID == beatmapObject.id) == null)
                    return null;

                return ((LevelObject)objects.Find(x => x.ID == beatmapObject.id)).visualObject.GameObject;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a BeatmapObjects associated LevelObject. Useful for other mods that want to retrieve this data.
        /// </summary>
        /// <param name="_beatmapObject"></param>
        /// <param name="objects"></param>
        /// <returns>ILevelObject from specified BeatmapObject.</returns>
        public static ILevelObject GetILevelObject(BeatmapObject _beatmapObject, List<ILevelObject> objects)
        {
            if (objects == null || objects.Count < 1)
            {
                return null;
            }

            return objects.Find(x => x.ID == _beatmapObject.id);
        }

        /// <summary>
        /// Gets a LevelObject with the specified ID. Useful for other mods that want to retrieve this data.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="objects"></param>
        /// <returns>ILevelObject with matching ID.</returns>
        public static ILevelObject GetILevelObject(string id, List<ILevelObject> objects)
        {
            if (objects == null || objects.Count < 1)
            {
                return null;
            }

            return objects.Find(x => x.ID == id);
        }

        /// <summary>
        /// Gets all the data it needs to pass to other methods and updates the LevelObjects.
        /// </summary>
        /// <param name="_objectSelection"></param>
        /// <param name="reinsert"></param>
        public static void updateProcessor(ObjEditor.ObjectSelection _objectSelection, bool reinsert = true)
        {
            if (_objectSelection.IsPrefab() || _objectSelection.GetObjectData() == null)
                return;

            var lp = levelProcessor;
            if (lp != null)
            {
                var level = levelProcessor.level;
                var converter = levelProcessor.converter;
                var engine = levelProcessor.engine;
                var objectSpawner = engine.objectSpawner;

                if (level != null && converter != null)
                {
                    var objects = level.objects;

                    FunctionsPlugin.inst.StartCoroutine(RecacheSequences(_objectSelection.GetObjectData(), converter, reinsert));
                    FunctionsPlugin.inst.StartCoroutine(updateObjects(_objectSelection.GetObjectData(), level, objects, converter, objectSpawner, reinsert));
                }
            }
        }

        // Not used
        //public static void updateProcessor(BeatmapObject _beatmapObject, bool reinsert = true)
        //{
        //    var levelProcessor = Instance.levelProcessor;
        //    if (levelProcessor != null)
        //    {
        //        var level = Instance.levelProcessor.level;
        //        var converter = Instance.levelProcessor.converter;
        //        var engine = Instance.levelProcessor.engine;
        //        var objectSpawner = engine.objectSpawner;

        //        if (level != null && converter != null)
        //        {
        //            var objects = level.objects;

        //            Instance.StartCoroutine(RecacheSequences(_beatmapObject, converter, reinsert));
        //            Instance.StartCoroutine(updateObjects(_beatmapObject, level, objects, converter, objectSpawner, reinsert));
        //        }
        //    }
        //}

        /// <summary>
        /// Recaches all the keyframe sequences related to the BeatmapObject.
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="converter"></param>
        /// <param name="reinsert"></param>
        /// <returns></returns>
        public static IEnumerator RecacheSequences(BeatmapObject bm, ObjectConverter converter, bool reinsert = true)
        {
            if (converter.cachedSequences.ContainsKey(bm.id))
            {
                converter.cachedSequences[bm.id] = null;
                converter.cachedSequences.Remove(bm.id);
            }

            foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
            {
                if (beatmapObject.parent == bm.id)
                {
                    // Recursive recaching.
                    FunctionsPlugin.inst.StartCoroutine(RecacheSequences(beatmapObject, converter));
                }
            }

            if (reinsert)
            {
                ObjectConverter.CachedSequences collection = new ObjectConverter.CachedSequences();
                // For the mods that add Z axis to position keyframes.
                if (bm.events[0][0].eventValues.Length > 2)
                {
                    collection = new ObjectConverter.CachedSequences()
                    {
                        Position3DSequence = converter.GetVector3Sequence(bm.events[0], new Vector3Keyframe(0.0f, Vector3.zero, Ease.Linear)),
                        ScaleSequence = converter.GetVector2Sequence(bm.events[1], new Vector2Keyframe(0.0f, Vector2.one, Ease.Linear)),
                        RotationSequence = converter.GetFloatSequence(bm.events[2], new FloatKeyframe(0.0f, 0.0f, Ease.Linear), true)
                    };
                }
                // If array is regular length
                else
                {
                    collection = new ObjectConverter.CachedSequences()
                    {
                        PositionSequence = converter.GetVector2Sequence(bm.events[0], new Vector2Keyframe(0.0f, Vector2.zero, Ease.Linear)),
                        ScaleSequence = converter.GetVector2Sequence(bm.events[1], new Vector2Keyframe(0.0f, Vector2.one, Ease.Linear)),
                        RotationSequence = converter.GetFloatSequence(bm.events[2], new FloatKeyframe(0.0f, 0.0f, Ease.Linear), true)
                    };
                }

                // Empty objects don't need a color sequence, so it is not cached
                if (bm.objectType != ObjectType.Empty)
                {
                    // For mods with Opacity and HSV values.
                    if (bm.events[3][0].eventValues.Length > 2)
                    {
                        collection.ColorSequence = converter.GetColorSequence(bm.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                        collection.OpacitySequence = converter.GetOpacitySequence(bm.events[3], 1, new FloatKeyframe(0.0f, 0, Ease.Linear));
                        collection.HueSequence = converter.GetOpacitySequence(bm.events[3], 2, new FloatKeyframe(0.0f, 0, Ease.Linear));
                        collection.SaturationSequence = converter.GetOpacitySequence(bm.events[3], 3, new FloatKeyframe(0.0f, 0, Ease.Linear));
                        collection.ValueSequence = converter.GetOpacitySequence(bm.events[3], 4, new FloatKeyframe(0.0f, 0, Ease.Linear));
                    }
                    // For mods with Opacity.
                    else if (bm.events[3][0].eventValues.Length > 1)
                    {
                        collection.ColorSequence = converter.GetColorSequence(bm.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                        collection.OpacitySequence = converter.GetOpacitySequence(bm.events[3], 1, new FloatKeyframe(0.0f, 0, Ease.Linear));
                    }
                    // If array is regular length.
                    else
                    {
                        collection.ColorSequence = converter.GetColorSequence(bm.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                    }
                }

                converter.cachedSequences.Add(bm.id, collection);
            }

            yield break;
        }

        /// <summary>
        /// Removes and recreates the object if it still exists.
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="level"></param>
        /// <param name="objects"></param>
        /// <param name="converter"></param>
        /// <param name="objectSpawner"></param>
        /// <param name="reinsert"></param>
        /// <returns></returns>
        public static IEnumerator updateObjects(BeatmapObject bm, LevelStorage level, List<ILevelObject> objects, ObjectConverter converter, ObjectSpawner objectSpawner, bool reinsert = true)
        {
            string id = bm.id;

            foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
            {
                if (beatmapObject.parent == id)
                {
                    // Recursing updating.
                    FunctionsPlugin.inst.StartCoroutine(updateObjects(beatmapObject, level, objects, converter, objectSpawner));
                }
            }

            // Get ILevelObject related to BeatmapObject.
            var iLevelObject = GetILevelObject(id, objects);

            // If ILevelObject is not null, then start destroying.
            if (iLevelObject != null)
            {
                var visualObject = ((LevelObject)iLevelObject).visualObject;

                var gameObject = visualObject.GameObject;

                if (gameObject != null)
                {
                    // Get the top-most parent that isn't the "GameObjects" object.
                    while (gameObject.transform.parent.name != "GameObjects")
                        gameObject = gameObject.transform.parent.gameObject;

                    // Remove GameObject.
                    UnityEngine.Object.Destroy(gameObject);
                    objects.Remove(iLevelObject);
                }

                // Remove BeatmapObject from converter.
                //Managers.Objects.beatmapObjects.Remove(id);

                ((LevelObject)iLevelObject).parentObjects.Clear();

                iLevelObject = null;
            }

            // If the object should be reinserted and it is not null then we reinsert the object.
            if (reinsert && bm != null)
            {
                // It's important that the beatmapObjects Dictionary has a reference to the object.
                if (!converter.beatmapObjects.ContainsKey(bm.id))
                    converter.beatmapObjects.Add(bm.id, bm);

                // Convert object to ILevelObject.
                var ilevelObj = converter.ToILevelObject(bm);
                if (ilevelObj != null)
                    level.InsertObject(ilevelObj);
            }

            yield break;
        }

        /// <summary>
        /// Updates everything and reinitializes the engine. There's probably a better way of doing this but I'm not sure of how to do that.
        /// </summary>
        /// <param name="restart"></param>
        public static void updateObjects(bool restart = true)
        {
            // We check if LevelProcessor has been invoked and if the level should restart.
            if (levelProcessor == null && restart)
                GameManagerPatch.StartInvoke();

            // If it is not null then we continue.
            if (levelProcessor != null)
            {
                var level = levelProcessor.level;
                var objects = level.objects;

                // Here we get all the GameObjects and destroy them.
                foreach (var obj in objects)
                {
                    var levelObject = (LevelObject)obj;

                    var visualObject = levelObject.visualObject;
                    var gameObject = visualObject.GameObject;
                    if (gameObject != null && gameObject.transform.parent.name != "GameObjects")
                        UnityEngine.Object.Destroy(gameObject.transform.parent.gameObject);
                }

                level.objects.Clear();

                // Just in case there's anything left behind, we delete all the "GameObjects" children.
                LSFunctions.LSHelpers.DeleteChildren(UnityEngine.GameObject.Find("GameObjects").transform);

                // Removing and reinserting prefabs.
                DataManager.inst.gameData.beatmapObjects.RemoveAll(x => x.fromPrefab);
                for (int i = 0; i < DataManager.inst.gameData.prefabObjects.Count; i++)
                    ObjectManager.inst.AddPrefabToLevel(DataManager.inst.gameData.prefabObjects[i]);

                // End and restart.
                GameManagerPatch.EndInvoke();
                if (restart)
                    GameManagerPatch.StartInvoke();
            }
        }

    }
}
