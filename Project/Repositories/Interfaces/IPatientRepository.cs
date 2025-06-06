using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IPatientRepository : IBaseRepository<Patient>
    {
        Task<IEnumerable<Patient>> GetAllAdvancedAsync();
        Task<Patient?> GetByIdAdvancedAsync(Guid id);
    }
}
