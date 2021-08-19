using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class AutomappedFilter : IRequestFilter<Map>
    {
        private readonly Config _config;

        public AutomappedFilter(Config config)
        {
            _config = config;
        }

        public Task<FilterResult> Resolve(Map map, RequestEventArgs requestData)
        {
            if (_config.AllowAutoMappedSongs)
                return Task.FromResult(new FilterResult(true));

            bool safe = !map.Automapper;
            return Task.FromResult(new FilterResult(safe, safe ? null : $"Automapped maps are banned!"));
        }
    }
}