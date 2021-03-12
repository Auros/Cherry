using Cherry.Interfaces;
using Cherry.Models;
using System.Linq;
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
                if (subject.MapMetadata.MapCharacteristics.Any(c => c.Difficulties.MinNJS >= _config.MinNJS))
                    return Task.FromResult(new FilterResult(true));
                return Task.FromResult(new FilterResult(false, $"NJS is too low! Minimum: {_config.MaxNJS}"));
            }
            if (_config.DoMaxNJS)
            {
                if (subject.MapMetadata.MapCharacteristics.Any(c => _config.MaxNJS >= c.Difficulties.MaxNJS))
                    return Task.FromResult(new FilterResult(true));
                return Task.FromResult(new FilterResult(false, $"NJS is too high! Maximum: {_config.MaxNJS}"));
            }
            return Task.FromResult(new FilterResult(true));
        }
    }
}