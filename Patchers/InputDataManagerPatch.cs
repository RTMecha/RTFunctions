using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using RTFunctions.Functions.Data.Player;
using InControl;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(InputDataManager))]
    public class InputDataManagerPatch : MonoBehaviour
	{
		[HarmonyPatch("AlivePlayers", MethodType.Getter)]
		[HarmonyPrefix]
		static bool GetAlivePlayers(InputDataManager __instance, ref List<InputDataManager.CustomPlayer> __result)
		{
			__result = __instance.players.FindAll(x => x is CustomPlayer && (x as CustomPlayer).Player && (x as CustomPlayer).Player.PlayerAlive);
			return false;
		}

		[HarmonyPatch("SetAllControllerRumble", new[] { typeof(float), typeof(float), typeof(bool) })]
		[HarmonyPrefix]
		static bool SetAllControllerRumble(InputDataManager __instance, float __0, float __1, bool __2 = true)
		{
			if (DataManager.inst.GetSettingBool("ControllerVibrate", true))
			{
				foreach (var customPlayer in __instance.players)
				{
					customPlayer.device?.Vibrate(Mathf.Clamp(__0, 0f, 0.5f), Mathf.Clamp(__1, 0f, 0.5f));
				}
			}
			return false;
		}

		[HarmonyPatch("SetControllerRumble", new[] { typeof(int), typeof(float), typeof(float), typeof(bool) })]
		[HarmonyPrefix]
		static bool SetControllerRumble(InputDataManager __instance, int __0, float __1, float __2, bool __3 = true)
		{
			foreach (var customPlayer in __instance.players)
			{
				if (customPlayer is CustomPlayer && (customPlayer as CustomPlayer).Player && (customPlayer as CustomPlayer).Player.playerIndex == __0)
					customPlayer.device?.Vibrate(Mathf.Clamp(__1, 0f, 0.5f), Mathf.Clamp(__2, 0f, 0.5f));
			}
			return false;
		}

		[HarmonyPatch("RemovePlayer")]
		[HarmonyPrefix]
		static bool RemovePlayer(InputDataManager __instance, InputDataManager.CustomPlayer __0)
		{
			int index = __0.index;
			if (__0 is CustomPlayer && (__0 as CustomPlayer).Player)
			{
				__instance.StopControllerRumble(index);
				(__0 as CustomPlayer).Player.Actions = null;
				(__0 as CustomPlayer).Player.faceController = null;
				if ((__0 as CustomPlayer).Player.gameObject != null)
				{
					Destroy((__0 as CustomPlayer).Player.gameObject);
				}
			}

			__instance.StopAllControllerRumble();
			__instance.players.RemoveAt(index);
			return false;
		}

        //[HarmonyPatch("Update")]
        //[HarmonyTranspiler]
        //static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instruction)
        //{
        //	return new CodeMatcher(instruction)
        //		.Start()
        //		.Advance(6)
        //		.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_8))
        //		.InstructionEnumeration();
        //}

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static bool UpdatePrefix(InputDataManager __instance)
		{
			if (__instance.playersCanJoin && __instance.players.Count < 8)
			{
				if (__instance.JoinButtonWasPressedOnListener(__instance.joystickListener))
				{
					InputDevice activeDevice = InputManager.ActiveDevice;
					if (__instance.ThereIsNoPlayerUsingJoystick(activeDevice))
						__instance.players.Add(new CustomPlayer(true, __instance.players.Count, activeDevice));
				}
				if (__instance.JoinButtonWasPressedOnListener(__instance.keyboardListener) && __instance.ThereIsNoPlayerUsingKeyboard())
					__instance.players.Add(new CustomPlayer(true, __instance.players.Count, null));
			}
			return false;
		}
	}
}
