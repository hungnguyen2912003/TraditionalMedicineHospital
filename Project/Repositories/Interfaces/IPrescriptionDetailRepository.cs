using Project.Areas.Staff.Models.Entities;
using Project.Repositories;

namespace Repositories.Interfaces
{
    public interface IPrescriptionDetailRepository : IBaseRepository<PrescriptionDetail>
    {
        Task<IEnumerable<PrescriptionDetail>> GetAllAdvancedAsync();
        Task<PrescriptionDetail?> GetByIdAdvancedAsync(Guid id);
        Task<IEnumerable<PrescriptionDetail>> GetByPrescriptionIdAsync(Guid prescriptionId);
    }
}
