using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentMethodRepository : IBaseRepository<TreatmentMethod>
    {
        Task<IEnumerable<TreatmentMethod>> GetAllAdvancedAsync();
        Task<TreatmentMethod?> GetByIdAdvancedAsync(Guid id);
    }
}
