using Project.Areas.Admin.Models.Entities;
using Project.Areas.BacSi.Models.DTOs;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentRecordDetailRepository : IBaseRepository<TreatmentRecordDetail>
    {
        Task<List<TreatmentRecordDetail>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId);
        new Task<TreatmentRecordDetail?> GetByCodeAsync(string code);
        Task<TreatmentRecordDetailDto?> GetDetailWithNamesAsync(string code);
        Task<List<Patient>> GetPatientsByRoomIdAsync(Guid roomId);
        Task<TreatmentRecordDetail?> GetByPatientAndRoomAsync(Guid patientId, Guid roomId);
        Task<TreatmentRecordDetail?> GetByIdAdvancedAsync(Guid id);
        Task<List<TreatmentRecordDetail>> GetDetailsByRoomIdAsync(Guid roomId);
    }
}

