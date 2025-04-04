using Project.Areas.Admin.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{
    public class RegulationService : BaseService<Regulation>, IBaseService<Regulation>
    {
        public RegulationService(IBaseRepository<Regulation> repository) : base(repository)
        {
        }
    }
}
