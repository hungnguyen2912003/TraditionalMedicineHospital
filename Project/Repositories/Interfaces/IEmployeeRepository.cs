using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetAllAdvancedAsync();
        Task<Employee?> GetByIdAdvancedAsync(Guid id);
        Task<Employee?> GetByUsernameAsync(string username);
    }
}
