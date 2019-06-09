using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using hitmanstat.us.Models;
using hitmanstat.us.Clients;

namespace hitmanstat.us.Framework
{
    internal class HitmanStatusSeekerHostedService : BackgroundService
    {
        private readonly IHitmanClient _client;
        private IMemoryCache _cache;

        public HitmanStatusSeekerHostedService(IHitmanClient client, IMemoryCache cache)
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
                    // Fake data (debug purpose)
                    // endpoint.Status = Utilities.ReadResourceFile(Properties.Resources.hitmandebug);

                    _cache.Set(
                        CacheKeys.HitmanKey,
                        endpoint,
                        new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));
                }

                await Task.Delay(TimeSpan.FromSeconds(25), stoppingToken);
            }
        }
    }
}
