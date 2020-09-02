using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
                new HitmanService {
                    Name = "HITMAN 2 STADIA",
                    Ref = "h2st",
                    Node = "stadia2-service.hitman.io" }
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
    }
}
