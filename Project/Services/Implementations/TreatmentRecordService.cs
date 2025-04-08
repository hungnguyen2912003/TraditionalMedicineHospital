using Project.Areas.Staff.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{
    public class TreatmentRecordService : BaseService<TreatmentRecord>, IBaseService<TreatmentRecord>
    {
        public TreatmentRecordService(IBaseRepository<TreatmentRecord> repository) : base(repository)
        {
        }
    }
}
