using System;
using System.IO;
using System.Threading.Tasks;
using Gestalt.Common.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Gestalt.Common.Services;
using Gestalt.Utilites.DataExtractor.Interfaces;
using Gestalt.Utilites.DataExtractor.Models;
using Gestalt.Utilites.DataExtractor.Services;

namespace Gestalt.Utilites.DataExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("consoleapp.log")
                .CreateLogger();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(configure => configure.AddSerilog())
                .AddTransient<IDataService, DataService>()
                .AddTransient<IPopularControl, PopularControl>()
                .AddTransient<IPopularControlSavingService, RequestSavingService>()
                .AddTransient<IPopularControlHttpClient, PopularControlHttpClient>()
                .AddMongo(configuration);

            var isFirstLoadConfig =
                configuration["Environment:IS_FIRST_LOAD"]
                ?? Environment.GetEnvironmentVariable("IS_FIRST_LOAD")
                ?? "false";
            bool.TryParse(isFirstLoadConfig, out var isNeedSaveRequests);

            if (isNeedSaveRequests)
            {
                serviceCollection.AddTransient<IPopularControlSavingService, RequestSavingService>();
            }
            else
            {
                serviceCollection.AddTransient<IPopularControlSavingService, ResponseListEntitySavingService>();
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger.LogInformation("Starting application");

            // do the actual work here
            var task = Task.Run(() => serviceProvider.GetService<IDataService>().WriteToFileFromApi());
            task.Wait();
            logger.LogInformation("All done!");
        }
    }
}