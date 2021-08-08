using Hangfire;
using Hangfire.Console;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using ScheduleTemplate.Filters;
using ScheduleTemplate.Middlewares;
using ScheduleTemplate.Util;
using System;

namespace ScheduleTemplate
{
    public class Startup
    {

        public static IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
           // ApplicationLogging.LoggerFactory = logFactory;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddHangfire(x =>
            {
                x.UseMemoryStorage(new MemoryStorageOptions()
                {
                    FetchNextJobTimeout = TimeSpan.FromHours(1),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1)
                });
                x.UseConsole();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //�[�J�Ƶ{�u�@
            app.UseJobService();

            app.UseHangfireServer();

            app.Use((context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/hangfire"))
                {
                    context.Request.PathBase = new PathString(context.Request.Headers["X-Forwarded-Prefix"]);
                }
                return next();
            });

            app.UseRouting();

            // �ҥ�Hangfire��Dashboard
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AllowAllAuthorizationFilter() }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
