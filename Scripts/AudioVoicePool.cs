using System;

using UnityEngine;
using UnityEngine.Audio;

namespace YeKostenko.AudioEngine
{
    internal sealed class AudioVoicePool
    {
        private readonly IAudioVoiceController _audioVoiceController;
        private readonly AudioBusManager _busManager;
        private readonly Transform _parentTransform;
        private readonly RandomNumberGenerator _randomGenerator;
        private readonly SoundSettings _settings;

        private int _activeVoiceCount;

        private AudioVoice[] _voices;

        public AudioVoicePool(Transform parent, SoundSettings settings, AudioBusManager busManager,
            IAudioVoiceController audioVoiceController)
        {
            _parentTransform = parent;
            _settings = settings;
            _busManager = busManager;
            _audioVoiceController = audioVoiceController;
            _randomGenerator = new RandomNumberGenerator();

            InitializePool();
        }

        private void InitializePool()
        {
            int targetPoolSize = _settings != null ? Mathf.Max(8, _settings.PrewarmSources) : 32;
            _voices = new AudioVoice[targetPoolSize];

            for (int index = 0; index < targetPoolSize; index++)
            {
                GameObject voiceObject = new($"__AudioVoice_{index}");
                voiceObject.transform.SetParent(_parentTransform, false);
                AudioSource source = voiceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;

                _voices[index] = new AudioVoice(index, source)
                {
                    Version = 1,
                    State = VoiceState.Free
                };
            }
        }

        public void Update()
        {
            float currentTime = Time.time;
            int activeCount = 0;

            foreach (AudioVoice voice in _voices)
            {
                if (voice.State == VoiceState.Free)
                {
                    continue;
                }

                UpdateVoicePosition(voice);
                ProcessDelayedStart(voice, currentTime);

                if (voice.State == VoiceState.Playing || voice.State == VoiceState.Stopping ||
                    voice.State == VoiceState.Starting)
                {
                    UpdateVoiceVolume(voice, currentTime);

                    if (ShouldRecycleVoice(voice, currentTime))
                    {
                        RecycleVoice(voice);
                        continue;
                    }

                    activeCount++;
                }
            }

            _activeVoiceCount = activeCount;
        }

        private void UpdateVoicePosition(AudioVoice voice)
        {
            if (voice.HasFollow)
            {
                voice.Source.transform.position = voice.FollowTransform.position;
            }
            else if (voice.HasEvent && voice.Event.Spatial.Is3D && voice.State != VoiceState.Free)
            {
                voice.Source.transform.position = voice.StaticPosition;
            }
        }

        private void ProcessDelayedStart(AudioVoice voice, float currentTime)
        {
            if (voice.State == VoiceState.Starting && voice.HasDelayedStart && currentTime >= voice.ScheduledStartTime)
            {
                voice.HasDelayedStart = false;
                voice.State = VoiceState.Playing;
                voice.StartTime = currentTime;
                ApplyBusPausedState(voice);
                voice.Source.Play();
            }
        }

        private bool ShouldRecycleVoice(AudioVoice voice, float currentTime)
        {
            if (voice.State != VoiceState.Starting && voice.HasEvent && !voice.Event.Loop)
            {
                if (!voice.Source.isPlaying && !voice.HasDelayedStart)
                {
                    return true;
                }
            }

            if (voice.State == VoiceState.Stopping)
            {
                float elapsedTime = currentTime - voice.FadeOutStartTime;
                if (!voice.HasFadeOut || elapsedTime >= voice.FadeOutDuration)
                {
                    voice.Source.Stop();
                    return true;
                }
            }

            return false;
        }

        public PlayHandle Play(AudioEvent audioEvent, Vector3 position, Transform follow, PlayParams parameters,
            AudioEventTracker eventTracker)
        {
            if (audioEvent == null || audioEvent.Clips == null || audioEvent.Clips.Length == 0)
            {
                throw new ArgumentNullException();
            }

            if (!eventTracker.CanStart(audioEvent))
            {
                return PlayHandle.Invalid;
            }

            int voiceIndex = FindFreeVoiceIndex();
            if (voiceIndex < 0)
            {
                voiceIndex = StealVoice(parameters.Priority ?? audioEvent.Priority.Priority);
                if (voiceIndex < 0)
                {
                    return default;
                }
            }

            AudioVoice voice = _voices[voiceIndex];
            PrepareVoice(voice, audioEvent, position, follow, parameters ?? PlayParams.Default);
            eventTracker.OnVoiceStarted(audioEvent);

            return new PlayHandle(voice.Id, voice.Version, _audioVoiceController);
        }

        public void StopEvent(AudioEvent audioEvent, float fadeOut)
        {
            if (audioEvent == null)
            {
                return;
            }

            foreach (AudioVoice v in _voices)
            {
                if (v.State == VoiceState.Free)
                {
                    continue;
                }

                if (v.Event != audioEvent)
                {
                    continue;
                }

                SetVoicePaused(v.Id, false);
                StopVoice(v.Id, fadeOut);
            }
        }

        public void StopBus(SoundBus bus, float fadeOut)
        {
            foreach (AudioVoice v in _voices)
            {
                if (v.State == VoiceState.Free)
                {
                    continue;
                }

                if (v.Bus != bus)
                {
                    continue;
                }

                SetVoicePaused(v.Id, false);
                StopVoice(v.Id, fadeOut);
            }
        }


        public void StopAll(float fadeOut)
        {
            foreach (AudioVoice v in _voices)
            {
                if (v.State == VoiceState.Free)
                {
                    continue;
                }

                SetVoicePaused(v.Id, false);
                StopVoice(v.Id, fadeOut);
            }
        }

        public void OnBusVolumeChanged(SoundBus bus)
        {
            foreach (AudioVoice v in _voices)
            {
                if (v.State == VoiceState.Free)
                {
                    continue;
                }

                if (v.Bus != bus)
                {
                    continue;
                }

                UpdateVoiceVolume(v, Time.time);
            }
        }

        public void OnBusPauseChanged(SoundBus bus)
        {
            foreach (AudioVoice v in _voices)
            {
                if (v.State == VoiceState.Free)
                {
                    continue;
                }

                if (v.Bus != bus)
                {
                    continue;
                }

                ApplyBusPausedState(v);
            }
        }

        public bool IsHandleValid(int id, int version)
        {
            if (id < 0 || id >= _voices.Length)
            {
                return false;
            }

            AudioVoice voice = _voices[id];
            return voice.Version == version && voice.State != VoiceState.Free;
        }

        public void StopVoice(int id, float fadeOut)
        {
            if (id < 0 || id >= _voices.Length)
            {
                return;
            }

            AudioVoice voice = _voices[id];
            if (voice.State == VoiceState.Free || voice.State == VoiceState.Stopping)
            {
                return;
            }

            voice.State = VoiceState.Stopping;
            voice.FadeOutDuration = Mathf.Max(0f, fadeOut);
            voice.FadeOutStartTime = Time.time;

            if (voice.FadeOutDuration <= 0f)
            {
                voice.Source.Stop();
            }
        }

        public void SetVoiceVolume(int id, float volume01)
        {
            if (id < 0 || id >= _voices.Length)
            {
                return;
            }

            AudioVoice voice = _voices[id];
            if (voice.State == VoiceState.Free)
            {
                return;
            }

            voice.BaseVolume = Mathf.Clamp01(volume01);
            UpdateVoiceVolume(voice, Time.time);
        }

        public void SetVoicePitch(int id, float pitch)
        {
            if (id < 0 || id >= _voices.Length)
            {
                return;
            }

            AudioVoice voice = _voices[id];
            if (voice.State == VoiceState.Free)
            {
                return;
            }

            voice.BasePitch = pitch;
            voice.Source.pitch = pitch;
        }

        public void SetVoicePaused(int id, bool paused)
        {
            if (id < 0 || id >= _voices.Length)
            {
                return;
            }

            AudioVoice voice = _voices[id];
            if (voice.State == VoiceState.Free)
            {
                return;
            }

            if (paused)
            {
                voice.Source.Pause();
            }
            else
            {
                if (voice.State == VoiceState.Playing && !voice.HasDelayedStart)
                {
                    voice.Source.UnPause();
                }
            }
        }

        public void SetVoiceFollow(int id, Transform follow)
        {
            if (id < 0 || id >= _voices.Length)
            {
                return;
            }

            AudioVoice voice = _voices[id];
            if (voice.State == VoiceState.Free)
            {
                return;
            }

            voice.FollowTransform = follow;
        }

        public AudioSource GetVoiceSource(int id)
        {
            if (id < 0 || id >= _voices.Length)
            {
                return null;
            }

            AudioVoice voice = _voices[id];
            if (voice.State == VoiceState.Free)
            {
                return null;
            }

            return voice.Source;
        }

        private int FindFreeVoiceIndex()
        {
            for (int index = 0; index < _voices.Length; index++)
            {
                if (_voices[index].State == VoiceState.Free)
                {
                    return index;
                }
            }

            return -1;
        }

        private int StealVoice(int priority)
        {
            int candidateIndex = -1;
            float lowestPriority = priority;
            float longestLifetime = float.MinValue;
            float lowestVolume = float.MaxValue;
            float currentTime = Time.time;

            for (int index = 0; index < _voices.Length; index++)
            {
                AudioVoice voice = _voices[index];

                if (voice.State == VoiceState.Free || voice.Event == null)
                {
                    continue;
                }

                if (voice.Event.Priority.IsCritical)
                {
                    continue;
                }

                float voiceLifetime = currentTime - voice.StartTime;
                if (voiceLifetime < voice.Event.Priority.MinLifetime)
                {
                    continue;
                }

                bool shouldSteal = false;

                switch (voice.Event.Priority.Steal)
                {
                    case VoiceStealMode.Oldest:
                        if (voiceLifetime > longestLifetime)
                        {
                            longestLifetime = voiceLifetime;
                            shouldSteal = true;
                        }

                        break;

                    case VoiceStealMode.Quietest:
                        float volume = voice.Source.volume;
                        if (volume < lowestVolume)
                        {
                            lowestVolume = volume;
                            shouldSteal = true;
                        }

                        break;

                    case VoiceStealMode.LowestPriority:
                        if (voice.Priority < lowestPriority)
                        {
                            lowestPriority = voice.Priority;
                            shouldSteal = true;
                        }

                        break;

                    case VoiceStealMode.None:
                    default:
                        continue;
                }

                if (shouldSteal)
                {
                    candidateIndex = index;
                }
            }

            return candidateIndex;
        }

        private void PrepareVoice(AudioVoice voice, AudioEvent audioEvent, Vector3 position,
            Transform follow, PlayParams parameters)
        {
            AudioClip selectedClip;

            if (audioEvent.Clips.Length == 1)
            {
                selectedClip = audioEvent.Clips[0];
            }
            else
            {
                int randomIndex = (int)(_randomGenerator.Next() % (uint)audioEvent.Clips.Length);
                selectedClip = audioEvent.Clips[randomIndex];
            }

            voice.Event = audioEvent;
            voice.Bus = audioEvent.Bus;
            voice.Priority = audioEvent.Priority.Priority;

            voice.FollowTransform = follow;
            voice.StaticPosition = position;

            float volumeVariation = _randomGenerator.Range(audioEvent.VolumeMin, audioEvent.VolumeMax);
            voice.BaseVolume = volumeVariation * parameters.VolumeMul;

            float pitchVariation = _randomGenerator.Range(audioEvent.PitchMin, audioEvent.PitchMax);
            voice.BasePitch = pitchVariation * parameters.PitchMul;

            AudioSource source = voice.Source;
            source.clip = selectedClip;
            source.loop = audioEvent.Loop;
            source.pitch = voice.BasePitch;

            ApplyRouteToSource(voice, audioEvent);

            if (audioEvent.Spatial.Is3D)
            {
                source.spatialBlend = audioEvent.Spatial.SpatialBlend;
                source.minDistance = audioEvent.Spatial.MinDistance;
                source.maxDistance = audioEvent.Spatial.MaxDistance;
                source.rolloffMode = audioEvent.Spatial.Rolloff;
                source.dopplerLevel = audioEvent.Spatial.DopplerLevel;
                source.spread = audioEvent.Spatial.Spread;
            }
            else
            {
                source.spatialBlend = 0f;
            }

            source.outputAudioMixerGroup = _settings.GetDefaultGroupForBus(audioEvent.Bus);

            float currentTime = Time.time;
            voice.StartTime = currentTime;
            voice.StopRequestedTime = -1f;

            voice.HasFadeIn = audioEvent.Fade.HasFadeIn;
            voice.FadeInDuration = audioEvent.Fade.In;
            voice.FadeInStartTime = currentTime;

            voice.HasFadeOut = audioEvent.Fade.HasFadeOut;
            voice.FadeOutDuration = audioEvent.Fade.Out;
            voice.FadeOutStartTime = Time.time + parameters.Delay + selectedClip.length - voice.FadeOutDuration;

            voice.HasDelayedStart = parameters.Delay > 0f;
            voice.ScheduledStartTime = currentTime + parameters.Delay;

            voice.State = voice.HasDelayedStart ? VoiceState.Starting : VoiceState.Playing;


            if (voice.HasFadeIn)
            {
                source.volume = 0f;
            }
            else
            {
                UpdateVoiceVolume(voice, currentTime);
            }

            ApplyBusPausedState(voice);

            if (!voice.HasDelayedStart)
            {
                voice.LastStartDspTime = AudioSettings.dspTime;
                source.Play();
            }

            voice.Version++;
        }

        private void ApplyRouteToSource(AudioVoice voice, AudioEvent audioEvent)
        {
            AudioSource source = voice.Source;

            AudioMixerGroup group = audioEvent != null && audioEvent.Route != null ? audioEvent.Route.Output : null;
            if (group == null && _settings != null)
            {
                group = _settings.GetDefaultGroupForBus(SoundBus.Music);
            }

            if (group != null)
            {
                source.outputAudioMixerGroup = group;
            }

            if (audioEvent != null && audioEvent.Route != null && audioEvent.Route.Snapshot != null)
            {
                audioEvent.Route.Snapshot.TransitionTo(0f);
            }
        }

        private void UpdateVoiceVolume(AudioVoice voice, float currentTime)
        {
            float busVolume = _busManager.GetVolume(voice.Bus);
            float finalVolume = voice.BaseVolume * busVolume;

            if (voice.HasFadeIn)
            {
                float elapsedTime = currentTime - voice.FadeInStartTime;
                if (elapsedTime < voice.FadeInDuration)
                {
                    float fadeProgress = elapsedTime / voice.FadeInDuration;
                    finalVolume *= fadeProgress;
                }
                else
                {
                    voice.HasFadeIn = false;
                }
            }

            if (voice.HasFadeOut)
            {
                float elapsedTime = currentTime - voice.FadeOutStartTime;
                if (elapsedTime < voice.FadeOutDuration)
                {
                    voice.State = VoiceState.Stopping;
                    float fadeProgress = 1f - (elapsedTime / voice.FadeOutDuration);
                    finalVolume *= fadeProgress;
                }
                else
                {
                    finalVolume = 0f;
                }
            }

            voice.Source.volume = finalVolume;
        }

        private void ApplyBusPausedState(AudioVoice voice)
        {
            bool isBusPaused = _busManager.IsPaused(voice.Bus);
            if (isBusPaused)
            {
                voice.Source.Pause();
            }
            else
            {
                if (voice.State == VoiceState.Playing && !voice.HasDelayedStart)
                {
                    voice.Source.UnPause();
                }
            }
        }

        private void RecycleVoice(AudioVoice voice)
        {
            voice.Source.Stop();
            voice.Source.clip = null;
            voice.Event = null;
            voice.FollowTransform = null;
            voice.State = VoiceState.Free;
        }
    }
}