using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using UnityEngine;

using RTFunctions.Functions.IO;

namespace RTFunctions.Patchers
{
	[HarmonyPatch(typeof(Debug))]
    public class DebugPatcher : MonoBehaviour
    {
		[HarmonyPatch("Log", new Type[] { typeof(object) })]
		[HarmonyPostfix]
		static void LogPostfix(object __0) => RTLogger.AddLog(__0.ToString());

		[HarmonyPatch("LogFormat", new Type[] { typeof(string), typeof(object[]) })]
		[HarmonyPostfix]
		static void LogFormatpostfix(string __0, params object[] __1) => RTLogger.AddLog(string.Format(__0, __1));

		[HarmonyPatch("LogError", new Type[] { typeof(object) })]
		[HarmonyPostfix]
		static void LogErrorPostfix(object __0) => RTLogger.AddLog("<color=#FF0000>" + __0.ToString());

		[HarmonyPatch("LogErrorFormat", new Type[] { typeof(string), typeof(object[]) })]
		[HarmonyPostfix]
		static void LogErrorFormatPostfix(string __0, params object[] __1) => RTLogger.AddLog(string.Format(__0, __1));

		[HarmonyPatch("LogWarning", new Type[] { typeof(object) })]
		[HarmonyPostfix]
		static void LogWarningPostfix(object __0) => RTLogger.AddLog("<color=#FFFF00>" + __0.ToString());

		[HarmonyPatch("LogWarningFormat", new Type[] { typeof(string), typeof(object[]) })]
		[HarmonyPostfix]
		static void LogWarningFormatPostfix(string __0, params object[] __1) => RTLogger.AddLog("<color=#FFFF00>" + string.Format(__0, __1));
	}
}
