using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using TMPro;

using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Managers.Networking;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using EventKeyframe = DataManager.GameData.EventKeyframe;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BasePrefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;
using BaseBackground = DataManager.GameData.BackgroundObject;
using BaseEditorData = DataManager.GameData.BeatmapObject.EditorData;
using BasePrefabType = DataManager.PrefabType;

namespace RTFunctions.Functions.Data
{
    public class PrefabType : BasePrefabType
    {
        PrefabType()
        {

        }

        public PrefabType(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        static string[] iconLocations = new string[]
        {

        };

        Sprite icon;
        public Sprite Icon
        {
            get
            {
                try
                {
                    if (!icon)
                        AlephNetworkManager.inst.StartCoroutine(AlephNetworkManager.DownloadImageTexture(iconLocations[DataManager.inst.PrefabTypes.FindIndex(x => x.Name == Name)], delegate (Texture2D texture2D)
                        {
                            icon = RTSpriteManager.CreateSprite(texture2D);
                        }));
                    return icon;
                }
                catch
                {
                    return null;
                }
            }
            set => icon = value;
        }
    }
}
