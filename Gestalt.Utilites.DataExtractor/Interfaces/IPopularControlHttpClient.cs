using System;
using System.Net.Http;
using System.Threading.Tasks;
using Gestalt.Utilites.DataExtractor.Models;

namespace Gestalt.Utilites.DataExtractor.Interfaces
{
    public interface IPopularControlHttpClient : IDisposable
    {
        public Task<HttpResponseMessage> GetAsync(RequestListParameters parameters, HttpCompletionOption completionOption);
    }
}