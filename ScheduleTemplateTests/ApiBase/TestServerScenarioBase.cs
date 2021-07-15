using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScheduleTemplate;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScheduleTemplateTests.ApiBase
{
    [TestClass]
    public abstract class TestServerScenarioBase
    {
        protected static string AccessToken;

        internal static IConfiguration _Configuration;

        internal static ILoggerFactory _LoggerFactory;

        protected readonly JsonSerializerOptions _JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        [AssemblyInitialize]
        public static async Task TestInitializeAsync(TestContext testContext)
        {
            _Configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.Test.json")
                            .Build();

            var serviceProvider = new ServiceCollection()
                                    .AddLogging()
                                    .BuildServiceProvider();

            _LoggerFactory = serviceProvider.GetService<ILoggerFactory>();

            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                using (var server = CreateServer())
                {

                }
            }
        }

        public static TestServer CreateServer()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var path = Assembly.GetAssembly(typeof(TestServerScenarioBase))
               .Location;

            var hostBuilder = new WebHostBuilder()
                .UseContentRoot(Path.GetDirectoryName(path))
                .ConfigureAppConfiguration(cb =>
                {
                    cb.AddJsonFile("appsettings.Test.json", optional: false)
                    .AddEnvironmentVariables();
                })
              .ConfigureLogging((hostingContext, logging) =>
              {
                  logging.ClearProviders();//移除已經註冊的其他日誌處理程序
                  logging.SetMinimumLevel(LogLevel.Trace); //設定最小的日誌級別
              })
              .ConfigureTestServices(services =>
              {
                  services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
              })
                .UseStartup<TestServerStartup>()
                .UseSerilog();

            return new TestServer(hostBuilder);
        }

        /// <summary>
        /// 建立Json 呼叫HttpContent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TRequest"></param>
        /// <returns></returns>
        public static HttpContent CreateJsonRequestContent<T>(T TRequest)
        {
            var request = new StringContent(JsonSerializer.Serialize(TRequest), Encoding.UTF8, "application/json");
            return request;
        }

        /// <summary>
        /// 建立From 呼叫HttpContent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TRequest"></param>
        /// <returns></returns>
        public static HttpContent CreateFormUrlContent<T>(T TRequest)
        {
            var str = JsonSerializer.Serialize(TRequest);
            var dictionary = JsonSerializer.Deserialize<List<JsonProperty>>(str)
                                            .Select(o=>new KeyValuePair<string,string>( o.Name,o.Value.ToString()));
            var credentials = new FormUrlEncodedContent(dictionary.ToList());
            return credentials;
        }
    }
}

