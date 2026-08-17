using Cherry.Interfaces;
using Cherry.Models;
using Cherry.UI;
using IPA.Utilities;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace Cherry.Managers
{
    internal sealed class RequestQueueService : IInitializable, ITickable, IDisposable
    {
        private static readonly TimeSpan ResolveTimeout = TimeSpan.FromSeconds(30);

        private readonly Config _config;
        private readonly SiraLog _siraLog;
        private readonly MapStore _mapStore;
        private readonly IRequestHistory _requestHistory;
        private readonly IRequestManager _requestManager;
        private readonly WebImageAsyncLoader _webImageAsyncLoader;
        private readonly CancellationTokenSource _disposeTokenSource = new CancellationTokenSource();
        private readonly Queue<RequestEventArgs> _pendingRequests = new Queue<RequestEventArgs>();
        private readonly List<RequestCellInfo> _requests = new List<RequestCellInfo>();

        private bool _currentRequestWasCancelled;
        private bool _isDisposed;
        private RequestEventArgs? _currentRequest;

        internal event Action? Changed;

        internal IReadOnlyList<RequestCellInfo> Requests => _requests;

        public RequestQueueService(Config config, SiraLog siraLog, MapStore mapStore, IRequestHistory requestHistory, IRequestManager requestManager, WebImageAsyncLoader webImageAsyncLoader)
        {
            _config = config;
            _siraLog = siraLog;
            _mapStore = mapStore;
            _requestHistory = requestHistory;
            _requestManager = requestManager;
            _webImageAsyncLoader = webImageAsyncLoader;
        }

        public void Initialize()
        {
            _requestManager.SongRequested += SongRequested;
            _requestManager.SongSkipped += SongRemoved;
            _requestManager.SongAccepted += SongRemoved;
            _ = LoadActiveHistoryAsync();
        }

        public void Tick()
        {
            if (_currentRequest != null || _pendingRequests.Count == 0)
                return;

            _currentRequest = _pendingRequests.Dequeue();
            _currentRequestWasCancelled = false;
            _ = ResolveNextAsync(_currentRequest);
        }

        internal async Task<RequestCellInfo?> ResolveAsync(RequestEventArgs request, CancellationToken cancellationToken)
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(_disposeTokenSource.Token, cancellationToken);
            timeoutSource.CancelAfter(ResolveTimeout);

            if (await _mapStore.GetMapAsync(request.Key, timeoutSource.Token) is not Map map)
                return null;

            await UnityGame.SwitchToMainThreadAsync();
            var cover = await _webImageAsyncLoader.LoadSpriteAsync(map.LatestVersion.CoverURL, timeoutSource.Token);
            return new RequestCellInfo(request, map, cover);
        }

        private async Task LoadActiveHistoryAsync()
        {
            try
            {
                var requests = (await _requestHistory.History())
                    .Where(request => !request.WasPlayed && request.RequestTime >= DateTime.Now.AddHours(-_config.SesssionLengthInHours))
                    .OrderBy(request => request.Args.RequestTime)
                    .Select(request => request.Args)
                    .ToArray();

                await UnityGame.SwitchToMainThreadAsync();
                foreach (var request in requests)
                    Enqueue(request);
            }
            catch (Exception exception)
            {
                _siraLog.Error("Failed to load the active request queue.");
                _siraLog.Error(exception);
            }
        }

        private async Task ResolveNextAsync(RequestEventArgs request)
        {
            try
            {
                var resolved = await ResolveAsync(request, CancellationToken.None);
                await UnityGame.SwitchToMainThreadAsync();

                if (resolved != null && !_currentRequestWasCancelled && !_requests.Any(item => Matches(item.request, request)))
                {
                    _requests.Add(resolved);
                    Changed?.Invoke();
                }
            }
            catch (OperationCanceledException) when (!_disposeTokenSource.IsCancellationRequested)
            {
                _siraLog.Warn($"Timed out resolving request {request.Key}.");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _siraLog.Error($"Failed to resolve request {request.Key}.");
                _siraLog.Error(exception);
            }
            finally
            {
                _currentRequest = null;
                _currentRequestWasCancelled = false;
            }
        }

        private void SongRequested(object sender, RequestEventArgs request)
        {
            MainThreadInvoker.Invoke(() => Enqueue(request));
        }

        private void SongRemoved(object sender, RequestEventArgs request)
        {
            MainThreadInvoker.Invoke(() => Remove(request));
        }

        private void Enqueue(RequestEventArgs request)
        {
            if (_isDisposed || Contains(request))
                return;

            _pendingRequests.Enqueue(request);
        }

        private bool Contains(RequestEventArgs request)
        {
            return (_currentRequest != null && Matches(_currentRequest, request))
                || _pendingRequests.Any(item => Matches(item, request))
                || _requests.Any(item => Matches(item.request, request));
        }

        private void Remove(RequestEventArgs request)
        {
            if (_isDisposed)
                return;

            if (_currentRequest != null && Matches(_currentRequest, request))
                _currentRequestWasCancelled = true;

            var remaining = _pendingRequests.Where(item => !Matches(item, request)).ToArray();
            _pendingRequests.Clear();
            foreach (var item in remaining)
                _pendingRequests.Enqueue(item);

            if (_requests.RemoveAll(item => Matches(item.request, request)) > 0)
                Changed?.Invoke();
        }

        private static bool Matches(RequestEventArgs left, RequestEventArgs right)
        {
            return left.RequestTime == right.RequestTime
                && string.Equals(left.Requester.ID, right.Requester.ID, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _disposeTokenSource.Cancel();
            _requestManager.SongRequested -= SongRequested;
            _requestManager.SongSkipped -= SongRemoved;
            _requestManager.SongAccepted -= SongRemoved;
        }
    }
}
