using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using hitmanstat.us.Models;
using hitmanstat.us.Data;

namespace hitmanstat.us.Framework
{
    internal class HitmanReportStatusSeekerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;

        public HitmanReportStatusSeekerHostedService(
            ILogger<HitmanReportStatusSeekerHostedService> logger, 
            IServiceScopeFactory scopeFactory, 
            IMemoryCache cache)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("HitmanReportStatusSeekerHostedService is starting");

            stoppingToken.Register(() =>
                _logger.LogDebug("HitmanReportStatusSeekerHostedService has been canceled"));

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("HitmanReportStatusSeekerHostedService is running");

                // Get chart data
                // ---------------------------------------------------------------------------------------

                var counters = await (from c in db.UserReportCounters
                                      where c.Date > DateTime.Now.AddDays(-7)
                                      select c)
                                      .AsNoTracking()
                                      .ToListAsync(cancellationToken: stoppingToken);

                var categories = new List<string>();
                var series = new List<ChartSerie>();

                var h1pc = new List<int>();
                var h1xb = new List<int>();
                var h1ps = new List<int>();

                var h2pc = new List<int>();
                var h2xb = new List<int>();
                var h2ps = new List<int>();

                var h3pc = new List<int>();
                var h3xb = new List<int>();
                var h3ps = new List<int>();
                var h3st = new List<int>();
                var h3sw = new List<int>();

                var thre = new List<int>();

                foreach (var counter in counters)
                {
                    categories.Add(counter.Date.ToString("MMM d"));

                    h1pc.Add(counter.H1pc);
                    h1xb.Add(counter.H1xb);
                    h1ps.Add(counter.H1ps);

                    h2pc.Add(counter.H2pc);
                    h2xb.Add(counter.H2xb);
                    h2ps.Add(counter.H2ps);

                    h3pc.Add(counter.H3pc);
                    h3xb.Add(counter.H3xb);
                    h3ps.Add(counter.H3ps);
                    h3st.Add(counter.H3st);
                    h3sw.Add(counter.H3sw);

                    thre.Add(100);
                }

                series.Add(new ChartSerie { Name = "HITMAN PC", Data = h1pc });
                series.Add(new ChartSerie { Name = "HITMAN XBOX ONE", Data = h1xb });
                series.Add(new ChartSerie { Name = "HITMAN PS4", Data = h1ps });

                series.Add(new ChartSerie { Name = "HITMAN 2 PC", Data = h2pc });
                series.Add(new ChartSerie { Name = "HITMAN 2 XBOX ONE", Data = h2xb });
                series.Add(new ChartSerie { Name = "HITMAN 2 PS4", Data = h2ps });

                series.Add(new ChartSerie { Name = "HITMAN 3 PC", Data = h3pc });
                series.Add(new ChartSerie { Name = "HITMAN 3 XBOX", Data = h3xb });
                series.Add(new ChartSerie { Name = "HITMAN 3 PLAYSTATION", Data = h3ps });
                series.Add(new ChartSerie { Name = "HITMAN 3 STADIA", Data = h3st });
                series.Add(new ChartSerie { Name = "HITMAN 3 SWITCH", Data = h3sw });

                series.Add(new ChartSerie { Name = "THRESHOLD", Data = thre });

                // Get heatmap data
                // ---------------------------------------------------------------------------------------

                var reports = await (from c in db.UserReports
                                     where c.Date > DateTime.Now.AddDays(-1)
                                     select c)
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken: stoppingToken);

                var model = new FeatureCollection();

                foreach (var report in reports)
                {
                    if (report.Latitude != 0 && report.Longitude != 0)
                    {
                        var position = new Position(report.Latitude, report.Longitude);
                        var point = new Point(position);
                        var feature = new Feature(point);

                        model.Features.Add(feature);
                    }
                }

                var geoData = JsonConvert.SerializeObject(model);

                var chart = new Chart
                {
                    Categories = categories,
                    Series = series,
                    GeoData = geoData
                };

                _cache.Set(CacheKeys.HitmanChartKey, chart, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));

                await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);
            }

            _logger.LogDebug("HitmanReportStatusSeekerHostedService has been stopped");
        }
    }
}
