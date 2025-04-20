using Project.Areas.Admin.Models.Entities;
using Project.Areas.Staff.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IHealthInsuranceRepository : IBaseRepository<HealthInsurance>
    {
        Task<IEnumerable<HealthInsurance>> GetAllAdvancedAsync();

        Task<HealthInsurance?> GetByIdAdvancedAsync(Guid id);
        Task<HealthInsurance?> GetByPatientIdAsync(Guid patientId);
    }
}
