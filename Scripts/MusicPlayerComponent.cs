using System;
using System.Collections.Generic;
using UnityEngine;

namespace YeKostenko.AudioEngine
{
    public class MusicPlayerComponent : MonoBehaviour
    {
        [SerializeField]
        private List<MusicTrack> _musicTracks;

        [Header("Music Player Settings")]
        [SerializeField]
        private bool _playOnStart = false;
        [SerializeField]
        private bool _crossfadeOnChange = true;
        [SerializeField]
        [Range(0, 1f)]
        private float _volumeMultiplier = 1f;
        [SerializeField]
        private float _fadeInDuration = 1f;
        [SerializeField]
        private bool _loop;

        [Header("General Settings")]
        [SerializeField]
        private bool _dontDestroyOnLoad;

        private MusicHandle _currentMusicHandle;
        private int _currentTrackIndex = -1;
        private float _currentTrackTime = 0f;
        private float _currentTrackDuration = 0f;
        private bool _isPlaying;

        [ContextMenu("Play")]
        public void Play()
        {
            if (_musicTracks.Count == 0)
            {
                Debug.LogWarning("No music tracks assigned to MusicPlayerComponent.", this);
                return;
            }

            _currentTrackIndex = 0;
            PlayTrack(_currentTrackIndex);
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            if (_currentMusicHandle.IsValid)
            {
                _currentMusicHandle.Stop(_fadeInDuration);
            }

            _isPlaying = false;
            _currentTrackIndex = -1;
            _currentTrackTime = 0f;
            _currentTrackDuration = 0f;
            _currentMusicHandle = MusicHandle.Invalid;
        }

        [ContextMenu("Pause")]
        public void Pause()
        {
            if (_currentMusicHandle.IsValid)
            {
                _currentMusicHandle.SetPaused(true);
            }

            _isPlaying = false;
        }

        [ContextMenu("Unpause")]
        public void Unpause()
        {
            if (_currentMusicHandle.IsValid)
            {
                _currentMusicHandle.SetPaused(false);
            }

            _isPlaying = true;
        }

        [ContextMenu("Next Track")]
        public void NextTrack()
        {
            if (_musicTracks.Count == 0)
            {
                Debug.LogWarning("No music tracks assigned to MusicPlayerComponent.", this);
                return;
            }

            _currentTrackIndex = (_currentTrackIndex + 1) % _musicTracks.Count;

            if (_currentTrackIndex == 0 && !_loop)
            {
                Stop();
                return;
            }

            PlayTrack(_currentTrackIndex);
        }

        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            if (_playOnStart)
            {
                Play();
            }
        }

        private void PlayTrack(int index)
        {
            MusicTrack trackToPlay = _musicTracks[index];
            _currentTrackDuration = trackToPlay.Main.length;
            _currentTrackTime = 0;
            _isPlaying = true;
            _currentMusicHandle = Sound.Music(trackToPlay, new MusicParams
            {
                VolumeMul = _volumeMultiplier,
                FadeIn = _fadeInDuration,
                AllowCrossfade = _crossfadeOnChange
            });
        }

        private void Update()
        {
            if (_isPlaying && _currentMusicHandle.IsValid)
            {
                _currentTrackTime += Time.deltaTime;
                if (_currentTrackTime >= _currentTrackDuration - _fadeInDuration)
                {
                    NextTrack();
                }
            }
        }
    }
}