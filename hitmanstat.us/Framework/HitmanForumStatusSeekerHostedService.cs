using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using hitmanstat.us.Models;
using hitmanstat.us.Clients;
using hitmanstat.us.Data;

namespace hitmanstat.us.Framework
{
    internal class HitmanForumStatusSeekerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHitmanForumClient _client;
        private IMemoryCache _cache;

        public HitmanForumStatusSeekerHostedService(IServiceScopeFactory scopeFactory, IMemoryCache cache, IHitmanForumClient client)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var manager = new EventManager(db);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var endpointException = new EndpointStatusException("HITMAN FORUM");

                    try
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
                        else
                        {
                            endpointException.Status = endpoint.Status;
                        }
                    }
                    catch (Exception e)
                    {
                        endpointException.Status = "Unhandled error";
                        endpointException.Message = e.Message;
                    }
                    finally
                    {
                        if (!string.IsNullOrEmpty(endpointException.Status))
                        {
                            _cache.Set(
                                CacheKeys.HitmanForumExceptionKey,
                                endpointException,
                                new MemoryCacheEntryOptions()
                                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));

                            await manager.InsertEndpointException(endpointException);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(25), stoppingToken);
                }
            }
        }
    }
}
