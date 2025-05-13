using Project.Areas.Staff.Models.DTOs.TrackingDTO;
using Project.Areas.Staff.Models.Entities;
using Project.Models;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentTrackingRepository : IBaseRepository<TreatmentTracking>
    {
        Task<List<TreatmentTracking>> GetAllAdvancedAsync();
        Task<IEnumerable<TreatmentTracking>> GetByDetailIdAsync(Guid detailId);
        Task<IEnumerable<TreatmentTracking>> GetByPatientIdAsync(Guid patientId);
    }
}