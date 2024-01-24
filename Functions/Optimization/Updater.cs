using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using RTFunctions.Patchers;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.Optimization.Level;
using RTFunctions.Functions.Optimization.Objects;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BasePrefabObject = DataManager.GameData.PrefabObject;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using ObjectAutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;

namespace RTFunctions.Functions.Optimization
{
    public class Updater
    {
        public static string className = "[<color=#0E36FD>RT<color=#4FBDD1>Functions</color> Updater] \n";

        public static LevelProcessor levelProcessor;

        public static bool Active => levelProcessor && levelProcessor.level;

        public static bool HasObject(BaseBeatmapObject beatmapObject) => Active && (LevelObject)levelProcessor.level.objects.Find(x => x.ID == beatmapObject.id);

        public static bool TryGetObject(BaseBeatmapObject beatmapObject, out LevelObject levelObject)
        {
            if (beatmapObject is BeatmapObject && (beatmapObject as BeatmapObject).levelObject)
            {
                levelObject = (beatmapObject as BeatmapObject).levelObject;
                return true;
            }

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
        public static ILevelObject GetLevelObject(BaseBeatmapObject _beatmapObject)
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
        public static GameObject GetGameObject(BaseBeatmapObject beatmapObject)
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
        public static ILevelObject GetILevelObject(BaseBeatmapObject _beatmapObject, List<ILevelObject> objects)
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

        public static void UpdateProcessor(BaseBeatmapObject beatmapObject, bool recache = true, bool update = true, bool reinsert = true)
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

        public static void UpdateProcessor(string id, bool recache = true, bool update = true, bool reinsert = true)
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
                        FunctionsPlugin.inst.StartCoroutine(RecacheSequences(id, converter, reinsert));
                    if (update)
                        FunctionsPlugin.inst.StartCoroutine(UpdateObjects(id, level, objects, converter, objectSpawner, reinsert));
                }
            }
        }

        /// <summary>
        /// Updates a specific value.
        /// </summary>
        /// <param name="beatmapObject"></param>
        /// <param name="context"></param>
        /// <param name="value">The specific context to update under.</param>
        public static void UpdateProcessor(BaseBeatmapObject beatmapObject, string context)
        {
            if (TryGetObject(beatmapObject, out LevelObject levelObject))
            {
                switch (context.ToLower().Replace(" ", "").Replace("_", ""))
                {
                    case "objecttype":
                        {
                            UpdateProcessor(beatmapObject);
                            break;
                        } // ObjectType
                    case "time":
                    case "starttime":
                        {
                            if (levelProcessor && levelProcessor.engine && levelProcessor.engine.objectSpawner != null)
                            {
                                var spawner = levelProcessor.engine.objectSpawner;

                                levelObject.StartTime = beatmapObject.StartTime;
                                levelObject.KillTime = beatmapObject.StartTime + beatmapObject.GetObjectLifeLength(0.0f, true);

                                spawner.activateList.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
                                spawner.deactivateList.Sort((a, b) => a.KillTime.CompareTo(b.KillTime));
                                spawner.RecalculateObjectStates();

                                if (!beatmapObject.TimeWithinLifespan())
                                    levelObject.SetActive(false);

                                foreach (var levelParent in levelObject.parentObjects)
                                {
                                    if (DataManager.inst.gameData.beatmapObjects.TryFind(x => x.id == levelParent.ID, out BaseBeatmapObject parent))
                                    {
                                        levelParent.TimeOffset = parent.StartTime;

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
                        } // StartTime
                    case "autokilltype":
                    case "autokilloffset":
                    case "autokill":
                        {
                            if (levelProcessor && levelProcessor.engine && levelProcessor.engine.objectSpawner != null)
                            {
                                var spawner = levelProcessor.engine.objectSpawner;

                                spawner.deactivateList.Sort((a, b) => a.KillTime.CompareTo(b.KillTime));
                                spawner.RecalculateObjectStates();

                                levelObject.KillTime = beatmapObject.StartTime + beatmapObject.GetObjectLifeLength(0.0f, true);
                            }

                            break;
                        } // Autokill
                    case "parent":
                        {
                            var parentChain = beatmapObject.GetParentChain();
                            if (beatmapObject.parent == "CAMERA_PARENT" || parentChain.Count > 1 && parentChain[parentChain.Count - 1].parent == "CAMERA_PARENT")
                            {
                                var beatmapParent = parentChain.Count > 1 && parentChain[parentChain.Count - 1].parent == "CAMERA_PARENT" ? parentChain[parentChain.Count - 1] : beatmapObject;

                                var ids = new List<string>();
                                foreach (var child in beatmapParent.GetChildChain())
                                {
                                    ids.AddRange(child.Where(x => !ids.Contains(x.id)).Select(x => x.id));
                                }

                                foreach (var id in ids)
                                {
                                    var child = DataManager.inst.gameData.beatmapObjects.Find(x => x.id == id);
                                    if (TryGetObject(child, out LevelObject childLevelObject))
                                    {
                                        childLevelObject.cameraParent = beatmapParent.parent == "CAMERA_PARENT";

                                        childLevelObject.positionParent = beatmapParent.GetParentType(0);
                                        childLevelObject.scaleParent = beatmapParent.GetParentType(1);
                                        childLevelObject.rotationParent = beatmapParent.GetParentType(2);

                                        childLevelObject.positionParentOffset = ((Data.BeatmapObject)beatmapParent).parallaxSettings[0];
                                        childLevelObject.scaleParentOffset = ((Data.BeatmapObject)beatmapParent).parallaxSettings[1];
                                        childLevelObject.rotationParentOffset = ((Data.BeatmapObject)beatmapParent).parallaxSettings[2];
                                    }
                                }
                            }
                            else
                            {
                                UpdateProcessor(beatmapObject);
                            }

                            break;
                        } // Parent
                    case "parenttype":
                    case "parentoffset":
                        {
                            var parentChain = beatmapObject.GetParentChain();
                            if (beatmapObject.parent == "CAMERA_PARENT" || parentChain.Count > 1 && parentChain[parentChain.Count - 1].parent == "CAMERA_PARENT")
                            {
                                var beatmapParent = parentChain.Count > 1 && parentChain[parentChain.Count - 1].parent == "CAMERA_PARENT" ? parentChain[parentChain.Count - 1] : beatmapObject;

                                var ids = new List<string>();
                                foreach (var child in beatmapParent.GetChildChain())
                                {
                                    ids.AddRange(child.Where(x => !ids.Contains(x.id)).Select(x => x.id));
                                }

                                foreach (var id in ids)
                                {
                                    var child = DataManager.inst.gameData.beatmapObjects.Find(x => x.id == id);
                                    if (TryGetObject(child, out LevelObject childLevelObject))
                                    {
                                        childLevelObject.cameraParent = beatmapParent.parent == "CAMERA_PARENT";

                                        childLevelObject.positionParent = beatmapParent.GetParentType(0);
                                        childLevelObject.scaleParent = beatmapParent.GetParentType(1);
                                        childLevelObject.rotationParent = beatmapParent.GetParentType(2);

                                        childLevelObject.positionParentOffset = ((Data.BeatmapObject)beatmapParent).parallaxSettings[0];
                                        childLevelObject.scaleParentOffset = ((Data.BeatmapObject)beatmapParent).parallaxSettings[1];
                                        childLevelObject.rotationParentOffset = ((Data.BeatmapObject)beatmapParent).parallaxSettings[2];
                                    }
                                }
                            }

                            foreach (var levelParent in levelObject.parentObjects)
                            {
                                if (DataManager.inst.gameData.beatmapObjects.TryFind(x => x.id == levelParent.ID, out BaseBeatmapObject parent))
                                {
                                    levelParent.ParentAnimatePosition = parent.GetParentType(0);
                                    levelParent.ParentAnimateScale = parent.GetParentType(1);
                                    levelParent.ParentAnimateRotation = parent.GetParentType(2);

                                    levelParent.ParentOffsetPosition = parent.getParentOffset(0);
                                    levelParent.ParentOffsetScale = parent.getParentOffset(1);
                                    levelParent.ParentOffsetRotation = parent.getParentOffset(2);
                                }
                            }

                            break;
                        }
                    case "origin":
                    case "depth":
                    case "originoffset":
                        {
                            levelObject.depth = beatmapObject.depth;
                            if (levelObject.visualObject != null && levelObject.visualObject.GameObject)
                                levelObject.visualObject.GameObject.transform.localPosition = new Vector3(beatmapObject.origin.x, beatmapObject.origin.y, beatmapObject.depth * 0.1f);
                            break;
                        } // Origin & Depth
                    case "shape":
                        {
                            //if (beatmapObject.shape == 4 || beatmapObject.shape == 6)
                                UpdateProcessor(beatmapObject);
                            //else if (ShapeManager.GetShape(beatmapObject.shape, beatmapObject.shapeOption).mesh != null)
                            //    levelObject.visualObject.GameObject.GetComponent<MeshFilter>().mesh = ShapeManager.GetShape(beatmapObject.shape, beatmapObject.shapeOption).mesh;
                            break;
                        } // Shape
                    case "text":
                        {
                            if (levelObject.visualObject != null && levelObject.visualObject is Objects.Visual.TextObject)
                                (levelObject.visualObject as Objects.Visual.TextObject).TextMeshPro.text = beatmapObject.text;
                            break;
                        }
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
            else if (context.ToLower() == "starttime" || context.ToLower() == "time")
            {
                if (levelProcessor && levelProcessor.engine && levelProcessor.engine.objectSpawner != null)
                {
                    var spawner = levelProcessor.engine.objectSpawner;

                    spawner.activateList.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
                    spawner.deactivateList.Sort((a, b) => a.KillTime.CompareTo(b.KillTime));
                    spawner.RecalculateObjectStates();
                }
            }
        }

        public static void UpdatePrefab(BasePrefabObject prefabObject, bool reinsert = true)
        {
            DataManager.inst.gameData.beatmapObjects.Where(x => x.fromPrefab && x.prefabInstanceID == prefabObject.ID).ToList().ForEach(x => UpdateProcessor(x, reinsert: false));

            DataManager.inst.gameData.beatmapObjects.RemoveAll(x => x.prefabInstanceID == prefabObject.ID);

            if (reinsert)
                AddPrefabToLevel(prefabObject);

            //if ((DataManager.inst.gameData.beatmapObjects.FindAll(x => x.fromPrefab && x.prefabInstanceID == prefabObject.ID).Count < 0) && reinsert)
            //    AddPrefabToLevel(prefabObject);

            //foreach (var bm in DataManager.inst.gameData.beatmapObjects.FindAll(x => x.fromPrefab && x.prefabInstanceID == prefabObject.ID))
            //{
            //    UpdateProcessor(bm, reinsert: reinsert);
            //}
        }

        public static void RemovePrefab(BasePrefabObject prefabObject)
        {
            foreach (var bm in DataManager.inst.gameData.beatmapObjects.Where(x => x.fromPrefab && x.prefabInstanceID == prefabObject.ID))
            {
                UpdateProcessor(bm, reinsert: false);
            }

            DataManager.inst.gameData.beatmapObjects.RemoveAll(x => x.prefabInstanceID == prefabObject.ID);
        }

        public static void UpdatePrefab(BasePrefabObject prefabObject, string context)
        {
            string lower = context.ToLower().Replace(" ", "").Replace("_", "");
            switch (lower)
            {
                case "offset":
                case "transformoffset":
                    {
                        foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects.Where(x => x.fromPrefab && x.prefabInstanceID == prefabObject.ID && x is Data.BeatmapObject).Select(x => x as Data.BeatmapObject))
                        {
                            if (beatmapObject.levelObject && beatmapObject.levelObject.visualObject != null && beatmapObject.levelObject.visualObject.Top)
                            {
                                var top = beatmapObject.levelObject.visualObject.Top;

                                bool hasPosX = prefabObject.events.Count > 0 && prefabObject.events[0] != null && prefabObject.events[0].eventValues.Length > 0;
                                bool hasPosY = prefabObject.events.Count > 0 && prefabObject.events[0] != null && prefabObject.events[0].eventValues.Length > 1;

                                bool hasScaX = prefabObject.events.Count > 1 && prefabObject.events[1] != null && prefabObject.events[1].eventValues.Length > 0;
                                bool hasScaY = prefabObject.events.Count > 1 && prefabObject.events[1] != null && prefabObject.events[1].eventValues.Length > 1;

                                bool hasRot = prefabObject.events.Count > 2 && prefabObject.events[2] != null && prefabObject.events[2].eventValues.Length > 0;

                                var pos = new Vector3(hasPosX ? prefabObject.events[0].eventValues[0] : 0f, hasPosY ? prefabObject.events[0].eventValues[1] : 0f, 0f);
                                var sca = new Vector3(hasScaX ? prefabObject.events[1].eventValues[0] : 1f, hasScaY ? prefabObject.events[1].eventValues[1] : 1f, 1f);
                                var rot = Quaternion.Euler(0f, 0f, hasRot ? prefabObject.events[2].eventValues[0] : 0f);

                                try
                                {
                                    if (prefabObject.events[0].random != 0)
                                        pos = ObjectManager.inst.RandomVector2Parser(prefabObject.events[0]);
                                    if (prefabObject.events[1].random != 0)
                                        sca = ObjectManager.inst.RandomVector2Parser(prefabObject.events[1]);
                                    if (prefabObject.events[2].random != 0)
                                        rot = Quaternion.Euler(0f, 0f, ObjectManager.inst.RandomFloatParser(prefabObject.events[2]));
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogError($"{className}Prefab Randomization error.\n{ex}");
                                }

                                beatmapObject.levelObject.prefabOffsetPosition = pos;
                                beatmapObject.levelObject.prefabOffsetScale = sca.x != 0f && sca.y != 0f ? sca : Vector3.one;
                                beatmapObject.levelObject.prefabOffsetRotation = rot.eulerAngles;

                                if (!hasPosX)
                                    Debug.LogError($"{className}PrefabObject does not have Postion X in its' eventValues.\nPossible causes:");
                                if (!hasPosY)
                                    Debug.LogError($"{className}PrefabObject does not have Postion Y in its' eventValues.");
                                if (!hasScaX)
                                    Debug.LogError($"{className}PrefabObject does not have Scale X in its' eventValues.");
                                if (!hasScaY)
                                    Debug.LogError($"{className}PrefabObject does not have Scale Y in its' eventValues.");
                                if (!hasRot)
                                    Debug.LogError($"{className}PrefabObject does not have Rotation in its' eventValues.");
                            }
                        }
                        break;
                    }
                case "time":
                case "starttime":
                case "speed":
                    {
                        float t = 1f;

                        var moddedPrefab = (PrefabObject)prefabObject;

                        if (prefabObject.RepeatOffsetTime != 0f)
                            t = prefabObject.RepeatOffsetTime;

                        float timeToAdd = 0f;

                        var prefab = DataManager.inst.gameData.prefabs.Find(x => x.ID == prefabObject.prefabID);

                        for (int i = 0; i < prefabObject.RepeatCount + 1; i++)
                        {
                            foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects.Where(x => x.fromPrefab && x.prefabInstanceID == prefabObject.ID))
                            {
                                if (prefab.objects.TryFind(x => x.id == ((BeatmapObject)beatmapObject).originalID, out BaseBeatmapObject original))
                                {
                                    beatmapObject.StartTime = prefabObject.StartTime + prefab.Offset + (original.StartTime + timeToAdd) / Mathf.Clamp(moddedPrefab.speed, 0.1f, MaxFastSpeed);

                                    if (lower == "speed")
                                    {
                                        for (int j = 0; j < beatmapObject.events.Count; j++)
                                        {
                                            for (int k = 0; k < beatmapObject.events[j].Count; k++)
                                            {
                                                beatmapObject.events[i][k].eventTime = original.events[i][k].eventTime / Mathf.Clamp(moddedPrefab.speed, 0.1f, 100f);
                                            }
                                        }
                                        UpdateProcessor(beatmapObject, "Keyframes");
                                    }

                                    UpdateProcessor(beatmapObject, "Start Time");
                                }
                            }

                            timeToAdd += t;
                        }

                        break;
                    }
                case "autokill":
                    {
                        var pr = prefabObject as PrefabObject;

                        foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects.Where(x => x.fromPrefab && x.prefabInstanceID == prefabObject.ID))
                        {
                            if (pr.autoKillType != PrefabObject.AutoKillType.Regular && prefabObject.StartTime + pr.Prefab?.Offset + beatmapObject.GetObjectLifeLength(_oldStyle: true) > pr.autoKillOffset)
                            {
                                beatmapObject.autoKillType = ObjectAutoKillType.SongTime;
                                beatmapObject.autoKillOffset = pr.autoKillType == PrefabObject.AutoKillType.StartTimeOffset ? prefabObject.StartTime + pr.Prefab?.Offset ?? 0f + pr.autoKillOffset : pr.autoKillOffset;
                            }

                            if (pr.autoKillType == PrefabObject.AutoKillType.Regular)
                            {
                                UpdatePrefab(prefabObject);
                            }

                            UpdateProcessor(beatmapObject, "Start Time");
                        }

                        break;
                    }
            }
        }
        public static float MaxFastSpeed => 1000f;
        public static void AddPrefabToLevel(BasePrefabObject __0)
        {
            var prefabObject = (PrefabObject)__0;

            // Checks if prefab exists.
            bool prefabExists = DataManager.inst.gameData.prefabs.FindIndex(x => x.ID == __0.prefabID) != -1;
            if (string.IsNullOrEmpty(__0.prefabID) || !prefabExists)
            {
                DataManager.inst.gameData.prefabObjects.RemoveAll(x => x.prefabID == __0.prefabID);
                //Debug.LogError($"{className}Prefab does not exist, so removing prefab objects.");
                return;
            }

            float t = 1f;

            if (__0.RepeatOffsetTime != 0f)
                t = __0.RepeatOffsetTime;

            float timeToAdd = 0f;

            var prefab = DataManager.inst.gameData.prefabs.Find(x => x.ID == __0.prefabID);

            var list = new List<BeatmapObject>();
            if (prefab.objects.Count > 0)
                for (int i = 0; i < __0.RepeatCount + 1; i++)
                {
                    var ids = prefab.objects.ToDictionary(x => x.id, x => LSFunctions.LSText.randomString(16));

                    string iD = __0.ID;
                    foreach (var beatmapObj in prefab.objects)
                    {
                        var beatmapObject = BeatmapObject.DeepCopy((BeatmapObject)beatmapObj, false);
                        try
                        {
                            if (ids.ContainsKey(beatmapObj.id))
                                beatmapObject.id = ids[beatmapObj.id];

                            if (ids.ContainsKey(beatmapObj.parent))
                                beatmapObject.parent = ids[beatmapObj.parent];
                            else if (DataManager.inst.gameData.beatmapObjects.FindIndex(x => x.id == beatmapObj.parent) == -1)
                                beatmapObject.parent = "";
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"{className}Failed to set object ID.\n{ex}");
                        }

                        beatmapObject.active = false;
                        beatmapObject.fromPrefab = true;
                        beatmapObject.prefabInstanceID = iD;

                        beatmapObject.StartTime = __0.StartTime + prefab.Offset + (beatmapObject.StartTime + timeToAdd) / Mathf.Clamp(prefabObject.speed, 0.1f, MaxFastSpeed);

                        try
                        {
                            if (beatmapObject.events.Count > 0)
                                for (int j = 0; j < beatmapObject.events.Count; j++)
                                {
                                    beatmapObject.events[j].ForEach(x => x.eventTime /= Mathf.Clamp(prefabObject.speed, 0.1f, MaxFastSpeed));
                                }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"{className}Failed to set event speed.\n{ex}");
                        }

                        if (prefabObject.autoKillType != PrefabObject.AutoKillType.Regular && __0.StartTime + prefab.Offset + beatmapObject.GetObjectLifeLength(_oldStyle: true) > prefabObject.autoKillOffset)
                        {
                            beatmapObject.autoKillType = ObjectAutoKillType.SongTime;
                            beatmapObject.autoKillOffset = prefabObject.autoKillType == PrefabObject.AutoKillType.StartTimeOffset ? prefabObject.StartTime + prefab.Offset + prefabObject.autoKillOffset : prefabObject.autoKillOffset;
                        }

                        beatmapObject.prefabID = __0.prefabID;

                        beatmapObject.originalID = beatmapObj.id;
                        DataManager.inst.gameData.beatmapObjects.Add(beatmapObject);
                        list.Add(beatmapObject);
                    }

                    timeToAdd += t;
                }

            foreach (var bm in list)
            {
                UpdateProcessor(bm);
            }
            list.Clear();
            list = null;
        }

        /// <summary>
        /// Recaches all the keyframe sequences related to the BeatmapObject.
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="converter"></param>
        /// <param name="reinsert"></param>
        /// <returns></returns>
        public static IEnumerator RecacheSequences(BaseBeatmapObject bm, ObjectConverter converter, bool reinsert = true, bool updateParents = false)
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

                if (TryGetObject(bm, out LevelObject levelObject))
                {
                    if (converter.cachedSequences.ContainsKey(bm.id))
                        levelObject.SetSequences(
                            converter.cachedSequences[bm.id].ColorSequence,
                            converter.cachedSequences[bm.id].OpacitySequence,
                            converter.cachedSequences[bm.id].HueSequence,
                            converter.cachedSequences[bm.id].SaturationSequence,
                            converter.cachedSequences[bm.id].ValueSequence);

                    if (updateParents)
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
        /// Recaches all the keyframe sequences related to the BeatmapObject.
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="converter"></param>
        /// <param name="reinsert"></param>
        /// <returns></returns>
        public static IEnumerator RecacheSequences(string id, ObjectConverter converter, bool reinsert = true, bool updateParents = false)
        {
            if (converter.cachedSequences.ContainsKey(id))
            {
                converter.cachedSequences[id] = null;
                converter.cachedSequences.Remove(id);
            }

            // Recursive recaching.
            foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
            {
                if (beatmapObject.parent == id)
                    FunctionsPlugin.inst.StartCoroutine(RecacheSequences(beatmapObject.id, converter, reinsert, updateParents));
            }

            if (reinsert && DataManager.inst.gameData.beatmapObjects.TryFind(x => x.id == id, out BaseBeatmapObject result))
            {
                yield return FunctionsPlugin.inst.StartCoroutine(converter.CacheSequence(result));

                if (updateParents && TryGetObject(result, out LevelObject levelObject))
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
        public static IEnumerator UpdateObjects(BaseBeatmapObject bm, LevelStorage level, List<ILevelObject> objects, ObjectConverter converter, ObjectSpawner objectSpawner, bool reinsert = true)
        {
            string id = bm.id;

            if (bm is BeatmapObject modObject)
            {
                modObject.reactivePositionOffset = Vector3.zero;
                modObject.reactiveScaleOffset = Vector3.zero;
                modObject.reactiveRotationOffset = 0f;
                modObject.positionOffset = Vector3.zero;
                modObject.scaleOffset = Vector3.zero;
                modObject.rotationOffset = Vector3.zero;
            }

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

                var top = visualObject.Top;

                // Remove GameObject.
                objectSpawner.RemoveObject(iLevelObject);
                objects.Remove(iLevelObject);
                Object.Destroy(top.gameObject);
                ((LevelObject)iLevelObject).parentObjects.Clear();

                // Remove BeatmapObject from converter.
                converter.beatmapObjects.Remove(id);

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
        /// Removes and recreates the object if it still exists.
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="level"></param>
        /// <param name="objects"></param>
        /// <param name="converter"></param>
        /// <param name="objectSpawner"></param>
        /// <param name="reinsert"></param>
        /// <returns></returns>
        public static IEnumerator UpdateObjects(string id, LevelStorage level, List<ILevelObject> objects, ObjectConverter converter, ObjectSpawner objectSpawner, bool reinsert = true)
        {
            // Recursing updating.
            foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
            {
                if (beatmapObject.parent == id)
                    FunctionsPlugin.inst.StartCoroutine(UpdateObjects(beatmapObject.id, level, objects, converter, objectSpawner));
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
            if (reinsert && DataManager.inst.gameData.beatmapObjects.TryFind(x => x.id == id, out BaseBeatmapObject result))
            {
                // It's important that the beatmapObjects Dictionary has a reference to the object.
                if (!converter.beatmapObjects.ContainsKey(id))
                    converter.beatmapObjects.Add(id, result);

                // Convert object to ILevelObject.
                var ilevelObj = converter.ToILevelObject(result);
                if (ilevelObj != null)
                    level.InsertObject(ilevelObj);
            }

            yield break;
        }

        public static void RemoveObjects(List<string> ids)
        {
            levelProcessor.level.objects
                .Where(x => ids.Contains(x.ID))
                .ToList()
                .ForEachReturn(x => Object.DestroyImmediate(((LevelObject)x).visualObject.Top?.gameObject))
                .RemoveAll(x => ids.Contains(x.ID));
        }

        public static void RemoveObject(string id)
        {
            var levelObject = (LevelObject)levelProcessor.level.objects.Find(x => x.ID == id);
            Object.Destroy(levelObject.visualObject.GameObject);
            levelProcessor.level.objects.Remove(levelObject);
            levelProcessor.converter.beatmapObjects.Remove(id);
        }

        /// <summary>
        /// Updates everything and reinitializes the engine.
        /// </summary>
        /// <param name="restart">If the engine should restart or not.</param>
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

                level.objects.Clear();

                // Delete all the "GameObjects" children.
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
