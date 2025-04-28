using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentMethodRepository : IBaseRepository<TreatmentMethod>
    {
        Task<IEnumerable<TreatmentMethod>> GetAllAdvancedAsync();
        Task<TreatmentMethod?> GetByIdAdvancedAsync(Guid id);
        Task<IEnumerable<TreatmentMethod>> GetAvailableTreatmentMethodsAsync(Guid treatmentRecordId);

        Task<IEnumerable<TreatmentMethod>> GetAllByDepartmentAsync(Guid departmentId);
    }
}
