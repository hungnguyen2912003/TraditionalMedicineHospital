using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentRecordDetailRepository : BaseRepository<TreatmentRecordDetail>, ITreatmentRecordDetailRepository
    {
        public TreatmentRecordDetailRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }
    }
}
