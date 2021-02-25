using Cherry.Models;
using System;

namespace Cherry.Interfaces
{
    public interface IRequestManager
    {
        event EventHandler<RequestEventArgs> SongSkipped;
        event EventHandler<RequestEventArgs> SongRequested;
        event EventHandler<CancelEventArgs> RequestCancelled;

        void Remove(RequestEventArgs request);
    }
}