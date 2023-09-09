using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using UnityEngine;
using LSFunctions;

using RTFunctions.Functions;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Patchers
{
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

			for (int i = 0; i < __0.RepeatCount + 1; i++)
			{
				var dictionary = new Dictionary<string, string>();

				var prefab = DataManager.inst.gameData.prefabs.Find(x => x.ID == __0.prefabID);

				foreach (var beatmapObject in prefab.objects)
				{
					string value = LSText.randomString(16);
					dictionary.Add(beatmapObject.id, value);
				}

				string iD = __0.ID;
				foreach (var beatmapObj in prefab.objects)
				{
					var beatmapObject = DataManager.GameData.BeatmapObject.DeepCopy(beatmapObj, false);
					if (dictionary.ContainsKey(beatmapObj.id))
					{
						beatmapObject.id = dictionary[beatmapObj.id];
					}
					if (dictionary.ContainsKey(beatmapObj.parent))
					{
						beatmapObject.parent = dictionary[beatmapObj.parent];
					}
					else if (DataManager.inst.gameData.beatmapObjects.FindIndex(x => x.id == beatmapObj.parent) == -1)
					{
						beatmapObject.parent = "";
					}
					beatmapObject.active = false;
					beatmapObject.fromPrefab = true;
                    beatmapObject.prefabInstanceID = iD;

                    beatmapObject.StartTime += timeToAdd;
					beatmapObject.StartTime += __0.StartTime;
					beatmapObject.StartTime += prefab.Offset;

					beatmapObject.prefabID = __0.prefabID;

					//if (EditorManager.inst != null)
					//{
					//	beatmapObject.editorData.Layer = EditorManager.inst.layer;
					//}
					DataManager.inst.gameData.beatmapObjects.Add(beatmapObject);
				}

				timeToAdd += t;
			}

			return false;
		}

		//[HarmonyPatch("updateObjects", new Type[] { typeof(string) })]
		//[HarmonyPrefix]
		//static void updateObjectsPrefix1(ObjectManager __instance)
		//{
		//    Debug.LogFormat("{0}Updating Objects", FunctionsPlugin.className);
		//    Objects.inst.StartCoroutine(Objects.inst.updateObjects());
		//}

		//[HarmonyPatch("updateObjectsForAll", new Type[] { typeof(string) })]
		//[HarmonyPrefix]
		//static void updateObjectsPrefix2(ObjectManager __instance)
		//{
		//    Debug.LogFormat("{0}Updating Objects", FunctionsPlugin.className);
		//    Objects.inst.StartCoroutine(Objects.inst.updateObjects());
		//}

		//[HarmonyPatch("updateObjects", new Type[] { typeof(ObjEditor.ObjectSelection), typeof(bool) })]
		//[HarmonyPrefix]
		//static void updateObjectsPrefix3(ObjectManager __instance, ObjEditor.ObjectSelection __0, bool __1)
		//{
		//    if (__0.IsObject())
		//    {
		//        Debug.LogFormat("{0}Updating Objects", FunctionsPlugin.className);
		//        Objects.inst.StartCoroutine(Objects.inst.updateObjects(__0));
		//    }
		//}

		//[HarmonyPatch("updateObjects", new Type[] { })]
		//[HarmonyPrefix]
		//static void updateObjectsPrefix4(ObjectManager __instance)
		//{
		//    Debug.LogFormat("{0}Updating Objects", FunctionsPlugin.className);
		//    Objects.inst.StartCoroutine(Objects.inst.updateObjects());
		//}
	}
}
