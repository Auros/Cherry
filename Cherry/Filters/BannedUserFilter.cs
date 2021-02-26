using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class BannedUserFilter : IRequestFilter<Map>
    {
        private readonly IDenier _denier;

        public BannedUserFilter(IDenier denier)
        {
            _denier = denier;
        }

        public Task<FilterResult> Resolve(Map subject, RequestEventArgs requestData)
        {
            bool safe = !_denier.IsUserBanned(requestData.Requester.ID);
            return Task.FromResult(new FilterResult(safe, safe ? null : $"{requestData.Requester.Username}, you are banned from requesting."));
        }
    }
}