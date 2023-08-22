using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YellowTape.AudioEngine
{
    public interface ISfxEntity
    {
        float Volume { get; }
        bool IsSoundOn { get; }
        void UpdateVolume(float newValue);
        void UpdateSoundOn(bool isOn);
    }

    public class SfxEntity : ISfxEntity
    {
        public float Volume => _config.Volume;
        public bool IsSoundOn => _config.IsSoundOn;

        private readonly SfxParameters _config;
        private readonly AudioSource _source;
        private readonly Dictionary<string, AudioClip> _allClips;
        private readonly GlobalAudioEngineSceneEntity _sceneEntity;

        private float _dampingDuration = 0.5f;

        public SfxEntity(SfxParameters parameters, AudioSource source, GlobalAudioEngineSceneEntity sceneEntity)
        {
            _source = source;
            _config = parameters;
            _sceneEntity = sceneEntity;
            SetSourceWithConfig();
            _allClips = new Dictionary<string, AudioClip>();
            foreach (var clipData in parameters.AllClips)
            {
                _allClips.Add(clipData.name, clipData.Clip);
            }
        }

        public void PlayClip(string name, bool loop, bool swelling)
        {
            var clip = GetClip(name);
            if (clip == null) return;

            _source.loop = loop;
            if (loop)
            {
                _source.clip = clip;
                _source.Play();
            }
            else
            {
                _source.PlayOneShot(clip);
            }

            if (swelling)
            {
                _sceneEntity.StartCustomCoroutine(SwellingCoroutine());
            }
        }

        public void StopPlaying(bool damping)
        {
            if (damping)
            {
                _sceneEntity.StartCustomCoroutine(DampingCoroutine());
            }
            else
            {
                _source.clip = null;
                _source.Stop();
            }
        }

        public void UpdateVolume(float newValue)
        {
            _config.Volume = newValue;
            SetSourceWithConfig();
        }

        public void UpdateSoundOn(bool isOn)
        {
            _config.IsSoundOn = isOn;
            SetSourceWithConfig();
        }
        
        public AudioClip GetClipByName(string name)
        {
            return GetClip(name);
        }

        private void SetSourceWithConfig()
        {
            _source.volume = _config.Volume;
            _source.mute = !_config.IsSoundOn;
        }

        private AudioClip GetClip(string name)
        {
            if (_allClips.TryGetValue(name, out var clip))
                return clip;
            else
                NoClipError(name);
            return null;
        }

        private void NoClipError(string name)
        {
            Debug.LogError($"Cannot find audio clip with name {name}");
        }

        private IEnumerator DampingCoroutine()
        {
            float counter = _dampingDuration;
            while (counter > 0)
            {
                _source.volume = Mathf.Lerp(Volume, 0, Mathf.Lerp(1, 0, counter / _dampingDuration));
                counter -= Time.deltaTime;
                yield return null;
            }

            _source.clip = null;
            _source.volume = Volume;
            _source.Stop();
        }
        
        private IEnumerator SwellingCoroutine()
        {
            float counter = _dampingDuration;
            while (counter > 0)
            {
                _source.volume = Mathf.Lerp(0, Volume, Mathf.Lerp(1, 0, counter / _dampingDuration));
                counter -= Time.deltaTime;
                yield return null;
            }

            _source.volume = Volume;
        }
    }
}