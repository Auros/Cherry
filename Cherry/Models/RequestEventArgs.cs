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
    }
}