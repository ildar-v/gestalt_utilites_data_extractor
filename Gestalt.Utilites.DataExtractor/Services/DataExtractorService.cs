namespace Gestalt.Utilites.DataExtractor.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Gestalt.Common.Exceptions;
    using Gestalt.Common.Models;
    using Gestalt.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class DataExtractorService : IDataExtractorService
    {
        private readonly ILogger<DataExtractorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPopularControlExtractor _popularControlExtractor;

        public DataExtractorService(ILogger<DataExtractorService> logger, IConfiguration configuration,
            IPopularControlExtractor popularControlExtractor)
        {
            _logger = logger;
            _configuration = configuration;
            _popularControlExtractor = popularControlExtractor;
        }

        public async Task WriteToFileFromApi()
        {
            await _popularControlExtractor.ExtractData();

            Console.WriteLine("Data saved successfully.");
        }

        public async Task ReadFromFile()
        {
            var jsonData = await File.ReadAllTextAsync(_configuration["FilesPath"]
                                                       ?? throw new NoConfigurationValueException("FilesPath"));
            var result = JsonConvert.DeserializeObject<MainResponse>(jsonData);
            _logger.LogInformation(JsonConvert.ToString(result));
        }
    }
}