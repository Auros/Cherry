using Cherry.Models;
using SiraUtil;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cherry.Managers
{
    internal class MapStore
    {
        private readonly SiraClient _siraClient;
        private readonly Dictionary<string, Map> _mapCache;

        public MapStore(SiraClient siraClient)
        {
            _siraClient = siraClient;
            _mapCache = new Dictionary<string, Map>();
        }

        public async Task<Map?> GetMapAsync(string key, CancellationToken? token = null)
        {
            if (!_mapCache.TryGetValue(key, out Map map))
            {
                WebResponse webResponse = await _siraClient.GetAsync($"https://beatsaver.com/api/maps/detail/{key}", token ?? CancellationToken.None);
                if (!webResponse.IsSuccessStatusCode)
                {

                    return null;
                }
                map = webResponse.ContentToJson<Map>();
                if (!_mapCache.ContainsKey(key))
                    _mapCache.Add(key, map);
            }
            return map;
        }
    }
}