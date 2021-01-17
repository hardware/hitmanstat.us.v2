using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using hitmanstat.us.Models;
using hitmanstat.us.Data;

namespace hitmanstat.us.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly IMemoryCache _cache;

        public HomeController(DatabaseContext context, IMemoryCache cache)
        {
            _db = context;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/map")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Map()
        {
            return View();
        }

        [Route("/about")]
        [ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any)]
        public IActionResult About()
        {
            return View();
        }

        [Route("/events")]
        [Route("/events/{days:int:range(1,7)}")]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "days" })]
        public async Task<IActionResult> Events(int days = 1)
        {
            ViewBag.days = days;

            var cacheKey = $"_HitmanEventsList-Days-{days}";

            if (_cache.TryGetValue(cacheKey, out List<Event> events))
            {
                return View(events);
            }

            events = await (from e in _db.Events where e.Date > DateTime.Now.AddDays(-days) select e)
                .OrderByDescending(e => e.ID)
                .AsNoTracking()
                .ToListAsync();

            events.RemoveAll(e => e.Service.Contains("FORUM"));

            _cache.Set(cacheKey, events, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));

            return View(events);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewBag.mainError = true;

            return View(new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [Route("error/{code:int}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int code)
        {
            ViewBag.mainError = true;

            return View(new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorCode = code
            });
        }
    }
}
