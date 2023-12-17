using UnityEngine;

using SimpleJSON;

using BaseEditorData = DataManager.GameData.BeatmapObject.EditorData;

namespace RTFunctions.Functions.Data
{
    public class ObjectEditorData : BaseEditorData
    {
        public ObjectEditorData()
        {

        }

        public ObjectEditorData(int bin, int layer, bool collapse, bool locked)
        {
            Bin = bin;
            Layer = layer;
            this.collapse = collapse;
            this.locked = locked;
        }

        public new int Layer
        {
            get => Mathf.Clamp(layer, 0, int.MaxValue);
            set => layer = Mathf.Clamp(value, 0, int.MaxValue);
        }

        #region Methods

        public static ObjectEditorData DeepCopy(ObjectEditorData orig) => new ObjectEditorData
        {
            Bin = orig.Bin,
            Layer = orig.Layer,
            collapse = orig.collapse,
            locked = orig.locked
        };

        public static ObjectEditorData Parse(JSONNode jn) => new ObjectEditorData
        {
            Bin = jn["bin"] == null ? 0 : jn["bin"].AsInt,
            Layer = jn["layer"] == null ? 0 : jn["layer"].AsInt,
            collapse = jn["shrink"] == null ? jn["collapse"] == null ? false : jn["collapse"].AsBool : jn["shrink"].AsBool,
            locked = jn["locked"] == null ? false : jn["locked"].AsBool,
        };

        public JSONNode ToJSON()
        {
            var jn = JSON.Parse("{}");

            jn["bin"] = Bin.ToString();
            jn["layer"] = Layer.ToString();
            if (collapse)
                jn["collapse"] = collapse.ToString();
            if (locked)
                jn["locked"] = locked.ToString();

            return jn;
        }

        #endregion
    }
}
