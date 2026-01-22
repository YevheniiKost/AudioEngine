using System;

using UnityEngine;

namespace YeKostenko.AudioEngine
{
    internal sealed class AudioManager : MonoBehaviour, ISoundService, IAudioVoiceController, IMusicController
    {
        [SerializeField]
        private SoundSettings _settings;

        private AudioBusManager _busManager;
        private AudioEventTracker _eventTracker;
        private MusicPlayer _musicPlayer;
        private AudioVoicePool _voicePool;

        private bool _isInitialized;

        public SoundSettings Settings => _settings;

        public void Initialize(SoundSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings),
                    "AudioManager requires valid SoundSettings to initialize. \n" +
                    "Please put SoundSettings ScriptableObject file into Resources Folder");
            }

            _settings = settings;
            Bootstrap();

            _isInitialized = true;
        }

        private void Bootstrap()
        {
            _busManager = new AudioBusManager(_settings);
            _eventTracker = new AudioEventTracker();
            _voicePool = new AudioVoicePool(transform, _settings, _busManager, this);
            _musicPlayer = new MusicPlayer(transform, _settings, _busManager, this);

            SoundLocator.Bind(this, _settings);
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            _voicePool.Update();
            _musicPlayer.Update(Time.time, AudioSettings.dspTime);
        }

        public PlayHandle Play(AudioEvent audioEvent, PlayParams parameters = null)
        {
            if (audioEvent == null)
            {
                throw new ArgumentNullException(nameof(audioEvent),
                    "AudioEvent parameter cannot be null when calling AudioManager.Play");
            }

            return _voicePool.Play(audioEvent, Vector3.zero, null, parameters, _eventTracker);
        }

        public PlayHandle PlayAt(AudioEvent audioEvent, Vector3 position, PlayParams parameters = null)
        {
            if (audioEvent == null)
            {
                throw new ArgumentNullException(nameof(audioEvent),
                    "AudioEvent parameter cannot be null when calling AudioManager.Play");
            }

            return _voicePool.Play(audioEvent, position, null, parameters, _eventTracker);
        }

        public PlayHandle PlayFollow(AudioEvent audioEvent, Transform follow, PlayParams parameters = null)
        {
            if (audioEvent == null)
            {
                throw new ArgumentNullException(nameof(audioEvent),
                    "AudioEvent parameter cannot be null when calling AudioManager.Play");
            }

            if (follow == null)
            {
                throw new ArgumentNullException(nameof(follow),
                    "Follow transform parameter cannot be null when calling AudioManager.PlayFollow");
            }

            return _voicePool.Play(audioEvent, Vector3.zero, follow, parameters, _eventTracker);
        }

        public void StopEvent(AudioEvent audioEvent, float fadeOut = 0f) => _voicePool.StopEvent(audioEvent, fadeOut);

        public void StopBus(SoundBus bus, float fadeOut = 0f)
        {
            _voicePool.StopBus(bus, fadeOut);

            if (bus == SoundBus.Music || bus == SoundBus.Master)
            {
                float musicFadeOut = fadeOut <= 0f ? _settings != null ? _settings.DefaultMusicFade : 0.35f : fadeOut;
                _musicPlayer.Stop(musicFadeOut);
            }
        }

        public void StopAll(float fadeOut = 0f)
        {
            _voicePool.StopAll(fadeOut);
            float musicFadeOut = fadeOut <= 0f ? _settings != null ? _settings.DefaultMusicFade : 0.35f : fadeOut;
            _musicPlayer.Stop(musicFadeOut);
        }

        public float GetBusVolume(SoundBus bus) => _busManager.GetVolume(bus);

        public void SetBusVolume(SoundBus bus, float volume01)
        {
            _busManager.SetVolume(bus, volume01);
            _voicePool.OnBusVolumeChanged(bus);
        }

        public bool IsBusPaused(SoundBus bus) => _busManager.IsPaused(bus);

        public void SetBusPaused(SoundBus bus, bool paused)
        {
            _busManager.SetPaused(bus, paused);
            _voicePool.OnBusPauseChanged(bus);

            if (bus == SoundBus.Music)
            {
                _musicPlayer.SetPaused(paused);
            }
        }

        public bool IsPaused { get; private set; }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            _busManager.SetAllPaused(paused);
            _voicePool.OnBusPauseChanged(SoundBus.SFX);
            _musicPlayer.SetPaused(paused);
        }

        public MusicHandle PlayMusic(MusicTrack track, MusicParams parameters = null)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track),
                    "MusicTrack parameter cannot be null when calling AudioManager.PlayMusic");
            }

            parameters ??= MusicParams.Default;

            if (parameters.AllowCrossfade)
            {
                return _musicPlayer.CrossfadeTo(track, parameters.FadeIn);
            }

            return _musicPlayer.Play(track, parameters);
        }

        public void StopMusic(float fadeOut = 0.3f) => _musicPlayer.Stop(fadeOut);

        public MusicHandle CurrentMusic => _musicPlayer.CurrentHandle;



        bool IAudioVoiceController.IsValid(int id, int version) => _voicePool.IsHandleValid(id, version);

        void IAudioVoiceController.Stop(int id, float fadeOut) => _voicePool.StopVoice(id, fadeOut);

        void IAudioVoiceController.SetVolume(int id, float volume01) => _voicePool.SetVoiceVolume(id, volume01);

        void IAudioVoiceController.SetPitch(int id, float pitch) => _voicePool.SetVoicePitch(id, pitch);

        void IAudioVoiceController.SetPaused(int id, bool paused) => _voicePool.SetVoicePaused(id, paused);

        void IAudioVoiceController.SetFollow(int id, Transform follow) => _voicePool.SetVoiceFollow(id, follow);

#if UNITY_EDITOR
        AudioSource IAudioVoiceController.GetSource(int id) => _voicePool.GetVoiceSource(id);
#endif

        bool IMusicController.IsValid(int id, int version) => _musicPlayer.IsHandleValid(id, version);

        void IMusicController.Stop(float fadeOut) => _musicPlayer.Stop(fadeOut);

        void IMusicController.SetVolume(float volume01) => _musicPlayer.SetVolume(volume01);

        void IMusicController.Pause(bool paused) => _musicPlayer.SetPaused(paused);

        MusicHandle IMusicController.CrossfadeTo(MusicTrack next, float duration) =>
            _musicPlayer.CrossfadeTo(next, duration);

        MusicTrack IMusicController.GetTrack(int id) => _musicPlayer.CurrentTrack;
    }
}