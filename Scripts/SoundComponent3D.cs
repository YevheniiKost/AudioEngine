using UnityEngine;

namespace YeKostenko.AudioEngine
{
    public class SoundComponent3D : SoundComponent
    {
        [Header("3D Sound Settings")]
        [SerializeField]
        private bool _followTransform = false;

        public override PlayHandle Play(PlayParams parameters = null)
        {
            if (AudioEvent == null)
            {
                Debug.LogWarning("AudioEvent is not assigned in SoundComponent3D.", this);
                return PlayHandle.Invalid;
            }

            if (!AudioEvent.Spatial.Is3D)
            {
                Debug.LogWarning(
                    "AudioEvent assigned in SoundComponent3D is not 3D. Use SoundComponent2D for 2D sounds.", this);
                return PlayHandle.Invalid;
            }

            parameters ??= GetPlayParams;

            return _followTransform ?
                Sound.PlayFollow(AudioEvent, transform, parameters) :
                Sound.PlayAt(AudioEvent, transform.position, parameters);
        }
    }
}