using UnityEngine;

namespace YeKostenko.AudioEngine
{
    public abstract class SoundComponent : MonoBehaviour
    {
        [Header("Audio Event")]
        [SerializeField]
        private AudioEvent _audioEvent;

        [Header("Play Settings")]
        [SerializeField]
        private bool _playOnStart = false;

        [Header("Optional Settings")]
        [SerializeField]
        [Range(0, 1f)]
        private float _volumeMultiplier = 1;
        [SerializeField]
        [Min(0f)]
        private float _pitchMultiplier = 1;
        [SerializeField]
        [Min(0f)]
        private float _delay;
        [SerializeField]
        private int _priority = -1;

        public AudioEvent AudioEvent => _audioEvent;

        protected PlayParams GetPlayParams => new PlayParams
        {
            VolumeMul = _volumeMultiplier,
            PitchMul = _pitchMultiplier,
            Delay = _delay,
            Priority = _priority >= 0 ? _priority : null
        };

        [ContextMenu("Play One Shot")]
        public void PlayOneShot() => Play();

        public abstract PlayHandle Play(PlayParams parameters = null);
        
        private void Start()
        {
            if (_playOnStart)
            {
                Play();
            }
        }
    }
}