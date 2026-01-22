using UnityEngine;

namespace YeKostenko.AudioEngine
{
    [CreateAssetMenu(menuName = "YeKostenko/AudioEngine/Music Track")]
    public sealed class MusicTrack : ScriptableObject
    {
        [SerializeField]
        private AudioClip _main;

        [SerializeField]
        private MixerRoute _route;

        [Range(0f, 1f)]
        [SerializeField]
        private float _baseVolume = 1f;

        [SerializeField]
        private bool _loop;
        [SerializeField]
        private FadeSettings _fade;

        [SerializeField]
        private PrioritySettings _priority;

        [SerializeField]
        private string _debugName;

        public AudioClip Main => _main;
        public MixerRoute Route => _route;
        public float BaseVolume => _baseVolume;
        public bool Loop => _loop;
        public FadeSettings Fade => _fade;
        public string DebugName => _debugName;
    }
}