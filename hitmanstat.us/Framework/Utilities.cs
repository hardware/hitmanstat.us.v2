using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Models;

namespace hitmanstat.us.Framework
{
    public class Utilities
    {
        public static bool IsJsonResponse(HttpContentHeaders headers)
        {
            string value = string.Empty;

            if (headers.TryGetValues("Content-Type", out IEnumerable<string> values))
            {
                value = values.First();
            }

            return value.Contains("application/json");
        }

        public static IEnumerable<HitmanService> ParseHitmanServicesEntities(JObject json)
        {
            var entities = new List<HitmanService> {
                // HITMAN 1 (PC, PS4, XBOX ONE)
                // -----------------------------------------------
                new HitmanService {
                    Name = "HITMAN PC",
                    Ref = "h1pc",
                    Node = "pc-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN PS4",
                    Ref = "h1ps",
                    Node = "ps4-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN XBOX ONE",
                    Ref = "h1xb",
                    Node = "xboxone-service.hitman.io" },
                // HITMAN 2 (PC, PS4, XBOX ONE)
                // -----------------------------------------------
                new HitmanService {
                    Name = "HITMAN 2 PC",
                    Ref = "h2pc",
                    Node = "pc2-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 2 PS4",
                    Ref = "h2ps",
                    Node = "ps42-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 2 XBOX ONE",
                    Ref = "h2xb",
                    Node = "xboxone2-service.hitman.io" },
                // HITMAN 3 (PC, PLAYSTATION, XBOX, STADIA, SWITCH)
                // -----------------------------------------------
                new HitmanService {
                    Name = "HITMAN 3 PC",
                    Ref = "h3pc",
                    Node = "epic.hm3-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 3 PLAYSTATION",
                    Ref = "h3ps",
                    Node = "ps.hm3-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 3 XBOX",
                    Ref = "h3xb",
                    Node = "xbox.hm3-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 3 STADIA",
                    Ref = "h3st",
                    Node = "stadia.hm3-service.hitman.io" },
                new HitmanService {
                    Name = "HITMAN 3 SWITCH",
                    Ref = "h3sw",
                    Node = "switch.hm3-service.hitman.io" }
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

            return entities;
        }

        public static void TimestampMonitoring(IMemoryCache cache, EndpointStatus endpoint, TelemetryClient telemetry)
        {
            if (!cache.TryGetValue(CacheKeys.HitmanTimestampKey, out dynamic _))
            {
                cache.Set(CacheKeys.HitmanTimestampKey, string.Empty, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));

                var json = JObject.Parse(endpoint.Status);

                if (DateTime.TryParse((string)json["timestamp"], out DateTime timestamp))
                {
                    if (timestamp <= DateTime.UtcNow.Add(new TimeSpan(0, -10, 0)))
                    {
                        if (!cache.TryGetValue(CacheKeys.TimestampNotUpdatedBurnout, out dynamic _))
                        {
                            try
                            {
                                telemetry.TrackEvent("HitmanTimestampNotUpdated");

                                cache.Set(CacheKeys.TimestampNotUpdatedBurnout, string.Empty, new MemoryCacheEntryOptions()
                                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));
                            }
                            catch { }
                        }
                    }
                }
            }
        }
    }
}
