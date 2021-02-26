using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class BannedMapFilter : IRequestFilter<Map>
    {
        private readonly IDenier _denier;

        public BannedMapFilter(IDenier denier)
        {
            _denier = denier;
        }

        public Task<FilterResult> Resolve(Map subject, RequestEventArgs requestData)
        {
            bool safe = !_denier.IsSongBanned(subject.Key);
            return Task.FromResult(new FilterResult(safe, safe ? null : $"Map ({subject.Key}) is banned!"));
        }
    }
}