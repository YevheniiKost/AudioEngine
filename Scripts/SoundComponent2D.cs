using UnityEngine;

namespace YeKostenko.AudioEngine
{
    public class SoundComponent2D : SoundComponent
    {
        public override PlayHandle Play(PlayParams parameters = null)
        {
            if(AudioEvent == null)
            {
                Debug.LogWarning("AudioEvent is not assigned in SoundComponent.", this);
                return PlayHandle.Invalid;
            }

            if (AudioEvent.Spatial.Is3D)
            {
                Debug.LogWarning("AudioEvent assigned in SoundComponent is 3D. Use SoundComponent3D for 3D sounds.", this);
                return PlayHandle.Invalid;
            }

            parameters ??= GetPlayParams;

            return Sound.Play(AudioEvent, parameters);
        }
    }
}