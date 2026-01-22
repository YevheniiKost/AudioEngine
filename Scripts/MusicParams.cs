namespace YeKostenko.AudioEngine
{
    public class MusicParams
    {
        public float VolumeMul { get; set; }
        public float FadeIn { get; set; }
        public bool ForceRestart { get; set; }
        public bool AllowCrossfade { get; set; }

        public static MusicParams Default => new MusicParams
        {
            VolumeMul = 1f,
            FadeIn = 0f,
            ForceRestart = false,
            AllowCrossfade = true,
        };
    }
}