namespace YeKostenko.AudioEngine
{
    public readonly struct MusicHandle
    {
        private readonly IMusicController _controller;

        internal MusicHandle(int id, int version, IMusicController controller)
        {
            Id = id;
            Version = version;
            _controller = controller;
        }

        public static MusicHandle Invalid => default;

        public int Id { get; }

        public int Version { get; }

        public bool IsValid =>
            _controller != null && _controller.IsValid(Id, Version);

        public void Stop(float fadeOut = -1f)
        {
            if (!IsValid)
            {
                return;
            }

            _controller.Stop(fadeOut);
        }

        public void SetVolume(float volume01)
        {
            if (!IsValid)
            {
                return;
            }

            _controller.SetVolume(volume01);
        }

        public void SetPaused(bool paused)
        {
            if (!IsValid)
            {
                return;
            }

            _controller.Pause(paused);
        }

        public MusicHandle CrossfadeTo(
            MusicTrack next,
            float duration = -1f)
        {
            if (!IsValid)
            {
                return default;
            }

            return _controller.CrossfadeTo(next, duration);
        }

        public MusicTrack Track =>
            IsValid ? _controller.GetTrack(Id) : null;
    }
}