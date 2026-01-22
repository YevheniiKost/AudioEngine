namespace YeKostenko.AudioEngine
{
    public class PlayParams
    {
        public float VolumeMul { get; set; }
        public float PitchMul { get; set; }
        public float Delay { get; set; }
        public int? Priority { get; set; }

        public static PlayParams Default => new PlayParams
        {
            VolumeMul = 1,
            PitchMul = 1,
            Delay = 0,
            Priority = null
        };
    }
}