using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class AlwaysTrueFilter : IRequestFilter<Map>
    {
        public Task<FilterResult> Resolve(Map subject, RequestEventArgs requestData)
        {
            return Task.FromResult(new FilterResult(true));
        }
    }
}