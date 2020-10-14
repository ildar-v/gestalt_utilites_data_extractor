using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Gestalt.Common.Extensions;
using Gestalt.Common.Models;
using Gestalt.Common.Services;
using Gestalt.Utilites.DataExtractor.Interfaces;
using Gestalt.Utilites.DataExtractor.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Gestalt.Utilites.DataExtractor.Services
{
    /// <summary>
    /// Class of integration with "Народный контроль" system
    /// Класс для работы с системой "Народный контроль" (Казань)
    /// </summary>
    public class PopularControl : IPopularControl
    {
        private readonly ILogger<PopularControl> _logger;
        private readonly IPopularControlSavingService _popularControlSavingService;
        private readonly IPopularControlHttpClient _popularControlHttpClient;
        private readonly int _entriesPerPage = 100;
        private readonly int _fromPageNumber = 1;
        private int _pageCount = 10;

        public PopularControl(
            ILogger<PopularControl> logger,
            IPopularControlSavingService popularControlSavingService,
            IPopularControlHttpClient popularControlHttpClient)
        {
            _logger = logger;
            _popularControlSavingService = popularControlSavingService;
            _popularControlHttpClient = popularControlHttpClient;
        }

        /// <summary>
        /// Extracts data from "Народный контроль" system and puts it in the storage.
        /// </summary>
        /// <returns></returns>
        public async Task ExtractData()
        {
            for (var pageNumber = this._fromPageNumber; pageNumber <= this._pageCount; pageNumber++)
            {
                await this.LoadAndSaveData(pageNumber);
            }

            _logger.LogInformation("__________ Data loading completed __________");
        }

        private async Task LoadAndSaveData(int pageNumber)
        {
            _logger.LogInformation($"Starting loading page {pageNumber} at {DateTime.Now}");
            var sw = Stopwatch.StartNew();
            var responseEntry = await this.GetRequestList(pageNumber);
            if (responseEntry == null)
            {
                _logger.LogInformation($"An error while data receiving. Page {pageNumber} at {DateTime.Now}");
                return;
            }

            sw.Stop();

            _logger.LogInformation(
                $"__________ Loading page {pageNumber} ended at {DateTime.Now}, time elapsed = {sw.DetailedTimeString()}");

            this._pageCount = int.Parse(responseEntry.count) / this._entriesPerPage + 1;

            await _popularControlSavingService.SaveAsync(responseEntry);

            _logger.LogInformation($"---------- Page {pageNumber} saved at {DateTime.Now}");
        }

        private async Task<RequestList> GetRequestList(int pageNumber)
        {
            var requestListParameters = new RequestListParameters
            {
                PageNumber = pageNumber,
                EntriesPerPage = this._entriesPerPage
            };

            using var httpResponse =
                await _popularControlHttpClient.GetAsync(requestListParameters, HttpCompletionOption.ResponseHeadersRead);

            httpResponse.EnsureSuccessStatusCode(); // throws if code is not 2xx

            if (httpResponse.Content != null &&
                httpResponse.Content.Headers.ContentType.MediaType == MediaTypeNames.Application.Json)
            {
                var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);

                try
                {
                    return new JsonSerializer().Deserialize<RequestList>(jsonReader);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogInformation($"Invalid JSON. \n${ex.Message}");
                }
            }
            else
            {
                _logger.LogInformation("HTTP Response was invalid and can't be deserialized.");
            }

            return null;
        }
    }
}