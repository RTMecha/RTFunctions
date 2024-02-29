using System;
using System.Linq;

using UnityEngine;
using LSFunctions;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Managers
{
    public static class RTDebugger
    {
        public static void TogglePlay() => (AudioManager.inst.CurrentAudioSource.isPlaying ? (Action)AudioManager.inst.CurrentAudioSource.Pause : AudioManager.inst.CurrentAudioSource.Play).Invoke();

        public static int BeatmapObjectAliveCount() => GameData.Current.BeatmapObjects.Where(x => x.objectType != BeatmapObject.ObjectType.Empty && x.TimeWithinLifespan()).Count();

        public static void LogColor(Color color) => Debug.Log($"[<color=#{RTHelpers.ColorToHex(color)}>▓▓▓▓▓▓▓▓▓▓▓▓▓</color>]");
        public static void LogColor(string color) => Debug.Log($"[<color={color}>▓▓▓▓▓▓▓▓▓▓▓▓▓</color>]");
    }
}
