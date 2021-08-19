using Hangfire;
using Hangfire.Console;
using Hangfire.MemoryStorage;
using Prometheus;
using ScheduleTemplate.Filters;
using ScheduleTemplate.Middlewares;
using Serilog;
using Serilog.Enrichers;
using Microsoft.OpenApi.Models;

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

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddHealthChecks();
    builder.Services.AddHangfire(x =>
    {
        x.UseMemoryStorage(new MemoryStorageOptions()
        {
            FetchNextJobTimeout = TimeSpan.FromHours(1),
            JobExpirationCheckInterval = TimeSpan.FromHours(1)
        });
        x.UseConsole();
    });

    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "ScheduleTemplate", Version = "v1" });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ScheduleTemplate v1"));
    }

    app.UseRouting();


    //app.UseHttpsRedirection();

    //app.UseAuthorization();

    // 加入JobService
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


   
    // 加入hangfire Dashboard
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new AllowAllAuthorizationFilter() }
    });

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapMetrics();
        endpoints.MapHealthChecks("/health");
        endpoints.MapControllers();
    });
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex.Message, "Host terminated unexpectedy!");

}
finally
{
    Log.CloseAndFlush();
}


