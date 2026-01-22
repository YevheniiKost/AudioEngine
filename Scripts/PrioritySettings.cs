using UnityEngine;

namespace YeKostenko.AudioEngine
{
    [System.Serializable]
    public class PrioritySettings
    {
        [Range(0, 256)]
        [SerializeField]
        private int _priority = 256;
        [SerializeField]
        private VoiceStealMode _steal = VoiceStealMode.None;
        [SerializeField]
        private bool _isCritical;
        [SerializeField]
        private float _minLifetime = 0.5f;

        public int Priority => _priority;
        public VoiceStealMode Steal => _steal;
        public bool IsCritical => _isCritical;
        public float MinLifetime => _minLifetime;
    }
}