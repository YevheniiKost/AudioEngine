using UnityEngine;

namespace YellowTape.AudioEngine
{
    public static class AudioEngineHelper
    {
        public static AudioEngineConfig LoadConfig()
        {
            return (AudioEngineConfig)Resources.Load("Data/AudioConfig");
        }
    }
}