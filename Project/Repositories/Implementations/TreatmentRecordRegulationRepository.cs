using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentRecordRegulationRepository : BaseRepository<TreatmentRecord_Regulation>, ITreatmentRecordRegulationRepository
    {
        public TreatmentRecordRegulationRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }
    }
}

