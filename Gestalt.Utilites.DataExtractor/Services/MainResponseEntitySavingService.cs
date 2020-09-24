namespace Gestalt.Utilites.DataExtractor.Services
{
    using System.Threading.Tasks;
    using Gestalt.Common.DAL;
    using Gestalt.Common.DAL.Entities;
    using Gestalt.Common.DAL.MongoDBImpl;
    using Gestalt.Common.Models;
    using Gestalt.Utilites.DataExtractor.Interfaces;

    public class MainResponseEntitySavingService : IMainResponseSavingService
    {
        private readonly IMongoRepository<MainResponseEntity> _mainResponseEntityRepository;

        public MainResponseEntitySavingService(IMongoRepository<MainResponseEntity> mainResponseEntityRepository)
        {
            _mainResponseEntityRepository = mainResponseEntityRepository;
        }

        public async Task SaveAsync(MainResponse model)
        {
            var mainResponseEntity = model.ToEntity<MainResponseEntity, MainResponse>();
            await _mainResponseEntityRepository.AddAsync(mainResponseEntity);
        }
    }
}