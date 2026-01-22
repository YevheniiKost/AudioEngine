using System;

using UnityEngine;
using UnityEngine.Audio;

namespace YeKostenko.AudioEngine
{
    internal sealed class MusicPlayer
    {
        private readonly AudioBusManager _busManager;
        private readonly IMusicController _musicController;
        private readonly Transform _parentTransform;
        private readonly SoundSettings _settings;
        private float _crossfadeDuration;

        private float _crossfadeOutStartVolume;
        private AudioSource _crossfadeSource;

        private float _crossfadeStartTime;
        private MusicHandle _currentHandle;

        private AudioSource _currentSource;

        private int _currentVersion;
        private float _currentVolume;
        private float _fadeInDuration;

        private float _fadeInStartTime;
        private float _fadeOutDuration;

        private float _fadeOutStartTime;
        private float _fadeOutStartVolume;

        private int _handleIdCounter;
        private bool _isCrossfading;
        private bool _isFadingIn;
        private bool _isFadingOut;

        private float _targetVolume;

        public MusicPlayer(Transform parent, SoundSettings settings, AudioBusManager busManager,
            IMusicController musicController)
        {
            _parentTransform = parent;
            _settings = settings;
            _busManager = busManager;
            _musicController = musicController;
            _handleIdCounter = 0;
            _currentVersion = 1;

            InitializeSources();
        }

        public MusicHandle CurrentHandle => _currentHandle;
        public MusicTrack CurrentTrack { get; private set; }

        private void InitializeSources()
        {
            GameObject mainObject = new("__MusicSource_Main");
            mainObject.transform.SetParent(_parentTransform, false);
            _currentSource = mainObject.AddComponent<AudioSource>();
            _currentSource.playOnAwake = false;

            GameObject crossfadeObject = new("__MusicSource_Crossfade");
            crossfadeObject.transform.SetParent(_parentTransform, false);
            _crossfadeSource = crossfadeObject.AddComponent<AudioSource>();
            _crossfadeSource.playOnAwake = false;
        }

        public void Update(float time, double dspTime)
        {
            if (_isCrossfading)
            {
                UpdateCrossfade(time);
                return;
            }

            if (_isFadingOut)
            {
                UpdateFadeOut(time);
                return;
            }

            if (_isFadingIn)
            {
                UpdateFadeIn(time);
            }
        }

        public MusicHandle Play(MusicTrack track, in MusicParams parameters)
        {
            if (track == null || track.Main == null)
            {
                throw new ArgumentException(
                    "MusicTrack parameter must have a valid Main AudioClip when calling MusicPlayer.Play");
            }

            if (track == CurrentTrack && parameters is { ForceRestart: false })
            {
                return MusicHandle.Invalid;
            }

            Stop(-1f);

            CurrentTrack = track;

            _targetVolume = Mathf.Clamp01(track.BaseVolume * parameters.VolumeMul);
            _currentVolume = 0f;

            float fadeIn = ResolveFadeIn(track, parameters.FadeIn);

            _isCrossfading = false;
            _isFadingOut = false;

            _isFadingIn = fadeIn > 0f;
            _fadeInStartTime = Time.time;
            _fadeInDuration = Mathf.Max(0f, fadeIn);

            ConfigureSourceForTrack(_currentSource, track);

            _currentSource.volume = 0f;
            _currentSource.Play();

            if (!_isFadingIn)
            {
                float busVolume = _busManager.GetVolume(SoundBus.Music);
                _currentSource.volume = _targetVolume * busVolume;
                _currentVolume = _targetVolume;
            }

            _handleIdCounter++;
            _currentVersion++;
            _currentHandle = new MusicHandle(_handleIdCounter, _currentVersion, _musicController);

            return _currentHandle;
        }

        public void Stop(float fadeOut)
        {
            if (_currentSource == null)
            {
                CurrentTrack = null;
                _currentHandle = default;
                return;
            }

            _isCrossfading = false;
            _isFadingIn = false;

            if (!_currentSource.isPlaying)
            {
                CurrentTrack = null;
                _currentHandle = default;
                return;
            }

            float resolvedFadeOut = ResolveFadeOut(CurrentTrack, fadeOut);

            if (resolvedFadeOut <= 0f)
            {
                _currentSource.Stop();
                _currentSource.clip = null;

                if (_crossfadeSource != null && _crossfadeSource.isPlaying)
                {
                    _crossfadeSource.Stop();
                    _crossfadeSource.clip = null;
                }

                _isFadingOut = false;
                CurrentTrack = null;
                _currentHandle = default;
                _currentVolume = 0f;
                return;
            }

            _isFadingOut = true;
            _fadeOutStartTime = Time.time;
            _fadeOutDuration = resolvedFadeOut;
            _fadeOutStartVolume = _currentSource.volume;

            _handleIdCounter++;
            _currentVersion++;
            _currentHandle = new MusicHandle(_handleIdCounter, _currentVersion, _musicController);
        }

        public MusicHandle CrossfadeTo(MusicTrack next, float duration)
        {
            if (next == null || next.Main == null)
            {
                return default;
            }

            float resolvedDuration = ResolveCrossfadeDuration(CurrentTrack, next, duration);

            if (resolvedDuration <= 0f || CurrentTrack == null || !_currentSource.isPlaying)
            {
                return Play(next, MusicParams.Default);
            }

            _isFadingIn = false;
            _isFadingOut = false;

            // Swap sources: current becomes "incoming", other becomes "outgoing"
            (_crossfadeSource, _currentSource) = (_currentSource, _crossfadeSource);

            _crossfadeOutStartVolume = _crossfadeSource.volume;

            CurrentTrack = next;

            ConfigureSourceForTrack(_currentSource, next);
            _currentSource.volume = 0f;
            _currentSource.Play();

            _isCrossfading = true;
            _crossfadeStartTime = Time.time;
            _crossfadeDuration = resolvedDuration;

            _handleIdCounter++;
            _currentVersion++;
            _currentHandle = new MusicHandle(_handleIdCounter, _currentVersion, _musicController);

            return _currentHandle;
        }

        public void SetVolume(float volume01)
        {
            _targetVolume = Mathf.Clamp01(volume01);

            float busVolume = _busManager.GetVolume(SoundBus.Music);
            _currentSource.volume = _targetVolume * busVolume;
        }

        public void SetPaused(bool paused)
        {
            if (paused)
            {
                if (_currentSource.isPlaying)
                {
                    _currentSource.Pause();
                }

                if (_crossfadeSource.isPlaying)
                {
                    _crossfadeSource.Pause();
                }
            }
            else
            {
                if (_currentSource.clip != null)
                {
                    _currentSource.UnPause();
                }

                if (_crossfadeSource.clip != null)
                {
                    _crossfadeSource.UnPause();
                }
            }
        }

        public bool IsHandleValid(int id, int version) => _currentHandle.Id == id && _currentHandle.Version == version;

        private void UpdateFadeIn(float time)
        {
            if (_fadeInDuration <= 0f)
            {
                _isFadingIn = false;
                return;
            }

            float elapsed = time - _fadeInStartTime;
            float t = Mathf.Clamp01(elapsed / _fadeInDuration);

            float busVolume = _busManager.GetVolume(SoundBus.Music);
            float vol = t * _targetVolume * busVolume;

            _currentSource.volume = vol;
            _currentVolume = Mathf.Lerp(0f, _targetVolume, t);

            if (t >= 1f)
            {
                _isFadingIn = false;
                _currentVolume = _targetVolume;
            }
        }

        private void UpdateFadeOut(float time)
        {
            if (_fadeOutDuration <= 0f)
            {
                _isFadingOut = false;
                return;
            }

            float elapsed = time - _fadeOutStartTime;
            float t = Mathf.Clamp01(elapsed / _fadeOutDuration);

            float vol = Mathf.Lerp(_fadeOutStartVolume, 0f, t);
            _currentSource.volume = vol;

            if (t >= 1f)
            {
                _currentSource.Stop();
                _currentSource.clip = null;

                if (_crossfadeSource != null && _crossfadeSource.isPlaying)
                {
                    _crossfadeSource.Stop();
                    _crossfadeSource.clip = null;
                }

                _isFadingOut = false;
                CurrentTrack = null;
                _currentHandle = default;
                _currentVolume = 0f;
            }
        }

        private void UpdateCrossfade(float time)
        {
            float duration = Mathf.Max(0.0001f, _crossfadeDuration);
            float elapsed = time - _crossfadeStartTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            float busVolume = _busManager.GetVolume(SoundBus.Music);

            float inVol = progress * _targetVolume * busVolume;
            float outVol = (1f - progress) * _crossfadeOutStartVolume;

            _currentSource.volume = inVol;
            _crossfadeSource.volume = outVol;

            _currentVolume = Mathf.Lerp(0f, _targetVolume, progress);

            if (progress >= 1f)
            {
                _crossfadeSource.Stop();
                _crossfadeSource.clip = null;
                _isCrossfading = false;

                _currentVolume = _targetVolume;
                _currentSource.volume = _targetVolume * busVolume;
            }
        }

        private void ConfigureSourceForTrack(AudioSource source, MusicTrack track)
        {
            ApplyRoute(track);

            source.clip = track.Main;
            source.loop = track.Loop;
        }

        private void ApplyRoute(MusicTrack track)
        {
            AudioMixerGroup group = track != null && track.Route != null ? track.Route.Output : null;
            if (group == null && _settings != null)
            {
                group = _settings.GetDefaultGroupForBus(SoundBus.Master);
            }

            if (group != null)
            {
                _currentSource.outputAudioMixerGroup = group;
                _crossfadeSource.outputAudioMixerGroup = group;
            }

            if (track != null && track.Route != null && track.Route.Snapshot != null)
            {
                track.Route.Snapshot.TransitionTo(0f);
            }
        }

        private float ResolveFadeIn(MusicTrack track, float requestedFadeIn)
        {
            if (requestedFadeIn > 0f)
            {
                return requestedFadeIn;
            }

            if (track != null && track.Fade != null && track.Fade.HasFadeIn && track.Fade.In > 0f)
            {
                return track.Fade.In;
            }

            return _settings != null ? Mathf.Max(0f, _settings.DefaultMusicFade) : 0.35f;
        }

        private float ResolveFadeOut(MusicTrack track, float requestedFadeOut)
        {
            if (requestedFadeOut >= 0f)
            {
                return requestedFadeOut;
            }

            if (track != null && track.Fade != null && track.Fade.HasFadeOut && track.Fade.Out > 0f)
            {
                return track.Fade.Out;
            }

            return _settings != null ? Mathf.Max(0f, _settings.DefaultMusicFade) : 0.35f;
        }

        private float ResolveCrossfadeDuration(MusicTrack current, MusicTrack next, float requestedDuration)
        {
            if (requestedDuration >= 0f)
            {
                return requestedDuration;
            }

            if (next != null && next.Fade != null && next.Fade.HasFadeIn && next.Fade.In > 0f)
            {
                return next.Fade.In;
            }

            if (current != null && current.Fade != null && current.Fade.HasFadeOut && current.Fade.Out > 0f)
            {
                return current.Fade.Out;
            }

            return _settings != null ? Mathf.Max(0f, _settings.DefaultMusicFade) : 0.35f;
        }
    }
}