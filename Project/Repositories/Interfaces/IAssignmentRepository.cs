using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IAssignmentRepository : IBaseRepository<Assignment>
    {
        Task<IEnumerable<Assignment>> GetAllAdvancedAsync();
        Task<Assignment?> GetByIdAdvancedAsync(Guid id);
        Task<List<Assignment>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId);
        new Task<Assignment?> GetByCodeAsync(string code);
    }
}

