using Cherry.Models;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Cherry.Managers
{
    internal class MapStore
    {
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly ConcurrentDictionary<string, Map> _mapCache = new ConcurrentDictionary<string, Map>();

        public MapStore(SiraLog siraLog, IHttpService httpService)
        {
            _siraLog = siraLog;
            _httpService = httpService;
        }

        public async Task<Map?> GetMapAsync(string key, CancellationToken cancellationToken = default)
        {
            key = key.ToLowerInvariant();
            if (!_mapCache.TryGetValue(key, out var map))
            {
                _siraLog.Debug($"Fetching map with key '{key}'.");
                var webResponse = await _httpService.GetAsync($"https://api.beatsaver.com/maps/id/{key}", cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!webResponse.Successful)
                {
                    _siraLog.Warn(webResponse.Code);
                    return null;
                }
                if (DeserializeMap(await webResponse.ReadAsStringAsync()) is not Map fetchedMap)
                    return null;

                map = fetchedMap;
                _mapCache.TryAdd(key, map);
            }
            return map;
        }

        public async Task<Map?> GetMapByHashAsync(string hash, CancellationToken cancellationToken)
        {
            hash = hash.ToLowerInvariant();
            _siraLog.Debug($"Fetching map with hash '{hash}'.");
            var response = await _httpService.GetAsync($"https://api.beatsaver.com/maps/hash/{hash}", cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!response.Successful)
            {
                _siraLog.Warn(response.Code);
                return null;
            }

            if (DeserializeMap(await response.ReadAsStringAsync()) is not Map map)
                return null;

            _mapCache.TryAdd(map.Key.ToLowerInvariant(), map);
            return map;
        }

        private Map? DeserializeMap(string json)
        {
            var map = JsonConvert.DeserializeObject<Map>(json);
            if (!string.IsNullOrWhiteSpace(map.Key) && map.Versions != null && map.Versions.Length > 0)
                return map;

            _siraLog.Warn("BeatSaver returned incomplete map data.");
            return null;
        }
    }
}
