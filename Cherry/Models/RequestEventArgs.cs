using Cherry.Interfaces;
using System;

namespace Cherry.Models
{
    public class RequestEventArgs : EventArgs
    {
        public string Key { get; } = null!;
        public IRequester Requester { get; } = null!;
        public BeatmapDifficulty? Difficulty { get; }
        public GameplayModifierMask? Modifiers { get; }
        public int Priority { get; }

        public RequestEventArgs(string key, IRequester requester, BeatmapDifficulty? difficulty = null, GameplayModifierMask? modifiers = null, int? priority = null)
        {
            Key = key;
            Requester = requester;
            Difficulty = difficulty;
            Modifiers = modifiers;
            Priority = priority ?? 0;
        }
    }
}