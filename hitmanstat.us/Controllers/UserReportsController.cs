﻿using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using hitmanstat.us.Clients;
using hitmanstat.us.Data;
using hitmanstat.us.Models;
using hitmanstat.us.Framework;

namespace hitmanstat.us.Controllers
{
    public class UserReportsController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly IMemoryCache _cache;
        private readonly IRecaptchaClient _captcha;

        public UserReportsController(DatabaseContext context, IMemoryCache cache, IRecaptchaClient client)
        {
            _db = context;
            _cache = cache;
            _captcha = client;
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
                    message = "A large number of reports have been detected, slow mode enabled for 2 minutes. Please wait."
                });
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

            if (!await _captcha.Validate(model.RecaptchaToken, address))
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

            var lastMinuteCount = await (from r in _db.UserReports
                                     where (r.Date > DateTime.Now.AddMinutes(-1))
                                     select r).AsNoTracking().CountAsync();

            if (lastMinuteCount >= 50)
            {
                _cache.Set(CacheKeys.ReportBurnout, 1, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
            }

            var userCount = await (from r in _db.UserReports
                         where (r.IPAddressBytes == address.GetAddressBytes() 
                         || r.Fingerprint == model.Fingerprint)
                         && r.Date > DateTime.Now.AddHours(-1)
                         select r).AsNoTracking().CountAsync();

            if (userCount > 0)
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
                        case "h3steam": today.H3steam++; break;
                        case "h3epic": today.H3epic++; break;
                        case "h3xb": today.H3xb++; break;
                        case "h3ps": today.H3ps++; break;
                        case "h3sw": today.H3sw++; break;
                    }
                }
                else
                {
                    _db.Add(new UserReportCounter
                    {
                        H3steam = model.Reference == "h3steam" ? 1 : 0,
                        H3epic = model.Reference == "h3epic" ? 1 : 0,
                        H3xb = model.Reference == "h3xb" ? 1 : 0,
                        H3ps = model.Reference == "h3ps" ? 1 : 0,
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
                case "h3steam":
                    name = "HITMAN STEAM";
                    break;
                case "h3epic":
                    name = "HITMAN EPIC";
                    break;
                case "h3xb":
                    name = "HITMAN XBOX";
                    break;
                case "h3ps":
                    name = "HITMAN PLAYSTATION";
                    break;
                case "h3sw":
                    name = "HITMAN SWITCH";
                    break;
            }

            return name;
        }
    }
}
