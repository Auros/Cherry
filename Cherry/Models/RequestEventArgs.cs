using Cherry.Interfaces;
using Newtonsoft.Json;
using System;

namespace Cherry.Models
{
    public class RequestEventArgs : EventArgs
    {
        public string Key { get; private set; } = null!;
        public IRequester Requester { get; private set; } = null!;
        public BeatmapDifficulty? Difficulty { get; private set; }
        public GameplayModifierMask? Modifiers { get; private set; }
        public DateTime RequestTime { get; private set; }
        public int Priority { get; private set; }

        [JsonConstructor]
        private RequestEventArgs()
        {

        }

        public RequestEventArgs(string key, IRequester requester, DateTime requestTime, BeatmapDifficulty? difficulty = null, GameplayModifierMask? modifiers = null, int? priority = null)
        {
            Key = key;
            Requester = requester;
            Difficulty = difficulty;
            RequestTime = requestTime;
            Modifiers = modifiers;
            Priority = priority ?? 0;
        }
    }
}