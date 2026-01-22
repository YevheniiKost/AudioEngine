using UnityEngine;
using UnityEngine.Audio;

namespace YeKostenko.AudioEngine
{
    [CreateAssetMenu(menuName = "YeKostenko/AudioEngine/Sound Settings")]
    public sealed class SoundSettings : ScriptableObject
    {
        [Header("Manager")]
        [Min(1)]
        [SerializeField]
        private int _prewarmSources = 48;

        [Header("Default Routes")]
        [SerializeField]
        private AudioMixerGroup _masterGroup;
        [SerializeField]
        private AudioMixerGroup _defaultSfxGroup;
        [SerializeField]
        private AudioMixerGroup _defaultUiGroup;
        [SerializeField]
        private AudioMixerGroup _defaultAmbienceGroup;
        [SerializeField]
        private AudioMixerGroup _defaultVoiceGroup;
        [SerializeField]
        private AudioMixerGroup _defaultMusicGroup;

        [Header("Music")]
        [Min(0f)]
        [SerializeField]
        private float _defaultMusicFade = 0.35f;
        [Min(0f)]
        [SerializeField]
        private float _defaultMusicCrossfade = 1f;

        public int PrewarmSources => _prewarmSources;
        public float DefaultMusicFade => _defaultMusicFade;
        public float DefaultMusicCrossfade => _defaultMusicCrossfade;

        public AudioMixerGroup GetDefaultGroupForBus(SoundBus bus)
        {
            return bus switch
            {
                SoundBus.Master => _masterGroup,
                SoundBus.SFX => _defaultSfxGroup,
                SoundBus.UI => _defaultUiGroup,
                SoundBus.Ambience => _defaultAmbienceGroup,
                SoundBus.Voice => _defaultVoiceGroup,
                SoundBus.Music => _defaultMusicGroup,
                _ => _defaultSfxGroup
            };
        }
    }
}