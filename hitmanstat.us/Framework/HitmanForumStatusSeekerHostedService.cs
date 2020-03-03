using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using hitmanstat.us.Models;
using hitmanstat.us.Clients;
using hitmanstat.us.Data;

namespace hitmanstat.us.Framework
{
    internal class HitmanForumStatusSeekerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHitmanForumClient _client;
        private readonly IMemoryCache _cache;

        public HitmanForumStatusSeekerHostedService(
            ILogger<HitmanForumStatusSeekerHostedService> logger, 
            IServiceScopeFactory scopeFactory, 
            IMemoryCache cache, 
            IHitmanForumClient client)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _client = client;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("HitmanForumStatusSeekerHostedService is starting");

            stoppingToken.Register(() =>
                _logger.LogDebug("HitmanForumStatusSeekerHostedService has been canceled"));

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var manager = new EventManager(db, _logger, _cache);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("HitmanForumStatusSeekerHostedService is running");

                var endpointException = new EndpointStatusException(EndpointName.HitmanForum);

                try
                {
                    EndpointStatus endpoint = await _client.GetStatusAsync();

                    if (endpoint.State == EndpointState.Up)
                    {
                        _cache.Set(CacheKeys.HitmanForumKey, endpoint, new MemoryCacheEntryOptions()
                            .SetPriority(CacheItemPriority.NeverRemove));

                        manager.RemoveCache(new List<string>
                            {
                                CacheKeys.HitmanForumErrorCountKey,
                                CacheKeys.HitmanForumErrorEventKey
                            });
                    }
                    else
                    {
                        endpointException.Status = endpoint.Status;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception in the HitmanForumStatusSeekerHostedService");

                    endpointException.Status = "Unhandled error";
                    endpointException.Message = e.Message;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(endpointException.Status))
                    {
                        _cache.Set(CacheKeys.HitmanForumExceptionKey, endpointException, new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(45)));

                        await manager.InsertEndpointExceptionAsync(endpointException);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogDebug("HitmanForumStatusSeekerHostedService has been stopped");
        }
    }
}
