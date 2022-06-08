﻿using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class NJSFilter : IRequestFilter<Map>
    {
        private readonly Config _config;

        public NJSFilter(Config config)
        {
            _config = config;
        }

        public Task<FilterResult> Resolve(Map subject, RequestEventArgs requestData)
        {
            if (_config.DoMinNJS)
            {
                if (subject.LatestVersion.MinNJS <= _config.MinNJS)
                    return Task.FromResult(new FilterResult(true));
                return Task.FromResult(new FilterResult(false, $"NJS is too low! Minimum: {_config.MinNJS}"));
            }
            if (_config.DoMaxNJS)
            {
                if (subject.LatestVersion.MaxNJS >= _config.MaxNJS)
                    return Task.FromResult(new FilterResult(true));
                return Task.FromResult(new FilterResult(false, $"NJS is too high! Maximum: {_config.MaxNJS}"));
            }
            return Task.FromResult(new FilterResult(true));
        }
    }
}