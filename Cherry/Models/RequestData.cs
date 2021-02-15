namespace Cherry.Models
{
    public class RequestData
    {
        public int? Priority { get; }
        public string Key { get; } = null!;
        public BeatmapDifficulty? Difficulty { get; }
        public GameplayModifierMask? Modifiers { get; }

        public RequestData(string key, BeatmapDifficulty? difficulty = null, GameplayModifierMask? modifiers = null, int? priority = null)
        {
            Key = key;
            Difficulty = difficulty;
            Modifiers = modifiers;
            Priority = priority;
        }
    }
}