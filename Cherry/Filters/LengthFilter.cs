using Cherry.Interfaces;
using Cherry.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class LengthFilter : IRequestFilter<Map>
    {
        private readonly Config _config;

        public LengthFilter(Config config)
        {
            _config = config;
        }

        public Task<FilterResult> Resolve(Map subject, RequestEventArgs requestData)
        {
            if (!_config.DoMaxSongLength)
                return Task.FromResult(new FilterResult(true));

            bool safe = _config.MaxSongLengthInMinutes >= (subject.MapMetadata.Duration == 0 ? subject.MapMetadata.MapCharacteristics.Select(c => c.Difficulties.AnyLength).FirstOrDefault() : (subject.MapMetadata.Duration / 60f));
            return Task.FromResult(new FilterResult(safe, safe ? null : $"Map ({subject.Key}) is too long!"));
        }
    }
}