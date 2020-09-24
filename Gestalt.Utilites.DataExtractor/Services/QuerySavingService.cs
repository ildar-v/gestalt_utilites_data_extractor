namespace Gestalt.Utilites.DataExtractor.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Gestalt.Common.DAL;
    using Gestalt.Common.DAL.Entities;
    using Gestalt.Common.DAL.MongoDBImpl;
    using Gestalt.Common.Models;
    using Gestalt.Utilites.DataExtractor.Interfaces;
    using Microsoft.Extensions.Configuration;
    using MongoDB.Bson;

    public class QuerySavingService : IMainResponseSavingService
    {
        private readonly IMongoRepository<QueryEntity> _queryEntityRepository;
        private readonly IConfiguration _configuration;

        public QuerySavingService(IMongoRepository<QueryEntity> queryEntityRepository, IConfiguration configuration)
        {
            _queryEntityRepository = queryEntityRepository;
            _configuration = configuration;
        }

        public async Task SaveAsync(MainResponse model)
        {
            var isFirstLoadConfig =
                _configuration["Environment:IS_FIRST_LOAD"]
                ?? Environment.GetEnvironmentVariable("IS_FIRST_LOAD")
                ?? "false";
            bool.TryParse(isFirstLoadConfig, out var isFirstLoad);

            if (isFirstLoad)
            {
                var entitiesToAdd = model.requests.ToEntities<QueryEntity, Queries>();
                await _queryEntityRepository.AddManyAsync(entitiesToAdd);
                return;
            }

            var requestIds = model.requests.Select(x => x.RequestId).ToList();
            var alreadySavedRequests =
                (await _queryEntityRepository.FindAsync(x => requestIds.Contains(x.RequestId)))
                .Select(x => (x.EntityId, x.RequestId))
                .ToDictionary(x => x.RequestId, x => x.EntityId);

            var requestsToUpdate = model.requests.Where(x => alreadySavedRequests.Keys.Contains(x.RequestId));
            var requestEntitiesToUpdate = requestsToUpdate.ToEntities<QueryEntity, Queries>();
            await UpdateAlreadySavedRequestsAsync(requestEntitiesToUpdate, alreadySavedRequests);

            var requestsToAdd = model.requests.Where(x => !alreadySavedRequests.Keys.Contains(x.RequestId));
            var requestEntitiesToAdd = requestsToAdd.ToEntities<QueryEntity, Queries>();
            await AddRequestsAsync(requestEntitiesToAdd);
        }

        private async Task UpdateAlreadySavedRequestsAsync(
            IEnumerable<QueryEntity> requestEntitiesToUpdate,
            IDictionary<long, ObjectId> alreadySavedRequests)
        {
            var savingList = new List<Task>();
            foreach (var requestEntity in requestEntitiesToUpdate)
            {
                requestEntity.EntityId = alreadySavedRequests[requestEntity.RequestId];
                savingList.Add(_queryEntityRepository.UpdateAsync(requestEntity));
            }

            await Task.WhenAll(savingList);
        }

        private async Task AddRequestsAsync(IEnumerable<QueryEntity> requestEntitiesToUpdate)
        {
            var savingList = new List<Task>();
            foreach (var requestEntity in requestEntitiesToUpdate)
            {
                savingList.Add(_queryEntityRepository.AddAsync(requestEntity));
            }

            await Task.WhenAll(savingList);
        }
    }
}