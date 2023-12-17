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

        public DataManager.PrefabType PrefabType => DataManager.inst.PrefabTypes[Type];
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

        public JSONNode ToJSON()
        {
            JSONNode jn = JSON.Parse("{}");
            jn["name"] = Name;
            jn["type"] = Type.ToString();
            jn["offset"] = Offset.ToString();

            if (ID != null)
                jn["id"] = ID.ToString();

            if (MainObjectID != null)
                jn["main_obj_id"] = MainObjectID.ToString();

            jn["desc"] = description == null ? "" : description;

            for (int i = 0; i < objects.Count; i++)
                if (objects[i] != null)
                    jn["objects"][i] = ((BeatmapObject)objects[i]).ToJSON();

            for (int i = 0; i < prefabObjects.Count; i++)
                if (prefabObjects[i] != null)
                    jn["prefab_objects"][i] = ((PrefabObject)prefabObjects[i]).ToJSON();
            return jn;
        }

        #endregion
    }
}
