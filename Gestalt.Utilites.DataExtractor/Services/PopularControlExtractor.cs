namespace Gestalt.Utilites.DataExtractor
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Gestalt.Common.Models;
    using Gestalt.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class PopularControlExtractor : IPopularControlExtractor
    {
        private readonly ILogger<PopularControlExtractor> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMainResponseSaverService _saverService;

        public PopularControlExtractor(IConfiguration configuration, ILogger<PopularControlExtractor> logger,
            IMainResponseSaverService saverService)
        {
            _configuration = configuration;
            _logger = logger;
            _saverService = saverService;
        }

        private readonly int _entriesCount = 100;

        private readonly int _fromPageNumber = 1;

        private readonly int _pageCount = 10;

        public async Task ExtractData()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://uslugi.tatarstan.ru/");

            client.DefaultRequestHeaders.Add("authority", "uslugi.tatarstan.ru");
            client.DefaultRequestHeaders.Add("accept", "application/json, text/javascript, */*; q=0.01");
            client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            client.DefaultRequestHeaders.Add("referer", "https://uslugi.tatarstan.ru/open-gov");
            client.DefaultRequestHeaders.Add("accept-encoding", "deflate, br");
            client.DefaultRequestHeaders.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("cookie",
                _configuration["Environment:cookie"]);

            // var tasks = new List<Task>();
            for (var pageNumber = this._fromPageNumber; pageNumber <= this._pageCount; pageNumber++)
            {
                // var filePath = path + $"{DateTime.Now.ToShortDateString()}_page_{pageNumber}.json";
                await this.LoadAndSaveData(client, pageNumber);
            }

            // Task.WaitAll(tasks.ToArray());
            _logger.LogInformation("__________ Data loading completed __________");
        }

        private async Task LoadAndSaveData(HttpClient client, int pageNumber)
        {
            var uri =
                "open-gov/get-requests-list" +
                $"?page_num={pageNumber}" +
                $"&page_size={this._entriesCount}" +
                "&own=false" +
                "&popular= false" +
                "&from_id=null" +
                "&offset=0" +
                "&save=1" +
                "&from_date=2012-01-01 00:00:00" +
                "&to_date=2020-03-15 23:59:59" +
                "&period=all" +
                "&search_text=" +
                "&order_by_status_change_time=false" +
                "&height_degrees= 0.27029173077504964" +
                "&width_degrees= 1.318359374999993" +
                "&center_lat= 55.78197922246136" +
                "&center_long= 49.12399291992188";

            _logger.LogInformation($"Starting loading page {pageNumber} at {DateTime.Now}");
            var sw = Stopwatch.StartNew();
            var responseEntry = await this.GetMainResponse(uri, client);
            if (responseEntry == null)
            {
                _logger.LogInformation($"An error while data receiving. Page {pageNumber} at {DateTime.Now}");
                // var fileName = Path.GetFileName(filePath);
                // var directoryName = Path.GetDirectoryName(filePath);
                // await File.WriteAllTextAsync(directoryName + "ERROR." + fileName, "ERROR");
                return;
            }

            sw.Stop();

            _logger.LogInformation(
                $"__________ Loading page {pageNumber} ended at {DateTime.Now}," +
                $" time elapsed = {this.MillisecondsToString(sw.ElapsedMilliseconds)}");

            // await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(responseEntry));
            await _saverService.SaveMainResponse(responseEntry);
            _logger.LogInformation($"---------- Page {pageNumber} saved at {DateTime.Now}");
        }

        private async Task<MainResponse> GetMainResponse(string uri, HttpClient httpClient)
        {
            using var httpResponse = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

            if (httpResponse.Content is object &&
                httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);

                var serializer = new JsonSerializer();

                try
                {
                    return serializer.Deserialize<MainResponse>(jsonReader);
                }
                catch (JsonReaderException)
                {
                    _logger.LogInformation("Invalid JSON.");
                }
            }
            else
            {
                _logger.LogInformation("HTTP Response was invalid and cannot be deserialised.");
            }

            return null;
        }

        private string MillisecondsToString(double ms)
        {
            var t = TimeSpan.FromMilliseconds(ms);
            return
                $"{t.Hours.ToString():D2}h" +
                $":{t.Minutes.ToString():D2}m" +
                $":{t.Seconds.ToString():D2}s" +
                $":{t.Milliseconds.ToString():D3}ms";
        }
    }
}