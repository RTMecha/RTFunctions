using System;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;
using LSFunctions;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.Optimization;

namespace RTFunctions.Patchers
{
	public delegate void LevelTickEventHandler();

	[HarmonyPatch(typeof(ObjectManager))]
    public class ObjectManagerPatch : MonoBehaviour
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void AwakePatch(ObjectManager __instance)
        {
            foreach (var option in __instance.objectPrefabs[5].options)
            {
                option.GetComponentInChildren<Collider2D>().isTrigger = true;
            }
		}

		[HarmonyPatch("AddPrefabToLevel")]
		[HarmonyPrefix]
		static bool AddPrefabToLevelPrefix(DataManager.GameData.PrefabObject __0)
		{
			var prefabObject = (PrefabObject)__0;

			bool flag = DataManager.inst.gameData.prefabs.FindIndex(x => x.ID == __0.prefabID) != -1;
			if (!flag)
			{
				DataManager.inst.gameData.prefabObjects.RemoveAll(x => x.prefabID == __0.prefabID);
			}

			if (!(!string.IsNullOrEmpty(__0.prefabID) && flag))
			{
				return false;
			}

			float t = 1f;

			if (__0.RepeatOffsetTime != 0f)
				t = __0.RepeatOffsetTime;

			float timeToAdd = 0f;

			var prefab = DataManager.inst.gameData.prefabs.Find(x => x.ID == __0.prefabID);
			
			for (int i = 0; i < __0.RepeatCount + 1; i++)
			{
				var ids = new Dictionary<string, string>();

				foreach (var beatmapObject in prefab.objects)
				{
					string value = LSText.randomString(16);
					ids.Add(beatmapObject.id, value);
				}

				string iD = __0.ID;
				foreach (var beatmapObj in prefab.objects)
				{
					var beatmapObject = BeatmapObject.DeepCopy((BeatmapObject)beatmapObj, false);
					if (ids.ContainsKey(beatmapObj.id))
						beatmapObject.id = ids[beatmapObj.id];

					if (ids.ContainsKey(beatmapObj.parent))
						beatmapObject.parent = ids[beatmapObj.parent];
					else if (DataManager.inst.gameData.beatmapObjects.FindIndex(x => x.id == beatmapObj.parent) == -1)
						beatmapObject.parent = "";

					beatmapObject.active = false;
					beatmapObject.fromPrefab = true;
                    beatmapObject.prefabInstanceID = iD;

					beatmapObject.StartTime = __0.StartTime + prefab.Offset + (beatmapObject.StartTime + timeToAdd) * prefabObject.speed;

                    //beatmapObject.StartTime += timeToAdd;
					//beatmapObject.StartTime += __0.StartTime;
					//beatmapObject.StartTime += prefab.Offset;

					beatmapObject.prefabID = __0.prefabID;

					//if (EditorManager.inst != null)
					//{
					//	beatmapObject.editorData.Layer = EditorManager.inst.layer;
					//}
					DataManager.inst.gameData.beatmapObjects.Add(beatmapObject);

					Updater.UpdateProcessor(beatmapObject);
				}

				timeToAdd += t;
			}

			return false;
		}

		public static event LevelTickEventHandler LevelTick;

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static bool UpdatePrefix()
		{
			LevelTick?.Invoke();
			return false;
		}

		[HarmonyPatch("updateObjects", new Type[] { })]
		[HarmonyPrefix]
		static bool updateObjectsPrefix4(ObjectManager __instance)
		{
			AddPrefabObjects(__instance);
			Updater.UpdateObjects();
			return false;
		}

		public static void AddPrefabObjects(ObjectManager __instance)
		{
			DataManager.inst.gameData.beatmapObjects.RemoveAll(x => x.fromPrefab);
			for (int i = 0; i < DataManager.inst.gameData.prefabObjects.Count; i++)
			{
				__instance.AddPrefabToLevel(DataManager.inst.gameData.prefabObjects[i]);
			}
		}
	}
}
