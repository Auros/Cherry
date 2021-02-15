using Cherry.Interfaces;
using System;

namespace Cherry.Models
{
    public class CancelEventArgs : EventArgs
    {
        public string Key { get; } = null!;
        public IRequester Requester { get; } = null!;
    }
}