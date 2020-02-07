using System;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        private IMemoryCache _cache;

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
            if (_cache.TryGetValue(CacheKeys.HitmanChartKey, out Chart cachedChart))
            {
                return Json(cachedChart);
            }

            var counters = await (from c in _db.UserReportCounters
                            where c.Date > DateTime.Now.AddDays(-7)
                            select c).AsNoTracking().ToListAsync();

            var categories = new List<string>();
            var series = new List<ChartSerie>();

            var h1pc = new List<int>();
            var h1xb = new List<int>();
            var h1ps = new List<int>();
            var h2pc = new List<int>();
            var h2xb = new List<int>();
            var h2ps = new List<int>();

            foreach (var counter in counters)
            {
                categories.Add(counter.Date.ToString("MMM d"));
                h1pc.Add(counter.H1pc);
                h1xb.Add(counter.H1xb);
                h1ps.Add(counter.H1ps);
                h2pc.Add(counter.H2pc);
                h2xb.Add(counter.H2xb);
                h2ps.Add(counter.H2ps);
            }

            series.Add(new ChartSerie { Name = "HITMAN PC", Data = h1pc });
            series.Add(new ChartSerie { Name = "HITMAN XBOX ONE", Data = h1xb });
            series.Add(new ChartSerie { Name = "HITMAN PS4", Data = h1ps });
            series.Add(new ChartSerie { Name = "HITMAN 2 PC", Data = h2pc });
            series.Add(new ChartSerie { Name = "HITMAN 2 XBOX ONE", Data = h2xb });
            series.Add(new ChartSerie { Name = "HITMAN 2 PS4", Data = h2ps });

            var chart = new Chart
            {
                Categories = categories,
                Series = series
            };

            _cache.Set(CacheKeys.HitmanChartKey, chart, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));

            return Json(chart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReport(string reference, string fingerprint, string state)
        {
            string service;

            var badRequest = new { 
                type = "error", 
                message = "Invalid submitted data."
            };

            var maintenance = new { 
                type = "warning", 
                message = "This service is currently under maintenance, reporting is disabled."
            };

            if (reference == null || fingerprint == null || state == null || !Regex.Match(fingerprint, @"^[a-z0-9]{32}$").Success)
            {
                return Json(badRequest);
            }

            switch (reference)
            {
                case "h1pc": service = "HITMAN PC"; break;
                case "h1xb": service = "HITMAN XBOX ONE"; break;
                case "h1ps": service = "HITMAN PS4"; break;
                case "h2pc": service = "HITMAN 2 PC"; break;
                case "h2xb": service = "HITMAN 2 XBOX ONE"; break;
                case "h2ps": service = "HITMAN 2 PS4"; break;
                default:
                    return Json(badRequest);
            }

            if (_cache.TryGetValue(CacheKeys.HitmanKey, out EndpointStatus cachedEndpoint))
            {
                var json = JObject.Parse(cachedEndpoint.Status);
                var services = Utilities.ParseHitmanServicesEntities(json);
                var refService = services.Where(service => service.Ref == reference).First();

                if(refService.Health == HitmanServiceHealth.Maintenance)
                {
                    return Json(maintenance);
                }
            }
            else
            {
                if (state == HitmanServiceHealth.Maintenance.GetAttribute<DisplayAttribute>().Name)
                {
                    return Json(maintenance);
                }
            }

            IPAddress address = Request.HttpContext.Connection.RemoteIpAddress;

            var count = await (from r in _db.UserReports
                         where (r.IPAddressBytes == address.GetAddressBytes() 
                         || r.Fingerprint == fingerprint)
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
                    Fingerprint = fingerprint,
                    Service = service
                });

                var today = await _db.UserReportCounters.SingleOrDefaultAsync(c => c.Date.Date == DateTime.Today);

                if (today != null)
                {
                    switch (reference)
                    {
                        case "h1pc": today.H1pc++; break;
                        case "h1xb": today.H1xb++; break;
                        case "h1ps": today.H1ps++; break;
                        case "h2pc": today.H2pc++; break;
                        case "h2xb": today.H2xb++; break;
                        case "h2ps": today.H2ps++; break;
                    }
                }
                else
                {
                    _db.Add(new UserReportCounter
                    {
                        H1pc = reference == "h1pc" ? 1 : 0,
                        H1xb = reference == "h1xb" ? 1 : 0,
                        H1ps = reference == "h1ps" ? 1 : 0,
                        H2pc = reference == "h2pc" ? 1 : 0,
                        H2xb = reference == "h2xb" ? 1 : 0,
                        H2ps = reference == "h2ps" ? 1 : 0,
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
    }
}
