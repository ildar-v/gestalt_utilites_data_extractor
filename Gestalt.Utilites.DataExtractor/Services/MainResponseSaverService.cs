namespace Gestalt.Utilites.DataExtractor.Services
{
    using System.Threading.Tasks;
    using Gestalt.Common.Models;
    using Gestalt.Common.Services;

    public class MainResponseSaverService : IMainResponseSaverService
    {
        private readonly IRequestsService _requestsService;

        public MainResponseSaverService(IRequestsService requestsService)
        {
            _requestsService = requestsService;
        }

        public async Task SaveMainResponse(MainResponse model)
        {
            await _requestsService.SaveAsync(model);
        }
    }
}