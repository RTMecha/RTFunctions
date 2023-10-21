using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using RTFunctions.Functions.Animation;
using RTFunctions.Functions.Animation.Keyframe;
using RTFunctions.Functions.Components;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization.Objects;
using RTFunctions.Functions.Optimization.Objects.Visual;

using GameData = DataManager.GameData;
using BeatmapObject = DataManager.GameData.BeatmapObject;
using EventKeyframe = DataManager.GameData.EventKeyframe;
using Object = UnityEngine.Object;
using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;

namespace RTFunctions.Functions.Optimization.Level
{
    // WARNING: This class has side effects and will instantiate GameObjects
    // Converts GameData to LevelObjects to be used by the mod
    public class ObjectConverter
    {
        // Add Homing Objects to Random EventKeyframe

        public class CachedSequences
        {
            public Sequence<Vector2> PositionSequence { get; set; }
            public Sequence<Vector3> Position3DSequence { get; set; }
            public Sequence<Vector2> ScaleSequence { get; set; }
            public Sequence<float> RotationSequence { get; set; }
            public Sequence<Color> ColorSequence { get; set; }

            public Sequence<float> OpacitySequence { get; set; }
            public Sequence<float> HueSequence { get; set; }
            public Sequence<float> SaturationSequence { get; set; }
            public Sequence<float> ValueSequence { get; set; }
        }

        public Dictionary<string, CachedSequences> cachedSequences = new Dictionary<string, CachedSequences>();
        public Dictionary<string, BeatmapObject> beatmapObjects = new Dictionary<string, BeatmapObject>();

        public bool ShowEmpties => ModCompatibility.sharedFunctions.ContainsKey("ShowEmpties") && (bool)ModCompatibility.sharedFunctions["ShowEmpties"];
        
        public bool ShowDamagable => ModCompatibility.sharedFunctions.ContainsKey("ShowDamagable") && (bool)ModCompatibility.sharedFunctions["ShowDamagable"];

        readonly GameData gameData;

        public ObjectConverter(GameData gameData)
        {
            this.gameData = gameData;

            foreach (BeatmapObject beatmapObject in gameData.beatmapObjects)
            {
                if (!beatmapObjects.ContainsKey(beatmapObject.id))
                    beatmapObjects.Add(beatmapObject.id, beatmapObject);
            }

            foreach (BeatmapObject beatmapObject in beatmapObjects.Values)
            {
                CachedSequences collection = new CachedSequences();
                if (beatmapObject.events[0][0].eventValues.Length > 2)
                {
                    collection = new CachedSequences()
                    {
                        Position3DSequence = GetVector3Sequence(beatmapObject.events[0], new Vector3Keyframe(0.0f, Vector3.zero, Ease.Linear)),
                        ScaleSequence = GetVector2Sequence(beatmapObject.events[1], new Vector2Keyframe(0.0f, Vector2.one, Ease.Linear)),
                        RotationSequence = GetFloatSequence(beatmapObject.events[2], new FloatKeyframe(0.0f, 0.0f, Ease.Linear), true)
                    };
                }
                else
                {
                    Debug.Log($"{Updater.className}Position does not include Z axis so I know not to remove this.");
                    collection = new CachedSequences()
                    {
                        PositionSequence = GetVector2Sequence(beatmapObject.events[0], new Vector2Keyframe(0.0f, Vector2.zero, Ease.Linear)),
                        ScaleSequence = GetVector2Sequence(beatmapObject.events[1], new Vector2Keyframe(0.0f, Vector2.one, Ease.Linear)),
                        RotationSequence = GetFloatSequence(beatmapObject.events[2], new FloatKeyframe(0.0f, 0.0f, Ease.Linear), true)
                    };
                }

                // Empty objects don't need a color sequence, so it is not cached
                if (beatmapObject.objectType != ObjectType.Empty)
                {
                    if (beatmapObject.events[3][0].eventValues.Length > 2)
                    {
                        collection.ColorSequence = GetColorSequence(beatmapObject.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                        collection.OpacitySequence = GetOpacitySequence(beatmapObject.events[3], 1, new FloatKeyframe(0.0f, 0, Ease.Linear));
                        collection.HueSequence = GetOpacitySequence(beatmapObject.events[3], 2, new FloatKeyframe(0.0f, 0, Ease.Linear));
                        collection.SaturationSequence = GetOpacitySequence(beatmapObject.events[3], 3, new FloatKeyframe(0.0f, 0, Ease.Linear));
                        collection.ValueSequence = GetOpacitySequence(beatmapObject.events[3], 4, new FloatKeyframe(0.0f, 0, Ease.Linear));
                    }
                    else if (beatmapObject.events[3][0].eventValues.Length > 1)
                    {
                        collection.ColorSequence = GetColorSequence(beatmapObject.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                        collection.OpacitySequence = GetOpacitySequence(beatmapObject.events[3], 1, new FloatKeyframe(0.0f, 0, Ease.Linear));
                    }
                    else
                    {
                        collection.ColorSequence = GetColorSequence(beatmapObject.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                    }
                }

                cachedSequences.Add(beatmapObject.id, collection);
            }

            if (!cachedSequences.ContainsKey("CAMERA_PARENT"))
            {
                CachedSequences collection = new CachedSequences();
                collection = new CachedSequences
                {
                    PositionSequence = GetVector2Sequence(gameData.eventObjects.allEvents[0], new Vector2Keyframe(0.0f, Vector2.zero, Ease.Linear)),
                    OpacitySequence = GetFloatSequence(gameData.eventObjects.allEvents[1], new FloatKeyframe(0.0f, 0.0f, Ease.Linear)),
                    RotationSequence = GetFloatSequence(gameData.eventObjects.allEvents[2], new FloatKeyframe(0.0f, 0.0f, Ease.Linear)),
                };
            }
        }

        public IEnumerable<ILevelObject> ToLevelObjects()
        {
            foreach (BeatmapObject beatmapObject in gameData.beatmapObjects)
            {
                if (beatmapObject.objectType == ObjectType.Empty)
                {
                    continue;
                }

                LevelObject levelObject = null;

                try
                {
                    levelObject = ToLevelObject(beatmapObject);
                }
                catch (Exception e)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"{Updater.className}Failed to convert object '{beatmapObject.id}' to {nameof(LevelObject)}.");
                    stringBuilder.AppendLine($"Exception: {e.Message}");
                    stringBuilder.AppendLine(e.StackTrace);

                    Debug.LogError(stringBuilder.ToString());
                }

                if (levelObject != null)
                {
                    yield return levelObject;
                }

                //yield return ToLevelObject(beatmapObject);
            }
        }

        public ILevelObject ToILevelObject(BeatmapObject beatmapObject)
        {
            if (beatmapObject.objectType != ObjectType.Empty)
            {
                LevelObject levelObject = null;

                try
                {
                    levelObject = ToLevelObject(beatmapObject);
                }
                catch (Exception e)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"{Updater.className}Failed to convert object '{beatmapObject.id}' to {nameof(LevelObject)}.");
                    stringBuilder.AppendLine($"Exception: {e.Message}");
                    stringBuilder.AppendLine(e.StackTrace);

                    Debug.LogError(stringBuilder.ToString());
                }

                if (levelObject != null)
                    return levelObject;

                //return ToLevelObject(beatmapObject);
            }
            return null;
        }

        LevelObject ToLevelObject(BeatmapObject beatmapObject)
        {
            List<LevelParentObject> parentObjects = new List<LevelParentObject>();

            GameObject parent = null;

            if (!string.IsNullOrEmpty(beatmapObject.parent) && beatmapObjects.ContainsKey(beatmapObject.parent))
                parent = InitParentChain(beatmapObjects[beatmapObject.parent], parentObjects);

            var shape = Mathf.Clamp(beatmapObject.shape, 0, ObjectManager.inst.objectPrefabs.Count - 1);
            var shapeOption = Mathf.Clamp(beatmapObject.shapeOption, 0, ObjectManager.inst.objectPrefabs[shape].options.Count - 1);

            GameObject baseObject = Object.Instantiate(ObjectManager.inst.objectPrefabs[shape].options[shapeOption], parent == null ? null : parent.transform);
            baseObject.transform.localScale = Vector3.one;

            GameObject visualObject = baseObject.transform.GetChild(0).gameObject;
            visualObject.transform.localPosition = new Vector3(beatmapObject.origin.x, beatmapObject.origin.y, beatmapObject.Depth * 0.1f);
            //visualObject.name = "Visual [ " + beatmapObject.id + " ]";
            visualObject.name = "Visual [ " + beatmapObject.name + " ]";

            int num = 0;
            if (parentObjects != null)
                num = parentObjects.Count;

            try
            {
                var p = InitLevelParentObject(beatmapObject, baseObject);
                if (parentObjects.Count > 0)
                    parentObjects.Insert(0, p);
                else
                    parentObjects.Add(p);
            }
            catch (Exception e)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{Updater.className}Failed to init parent chain for '{beatmapObject.id}'. ParentObjects Null: {parentObjects == null} Count: {num}");
                stringBuilder.AppendLine($"Exception: {e.Message}");
                stringBuilder.AppendLine(e.StackTrace);

                Debug.LogError(stringBuilder.ToString());
            }

            //baseObject.name = beatmapObject.id + " [ " + beatmapObject.name + " ]";
            baseObject.name = beatmapObject.name;

            var top = new GameObject("top");
            top.transform.SetParent(ObjectManager.inst.objectParent.transform);
            top.transform.localScale = Vector3.one;
            
            try
            {
                if (beatmapObject.fromPrefab && !string.IsNullOrEmpty(beatmapObject.prefabInstanceID))
                {
                    var prefab = gameData.prefabObjects.Find(x => x.ID == beatmapObject.prefabInstanceID);

                    var pos = new Vector3(prefab.events[0].eventValues[0], prefab.events[0].eventValues[1], 0f);
                    var sca = new Vector3(prefab.events[1].eventValues[0], prefab.events[1].eventValues[1], 1f);
                    var rot = Quaternion.Euler(0f, 0f, prefab.events[2].eventValues[0]);

                    if (prefab.events[0].random != 0)
                    {
                        pos = ObjectManager.inst.RandomVector2Parser(prefab.events[0]);
                    }
                    if (prefab.events[1].random != 0)
                    {
                        sca = ObjectManager.inst.RandomVector2Parser(prefab.events[1]);
                    }
                    if (prefab.events[2].random != 0)
                    {
                        rot = Quaternion.Euler(0f, 0f, ObjectManager.inst.RandomFloatParser(prefab.events[2]));
                    }

                    top.transform.localPosition = pos;
                    if ((sca.x > 0f || sca.x < 0f) && (sca.y > 0f || sca.y < 0f))
                        top.transform.localScale = sca;
                    top.transform.localRotation = rot;
                }
            }
            catch (Exception e)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{Updater.className}Failed to set prefab values for '{beatmapObject.id}'.");
                stringBuilder.AppendLine($"Exception: {e.Message}");
                stringBuilder.AppendLine(e.StackTrace);

                Debug.LogError(stringBuilder.ToString());
            } // Prefab

            try
            {
                var pc = beatmapObject.GetParentChain();
                if (pc != null && pc.Count > 0)
                {
                    var beatmapParent = pc[pc.Count - 1];

                    if (beatmapParent.parent == "CAMERA_PARENT")
                    {
                        GameObject camParent;
                        if (!ObjectManager.inst.objectParent.transform.Find("CAMERA_PARENT [" + beatmapParent.id + "]"))
                        {
                            camParent = new GameObject("CAMERA_PARENT [" + beatmapParent.id + "]");
                            camParent.transform.SetParent(ObjectManager.inst.objectParent.transform);
                            camParent.transform.localScale = Vector3.zero;
                            var camParentComponent = camParent.AddComponent<CameraParent>();

                            camParentComponent.parentObject = beatmapParent;

                            camParentComponent.positionParent = beatmapParent.GetParentType(0);
                            camParentComponent.scaleParent = beatmapParent.GetParentType(1);
                            camParentComponent.rotationParent = beatmapParent.GetParentType(2);

                            camParentComponent.positionParentOffset = beatmapParent.getParentOffset(0);
                            camParentComponent.scaleParentOffset = beatmapParent.getParentOffset(1);
                            camParentComponent.rotationParentOffset = beatmapParent.getParentOffset(2);
                        }
                        else if (!ObjectManager.inst.objectParent.transform.Find("CAMERA_PARENT [" + beatmapParent.id + "]").GetComponent<CameraParent>())
                        {
                            camParent = ObjectManager.inst.objectParent.transform.Find("CAMERA_PARENT [" + beatmapParent.id + "]").gameObject;

                            var camParentComponent = camParent.AddComponent<CameraParent>();

                            camParentComponent.parentObject = beatmapParent;

                            camParentComponent.positionParent = beatmapParent.GetParentType(0);
                            camParentComponent.scaleParent = beatmapParent.GetParentType(1);
                            camParentComponent.rotationParent = beatmapParent.GetParentType(2);

                            camParentComponent.positionParentOffset = beatmapParent.getParentOffset(0);
                            camParentComponent.scaleParentOffset = beatmapParent.getParentOffset(1);
                            camParentComponent.rotationParentOffset = beatmapParent.getParentOffset(2);
                        }
                        else
                        {
                            camParent = ObjectManager.inst.objectParent.transform.Find("CAMERA_PARENT [" + beatmapParent.id + "]").gameObject;

                            var camParentComponent = camParent.GetComponent<CameraParent>();

                            camParentComponent.parentObject = beatmapParent;

                            camParentComponent.positionParent = beatmapParent.GetParentType(0);
                            camParentComponent.scaleParent = beatmapParent.GetParentType(1);
                            camParentComponent.rotationParent = beatmapParent.GetParentType(2);

                            camParentComponent.positionParentOffset = beatmapParent.getParentOffset(0);
                            camParentComponent.scaleParentOffset = beatmapParent.getParentOffset(1);
                            camParentComponent.rotationParentOffset = beatmapParent.getParentOffset(2);
                        }

                        top.transform.SetParent(camParent.transform);
                        top.transform.localScale = Vector3.one;
                    }
                    else if (ObjectManager.inst.objectParent.transform.Find("CAMERA_PARENT [" + beatmapParent.id + "]"))
                        Object.Destroy(ObjectManager.inst.objectParent.transform.Find("CAMERA_PARENT [" + beatmapParent.id + "]").gameObject);

                    if (beatmapParent.parent == "PLAYER_PARENT")
                    {
                        GameObject playerParent;
                        if (!ObjectManager.inst.objectParent.transform.Find("PLAYER_PARENT [" + beatmapParent.id + "]"))
                        {
                            playerParent = new GameObject("PLAYER_PARENT [" + beatmapObject.id + "]");
                            playerParent.transform.SetParent(ObjectManager.inst.objectParent.transform);
                            playerParent.transform.localScale = Vector3.zero;
                            var delayTracker = playerParent.AddComponent<RTDelayTracker>();

                            delayTracker.move = beatmapObject.GetParentType(0);
                            delayTracker.rotate = beatmapObject.GetParentType(2);

                            delayTracker.moveDelay = beatmapObject.getParentOffset(0);
                            delayTracker.rotateDelay = beatmapObject.getParentOffset(2);
                        }
                        else if (!ObjectManager.inst.objectParent.transform.Find("PLAYER_PARENT [" + pc[pc.Count - 1].id + "]").GetComponent<CameraParent>())
                        {
                            playerParent = ObjectManager.inst.objectParent.transform.Find("PLAYER_PARENT [" + pc[pc.Count - 1].id + "]").gameObject;

                            var delayTracker = playerParent.AddComponent<RTDelayTracker>();

                            delayTracker.move = beatmapObject.GetParentType(0);
                            delayTracker.rotate = beatmapObject.GetParentType(2);

                            delayTracker.moveDelay = beatmapObject.getParentOffset(0);
                            delayTracker.rotateDelay = beatmapObject.getParentOffset(2);
                        }
                        else
                        {
                            playerParent = ObjectManager.inst.objectParent.transform.Find("PLAYER_PARENT [" + pc[pc.Count - 1].id + "]").gameObject;

                            var delayTracker = playerParent.GetComponent<RTDelayTracker>();

                            delayTracker.move = beatmapObject.GetParentType(0);
                            delayTracker.rotate = beatmapObject.GetParentType(2);

                            delayTracker.moveDelay = beatmapObject.getParentOffset(0);
                            delayTracker.rotateDelay = beatmapObject.getParentOffset(2);
                        }

                        top.transform.SetParent(playerParent.transform);
                        top.transform.localScale = Vector3.one;
                    }
                    else if (ObjectManager.inst.objectParent.transform.Find("PLAYER_PARENT [" + beatmapParent.id + "]"))
                        Object.Destroy(ObjectManager.inst.objectParent.transform.Find("PLAYER_PARENT [" + beatmapParent.id + "]").gameObject);
                }
            }
            catch (Exception e)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{Updater.className}Failed to set camera / player parent for '{beatmapObject.id}'.");
                stringBuilder.AppendLine($"Exception: {e.Message}");
                stringBuilder.AppendLine(e.StackTrace);

                Debug.LogError(stringBuilder.ToString());
            } // Camera / Player parenting

            try
            {
                if (parentObjects != null && parentObjects.Count > 0 && parentObjects[parentObjects.Count - 1] && parentObjects[parentObjects.Count - 1].Transform)
                {
                    parentObjects[parentObjects.Count - 1].Transform.SetParent(top.transform);
                    parentObjects[parentObjects.Count - 1].Transform.localScale = Vector3.one;
                }
                else
                {
                    baseObject.transform.SetParent(top.transform);
                    baseObject.transform.localScale = Vector3.one;
                }
            }
            catch (Exception e)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{Updater.className}Failed to set parent for '{beatmapObject.id}'.");
                stringBuilder.AppendLine($"ParentObjects is null: {parentObjects == null}.");
                if (parentObjects != null)
                {
                    stringBuilder.AppendLine($"ParentObjects Count: {parentObjects.Count}.");
                    if (parentObjects.Count > 0)
                    {
                        stringBuilder.AppendLine($"ParentObjects ParentObject {parentObjects.Count - 1} is null: {parentObjects[parentObjects.Count - 1] == null}.");
                        if (parentObjects[parentObjects.Count - 1] != null)
                            stringBuilder.AppendLine($"ParentObjects Transform {parentObjects.Count - 1} is null: {parentObjects[parentObjects.Count - 1].Transform == null}.");
                    }
                }
                stringBuilder.AppendLine($"Exception: {e.Message}");
                stringBuilder.AppendLine(e.StackTrace);

                Debug.LogError(stringBuilder.ToString());

                Object.Destroy(baseObject);
                Object.Destroy(top);

                return null;
            } // Parenting

            baseObject.SetActive(true);
            visualObject.SetActive(true);

            // Init visual object wrapper
            float opacity = beatmapObject.objectType == ObjectType.Helper ? 0.35f : 1.0f;
            bool hasCollider = beatmapObject.objectType == ObjectType.Helper ||
                               beatmapObject.objectType == ObjectType.Decoration;

            bool isSolid = beatmapObject.objectType == (ObjectType)4;

            // 4 = text object
            // 6 = image object
            VisualObject visual; //= beatmapObject.shape == 4
                //? new TextObject(visualObject, opacity, beatmapObject.text)
                //: new SolidObject(visualObject, opacity, hasCollider, isSolid);

            if (beatmapObject.shape == 4)
                visual = new TextObject(visualObject, opacity, beatmapObject.text);
            else if (beatmapObject.shape == 6)
                visual = new ImageObject(visualObject, opacity, beatmapObject.text);
            else
                visual = new SolidObject(visualObject, opacity, hasCollider, isSolid);

            LevelObject levelObject = new LevelObject(beatmapObject.id, 
                beatmapObject.StartTime,
                beatmapObject.StartTime + beatmapObject.GetObjectLifeLength(0.0f, true),
                cachedSequences[beatmapObject.id].ColorSequence,
                beatmapObject.Depth,
                parentObjects,
                visual,
                cachedSequences[beatmapObject.id].OpacitySequence,
                cachedSequences[beatmapObject.id].HueSequence,
                cachedSequences[beatmapObject.id].SaturationSequence,
                cachedSequences[beatmapObject.id].ValueSequence);

            levelObject.SetActive(false);

            try
            {
                if (EditorManager.inst)
                {
                    if (visualObject.TryGetComponent(out RTObject obj))
                    {
                        obj.SetObject(beatmapObject.id);

                        if (ModCompatibility.sharedFunctions.ContainsKey("HighlightColor"))
                            obj.highlightColor = (Color)ModCompatibility.sharedFunctions["HighlightColor"];
                        if (ModCompatibility.sharedFunctions.ContainsKey("HighlightDoubleColor"))
                            obj.highlightDoubleColor = (Color)ModCompatibility.sharedFunctions["HighlightDoubleColor"];
                        if (ModCompatibility.sharedFunctions.ContainsKey("CanHightlightObjects"))
                            obj.highlightObjects = (bool)ModCompatibility.sharedFunctions["CanHightlightObjects"];
                        if (ModCompatibility.sharedFunctions.ContainsKey("ShowObjectsOnLayer"))
                            obj.showObjectsOnlyOnLayer = (bool)ModCompatibility.sharedFunctions["ShowObjectsOnLayer"];
                        if (ModCompatibility.sharedFunctions.ContainsKey("ShowObjectsAlpha"))
                            obj.layerOpacity = (float)ModCompatibility.sharedFunctions["ShowObjectsAlpha"];

                        //if (ModCompatibility.sharedFunctions.ContainsKey("ShowEmpties"))
                        //    ??? = (bool)ModCompatibility.sharedFunctions["ShowEmpties"];
                        //if (ModCompatibility.sharedFunctions.ContainsKey("ShowDamagable"))
                        //    ??? = (bool)ModCompatibility.sharedFunctions["ShowDamagable"];
                    }

                    if (visualObject.TryGetComponent(out SelectObjectInEditor selectObjectInEditor))
                    {
                        selectObjectInEditor.obj = gameData.beatmapObjects.IndexOf(beatmapObject);
                    }
                }
            }
            catch (Exception e)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{Updater.className}Failed to generate FunctionObject for {beatmapObject.id} (Editor)");
                stringBuilder.AppendLine($"Exception: {e.Message}");
                stringBuilder.AppendLine(e.StackTrace);

                Debug.LogError(stringBuilder.ToString());
            } // Editor

            return levelObject;
        }

        GameObject InitParentChain(BeatmapObject beatmapObject, List<LevelParentObject> parentObjects)
        {
            GameObject gameObject = new GameObject(beatmapObject.name);

            parentObjects.Add(InitLevelParentObject(beatmapObject, gameObject));

            // Has parent - init parent (recursive)
            if (!string.IsNullOrEmpty(beatmapObject.parent) && beatmapObjects.ContainsKey(beatmapObject.parent))
            {
                GameObject parentObject = InitParentChain(beatmapObjects[beatmapObject.parent], parentObjects);

                gameObject.transform.SetParent(parentObject.transform);
            }

            return gameObject;
        }

        LevelParentObject InitLevelParentObject(BeatmapObject beatmapObject, GameObject gameObject)
        {
            CachedSequences cachedSequences = null;

            try
            {
                if (this.cachedSequences.ContainsKey(beatmapObject.id))
                    cachedSequences = this.cachedSequences[beatmapObject.id];
                //else
                //{
                //    CachedSequences collection = new CachedSequences();
                //    // For the mods that add Z axis to position keyframes.
                //    if (beatmapObject.events[0][0].eventValues.Length > 2)
                //    {
                //        collection = new CachedSequences()
                //        {
                //            Position3DSequence = GetVector3Sequence(beatmapObject.events[0], new Vector3Keyframe(0.0f, Vector3.zero, Ease.Linear)),
                //            ScaleSequence = GetVector2Sequence(beatmapObject.events[1], new Vector2Keyframe(0.0f, Vector2.one, Ease.Linear)),
                //            RotationSequence = GetFloatSequence(beatmapObject.events[2], new FloatKeyframe(0.0f, 0.0f, Ease.Linear), true)
                //        };
                //    }
                //    // If array is regular length
                //    else
                //    {
                //        collection = new CachedSequences()
                //        {
                //            PositionSequence = GetVector2Sequence(beatmapObject.events[0], new Vector2Keyframe(0.0f, Vector2.zero, Ease.Linear)),
                //            ScaleSequence = GetVector2Sequence(beatmapObject.events[1], new Vector2Keyframe(0.0f, Vector2.one, Ease.Linear)),
                //            RotationSequence = GetFloatSequence(beatmapObject.events[2], new FloatKeyframe(0.0f, 0.0f, Ease.Linear), true)
                //        };
                //    }

                //    // Empty objects don't need a color sequence, so it is not cached
                //    if (beatmapObject.objectType != ObjectType.Empty)
                //    {
                //        // For mods with Opacity and HSV values.
                //        if (beatmapObject.events[3][0].eventValues.Length > 2)
                //        {
                //            collection.ColorSequence = GetColorSequence(beatmapObject.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                //            collection.OpacitySequence = GetOpacitySequence(beatmapObject.events[3], 1, new FloatKeyframe(0.0f, 0, Ease.Linear));
                //            collection.HueSequence = GetOpacitySequence(beatmapObject.events[3], 2, new FloatKeyframe(0.0f, 0, Ease.Linear));
                //            collection.SaturationSequence = GetOpacitySequence(beatmapObject.events[3], 3, new FloatKeyframe(0.0f, 0, Ease.Linear));
                //            collection.ValueSequence = GetOpacitySequence(beatmapObject.events[3], 4, new FloatKeyframe(0.0f, 0, Ease.Linear));
                //        }
                //        // For mods with Opacity.
                //        else if (beatmapObject.events[3][0].eventValues.Length > 1)
                //        {
                //            collection.ColorSequence = GetColorSequence(beatmapObject.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                //            collection.OpacitySequence = GetOpacitySequence(beatmapObject.events[3], 1, new FloatKeyframe(0.0f, 0, Ease.Linear));
                //        }
                //        // If array is regular length.
                //        else
                //        {
                //            collection.ColorSequence = GetColorSequence(beatmapObject.events[3], new ThemeKeyframe(0.0f, 0, Ease.Linear));
                //        }
                //    }

                //    this.cachedSequences.Add(beatmapObject.id, collection);
                //    cachedSequences = this.cachedSequences[beatmapObject.id];
                //}

            }
            catch (Exception e)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Failed to init level parent object sequence for '{beatmapObject.id}'.");
                stringBuilder.AppendLine($"Exception: {e.Message}");
                stringBuilder.AppendLine(e.StackTrace);

                Debug.LogError(stringBuilder.ToString());
            }

            LevelParentObject levelParentObject = null;

            try
            {
                if (cachedSequences != null)
                    if (beatmapObject.events[0][0].eventValues.Length > 2)
                    {
                        levelParentObject = new LevelParentObject
                        {
                            Position3DSequence = cachedSequences.Position3DSequence,
                            ScaleSequence = cachedSequences.ScaleSequence,
                            RotationSequence = cachedSequences.RotationSequence,

                            TimeOffset = beatmapObject.StartTime,

                            ParentAnimatePosition = beatmapObject.GetParentType(0),
                            ParentAnimateScale = beatmapObject.GetParentType(1),
                            ParentAnimateRotation = beatmapObject.GetParentType(2),

                            ParentOffsetPosition = beatmapObject.getParentOffset(0),
                            ParentOffsetScale = beatmapObject.getParentOffset(1),
                            ParentOffsetRotation = beatmapObject.getParentOffset(2),

                            GameObject = gameObject,
                            Transform = gameObject.transform
                        };
                    }
                    else
                    {
                        levelParentObject = new LevelParentObject
                        {
                            PositionSequence = cachedSequences.PositionSequence,
                            ScaleSequence = cachedSequences.ScaleSequence,
                            RotationSequence = cachedSequences.RotationSequence,

                            TimeOffset = beatmapObject.StartTime,

                            ParentAnimatePosition = beatmapObject.GetParentType(0),
                            ParentAnimateScale = beatmapObject.GetParentType(1),
                            ParentAnimateRotation = beatmapObject.GetParentType(2),

                            ParentOffsetPosition = beatmapObject.getParentOffset(0),
                            ParentOffsetScale = beatmapObject.getParentOffset(1),
                            ParentOffsetRotation = beatmapObject.getParentOffset(2),

                            GameObject = gameObject,
                            Transform = gameObject.transform
                        };
                    }
                else
                {

                    var pos = new List<IKeyframe<Vector3>>();
                    pos.Add(new Vector3Keyframe(0f, Vector3.zero, Ease.Linear));

                    var sca = new List<IKeyframe<Vector2>>();
                    sca.Add(new Vector2Keyframe(0f, Vector2.one, Ease.Linear));

                    var rot = new List<IKeyframe<float>>();
                    rot.Add(new FloatKeyframe(0f, 0f, Ease.Linear));

                    levelParentObject = new LevelParentObject
                    {
                        Position3DSequence = new Sequence<Vector3>(pos),
                        ScaleSequence = new Sequence<Vector2>(sca),
                        RotationSequence = new Sequence<float>(rot),

                        TimeOffset = beatmapObject.StartTime,

                        ParentAnimatePosition = beatmapObject.GetParentType(0),
                        ParentAnimateScale = beatmapObject.GetParentType(1),
                        ParentAnimateRotation = beatmapObject.GetParentType(2),

                        ParentOffsetPosition = beatmapObject.getParentOffset(0),
                        ParentOffsetScale = beatmapObject.getParentOffset(1),
                        ParentOffsetRotation = beatmapObject.getParentOffset(2),

                        GameObject = gameObject,
                        Transform = gameObject.transform
                    };
                } // In case the CashedSequence is null, set defaults.
            }
            catch (Exception e)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Failed to init level parent object for '{beatmapObject.id}'.");
                stringBuilder.AppendLine($"Exception: {e.Message}");
                stringBuilder.AppendLine(e.StackTrace);

                Debug.LogError(stringBuilder.ToString());
            }

            return levelParentObject;
        }

        public Sequence<Vector3> GetVector3Sequence(List<EventKeyframe> eventKeyframes, Vector3Keyframe defaultKeyframe, bool relative = false)
        {
            List<IKeyframe<Vector3>> keyframes = new List<IKeyframe<Vector3>>(eventKeyframes.Count);

            Vector3 currentValue = Vector3.zero;
            foreach (EventKeyframe eventKeyframe in eventKeyframes)
            {
                Vector3 value = new Vector3(eventKeyframe.eventValues[0], eventKeyframe.eventValues[1], eventKeyframe.eventValues[2]);
                if (eventKeyframe.random != 0)
                {
                    Vector2 random = ObjectManager.inst.RandomVector2Parser(eventKeyframe);
                    value.x = random.x;
                    value.y = random.y;
                }

                currentValue = relative ? currentValue + value : value;

                keyframes.Add(new Vector3Keyframe(eventKeyframe.eventTime, value, Ease.GetEaseFunction(eventKeyframe.curveType.Name)));
            }

            // If there is no keyframe, add default
            if (keyframes.Count == 0)
            {
                keyframes.Add(defaultKeyframe);
            }

            return new Sequence<Vector3>(keyframes);
        }

        public Sequence<Vector2> GetVector2Sequence(List<EventKeyframe> eventKeyframes, Vector2Keyframe defaultKeyframe, bool relative = false)
        {
            List<IKeyframe<Vector2>> keyframes = new List<IKeyframe<Vector2>>(eventKeyframes.Count);

            Vector2 currentValue = Vector2.zero;
            foreach (EventKeyframe eventKeyframe in eventKeyframes)
            {
                Vector2 value = new Vector2(eventKeyframe.eventValues[0], eventKeyframe.eventValues[1]);
                if (eventKeyframe.random != 0)
                {
                    Vector2 random = ObjectManager.inst.RandomVector2Parser(eventKeyframe);
                    value.x = random.x;
                    value.y = random.y;
                }

                currentValue = relative ? currentValue + value : value;

                keyframes.Add(new Vector2Keyframe(eventKeyframe.eventTime, value, Ease.GetEaseFunction(eventKeyframe.curveType.Name)));
            }

            // If there is no keyframe, add default
            if (keyframes.Count == 0)
            {
                keyframes.Add(defaultKeyframe);
            }

            return new Sequence<Vector2>(keyframes);
        }

        public Sequence<float> GetFloatSequence(List<EventKeyframe> eventKeyframes, FloatKeyframe defaultKeyframe, bool relative = false)
        {
            List<IKeyframe<float>> keyframes = new List<IKeyframe<float>>(eventKeyframes.Count);

            float currentValue = 0.0f;
            foreach (EventKeyframe eventKeyframe in eventKeyframes)
            {
                float value = eventKeyframe.eventValues[0];
                if (eventKeyframe.random != 0)
                {
                    value = ObjectManager.inst.RandomFloatParser(eventKeyframe);
                }

                currentValue = relative ? currentValue + value : value;

                keyframes.Add(new FloatKeyframe(eventKeyframe.eventTime, currentValue, Ease.GetEaseFunction(eventKeyframe.curveType.Name)));
            }

            // If there is no keyframe, add default
            if (keyframes.Count == 0)
            {
                keyframes.Add(defaultKeyframe);
            }

            return new Sequence<float>(keyframes);
        }

        public Sequence<float> GetOpacitySequence(List<EventKeyframe> eventKeyframes, int val, FloatKeyframe defaultKeyframe, bool relative = false)
        {
            List<IKeyframe<float>> keyframes = new List<IKeyframe<float>>(eventKeyframes.Count);

            float currentValue = 0.0f;
            foreach (EventKeyframe eventKeyframe in eventKeyframes)
            {
                float value = eventKeyframe.eventValues[val];
                if (eventKeyframe.random != 0)
                {
                    value = ObjectManager.inst.RandomFloatParser(eventKeyframe);
                }

                currentValue = relative ? currentValue + value : value;

                keyframes.Add(new FloatKeyframe(eventKeyframe.eventTime, currentValue, Ease.GetEaseFunction(eventKeyframe.curveType.Name)));
            }

            // If there is no keyframe, add default
            if (keyframes.Count == 0)
            {
                keyframes.Add(defaultKeyframe);
            }

            return new Sequence<float>(keyframes);
        }

        public Sequence<Color> GetColorSequence(List<EventKeyframe> eventKeyframes, ThemeKeyframe defaultKeyframe)
        {
            List<IKeyframe<Color>> keyframes = new List<IKeyframe<Color>>(eventKeyframes.Count);

            foreach (EventKeyframe eventKeyframe in eventKeyframes)
            {
                int value = (int)eventKeyframe.eventValues[0];

                value = Mathf.Clamp(value, 0, GameManager.inst.LiveTheme.objectColors.Count - 1);

                keyframes.Add(new ThemeKeyframe(eventKeyframe.eventTime, value, Ease.GetEaseFunction(eventKeyframe.curveType.Name)));
            }

            // If there is no keyframe, add default
            if (keyframes.Count == 0)
            {
                keyframes.Add(defaultKeyframe);
            }

            return new Sequence<Color>(keyframes);
        }
    }
}
