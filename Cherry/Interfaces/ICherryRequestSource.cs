using Cherry.Models;
using System;
using System.Threading.Tasks;

namespace Cherry.Interfaces
{
    public interface ICherryRequestSource
    {
        event EventHandler<RequestEventArgs>? SongRequested;
        event EventHandler<CancelEventArgs>? RequestCancelled;

        Task Run();
        Task Stop();
        void SendMessage(object sender, string message);
    }
}