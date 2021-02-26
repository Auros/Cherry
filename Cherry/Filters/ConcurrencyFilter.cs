using Cherry.Interfaces;
using Cherry.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace Cherry.Filters
{
    internal class ConcurrencyFilter : IRequestFilter<Map>, IInitializable
    {
        private readonly Config _config;
        private readonly DiContainer _container;
        private IRequestHistory _requestHistory = null!;

        public ConcurrencyFilter(Config config, DiContainer container)
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
            bool enabled = false;
            int max = 0;

            switch (requestData.Requester.Elevation)
            {
                case Power.Level1:
                    enabled = _config.Level1Concurrent.Enabled;
                    max = _config.Level1Concurrent.MaxConcurrentRequests;
                    break;
                case Power.Level2:
                    enabled = _config.Level2Concurrent.Enabled;
                    max = _config.Level2Concurrent.MaxConcurrentRequests;
                    break;
                case Power.Level3:
                    enabled = _config.Level3Concurrent.Enabled;
                    max = _config.Level3Concurrent.MaxConcurrentRequests;
                    break;
                case Power.Level4:
                    enabled = _config.Level4Concurrent.Enabled;
                    max = _config.Level4Concurrent.MaxConcurrentRequests;
                    break;
                default:
                    enabled = _config.GlobalConcurrent.Enabled;
                    max = _config.GlobalConcurrent.MaxConcurrentRequests;
                    break;
            }

            bool isAllowed = true;
            if (enabled)
            {
                var id = requestData.Requester.ID.ToLower();
                var sessionStart = DateTime.Now.AddHours(-_config.SesssionLengthInHours);
                var history = await _requestHistory.History();
                isAllowed = max > history.Count(h => !h.WasPlayed && h.RequestTime > sessionStart && h.Requester.ID.ToLower() == id);
            }
            return new FilterResult(isAllowed, isAllowed ? null : $"{requestData.Requester.Username}, you can't have more than {max} requests in the queue!");
        }
    }
}