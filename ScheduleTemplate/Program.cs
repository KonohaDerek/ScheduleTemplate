using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers;
using System;

namespace ScheduleTemplate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                             .AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables()
                             .AddCommandLine(args)
                             .Build();

            Log.Logger = new LoggerConfiguration()
           .ReadFrom.Configuration(configuration)
           .Enrich.With(new ThreadIdEnricher())
           .Enrich.WithProperty("API Version", "1.0.1")
           .CreateLogger();
            try
            {
                Log.Information("Starting Serilog demo web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message, "Host terminated unexpectedy!");

            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
