namespace YeKostenko.AudioEngine
{
    public enum VoiceStealMode : byte
    {
        None = 0,
        Oldest = 1,
        Quietest = 2,
        LowestPriority = 3
    }
}