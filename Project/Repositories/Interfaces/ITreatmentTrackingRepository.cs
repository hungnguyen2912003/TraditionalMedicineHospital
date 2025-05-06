using Project.Areas.Staff.Models.DTOs.TrackingDTO;
using Project.Areas.Staff.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentTrackingRepository : IBaseRepository<TreatmentTracking>
    {
        Task<List<TreatmentTracking>> GetAllAdvancedAsync();
    }
}