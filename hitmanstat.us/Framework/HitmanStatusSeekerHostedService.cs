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
    internal class HitmanStatusSeekerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHitmanClient _client;
        private IMemoryCache _cache;

        public HitmanStatusSeekerHostedService(IServiceScopeFactory scopeFactory, IMemoryCache cache, IHitmanClient client)
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
                    var endpointException = new EndpointStatusException("HITMAN AUTHENTICATION");

                    try
                    {
                        EndpointStatus endpoint = await _client.GetStatusAsync();

                        if (endpoint.State == EndpointState.Up)
                        {
                            // Fake data (debug purpose)
                            // endpoint.Status = Utilities.ReadResourceFile(Properties.Resources.hitmandebug_1down);

                            _cache.Set(
                                CacheKeys.HitmanKey,
                                endpoint,
                                new MemoryCacheEntryOptions()
                                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));

                            await manager.InsertHitmanServicesEntitiesAsync(endpoint.Status);
                        }
                        else
                        {
                            endpointException.Status = endpoint.Status;
                            endpointException.State = endpoint.State;
                        }
                    }
                    catch (Exception e)
                    {
                        endpointException.Status = "Unhandled error";
                        endpointException.Message = e.Message;
                    }
                    finally
                    {
                        if(!string.IsNullOrEmpty(endpointException.Status))
                        {
                            _cache.Set(
                                CacheKeys.HitmanExceptionKey,
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
