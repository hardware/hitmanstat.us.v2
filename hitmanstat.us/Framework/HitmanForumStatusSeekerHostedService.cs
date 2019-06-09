using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using hitmanstat.us.Models;
using hitmanstat.us.Clients;

namespace hitmanstat.us.Framework
{
    internal class HitmanForumStatusSeekerHostedService : BackgroundService
    {
        private readonly IHitmanForumClient _client;
        private IMemoryCache _cache;

        public HitmanForumStatusSeekerHostedService(IHitmanForumClient client, IMemoryCache cache)
        {
            _client = client;
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                EndpointStatus endpoint = await _client.GetStatusAsync();

                if (endpoint.State == EndpointState.Up)
                {
                    _cache.Set(
                        CacheKeys.HitmanForumKey,
                        endpoint,
                        new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));
                }

                await Task.Delay(TimeSpan.FromSeconds(55), stoppingToken);
            }
        }
    }
}
