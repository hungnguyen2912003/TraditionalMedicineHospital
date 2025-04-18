using Project.Areas.Staff.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IAssignmentRepository : IBaseRepository<Assignment>
    {
        Task<IEnumerable<Assignment>> GetAllAdvancedAsync();
        Task<Assignment?> GetByIdAdvancedAsync(Guid id);
    }
}

