using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        Task<Department?> GetByNameAsync(string name);
        Task<Department?> GetByCodeAsync(string code);
    }
}
