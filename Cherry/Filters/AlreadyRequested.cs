using Cherry.Interfaces;
using Cherry.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace Cherry.Filters
{
    internal class AlreadyRequested : IRequestFilter<Map>, IInitializable
    {
        private readonly Config _config;
        private readonly DiContainer _container;
        private IRequestHistory _requestHistory = null!;

        public AlreadyRequested(Config config, DiContainer container)
        {
            _config = config;
            _container = container;
        }

        public void Initialize()
        {
            _requestHistory = _container.Resolve<IRequestHistory>();
        }

        public async Task<FilterResult> Resolve(Map subject, RequestEventArgs requestData)
        {
            var sessionStart = DateTime.Now.AddHours(-_config.SesssionLengthInHours);
            var recentHistory = (await _requestHistory.History()).Where(h => h.RequestTime > sessionStart);
            bool safe = !recentHistory.Any(r => r.Key == subject.Key);
            return new FilterResult(safe, safe ? null : $"Map ({subject.Key}) has already been requested this session!");
        }
    }
}