using Cherry.Interfaces;
using Cherry.Models;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace Cherry.Managers
{
    internal class CherryRequestManager : IRequestManager, IInitializable, IDisposable
    {
        private readonly Config _config;
        private readonly SiraLog _siraLog;
        private readonly MapStore _mapStore;
        private readonly RequestKeyResolver _requestKeyResolver;
        private readonly List<IRequestFilter<Map>> _mapRequestFilters;
        private readonly List<ICherryRequestSource> _cherryRequestSource;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public bool HasNewRequests { get; set; }

        public event EventHandler<RequestEventArgs>? SongSkipped;
        public event EventHandler<RequestEventArgs>? SongAccepted;
        public event EventHandler<RequestEventArgs>? SongRequested;

        public CherryRequestManager(Config config, SiraLog siraLog, MapStore mapStore, RequestKeyResolver requestKeyResolver, List<IRequestFilter<Map>> mapRequestFilters, List<ICherryRequestSource> cherryRequestSource)
        {
            _config = config;
            _siraLog = siraLog;
            _mapStore = mapStore;
            _requestKeyResolver = requestKeyResolver;
            _mapRequestFilters = mapRequestFilters;
            _cherryRequestSource = cherryRequestSource;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void RequestReceived(object sender, RequestEventArgs e)
        {
            _ = RequestReceivedAsync(sender, e);
        }

        private void SongCancelled(object sender, RequestEventArgs e)
        {
            MainThreadInvoker.Invoke(() => SongSkipped?.Invoke(sender, e));
        }

        private async Task RequestReceivedAsync(object sender, RequestEventArgs e)
        {
            var callback = sender as DynamicSender;
            try
            {
                var key = await _requestKeyResolver.ResolveAsync(e.Key, _cancellationTokenSource.Token);
                if (key == null)
                {
                    callback?.SendMessage($"Invalid map reference ({e.Key}).");
                    return;
                }

                if (await _mapStore.GetMapAsync(key, _cancellationTokenSource.Token) is not Map map)
                {
                    callback?.SendMessage($"Map ({key}) does not exist.");
                    return;
                }

                var request = new RequestEventArgs(map.Key, e.Requester, e.RequestTime, e.Difficulty, e.Modifiers, e.Priority);
                if (e.Requester.Elevation != Power.Level4 && !(e.Requester.Elevation == Power.Level3 && _config.AllowL3FilterBypass))
                {
                    foreach (var filter in _mapRequestFilters)
                    {
                        var result = await filter.Resolve(map, request);
                        if (!result.IsValid)
                        {
                            if (result.Error != null)
                                callback?.SendMessage(result.Error);
                            return;
                        }
                    }
                }

                HasNewRequests = true;
                _siraLog.Debug($"{map.Name} has been requested by {e.Requester.Username}.");
                MainThreadInvoker.Invoke(() => SongRequested?.Invoke(sender, request));
                if (callback != null)
                {
                    var template = _config.ActiveRequestMessageTemplate;
                    if (Utilities.HasDangerousMessageTemplateProperty(template) && !_config.AllowDangerousTemplateProperties)
                        template = Config.DefaultRequestMessageTemplate;

                    var templater = new Templater(template);
                    templater.AddReplacer("key", map.Key);
                    templater.AddReplacer("map.name", map.Name);
                    templater.AddReplacer("map.uploader.name", map.Uploader.Name);
                    templater.AddReplacer("requester.mention", "@" + e.Requester.Username);
                    callback.SendMessage(templater.Build());
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _siraLog.Error($"Failed to process map request '{e.Key}'.");
                _siraLog.Error(exception);
                callback?.SendMessage($"Could not process map ({e.Key}).");
            }
        }

        public void Remove(RequestEventArgs request)
        {
            SongSkipped?.Invoke(this, request);
        }

        public void MarkAsRead(RequestEventArgs request)
        {
            SongAccepted?.Invoke(this, request);
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
