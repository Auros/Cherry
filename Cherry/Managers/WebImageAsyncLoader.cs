using IPA.Loader;
using SiraUtil.Logging;
using SiraUtil.Web;
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
        private readonly IHttpService _httpService;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        public WebImageAsyncLoader(SiraLog siraLog, IHttpService httpService, UBinder<Plugin, PluginMetadata> metadataBinder)
        {
            _siraLog = siraLog;
            _httpService = httpService;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<Sprite> LoadSpriteAsync(string path, CancellationToken cancellationToken)
        {
            if (_spriteCache.TryGetValue(path, out Sprite sprite))
            {
                return sprite;
            }
            _siraLog.Debug("Downloading Sprite at " + path);
            var response = await _httpService.GetAsync(path, cancellationToken: cancellationToken == default ? _cancellationTokenSource.Token : cancellationToken);
            if (response.Successful)
            {
                var imageBytes = await response.ReadAsByteArrayAsync();
                if (imageBytes.Length > 0)
                {
                    _siraLog.Debug("Successfully downloaded sprite. Parsing and adding to cache.");
                    if (_spriteCache.TryGetValue(path, out sprite))
                    {
                        return sprite;
                    }
                    sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                    sprite.texture.wrapMode = TextureWrapMode.Clamp;
                    _spriteCache.Add(path, sprite);
                    return sprite;
                }
            }
            _siraLog.Warn("Could not downloading and parse sprite. Using blank sprite...");
            return BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}