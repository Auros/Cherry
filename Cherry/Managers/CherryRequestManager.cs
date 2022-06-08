﻿using Cherry.Interfaces;
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
        private readonly List<IRequestFilter<Map>> _mapRequestFilters;
        private readonly List<ICherryRequestSource> _cherryRequestSource;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<RequestEventArgs>? SongSkipped;
        public event EventHandler<RequestEventArgs>? SongAccepted;
        public event EventHandler<RequestEventArgs>? SongRequested;

        public CherryRequestManager(Config config, SiraLog siraLog, MapStore mapStore, List<IRequestFilter<Map>> mapRequestFilters, List<ICherryRequestSource> cherryRequestSource)
        {
            _config = config;
            _siraLog = siraLog;
            _mapStore = mapStore;
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
            Map? map = await _mapStore.GetMapAsync(e.Key);
            if (!map.HasValue)
            {
                if (sender is DynamicSender callback)
                {
                    callback.SendMessage($"Map ({e.Key}) does not exist.");
                }
                return;
            }
            if (e.Requester.Elevation != Power.Level4 && !(e.Requester.Elevation == Power.Level3 && _config.AllowL3FilterBypass))
            {
                foreach (var filter in _mapRequestFilters)
                {
                    FilterResult result = await filter.Resolve(map.Value, e);
                    if (!result.IsValid)
                    {
                        if (result.Error != null && sender is DynamicSender callback)
                        {
                            callback.SendMessage(result.Error);
                        }
                        return;
                    }
                }
            }
            _siraLog.Debug($"{map.Value.Name} has been requested by {e.Requester.Username}.");
            MainThreadInvoker.Invoke(() => SongRequested?.Invoke(sender, e));
            if (sender is DynamicSender callbackButCooler)
            {
                var template = _config.ActiveRequestMessageTemplate;
                
                if (Utilities.HasDangerousMessageTemplateProperty(template) && !_config.AllowDangerousTemplateProperties)
                    template = Config.DefaultRequestMessageTemplate;

                Templater templater = new Templater(template);
                templater.AddReplacer("key", map.Value.Key);
                templater.AddReplacer("map.name", map.Value.Name);
                templater.AddReplacer("map.uploader.name", map.Value.Uploader.Name);
                templater.AddReplacer("requester.mention", "@" + e.Requester.Username);

                callbackButCooler.SendMessage(templater.Build());
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