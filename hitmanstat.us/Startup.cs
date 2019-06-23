using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
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
            // Database context
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // HTTP Response Caching
            services.AddResponseCaching();

            // ASP.NET Core MVC middleware
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                //ForwardedForHeaderName = "CF-Connecting-IP",
                ForwardedHeaders = ForwardedHeaders.All
            }); ;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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
                context.Response.Headers.Add("Feature-Policy", "fullscreen 'self';camera 'none';geolocation 'none';gyroscope 'none';magnetometer 'none';microphone 'none';midi 'none';payment 'none';speaker 'none';sync-xhr 'none'");
                context.Response.Headers.Add("Content-Security-Policy", "default-src 'none'; script-src 'self' 'unsafe-inline' *.msecnd.net; style-src 'self'; img-src 'self' data:; frame-src 'self'; font-src 'self'; media-src 'self'; connect-src 'self' *.visualstudio.com; manifest-src 'self'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'");
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
            app.UseMvcWithDefaultRoute();
        }
    }
}
