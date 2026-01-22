using UnityEngine;
using UnityEngine.Audio;

namespace YeKostenko.AudioEngine
{
    [System.Serializable]
    public class MixerRoute
    {
        [SerializeField]
        private AudioMixerGroup _output;
        [SerializeField]
        private AudioMixerSnapshot _snapshot;

        public AudioMixerGroup Output => _output;
        public AudioMixerSnapshot Snapshot => _snapshot;
    }
}