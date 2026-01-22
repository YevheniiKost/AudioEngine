using UnityEngine;

namespace YeKostenko.AudioEngine
{
    [System.Serializable]
    public class FadeSettings
    {
        [SerializeField]
        private FadeMode _mode;

        [Min(0f)]
        [SerializeField]
        private float _in;

        [Min(0f)]
        [SerializeField]
        private float _out;

        public float In => _in;
        public float Out => _out;
        public bool HasFadeIn => _mode == FadeMode.In || _mode == FadeMode.InOut;
        public bool HasFadeOut => _mode == FadeMode.Out || _mode == FadeMode.InOut;
    }
}