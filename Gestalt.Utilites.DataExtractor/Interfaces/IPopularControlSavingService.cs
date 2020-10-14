using System.Threading.Tasks;
using Gestalt.Common.Models;

namespace Gestalt.Utilites.DataExtractor.Interfaces
{
    public interface IPopularControlSavingService
    {
        Task SaveAsync(RequestList model);
    }
}