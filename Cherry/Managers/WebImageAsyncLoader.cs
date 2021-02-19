using IPA.Loader;
using SiraUtil;
using SiraUtil.Tools;
using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cherry.Managers
{
    internal class WebImageAsyncLoader : IDisposable, ISpriteAsyncLoader
    {
        private readonly SiraLog _siraLog;
        private readonly SiraClient _siraClient;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>(); 

        public WebImageAsyncLoader(SiraLog siraLog, SiraClient siraClient, UBinder<Plugin, PluginMetadata> metadataBinder)
        {
            _siraLog = siraLog;
            _siraClient = siraClient;
            _cancellationTokenSource = new CancellationTokenSource();
            _siraClient.SetUserAgent(nameof(Cherry), metadataBinder.Value.Version);
        }

        public async Task<Sprite> LoadSpriteAsync(string path, CancellationToken cancellationToken)
        {
            if (_spriteCache.TryGetValue(path, out Sprite sprite))
            {
                return sprite;
            }
            _siraLog.Debug("Downloading Sprite at " + path);
            byte[] imageBytes = await _siraClient.DownloadImage(path, cancellationToken == null ? _cancellationTokenSource.Token : cancellationToken);
            if (imageBytes.Length > 0)
            {
                _siraLog.Debug("Successfully downloaded sprite. Parsing and adding to cache.");
                if (_spriteCache.TryGetValue(path, out sprite))
                {
                    return sprite;
                }
                sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                _spriteCache.Add(path, sprite);
                return sprite;
            }
            _siraLog.Warning("Could not downloading and parse sprite. Using blank sprite...");
            return BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}