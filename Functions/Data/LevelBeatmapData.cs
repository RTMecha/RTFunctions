﻿using SimpleJSON;
using System.Collections.Generic;
using BaseBeatmapData = DataManager.GameData.BeatmapData;

namespace RTFunctions.Functions.Data
{
    public class LevelBeatmapData : BaseBeatmapData
    {
        public LevelBeatmapData()
        {

        }

		public static LevelBeatmapData ParseVG(JSONNode jn)
        {
			var beatmapData = new LevelBeatmapData();

			beatmapData.editorData = new LevelEditorData();

			beatmapData.markers = new List<Marker>();

			for (int i = 0; i < jn["markers"].Count; i++)
            {
				var jnmarker = jn["markers"][i];

				var name = jnmarker["n"] == null ? "Marker" : (string)jnmarker["n"];

				var desc = jnmarker["d"] == null ? "" : (string)jnmarker["d"];

				var col = jnmarker["c"].AsInt;

				var time = jnmarker["t"].AsFloat;

				beatmapData.markers.Add(new Marker(true, name, desc, col, time));
			}
			return beatmapData;
        }

        public static LevelBeatmapData Parse(JSONNode jn)
        {
            var beatmapData = new LevelBeatmapData();

            beatmapData.editorData = LevelEditorData.Parse(jn["ed"]);

			beatmapData.markers = new List<Marker>();
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
			var jn = ((LevelEditorData)editorData).ToJSON();

			for (int i = 0; i < markers.Count; i++)
            {
				jn["markers"][i]["active"] = markers[i].active;
				jn["markers"][i]["name"] = markers[i].name;
				jn["markers"][i]["desc"] = markers[i].desc;
				jn["markers"][i]["col"] = markers[i].color;
				jn["markers"][i]["t"] = markers[i].time;
            }

			return jn;
        }
	}
}
