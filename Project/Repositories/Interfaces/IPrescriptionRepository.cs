using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IPrescriptionRepository : IBaseRepository<Prescription>
    {
        Task<IEnumerable<Prescription>> GetAllAdvancedAsync();
        Task<Prescription?> GetByIdAdvancedAsync(Guid id);
        Task<IEnumerable<Prescription>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId);
    }
}
