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

namespace Gestalt.Utilites.DataExtractor.Services
{
    public class RequestSavingService : IPopularControlSavingService
    {
        private readonly IMongoRepository<RequestEntity> _requestEntityRepository;
        private readonly IConfiguration _configuration;

        public RequestSavingService(IMongoRepository<RequestEntity> requestEntityRepository,
            IConfiguration configuration)
        {
            _requestEntityRepository = requestEntityRepository;
            _configuration = configuration;
        }

        public async Task SaveAsync(RequestList model)
        {
            var isFirstLoadConfig =
                _configuration["Environment:IS_FIRST_LOAD"]
                ?? Environment.GetEnvironmentVariable("IS_FIRST_LOAD")
                ?? "false";
            bool.TryParse(isFirstLoadConfig, out var isFirstLoad);

            if (isFirstLoad)
            {
                var entitiesToAdd = model.requests.ToEntities<RequestEntity, Request>();
                await _requestEntityRepository.AddManyAsync(entitiesToAdd);
                return;
            }

            var requestIds = model.requests.Select(x => x.RequestId).ToList();
            var alreadySavedRequests =
                (await _requestEntityRepository.FindAsync(x => requestIds.Contains(x.RequestId)))
                .Select(x => (x.EntityId, x.RequestId))
                .ToDictionary(x => x.RequestId, x => x.EntityId);

            var requestsToUpdate = model.requests.Where(x => alreadySavedRequests.Keys.Contains(x.RequestId));
            var requestEntitiesToUpdate = requestsToUpdate.ToEntities<RequestEntity, Request>();
            await UpdateAlreadySavedRequestsAsync(requestEntitiesToUpdate, alreadySavedRequests);

            var requestsToAdd = model.requests.Where(x => !alreadySavedRequests.Keys.Contains(x.RequestId));
            var requestEntitiesToAdd = requestsToAdd.ToEntities<RequestEntity, Request>();
            await AddRequestsAsync(requestEntitiesToAdd);
        }

        private async Task UpdateAlreadySavedRequestsAsync(
            IEnumerable<RequestEntity> requestEntitiesToUpdate,
            IDictionary<long, ObjectId> alreadySavedRequests)
        {
            var savingList = new List<Task>();
            foreach (var requestEntity in requestEntitiesToUpdate)
            {
                requestEntity.EntityId = alreadySavedRequests[requestEntity.RequestId];
                savingList.Add(_requestEntityRepository.UpdateAsync(requestEntity));
            }

            await Task.WhenAll(savingList);
        }

        private async Task AddRequestsAsync(IEnumerable<RequestEntity> requestEntitiesToUpdate)
        {
            var savingList = new List<Task>();

            foreach (var requestEntity in requestEntitiesToUpdate)
            {
                savingList.Add(_requestEntityRepository.AddAsync(requestEntity));
            }

            await Task.WhenAll(savingList);
        }
    }
}