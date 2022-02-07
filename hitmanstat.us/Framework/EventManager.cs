using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Data;
using hitmanstat.us.Models;

namespace hitmanstat.us.Framework
{
    public class EventManager
    {
        private readonly DatabaseContext _db;
        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;

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

        public async Task SeedCurrentUserReportCountersAsync()
        {
            if (!_cache.TryGetValue(CacheKeys.CurrentUserReportCountersKey, out dynamic _))
            {
                _cache.Set(CacheKeys.CurrentUserReportCountersKey, string.Empty, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1)));

                var today = await _db.UserReportCounters
                    .AsNoTracking()
                    .SingleOrDefaultAsync(c => c.Date.Date == DateTime.Today);

                if (today == null)
                {
                    _db.Add(new UserReportCounter());
                }

                var oldCounters = (from c in _db.UserReportCounters
                                   where c.Date < DateTime.Now.AddDays(-15)
                                   select c);

                if (oldCounters.Count() > 0)
                {
                    _db.UserReportCounters.RemoveRange(oldCounters);
                }

                var oldReports = (from r in _db.UserReports
                                  where r.Date < DateTime.Now.AddDays(-15)
                                  select r);

                if (oldReports.Count() > 0)
                {
                    _db.UserReports.RemoveRange(oldReports);
                }

                await _db.SaveChangesAsync();
            }
        }

        public async Task InsertHitmanServicesEntitiesAsync(JObject json)
        {
            await InsertHitmanEventsAsync(Utilities.ParseHitmanServicesEntities(json));
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

                /*
                 * Maintenance events grouping (per game)
                 * 6(H3) + 3(H2) + 3(H1) = 12
                 */
                var min = 2;
                var max = 12;

                if (events.Count >= min)
                {
                    var down = HitmanServiceHealth.Down.GetAttribute<DisplayAttribute>().Name;
                    var maintenance = HitmanServiceHealth.Maintenance.GetAttribute<DisplayAttribute>().Name;
                    var maintenanceEvents = events.Where(e => e.State == maintenance);
                    var maintenanceCount = maintenanceEvents.Count();

                    if (maintenanceCount >= min && maintenanceCount < max)
                    {
                        var downEvents = events.Where(e => e.State == down).ToList();

                        var h3MaintenanceEvents = new List<Event>();
                        var h2MaintenanceEvents = new List<Event>();
                        var h1MaintenanceEvents = new List<Event>();

                        foreach (var ev in maintenanceEvents)
                        {
                            if (ev.Service.StartsWith("HITMAN 3"))
                            {
                                h3MaintenanceEvents.Add(ev);

                            }
                            else if (ev.Service.StartsWith("HITMAN 2"))
                            {
                                h2MaintenanceEvents.Add(ev);
                            }
                            else
                            {
                                h1MaintenanceEvents.Add(ev);
                            }
                        }

                        events.Clear();

                        if (h3MaintenanceEvents.Count > 0)
                        {
                            // 6(H3)
                            if (h3MaintenanceEvents.Count == 6)
                            {
                                events.Add(new Event()
                                {
                                    Service = "ALL HITMAN 3 SERVICES",
                                    State = maintenance
                                });
                            }
                            else
                            {
                                var items = new List<string>();

                                foreach (var ev in h3MaintenanceEvents)
                                {
                                    var item = ev.Service.Replace("HITMAN 3 ", string.Empty);
                                    items.Add(item);
                                }

                                events.Add(new Event()
                                {
                                    Service = $"HITMAN 3 { string.Join(" / ", items) }",
                                    State = maintenance
                                });
                            }
                        }

                        if (h2MaintenanceEvents.Count > 0)
                        {
                            // 3(H2)
                            if (h2MaintenanceEvents.Count == 3)
                            {
                                events.Add(new Event()
                                {
                                    Service = "ALL HITMAN 2 SERVICES",
                                    State = maintenance
                                });
                            }
                            else
                            {
                                var items = new List<string>();

                                foreach (var ev in h2MaintenanceEvents)
                                {
                                    var item = ev.Service.Replace("HITMAN 2 ", string.Empty);
                                    items.Add(item);
                                }

                                events.Add(new Event()
                                {
                                    Service = $"HITMAN 2 { string.Join(" / ", items) }",
                                    State = maintenance
                                });
                            }
                        }

                        if (h1MaintenanceEvents.Count > 0)
                        {
                            // 3(H1)
                            if (h1MaintenanceEvents.Count == 3)
                            {
                                events.Add(new Event()
                                {
                                    Service = "ALL HITMAN 1 SERVICES",
                                    State = maintenance
                                });
                            }
                            else
                            {
                                var items = new List<string>();

                                foreach (var ev in h1MaintenanceEvents)
                                {
                                    var item = ev.Service.Replace("HITMAN ", string.Empty);
                                    items.Add(item);
                                }

                                events.Add(new Event()
                                {
                                    Service = $"HITMAN 1 { string.Join(" / ", items) }",
                                    State = maintenance
                                });
                            }
                        }

                        if (downEvents.Count > 0)
                        {
                            events.AddRange(downEvents);
                        }
                    }
                    else if (maintenanceCount == max)
                    {
                        events.Clear();
                        events.Add(new Event()
                        {
                            Service = "ALL HITMAN SERVICES",
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
