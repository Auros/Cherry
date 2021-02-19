using System;

namespace Cherry.Models
{
    public class CachedRequest
    {
        public RequestEventArgs Args { get; set; } = null!;
        public DateTime SaveTime { get; set; }
        public bool WasPlayed { get; set; }
    }
}