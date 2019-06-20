using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Data;
using hitmanstat.us.Models;

namespace hitmanstat.us.Framework
{
    public class EventManager
    {
        private readonly DatabaseContext _db;
        private readonly ILogger _logger;
        private IMemoryCache _cache;

        public EventManager(DatabaseContext context, ILogger logger, IMemoryCache cache)
        {
            _db = context;
            _logger = logger;
            _cache = cache;
        }

        public void RemoveCache(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (_cache.TryGetValue(key, out dynamic _))
                {
                    _logger.LogDebug($"Removing {key} cache store");
                    _cache.Remove(key);
                }
            }
        }

        public bool IsMostRecentStatus(DateTime timestamp)
        {
            bool result = false;

            if (_cache.TryGetValue(CacheKeys.HitmanLastRequestTimestampKey, out DateTime cachedTimestamp))
            {
                if(timestamp > cachedTimestamp)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }

            if (result)
            {
                _cache.Set(CacheKeys.HitmanLastRequestTimestampKey, timestamp, new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.NeverRemove));
            }

            return result;
        }

        public async Task InsertHitmanServicesEntitiesAsync(JObject json)
        {
            var entities = new List<HitmanService> {
                new HitmanService {
                    Name = "HITMAN PC",
                    Node = "pc-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN PS4",
                    Node = "ps4-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN XBOX ONE",
                    Node = "xboxone-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 2 PC",
                    Node = "pc2-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 2 PS4",
                    Node = "ps42-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 2 XBOX ONE",
                    Node = "xboxone2-service.hitman.io" }
            };

            foreach (var entity in entities)
            {
                var health = (string)json["services"][entity.Node]["health"];

                switch (health)
                {
                    case "unknown":
                        entity.Health = HitmanServiceHealth.Unknown;
                        break;
                    case "down":
                        entity.Health = HitmanServiceHealth.Down;
                        break;
                    case "maintenance":
                        entity.Health = HitmanServiceHealth.Maintenance;
                        break;
                    case "slow":
                        entity.Health = HitmanServiceHealth.Slow;
                        break;
                    case "healthy":
                        entity.Health = HitmanServiceHealth.Healthy;
                        break;
                }
            }

            await InsertHitmanEventsAsync(entities);
        }

        public async Task InsertEndpointExceptionAsync(EndpointStatusException endpoint)
        {
            var cacheEventKey = string.Empty;
            var cacheCountKey = string.Empty;

            switch (endpoint.Name)
            {
                case EndpointName.HitmanAuthentication:
                    cacheCountKey = CacheKeys.HitmanErrorCountKey;
                    cacheEventKey = CacheKeys.HitmanErrorEventKey;
                    break;
                case EndpointName.HitmanForum:
                    cacheCountKey = CacheKeys.HitmanForumErrorCountKey;
                    cacheEventKey = CacheKeys.HitmanForumErrorEventKey;
                    break;
            }

            if (string.IsNullOrEmpty(cacheCountKey) || string.IsNullOrEmpty(cacheEventKey))
            {
                return;
            }

            _logger.LogDebug($"An submission request to the database has been initiated for {endpoint.Name} endpoint");

            var multiplier = 2;
            var firstEvent = false;

            if (!_cache.TryGetValue(cacheCountKey, out int counter))
            {
                counter = 1;
                multiplier = counter;
                firstEvent = true;
            }
            else
            {
                counter++;
            }

            if (!_cache.TryGetValue(cacheEventKey, out string _))
            {
                _cache.Set(cacheCountKey, counter, new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.NeverRemove));

                if (firstEvent)
                {
                    _logger.LogDebug("Isolated event, waiting for the next one.");
                    return;
                }

                var state = endpoint.State.GetAttribute<DisplayAttribute>();
                var service = endpoint.Name.GetAttribute<DisplayAttribute>();

                var entity = new Event()
                {
                    Service = service.Name,
                    State = state.Name,
                    Status = endpoint.Status,
                    Message = endpoint.Message
                };

                try
                {
                    _db.Add(entity);
                    await _db.SaveChangesAsync();

                    _logger.LogDebug("Event added in the database.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Database insert exception in InsertEndpointExceptionAsync() method");
                }

                var delay = counter * multiplier * 60;

                _cache.Set(cacheEventKey, string.Empty, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(delay)));

                _logger.LogDebug($"Lock database submissions for {delay} seconds " +
                    $"({counter} (counter) * {multiplier} (multiplier) * 60)");
            }
            else
            {
                _logger.LogDebug("Database is locked with a delay, no events submitted");
            }
        }

        private async Task InsertHitmanEventsAsync(IEnumerable<HitmanService> services)
        {
            var events = new List<Event>();

            foreach (var service in services)
            {
                var health = service.Health.GetAttribute<DisplayAttribute>();

                if (health == null)
                {
                    continue;
                }

                var entity = new Event()
                {
                    Service = service.Name,
                    State = health.Name,
                };

                events.Add(entity);
            }

            if (events.Count == 0)
            {
                _logger.LogDebug("No events to process, all Hitman services operate without any issue");

                RemoveCache(new List<string>
                {
                    CacheKeys.HitmanEntityCountKey,
                    CacheKeys.HitmanEntityEventKey
                });

                return;
            }

            _logger.LogDebug($"A submission request to the database has been initiated ({events.Count} event(s))");

            var multiplier = 2;
            var firstEvent = false;

            if (!_cache.TryGetValue(CacheKeys.HitmanEntityCountKey, out int counter))
            {
                counter = 1;
                multiplier = counter;
                firstEvent = true;
            }
            else
            {
                counter++;
            }

            if (!_cache.TryGetValue(CacheKeys.HitmanEntityEventKey, out string _))
            {
                _cache.Set(CacheKeys.HitmanEntityCountKey, counter, new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.NeverRemove));

                if (firstEvent)
                {
                    _logger.LogDebug("Isolated event, waiting for the next one.");
                    return;
                }

                if (events.Count > 2)
                {
                    var down = HitmanServiceHealth.Down.GetAttribute<DisplayAttribute>().Name;
                    var maintenance = HitmanServiceHealth.Maintenance.GetAttribute<DisplayAttribute>().Name;

                    var maintenanceCount = events
                        .Where(e => e.State == maintenance)
                        .Count();

                    if (maintenanceCount >= 3 && maintenanceCount < 6)
                    {
                        var downEvents = events.Where(e => e.State == down).ToList();

                        if (events.First(e => e.State == maintenance).Service.StartsWith("HITMAN 2"))
                        {
                            events.Clear();
                            events.Add(new Event()
                            {
                                Service = "HITMAN 2 PC / XBOX ONE / PS4",
                                State = maintenance
                            });
                        }
                        else
                        {
                            events.Clear();
                            events.Add(new Event()
                            {
                                Service = "HITMAN PC / XBOX ONE / PS4",
                                State = maintenance
                            });
                        }

                        if(downEvents.Count() > 0)
                        {
                            events.AddRange(downEvents);
                        }
                    }
                    else if (maintenanceCount == 6)
                    {
                        events.Clear();
                        events.Add(new Event()
                        {
                            Service = "HITMAN & HITMAN 2 PC / XBOX ONE / PS4",
                            State = maintenance
                        });
                    }
                }

                try
                {
                    _db.AddRange(events);
                    await _db.SaveChangesAsync();

                    _logger.LogDebug("Event(s) added in the database.");
                }
                catch(Exception e)
                {
                    _logger.LogError(e, "Database insert exception in InsertHitmanEventsAsync() method");
                }

                var delay = counter * multiplier * 60;

                _cache.Set(CacheKeys.HitmanEntityEventKey, string.Empty, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(delay)));

                _logger.LogDebug($"Lock database submissions for {delay} seconds " +
                    $"({counter} (counter) * {multiplier} (multiplier) * 60)");
            }
            else
            {
                _logger.LogDebug("Database is locked with a delay, no events submitted");
            }
        }
    }
}
