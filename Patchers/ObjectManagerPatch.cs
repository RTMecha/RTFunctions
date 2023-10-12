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
using RTFunctions.Functions.Optimization;
using RTFunctions.Functions.Optimization.Level;
using RTFunctions.Functions.Optimization.Objects;

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
			if (!Objects.prefabObjects.ContainsKey(__0.ID))
				Objects.prefabObjects.Add(__0.ID, new Objects.PrefabObject(__0));

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
				var dictionary = new Dictionary<string, string>();

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


		public static event LevelTickEventHandler LevelTick;

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		public static bool UpdatePrefix()
		{
			LevelTick?.Invoke();
			return false;
		}

		[HarmonyPatch("updateObjects", new System.Type[] { typeof(string) })]
		[HarmonyPrefix]
		private static bool updateObjectsPrefix1(ObjectManager __instance)
		{
			AddPrefabObjects(__instance);
			Updater.updateObjects();
			return false;
		}

		[HarmonyPatch("updateObjectsForAll", new System.Type[] { typeof(string) })]
		[HarmonyPrefix]
		private static bool updateObjectsPrefix2(ObjectManager __instance)
		{
			AddPrefabObjects(__instance);
			Updater.updateObjects();
			return false;
		}

		[HarmonyPatch("updateObjects", new System.Type[] { typeof(ObjEditor.ObjectSelection), typeof(bool) })]
		[HarmonyPrefix]
		private static bool updateObjectsPrefix3(ObjectManager __instance, ObjEditor.ObjectSelection __0, bool __1)
		{
			//CatalystBase.updateObjects();
			if (__0.IsObject())
			{
				Updater.updateProcessor(__0);
			}
			if (__1)
			{
				__instance.updateObjectsForAll(__0.GetPrefabData().ID);
			}
			if (__0.IsPrefab())
			{
				__instance.updateObjects();
			}
			return false;
		}

		[HarmonyPatch("updateObjects", new System.Type[] { })]
		[HarmonyPrefix]
		private static bool updateObjectsPrefix4(ObjectManager __instance)
		{
			AddPrefabObjects(__instance);
			Updater.updateObjects();
			return false;
		}

		public static void AddPrefabObjects(ObjectManager __instance)
		{
			
			DataManager.inst.gameData.beatmapObjects.RemoveAll(x => x.fromPrefab);
			for (int i = 0; i < DataManager.inst.gameData.prefabObjects.Count; i++)
			{
				__instance.AddPrefabToLevel(DataManager.inst.gameData.prefabObjects[i]);
			}
			for (int j = 0; j < DataManager.inst.gameData.beatmapObjects.Count; j++)
			{
				if (!DataManager.inst.gameData.beatmapObjects[j].fromPrefab)
				{
					DataManager.inst.gameData.beatmapObjects[j].active = false;
					for (int k = 0; k < DataManager.inst.gameData.beatmapObjects[j].events.Count; k++)
					{
						for (int l = 0; l < DataManager.inst.gameData.beatmapObjects[j].events[k].Count; l++)
						{
							DataManager.inst.gameData.beatmapObjects[j].events[k][l].active = false;
						}
					}
				}
			}
		}
	}
}
