using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions.IO;

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
