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
        private IMemoryCache _cache;
        private readonly IHitmanClient _hitmanClient;
        private readonly IHitmanForumClient _hitmanForumClient;

        public ServicesController(IMemoryCache cache, IHitmanClient hmClient, IHitmanForumClient hmfClient)
        {
            _cache = cache;
            _hitmanClient = hmClient;
            _hitmanForumClient = hmfClient;
        }

        public async Task<IActionResult> Hitman()
        {
            // Get cached result from the background service
            if (_cache.TryGetValue(CacheKeys.HitmanKey, out EndpointStatus cachedEndpoint))
            {
                return Content(cachedEndpoint.Status, "application/json");
            }

            var endpointException = new EndpointStatusException("HITMAN AUTHENTICATION");

            try
            {
                EndpointStatus endpoint = await _hitmanClient.GetStatusAsync();

                if (endpoint.State == EndpointState.Up)
                {
                    return Content(endpoint.Status, "application/json");
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

            return Json(endpointException);
        }

        public async Task<IActionResult> HitmanForum()
        {
            // Get cached result from the background service
            if (_cache.TryGetValue(CacheKeys.HitmanForumKey, out EndpointStatus cachedEndpoint))
            {
                return Json(cachedEndpoint);
            }

            var endpointException = new EndpointStatusException("HITMAN FORUM");

            try
            {
                EndpointStatus endpoint = await _hitmanForumClient.GetStatusAsync();

                if (endpoint.State == EndpointState.Up)
                {
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

            return Json(endpointException);
        }
    }
}