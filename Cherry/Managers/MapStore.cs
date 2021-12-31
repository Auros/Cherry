using Cherry.Models;
using IPA.Loader;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using SiraUtil.Zenject;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cherry.Managers
{
    internal class MapStore
    {
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly Dictionary<string, Map> _mapCache;

        public MapStore(SiraLog siraLog, IHttpService siraClient, UBinder<Plugin, PluginMetadata> metadataBinder)
        {
            _siraLog = siraLog;
            _httpService = siraClient;
            _mapCache = new Dictionary<string, Map>();
        }

        public async Task<Map?> GetMapAsync(string key, CancellationToken? token = null)
        {
            if (!_mapCache.TryGetValue(key, out Map map))
            {
                key = key.ToLowerInvariant();
                _siraLog.Debug($"Fetching map with key '{key}'.");
                IHttpResponse webResponse = await _httpService.GetAsync($"https://api.beatsaver.com/maps/id/{key}", cancellationToken: token).ConfigureAwait(false);
                _siraLog.Info("HELLO");
                if (!webResponse.Successful)
                {
                    _siraLog.Warn(webResponse.Code);
                    return null;
                }
                map = JsonConvert.DeserializeObject<Map>(await webResponse.ReadAsStringAsync());
                if (!_mapCache.ContainsKey(key))
                    _mapCache.Add(key, map);
            }
            return map;
        }
    }
}