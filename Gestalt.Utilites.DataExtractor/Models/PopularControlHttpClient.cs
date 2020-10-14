using System;
using System.Net.Http;
using System.Threading.Tasks;
using Gestalt.Utilites.DataExtractor.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Gestalt.Utilites.DataExtractor.Models
{
    public class PopularControlHttpClient : IPopularControlHttpClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IConfiguration _configuration;

        public PopularControlHttpClient(IConfiguration configuration)
        {
            _configuration = configuration;
            this.Initialize();
        }

        public Task<HttpResponseMessage> GetAsync(RequestListParameters parameters, HttpCompletionOption completionOption)
        {
            return _httpClient.GetAsync(this.RequestUri(parameters), completionOption);
        }

        private string RequestUri(RequestListParameters parameters)
        {
            return
                "open-gov/get-requests-list" +
                $"?page_num={parameters.PageNumber}" +
                $"&page_size={parameters.EntriesPerPage}" +
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
        }

        private void Initialize()
        {
            _httpClient.BaseAddress = new Uri("https://uslugi.tatarstan.ru/");

            _httpClient.DefaultRequestHeaders.Add("authority", "uslugi.tatarstan.ru");
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
            _httpClient.DefaultRequestHeaders.Add("user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            _httpClient.DefaultRequestHeaders.Add("referer", "https://uslugi.tatarstan.ru/open-gov");
            _httpClient.DefaultRequestHeaders.Add("accept-encoding", "deflate, br");
            _httpClient.DefaultRequestHeaders.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            _httpClient.DefaultRequestHeaders.Add("cookie", _configuration["Environment:cookie"]);
        }

        private void ReleaseUnmanagedResources()
        {
            _httpClient.Dispose();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~PopularControlHttpClient()
        {
            ReleaseUnmanagedResources();
        }
    }
}