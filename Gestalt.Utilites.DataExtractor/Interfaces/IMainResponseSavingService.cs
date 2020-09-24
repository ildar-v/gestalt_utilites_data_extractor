namespace Gestalt.Utilites.DataExtractor.Interfaces
{
    using System.Threading.Tasks;
    using Gestalt.Common.Models;

    public interface IMainResponseSavingService
    {
        Task SaveAsync(MainResponse model);
    }
}