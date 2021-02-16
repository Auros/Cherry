using Cherry.Interfaces;
using Cherry.Models;
using SiraUtil.Tools;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace Cherry.Managers
{
    internal class CherryRequestManager : IRequestManager, IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly MapStore _mapStore;
        private readonly List<ICherryRequestSource> _cherryRequestSource;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<RequestEventArgs>? SongRequested;
        public event EventHandler<CancelEventArgs>? RequestCancelled;

        public CherryRequestManager(SiraLog siraLog, MapStore mapStore, List<ICherryRequestSource> cherryRequestSource)
        {
            _siraLog = siraLog;
            _mapStore = mapStore;
            _cherryRequestSource = cherryRequestSource;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void RequestReceived(object sender, RequestEventArgs e)
        {
            _ = RequestReceivedAsync(sender, e);
        }

        private void SongCancelled(object sender, CancelEventArgs e)
        {
            MainThreadInvoker.Invoke(() => RequestCancelled?.Invoke(sender, e));
        }

        private async Task RequestReceivedAsync(object sender, RequestEventArgs e)
        {
            Map? map = await _mapStore.GetMapAsync(e.Key);
            if (!map.HasValue)
            {

                return;
            }
            MainThreadInvoker.Invoke(() => SongRequested?.Invoke(sender, e));
            _siraLog.Debug($"{map.Value.Name} has been requested by {e.Requester.Username}.");
        }

        public void Initialize()
        {
            _ = InitializeAsync();
        }

        public void Dispose()
        {
            _ = DisposeAsync();
        }

        private async Task InitializeAsync()
        {
            foreach (var source in _cherryRequestSource)
            {
                source.RequestCancelled += SongCancelled;
                source.SongRequested += RequestReceived;
                await source.Run();
            }
        }

        private async Task DisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            foreach (var source in _cherryRequestSource)
            {
                await source.Stop();
                source.SongRequested -= RequestReceived;
                source.RequestCancelled -= SongCancelled;
            }
        }
    }
}