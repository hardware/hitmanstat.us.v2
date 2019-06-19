using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Models;
using hitmanstat.us.Clients;
using hitmanstat.us.Data;

namespace hitmanstat.us.Framework
{
    internal class HitmanStatusSeekerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHitmanClient _client;
        private IMemoryCache _cache;

        public HitmanStatusSeekerHostedService(
            ILogger<HitmanStatusSeekerHostedService> logger, 
            IServiceScopeFactory scopeFactory, 
            IMemoryCache cache, 
            IHitmanClient client)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _client = client;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("HitmanStatusSeekerHostedService is starting");

            stoppingToken.Register(() =>
                _logger.LogDebug("HitmanStatusSeekerHostedService has been canceled"));

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var manager = new EventManager(db, _logger, _cache);

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogDebug("HitmanStatusSeekerHostedService is running");

                    var endpointException = new EndpointStatusException(EndpointName.HitmanAuthentication);

                    try
                    {
                        EndpointStatus endpoint = await _client.GetStatusAsync();

                        if (endpoint.State == EndpointState.Up)
                        {
                            // Fake data (debug purpose)
                            //byte[] debugFile = Properties.Resources.hitmandebug_maintenance;
                            //endpoint.Status = Utilities.ReadResourceFile(debugFile);

                            var json = JObject.Parse(endpoint.Status);
                            var timestamp = (DateTime)json["timestamp"];

                            if (manager.IsMostRecentStatus(timestamp))
                            {
                                _cache.Set(CacheKeys.HitmanKey, endpoint, new MemoryCacheEntryOptions()
                                    .SetPriority(CacheItemPriority.NeverRemove));

                                await Task.Run(()
                                    => manager.InsertHitmanServicesEntitiesAsync(json), stoppingToken);
                            }

                            manager.RemoveCache(new List<string>
                            {
                                CacheKeys.HitmanErrorCountKey,
                                CacheKeys.HitmanErrorEventKey
                            });
                        }
                        else
                        {
                            endpointException.Status = endpoint.Status;
                            endpointException.State = endpoint.State;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception in the HitmanStatusSeekerHostedService");

                        endpointException.Status = "Unhandled error";
                        endpointException.Message = e.Message;
                    }
                    finally
                    {
                        if(!string.IsNullOrEmpty(endpointException.Status))
                        {
                            _cache.Set(CacheKeys.HitmanExceptionKey, endpointException, new MemoryCacheEntryOptions()
                                .SetAbsoluteExpiration(TimeSpan.FromSeconds(45)));

                            await manager.InsertEndpointExceptionAsync(endpointException);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

                _logger.LogDebug("HitmanStatusSeekerHostedService has been stopped");
            }
        }
    }
}
