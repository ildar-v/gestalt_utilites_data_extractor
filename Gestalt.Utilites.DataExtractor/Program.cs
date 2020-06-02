namespace Gestalt.Utilites.DataExtractor
{
    using System.IO;
    using System.Threading.Tasks;
    using Gestalt.Common.DAL;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Gestalt.Common.Services;
    using Gestalt.Utilites.DataExtractor.Services;

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
                .AddTransient<IRequestsService, RequestsService>()
                .AddTransient<IDataExtractorService, DataExtractorService>()
                .AddTransient<IMainResponseSaverService, MainResponseSaverService>()
                .AddTransient<IPopularControlExtractor, PopularControlExtractor>()
                .AddMongo(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger.LogInformation("Starting application");

            // do the actual work here
            var dataExtractorService = serviceProvider.GetService<IDataExtractorService>();
            var task = Task.Run(() => dataExtractorService.WriteToFileFromApi());
            task.Wait();
            logger.LogInformation("All done!");
        }
    }
}