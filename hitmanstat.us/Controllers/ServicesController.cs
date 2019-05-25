using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using hitmanstat.us.Clients;
using hitmanstat.us.Models;
using hitmanstat.us.Framework;

namespace hitmanstat.us.Controllers
{
    public class ServicesController : Controller
    {
        private IMemoryCache MemoryCache;
        private readonly IHitmanClient HitmanClient;
        private readonly IHitmanForumClient HitmanForumClient;

        public ServicesController(
            IMemoryCache memoryCache, 
            IHitmanClient hitmanClient, 
            IHitmanForumClient hitmanForumClient)
        {
            MemoryCache = memoryCache;
            HitmanClient = hitmanClient;
            HitmanForumClient = hitmanForumClient;
        }

        public async Task<IActionResult> Hitman()
        {
            string responseContentType = "application/json";

            if (MemoryCache.TryGetValue(CacheKeys.HitmanKey, out ServiceStatus cachedService))
            {
                return Content(cachedService.Status, responseContentType);
            }

            try
            {
                ServiceStatus service = await HitmanClient.GetStatus();

                if (service.State == ServiceState.Up)
                {
                    MemoryCache.Set(
                        CacheKeys.HitmanKey,
                        service,
                        new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));

                    //service.Status = Utilities.ReadResourceFile(Properties.Resources.hitmandebug);
                    return Content(service.Status, responseContentType);
                }
                else
                {
                    return Json(service);
                }
            }
            catch (Exception e)
            {
                return Json(new ServiceStatus { Status = e.Message });
            }
        }

        public async Task<IActionResult> HitmanForum()
        {
            if (MemoryCache.TryGetValue(CacheKeys.HitmanForumKey, out ServiceStatus cachedService))
            {
                return Json(cachedService);
            }

            try
            {
                ServiceStatus service = await HitmanForumClient.GetStatus();

                if (service.State == ServiceState.Up)
                {
                    MemoryCache.Set(
                        CacheKeys.HitmanForumKey,
                        service,
                        new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));
                }

                return Json(service);
            }
            catch (Exception e)
            {
                return Json(new ServiceStatus { Status = e.Message });
            }
        }
    }
}