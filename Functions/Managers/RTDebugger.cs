using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTFunctions.Functions.Managers
{
    public static class RTDebugger
    {
        public static void TogglePlay() => (AudioManager.inst.CurrentAudioSource.isPlaying ? (Action)AudioManager.inst.CurrentAudioSource.Pause : AudioManager.inst.CurrentAudioSource.Play).Invoke();
    }
}
