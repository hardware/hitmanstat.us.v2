using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Data;
using hitmanstat.us.Models;
using hitmanstat.us.Framework;

namespace hitmanstat.us.Controllers
{
    public class UserReportsController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly IMemoryCache _cache;

        public UserReportsController(DatabaseContext context, IMemoryCache cache)
        {
            _db = context;
            _cache = cache;
        }

        [Route("/ip")]
        public IActionResult Ip()
        {
            IPAddress address = Request.HttpContext.Connection.RemoteIpAddress;

            return Json(new
            {
                addr = address.ToString(),
                bytes = address.GetAddressBytes()
            });
        }

        [Route("/reports")]
        [Route("/UserReports/GetReports")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetReports()
        {
            while (true)
            {
                if (_cache.TryGetValue(CacheKeys.HitmanChartKey, out Chart cachedChart))
                {
                    return Json(cachedChart);
                }

                await Task.Delay(1000);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReport([FromForm]SubmitReport model)
        {
            if (_cache.TryGetValue(CacheKeys.ReportBurnout, out int cachedChart))
            {
                return Json(new
                {
                    type = "warning",
                    message = "A large number of reports have been detected, slow mode enabled for 1 hour. The amount of reports is high enough to have a reliable overview."
                });
            }

            var latestCount = await (from r in _db.UserReports
                               where (r.Date > DateTime.Now.AddHours(-1))
                               select r).AsNoTracking().CountAsync();

            if(latestCount >= 1000)
            {
                _cache.Set(CacheKeys.ReportBurnout, 1, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1)));
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    type = "error",
                    message = "The submission failed, you sent invalid data."
                });
            }

            var address = Request.HttpContext.Connection.RemoteIpAddress;

            if (!ReCaptcha.Validate(model.RecaptchaToken, address))
            {
                return Json(new
                {
                    type = "error",
                    message = "The submission failed the spam bot verification. Are you human ?"
                });
            }

            var maintenance = new
            {
                type = "warning",
                message = "This service is currently under maintenance, reporting is disabled."
            };

            if (_cache.TryGetValue(CacheKeys.HitmanKey, out EndpointStatus cachedEndpoint))
            {
                var json = JObject.Parse(cachedEndpoint.Status);
                var services = Utilities.ParseHitmanServicesEntities(json);
                var refService = services.Where(service => service.Ref == model.Reference).First();

                if(refService.Health == HitmanServiceHealth.Maintenance)
                {
                    return Json(maintenance);
                }
            }
            else
            {
                if (model.State == HitmanServiceHealth.Maintenance.GetAttribute<DisplayAttribute>().Name)
                {
                    return Json(maintenance);
                }
            }

            var count = await (from r in _db.UserReports
                         where (r.IPAddressBytes == address.GetAddressBytes() 
                         || r.Fingerprint == model.Fingerprint)
                         && r.Date > DateTime.Now.AddHours(-1)
                         select r).AsNoTracking().CountAsync();

            if (count > 0)
            {
                return Json(new { 
                    type = "info", 
                    message = "You can not submit more than once. Please wait before submitting your next report." 
                });
            }

            try
            {
                _db.Add(new UserReport
                {
                    IPAddress = address,
                    Fingerprint = model.Fingerprint,
                    Service = GetServiceName(model.Reference),
                    Latitude = model.Latitude,
                    Longitude = model.Longitude
                });

                var today = await _db.UserReportCounters.SingleOrDefaultAsync(c => c.Date.Date == DateTime.Today);

                if (today != null)
                {
                    switch (model.Reference)
                    {
                        case "h1pc": today.H1pc++; break;
                        case "h1xb": today.H1xb++; break;
                        case "h1ps": today.H1ps++; break;

                        case "h2pc": today.H2pc++; break;
                        case "h2xb": today.H2xb++; break;
                        case "h2ps": today.H2ps++; break;

                        case "h3pc": today.H3pc++; break;
                        case "h3xb": today.H3xb++; break;
                        case "h3ps": today.H3ps++; break;
                        case "h3st": today.H3st++; break;
                        case "h3sw": today.H3sw++; break;
                    }
                }
                else
                {
                    _db.Add(new UserReportCounter
                    {
                        H1pc = model.Reference == "h1pc" ? 1 : 0,
                        H1xb = model.Reference == "h1xb" ? 1 : 0,
                        H1ps = model.Reference == "h1ps" ? 1 : 0,

                        H2pc = model.Reference == "h2pc" ? 1 : 0,
                        H2xb = model.Reference == "h2xb" ? 1 : 0,
                        H2ps = model.Reference == "h2ps" ? 1 : 0,

                        H3pc = model.Reference == "h3pc" ? 1 : 0,
                        H3xb = model.Reference == "h3xb" ? 1 : 0,
                        H3ps = model.Reference == "h3ps" ? 1 : 0,
                        H3st = model.Reference == "h3st" ? 1 : 0,
                        H3sw = model.Reference == "h3sw" ? 1 : 0,
                    });
                }

                await _db.SaveChangesAsync();

                return Json(new { type = "success" });
            }
            catch(Exception e)
            {
                return Json(new { type = "error", message = e.Message });
            }
        }

        private static string GetServiceName(string reference)
        {
            string name = null;

            switch (reference)
            {
                // HITMAN 1
                case "h1pc":
                    name = "HITMAN PC";
                    break;
                case "h1xb":
                    name = "HITMAN XBOX ONE";
                    break;
                case "h1ps":
                    name = "HITMAN PS4";
                    break;
                // HITMAN 2
                case "h2pc":
                    name = "HITMAN 2 PC";
                    break;
                case "h2xb":
                    name = "HITMAN 2 XBOX ONE";
                    break;
                case "h2ps":
                    name = "HITMAN 2 PS4";
                    break;
                // HITMAN 3
                case "h3pc":
                    name = "HITMAN 3 PC";
                    break;
                case "h3xb":
                    name = "HITMAN 3 XBOX";
                    break;
                case "h3ps":
                    name = "HITMAN 3 PLAYSTATION";
                    break;
                case "h3st":
                    name = "HITMAN 3 STADIA";
                    break;
                case "h3sw":
                    name = "HITMAN 3 SWITCH";
                    break;
            }

            return name;
        }
    }
}
