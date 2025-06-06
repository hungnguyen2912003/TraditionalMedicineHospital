using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetAllAdvancedAsync();
        Task<Payment?> GetByIdAdvancedAsync(Guid id);
        Task<Payment?> GetByTreatmentRecordIdAsync(Guid treatmentRecordId);
    }
}
