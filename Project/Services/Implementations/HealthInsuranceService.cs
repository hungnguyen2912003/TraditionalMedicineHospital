using Project.Areas.Staff.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{

    public class HealthInsuranceService : BaseService<HealthInsurance>, IBaseService<HealthInsurance>
    {
        public HealthInsuranceService(IBaseRepository<HealthInsurance> repository) : base(repository)
        {
        }
    }
}
