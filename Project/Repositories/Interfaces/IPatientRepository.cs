using Project.Areas.Staff.Models.Entities;
using Project.Models;

namespace Project.Repositories.Interfaces
{
    public interface IPatientRepository : IBaseRepository<Patient>
    {
        Task<IEnumerable<Patient>> GetAllAdvancedAsync();
        Task<Patient?> GetByIdAdvancedAsync(Guid id);
    }
}
