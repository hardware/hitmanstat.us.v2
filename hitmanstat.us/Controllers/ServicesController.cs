using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration Configuration;
        private readonly EventManager EventManager;

        public ServicesController(IConfiguration configuration, IMemoryCache memoryCache, 
            IHitmanClient hitmanClient, IHitmanForumClient hitmanForumClient)
        {
            Configuration = configuration;
            MemoryCache = memoryCache;
            HitmanClient = hitmanClient;
            HitmanForumClient = hitmanForumClient;

            EventManager = new EventManager(
                Configuration.GetConnectionString("HitmanstatusDB"),
                Configuration.GetValue<string>("EventsTableName")
            );
        }

        public async Task<IActionResult> Hitman()
        {
            string responseContentType = "application/json";

            if (MemoryCache.TryGetValue(CacheKeys.HitmanKey, out EndpointStatus cachedEndpoint))
            {
                return Content(cachedEndpoint.Status, responseContentType);
            }

            var endpointException = new EndpointStatusException("HITMAN AUTHENTICATION");

            try
            {
                EndpointStatus endpoint = await HitmanClient.GetStatusAsync();

                if (endpoint.State == EndpointState.Up)
                {
                    MemoryCache.Set(
                        CacheKeys.HitmanKey,
                        endpoint,
                        new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));

                    // Fake data (debug purpose)
                    // endpoint.Status = Utilities.ReadResourceFile(Properties.Resources.hitmandebug);

                    _ = EventManager.InsertHitmanServicesEntitiesAsync(endpoint.Status);
                    return Content(endpoint.Status, responseContentType);
                }
                else
                {
                    endpointException.Status = endpoint.Status;
                    endpointException.State = endpoint.State;
                }
            }
            catch (Exception e)
            {
                endpointException.Message = e.Message;
            }

            _ = EventManager.InsertEndpointExceptionAsync(endpointException);
            return Json(endpointException);
        }

        public async Task<IActionResult> HitmanForum()
        {
            if (MemoryCache.TryGetValue(CacheKeys.HitmanForumKey, out EndpointStatus cachedEndpoint))
            {
                return Json(cachedEndpoint);
            }

            var endpointException = new EndpointStatusException("HITMAN FORUM");

            try
            {
                EndpointStatus endpoint = await HitmanForumClient.GetStatusAsync();

                if (endpoint.State == EndpointState.Up)
                {
                    MemoryCache.Set(
                        CacheKeys.HitmanForumKey,
                        endpoint,
                        new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));

                    return Json(endpoint);
                }
                else
                {
                    endpointException.Status = endpoint.Status;
                }
            }
            catch (Exception e)
            {
                endpointException.Message = e.Message;
            }

            _ = EventManager.InsertEndpointExceptionAsync(endpointException);
            return Json(endpointException);
        }
    }
}