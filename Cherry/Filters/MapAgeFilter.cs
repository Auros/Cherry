﻿using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class MapAgeFilter : IRequestFilter<Map>
    {
        private readonly Config _config;

        public MapAgeFilter(Config config)
        {
            _config = config;
        }

        public Task<FilterResult> Resolve(Map map, RequestEventArgs requestData)
        {
            if (!_config.DoMapAge)
                return Task.FromResult(new FilterResult(true));

            bool safe = map.Uploaded >= _config.MinimumAge;
            return Task.FromResult(new FilterResult(safe, safe ? null : $"Map ({map.Key}) is too old!"));
        }
    }
}