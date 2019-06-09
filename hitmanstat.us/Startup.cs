using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddResponseCaching();
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services
                .AddPolicies(Configuration)
                .AddHttpClient<IHitmanClient, HitmanClient, HitmanClientOptions>(
                    Configuration,
                    nameof(ApplicationOptions.HitmanClient))
                .AddHttpClient<IHitmanForumClient, HitmanForumClient, HitmanForumClientOptions>(
                    Configuration,
                    nameof(ApplicationOptions.HitmanForumClient));
            services.AddMemoryCache();
            services.AddHostedService<HitmanStatusSeekerHostedService>();
            services.AddHostedService<HitmanForumStatusSeekerHostedService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseResponseCaching();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
