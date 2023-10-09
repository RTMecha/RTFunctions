using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using TMPro;

using RTFunctions.Functions.Components;

using BeatmapObject = DataManager.GameData.BeatmapObject;
using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using EventKeyframe = DataManager.GameData.EventKeyframe;

using OGPrefab = DataManager.GameData.Prefab;
using OGPrefabObject = DataManager.GameData.PrefabObject;
using OGBackground = DataManager.GameData.BackgroundObject;

namespace RTFunctions.Functions.Managers
{
    public class Objects : MonoBehaviour
    {
        public static Objects inst;

        public bool active = false;

        void Awake() => inst = this;

        void Update()
        {
            if (DataManager.inst.gameData != null && active)
            {
                //foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
                //{
                //    if (beatmapObject.TimeWithinLifespan())
                //        updateObjects(beatmapObject);
                //    else
                //        beatmapObjects.Remove(beatmapObject.id);
                //}

                foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
                {
                    if (beatmapObject.objectType != ObjectType.Empty && beatmapObject.TimeWithinLifespan())
                    {
                        try
                        {
                            var functionObject = new FunctionObject(beatmapObject);

                            //beatmapObjects.Add(beatmapObject.id, functionObject);
                            updateFunctionObject(functionObject);
                        }
                        catch
                        {

                        }

                        //if (!beatmapObjects.ContainsKey(beatmapObject.id))
                        //{
                        //    var functionObject = new FunctionObject(beatmapObject);

                        //    beatmapObjects.Add(beatmapObject.id, functionObject);
                        //    updateFunctionObject(functionObject);
                        //}
                    }
                }

                //for (int i = 0; i < beatmapObjects.Count; i++)
                //{
                //    var objectBeatmap = beatmapObjects.ElementAt(i);
                //    var beatmapObject = objectBeatmap.Value.beatmapObject;
                //    if (DataManager.inst.gameData.beatmapObjects.Find(x => x.id == objectBeatmap.Key) == null || beatmapObject.objectType == ObjectType.Empty || !beatmapObject.TimeWithinLifespan())
                //    {
                //        beatmapObjects.Remove(objectBeatmap.Key);
                //    }
                //    else if (objectBeatmap.Value.gameObject == null)
                //    {
                //        updateFunctionObject(objectBeatmap.Value);
                //    }
                //}
            }

            //if (DataManager.inst.gameData == null || DataManager.inst.gameData.beatmapObjects == null || DataManager.inst.gameData.beatmapObjects.Count < 1)
            //{
            //    beatmapObjects.Clear();
            //}
        }

        public static void updateFunctionObject(FunctionObject functionObject)
        {
            var beatmapObject = functionObject.beatmapObject;

            if (beatmapObject != null && functionObject.gameObject == null)
            {
                if (beatmapObject.TryGetGameObject(out GameObject gm) && gm != null && beatmapObject.TryGetTransformChain(out List<Transform> tf) && tf != null)
                {
                    if (functionObject.gameObject == null && gm != null)
                    {
                        functionObject.gameObject = gm;
                    }

                    if ((functionObject.transformChain == null || functionObject.transformChain.Any(x => x == null)) && tf != null)
                    {
                        functionObject.transformChain = tf;
                    }

                    if (functionObject.renderer == null && gm != null)
                    {
                        if (gm.TryGetComponent(out Renderer renderer) && renderer != null)
                            functionObject.renderer = renderer;
                    }

                    if (functionObject.collider == null && gm != null)
                    {
                        if (gm.TryGetComponent(out Collider2D collider) && collider != null)
                            functionObject.collider = collider;
                    }

                    if (functionObject.meshFilter == null && gm != null)
                    {
                        if (gm.TryGetComponent(out MeshFilter meshFilter) && meshFilter != null)
                            functionObject.meshFilter = gm.GetComponent<MeshFilter>();
                    }

                    if (functionObject.selectObject == null && gm != null)
                    {
                        if (gm.TryGetComponent(out SelectObjectInEditor selectObject) && selectObject != null)
                            functionObject.selectObject = selectObject;
                    }
                    if (functionObject.rtObject == null && gm != null)
                    {
                        if (gm.TryGetComponent(out RTObject rt) && rt != null)
                            functionObject.rtObject = rt;
                    }
                    if (functionObject.text == null && gm != null && beatmapObject.shape == 4)
                    {
                        if (gm.TryGetComponent(out TextMeshPro text) && text != null)
                            functionObject.text = text;
                    }
                }
            }
        }

        //public static void updateFunctionObjects()
        //{
        //    if (GameManager.inst != null && GameManager.inst.gameState != GameManager.State.Loading && GameManager.inst.gameState != GameManager.State.Parsing)
        //    {
        //        if (beatmapObjects.Count > 0)
        //        {
        //            for (int i = 0; i < beatmapObjects.Count; i++)
        //            {
        //                var objectBeatmap = beatmapObjects.ElementAt(i);
        //                var functionObject = objectBeatmap.Value;
        //                var beatmapObject = objectBeatmap.Value.beatmapObject;

        //                if (beatmapObject != null && functionObject.gameObject == null)
        //                {
        //                    if (beatmapObject.TryGetGameObject(out GameObject gm) && gm != null && beatmapObject.TryGetTransformChain(out List<Transform> tf) && tf != null)
        //                    {
        //                        if (functionObject.gameObject == null && gm != null)
        //                        {
        //                            functionObject.gameObject = gm;
        //                        }

        //                        if (functionObject.transformChain == null && tf != null)
        //                        {
        //                            functionObject.transformChain = tf;
        //                        }

        //                        if (functionObject.renderer == null && gm != null)
        //                        {
        //                            if (gm.TryGetComponent(out Renderer renderer) && renderer != null)
        //                                functionObject.renderer = renderer;
        //                        }

        //                        if (functionObject.collider == null && gm != null)
        //                        {
        //                            if (gm.TryGetComponent(out Collider2D collider) && collider != null)
        //                                functionObject.collider = collider;
        //                        }

        //                        if (functionObject.meshFilter == null && gm != null)
        //                        {
        //                            if (gm.TryGetComponent(out MeshFilter meshFilter) && meshFilter != null)
        //                                functionObject.meshFilter = gm.GetComponent<MeshFilter>();
        //                        }

        //                        if (functionObject.selectObject == null && gm != null)
        //                        {
        //                            if (gm.TryGetComponent(out SelectObjectInEditor selectObject) && selectObject != null)
        //                                functionObject.selectObject = selectObject;
        //                        }
        //                        if (functionObject.rtObject == null && gm != null)
        //                        {
        //                            if (gm.TryGetComponent(out RTObject rt) && rt != null)
        //                                functionObject.rtObject = rt;
        //                        }
        //                        if (functionObject.text == null && gm != null && beatmapObject.shape == 4)
        //                        {
        //                            if (gm.TryGetComponent(out TextMeshPro text) && text != null)
        //                                functionObject.text = text;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public List<FunctionObject> functionObjects = new List<FunctionObject>();

        //public static Dictionary<string, FunctionObject> beatmapObjects = new Dictionary<string, FunctionObject>();

        public static List<BackgroundObject> backgroundObjects = new List<BackgroundObject>();

        public static Dictionary<string, Prefab> prefabs = new Dictionary<string, Prefab>();

        public static Dictionary<string, PrefabObject> prefabObjects = new Dictionary<string, PrefabObject>();

        public class FunctionObject
        {
            public string id;

            public FunctionObject(BeatmapObject beatmapObject)
            {
                id = beatmapObject.id;
                this.beatmapObject = beatmapObject;
                otherComponents = new Dictionary<string, object>();
            }

            public BeatmapObject beatmapObject;

            public Dictionary<string, object> otherComponents;

            public GameObject gameObject;

            public List<Transform> transformChain;

            public Renderer renderer;

            public TextMeshPro text;

            public SelectObjectInEditor selectObject;

            public Collider2D collider;

            public MeshFilter meshFilter;

            public RTObject rtObject;
        }

        public class BackgroundObject
        {
            public BackgroundObject()
            {

            }

            public OGBackground bg;

            public List<GameObject> gameObjects;
            public Vector2Int shape;

            public Vector2Int reactivePosChannels;
            public Vector2Int reactiveScaChannels;
            public int reactiveRotChannel;

            public Vector2 reactivePosIntensity;
            public Vector2 reactiveScaIntensity;
            public float reactiveRotIntensity;
        }

        public class PrefabObject
        {
            public PrefabObject(OGPrefabObject prefabObject)
            {
                this.prefabObject = prefabObject;
            }

            public OGPrefabObject prefabObject;
            public Dictionary<string, object> modifiers = new Dictionary<string, object>();
            public float speed = 1f;
        }

        public class Prefab
        {
            public Prefab(OGPrefab prefab)
            {
                this.prefab = prefab;
            }

            public OGPrefab prefab;

            public List<BeatmapObject> objects = new List<BeatmapObject>();
            public Dictionary<string, object> modifiers = new Dictionary<string, object>();
        }
    }
}
