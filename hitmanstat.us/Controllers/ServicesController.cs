using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using hitmanstat.us.Models;

namespace hitmanstat.us.Controllers
{
    public class ServicesController : Controller
    {
        private IMemoryCache _cache;

        public ServicesController(IMemoryCache cache) => _cache = cache;

        [Route("/status/hitman")]
        public async Task<IActionResult> Hitman()
        {
            while(true)
            {
                if (_cache.TryGetValue(CacheKeys.HitmanKey, out EndpointStatus cachedEndpoint))
                {
                    return Content(cachedEndpoint.Status, "application/json");
                }
                else if (_cache.TryGetValue(CacheKeys.HitmanExceptionKey, out EndpointStatusException cachedExceptionEndpoint))
                {
                    return Json(cachedExceptionEndpoint);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        [Route("/status/hitmanforum")]
        public async Task<IActionResult> HitmanForum()
        {
            while (true)
            {
                if (_cache.TryGetValue(CacheKeys.HitmanForumKey, out EndpointStatus cachedEndpoint))
                {
                    return Json(cachedEndpoint);
                }
                else if (_cache.TryGetValue(CacheKeys.HitmanForumExceptionKey, out EndpointStatusException cachedExceptionEndpoint))
                {
                    return Json(cachedExceptionEndpoint);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }
    }
}