using System;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;
using LSFunctions;

using RTFunctions.Functions;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;
using TMPro;

namespace RTFunctions.Patchers
{
	public delegate void LevelTickEventHandler();

	[HarmonyPatch(typeof(ObjectManager))]
    public class ObjectManagerPatch : MonoBehaviour
    {
		public static bool debugOpacity = false;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void AwakePatch(ObjectManager __instance)
		{
			ShapeManager.inst.Load();

			// This is here for debug purposes.
			if (debugOpacity)
				for (int i = 0; i < __instance.objectPrefabs.Count; i++)
				{
					foreach (var option in __instance.objectPrefabs[i].options)
						if (option.transform.childCount > 0 && option.transform.GetChild(0).gameObject.TryGetComponent(out Renderer renderer))
							renderer.material.color = new Color(1f, 1f, 1f, 1f);
				}

			// Fixes Text being red.
			__instance.objectPrefabs[4].options[0].GetComponentInChildren<TextMeshPro>().color = new Color(0f, 0f, 0f, 0f);

			// Fixes Hexagons being solid.
			foreach (var option in __instance.objectPrefabs[5].options)
            {
                option.GetComponentInChildren<Collider2D>().isTrigger = true;
            }
		}

		[HarmonyPatch("AddPrefabToLevel")]
		[HarmonyPrefix]
		static bool AddPrefabToLevelPrefix(DataManager.GameData.PrefabObject __0)
		{
			Updater.AddPrefabToLevel(__0);

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
