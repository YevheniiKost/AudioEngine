using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

namespace YeKostenko.AudioEngine
{
    internal sealed class AudioBusManager
    {
        private readonly Dictionary<SoundBus, BusState> _busStates;

        public AudioBusManager(SoundSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            int busCount = Enum.GetValues(typeof(SoundBus)).Length;
            _busStates = new Dictionary<SoundBus, BusState>(busCount);

            for (int i = 0; i < busCount; i++)
            {
                SoundBus bus = (SoundBus)i;
                _busStates[bus] = new BusState
                {
                    Volume = 1f,
                    Paused = false,
                    Group = settings.GetDefaultGroupForBus(bus)
                };
            }
        }

        public AudioMixerGroup GetGroup(SoundBus bus) => _busStates[bus].Group;

        public void OverrideGroup(SoundBus bus, AudioMixerGroup group)
        {
            if (bus == SoundBus.Master)
            {
                return;
            }

            _busStates[bus].Group = group;
        }

        public float GetVolume(SoundBus bus) => _busStates[bus].Volume;

        public void SetVolume(SoundBus bus, float volume01) => _busStates[bus].Volume = Mathf.Clamp01(volume01);

        public bool IsPaused(SoundBus bus) => _busStates[bus].Paused;

        public void SetPaused(SoundBus bus, bool paused) => _busStates[bus].Paused = paused;

        public void SetAllPaused(bool paused)
        {
            foreach (SoundBus bus in _busStates.Keys)
            {
                _busStates[bus].Paused = paused;
            }
        }

        public float GetEffectiveVolume(SoundBus bus)
        {
            float master = _busStates.TryGetValue(SoundBus.Master, out BusState masterState)
                ? masterState.Volume
                : 1f;

            if (bus == SoundBus.Master)
            {
                return master;
            }

            return Mathf.Clamp01(_busStates[bus].Volume * master);
        }

        public AudioMixerGroup GetEffectiveGroup(SoundBus bus)
        {
            if (bus == SoundBus.Master)
            {
                return null;
            }

            return _busStates[bus].Group;
        }

        public bool IsEffectivelyPaused(SoundBus bus)
        {
            if (bus == SoundBus.Master)
            {
                return _busStates[SoundBus.Master].Paused;
            }

            return _busStates[SoundBus.Master].Paused || _busStates[bus].Paused;
        }

        private sealed class BusState
        {
            public AudioMixerGroup Group; // default routing target for this bus
            public bool Paused;
            public float Volume; // 0..1 bus volume (pre-master)
        }
    }
}