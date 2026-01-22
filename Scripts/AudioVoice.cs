using UnityEngine;

namespace YeKostenko.AudioEngine
{
    internal class AudioVoice
    {
        private AudioEvent _event;
        private Transform _followTransform;

        public AudioVoice(int id, AudioSource source)
        {
            Id = id;
            Source = source;
        }

        public int Id { get; }
        public int Version { get; set; }
        public VoiceState State { get; set; }

        public AudioSource Source { get; }
        public bool HasEvent { get; private set; }

        public AudioEvent Event
        {
            get => _event;
            set
            {
                _event = value;
                HasEvent = value != null;
            }
        }

        public SoundBus Bus { get; set; }
        public int Priority { get; set; }
        public bool IsCritical { get; set; }

        public bool HasFollow { get; private set; }

        public Transform FollowTransform
        {
            get => _followTransform;
            set
            {
                if (value != null)
                {
                    HasFollow = true;
                    _followTransform = value;
                }
                else
                {
                    HasFollow = false;
                    _followTransform = null;
                }
            }
        }

        public Vector3 StaticPosition { get; set; }

        public float BaseVolume { get; set; }
        public float BasePitch { get; set; }

        public float StartTime { get; set; }
        public float StopRequestedTime { get; set; }

        public float FadeInDuration { get; set; }
        public float FadeOutDuration { get; set; }
        public float FadeInStartTime { get; set; }
        public float FadeOutStartTime { get; set; }
        public bool HasFadeIn { get; set; }
        public bool HasFadeOut { get; set; }

        public float ScheduledStartTime { get; set; }
        public bool HasDelayedStart { get; set; }

        public double LastStartDspTime { get; set; }
    }
}