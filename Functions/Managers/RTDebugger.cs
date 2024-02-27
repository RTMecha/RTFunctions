using System;
using System.Linq;

using RTFunctions.Functions.Data;

namespace RTFunctions.Functions.Managers
{
    public static class RTDebugger
    {
        public static void TogglePlay() => (AudioManager.inst.CurrentAudioSource.isPlaying ? (Action)AudioManager.inst.CurrentAudioSource.Pause : AudioManager.inst.CurrentAudioSource.Play).Invoke();

        public static int BeatmapObjectAliveCount() => GameData.Current.BeatmapObjects.Where(x => x.objectType != BeatmapObject.ObjectType.Empty && x.TimeWithinLifespan()).Count();
    }
}
