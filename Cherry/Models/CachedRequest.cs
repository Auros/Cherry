using Newtonsoft.Json;
using System;

namespace Cherry.Models
{
    public class CachedRequest
    {
        [JsonIgnore]
        public RequestEventArgs Args { get; set; } = null!;
        public DateTime SaveTime { get; set; }
        public bool WasPlayed { get; set; }

        public string Key { get; set; } = null!;
        public GenericRequester Requester { get; set;} = null!;
        public BeatmapDifficulty? Difficulty { get; set; }
        public GameplayModifierMask? Modifiers { get; set; }
        public DateTime RequestTime { get; set; }
        public int Priority { get; set; }
    }
}