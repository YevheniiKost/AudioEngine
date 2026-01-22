using UnityEngine;

namespace YeKostenko.AudioEngine
{
    public readonly struct PlayHandle
    {
        private readonly int _id;
        private readonly int _version;
        private readonly IAudioVoiceController _controller;

        internal PlayHandle(
            int id,
            int version,
            IAudioVoiceController controller)
        {
            _id = id;
            _version = version;
            _controller = controller;
        }

        public static PlayHandle Invalid => default;

        public bool IsValid =>
            _controller != null &&
            _controller.IsValid(_id, _version);

        public void Stop(float fadeOut = 0f)
        {
            if (!IsValid)
            {
                return;
            }

            _controller.Stop(_id, fadeOut);
        }

        public void SetVolume(float volume01)
        {
            if (!IsValid)
            {
                return;
            }

            _controller.SetVolume(_id, volume01);
        }

        public void SetPitch(float pitch)
        {
            if (!IsValid)
            {
                return;
            }

            _controller.SetPitch(_id, pitch);
        }

        public void SetPaused(bool paused)
        {
            if (!IsValid)
            {
                return;
            }

            _controller.SetPaused(_id, paused);
        }

        public void SetFollow(Transform follow)
        {
            if (!IsValid)
            {
                return;
            }

            _controller.SetFollow(_id, follow);
        }

#if UNITY_EDITOR
        public AudioSource DebugSource =>
            IsValid ? _controller.GetSource(_id) : null;
#endif
    }
}