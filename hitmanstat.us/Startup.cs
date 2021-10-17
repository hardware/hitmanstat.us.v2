using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.ApplicationInsights.Extensibility;
using hitmanstat.us.Clients;
using hitmanstat.us.Framework;
using hitmanstat.us.Options;
using hitmanstat.us.Data;

namespace hitmanstat.us
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Captures database-related exceptions during development
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Database context
            services.AddDbContext<DatabaseContext>(
                options => options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    providerOptions => providerOptions.EnableRetryOnFailure()));

            // HTTP Response Caching
            services.AddResponseCaching();

            // ASP.NET Core MVC middleware
            services.AddControllersWithViews()
                    .AddNewtonsoftJson();

            // Bundling and minification
            services.AddWebOptimizer(pipeline =>
            {
                /*
                 * Stylesheets
                 */

                // All pages
                pipeline.AddCssBundle("/css/bundle.css",
                    "Lib/external/twitter-bootstrap/css/bootstrap.min.css",
                    "Lib/external/toastr.js/toastr.min.css",
                    "Lib/external/mapbox-gl/mapbox-gl.min.css",
                    "Lib/local/css/site.css")
                .UseContentRoot();

                /*
                 * Javascript
                 */

                // Index page
                pipeline.AddJavaScriptBundle("/js/index.bundle.js",
                    "Lib/external/jquery/jquery.min.js",
                    "Lib/external/twitter-bootstrap/js/bootstrap.bundle.min.js",
                    "Lib/external/mithril/mithril.min.js",
                    "Lib/external/moment.js/moment.min.js",
                    "Lib/external/toastr.js/toastr.min.js",
                    "Lib/external/apexcharts/apexcharts.min.js",
                    "Lib/local/js/site.js",
                    "Lib/local/js/mithril/models/*.js",
                    "Lib/local/js/mithril/views/index/*.js")
                .UseContentRoot();

                // World map page
                pipeline.AddJavaScriptBundle("/js/map.bundle.js",
                    "Lib/external/jquery/jquery.min.js",
                    "Lib/external/twitter-bootstrap/js/bootstrap.bundle.min.js",
                    "Lib/external/mithril/mithril.min.js",
                    "Lib/local/js/site.js",
                    "Lib/local/js/mithril/views/map/*.js")
                .UseContentRoot();

                // All other pages
                pipeline.AddJavaScriptBundle("/js/catchall.bundle.js",
                    "Lib/external/jquery/jquery.min.js",
                    "Lib/external/moment.js/moment.min.js",
                    "Lib/external/twitter-bootstrap/js/bootstrap.bundle.min.js",
                    "Lib/local/js/site.js",
                    "Lib/local/js/mithril/views/events/*.js")
                .UseContentRoot();

                // This will minify any JS and CSS file that isn't part of any bundle
                pipeline.MinifyCssFiles();
                pipeline.MinifyJsFiles();
            });

            // CSRF protection
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "X-CSRF-TOKEN";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // IHttpClientFactory policies and clients
            services
                .AddPolicies(Configuration)
                .AddHttpClient<IHitmanClient, HitmanClient, HitmanClientOptions>(
                    Configuration,
                    nameof(ApplicationOptions.HitmanClient))
                .AddHttpClient<IHitmanForumClient, HitmanForumClient, HitmanForumClientOptions>(
                    Configuration,
                    nameof(ApplicationOptions.HitmanForumClient));

            // Memory caching
            services.AddMemoryCache();

            // Status seekers Hosted services
            services.AddHostedService<HitmanStatusSeekerHostedService>();
            services.AddHostedService<HitmanForumStatusSeekerHostedService>();
            services.AddHostedService<HitmanReportStatusSeekerHostedService>();

            // Enable Application Insights Telemetry
            services.AddApplicationInsightsTelemetry();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TelemetryConfiguration configuration)
        {
            app.UseWebOptimizer();

            var cultureInfo = new CultureInfo("en-US");

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor,
                ForwardedForHeaderName = "CF-CONNECTING-IP"
            };

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            app.UseForwardedHeaders(options);

            app.UseDeveloperExceptionPage();

            if (env.IsDevelopment())
            {
                app.UseMigrationsEndPoint();

                configuration.DisableTelemetry = true;
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("Referrer-Policy", "no-referrer");
                //context.Response.Headers.Add("Feature-Policy", "fullscreen 'self';camera 'none';geolocation 'self';gyroscope 'none';magnetometer 'none';microphone 'none';midi 'none';payment 'none';speaker 'none';sync-xhr 'none'");
                //context.Response.Headers.Add("Content-Security-Policy", "default-src 'none'; script-src 'self' 'unsafe-inline' *.msecnd.net www.google.com www.gstatic.com; style-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-src 'self' www.google.com; font-src 'self'; media-src 'self'; connect-src 'self' *.visualstudio.com *.mapbox.com; manifest-src 'self'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'");
                await next();
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=2592000";
                }
            });

            app.UseHttpsRedirection();
            app.UseResponseCaching();
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
