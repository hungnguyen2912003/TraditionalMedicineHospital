using Project.Areas.Staff.Models.Entities;
using Project.Repositories;

namespace Repositories.Interfaces
{
    public interface IPrescriptionRepository : IBaseRepository<Prescription>
    {
        Task<IEnumerable<Prescription>> GetAllAdvancedAsync();
        Task<Prescription?> GetByIdAdvancedAsync(Guid id);
        Task<IEnumerable<Prescription>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId);
    }
}
