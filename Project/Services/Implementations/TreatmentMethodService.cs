using Project.Areas.Admin.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{
    public class TreatmentMethodService : BaseService<TreatmentMethod>, IBaseService<TreatmentMethod>
    {
        public TreatmentMethodService(IBaseRepository<TreatmentMethod> repository)
            : base(repository)
        {
        }
    }
}
