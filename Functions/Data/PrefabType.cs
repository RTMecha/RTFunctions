using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions;
using RTFunctions.Functions.IO;
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

        public int Index { get; set; }

        Sprite icon;
        public Sprite Icon
        {
            get => icon;
            set => icon = value;
        }

        public static PrefabType Parse(JSONNode jn) => new PrefabType(jn["name"], LSFunctions.LSColors.HexToColorAlpha(jn["color"]));
        public JSONNode ToJSON()
        {
            var jn = JSON.Parse("{}");
            jn["name"] = Name;
            jn["color"] = RTHelpers.ColorToHex(Color);
            jn["index"] = Index.ToString();

            return jn;
        }
    }
}
