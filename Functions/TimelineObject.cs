using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.Optimization;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BaseEventKeyframe = DataManager.GameData.EventKeyframe;
using Prefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;

namespace RTFunctions.Functions
{
    public class TimelineObject<T> : Exists
    {
        public TimelineObject(T data)
        {
            Data = data;
        }

        public TimelineObject(T data, GameObject gameObject, Image image)
        {
            Data = data;
            GameObject = gameObject;
            Image = image;
        }
        
        public TimelineObject(T data, int type, GameObject gameObject, Image image)
        {
            Data = data;
            Type = type;
            GameObject = gameObject;
            Image = image;
        }

        public T Data { get; set; }
        public GameObject GameObject { get; set; }
        public Image Image { get; set; }

        public string ID
        {
            get
            {
                if (IsBeatmapObject)
                    return (Data as BaseBeatmapObject).id;
                if (IsPrefabObject)
                    return (Data as BasePrefabObject).ID;
                return "";
            }
        }

        public int Type { get; set; }
        int index;
        public int Index
        {
            get
            {
                if (IsBeatmapObject)
                    return DataManager.inst.gameData.beatmapObjects.IndexOf(Data as BeatmapObject);
                if (IsPrefabObject)
                    return DataManager.inst.gameData.prefabObjects.IndexOf(Data as PrefabObject);
                if (IsEventKeyframe)
                    return index;
                return -1;
            }
            set => index = value;
        }

        public float Time
        {
            get
            {
                if (IsBeatmapObject)
                    return (Data as BeatmapObject).StartTime;
                if (IsPrefabObject)
                    return (Data as PrefabObject).StartTime;
                if (IsEventKeyframe)
                    return (Data as EventKeyframe).eventTime;
                return -1;
            }
        }

        public int Layer
        {
            get
            {
                if (IsBeatmapObject)
                    return (Data as BeatmapObject).editorData.Layer;
                if (IsPrefabObject)
                    return (Data as PrefabObject).editorData.Layer;
                if (IsEventKeyframe)
                    return 5;
                return -1;
            }
        }
        public float timeOffset;
        public int binOffset;

        public bool selected;

        public bool IsBeatmapObject => Data != null && Data is BeatmapObject;
        public bool IsPrefabObject => Data != null && Data is PrefabObject;
        public bool IsEventKeyframe => Data != null && Data is EventKeyframe;
    }
}
