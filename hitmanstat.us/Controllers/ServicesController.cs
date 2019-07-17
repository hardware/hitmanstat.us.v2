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