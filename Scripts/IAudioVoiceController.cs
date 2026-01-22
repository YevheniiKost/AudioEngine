using UnityEngine;

namespace YeKostenko.AudioEngine
{
    internal interface IAudioVoiceController
    {
        bool IsValid(int id, int version);

        void Stop(int id, float fadeOut);
        void SetVolume(int id, float volume01);
        void SetPitch(int id, float pitch);
        void SetPaused(int id, bool paused);
        void SetFollow(int id, Transform follow);

#if UNITY_EDITOR
        AudioSource GetSource(int id);
#endif
    }
}