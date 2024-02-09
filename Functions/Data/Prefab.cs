using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using SimpleJSON;
using LSFunctions;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BasePrefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;

namespace RTFunctions.Functions.Data
{
    public class Prefab : BasePrefab
    {
        public Prefab()
        {

        }

        public Prefab(string name, int type, float offset, List<BeatmapObject> beatmapObjects, List<PrefabObject> pObjects)
        {
            Name = name;
            Type = type;
            Offset = offset;
            foreach (var beatmapObject in beatmapObjects)
            {
                objects.Add(BeatmapObject.DeepCopy(beatmapObject, false));
            }
            foreach (var prefabObject in pObjects)
            {
                prefabObjects.Add(PrefabObject.DeepCopy(prefabObject, false));
            }

            float a;
            if (objects.Count <= 0)
            {
                a = 10000000f;
            }
            else
            {
                a = objects.Min(val => val.StartTime);
            }

            float b;
            if (prefabObjects.Count <= 0)
            {
                b = 10000000f;
            }
            else
            {
                b = prefabObjects.Min(val => val.StartTime);
            }

            float num = Mathf.Min(a, b);
            int num2 = 0;
            foreach (var beatmapObject in objects)
            {
                this.objects[num2].StartTime -= num;
                num2++;
            }
            num2 = 0;
            foreach (var prefabObject in prefabObjects)
            {
                prefabObjects[num2].StartTime -= num;
                num2++;
            }
        }

        public Prefab(BasePrefab prefab)
        {

        }

        public string description;

        public PrefabType PrefabType => Type >= 0 && Type < DataManager.inst.PrefabTypes.Count ? (PrefabType)DataManager.inst.PrefabTypes[Type] : PrefabType.InvalidType;
        public Color TypeColor => PrefabType.Color;
        public string TypeName => PrefabType.Name;

        #region Methods

        public static Prefab DeepCopy(Prefab og, bool newID = true) => new Prefab()
        {
            description = og.description,
            ID = newID ? LSText.randomString(16) : og.ID,
            MainObjectID = og.MainObjectID,
            Name = og.Name,
            objects = og.objects.Clone(),
            Offset = og.Offset,
            prefabObjects = og.prefabObjects.Clone(),
            Type = og.Type
        };

        public static Prefab ParseVG(JSONNode jn)
        {
            var beatmapObjects = new List<BaseBeatmapObject>();
            for (int i = 0; i < jn["objs"].Count; i++)
                beatmapObjects.Add(BeatmapObject.ParseVG(jn["objs"][i]));

            return new Prefab
            {
                ID = jn["id"] == null ? LSText.randomString(16) : jn["id"],
                MainObjectID = LSText.randomString(16),
                Name = jn["n"],
                Type = jn["type"].AsInt,
                Offset = -jn["o"].AsFloat,
                objects = beatmapObjects,
                prefabObjects = new List<BasePrefabObject>(),
                description = jn["description"],
            };
        }

        public static Prefab Parse(JSONNode jn)
        {
            var beatmapObjects = new List<BaseBeatmapObject>();
            for (int j = 0; j < jn["objects"].Count; j++)
                beatmapObjects.Add(BeatmapObject.Parse(jn["objects"][j]));

            var prefabObjects = new List<BasePrefabObject>();
            for (int k = 0; k < jn["prefab_objects"].Count; k++)
                prefabObjects.Add(PrefabObject.Parse(jn["prefab_objects"][k]));

            return new Prefab
            {
                ID = jn["id"],
                MainObjectID = jn["main_obj_id"] == null ? LSText.randomString(16) : jn["main_obj_id"],
                Name = jn["name"],
                Type = jn["type"].AsInt,
                Offset = jn["offset"].AsFloat,
                objects = beatmapObjects,
                prefabObjects = prefabObjects,
                description = jn["desc"] == null ? "" : jn["desc"]
            };
        }

        public JSONNode ToJSONVG()
        {
            var jn = JSON.Parse("{}");
            jn["n"] = Name;
            if (ID != null)
                jn["id"] = ID;
            jn["type"] = Type;

            jn["o"] = -Offset;

            jn["description"] = description;

            for (int i = 0; i < objects.Count; i++)
                if (objects[i] != null)
                    jn["objs"][i] = ((BeatmapObject)objects[i]).ToJSONVG();

            return jn;
        }

        public JSONNode ToJSON()
        {
            var jn = JSON.Parse("{}");
            jn["name"] = Name;
            jn["type"] = Type.ToString();
            jn["offset"] = Offset.ToString();

            if (ID != null)
                jn["id"] = ID.ToString();

            if (MainObjectID != null)
                jn["main_obj_id"] = MainObjectID.ToString();

            jn["desc"] = description == null ? "" : description;

            for (int i = 0; i < objects.Count; i++)
                jn["objects"][i] = ((BeatmapObject)objects[i]).ToJSON();

            if (prefabObjects != null && prefabObjects.Count > 0)
                for (int i = 0; i < prefabObjects.Count; i++)
                        jn["prefab_objects"][i] = ((PrefabObject)prefabObjects[i]).ToJSON();
            return jn;
        }

        #endregion

        #region Operators

        public static implicit operator bool(Prefab exists) => exists != null;

        public override bool Equals(object obj) => obj is Prefab && ID == (obj as Prefab).ID;

        public override string ToString() => ID;

        #endregion
    }
}
