using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YellowTape.AudioEngine
{
    public interface IAudioEngine
    {
        /// <summary>
        /// Get sound entity of selected type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ISfxEntity GetSfxEntity(SoundType type);

        /// <summary>
        /// Play clip of selected type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="looping">False if play one time</param>
        /// <param name="swelling"></param>
        void PlayClip(SoundType type, string name, bool looping = false, bool swelling = false);
        void StopAllSoundsOfType(SoundType type, bool damping = false);
        void StopAllSounds();
        /// <summary>
        /// Get clip length in seconds
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        float GetClipLength(SoundType soundType, string name);
    }

    public class GlobalAudioEngine : IAudioEngine
    {
        private readonly Dictionary<SoundType, SfxEntity> _allSoundEntities;

        public GlobalAudioEngine(Dictionary<SoundType, SfxEntity> sfxEntities)
        {
            _allSoundEntities = sfxEntities ?? throw new NullReferenceException(nameof(sfxEntities));
        }

        public ISfxEntity GetSfxEntity(SoundType type)
        {
            return GetEntity(type);
        }

        public void PlayClip(SoundType type, string name, bool looping = false, bool swelling = false)
        {
            var entity = GetEntity(type);
            entity?.PlayClip(name, looping, swelling);
        }

        public void StopAllSoundsOfType(SoundType type, bool damping = false)
        {
            var entity = GetEntity(type);
            entity?.StopPlaying(damping);
        }

        public void StopAllSounds()
        {
            foreach (var sfxEntity in _allSoundEntities)
            {
                sfxEntity.Value.StopPlaying(false);
            }
        }

        public float GetClipLength(SoundType soundType, string name)
        {
            var entity = GetEntity(soundType);
            AudioClip clip = entity.GetClipByName(name);
            if (clip != null)
                return clip.length;
            return 0;
        }

        private SfxEntity GetEntity(SoundType type)
        {
            if (_allSoundEntities.TryGetValue(type, out var entity))
            {
                return entity;
            }
            else
            {
                NoEntityError(type);
                return null;
            }
        }

        private static void NoEntityError(SoundType type)
        {
            Debug.LogError($"Cannot find Sfx entity of type {type}");
        }
    }

    public enum SoundType
    {
        Background,
        UISound,
        GameplaySound
    }

    [System.Serializable]
    public class SfxParameters
    {
        public SoundType SoundType;
        public float Volume;
        public bool IsSoundOn;
        public List<ClipData> AllClips;
    }

    [Serializable]
    public class ClipData
    {
        public string name;
        public AudioClip Clip;
    }

}