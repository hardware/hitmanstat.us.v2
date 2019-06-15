using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Data;
using hitmanstat.us.Models;

namespace hitmanstat.us.Framework
{
    public class EventManager
    {
        private readonly DatabaseContext _db;

        public EventManager(DatabaseContext context)
           => _db = context;

        public async Task InsertHitmanServicesEntitiesAsync(string jsonString)
        {
            var json = JObject.Parse(jsonString);
            var entities = new List<HitmanService> {
                new HitmanService { Name = "HITMAN PC", Node = "pc-service.hitman.io" },
                new HitmanService { Name = "HITMAN PS4", Node = "ps4-service.hitman.io" },
                new HitmanService { Name = "HITMAN XBOX ONE", Node = "xboxone-service.hitman.io" },
                new HitmanService { Name = "HITMAN 2 PC", Node = "pc2-service.hitman.io" },
                new HitmanService { Name = "HITMAN 2 PS4", Node = "ps42-service.hitman.io" },
                new HitmanService { Name = "HITMAN 2 XBOX ONE", Node = "xboxone2-service.hitman.io" }
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

        public async Task InsertEndpointException(EndpointStatusException endpoint)
        {
            var state = endpoint.State.GetAttribute<DisplayAttribute>();
            var entity = new Event()
            {
                Service = endpoint.Name,
                State = state.Name,
                Status = endpoint.Status,
                Message = endpoint.Message
            };

            _db.Add(entity);
            await _db.SaveChangesAsync();
        }

        private async Task InsertHitmanEventsAsync(IEnumerable<HitmanService> services)
        {
            foreach (var service in services)
            {
                var health = service.Health.GetAttribute<DisplayAttribute>();

                if (health != null)
                {
                    var entity = new Event()
                    {
                        Service = service.Name,
                        State = health.Name,
                    };

                    _db.Add(entity);
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
