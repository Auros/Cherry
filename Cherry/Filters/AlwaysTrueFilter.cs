using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class AlwaysTrueFilter : IRequestFilter<Map>
    {
        public async Task<FilterResult> Resolve(Map subject, RequestEventArgs requestData)
        {
            await Task.CompletedTask;
            return new FilterResult(true);
        }
    }
}