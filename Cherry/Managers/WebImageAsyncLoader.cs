using Legato.Presentation;
using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cherry.Managers
{
    internal class WebImageAsyncLoader : IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        public WebImageAsyncLoader(SiraLog siraLog, IHttpService httpService)
        {
            _siraLog = siraLog;
            _httpService = httpService;
        }

        public async Task<Sprite> LoadSpriteAsync(string path, CancellationToken cancellationToken)
        {
            if (_spriteCache.TryGetValue(path, out var sprite))
            {
                return sprite;
            }
            _siraLog.Debug("Downloading Sprite at " + path);
            var response = await _httpService.GetAsync(path, cancellationToken: cancellationToken);
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
                    sprite = SpriteFactory.Create(imageBytes);
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
            foreach (var sprite in _spriteCache.Values)
            {
                SpriteFactory.Destroy(sprite);
            }
        }
    }
}
