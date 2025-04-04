using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IMedicineRepository : IBaseRepository<Medicine>
    {
        Task<IEnumerable<Medicine>> GetAllAdvancedAsync();
        Task<Medicine?> GetByIdAdvancedAsync(Guid id);

    }
}
