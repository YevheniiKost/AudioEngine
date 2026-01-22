using UnityEngine;

namespace YeKostenko.AudioEngine
{
    internal sealed class RandomNumberGenerator
    {
        private uint _state;

        public RandomNumberGenerator()
        {
            _state = 0x6D2B79F5u;
        }

        public uint Next()
        {
            _state = _state == 0 ? 0x6D2B79F5u : _state;
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }

        public float Range01()
        {
            uint random = Next();
            return (random & 0x00FFFFFFu) / 16777216f;
        }

        public float Range(float minimum, float maximum)
        {
            if (maximum < minimum)
            {
                float temp = maximum;
                maximum = minimum;
                minimum = temp;
            }

            if (Mathf.Approximately(minimum, maximum))
            {
                return minimum;
            }

            float normalized = Range01();
            return Mathf.Lerp(minimum, maximum, normalized);
        }
    }
}