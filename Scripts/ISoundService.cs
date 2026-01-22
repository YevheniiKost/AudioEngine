using UnityEngine;

namespace YeKostenko.AudioEngine
{
    public interface ISoundService
    {
        /// <summary>
        /// 2D playback (UI, non-spatial SFX). Uses AudioEvent defaults unless overridden by PlayParams.
        /// </summary>
        PlayHandle Play(AudioEvent evt, PlayParams p = null);

        /// <summary>
        /// 3D playback at world position. Spatial settings come from AudioEvent unless overridden.
        /// </summary>
        PlayHandle PlayAt(AudioEvent evt, Vector3 position, PlayParams p = null);

        /// <summary>
        /// Playback following a transform (e.g., engine loop on a vehicle). Typically used with Loop events.
        /// </summary>
        PlayHandle PlayFollow(AudioEvent evt, Transform follow, PlayParams p = null);

        /// <summary>
        /// Stops all currently playing instances of this AudioEvent (optional fade).
        /// </summary>
        void StopEvent(AudioEvent evt, float fadeOut = 0f);

        /// <summary>
        /// Stops everything (all buses). Intended for scene transitions/emergency resets.
        /// </summary>
        void StopAll(float fadeOut = 0f);

        /// <summary>
        /// Starts music (intro+loop supported by MusicTrack). Returns handle for further control
        /// .</summary>
        MusicHandle PlayMusic(MusicTrack track, MusicParams p = null);

        /// <summary>
        /// Stops music (fade out).
        /// </summary>
        void StopMusic(float fadeOut = 0.3f);

        /// <summary>
        /// Returns current music handle if any; invalid handle if none.
        /// </summary>
        MusicHandle CurrentMusic { get; }

        float GetBusVolume(SoundBus bus);
        void  SetBusVolume(SoundBus bus, float volume01);

        bool IsBusPaused(SoundBus bus);
        void SetBusPaused(SoundBus bus, bool paused);

        /// <summary>
        /// Stops all voices routed to this bus (optional fade).
        /// </summary>
        void StopBus(SoundBus bus, float fadeOut = 0f);

        bool IsPaused { get; }
        void SetPaused(bool paused);

        SoundSettings Settings { get; }
    }
}