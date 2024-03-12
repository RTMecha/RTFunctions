using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers.Networking;
using UnityEngine;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(SoundLibrary))]
    public class SoundLibraryPatch : MonoBehaviour
    {
        public static SoundLibrary Instance => AudioManager.inst.library;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void AwakePostfix(SoundLibrary __instance)
        {
            __instance.StartCoroutine(AlephNetworkManager.DownloadAudioClip($"file://{RTFile.ApplicationDirectory}{FunctionsPlugin.BepInExAssetsPath}click cut.ogg", AudioType.OGGVORBIS, delegate (AudioClip audioClip)
            {
                __instance.soundClips["UpDown"][0] = audioClip;
                __instance.soundClips["LeftRight"][0] = audioClip;
            }));
            __instance.StartCoroutine(AlephNetworkManager.DownloadAudioClip($"file://{RTFile.ApplicationDirectory}{FunctionsPlugin.BepInExAssetsPath}optionexit.ogg", AudioType.OGGVORBIS, delegate (AudioClip audioClip)
            {
                __instance.soundClips["Block"][0] = audioClip;
            }));
        }
    }
}
