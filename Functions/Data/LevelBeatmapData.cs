using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleJSON;

using BaseBeatmapData = DataManager.GameData.BeatmapData;

namespace RTFunctions.Functions.Data
{
    public class LevelBeatmapData : BaseBeatmapData
    {
        public LevelBeatmapData()
        {

        }

        public static LevelBeatmapData Parse(JSONNode jn)
        {
            var beatmapData = new LevelBeatmapData();

            beatmapData.editorData = LevelEditorData.Parse(jn["ed"]);

			beatmapData.markers = new List<Marker>();
			beatmapData.markers.Clear();
			for (int i = 0; i < jn["ed"]["markers"].Count; i++)
			{
				bool asBool = jn["ed"]["markers"][i]["active"].AsBool;
				string name = "Marker";
				if (jn["ed"]["markers"][i]["name"] != null)
				{
					name = jn["ed"]["markers"][i]["name"];
				}
				string desc = "";
				if (jn["ed"]["markers"][i]["desc"] != null)
				{
					desc = jn["ed"]["markers"][i]["desc"];
				}
				float asFloat = jn["ed"]["markers"][i]["t"].AsFloat;
				int color = 0;
				if (jn["ed"]["markers"][i]["col"] != null)
				{
					color = jn["ed"]["markers"][i]["col"].AsInt;
				}
				beatmapData.markers.Add(new Marker(asBool, name, desc, color, asFloat));
			}
			return beatmapData;
        }

		public JSONNode ToJSON()
        {
			var jn = JSON.Parse("{}");

			jn["ed"] = ((LevelEditorData)editorData).ToJSON();

			for (int i = 0; i < markers.Count; i++)
            {
				jn["ed"]["markers"][i]["active"] = markers[i].active;
				jn["ed"]["markers"][i]["name"] = markers[i].name;
				jn["ed"]["markers"][i]["desc"] = markers[i].desc;
				jn["ed"]["markers"][i]["col"] = markers[i].color;
				jn["ed"]["markers"][i]["t"] = markers[i].time;
            }

			return jn;
        }
    }
}
