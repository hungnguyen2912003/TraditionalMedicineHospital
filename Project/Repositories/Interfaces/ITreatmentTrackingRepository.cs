using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentTrackingRepository : IBaseRepository<TreatmentTracking>
    {
        Task<IEnumerable<TreatmentTracking>> GetAllAdvancedAsync();
        Task<IEnumerable<TreatmentTracking>> GetByDetailIdAsync(Guid detailId);
        Task<IEnumerable<TreatmentTracking>> GetByPatientIdAsync(Guid patientId);
        Task<IEnumerable<TreatmentTracking>> GetByCreatedByAsync(string createdBy);
        Task<IEnumerable<TreatmentTracking>> GetByDepartmentAsync(string departmentName);
    }
}