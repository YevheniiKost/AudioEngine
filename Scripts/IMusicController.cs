namespace YeKostenko.AudioEngine
{
    internal interface IMusicController
    {
        bool IsValid(int id, int version);

        void Stop(float fadeOut);
        void SetVolume(float volume01);
        void Pause(bool paused);

        MusicHandle CrossfadeTo(MusicTrack next, float duration);
        MusicTrack GetTrack(int id);
    }
}