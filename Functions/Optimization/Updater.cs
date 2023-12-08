using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using RTFunctions.Patchers;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization.Level;
using RTFunctions.Functions.Optimization.Objects;
using RTFunctions.Functions.Animation;
using RTFunctions.Functions.Animation.Keyframe;

using BeatmapObject = DataManager.GameData.BeatmapObject;
using PrefabObject = DataManager.GameData.PrefabObject;
using Prefab = DataManager.GameData.Prefab;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;

namespace RTFunctions.Functions.Optimization
{
    public class Updater
    {
        public static string className = "[<color=#0E36FD>RT<color=#4FBDD1>Functions</color> Updater] \n";

        public static LevelProcessor levelProcessor;

        public static bool Active => levelProcessor && levelProcessor.level;

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
            Debug.Log($"{className}Loading level");

            levelProcessor = new LevelProcessor(DataManager.inst.gameData);
        }

        public static void OnLevelEnd()
        {
            Debug.Log($"{className}Cleaning up level");

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
                return null;

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
                return null;

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
                return null;

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
                    FunctionsPlugin.inst.StartCoroutine(UpdateObjects(_objectSelection.GetObjectData(), level, objects, converter, objectSpawner, reinsert));
                }
            }
        }

        public static void UpdateProcessor(BeatmapObject beatmapObject, bool recache = true, bool update = true, bool reinsert = true)
        {
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

                    if (!reinsert)
                    {
                        recache = true;
                        update = true;
                    }

                    if (recache)
                        FunctionsPlugin.inst.StartCoroutine(RecacheSequences(beatmapObject, converter, reinsert));
                    if (update)
                        FunctionsPlugin.inst.StartCoroutine(UpdateObjects(beatmapObject, level, objects, converter, objectSpawner, reinsert));
                }
            }
        }

        /// <summary>
        /// Updates a specific value.
        /// </summary>
        /// <param name="beatmapObject"></param>
        /// <param name="context"></param>
        /// <param name="value">The specific context to update under.</param>
        public static void UpdateProcessor(BeatmapObject beatmapObject, string context)
        {
            if (TryGetObject(beatmapObject, out LevelObject levelObject))
            {
                switch (context.ToLower().Replace(" ", "").Replace("_", ""))
                {
                    case "objecttype":
                        {
                            
                            break;
                        } // ObjectType
                    case "starttime":
                        {
                            levelObject.StartTime = beatmapObject.StartTime;
                            levelObject.KillTime = beatmapObject.StartTime + beatmapObject.GetObjectLifeLength(0.0f, true);
                            break;
                        } // StartTime
                    case "autokill":
                        {
                            levelObject.KillTime = beatmapObject.StartTime + beatmapObject.GetObjectLifeLength(0.0f, true);
                            break;
                        } // Autokill
                    case "parent":
                        {
                            UpdateProcessor(beatmapObject);
                            break;
                        } // Parent
                    case "parentoffset":
                        {
                            if (levelProcessor && levelProcessor.converter != null)
                            {
                                var converter = levelProcessor.converter;

                                foreach (var levelParent in levelObject.parentObjects)
                                {
                                    if (DataManager.inst.gameData.beatmapObjects.TryFind(x => x.id == levelParent.ID, out BeatmapObject parent))
                                    {
                                        levelParent.ParentAnimatePosition = parent.GetParentType(0);
                                        levelParent.ParentAnimateScale = parent.GetParentType(1);
                                        levelParent.ParentAnimateRotation = parent.GetParentType(2);

                                        levelParent.ParentOffsetPosition = parent.getParentOffset(0);
                                        levelParent.ParentOffsetScale = parent.getParentOffset(1);
                                        levelParent.ParentOffsetRotation = parent.getParentOffset(2);
                                    }
                                }
                            }
                            break;
                        }
                    case "origin":
                        {
                            if (levelObject.visualObject != null)
                                levelObject.visualObject.GameObject.transform.localPosition = new Vector3(beatmapObject.origin.x, beatmapObject.origin.y, 0f);
                            break;
                        } // Origin
                    case "shape":
                        {
                            //if (beatmapObject.shape == 4 || beatmapObject.shape == 6)
                                UpdateProcessor(beatmapObject);
                            //else if (ShapeManager.GetShape(beatmapObject.shape, beatmapObject.shapeOption).mesh != null)
                            //    levelObject.visualObject.GameObject.GetComponent<MeshFilter>().mesh = ShapeManager.GetShape(beatmapObject.shape, beatmapObject.shapeOption).mesh;
                            break;
                        } // Shape
                    case "depth":
                        {
                            levelObject.depth = beatmapObject.Depth;
                            break;
                        } // Depth
                    case "keyframe":
                    case "keyframes":
                        {
                            levelObject.KillTime = beatmapObject.StartTime + beatmapObject.GetObjectLifeLength(0.0f, true);
                            FunctionsPlugin.inst.StartCoroutine(RecacheSequences(beatmapObject, levelProcessor.converter, true, true));

                            break;
                        }
                }
            }
            else if (context.ToLower() == "keyframe" || context.ToLower() == "keyframes")
            {
                FunctionsPlugin.inst.StartCoroutine(RecacheSequences(beatmapObject, levelProcessor.converter, true, true));
            }
        }

        public static void UpdatePrefab(PrefabObject prefabObject, bool reinsert = true)
        {
            if (DataManager.inst.gameData.beatmapObjects.FindAll(x => x.prefabInstanceID == prefabObject.ID).Count < 0 && reinsert)
                ObjectManager.inst.AddPrefabToLevel(prefabObject);

            foreach (var bm in DataManager.inst.gameData.beatmapObjects.FindAll(x => x.prefabInstanceID == prefabObject.ID))
            {
                UpdateProcessor(bm, reinsert: reinsert);
            }
        }

        /// <summary>
        /// Recaches all the keyframe sequences related to the BeatmapObject.
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="converter"></param>
        /// <param name="reinsert"></param>
        /// <returns></returns>
        public static IEnumerator RecacheSequences(BeatmapObject bm, ObjectConverter converter, bool reinsert = true, bool updateParents = false)
        {
            if (converter.cachedSequences.ContainsKey(bm.id))
            {
                converter.cachedSequences[bm.id] = null;
                converter.cachedSequences.Remove(bm.id);
            }

            // Recursive recaching.
            foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
            {
                if (beatmapObject.parent == bm.id)
                    FunctionsPlugin.inst.StartCoroutine(RecacheSequences(beatmapObject, converter, reinsert, updateParents));
            }

            if (reinsert)
            {
                yield return FunctionsPlugin.inst.StartCoroutine(converter.CacheSequence(bm));

                if (updateParents && TryGetObject(bm, out LevelObject levelObject))
                {
                    foreach (var levelParent in levelObject.parentObjects)
                    {
                        if (converter.cachedSequences.ContainsKey(levelParent.ID))
                        {
                            var cachedSequences = converter.cachedSequences[levelParent.ID];
                            levelParent.Position3DSequence = cachedSequences.Position3DSequence;
                            levelParent.ScaleSequence = cachedSequences.ScaleSequence;
                            levelParent.RotationSequence = cachedSequences.RotationSequence;
                        }
                    }
                }
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
        public static IEnumerator UpdateObjects(BeatmapObject bm, LevelStorage level, List<ILevelObject> objects, ObjectConverter converter, ObjectSpawner objectSpawner, bool reinsert = true)
        {
            string id = bm.id;

            // Recursing updating.
            foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
            {
                if (beatmapObject.parent == id)
                    FunctionsPlugin.inst.StartCoroutine(UpdateObjects(beatmapObject, level, objects, converter, objectSpawner));
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
                    while (gameObject.transform.parent.name != "GameObjects" && !gameObject.transform.parent.name.Contains("CAMERA_PARENT ["))
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
        public static void UpdateObjects(bool restart = true)
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
                LSFunctions.LSHelpers.DeleteChildren(GameObject.Find("GameObjects").transform);

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
