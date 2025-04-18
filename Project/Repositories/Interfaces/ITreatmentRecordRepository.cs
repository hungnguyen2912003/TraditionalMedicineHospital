using Project.Areas.Staff.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentRecordRepository : IBaseRepository<TreatmentRecord>
    {
        Task<IEnumerable<TreatmentRecord>> GetAllAdvancedAsync();
        Task<TreatmentRecord?> GetByIdAdvancedAsync(Guid id);
        Task<IEnumerable<TreatmentRecord>> GetByPatientIdAsync(Guid patientId);
    }
}
