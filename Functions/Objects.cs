using System;
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

namespace RTFunctions.Functions
{
    public class Objects : MonoBehaviour
    {
        //Move this to RTFunctions.Functions.Managers

        public static Objects inst;

        private void Awake()
        {
            inst = this;
        }

        private void Update()
        {
            if (DataManager.inst.gameData != null)
            {
                foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
                {
                    if (!beatmapObjects.ContainsKey(beatmapObject.id) && beatmapObject.objectType != ObjectType.Empty && beatmapObject.TimeWithinLifespan())
                    {
                        var functionObject = new FunctionObject(beatmapObject);

                        updateFunctionObject(functionObject);
                        beatmapObjects.Add(beatmapObject.id, functionObject);
                    }
                }

                for (int i = 0; i < beatmapObjects.Count; i++)
                {
                    var objectBeatmap = beatmapObjects.ElementAt(i);
                    var beatmapObject = objectBeatmap.Value.beatmapObject;
                    if (DataManager.inst.gameData.beatmapObjects.Find(x => x.id == objectBeatmap.Key) == null || beatmapObject.objectType == ObjectType.Empty || !beatmapObject.TimeWithinLifespan())
                    {
                        beatmapObjects.Remove(objectBeatmap.Key);
                    }
                    else if (objectBeatmap.Value.gameObject == null)
                    {
                        updateFunctionObject(objectBeatmap.Value);
                    }
                }
            }

            if (DataManager.inst.gameData == null || DataManager.inst.gameData.beatmapObjects.Count < 1)
            {
                beatmapObjects.Clear();
            }
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

        public static void updateFunctionObjects()
        {
            if (GameManager.inst != null && GameManager.inst.gameState != GameManager.State.Loading && GameManager.inst.gameState != GameManager.State.Parsing)
            {
                if (beatmapObjects.Count > 0)
                {
                    for (int i = 0; i < beatmapObjects.Count; i++)
                    {
                        var objectBeatmap = beatmapObjects.ElementAt(i);
                        var functionObject = objectBeatmap.Value;
                        var beatmapObject = objectBeatmap.Value.beatmapObject;

                        if (beatmapObject != null && functionObject.gameObject == null)
                        {
                            if (beatmapObject.TryGetGameObject(out GameObject gm) && gm != null && beatmapObject.TryGetTransformChain(out List<Transform> tf) && tf != null)
                            {
                                if (functionObject.gameObject == null && gm != null)
                                {
                                    functionObject.gameObject = gm;
                                }

                                if (functionObject.transformChain == null && tf != null)
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
                }
            }
        }

        public static Dictionary<string, FunctionObject> beatmapObjects = new Dictionary<string, FunctionObject>();

        public class FunctionObject
        {
            public FunctionObject(BeatmapObject beatmapObject)
            {
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
    }
}
