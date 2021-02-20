using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class MapAgeFilter : IRequestFilter<Map>
    {
        public async Task<FilterResult> Resolve(Map map, RequestEventArgs requestData)
        {
            await Task.CompletedTask;
            return new FilterResult(true);
        }
    }
}