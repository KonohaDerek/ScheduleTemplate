using Hangfire.Console;
using ScheduleTemplate.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScheduleTemplate.Jobs
{
    public class JobTest : IJobService
    {
        public override string Name => "排程測試";

        public override string Cron => "* * * * *";

        public override bool Enabled => false;

        public override string[] Crons => new string[] {
            "* * * * *",
            //"* * * * *",
            //"* * * * *",
            //"* * * * *",
            //"* * * * *",
            //"* * * * *"
        };

        public override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _PerformContext.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 開始");
            _PerformContext.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 結束");
            return Task.CompletedTask;
        }
    }
}
