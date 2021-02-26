using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class RatingFilter : IRequestFilter<Map>
    {
        private readonly Config _config;

        public RatingFilter(Config config)
        {
            _config = config;
        }

        public Task<FilterResult> Resolve(Map map, RequestEventArgs requestData)
        {
            if (!_config.DoMapRating)
                return Task.FromResult(new FilterResult(true));

            bool safe = map.MapStats.Rating >= _config.MiniumMapRating;
            return Task.FromResult(new FilterResult(safe, safe ? null : $"Map ({map.Key})'s rating ({string.Format("{0:0%}", map.MapStats.Rating)}) is too low! Minimum is {string.Format("{0:0%}", _config.MiniumMapRating)}."));
        }
    }
}