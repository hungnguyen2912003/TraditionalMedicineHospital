using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<Employee?> GetByNameAsync(string name);
        Task<Employee?> GetByCodeAsync(string code);
        Task<IEnumerable<Employee>> GetAllWithCategoryAsync();
        Task<Employee?> GetByIdWithCategoryAsync(Guid id);
    }
}
