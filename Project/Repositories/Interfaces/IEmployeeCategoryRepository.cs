using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IEmployeeCategoryRepository : IBaseRepository<EmployeeCategory>
    {
        Task<EmployeeCategory?> GetByNameAsync(string name);
        Task<EmployeeCategory?> GetByCodeAsync(string code);
        Task<IEnumerable<EmployeeCategory>> GetAllActiveAsync();
    }
}
