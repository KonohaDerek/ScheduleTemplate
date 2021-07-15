using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ScheduleTemplate.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace ScheduleTemplate.Middlewares
{
    public class JobServiceMiddleware
    {
        private readonly RequestDelegate next;

        private readonly IServiceProvider provider;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool IsInit { get; set; }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="next"></param>
        /// <param name="appsettings"></param>
        /// <param name="localizer"></param>
        public JobServiceMiddleware(RequestDelegate next, IServiceProvider provider)
        {
            this.next = next;
            this.provider = provider;
        }

        /// <summary>
        /// 動作處理
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (!IsInit)
            {
                using (var connection = JobStorage.Current.GetConnection())
                {
                    foreach (var recurringJob in connection.GetRecurringJobs())
                    {
                        RecurringJob.RemoveIfExists(recurringJob.Id);
                    }
                }
                var jobAsmes = Assembly.GetAssembly(typeof(IJobService));
                var jobItems = from type in jobAsmes.GetTypes()
                               where type.IsSubclassOf(typeof(IJobService))
                               let jobObj = (IJobService)ActivatorUtilities.CreateInstance(provider.CreateScope().ServiceProvider, type)
                               select jobObj;
                foreach (var item in jobItems)
                {
                    var index = 0;
                    foreach (var cron in item.Crons)
                    {
                        if (item.Enabled)
                        {
                            var tzi = TZConvert.GetTimeZoneInfo("Asia/Taipei");
                            RecurringJob.AddOrUpdate($"{item.Name}-{index}", () => item.Init(null), cron, tzi);
                        }
                        else
                        {
                            RecurringJob.RemoveIfExists($"{item.Name}-{index}");
                        }
                        index++;
                    }
                }
                IsInit = true;
            }
            await next(context);
        }
    }

    /// <summary>
    /// JobServiceMiddleware 擴充方法
    /// </summary>
    public static class JobServiceMiddlewareExtensions
    {
        /// <summary>
        /// 加入使用排程服務
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseJobService(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JobServiceMiddleware>();
        }
    }
}
