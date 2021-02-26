using Cherry.Interfaces;
using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Filters
{
    internal class QueueStatusFilter : IRequestFilter<Map>
    {
        private readonly Config _config;

        public QueueStatusFilter(Config config)
        {
            _config = config;
        }

        public Task<FilterResult> Resolve(Map subject, RequestEventArgs requestData)
        {
            return Task.FromResult(new FilterResult(_config.QueueOpened, _config.QueueOpened ? null : "Queue is closed."));
        }
    }
}