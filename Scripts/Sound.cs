using UnityEngine;

namespace YeKostenko.AudioEngine
{
    public static class Sound
    {
        public static ISoundService SoundService => SoundLocator.Resolve();

        public static PlayHandle Play(AudioEvent evt, in PlayParams p = null)
        {
            try
            {
                return SoundService.Play(evt, p);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error playing sound event '{evt?.name}': {ex.Message}");
                return PlayHandle.Invalid;
            }
        }

        public static PlayHandle PlayAt(AudioEvent evt, Vector3 pos, PlayParams p = null)
        {
            try
            {
                return SoundService.PlayAt(evt, pos, p);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error playing sound event '{evt?.name}' at position {pos}: {ex.Message}");
                return PlayHandle.Invalid;
            }
        }

        public static PlayHandle PlayFollow(AudioEvent evt, Transform t, PlayParams p = null)
        {
            try
            {
                return SoundService.PlayFollow(evt, t, p);
            } catch (System.Exception ex)
            {
                Debug.LogError($"Error playing sound event '{evt?.name}' following transform '{t?.name}': {ex.Message}");
                return PlayHandle.Invalid;
            }
        }

        public static MusicHandle Music(MusicTrack track, MusicParams p = null)
        {
            try
            {
                return SoundService.PlayMusic(track, p);
            } catch (System.Exception ex)
            {
                Debug.LogError($"Error playing music track '{track?.name}': {ex.Message}");
                return MusicHandle.Invalid;
            }
        }
    }
}