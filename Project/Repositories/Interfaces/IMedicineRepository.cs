using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IMedicineRepository : IBaseRepository<Medicine>
    {
        Task<Medicine?> GetByNameAsync(string name);
        Task<Medicine?> GetByCodeAsync(string code);
        Task<IEnumerable<Medicine>> GetAllWithCategoryAsync();
        Task<Medicine?> GetByIdWithCategoryAsync(Guid id);

    }
}
