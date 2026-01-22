using UnityEngine;

namespace YeKostenko.AudioEngine
{
    [CreateAssetMenu(fileName = "AudioEvent", menuName = "YeKostenko/AudioEngine/Audio Event", order = 1)]
    public class AudioEvent : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Audio clips to be played randomly by this event.")]
        private AudioClip[] _clips;

        [SerializeField]
        private SoundBus _soundBus;
        [SerializeField]
        private MixerRoute _mixerRoute;
        [SerializeField]
        private float _cooldown = 1f;

        [Range(0f, 1f)]
        [SerializeField]
        private float _volumeMin = 1f;
        [Range(0f, 1f)]
        [SerializeField]
        private float _volumeMax = 1f;
        [SerializeField]
        private float _pitchMax = 1f;
        [SerializeField]
        private float _pitchMin = 1f;

        [Header("Playback")]
        [SerializeField]
        private bool _loop;
        [SerializeField]
        private int _maxVoices;
        [SerializeField]
        private FadeSettings _fadeSettings;

        [SerializeField]
        private SpatialSettings _spatialSettings;

        [SerializeField]
        private PrioritySettings _prioritySettings;

        [SerializeField]
        private string _debugName;

        public AudioClip[] Clips => _clips;
        public float VolumeMin => _volumeMin;
        public float VolumeMax => _volumeMax;
        public float PitchMin => _pitchMin;
        public float PitchMax => _pitchMax;
        public SoundBus Bus => _soundBus;
        public MixerRoute Route => _mixerRoute;
        public float Cooldown => _cooldown;
        public bool Loop => _loop;
        public int MaxVoices => _maxVoices;
        public FadeSettings Fade => _fadeSettings;
        public SpatialSettings Spatial => _spatialSettings;
        public PrioritySettings Priority => _prioritySettings;
        public string DebugName => _debugName;
    }
}