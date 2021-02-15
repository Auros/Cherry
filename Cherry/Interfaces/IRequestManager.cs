using Cherry.Models;
using System;

namespace Cherry.Interfaces
{
    public interface IRequestManager
    {
        event EventHandler<RequestEventArgs> SongRequested;
        event EventHandler<CancelEventArgs> RequestCancelled;
    }
}