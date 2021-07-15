using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using ScheduleTemplate.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ScheduleTemplate.Interfaces
{
    public abstract class IJobService
    {
        /// <summary>
        /// Logger 紀錄器
        /// </summary>
        protected readonly ILogger _logger;

        public IJobService()
        {
            _logger = ApplicationLogging.CreateLogger(GetType().Name);
        }

        protected PerformContext _PerformContext;

        /// <summary>
        /// 名稱
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 時間設定
        /// </summary>
        public abstract string Cron { get; }

        /// <summary>
        /// 時間設定群組
        /// </summary>
        public virtual string[] Crons => new string[] { Cron };

        /// <summary>
        /// 啟用
        /// </summary>
        public virtual bool Enabled => true;


        /// <summary>
        /// 初始化工作
        /// </summary>
        /// <param name="Configuration"></param>
        [AutomaticRetry(Attempts = 0, LogEvents = false)]
        public Task Init(PerformContext performContext)
        {
            _PerformContext = performContext;
            return Task.Run(async () =>
            {
                try
                {
                    await StartAsync(new CancellationToken());
                    await ExecuteAsync(new CancellationToken());
                }
                catch (Exception ex)
                {
                    var message = GetExceptionInnerMessage(ex);
                    _logger.LogError(ex, $"{GetType().Name} 發生錯誤 ， Message : {message}");
                    throw;
                }
                finally
                {
                    await StopAsync(new CancellationToken());
                }
            });
        }


        /// <summary>
        /// 在排程執行之前處理項目
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _PerformContext.WriteLine(ConsoleTextColor.Cyan, $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {Name} Running ...");
            return Task.CompletedTask;
        }

        public abstract Task ExecuteAsync(CancellationToken cancellationToken);


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _PerformContext.WriteLine(ConsoleTextColor.Cyan, $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {Name} Run End ...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 取得錯誤訊息內容
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetExceptionInnerMessage(Exception ex)
        {
            if (ex.InnerException is null)
            {
                return ex.Message;
            }
            else
            {
                return string.Join(",", ex.Message, GetExceptionInnerMessage(ex.InnerException));
            }
        }
    }
}
