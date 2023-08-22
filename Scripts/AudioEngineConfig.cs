using System.Collections.Generic;
using UnityEngine;

namespace YellowTape.AudioEngine
{
    [CreateAssetMenu(fileName = "Audio Config", menuName = "Yellow Tape/Audio Engine/Config")]
    public class AudioEngineConfig : ScriptableObject
    {
        public List<SfxParameters> Parameters;
    }
}