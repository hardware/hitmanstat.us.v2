using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Models;

namespace hitmanstat.us.Controllers
{
    public class ServicesController : Controller
    {
        private readonly TelemetryClient _telemetry;
        private readonly IMemoryCache _cache;

        public ServicesController(IMemoryCache cache, TelemetryClient telemetry)
        {
            _cache = cache;
            _telemetry = telemetry;
        }

        [Route("/status/hitman")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Hitman()
        {
            while(true)
            {
                if (_cache.TryGetValue(CacheKeys.HitmanExceptionKey, out EndpointStatusException cachedExceptionEndpoint))
                {
                    return Json(cachedExceptionEndpoint);
                }
                else if (_cache.TryGetValue(CacheKeys.HitmanKey, out EndpointStatus cachedEndpoint))
                {
                    try
                    {
                        var json = JObject.Parse(cachedEndpoint.Status);
                        var timestamp = (DateTime)json["timestamp"];

                        if (timestamp <= DateTime.UtcNow.Add(new TimeSpan(0, -10, 0)))
                        {
                            if (!_cache.TryGetValue(CacheKeys.TimestampNotUpdatedBurnout, out int cachedChart))
                            {
                                _telemetry.TrackEvent("HitmanTimestampNotUpdated");
                                _cache.Set(CacheKeys.TimestampNotUpdatedBurnout, 1, new MemoryCacheEntryOptions()
                                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));
                            }
                        }
                    }
                    catch
                    {}

                    return Content(cachedEndpoint.Status, "application/json");
                }

                await Task.Delay(1000);
            }
        }

        [Route("/status/hitmanforum")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> HitmanForum()
        {
            while (true)
            {
                if (_cache.TryGetValue(CacheKeys.HitmanForumExceptionKey, out EndpointStatusException cachedExceptionEndpoint))
                {
                    return Json(cachedExceptionEndpoint);
                }
                else if (_cache.TryGetValue(CacheKeys.HitmanForumKey, out EndpointStatus cachedEndpoint))
                {
                    return Json(cachedEndpoint);
                }

                await Task.Delay(1000);
            }
        }
    }
}