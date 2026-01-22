using System.Collections.Generic;
using UnityEngine;

namespace YeKostenko.AudioEngine
{
    internal sealed class AudioEventTracker
    {
        private struct EventRuntimeState
        {
            public float NextAllowedStartTime;
            public int ActiveInstanceCount;
        }

        private readonly Dictionary<AudioEvent, EventRuntimeState> _eventStates;

        public AudioEventTracker()
        {
            _eventStates = new Dictionary<AudioEvent, EventRuntimeState>(128);
        }

        public bool CanStart(AudioEvent audioEvent)
        {
            if (audioEvent.Cooldown > 0f)
            {
                if (_eventStates.TryGetValue(audioEvent, out EventRuntimeState state))
                {
                    if (Time.time < state.NextAllowedStartTime)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void OnVoiceStarted(AudioEvent audioEvent)
        {
            EventRuntimeState state = _eventStates.GetValueOrDefault(audioEvent);

            state.ActiveInstanceCount++;

            if (audioEvent.Cooldown > 0f)
            {
                state.NextAllowedStartTime = Time.time + audioEvent.Cooldown;
            }

            _eventStates[audioEvent] = state;
        }

        public void OnVoiceStopped(AudioEvent audioEvent)
        {
            if (_eventStates.TryGetValue(audioEvent, out EventRuntimeState state))
            {
                state.ActiveInstanceCount = Mathf.Max(0, state.ActiveInstanceCount - 1);
                _eventStates[audioEvent] = state;
            }
        }
    }
}