using UnityEngine;

namespace YeKostenko.AudioEngine
{
    [System.Serializable]
    public class SpatialSettings
    {
        [SerializeField]
        private SpatialMode _mode;

        [Header("3D Settings")]
        [Range(0f, 1f)]
        [Tooltip("0 = 2D sound, 1 = fully 3D sound")]
        [SerializeField]
        private float _spatialBlend;

        [Min(0f)]
        [SerializeField]
        private float _minDistance = 0.5f;

        [Min(0f)]
        [SerializeField]
        private float _maxDistance = 20f;

        [SerializeField]
        private AudioRolloffMode _rolloff;

        [SerializeField]
        private float _dopplerLevel;
        [SerializeField]
        private float _spread;

        public float SpatialBlend => _spatialBlend;
        public float MinDistance => _minDistance;
        public float MaxDistance => _maxDistance;
        public AudioRolloffMode Rolloff => _rolloff;
        public float DopplerLevel => _dopplerLevel;
        public float Spread => _spread;
        public bool Is3D => _mode == SpatialMode.ThreeD;
    }
}