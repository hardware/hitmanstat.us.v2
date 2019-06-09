using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Models;

namespace hitmanstat.us.Framework
{

    //public class EventManager
    //{
    //    private readonly string ConnectionString;
    //    private readonly string TableName;
    //    private const string PARTITION_KEY = "Events";

    //    public EventManager(string connectionString, string tableName)
    //    {
    //        ConnectionString = connectionString;
    //        TableName = tableName;
    //    }

    //    public async Task<List<EventEntity>> GetEventsAsync(int limit)
    //    {
    //        var storage = new AzureTableStorage(ConnectionString);
    //        var table = await storage.GetTableAsync(TableName);

    //        return await storage.GetTablePartitionEntitiesAsync(table, PARTITION_KEY, limit);
    //    }

    //    public async Task InsertHitmanServicesEntitiesAsync(string jsonString)
    //    {
    //        var json = JObject.Parse(jsonString);
    //        var entities = new List<HitmanService> {
    //            new HitmanService { Name = "HITMAN PC", Node = "pc-service.hitman.io" },
    //            new HitmanService { Name = "HITMAN PS4", Node = "ps4-service.hitman.io" },
    //            new HitmanService { Name = "HITMAN XBOX ONE", Node = "xboxone-service.hitman.io" },
    //            new HitmanService { Name = "HITMAN 2 PC", Node = "pc2-service.hitman.io" },
    //            new HitmanService { Name = "HITMAN 2 PS4", Node = "ps42-service.hitman.io" },
    //            new HitmanService { Name = "HITMAN 2 XBOX ONE", Node = "xboxone2-service.hitman.io" }
    //        };

    //        foreach (var entity in entities)
    //        {
    //            var health = (string)json["services"][entity.Node]["health"];

    //            switch (health)
    //            {
    //                case "unknown":
    //                    entity.Health = HitmanServiceHealth.Unknown;
    //                    break;
    //                case "down":
    //                    entity.Health = HitmanServiceHealth.Down;
    //                    break;
    //                case "maintenance":
    //                    entity.Health = HitmanServiceHealth.Maintenance;
    //                    break;
    //                case "slow":
    //                    entity.Health = HitmanServiceHealth.Slow;
    //                    break;
    //                case "healthy":
    //                    entity.Health = HitmanServiceHealth.Healthy;
    //                    break;
    //            }
    //        }

    //        await InsertHitmanEventsAsync(entities);
    //    }

    //    public async Task InsertEndpointExceptionAsync(EndpointStatusException entity)
    //    {
    //        var storage = new AzureTableStorage(ConnectionString);
    //        var table = await storage.GetTableAsync(TableName);
    //        var rowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
    //        var state = entity.State.GetAttribute<DisplayAttribute>();

    //        if (state != null)
    //        {
    //            var EventEntity = new EventEntity(PARTITION_KEY, rowKey)
    //            {
    //                Service = entity.Name,
    //                Status = state.Name
    //            };

    //            await storage.InsertEntityAsync(table, EventEntity);
    //        }
    //    }

    //    private async Task InsertHitmanEventsAsync(IEnumerable<HitmanService> services)
    //    {
    //        var storage = new AzureTableStorage(ConnectionString);
    //        var table = await storage.GetTableAsync(TableName);

    //        foreach (var service in services)
    //        {
    //            var rowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
    //            var health = service.Health.GetAttribute<DisplayAttribute>();

    //            if(health != null)
    //            {
    //                var EventEntity = new EventEntity(PARTITION_KEY, rowKey)
    //                {
    //                    Service = service.Name,
    //                    Status = health.Name
    //                };

    //                await storage.InsertEntityAsync(table, EventEntity);
    //            }
    //        };
    //    }
    //}
}
