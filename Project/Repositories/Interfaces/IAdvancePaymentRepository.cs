using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IAdvancePaymentRepository : IBaseRepository<AdvancePayment>
    {
        Task<IEnumerable<AdvancePayment>> GetAllAdvancedAsync();
        Task<AdvancePayment?> GetByIdAdvancedAsync(Guid id);
        Task<AdvancePayment?> GetByTreatmentRecordIdAsync(Guid treatmentRecordId);
        Task<IEnumerable<AdvancePayment>> GetListByTreatmentRecordIdAsync(Guid treatmentRecordId);
    }
}
