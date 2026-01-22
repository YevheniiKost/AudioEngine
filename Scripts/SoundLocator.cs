using UnityEngine;

namespace YeKostenko.AudioEngine
{
    internal static class SoundLocator
    {
        private static ISoundService s_svc;
        private static SoundSettings s_settings;

        public static void Bind(ISoundService svc, SoundSettings settings)
        {
            s_svc = svc;
            s_settings = settings;
        }

        public static ISoundService Resolve()
        {
            if (s_svc != null)
            {
                return s_svc;
            }

            GameObject go = new("__AudioManager");
            Object.DontDestroyOnLoad(go);
            AudioManager mgr = go.AddComponent<AudioManager>();

            s_settings = Resources.Load<SoundSettings>("SoundSettings");
            mgr.Initialize(s_settings);

            s_svc = mgr;
            return s_svc;
        }
    }
}