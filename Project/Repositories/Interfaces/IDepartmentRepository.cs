using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        Task<IEnumerable<Department>> GetAllAdvancedAsync();
        Task<Department?> GetByIdAdvancedAsync(Guid id);
    }
}
