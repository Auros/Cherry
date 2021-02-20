using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class MapAgeFilter : IRequestFilter<Map>
    {
        public Task<FilterResult> Resolve(Map map, RequestEventArgs requestData)
        {
            return Task.FromResult(new FilterResult(true));
        }
    }
}