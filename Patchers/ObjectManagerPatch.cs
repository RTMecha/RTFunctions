using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using UnityEngine;

using RTFunctions.Functions;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(ObjectManager))]
    public class ObjectManagerPatch : MonoBehaviour
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void AwakePatch(ObjectManager __instance)
        {
            foreach (var option in __instance.objectPrefabs[5].options)
            {
                option.GetComponentInChildren<Collider2D>().isTrigger = true;
            }
		}
	}
}
