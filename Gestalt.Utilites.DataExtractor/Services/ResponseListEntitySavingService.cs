using System.Threading.Tasks;
using Gestalt.Common.DAL;
using Gestalt.Common.DAL.Entities;
using Gestalt.Common.DAL.MongoDBImpl;
using Gestalt.Common.Models;
using Gestalt.Utilites.DataExtractor.Interfaces;

namespace Gestalt.Utilites.DataExtractor.Services
{
    public class ResponseListEntitySavingService : IPopularControlSavingService
    {
        private readonly IMongoRepository<RequestListEntity> _requestListEntityRepository;

        public ResponseListEntitySavingService(IMongoRepository<RequestListEntity> requestListEntityRepository)
        {
            _requestListEntityRepository = requestListEntityRepository;
        }

        public async Task SaveAsync(RequestList model)
        {
            var requestListEntity = model.ToEntity<RequestListEntity, RequestList>();
            await _requestListEntityRepository.AddAsync(requestListEntity);
        }
    }
}